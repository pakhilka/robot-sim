using RobotSim.Levels.Interfaces;
using UnityEngine;

namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Static dependencies and runtime options for RunAttemptUseCase.
    /// </summary>
    public readonly struct RunAttemptUseCaseConfig
    {
        public RunAttemptUseCaseConfig(
            ILevelPrefabProvider prefabProvider,
            GameObject robotPrefab,
            int socketConnectTimeoutMilliseconds,
            bool skipSocketPreCheck,
            RequestLoadingOptions loadingOptions,
            string editorDebugOutputPath)
        {
            PrefabProvider = prefabProvider;
            RobotPrefab = robotPrefab;
            SocketConnectTimeoutMilliseconds = socketConnectTimeoutMilliseconds;
            SkipSocketPreCheck = skipSocketPreCheck;
            LoadingOptions = loadingOptions;
            EditorDebugOutputPath = editorDebugOutputPath ?? string.Empty;
        }

        public ILevelPrefabProvider PrefabProvider { get; }
        public GameObject RobotPrefab { get; }
        public int SocketConnectTimeoutMilliseconds { get; }
        public bool SkipSocketPreCheck { get; }
        public RequestLoadingOptions LoadingOptions { get; }
        public string EditorDebugOutputPath { get; }
    }
}
