using System;
using System.Collections;
using System.Collections.Generic;
using ccl.rewind_plugin;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class MoveTarget : RewindCustomMonoBehaviourAttributes
    {
      //  public static List<MoveTarget> targetList = new List<MoveTarget>();

        private Material _material;

        [Rewind(Lerp=false)]
        public int CapturedTeamIndex;

        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            base.Awake();
        }

        public void Capture(Robot robot)
        {
            CapturedTeamIndex = robot.Team.teamIndex;
            _material.color = Color.white;
        }

        private void Update()
        {
            //lerp color back to the team color
            _material.color = Color.Lerp(_material.color, RobotLevel.Instance.GetTeamColor(CapturedTeamIndex), 0.05f);
        }
    }
}