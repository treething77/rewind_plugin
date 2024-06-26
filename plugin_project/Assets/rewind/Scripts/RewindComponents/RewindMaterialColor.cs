using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    /// Implementation of RewindComponentBase for handling material color changes
    /// </summary>
    public class RewindMaterialColor : RewindComponentBase {
        private Material _material;
        private Renderer _renderer;
        
        public override RewindDataSchema makeDataSchema() {
            return new RewindDataSchema().addColor();
        }

        public override uint HandlerTypeID => 5;

        public override void rewindStore(NativeByteArrayWriter writer) {
            writer.writeColor(_material.color);
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            var cA = frameReaderA.readColor();
            var cB = frameReaderB.readColor();

            _material.color = Color.Lerp(cA, cB, frameT);
        }
        
        private void Awake() {
            _renderer = GetComponent<Renderer>();
            _material = _renderer.material;
        }
    }
}