using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class root : MonoBehaviour
{
    Transform transform1;

    private Vector3 prevP;
    private Vector3 motion;

    private Vector3 moveStartPt;
    private MoveTarget moveTarget;
    private float moveBlendStart;
    private float moveBlendEnd;

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

    private void ChooseTarget()
    {
        MoveTarget newTarget = null;
        do
        {
            int moveTargetIndex = UnityEngine.Random.Range(0, MoveTarget.targetList.Count);
            newTarget = MoveTarget.targetList[moveTargetIndex];
        } while (newTarget == moveTarget);

        moveTarget = newTarget;

        moveStartPt = transform.position;
        moveBlendStart = UnityEngine.Random.Range(0.0f, 1.0f);
        moveBlendEnd = UnityEngine.Random.Range(0.0f, 1.0f);
    }

    private void OnAnimatorMove()
    {
        Vector3 dMove = moveTarget.transform.position - transform1.position;        
        dMove.y = 0.0f;

        var gravity = Vector3.up*5.0f;
        
        var animMove = a.deltaPosition;
        animMove.y = 0.0f;

        Vector3 actualMove = dMove.normalized * animMove.magnitude;
        
        c.Move(actualMove - gravity*Time.deltaTime);

        if (dMove.magnitude < 0.5f)
        {
            //choose new target
            ChooseTarget();
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform1.LookAt(moveTarget.transform);
        
        float moveT = (moveTarget.transform.position - moveStartPt).magnitude / (transform1.position - moveStartPt).magnitude;
        float moveBlend = Mathf.Lerp(moveBlendStart, moveBlendEnd, moveT);
        a.SetFloat(Blend, moveBlend);
    }
}
