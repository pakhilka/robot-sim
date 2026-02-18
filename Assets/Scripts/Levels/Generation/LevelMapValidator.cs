using RobotSim.Levels.Data;

namespace RobotSim.Levels.Generation
{
    /// <summary>
    /// Валидирует сырой map и преобразует символы в типы ячеек.
    /// </summary>
    public static class LevelMapValidator
    {
        public static bool TryValidateAndMap(
            string[][] map,
            out LevelMapValidationResult result,
            out string error)
        {
            result = null;
            error = string.Empty;

            if (map == null || map.Length == 0)
            {
                error = "Map must contain at least one row.";
                return false;
            }

            if (map[0] == null || map[0].Length == 0)
            {
                error = "Map must contain at least one column.";
                return false;
            }

            int rows = map.Length;
            int cols = map[0].Length;
            var cells = new LevelCellType[rows, cols];

            int startCount = 0;
            int finishCount = 0;
            int startRow = -1;
            int startCol = -1;
            int finishRow = -1;
            int finishCol = -1;

            for (int row = 0; row < rows; row++)
            {
                if (map[row] == null)
                {
                    error = $"Map row {row} is null.";
                    return false;
                }

                if (map[row].Length != cols)
                {
                    error = "Map must be rectangular (all rows same length).";
                    return false;
                }

                for (int col = 0; col < cols; col++)
                {
                    LevelCellType cell = MapSymbolToCellType(map[row][col]);
                    cells[row, col] = cell;

                    if (cell == LevelCellType.Start)
                    {
                        startCount++;
                        startRow = row;
                        startCol = col;
                    }

                    if (cell == LevelCellType.Finish)
                    {
                        finishCount++;
                        finishRow = row;
                        finishCol = col;
                    }
                }
            }

            if (startCount != 1)
            {
                error = "Map must contain exactly one S (Start).";
                return false;
            }

            if (finishCount != 1)
            {
                error = "Map must contain exactly one F (Finish).";
                return false;
            }

            result = new LevelMapValidationResult(
                cells,
                startRow,
                startCol,
                finishRow,
                finishCol);
            return true;
        }

        public static LevelCellType MapSymbolToCellType(string symbol)
        {
            if (symbol == "W")
            {
                return LevelCellType.Wall;
            }

            if (symbol == "S")
            {
                return LevelCellType.Start;
            }

            if (symbol == "F")
            {
                return LevelCellType.Finish;
            }

            return LevelCellType.Empty;
        }
    }
}
