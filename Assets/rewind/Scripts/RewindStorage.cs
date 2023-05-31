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
        private NativeByteArray nativeStorage;

      //  private RewindHandlerStorage[] handlerStorage;
        private int rewindFramesCount;

        private bool supportsRewind;
        
        //Map of ID to RewindHandlerStorage
        private Dictionary<uint, RewindHandlerStorage> rewindHandlerStorageMap = new Dictionary<uint, RewindHandlerStorage>();
        
        public RewindStorage(RewindScene rewindScene, int maxFrameCount, bool supportsRewind)
        {
            // Storage size calculation:
            // -sum of the required space for all rewind handlers
            // -plus any required space for bookkeeping of rewind handlers
            // -and then double it to account for certain usage scenarios
            //   -reverse playback of 1 or more objects
            
            int bufferSizeBytes = 0;
            
            foreach (var rewindHandler in rewindScene.RewindHandlers)
            {
                int bufferHandlerStartOffset = bufferSizeBytes;
                
                int handlerFrameSizeBytes = rewindHandler.RequiredBufferSizeBytes;
                
                //add space for bookkeeping data
                handlerFrameSizeBytes += 4;//sentinel
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
            
            //Each rewind handler will be assigned 1 portion of this buffer
            //that will be a contiguous memory area for that handler to use
            // <|--component A--|--component B --| -- etc >
            //vs an interleaved approach
            // <|A1|B1|A2|B2|A3|B3>
            //This allows us to easily support:
            // -storing different objects at different rates
            // -playback/rewind of individual objects
            
            //Need some kind of structure per component that allows us to look up
            //by ID, and then to go frame by frame, or index into specific frames
            
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
            nativeStorage.writer.writeUInt(rewindHandler.ID);
          
            rewindHandler.rewindStore(nativeStorage.writer);
        }

        public void writeFrameStart()
        {
  
        }

        public void writeFrameEnd()
        {
            rewindFramesCount++;
        }
    }
}