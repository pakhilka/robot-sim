using RobotSim.Robot.Interfaces;
using RobotSim.Robot.Services;

namespace RobotSim.Robot.Brains
{
    /// <summary>
    /// Factory для создания экземпляров мозгов
    /// Чистый класс, не зависит от MonoBehaviour
    /// </summary>
    public class BrainSelector
    {
        public static IRobotBrain CreateBrain(
            BrainType type,
            string tcpHost = "127.0.0.1",
            int tcpPort = 9999,
            BrainConfig config = null)
        {
            BrainConfig effectiveConfig = config ?? new BrainConfig();

            return type switch
            {
                BrainType.WokwiTcp => new WokwiTcpBrain(
                    new TcpClientService(tcpHost, tcpPort),
                    effectiveConfig
                ),
                BrainType.LocalMock => new LocalMockBrain(effectiveConfig),
                _ => throw new System.ArgumentException($"Unknown brain type: {type}")
            };
        }
    }
}
