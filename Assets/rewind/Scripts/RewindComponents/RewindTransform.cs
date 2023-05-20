using System;
using UnityEngine;

namespace rewind_plugin
{
    public class RewindTransform : MonoBehaviour, IRewindHandler
    {
        private Transform _transform;
        
        private void Start()
        {
            _transform = transform;
        }

        public void rewindStore(NativeByteArrayWriter writer)
        {
            writer.writeV3(_transform.position);
            writer.writeV3(_transform.localScale);
            writer.writeQuaternion(_transform.rotation);
        }

        public void rewindRestore(NativeByteArrayReader reader)
        {
            _transform.position = reader.readV3();
            _transform.localScale = reader.readV3();
            _transform.rotation = reader.readQuaternion();
        }
    }

}