
namespace ccl.rewind_plugin
{
    public interface IRewindHandler
    {
        uint ID { get; }
        
        void rewindStore(NativeByteArrayWriter writer);
        void rewindRestore(NativeByteArrayReader reader);
        
        int RequiredBufferSizeBytes { get; }
        uint HandlerTypeID { get; }
    }
}
