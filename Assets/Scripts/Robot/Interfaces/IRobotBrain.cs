using RobotSim.Robot.Data.DTOs;
using RobotSim.Robot.Data.Results;

namespace RobotSim.Robot.Interfaces
{
    /// <summary>
    /// Интерфейс для реализации "мозга" робота
    /// </summary>
    public interface IRobotBrain
    {
        /// <summary>
        /// Выполнить один шаг логики мозга
        /// </summary>
        BrainStepResultDTO Tick(SensorDataDTO sensors);
    }
}
