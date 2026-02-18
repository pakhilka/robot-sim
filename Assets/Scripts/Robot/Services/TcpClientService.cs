using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace RobotSim.Robot.Services
{
    /// <summary>
    /// Сервис для TCP коммуникации с backend
    /// Чистый класс, содержит только сетевую логику
    /// </summary>
    public class TcpClientService : ITcpClientService
    {
        private readonly string _host;
        private readonly int _port;

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        private ConcurrentQueue<string> _incomingQueue = new();
        private Thread _readThread;
        private CancellationTokenSource _cancellationTokenSource;

        public TcpClientService(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Connect()
        {
            if (_tcpClient != null && _tcpClient.Connected)
                return;

            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(_host, _port);
                _networkStream = _tcpClient.GetStream();
                _streamReader = new StreamReader(_networkStream);
                _streamWriter = new StreamWriter(_networkStream) { AutoFlush = true };

                _incomingQueue = new ConcurrentQueue<string>();
                _cancellationTokenSource = new CancellationTokenSource();
                
                _readThread = new Thread(ReadLoop)
                {
                    IsBackground = true,
                    Name = "TcpClientService-Reader"
                };
                _readThread.Start();

                Debug.Log($"[TcpClientService] Подключено к {_host}:{_port}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TcpClientService] Ошибка подключения: {ex.Message}");
                CleanupConnection();
                throw;
            }
        }

        public void Disconnect()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            if (_readThread != null && _readThread.IsAlive)
            {
                _readThread.Join(TimeSpan.FromSeconds(1));
            }

            CleanupConnection();
        }

        private void CleanupConnection()
        {
            try
            {
                _streamWriter?.Close();
                _streamReader?.Close();
                _networkStream?.Close();
                _tcpClient?.Close();
            }
            catch { }

            _streamWriter = null;
            _streamReader = null;
            _networkStream = null;
            _tcpClient = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private void ReadLoop()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested && _streamReader != null)
                {
                    string line = _streamReader.ReadLine();
                    if (line != null)
                    {
                        _incomingQueue.Enqueue(line);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Debug.LogWarning($"[TcpClientService] Ошибка чтения: {ex.Message}");
                }
            }
        }

        public string SendDistance(int distance)
        {
            if (_tcpClient == null || !_tcpClient.Connected)
                throw new InvalidOperationException("TCP клиент не подключен");

            try
            {
                _streamWriter.WriteLine(distance);

                // Получаем последнее сообщение из очереди (если есть)
                string lastMessage = null;
                while (_incomingQueue.TryDequeue(out string message))
                {
                    lastMessage = message;
                }
                return lastMessage;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TcpClientService] Ошибка отправки: {ex.Message}");
                CleanupConnection();
                throw;
            }
        }

        ~TcpClientService()
        {
            Disconnect();
        }
    }
}
