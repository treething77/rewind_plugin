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

        public void startPlayback()
        {
            _playback = new RewindPlayback();
        }

        private void Update()
        {
            _playback.playbackUpdate();
        }
    }
}
