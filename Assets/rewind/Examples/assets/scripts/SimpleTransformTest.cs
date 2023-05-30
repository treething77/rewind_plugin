using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace ccl.rewind_plugin_demos
{
    public class SimpleTransformTest : MonoBehaviour
    {
        private const float TestTime = 3.0f;
        private const float SpinSpeed = 2.0f;
        private const float ScaleSpeed = 1.0f;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        void Start()
        {
            StartCoroutine(SimpleTestCoroutine());
        }
        
        private IEnumerator SimpleTestCoroutine()
        {
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

            //then replay
            
        }
    }
}
