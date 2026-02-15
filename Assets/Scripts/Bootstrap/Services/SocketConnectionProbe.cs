using System;
using System.Net.Sockets;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Lightweight pre-run socket connectivity probe.
    /// </summary>
    public sealed class SocketConnectionProbe
    {
        public bool TryConnect(string socketAddress, int timeoutMilliseconds, out string error)
        {
            error = string.Empty;

            if (!TryParseSocketAddress(socketAddress, out string host, out int port, out error))
            {
                return false;
            }

            if (timeoutMilliseconds <= 0)
            {
                timeoutMilliseconds = 3000;
            }

            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                bool connectedInTime = connectTask.Wait(timeoutMilliseconds);
                if (!connectedInTime || !client.Connected)
                {
                    error = $"Failed to connect to {host}:{port} within {timeoutMilliseconds} ms.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to connect to {host}:{port}. {ex.Message}";
                return false;
            }
        }

        public static bool TryParseSocketAddress(string socketAddress, out string host, out int port, out string error)
        {
            host = string.Empty;
            port = 0;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(socketAddress))
            {
                error = "socketAddress is empty.";
                return false;
            }

            string[] parts = socketAddress.Split(':');
            if (parts.Length != 2)
            {
                error = "socketAddress must be in '<host>:<port>' format.";
                return false;
            }

            host = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(host))
            {
                error = "socketAddress host is empty.";
                return false;
            }

            if (!int.TryParse(parts[1], out port) || port <= 0 || port > 65535)
            {
                error = "socketAddress port is invalid.";
                return false;
            }

            return true;
        }
    }
}
