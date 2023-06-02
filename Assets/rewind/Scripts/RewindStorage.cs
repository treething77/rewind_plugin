using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindHandlerStorage
    {
        readonly uint handlerId;//TODO: remove this, not needed
        readonly int handlerStorageOffset;
        readonly int handlerFrameSizeBytes;

        public RewindHandlerStorage(uint _handlerId, int _handlerStorageOffset, int _handlerFrameSizeBytes)
        {
            handlerId = _handlerId;
            handlerStorageOffset = _handlerStorageOffset;
            handlerFrameSizeBytes = _handlerFrameSizeBytes;
        }

        public int HandlerStorageOffset => handlerStorageOffset;
        public int HandlerFrameSizeBytes => handlerFrameSizeBytes;
    }
    
    public class RewindStorage
    {
        private readonly NativeByteArray nativeStorage;

        private readonly NativeByteArrayWriter storageWriter;
        
        private readonly NativeByteArrayReader frameReaderA;
        private readonly NativeByteArrayReader frameReaderB;

        private int rewindFramesCount;
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

                RewindHandlerStorage handlerStorage = new RewindHandlerStorage(rewindHandler.ID, bufferHandlerStartOffset, handlerFrameSizeBytes);
                
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

        public int RecordedFrameCount => rewindFramesCount;

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
            RewindHandlerStorage handlerStorage = getHandlerStorage(rewindHandler.ID);
            
            //set the write head to the correct location
            storageWriter.setWriteHead(handlerStorage.HandlerStorageOffset + (handlerStorage.HandlerFrameSizeBytes * rewindFramesCount));
            
            //store ID
            //do we really need to store the ID here? we could just use the offset to determine the ID
            storageWriter.writeUInt(rewindHandler.ID);
          
            rewindHandler.rewindStore(storageWriter);
        }

        public void writeFrameStart(float frameTimeRelativeToStart)
        {
            // write frame time
            int currentFrameTimeOffset = _frameDataOffset + (4 * rewindFramesCount);
            storageWriter.setWriteHead(currentFrameTimeOffset);
            
            //frame time is the current time? time from start of recording?
            storageWriter.writeFloat(frameTimeRelativeToStart);
        }

        public void writeFrameEnd()
        {
            rewindFramesCount++;
        }

        public (int frameA,int frameB,float frameT) findPlaybackFrames(float playbackTimeRelative)
        {
            //if only 1 frame then use it for both
            //   i.e. we always interpolate between 2 frames
            if (rewindFramesCount == 1)
            {
                return (0, 0, 0.0f);
            }
        
            int frameA = 0;
            int frameB = 0;
            float frameT = 0;

            frameReaderA.setReadHead(_frameDataOffset);

            unsafe
            {
                float* pTimes = frameReaderA.getDataPtr<float>();
                
                //special cases, before start time or after end time
                if (playbackTimeRelative < pTimes[0])
                {
                    return (0, 0, 0.0f);
                }
                if (playbackTimeRelative > pTimes[rewindFramesCount - 1])
                {
                    return (rewindFramesCount - 1, rewindFramesCount - 1, 0.0f);
                }
                
                //the list is sorted so perform a binary search to find the frames before and after the specified time
                int low = 0;
                int high = rewindFramesCount - 1;
                
                while (low <= high)
                {
                    int mid = (low + high) / 2;
                    float midVal = pTimes[mid];
                    if (midVal < playbackTimeRelative)
                    {
                        low = mid + 1;
                    }
                    else if (midVal > playbackTimeRelative)
                    {
                        high = mid - 1;
                    }
                    else
                    {
                        //exact match
                        frameA = mid;
                        frameB = mid;
                        frameT = 0.0f;
                        return (frameA, frameB, frameT);
                    }
                }

                frameA = low;
                frameB = high;
                
                //frameT should be the normalized value between the two frame times
                //  i.e. 0.0f = frameA, 1.0f = frameB
                //  so we need to calculate the normalized value between the two frame times
                //  and then subtract the frameA time from it
                frameT = (playbackTimeRelative - pTimes[frameA]) / (pTimes[frameB] - pTimes[frameA]);
                Debug.Assert(frameT >= 0.0f);
                Debug.Assert(frameT <= 1.0f);
            }

            return (frameA, frameB, frameT);
        }

        public void restoreHandlerInterpolated(IRewindHandler rewindHandler, int frameA, int frameB, float frameT)
        {
            RewindHandlerStorage handlerStorage = getHandlerStorage(rewindHandler.ID);
            
            //set the read head to the correct location
            frameReaderA.setReadHead(handlerStorage.HandlerStorageOffset + (handlerStorage.HandlerFrameSizeBytes * frameA));
            frameReaderB.setReadHead(handlerStorage.HandlerStorageOffset + (handlerStorage.HandlerFrameSizeBytes * frameB));

            uint handlerIDA = frameReaderA.readUInt();
            uint handlerIDB = frameReaderB.readUInt();
            
            //sanity check
            Debug.Assert(handlerIDA == handlerIDB);
            Debug.Assert(handlerIDA == rewindHandler.ID);
            
            //read the data from the 2 frames
            rewindHandler.rewindRestoreInterpolated(frameReaderA, frameReaderB, frameT);
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
    }
}