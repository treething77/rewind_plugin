using System;
using JetBrains.Annotations;
using UnityEngine;

namespace ccl.rewind_plugin
{
    public class RewindAnimator : RewindComponentBase
    {
        private Animator _animator;
        private AnimatorControllerParameter[] animParams;
        private int animParamCount;
        private int animStateCount;
        
        public struct AnimationStateInfo
        {
            public int stateHash;
            public float stateTime;
        }
        
        public struct AnimationParameterInfo
        {
            public int hash;
            public AnimatorControllerParameterType type;

            public int iValue;
            public bool bValue;
            public float fValue;
        }
        
        public struct AnimationStoredState
        {
            public int stateCount;
            public int parameterCount;

            public AnimationStateInfo[] stateInfos;
            public AnimationParameterInfo[] parameterInfos;
        }

        private AnimationStoredState _animStateA;
        private AnimationStoredState _animStateB;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            
            //Cache the AnimatorControllerParameter objects because the GetParameter calls
            //are actually VERY expensive
            animParams = new AnimatorControllerParameter[_animator.parameterCount];
            for (int i = 0; i < _animator.parameterCount; i++)
            {
                // Get the observed animator parameter info
                animParams[i] = _animator.GetParameter(i);
            }
            
            animStateCount = _animator.layerCount;
            animParamCount = _animator.parameterCount;

            _animStateA.stateInfos = new AnimationStateInfo[animStateCount];
            _animStateB.stateInfos = new AnimationStateInfo[animStateCount];
            _animStateA.parameterInfos = new AnimationParameterInfo[animParamCount];
            _animStateB.parameterInfos = new AnimationParameterInfo[animParamCount];
        }

        public override void rewindStore(NativeByteArrayWriter writer)
        {
            writer.writeInt(animStateCount);
            writer.writeInt(animParamCount);

            for (int i = 0; i < animStateCount; i++)
            {
                //hash, time
                AnimatorStateInfo animState = _animator.GetCurrentAnimatorStateInfo(i);
                
                // Create the layer state info
                writer.writeInt(animState.fullPathHash);
                writer.writeFloat(animState.normalizedTime);
            }
            
            for (int i = 0; i < animParamCount; i++)
            {
                //hash, type, value - all 4 bytes
                AnimatorControllerParameter animParam = animParams[i];
                int paramHash = animParam.nameHash;
                int paramType = (int) animParam.type;
                
                writer.writeInt(paramHash);
                writer.writeInt(paramType);

                switch(animParam.type)
                {
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


        private bool validateAnimationStates()
        {
            if (_animStateA.stateCount != _animStateB.stateCount)
            {
                Debug.LogError("We cannot interpolate between two frames that have different animation state counts");
                return false; 
            }
            if (_animStateA.parameterCount != _animStateB.parameterCount)
            {
                Debug.LogError("We cannot interpolate between two frames that have different parameter count");
                return false;
            }
            return true;
        }

        private void readAnimationState(NativeByteArrayReader reader, ref AnimationStoredState animState)
        {
            animState.stateCount = reader.readInt();
            animState.parameterCount = reader.readInt();

            for (int i = 0; i < animState.stateCount; i++)
            {
                animState.stateInfos[i].stateHash = reader.readInt();
                animState.stateInfos[i].stateTime = reader.readFloat();
            }
            
            for (int i = 0; i < animState.parameterCount; i++)
            {
                animState.parameterInfos[i].hash = reader.readInt();
                animState.parameterInfos[i].type = (AnimatorControllerParameterType)reader.readInt();
                
                //interpolate the value
                switch(animState.parameterInfos[i].type)
                {
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
        
        public override bool shouldDisableComponent(Component component)
        {
            //don't disable the Animator
            if (component is Animator) return false;
            return true;
        }

        public override void rewindRestoreInterpolated([NotNull] NativeByteArrayReader frameReaderA, [NotNull] NativeByteArrayReader frameReaderB, float frameT)
        {
            //First read both animation states in full and then interpolate and restore the state
            readAnimationState(frameReaderA, ref _animStateA);
            readAnimationState(frameReaderB, ref _animStateB);

            if (!validateAnimationStates())
            {
                return;
            }
            
            for (int i = 0; i < _animStateA.stateCount; i++)
            {
                AnimationStateInfo stateA = _animStateA.stateInfos[i];
                AnimationStateInfo stateB = _animStateB.stateInfos[i];
                
                //If the state changed then don't interpolate the time
                if (stateA.stateHash == stateB.stateHash)
                {
                    float newTimeN = stateB.stateTime;
                    float lastTimeN = stateA.stateTime;
                    float newStateTime = Mathf.Lerp(lastTimeN, newTimeN, frameT);

                    _animator.Play(stateA.stateHash, i, newStateTime);
                }
                else
                {
                    _animator.Play(stateB.stateHash, i, stateB.stateTime);
                }
            }
            
            for (int i = 0; i < _animStateA.parameterCount; i++)
            {
                AnimationParameterInfo paramA = _animStateA.parameterInfos[i];
                AnimationParameterInfo paramB = _animStateA.parameterInfos[i];

                //interpolate the value
                switch (paramA.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        _animator.SetBool(paramA.hash, paramB.bValue);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        //we dont need to restore triggers? Framerate differences might make it difficult, and anyway
                        //if we restore anim state and time that should be enough?
                        break;
                    case AnimatorControllerParameterType.Int:
                        int newIntValue = RewindUtilities.LerpInt(paramA.iValue, paramB.iValue, frameT);
                        _animator.SetFloat(paramA.hash, newIntValue);
                        break;
                    case AnimatorControllerParameterType.Float:
                        float newFloatValue = Mathf.Lerp(paramA.fValue, paramB.fValue, frameT);
                        _animator.SetFloat(paramA.hash, newFloatValue);
                        break;
                    default:
                        Debug.LogError("Animator parameter type not supported: " + (int) paramA.type);
                        break;
                }
            }

        }

        public override int RequiredBufferSizeBytes
        {
            get
            {
                int stateCount = _animator.layerCount;
                int paramCount = _animator.parameterCount;

                return 8 + // (state count, param count)
                       (stateCount * 8) + // (hash, time)
                       (paramCount * 12); // (hash, type, value)
            }
        }

        public override uint HandlerTypeID => 3;

    }
}
