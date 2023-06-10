using ccl.rewind_plugin;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class Robot : RewindCustomMonoBehaviourAttributes
    {
        public RobotLevel _level;

        public bool playerControlled;
        
        private Transform transform1;

        private Vector3 prevP;
        private Vector3 motion;

        [Rewind]
        public Vector3 moveStartPt;
        
        [Rewind]
        public Vector3 moveTargetPt;

    //    private MoveTarget moveTarget;
        
        [Rewind]
        public float moveBlendStart;
        
        [Rewind]
        public float moveBlendEnd;

        [Rewind(Lerp=false)] public int moveTargetIndex;
        
        private Animator a;
        CharacterController c;
        
        private static readonly int Blend = Animator.StringToHash("Blend");
        public RobotTeam Team { get; set; }

        void Start()
        {
            transform1 = transform;
            prevP = transform1.position;

            //pick a target to move towards
            ChooseTarget();

            a = GetComponent<Animator>();
            c = GetComponent<CharacterController>();
        }

        private bool playbackActive;

        public override void startPlayback()
        {
            playbackActive = true;
        }

        public override void stopPlayback()
        {
            playbackActive = false;
        }
                    
        private void ChooseTarget()
        {
            moveTargetIndex = _level.FindTarget(this);

            moveTargetPt = _level.GetTargetPosition(moveTargetIndex);
     
            moveStartPt = transform.position;
            moveBlendStart = UnityEngine.Random.Range(0.0f, 1.0f);
            moveBlendEnd = UnityEngine.Random.Range(0.0f, 1.0f);
        }

        private void OnAnimatorMove()
        {
            var gravity = Vector3.up * 5.0f;
            var animMove = a.deltaPosition;
            animMove.y = 0.0f;

            //move forward
            {
                Vector3 moveDirection = transform1.forward;
                moveDirection.y = 0.0f;

                Vector3 actualMove = moveDirection.normalized * animMove.magnitude;

                Vector3 moveAmount = actualMove - gravity * Time.deltaTime;
                c.Move(moveAmount);
            }

            if (playerControlled)
            {
                _level.CaptureTargetsWithinRange(transform1.position, 1.5f, this);
            }
            else
            {
                if (( moveTargetPt - transform1.position).magnitude < 1.5f)
                {
                    _level.CaptureTarget(moveTargetIndex, this);

                    //choose new target
                    ChooseTarget();
                }
            }
        }

        private float playerSpeed = 0.0f;

        // Update is called once per frame
        void Update()
        {
            if (playerControlled)
            {
                
                bool keyFwd = Input.GetKey(KeyCode.W);
                bool keyLeft = Input.GetKey(KeyCode.A);
                bool keyRight = Input.GetKey(KeyCode.D);

                if (keyFwd)
                {
                    playerSpeed += Time.deltaTime * 4.0f;
                    playerSpeed = Mathf.Min(playerSpeed, 1.0f);
                }
                else
                {
                    playerSpeed -= Time.deltaTime * 2.0f;
                }

                playerSpeed = Mathf.Clamp01(playerSpeed);
                
                a.SetFloat(Blend, playerSpeed);

                if (keyLeft)
                {
                    transform1.Rotate(Vector3.up, -100.0f * Time.deltaTime);
                }
                if (keyRight)
                {
                    transform1.Rotate(Vector3.up, 100.0f * Time.deltaTime);
                }
                
            }
            else
            {
                var position = moveTargetPt;
                Vector3 lookAt = position;
                var position1 = transform1.position;
                lookAt.y = position1.y;

                var ogRot = transform1.rotation;
                transform1.LookAt(lookAt);
                transform1.rotation = Quaternion.Lerp(ogRot, transform1.rotation, 0.1f);

                float moveT = (position1 - moveStartPt).magnitude / (position - moveStartPt).magnitude;
                float moveBlend = Mathf.Lerp(moveBlendStart, moveBlendEnd, moveT);
                a.SetFloat(Blend, moveBlend);
            }
        }
    }
}
