using System;
using JetBrains.Annotations;
using UnityEngine;

namespace aeric.rewind_plugin
{
    public class RewindMaterialColor : RewindComponentBase
    {
        private Renderer _renderer;
        private Material _material;
        
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _material = _renderer.material;
            //TODO: flag for shared material
        }

        public override void rewindStore(NativeByteArrayWriter writer)
        {
            writer.writeColor(_material.color);  
        }

        public override void rewindRestoreInterpolated([NotNull] NativeByteArrayReader frameReaderA, [NotNull] NativeByteArrayReader frameReaderB, float frameT)
        {
            Color cA = frameReaderA.readColor();
            Color cB = frameReaderB.readColor();

            _material.color = Color.Lerp(cA, cB, frameT);
        }

        public override int RequiredBufferSizeBytes => 16; //rgba float

        public override uint HandlerTypeID => 5;

    }
}
