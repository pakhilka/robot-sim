using System.Collections.Generic;

namespace RobotSim.Levels.Data
{
    /// <summary>
    /// Нормализованная модель уровня в клеточной системе координат.
    /// row -> X, col -> Z.
    /// </summary>
    public sealed class LevelGrid
    {
        public const float DefaultCellSize = 10f;

        private readonly LevelCellType[,] _cells;
        private readonly LevelDirection[,] _openings;

        public LevelGrid(
            LevelCellType[,] cells,
            LevelDirection[,] openings,
            int startRow,
            int startCol,
            int finishRow,
            int finishCol,
            float cellSize = DefaultCellSize)
        {
            _cells = cells;
            _openings = openings;
            StartRow = startRow;
            StartCol = startCol;
            FinishRow = finishRow;
            FinishCol = finishCol;
            CellSize = cellSize;
        }

        public int Height => _cells.GetLength(0);
        public int Width => _cells.GetLength(1);
        public float CellSize { get; }

        public int StartRow { get; }
        public int StartCol { get; }
        public int FinishRow { get; }
        public int FinishCol { get; }

        public float WorldSizeX => Height * CellSize;
        public float WorldSizeZ => Width * CellSize;
        public float MinX => -WorldSizeX * 0.5f;
        public float MaxX => WorldSizeX * 0.5f;
        public float MinZ => -WorldSizeZ * 0.5f;
        public float MaxZ => WorldSizeZ * 0.5f;

        public LevelCellType this[int row, int col] => _cells[row, col];
        public LevelDirection GetOpenings(int row, int col) => _openings[row, col];

        public (float x, float z) GetCellCenter(int row, int col)
        {
            float half = CellSize * 0.5f;
            float x = MinX + (row * CellSize) + half;
            float z = MinZ + (col * CellSize) + half;
            return (x, z);
        }

        public (float minX, float maxX, float minZ, float maxZ) GetCellBounds(int row, int col)
        {
            float minX = MinX + (row * CellSize);
            float minZ = MinZ + (col * CellSize);
            return (minX, minX + CellSize, minZ, minZ + CellSize);
        }

        public bool IsWithinBounds(float x, float z)
        {
            return x >= MinX && x < MaxX && z >= MinZ && z < MaxZ;
        }

        public bool IsTraversable(int row, int col)
        {
            return IsTraversableCellType(_cells[row, col]);
        }

        public IReadOnlyList<LevelBoundaryEdge> GetBoundaryEdges()
        {
            var edges = new List<LevelBoundaryEdge>();
            var seen = new HashSet<BoundaryEdgeKey>();

            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (!IsTraversable(row, col))
                    {
                        continue;
                    }

                    TryAddBoundaryEdge(edges, seen, row, col, LevelDirection.North);
                    TryAddBoundaryEdge(edges, seen, row, col, LevelDirection.East);
                    TryAddBoundaryEdge(edges, seen, row, col, LevelDirection.South);
                    TryAddBoundaryEdge(edges, seen, row, col, LevelDirection.West);
                }
            }

            return edges;
        }

        private void TryAddBoundaryEdge(
            List<LevelBoundaryEdge> edges,
            HashSet<BoundaryEdgeKey> seen,
            int row,
            int col,
            LevelDirection direction)
        {
            if (IsPassableEdge(row, col, direction))
            {
                return;
            }

            if (!TryBuildBoundaryEdge(row, col, direction, out LevelBoundaryEdge edge, out BoundaryEdgeKey key))
            {
                return;
            }

            if (seen.Add(key))
            {
                edges.Add(edge);
            }
        }

        private bool TryBuildBoundaryEdge(
            int row,
            int col,
            LevelDirection direction,
            out LevelBoundaryEdge edge,
            out BoundaryEdgeKey key)
        {
            edge = default;
            key = default;

            (float minX, float maxX, float minZ, float maxZ) = GetCellBounds(row, col);
            switch (direction)
            {
                case LevelDirection.North:
                    edge = new LevelBoundaryEdge(row, col, direction, minX, (minZ + maxZ) * 0.5f);
                    key = new BoundaryEdgeKey(isHorizontal: true, lineRow: row, lineCol: col);
                    return true;
                case LevelDirection.East:
                    edge = new LevelBoundaryEdge(row, col, direction, (minX + maxX) * 0.5f, maxZ);
                    key = new BoundaryEdgeKey(isHorizontal: false, lineRow: row, lineCol: col + 1);
                    return true;
                case LevelDirection.South:
                    edge = new LevelBoundaryEdge(row, col, direction, maxX, (minZ + maxZ) * 0.5f);
                    key = new BoundaryEdgeKey(isHorizontal: true, lineRow: row + 1, lineCol: col);
                    return true;
                case LevelDirection.West:
                    edge = new LevelBoundaryEdge(row, col, direction, (minX + maxX) * 0.5f, minZ);
                    key = new BoundaryEdgeKey(isHorizontal: false, lineRow: row, lineCol: col);
                    return true;
                default:
                    return false;
            }
        }

        private bool IsPassableEdge(int row, int col, LevelDirection direction)
        {
            if (!IsTraversable(row, col))
            {
                return false;
            }

            if ((_openings[row, col] & direction) == 0)
            {
                return false;
            }

            if (!TryGetNeighbor(row, col, direction, out int neighborRow, out int neighborCol))
            {
                return false;
            }

            if (!IsTraversable(neighborRow, neighborCol))
            {
                return false;
            }

            LevelDirection opposite = LevelDirectionUtility.Opposite(direction);
            return (_openings[neighborRow, neighborCol] & opposite) != 0;
        }

        private bool TryGetNeighbor(int row, int col, LevelDirection direction, out int neighborRow, out int neighborCol)
        {
            neighborRow = row;
            neighborCol = col;

            if (!LevelDirectionUtility.TryStep(direction, out int rowDelta, out int colDelta))
            {
                return false;
            }

            neighborRow = row + rowDelta;
            neighborCol = col + colDelta;
            return neighborRow >= 0 && neighborRow < Height && neighborCol >= 0 && neighborCol < Width;
        }

        private static bool IsTraversableCellType(LevelCellType cellType)
        {
            return cellType == LevelCellType.Road ||
                   cellType == LevelCellType.Start ||
                   cellType == LevelCellType.Finish;
        }

        private readonly struct BoundaryEdgeKey
        {
            public BoundaryEdgeKey(bool isHorizontal, int lineRow, int lineCol)
            {
                IsHorizontal = isHorizontal;
                LineRow = lineRow;
                LineCol = lineCol;
            }

            public bool IsHorizontal { get; }
            public int LineRow { get; }
            public int LineCol { get; }
        }
    }
}
