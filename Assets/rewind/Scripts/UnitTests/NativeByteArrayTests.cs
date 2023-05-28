using System;
using NUnit.Framework;
using ccl.rewind_plugin;
using UnityEngine;

namespace NativeByteArrayTests
{
    public class NativeByteArrayTests
    {
        NativeByteArray nativeByteArray_large;
        NativeByteArray nativeByteArray_small;
        
        NativeByteArrayWriter nativeArraySmallWriter;
        NativeByteArrayReader nativeArraySmallReader;

        [Test]
        public void TestSliceWriteRead_SingleBytes()
        {
            byte[] bytes_in = new byte[] { 5, 7 };
            nativeArraySmallWriter.writeByteArray(bytes_in);

            byte r1 = nativeArraySmallReader.readByte();
            byte r2 = nativeArraySmallReader.readByte();
            Assert.IsTrue(r1 == 5);
            Assert.IsTrue(r2 == 7);
        }

        [Test]
        public void TestByteArrayWriteRead_SingleBytes()
        {
            byte[] bytes_in = new byte[] { 0, 1, 2, 3, 4, 5 };
            nativeArraySmallWriter.writeByteArray(bytes_in);

            for (int i = 0; i < 6; i++)
            {
                byte r = nativeArraySmallReader.readByte();
                Assert.IsTrue(r == i);
            }
        }

        [Test]
        public void TestMultipleWriteReads()
        {
            nativeArraySmallWriter.writeByte(32);
            nativeArraySmallWriter.writeFloat(3.141f);

            byte r1 = nativeArraySmallReader.readByte();
            float r2 = nativeArraySmallReader.readFloat();

            Assert.IsTrue(r1 == 32);
            Assert.IsTrue(r2 == 3.141f);
        }

        [Test]
        public void TestReadWriteVector3()
        {
            Vector3 a = new Vector3(1, 2, 3);
            nativeArraySmallWriter.writeV3(a);
            Vector3 b = nativeArraySmallReader.readV3();

            Assert.IsTrue(a == b);
        }

        [Test]
        public void TestReadWriteQuaternion()
        {
            Quaternion a = Quaternion.Euler(10,20,30);
            nativeArraySmallWriter.writeQuaternion(a);
            Quaternion b = nativeArraySmallReader.readQuaternion();

            Assert.IsTrue(a == b);
        }
        
        [Test]
        public void TestSingleWriteReadColor()
        {
            nativeArraySmallWriter.writeColor(Color.yellow);
            Color c= nativeArraySmallReader.readColor();
            Assert.IsTrue(c == Color.yellow);
        }

        [Test]
        public void TestSingleWriteReadByte()
        {
            nativeArraySmallWriter.writeByte(32);
            byte r = nativeArraySmallReader.readByte();
            Assert.IsTrue(r == 32);
        }

        [Test]
        public void TestSingleWriteReadInt()
        {
            nativeArraySmallWriter.writeInt(3);
            int r = nativeArraySmallReader.readInt();
            Assert.IsTrue(r == 3);
        }

        [Test]
        public void TestSingleWriteReadFloat()
        {
            nativeArraySmallWriter.writeFloat(3.141f);
            float r = nativeArraySmallReader.readFloat();

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            Assert.IsTrue(r == 3.141f);
        }

        [SetUp]
        public void NativeArraySetup() {
            nativeByteArray_small = new NativeByteArray(16);
            nativeByteArray_large = new NativeByteArray(1000*1000);
            
            nativeArraySmallWriter = nativeByteArray_small.writer;
            nativeArraySmallReader = nativeByteArray_small.reader;
        }

        [TearDown]
        public void NativeArrayTeardown() {
            nativeArraySmallWriter = null;
            nativeArraySmallReader = null;

            nativeByteArray_large.Dispose();
            nativeByteArray_large = null;

            nativeByteArray_small.Dispose();
            nativeByteArray_small = null;
        }
    }
}
