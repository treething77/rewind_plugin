using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindHandlerStorage
    {
        readonly int handlerStorageOffset;
        readonly int handlerFrameSizeBytes;

        public RewindHandlerStorage(int _handlerStorageOffset, int _handlerFrameSizeBytes)
        {
            handlerStorageOffset = _handlerStorageOffset;
            handlerFrameSizeBytes = _handlerFrameSizeBytes;
        }

        public int HandlerStorageOffset => handlerStorageOffset;
        public int HandlerFrameSizeBytes => handlerFrameSizeBytes;
    }

    public enum RewindMappedFrame : int
    {
    }

    public class RewindStorage
    {
        private readonly NativeByteArray nativeStorage;

        private readonly NativeByteArrayWriter storageWriter;
        
        private readonly NativeByteArrayReader frameReaderA;
        private readonly NativeByteArrayReader frameReaderB;

        private int rewindFramesCount;

        private int rewindWriteHead => (rewindReadHead + rewindFramesCount) % _maxFrameCount;
        private int rewindReadHead;
        
        private bool supportsRewind;
        
        //Map of ID to RewindHandlerStorage
        private Dictionary<uint, RewindHandlerStorage> rewindHandlerStorageMap = new Dictionary<uint, RewindHandlerStorage>();

        private int _frameDataOffset;
        private int _handlerDataOffset;
        private readonly int _maxFrameCount;

        public bool isFull => rewindFramesCount == _maxFrameCount;

        public RewindStorage(RewindScene rewindScene, int maxFrameCount, bool supportsRewind)
        {
            _maxFrameCount = maxFrameCount;
            
            // Storage size calculation:
            // -sum of the required space for all rewind handlers
            // -plus any required space for bookkeeping of rewind handlers
            // -and then double it to account for certain usage scenarios
            //   -reverse playback of 1 or more objects
            
            //Each rewind handler will be assigned 1 portion of this buffer
            //that will be a contiguous memory area for that handler to use
            // <|--component A--|--component B --| -- etc >
            //vs an interleaved approach
            // <|A1|B1|A2|B2|A3|B3>
            //This allows us to easily support:
            // -storing different objects at different rates
            // -playback/rewind of individual objects
            // -playback/rewind of all objects
            
            int bufferSizeBytes = 0;
            
            //Allocate the storage for the recording info
            //frame count
            bufferSizeBytes += 4;
            //handler count
            bufferSizeBytes += 4;

            _frameDataOffset = bufferSizeBytes;
            //Allocate the storage for the frame info
            //timestamps
            bufferSizeBytes += 4 * maxFrameCount;

            _handlerDataOffset = bufferSizeBytes;
            //Allocate the storage for the handler storage info
            //ids
            bufferSizeBytes += 8 * rewindScene.RewindHandlers.Count;
            //offsets
            bufferSizeBytes += 4 * rewindScene.RewindHandlers.Count;
            
            foreach (var rewindHandler in rewindScene.RewindHandlers)
            {
                int bufferHandlerStartOffset = bufferSizeBytes;
                
                int handlerFrameSizeBytes = rewindHandler.RequiredBufferSizeBytes;
                
                //add space for bookkeeping data
                handlerFrameSizeBytes += 8;//ID

                int handlerStorageSizeBytes = handlerFrameSizeBytes * maxFrameCount;
                
                if (supportsRewind)
                {
                    handlerStorageSizeBytes *= 2;
                }

                RewindHandlerStorage handlerStorage = new RewindHandlerStorage(bufferHandlerStartOffset, handlerFrameSizeBytes);
                
                rewindHandlerStorageMap.Add(rewindHandler.ID, handlerStorage);

                bufferSizeBytes += handlerStorageSizeBytes;
            }
           
            nativeStorage = new NativeByteArray(bufferSizeBytes);
            storageWriter = new NativeByteArrayWriter(nativeStorage);

            frameReaderA = new NativeByteArrayReader(nativeStorage);
            frameReaderB = new NativeByteArrayReader(nativeStorage);
            
            //write out the basic storage info
            storageWriter.writeInt(maxFrameCount);
            storageWriter.writeInt(rewindScene.RewindHandlers.Count);
            
            //write out the handler info (ids, offsets)
            storageWriter.setWriteHead(_handlerDataOffset);
            foreach (var rewindHandler in rewindScene.RewindHandlers)
            {
                RewindHandlerStorage handlerStorage = getHandlerStorage(rewindHandler.ID);
                
                storageWriter.writeUInt(rewindHandler.ID);
                storageWriter.writeInt(handlerStorage.HandlerStorageOffset);
            }
        }

        // Destructor
        ~RewindStorage()
        {
            if (!nativeStorage.isDisposed)
            {
                nativeStorage.Dispose();
            }
        }

        public void Dispose()
        {
            if (!nativeStorage.isDisposed)
            {
                nativeStorage.Dispose();
            }
        }
        
        public int RecordedFrameCount => rewindFramesCount;
        public int FrameReadIndex => rewindReadHead;
        public int FrameWriteIndex => rewindWriteHead;

        public RewindHandlerStorage getHandlerStorage(uint rewindHandlerID)
        {
            RewindHandlerStorage storage = null;
            if (!rewindHandlerStorageMap.TryGetValue(rewindHandlerID, out storage))
            {
                Debug.LogError($"We do not have storage for handler {rewindHandlerID} was it registered when the scene was created?");
            }
            return storage;
        }

        public void writeHandlerFrame(IRewindHandler rewindHandler)
        {
           // Debug.Log($" Writing frame {rewindWriteHead}");

            RewindHandlerStorage handlerStorage = getHandlerStorage(rewindHandler.ID);
            
            //set the write head to the correct location
            storageWriter.setWriteHead(handlerStorage.HandlerStorageOffset + (handlerStorage.HandlerFrameSizeBytes * rewindWriteHead));
            
            //store ID
            //do we really need to store the ID here? we could just use the offset to determine the ID
            storageWriter.writeUInt(rewindHandler.ID);
          
            rewindHandler.rewindStore(storageWriter);
        }

        public void writeFrameStart(float frameTimeRelativeToStart)
        {
            // write frame time
            int currentFrameTimeOffset = _frameDataOffset + (4 * rewindWriteHead);
            storageWriter.setWriteHead(currentFrameTimeOffset);
            
            //frame time is the current time? time from start of recording?
            storageWriter.writeFloat(frameTimeRelativeToStart);
        }

        public void writeFrameEnd()
        {
            //Clamp the number of frames recorded at the max

            //write head is implicit so moves when we increase frame count
            rewindFramesCount++;
            if (rewindFramesCount > _maxFrameCount)
            {
                rewindFramesCount = _maxFrameCount;
                
                //move the read head if the buffer is full
                rewindReadHead++;
                if (rewindReadHead >= _maxFrameCount) rewindReadHead = 0;
            }
        }

        public (RewindMappedFrame frameMappedA, RewindMappedFrame frameMappedB,float frameT, int frameUnmappedA, int frameUnmappedB) findPlaybackFrames(float playbackTime)
        {
            //if only 1 frame then use it for both
            //   i.e. we always interpolate between 2 frames
            if (rewindFramesCount == 1)
            {
                //TODO: if we rewound then the first frame wont necessarily be at 0
                return (0, 0, 0.0f, 0, 0);
            }
        
            RewindMappedFrame frameMappedA = 0;
            RewindMappedFrame frameMappedB = 0;
            int frameA = 0;
            int frameB = 0;
            float frameT = 0;

            frameReaderA.setReadHead(_frameDataOffset);

            unsafe
            {
                float* pTimes = frameReaderA.getReadHeadDataPtr<float>();
                //Have to handle the continuous recording case
                //One way to do that would be to remap the indices
                
                //currently we search between 0 and rewindFrameCount-1
                // [0, rewindFrameCount-1]
                // 0 -> rewindFrameWriteIndex
                // rewindFrameCount-1 -> (rewindFrameWriteIndex + rewindFrameCount-1) % maxFrameCount
                RewindMappedFrame startTimeIndex = remapIndex(0);
                RewindMappedFrame endTimeIndex = remapIndex(rewindFramesCount - 1);

                //special cases, before start time or after end time
                if (playbackTime <= (pTimes[(int)startTimeIndex]))
                {
                    return (startTimeIndex, startTimeIndex, 0.0f, 0, 0);
                }
                if (playbackTime >= (pTimes[(int)endTimeIndex]))
                {
                    return (endTimeIndex, endTimeIndex, 1.0f, rewindFramesCount-1, rewindFramesCount-1);
                }

                //do a linear search
                //start at the beginning and work our way along until the next time is past our playbackTime
                int frame = 0;
                while (frame < rewindFramesCount)
                {
                    RewindMappedFrame remappedFrame = remapIndex(frame);
                    if (pTimes[(int) remappedFrame] >= playbackTime)
                    {
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
                float frameTimeA = pTimes[(int)frameMappedA];
                float frameTimeB = pTimes[(int)frameMappedB];
                
                frameT = (playbackTime - frameTimeA) / (frameTimeB - frameTimeA);
                
                Debug.Assert(frameT >= 0.0f);
                Debug.Assert(frameT <= 1.0f);
            }

            return (frameMappedA, frameMappedB, frameT, frameA, frameB);
        }

        /// <summary>
        /// Takes in a "normalized" frame index, where the range is always [0, frameCount-1]
        /// and maps it onto the buffer taking into account wrapping.
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <returns></returns>
        private RewindMappedFrame remapIndex(int frameIndex)
        {
            //is buffer full yet?
            if (rewindFramesCount < _maxFrameCount) return (RewindMappedFrame)frameIndex;
            
            int mappedFrameIndex = (rewindReadHead + frameIndex) % _maxFrameCount;
            return (RewindMappedFrame)mappedFrameIndex;
        }

        public void restoreHandlerInterpolated(IRewindHandler rewindHandler, RewindMappedFrame frameA, RewindMappedFrame frameB, float frameT)
        {
            RewindHandlerStorage handlerStorage = getHandlerStorage(rewindHandler.ID);

            int frameIndexA = (int)(frameA);
            int frameIndexB = (int)(frameB);
            
            //set the read head to the correct location
            frameReaderA.setReadHead(handlerStorage.HandlerStorageOffset + (handlerStorage.HandlerFrameSizeBytes * frameIndexA));
            frameReaderB.setReadHead(handlerStorage.HandlerStorageOffset + (handlerStorage.HandlerFrameSizeBytes * frameIndexB));

            uint handlerIDA = frameReaderA.readUInt();
            uint handlerIDB = frameReaderB.readUInt();
            
            //sanity check
            Debug.Assert(handlerIDA == handlerIDB);
            Debug.Assert(handlerIDA == rewindHandler.ID);
            
            rewindHandler.preRestore();

            //read the data from the 2 frames
            rewindHandler.rewindRestoreInterpolated(frameReaderA, frameReaderB, frameT);
            
            rewindHandler.postRestore();
        }

        public void writeToFile(string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                //write the header
                fileStream.WriteByte(1);//v1
                
                //write the data
                byte[] managedArray = nativeStorage.getManagedArray();
                fileStream.Write(managedArray);
            }
        }

        public void loadFromFile(string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                //write the header
                int version = fileStream.ReadByte();
                
                //read the data
                byte[] managedArray = nativeStorage.getManagedArray();
                int bytesRead = fileStream.Read(managedArray);
                Debug.Log($"Read {bytesRead} bytes from file");
                
                //copy back into native storage
                nativeStorage.setManagedArray(managedArray);
            }
            
            //read the frame count
            frameReaderA.setReadHead(0);
            rewindFramesCount = frameReaderA.readInt();
        }

        public float getTime(int timeFrameIndex)
        {
            unsafe
            {
                frameReaderA.setReadHead(_frameDataOffset);
                float* pTimes = frameReaderA.getReadHeadDataPtr<float>();
                RewindMappedFrame mappedTimeIndex = remapIndex(timeFrameIndex);
                return pTimes[(int)mappedTimeIndex];
            }
        }

        public void rewindFrames(int frameCountToRewind)
        {
            frameCountToRewind = Mathf.Min(frameCountToRewind, rewindFramesCount);

            Debug.Log($"Rewinding {frameCountToRewind} frames");
            
            //the READ head does not move (start of valid data)
            //the WRITE head is implicit so moves when we set frame count
            rewindFramesCount -= frameCountToRewind;
            
            //rewindFrameWriteIndex -= frameCountToRewind;
            //if (rewindFrameWriteIndex < 0) rewindFrameWriteIndex += _maxFrameCount;
        }

        public float getFrameTime(int unmappedFrameIndex)
        {
            unsafe
            {
                frameReaderA.setReadHead(_frameDataOffset);
                float* pTimes = frameReaderA.getReadHeadDataPtr<float>();
                return pTimes[unmappedFrameIndex];
            }
        }

        public Vector3 getFramePosition(int unmappedFrameIndex, IRewindHandler rewindHandler)
        {
            RewindHandlerStorage handlerStorage = getHandlerStorage(rewindHandler.ID);
            
            //set the read head to the correct location
            frameReaderA.setReadHead(handlerStorage.HandlerStorageOffset + (handlerStorage.HandlerFrameSizeBytes * unmappedFrameIndex));
            uint handlerIDA = frameReaderA.readUInt();

            //assume the handler is RewindTransform and read the position
            return frameReaderA.readV3();
        }
    }
}