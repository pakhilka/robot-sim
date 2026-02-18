using RobotSim.Bootstrap.Data;
using RobotSim.Bootstrap.Interfaces;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Default recorder stub used before concrete frame capture/encoding implementation.
    /// </summary>
    public sealed class NoopAttemptVideoRecorder : IAttemptVideoRecorder
    {
        public bool IsCapturing { get; private set; }

        public bool TryStartCapture(AttemptVideoRecorderRequest request, out string error)
        {
            error = "Video recorder is not configured.";
            IsCapturing = false;
            return false;
        }

        public bool TryCaptureFrame(out string error)
        {
            error = "Video recorder is not configured.";
            return false;
        }

        public bool TryStopAndEncode(float attemptDurationSeconds, out AttemptVideoRecorderResult result)
        {
            IsCapturing = false;
            result = new AttemptVideoRecorderResult(
                false,
                0,
                0f,
                "Video recorder is not configured.");
            return false;
        }
    }
}
