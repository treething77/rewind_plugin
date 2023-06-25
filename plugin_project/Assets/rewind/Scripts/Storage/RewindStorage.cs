using System.Collections.Generic;
using UnityEngine;

namespace aeric.rewind_plugin {
    public enum RewindMappedFrame {}

    public partial class RewindStorage {
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
            if (!_nativeStorage._isDisposed) _nativeStorage.Dispose();
        }

        public void Dispose() {
            if (!_nativeStorage._isDisposed) _nativeStorage.Dispose();
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
            if (RecordedFrameCount == 1) {
                var startTimeIndex = remapIndex(0);
                return (startTimeIndex, startTimeIndex, 0.0f, 0, 0);
            }

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
    }
}