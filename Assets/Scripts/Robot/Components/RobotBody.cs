using UnityEngine;
using RobotSim.Data.DTOs;

namespace RobotSim.Components
{
    /// <summary>
    /// MonoBehaviour компонент - физическое тело робота с моторами
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class RobotBody : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private float _maxSpeed = 10f;

        [SerializeField]
        [Tooltip("На будущее: ускорение/замедление, пока не используется")]
        private float _motorAcceleration = 5f;

        [SerializeField]
        [Tooltip("На будущее: тормозное усилие, пока не используется")]
        private float _motorBrakingForce = 10f;

        private Rigidbody _rb;
        private float _leftSpeed;
        private float _rightSpeed;
        private float _currentSpeed;

        public float CurrentSpeed => _currentSpeed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            // Усреднение скоростей
            _currentSpeed = (_leftSpeed + _rightSpeed) / 2f;
            _currentSpeed = Mathf.Clamp(_currentSpeed, -_maxSpeed, _maxSpeed);

            // Применяем скорость вперёд
            Vector3 vel = transform.forward * _currentSpeed;
            Vector3 currentVelocity = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(vel.x, currentVelocity.y, vel.z);
        }

        /// <summary>
        /// Установить скорости левого и правого мотора
        /// </summary>
        public void SetMotors(float left, float right)
        {
            _leftSpeed = Mathf.Clamp(left, -1f, 1f) * _maxSpeed;
            _rightSpeed = Mathf.Clamp(right, -1f, 1f) * _maxSpeed;
        }

        private void OnValidate()
        {
            if (_maxSpeed < 0f)
            {
                _maxSpeed = 0f;
            }
        }
    }
}
