using System;
using UnityEngine;

namespace aeric.rewind_plugin {
    namespace RewindStorageDataTypes {
        //We convert our NativeArray storage to these generic types using the schema for each handler. Then we 
        //have methods to convert to/from these generic types into the various data formats for serialization.
        [Serializable]
        public class RewindStorageData_Value {
            public RewindDataPointType valueType; 
            
            public float f;
            public int i;
            public bool b;
            public Vector3 v;
            public Quaternion q;
            public Color c;
        }
        
        [Serializable]
        public class RewindStorageData_Handler {
            public uint id;
            public RewindStorageData_Value[] values;
        }
        
        [Serializable]
        public class RewindStorageData {
            public int maxFrameCount;
            public int recordedFrameCount;
            public int handlerCount;
            public int version = 1;

            public float[] frameTimeData;
            public RewindStorageData_Handler[] handlerData;
        }

    }
}