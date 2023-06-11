using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class Robot : RewindCustomMonoBehaviourAttributes {
        private static readonly int Blend = Animator.StringToHash("Blend");
        public RobotLevel _level;

        public bool playerControlled;

        [Rewind] public Vector3 moveStartPt;

        [Rewind] public Vector3 moveTargetPt;

        //    private MoveTarget moveTarget;

        [Rewind] public float moveBlendStart;

        [Rewind] public float moveBlendEnd;

        [Rewind(Lerp = false)] public int moveTargetIndex;

        private Animator a;
        private CharacterController c;
        private Vector3 motion;

        private bool playbackActive;

        private float playerSpeed;

        private Vector3 prevP;

        private Transform transform1;
        public RobotTeam Team { get; set; }

        private void Start() {
            transform1 = transform;
            prevP = transform1.position;

            a = GetComponent<Animator>();
            c = GetComponent<CharacterController>();

            moveTargetIndex = -1;
        }

        // Update is called once per frame
        private void Update() {
            if (playerControlled) {
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
            else {
                //pick a target to move towards
                if (moveTargetIndex == -1)
                    ChooseTarget();

                var position = moveTargetPt;
                var lookAt = position;
                var position1 = transform1.position;
                lookAt.y = position1.y;

                var ogRot = transform1.rotation;
                transform1.LookAt(lookAt);
                transform1.rotation = Quaternion.Lerp(ogRot, transform1.rotation, 0.1f);

                var moveT = (position1 - moveStartPt).magnitude / (position - moveStartPt).magnitude;
                var moveBlend = Mathf.Lerp(moveBlendStart, moveBlendEnd, moveT);
                a.SetFloat(Blend, moveBlend);
            }
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

            if (playerControlled) {
                _level.CaptureTargetsWithinRange(transform1.position, 1.5f, this);
            }
            else {
                if ((moveTargetPt - transform1.position).magnitude < 1.5f) {
                    _level.CaptureTarget(moveTargetIndex, this);

                    //choose new target
                    ChooseTarget();
                }
            }
        }

        public override void startPlayback() {
            playbackActive = true;
        }

        public override void stopPlayback() {
            playbackActive = false;
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