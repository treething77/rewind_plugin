using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace aeric.rewind_plugin {
    public class NativeByteArrayReader {
        private readonly NativeByteArray _nativeArray;
        private int _readHead;

        public NativeByteArrayReader(NativeByteArray nativeByteArray) {
            _nativeArray = nativeByteArray;
        }

        private T readValue<T>() where T : unmanaged {
            unsafe {
                UnsafeUtility.CopyPtrToStructure((byte*)_nativeArray.GetUnsafeReadPtr() + _readHead, out T value);

                var valueSizeBytes = sizeof(T);
                var endIndex = _readHead + valueSizeBytes;
                _readHead = endIndex;

                return value;
            }
        }

        public unsafe T* getReadHeadDataPtr<T>() where T : unmanaged {
            return (T*)((byte*)_nativeArray.GetUnsafeReadPtr() + _readHead);
        }

        public float readFloat() {
            return readValue<float>();
        }

        public int readInt() {
            return readValue<int>();
        }

        public bool readBool() {
            return readValue<bool>();
        }

        public byte readByte() {
            return readValue<byte>();
        }

        public Vector3 readVector3() {
            return readValue<Vector3>();
        }

        public Quaternion readQuaternion() {
            return readValue<Quaternion>();
        }

        public Color readColor() {
            return readValue<Color>();
        }

        public uint readUInt() {
            return readValue<uint>();
        }

        public void setReadHead(int readOffsetBytes) {
            _readHead = readOffsetBytes;
        }
    }
}