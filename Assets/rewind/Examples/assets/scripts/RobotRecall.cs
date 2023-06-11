using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class RobotRecall : MonoBehaviour {
        private static readonly int Blend = Animator.StringToHash("Blend");
        
        private Animator a;
        private CharacterController c;
        private Vector3 motion;

        private float playerSpeed;

        private Transform transform1;

        private void Start() {
            transform1 = transform;
            a = GetComponent<Animator>();
            c = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        private void Update() {
            var keyFwd = Input.GetKey(KeyCode.W);
            var keyLeft = Input.GetKey(KeyCode.A);
            var keyRight = Input.GetKey(KeyCode.D);

            if (keyFwd) {
                playerSpeed += Time.deltaTime * 4.0f;
                playerSpeed = Mathf.Min(playerSpeed, 1.0f);
            }
            else {
                playerSpeed -= Time.deltaTime * 2.0f;
            }

            playerSpeed = Mathf.Clamp01(playerSpeed);

            a.SetFloat(Blend, playerSpeed);

            if (keyLeft) transform1.Rotate(Vector3.up, -100.0f * Time.deltaTime);
            if (keyRight) transform1.Rotate(Vector3.up, 100.0f * Time.deltaTime);
        }

        private void OnAnimatorMove() {
            var gravity = Vector3.up * 5.0f;
            var animMove = a.deltaPosition;
            animMove.y = 0.0f;

            //move forward
            {
                var moveDirection = transform1.forward;
                moveDirection.y = 0.0f;

                var actualMove = moveDirection.normalized * animMove.magnitude;

                var moveAmount = actualMove - gravity * Time.deltaTime;
                c.Move(moveAmount);
            }
        }
    }
}