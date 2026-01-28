using System;
using System.Collections.Generic;
using UnityEngine;
using RobotSim.Interfaces;
using RobotSim.Data.DTOs;
using RobotSim.Components;

namespace RobotSim.Sensors
{
    /// <summary>
    /// Менеджер датчиков робота
    /// Позволяет динамически регистрировать и управлять датчиками
    /// </summary>
    public class SensorManager
    {
        private Dictionary<string, ISensor> _sensors = new();

        /// <summary>
        /// Зарегистрировать датчик
        /// </summary>
        public void RegisterSensor(ISensor sensor)
        {
            if (sensor == null)
            {
                Debug.LogError("[SensorManager] Попытка регистрации null датчика");
                return;
            }

            _sensors[sensor.SensorId] = sensor;
            sensor.Initialize();
            Debug.Log($"[SensorManager] Датчик '{sensor.SensorId}' зарегистрирован");
        }

        /// <summary>
        /// Отменить регистрацию датчика
        /// </summary>
        public void UnregisterSensor(string sensorId)
        {
            if (_sensors.Remove(sensorId))
            {
                Debug.Log($"[SensorManager] Датчик '{sensorId}' удален");
            }
        }

        /// <summary>
        /// Получить датчик по ID
        /// </summary>
        public ISensor GetSensor(string sensorId)
        {
            return _sensors.TryGetValue(sensorId, out var sensor) ? sensor : null;
        }

        /// <summary>
        /// Получить значение конкретного датчика
        /// </summary>
        public object GetSensorValue(string sensorId)
        {
            var sensor = GetSensor(sensorId);
            return sensor?.GetValue();
        }

        /// <summary>
        /// Получить значение датчика как float (с fallback)
        /// </summary>
        public float GetSensorValueAsFloat(string sensorId, float defaultValue = 0f)
        {
            var value = GetSensorValue(sensorId);
            if (value is float f)
                return f;
            if (value is int i)
                return i;
            return defaultValue;
        }

        /// <summary>
        /// Собрать данные всех датчиков
        /// </summary>
        public Dictionary<string, object> CollectAllSensorData()
        {
            var data = new Dictionary<string, object>();
            foreach (var (sensorId, sensor) in _sensors)
            {
                data[sensorId] = sensor.GetValue();
            }
            return data;
        }

        /// <summary>
        /// Собрать основные данные датчиков в SensorDataDTO
        /// </summary>
        public SensorDataDTO Collect(RobotBody body, int tick)
        {
            var allData = CollectAllSensorData();

            return new SensorDataDTO
            {
                distanceFront = GetSensorValueAsFloat("LaserDistance", 9999f),
                currentSpeed = body != null ? body.CurrentSpeed : 0f,
                tick = tick,
                dt = Time.fixedDeltaTime,
                allSensorsData = allData
            };
        }

        /// <summary>
        /// Получить количество зарегистрированных датчиков
        /// </summary>
        public int GetSensorCount() => _sensors.Count;

        /// <summary>
        /// Получить список всех датчиков
        /// </summary>
        public IEnumerable<string> GetAllSensorIds() => _sensors.Keys;
    }
}
