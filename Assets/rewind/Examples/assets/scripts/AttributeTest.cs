using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos
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

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT)
        {
            Color cA = frameReaderA.readColor();
            Color cB = frameReaderB.readColor();
            mesh.material.color = Color.Lerp(cA, cB, frameT);
            
            base.rewindRestoreInterpolated(frameReaderA, frameReaderB, frameT);
        }
    }


}