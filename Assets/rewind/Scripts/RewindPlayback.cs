using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindPlayback
    {
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;

        private float playbackCurrentTime;

        private bool playbackComplete;

        public bool isPlaybackComplete => playbackComplete;
        
        public RewindPlayback(RewindScene rewindScene, RewindStorage rewindStorage)
        {
            _rewindScene = rewindScene;
            _rewindStorage = rewindStorage;
        }

        public float startTime
        {
            get
            {
                //get the times from the times array in the storage
                return _rewindStorage.getTime(0);
            }
        }

        public float endTime
        {
            get
            {
                return _rewindStorage.getTime(_rewindStorage.RecordedFrameCount-1);
            }
        }

        public float currentTime
        {
            get
            {
                return playbackCurrentTime;
            }
        }
        
        public void SetPlaybackTime(float newTime)
        {
            playbackCurrentTime = newTime;
        }

        public void AdvancePlaybackTime()
        {
            playbackCurrentTime += Time.deltaTime;
        }
        
        public void restoreFrameAtCurrentTime()
        {
            //find 2 frame indices to interpolate
            (int frameA, int frameB, float frameT) playbackFrames = _rewindStorage.findPlaybackFrames(playbackCurrentTime);

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
            playbackCurrentTime = startTime;
            playbackComplete = false;
            return true;
        }

        public void stopPlayback()
        {
            //TODO: set playbackComplete?
        }

    }
}
