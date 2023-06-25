using System.IO;
using aeric.rewind_plugin.RewindStorageDataTypes;
using UnityEngine;

namespace aeric.rewind_plugin {
    public partial class RewindStorage {

        public void writeToBinaryStreamFile(string fileName) {
            var storageData = convertToStorage();
            
            using (var fileStream = new FileStream(fileName, FileMode.Create)) {
                using (var binaryWriter = new BinaryWriter(fileStream)) {
                    //write the header
                    binaryWriter.Write(storageData.maxFrameCount);
                    binaryWriter.Write(storageData.recordedFrameCount);
                    binaryWriter.Write(storageData.handlerCount);
                    binaryWriter.Write(storageData.version);
                    
                    //write the frame times
                    for (int i = 0; i < storageData.recordedFrameCount; i++) {
                        binaryWriter.Write(storageData.frameTimeData[i]);
                    }
                    
                    //write the handler data
                    for (int i = 0; i < storageData.handlerData.Length; i++) {
                        RewindStorageData_Handler handlerData = storageData.handlerData[i];
                        binaryWriter.Write(handlerData.id);
                        binaryWriter.Write(handlerData.values.Length);
                        for (int v = 0; v < handlerData.values.Length; v++) {
                            RewindStorageData_Value val = handlerData.values[v];
                            binaryWriter.Write((int) val.valueType);
                            switch (val.valueType) {
                            case RewindDataPointType.FLOAT:
                                binaryWriter.Write(val.f);
                                break;
                            case RewindDataPointType.INT:
                                binaryWriter.Write(val.i);
                                break;
                            case RewindDataPointType.COLOR:
                                binaryWriter.Write(val.c.r);
                                binaryWriter.Write(val.c.g);
                                binaryWriter.Write(val.c.b);
                                binaryWriter.Write(val.c.a);
                                break;
                            case RewindDataPointType.BOOL:
                                binaryWriter.Write(val.b);
                                break;
                            case RewindDataPointType.VECTOR3:
                                binaryWriter.Write(val.v.x);
                                binaryWriter.Write(val.v.y);
                                binaryWriter.Write(val.v.z);
                                break;
                            case RewindDataPointType.QUATERNION:
                                binaryWriter.Write(val.q.x);
                                binaryWriter.Write(val.q.y);
                                binaryWriter.Write(val.q.z);
                                binaryWriter.Write(val.q.w);
                                break;
                            default:
                                Debug.LogError($"Type not handled {val.valueType}");
                                break;
                            }
                        }
                    }
                }
            }
        }
       
        
        public void loadFromBinaryStreamFile(string fullPath) {
            using (var stream = File.Open(fullPath, FileMode.Open)) {
                using (var reader = new BinaryReader(stream)) {
                    
                    var storageData = new RewindStorageData();
                    storageData.maxFrameCount = reader.ReadInt32();
                    storageData.recordedFrameCount = reader.ReadInt32();
                    storageData.handlerCount = reader.ReadInt32();
                    storageData.version = reader.ReadInt32();

                    storageData.frameTimeData = new float[storageData.recordedFrameCount];
                    for (int i = 0; i < storageData.recordedFrameCount; i++) {
                        storageData.frameTimeData[i] = reader.ReadSingle();
                    }
                    
                    //read the handler data  
                    int dataIndex = 0;
                    var handlerData = new RewindStorageData_Handler[storageData.handlerCount*storageData.recordedFrameCount];
                    for (int i = 0; i < storageData.handlerCount; i++) {
                        for (int frameIndex = 0; frameIndex < storageData.recordedFrameCount; frameIndex++) {

                            RewindStorageData_Handler handler = new RewindStorageData_Handler();
                            handler.id = reader.ReadUInt32();
                            handler.values = new RewindStorageData_Value[reader.ReadUInt32()];
                            for (int v = 0; v < handler.values.Length; v++) {
                                RewindStorageData_Value val = new RewindStorageData_Value();
                                val.valueType = (RewindDataPointType)reader.ReadUInt32();

                                switch (val.valueType) {
                                case RewindDataPointType.FLOAT:
                                    val.f = reader.ReadSingle();
                                    break;
                                case RewindDataPointType.INT:
                                    val.i = reader.ReadInt32();
                                    break;
                                case RewindDataPointType.COLOR:
                                    val.c = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                    break;
                                case RewindDataPointType.BOOL:
                                    val.b = reader.ReadBoolean();
                                    break;
                                case RewindDataPointType.VECTOR3:
                                    val.v = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                    break;
                                case RewindDataPointType.QUATERNION:
                                    val.q = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                    break;
                                default:
                                    Debug.LogError($"Type not handled {val.valueType}");
                                    break;
                                }

                                handler.values[v] = val;
                            }
     
                            handlerData[dataIndex] = handler;
                            dataIndex++;
                        }
                    }
                    storageData.handlerData = handlerData;
                    
                    loadFromStorage(storageData);
                }
            }
        }


    }
}