using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = System.Random;

namespace ccl.rewind_plugin
{
    public static class ComponentIDGenerator
    {
        public static uint generateID(IRewindHandler rewindHandler) 
        {
            //GetInstanceID does not persist between runtime and editor, so we can't use it to 
            //reliably match components with GameObjects
          //  var id = (ulong)rewindHandler.gameObject.GetInstanceID();
          //  id <<= 32;//top 32 bits are the GameObject ID

            uint id = (uint) UnityEngine.Random.Range(0, 2 << 24) << 8;
  
            id |= (uint) rewindHandler.HandlerTypeID; 
  
            return id;
        }

        static List<RewindComponentBase> rewindComponents = new List<RewindComponentBase>();

        public static void register(RewindComponentBase rewindComponentBase)
        {
            rewindComponents.Add(rewindComponentBase);
        }

        public static bool isRegistered(uint id)
        {
            foreach (var rewindComponent in rewindComponents)
            {
                if (rewindComponent.ID == id) return true;
            }

            return false;
        }
    }
    
    public class RewindCustomMonoBehaviourAttributes : RewindComponentBase
    {
        //[HideInInspector] [SerializeField] private ComponentIdentity identity; 

        private FieldInfo[] rewindFields;
        private int requiredBufferSize;
        
        private void Awake() {
            //get all the fields on this object that have the Rewind attribute
            rewindFields = RewindAttributeHelper.GetRewindFields(this);

            foreach (var rewindField in rewindFields)
            {
                requiredBufferSize += Marshal.SizeOf(rewindField.FieldType);
            }
            
            //TODO: register here too?
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
            }
        }
        
        public override void rewindRestore(NativeByteArrayReader reader) {
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

        public override int RequiredBufferSizeBytes => requiredBufferSize;
        public override uint HandlerTypeID => 2;
    }

}