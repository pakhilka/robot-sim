using RobotSim.Bootstrap.Attempts;
using RobotSim.Levels.Data;
using UnityEngine;

namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Mutable per-attempt state bag passed between orchestration stages.
    /// </summary>
    public sealed class RunAttemptRuntimeState
    {
        public RequestLoadResult LoadedRequest { get; set; }
        public RequestValidationResult Validation { get; set; }
        public LevelPreparationResult Preparation { get; set; }
        public AttemptArtifactsLayout ArtifactsLayout { get; set; }
        public bool HasArtifactsLayout { get; set; }

        public RuntimeAttemptSceneHandle SceneHandle { get; set; }
        public LevelGrid LevelGrid { get; set; }
        public GameObject RobotInstance { get; set; }

        public AttemptController AttemptController { get; set; }
        public RuntimeBindingHandle RuntimeBinding { get; set; }
        public AttemptExecutionSnapshot ExecutionSnapshot { get; set; }

        public void ClearRuntime()
        {
            RuntimeBinding?.Detach();
            RuntimeBinding = null;
            SceneHandle = null;
            LevelGrid = null;
            RobotInstance = null;
            AttemptController = null;
        }
    }
}
