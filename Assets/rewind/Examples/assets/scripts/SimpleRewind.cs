using System.Drawing;
using ccl.rewind_plugin;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ccl.rewind_plugin_demos
{
    public class SimpleRewind : MonoBehaviour
    {
        public TMPro.TMP_Text statusText;
        public RewindComponentBase rewindCube;
        
        public RewindPlaybackPreparer playbackPreparer;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;
        private RewindRecorder _recorder;
        private RewindPlayback _playback;

        private bool playback;
        private float newPlaybackTime = -1.0f;

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
            rewindScene.addRewindObject(rewindCube);

            rewindStorage = new RewindStorage(rewindScene, 10, false);
   
            _recorder = new RewindRecorder(rewindScene, rewindStorage, 0, true);
            _playback = new RewindPlayback(rewindScene, rewindStorage);
            
            _recorder.startRecording();
            changeState(DemoState.Recording);
            
           // Debug.Break();
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
                    //Debug.DebugBreak();
                    
                    float startTime = _playback.startTime;
                    float endTime = _playback.endTime;
                    
                    playbackPreparer.startPlayback();
                    _playback.startPlayback();
                    Time.timeScale = 0.0f;

                    //start at the end
                    _playback.SetPlaybackTime(endTime);
                    
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

                    if (rewindStorage.RecordedFrameCount == 10 && rewindStorage.FrameWriteIndex == 4)
                    {
                        changeState(DemoState.Paused);
                    }
                    break;
                }
                case DemoState.Paused:
                {
                    // Set the replay time to the scrubber value
                    float currentTime = _playback.currentTime;
                    if (newPlaybackTime != currentTime)
                    {
                        _playback.SetPlaybackTime(newPlaybackTime);
                        _playback.restoreFrameAtCurrentTime();
                    }
                    statusText.text = $"Paused";
                    break;
                }
            }
        }
        
        void DrawQuad(Rect position, Color color) {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0,0,color);
            texture.Apply();
            GUI.skin.box.normal.background = texture;
            GUI.Box(position, GUIContent.none);
        }
        
        private void OnGUI()
        {
            Color c = Color.grey;
            c.a = 0.2f;
            DrawQuad(new Rect(0,0,400,400), c);
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
                        int newUnmappedEndFrame = frameInfo.frameB;
                        
                        rewindStorage.rewindFrames(currentFrameCount - 1 - newUnmappedEndFrame);
                        
                        changeState(DemoState.Recording);
                        
                        _recorder.recordFrame();
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
