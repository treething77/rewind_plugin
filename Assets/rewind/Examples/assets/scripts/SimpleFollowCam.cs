using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class SimpleFollowCam : MonoBehaviour
    {
        public Transform player;
        public float distance = 3;
        public float smoothTime = 0.25f;
        public float minHeight = 2.0f;
        
        
        Vector3 currentVelocity;
     
        void LateUpdate()
        {
            Vector3 target = player.position - (player.transform.forward * distance);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref currentVelocity, smoothTime);

            if (transform.position.y < minHeight)
            {
                transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
            }
            
            transform.LookAt(player);
        }

    }
}
