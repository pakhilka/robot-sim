using RobotSim.Interfaces;
using RobotSim.Data.DTOs;
using RobotSim.Data.Results;

namespace RobotSim.Brains
{
    /// <summary>
    /// Простой мозг для отладки: стопится перед препятствием
    /// Чистый класс, не зависит от MonoBehaviour
    /// </summary>
    public class LocalMockBrain : IRobotBrain
    {
        private readonly BrainConfig _config;

        public LocalMockBrain(BrainConfig config)
        {
            _config = config;
        }

        public BrainStepResultDTO Tick(SensorDataDTO sensors)
        {
            MotorCommandDTO command;

            if (sensors.distanceFront <= _config.StopDistance)
            {
                command = new MotorCommandDTO(0f, 0f);
            }
            else
            {
                command = _config.DriveCommand;
            }

            return new BrainStepResultDTO(BrainStatusDTO.Ready, command, "mock_brain");
        }
    }
}
