using RobotSim.Bootstrap.Attempts;
using RobotSim.Bootstrap.Data;
using RobotSim.Robot.Data.Results;
using UnityEngine;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Manages attempt lifecycle execution in runtime ticks.
    /// </summary>
    public sealed class AttemptExecutionService
    {
        public AttemptController Start(float timeLimitSeconds)
        {
            var controller = new AttemptController(timeLimitSeconds);
            controller.Start();
            return controller;
        }

        public void Tick(
            AttemptController attemptController,
            RuntimeBindingHandle runtimeBinding,
            float deltaSeconds)
        {
            if (attemptController == null || !attemptController.IsRunning)
            {
                return;
            }

            attemptController.Tick(deltaSeconds);

            if (runtimeBinding?.TerminalEvaluator == null || runtimeBinding.RobotTransform == null)
            {
                return;
            }

            Vector3 robotPosition = runtimeBinding.RobotTransform.position;
            runtimeBinding.TerminalEvaluator.Evaluate(robotPosition.x, robotPosition.z);
        }

        public bool TryCompletePass(AttemptController attemptController, string reason)
        {
            return attemptController != null && attemptController.TryCompletePass(reason);
        }

        public bool TryCompleteFail(
            AttemptController attemptController,
            FailureType failureType,
            string reason)
        {
            return attemptController != null && attemptController.TryCompleteFail(failureType, reason);
        }

        public void ForceFail(
            AttemptController attemptController,
            FailureType failureType,
            string reason)
        {
            if (attemptController == null)
            {
                return;
            }

            attemptController.ForceFail(failureType, reason);
        }

        public AttemptExecutionSnapshot BuildSnapshot(AttemptController attemptController)
        {
            return AttemptExecutionSnapshot.FromController(attemptController);
        }
    }
}
