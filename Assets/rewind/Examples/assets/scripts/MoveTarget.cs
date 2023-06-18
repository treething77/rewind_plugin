using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class MoveTarget : RewindCustomMonoBehaviourAttributes {
        [Rewind(Lerp = false)] public int CapturedTeamIndex;

        public ParticleSystem captureVFX;
        
        //component reference caching
        private Material _material;

        public Material _vfxMaterialBlue;
        public Material _vfxMaterialRed;

        private new void Awake() {
            _material = GetComponent<MeshRenderer>().material;
            base.Awake();
        }

        private void Update() {
            //lerp color back to the team color
            _material.color = Color.Lerp(_material.color, RobotLevel._instance.GetTeamColor(CapturedTeamIndex), Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime);
        }

        public void Capture(Robot robot) {
            CapturedTeamIndex = robot.Team.teamIndex;

            //turn white and scale up briefly when captured
            _material.color = Color.white;
            transform.localScale = new Vector3(1.5f, 1.8f, 1.5f);

            if (robot.Team.teamIndex == 2)
               captureVFX.GetComponent<Renderer>().material = _vfxMaterialRed;
            else
               captureVFX.GetComponent<Renderer>().material = _vfxMaterialBlue;
           
            captureVFX.Play();
        }
    }
}