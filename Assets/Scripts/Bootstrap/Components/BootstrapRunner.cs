using System;
using System.Collections;
using System.IO;
using RobotSim.Bootstrap.Attempts;
using RobotSim.Bootstrap.Data;
using RobotSim.Bootstrap.Interfaces;
using RobotSim.Bootstrap.Services;
using RobotSim.Levels.Components;
using RobotSim.Levels.Data;
using RobotSim.Levels.Generation;
using RobotSim.Levels.Interfaces;
using RobotSim.Robot.Data.DTOs;
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
    /// 4) Runner can already load request JSON from CLI (-request <path>).
    /// Final end-to-end orchestration is implemented in next tasks.
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
        [Tooltip("Auto-run orchestration on scene start (CLI mode).")]
        private bool _autoRunOnStart = true;

        private RuntimeAttemptSceneService _runtimeSceneService;
        private SocketConnectionProbe _socketConnectionProbe;
        private CommandLineRequestLoader _commandLineRequestLoader;
        private AttemptArtifactsService _attemptArtifactsService;
        private IAttemptVideoRecorder _attemptVideoRecorder;
        private ILevelPrefabProvider _prefabProvider;
        private AttemptController _activeAttemptController;
        private AttemptTerminalConditionEvaluator _terminalConditionEvaluator;
        private LevelGrid _activeLevelGrid;
        private Transform _activeRobotTransform;
        private GroundWithBounds _activeGroundWithBounds;

        public AttemptController ActiveAttemptController => _activeAttemptController;

        private void Awake()
        {
            _runtimeSceneService = new RuntimeAttemptSceneService();
            _socketConnectionProbe = new SocketConnectionProbe();
            _commandLineRequestLoader = new CommandLineRequestLoader();
            _attemptArtifactsService = new AttemptArtifactsService();
            _attemptVideoRecorder = new PngFrameCaptureVideoRecorder();
            _prefabProvider = _levelPrefabProviderBehaviour as ILevelPrefabProvider;
        }

        private void Update()
        {
            if (_attemptVideoRecorder != null && _attemptVideoRecorder.IsCapturing)
            {
                if (!_attemptVideoRecorder.TryCaptureFrame(out string captureError))
                {
                    Debug.LogError($"[BootstrapRunner] Video frame capture failed: {captureError}");
                }
            }

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

        private IEnumerator Start()
        {
            if (!_autoRunOnStart)
            {
                yield break;
            }

            yield return RunFromCommandLineCoroutine();
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
            RuntimeAttemptSceneHandle sceneHandle = null;
            AttemptArtifactsLayout artifactsLayout = default;
            bool hasArtifactsLayout = false;
            LevelRunRequestDTO request = default;

            if (!TryLoadRequestFromCommandLine(out request, out string requestPath, out string loadError))
            {
                if (TryCreateAttemptArtifactsLayout("invalid-request", out artifactsLayout, out string layoutError))
                {
                    hasArtifactsLayout = true;
                    AttemptArtifactsDTO artifactsDto = BuildArtifactsDto(artifactsLayout);
                    LevelRunResultDTO invalidInputResult = BuildFailResult(
                        "invalid-request",
                        FailureType.InvalidInput,
                        loadError,
                        0f,
                        artifactsDto);
                    TryWriteResultJson(invalidInputResult, artifactsLayout, out _);
                }
                else
                {
                    Debug.LogError($"[BootstrapRunner] Failed to create artifacts layout for invalid input: {layoutError}");
                }

                Debug.LogError($"[BootstrapRunner] CLI request load failed: {loadError}");
                yield break;
            }

            if (!TryCreateAttemptArtifactsLayout(request.name, out artifactsLayout, out string artifactsError))
            {
                Debug.LogError($"[BootstrapRunner] Failed to create artifacts layout: {artifactsError}");
                yield break;
            }
            hasArtifactsLayout = true;

            if (!TryCopyRequestJsonToArtifacts(requestPath, artifactsLayout, out string copyRequestError))
            {
                LevelRunResultDTO copyFailureResult = BuildFailResult(
                    request.name,
                    FailureType.Error,
                    copyRequestError,
                    0f,
                    BuildArtifactsDto(artifactsLayout));
                TryWriteResultJson(copyFailureResult, artifactsLayout, out _);
                Debug.LogError($"[BootstrapRunner] Request copy failed: {copyRequestError}");
                yield break;
            }

            if (!TryValidateRequest(request, out string requestValidationError))
            {
                LevelRunResultDTO invalidRequestResult = BuildFailResult(
                    request.name,
                    FailureType.InvalidInput,
                    requestValidationError,
                    0f,
                    BuildArtifactsDto(artifactsLayout));
                TryWriteResultJson(invalidRequestResult, artifactsLayout, out _);
                Debug.LogError($"[BootstrapRunner] Request validation failed: {requestValidationError}");
                yield break;
            }

            if (!TryCheckSocketConnection(request.socketAddress, out string socketError))
            {
                LevelRunResultDTO connectionFailureResult = BuildFailResult(
                    request.name,
                    FailureType.Connection,
                    socketError,
                    0f,
                    BuildArtifactsDto(artifactsLayout));
                TryWriteResultJson(connectionFailureResult, artifactsLayout, out _);
                Debug.LogError($"[BootstrapRunner] Socket pre-check failed: {socketError}");
                yield break;
            }

            if (!LevelGridFactory.TryCreate(request, out LevelGrid grid, out string gridError))
            {
                LevelRunResultDTO gridFailureResult = BuildFailResult(
                    request.name,
                    FailureType.InvalidInput,
                    gridError,
                    0f,
                    BuildArtifactsDto(artifactsLayout));
                TryWriteResultJson(gridFailureResult, artifactsLayout, out _);
                Debug.LogError($"[BootstrapRunner] Level grid creation failed: {gridError}");
                yield break;
            }

            if (_prefabProvider == null || _robotPrefab == null)
            {
                LevelRunResultDTO wiringFailureResult = BuildFailResult(
                    request.name,
                    FailureType.Error,
                    "Bootstrap wiring is incomplete: LevelPrefabProvider or Robot Prefab is missing.",
                    0f,
                    BuildArtifactsDto(artifactsLayout));
                TryWriteResultJson(wiringFailureResult, artifactsLayout, out _);
                Debug.LogError("[BootstrapRunner] Missing LevelPrefabProvider or Robot Prefab.");
                yield break;
            }

            sceneHandle = CreateAttemptScene(request.name);
            SpawnLevel(sceneHandle, grid);
            GameObject robot = SpawnRobot(sceneHandle, grid, request.startRotationDegrees);
            if (robot == null)
            {
                LevelRunResultDTO robotFailureResult = BuildFailResult(
                    request.name,
                    FailureType.Error,
                    "Failed to spawn robot.",
                    0f,
                    BuildArtifactsDto(artifactsLayout));
                TryWriteResultJson(robotFailureResult, artifactsLayout, out _);
                Debug.LogError("[BootstrapRunner] Robot spawn failed.");
                yield return UnloadAttemptScene(sceneHandle);
                yield break;
            }

            AttemptController attempt = StartAttemptLifecycle(request.levelCompletionLimitSeconds);
            AttachAttemptRuntime(grid, robot);
            AttachGroundWithBoundsRuntime(sceneHandle);
            var videoRequest = new AttemptVideoRecorderRequest(
                artifactsLayout.FramesPath,
                artifactsLayout.VideoPath,
                Screen.width,
                Screen.height);

            if (!TryStartVideoRecording(videoRequest, out string videoStartError))
            {
                attempt.ForceFail(FailureType.VideoError, videoStartError);
            }

            while (attempt.IsRunning)
            {
                yield return null;
            }

            TryStopVideoRecording(attempt.ElapsedSeconds, out _);

            if (hasArtifactsLayout)
            {
                LevelRunResultDTO result = BuildResultFromAttempt(request.name, attempt, BuildArtifactsDto(artifactsLayout));
                if (!TryWriteResultJson(result, artifactsLayout, out string writeError))
                {
                    Debug.LogError($"[BootstrapRunner] Failed to write result.json: {writeError}");
                }
            }

            if (sceneHandle != null)
            {
                yield return UnloadAttemptScene(sceneHandle);
            }
        }

        public AttemptController StartAttemptLifecycle(float timeLimitSeconds)
        {
            DetachGroundWithBoundsRuntime();
            _activeAttemptController = new AttemptController(timeLimitSeconds);
            _activeAttemptController.Start();
            _terminalConditionEvaluator = null;
            _activeLevelGrid = null;
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
                _activeLevelGrid = null;
                _activeRobotTransform = null;
                return;
            }

            _activeLevelGrid = grid;
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
            if (_activeGroundWithBounds != null && handle != null && _activeGroundWithBounds.gameObject.scene == handle.Scene)
            {
                DetachGroundWithBoundsRuntime();
            }

            if (_activeRobotTransform != null && handle != null && _activeRobotTransform.gameObject.scene == handle.Scene)
            {
                _activeLevelGrid = null;
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

        public bool TryLoadRequestFromCommandLine(
            out LevelRunRequestDTO request,
            out string requestPath,
            out string error)
        {
            return _commandLineRequestLoader.TryLoadFromCommandLine(
                out request,
                out requestPath,
                out error);
        }

        public bool TryCreateAttemptArtifactsLayout(
            string levelName,
            out AttemptArtifactsLayout layout,
            out string error)
        {
            return _attemptArtifactsService.TryCreateLayout(
                levelName,
                out layout,
                out error);
        }

        public void SetAttemptVideoRecorder(IAttemptVideoRecorder recorder)
        {
            _attemptVideoRecorder = recorder ?? new NoopAttemptVideoRecorder();
        }

        public bool TryStartVideoRecording(AttemptVideoRecorderRequest request, out string error)
        {
            return _attemptVideoRecorder.TryStartCapture(request, out error);
        }

        public bool TryCaptureVideoFrame(out string error)
        {
            return _attemptVideoRecorder.TryCaptureFrame(out error);
        }

        public bool TryStopVideoRecording(float attemptDurationSeconds, out AttemptVideoRecorderResult result)
        {
            bool succeeded = _attemptVideoRecorder.TryStopAndEncode(attemptDurationSeconds, out result);
            if (!succeeded && _activeAttemptController != null)
            {
                string reason = string.IsNullOrWhiteSpace(result.Error)
                    ? "Video recording failed."
                    : result.Error;
                _activeAttemptController.ForceFail(FailureType.VideoError, reason);
            }

            return succeeded;
        }

        public bool TryCopyRequestJsonToArtifacts(
            string sourceRequestPath,
            AttemptArtifactsLayout layout,
            out string error)
        {
            return _attemptArtifactsService.TryCopyRequestJson(
                sourceRequestPath,
                layout,
                out error);
        }

        public bool TryWriteResultJson(
            LevelRunResultDTO result,
            AttemptArtifactsLayout layout,
            out string error)
        {
            return _attemptArtifactsService.TryWriteResultJson(
                result,
                layout,
                out error);
        }

        private void AttachGroundWithBoundsRuntime(RuntimeAttemptSceneHandle handle)
        {
            DetachGroundWithBoundsRuntime();

            if (handle?.LevelRoot == null)
            {
                return;
            }

            _activeGroundWithBounds = handle.LevelRoot.GetComponentInChildren<GroundWithBounds>(true);
            if (_activeGroundWithBounds == null)
            {
                return;
            }

            _activeGroundWithBounds.PerimeterTriggerEntered += OnPerimeterTriggerEntered;
        }

        private void DetachGroundWithBoundsRuntime()
        {
            if (_activeGroundWithBounds != null)
            {
                _activeGroundWithBounds.PerimeterTriggerEntered -= OnPerimeterTriggerEntered;
                _activeGroundWithBounds = null;
            }
        }

        private void OnPerimeterTriggerEntered(Collider other)
        {
            if (_activeAttemptController == null || !_activeAttemptController.IsRunning)
            {
                return;
            }

            if (_activeRobotTransform == null || other == null)
            {
                return;
            }

            if (!other.transform.IsChildOf(_activeRobotTransform))
            {
                return;
            }

            if (_activeLevelGrid != null &&
                _activeLevelGrid.IsWithinBounds(_activeRobotTransform.position.x, _activeRobotTransform.position.z))
            {
                return;
            }

            _activeAttemptController.TryCompleteFail(
                FailureType.OutOfBounds,
                "Robot left level bounds (perimeter trigger).");
        }

        private static bool TryValidateRequest(LevelRunRequestDTO request, out string error)
        {
            if (string.IsNullOrWhiteSpace(request.name))
            {
                error = "Request field 'name' is missing.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.socketAddress))
            {
                error = "Request field 'socketAddress' is missing.";
                return false;
            }

            if (request.levelCompletionLimitSeconds <= 0)
            {
                error = "Request field 'levelCompletionLimitSeconds' must be > 0.";
                return false;
            }

            if (request.map == null || request.map.Length == 0)
            {
                error = "Request field 'map' is missing or empty.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static LevelRunResultDTO BuildResultFromAttempt(
            string levelName,
            AttemptController attempt,
            AttemptArtifactsDTO artifacts)
        {
            if (attempt == null)
            {
                return BuildFailResult(levelName, FailureType.Error, "Attempt controller is missing.", 0f, artifacts);
            }

            string status = string.IsNullOrWhiteSpace(attempt.Status) ? "fail" : attempt.Status;
            FailureType failureType = status == "pass" ? FailureType.None : attempt.FailureType;

            return new LevelRunResultDTO(
                levelName,
                status,
                failureType,
                attempt.Reason ?? string.Empty,
                attempt.ElapsedSeconds,
                artifacts);
        }

        private static LevelRunResultDTO BuildFailResult(
            string levelName,
            FailureType failureType,
            string reason,
            float durationSeconds,
            AttemptArtifactsDTO artifacts)
        {
            return new LevelRunResultDTO(
                levelName,
                "fail",
                failureType,
                reason ?? string.Empty,
                durationSeconds,
                artifacts);
        }

        private static AttemptArtifactsDTO BuildArtifactsDto(AttemptArtifactsLayout layout)
        {
            return new AttemptArtifactsDTO
            {
                request = ToProjectRelativePath(layout.ProjectRootPath, layout.RequestPath),
                result = ToProjectRelativePath(layout.ProjectRootPath, layout.ResultPath),
                video = ToProjectRelativePath(layout.ProjectRootPath, layout.VideoPath)
            };
        }

        private static string ToProjectRelativePath(string projectRootPath, string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(projectRootPath) || string.IsNullOrWhiteSpace(absolutePath))
            {
                return string.Empty;
            }

            try
            {
                string root = AppendDirectorySeparator(Path.GetFullPath(projectRootPath));
                string full = Path.GetFullPath(absolutePath);

                Uri rootUri = new(root);
                Uri fileUri = new(full);
                string relative = Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString());
                return relative.Replace('/', Path.DirectorySeparatorChar);
            }
            catch
            {
                return absolutePath;
            }
        }

        private static string AppendDirectorySeparator(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return path.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? path
                : path + Path.DirectorySeparatorChar;
        }
    }
}
