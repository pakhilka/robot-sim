using UnityEngine;
using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Robot.Components
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

        [SerializeField]
        [Tooltip("Максимальная скорость поворота вокруг Y при разнице скоростей моторов.")]
        private float _maxTurnDegreesPerSecond = 180f;

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
            bool pivotTurn = _leftSpeed * _rightSpeed < 0f;
            if (pivotTurn)
            {
                // Для разворота на месте полностью гасим линейную скорость,
                // чтобы робот не продолжал "въезжать" в стену во время поворота.
                _currentSpeed = 0f;
                _rb.linearVelocity = Vector3.zero;
            }
            else
            {
                // Усреднение скоростей
                _currentSpeed = (_leftSpeed + _rightSpeed) / 2f;
                _currentSpeed = Mathf.Clamp(_currentSpeed, -_maxSpeed, _maxSpeed);

                // Применяем поступательную скорость в плоскости XZ (top-down режим).
                Vector3 vel = transform.forward * _currentSpeed;
                _rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);
            }

            // Дифференциальный поворот: разница левого/правого мотора задает скорость поворота.
            float turnInput = 0f;
            if (_maxSpeed > 0f)
            {
                turnInput = (_rightSpeed - _leftSpeed) / (2f * _maxSpeed);
            }

            float yawDelta = turnInput * _maxTurnDegreesPerSecond * Time.fixedDeltaTime;
            if (!Mathf.Approximately(yawDelta, 0f))
            {
                _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0f, yawDelta, 0f));
            }
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

            if (_motorAcceleration < 0f)
            {
                _motorAcceleration = 0f;
            }

            if (_motorBrakingForce < 0f)
            {
                _motorBrakingForce = 0f;
            }

            if (_maxTurnDegreesPerSecond < 0f)
            {
                _maxTurnDegreesPerSecond = 0f;
            }
        }
    }
}
