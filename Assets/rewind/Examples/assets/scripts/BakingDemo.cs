using System;
using ccl.rewind_plugin;
using UnityEditor;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class BakingDemo : MonoBehaviour
    {
        public GameObject stackParent;
        public Rigidbody bomb;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;
        private RewindRecorder _recorder;

        private bool recording;
        private float explosionTimer;

        private const float ExplosionWaitTime = 0.5f;

        void Start()
        {
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);

            rewindStorage = new RewindStorage(rewindScene, 150, false);
   
            _recorder = new RewindRecorder(rewindScene, rewindStorage, 30, false);
        }

        private void Update()
        {
            if (recording)
            {
                if (explosionTimer < ExplosionWaitTime)
                {
                    explosionTimer += Time.deltaTime;
                    if (explosionTimer >= ExplosionWaitTime)
                    {
                        //set off the "bomb"
                        bomb.velocity = Vector3.up * 20.0f;
                        bomb.AddExplosionForce(100.0f, bomb.transform.position, 1.0f);
                    }
                }

                //if the recording is full then stop
                if (!rewindStorage.isFull)
                {
                    _recorder.updateRecording();
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width-200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Start & Record"))
            {
                recording = true;
                _recorder.startRecording();
            }
            
            #if UNITY_EDITOR
            if (rewindStorage.isFull)
            {
                if (GUILayout.Button("Write Bake To File"))
                {
                    //Write the recording to a file
                    //write to the correct location in the Assets folder
                    
                    //get the Assets folder location
                    string path = Application.dataPath;
                    path += "/rewind/Examples/assets/bakes/baked_rewind_data";

                    rewindStorage.writeToFile(path);
                }
            }
            #endif            

            GUILayout.EndArea();
        }
    }

}