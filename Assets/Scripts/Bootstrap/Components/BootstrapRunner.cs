using System.Collections;
using RobotSim.Bootstrap.Attempts;
using RobotSim.Bootstrap.Data;
using RobotSim.Bootstrap.Services;
using RobotSim.Levels.Data;
using RobotSim.Levels.Interfaces;
using RobotSim.Robot.Data.Results;
using UnityEngine;

namespace RobotSim.Bootstrap.Components
{
    /// <summary>
    /// Scene entrypoint for CLI/headless orchestration.
    /// Scene wiring checklist:
    /// 1) Add this component to the Bootstrap scene root object.
    /// 2) Assign Level Prefab Provider with Wall/Start/Finish/GroundWithBounds references.
    /// 3) Assign Robot Prefab used for runtime spawn at Start cell.
    /// Full request loading and attempt execution are implemented in next tasks.
    /// </summary>
    public sealed class BootstrapRunner : MonoBehaviour
    {
        [Header("Scene Wiring")]
        [SerializeField]
        [Tooltip("MonoBehaviour that implements ILevelPrefabProvider (LevelPrefabProvider).")]
        private MonoBehaviour _levelPrefabProviderBehaviour;

        [SerializeField]
        [Tooltip("Robot prefab spawned at Start cell center in each runtime attempt scene.")]
        private GameObject _robotPrefab;

        [SerializeField]
        [Tooltip("Timeout for pre-start socket connectivity check in milliseconds.")]
        private int _socketConnectTimeoutMilliseconds = 3000;

        private RuntimeAttemptSceneService _runtimeSceneService;
        private SocketConnectionProbe _socketConnectionProbe;
        private ILevelPrefabProvider _prefabProvider;
        private AttemptController _activeAttemptController;
        private AttemptTerminalConditionEvaluator _terminalConditionEvaluator;
        private Transform _activeRobotTransform;

        public AttemptController ActiveAttemptController => _activeAttemptController;

        private void Awake()
        {
            _runtimeSceneService = new RuntimeAttemptSceneService();
            _socketConnectionProbe = new SocketConnectionProbe();
            _prefabProvider = _levelPrefabProviderBehaviour as ILevelPrefabProvider;
        }

        private void Update()
        {
            if (_activeAttemptController == null || !_activeAttemptController.IsRunning)
            {
                return;
            }

            _activeAttemptController.Tick(Time.deltaTime);

            if (_terminalConditionEvaluator == null || _activeRobotTransform == null)
            {
                return;
            }

            Vector3 robotPosition = _activeRobotTransform.position;
            _terminalConditionEvaluator.Evaluate(robotPosition.x, robotPosition.z);
        }

        private void OnValidate()
        {
            if (_socketConnectTimeoutMilliseconds < 100)
            {
                _socketConnectTimeoutMilliseconds = 100;
            }
        }

        public AttemptController StartAttemptLifecycle(float timeLimitSeconds)
        {
            _activeAttemptController = new AttemptController(timeLimitSeconds);
            _activeAttemptController.Start();
            _terminalConditionEvaluator = null;
            _activeRobotTransform = null;
            return _activeAttemptController;
        }

        public bool CompleteAttemptPass(string reason)
        {
            if (_activeAttemptController == null)
            {
                return false;
            }

            return _activeAttemptController.TryCompletePass(reason);
        }

        public bool CompleteAttemptFail(FailureType failureType, string reason)
        {
            if (_activeAttemptController == null)
            {
                return false;
            }

            return _activeAttemptController.TryCompleteFail(failureType, reason);
        }

        public void AttachAttemptRuntime(LevelGrid grid, GameObject robotInstance)
        {
            if (_activeAttemptController == null || grid == null || robotInstance == null)
            {
                _terminalConditionEvaluator = null;
                _activeRobotTransform = null;
                return;
            }

            _activeRobotTransform = robotInstance.transform;
            _terminalConditionEvaluator = new AttemptTerminalConditionEvaluator(grid, _activeAttemptController);
        }

        public RuntimeAttemptSceneHandle CreateAttemptScene(string requestName)
        {
            return _runtimeSceneService.CreateAttemptScene(requestName);
        }

        public int SpawnLevel(RuntimeAttemptSceneHandle handle, LevelGrid grid)
        {
            if (_prefabProvider == null)
            {
                Debug.LogError("[BootstrapRunner] LevelPrefabProvider is not assigned or does not implement ILevelPrefabProvider.");
                return 0;
            }

            return _runtimeSceneService.SpawnLevel(handle, grid, _prefabProvider);
        }

        public GameObject SpawnRobot(RuntimeAttemptSceneHandle handle, LevelGrid grid, float startRotationDegrees)
        {
            if (_robotPrefab == null)
            {
                Debug.LogError("[BootstrapRunner] Robot Prefab is not assigned.");
                return null;
            }

            GameObject robot = _runtimeSceneService.SpawnRobot(handle, _robotPrefab, grid, startRotationDegrees);
            if (_activeAttemptController != null && _activeAttemptController.IsRunning)
            {
                AttachAttemptRuntime(grid, robot);
            }

            return robot;
        }

        public IEnumerator UnloadAttemptScene(RuntimeAttemptSceneHandle handle)
        {
            if (_activeRobotTransform != null && handle != null && _activeRobotTransform.gameObject.scene == handle.Scene)
            {
                _activeRobotTransform = null;
                _terminalConditionEvaluator = null;
            }

            return _runtimeSceneService.UnloadAttemptSceneCoroutine(handle);
        }

        public bool TryCheckSocketConnection(string socketAddress, out string error)
        {
            return _socketConnectionProbe.TryConnect(
                socketAddress,
                _socketConnectTimeoutMilliseconds,
                out error);
        }
    }
}
