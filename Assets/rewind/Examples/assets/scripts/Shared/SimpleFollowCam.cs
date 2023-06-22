using UnityEngine;

namespace aeric.rewind_plugin_demos {
    //Used in the Rewind and Recall demos
    public class SimpleFollowCam : MonoBehaviour {
        public Transform player;
        public float distance = 3;
        public float smoothTime = 0.25f;
        public float minHeight = 2.0f;

        private Vector3 _currentVelocity;

        private void LateUpdate() {
            var target = player.position - player.transform.forward * distance;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _currentVelocity, smoothTime);

            float minTargetRelativeHeight = player.transform.position.y + minHeight;
            if (transform.position.y < minTargetRelativeHeight) transform.position = new Vector3(transform.position.x, minTargetRelativeHeight, transform.position.z);

            transform.LookAt(player);
        }
    }
}