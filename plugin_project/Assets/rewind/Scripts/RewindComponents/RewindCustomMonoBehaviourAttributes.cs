using System.Reflection;
using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    /// Stores changes on any fields on this component that use the Rewind attribute.
    /// </summary>
    public class RewindCustomMonoBehaviourAttributes : RewindComponentBase {
        private bool[] rewindFieldLerp;
        private FieldInfo[] rewindFields;

        private RewindDataSchema _schema;

        public override RewindDataSchema makeDataSchema() {
            return initializeFromFields();
        }

        public override uint HandlerTypeID => 3;

        private RewindDataSchema initializeFromFields() {
            if (_schema != null) return _schema;
            
            //get all the fields on this object that have the Rewind attribute
            rewindFields = RewindAttributeHelper.GetRewindFields(this);

            rewindFieldLerp = new bool[rewindFields.Length];

            _schema = new RewindDataSchema();

            for (var i = 0; i < rewindFields.Length; i++) {
                var rewindField = rewindFields[i];
                _schema.addType(rewindField.FieldType);

                var rewindAttribute = rewindField.GetCustomAttribute(typeof(RewindAttribute));
                rewindFieldLerp[i] = ((RewindAttribute)rewindAttribute).Lerp;
            }

            return _schema;
        }

        public void Awake() {
            initializeFromFields();
        }

        public override void rewindStore(NativeByteArrayWriter writer) {
            foreach (var rewindField in rewindFields) {
                //Call a different method in writer depending on the type of the field
                if (rewindField.FieldType == typeof(float))
                    writer.writeFloat((float)rewindField.GetValue(this));
                else if (rewindField.FieldType == typeof(Vector3))
                    writer.writeVector3((Vector3)rewindField.GetValue(this));
                else if (rewindField.FieldType == typeof(Quaternion))
                    writer.writeQuaternion((Quaternion)rewindField.GetValue(this));
                else if (rewindField.FieldType == typeof(int))
                    writer.writeInt((int)rewindField.GetValue(this));
                else if (rewindField.FieldType == typeof(Color))
                    writer.writeColor((Color)rewindField.GetValue(this));
                else if (rewindField.FieldType == typeof(bool)) writer.writeBool((bool)rewindField.GetValue(this));
            }
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            for (var i = 0; i < rewindFields.Length; i++) {
                var rewindField = rewindFields[i];

                //Call a different method in reader depending on the type of the field
                if (rewindField.FieldType == typeof(float)) {
                    var fA = frameReaderA.readFloat();
                    var fB = frameReaderB.readFloat();
                    var v = rewindFieldLerp[i] ? Mathf.Lerp(fA, fB, frameT) : fB;
                    rewindField.SetValue(this, v);
                }
                else if (rewindField.FieldType == typeof(Vector3)) {
                    var vA = frameReaderA.readVector3();
                    var vB = frameReaderB.readVector3();
                    var v = rewindFieldLerp[i] ? Vector3.Lerp(vA, vB, frameT) : vB;
                    rewindField.SetValue(this, v);
                }
                else if (rewindField.FieldType == typeof(Quaternion)) {
                    var qA = frameReaderA.readQuaternion();
                    var qB = frameReaderB.readQuaternion();
                    var q = rewindFieldLerp[i] ? Quaternion.Lerp(qA, qB, frameT) : qB;
                    rewindField.SetValue(this, q);
                }
                else if (rewindField.FieldType == typeof(Color)) {
                    var cA = frameReaderA.readColor();
                    var cB = frameReaderB.readColor();
                    var c = rewindFieldLerp[i] ? Color.Lerp(cA, cB, frameT) : cB;
                    rewindField.SetValue(this, c);
                }
                else if (rewindField.FieldType == typeof(bool)) {
                    //we dont interpolate booleans
                    frameReaderA.readBool();
                    var v = frameReaderB.readBool();
                    rewindField.SetValue(this, v);
                }
                else if (rewindField.FieldType == typeof(int)) {
                    var iA = frameReaderA.readInt();
                    var iB = frameReaderB.readInt();
                    var iV = rewindFieldLerp[i] ? RewindUtilities.LerpInt(iA, iB, frameT) : iB;
                    rewindField.SetValue(this, iV);
                }
            }
        }
    }
}