namespace RobotSim.Robot.Data.Results
{
    /// <summary>
    /// Тип причины неуспешного прохождения уровня
    /// </summary>
    public enum FailureType
    {
        None = 0,
        InvalidInput,
        Connection,
        Timeout,
        OutOfBounds,
        Error,
        VideoError
    }
}
