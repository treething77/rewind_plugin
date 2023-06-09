using ccl.rewind_plugin;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class Robot : RewindCustomMonoBehaviourAttributes
    {
        public RobotLevel _level;
        
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
      //      if (playbackActive)
        //        return;
            
            Vector3 dMove = moveTargetPt - transform1.position;
            dMove.y = 0.0f;

            var gravity = Vector3.up * 5.0f;

            var animMove = a.deltaPosition;
            animMove.y = 0.0f;

            Vector3 actualMove = dMove.normalized * animMove.magnitude;

            Vector3 moveAmount = actualMove - gravity * Time.deltaTime; 
           // Debug.Log($"Robot move: {moveAmount}");
            c.Move(moveAmount);

            if (dMove.magnitude < 0.7f)
            {
                _level.CaptureTarget(moveTargetIndex, this);
                
                //choose new target
                ChooseTarget();
            }
        }

        // Update is called once per frame
        void Update()
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
