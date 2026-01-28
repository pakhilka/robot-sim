using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using RobotSim.Interfaces;
using RobotSim.Data.DTOs;
using RobotSim.Data.Results;
using RobotSim.Services;

namespace RobotSim.Brains
{
    /// <summary>
    /// TCP-клиент мозга для подключения к docker wokwi-tcp
    /// Протокол: отправляем расстояние, получаем "alarm" если слишком близко
    /// Чистый класс, не зависит от MonoBehaviour
    /// </summary>
    public class WokwiTcpBrain : IRobotBrain
    {
        private readonly ITcpClientService _tcpService;
        private readonly BrainConfig _config;
        
        private BrainStatusDTO _status = BrainStatusDTO.Disconnected;
        private string _lastLine = "";
        private MotorCommandDTO _currentCmd;

        public WokwiTcpBrain(ITcpClientService tcpService, BrainConfig config)
        {
            _tcpService = tcpService;
            _config = config;
            _currentCmd = new MotorCommandDTO(0f, 0f);
            
            // Подключаемся при инициализации
            Connect();
        }

        private void Connect()
        {
            if (_status != BrainStatusDTO.Disconnected && _status != BrainStatusDTO.Error)
                return;

            _status = BrainStatusDTO.Connecting;
            _lastLine = "";

            try
            {
                _tcpService.Connect();
                Debug.Log("[WokwiTcpBrain] Подключение инициировано");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WokwiTcpBrain] Ошибка подключения: {ex.Message}");
                _status = BrainStatusDTO.Error;
            }
        }

        private void Disconnect()
        {
            try
            {
                _tcpService.Disconnect();
            }
            catch { }
            
            _status = BrainStatusDTO.Disconnected;
        }

        public BrainStepResultDTO Tick(SensorDataDTO sensors)
        {
            // Если статус не Ready/Error — пытаемся пересоединиться
            if (_status == BrainStatusDTO.Disconnected || _status == BrainStatusDTO.Error)
            {
                Connect();
            }

            // Отправляем расстояние, если есть активное соединение
            if (_status != BrainStatusDTO.Disconnected && _status != BrainStatusDTO.Error)
            {
                try
                {
                    int distance = Mathf.RoundToInt(sensors.distanceFront);
                    string response = _tcpService.SendDistance(distance);
                    
                    if (response != null)
                    {
                        _lastLine = response;

                        // Проверяем handshake
                        if (_status == BrainStatusDTO.Connecting && response.Contains(_config.ReadyToken))
                        {
                            _status = BrainStatusDTO.Ready;
                            Debug.Log("[WokwiTcpBrain] Получен handshake, статус Ready");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WokwiTcpBrain] Ошибка отправки: {ex.Message}");
                    _status = BrainStatusDTO.Error;
                }
            }

            // Вычисляем целевую команду
            MotorCommandDTO targetCmd;

            if (_status == BrainStatusDTO.Ready)
            {
                // Если получили "alarm" — тормозим
                if (_lastLine.Equals(_config.AlarmToken, StringComparison.OrdinalIgnoreCase))
                {
                    targetCmd = new MotorCommandDTO(0f, 0f);
                }
                else
                {
                    targetCmd = _config.DriveCommand;
                }
            }
            else
            {
                // Connecting/Error/Disconnected — остановка
                targetCmd = new MotorCommandDTO(0f, 0f);
            }

            // Плавный переход к целевой команде
            float timeToTarget = (targetCmd.left == 0f && targetCmd.right == 0f) ? _config.BrakeTime : _config.AccelTime;
            _currentCmd = SmoothTo(_currentCmd, targetCmd, timeToTarget);

            var result = new BrainStepResultDTO(_status, _currentCmd, _lastLine);
            return result;
        }

        private MotorCommandDTO SmoothTo(MotorCommandDTO current, MotorCommandDTO target, float timeToTarget)
        {
            float t = timeToTarget <= 0.0001f ? 1f : Mathf.Clamp01(Time.fixedDeltaTime / timeToTarget);

            return new MotorCommandDTO(
                Mathf.Lerp(current.left, target.left, t),
                Mathf.Lerp(current.right, target.right, t)
            );
        }

        ~WokwiTcpBrain()
        {
            Disconnect();
        }
    }
}
