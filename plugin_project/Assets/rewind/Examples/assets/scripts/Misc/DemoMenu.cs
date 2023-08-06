using UnityEngine;
using UnityEngine.SceneManagement;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Demo menu implementation for accessing various demo scenes.
    /// </summary>
    public class DemoMenu : MonoBehaviour {
        public void loadSportsReplay() {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/demos/sports-replay/sports-replay.unity");
        }

        public void loadSportsRewind() {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/demos/sports-rewind/sports-rewind.unity");
        }

        public void loadRecall() {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/demos/recall/recall.unity");
        }

        public void loadPlayBake() {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/demos/baking/play_bake.unity");
        }

        public void loadSimple() {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/tests/simple/simple.unity");
        }

        public void loadSimpleRewind() {
            SceneManager.LoadScene("Assets/rewind/Examples/scenes/tests/simple-rewind/simple-rewind.unity");
        }
    }
}