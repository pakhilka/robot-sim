using UnityEngine;
using UnityEngine.SceneManagement;

namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Holds runtime scene objects for a single level attempt.
    /// </summary>
    public sealed class RuntimeAttemptSceneHandle
    {
        public RuntimeAttemptSceneHandle(
            string sceneName,
            Scene scene,
            GameObject sceneRoot,
            GameObject levelRoot,
            GameObject robotRoot)
        {
            SceneName = sceneName;
            Scene = scene;
            SceneRoot = sceneRoot;
            LevelRoot = levelRoot;
            RobotRoot = robotRoot;
        }

        public string SceneName { get; }
        public Scene Scene { get; }
        public GameObject SceneRoot { get; }
        public GameObject LevelRoot { get; }
        public GameObject RobotRoot { get; }
    }
}
