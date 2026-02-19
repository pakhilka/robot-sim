using System.Collections;
using RobotSim.Bootstrap.Attempts;
using RobotSim.Bootstrap.Data;
using RobotSim.Bootstrap.Interfaces;
using RobotSim.Bootstrap.Services;
using RobotSim.Levels.Interfaces;
using UnityEngine;

namespace RobotSim.Bootstrap.Components
{
    /// <summary>
    /// Thin Unity facade that delegates run orchestration to RunAttemptUseCase.
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

        [SerializeField]
        [Tooltip("Skip pre-start socket connectivity check (useful for local mock brain development).")]
        private bool _skipSocketPreCheck;

        [SerializeField]
        [Tooltip("Auto-run orchestration on scene start (CLI mode).")]
        private bool _autoRunOnStart = true;

        [Header("Editor Local Validation (Fallback)")]
        [SerializeField]
        [Tooltip("Enable local request fallback in Unity Editor when CLI '-request <path>' is not provided.")]
        private bool _useEditorRequestFallback;

        [SerializeField]
        [Tooltip("Optional local request file path used only for editor fallback mode.")]
        private string _editorRequestPath = string.Empty;

        [SerializeField]
        [Tooltip("Optional output folder for editor fallback debug files (last-request.json, last-result.json). Relative paths are resolved from project root.")]
        private string _editorDebugOutputPath = string.Empty;

        private ILevelPrefabProvider _prefabProvider;
        private RunAttemptUseCase _runAttemptUseCase;
        private IAttemptVideoRecorder _pendingVideoRecorder;

        public AttemptController ActiveAttemptController => _runAttemptUseCase?.ActiveAttemptController;

        private void Awake()
        {
            _prefabProvider = _levelPrefabProviderBehaviour as ILevelPrefabProvider;

            var config = new RunAttemptUseCaseConfig(
                _prefabProvider,
                _robotPrefab,
                _socketConnectTimeoutMilliseconds,
                _skipSocketPreCheck,
                new RequestLoadingOptions(_useEditorRequestFallback, _editorRequestPath),
                _editorDebugOutputPath);

            _runAttemptUseCase = new RunAttemptUseCase(config);

            if (_pendingVideoRecorder != null)
            {
                _runAttemptUseCase.SetVideoRecorder(_pendingVideoRecorder);
                _pendingVideoRecorder = null;
            }
        }

        private IEnumerator Start()
        {
            if (!_autoRunOnStart || _runAttemptUseCase == null)
            {
                yield break;
            }

            yield return _runAttemptUseCase.RunCoroutine();
        }

        private void Update()
        {
            _runAttemptUseCase?.Tick(Time.deltaTime);
        }

        private void OnValidate()
        {
            if (_socketConnectTimeoutMilliseconds < 100)
            {
                _socketConnectTimeoutMilliseconds = 100;
            }
        }

        public IEnumerator RunFromCommandLineCoroutine()
        {
            if (_runAttemptUseCase == null)
            {
                yield break;
            }

            yield return _runAttemptUseCase.RunCoroutine();
        }

        public void SetAttemptVideoRecorder(IAttemptVideoRecorder recorder)
        {
            if (_runAttemptUseCase == null)
            {
                _pendingVideoRecorder = recorder;
                return;
            }

            _runAttemptUseCase.SetVideoRecorder(recorder);
        }
    }
}
