using System.Reflection;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
#endif
using UnityEngine;
using Random = System.Random;

namespace aeric.rewind_plugin
{
    public class RewindCustomMonoBehaviourAttributes : RewindComponentBase
    {
        private FieldInfo[] rewindFields;
        private bool[] rewindFieldLerp;
        private int requiredBufferSize;

        public void Awake() {
            //get all the fields on this object that have the Rewind attribute
            rewindFields = RewindAttributeHelper.GetRewindFields(this);

            rewindFieldLerp = new bool[rewindFields.Length];

            for(int i=0;i<rewindFields.Length;i++)
            {
                var rewindField = rewindFields[i];
                requiredBufferSize += Marshal.SizeOf(rewindField.FieldType);

                var rewindAttribute = rewindField.GetCustomAttribute(typeof(RewindAttribute));
                rewindFieldLerp[i] = ((RewindAttribute)rewindAttribute).Lerp;
            }
        }


        public override void rewindStore(NativeByteArrayWriter writer) {
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
                else if (rewindField.FieldType == typeof(Quaternion))
                {
                    writer.writeQuaternion((Quaternion)rewindField.GetValue(this));
                }
                else if (rewindField.FieldType == typeof(int))
                {
                    writer.writeInt((int)rewindField.GetValue(this));
                }
                else if (rewindField.FieldType == typeof(Color))
                {
                    writer.writeColor((Color)rewindField.GetValue(this));
                }
                else if (rewindField.FieldType == typeof(bool))
                {
                    writer.writeBool((bool)rewindField.GetValue(this));
                }
            }
        }
        
        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT)
        {
            for(int i=0;i<rewindFields.Length;i++)
            {
                var rewindField = rewindFields[i];

                //Call a different method in reader depending on the type of the field
                if (rewindField.FieldType == typeof(float))
                {
                    float fA = frameReaderA.readFloat();
                    float fB = frameReaderB.readFloat();
                    float v = rewindFieldLerp[i] ? Mathf.Lerp(fA, fB, frameT) : fB;
                    rewindField.SetValue(this, v);
                }
                else if (rewindField.FieldType == typeof(Vector3))
                {
                    Vector3 vA = frameReaderA.readV3();
                    Vector3 vB = frameReaderB.readV3();
                    Vector3 v = rewindFieldLerp[i] ? Vector3.Lerp(vA, vB, frameT) : vB;
                    rewindField.SetValue(this, v);
                }
                else if (rewindField.FieldType == typeof(Quaternion))
                {
                    Quaternion qA = frameReaderA.readQuaternion();
                    Quaternion qB = frameReaderB.readQuaternion();
                    Quaternion q = rewindFieldLerp[i] ? Quaternion.Lerp(qA, qB, frameT) : qB;
                    rewindField.SetValue(this, q);
                }
                else if (rewindField.FieldType == typeof(Color))
                {
                    Color cA = frameReaderA.readColor();
                    Color cB = frameReaderB.readColor();
                    Color c = rewindFieldLerp[i] ? Color.Lerp(cA, cB, frameT) : cB;
                    rewindField.SetValue(this, c);
                }
                else if (rewindField.FieldType == typeof(bool))
                {
                    //we dont interpolate booleans
                    frameReaderA.readBool();
                    bool v = frameReaderB.readBool();
                    rewindField.SetValue(this, v);
                }
                else if (rewindField.FieldType == typeof(int))
                {
                    int iA = frameReaderA.readInt();
                    int iB = frameReaderB.readInt();
                    int iV = rewindFieldLerp[i] ? RewindUtilities.LerpInt(iA, iB, frameT) : iB;
                    rewindField.SetValue(this, iV);
                }
            }
        }
 
        public override int RequiredBufferSizeBytes => requiredBufferSize;
        public override uint HandlerTypeID => 3;

    }

}