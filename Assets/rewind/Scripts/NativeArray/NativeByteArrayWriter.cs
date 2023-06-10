using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace aeric.rewind_plugin
{
    public class NativeByteArrayWriter
    {
        private int _writeHead;

        private NativeByteArray _nativeArray;

        public NativeByteArrayWriter(NativeByteArray nativeByteArray)
        {
            _nativeArray = nativeByteArray;
        }
        
        public void writeFloat(float f)
        {
            writeSimpleValueImpl(f);
        }
        
        public void writeByte(byte value)
        {
            writeSimpleValueImpl(value);
        }

        public void writeByteArray(byte[] bytesIn)
        {
            unsafe
            {
                fixed (byte* pByteSrc = bytesIn)
                {
                    UnsafeUtility.MemCpy((byte*)_nativeArray.GetUnsafeWritePtr() + _writeHead, pByteSrc, bytesIn.Length);
                }
            }

            int endIndex = _writeHead + bytesIn.Length;
            _writeHead = endIndex;
        }
        
        public void writeV3(Vector3 value)
        {
            writeSimpleValueImpl(value);
        }
        
        public void writeQuaternion(Quaternion value)
        {
            writeSimpleValueImpl(value);
        }
        
        private void writeSimpleValueImpl<T>(T value) where T : unmanaged
        {
            unsafe
            {
                UnsafeUtility.CopyStructureToPtr(ref value, (byte*)_nativeArray.GetUnsafeWritePtr() + _writeHead);

                int valueSizeBytes = sizeof(T);
                int endIndex = _writeHead + valueSizeBytes;
                _writeHead = endIndex;
            }
        }

        public void writeInt(int value)
        {
            writeSimpleValueImpl(value);
        }
        
        public void writeBool(bool value)
        {
            writeSimpleValueImpl(value);
        }

        public void writeColor(Color value)
        {
            writeSimpleValueImpl(value);
        }

        public void writeUInt(uint value)
        {
            writeSimpleValueImpl(value);
        }

        public void setWriteHead(int writeOffset)
        {
            _writeHead = writeOffset;
            
            //TODO: validate its within the buffer
        }
    }
}
