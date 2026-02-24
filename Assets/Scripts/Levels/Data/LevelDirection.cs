using System;

namespace RobotSim.Levels.Data
{
    /// <summary>
    /// Направления соседства в клеточной карте.
    /// </summary>
    [Flags]
    public enum LevelDirection
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3
    }
}
