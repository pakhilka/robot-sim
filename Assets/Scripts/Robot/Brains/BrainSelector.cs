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
            int tcpPort = 9999)
        {
            return type switch
            {
                BrainType.WokwiTcp => new WokwiTcpBrain(
                    new TcpClientService(tcpHost, tcpPort),
                    new BrainConfig()
                ),
                BrainType.LocalMock => new LocalMockBrain(new BrainConfig()),
                _ => throw new System.ArgumentException($"Unknown brain type: {type}")
            };
        }
    }
}
