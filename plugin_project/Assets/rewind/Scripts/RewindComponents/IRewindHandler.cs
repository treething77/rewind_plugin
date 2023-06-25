namespace aeric.rewind_plugin {
    public interface IRewindHandler {
        uint ID { get; set; }

        RewindDataSchema makeDataSchema();

        uint HandlerTypeID { get; }

        void startRecording();

        void rewindStore(NativeByteArrayWriter writer);
        void rewindRestoreInterpolated(NativeByteArrayReader frameReaderA, NativeByteArrayReader frameReaderB, float frameT);

        void preRestore(); //callback when values have been restored
        void postRestore(); //callback when values have been restored
    }
}