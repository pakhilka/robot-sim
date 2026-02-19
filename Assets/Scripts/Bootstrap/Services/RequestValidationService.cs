using RobotSim.Bootstrap.Data;
using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Performs business validation for level run request fields.
    /// </summary>
    public sealed class RequestValidationService
    {
        public RequestValidationResult Validate(LevelRunRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.name))
            {
                return RequestValidationResult.Fail("Request field 'name' is missing.");
            }

            if (string.IsNullOrWhiteSpace(request.socketAddress))
            {
                return RequestValidationResult.Fail("Request field 'socketAddress' is missing.");
            }

            if (!SocketConnectionProbe.TryParseSocketAddress(request.socketAddress, out _, out _, out string socketError))
            {
                return RequestValidationResult.Fail($"Request field 'socketAddress' is invalid. {socketError}");
            }

            if (request.levelCompletionLimitSeconds <= 0)
            {
                return RequestValidationResult.Fail("Request field 'levelCompletionLimitSeconds' must be > 0.");
            }

            if (request.map == null || request.map.Length == 0)
            {
                return RequestValidationResult.Fail("Request field 'map' is missing or empty.");
            }

            return RequestValidationResult.Success();
        }
    }
}
