using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    /// Stores the enabled state for a GameObject
    /// </summary>
    public class RewindGameObjectEnabled : RewindComponentBase {
        private GameObject _gameObject;

       public override RewindDataSchema makeDataSchema() => new RewindDataSchema().addBool();

       public override uint HandlerTypeID => 10;

        private void Awake() {
            _gameObject = gameObject;
        }

        public override void rewindStore(NativeByteArrayWriter writer) {
            writer.writeBool(_gameObject.activeSelf);
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            bool b1 = frameReaderA.readBool();
            bool b2 = frameReaderB.readBool();

            bool shouldBeActive = Mathf.Lerp(b1 ? 1 : 0, b2 ? 1 : 0, frameT) > 0.5f;
            if (shouldBeActive != _gameObject.activeSelf) {
                _gameObject.SetActive(shouldBeActive);
            }
        }
    }
}