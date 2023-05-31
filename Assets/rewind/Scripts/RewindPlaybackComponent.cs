using System;
using UnityEngine;

namespace ccl.rewind_plugin
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
            _isPlaying = true;

            _playback.startPlayback();
        }

        private void Update()
        {
            if (_isPlaying)
            {
                _playback.playbackUpdate();
            }
        }
    }
}
