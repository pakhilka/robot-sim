using UnityEngine;
using RobotSim.Robot.Brains;
using RobotSim.Robot.Sensors;
using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Robot.Components
{
    /// <summary>
    /// MonoBehaviour компонент - низкоуровневый контроллер робота
    /// Единственный компонент в сцене, управляет физикой, сенсорами и оркестрирует контроллер
    /// </summary>
    [DisallowMultipleComponent]
    public class RobotBrain : MonoBehaviour
    {
        [Header("Physical Components")]
        [SerializeField]
        private RobotBody _body;

        [SerializeField]
        private LaserDistanceSensor _laserSensor;

        [Header("Brain Configuration")]
        [SerializeField]
        private BrainType _brainType = BrainType.LocalMock;

        [SerializeField]
        private string _tcpHost = "127.0.0.1";

        [SerializeField]
        private int _tcpPort = 9999;

        private SensorManager _sensorManager;
        private RobotController _robotController;
        private int _tick;

        private void Awake()
        {
            // Инициализируем сенсоры
            _sensorManager = new SensorManager();
            if (_laserSensor != null)
            {
                _sensorManager.RegisterSensor(_laserSensor);
            }

            // Создаем контроллер (выбор и инициализация мозга)
            _robotController = new RobotController(_brainType, _tcpHost, _tcpPort);

            Debug.Log($"[RobotBrain] Инициализирован с мозгом {_brainType}");
        }

        private void FixedUpdate()
        {
            if (_body == null || _robotController == null)
            {
                return;
            }

            _tick++;

            // Собираем данные сенсоров
            SensorDataDTO sensorData = _sensorManager.Collect(_body, _tick);

            // Мозг принимает решение
            MotorCommandDTO command = _robotController.Tick(sensorData);

            // Исполняем команду на теле
            _body.SetMotors(command.left, command.right);
        }

        private void Reset()
        {
            _body = GetComponent<RobotBody>();
            _laserSensor = GetComponentInChildren<LaserDistanceSensor>();
        }

        private void OnValidate()
        {
            if (_tcpPort < 0)
            {
                _tcpPort = 0;
            }

            if (string.IsNullOrWhiteSpace(_tcpHost))
            {
                _tcpHost = "127.0.0.1";
            }
        }
    }
}
