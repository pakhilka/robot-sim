namespace RobotSim.Levels.Data
{
    /// <summary>
    /// Геометрия одной тонкой граничной стенки по ребру клетки.
    /// </summary>
    public readonly struct LevelBoundaryEdge
    {
        public LevelBoundaryEdge(int row, int col, LevelDirection direction, float x, float z)
        {
            Row = row;
            Col = col;
            Direction = direction;
            X = x;
            Z = z;
        }

        public int Row { get; }
        public int Col { get; }
        public LevelDirection Direction { get; }
        public float X { get; }
        public float Z { get; }
    }
}
