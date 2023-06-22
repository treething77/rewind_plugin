using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Handles the AI and player behavior/controls in the rewind/replay demos
    ///
    /// TODO: split into Robot and PlayerRobot
    /// </summary>
    public class Robot : RewindCustomMonoBehaviourAttributes {
        private static readonly int Blend = Animator.StringToHash("Blend");
        
        public RobotLevel _level;
        public bool playerControlled;
        public Camera playerCamera;
        
        public AudioClip footStepSFX;
        public AudioClip footStepSFXBackwards;

        public bool playReversedSoundInPlayback;
        
        // Rewind state
        [Rewind] public Vector3 moveStartPt;
        [Rewind] public Vector3 moveTargetPt;
        [Rewind] public float moveBlendStart;
        [Rewind] public float moveBlendEnd;
        [Rewind(Lerp = false)] public int moveTargetIndex;

        //component reference caching
        private Animator _animator;
        private CharacterController _controller;
        private Transform _transform;

        
        private Vector3 _motion;
        private bool _playbackActive;
        private float _playerSpeed;

        public RobotTeam Team { get; set; }

        private void Start() {
            _transform = transform;
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();

            moveTargetIndex = -1;
        }

        public void Step(int i) {
            if (playerControlled) {
                float vol = 0.7f;
                if (i == 1) vol = 0.4f;
                if (i == 0) vol = 0.2f;

                //If we are in playback mode then play a reversed version of the sound
                var audioSrc = GetComponent<AudioSource>();
                if (_playbackActive && playReversedSoundInPlayback)
                    audioSrc.PlayOneShot(footStepSFXBackwards, vol);
                else
                    audioSrc.PlayOneShot(footStepSFX, vol);
            }
        }


        // Update is called once per frame
        private void Update() {
            if (playerControlled) {
                var keyFwd = Input.GetKey(KeyCode.W);
                var keyLeft = Input.GetKey(KeyCode.A);
                var keyRight = Input.GetKey(KeyCode.D);

                if (keyFwd) {
                    _playerSpeed += Time.deltaTime * 4.0f;
                    _playerSpeed = Mathf.Min(_playerSpeed, 1.0f);
                }
                else {
                    _playerSpeed -= Time.deltaTime * 2.0f;
                }

                _playerSpeed = Mathf.Clamp01(_playerSpeed);

                if (playerCamera != null) {
                    float targetFOV = Mathf.Lerp(55.0f, 65.0f, _playerSpeed);
                    playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 2.0f);
                }

                _animator.SetFloat(Blend, _playerSpeed);

                if (keyLeft) _transform.Rotate(Vector3.up, -100.0f * Time.deltaTime);
                if (keyRight) _transform.Rotate(Vector3.up, 100.0f * Time.deltaTime);
            }
            else {
                //pick a target to move towards
                if (moveTargetIndex == -1)
                    ChooseTarget();

                var position = moveTargetPt;
                var lookAt = position;
                var position1 = _transform.position;
                lookAt.y = position1.y;

                var ogRot = _transform.rotation;
                _transform.LookAt(lookAt);
                _transform.rotation = Quaternion.Lerp(ogRot, _transform.rotation, 0.1f);

                var moveT = (position1 - moveStartPt).magnitude / (position - moveStartPt).magnitude;
                var moveBlend = Mathf.Lerp(moveBlendStart, moveBlendEnd, moveT);
                _animator.SetFloat(Blend, moveBlend);
            }
        }

        private void OnAnimatorMove() {
            var gravity = Vector3.up * 5.0f;
            var animMove = _animator.deltaPosition;
            animMove.y = 0.0f;//ignore vertical motion from animation

            //move forward
            {
                var moveDirection = _transform.forward;
                moveDirection.y = 0.0f;

                var actualMove = moveDirection.normalized * animMove.magnitude;

                var moveAmount = actualMove - gravity * Time.deltaTime;
                _controller.Move(moveAmount);
            }

            if (playerControlled) {
                _level.CaptureTargetsWithinRange(_transform.position, 1.5f, this);
            }
            else {
                if ((moveTargetPt - _transform.position).magnitude < 1.5f) {
                    _level.CaptureTarget(moveTargetIndex, this);

                    //choose new target
                    ChooseTarget();
                }
            }
        }

        public override void startPlayback() {
            _playbackActive = true;
        }

        public override void stopPlayback() {
            _playbackActive = false;
        }

        private void ChooseTarget() {
            moveTargetIndex = _level.FindTarget(this);

            moveTargetPt = _level.GetTargetPosition(moveTargetIndex);

            moveStartPt = transform.position;
            moveBlendStart = Random.Range(0.2f, 1.0f);
            moveBlendEnd = Random.Range(0.2f, 1.0f);
        }
    }
}