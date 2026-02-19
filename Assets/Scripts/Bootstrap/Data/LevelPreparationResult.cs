using RobotSim.Levels.Data;

namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Result contract for level preparation stage.
    /// </summary>
    public readonly struct LevelPreparationResult
    {
        public LevelPreparationResult(bool succeeded, LevelGrid grid, string error)
        {
            Succeeded = succeeded;
            Grid = grid;
            Error = error ?? string.Empty;
        }

        public bool Succeeded { get; }
        public LevelGrid Grid { get; }
        public string Error { get; }

        public static LevelPreparationResult Success(LevelGrid grid)
        {
            return new LevelPreparationResult(true, grid, string.Empty);
        }

        public static LevelPreparationResult Fail(string error)
        {
            return new LevelPreparationResult(false, null, error);
        }
    }
}
