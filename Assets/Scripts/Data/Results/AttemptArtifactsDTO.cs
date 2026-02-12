using System;

namespace RobotSim.Data.Results
{
    /// <summary>
    /// DTO - Пути к артефактам попытки (относительно корня проекта)
    /// </summary>
    [Serializable]
    public struct AttemptArtifactsDTO
    {
        public string request;
        public string result;
        public string video;
    }
}
