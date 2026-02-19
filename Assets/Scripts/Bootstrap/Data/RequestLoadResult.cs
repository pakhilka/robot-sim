using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Result contract for request loading stage.
    /// </summary>
    public readonly struct RequestLoadResult
    {
        public RequestLoadResult(
            bool succeeded,
            LevelRunRequestDTO request,
            string requestPath,
            RequestSource source,
            string error)
        {
            Succeeded = succeeded;
            Request = request;
            RequestPath = requestPath ?? string.Empty;
            Source = source;
            Error = error ?? string.Empty;
        }

        public bool Succeeded { get; }
        public LevelRunRequestDTO Request { get; }
        public string RequestPath { get; }
        public RequestSource Source { get; }
        public string Error { get; }

        public static RequestLoadResult Success(
            LevelRunRequestDTO request,
            string requestPath,
            RequestSource source)
        {
            return new RequestLoadResult(
                true,
                request,
                requestPath,
                source,
                string.Empty);
        }

        public static RequestLoadResult Fail(RequestSource source, string error)
        {
            return new RequestLoadResult(
                false,
                default,
                string.Empty,
                source,
                error);
        }
    }
}
