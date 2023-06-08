using UnityEngine;

namespace ccl.rewind_plugin
{
    /// <summary>
    /// This is a basic implementation of a recorder that records at a specified framerate for
    /// all objects in the RewindScene.
    /// </summary>
    public class RewindRecorder
    {
        private readonly RewindScene _rewindScene;
        private readonly RewindStorage _rewindStorage;
        private readonly int _recordFPS;
        private readonly bool _continuous;
        
        private float timeSinceFrameRecorded = 0.0f;
        private float _recordingStartTime;

        public RewindRecorder(RewindScene rewindScene, RewindStorage rewindStorage, int recordFPS, bool continuousRecording)
        {
            _rewindScene = rewindScene;
            _rewindStorage = rewindStorage;
            _recordFPS = recordFPS;
            _continuous = continuousRecording;
        }

        public void updateRecording()
        {
            if (!_continuous)
            {
                //check if we are full and don't allow any more recording
                if (_rewindStorage.isFull) return;
            }

            //Always record if nothing recorded yet or if 0 is specified as the fps
            bool recordSnapshot = _rewindStorage.RecordedFrameCount == 0 || (_recordFPS == 0);
            if (!recordSnapshot)
            {
                float recordTimeInterval = 1.0f / _recordFPS;
                if (timeSinceFrameRecorded >= recordTimeInterval)
                {
                    recordSnapshot = true;
                }
            }

            if (recordSnapshot)
            {
                timeSinceFrameRecorded = 0.0f;
            }
            else
            {
                timeSinceFrameRecorded += Time.deltaTime;
            }

            //stop if we run out of storage

            if (recordSnapshot)
            {
                recordFrame();
            }
        }
        
        public void recordFrame()
        {
            //TODO: write the timestamp of the frame? or will that go into individual component data frames?
            //   after all the goal is we don't have to update all components every frame, and otherwise how 
            //   do we know which ones were updated when for the replay?
            // For now just worry about the basic case. Other cases will maybe require custom recorders.
            float currentRelativeTime = Time.time - _recordingStartTime;
            _rewindStorage.writeFrameStart(currentRelativeTime);

            //foreach object in scene
            foreach (IRewindHandler rewindHandler in _rewindScene.RewindHandlers)
            {
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

            _rewindStorage.writeFrameEnd();
        }

        public void startRecording()
        {
            _recordingStartTime = Time.time;
        }

    }
}
