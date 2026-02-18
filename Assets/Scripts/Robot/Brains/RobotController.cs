using System;
using RobotSim.Robot.Interfaces;
using RobotSim.Robot.Data.DTOs;
using RobotSim.Robot.Data.Results;

namespace RobotSim.Robot.Brains
{
    /// <summary>
    /// Контроллер робота - оркестрирует работу мозга и тела
    /// Чистый класс, не зависит от MonoBehaviour
    /// </summary>
    public class RobotController
    {
        private readonly IRobotBrain _brain;
        private readonly BrainConfig _config;

        // TEMP(local-brain-right-turn-5m):
        // Временная стабилизация для узких коридоров.
        // План: вынести в отдельный policy/service уровня Core для alarm-handling.
        private ControllerState _state = ControllerState.DriveForward;
        private float _turnElapsedSeconds;
        private int _clearAlarmTicks;

        private enum ControllerState
        {
            DriveForward = 0,
            TurnRight
        }

        public RobotController(
            BrainType brainType,
            string tcpHost = "127.0.0.1",
            int tcpPort = 9999,
            BrainConfig config = null)
        {
            _config = config ?? new BrainConfig();
            _brain = BrainSelector.CreateBrain(brainType, tcpHost, tcpPort, _config);
        }

        /// <summary>
        /// Выполнить один шаг логики - мозг принимает решение
        /// </summary>
        public MotorCommandDTO Tick(SensorDataDTO sensorData)
        {
            BrainStepResultDTO result = _brain.Tick(sensorData);

            // Если мозг не в ready-состоянии (например, TCP еще не подключен),
            // просто исполняем его команду напрямую.
            if (result.status != BrainStatusDTO.Ready)
            {
                ResetTurnState();
                return result.command;
            }

            // TEMP(local-brain-right-turn-5m):
            // До внедрения постоянной policy-логики используем alarm-driven FSM.
            bool alarmActive = IsAlarmActive(result);
            float dt = sensorData.dt > 0f ? sensorData.dt : 0.02f;
            UpdateStateMachine(alarmActive, dt);

            return _state == ControllerState.TurnRight
                ? _config.LocalRightTurnCommand
                : _config.DriveCommand;
        }

        private bool IsAlarmActive(BrainStepResultDTO result)
        {
            if (!string.IsNullOrWhiteSpace(result.lastMessage) &&
                result.lastMessage.Trim().Equals(_config.AlarmToken, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // TEMP(local-brain-right-turn-5m):
            // Fallback для LocalMock, пока у нас нет унифицированного alarm-контракта между мозгами.
            return IsSameCommand(result.command, _config.LocalRightTurnCommand);
        }

        private void UpdateStateMachine(bool alarmActive, float dt)
        {
            // TEMP(local-brain-right-turn-5m):
            // Антидребезг-переходы. Будет заменено на постоянную стратегию obstacle recovery.
            if (_state == ControllerState.DriveForward)
            {
                if (alarmActive)
                {
                    _state = ControllerState.TurnRight;
                    _turnElapsedSeconds = 0f;
                    _clearAlarmTicks = 0;
                }

                return;
            }

            _turnElapsedSeconds += dt;
            if (alarmActive)
            {
                _clearAlarmTicks = 0;
            }
            else
            {
                _clearAlarmTicks++;
            }

            float minTurnSeconds = Math.Max(0f, _config.ControllerMinTurnDurationSeconds);
            int requiredClearTicks = Math.Max(1, _config.ControllerAlarmClearTicksRequired);
            if (_turnElapsedSeconds < minTurnSeconds || _clearAlarmTicks < requiredClearTicks)
            {
                return;
            }

            _state = ControllerState.DriveForward;
            ResetTurnState();
        }

        private void ResetTurnState()
        {
            _turnElapsedSeconds = 0f;
            _clearAlarmTicks = 0;
            _state = ControllerState.DriveForward;
        }

        private static bool IsSameCommand(MotorCommandDTO left, MotorCommandDTO right)
        {
            return Math.Abs(left.left - right.left) <= 0.0001f &&
                   Math.Abs(left.right - right.right) <= 0.0001f;
        }
    }

    public enum BrainType { WokwiTcp, LocalMock }
}
