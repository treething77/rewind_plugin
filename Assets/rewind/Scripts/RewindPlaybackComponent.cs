using UnityEngine;

namespace aeric.rewind_plugin
{
    /// <summary>
    /// Helper class that encapsulates the ownership of the RewindPlayback and its update logic
    /// </summary>
    public class RewindPlaybackComponent : MonoBehaviour
    {
        private RewindPlayback _playback;
        private bool _isPlaying;
  
        public void startPlayback(RewindScene rewindScene, RewindStorage rewindStorage)
        {
            _playback = new RewindPlayback(rewindScene, rewindStorage);
            _isPlaying = _playback.startPlayback();
        }

        public void stopPlayback()
        {
            _isPlaying = false;
            _playback.stopPlayback();
        }
        
        private void Update()
        {
            if (_isPlaying)
            {
                _playback.AdvancePlaybackTime();
                _playback.restoreFrameAtCurrentTime();
            }
        }
    }
}
