using System.Collections;
using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Handles the playback of the baked state data.
    /// Spins the camera around and plays with timeScale for "cinematic" effect
    /// </summary>
    public class PlayBakeDemo : MonoBehaviour {
        public GameObject stackParent;

        public RewindPlaybackPreparer playbackPreparer;

        //component reference caching
        private Camera _camera;

        private bool _playingBack;
        private RewindPlayback _rewindPlayback;

        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;

        private void Start() {
            _camera = Camera.main;
            _rewindScene = new RewindScene();
            _rewindScene.addAllChildren(stackParent);

            _rewindStorage = new RewindStorage(_rewindScene, 150, false);
            _rewindPlayback = new RewindPlayback(_rewindScene, _rewindStorage);

            //Load the bake and apply the starting frame to get the settled starting position
            var path = Application.dataPath;
            path += "/rewind/Examples/assets/bakes/baked_rewind_data";

            _rewindStorage.loadFromFile(path);

            playbackPreparer.startPlayback();
            _rewindPlayback.startPlayback();

            _rewindPlayback.restoreFrameAtCurrentTime();

            _camera.transform.LookAt(Vector3.up);
        }

        private void Update() {
            if (_playingBack) {
                _rewindPlayback.AdvancePlaybackTime();
                _rewindPlayback.restoreFrameAtCurrentTime();

                if (_rewindPlayback.isPlaybackComplete) {
                    _rewindPlayback.stopPlayback();
                    playbackPreparer.stopPlayback();
                    _playingBack = false;
                }
            }
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(Screen.width - 200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Play Baked Sim")) {
                _playingBack = true;

                //call again to reset the start time so we get correct relative times
                _rewindPlayback.startPlayback();

                StartCoroutine(BoomCam());
            }

            GUILayout.EndArea();
        }

        private IEnumerator BoomCam() {
            yield return new WaitForSeconds(0.5f);

            //Do a bullet time slowmo rotation around the stack
            Time.timeScale = 0.15f;

            var st = Time.unscaledTime;

            var offset = _camera.transform.position;

            while (Time.unscaledTime < st + 2.0f) {
                // Calculate the desired rotation based on the mouse input
                var rotation = Quaternion.Euler(0f, 100.0f * Time.unscaledDeltaTime, 0f);

                // Apply the rotation to the camera's position
                offset = rotation * offset;

                // Move the camera to the new position around the target point
                _camera.transform.position = offset;

                _camera.transform.LookAt(Vector3.up);

                yield return null;
            }

            //Speed up to accentuate the end of slowmo
            Time.timeScale = 2.0f;
        }
    }
}