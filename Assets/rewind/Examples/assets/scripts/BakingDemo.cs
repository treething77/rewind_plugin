using System;
using ccl.rewind_plugin;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class BakingDemo : MonoBehaviour
    {
        public GameObject stackParent;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;
        private RewindRecorder _recorder;

        void Start()
        {
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);

            rewindStorage = new RewindStorage(rewindScene, 150, false);
   
            _recorder = new RewindRecorder(rewindScene, rewindStorage, 30);
         
            //start with the simulation paused
            Time.timeScale = 0.0f;
        }

        private void Update()
        {
            if (Time.timeScale > 0.0f)
            {
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
            if (GUILayout.Button("Start Sim & Record"))
            {
                Time.timeScale = 1.0f;
                _recorder.startRecording();
            }

            if (rewindStorage.isFull)
            {
                if (GUILayout.Button("Write Bake To File"))
                {
                    //Write the recording to a file
                    rewindStorage.writeToFile("baked_rewind_data");
                }
            }
            
            GUILayout.EndArea();
        }
    }

}