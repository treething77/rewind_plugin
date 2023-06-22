using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    /// <summary>
    /// Handles the player motion and controls in the recall demo. There are some
    /// complications in this to handle smooth root motion on moving platforms.
    /// </summary>
    public class RobotRecall : MonoBehaviour {
        //animation state constants
        private static readonly int Blend = Animator.StringToHash("Blend");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Land = Animator.StringToHash("Land");

        //inspector references
        public AudioClip footStepSFX;
        public RecallScanningUI scanningUI;

        public List<RecallPlatform> platforms;

        public List<Material> platformMaterials;
        public Material scanningMaterial;
        
        //component reference caches
        private Animator _animator;
        private CharacterController _controller;
        private Transform _transform;

        private RecallPlatform highlightedPlatform;
        
        private Vector3 motion;
        private float playerSpeed;
        private GameObject _platformObject;
        
                
        enum MoveState {
            Moving,
            Jumping,
        }

        private MoveState moveState = MoveState.Moving;

        private Vector3 jumpVelocity;


        private void Start() {
            _transform = transform;
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
        }

        private bool _scanningEnabled;
        private bool _rewinding;
        private float _scanT;
        
        //If we dont move for this period we disable the CharacterController
        //this is to make the movement stable when on platforms
        private float _controllerEnableTimer;

        private void Update() {
            var keyFwd = Input.GetKey(KeyCode.W);
            
            var keyLeft = Input.GetKey(KeyCode.A);
            var keyRight = Input.GetKey(KeyCode.D);

            var keyScanningToggle = Input.GetKeyDown(KeyCode.E);
            var keyQ = Input.GetKey(KeyCode.Q);

            if (keyScanningToggle) {
                _scanningEnabled = !_scanningEnabled;
                
                //If entering scan mode set all platforms to scanning
                //so they stop moving and recording move data
                foreach (var platform in platforms) {
                    platform.changeState(RecallPlatform.PlatformState.Scanning);
                }
            }

            scanningUI.SetScanningState(_scanningEnabled, highlightedPlatform, _rewinding);

            if (keyFwd) {
                _controller.enabled = true;
                _controllerEnableTimer = 0.5f;

                playerSpeed += Time.deltaTime * 3.0f;
                playerSpeed = Mathf.Min(playerSpeed, 1.0f);
            }
            else {
                playerSpeed -= Time.deltaTime * 2.0f;
            }

            if (_scanningEnabled) {
                //scan for platforms
                RecallPlatform closestPlatform = null;
                float closestDP = -1.0f;
                
                foreach (var platform in platforms) {
                    //check if the platform is in front of the player
                    var playerPos = _transform.position;
                    var platformPos = platform.transform.position;
                    var platformOffset = platformPos - playerPos;
                    platformOffset.y = 0.0f;
                    var platformOffsetN = platformOffset.normalized;
                    float dp = Vector3.Dot(platformOffsetN, _transform.forward);
                    if (dp > 0.8f && dp > closestDP) {
                        closestPlatform = platform;
                        closestDP = dp;
                    }
                }

                SetHightlightedPlatform(closestPlatform);
            }
            else {
                SetHightlightedPlatform(null);
            }

            if (keyQ && highlightedPlatform != null) {
                if (!_rewinding) {
                    _rewinding = true;
                    highlightedPlatform.startRewinding();
                }
            }
            else {
                if (_rewinding) {
                    if (highlightedPlatform != null)
                        highlightedPlatform.stopRewinding();
                }

                foreach (var platform in platforms) {
                    if (_scanningEnabled) platform.changeState( RecallPlatform.PlatformState.Scanning );
                    else platform.changeState( RecallPlatform.PlatformState.Recording );
                }
                
                _rewinding = false;
            }

            playerSpeed = Mathf.Clamp01(playerSpeed);

            _animator.SetFloat(Blend, playerSpeed);

            if (keyLeft) _transform.Rotate(Vector3.up, -100.0f * Time.deltaTime);
            if (keyRight) _transform.Rotate(Vector3.up, 100.0f * Time.deltaTime);
            
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 2.0f, -Vector3.up, 4.0f);
            _controllerEnableTimer -= Time.deltaTime;
            foreach (var hit in hits) {
                if (_controller.isGrounded) {
                    //You should use tags/layers for this. I'm trying not to do that since this will be imported into 
                    //another project
                    if (hit.collider.gameObject.name.Contains("platform")) {
                        _platformObject = hit.collider.gameObject;
                        this._transform.SetParent(_platformObject.transform, true);
                        Camera.main.transform.SetParent(_platformObject.transform, true);
                        if (_controllerEnableTimer < 0.0f)
                            _controller.enabled = false;
                    }
                }
            }

            if (!_controller.isGrounded && _platformObject != null) {
                _platformObject = null;
                this._transform.SetParent(null, true);
                Camera.main.transform.SetParent(null, true);
            }

            _scanT += Time.deltaTime;
            float scanPulseT = Mathf.PingPong(_scanT, 1.0f);

            Color scanColor = Color.Lerp(Color.blue, Color.cyan, scanPulseT);
            scanningMaterial.color = scanColor;
        }

        private void SetHightlightedPlatform(RecallPlatform platform) {
            if (highlightedPlatform != platform) {
                if (highlightedPlatform != null) {
                    //If we are scanning go back to scan mode
                    //otherwise go to recording
                    if (_scanningEnabled)
                        highlightedPlatform.changeState( RecallPlatform.PlatformState.Scanning );
                    else
                        highlightedPlatform.changeState( RecallPlatform.PlatformState.Recording );
                    
                    //clear the material changes
                    var meshRenderers = highlightedPlatform.GetComponentsInChildren<MeshRenderer>();
                    foreach (var meshRenderer in meshRenderers) {
                        var materials = meshRenderer.materials;
                        materials[0] = platformMaterials[0];
                        materials[1] = platformMaterials[1];
                        materials[2] = platformMaterials[2];
                        meshRenderer.materials = materials;
                    }
                }
            }
            highlightedPlatform = platform;

            if (highlightedPlatform != null) {
                var meshRenderers = highlightedPlatform.GetComponentsInChildren<MeshRenderer>();
                foreach (var meshRenderer in meshRenderers) {
                    var materials = meshRenderer.materials;
                    materials[0] = scanningMaterial;
                    materials[1] = scanningMaterial;
                    materials[2] = scanningMaterial;
                    meshRenderer.materials = materials;
                }
            }
        }

        private void OnAnimatorMove() {
            //Called from Unity root motion system
            var animMove = _animator.deltaPosition;
            animMove.y = 0.0f;//ignore vertical motion from the animation

            Vector3 platformMove = Vector3.zero;
            if (_platformObject != null) {
                RecallPlatform platform = _platformObject.GetComponent<RecallPlatform>();
                platformMove = platform.move;
            }

            //move forward
            if (moveState == MoveState.Moving) {
                var moveDirection = _transform.forward;
                moveDirection.y = 0.0f;

                var actualMove = moveDirection.normalized * animMove.magnitude + platformMove;

                var gravity = Vector3.up * 5.0f;
                bool groundedPlayer = _controller.isGrounded;

                var moveAmount = actualMove - gravity * Time.deltaTime;
                
                //Jumping
                if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer) {
                    _animator.SetTrigger(Jump);
                    jumpVelocity = actualMove + Vector3.up*0.3f;
                    moveState = MoveState.Jumping;
                    _controller.enabled = true;
                    _controllerEnableTimer = 0.5f;
                }
                
                _controller.Move(moveAmount);
            }
            else {
                var gravity = Vector3.up * 1.0f;
                jumpVelocity -= gravity * Time.deltaTime;
                _controller.Move(jumpVelocity);
                if (_controller.isGrounded) {
                    moveState = MoveState.Moving;
                    _animator.SetTrigger(Land);
                }
            }
        }
        
        public void Step(int i) {
            if (_controller.isGrounded) {
                float vol = 0.3f;//running
                if (i == 1) vol = 0.2f;//slower
                if (i == 0) vol = 0.1f;//walk

                var audioSrc = GetComponent<AudioSource>();
                audioSrc.PlayOneShot(footStepSFX, vol);
            }
        }
    }
}