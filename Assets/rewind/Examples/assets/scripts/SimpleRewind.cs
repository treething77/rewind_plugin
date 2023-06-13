using aeric.rewind_plugin;
using TMPro;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class SimpleRewind : MonoBehaviour {
        //inspector references
        public TMP_Text statusText;
        public RewindComponentBase rewindCube;
        public RewindPlaybackPreparer playbackPreparer;
        
        private enum DemoState {
            None,
            Recording,
            Paused
        }
        private DemoState _demoState = DemoState.None;
        private float _newPlaybackTime = -1.0f;

        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;
        private RewindPlayback _playback;
        private RewindRecorder _recorder;

        private void Start() {
            _rewindScene = new RewindScene();
            _rewindScene.addRewindObject(rewindCube);

            _rewindStorage = new RewindStorage(_rewindScene, 10, false);

            _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 10, true);
            _playback = new RewindPlayback(_rewindScene, _rewindStorage);

            _recorder.startRecording();
            changeState(DemoState.Recording);
        }

        private void Update() {
            switch (_demoState) {
            case DemoState.Recording: {
                _recorder.updateRecording();
                _recorder.advanceRecordingTime();
                statusText.text = $"Record - {_rewindStorage.RecordedFrameCount} - {_rewindStorage.FrameWriteIndex}";

                if (_rewindStorage.RecordedFrameCount == 10 && _rewindStorage.FrameWriteIndex == 4) changeState(DemoState.Paused);
                break;
            }
            case DemoState.Paused: {
                // Set the replay time to the scrubber value
                var currentTime = _playback.currentTime;
                if (_newPlaybackTime != currentTime) {
                    _playback.SetPlaybackTime(_newPlaybackTime);
                    _playback.restoreFrameAtCurrentTime();
                }

                statusText.text = "Paused";
                break;
            }
            }
        }

        private void OnGUI() {
            var c = Color.grey;
            c.a = 0.3f;
            DrawQuad(new Rect(0, 0, 400, 400), c);
            GUILayout.BeginArea(new Rect(0, 0, 400, 400));
            var startTime = _playback.startTime;
            var endTime = _playback.endTime;
            var currentTime = _playback.currentTime;

            switch (_demoState) {
            case DemoState.Recording: {
                if (GUILayout.Button("Pause")) changeState(DemoState.Paused);
                break;
            }
            case DemoState.Paused: {
                var shouldContinue = GUILayout.Button("Continue");

                // Add a scrubber component to control the replay time
                _newPlaybackTime = GUILayout.HorizontalSlider(currentTime, startTime, endTime);

                if (shouldContinue) {
                    var frameInfo = _rewindStorage.findPlaybackFrames(_newPlaybackTime);

                    var currentFrameCount = _rewindStorage.RecordedFrameCount;
                    var newUnmappedEndFrame = frameInfo.frameUnmappedB;

                    _rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);

                    changeState(DemoState.Recording);

                    _recorder.setRecordTime(_newPlaybackTime);
                }

                break;
            }
            }

            // Add a label to display the current time
            GUILayout.Label("startTime: " + startTime.ToString("F2"));
            GUILayout.Label("endTime: " + endTime.ToString("F2"));
            GUILayout.Label("Time: " + currentTime.ToString("F2"));

            GUILayout.EndArea();

            c = Color.grey;
            c.a = 0.4f;

            DrawQuad(new Rect(Screen.width - 400, 0, 400, 800), c);
            GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, 800));

            GUILayout.Label("frame count: " + _rewindStorage.RecordedFrameCount);
            GUILayout.Label("read head: " + _rewindStorage.FrameReadIndex);
            GUILayout.Label("write head: " + _rewindStorage.FrameWriteIndex);

            //Draw a table with 3 columns
            // frame number, time value, x position
            GUILayout.BeginHorizontal();
            GUILayout.Label("Frame Number");
            GUILayout.Label("Time");
            GUILayout.Label("X Position");
            GUILayout.EndHorizontal();

            for (var i = 0; i < 10; i++) {
                GUILayout.BeginHorizontal();

                var isReadHead = i == _rewindStorage.FrameReadIndex;
                var isWriteHead = i == _rewindStorage.FrameWriteIndex;

                var lbl = i.ToString();
                if (isReadHead) lbl += " R";
                if (isWriteHead) lbl += " W";

                GUILayout.Label(lbl);
                GUILayout.Label(_rewindStorage.getFrameTime(i).ToString("F3"));
                GUILayout.Label(_rewindStorage.getFramePosition(i, rewindCube).x.ToString("F1"));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        private void changeState(DemoState newState) {
            switch (newState) {
            case DemoState.Recording: {
                _playback.stopPlayback();
                playbackPreparer.stopPlayback();

                _recorder.startRecording();
                break;
            }
            case DemoState.Paused: {
                var endTime = _playback.endTime;

                playbackPreparer.startPlayback();
                _playback.startPlayback();

                //start at the end
                _playback.SetPlaybackTime(endTime);
                break;
            }
            }

            _demoState = newState;
        }

        private void DrawQuad(Rect position, Color color) {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            GUI.skin.box.normal.background = texture;
            GUI.Box(position, GUIContent.none);
        }
    }
}