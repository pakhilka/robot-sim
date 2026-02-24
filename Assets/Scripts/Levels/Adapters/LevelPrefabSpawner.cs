using RobotSim.Levels.Data;
using RobotSim.Levels.Interfaces;
using RobotSim.Levels.Components;
using UnityEngine;

namespace RobotSim.Levels.Adapters
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
                    if (!grid.IsTraversable(row, col))
                    {
                        continue;
                    }

                    if (!provider.TryGetPrefab(cell, out GameObject prefab))
                    {
                        continue;
                    }

                    (float x, float z) = grid.GetCellCenter(row, col);
                    Quaternion cellRotation = GetCellRotation(grid, row, col, cell);
                    Object.Instantiate(prefab, new Vector3(x, y, z), cellRotation, parent);
                    spawnedCount++;
                }
            }

            spawnedCount += SpawnBoundaryWalls(grid, provider, parent, y);
            return spawnedCount;
        }

        private int SpawnBoundaryWalls(LevelGrid grid, ILevelPrefabProvider provider, Transform parent, float y)
        {
            if (!provider.TryGetBoundaryWallPrefab(out GameObject boundaryWallPrefab))
            {
                return 0;
            }

            float boundaryWallHalfHeight = GetBoundaryWallHalfHeight(boundaryWallPrefab);
            int spawned = 0;
            foreach (LevelBoundaryEdge edge in grid.GetBoundaryEdges())
            {
                Quaternion rotation = LevelDirectionUtility.IsNorthSouth(edge.Direction)
                    ? Quaternion.Euler(0f, 90f, 0f)
                    : Quaternion.identity;

                Object.Instantiate(
                    boundaryWallPrefab,
                    new Vector3(edge.X, y + boundaryWallHalfHeight, edge.Z),
                    rotation,
                    parent);

                spawned++;
            }

            return spawned;
        }

        private static float GetBoundaryWallHalfHeight(GameObject boundaryWallPrefab)
        {
            if (boundaryWallPrefab == null)
            {
                return 0f;
            }

            Transform transform = boundaryWallPrefab.transform;

            BoxCollider boxCollider = boundaryWallPrefab.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                float colliderHeight = Mathf.Abs(boxCollider.size.y * transform.localScale.y);
                if (colliderHeight > 0f)
                {
                    return colliderHeight * 0.5f;
                }
            }

            MeshFilter meshFilter = boundaryWallPrefab.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                float meshHeight = Mathf.Abs(meshFilter.sharedMesh.bounds.size.y * transform.localScale.y);
                if (meshHeight > 0f)
                {
                    return meshHeight * 0.5f;
                }
            }

            return Mathf.Abs(transform.localScale.y) * 0.5f;
        }

        private static Quaternion GetCellRotation(LevelGrid grid, int row, int col, LevelCellType cellType)
        {
            if (cellType != LevelCellType.Start && cellType != LevelCellType.Finish)
            {
                return Quaternion.identity;
            }

            LevelDirection openings = grid.GetOpenings(row, col);
            if (!TryGetPrimaryDirection(openings, out LevelDirection direction))
            {
                return Quaternion.identity;
            }

            return Quaternion.Euler(0f, GetYawByDirection(direction), 0f);
        }

        private static bool TryGetPrimaryDirection(LevelDirection openings, out LevelDirection direction)
        {
            foreach (LevelDirection candidate in LevelDirectionUtility.CardinalDirections)
            {
                if ((openings & candidate) != 0)
                {
                    direction = candidate;
                    return true;
                }
            }

            direction = LevelDirection.None;
            return false;
        }

        private static float GetYawByDirection(LevelDirection direction)
        {
            switch (direction)
            {
                case LevelDirection.North:
                    return 270f;
                case LevelDirection.East:
                    return 0f;
                case LevelDirection.South:
                    return 90f;
                case LevelDirection.West:
                    return 180f;
                default:
                    return 0f;
            }
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
