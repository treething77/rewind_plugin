using UnityEngine;

namespace ccl.rewind_plugin
{
    /// <summary>
    /// Helper class that encapsulates the ownership of the RewindRecorder and its update logic
    /// </summary>
    public class RewindRecorderComponent : MonoBehaviour
    {
        private RewindRecorder _recorder;
        
        public void startRecording(RewindScene rewindScene, RewindStorage rewindStorage)
        {
            _recorder = new RewindRecorder(rewindScene, rewindStorage);
        }

        public void Update()
        {
            _recorder.updateRecording();
        }

        public void stopRecording()
        {
            throw new System.NotImplementedException();
        }
    }
}
