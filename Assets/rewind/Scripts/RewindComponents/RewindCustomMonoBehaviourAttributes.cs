using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace rewind_plugin
{
    public class RewindCustomMonoBehaviourAttributes : MonoBehaviour, IRewindHandler
    {
        private FieldInfo[] rewindFields;
        private int requiredBufferSize;
        
        private void Awake()
        {
            //get all the fields on this object that have the Rewind attribute
            rewindFields = RewindAttributeHelper.GetRewindFields(this);

            foreach (var rewindField in rewindFields)
            {
                requiredBufferSize += Marshal.SizeOf(rewindField.FieldType);
            }
        }

        public virtual void rewindStore(NativeByteArrayWriter writer)
        {
            foreach (FieldInfo rewindField in rewindFields)
            {
                //Call a different method in writer depending on the type of the field
                if (rewindField.FieldType == typeof(float))
                {
                    writer.writeFloat((float)rewindField.GetValue(this));
                }
                else if (rewindField.FieldType == typeof(Vector3))
                {
                    writer.writeV3((Vector3)rewindField.GetValue(this));
                }
            }
        }
        
        public virtual void rewindRestore(NativeByteArrayReader reader)
        {
            foreach (FieldInfo rewindField in rewindFields)
            {
                //Call a different method in reader depending on the type of the field
                if (rewindField.FieldType == typeof(float))
                {
                    rewindField.SetValue(this, reader.readFloat());
                }
                else if (rewindField.FieldType == typeof(Vector3))
                {
                    rewindField.SetValue(this, reader.readV3());
                }
            }
        }

        public int RequiredBufferSizeBytes => requiredBufferSize;
    }

}