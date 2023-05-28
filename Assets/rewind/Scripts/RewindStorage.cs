using System.Collections.Generic;

namespace ccl.rewind_plugin
{
    public class RewindStorage
    {
        private NativeByteArray nativeStorage;

        struct RewindHandlerStorage
        {
            private int rewindHandlerID;
            int rewindHandlerStorageOffset;
            private int dataFrameSizeBytes;
        }

        private RewindHandlerStorage[] handlerStorage;
        private int rewindFramesCount;
        
        //Map of ID to RewindHandlerStorage
        private Dictionary<int, RewindHandlerStorage> rewindHandlerStorage;
        
        public RewindStorage(RewindScene rewindScene)
        {
            // Storage size calculation:
            // -sum of the required space for all rewind handlers
            // -plus any required space for bookkeeping of rewind handlers
            // -and then double it to account for certain usage scenarios
            //   -reverse playback of 1 or more objects
            
            int bufferSizeBytes = 0;
            
            foreach (var rewindHandler in rewindScene.RewindHandlers)
            {
                bufferSizeBytes += rewindHandler.RequiredBufferSizeBytes;
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
    }
}