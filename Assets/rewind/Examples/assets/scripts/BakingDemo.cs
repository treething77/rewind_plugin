using ccl.rewind_plugin;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class BakingDemo : MonoBehaviour
    {
        public GameObject stackParent;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;
        
        void Start()
        {
            rewindScene = new RewindScene();
            rewindScene.addAllChildren(stackParent);

            rewindStorage = new RewindStorage(rewindScene, 90, false);

            //start with the simulation paused
            Time.timeScale = 0.0f;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width-200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Start Sim & Record"))
            {
                Time.timeScale = 1.0f;
                
                
            }
            
            GUILayout.EndArea();
        }
    }

}