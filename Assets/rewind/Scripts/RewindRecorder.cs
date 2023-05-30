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
            //foreach object in scene
            foreach (IRewindHandler rewindHandler in _rewindScene.RewindHandlers)
            {
                //should we record this frame?
                //if so, record it
                
                //when we create the storage, allocate a section of it for each handler
                //then each handler writes into that
                //dont store storage details IN the handler, because then that limits us to 1
                //scene per handler. Can't then have handlers in multiple scenes.
                
                //so the details of where the handler writes to is stored in the storage itself
                //the storage needs to store a map of ID->storage location
                //the storage location will then store the array of frame data
                    
                //write the data to the rewind storage
                rewindHandler.rewindStore();
            }
        }
    }
}
