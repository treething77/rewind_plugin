using aeric.rewind_plugin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace aeric.rewind_plugin_demos {
    public class SportsRewind : MonoBehaviour {
        public TMP_Text statusText;
        public GameObject stackParent;
        public GameObject targetParent;
        public RewindComponentBase robotCamRewind;

        public Image rewindBar;

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
            rewindScene.addAllChildren(stackParent);
            rewindScene.addAllChildren(targetParent);
            rewindScene.addRewindObject(robotCamRewind);

            rewindStorage = new RewindStorage(rewindScene, 150, false);

            _recorder = new RewindRecorder(rewindScene, rewindStorage, 30, true);
            _playback = new RewindPlayback(rewindScene, rewindStorage);

            _recorder.startRecording();
            changeState(DemoState.Recording);
        }

        private void Update() {
            switch (demoState) {
            case DemoState.Recording: {
                _recorder.updateRecording();
                _recorder.advanceRecordingTime();

                var startTime = _playback.startTime;
                var endTime = _playback.endTime;
                var currentTime = _playback.currentTime;
                var fillTime = endTime - startTime;

                if (fillTime <= 0.0f) fillTime = 0.0f;
                rewindBar.fillAmount = fillTime / 5.0f;

                if (Input.GetKey(KeyCode.Space)) changeState(DemoState.Rewinding);
                break;
            }
            case DemoState.Rewinding: {
                var currentTime = _playback.currentTime;
                var startTime = _playback.startTime;

                newPlaybackTime = currentTime - Time.deltaTime * 1.0f;
                if (newPlaybackTime < startTime) newPlaybackTime = startTime;

                {
                    _playback.SetPlaybackTime(newPlaybackTime);
                    _playback.restoreFrameAtCurrentTime();
                }

                var endTime = _playback.endTime;
                var fillTime = currentTime - startTime;
                rewindBar.fillAmount = fillTime / 5.0f;

                if (!Input.GetKey(KeyCode.Space) || fillTime < 0.1f) {
                    var frameInfo = rewindStorage.findPlaybackFrames(newPlaybackTime);

                    var currentFrameCount = rewindStorage.RecordedFrameCount;
                    var newUnmappedEndFrame = frameInfo.frameUnmappedB;

                    startTime = _playback.startTime;
                    endTime = _playback.endTime;

                    rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);

                    startTime = _playback.startTime;
                    endTime = _playback.endTime;

                    changeState(DemoState.Recording);

                    _recorder.setRecordTime(newPlaybackTime);
                }

                break;
            }
            }
        }

        private void changeState(DemoState n) {
            switch (n) {
            case DemoState.Recording: {
                _playback.stopPlayback();
                playbackPreparer.stopPlayback();

                _recorder.startRecording();
                //    Time.timeScale = 1.0f;
                break;
            }
            case DemoState.Rewinding: {
                //Debug.DebugBreak();

                // float startTime = _playback.startTime;
                var endTime = _playback.endTime;

                playbackPreparer.startPlayback();
                _playback.startPlayback();

                //start at the end
                _playback.SetPlaybackTime(endTime);
                newPlaybackTime = endTime;

                break;
            }
            }

            demoState = n;
        }

        private enum DemoState {
            None,
            Recording,
            Rewinding
        }
        /*
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0,0,400,400));
            float startTime = _playback.startTime;
            float endTime = _playback.endTime;
            float currentTime = _playback.currentTime;
            
            switch (demoState)
            {
                case DemoState.Recording:
                {
                    if (GUILayout.Button("Pause"))
                    {
                        changeState(DemoState.Paused);
                    }
                    break;
                }
                case DemoState.Paused:
                {
                    bool shouldContinue = GUILayout.Button("Continue");

                    // Add a scrubber component to control the replay time
                    newPlaybackTime = GUILayout.HorizontalSlider(currentTime, startTime, endTime);
                    

                    if (shouldContinue)
                    {
                        var frameInfo = rewindStorage.findPlaybackFrames(newPlaybackTime);

                        int currentFrameCount = rewindStorage.RecordedFrameCount;
                        int newUnmappedEndFrame = frameInfo.frameUnmappedB;
                        
                        rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);

                        changeState(DemoState.Recording);

                        _recorder.setRecordTime(newPlaybackTime);

                    }

                    break;
                }
            }
            
            // Add a label to display the current time
            GUILayout.Label("startTime: " + startTime.ToString("F2"));
            GUILayout.Label("endTime: " + endTime.ToString("F2"));
            GUILayout.Label("Time: " + currentTime.ToString("F2"));

            GUILayout.EndArea();
        }*/
    }
}