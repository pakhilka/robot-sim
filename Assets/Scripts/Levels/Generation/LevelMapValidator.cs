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
            var definitions = new LevelRoadSymbolDefinition[rows, cols];

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
                    string symbol = map[row][col];
                    if (!LevelRoadSymbolCatalog.TryGetDefinition(symbol, out LevelRoadSymbolDefinition definition))
                    {
                        error = $"Unknown map symbol '{symbol}' at row {row}, col {col}.";
                        return false;
                    }

                    definitions[row, col] = definition;
                    LevelCellType cell = definition.CellType;
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

            LevelDirection[,] openings = LevelRoadOpeningsResolver.Resolve(definitions);
            if (!ValidateStartAndFinishConnectivity(openings, startRow, startCol, finishRow, finishCol, out error))
            {
                return false;
            }

            if (!ValidateMutualEdgeConsistency(cells, openings, out error))
            {
                return false;
            }

            result = new LevelMapValidationResult(
                cells,
                openings,
                startRow,
                startCol,
                finishRow,
                finishCol);
            return true;
        }

        private static bool ValidateStartAndFinishConnectivity(
            LevelDirection[,] openings,
            int startRow,
            int startCol,
            int finishRow,
            int finishCol,
            out string error)
        {
            error = string.Empty;

            if (openings[startRow, startCol] == LevelDirection.None)
            {
                error = "Start tile S must connect to at least one traversable neighbor.";
                return false;
            }

            if (openings[finishRow, finishCol] == LevelDirection.None)
            {
                error = "Finish tile F must connect to at least one traversable neighbor.";
                return false;
            }

            return true;
        }

        private static bool ValidateMutualEdgeConsistency(
            LevelCellType[,] cells,
            LevelDirection[,] openings,
            out string error)
        {
            error = string.Empty;
            int rows = cells.GetLength(0);
            int cols = cells.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (!IsTraversable(cells[row, col]))
                    {
                        continue;
                    }

                    foreach (LevelDirection direction in LevelDirectionUtility.CardinalDirections)
                    {
                        if ((openings[row, col] & direction) == 0)
                        {
                            continue;
                        }

                        if (!LevelDirectionUtility.TryStep(direction, out int rowDelta, out int colDelta))
                        {
                            continue;
                        }

                        int neighborRow = row + rowDelta;
                        int neighborCol = col + colDelta;
                        if (neighborRow < 0 || neighborRow >= rows || neighborCol < 0 || neighborCol >= cols)
                        {
                            // Открытый выход к краю карты считается валидным тупиком.
                            continue;
                        }

                        if (!IsTraversable(cells[neighborRow, neighborCol]))
                        {
                            // Открытый выход в пустоту считается валидным тупиком.
                            continue;
                        }

                        LevelDirection opposite = LevelDirectionUtility.Opposite(direction);
                        bool neighborHasOpposite = (openings[neighborRow, neighborCol] & opposite) != 0;
                        if (!neighborHasOpposite)
                        {
                            error = $"Inconsistent connectivity between ({row},{col}) and ({neighborRow},{neighborCol}).";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool IsTraversable(LevelCellType cellType)
        {
            return cellType == LevelCellType.Road ||
                   cellType == LevelCellType.Start ||
                   cellType == LevelCellType.Finish;
        }
    }
}
