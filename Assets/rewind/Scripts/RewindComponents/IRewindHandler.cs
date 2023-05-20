
namespace rewind_plugin
{
    public interface IRewindHandler
    {
        void rewindStore(NativeByteArrayWriter writer);
        void rewindRestore(NativeByteArrayReader reader);
    }
}
