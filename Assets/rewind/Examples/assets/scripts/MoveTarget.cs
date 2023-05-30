using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class MoveTarget : MonoBehaviour
    {
        public static List<MoveTarget> targetList = new List<MoveTarget>();

        private void Awake()
        {
            targetList.Add(this);
        }

    }
}