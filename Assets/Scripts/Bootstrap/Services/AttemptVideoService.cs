using RobotSim.Bootstrap.Attempts;
using RobotSim.Bootstrap.Data;
using RobotSim.Bootstrap.Interfaces;
using RobotSim.Robot.Data.Results;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Owns video lifecycle and maps recorder failures to video_error.
    /// </summary>
    public sealed class AttemptVideoService
    {
        private IAttemptVideoRecorder _videoRecorder;

        public AttemptVideoService()
            : this(new PngFrameCaptureVideoRecorder())
        {
        }

        public AttemptVideoService(IAttemptVideoRecorder videoRecorder)
        {
            _videoRecorder = videoRecorder ?? new NoopAttemptVideoRecorder();
        }

        public bool IsCapturing => _videoRecorder != null && _videoRecorder.IsCapturing;

        public void SetRecorder(IAttemptVideoRecorder recorder)
        {
            _videoRecorder = recorder ?? new NoopAttemptVideoRecorder();
        }

        public bool TryStart(
            AttemptController attemptController,
            AttemptVideoRecorderRequest request,
            out string error)
        {
            if (_videoRecorder.TryStartCapture(request, out error))
            {
                return true;
            }

            ApplyVideoFailure(attemptController, error);
            return false;
        }

        public bool TryCaptureFrame(
            AttemptController attemptController,
            out string error)
        {
            if (_videoRecorder.TryCaptureFrame(out error))
            {
                return true;
            }

            ApplyVideoFailure(attemptController, error);
            return false;
        }

        public bool TryStop(
            AttemptController attemptController,
            float attemptDurationSeconds,
            out AttemptVideoRecorderResult result)
        {
            bool success = _videoRecorder.TryStopAndEncode(attemptDurationSeconds, out result);
            if (success)
            {
                return true;
            }

            string error = string.IsNullOrWhiteSpace(result.Error)
                ? "Video recording failed."
                : result.Error;
            ApplyVideoFailure(attemptController, error);
            return false;
        }

        private static void ApplyVideoFailure(AttemptController attemptController, string error)
        {
            if (attemptController == null)
            {
                return;
            }

            string normalizedError = string.IsNullOrWhiteSpace(error)
                ? "Video recording failed."
                : error;
            attemptController.ForceFail(FailureType.VideoError, normalizedError);
        }
    }
}
