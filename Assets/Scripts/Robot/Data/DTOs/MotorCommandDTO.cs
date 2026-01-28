using System;

namespace RobotSim.Data.DTOs
{
    /// <summary>
    /// DTO - Команда для моторов (левый и правый)
    /// </summary>
    [Serializable]
    public struct MotorCommandDTO
    {
        public float left;
        public float right;

        public MotorCommandDTO(float left = 0f, float right = 0f)
        {
            this.left = left;
            this.right = right;
        }
    }
}
