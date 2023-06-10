using aeric.rewind_plugin;
using TMPro;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class SportsDemo : MonoBehaviour {
        public TMP_Text statusText;
        public GameObject stackParent;
        public GameObject targetsParent;
        public RewindPlaybackPreparer playbackPreparer;
        private RewindPlayback _playback;
        private RewindRecorder _recorder;

        private bool playback;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;

        private void Start() {
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);
            rewindScene.addAllChildren(targetsParent);

            rewindStorage = new RewindStorage(rewindScene, 150, false);

            _recorder = new RewindRecorder(rewindScene, rewindStorage, 30, true);
            _playback = new RewindPlayback(rewindScene, rewindStorage);

            _recorder.startRecording();
        }

        private void Update() {
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
                statusText.text = $"Record - {rewindStorage.RecordedFrameCount} - {rewindStorage.FrameWriteIndex}";
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