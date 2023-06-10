using UnityEngine;
using UnityEngine.SceneManagement;

namespace aeric.rewind_plugin_demos {
    public class menu_return : MonoBehaviour {
        public void menuReturn() {
            //Reset the timescale in case we changed it
            Time.timeScale = 1.0f;
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/menu.unity");
        }
    }
}