using System.Collections.Generic;
using UnityEngine;

namespace aeric.rewind_plugin {
    //TODO: require component?
    public class RewindRendererMaterialChange : RewindComponentBase {
        
        //does not support baking
        //make a list of material that we've seen and store indices into that list
        private List<Material> _materials = new List<Material>();
        
        private Renderer _renderer;

        public override int RequiredBufferSizeBytes => 4;
        public override uint HandlerTypeID => 7;

        private void Awake() {
            TryGetComponent(out _renderer);
        }

        public override void rewindStore(NativeByteArrayWriter writer) {
            Material mat = _renderer.material;
            if (!_materials.Contains(mat)) {
                _materials.Add(mat);
            }

            int materialIndex = _materials.IndexOf(mat);
            writer.writeInt(materialIndex);
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            int materialIndex1 = frameReaderA.readInt();
            int materialIndex2 = frameReaderB.readInt();
            
            int newMaterialIndex = RewindUtilities.LerpInt(materialIndex1, materialIndex2, frameT);
            Material mat = _materials[newMaterialIndex];
            if (mat != _renderer.material) {
                _renderer.material = mat;
            }

        }
    }
}