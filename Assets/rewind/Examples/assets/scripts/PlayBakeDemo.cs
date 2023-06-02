using System;
using ccl.rewind_plugin;
using UnityEditor;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class PlayBakeDemo : MonoBehaviour
    {
        public GameObject stackParent;

        public RewindPlaybackPreparer playbackPreparer;
        
        private RewindScene rewindScene;
        private RewindStorage rewindStorage;
        private RewindPlayback rewindPlayback;

        private float playbackTimer;
        private bool playingBack;
        
        void Start()
        {
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);

            rewindStorage = new RewindStorage(rewindScene, 150, false);

            rewindPlayback = new RewindPlayback(rewindScene, rewindStorage);

            //Load the bake and apply the starting frame to get the settled starting position
            string path = Application.dataPath;
            path += "/rewind/Examples/assets/bakes/baked_rewind_data";

            rewindStorage.loadFromFile(path);

            playbackPreparer.startPlayback();
            rewindPlayback.startPlayback();
            
            rewindPlayback.playbackUpdate();
        }

        private void Update()
        {
            if (playingBack)
            {
                rewindPlayback.playbackUpdate();
                playbackTimer += Time.deltaTime;

                //TODO: better end condition
                if (playbackTimer > 5.0f)
                {
                    rewindPlayback.stopPlayback();
                    playbackPreparer.stopPlayback();
                    playingBack = false;
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width-200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Play Baked Sim"))
            {
                playingBack = true;

                //call again to reset the start time so we get correct relative times
                rewindPlayback.startPlayback();
            }

            GUILayout.EndArea();
        }
    }

}