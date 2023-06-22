using System.Collections.Generic;
using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    /// Stores material changes for a Renderer
    /// </summary>
    public class RewindRendererMaterialChange : RewindComponentBase {
        
        //does not support baking
        //make a list of material that we've seen and store indices into that list
        private List<Material> _materials = new();
        private Renderer _renderer;
        
        public override RewindDataSchema makeDataSchema() => new RewindDataSchema().addInt();

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
            if (newMaterialIndex < 0 || newMaterialIndex >= _materials.Count)
                Debug.LogError("Material index out of bounds.");
            else {
                Material mat = _materials[newMaterialIndex];
                if (mat != _renderer.material) {
                    _renderer.material = mat;
                }
            }
        }
    }
}