using System.Collections.Generic;
using UnityEngine;

namespace ccl.rewind_plugin_demos
{
    public class RobotTeam : MonoBehaviour
    {
        public List<Robot> _robots;

        public Color teamColor;
        public int teamIndex;
        
        private void Start()
        {
            foreach (var robot in _robots)
            {
                robot.Team = this;
            }
        }
    }
}