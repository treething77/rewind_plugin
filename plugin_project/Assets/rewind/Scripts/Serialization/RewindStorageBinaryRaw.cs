using System.IO;
using UnityEngine;

namespace aeric.rewind_plugin {
    public partial class RewindStorage {
        public void writeToRawBinaryFile(string fileName) {
            using (var fileStream = new FileStream(fileName, FileMode.Create)) {
                //write the header
                fileStream.WriteByte(1); //v1

                //write the data
                var managedArray = _nativeStorage.getManagedArray();
                fileStream.Write(managedArray);
            }
        }

        public void loadFromRawBinaryFile(string fileName) {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                //write the header
                var version = fileStream.ReadByte();

                //read the data
                var managedArray = _nativeStorage.getManagedArray();
                var bytesRead = fileStream.Read(managedArray);
                Debug.Log($"Read {bytesRead} bytes from file");

                //copy back into native storage
                _nativeStorage.setManagedArray(managedArray);
            }

            //read the frame count
            _frameReaderA.setReadHead(0);
            RecordedFrameCount = _frameReaderA.readInt();
        }
    }
}