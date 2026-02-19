namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Pre-start connectivity probe service for socket address.
    /// </summary>
    public sealed class ConnectionProbeService
    {
        private readonly SocketConnectionProbe _socketProbe;

        public ConnectionProbeService()
            : this(new SocketConnectionProbe())
        {
        }

        public ConnectionProbeService(SocketConnectionProbe socketProbe)
        {
            _socketProbe = socketProbe ?? new SocketConnectionProbe();
        }

        public bool TryProbe(
            string socketAddress,
            int timeoutMilliseconds,
            out string error)
        {
            return _socketProbe.TryConnect(
                socketAddress,
                timeoutMilliseconds,
                out error);
        }
    }
}
