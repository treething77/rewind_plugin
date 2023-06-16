using UnityEngine;

namespace aeric.rewind_plugin {
    public abstract class RewindComponentBase : MonoBehaviour, IRewindHandler {
        //[HideInInspector] 
        [SerializeField] private uint id;

        public virtual bool ShouldStayEnabledDuringReplay => false;

        public uint ID {
            get => id;
            set => id = value;
        }

        //Required to be implemented by sub-classes
        public abstract void rewindStore(NativeByteArrayWriter writer);

        public abstract int RequiredBufferSizeBytes { get; }
        public abstract uint HandlerTypeID { get; }
        public abstract void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT);

        /// <summary>
        /// Called before each restore operation
        /// </summary>
        public virtual void preRestore() { }

        /// <summary>
        /// Called after each restore operation
        /// </summary>
        public virtual void postRestore() { }

        /// <summary>
        /// Called before we start recording
        /// </summary>
        public virtual void startRecording() {}

        public void OnBeforeSerialize() {
            // If we don't have an ID yet then generate one
            if (id == 0) id = RewindComponentIDGenerator.generateID(this);
        }

        public virtual bool shouldDisableComponent(Component component) {
            if (component is Camera) return false;
            if (component is AudioSource) return false;
            if (component is AudioListener) return false;
            return true; //by default disable all other components
        }

        public virtual void startPlayback() { }

        public virtual void stopPlayback() { }
    }
}