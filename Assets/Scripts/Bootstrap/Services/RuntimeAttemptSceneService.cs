using System;
using System.Collections;
using System.Text.RegularExpressions;
using RobotSim.Bootstrap.Data;
using RobotSim.Levels.Data;
using RobotSim.Levels.Interfaces;
using RobotSim.Levels.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Unity adapter for per-request runtime scene lifecycle.
    /// </summary>
    public sealed class RuntimeAttemptSceneService
    {
        private const string DefaultSceneNamePrefix = "attempt";

        private readonly LevelPrefabSpawner _levelPrefabSpawner = new();

        public RuntimeAttemptSceneHandle CreateAttemptScene(string requestName)
        {
            string sceneName = BuildSceneName(requestName);
            Scene attemptScene = SceneManager.CreateScene(
                sceneName,
                new CreateSceneParameters(LocalPhysicsMode.Physics3D));

            GameObject root = new("AttemptRoot");
            SceneManager.MoveGameObjectToScene(root, attemptScene);

            GameObject levelRoot = new("LevelRoot");
            levelRoot.transform.SetParent(root.transform, false);

            GameObject robotRoot = new("RobotRoot");
            robotRoot.transform.SetParent(root.transform, false);

            return new RuntimeAttemptSceneHandle(
                sceneName,
                attemptScene,
                root,
                levelRoot,
                robotRoot);
        }

        public int SpawnLevel(
            RuntimeAttemptSceneHandle handle,
            LevelGrid grid,
            ILevelPrefabProvider prefabProvider,
            float y = 0f)
        {
            if (handle == null || grid == null || prefabProvider == null)
            {
                return 0;
            }

            return _levelPrefabSpawner.Spawn(
                grid,
                prefabProvider,
                handle.LevelRoot.transform,
                y);
        }

        public GameObject SpawnRobot(
            RuntimeAttemptSceneHandle handle,
            GameObject robotPrefab,
            LevelGrid grid,
            float startRotationDegrees,
            float y = 0f)
        {
            if (handle == null || robotPrefab == null || grid == null)
            {
                return null;
            }

            (float x, float z) = grid.GetCellCenter(grid.StartRow, grid.StartCol);
            Quaternion rotation = Quaternion.Euler(0f, startRotationDegrees, 0f);

            GameObject robot = UnityEngine.Object.Instantiate(
                robotPrefab,
                new Vector3(x, y, z),
                rotation,
                handle.RobotRoot.transform);

            if (robot.scene != handle.Scene)
            {
                SceneManager.MoveGameObjectToScene(robot, handle.Scene);
            }

            return robot;
        }

        public IEnumerator UnloadAttemptSceneCoroutine(RuntimeAttemptSceneHandle handle)
        {
            if (handle == null || !handle.Scene.isLoaded)
            {
                yield break;
            }

            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(handle.Scene);
            if (unloadOperation == null)
            {
                yield break;
            }

            while (!unloadOperation.isDone)
            {
                yield return null;
            }
        }

        public static string BuildSceneName(string requestName)
        {
            string candidate = string.IsNullOrWhiteSpace(requestName)
                ? DefaultSceneNamePrefix
                : requestName.Trim().ToLowerInvariant();

            candidate = Regex.Replace(candidate, "[^a-z0-9-]+", "-");
            candidate = candidate.Trim('-');

            if (string.IsNullOrWhiteSpace(candidate))
            {
                candidate = DefaultSceneNamePrefix;
            }

            if (candidate.Length > 40)
            {
                candidate = candidate.Substring(0, 40);
            }

            string sceneName = $"{DefaultSceneNamePrefix}-{candidate}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
            if (sceneName.Length > 64)
            {
                sceneName = sceneName.Substring(0, 64);
            }

            return sceneName;
        }
    }
}
