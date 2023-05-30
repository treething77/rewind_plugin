using UnityEngine;

namespace ccl.rewind_plugin
{
    internal class RewindRecorder
    {
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;

        public RewindRecorder(RewindScene rewindScene, RewindStorage rewindStorage)
        {
            _rewindScene = rewindScene;
            _rewindStorage = rewindStorage;
        }

        public void updateRecording()
        {
            throw new System.NotImplementedException();
        }
    }
}
