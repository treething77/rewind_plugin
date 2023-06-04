using ccl.rewind_plugin;
using UnityEngine;
using UnityEngine.XR;

namespace ccl.rewind_plugin_demos
{
    public class SportsRewind : MonoBehaviour
    {
        public TMPro.TMP_Text statusText;
        public GameObject stackParent;
        public RewindPlaybackPreparer playbackPreparer;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;
        private RewindRecorder _recorder;
        private RewindPlayback _playback;

        private bool playback;

        enum DemoState
        {
            None,
            Recording,
            Paused
        }

        private DemoState demoState = DemoState.None;

        void Start()
        {
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);

            rewindStorage = new RewindStorage(rewindScene, 150, false);
   
            _recorder = new RewindRecorder(rewindScene, rewindStorage, 30, true);
            _playback = new RewindPlayback(rewindScene, rewindStorage);
            
            _recorder.startRecording();
            changeState(DemoState.Recording);
        }

        private void changeState(DemoState n)
        {
            switch (n)
            {
                case DemoState.Recording:
                {
                    _playback.stopPlayback();
                    playbackPreparer.stopPlayback();
                    _recorder.startRecording();
                    Time.timeScale = 1.0f;
                    break;
                }
                case DemoState.Paused:
                {
                    Debug.DebugBreak();
                    float startTime = _playback.startTime;
                    float endTime = _playback.endTime;
                    float currentTime = _playback.currentTime;

                    
                    playbackPreparer.startPlayback();
                    _playback.startPlayback();
                    Time.timeScale = 0.0f;
                    
                    startTime = _playback.startTime;
                    endTime = _playback.endTime;
                    currentTime = _playback.currentTime;

                    break;
                }
            }
            demoState = n;
        }

        private void Update()
        {
            switch (demoState)
            {
                case DemoState.Recording:
                {
                    _recorder.updateRecording();
                    statusText.text = $"Record - {rewindStorage.RecordedFrameCount} - {rewindStorage.FrameWriteIndex}";
                    break;
                }
                case DemoState.Paused:
                {
                    //_playback.AdvancePlaybackTime();
                    _playback.restoreFrameAtCurrentTime();
                    statusText.text = $"Paused";
                    break;
                }
            }
        }
        
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
                    if (GUILayout.Button("Continue"))
                    {
                        changeState(DemoState.Recording);
                    }
      
                    // Add a scrubber component to control the replay time
                    float newTime = GUILayout.HorizontalSlider(currentTime, startTime, endTime);
                    
                    // Check if the scrubber value has changed
                    if (currentTime != newTime)
                    {
                        // Set the replay time to the scrubber value
                        _playback.SetPlaybackTime(newTime);
                    }
                    break;
                }
            }
            
            // Add a label to display the current time
            GUILayout.Label("startTime: " + startTime.ToString("F2"));
            GUILayout.Label("endTime: " + endTime.ToString("F2"));
            GUILayout.Label("Time: " + currentTime.ToString("F2"));

            GUILayout.EndArea();
        }
    }
}
