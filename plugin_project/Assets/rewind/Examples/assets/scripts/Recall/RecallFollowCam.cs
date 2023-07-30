using UnityEngine;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Camera script for the recall demo.
    /// Follows the player behind, raycast against terrain to avoid clipping the ground.
    /// </summary>
    public class RecallFollowCam : MonoBehaviour {
        public Transform player;
        public float distance = 3;
        public float smoothTime = 0.25f;
        public float minHeight = 2.0f;

        private Vector3 _currentVelocity;

        private void LateUpdate() {
            var target = player.position - player.transform.forward * distance;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _currentVelocity, smoothTime);

            float minTargetHeight = player.transform.position.y + minHeight;

            if (Physics.Raycast(transform.position + Vector3.up * 10.0f, -Vector3.up, out var hit, 100.0f)) {
                //You should use tags/layers for this. I'm trying not to do that since this will be imported into 
                //another project
                if (!hit.collider.gameObject.name.Contains("platform")) {
                    if (hit.point.y > (minTargetHeight - 1.0f)) {
                        minTargetHeight = hit.point.y + 1.0f;
                    }
                }
            }
            
            if (transform.position.y < minTargetHeight) transform.position = new Vector3(transform.position.x, minTargetHeight, transform.position.z);

            transform.LookAt(player);
        }
    }
}