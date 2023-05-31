using UnityEngine;

namespace ccl.rewind_plugin
{
    /// <summary>
    /// This is a basic implementation of a recorder that records at a specified framerate for
    /// all objects in the RewindScene.
    /// </summary>
    internal class RewindRecorder
    {
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;
        private readonly int _recordFPS;

        public RewindRecorder(RewindScene rewindScene, RewindStorage rewindStorage, int recordFPS)
        {
            _rewindScene = rewindScene;
            _rewindStorage = rewindStorage;
            _recordFPS = recordFPS;
        }

        public void updateRecording()
        {
            //stop if we run out of storage
            
            //foreach object in scene
            foreach (IRewindHandler rewindHandler in _rewindScene.RewindHandlers)
            {
                //should we record this frame?
                //need to write the framerate check
                //when did we last record? do we need to again?
                
                //when we create the storage, allocate a section of it for each handler
                //then each handler writes into that
                //dont store storage details IN the handler, because then that limits us to 1
                //scene per handler. Can't then have handlers in multiple scenes.
                
                //the details of where the handler writes to is stored in the storage itself
                //the storage needs to store a map of ID->storage location
                //the storage location will then store the array of frame data
                    
                //write the data to the rewind storage
                _rewindStorage.writeHandlerFrame(rewindHandler);
            }
        }
    }
}
