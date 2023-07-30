using UnityEngine;

namespace aeric.rewind_plugin {
    public class RewindSphereCollider : RewindComponentBase {
        private SphereCollider _collider;
 
        private void Awake() {
            _collider = GetComponent<SphereCollider>();
        }

        public override RewindDataSchema makeDataSchema() => new RewindDataSchema().addVector3().addFloat();


        public override uint HandlerTypeID => 11;

        public override void rewindStore(NativeByteArrayWriter writer) {
            writer.writeVector3(_collider.center);
            writer.writeFloat(_collider.radius);
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            _collider.center = Vector3.Lerp(frameReaderA.readVector3(), frameReaderB.readVector3(), frameT);
            _collider.radius = Mathf.Lerp(frameReaderA.readFloat(), frameReaderB.readFloat(), frameT);
        }
    }
}