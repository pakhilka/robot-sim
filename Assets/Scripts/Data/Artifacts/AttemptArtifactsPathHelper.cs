using System;
using System.IO;

namespace RobotSim.Data.Artifacts
{
    /// <summary>
    /// Helper - Формирование путей для артефактов попытки
    /// </summary>
    public static class AttemptArtifactsPathHelper
    {
        public const string ArtifactsRootFolderName = "artifacts";
        public const string FramesFolderName = "frames";
        public const string RequestFileName = "request.json";
        public const string ResultFileName = "result.json";
        public const string VideoFileName = "video.mp4";

        public static string BuildAttemptFolderName(string name, DateTime utcNow, Guid uuid)
        {
            string timestamp = utcNow.ToString("yyyyMMdd-HHmmss");
            return $"{name}-{timestamp}-{uuid}";
        }

        public static string BuildArtifactsRootPath(string projectRootPath)
        {
            return Path.Combine(projectRootPath, ArtifactsRootFolderName);
        }

        public static string BuildAttemptFolderPath(string projectRootPath, string name, DateTime utcNow, Guid uuid)
        {
            return Path.Combine(BuildArtifactsRootPath(projectRootPath), BuildAttemptFolderName(name, utcNow, uuid));
        }

        public static string BuildFramesFolderPath(string attemptFolderPath)
        {
            return Path.Combine(attemptFolderPath, FramesFolderName);
        }

        public static string BuildRequestPath(string attemptFolderPath)
        {
            return Path.Combine(attemptFolderPath, RequestFileName);
        }

        public static string BuildResultPath(string attemptFolderPath)
        {
            return Path.Combine(attemptFolderPath, ResultFileName);
        }

        public static string BuildVideoPath(string attemptFolderPath)
        {
            return Path.Combine(attemptFolderPath, VideoFileName);
        }
    }
}
