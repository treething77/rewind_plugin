using System.Collections;
using aeric.rewind_plugin;
using TMPro;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class SimpleTransformTest : MonoBehaviour {
        private const float TestTime = 3.0f;
        private const float SpinSpeed = 2.0f;
        private const float ScaleSpeed = 1.0f;
        private const float BounceSpeed = 4.0f;
        private readonly int recordFPS = 30;

        //inspector references
        public TMP_Text statusText;

        //caching component references
        private Transform _transform;
        private RewindTransform _transformTest;

        private RewindPlaybackComponent _playbackComponent;
        private RewindRecorderComponent _recorderComponent;
        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;

        private void Awake() {
            _transform = transform;
            _transformTest = GetComponent<RewindTransform>();
            _recorderComponent = GetComponent<RewindRecorderComponent>();
            _playbackComponent = GetComponent<RewindPlaybackComponent>();
        }

        private void Start() {
            _rewindScene = new RewindScene();
            _rewindScene.addRewindObject(_transformTest);

            //3 seconds at max 30fps
            _rewindStorage = new RewindStorage(_rewindScene, recordFPS * 3, false);

            StartCoroutine(SimpleTestCoroutine());
        }

        private IEnumerator SimpleTestCoroutine() {
            statusText.text = "RECORDING";
            //start recording (create recorder component, add to object)
            _recorderComponent.startRecording(_rewindScene, _rewindStorage, recordFPS);

            //spin and move for 3 seconds
            var timer = 0.0f;

            var startPos = _transform.position;

            while (timer < TestTime) {
                timer += Time.deltaTime;

                var scale = Mathf.Abs(Mathf.Sin(timer * SpinSpeed)) + 0.5f;

                _transform.localScale = new Vector3(scale, scale, scale);

                var rot = Mathf.Cos(timer * ScaleSpeed);

                _transform.localRotation = Quaternion.Euler(0.0f, rot * 100.0f, 0.0f);

                var posTimeScale = Mathf.Sin(timer * BounceSpeed);
                posTimeScale *= posTimeScale;
                var pos = Vector3.Lerp(startPos, startPos + Vector3.one, posTimeScale);
                _transform.position = pos;

                yield return null;
            }

            //stop recording
            _recorderComponent.stopRecording();

            statusText.text = "PLAYBACK";

            //start playback
            _playbackComponent.startPlayback(_rewindScene, _rewindStorage);
        }
    }
}