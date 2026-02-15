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

        public LevelGrid(
            LevelCellType[,] cells,
            int startRow,
            int startCol,
            int finishRow,
            int finishCol,
            float cellSize = DefaultCellSize)
        {
            _cells = cells;
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

        public LevelCellType this[int row, int col] => _cells[row, col];

        public (float x, float z) GetCellCenter(int row, int col)
        {
            float half = CellSize * 0.5f;
            float x = (row * CellSize) + half;
            float z = (col * CellSize) + half;
            return (x, z);
        }

        public bool IsWithinBounds(float x, float z)
        {
            return x >= 0f && x < Height * CellSize && z >= 0f && z < Width * CellSize;
        }
    }
}
