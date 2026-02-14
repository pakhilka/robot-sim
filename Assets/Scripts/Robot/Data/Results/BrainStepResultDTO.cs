using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Robot.Data.Results
{
    /// <summary>
    /// DTO - Результат одного шага (Tick) мозга
    /// </summary>
    public struct BrainStepResultDTO
    {
        public BrainStatusDTO status;
        public MotorCommandDTO command;
        public string lastMessage;

        public BrainStepResultDTO(BrainStatusDTO status, MotorCommandDTO command, string lastMessage = "")
        {
            this.status = status;
            this.command = command;
            this.lastMessage = lastMessage;
        }
    }
}
