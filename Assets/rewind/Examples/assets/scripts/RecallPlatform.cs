using aeric.rewind_plugin;
using UnityEngine;

//TODO: namespace


public class RecallPlatform : RewindCustomMonoBehaviourAttributes, IRewindDataHandler {
    public Transform startPt;
    public Transform endPt;

    public float speed;

    [HideInInspector]
    public Vector3 move;

    [Rewind] private float moveT;

    public AnimationCurve moveCurve;

    private Transform _transform;
    
    //TODO: encapsulate this stuff
    private RewindScene _rewindScene;
    private RewindStorage _rewindStorage;
    private RewindPlayback _playback;
    private RewindRecorder _recorder;
    public RewindPlaybackPreparer playbackPreparer;
    private float newPlaybackTime = -1.0f;

    Vector3[] rewindPath = new Vector3[100];
    private int pathIndex;
    private int pathLength;

    public LineRenderer lineRenderer;
    public TrailRenderer trailRenderer;
    
    private new void Awake() {
        _transform = transform;
        base.Awake();
    }
    
    private void Start() {
        _rewindScene = new RewindScene();
        _rewindScene.addRewindObject(this);

        _rewindStorage = new RewindStorage(_rewindScene, 100, true);
        _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 10, true);
        _playback = new RewindPlayback(_rewindScene, _rewindStorage);
        _recorder.startRecording();
        lineRenderer.gameObject.SetActive(false);
        trailRenderer.gameObject.SetActive(false);
    }

    enum PlatformState {
        None,
        Recording,
        Rewinding
    }

    private PlatformState _platformState = PlatformState.Recording;
    
    private void changeState(PlatformState newState) {
        switch (newState) {
        case PlatformState.Recording: {
            _playback.stopPlayback();
            playbackPreparer.stopPlayback();

            _recorder.startRecording();
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
    
    private void Update() {
        while (moveT >= 2.0f) moveT -= 2.0f;

        float lerpT = moveCurve.Evaluate(moveT);
        
        //ping pong between the two points
        Vector3 newPos = Vector3.Lerp(startPt.position, endPt.position, lerpT);

        move = newPos - _transform.position;
        _transform.position = newPos;

        if (_platformState == PlatformState.Recording) {
            moveT += Time.deltaTime * speed;
            _recorder.updateRecording();
            _recorder.advanceRecordingTime();
            if (Input.GetKey(KeyCode.E)) {
                changeState(PlatformState.Rewinding);
            }
        }
        else {
            var currentTime = _playback.currentTime;
            var startTime = _playback.startTime;

            newPlaybackTime = currentTime - Time.deltaTime * 1.0f;
            if (newPlaybackTime < startTime) newPlaybackTime = startTime;

            {
                _playback.SetPlaybackTime(newPlaybackTime);
                _playback.restoreFrameAtCurrentTime();
            }
            
            //Get all the points in the platforms rewind path
            {
                pathIndex = 0;
                var frameInfo = _rewindStorage.findPlaybackFrames(newPlaybackTime);

                int startPathFrame = 0;
                int endPathFrame = frameInfo.frameUnmappedB;
                
                for (int i = startPathFrame; i <= endPathFrame; i++) {
                    _rewindStorage.getUnmappedFrameData(i, this, this);
                    pathIndex++;
                }

                pathLength = endPathFrame+1;

                lineRenderer.positionCount = pathLength;
                lineRenderer.SetPositions(rewindPath);
             //   trailRenderer.positionCount = pathLength;
                trailRenderer.SetPositions(rewindPath);
            }

            var endTime = _playback.endTime;
            var fillTime = currentTime - startTime;
        //    rewindBar.fillAmount = fillTime / 5.0f;

            if (!Input.GetKey(KeyCode.E) || fillTime < 0.1f) {
                var frameInfo = _rewindStorage.findPlaybackFrames(newPlaybackTime);

                var currentFrameCount = _rewindStorage.RecordedFrameCount;
                var newUnmappedEndFrame = frameInfo.frameUnmappedB;

                _rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);

                changeState(PlatformState.Recording);

                _recorder.setRecordTime(newPlaybackTime);
            }
        }

    }

    public void RewindHandlerData(IRewindHandler rewindHandler, NativeByteArrayReader nativeByteArrayReader) {
        if (ReferenceEquals(rewindHandler, this)) {
            float moveT = nativeByteArrayReader.readFloat();
            float lerpT = moveCurve.Evaluate(moveT);
        
            //ping pong between the two points
            Vector3 newPos = Vector3.Lerp(startPt.position, endPt.position, lerpT);
            rewindPath[pathIndex] = newPos;
        }
    }
}
