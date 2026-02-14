using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Robot.Brains
{
    /// <summary>
    /// Конфигурация для мозгов робота
    /// </summary>
    public class BrainConfig
    {
        public string ReadyToken = "Parktronik ready";
        public string AlarmToken = "alarm";
        public MotorCommandDTO DriveCommand = new(1f, 1f);
        public float BrakeTime = 0.6f;
        public float AccelTime = 0.25f;
        public float StopDistance = 10f;
    }
}
