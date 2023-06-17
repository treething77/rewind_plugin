using System;
using aeric.rewind_plugin;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Bloom = UnityEngine.Rendering.Universal.Bloom;

namespace aeric.rewind_plugin_demos {
    public class SportsRewind : MonoBehaviour {
        //inspector references
        public GameObject stackParent;
        public GameObject targetParent;
        public RewindComponentBase robotCamRewind;
        public Image rewindBar;
        public RewindPlaybackPreparer playbackPreparer;

        public Volume m_Volume;
      // public VolumeComponent bloom;
        
        private RewindPlayback _playback;
        private RewindRecorder _recorder;
        private RewindScene _rewindScene;
        private RewindStorage _rewindStorage;
        
        enum DemoState {
            None,
            Recording,
            Rewinding
        }
        private DemoState demoState = DemoState.None;
        private float newPlaybackTime = -1.0f;
        private bool playback;

        private void Start() {
            _rewindScene = new RewindScene();
            _rewindScene.addAllChildren(stackParent);
            _rewindScene.addAllChildren(targetParent);
            _rewindScene.addRewindObject(robotCamRewind);

            _rewindStorage = new RewindStorage(_rewindScene, 150, false);

            _recorder = new RewindRecorder(_rewindScene, _rewindStorage, 30, true);
            _playback = new RewindPlayback(_rewindScene, _rewindStorage);

            _recorder.startRecording();
            changeState(DemoState.Recording);
        }

        private void OnDestroy() {
            _rewindStorage.Dispose();
        }

        private float ppRamp = 0.0f;
        private float rewindTime;

        private void Update() {
            switch (demoState) {
            case DemoState.Recording: {
                _recorder.updateRecording();
                _recorder.advanceRecordingTime();

                var startTime = _playback.startTime;
                var endTime = _playback.endTime;
                var currentTime = _playback.currentTime;
                var fillTime = endTime - startTime;

                if (fillTime <= 0.0f) fillTime = 0.0f;
                rewindBar.fillAmount = fillTime / 5.0f;

                if (Input.GetKey(KeyCode.Space)) changeState(DemoState.Rewinding);
                ppRamp -= Time.deltaTime * 3.0f;
                break;
            }
            case DemoState.Rewinding: {
                var currentTime = _playback.currentTime;
                var startTime = _playback.startTime;

                ppRamp += Time.deltaTime * 1.5f;
                rewindTime += Time.deltaTime;
                newPlaybackTime = currentTime - Time.deltaTime * 1.0f;
                if (newPlaybackTime < startTime) newPlaybackTime = startTime;

                {
                    _playback.SetPlaybackTime(newPlaybackTime);
                    _playback.restoreFrameAtCurrentTime();
                }

                var endTime = _playback.endTime;
                var fillTime = currentTime - startTime;
                rewindBar.fillAmount = fillTime / 5.0f;

                if (!Input.GetKey(KeyCode.Space) || fillTime < 0.1f) {
                    var frameInfo = _rewindStorage.findPlaybackFrames(newPlaybackTime);

                    var currentFrameCount = _rewindStorage.RecordedFrameCount;
                    var newUnmappedEndFrame = frameInfo.frameUnmappedB;

                    _rewindStorage.rewindFrames(currentFrameCount - newUnmappedEndFrame);

                    changeState(DemoState.Recording);

                    _recorder.setRecordTime(newPlaybackTime);
                }

                break;
            }
            }

            ppRamp = Mathf.Clamp01(ppRamp);

            m_Volume.weight = ppRamp;
          //  for (int i = 0; i < 3; i++) {
              //  if (m_Volume.profile.components[0].name == "Bloom(Clone)") {
                    Bloom b = (Bloom)m_Volume.profile.components[0];
                    b.intensity.value = ((Mathf.Sin(rewindTime * 10.0f) + 1.0f) * 3.0f) * ppRamp + 1.0f;
           //     }
          //  }
        }

        private void changeState(DemoState newState) {
            switch (newState) {
            case DemoState.Recording: {
                _playback.stopPlayback();
                playbackPreparer.stopPlayback();

                _recorder.startRecording();
                break;
            }
            case DemoState.Rewinding: {
                var endTime = _playback.endTime;

                playbackPreparer.startPlayback();
                _playback.startPlayback();

                //start at the end
                _playback.SetPlaybackTime(endTime);
                newPlaybackTime = endTime;

                rewindTime = 0.0f;
                break;
            }
            }

            demoState = newState;
        }

    }
}