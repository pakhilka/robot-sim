namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Input contract for runtime frame capture and external encoding.
    /// </summary>
    public readonly struct AttemptVideoRecorderRequest
    {
        public AttemptVideoRecorderRequest(
            string framesDirectoryPath,
            string outputVideoPath,
            int captureWidth,
            int captureHeight)
        {
            FramesDirectoryPath = framesDirectoryPath;
            OutputVideoPath = outputVideoPath;
            CaptureWidth = captureWidth;
            CaptureHeight = captureHeight;
        }

        public string FramesDirectoryPath { get; }
        public string OutputVideoPath { get; }
        public int CaptureWidth { get; }
        public int CaptureHeight { get; }
    }
}
