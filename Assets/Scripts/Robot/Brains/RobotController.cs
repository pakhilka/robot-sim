using RobotSim.Robot.Interfaces;
using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Robot.Brains
{
    /// <summary>
    /// Контроллер робота - оркестрирует работу мозга и тела
    /// Чистый класс, не зависит от MonoBehaviour
    /// </summary>
    public class RobotController
    {
        private readonly IRobotBrain _brain;

        public RobotController(BrainType brainType, string tcpHost = "127.0.0.1", int tcpPort = 9999)
        {
            _brain = BrainSelector.CreateBrain(brainType, tcpHost, tcpPort);
        }

        /// <summary>
        /// Выполнить один шаг логики - мозг принимает решение
        /// </summary>
        public MotorCommandDTO Tick(SensorDataDTO sensorData)
        {
            var result = _brain.Tick(sensorData);
            return result.command;
        }
    }

    public enum BrainType { WokwiTcp, LocalMock }
}
