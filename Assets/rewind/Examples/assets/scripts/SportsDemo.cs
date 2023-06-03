using ccl.rewind_plugin;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class SportsDemo : MonoBehaviour
    {
        public TMPro.TMP_Text statusText;
        public GameObject stackParent;
        public RewindPlaybackPreparer playbackPreparer;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;
        private RewindRecorder _recorder;
        private RewindPlayback _playback;

        private bool playback;
        
        void Start()
        {
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);

            rewindStorage = new RewindStorage(rewindScene, 150, false);
   
            _recorder = new RewindRecorder(rewindScene, rewindStorage, 30, true);
            _playback = new RewindPlayback(rewindScene, rewindStorage);
            
            _recorder.startRecording();
        }

        private void Update()
        {
            if (playback)
            {
                _playback.playbackUpdate();
                
                if (_playback.isPlaybackComplete)
                {
                    playback = false;
                    statusText.text = "Record";
                    
                    _recorder.startRecording();
                    _playback.stopPlayback();
                    playbackPreparer.stopPlayback();
                }
            }
            else
            {
                _recorder.updateRecording();
                statusText.text = $"Record - {rewindStorage.RecordedFrameCount} - {rewindStorage.FrameWriteIndex}";
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Start Replay"))
            {
                statusText.text = "Replay";
                playback = true;
                playbackPreparer.startPlayback();
                _playback.startPlayback();
            }

            GUILayout.EndArea();
        }
    }
}
