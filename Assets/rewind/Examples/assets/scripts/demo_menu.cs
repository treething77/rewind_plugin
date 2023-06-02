using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ccl.rewind_plugin_demos
{
    public class demo_menu : MonoBehaviour
    {
        public void loadSportsReplay()
        {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/sports-replay/sports-replay.unity");
        }
        
        public void loadSportsRewind()
        {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/sports-rewind/sports-rewind.unity");
        }

        public void loadRecall()
        {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/recall/recall.unity");
        }

        public void loadPlayBake()
        {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/baking/play_bake.unity");
        }

    }
}
