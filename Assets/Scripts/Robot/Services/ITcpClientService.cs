namespace RobotSim.Services
{
    /// <summary>
    /// Интерфейс для TCP сервиса коммуникации
    /// </summary>
    public interface ITcpClientService
    {
        void Connect();
        void Disconnect();
        string SendDistance(int distance);
    }
}
