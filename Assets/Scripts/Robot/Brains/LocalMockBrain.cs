using RobotSim.Robot.Interfaces;
using RobotSim.Robot.Data.DTOs;
using RobotSim.Robot.Data.Results;
using UnityEngine;

namespace RobotSim.Robot.Brains
{
    /// <summary>
    /// Простой локальный мозг для отладки.
    /// При близком препятствии выполняет поворот направо.
    /// Чистый класс, не зависит от MonoBehaviour
    /// </summary>
    public class LocalMockBrain : IRobotBrain
    {
        private readonly BrainConfig _config;
        private DecisionState _lastDecisionState = DecisionState.None;
        private int _lastLoggedRoundedDistance = int.MinValue;

        private enum DecisionState
        {
            None = 0,
            DriveForward,
            TurnRight
        }

        public LocalMockBrain(BrainConfig config)
        {
            _config = config;
        }

        public BrainStepResultDTO Tick(SensorDataDTO sensors)
        {
            LogDistanceInput(sensors.distanceFront);

            MotorCommandDTO command;
            DecisionState decision;

            if (sensors.distanceFront <= _config.LocalRightTurnDistanceThresholdMeters)
            {
                command = _config.LocalRightTurnCommand;
                decision = DecisionState.TurnRight;
            }
            else
            {
                command = _config.DriveCommand;
                decision = DecisionState.DriveForward;
            }

            LogDecisionTransition(decision, sensors.distanceFront);
            _lastDecisionState = decision;

            return new BrainStepResultDTO(BrainStatusDTO.Ready, command, "mock_brain");
        }

        private void LogDistanceInput(float distanceFront)
        {
            int roundedDistance = Mathf.RoundToInt(distanceFront);
            if (roundedDistance == _lastLoggedRoundedDistance)
            {
                return;
            }

            _lastLoggedRoundedDistance = roundedDistance;
            Debug.Log($"[LocalMockBrain] Received distanceFront={distanceFront:F2}m (rounded={roundedDistance}m).");
        }

        private void LogDecisionTransition(DecisionState decision, float distanceFront)
        {
            if (decision == _lastDecisionState)
            {
                return;
            }

            if (decision == DecisionState.DriveForward)
            {
                Debug.Log(
                    $"[LocalMockBrain] Start driving forward. distanceFront={distanceFront:F2}m > threshold={_config.LocalRightTurnDistanceThresholdMeters:F2}m.");
                return;
            }

            if (decision == DecisionState.TurnRight)
            {
                Debug.Log(
                    $"[LocalMockBrain] Turn right triggered. distanceFront={distanceFront:F2}m <= threshold={_config.LocalRightTurnDistanceThresholdMeters:F2}m.");
            }
        }
    }
}
