using System;
using UnityEngine;

namespace rewind_plugin
{
    public class RewindTransform : MonoBehaviour, IRewindHandler
    {
        private Transform _transform;

        public bool recordScale = true;
        
        private void Awake() {
            _transform = transform;
        }

        public void rewindStore(NativeByteArrayWriter writer) {
            writer.writeV3(_transform.position);
            writer.writeQuaternion(_transform.rotation);
            if (recordScale) writer.writeV3(_transform.localScale);
        }

        public void rewindRestore(NativeByteArrayReader reader) {
            _transform.SetPositionAndRotation(reader.readV3(), reader.readQuaternion());
            if (recordScale) _transform.localScale = reader.readV3();
        }

        public int RequiredBufferSizeBytes => (4 * 3) + (4 * 4) + (recordScale ? (4 * 3) : 0);
    }

}