using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = System.Random;

namespace ccl.rewind_plugin
{
    //TODO: move to its own file
    public static class ComponentIDGenerator
    {
        private static Random _idRandom;
        
        public static uint generateID(IRewindHandler rewindHandler) 
        {
            if (_idRandom == null)
            {
                _idRandom = new Random();
            }
          
            uint id = (uint) _idRandom.Next(0, 2 << 24) << 8;
  
            id |= rewindHandler.HandlerTypeID; 
            Debug.Log($"Generated id {id}");

            return id;
        }
/*
        static List<RewindComponentBase> rewindComponents = new List<RewindComponentBase>();

        public static void register(RewindComponentBase rewindComponentBase)
        {
            rewindComponents.Add(rewindComponentBase);
        }

        public static void registerHierarchy(Transform rewindParent)
        {
            RewindComponentBase[] rewindComponentBases = rewindParent.GetComponentsInChildren<RewindComponentBase>(true);
            foreach (RewindComponentBase rewindComponentBase in rewindComponentBases)
            {
                register(rewindComponentBase);
            }
        }

        public static void unregister(RewindComponentBase rewindComponentBase)
        {
            rewindComponents.Remove(rewindComponentBase);
        }

        public static void unregisterHierarchy(Transform rewindParent)
        {
            RewindComponentBase[] rewindComponentBases = rewindParent.GetComponentsInChildren<RewindComponentBase>(true);
            foreach (RewindComponentBase rewindComponentBase in rewindComponentBases)
            {
                unregister(rewindComponentBase);
            }
        }
        
        public static bool isRegistered(uint id)
        {
            foreach (var rewindComponent in rewindComponents)
            {
                if (rewindComponent.ID == id) return true;
            }

            return false;
        }
        */
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
        
        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT)
        {
            throw new NotImplementedException();
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