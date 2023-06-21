using UnityEngine;

namespace aeric.rewind_plugin {
    public class RewindPlayback {
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;

        public RewindPlayback(RewindScene rewindScene, RewindStorage rewindStorage) {
            _rewindScene = rewindScene;
            _rewindStorage = rewindStorage;
        }

        public bool isPlaybackComplete { get; private set; }

        public float startTime => _rewindStorage.getTime(0);

        public float endTime => _rewindStorage.getTime(_rewindStorage.RecordedFrameCount - 1);

        public float currentTime { get; private set; }

        public void SetPlaybackTime(float newTime) {
            currentTime = newTime;
        }

        public void AdvancePlaybackTime() {
            currentTime += Time.deltaTime;
        }

        public void restoreFrameAtCurrentTime() {
            //find 2 frame indices to interpolate
            var playbackFrames = _rewindStorage.findPlaybackFrames(currentTime);

            foreach (var rewindHandler in _rewindScene.RewindHandlers) _rewindStorage.restoreHandlerInterpolated(rewindHandler, playbackFrames.frameMappedA, playbackFrames.frameMappedB, playbackFrames.frameT);

            if (playbackFrames.frameMappedA == playbackFrames.frameMappedB && playbackFrames.frameMappedA > 0) isPlaybackComplete = true;
        }

        public bool startPlayback() {
            //get starting time
            currentTime = startTime;
            isPlaybackComplete = false;
            return true;
        }

        public void stopPlayback() {
        }
    }
}