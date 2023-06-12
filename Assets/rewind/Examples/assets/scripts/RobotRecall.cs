using UnityEngine;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Handles the player motion and controls in the recall demo
    /// </summary>
    public class RobotRecall : MonoBehaviour {
        //animation state constants
        private static readonly int Blend = Animator.StringToHash("Blend");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Land = Animator.StringToHash("Land");

        //inspector references
        public AudioClip footStepSFX;
        
        //component reference caches
        private Animator _animator;
        private CharacterController _controller;
        private Transform _transform;

        
        private Vector3 motion;
        private float playerSpeed;
        private GameObject _platformObject;
        
                
        enum MoveState {
            Moving,
            Jumping,
        }

        private MoveState moveState = MoveState.Moving;

        private Vector3 jumpVelocity;


        private void Start() {
            _transform = transform;
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
        }
        
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

            _animator.SetFloat(Blend, playerSpeed);

            if (keyLeft) _transform.Rotate(Vector3.up, -100.0f * Time.deltaTime);
            if (keyRight) _transform.Rotate(Vector3.up, 100.0f * Time.deltaTime);
            
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 2.0f, -Vector3.up, 4.0f);

            foreach (var hit in hits) {
                if (_controller.isGrounded) {
                    //You should use tags/layers for this. I'm trying not to do that since this will be imported into 
                    //another project
                    if (hit.collider.gameObject.name.Contains("platform")) {
                        _platformObject = hit.collider.gameObject;
                    }
                }
            }

            if (!_controller.isGrounded && _platformObject != null) {
                _platformObject = null;
            }
        }

        private void OnAnimatorMove() {
            var animMove = _animator.deltaPosition;
            animMove.y = 0.0f;//ignore vertical motion from the animation

            Vector3 platformMove = Vector3.zero;
            if (_platformObject != null) {
                RecallPlatform platform = _platformObject.GetComponent<RecallPlatform>();
                platformMove = platform.move;
            }

            //move forward
            if (moveState == MoveState.Moving) {
                var moveDirection = _transform.forward;
                moveDirection.y = 0.0f;

                var actualMove = moveDirection.normalized * animMove.magnitude + platformMove;

                var gravity = Vector3.up * 5.0f;
                var moveAmount = actualMove - gravity * Time.deltaTime;
                
                bool groundedPlayer = _controller.isGrounded;
            
                if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer) {
                    _animator.SetTrigger(Jump);
                    jumpVelocity = actualMove*1.2f + Vector3.up*0.3f;
                    moveState = MoveState.Jumping;
                }
                
                _controller.Move(moveAmount);
            }
            else {
                var gravity = Vector3.up * 1.0f;
                jumpVelocity -= gravity * Time.deltaTime;
                _controller.Move(jumpVelocity);
                if (_controller.isGrounded) {
                    moveState = MoveState.Moving;
                    _animator.SetTrigger(Land);
                }
            }
        }
        
        public void Step(int i) {
            if (_controller.isGrounded) {
                float vol = 0.3f;//running
                if (i == 1) vol = 0.2f;//slower
                if (i == 0) vol = 0.1f;//walk

                var audioSrc = GetComponent<AudioSource>();
                audioSrc.PlayOneShot(footStepSFX, vol);
            }
        }
    }
}