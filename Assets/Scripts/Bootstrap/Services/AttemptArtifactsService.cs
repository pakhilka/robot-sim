using System;
using System.IO;
using System.Text;
using RobotSim.Bootstrap.Data;
using RobotSim.Robot.Data.Artifacts;
using RobotSim.Robot.Data.DTOs;
using RobotSim.Robot.Data.Results;
using UnityEngine;
using Newtonsoft.Json;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Creates artifacts folder structure for one attempt.
    /// </summary>
    public sealed class AttemptArtifactsService
    {
        public bool TryCreateLayout(
            string levelName,
            DateTime utcNow,
            Guid attemptId,
            out AttemptArtifactsLayout layout,
            out string error)
        {
            layout = default;
            error = string.Empty;

            string projectRootPath = ResolveProjectRootPath();
            if (string.IsNullOrWhiteSpace(projectRootPath))
            {
                error = "Failed to resolve project root path.";
                return false;
            }

            string artifactsRootPath = AttemptArtifactsPathHelper.BuildArtifactsRootPath(projectRootPath);
            string safeName = SanitizeName(levelName);
            string attemptFolderPath = AttemptArtifactsPathHelper.BuildAttemptFolderPath(projectRootPath, safeName, utcNow, attemptId);

            try
            {
                Directory.CreateDirectory(artifactsRootPath);
                Directory.CreateDirectory(attemptFolderPath);
            }
            catch (Exception ex)
            {
                error = $"Failed to create artifacts folder '{attemptFolderPath}'. {ex.Message}";
                return false;
            }

            layout = new AttemptArtifactsLayout(
                projectRootPath,
                artifactsRootPath,
                attemptFolderPath,
                AttemptArtifactsPathHelper.BuildRequestPath(attemptFolderPath),
                AttemptArtifactsPathHelper.BuildResultPath(attemptFolderPath),
                AttemptArtifactsPathHelper.BuildVideoPath(attemptFolderPath),
                AttemptArtifactsPathHelper.BuildFramesFolderPath(attemptFolderPath));

            return true;
        }

        public bool TryCopyRequestJson(
            string sourceRequestPath,
            AttemptArtifactsLayout layout,
            out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(sourceRequestPath))
            {
                error = "Source request path is empty.";
                return false;
            }

            string sourcePath = Path.GetFullPath(sourceRequestPath);
            if (!File.Exists(sourcePath))
            {
                error = $"Source request file does not exist: {sourcePath}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(layout.RequestPath))
            {
                error = "Artifacts layout request path is empty.";
                return false;
            }

            try
            {
                string requestDirectory = Path.GetDirectoryName(layout.RequestPath);
                if (!string.IsNullOrWhiteSpace(requestDirectory))
                {
                    Directory.CreateDirectory(requestDirectory);
                }

                File.Copy(sourcePath, layout.RequestPath, true);
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to copy request JSON to artifacts. {ex.Message}";
                return false;
            }
        }

        public bool TryWriteResultJson(
            LevelRunResultDTO result,
            AttemptArtifactsLayout layout,
            out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(layout.ResultPath))
            {
                error = "Artifacts layout result path is empty.";
                return false;
            }

            try
            {
                string resultDirectory = Path.GetDirectoryName(layout.ResultPath);
                if (!string.IsNullOrWhiteSpace(resultDirectory))
                {
                    Directory.CreateDirectory(resultDirectory);
                }

                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                File.WriteAllText(layout.ResultPath, json);
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to write result JSON. {ex.Message}";
                return false;
            }
        }

        public bool TryWriteEditorDebugRequest(
            LevelRunRequestDTO request,
            RequestSource requestSource,
            string editorDebugOutputPath,
            out string error)
        {
            error = string.Empty;
            if (!ShouldWriteEditorDebug(requestSource, editorDebugOutputPath))
            {
                return true;
            }

            return TryWriteEditorDebugJson("last-request.json", request, editorDebugOutputPath, out error);
        }

        public bool TryWriteEditorDebugResult(
            LevelRunResultDTO result,
            RequestSource requestSource,
            string editorDebugOutputPath,
            out string error)
        {
            error = string.Empty;
            if (!ShouldWriteEditorDebug(requestSource, editorDebugOutputPath))
            {
                return true;
            }

            return TryWriteEditorDebugJson("last-result.json", result, editorDebugOutputPath, out error);
        }

        public bool TryCreateLayout(
            string levelName,
            out AttemptArtifactsLayout layout,
            out string error)
        {
            return TryCreateLayout(
                levelName,
                DateTime.UtcNow,
                Guid.NewGuid(),
                out layout,
                out error);
        }

        public static string ResolveProjectRootPath()
        {
            string assetsPath = Application.dataPath;
            if (string.IsNullOrWhiteSpace(assetsPath))
            {
                return string.Empty;
            }

            string projectRootPath = Path.GetFullPath(Path.Combine(assetsPath, ".."));
            return projectRootPath;
        }

        private static bool ShouldWriteEditorDebug(
            RequestSource requestSource,
            string editorDebugOutputPath)
        {
            if (!Application.isEditor || requestSource != RequestSource.EditorPath)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(editorDebugOutputPath);
        }

        private static bool TryWriteEditorDebugJson<T>(
            string fileName,
            T value,
            string editorDebugOutputPath,
            out string error)
        {
            error = string.Empty;

            string debugFolderPath = ResolveEditorDebugFolderPath(editorDebugOutputPath);
            if (string.IsNullOrWhiteSpace(debugFolderPath))
            {
                error = "Failed to resolve editor debug output folder path.";
                return false;
            }

            try
            {
                Directory.CreateDirectory(debugFolderPath);
                string outputPath = Path.Combine(debugFolderPath, fileName);
                string json = JsonConvert.SerializeObject(value, Formatting.Indented);
                File.WriteAllText(outputPath, json);
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to write editor debug JSON '{fileName}'. {ex.Message}";
                return false;
            }
        }

        private static string ResolveEditorDebugFolderPath(string inputPath)
        {
            string normalizedInput = inputPath?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedInput))
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(normalizedInput))
            {
                return normalizedInput;
            }

            string projectRootPath = ResolveProjectRootPath();
            if (string.IsNullOrWhiteSpace(projectRootPath))
            {
                return string.Empty;
            }

            return Path.GetFullPath(Path.Combine(projectRootPath, normalizedInput));
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "attempt";
            }

            string lower = name.Trim().ToLowerInvariant();
            var builder = new StringBuilder(lower.Length);
            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (char c in lower)
            {
                if (Array.IndexOf(invalidChars, c) >= 0)
                {
                    builder.Append('-');
                    continue;
                }

                builder.Append(char.IsWhiteSpace(c) ? '-' : c);
            }

            string sanitized = builder.ToString().Trim('-');
            return string.IsNullOrWhiteSpace(sanitized) ? "attempt" : sanitized;
        }
    }
}
