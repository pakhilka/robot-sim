using RobotSim.Levels.Data;
using RobotSim.Levels.Interfaces;
using UnityEngine;

namespace RobotSim.Levels.Components
{
    /// <summary>
    /// Unity-адаптер для инстанса префабов по LevelGrid.
    /// </summary>
    public sealed class LevelPrefabSpawner
    {
        public int Spawn(LevelGrid grid, ILevelPrefabProvider provider, Transform parent = null, float y = 0f)
        {
            if (grid == null || provider == null)
            {
                return 0;
            }

            SpawnGroundWithBounds(grid, provider, parent, y);

            int spawnedCount = 0;

            for (int row = 0; row < grid.Height; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    LevelCellType cell = grid[row, col];
                    if (!provider.TryGetPrefab(cell, out GameObject prefab))
                    {
                        continue;
                    }

                    (float x, float z) = grid.GetCellCenter(row, col);
                    Object.Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, parent);
                    spawnedCount++;
                }
            }

            return spawnedCount;
        }

        public GameObject SpawnGroundWithBounds(LevelGrid grid, ILevelPrefabProvider provider, Transform parent = null, float y = 0f)
        {
            if (grid == null || provider == null)
            {
                return null;
            }

            if (!provider.TryGetGroundWithBoundsPrefab(out GameObject groundPrefab))
            {
                return null;
            }

            GameObject instance = Object.Instantiate(groundPrefab, new Vector3(0f, y, 0f), Quaternion.identity, parent);

            GroundWithBounds groundWithBounds = instance.GetComponent<GroundWithBounds>();
            if (groundWithBounds != null)
            {
                float worldSizeX = grid.Height * grid.CellSize;
                float worldSizeZ = grid.Width * grid.CellSize;
                groundWithBounds.Configure(worldSizeX, worldSizeZ);
            }

            return instance;
        }
    }
}
