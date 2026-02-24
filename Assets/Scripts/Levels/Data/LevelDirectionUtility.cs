namespace RobotSim.Levels.Data
{
    /// <summary>
    /// Утилиты для работы с направлениями.
    /// </summary>
    public static class LevelDirectionUtility
    {
        public static readonly LevelDirection[] CardinalDirections =
        {
            LevelDirection.North,
            LevelDirection.East,
            LevelDirection.South,
            LevelDirection.West
        };

        public static LevelDirection Opposite(LevelDirection direction)
        {
            switch (direction)
            {
                case LevelDirection.North:
                    return LevelDirection.South;
                case LevelDirection.East:
                    return LevelDirection.West;
                case LevelDirection.South:
                    return LevelDirection.North;
                case LevelDirection.West:
                    return LevelDirection.East;
                default:
                    return LevelDirection.None;
            }
        }

        public static bool TryStep(LevelDirection direction, out int rowDelta, out int colDelta)
        {
            rowDelta = 0;
            colDelta = 0;

            switch (direction)
            {
                case LevelDirection.North:
                    rowDelta = -1;
                    return true;
                case LevelDirection.East:
                    colDelta = 1;
                    return true;
                case LevelDirection.South:
                    rowDelta = 1;
                    return true;
                case LevelDirection.West:
                    colDelta = -1;
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNorthSouth(LevelDirection direction)
        {
            return direction == LevelDirection.North || direction == LevelDirection.South;
        }
    }
}
