
namespace aeric.rewind_plugin
{
    public interface IRewindHandler
    {
        uint ID { get; set; }

        void rewindStore(NativeByteArrayWriter writer);
   //     void rewindRestore(NativeByteArrayReader reader);
        
        int RequiredBufferSizeBytes { get; }
        uint HandlerTypeID { get; }
        void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT);

        void preRestore();//callback when values have been restored
        void postRestore();//callback when values have been restored
    }
}
