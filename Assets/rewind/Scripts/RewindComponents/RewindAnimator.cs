using UnityEngine;

namespace aeric.rewind_plugin {
    public class RewindAnimator : RewindComponentBase {
        private Animator _animator;

        private AnimationStoredState _animStateA;
        private AnimationStoredState _animStateB;
        private int animParamCount;
        private AnimatorControllerParameter[] animParams;
        private int animStateCount;

        public override RewindDataSchema makeDataSchema() {
            RewindDataSchema schema = new RewindDataSchema();

            schema.addInt().addInt();//state count, param count
            
            for (int i=0;i<animStateCount;i++)
                schema.addInt().addFloat();//hash, time

            for (var i = 0; i < animParamCount; i++) {
                var animParam = animParams[i];
         
                schema.addInt().addInt();//hash, type

                switch (animParam.type) {
                case AnimatorControllerParameterType.Bool:
                    schema.addBool();
                    break;
                case AnimatorControllerParameterType.Trigger:
                    schema.addBool();
                    break;
                case AnimatorControllerParameterType.Int:
                    schema.addInt();
                    break;
                case AnimatorControllerParameterType.Float:
                    schema.addFloat();
                    break;
                default:
                    Debug.LogError("Parameter Type unsupported: " + animParam.type);
                    break;
                }
            }
            return schema;
        }

        public override uint HandlerTypeID => 4;

        private void Awake() {
            _animator = GetComponent<Animator>();

            //Cache the AnimatorControllerParameter objects because the GetParameter calls
            //are actually VERY expensive
            animParams = new AnimatorControllerParameter[_animator.parameterCount];
            for (var i = 0; i < _animator.parameterCount; i++)
                // Get the observed animator parameter info
                animParams[i] = _animator.GetParameter(i);

            animStateCount = _animator.layerCount;
            animParamCount = _animator.parameterCount;

            _animStateA.stateInfos = new AnimationStateInfo[animStateCount];
            _animStateB.stateInfos = new AnimationStateInfo[animStateCount];
            _animStateA.parameterInfos = new AnimationParameterInfo[animParamCount];
            _animStateB.parameterInfos = new AnimationParameterInfo[animParamCount];
        }

        public override void rewindStore(NativeByteArrayWriter writer) {
            writer.writeInt(animStateCount);
            writer.writeInt(animParamCount);

            for (var i = 0; i < animStateCount; i++) {
                //hash, time
                var animState = _animator.GetCurrentAnimatorStateInfo(i);

                // Create the layer state info
                writer.writeInt(animState.fullPathHash);
                writer.writeFloat(animState.normalizedTime);
            }

            for (var i = 0; i < animParamCount; i++) {
                //hash, type, value - all 4 bytes
                var animParam = animParams[i];
                var paramHash = animParam.nameHash;
                var paramType = (int)animParam.type;

                writer.writeInt(paramHash);
                writer.writeInt(paramType);

                switch (animParam.type) {
                case AnimatorControllerParameterType.Bool:
                    writer.writeBool(_animator.GetBool(paramHash));
                    break;
                case AnimatorControllerParameterType.Trigger:
                    writer.writeBool(_animator.GetBool(paramHash));
                    break;
                case AnimatorControllerParameterType.Int:
                    writer.writeInt(_animator.GetInteger(paramHash));
                    break;
                case AnimatorControllerParameterType.Float:
                    writer.writeFloat(_animator.GetFloat(paramHash));
                    break;
                default:
                    Debug.LogError("Parameter Type unsupported: " + animParam.type);
                    break;
                }
            }
        }


        private bool validateAnimationStates() {
            if (_animStateA.stateCount != _animStateB.stateCount) {
                Debug.LogError("We cannot interpolate between two frames that have different animation state counts");
                return false;
            }

            if (_animStateA.parameterCount != _animStateB.parameterCount) {
                Debug.LogError("We cannot interpolate between two frames that have different parameter count");
                return false;
            }

            return true;
        }

