namespace RobotSim.Bootstrap.Data
{
    // Keep RequestSource in an already-included Data file so BootstrapRunner can always resolve it.
    public enum RequestSource
    {
        None = 0,
        CommandLinePath,
        EditorPath
    }

    /// <summary>
    /// Output contract for runtime frame capture and external encoding.
    /// </summary>
    public readonly struct AttemptVideoRecorderResult
    {
        public AttemptVideoRecorderResult(
            bool succeeded,
            int capturedFrames,
            float effectiveFramesPerSecond,
            string error)
        {
            Succeeded = succeeded;
            CapturedFrames = capturedFrames;
            EffectiveFramesPerSecond = effectiveFramesPerSecond;
            Error = error;
        }

        public bool Succeeded { get; }
        public int CapturedFrames { get; }
        public float EffectiveFramesPerSecond { get; }
        public string Error { get; }
    }
}
