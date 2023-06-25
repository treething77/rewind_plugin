using System;
using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {

    public class RecallPlatform : RewindCustomMonoBehaviourAttributes, IRewindDataHandler {
        public Transform startPt;
        public Transform endPt;

        public float speed;

        [HideInInspector] public Vector3 move;

        //Value that moves from 0-2 and wraps, controls all movement of the platform
        //By rewinding this value we rewind the motion of the platform
        [Rewind] private float moveT;

        public AnimationCurve moveCurve;
        public LineRenderer lineRenderer;
        public TrailRenderer trailRenderer;

        private Transform _transform;

        //TODO: encapsulate this stuff
        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;
        private RewindPlayback _playback;
        private RewindRecorder _recorder;
        private RewindPlaybackPreparer playbackPreparer;
        private float newPlaybackTime = -1.0f;

        Vector3[] rewindPath = new Vector3[101];
        private int pathIndex;
        private int pathLength;


        private new void Awake() {
            _transform = transform;
            base.Awake();
        }

        private void OnDestroy() {
            _rewindStorage.Dispose();
        }

        private void Start() {
            playbackPreparer = GetComponent<RewindPlaybackPreparer>();
            _rewindScene = new RewindScene();
            _rewindScene.addRewindHandler(this);

            _rewindStorage = new RewindStorage(_rewindScene, 100);
            _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 10, true);
            _playback = new RewindPlayback(_rewindScene, _rewindStorage);
            _recorder.startRecording();
            lineRenderer.gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }

        public enum PlatformState {
            None,
            Recording,
            Scanning,
            Rewinding
        }

        private PlatformState _platformState = PlatformState.Recording;

        public void changeState(PlatformState newState) {
            switch (newState) {
            case PlatformState.Recording: {
                _playback.stopPlayback();
                playbackPreparer.stopPlayback();

                _recorder.startRecording();
                lineRenderer.gameObject.SetActive(false);
                trailRenderer.gameObject.SetActive(false);
                break;
            }
            case PlatformState.Scanning: {
                lineRenderer.gameObject.SetActive(false);
                trailRenderer.gameObject.SetActive(false);
                break;
            }
            case PlatformState.Rewinding: {
                var endTime = _playback.endTime;

                playbackPreparer.startPlayback();
                _playback.startPlayback();

                //start at the end
                _playback.SetPlaybackTime(endTime);
                newPlaybackTime = endTime;
                lineRenderer.gameObject.SetActive(true);
                trailRenderer.gameObject.SetActive(true);

                break;
            }
            }

            _platformState = newState;
        }

        public void startRewinding() {
            changeState(PlatformState.Rewinding);
        }

        public void stopRewinding() {
            var frameInfo = _rewindStorage.findPlaybackFrames(newPlaybackTime);

            var currentFrameCount = _rewindStorage.RecordedFrameCount;
            var newUnmappedEndFrame = frameInfo.frameUnmappedB;

            _rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);

            changeState(PlatformState.Recording);

            _recorder.setRecordTime(newPlaybackTime);
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

            if (_platformState == PlatformState.Recording) {
                moveT += Time.deltaTime * speed;
                _recorder.updateRecording();
                _recorder.advanceRecordingTime();
            }
            else if (_platformState == PlatformState.Rewinding) {
                var currentTime = _playback.currentTime;
                var startTime = _playback.startTime;

                newPlaybackTime = currentTime - Time.deltaTime * 1.0f;
                if (newPlaybackTime < startTime) newPlaybackTime = startTime;

                _playback.SetPlaybackTime(newPlaybackTime);
                _playback.restoreFrameAtCurrentTime();

                //Get all the points in the platforms rewind path
                //TODO: encapsulate this
                {
                    pathIndex = 0;
                    var frameInfo = _rewindStorage.findPlaybackFrames(newPlaybackTime);

                    int startPathFrame = 0;
                    int endPathFrame = frameInfo.frameUnmappedB;

                    for (int i = startPathFrame; i <= endPathFrame; i++) {
                        _rewindStorage.getUnmappedFrameData(i, this, this);
                        pathIndex++;
                    }

                    rewindPath[pathIndex - 1] = newPos;

                    pathLength = endPathFrame + 1;

                    lineRenderer.positionCount = pathLength;
                    lineRenderer.SetPositions(rewindPath);
                    trailRenderer.SetPositions(rewindPath);
                }

                var fillTime = currentTime - startTime;

                if (fillTime < 0.1f) {
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
            }
        }
    }
}
