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

            rewindStorage = new RewindStorage(rewindScene, 300, false);
   
            _recorder = new RewindRecorder(rewindScene, rewindStorage, 30);
            _playback = new RewindPlayback(rewindScene, rewindStorage);

            statusText.text = "Recording";
        }

        private void Update()
        {
            if (playback)
            {
                _playback.playbackUpdate();
            }
            else
            {
                //if the recording is full then stop
                if (!rewindStorage.isFull)
                {
                    _recorder.updateRecording();
                }
                else
                {
                    statusText.text = "Playback";
                    playback = true;
                    playbackPreparer.startPlayback();
                    _playback.startPlayback();
                }
            }
        }
    }
}
