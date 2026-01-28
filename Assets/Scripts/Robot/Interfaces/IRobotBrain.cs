using RobotSim.Data.DTOs;
using RobotSim.Data.Results;

namespace RobotSim.Interfaces
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
