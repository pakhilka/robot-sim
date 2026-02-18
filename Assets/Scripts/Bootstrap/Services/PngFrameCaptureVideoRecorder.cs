using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using RobotSim.Bootstrap.Data;
using RobotSim.Bootstrap.Interfaces;
using UnityEngine;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Captures PNG frames to artifacts/<attempt>/frames.
    /// External encoding is added in the next task.
    /// </summary>
    public sealed class PngFrameCaptureVideoRecorder : IAttemptVideoRecorder
    {
        private const string FfmpegPathEnvVar = "ROBOTSIM_FFMPEG_PATH";

        private string _framesDirectoryPath = string.Empty;
        private string _outputVideoPath = string.Empty;
        private int _frameIndex;
        private int _capturedFrames;

        public bool IsCapturing { get; private set; }

        public bool TryStartCapture(AttemptVideoRecorderRequest request, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(request.FramesDirectoryPath))
            {
                error = "Frames directory path is empty.";
                return false;
            }

            try
            {
                Directory.CreateDirectory(request.FramesDirectoryPath);
            }
            catch (Exception ex)
            {
                error = $"Failed to create frames directory '{request.FramesDirectoryPath}'. {ex.Message}";
                return false;
            }

            _framesDirectoryPath = request.FramesDirectoryPath;
            _outputVideoPath = request.OutputVideoPath;
            _frameIndex = 0;
            _capturedFrames = 0;
            IsCapturing = true;
            return true;
        }

        public bool TryCaptureFrame(out string error)
        {
            error = string.Empty;

            if (!IsCapturing)
            {
                error = "Frame capture is not active.";
                return false;
            }

            string filePath = Path.Combine(_framesDirectoryPath, $"frame-{_frameIndex:D06}.png");

            try
            {
                ScreenCapture.CaptureScreenshot(filePath);
                _frameIndex++;
                _capturedFrames++;
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to capture frame '{filePath}'. {ex.Message}";
                return false;
            }
        }

        public bool TryStopAndEncode(float attemptDurationSeconds, out AttemptVideoRecorderResult result)
        {
            IsCapturing = false;

            float effectiveFps = attemptDurationSeconds > 0f
                ? _capturedFrames / attemptDurationSeconds
                : 0f;

            if (string.IsNullOrWhiteSpace(_outputVideoPath))
            {
                result = new AttemptVideoRecorderResult(
                    false,
                    _capturedFrames,
                    effectiveFps,
                    "Output video path is empty.");
                return false;
            }

            if (_capturedFrames <= 0)
            {
                result = new AttemptVideoRecorderResult(
                    false,
                    _capturedFrames,
                    effectiveFps,
                    "No frames were captured.");
                return false;
            }

            float ffmpegFps = effectiveFps > 0f ? effectiveFps : 30f;
            if (!TryEncodeWithFfmpeg(ffmpegFps, out string encodeError))
            {
                result = new AttemptVideoRecorderResult(
                    false,
                    _capturedFrames,
                    ffmpegFps,
                    encodeError);
                return false;
            }

            if (!TryCleanupFramesDirectory(out string cleanupError))
            {
                result = new AttemptVideoRecorderResult(
                    false,
                    _capturedFrames,
                    ffmpegFps,
                    cleanupError);
                return false;
            }

            result = new AttemptVideoRecorderResult(
                true,
                _capturedFrames,
                ffmpegFps,
                string.Empty);

            return true;
        }

        private bool TryEncodeWithFfmpeg(float framesPerSecond, out string error)
        {
            error = string.Empty;

            if (!TryResolveFfmpegExecutablePath(out string ffmpegPath, out string resolveError))
            {
                error = resolveError;
                return false;
            }

            string outputDirectory = Path.GetDirectoryName(_outputVideoPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string inputPattern = Path.Combine(_framesDirectoryPath, "frame-%06d.png");
            string fps = framesPerSecond.ToString("0.###", CultureInfo.InvariantCulture);
            // libx264 + yuv420p требует четные размеры кадра (width/height % 2 == 0).
            // Подгоняем размер через pad, чтобы не падать на odd-width/odd-height в dev/CI.
            const string evenSizeFilter = "pad=ceil(iw/2)*2:ceil(ih/2)*2";
            string arguments = $"-y -hide_banner -loglevel error -framerate {fps} -i \"{inputPattern}\" -vf \"{evenSizeFilter}\" -c:v libx264 -pix_fmt yuv420p \"{_outputVideoPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    error = "Failed to start ffmpeg process.";
                    return false;
                }

                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0 || !File.Exists(_outputVideoPath))
                {
                    error = $"ffmpeg encoding failed. exitCode={process.ExitCode}. {stdErr} {stdOut}".Trim();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to run ffmpeg. {ex.Message}";
                return false;
            }
        }

        private bool TryResolveFfmpegExecutablePath(out string resolvedPath, out string error)
        {
            resolvedPath = string.Empty;
            error = string.Empty;

            var candidates = new List<string>();
            AddFfmpegCandidates(candidates);

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                string normalized = Path.GetFullPath(candidate);
                if (!visited.Add(normalized))
                {
                    continue;
                }

                if (!File.Exists(normalized))
                {
                    continue;
                }

                resolvedPath = normalized;
                return true;
            }

            error =
                $"ffmpeg executable was not found. Checked env '{FfmpegPathEnvVar}', project-local 'Tools/ffmpeg', PATH directories, and common system locations.";
            return false;
        }

        private void AddFfmpegCandidates(List<string> candidates)
        {
            if (candidates == null)
            {
                return;
            }

            string envPath = Environment.GetEnvironmentVariable(FfmpegPathEnvVar);
            AddCandidate(candidates, envPath);

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            AddCandidate(candidates, Path.Combine(projectRoot, "Tools", "ffmpeg", "ffmpeg"));
            AddCandidate(candidates, Path.Combine(projectRoot, "Tools", "ffmpeg", "ffmpeg.exe"));

            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] pathDirectories = pathEnv.Split(Path.PathSeparator);
            foreach (string directory in pathDirectories)
            {
                AddCandidate(candidates, Path.Combine(directory, "ffmpeg"));
                AddCandidate(candidates, Path.Combine(directory, "ffmpeg.exe"));
            }

            AddCandidate(candidates, "/opt/homebrew/bin/ffmpeg");
            AddCandidate(candidates, "/usr/local/bin/ffmpeg");
            AddCandidate(candidates, "/usr/bin/ffmpeg");
        }

        private static void AddCandidate(List<string> candidates, string path)
        {
            if (candidates == null || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            candidates.Add(path.Trim());
        }

        private bool TryCleanupFramesDirectory(out string error)
        {
            error = string.Empty;

            try
            {
                if (Directory.Exists(_framesDirectoryPath))
                {
                    Directory.Delete(_framesDirectoryPath, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to cleanup frames directory '{_framesDirectoryPath}'. {ex.Message}";
                return false;
            }
        }
    }
}
