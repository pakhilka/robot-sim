using System.Collections;
using RobotSim.Bootstrap.Attempts;
using RobotSim.Bootstrap.Data;
using RobotSim.Bootstrap.Interfaces;
using RobotSim.Robot.Data.DTOs;
using RobotSim.Robot.Data.Results;
using UnityEngine;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Main orchestration use case for one level-run attempt.
    /// </summary>
    public sealed class RunAttemptUseCase
    {
        private readonly RunAttemptUseCaseConfig _config;
        private readonly RequestLoadingService _requestLoadingService;
        private readonly RequestValidationService _requestValidationService;
        private readonly LevelPreparationService _levelPreparationService;
        private readonly ConnectionProbeService _connectionProbeService;
        private readonly RuntimeSceneService _runtimeSceneService;
        private readonly AttemptRuntimeBindingService _runtimeBindingService;
        private readonly AttemptExecutionService _attemptExecutionService;
        private readonly AttemptVideoService _attemptVideoService;
        private readonly AttemptArtifactsService _attemptArtifactsService;
        private readonly AttemptResultFactory _attemptResultFactory;
        private readonly AttemptTeardownService _attemptTeardownService;

        private RunAttemptRuntimeState _runtimeState;

        public RunAttemptUseCase(RunAttemptUseCaseConfig config)
            : this(
                config,
                new RequestLoadingService(),
                new RequestValidationService(),
                new LevelPreparationService(),
                new ConnectionProbeService(),
                new RuntimeSceneService(),
                new AttemptRuntimeBindingService(),
                new AttemptExecutionService(),
                new AttemptVideoService(),
                new AttemptArtifactsService(),
                new AttemptResultFactory(),
                new AttemptTeardownService())
        {
        }

        public RunAttemptUseCase(
            RunAttemptUseCaseConfig config,
            RequestLoadingService requestLoadingService,
            RequestValidationService requestValidationService,
            LevelPreparationService levelPreparationService,
            ConnectionProbeService connectionProbeService,
            RuntimeSceneService runtimeSceneService,
            AttemptRuntimeBindingService runtimeBindingService,
            AttemptExecutionService attemptExecutionService,
            AttemptVideoService attemptVideoService,
            AttemptArtifactsService attemptArtifactsService,
            AttemptResultFactory attemptResultFactory,
            AttemptTeardownService attemptTeardownService)
        {
            _config = config;
            _requestLoadingService = requestLoadingService ?? new RequestLoadingService();
            _requestValidationService = requestValidationService ?? new RequestValidationService();
            _levelPreparationService = levelPreparationService ?? new LevelPreparationService();
            _connectionProbeService = connectionProbeService ?? new ConnectionProbeService();
            _runtimeSceneService = runtimeSceneService ?? new RuntimeSceneService();
            _runtimeBindingService = runtimeBindingService ?? new AttemptRuntimeBindingService();
            _attemptExecutionService = attemptExecutionService ?? new AttemptExecutionService();
            _attemptVideoService = attemptVideoService ?? new AttemptVideoService();
            _attemptArtifactsService = attemptArtifactsService ?? new AttemptArtifactsService();
            _attemptResultFactory = attemptResultFactory ?? new AttemptResultFactory();
            _attemptTeardownService = attemptTeardownService ?? new AttemptTeardownService();
        }

        public AttemptController ActiveAttemptController => _runtimeState?.AttemptController;

        public void SetVideoRecorder(IAttemptVideoRecorder videoRecorder)
        {
            _attemptVideoService.SetRecorder(videoRecorder);
        }

        public void Tick(float deltaSeconds)
        {
            if (_runtimeState?.AttemptController == null || !_runtimeState.AttemptController.IsRunning)
            {
                return;
            }

            if (_attemptVideoService.IsCapturing &&
                !_attemptVideoService.TryCaptureFrame(_runtimeState.AttemptController, out string captureError))
            {
                Debug.LogError($"[RunAttemptUseCase] Video frame capture failed: {captureError}");
            }

            _attemptExecutionService.Tick(
                _runtimeState.AttemptController,
                _runtimeState.RuntimeBinding,
                deltaSeconds);
        }

        public IEnumerator RunCoroutine()
        {
            _runtimeState = new RunAttemptRuntimeState();

            RequestLoadResult loadResult = _requestLoadingService.Load(_config.LoadingOptions);
            _runtimeState.LoadedRequest = loadResult;
            RequestSource requestSource = loadResult.Source;

            if (!loadResult.Succeeded)
            {
                if (_attemptArtifactsService.TryCreateLayout("invalid-request", out AttemptArtifactsLayout invalidLayout, out string layoutError))
                {
                    _runtimeState.ArtifactsLayout = invalidLayout;
                    _runtimeState.HasArtifactsLayout = true;

                    LevelRunResultDTO failResult = _attemptResultFactory.CreateFail(
                        "invalid-request",
                        FailureType.InvalidInput,
                        loadResult.Error,
                        0f,
                        invalidLayout);
                    WriteResultAndEditorDebug(failResult, requestSource);
                }
                else
                {
                    Debug.LogError($"[RunAttemptUseCase] Failed to create artifacts layout for invalid input: {layoutError}");
                }

                Debug.LogError($"[RunAttemptUseCase] Request load failed: {loadResult.Error}");
                yield break;
            }

            LevelRunRequestDTO request = loadResult.Request;

            if (!_attemptArtifactsService.TryCreateLayout(request.name, out AttemptArtifactsLayout artifactsLayout, out string artifactsError))
            {
                Debug.LogError($"[RunAttemptUseCase] Failed to create artifacts layout: {artifactsError}");
                yield break;
            }

            _runtimeState.ArtifactsLayout = artifactsLayout;
            _runtimeState.HasArtifactsLayout = true;

            if (!_attemptArtifactsService.TryCopyRequestJson(loadResult.RequestPath, artifactsLayout, out string requestCopyError))
            {
                LevelRunResultDTO copyFailureResult = _attemptResultFactory.CreateFail(
                    request.name,
                    FailureType.Error,
                    requestCopyError,
                    0f,
                    artifactsLayout);
                WriteResultAndEditorDebug(copyFailureResult, requestSource);
                Debug.LogError($"[RunAttemptUseCase] Request copy/write failed: {requestCopyError}");
                yield break;
            }

            _attemptArtifactsService.TryWriteEditorDebugRequest(
                request,
                requestSource,
                _config.EditorDebugOutputPath,
                out _);

            RequestValidationResult validationResult = _requestValidationService.Validate(request);
            _runtimeState.Validation = validationResult;
            if (!validationResult.Succeeded)
            {
                LevelRunResultDTO invalidRequestResult = _attemptResultFactory.CreateFail(
                    request.name,
                    FailureType.InvalidInput,
                    validationResult.Error,
                    0f,
                    artifactsLayout);
                WriteResultAndEditorDebug(invalidRequestResult, requestSource);
                Debug.LogError($"[RunAttemptUseCase] Request validation failed: {validationResult.Error}");
                yield break;
            }

            if (!_config.SkipSocketPreCheck)
            {
                if (!_connectionProbeService.TryProbe(
                        request.socketAddress,
                        _config.SocketConnectTimeoutMilliseconds,
                        out string socketError))
                {
                    LevelRunResultDTO connectionFailureResult = _attemptResultFactory.CreateFail(
                        request.name,
                        FailureType.Connection,
                        socketError,
                        0f,
                        artifactsLayout);
                    WriteResultAndEditorDebug(connectionFailureResult, requestSource);
                    Debug.LogError($"[RunAttemptUseCase] Socket pre-check failed: {socketError}");
                    yield break;
                }
            }
            else
            {
                Debug.Log("[RunAttemptUseCase] Socket pre-check skipped by config flag.");
            }

            LevelPreparationResult preparationResult = _levelPreparationService.Prepare(request);
            _runtimeState.Preparation = preparationResult;
            if (!preparationResult.Succeeded)
            {
                LevelRunResultDTO preparationFailureResult = _attemptResultFactory.CreateFail(
                    request.name,
                    FailureType.InvalidInput,
                    preparationResult.Error,
                    0f,
                    artifactsLayout);
                WriteResultAndEditorDebug(preparationFailureResult, requestSource);
                Debug.LogError($"[RunAttemptUseCase] Level preparation failed: {preparationResult.Error}");
                yield break;
            }

            _runtimeState.LevelGrid = preparationResult.Grid;

            if (_config.PrefabProvider == null || _config.RobotPrefab == null)
            {
                LevelRunResultDTO wiringFailureResult = _attemptResultFactory.CreateFail(
                    request.name,
                    FailureType.Error,
                    "Bootstrap wiring is incomplete: LevelPrefabProvider or Robot Prefab is missing.",
                    0f,
                    artifactsLayout);
                WriteResultAndEditorDebug(wiringFailureResult, requestSource);
                Debug.LogError("[RunAttemptUseCase] Missing LevelPrefabProvider or Robot Prefab.");
                yield break;
            }

            _runtimeState.SceneHandle = _runtimeSceneService.Create(request.name);
            _runtimeSceneService.SpawnLevel(_runtimeState.SceneHandle, preparationResult.Grid, _config.PrefabProvider);
            _runtimeState.RobotInstance = _runtimeSceneService.SpawnRobot(
                _runtimeState.SceneHandle,
                _config.RobotPrefab,
                preparationResult.Grid,
                request.startRotationDegrees);

            if (_runtimeState.RobotInstance == null)
            {
                LevelRunResultDTO robotFailureResult = _attemptResultFactory.CreateFail(
                    request.name,
                    FailureType.Error,
                    "Failed to spawn robot.",
                    0f,
                    artifactsLayout);
                WriteResultAndEditorDebug(robotFailureResult, requestSource);
                Debug.LogError("[RunAttemptUseCase] Robot spawn failed.");

                yield return _attemptTeardownService.Teardown(
                    _runtimeState,
                    _runtimeSceneService,
                    null);
                yield break;
            }

            _runtimeState.AttemptController = _attemptExecutionService.Start(request.levelCompletionLimitSeconds);

            if (_runtimeBindingService.TryBind(
                    _runtimeState.SceneHandle,
                    preparationResult.Grid,
                    _runtimeState.RobotInstance,
                    _runtimeState.AttemptController,
                    out RuntimeBindingHandle bindingHandle,
                    out string bindingError))
            {
                _runtimeState.RuntimeBinding = bindingHandle;
            }
            else
            {
                _attemptExecutionService.ForceFail(
                    _runtimeState.AttemptController,
                    FailureType.Error,
                    bindingError);
            }

            var videoRequest = new AttemptVideoRecorderRequest(
                artifactsLayout.FramesPath,
                artifactsLayout.VideoPath,
                Screen.width,
                Screen.height);

            if (_runtimeState.AttemptController != null && _runtimeState.AttemptController.IsRunning)
            {
                _attemptVideoService.TryStart(_runtimeState.AttemptController, videoRequest, out _);
            }

            while (_runtimeState.AttemptController != null && _runtimeState.AttemptController.IsRunning)
            {
                yield return null;
            }

            if (_attemptVideoService.IsCapturing)
            {
                float elapsedSeconds = _runtimeState.AttemptController != null
                    ? _runtimeState.AttemptController.ElapsedSeconds
                    : 0f;
                _attemptVideoService.TryStop(_runtimeState.AttemptController, elapsedSeconds, out _);
            }

            _runtimeState.ExecutionSnapshot = _attemptExecutionService.BuildSnapshot(_runtimeState.AttemptController);

            LevelRunResultDTO result = _attemptResultFactory.CreateFromSnapshot(
                request.name,
                _runtimeState.ExecutionSnapshot,
                artifactsLayout);

            if (!_attemptArtifactsService.TryWriteResultJson(result, artifactsLayout, out string writeError))
            {
                Debug.LogError($"[RunAttemptUseCase] Failed to write result.json: {writeError}");
            }

            _attemptArtifactsService.TryWriteEditorDebugResult(
                result,
                requestSource,
                _config.EditorDebugOutputPath,
                out _);

            yield return _attemptTeardownService.Teardown(
                _runtimeState,
                _runtimeSceneService,
                null);
        }

        private void WriteResultAndEditorDebug(
            LevelRunResultDTO result,
            RequestSource requestSource)
        {
            if (!_runtimeState.HasArtifactsLayout)
            {
                return;
            }

            if (!_attemptArtifactsService.TryWriteResultJson(result, _runtimeState.ArtifactsLayout, out string writeError))
            {
                Debug.LogError($"[RunAttemptUseCase] Failed to write result.json: {writeError}");
            }

            _attemptArtifactsService.TryWriteEditorDebugResult(
                result,
                requestSource,
                _config.EditorDebugOutputPath,
                out _);
        }
    }
}
