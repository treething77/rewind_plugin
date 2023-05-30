using System;
using System.Collections;
using ccl.rewind_plugin;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Animations;

namespace ccl.rewind_plugin_demos
{
    public class SimpleTransformTest : MonoBehaviour
    {
        private const float TestTime = 3.0f;
        private const float SpinSpeed = 2.0f;
        private const float ScaleSpeed = 1.0f;

        public RewindTransform transformTest;

        public RewindRecorderComponent recorderComponent;
        public RewindPlaybackComponent playbackComponent;
        
        private Transform _transform;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;

        private void Awake()
        {
            _transform = transform;
            transformTest = GetComponent<RewindTransform>();
            recorderComponent = GetComponent<RewindRecorderComponent>();
            playbackComponent = GetComponent<RewindPlaybackComponent>();
        }

        void Start()
        {
            rewindScene = new RewindScene();
            rewindScene.addRewindObject(transformTest);
            
            rewindStorage = new RewindStorage(rewindScene);

            StartCoroutine(SimpleTestCoroutine());
        }
        
        private IEnumerator SimpleTestCoroutine()
        {
            //start recording (create recorder component, add to object)
            recorderComponent.startRecording(rewindScene, rewindStorage);
            
            //spin and move for 3 seconds
            float timer = 0.0f;

            while (timer < TestTime)
            {
                timer += Time.deltaTime;

                float scale = Mathf.Abs(Mathf.Sin(timer * SpinSpeed)) + 0.5f;

                _transform.localScale = new Vector3(scale, scale, scale);

                float rot = Mathf.Cos(timer * SpinSpeed);
                
                _transform.localRotation = Quaternion.Euler(0.0f, rot * 100.0f, 0.0f);

                yield return null;
            }

            //stop recording
            recorderComponent.stopRecording();

            //start playback
            playbackComponent.startPlayback();
        }
    }
}
