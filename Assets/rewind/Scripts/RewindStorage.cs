using System.Collections.Generic;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindHandlerStorage
    {
        readonly uint handlerId;
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

        private int rewindFramesCount;
        private bool supportsRewind;
        
        //Map of ID to RewindHandlerStorage
        private Dictionary<uint, RewindHandlerStorage> rewindHandlerStorageMap = new Dictionary<uint, RewindHandlerStorage>();

        private int _frameDataOffset;
        private int _handlerDataOffset;
        
        public RewindStorage(RewindScene rewindScene, int maxFrameCount, bool supportsRewind)
        {
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
            NativeByteArrayWriter writer = nativeStorage.writer;
            
            //write out the basic storage info
            writer.writeInt(maxFrameCount);
            writer.writeInt(rewindScene.RewindHandlers.Count);
            
            //write out the handler info (ids, offsets)
            writer.setWriteHead(_handlerDataOffset);
            foreach (var rewindHandler in rewindScene.RewindHandlers)
            {
                RewindHandlerStorage handlerStorage = getHandlerStorage(rewindHandler.ID);
                
                writer.writeUInt(rewindHandler.ID);
                writer.writeInt(handlerStorage.HandlerStorageOffset);
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
            nativeStorage.writer.setWriteHead(handlerStorage.HandlerStorageOffset + (handlerStorage.HandlerFrameSizeBytes * rewindFramesCount));
            
            //store ID
            //do we really need to store the ID here? we could just use the offset to determine the ID
            nativeStorage.writer.writeUInt(rewindHandler.ID);
          
            rewindHandler.rewindStore(nativeStorage.writer);
        }

        public void writeFrameStart(float frameTimeRelativeToStart)
        {
            // write frame time
            int currentFrameTimeOffset = _frameDataOffset + (4 * rewindFramesCount);
            nativeStorage.writer.setWriteHead(currentFrameTimeOffset);
            
            //frame time is the current time? time from start of recording?
            nativeStorage.writer.writeFloat(frameTimeRelativeToStart);
        }

        public void writeFrameEnd()
        {
            rewindFramesCount++;
        }
    }
}