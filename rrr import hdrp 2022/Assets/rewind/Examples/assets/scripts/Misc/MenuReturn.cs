using UnityEngine;
using UnityEngine.SceneManagement;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Attached to the back button in each scene to return to main demo menu scene
    /// </summary>
    public class MenuReturn : MonoBehaviour {
        public void menuReturn() {
            //Reset the timescale in case we changed it
            Time.timeScale = 1.0f;
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/menu.unity");
        }
    }
}