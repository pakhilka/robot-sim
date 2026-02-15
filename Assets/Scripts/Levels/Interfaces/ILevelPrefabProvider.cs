using RobotSim.Levels.Data;
using UnityEngine;

namespace RobotSim.Levels.Interfaces
{
    /// <summary>
    /// Поставщик префабов по типу ячейки.
    /// </summary>
    public interface ILevelPrefabProvider
    {
        bool TryGetPrefab(LevelCellType cellType, out GameObject prefab);

        /// <summary>
        /// Возвращает единый префаб ground+perimeter bounds, если он настроен.
        /// </summary>
        bool TryGetGroundWithBoundsPrefab(out GameObject prefab);
    }
}
