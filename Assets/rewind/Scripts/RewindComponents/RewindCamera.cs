using UnityEngine;

namespace aeric.rewind_plugin {
    public class RewindCamera : RewindComponentBase {
        private Camera _camera;

       public override RewindDataSchema makeDataSchema() {
           return new RewindDataSchema().addFloat(5);
       }

       public override uint HandlerTypeID => 8;

        private void Awake() {
            _camera = GetComponent<Camera>();
        }

        public override void rewindStore(NativeByteArrayWriter writer) {
            writer.writeFloat(_camera.fieldOfView);
            writer.writeFloat(_camera.focalLength);
            writer.writeFloat(_camera.aspect);
            writer.writeFloat(_camera.nearClipPlane);
            writer.writeFloat(_camera.farClipPlane);
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
          _camera.fieldOfView = Mathf.Lerp(frameReaderA.readFloat(), frameReaderB.readFloat(), frameT);
          _camera.focalLength = Mathf.Lerp(frameReaderA.readFloat(), frameReaderB.readFloat(), frameT);
          _camera.aspect = Mathf.Lerp(frameReaderA.readFloat(), frameReaderB.readFloat(), frameT);
          _camera.nearClipPlane = Mathf.Lerp(frameReaderA.readFloat(), frameReaderB.readFloat(), frameT);
          _camera.farClipPlane = Mathf.Lerp(frameReaderA.readFloat(), frameReaderB.readFloat(), frameT);
        }
    }
}