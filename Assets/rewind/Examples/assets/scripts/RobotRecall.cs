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
        private static readonly int Jump = Animator.StringToHash("Jump");

        private void Start() {
            transform1 = transform;
            a = GetComponent<Animator>();
            c = GetComponent<CharacterController>();
        }

        private float jumpCooldown;
        
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
        
        enum MoveState {
            Moving,
            Jumping,
        }

        private MoveState moveState = MoveState.Moving;

        private Vector3 jumpVelocity;
        private static readonly int Land = Animator.StringToHash("Land");

        private void OnAnimatorMove() {
            var animMove = a.deltaPosition;
            animMove.y = 0.0f;

            //move forward
            if (moveState == MoveState.Moving) {
                var moveDirection = transform1.forward;
                moveDirection.y = 0.0f;

                var actualMove = moveDirection.normalized * animMove.magnitude;

                var gravity = Vector3.up * 5.0f;
                var moveAmount = actualMove - gravity * Time.deltaTime;
                
                bool groundedPlayer = c.isGrounded;
            
                if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer) {
                    a.SetTrigger(Jump);
                    jumpCooldown = 1.0f;
                 //   moveAmount.y += 5.0f;

                    jumpVelocity = actualMove + Vector3.up*0.3f;
                    moveState = MoveState.Jumping;
                }
                
                c.Move(moveAmount);
            }
            else {
                var gravity = Vector3.up * 1.0f;
                jumpVelocity -= gravity * Time.deltaTime;
                c.Move(jumpVelocity);
                if (c.isGrounded) {
                    moveState = MoveState.Moving;
                    a.SetTrigger(Land);
                }
            }
        }
    }
}