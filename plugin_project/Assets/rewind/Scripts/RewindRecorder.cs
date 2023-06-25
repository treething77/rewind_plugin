using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    ///     Records frame data at a specified framerate for
    ///     all objects in the RewindScene.
    /// </summary>
    public class RewindRecorder {
        private readonly bool _loopRecordingWhenFull;
        private readonly int _recordFPS;
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;
        private float _lastFrameWriteTime;

        private float _recordingTime;

        public float RecordingTime => _recordingTime;
        
        public RewindRecorder(RewindScene rewindScene, RewindStorage rewindStorage, int recordFPS, bool loopRecordingWhenFullRecording) {
            _rewindScene = rewindScene;
            _rewindStorage = rewindStorage;
            _recordFPS = recordFPS;
            _loopRecordingWhenFull = loopRecordingWhenFullRecording;
        }

        public void updateRecording() {
            if (!_loopRecordingWhenFull)
                //check if we are full and don't allow any more recording
                if (_rewindStorage.isFull)
                    return;

            //Always record if nothing recorded yet or if 0 is specified as the fps
            var recordSnapshot = _rewindStorage.RecordedFrameCount == 0;
            if (!recordSnapshot) {
                var timeSinceFrameRecorded = _recordingTime - _lastFrameWriteTime;
                if (_recordFPS == 0) {
                    //there still needs to be some time change so we aren't recording while paused
                    recordSnapshot = timeSinceFrameRecorded > 0.0f;
                }
                else {
                    var recordTimeInterval = 1.0f / _recordFPS;
                    if (timeSinceFrameRecorded >= recordTimeInterval) recordSnapshot = true;
                }
            }
            if (recordSnapshot) recordFrame();
        }

        public void advanceRecordingTime() {
            _recordingTime += Time.deltaTime;
        }

        public void recordFrame() {
            _rewindStorage.writeFrameStart(_recordingTime);
            _lastFrameWriteTime = _recordingTime;

            foreach (var rewindHandler in _rewindScene.RewindHandlers)
                _rewindStorage.writeHandlerFrame(rewindHandler);

            _rewindStorage.writeFrameEnd();
        }

        public void startRecording() {
            foreach (var rewindHandler in _rewindScene.RewindHandlers) {
                rewindHandler.startRecording();
            }
        }

        public void setRecordTime(float newPlaybackTime) {
            _recordingTime = newPlaybackTime;

            var recordTimeInterval = 1.0f / _recordFPS;
            _lastFrameWriteTime = _recordingTime - recordTimeInterval - 1.0f;
        }
    }
}