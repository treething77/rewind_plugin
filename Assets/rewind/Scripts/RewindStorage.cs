using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace aeric.rewind_plugin {
    //TODO: move interface to its own file
    public interface IRewindDataHandler {
        public void RewindHandlerData(IRewindHandler rewindHandler, NativeByteArrayReader nativeByteArrayReader);
    }

    public enum RewindMappedFrame {}

    public class RewindStorage {
        private readonly int _maxFrameCount;
        private readonly NativeByteArray _nativeStorage;

        //frame data readers A and B, read 2 adjacent frames and interpolate the results
        private readonly NativeByteArrayReader _frameReaderA;
        private readonly NativeByteArrayReader _frameReaderB;

        private readonly NativeByteArrayWriter _storageWriter;

        private readonly int _frameDataOffset;
        private readonly int _handlerDataOffset;

        //Map of ID to RewindHandlerStorage
        private readonly Dictionary<uint, RewindHandlerStorage> rewindHandlerStorageMap = new();
        private readonly RewindScene _scene;

        public RewindStorage(RewindScene rewindScene, int maxFrameCount) {
            _maxFrameCount = maxFrameCount;
            _scene = rewindScene;

            // Storage size calculation:
            // -sum of the required space for all rewind handlers
            // -plus any required space for bookkeeping of rewind handlers
            
            //Each rewind handler will be assigned 1 portion of the buffer
            //that will be a contiguous memory area for that handler to use
            // <|--component A--|--component B --| -- etc >
            //vs an interleaved approach
            // <|A1|B1|A2|B2|A3|B3>
            // This approach gives us more flexibility (which we dont really take advantage of yet)
            
            var bufferSizeBytes = 0;

            //Allocate the storage for the recording info
            //frame count
            bufferSizeBytes += 4;
            //handler count
            bufferSizeBytes += 4;

            _frameDataOffset = bufferSizeBytes;
            //Allocate the storage for the frame timestamps
            bufferSizeBytes += 4 * maxFrameCount;

            _handlerDataOffset = bufferSizeBytes;
            //Allocate the storage for the handler storage info
            //ids
            bufferSizeBytes += 8 * rewindScene.RewindHandlers.Count;
            //offsets
            bufferSizeBytes += 4 * rewindScene.RewindHandlers.Count;

            foreach (var rewindHandler in rewindScene.RewindHandlers) {
                var bufferHandlerStartOffset = bufferSizeBytes;

                var handlerFrameSizeBytes = rewindHandler.makeDataSchema().getSchemaSize();

                //add space for bookkeeping data
                handlerFrameSizeBytes += 8; //ID

                var handlerStorageSizeBytes = handlerFrameSizeBytes * maxFrameCount;
                
                var handlerStorage = new RewindHandlerStorage(bufferHandlerStartOffset, handlerFrameSizeBytes);

                if (rewindHandler.ID == 0) {
                    rewindHandler.ID = RewindComponentIDGenerator.generateID(rewindHandler);
                }
                
                rewindHandlerStorageMap.Add(rewindHandler.ID, handlerStorage);

                bufferSizeBytes += handlerStorageSizeBytes;
            }

            _nativeStorage = new NativeByteArray(bufferSizeBytes);
            _storageWriter = new NativeByteArrayWriter(_nativeStorage);

            _frameReaderA = new NativeByteArrayReader(_nativeStorage);
            _frameReaderB = new NativeByteArrayReader(_nativeStorage);

            //write out the basic storage info
            _storageWriter.writeInt(maxFrameCount);
            _storageWriter.writeInt(rewindScene.RewindHandlers.Count);

            //write out the handler info (ids, offsets)
            _storageWriter.setWriteHead(_handlerDataOffset);
            foreach (var rewindHandler in rewindScene.RewindHandlers) {
                var handlerStorage = getHandlerStorage(rewindHandler.ID);

                _storageWriter.writeUInt(rewindHandler.ID);
                _storageWriter.writeInt(handlerStorage.HandlerStorageOffset);
            }
        }

        public bool isFull => RecordedFrameCount == _maxFrameCount;

        public int RecordedFrameCount { get; private set; }

        public int FrameReadIndex { get; private set; }

        public int FrameWriteIndex => (FrameReadIndex + RecordedFrameCount) % _maxFrameCount;

        // Destructor
        ~RewindStorage() {
            if (!_nativeStorage.isDisposed) _nativeStorage.Dispose();
        }

        public void Dispose() {
            if (!_nativeStorage.isDisposed) _nativeStorage.Dispose();
        }

        public RewindHandlerStorage getHandlerStorage(uint rewindHandlerID) {
            RewindHandlerStorage storage = null;
            if (!rewindHandlerStorageMap.TryGetValue(rewindHandlerID, out storage)) Debug.LogError($"We do not have storage for handler {rewindHandlerID} was it registered when the scene was created?");
            return storage;
        }

        public void writeHandlerFrame(IRewindHandler rewindHandler) {
            // Debug.Log($" Writing frame {rewindWriteHead}");

            var handlerStorage = getHandlerStorage(rewindHandler.ID);

            //set the write head to the correct location
            _storageWriter.setWriteHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * FrameWriteIndex);

            //store ID
            //do we really need to store the ID here? we could just use the offset to determine the ID 
            _storageWriter.writeUInt(rewindHandler.ID);

            rewindHandler.rewindStore(_storageWriter);
        }

        public void writeFrameStart(float frameTimeRelativeToStart) {
            // write frame time
            var currentFrameTimeOffset = _frameDataOffset + 4 * FrameWriteIndex;
            _storageWriter.setWriteHead(currentFrameTimeOffset);

            //frame time is the current time? time from start of recording?
            _storageWriter.writeFloat(frameTimeRelativeToStart);
        }

        public void writeFrameEnd() {
            //Clamp the number of frames recorded at the max

            //write head is implicit so moves when we increase frame count
            RecordedFrameCount++;
            if (RecordedFrameCount > _maxFrameCount) {
                RecordedFrameCount = _maxFrameCount;

                //move the read head if the buffer is full
                FrameReadIndex++;
                if (FrameReadIndex >= _maxFrameCount) FrameReadIndex = 0;
            }
        }

        public (RewindMappedFrame frameMappedA, RewindMappedFrame frameMappedB, float frameT, int frameUnmappedA, int frameUnmappedB) findPlaybackFrames(float playbackTime) {
            //if only 1 frame then use it for both
            //   i.e. we always interpolate between 2 frames
            if (RecordedFrameCount == 1)
                //TODO: if we rewound then the first frame wont necessarily be at 0
                return (0, 0, 0.0f, 0, 0);

            RewindMappedFrame frameMappedA = 0;
            RewindMappedFrame frameMappedB = 0;
            var frameA = 0;
            var frameB = 0;
            float frameT = 0;

            _frameReaderA.setReadHead(_frameDataOffset);

            unsafe {
                var pTimes = _frameReaderA.getReadHeadDataPtr<float>();
                //Have to handle the continuous recording case
                //One way to do that would be to remap the indices

                //currently we search between 0 and rewindFrameCount-1
                // [0, rewindFrameCount-1]
                // 0 -> rewindFrameWriteIndex
                // rewindFrameCount-1 -> (rewindFrameWriteIndex + rewindFrameCount-1) % maxFrameCount
                var startTimeIndex = remapIndex(0);
                var endTimeIndex = remapIndex(RecordedFrameCount - 1);

                //special cases, before start time or after end time
                if (playbackTime <= pTimes[(int)startTimeIndex]) return (startTimeIndex, startTimeIndex, 0.0f, 0, 0);
                if (playbackTime >= pTimes[(int)endTimeIndex]) return (endTimeIndex, endTimeIndex, 1.0f, RecordedFrameCount - 1, RecordedFrameCount - 1);

                //do a linear search
                //start at the beginning and work our way along until the next time is past our playbackTime
                var frame = 0;
                while (frame < RecordedFrameCount) {
                    var remappedFrame = remapIndex(frame);
                    if (pTimes[(int)remappedFrame] >= playbackTime) {
                        frameA = frame - 1;
                        frameB = frame;
                        frameMappedB = remappedFrame;
                        frameMappedA = remapIndex(frame - 1);
                        break;
                    }

                    frame++;
                }

                //frameT should be the normalized value between the two frame times
                //  i.e. 0.0f = frameA, 1.0f = frameB
                //  so we need to calculate the normalized value between the two frame times
                //  and then subtract the frameA time from it 
                var frameTimeA = pTimes[(int)frameMappedA];
                var frameTimeB = pTimes[(int)frameMappedB];

                frameT = (playbackTime - frameTimeA) / (frameTimeB - frameTimeA);

                Debug.Assert(frameT >= 0.0f);
                Debug.Assert(frameT <= 1.0f);
            }

            return (frameMappedA, frameMappedB, frameT, frameA, frameB);
        }

        /// <summary>
        ///     Takes in a "normalized" frame index, where the range is always [0, frameCount-1]
        ///     and maps it onto the buffer taking into account wrapping.
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <returns></returns>
        private RewindMappedFrame remapIndex(int frameIndex) {
            var mappedFrameIndex = (FrameReadIndex + frameIndex) % _maxFrameCount;
            return (RewindMappedFrame)mappedFrameIndex;
        }

        public void restoreHandlerInterpolated(IRewindHandler rewindHandler, RewindMappedFrame frameA, RewindMappedFrame frameB, float frameT) {
            var handlerStorage = getHandlerStorage(rewindHandler.ID);

            var frameIndexA = (int)frameA;
            var frameIndexB = (int)frameB;

            //set the read head to the correct location
            _frameReaderA.setReadHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * frameIndexA);
            _frameReaderB.setReadHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * frameIndexB);

            var handlerIDA = _frameReaderA.readUInt();
            var handlerIDB = _frameReaderB.readUInt();

            //sanity check
            Debug.Assert(handlerIDA == handlerIDB);
            Debug.Assert(handlerIDA == rewindHandler.ID);

            rewindHandler.preRestore();

            //read the data from the 2 frames
            rewindHandler.rewindRestoreInterpolated(_frameReaderA, _frameReaderB, frameT);

            rewindHandler.postRestore();
        }
        
        
        public float getTime(int timeFrameIndex) {
            unsafe {
                _frameReaderA.setReadHead(_frameDataOffset);
                var pTimes = _frameReaderA.getReadHeadDataPtr<float>();
                var mappedTimeIndex = remapIndex(timeFrameIndex);
                return pTimes[(int)mappedTimeIndex];
            }
        }

        public void rewindFrames(int frameCountToRewind) {
            frameCountToRewind = Mathf.Min(frameCountToRewind, RecordedFrameCount);

            Debug.Log($"Rewinding {frameCountToRewind} frames");

            //the READ head does not move (start of valid data)
            //the WRITE head is implicit so moves when we set frame count
            RecordedFrameCount -= frameCountToRewind;
            if (RecordedFrameCount < 0) RecordedFrameCount = 0;
        }
        
        public float getFrameTime(int unmappedFrameIndex) {
            unsafe {
                _frameReaderA.setReadHead(_frameDataOffset);
                var pTimes = _frameReaderA.getReadHeadDataPtr<float>();
                return pTimes[unmappedFrameIndex];
            }
        }

        public Vector3 getFramePosition(int unmappedFrameIndex, IRewindHandler rewindHandler) {
            var handlerStorage = getHandlerStorage(rewindHandler.ID);

            //set the read head to the correct location
            _frameReaderA.setReadHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * unmappedFrameIndex);
            var handlerIDA = _frameReaderA.readUInt();

            //assume the handler is RewindTransform and read the position
            return _frameReaderA.readVector3();
        }

        public void getUnmappedFrameData(int unmappedFrameIndex, IRewindHandler rewindHandler, IRewindDataHandler dataHandler) {
            var handlerStorage = getHandlerStorage(rewindHandler.ID);

            RewindMappedFrame frameIndex = remapIndex(unmappedFrameIndex);
            //set the read head to the correct location
            _frameReaderA.setReadHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * (int)frameIndex);
            var handlerIDA = _frameReaderA.readUInt();
            dataHandler.RewindHandlerData(rewindHandler, _frameReaderA);
        }
        
        //TODO: everything below here move to separate files
        
        [Serializable]
        public class RewindStorageData_Value {
            public RewindDataPointType valueType; 
            
            public float f;
            public int i;
            public bool b;
            public Vector3 v;
            public Quaternion q;
            public Color c;
        }


        [Serializable]
        public class RewindStorageData_Handler {
            public uint id;
            public RewindStorageData_Value[] values;
        }
        
        [Serializable]
        public class RewindStorageData {
            public int maxFrameCount;
            public int recordedFrameCount;
            public int handlerCount;
            public int version = 1;

            public float[] frameTimeData;
            public RewindStorageData_Handler[] handlerData;
        }

        private RewindStorageData convertToStorage() {
            
            var storageData = new RewindStorageData();
            storageData.maxFrameCount = _maxFrameCount;
            storageData.recordedFrameCount = RecordedFrameCount;
             
            _frameReaderA.setReadHead(0);
            _frameReaderA.readInt();
            storageData.handlerCount =  _frameReaderA.readInt();
            
            //read head?
            
            //convert the  contents of the NativeArray to json using the schema of each component

            _frameReaderA.setReadHead(_frameDataOffset);
            //frame data
            storageData.frameTimeData = new float[storageData.recordedFrameCount];
            for (int i = 0; i < storageData.recordedFrameCount; i++) {
                float frameTime = _frameReaderA.readFloat();
                storageData.frameTimeData[i] = frameTime;
            }
            
            //handler data
            storageData.handlerData = new RewindStorageData_Handler[storageData.handlerCount * storageData.recordedFrameCount];

            int dataIndex = 0;
            for (int i = 0; i < storageData.handlerCount; i++) {
                KeyValuePair<uint, RewindHandlerStorage> handlerStoragePair = rewindHandlerStorageMap.ElementAt(i);

                var handlerID = handlerStoragePair.Key;
                var handlerStorage = handlerStoragePair.Value;

                for (int frame = 0; frame < storageData.recordedFrameCount; frame++) {
                    RewindMappedFrame mappedFrame = remapIndex(frame);
                    _frameReaderA.setReadHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * (int)mappedFrame);

                    uint id = _frameReaderA.readUInt();//id  
                    Debug.Assert(id == handlerID);
                    
                    IRewindHandler handler = _scene.getHandler(handlerID);
                    
                    RewindDataSchema dataSchema = handler.makeDataSchema();

                    RewindStorageData_Handler data = new RewindStorageData_Handler();
                    data.id = handlerID;

                    List<RewindDataPoint> schema = dataSchema.getSchema();
                    data.values = new RewindStorageData_Value[dataSchema.GetValueCount()];

                    int valueIndex = 0;
                    for (int v = 0; v < schema.Count; v++) {
                        RewindDataPoint schemaDataPoint = schema[v];

                        for (int vi = 0; vi < schemaDataPoint._count; vi++) {
                            RewindStorageData_Value val = new RewindStorageData_Value();
                            val.valueType = schemaDataPoint._type;
                            switch (val.valueType) {
                            case RewindDataPointType.FLOAT:
                                val.f = _frameReaderA.readFloat();
                                break;
                            case RewindDataPointType.INT:
                                val.i = _frameReaderA.readInt();
                                break;
                            case RewindDataPointType.COLOR:
                                val.c = _frameReaderA.readColor();
                                break;
                            case RewindDataPointType.BOOL:
                                val.b = _frameReaderA.readBool();
                                break;
                            case RewindDataPointType.VECTOR3:
                                val.v = _frameReaderA.readVector3();
                                break;
                            case RewindDataPointType.QUATERNION:
                                val.q = _frameReaderA.readQuaternion();
                                break;
                            default:
                                Debug.LogError($"Type not handled {val.valueType}");
                                break;
                            }
                            
                            data.values[valueIndex] = val;
                            valueIndex++;
                        }
                    }
                    
                    storageData.handlerData[dataIndex] = data;
                    dataIndex++;
                }
            }

            return storageData;
        }

        public string writeToJson() {
            var storageData = convertToStorage();
            string jsonStr = JsonUtility.ToJson(storageData, true);
            return jsonStr;
        }

        public void writeToRawBinaryFile(string fileName) {
            using (var fileStream = new FileStream(fileName, FileMode.Create)) {
                //write the header
                fileStream.WriteByte(1); //v1

                //write the data
                var managedArray = _nativeStorage.getManagedArray();
                fileStream.Write(managedArray);
            }
        }
        
        public void writeToBinaryStreamFile(string fileName) {
            var storageData = convertToStorage();
            
            using (var fileStream = new FileStream(fileName, FileMode.Create)) {
                using (var binaryWriter = new BinaryWriter(fileStream)) {
                    //write the header
                    binaryWriter.Write(storageData.maxFrameCount);
                    binaryWriter.Write(storageData.recordedFrameCount);
                    binaryWriter.Write(storageData.handlerCount);
                    binaryWriter.Write(storageData.version);
                    
                    //write the frame times
                    for (int i = 0; i < storageData.recordedFrameCount; i++) {
                        binaryWriter.Write(storageData.frameTimeData[i]);
                    }
                    
                    //write the handler data
                    for (int i = 0; i < storageData.handlerData.Length; i++) {
                        RewindStorageData_Handler handlerData = storageData.handlerData[i];
                        binaryWriter.Write(handlerData.id);
                        binaryWriter.Write(handlerData.values.Length);
                        for (int v = 0; v < handlerData.values.Length; v++) {
                            RewindStorageData_Value val = handlerData.values[v];
                            binaryWriter.Write((int) val.valueType);
                            switch (val.valueType) {
                            case RewindDataPointType.FLOAT:
                                binaryWriter.Write(val.f);
                                break;
                            case RewindDataPointType.INT:
                                binaryWriter.Write(val.i);
                                break;
                            case RewindDataPointType.COLOR:
                                binaryWriter.Write(val.c.r);
                                binaryWriter.Write(val.c.g);
                                binaryWriter.Write(val.c.b);
                                binaryWriter.Write(val.c.a);
                                break;
                            case RewindDataPointType.BOOL:
                                binaryWriter.Write(val.b);
                                break;
                            case RewindDataPointType.VECTOR3:
                                binaryWriter.Write(val.v.x);
                                binaryWriter.Write(val.v.y);
                                binaryWriter.Write(val.v.z);
                                break;
                            case RewindDataPointType.QUATERNION:
                                binaryWriter.Write(val.q.x);
                                binaryWriter.Write(val.q.y);
                                binaryWriter.Write(val.q.z);
                                binaryWriter.Write(val.q.w);
                                break;
                            default:
                                Debug.LogError($"Type not handled {val.valueType}");
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        private void loadFromStorage(RewindStorageData storageData) {
            RecordedFrameCount = storageData.recordedFrameCount;
            
            //frame times
            _storageWriter.setWriteHead(_frameDataOffset);

            for (int i = 0; i < storageData.recordedFrameCount; i++) {
                float frameTime = storageData.frameTimeData[i];
                _storageWriter.writeFloat(frameTime);
            }
            
            //handler data
            for (int i = 0; i < storageData.handlerData.Length; i++) {
                RewindStorageData_Handler handlerData = storageData.handlerData[i];
                
                var handlerStorage = getHandlerStorage(handlerData.id);

                int frameIndex = i % storageData.recordedFrameCount;

                //set the write head to the correct location
                _storageWriter.setWriteHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * frameIndex);

                _storageWriter.writeUInt(handlerData.id);

                //write data
                for (int v = 0; v < handlerData.values.Length; v++) {
                    RewindStorageData_Value val = handlerData.values[v];
                    switch (val.valueType) {
                    case RewindDataPointType.FLOAT:
                        _storageWriter.writeFloat(val.f);
                        break;
                    case RewindDataPointType.INT:
                        _storageWriter.writeInt(val.i);
                        break;
                    case RewindDataPointType.COLOR:
                        _storageWriter.writeColor(val.c);
                        break;
                    case RewindDataPointType.BOOL:
                        _storageWriter.writeBool(val.b);
                        break;
                    case RewindDataPointType.VECTOR3:
                        _storageWriter.writeVector3(val.v);
                        break;
                    case RewindDataPointType.QUATERNION:
                        _storageWriter.writeQuaternion(val.q);
                        break;
                    default:
                        Debug.LogError($"Type not handled {val.valueType}");
                        break;
                    }
                }
            }
        }
        
        public void loadFromJsonFile(string fullPath) {
            string jsonTxt = File.ReadAllText(fullPath);
            var storageData = JsonUtility.FromJson<RewindStorageData>(jsonTxt);
            loadFromStorage(storageData);
        }

        public void loadFromBinaryStreamFile(string fullPath) {
            using (var stream = File.Open(fullPath, FileMode.Open)) {
                using (var reader = new BinaryReader(stream)) {
                    
                    var storageData = new RewindStorageData();
                    storageData.maxFrameCount = reader.ReadInt32();
                    storageData.recordedFrameCount = reader.ReadInt32();
                    storageData.handlerCount = reader.ReadInt32();
                    storageData.version = reader.ReadInt32();

                    storageData.frameTimeData = new float[storageData.recordedFrameCount];
                    for (int i = 0; i < storageData.recordedFrameCount; i++) {
                        storageData.frameTimeData[i] = reader.ReadSingle();
                    }
                    
                    //read the handler data  
                    int dataIndex = 0;
                    var handlerData = new RewindStorageData_Handler[storageData.handlerCount*storageData.recordedFrameCount];
                    for (int i = 0; i < storageData.handlerCount; i++) {
                        for (int frameIndex = 0; frameIndex < storageData.recordedFrameCount; frameIndex++) {

                            RewindStorageData_Handler handler = new RewindStorageData_Handler();
                            handler.id = reader.ReadUInt32();
                            handler.values = new RewindStorageData_Value[reader.ReadUInt32()];
                            for (int v = 0; v < handler.values.Length; v++) {
                                RewindStorageData_Value val = new RewindStorageData_Value();
                                val.valueType = (RewindDataPointType)reader.ReadUInt32();

                                switch (val.valueType) {
                                case RewindDataPointType.FLOAT:
                                    val.f = reader.ReadSingle();
                                    break;
                                case RewindDataPointType.INT:
                                    val.i = reader.ReadInt32();
                                    break;
                                case RewindDataPointType.COLOR:
                                    val.c = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                    break;
                                case RewindDataPointType.BOOL:
                                    val.b = reader.ReadBoolean();
                                    break;
                                case RewindDataPointType.VECTOR3:
                                    val.v = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                    break;
                                case RewindDataPointType.QUATERNION:
                                    val.q = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                    break;
                                default:
                                    Debug.LogError($"Type not handled {val.valueType}");
                                    break;
                                }

                                handler.values[v] = val;
                            }
     
                            handlerData[dataIndex] = handler;
                            dataIndex++;
                        }
                    }
                    storageData.handlerData = handlerData;
                    
                    loadFromStorage(storageData);
                }
            }
        }

        public void writeToJsonFile(string fileName) {
            string jsonStr = writeToJson();
            File.WriteAllText(fileName, jsonStr);
        }

        public void loadFromRawBinaryFile(string fileName) {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                //write the header
                var version = fileStream.ReadByte();

                //read the data
                var managedArray = _nativeStorage.getManagedArray();
                var bytesRead = fileStream.Read(managedArray);
                Debug.Log($"Read {bytesRead} bytes from file");

                //copy back into native storage
                _nativeStorage.setManagedArray(managedArray);
            }

            //read the frame count
            _frameReaderA.setReadHead(0);
            RecordedFrameCount = _frameReaderA.readInt();
        }


    }
}