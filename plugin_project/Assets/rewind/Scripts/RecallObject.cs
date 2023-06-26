using UnityEngine;

namespace aeric.rewind_plugin {
    /// <summary>
    /// Encapsulates the entire rewind/playback system for adding rewind capability to a single object.
    /// Used when you want to be able to pause the scene and rewind a single object.
    /// </summary>
    public class RecallObject {
        
        public enum RecallObjectState {
            Recording,//moving forwards in time, recording data
            Paused,//object is paused, not recording or playing back
            Rewinding//moving backwards in time, playing back data
        }

        public RecallObjectState RecallState { get; set; }= RecallObjectState.Recording;

        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;
        private RewindPlayback _playback;
        private RewindRecorder _recorder;
        private float _newPlaybackTime = -1.0f;
        
        private IRewindHandler _rewindHandler;
        private IRewindDataHandler _dataHandler;

        public void Initialize(IRewindHandler handler, IRewindDataHandler dataHandler) {
            _rewindScene = new RewindScene();
            _rewindScene.addRewindHandler(handler);
            _rewindHandler = handler;
            _dataHandler = dataHandler;

            _rewindStorage = new RewindStorage(_rewindScene, 100);
            _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 10, true);
            _playback = new RewindPlayback(_rewindScene, _rewindStorage);
            _recorder.startRecording();
        }

        public void UpdateRecording() {
            _recorder.updateRecording();
            _recorder.advanceRecordingTime();
        }

        public void Dispose() {
            _rewindStorage.Dispose();
        }

        public void StartPlayback() {
            var endTime = _playback.endTime;
            _playback.startPlayback();
            //start at the end
            _playback.SetPlaybackTime(endTime);
            _newPlaybackTime = endTime;
        }

        public void StartRecording() {
            _playback.stopPlayback();
            _recorder.startRecording();
        }

        public void StopRewind() {
            //When we are done rewinding we call into the storage to reset the write state to that point
            //so we can move forwards from there
            var frameInfo = _rewindStorage.findPlaybackFrames(_newPlaybackTime);

            var currentFrameCount = _rewindStorage.RecordedFrameCount;
            var newUnmappedEndFrame = frameInfo.frameUnmappedB;

            _rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);
            _recorder.setRecordTime(_newPlaybackTime);
        }

        public float GetRecallTimeLeft() {
            var currentTime = _playback.currentTime;
            var startTime = _playback.startTime;
            return currentTime - startTime;
        }

        public void RewindByTime(float deltaTime) {
            var currentTime = _playback.currentTime;
            var startTime = _playback.startTime;

            _newPlaybackTime = currentTime - deltaTime;
            if (_newPlaybackTime < startTime) _newPlaybackTime = startTime;

            _playback.SetPlaybackTime(_newPlaybackTime);
            _playback.restoreFrameAtCurrentTime();

            //Get all the points in the platforms rewind path
            var frameInfo = _rewindStorage.findPlaybackFrames(_newPlaybackTime);

            int startPathFrame = 0;
            int endPathFrame = frameInfo.frameUnmappedB;

            for (int i = startPathFrame; i <= endPathFrame; i++) {
                _rewindStorage.getUnmappedFrameData(i, _rewindHandler, _dataHandler);
            }
        }
    }

}