using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace aeric.rewind_plugin {
    public class RewindTransform : RewindComponentBase {
        public bool recordScale = true;
        private CharacterController _controller;
        private Transform _transform;

        private bool controllerEnabledState;
        
        public override uint HandlerTypeID => 1;

        private void Awake() {
            _transform = transform;
            TryGetComponent(out _controller);
        }

        public override RewindDataSchema makeDataSchema() {
            var schema = new RewindDataSchema().addVector3().addQuaternion();
            if (recordScale) schema.addVector3();
            return schema;
        }
        
        public override void rewindStore(NativeByteArrayWriter writer) {
            writer.writeV3(_transform.position);
            writer.writeQuaternion(_transform.rotation);
            if (recordScale) writer.writeV3(_transform.localScale);
        }

        public override void preRestore() {
            if (_controller != null) {
                controllerEnabledState = _controller.enabled;
                _controller.enabled = false;
            }
        }

        public override void postRestore() {
            if (_controller != null)
                _controller.enabled = controllerEnabledState;
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            var posA = frameReaderA.readVector3();
            var posB = frameReaderB.readVector3();
            var position = Vector3.Lerp(posA, posB, frameT);

            //  Debug.Log("posA.x=" + posA.x + " posB.x=" + posB.x + " frameT: " + frameT + " result: " + position.x);

            var rotation = Quaternion.Lerp(frameReaderA.readQuaternion(), frameReaderB.readQuaternion(), frameT);
            _transform.SetPositionAndRotation(position, rotation);

            if (recordScale) {
                var scale = Vector3.Lerp(frameReaderA.readVector3(), frameReaderB.readVector3(), frameT);
                _transform.localScale = scale;
            }
        }
    }
}