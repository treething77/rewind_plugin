using UnityEngine;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Trivial script to move an object along the x axis at a fixed amount each frame. 
    /// </summary>
    public class CubeMotion : MonoBehaviour {
        private void Update() {
            var p = transform.position;
            p.x += 0.1f;
            transform.position = p;
        }
    }
}