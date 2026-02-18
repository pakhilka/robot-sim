using RobotSim.Robot.Data.Results;

namespace RobotSim.Bootstrap.Attempts
{
    /// <summary>
    /// Core lifecycle controller for a single attempt run.
    /// Keeps timing and finalized result state.
    /// </summary>
    public sealed class AttemptController
    {
        public AttemptController(float timeLimitSeconds)
        {
            TimeLimitSeconds = timeLimitSeconds < 0f ? 0f : timeLimitSeconds;
            Reset();
        }

        public float TimeLimitSeconds { get; }
        public float ElapsedSeconds { get; private set; }

        public string Status { get; private set; }
        public FailureType FailureType { get; private set; }
        public string Reason { get; private set; }

        public bool IsStarted { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsRunning => IsStarted && !IsCompleted;
        public bool IsTimeLimitExceeded => IsRunning && ElapsedSeconds >= TimeLimitSeconds;

        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            IsStarted = true;
            IsCompleted = false;
            ElapsedSeconds = 0f;
            Status = string.Empty;
            FailureType = FailureType.None;
            Reason = string.Empty;
        }

        public void Tick(float deltaSeconds)
        {
            if (!IsRunning)
            {
                return;
            }

            if (deltaSeconds < 0f)
            {
                deltaSeconds = 0f;
            }

            ElapsedSeconds += deltaSeconds;
        }

        public bool TryCompletePass(string reason)
        {
            return TryComplete("pass", FailureType.None, reason);
        }

        public bool TryCompleteFail(FailureType failureType, string reason)
        {
            FailureType mappedFailureType = failureType == FailureType.None ? FailureType.Error : failureType;
            return TryComplete("fail", mappedFailureType, reason);
        }

        public void Reset()
        {
            IsStarted = false;
            IsCompleted = false;
            ElapsedSeconds = 0f;
            Status = string.Empty;
            FailureType = FailureType.None;
            Reason = string.Empty;
        }

        public void ForceFail(FailureType failureType, string reason)
        {
            FailureType mappedFailureType = failureType == FailureType.None ? FailureType.Error : failureType;

            if (!IsStarted)
            {
                IsStarted = true;
            }

            Status = "fail";
            FailureType = mappedFailureType;
            Reason = reason ?? string.Empty;
            IsCompleted = true;
        }

        private bool TryComplete(string status, FailureType failureType, string reason)
        {
            if (!IsRunning)
            {
                return false;
            }

            Status = status ?? string.Empty;
            FailureType = failureType;
            Reason = reason ?? string.Empty;
            IsCompleted = true;
            return true;
        }
    }
}
