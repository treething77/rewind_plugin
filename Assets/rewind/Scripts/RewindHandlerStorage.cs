namespace aeric.rewind_plugin {
    public class RewindHandlerStorage {
        public RewindHandlerStorage(int _handlerStorageOffset, int _handlerFrameSizeBytes) {
            HandlerStorageOffset = _handlerStorageOffset;
            HandlerFrameSizeBytes = _handlerFrameSizeBytes;
        }

        public int HandlerStorageOffset { get; }

        public int HandlerFrameSizeBytes { get; }
    }
}