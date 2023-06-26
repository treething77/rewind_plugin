using System;
using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {

    public class RecallObject {
        //TODO: encapsulate this stuff
        public RewindScene _rewindScene;
        public RewindStorage _rewindStorage;
        public RewindPlayback _playback;
        public RewindRecorder _recorder;
        public float newPlaybackTime = -1.0f;
        
        private IRewindHandler _rewindHandler;
 
        public void Initialize(IRewindHandler handler) {
            _rewindScene = new RewindScene();
            _rewindScene.addRewindHandler(handler);
            _rewindHandler = handler;

            _rewindStorage = new RewindStorage(_rewindScene, 100);
            _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 10, true);
            _playback = new RewindPlayback(_rewindScene, _rewindStorage);
            _recorder.startRecording();
        }

        public void UpdateRecording() {
            _recorder.updateRecording();
            _recorder.advanceRecordingTime();
        }

        public void Dispose() {
            _rewindStorage.Dispose();
        }

        public void StartPlayback() {
            var endTime = _playback.endTime;
            _playback.startPlayback();
            //start at the end
            _playback.SetPlaybackTime(endTime);
            newPlaybackTime = endTime;
        }
    }

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
            
            _recall.Initialize(this);

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
                _playbackPreparer.stopPlayback();
                {
                    _recall._playback.stopPlayback();
                   _recall._recorder.startRecording();
                }
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
                _playbackPreparer.startPlayback();

                _recall.StartPlayback();

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
            //When we are done rewinding we call into the storage to reset the write state to that point
            //so we can move forwards from there
            var frameInfo = _recall._rewindStorage.findPlaybackFrames(_recall.newPlaybackTime);

            var currentFrameCount = _recall._rewindStorage.RecordedFrameCount;
            var newUnmappedEndFrame = frameInfo.frameUnmappedB;

            _recall._rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);

            changeState(PlatformState.Recording);

            _recall._recorder.setRecordTime(_recall.newPlaybackTime);
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
                _recall.UpdateRecording();
            }
            else if (_platformState == PlatformState.Rewinding) {
                var currentTime = _recall._playback.currentTime;
                var startTime = _recall._playback.startTime;

                _recall.newPlaybackTime = currentTime - Time.deltaTime * 1.0f;
                if (_recall.newPlaybackTime < startTime) _recall.newPlaybackTime = startTime;

                _recall._playback.SetPlaybackTime(_recall.newPlaybackTime);
                _recall._playback.restoreFrameAtCurrentTime();

                //Get all the points in the platforms rewind path
                //TODO: encapsulate this
                {
                    pathIndex = 0;
                    var frameInfo = _recall._rewindStorage.findPlaybackFrames(_recall.newPlaybackTime);

                    int startPathFrame = 0;
                    int endPathFrame = frameInfo.frameUnmappedB;

                    for (int i = startPathFrame; i <= endPathFrame; i++) {
                        _recall._rewindStorage.getUnmappedFrameData(i, this, this);
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
