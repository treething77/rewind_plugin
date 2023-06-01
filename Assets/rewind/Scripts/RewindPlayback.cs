using UnityEngine;

namespace ccl.rewind_plugin
{
    internal class RewindPlayback
    {
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;

        private float playbackStartTime;
        
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
        }

        public bool startPlayback()
        {
            //get starting time
            playbackStartTime = Time.time;
            return true;
        }

        public void stopPlayback()
        {
        }
    }
}
