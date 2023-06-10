using System.Collections;
using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class PlayBakeDemo : MonoBehaviour {
        public GameObject stackParent;

        public RewindPlaybackPreparer playbackPreparer;

        private Camera _camera;

        private bool playingBack;
        private RewindPlayback rewindPlayback;

        private RewindScene rewindScene;
        private RewindStorage rewindStorage;

        private void Start() {
            _camera = Camera.main;
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);

            rewindStorage = new RewindStorage(rewindScene, 150, false);

            rewindPlayback = new RewindPlayback(rewindScene, rewindStorage);

            //Load the bake and apply the starting frame to get the settled starting position
            var path = Application.dataPath;
            path += "/rewind/Examples/assets/bakes/baked_rewind_data";

            rewindStorage.loadFromFile(path);

            playbackPreparer.startPlayback();
            rewindPlayback.startPlayback();

            rewindPlayback.restoreFrameAtCurrentTime();

            _camera.transform.LookAt(Vector3.up);
        }

        private void Update() {
            if (playingBack) {
                rewindPlayback.AdvancePlaybackTime();
                rewindPlayback.restoreFrameAtCurrentTime();

                if (rewindPlayback.isPlaybackComplete) {
                    rewindPlayback.stopPlayback();
                    playbackPreparer.stopPlayback();
                    playingBack = false;
                }
            }
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(Screen.width - 200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Play Baked Sim")) {
                playingBack = true;

                //call again to reset the start time so we get correct relative times
                rewindPlayback.startPlayback();

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