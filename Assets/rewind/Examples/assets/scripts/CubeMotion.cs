using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class CubeMotion : MonoBehaviour {
        private void Update() {
            var p = transform.position;
            p.x += 0.1f;
            transform.position = p;
        }
    }
}