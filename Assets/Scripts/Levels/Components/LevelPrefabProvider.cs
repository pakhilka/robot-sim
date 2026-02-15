using RobotSim.Levels.Data;
using RobotSim.Levels.Interfaces;
using UnityEngine;

namespace RobotSim.Levels.Components
{
    /// <summary>
    /// Unity-адаптер для выдачи префабов уровня.
    /// Настройка в сцене:
    /// 1) Добавить компонент на GameObject.
    /// 2) Заполнить Wall/Start/Finish префабы в инспекторе.
    /// 3) Опционально заполнить Ground With Bounds префаб (пол + 4 trigger стены).
    /// </summary>
    public sealed class LevelPrefabProvider : MonoBehaviour, ILevelPrefabProvider
    {
        [Header("Cell Prefabs")]
        [SerializeField]
        private GameObject _wallPrefab;

        [SerializeField]
        private GameObject _startPrefab;

        [SerializeField]
        private GameObject _finishPrefab;

        [Header("Level Root Prefabs")]
        [SerializeField]
        private GameObject _groundWithBoundsPrefab;

        public bool TryGetPrefab(LevelCellType cellType, out GameObject prefab)
        {
            prefab = null;

            if (cellType == LevelCellType.Wall)
            {
                prefab = _wallPrefab;
                return prefab != null;
            }

            if (cellType == LevelCellType.Start)
            {
                prefab = _startPrefab;
                return prefab != null;
            }

            if (cellType == LevelCellType.Finish)
            {
                prefab = _finishPrefab;
                return prefab != null;
            }

            return false;
        }

        public bool TryGetGroundWithBoundsPrefab(out GameObject prefab)
        {
            prefab = _groundWithBoundsPrefab;
            return prefab != null;
        }
    }
}
