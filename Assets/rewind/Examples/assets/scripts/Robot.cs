using System;
using System.Collections;
using System.Collections.Generic;
using ccl.rewind_plugin;
using UnityEngine;
using Random = System.Random;

namespace ccl.rewind_plugin_demos
{
    public class Robot : RewindCustomMonoBehaviourAttributes
    {
        private Transform transform1;

        private Vector3 prevP;
        private Vector3 motion;

        [RewindAttribute]
        public Vector3 moveStartPt;
        
        [RewindAttribute]
        public Vector3 moveTargetPt;

        private MoveTarget moveTarget;
        
        [RewindAttribute]
        public float moveBlendStart;
        
        [RewindAttribute]
        public float moveBlendEnd;

        private Animator a;
        CharacterController c;
        
        private static readonly int Blend = Animator.StringToHash("Blend");

        void Start()
        {
            transform1 = transform;
            prevP = transform1.position;

            //pick a target to move towards
            ChooseTarget();

            a = GetComponent<Animator>();
            c = GetComponent<CharacterController>();

        }

        public override void postRestored()
        {
        }
        
        private void ChooseTarget()
        {
            MoveTarget newTarget = null;
            do
            {
                int moveTargetIndex = UnityEngine.Random.Range(0, MoveTarget.targetList.Count);
                newTarget = MoveTarget.targetList[moveTargetIndex];
            } while (newTarget == moveTarget);

            moveTarget = newTarget;
            moveTargetPt = newTarget.transform.position;
            
            moveStartPt = transform.position;
            moveBlendStart = UnityEngine.Random.Range(0.0f, 1.0f);
            moveBlendEnd = UnityEngine.Random.Range(0.0f, 1.0f);
        }

        private void OnAnimatorMove()
        {
            Vector3 dMove = moveTarget.transform.position - transform1.position;
            dMove.y = 0.0f;

            var gravity = Vector3.up * 5.0f;

            var animMove = a.deltaPosition;
            animMove.y = 0.0f;

            Vector3 actualMove = dMove.normalized * animMove.magnitude;

            c.Move(actualMove - gravity * Time.deltaTime);

            if (dMove.magnitude < 0.7f)
            {
                //choose new target
                ChooseTarget();
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 lookAt = moveTarget.transform.position;
            lookAt.y = transform1.position.y;
            transform1.LookAt(lookAt);

            float moveT = (transform1.position - moveStartPt).magnitude / (moveTarget.transform.position - moveStartPt).magnitude;
            float moveBlend = Mathf.Lerp(moveBlendStart, moveBlendEnd, moveT);
            a.SetFloat(Blend, moveBlend);
        }
    }
}
