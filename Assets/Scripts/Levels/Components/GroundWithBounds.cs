using System;
using UnityEngine;

namespace RobotSim.Levels.Components
{
    /// <summary>
    /// Настройка единого префаба пола и периметральных trigger-стен под размер сгенерированного уровня.
    /// Source of truth для out-of-bounds остается в LevelGrid bounds.
    /// </summary>
    public sealed class GroundWithBounds : MonoBehaviour
    {
        [Header("Floor (optional)")]
        [SerializeField]
        private Transform _floorTransform;

        [SerializeField]
        [Min(0.01f)]
        private float _floorBaseSizeX = 10f;

        [SerializeField]
        [Min(0.01f)]
        private float _floorBaseSizeZ = 10f;

        [Header("Perimeter Triggers (required for trigger-based detection)")]
        [SerializeField]
        private BoxCollider _northTrigger;

        [SerializeField]
        private BoxCollider _southTrigger;

        [SerializeField]
        private BoxCollider _eastTrigger;

        [SerializeField]
        private BoxCollider _westTrigger;

        [Header("Trigger Shape")]
        [SerializeField]
        [Min(0.01f)]
        private float _triggerThickness = 1f;

        [SerializeField]
        [Min(0.01f)]
        private float _triggerHeight = 5f;

        private GroundPerimeterTrigger _northPerimeterTrigger;
        private GroundPerimeterTrigger _southPerimeterTrigger;
        private GroundPerimeterTrigger _eastPerimeterTrigger;
        private GroundPerimeterTrigger _westPerimeterTrigger;

        public event Action<Collider> PerimeterTriggerEntered;

        private void Awake()
        {
            WirePerimeterTriggerEvents();
        }

        private void OnDestroy()
        {
            UnwirePerimeterTriggerEvents();
        }

        public void Configure(float worldSizeX, float worldSizeZ)
        {
            if (worldSizeX <= 0f || worldSizeZ <= 0f)
            {
                return;
            }

            ConfigureFloor(worldSizeX, worldSizeZ);
            ConfigurePerimeterTriggers(worldSizeX, worldSizeZ);
        }

        private void ConfigureFloor(float worldSizeX, float worldSizeZ)
        {
            if (_floorTransform == null)
            {
                return;
            }

            Vector3 scale = _floorTransform.localScale;
            scale.x = worldSizeX / _floorBaseSizeX;
            scale.z = worldSizeZ / _floorBaseSizeZ;
            _floorTransform.localScale = scale;

            _floorTransform.localPosition = new Vector3(worldSizeX * 0.5f, _floorTransform.localPosition.y, worldSizeZ * 0.5f);
        }

        private void ConfigurePerimeterTriggers(float worldSizeX, float worldSizeZ)
        {
            if (_northTrigger != null)
            {
                ConfigureTrigger(
                    _northTrigger,
                    new Vector3(worldSizeX * 0.5f, _triggerHeight * 0.5f, worldSizeZ + (_triggerThickness * 0.5f)),
                    new Vector3(worldSizeX + (_triggerThickness * 2f), _triggerHeight, _triggerThickness));
            }

            if (_southTrigger != null)
            {
                ConfigureTrigger(
                    _southTrigger,
                    new Vector3(worldSizeX * 0.5f, _triggerHeight * 0.5f, -(_triggerThickness * 0.5f)),
                    new Vector3(worldSizeX + (_triggerThickness * 2f), _triggerHeight, _triggerThickness));
            }

            if (_eastTrigger != null)
            {
                ConfigureTrigger(
                    _eastTrigger,
                    new Vector3(worldSizeX + (_triggerThickness * 0.5f), _triggerHeight * 0.5f, worldSizeZ * 0.5f),
                    new Vector3(_triggerThickness, _triggerHeight, worldSizeZ + (_triggerThickness * 2f)));
            }

            if (_westTrigger != null)
            {
                ConfigureTrigger(
                    _westTrigger,
                    new Vector3(-(_triggerThickness * 0.5f), _triggerHeight * 0.5f, worldSizeZ * 0.5f),
                    new Vector3(_triggerThickness, _triggerHeight, worldSizeZ + (_triggerThickness * 2f)));
            }

            WirePerimeterTriggerEvents();
        }

        private static void ConfigureTrigger(BoxCollider trigger, Vector3 center, Vector3 size)
        {
            trigger.isTrigger = true;
            trigger.center = center;
            trigger.size = size;
        }

        private void WirePerimeterTriggerEvents()
        {
            WirePerimeterTrigger(_northTrigger, ref _northPerimeterTrigger);
            WirePerimeterTrigger(_southTrigger, ref _southPerimeterTrigger);
            WirePerimeterTrigger(_eastTrigger, ref _eastPerimeterTrigger);
            WirePerimeterTrigger(_westTrigger, ref _westPerimeterTrigger);
        }

        private void WirePerimeterTrigger(
            BoxCollider triggerCollider,
            ref GroundPerimeterTrigger triggerComponent)
        {
            if (triggerCollider == null)
            {
                return;
            }

            triggerComponent = triggerCollider.GetComponent<GroundPerimeterTrigger>();
            if (triggerComponent == null)
            {
                triggerComponent = triggerCollider.gameObject.AddComponent<GroundPerimeterTrigger>();
            }

            triggerComponent.TriggerEntered -= OnPerimeterTriggerEntered;
            triggerComponent.TriggerEntered += OnPerimeterTriggerEntered;
        }

        private void UnwirePerimeterTriggerEvents()
        {
            UnwirePerimeterTrigger(_northPerimeterTrigger);
            UnwirePerimeterTrigger(_southPerimeterTrigger);
            UnwirePerimeterTrigger(_eastPerimeterTrigger);
            UnwirePerimeterTrigger(_westPerimeterTrigger);
        }

        private void UnwirePerimeterTrigger(GroundPerimeterTrigger triggerComponent)
        {
            if (triggerComponent == null)
            {
                return;
            }

            triggerComponent.TriggerEntered -= OnPerimeterTriggerEntered;
        }

        private void OnPerimeterTriggerEntered(Collider other)
        {
            PerimeterTriggerEntered?.Invoke(other);
        }
    }
}
