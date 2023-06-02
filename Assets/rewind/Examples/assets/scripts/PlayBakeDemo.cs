using System;
using ccl.rewind_plugin;
using UnityEditor;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class PlayBakeDemo : MonoBehaviour
    {
        public GameObject stackParent;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;

        private RewindPlayback rewindPlayback;
      //  private RewindRecorder _recorder;

        void Start()
        {
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);

            rewindStorage = new RewindStorage(rewindScene, 150, false);

            rewindPlayback = new RewindPlayback(rewindScene, rewindStorage);
            
          //  _recorder = new RewindRecorder(rewindScene, rewindStorage, 30);
         
            //start with the simulation paused
            Time.timeScale = 0.0f;
        }

        private void Update()
        {
            if (Time.timeScale > 0.0f)
            {
                rewindPlayback.playbackUpdate();   
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width-200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Play Baked Sim"))
            {
                Time.timeScale = 1.0f;
                
                string path = Application.dataPath;
                path += "/rewind/Examples/assets/bakes/baked_rewind_data";

                rewindStorage.loadFromFile(path);

                rewindPlayback.startPlayback();
            }

            GUILayout.EndArea();
        }
    }

}