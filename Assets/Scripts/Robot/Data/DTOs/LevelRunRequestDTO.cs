using System;

namespace RobotSim.Robot.Data.DTOs
{
    /// <summary>
    /// DTO - Запрос на запуск уровня (CLI/Headless)
    /// </summary>
    [Serializable]
    public struct LevelRunRequestDTO
    {
        public string name;
        public string socketAddress;
        public int levelCompletionLimitSeconds;
        public float startRotationDegrees;
        public string[][] map;
    }
}
