using UnityEngine;

namespace aeric.rewind_plugin
{
    public class RewindTransform : RewindComponentBase
    {
        private Transform _transform;
        private CharacterController _controller;

        public bool recordScale = true;

        private bool controllerEnabledState;
        
        private void Awake() {
            _transform = transform;
            TryGetComponent(out _controller);
        }

        public override void rewindStore(NativeByteArrayWriter writer)
        {
            writer.writeV3(_transform.position);
            writer.writeQuaternion(_transform.rotation);
            if (recordScale) writer.writeV3(_transform.localScale);
        }

        public override void preRestore() {
            if (_controller != null)
            {
                controllerEnabledState = _controller.enabled;
                _controller.enabled = false;
            }
        }
        
        public override void postRestore() {
            if(_controller != null)
               _controller.enabled = controllerEnabledState;
        }
        
        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT)
        {
            Vector3 posA = frameReaderA.readV3();
            Vector3 posB = frameReaderB.readV3();
            Vector3 position = Vector3.Lerp(posA, posB, frameT);

          //  Debug.Log("posA.x=" + posA.x + " posB.x=" + posB.x + " frameT: " + frameT + " result: " + position.x);
            
            Quaternion rotation = Quaternion.Lerp(frameReaderA.readQuaternion(), frameReaderB.readQuaternion(), frameT);
            _transform.SetPositionAndRotation(position, rotation);

            if (recordScale)
            {
                Vector3 scale = Vector3.Lerp(frameReaderA.readV3(), frameReaderB.readV3(), frameT);
                _transform.localScale = scale;
            }
        }
        
        public override int RequiredBufferSizeBytes => (4 * 3) + (4 * 4) + (recordScale ? (4 * 3) : 0);
        public override uint HandlerTypeID => 1;
    }
}