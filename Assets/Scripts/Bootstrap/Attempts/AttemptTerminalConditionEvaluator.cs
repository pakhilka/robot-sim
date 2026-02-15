using RobotSim.Levels.Data;
using RobotSim.Robot.Data.Results;

namespace RobotSim.Bootstrap.Attempts
{
    /// <summary>
    /// Evaluates terminal conditions using runtime robot position and attempt timer.
    /// </summary>
    public sealed class AttemptTerminalConditionEvaluator
    {
        private readonly LevelGrid _grid;
        private readonly AttemptController _attemptController;

        public AttemptTerminalConditionEvaluator(LevelGrid grid, AttemptController attemptController)
        {
            _grid = grid;
            _attemptController = attemptController;
        }

        public bool Evaluate(float robotX, float robotZ)
        {
            if (_grid == null || _attemptController == null || !_attemptController.IsRunning)
            {
                return false;
            }

            if (IsInsideFinishCell(robotX, robotZ))
            {
                return _attemptController.TryCompletePass("Robot reached finish area.");
            }

            if (!_grid.IsWithinBounds(robotX, robotZ))
            {
                return _attemptController.TryCompleteFail(
                    FailureType.OutOfBounds,
                    "Robot left level bounds.");
            }

            if (_attemptController.IsTimeLimitExceeded)
            {
                return _attemptController.TryCompleteFail(
                    FailureType.Timeout,
                    "Level completion time limit exceeded.");
            }

            return false;
        }

        private bool IsInsideFinishCell(float x, float z)
        {
            float minX = _grid.FinishRow * _grid.CellSize;
            float maxX = minX + _grid.CellSize;
            float minZ = _grid.FinishCol * _grid.CellSize;
            float maxZ = minZ + _grid.CellSize;

            return x >= minX && x < maxX && z >= minZ && z < maxZ;
        }
    }
}
