using aeric.rewind_plugin;
using TMPro;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class SimpleRewind : MonoBehaviour {
        public TMP_Text statusText;
        public RewindComponentBase rewindCube;

        public RewindPlaybackPreparer playbackPreparer;
        private RewindPlayback _playback;
        private RewindRecorder _recorder;

        private DemoState demoState = DemoState.None;
        private float newPlaybackTime = -1.0f;

        private bool playback;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;

        private void Start() {
            rewindScene = new RewindScene();
            rewindScene.addRewindObject(rewindCube);

            rewindStorage = new RewindStorage(rewindScene, 10, false);

            _recorder = new RewindRecorder(rewindScene, rewindStorage, 10, true);
            _playback = new RewindPlayback(rewindScene, rewindStorage);

            _recorder.startRecording();
            changeState(DemoState.Recording);

            // Debug.Break();
        }

        private void Update() {
            switch (demoState) {
            case DemoState.Recording: {
                _recorder.updateRecording();
                _recorder.advanceRecordingTime();
                statusText.text = $"Record - {rewindStorage.RecordedFrameCount} - {rewindStorage.FrameWriteIndex}";

                if (rewindStorage.RecordedFrameCount == 10 && rewindStorage.FrameWriteIndex == 4) changeState(DemoState.Paused);
                break;
            }
            case DemoState.Paused: {
                // Set the replay time to the scrubber value
                var currentTime = _playback.currentTime;
                if (newPlaybackTime != currentTime) {
                    _playback.SetPlaybackTime(newPlaybackTime);
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

            switch (demoState) {
            case DemoState.Recording: {
                if (GUILayout.Button("Pause")) changeState(DemoState.Paused);
                break;
            }
            case DemoState.Paused: {
                var shouldContinue = GUILayout.Button("Continue");

                // Add a scrubber component to control the replay time
                newPlaybackTime = GUILayout.HorizontalSlider(currentTime, startTime, endTime);

                if (shouldContinue) {
                    var frameInfo = rewindStorage.findPlaybackFrames(newPlaybackTime);

                    var currentFrameCount = rewindStorage.RecordedFrameCount;
                    var newUnmappedEndFrame = frameInfo.frameUnmappedB;

                    rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);

                    changeState(DemoState.Recording);

                    _recorder.setRecordTime(newPlaybackTime);

                    //Debug.Break();
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

            GUILayout.Label("frame count: " + rewindStorage.RecordedFrameCount);
            GUILayout.Label("read head: " + rewindStorage.FrameReadIndex);
            GUILayout.Label("write head: " + rewindStorage.FrameWriteIndex);

            //Draw a table with 3 columns
            // frame number, time value, x position
            GUILayout.BeginHorizontal();
            GUILayout.Label("Frame Number");
            GUILayout.Label("Time");
            GUILayout.Label("X Position");
            GUILayout.EndHorizontal();

            for (var i = 0; i < 10; i++) {
                GUILayout.BeginHorizontal();

                var isReadHead = i == rewindStorage.FrameReadIndex;
                var isWriteHead = i == rewindStorage.FrameWriteIndex;

                var lbl = i.ToString();
                if (isReadHead) lbl += " R";
                if (isWriteHead) lbl += " W";

                GUILayout.Label(lbl);
                GUILayout.Label(rewindStorage.getFrameTime(i).ToString("F3"));
                GUILayout.Label(rewindStorage.getFramePosition(i, rewindCube).x.ToString("F1"));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        private void changeState(DemoState n) {
            switch (n) {
            case DemoState.Recording: {
                _playback.stopPlayback();
                playbackPreparer.stopPlayback();

                _recorder.startRecording();
                //Time.timeScale = 1.0f;
                break;
            }
            case DemoState.Paused: {
                //Debug.DebugBreak();

                var startTime = _playback.startTime;
                var endTime = _playback.endTime;

                playbackPreparer.startPlayback();
                _playback.startPlayback();
                //Time.timeScale = 0.0f;

                //start at the end
                _playback.SetPlaybackTime(endTime);

                break;
            }
            }

            demoState = n;
        }

        private void DrawQuad(Rect position, Color color) {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            GUI.skin.box.normal.background = texture;
            GUI.Box(position, GUIContent.none);
        }

        private enum DemoState {
            None,
            Recording,
            Paused
        }
    }
}