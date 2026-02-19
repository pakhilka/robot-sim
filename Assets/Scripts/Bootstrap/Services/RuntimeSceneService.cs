using System.Collections;
using RobotSim.Bootstrap.Data;
using RobotSim.Levels.Data;
using RobotSim.Levels.Interfaces;
using UnityEngine;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Facade over runtime attempt scene operations.
    /// </summary>
    public sealed class RuntimeSceneService
    {
        private readonly RuntimeAttemptSceneService _runtimeAttemptSceneService;

        public RuntimeSceneService()
            : this(new RuntimeAttemptSceneService())
        {
        }

        public RuntimeSceneService(RuntimeAttemptSceneService runtimeAttemptSceneService)
        {
            _runtimeAttemptSceneService = runtimeAttemptSceneService ?? new RuntimeAttemptSceneService();
        }

        public RuntimeAttemptSceneHandle Create(string requestName)
        {
            return _runtimeAttemptSceneService.CreateAttemptScene(requestName);
        }

        public int SpawnLevel(
            RuntimeAttemptSceneHandle handle,
            LevelGrid grid,
            ILevelPrefabProvider prefabProvider,
            float y = 0f)
        {
            return _runtimeAttemptSceneService.SpawnLevel(handle, grid, prefabProvider, y);
        }

        public GameObject SpawnRobot(
            RuntimeAttemptSceneHandle handle,
            GameObject robotPrefab,
            LevelGrid grid,
            float startRotationDegrees,
            float y = 0f)
        {
            return _runtimeAttemptSceneService.SpawnRobot(handle, robotPrefab, grid, startRotationDegrees, y);
        }

        public IEnumerator Unload(RuntimeAttemptSceneHandle handle)
        {
            return _runtimeAttemptSceneService.UnloadAttemptSceneCoroutine(handle);
        }
    }
}
