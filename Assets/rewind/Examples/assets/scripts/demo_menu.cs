using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ccl.rewind_plugin_demos
{
    public class demo_menu : MonoBehaviour
    {
        public void loadSports()
        {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/sports/sports.unity");
        }

        public void loadRewind()
        {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/rewind/rewind.unity");
        }

        public void loadPlayBake()
        {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/baking/play_bake.unity");
        }

    }
}
