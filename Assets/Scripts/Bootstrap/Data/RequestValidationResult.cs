namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Result contract for request business validation stage.
    /// </summary>
    public readonly struct RequestValidationResult
    {
        public RequestValidationResult(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Error = error ?? string.Empty;
        }

        public bool Succeeded { get; }
        public string Error { get; }

        public static RequestValidationResult Success()
        {
            return new RequestValidationResult(true, string.Empty);
        }

        public static RequestValidationResult Fail(string error)
        {
            return new RequestValidationResult(false, error);
        }
    }
}
