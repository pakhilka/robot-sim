using RobotSim.Levels.Data;

namespace RobotSim.Levels.Generation
{
    /// <summary>
    /// Разрешает направления проезда для всех проезжаемых клеток карты.
    /// </summary>
    public static class LevelRoadOpeningsResolver
    {
        public static LevelDirection[,] Resolve(LevelRoadSymbolDefinition[,] definitions)
        {
            int rows = definitions.GetLength(0);
            int cols = definitions.GetLength(1);
            var openings = new LevelDirection[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    LevelRoadSymbolDefinition definition = definitions[row, col];
                    if (!definition.IsTraversable || definition.InferOpeningsFromNeighbors)
                    {
                        continue;
                    }

                    openings[row, col] = definition.Openings;
                }
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    LevelRoadSymbolDefinition definition = definitions[row, col];
                    if (!definition.IsTraversable || !definition.InferOpeningsFromNeighbors)
                    {
                        continue;
                    }

                    openings[row, col] = InferOpeningsFromNeighbors(definitions, row, col);
                }
            }

            return openings;
        }

        private static LevelDirection InferOpeningsFromNeighbors(LevelRoadSymbolDefinition[,] definitions, int row, int col)
        {
            int rows = definitions.GetLength(0);
            int cols = definitions.GetLength(1);

            LevelDirection resolved = LevelDirection.None;
            foreach (LevelDirection direction in LevelDirectionUtility.CardinalDirections)
            {
                if (!LevelDirectionUtility.TryStep(direction, out int rowDelta, out int colDelta))
                {
                    continue;
                }

                int neighborRow = row + rowDelta;
                int neighborCol = col + colDelta;
                if (neighborRow < 0 || neighborRow >= rows || neighborCol < 0 || neighborCol >= cols)
                {
                    continue;
                }

                LevelRoadSymbolDefinition neighbor = definitions[neighborRow, neighborCol];
                if (!neighbor.IsTraversable)
                {
                    continue;
                }

                LevelDirection opposite = LevelDirectionUtility.Opposite(direction);
                if (CanAcceptConnectionFromNeighbor(neighbor, opposite))
                {
                    resolved |= direction;
                }
            }

            return resolved;
        }

        private static bool CanAcceptConnectionFromNeighbor(LevelRoadSymbolDefinition neighbor, LevelDirection oppositeDirection)
        {
            if (!neighbor.IsTraversable)
            {
                return false;
            }

            if (neighbor.InferOpeningsFromNeighbors)
            {
                return true;
            }

            return (neighbor.Openings & oppositeDirection) != 0;
        }
    }
}
