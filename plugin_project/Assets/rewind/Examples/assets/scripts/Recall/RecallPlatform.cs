using System;
using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {
    public class RecallPlatform : RewindCustomMonoBehaviourAttributes, IRewindDataHandler {
        public Transform startPt;
        public Transform endPt;

        public float speed;

        //This value is used to control movement of the character when standing on a moving platform
        [HideInInspector] public Vector3 move;

        //Value that moves from 0-2 and wraps, controls all movement of the platform
        //By rewinding this value we rewind the motion of the platform
        [Rewind] private float moveT;

        //We feed the linear moveT into an animation curve to get a smoother motion
        public AnimationCurve moveCurve;
        public LineRenderer lineRenderer;
        public TrailRenderer trailRenderer;

        private Transform _transform;

        private RecallObject _recall = new();
        private RewindPlaybackPreparer _playbackPreparer;

        Vector3[] rewindPath = new Vector3[101];
        private int pathIndex;
        private int pathLength;


        private new void Awake() {
            _transform = transform;
            base.Awake();
        }

        private void OnDestroy() {
            _recall.Dispose();
        }

        private void Start() {
            _playbackPreparer = GetComponent<RewindPlaybackPreparer>();
            
            _recall.Initialize(this, this);

            lineRenderer.gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }



        public void changeState(RecallObject.RecallObjectState newState) {
            switch (newState) {
            case RecallObject.RecallObjectState.Recording: {
                _playbackPreparer.stopPlayback();
                _recall.StartRecording();
                break;
            }
            case RecallObject.RecallObjectState.Paused: {
                break;
            }
            case RecallObject.RecallObjectState.Rewinding: {
                _playbackPreparer.startPlayback();
                _recall.StartPlayback();
                break;
            }
            }

            lineRenderer.gameObject.SetActive(newState == RecallObject.RecallObjectState.Rewinding);
            trailRenderer.gameObject.SetActive(newState == RecallObject.RecallObjectState.Rewinding);

            _recall.RecallState = newState;
        }

        public void startRewinding() {
            changeState(RecallObject.RecallObjectState.Rewinding);
        }

        public void stopRewinding() {
            _recall.StopRewind();
            changeState(RecallObject.RecallObjectState.Recording);
        }

        private void Update() {
            while (moveT >= 2.0f) moveT -= 2.0f;

            //We feed the moveT linear value into a curve to get better movement
            float lerpT = moveCurve.Evaluate(moveT);

            //ping pong between the two points
            Vector3 newPos = Vector3.Lerp(startPt.position, endPt.position, lerpT);

            //Store the amount we are moving this frame. The character uses this to stay attached to the platform
            move = newPos - _transform.position;
            
            _transform.position = newPos;

            if (_recall.RecallState == RecallObject.RecallObjectState.Recording) {
                moveT += Time.deltaTime * speed;
                _recall.UpdateRecording();
            }
            else if (_recall.RecallState == RecallObject.RecallObjectState.Rewinding) {
                pathIndex = 0;
                 
                _recall.RewindByTime(Time.deltaTime);

                //Update the path rendering
                rewindPath[pathIndex - 1] = newPos;
                pathLength = pathIndex + 1;
                lineRenderer.positionCount = pathLength;
                lineRenderer.SetPositions(rewindPath);
                trailRenderer.SetPositions(rewindPath);
                
                if (_recall.GetRecallTimeLeft() < 0.1f) {
                    stopRewinding();
                }
            }
        }

        public void RewindHandlerData(IRewindHandler rewindHandler, NativeByteArrayReader nativeByteArrayReader) {
            if (ReferenceEquals(rewindHandler, this)) {
                float t = nativeByteArrayReader.readFloat();
                float lerpT = moveCurve.Evaluate(t);

                //ping pong between the two points
                Vector3 newPos = Vector3.Lerp(startPt.position, endPt.position, lerpT);
                rewindPath[pathIndex] = newPos;
                pathIndex++;
            }
        }
    }
}
