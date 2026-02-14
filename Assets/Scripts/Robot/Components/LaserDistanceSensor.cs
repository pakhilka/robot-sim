using UnityEngine;
using RobotSim.Robot.Interfaces;

namespace RobotSim.Robot.Components
{
    /// <summary>
    /// MonoBehaviour компонент - лазерный датчик расстояния
    /// </summary>
    [DisallowMultipleComponent]
    public class LaserDistanceSensor : MonoBehaviour, ISensor
    {
        [Header("Sensor")]
        [SerializeField]
        private string _sensorId = "LaserDistance";

        [Header("Raycast")]
        [SerializeField]
        [Tooltip("Максимальная дистанция проверки вперёд")]
        private float _maxDistance = 100f;

        [SerializeField]
        [Tooltip("Слои, которые считаем препятствиями")]
        private LayerMask _obstacleMask = ~0;

        public string SensorId => _sensorId;

        public void Initialize()
        {
            // Инициализация если нужна
        }

        public object GetValue()
        {
            return GetDistance();
        }

        /// <summary>
        /// Возвращает расстояние до ближайшего препятствия впереди.
        /// Если ничего не нашли – возвращаем maxDistance.
        /// </summary>
        public float GetDistance()
        {
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _maxDistance, _obstacleMask))
            {
                return hit.distance;
            }

            return _maxDistance;
        }

        // Для наглядности – рисуем луч в редакторе
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;
            Gizmos.DrawLine(origin, origin + direction * _maxDistance);
        }

        private void OnValidate()
        {
            if (_maxDistance < 0.01f)
            {
                _maxDistance = 0.01f;
            }
        }
    }
}
