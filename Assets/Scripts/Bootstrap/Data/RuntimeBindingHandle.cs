using System;
using RobotSim.Bootstrap.Attempts;
using RobotSim.Levels.Components;
using UnityEngine;

namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Runtime wiring handle created once per attempt and detached on teardown.
    /// </summary>
    public sealed class RuntimeBindingHandle
    {
        private Action _detachAction;

        public RuntimeBindingHandle(
            Transform robotTransform,
            AttemptTerminalConditionEvaluator terminalEvaluator,
            GroundWithBounds groundWithBounds,
            Action detachAction)
        {
            RobotTransform = robotTransform;
            TerminalEvaluator = terminalEvaluator;
            GroundWithBounds = groundWithBounds;
            _detachAction = detachAction;
        }

        public Transform RobotTransform { get; }
        public AttemptTerminalConditionEvaluator TerminalEvaluator { get; }
        public GroundWithBounds GroundWithBounds { get; }

        public void Detach()
        {
            if (_detachAction == null)
            {
                return;
            }

            Action detach = _detachAction;
            _detachAction = null;
            detach.Invoke();
        }
    }
}
