using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindPlayback
    {
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;

        private float playbackStartTime;

        private bool playbackComplete;
        
        public bool isPlaybackComplete => playbackComplete;
        
        public RewindPlayback(RewindScene rewindScene, RewindStorage rewindStorage)
        {
            _rewindScene = rewindScene;
            _rewindStorage = rewindStorage;
        }

        public void playbackUpdate()
        {
            //check time delta against starting time
            float playbackTimeRelative = Time.time - playbackStartTime;

            //find 2 frame indices to interpolate
            (int frameA, int frameB, float frameT) playbackFrames = _rewindStorage.findPlaybackFrames(playbackTimeRelative);

            foreach (IRewindHandler rewindHandler in _rewindScene.RewindHandlers)
            {
                _rewindStorage.restoreHandlerInterpolated(rewindHandler, playbackFrames.frameA, playbackFrames.frameB, playbackFrames.frameT);
            }

            if (playbackFrames.frameA == playbackFrames.frameB && playbackFrames.frameA == (_rewindStorage.RecordedFrameCount - 1))
            {
                playbackComplete = true;
            }
        }

        public bool startPlayback()
        {
            //get starting time
            playbackStartTime = Time.time;
            playbackComplete = false;
            return true;
        }

        public void stopPlayback()
        {
            //TODO: set playbackComplete?
        }
    }
}
