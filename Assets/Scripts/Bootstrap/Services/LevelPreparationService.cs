using RobotSim.Bootstrap.Data;
using RobotSim.Levels.Generation;
using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Builds LevelGrid domain model from validated request.
    /// </summary>
    public sealed class LevelPreparationService
    {
        public LevelPreparationResult Prepare(LevelRunRequestDTO request)
        {
            if (LevelGridFactory.TryCreate(request, out var levelGrid, out string error))
            {
                return LevelPreparationResult.Success(levelGrid);
            }

            return LevelPreparationResult.Fail(error);
        }
    }
}
