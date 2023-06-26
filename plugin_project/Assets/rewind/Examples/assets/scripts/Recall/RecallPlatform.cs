using System;
using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos {

    public class RecallObject {
        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;
        private RewindPlayback _playback;
        private RewindRecorder _recorder;
        private float newPlaybackTime = -1.0f;
        
        private IRewindHandler _rewindHandler;
        private IRewindDataHandler _dataHandler;

        public void Initialize(IRewindHandler handler, IRewindDataHandler dataHandler) {
            _rewindScene = new RewindScene();
            _rewindScene.addRewindHandler(handler);
            _rewindHandler = handler;
            _dataHandler = dataHandler;

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

        public void StartRecording() {
            _playback.stopPlayback();
            _recorder.startRecording();
        }

        public void StopRewind() {
            //When we are done rewinding we call into the storage to reset the write state to that point
            //so we can move forwards from there
            var frameInfo = _rewindStorage.findPlaybackFrames(newPlaybackTime);

            var currentFrameCount = _rewindStorage.RecordedFrameCount;
            var newUnmappedEndFrame = frameInfo.frameUnmappedB;

            _rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);
            _recorder.setRecordTime(newPlaybackTime);
        }

        public float GetRecallTimeLeft() {
            var currentTime = _playback.currentTime;
            var startTime = _playback.startTime;
            return currentTime - startTime;
        }

        public void RewindByTime(float deltaTime) {
            var currentTime = _playback.currentTime;
            var startTime = _playback.startTime;

            newPlaybackTime = currentTime - deltaTime;
            if (newPlaybackTime < startTime) newPlaybackTime = startTime;

            _playback.SetPlaybackTime(newPlaybackTime);
            _playback.restoreFrameAtCurrentTime();

            //Get all the points in the platforms rewind path
            var frameInfo = _rewindStorage.findPlaybackFrames(newPlaybackTime);

            int startPathFrame = 0;
            int endPathFrame = frameInfo.frameUnmappedB;

            for (int i = startPathFrame; i <= endPathFrame; i++) {
                _rewindStorage.getUnmappedFrameData(i, _rewindHandler, _dataHandler);
            }
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
            
            _recall.Initialize(this, this);

            lineRenderer.gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }

        public enum PlatformState {
            Recording,//moving forwards in time, recording data
            Scanning,//object is paused, not recording or playing back
            Rewinding//moving backwards in time, playing back data
        }

        private PlatformState _platformState = PlatformState.Recording;

        public void changeState(PlatformState newState) {
            switch (newState) {
            case PlatformState.Recording: {
                _playbackPreparer.stopPlayback();
                _recall.StartRecording();
                break;
            }
            case PlatformState.Scanning: {
                break;
            }
            case PlatformState.Rewinding: {
                _playbackPreparer.startPlayback();
                _recall.StartPlayback();
                break;
            }
            }

            lineRenderer.gameObject.SetActive(newState == PlatformState.Rewinding);
            trailRenderer.gameObject.SetActive(newState == PlatformState.Rewinding);

            _platformState = newState;
        }

        public void startRewinding() {
            changeState(PlatformState.Rewinding);
        }

        public void stopRewinding() {
            _recall.StopRewind();
            changeState(PlatformState.Recording);
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