        private static void ReadAnimationState(NativeByteArrayReader reader, ref AnimationStoredState animState) {
            animState.stateCount = reader.readInt();
            animState.parameterCount = reader.readInt();

            for (var i = 0; i < animState.stateCount; i++) {
                animState.stateInfos[i].stateHash = reader.readInt();
                animState.stateInfos[i].stateTime = reader.readFloat();
            }

            for (var i = 0; i < animState.parameterCount; i++) {
                animState.parameterInfos[i].hash = reader.readInt();
                animState.parameterInfos[i].type = (AnimatorControllerParameterType)reader.readInt();

                //interpolate the value
                switch (animState.parameterInfos[i].type) {
                case AnimatorControllerParameterType.Bool:
                    animState.parameterInfos[i].bValue = reader.readBool();
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animState.parameterInfos[i].bValue = reader.readBool();
                    break;
                case AnimatorControllerParameterType.Int:
                    animState.parameterInfos[i].iValue = reader.readInt();
                    break;
                case AnimatorControllerParameterType.Float:
                    animState.parameterInfos[i].fValue = reader.readFloat();
                    break;
                default:
                    Debug.LogError("Parameter Type unsupported: " + animState.parameterInfos[i].type);
                    break;
                }
            }
        }

        public override bool shouldDisableComponent(Component component) {
            //don't disable the Animator
            if (component is Animator) return false;
            return true;
        }

        public override void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT) {
            //First read both animation states in full and then interpolate and restore the state
            ReadAnimationState(frameReaderA, ref _animStateA);
            ReadAnimationState(frameReaderB, ref _animStateB);

            if (!validateAnimationStates()) return;

            for (var i = 0; i < _animStateA.stateCount; i++) {
                var stateA = _animStateA.stateInfos[i];
                var stateB = _animStateB.stateInfos[i];

                //If the state changed then don't interpolate the time
                if (stateA.stateHash == stateB.stateHash) {
                    var newTimeN = stateB.stateTime;
                    var lastTimeN = stateA.stateTime;
                    var newStateTime = Mathf.Lerp(lastTimeN, newTimeN, frameT);

                    _animator.Play(stateA.stateHash, i, newStateTime);
                }
                else {
                    _animator.Play(stateB.stateHash, i, stateB.stateTime);
                }
            }

            for (var i = 0; i < _animStateA.parameterCount; i++) {
                var paramA = _animStateA.parameterInfos[i];
                var paramB = _animStateA.parameterInfos[i];

                //interpolate the value
                switch (paramA.type) {
                case AnimatorControllerParameterType.Bool:
                    _animator.SetBool(paramA.hash, paramB.bValue);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    //we dont need to restore triggers? Framerate differences might make it difficult, and anyway
                    //if we restore anim state and time that should be enough?
                    break;
                case AnimatorControllerParameterType.Int:
                    var newIntValue = RewindUtilities.LerpInt(paramA.iValue, paramB.iValue, frameT);
                    _animator.SetFloat(paramA.hash, newIntValue);
                    break;
                case AnimatorControllerParameterType.Float:
                    var newFloatValue = Mathf.Lerp(paramA.fValue, paramB.fValue, frameT);
                    _animator.SetFloat(paramA.hash, newFloatValue);
                    break;
                default:
                    Debug.LogError("Animator parameter type not supported: " + (int)paramA.type);
                    break;
                }
            }
        }

        public struct AnimationStateInfo {
            public int stateHash;
            public float stateTime;
        }

        public struct AnimationParameterInfo {
            public int hash;
            public AnimatorControllerParameterType type;

            public int iValue;
            public bool bValue;
            public float fValue;
        }

        public struct AnimationStoredState {
            public int stateCount;
            public int parameterCount;

            public AnimationStateInfo[] stateInfos;
            public AnimationParameterInfo[] parameterInfos;
        }
    }
}