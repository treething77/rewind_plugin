using aeric.rewind_plugin;
using UnityEngine;
using UnityEngine.UI;

namespace aeric.rewind_plugin_demos {
    public class SportsDemo : MonoBehaviour {
        //inspector references
        public Text statusText;
        public GameObject stackParent;
        public GameObject targetsParent;
        public RewindPlaybackPreparer playbackPreparer;

        public RectTransform livePanel;
        public GameObject replayUI;
        
        private RewindPlayback _playback;
        private RewindRecorder _recorder;
        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;

        private bool playback;

        private void Start() {
            _rewindScene = new RewindScene();
            _rewindScene.addAllChildren(stackParent);
            _rewindScene.addAllChildren(targetsParent);

            _rewindStorage = new RewindStorage(_rewindScene, 150, false);
            _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 30, true);
            _playback = new RewindPlayback(_rewindScene, _rewindStorage);

            _recorder.startRecording();
        }

        private void Update() {
            replayUI.SetActive(playback);
            livePanel.gameObject.SetActive(!playback);

            if (playback) {
                _playback.AdvancePlaybackTime();
                _playback.restoreFrameAtCurrentTime();

                if (_playback.isPlaybackComplete) {
                    playback = false;
                    statusText.text = "Record";

                    _recorder.startRecording();
                    _playback.stopPlayback();
                    playbackPreparer.stopPlayback();
                }
            }
            else {
                _recorder.updateRecording();
                _recorder.advanceRecordingTime();
                statusText.text = $"Record - {_rewindStorage.RecordedFrameCount} - {_rewindStorage.FrameWriteIndex}";
            }
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(Screen.width - 200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Start Replay")) {
                statusText.text = "Replay";
                playback = true;
                playbackPreparer.startPlayback();
                _playback.startPlayback();
            }

            GUILayout.EndArea();
        }
    }
}