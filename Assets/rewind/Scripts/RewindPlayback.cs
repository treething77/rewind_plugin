using UnityEngine;

namespace ccl.rewind_plugin
{
    internal class RewindPlayback
    {
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;

        public RewindPlayback(RewindScene rewindScene, RewindStorage rewindStorage)
        {
            _rewindScene = rewindScene;
            _rewindStorage = rewindStorage;
        }

        public void playbackUpdate()
        {
            //check time delta against starting time
            //find 2 frame indices to interpolate
            //if only 1 frame then use it for both
            //   i.e. we always interpolate
            
            //foreach handler in scene
            //  read both frames of data
            //  do an interpolated restore
            
            // rewindRestore needs to take 2 readers, which have different read heads
        }

        public void startPlayback()
        {
            //get starting time
        }
    }
}
