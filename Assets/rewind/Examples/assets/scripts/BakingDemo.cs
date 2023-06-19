using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Demo to show baking state recording to a file for later playback.
    /// </summary>
    public class BakingDemo : MonoBehaviour {
        private const float ExplosionWaitTime = 0.5f;

        //inspector references
        public GameObject stackParent;
        public Rigidbody bomb;

        private float _explosionTimer;
        private bool _recording;

        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;
        private RewindRecorder _recorder;

        private void Start() {
            _rewindScene = new RewindScene();
            _rewindScene.addAllChildren(stackParent);

            _rewindStorage = new RewindStorage(_rewindScene, 150);

            _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 30, false);
        }

        private void Update() {
            if (_recording) {
                if (_explosionTimer < ExplosionWaitTime) {
                    _explosionTimer += Time.deltaTime;
                    if (_explosionTimer >= ExplosionWaitTime) {
                        //set off the "bomb"
                        bomb.velocity = Vector3.up * 20.0f;
                        bomb.AddExplosionForce(100.0f, bomb.transform.position, 1.0f);
                    }
                }

                //if the recording is full then stop
                if (!_rewindStorage.isFull) {
                    _recorder.updateRecording();
                    _recorder.advanceRecordingTime();
                }
            }
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(Screen.width - 200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Start & Record")) {
                _recording = true;
                _recorder.startRecording();
            }

#if UNITY_EDITOR
            if (_rewindStorage.isFull) {
                //Write the recording to a file
                //write to the correct location in the Assets folder
                //get the Assets folder location
                var path = Application.dataPath;
                path += "/rewind/Examples/assets/bakes/baked_rewind_data";

                if (GUILayout.Button("Write Bake To Raw Binary")) {
                    _rewindStorage.writeToRawBinaryFile(path + ".raw");
                }
                if (GUILayout.Button("Write Bake To Binary Stream")) {
                    _rewindStorage.writeToBinaryStreamFile(path + ".bin");
                }
                if (GUILayout.Button("Write Bake To Json")) {
                    _rewindStorage.writeToJsonFile(path + ".json");
                }
            }
#endif

            GUILayout.EndArea();
        }
    }
}