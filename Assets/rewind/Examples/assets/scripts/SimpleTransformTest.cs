using System.Collections;
using aeric.rewind_plugin;
using UnityEngine;

namespace aeric.rewind_plugin_demos
{
    public class SimpleTransformTest : MonoBehaviour
    {
        private const float TestTime = 3.0f;
        private const float SpinSpeed = 2.0f;
        private const float ScaleSpeed = 1.0f;
        private const float BounceSpeed = 4.0f;

        public TMPro.TMP_Text statusText;

        public RewindTransform transformTest;

        private RewindRecorderComponent recorderComponent;
        private RewindPlaybackComponent playbackComponent;
        
        private Transform _transform;

        private RewindScene rewindScene;

        private RewindStorage rewindStorage;

        private int recordFPS = 30;

        
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
            
            //3 seconds at max 30fps
            rewindStorage = new RewindStorage(rewindScene, recordFPS * 3, false);

            StartCoroutine(SimpleTestCoroutine());
        }
        
        private IEnumerator SimpleTestCoroutine()
        {
            statusText.text = "RECORDING";
            //start recording (create recorder component, add to object)
            recorderComponent.startRecording(rewindScene, rewindStorage, recordFPS);
            
            //spin and move for 3 seconds
            float timer = 0.0f;

            Vector3 startPos = _transform.position;

            while (timer < TestTime)
            {
                timer += Time.deltaTime;

                float scale = Mathf.Abs(Mathf.Sin(timer * SpinSpeed)) + 0.5f;

                _transform.localScale = new Vector3(scale, scale, scale);

                float rot = Mathf.Cos(timer * ScaleSpeed);
                
                _transform.localRotation = Quaternion.Euler(0.0f, rot * 100.0f, 0.0f);


                float posTimeScale = Mathf.Sin(timer * BounceSpeed);
                posTimeScale *= posTimeScale;
                Vector3 pos = Vector3.Lerp(startPos, startPos + Vector3.one, posTimeScale);
                _transform.position = pos;
                
                yield return null;
            }

            //stop recording
            recorderComponent.stopRecording();

            statusText.text = "PLAYBACK";
            
            //start playback
            playbackComponent.startPlayback(rewindScene, rewindStorage);
        }
    }
}
