namespace rewind_plugin
{
    public class RewindStorage
    {
        private NativeByteArray nativeStorage;
        
        public RewindStorage(RewindScene rewindScene)
        {
            int bufferSizeBytes = 0;
            
            foreach (var rewindHandler in rewindScene.RewindHandlers)
            {
                bufferSizeBytes += rewindHandler.RequiredBufferSizeBytes;
            }
            
            nativeStorage = new NativeByteArray(bufferSizeBytes);
        }
    }
}