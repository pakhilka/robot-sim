using System;

namespace RobotSim.Data.Results
{
    /// <summary>
    /// DTO - Результат попытки прохождения уровня
    /// </summary>
    [Serializable]
    public struct LevelRunResultDTO
    {
        public string name;
        public string status; // "pass" | "fail"
        public FailureType failureType;
        public string reason;
        public float durationSeconds;
        public AttemptArtifactsDTO artifacts;

        public LevelRunResultDTO(
            string name,
            string status,
            FailureType failureType,
            string reason,
            float durationSeconds,
            AttemptArtifactsDTO artifacts)
        {
            this.name = name;
            this.status = status;
            this.failureType = failureType;
            this.reason = reason;
            this.durationSeconds = durationSeconds;
            this.artifacts = artifacts;
        }
    }
}
