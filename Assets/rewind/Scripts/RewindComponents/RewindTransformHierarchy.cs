#define USE_BURST_FOR_REWIND_COMPONENTS

using Unity.Burst;
using UnityEngine;

namespace aeric.rewind_plugin
{
    public class RewindTransformHierarchy : RewindComponentBase
    {
        private Transform[] _transforms;
        
        private void Awake()
        {
            //Get all transforms in the hierarchy
            _transforms = GetComponentsInChildren<Transform>();
        }

#if USE_BURST_FOR_REWIND_COMPONENTS
        [BurstCompile]
#endif
        public override void rewindStore(NativeByteArrayWriter writer)
        {
            foreach (Transform t in _transforms)
            {
                t.GetLocalPositionAndRotation(out var localPos, out var localRot);
                writer.writeV3(localPos);    
                writer.writeQuaternion(localRot);    
                writer.writeV3(t.localScale);    
            }
        }

#if USE_BURST_FOR_REWIND_COMPONENTS
        [BurstCompile]
#endif
        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT)
        {
            foreach (Transform t in _transforms)
            {
                Vector3 p1 =    frameReaderA.readV3();    
                Quaternion r1 = frameReaderA.readQuaternion();    
                Vector3 s1 =    frameReaderA.readV3();    
                Vector3 p2 =    frameReaderB.readV3();    
                Quaternion r2 = frameReaderB.readQuaternion();    
                Vector3 s2 =    frameReaderB.readV3();    

                t.localPosition = Vector3.Lerp(p1, p2, frameT);
                t.localRotation = Quaternion.Lerp(r1, r2, frameT);
                t.localScale = Vector3.Lerp(s1, s2, frameT);
            }
        }

        public override int RequiredBufferSizeBytes
        {
            get
            {
                int transformCount = _transforms.Length;
                int boneCost = 4 * (3 + 4 + 3);
                int totalSizeBytes = boneCost * transformCount;
                return totalSizeBytes;
            }
        }

        public override uint HandlerTypeID => 2;

    }
}
