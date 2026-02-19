using System;
using RobotSim.Bootstrap.Attempts;
using RobotSim.Bootstrap.Data;
using RobotSim.Levels.Components;
using RobotSim.Levels.Data;
using RobotSim.Robot.Data.Results;
using UnityEngine;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Wires runtime references required for attempt execution.
    /// </summary>
    public sealed class AttemptRuntimeBindingService
    {
        public bool TryBind(
            RuntimeAttemptSceneHandle sceneHandle,
            LevelGrid grid,
            GameObject robotInstance,
            AttemptController attemptController,
            out RuntimeBindingHandle bindingHandle,
            out string error)
        {
            bindingHandle = null;
            error = string.Empty;

            if (sceneHandle == null)
            {
                error = "Runtime scene handle is missing.";
                return false;
            }

            if (grid == null)
            {
                error = "Level grid is missing.";
                return false;
            }

            if (robotInstance == null)
            {
                error = "Robot instance is missing.";
                return false;
            }

            if (attemptController == null)
            {
                error = "Attempt controller is missing.";
                return false;
            }

            Transform robotTransform = robotInstance.transform;
            if (robotTransform == null)
            {
                error = "Robot transform is missing.";
                return false;
            }

            var terminalEvaluator = new AttemptTerminalConditionEvaluator(grid, attemptController);
            GroundWithBounds groundWithBounds = null;

            Action detachAction = () => { };
            if (sceneHandle.LevelRoot != null)
            {
                groundWithBounds = sceneHandle.LevelRoot.GetComponentInChildren<GroundWithBounds>(true);
                if (groundWithBounds != null)
                {
                    void OnPerimeterTriggerEntered(Collider other)
                    {
                        if (attemptController == null || !attemptController.IsRunning)
                        {
                            return;
                        }

                        if (other == null || other.transform == null)
                        {
                            return;
                        }

                        if (!other.transform.IsChildOf(robotTransform))
                        {
                            return;
                        }

                        if (grid.IsWithinBounds(robotTransform.position.x, robotTransform.position.z))
                        {
                            return;
                        }

                        attemptController.TryCompleteFail(
                            FailureType.OutOfBounds,
                            "Robot left level bounds (perimeter trigger).");
                    }

                    groundWithBounds.PerimeterTriggerEntered += OnPerimeterTriggerEntered;
                    detachAction = () => groundWithBounds.PerimeterTriggerEntered -= OnPerimeterTriggerEntered;
                }
            }

            bindingHandle = new RuntimeBindingHandle(
                robotTransform,
                terminalEvaluator,
                groundWithBounds,
                detachAction);

            return true;
        }
    }
}
