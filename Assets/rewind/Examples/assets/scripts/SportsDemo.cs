using aeric.rewind_plugin;
using UnityEngine;
using UnityEngine.UI;

namespace aeric.rewind_plugin_demos {
    public struct CaptureEvent {
        public CaptureEvent(int moveTargetIndex, Robot robot, float capturetime) {
            _moveTargetIndex = moveTargetIndex;
            _robot = robot;
            _capturetime = capturetime;
        }
        
        private readonly int _moveTargetIndex;
        private readonly Robot _robot;
        private readonly float _capturetime;

        public int MoveTargetIndex => _moveTargetIndex;
        public Robot Robot => _robot;

        public float CaptureTime => _capturetime;
    }
    
    public class SportsDemo : MonoBehaviour {
        public static SportsDemo _instance;

        
        //inspector references
        public Text statusText;
        public GameObject stackParent;
        public GameObject targetsParent;
        public RewindPlaybackPreparer playbackPreparer;

        public RectTransform livePanel;
        public RectTransform replayPanel;

        // public GameObject replayUI;
        public RectTransform uiRobotMarker;
        public RectTransform canvasRectTransform;
        
        //Cameras
        public GameObject captureCamera;
        public GameObject liveCamera;
        public GameObject replayCamera;
        
        
        public RobotLevel level;

        private RewindPlayback _playback;
        public RewindRecorder _recorder;
        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;

        private RewindEventStream<CaptureEvent> _captureEventStream = new RewindEventStream<CaptureEvent>(100);

        private bool playback;
  
        private void Awake() {
            _instance = this;
        }
        
        private void Start() {
            _rewindScene = new RewindScene();
            _rewindScene.addAllChildren(stackParent);
            _rewindScene.addAllChildren(targetsParent);

            _rewindStorage = new RewindStorage(_rewindScene, 150, false);
            _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 30, true);
            _playback = new RewindPlayback(_rewindScene, _rewindStorage);

            _recorder.startRecording();
        }

        private float lastReplayCameraChangeTime;
        private Robot robotToTrack;

        private void Update() {
            livePanel.gameObject.SetActive(!playback);
            replayPanel.gameObject.SetActive(playback);

            liveCamera.SetActive(!playback);
            replayCamera.SetActive(playback);

            if (playback) {

                float t1 = _playback.currentTime;
                _playback.AdvancePlaybackTime();
                float t2 = _playback.currentTime;
                
                //look ahead
                t1 += 1.0f;
                t2 += 1.0f;

                var eventsInRange = _captureEventStream.findEventsInRange(t1, t2);
                if (eventsInRange.eventIndexStart != -1) {
                    for (int i = eventsInRange.eventIndexStart; i < eventsInRange.eventIndexEnd; i++) {
                        var captureEvent = _captureEventStream.getEvent(i);
                        Debug.Log("Replay Capture event! " + captureEvent.CaptureTime + " by " + captureEvent.Robot.name + " at time " + t1);
                        
                        Debug.Assert(captureEvent.CaptureTime >= t1);
                        Debug.Assert(captureEvent.CaptureTime < t2);

                        if (t1 - lastReplayCameraChangeTime > 1.5f) {
                            ReplayShowCaptureEvent(captureEvent);
                            lastReplayCameraChangeTime = t1;
                        }
                    }
                }

                if (robotToTrack != null) {
                    uiRobotMarker.gameObject.SetActive(true);

                    Vector3 robotScreenPt = Camera.main.WorldToScreenPoint(robotToTrack.transform.position);
                    robotScreenPt.z = 0.0f;
                    
                    // Convert the screen position to canvas local position
                    Vector2 localPosition;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, robotScreenPt, null, out localPosition);

                   
                    uiRobotMarker.anchoredPosition = localPosition;
                }
                else {
                    uiRobotMarker.gameObject.SetActive(false);
                }
                
                _playback.restoreFrameAtCurrentTime();

                if (_playback.isPlaybackComplete) {
                    playback = false;
                    statusText.text = "Record";
                    
                    _captureEventStream.ClearEvents();

                    _recorder.startRecording();
                    _playback.stopPlayback();
                    playbackPreparer.stopPlayback();
                    
                    uiRobotMarker.gameObject.SetActive(false);
                    lastReplayCameraChangeTime = 0.0f;
                    captureCamera.SetActive(false);
                    robotToTrack = null;
                }
            }
            else {
                _recorder.updateRecording();
                _recorder.advanceRecordingTime();
                statusText.text = $"Record - {_rewindStorage.RecordedFrameCount} - {_rewindStorage.FrameWriteIndex}";
            }
        }

        private void ReplayShowCaptureEvent(CaptureEvent captureEvent) {
            var robotPos = captureEvent.Robot.transform.position;
            robotPos.y = 0.0f;

            var capturePos =  level.GetTargetPosition(captureEvent.MoveTargetIndex);
            capturePos.y = 0.0f;

            var captureDir = (robotPos - capturePos).normalized;
            var rot = Quaternion.AngleAxis(120.0f, Vector3.up);
            var cameraDir = rot * captureDir;

            captureCamera.SetActive(true);
            captureCamera.transform.position = capturePos + (cameraDir * 4.0f) + Vector3.up;
            captureCamera.transform.LookAt(capturePos);
            

            robotToTrack = captureEvent.Robot;
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(Screen.width - 200.0f, 0.0f, 200.0f, Screen.height));
            if (GUILayout.Button("Start Replay")) {
                statusText.text = "Replay";
                playback = true;
                playbackPreparer.startPlayback();
                _playback.startPlayback();
            }

            GUILayout.EndArea();
        }

        public void AddCaptureEvent(CaptureEvent capEvent) {
            if (!playback) {
                Debug.Log("Capture event! " + capEvent.MoveTargetIndex + " by " + capEvent.Robot.name + " at time " + _recorder.RecordingTime);

                _captureEventStream.addEvent(capEvent, _recorder.RecordingTime);
            }
        }
    }
}