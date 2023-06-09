using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class CubeMotion : MonoBehaviour
    {
        void Update()
        {
            var p = transform.position;
            p.x += 0.1f;
            transform.position = p;
        }
    }
}