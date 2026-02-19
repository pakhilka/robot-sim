using System.Collections;
using RobotSim.Bootstrap.Data;
using RobotSim.Bootstrap.Interfaces;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Executes safe, idempotent cleanup for one run attempt.
    /// </summary>
    public sealed class AttemptTeardownService
    {
        public IEnumerator Teardown(
            RunAttemptRuntimeState runtimeState,
            RuntimeSceneService runtimeSceneService,
            IAttemptVideoRecorder videoRecorder)
        {
            if (runtimeState == null)
            {
                yield break;
            }

            runtimeState.RuntimeBinding?.Detach();

            if (videoRecorder != null && videoRecorder.IsCapturing)
            {
                float elapsedSeconds = runtimeState.AttemptController != null
                    ? runtimeState.AttemptController.ElapsedSeconds
                    : 0f;
                videoRecorder.TryStopAndEncode(elapsedSeconds, out _);
            }

            if (runtimeState.SceneHandle != null && runtimeSceneService != null)
            {
                IEnumerator unloadEnumerator = runtimeSceneService.Unload(runtimeState.SceneHandle);
                if (unloadEnumerator != null)
                {
                    while (unloadEnumerator.MoveNext())
                    {
                        yield return unloadEnumerator.Current;
                    }
                }
            }

            runtimeState.ClearRuntime();
        }
    }
}
