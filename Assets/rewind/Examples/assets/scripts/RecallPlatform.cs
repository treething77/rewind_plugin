using aeric.rewind_plugin;
using UnityEngine;

public class RecallPlatform : RewindCustomMonoBehaviourAttributes {
    public Transform startPt;
    public Transform endPt;

    public float speed;

    [HideInInspector]
    public Vector3 move;

    [Rewind] private float moveT;

    public AnimationCurve moveCurve;

    private Transform _transform;
    
    private RewindScene _rewindScene;
    private RewindStorage _rewindStorage;
    private RewindPlayback _playback;
    private RewindRecorder _recorder;
    public RewindPlaybackPreparer playbackPreparer;
    private float newPlaybackTime = -1.0f;

    private new void Awake() {
        _transform = transform;
        base.Awake();
    }
    
    private void Start() {
        _rewindScene = new RewindScene();
        _rewindScene.addRewindObject(this);

        _rewindStorage = new RewindStorage(_rewindScene, 100, true);
        _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 30, true);
        _playback = new RewindPlayback(_rewindScene, _rewindStorage);
        _recorder.startRecording();
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
            break;
        }
        case PlatformState.Rewinding: {
            var endTime = _playback.endTime;

            playbackPreparer.startPlayback();
            _playback.startPlayback();

            //start at the end
            _playback.SetPlaybackTime(endTime);
            newPlaybackTime = endTime;

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
}
