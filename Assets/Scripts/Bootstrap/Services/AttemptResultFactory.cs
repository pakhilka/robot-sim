using System;
using System.IO;
using RobotSim.Bootstrap.Data;
using RobotSim.Robot.Data.Results;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Builds final LevelRunResultDTO from execution snapshot and artifact layout.
    /// </summary>
    public sealed class AttemptResultFactory
    {
        public LevelRunResultDTO CreateFromSnapshot(
            string levelName,
            AttemptExecutionSnapshot executionSnapshot,
            AttemptArtifactsLayout artifactsLayout)
        {
            string normalizedStatus = string.IsNullOrWhiteSpace(executionSnapshot.Status)
                ? "fail"
                : executionSnapshot.Status;

            FailureType failureType = normalizedStatus == "pass"
                ? FailureType.None
                : (executionSnapshot.FailureType == FailureType.None ? FailureType.Error : executionSnapshot.FailureType);

            return new LevelRunResultDTO(
                levelName,
                normalizedStatus,
                failureType,
                executionSnapshot.Reason ?? string.Empty,
                executionSnapshot.ElapsedSeconds,
                BuildArtifactsDto(artifactsLayout));
        }

        public LevelRunResultDTO CreateFail(
            string levelName,
            FailureType failureType,
            string reason,
            float durationSeconds,
            AttemptArtifactsLayout artifactsLayout)
        {
            FailureType normalizedFailureType = failureType == FailureType.None
                ? FailureType.Error
                : failureType;

            return new LevelRunResultDTO(
                levelName,
                "fail",
                normalizedFailureType,
                reason ?? string.Empty,
                durationSeconds < 0f ? 0f : durationSeconds,
                BuildArtifactsDto(artifactsLayout));
        }

        public AttemptArtifactsDTO BuildArtifactsDto(AttemptArtifactsLayout layout)
        {
            return new AttemptArtifactsDTO
            {
                request = ToProjectRelativePath(layout.ProjectRootPath, layout.RequestPath),
                result = ToProjectRelativePath(layout.ProjectRootPath, layout.ResultPath),
                video = ToProjectRelativePath(layout.ProjectRootPath, layout.VideoPath)
            };
        }

        private static string ToProjectRelativePath(string projectRootPath, string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(projectRootPath) || string.IsNullOrWhiteSpace(absolutePath))
            {
                return string.Empty;
            }

            try
            {
                string root = AppendDirectorySeparator(Path.GetFullPath(projectRootPath));
                string full = Path.GetFullPath(absolutePath);

                Uri rootUri = new(root);
                Uri fileUri = new(full);
                string relative = Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString());
                return relative.Replace('/', Path.DirectorySeparatorChar);
            }
            catch
            {
                return absolutePath;
            }
        }

        private static string AppendDirectorySeparator(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return path.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? path
                : path + Path.DirectorySeparatorChar;
        }
    }
}
