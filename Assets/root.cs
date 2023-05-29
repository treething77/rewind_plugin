using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class root : MonoBehaviour
{
    Transform transform1;

    private Vector3 prevP;
    private Vector3 motion;
    
    void Start()
    {
        transform1 = transform;
        prevP = transform1.position;
    }

    private void LateUpdate()
    {
        var p = transform1.position;
        var dp = p - prevP;

        

        dp.y = 0.0f;
        motion = dp;

        p.y = 0.0f;
       // p.z = 0.0f;

        transform1.position = p;// (motion.normalized * (Time.deltaTime * 1.0f));

        transform1.rotation = Quaternion.identity;
        
        
        prevP = p;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
