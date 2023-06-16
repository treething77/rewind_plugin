using UnityEngine;

namespace aeric.rewind_plugin {
    public class RewindParticleSystem : RewindComponentBase {
        private ParticleSystem _particles;

        public override int RequiredBufferSizeBytes => 4*4 + 4;
        public override uint HandlerTypeID => 6;

        private void Awake() {
            TryGetComponent(out _particles);
        }

        private float _simulationTime = 0.0f;
        
        //Starting state
        private float _startTime;
        private bool _startIsPlaying;
        private bool _startEmitting;

        private bool _prevPlaying = false;
        
        public override void startRecording() {
            if (_particles.useAutoRandomSeed) {
                Debug.LogWarning("dont do this");
            }
            
            //Dont use "automatic random seed" 
            //set the duration of the system to match the lifetime of its particles
            //this way we can rewind the system to any point in time
            _startTime = Time.time;
            
            _startIsPlaying = _particles.isPlaying;
            _startEmitting = _particles.isEmitting;

            _prevPlaying = _startIsPlaying;
        }
        
        public override void rewindStore(NativeByteArrayWriter writer) {
            //ParticleSystem.totalTime only exists in >= 2022

            bool systemIsAlive = _particles.IsAlive(true);
            bool systemIsPlaying = _particles.isPlaying;

            if (systemIsPlaying && !_prevPlaying) {
                // started playing so reset simulation time
                _simulationTime = _particles.time;
                _startTime = Time.time;
            }
            
            //This is not necessarily called every frame so we can't use Time.deltaTime to accumulate time
            if (_particles.time > _simulationTime) {
                _simulationTime = _particles.time;
                _startTime = Time.time;
            }
            else {
                _simulationTime += Time.time - _startTime;
                _startTime = Time.time;
            }

            writer.writeBool(systemIsPlaying);
            writer.writeBool(_prevPlaying);
            writer.writeBool(_particles.isEmitting);
            writer.writeBool(systemIsAlive);
            
            Debug.Log("t : " + _simulationTime);
            
            //the particles.time is not enough to fully re-simulate the particle system because it is clamped
            //at the particles duration
            
            writer.writeFloat(_simulationTime);

            _prevPlaying = systemIsPlaying;
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            /////////////////////////////
            //TODO: if we dont use any of this data then dont store it
            bool systemIsPlayingA = frameReaderA.readBool();
            bool systemIsPlayingB = frameReaderB.readBool();
            bool prevIsPlayingA = frameReaderA.readBool();
            bool prevIsPlayingB = frameReaderB.readBool();
            bool systemIsEmittingA = frameReaderA.readBool();
            bool systemIsEmittingB = frameReaderB.readBool();
            /////////////////////////////
            
            bool systemIsAliveA = frameReaderA.readBool();
            bool systemIsAliveB = frameReaderB.readBool();
            
            float t1 = frameReaderA.readFloat();
            float t2 = frameReaderB.readFloat();
          
            float simulationTime = Mathf.Lerp(t1, t2, frameT);
            Debug.Log("r : " + simulationTime);
            if (systemIsAliveA)
                _particles.Simulate(simulationTime, true, true);
            if (!systemIsAliveA && !systemIsAliveB)
                _particles.Stop( true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}