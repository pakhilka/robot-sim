namespace RobotSim.Levels.Data
{
    /// <summary>
    /// Результат валидации и нормализации карты уровня.
    /// </summary>
    public sealed class LevelMapValidationResult
    {
        public LevelMapValidationResult(
            LevelCellType[,] cells,
            int startRow,
            int startCol,
            int finishRow,
            int finishCol)
        {
            Cells = cells;
            StartRow = startRow;
            StartCol = startCol;
            FinishRow = finishRow;
            FinishCol = finishCol;
        }

        public LevelCellType[,] Cells { get; }
        public int StartRow { get; }
        public int StartCol { get; }
        public int FinishRow { get; }
        public int FinishCol { get; }
        public int Height => Cells.GetLength(0);
        public int Width => Cells.GetLength(1);
    }
}
