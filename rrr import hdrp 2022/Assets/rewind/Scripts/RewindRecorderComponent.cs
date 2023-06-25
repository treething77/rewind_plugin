using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    ///     Helper class that encapsulates the ownership of the RewindRecorder and its update logic
    /// </summary>
    public class RewindRecorderComponent : MonoBehaviour {
        private RewindRecorder _recorder;

        public bool IsRecording { get; private set; }

        public void Update() {
            if (IsRecording) {
                _recorder.updateRecording();
                _recorder.advanceRecordingTime();
            }
        }

        public void startRecording(RewindScene rewindScene, RewindStorage rewindStorage, int recordFPS) {
            _recorder = new RewindRecorder(rewindScene, rewindStorage, recordFPS, false);
            IsRecording = true;
            _recorder.startRecording();
        }

        public void stopRecording() {
            IsRecording = false;
        }
    }
}