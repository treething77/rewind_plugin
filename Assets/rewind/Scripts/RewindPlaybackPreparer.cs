using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace aeric.rewind_plugin {
    public class RewindPlaybackPreparer : MonoBehaviour {
        public List<GameObject> playbackRoots;
        private readonly List<Rigidbody> nonKinematicRigidBodies = new();

        private readonly List<Behaviour> nonPlaybackEnabledBehaviors = new();
        private readonly List<RewindComponentBase> playbackComponents = new();

        public void startPlayback() {
            foreach (var playbackRoot in playbackRoots) {
                //get the list of unique objects that have rewind components attached to them
                var allRewindHandlers = playbackRoot.GetComponentsInChildren<RewindComponentBase>();

                var rewindObjects = allRewindHandlers.Select(rewindComponentBase => rewindComponentBase.gameObject).ToHashSet();

                //TODO: this is too nested and could use some caching
                foreach (var rewindObject in rewindObjects) {
                    //get all rewind components on this GameObject and ask them to filter the other components 
                    //that are parented to that GameObject
                    var rewindComponents = rewindObject.GetComponentsInChildren<RewindComponentBase>();
                    var generalComponents = rewindObject.GetComponentsInChildren<Component>();

                    playbackComponents.AddRange(rewindComponents);

                    foreach (var r in rewindComponents) r.startPlayback();

                    //    foreach (RewindComponentBase rewindComponent in rewindComponents)
                    {
                        //disable components that we don't want active during playback
                        foreach (var component in generalComponents)
                            if (component is RewindComponentBase) {
                                //For rewind components disable them unless they say otherwise. They may have other 
                                //logic in Update etc that we don't want running during a replay, or they may take
                                //care of that themselves by querying the playback state.
                                var rewindComponentBase = (RewindComponentBase)component;
                                if (rewindComponentBase.enabled && !rewindComponentBase.ShouldStayEnabledDuringReplay) {
                                    rewindComponentBase.enabled = false;
                                    nonPlaybackEnabledBehaviors.Add(rewindComponentBase);
                                }
                            }
                            else if (component is Rigidbody) {
                                var rigidBody = component as Rigidbody;
                                //We can't disable rigid bodies but we can make them kinematic
                                if (!rigidBody.isKinematic) {
                                    rigidBody.isKinematic = true;
                                    nonKinematicRigidBodies.Add(rigidBody);
                                }
                            }
                            else if (component is Behaviour) {
                                //Disable if all rewind components agree it should be disabled
                                if (rewindComponents.All(x => x.shouldDisableComponent(component))) {
                                    var behavior = component as Behaviour;
                                    //if it wasn't already enabled then ignore it
                                    if (behavior.enabled) {
                                        behavior.enabled = false;
                                        nonPlaybackEnabledBehaviors.Add(behavior);
                                    }
                                }
                            }
                    }
                }
            }
        }

        public void stopPlayback() {
            foreach (var component in nonPlaybackEnabledBehaviors) component.enabled = true;

            foreach (var rigidBody in nonKinematicRigidBodies) rigidBody.isKinematic = false;

            foreach (var r in playbackComponents) r.stopPlayback();
        }
    }
}