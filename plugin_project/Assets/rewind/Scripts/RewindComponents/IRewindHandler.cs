namespace aeric.rewind_plugin {
    /// <summary>
    /// Interface for anything that stores and restores game state.
    /// </summary>
    public interface IRewindHandler {
        /// <summary>
        /// ID value that uniquely identifies a rewind handler within a RewindScene
        /// </summary>
        uint ID { get; set; }

        /// <summary>
        /// Constructs the data schema that defines the storage needs for this components data
        /// </summary>
        /// <returns></returns>
        RewindDataSchema makeDataSchema();

        /// <summary>
        /// Each IRewindHandler implementation must have a unique HandlerTypeID.
        /// </summary>
        uint HandlerTypeID { get; }

        /// <summary>
        /// Allows handlers to implement custom logic to prepare for the recording of game state
        /// </summary>
        void startRecording();

        /// <summary>
        /// Store a frames worth of game state for this handler
        /// </summary>
        /// <param name="writer"></param>
        void rewindStore(NativeByteArrayWriter writer);
        
        /// <summary>
        /// Restore game state by reading from 2 frame readers and interpolating the results based on frameT
        /// </summary>
        /// <param name="frameReaderA">frame reader for the first frame</param>
        /// <param name="frameReaderB">frame reader for the next frame</param>
        /// <param name="frameT">value is between 0 and 1</param>
        void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT);

        /// <summary>
        /// callback when values have been restored
        /// </summary>
        void preRestore();
        
        /// <summary>
        /// callback when values have been restored
        /// </summary>
        void postRestore();
    }
}