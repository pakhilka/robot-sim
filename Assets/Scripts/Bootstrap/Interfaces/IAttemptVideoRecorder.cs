using RobotSim.Bootstrap.Data;

namespace RobotSim.Bootstrap.Interfaces
{
    /// <summary>
    /// Port for runtime attempt video recording:
    /// frame capture to directory + external encode to video file.
    /// </summary>
    public interface IAttemptVideoRecorder
    {
        bool IsCapturing { get; }

        bool TryStartCapture(AttemptVideoRecorderRequest request, out string error);

        bool TryCaptureFrame(out string error);

        bool TryStopAndEncode(float attemptDurationSeconds, out AttemptVideoRecorderResult result);
    }
}
