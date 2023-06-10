using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class MoveTarget : RewindCustomMonoBehaviourAttributes {
        [Rewind(Lerp = false)] public int CapturedTeamIndex;
        //  public static List<MoveTarget> targetList = new List<MoveTarget>();

        private Material _material;

        private void Awake() {
            _material = GetComponent<MeshRenderer>().material;
            base.Awake();
        }

        private void Update() {
            //lerp color back to the team color
            _material.color = Color.Lerp(_material.color, RobotLevel.Instance.GetTeamColor(CapturedTeamIndex), Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime);
        }

        public void Capture(Robot robot) {
            CapturedTeamIndex = robot.Team.teamIndex;

            //turn white and scale up briefly when captured
            _material.color = Color.white;
            transform.localScale = new Vector3(1.5f, 1.8f, 1.5f);
        }
    }
}