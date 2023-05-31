using UnityEngine;

namespace ccl.rewind_plugin
{
    /// <summary>
    /// Helper class that encapsulates the ownership of the RewindRecorder and its update logic
    /// </summary>
    public class RewindRecorderComponent : MonoBehaviour
    {
        private RewindRecorder _recorder;
        private bool _isRecording;
        
        public void startRecording(RewindScene rewindScene, RewindStorage rewindStorage, int recordFPS)
        {
            _recorder = new RewindRecorder(rewindScene, rewindStorage, recordFPS);
            _isRecording = true;
        }

        public void Update()
        {
            if (_isRecording)
            {
                _recorder.updateRecording();
            }
        }

        public void stopRecording()
        {
            _isRecording = false;
        }
    }
}