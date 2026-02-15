using System;
using UnityEngine;

namespace RobotSim.Levels.Components
{
    /// <summary>
    /// Trigger helper placed on GroundWithBounds perimeter colliders.
    /// Emits collider enter events to bootstrap attempt logic.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class GroundPerimeterTrigger : MonoBehaviour
    {
        public event Action<Collider> TriggerEntered;

        private void Awake()
        {
            EnsureTriggerCollider();
        }

        private void Reset()
        {
            EnsureTriggerCollider();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                return;
            }

            TriggerEntered?.Invoke(other);
        }

        private void EnsureTriggerCollider()
        {
            Collider colliderComponent = GetComponent<Collider>();
            if (colliderComponent != null)
            {
                colliderComponent.isTrigger = true;
            }
        }
    }
}
