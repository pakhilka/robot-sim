using RobotSim.Bootstrap.Attempts;
using RobotSim.Robot.Data.Results;

namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Immutable execution state snapshot used by result assembly.
    /// </summary>
    public readonly struct AttemptExecutionSnapshot
    {
        public AttemptExecutionSnapshot(
            bool isCompleted,
            string status,
            FailureType failureType,
            string reason,
            float elapsedSeconds)
        {
            IsCompleted = isCompleted;
            Status = status ?? string.Empty;
            FailureType = failureType;
            Reason = reason ?? string.Empty;
            ElapsedSeconds = elapsedSeconds < 0f ? 0f : elapsedSeconds;
        }

        public bool IsCompleted { get; }
        public string Status { get; }
        public FailureType FailureType { get; }
        public string Reason { get; }
        public float ElapsedSeconds { get; }

        public static AttemptExecutionSnapshot FromController(AttemptController controller)
        {
            if (controller == null)
            {
                return new AttemptExecutionSnapshot(
                    true,
                    "fail",
                    FailureType.Error,
                    "Attempt controller is missing.",
                    0f);
            }

            string status = string.IsNullOrWhiteSpace(controller.Status)
                ? "fail"
                : controller.Status;

            FailureType failureType = status == "pass"
                ? FailureType.None
                : (controller.FailureType == FailureType.None ? FailureType.Error : controller.FailureType);

            return new AttemptExecutionSnapshot(
                controller.IsCompleted,
                status,
                failureType,
                controller.Reason,
                controller.ElapsedSeconds);
        }

        public static AttemptExecutionSnapshot Fail(FailureType failureType, string reason, float elapsedSeconds)
        {
            FailureType normalizedFailureType = failureType == FailureType.None
                ? FailureType.Error
                : failureType;

            return new AttemptExecutionSnapshot(
                true,
                "fail",
                normalizedFailureType,
                reason,
                elapsedSeconds);
        }
    }
}
