using ccl.rewind_plugin;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class AttributeTest : RewindCustomMonoBehaviourAttributes
    {
        [Rewind] public float floatValue;
        [Rewind] public Vector3 vectorValue;

        public MeshRenderer mesh;
        
        public override void rewindStore(NativeByteArrayWriter writer)
        {
            writer.writeColor(mesh.material.color);
            base.rewindStore(writer);
        }

        public override void rewindRestore(NativeByteArrayReader reader)
        {
            mesh.material.color = reader.readColor();
            base.rewindRestore(reader);
        }
    }


}