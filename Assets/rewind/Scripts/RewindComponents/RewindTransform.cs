using System;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public abstract class RewindComponentBase : MonoBehaviour, IRewindHandler, ISerializationCallbackReceiver
    {
                
        //[HideInInspector] 
        [SerializeField] private uint id; 
        
        public void OnBeforeSerialize()
        {
            // If we don't have an ID yet then generate one
            if (id == 0)
                id = ComponentIDGenerator.generateID(this);
        }

        public void OnAfterDeserialize()
        {
            //make sure the id is unique before we register it
            while (ComponentIDGenerator.isRegistered(id))
            {
                id = ComponentIDGenerator.generateID(this);
            }
            ComponentIDGenerator.register(this);
        }
        
        public uint ID => id;
        
        //Required to be implemented by sub-classes
        public abstract void rewindStore(NativeByteArrayWriter writer);
        public abstract void rewindRestore(NativeByteArrayReader reader);
        public abstract int RequiredBufferSizeBytes { get; }
        public abstract uint HandlerTypeID  { get; }
        public abstract void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT);
    }
    
    public class RewindTransform : RewindComponentBase
    {
        private Transform _transform;

        public bool recordScale = true;
        
        private void Awake() {
            _transform = transform;
        }

        public override void rewindStore(NativeByteArrayWriter writer) {
            writer.writeV3(_transform.position);
            writer.writeQuaternion(_transform.rotation);
            if (recordScale) writer.writeV3(_transform.localScale);
        }

        public override void rewindRestore(NativeByteArrayReader reader) {
            _transform.SetPositionAndRotation(reader.readV3(), reader.readQuaternion());
            if (recordScale) _transform.localScale = reader.readV3();
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT)
        {
            Vector3 position = Vector3.Lerp(frameReaderA.readV3(), frameReaderB.readV3(), frameT);
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