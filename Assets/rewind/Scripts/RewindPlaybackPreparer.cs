using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindPlaybackPreparer : MonoBehaviour
    {
        public GameObject playbackRoot;
        
        private List<Behaviour> nonPlaybackEnabledBehaviors = new List<Behaviour>();
        private List<Rigidbody> nonKinematicRigidBodies = new List<Rigidbody>();

        public void startPlayback()
        {
            //get the list of unique objects that have rewind components attached to them
            RewindComponentBase[] allRewindHandlers = playbackRoot.GetComponentsInChildren<RewindComponentBase>();

            HashSet<GameObject> rewindObjects = allRewindHandlers.Select(rewindComponentBase => rewindComponentBase.gameObject).ToHashSet();
            
            foreach (var rewindObject in rewindObjects)
            {
                //disable components that we don't want active during playback
                foreach (Component component in rewindObject.GetComponentsInChildren<Component>())
                {
                    //if it wasn't already enabled then ignore it
                    if (component is Rigidbody)
                    {
                        Rigidbody rigidBody = component as Rigidbody;
                        if (!rigidBody.isKinematic)
                        {
                            rigidBody.isKinematic = true;
                            nonKinematicRigidBodies.Add(rigidBody);
                        }
                    }
                    else if (component is Behaviour)
                    {
                        Behaviour behavior = component as Behaviour;
                        if (behavior.enabled)
                        {
                            behavior.enabled = false;
                            nonPlaybackEnabledBehaviors.Add(behavior);
                        }
                    }
                }
            }
        }

        public void stopPlayback()
        {
            foreach (Behaviour component in nonPlaybackEnabledBehaviors)
            {
                component.enabled = true;
            }

            foreach (Rigidbody rigidBody in nonKinematicRigidBodies)
            {
                rigidBody.isKinematic = false;
            }
        }
    }
}