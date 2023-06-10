using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ccl.rewind_plugin_demos
{
    public class menu_return : MonoBehaviour
    {
        public void menuReturn()
        {
            //Reset the timescale in case we changed it
            Time.timeScale = 1.0f;
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/menu.unity");
        }
    }
}
