using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
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

            string outputDirectory = Path.GetDirectoryName(_outputVideoPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string inputPattern = Path.Combine(_framesDirectoryPath, "frame-%06d.png");
            string fps = framesPerSecond.ToString("0.###", CultureInfo.InvariantCulture);
            string arguments = $"-y -hide_banner -loglevel error -framerate {fps} -i \"{inputPattern}\" -c:v libx264 -pix_fmt yuv420p \"{_outputVideoPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
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
