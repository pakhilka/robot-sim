namespace RobotSim.Levels.Data
{
    /// <summary>
    /// Описание семантики одного символа входной карты.
    /// </summary>
    public readonly struct LevelRoadSymbolDefinition
    {
        public LevelRoadSymbolDefinition(
            string symbol,
            LevelCellType cellType,
            bool isTraversable,
            LevelDirection openings,
            bool inferOpeningsFromNeighbors)
        {
            Symbol = symbol;
            CellType = cellType;
            IsTraversable = isTraversable;
            Openings = openings;
            InferOpeningsFromNeighbors = inferOpeningsFromNeighbors;
        }

        public string Symbol { get; }
        public LevelCellType CellType { get; }
        public bool IsTraversable { get; }
        public LevelDirection Openings { get; }
        public bool InferOpeningsFromNeighbors { get; }
    }
}
