using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public class NativeByteArrayReader
    {
        private int _readHead;

        private NativeByteArray _nativeArray;

        public NativeByteArrayReader(NativeByteArray nativeByteArray)
        {
            _nativeArray = nativeByteArray;
        }

        private T readValue<T>() where T : unmanaged
        {
            unsafe
            {
                UnsafeUtility.CopyPtrToStructure( ((byte*)_nativeArray.GetUnsafeReadPtr() + _readHead), out T value);

                int valueSizeBytes = sizeof(T);
                int endIndex = _readHead + valueSizeBytes;
                _readHead = endIndex;

                return value;
            }
        }
        
        public float readFloat()
        {
            return readValue<float>();
        }
        
        public int readInt()
        {
            return readValue<int>();
        }

        public byte readByte()
        {
            return readValue<byte>();
        }

        public Vector3 readV3()
        {
            return readValue<Vector3>();
        }

        public Quaternion readQuaternion()
        {
            return readValue<Quaternion>();
        }

        public Color readColor()
        {
            return readValue<Color>();
        }
    }
}
