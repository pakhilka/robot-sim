using RobotSim.Robot.Data.DTOs;
using RobotSim.Levels.Data;

namespace RobotSim.Levels.Generation
{
    /// <summary>
    /// Строит LevelGrid из входного DTO запроса.
    /// </summary>
    public static class LevelGridFactory
    {
        public static bool TryCreate(LevelRunRequestDTO request, out LevelGrid grid, out string error)
        {
            grid = null;
            error = string.Empty;

            if (!LevelMapValidator.TryValidateAndMap(request.map, out LevelMapValidationResult validated, out error))
            {
                return false;
            }

            grid = new LevelGrid(
                validated.Cells,
                validated.StartRow,
                validated.StartCol,
                validated.FinishRow,
                validated.FinishCol,
                LevelGrid.DefaultCellSize);
            return true;
        }
    }
}
