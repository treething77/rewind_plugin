using System;
using System.Collections.Generic;
using UnityEngine;

namespace aeric.rewind_plugin {
    public enum RewindDataPointType : int {
        FLOAT,
        INT,
        BOOL,
        VECTOR3,
        QUATERNION,
        COLOR
    }

    public class RewindDataPoint {
        public readonly RewindDataPointType _type;
        public readonly int _count;

        public RewindDataPoint(RewindDataPointType rewindDataPointType, int count) {
            _type = rewindDataPointType;
            _count = count;
        }
    }

    public class RewindDataSchema {
        List<RewindDataPoint> schema = new List<RewindDataPoint>();

        public RewindDataSchema addInt(int count = 1) {
            return addDataPoint(RewindDataPointType.INT, count);
        }

        public RewindDataSchema addBool(int count = 1) {
            return addDataPoint(RewindDataPointType.BOOL, count);
        }

        public RewindDataSchema addFloat(int count = 1) {
            return addDataPoint(RewindDataPointType.FLOAT, count);
        }

        public RewindDataSchema addVector3(int count = 1) {
            return addDataPoint(RewindDataPointType.VECTOR3, count);
        }

        public RewindDataSchema addQuaternion(int count = 1) {
            return addDataPoint(RewindDataPointType.QUATERNION, count);
        }
        
        public RewindDataSchema addColor(int count = 1) {
            return addDataPoint(RewindDataPointType.COLOR, count);
        }
        
        private RewindDataSchema addDataPoint(RewindDataPointType rewindDataPointType, int count) {
            schema.Add(new RewindDataPoint(rewindDataPointType, count));
            return this;
        }

        public RewindDataSchema addType(Type rewindFieldFieldType) {
            if (rewindFieldFieldType == typeof(int)) {
                return addInt();
            }
            if (rewindFieldFieldType == typeof(bool)) {
                return addBool();
            }
            if (rewindFieldFieldType == typeof(float)) {
                return addFloat();
            }
            if (rewindFieldFieldType == typeof(Vector3)) {
                return addVector3();
            }
            if (rewindFieldFieldType == typeof(Quaternion)) {
                return addQuaternion();
            }
            if (rewindFieldFieldType == typeof(Color)) {
                return addColor();
            }
            throw new Exception("Unsupported type: " + rewindFieldFieldType);
        }

        public int getSchemaSize() {
            int size = 0;
            foreach (RewindDataPoint rewindDataPoint in schema) {
                int rewindDataPointSize = 0;
                unsafe {
                switch (rewindDataPoint._type) {
                    case RewindDataPointType.FLOAT:
                        rewindDataPointSize = sizeof(float);
                        break;
                    case RewindDataPointType.INT:
                        rewindDataPointSize = sizeof(int);
                        break;
                    case RewindDataPointType.BOOL:
                        rewindDataPointSize = sizeof(bool);
                        break;
                    case RewindDataPointType.VECTOR3:
                        rewindDataPointSize = sizeof(Vector3);
                        break;
                    case RewindDataPointType.QUATERNION:
                        rewindDataPointSize = sizeof(Quaternion);
                        break;
                    case RewindDataPointType.COLOR:
                        rewindDataPointSize = sizeof(Color);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                }

                size += rewindDataPointSize * rewindDataPoint._count;
            }

            return size;
        }

        public List<RewindDataPoint> getSchema() {
            return schema;
        }

        public int GetValueCount() {
            int count = 0;
            foreach (var v in schema) count += v._count;
            return count;
        }
    }
}