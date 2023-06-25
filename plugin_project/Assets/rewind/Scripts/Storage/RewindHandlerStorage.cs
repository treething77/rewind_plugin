namespace aeric.rewind_plugin {
    /// <summary>
    /// Stores information about how a handlers data is stored in the native array
    /// </summary>
    public class RewindHandlerStorage {
        public RewindHandlerStorage(int handlerStorageOffset, int handlerFrameSizeBytes) {
            HandlerStorageOffset = handlerStorageOffset;
            HandlerFrameSizeBytes = handlerFrameSizeBytes;
        }

        public int HandlerStorageOffset { get; }

        public int HandlerFrameSizeBytes { get; }
    }
}