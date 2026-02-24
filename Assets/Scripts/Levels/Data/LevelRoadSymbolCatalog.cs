using System.Collections.Generic;

namespace RobotSim.Levels.Data
{
    /// <summary>
    /// Единый каталог допустимых символов карты и их семантики.
    /// </summary>
    public static class LevelRoadSymbolCatalog
    {
        private static readonly Dictionary<string, LevelRoadSymbolDefinition> Definitions =
            new Dictionary<string, LevelRoadSymbolDefinition>
            {
                ["#"] = new LevelRoadSymbolDefinition(
                    "#",
                    LevelCellType.Void,
                    isTraversable: false,
                    openings: LevelDirection.None,
                    inferOpeningsFromNeighbors: false),
                ["S"] = new LevelRoadSymbolDefinition(
                    "S",
                    LevelCellType.Start,
                    isTraversable: true,
                    openings: LevelDirection.None,
                    inferOpeningsFromNeighbors: true),
                ["F"] = new LevelRoadSymbolDefinition(
                    "F",
                    LevelCellType.Finish,
                    isTraversable: true,
                    openings: LevelDirection.None,
                    inferOpeningsFromNeighbors: true),
                ["─"] = new LevelRoadSymbolDefinition(
                    "─",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.East | LevelDirection.West,
                    inferOpeningsFromNeighbors: false),
                ["│"] = new LevelRoadSymbolDefinition(
                    "│",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.North | LevelDirection.South,
                    inferOpeningsFromNeighbors: false),
                ["┌"] = new LevelRoadSymbolDefinition(
                    "┌",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.East | LevelDirection.South,
                    inferOpeningsFromNeighbors: false),
                ["┐"] = new LevelRoadSymbolDefinition(
                    "┐",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.West | LevelDirection.South,
                    inferOpeningsFromNeighbors: false),
                ["└"] = new LevelRoadSymbolDefinition(
                    "└",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.North | LevelDirection.East,
                    inferOpeningsFromNeighbors: false),
                ["┘"] = new LevelRoadSymbolDefinition(
                    "┘",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.North | LevelDirection.West,
                    inferOpeningsFromNeighbors: false),
                ["├"] = new LevelRoadSymbolDefinition(
                    "├",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.North | LevelDirection.East | LevelDirection.South,
                    inferOpeningsFromNeighbors: false),
                ["┤"] = new LevelRoadSymbolDefinition(
                    "┤",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.North | LevelDirection.West | LevelDirection.South,
                    inferOpeningsFromNeighbors: false),
                ["┬"] = new LevelRoadSymbolDefinition(
                    "┬",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.East | LevelDirection.South | LevelDirection.West,
                    inferOpeningsFromNeighbors: false),
                ["┴"] = new LevelRoadSymbolDefinition(
                    "┴",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.North | LevelDirection.East | LevelDirection.West,
                    inferOpeningsFromNeighbors: false),
                ["X"] = new LevelRoadSymbolDefinition(
                    "X",
                    LevelCellType.Road,
                    isTraversable: true,
                    openings: LevelDirection.North | LevelDirection.East | LevelDirection.South | LevelDirection.West,
                    inferOpeningsFromNeighbors: false)
            };

        public static bool TryGetDefinition(string symbol, out LevelRoadSymbolDefinition definition)
        {
            if (symbol == null)
            {
                definition = default;
                return false;
            }

            return Definitions.TryGetValue(symbol, out definition);
        }

        public static bool IsKnownSymbol(string symbol)
        {
            return symbol != null && Definitions.ContainsKey(symbol);
        }

        public static IReadOnlyCollection<string> GetKnownSymbols()
        {
            return Definitions.Keys;
        }
    }
}
