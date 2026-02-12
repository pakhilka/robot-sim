using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobotSim.Data.DTOs
{
    /// <summary>
    /// DTO - Данные от датчиков робота
    /// </summary>
    [Serializable]
    public struct SensorDataDTO
    {
        // Основные данные
        public float distanceFront;
        public float currentSpeed;
        public int tick;
        public float dt;

        // Расширенные данные от всех датчиков
        [System.NonSerialized]
        public Dictionary<string, object> allSensorsData;
    }
}
