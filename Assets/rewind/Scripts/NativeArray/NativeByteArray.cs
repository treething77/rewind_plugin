using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace aeric.rewind_plugin {
    public class NativeByteArray {
        private NativeArray<byte> _nativeBuffer;
        public bool isDisposed;

        public NativeByteArray(int bufferSizeBytes) {
            _nativeBuffer = new NativeArray<byte>(bufferSizeBytes, Allocator.Persistent);
        }

        public int Length => _nativeBuffer.Length;

        public void Dispose() {
            _nativeBuffer.Dispose();
            isDisposed = true;
        }

        public unsafe void* GetUnsafeWritePtr() {
            return _nativeBuffer.GetUnsafePtr();
        }

        public unsafe void* GetUnsafeReadPtr() {
            return _nativeBuffer.GetUnsafeReadOnlyPtr();
        }

        public byte[] getManagedArray() {
            var managedArray = new byte[Length];
            _nativeBuffer.CopyTo(managedArray);
            return managedArray;
        }

        public void setManagedArray(byte[] managedArray) {
            _nativeBuffer.CopyFrom(managedArray);
        }
    }
}