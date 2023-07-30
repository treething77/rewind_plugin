using UnityEngine;
#if USE_BURST_FOR_REWIND_COMPONENTS
using Unity.Burst;
#endif

namespace aeric.rewind_plugin {
    /// <summary>
    /// Stores and restores all transforms within a hierarchy.
    /// </summary>
    public class RewindTransformHierarchy : RewindComponentBase {
        private Transform[] _transforms;

        public override RewindDataSchema makeDataSchema() {
            var transformCount = _transforms.Length;
            return new RewindDataSchema().addVector3(transformCount)//position
                                         .addQuaternion(transformCount)//rotation
                                         .addVector3(transformCount); //scale
        }

        public override uint HandlerTypeID => 2;

        private void Awake() {
            //Get all transforms in the hierarchy
            _transforms = GetComponentsInChildren<Transform>(true);
        }

#if USE_BURST_FOR_REWIND_COMPONENTS
        [BurstCompile]
#endif
        public override void rewindStore(NativeByteArrayWriter writer) {
            foreach (var t in _transforms) {
                t.GetLocalPositionAndRotation(out var localPos, out var localRot);
                writer.writeVector3(localPos);
                writer.writeQuaternion(localRot);
                writer.writeVector3(t.localScale);
            }
        }

#if USE_BURST_FOR_REWIND_COMPONENTS
        [BurstCompile]
#endif
        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            foreach (var t in _transforms) {
                var p1 = frameReaderA.readVector3();
                var r1 = frameReaderA.readQuaternion();
                var s1 = frameReaderA.readVector3();
                var p2 = frameReaderB.readVector3();
                var r2 = frameReaderB.readQuaternion();
                var s2 = frameReaderB.readVector3();

                t.localPosition = Vector3.Lerp(p1, p2, frameT);
                t.localRotation = Quaternion.Lerp(r1, r2, frameT);
                t.localScale = Vector3.Lerp(s1, s2, frameT);
            }
        }
    }
}