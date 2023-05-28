using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ccl.rewind_plugin
{
    public class NativeByteArray
    {
        private NativeArray<byte> _nativeBuffer;

        private NativeByteArrayWriter _writer;
        private NativeByteArrayReader _reader;

        public NativeByteArray(int bufferSizeBytes)
        {
            _nativeBuffer = new NativeArray<byte>(bufferSizeBytes, Allocator.Persistent);
            _writer = new NativeByteArrayWriter(this);
            _reader = new NativeByteArrayReader(this);
        }

        public NativeByteArrayWriter writer => _writer;
        public NativeByteArrayReader reader => _reader;

        public int Length => _nativeBuffer.Length;

        public void Dispose()
        {
            _nativeBuffer.Dispose();
        }
        
        public unsafe void* GetUnsafeWritePtr()
        {
            return _nativeBuffer.GetUnsafePtr();
        }

        public unsafe void* GetUnsafeReadPtr()
        {
            return _nativeBuffer.GetUnsafeReadOnlyPtr();
        }
    }
}
