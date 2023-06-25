using System.Collections.Generic;
using System.Linq;
using aeric.rewind_plugin.RewindStorageDataTypes;
using UnityEngine;

namespace aeric.rewind_plugin {
    public partial class RewindStorage {
        private RewindStorageData convertToStorage() {
            
            var storageData = new RewindStorageData();
            storageData.maxFrameCount = _maxFrameCount;
            storageData.recordedFrameCount = RecordedFrameCount;
             
            _frameReaderA.setReadHead(0);
            _frameReaderA.readInt();
            storageData.handlerCount =  _frameReaderA.readInt();
            
            //read head?
            
            //convert the  contents of the NativeArray to json using the schema of each component

            _frameReaderA.setReadHead(_frameDataOffset);
            //frame data
            storageData.frameTimeData = new float[storageData.recordedFrameCount];
            for (int i = 0; i < storageData.recordedFrameCount; i++) {
                float frameTime = _frameReaderA.readFloat();
                storageData.frameTimeData[i] = frameTime;
            }
            
            //handler data
            storageData.handlerData = new RewindStorageData_Handler[storageData.handlerCount * storageData.recordedFrameCount];

            int dataIndex = 0;
            for (int i = 0; i < storageData.handlerCount; i++) {
                KeyValuePair<uint, RewindHandlerStorage> handlerStoragePair = rewindHandlerStorageMap.ElementAt(i);

                var handlerID = handlerStoragePair.Key;
                var handlerStorage = handlerStoragePair.Value;

                for (int frame = 0; frame < storageData.recordedFrameCount; frame++) {
                    RewindMappedFrame mappedFrame = remapIndex(frame);
                    _frameReaderA.setReadHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * (int)mappedFrame);

                    uint id = _frameReaderA.readUInt();//id  
                    Debug.Assert(id == handlerID);
                    
                    IRewindHandler handler = _scene.getHandler(handlerID);
                    
                    RewindDataSchema dataSchema = handler.makeDataSchema();

                    RewindStorageData_Handler data = new RewindStorageData_Handler();
                    data.id = handlerID;

                    List<RewindDataPoint> schema = dataSchema.getSchema();
                    data.values = new RewindStorageData_Value[dataSchema.GetValueCount()];

                    int valueIndex = 0;
                    for (int v = 0; v < schema.Count; v++) {
                        RewindDataPoint schemaDataPoint = schema[v];

                        for (int vi = 0; vi < schemaDataPoint._count; vi++) {
                            RewindStorageData_Value val = new RewindStorageData_Value();
                            val.valueType = schemaDataPoint._type;
                            switch (val.valueType) {
                            case RewindDataPointType.FLOAT:
                                val.f = _frameReaderA.readFloat();
                                break;
                            case RewindDataPointType.INT:
                                val.i = _frameReaderA.readInt();
                                break;
                            case RewindDataPointType.COLOR:
                                val.c = _frameReaderA.readColor();
                                break;
                            case RewindDataPointType.BOOL:
                                val.b = _frameReaderA.readBool();
                                break;
                            case RewindDataPointType.VECTOR3:
                                val.v = _frameReaderA.readVector3();
                                break;
                            case RewindDataPointType.QUATERNION:
                                val.q = _frameReaderA.readQuaternion();
                                break;
                            default:
                                Debug.LogError($"Type not handled {val.valueType}");
                                break;
                            }
                            
                            data.values[valueIndex] = val;
                            valueIndex++;
                        }
                    }
                    
                    storageData.handlerData[dataIndex] = data;
                    dataIndex++;
                }
            }

            return storageData;
        }
 
        private void loadFromStorage(RewindStorageData storageData) {
            RecordedFrameCount = storageData.recordedFrameCount;
            
            //frame times
            _storageWriter.setWriteHead(_frameDataOffset);

            for (int i = 0; i < storageData.recordedFrameCount; i++) {
                float frameTime = storageData.frameTimeData[i];
                _storageWriter.writeFloat(frameTime);
            }
            
            //handler data
            for (int i = 0; i < storageData.handlerData.Length; i++) {
                RewindStorageData_Handler handlerData = storageData.handlerData[i];
                
                var handlerStorage = getHandlerStorage(handlerData.id);

                int frameIndex = i % storageData.recordedFrameCount;

                //set the write head to the correct location
                _storageWriter.setWriteHead(handlerStorage.HandlerStorageOffset + handlerStorage.HandlerFrameSizeBytes * frameIndex);

                _storageWriter.writeUInt(handlerData.id);

                //write data
                for (int v = 0; v < handlerData.values.Length; v++) {
                    RewindStorageData_Value val = handlerData.values[v];
                    switch (val.valueType) {
                    case RewindDataPointType.FLOAT:
                        _storageWriter.writeFloat(val.f);
                        break;
                    case RewindDataPointType.INT:
                        _storageWriter.writeInt(val.i);
                        break;
                    case RewindDataPointType.COLOR:
                        _storageWriter.writeColor(val.c);
                        break;
                    case RewindDataPointType.BOOL:
                        _storageWriter.writeBool(val.b);
                        break;
                    case RewindDataPointType.VECTOR3:
                        _storageWriter.writeVector3(val.v);
                        break;
                    case RewindDataPointType.QUATERNION:
                        _storageWriter.writeQuaternion(val.q);
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