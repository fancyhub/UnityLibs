/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using UnityEngine;

namespace FH
{
    internal static class DebugConnectionEditorClient
    {
        private static IDebugConnectionClient _Client;
        private static string _Host;
        private static int _Port;
        private static bool _AutoPort;
        private static int _PortCount = DebugConnectionServer.DefaultPortScanCount;
        private static string _TargetDisplayName;
        private static Action _CleanupConnection;

        public const int DefaultAdbLocalPortStart = DebugConnectionEditorAdbTransport.DefaultLocalPortStart;
        public const int DefaultAdbLocalPortEnd = DebugConnectionEditorAdbTransport.DefaultLocalPortEnd;

        public static event Action Connected;
        public static event Action Disconnected;
        public static event Action<string> Error;
        public static event Action<DebugConnectionTargetInfo> TargetInfoChanged;

        public static string LastError { get; private set; }

        public static bool IsRunning
        {
            get
            {
                IDebugConnectionClient client = _Client;
                return client != null && client.IsRunning;
            }
        }

        public static bool IsConnected
        {
            get
            {
                IDebugConnectionClient client = _Client;
                return client != null && client.IsConnected;
            }
        }

        public static DebugConnectionClientConnectResult Connect(
            string host,
            int port = DebugConnectionServer.DefaultPort,
            float reconnectIntervalSeconds = 2f)
        {
            return ConnectInternal(host, port, false, 1, reconnectIntervalSeconds);
        }

        public static DebugConnectionClientConnectResult ConnectAutoPort(
            string host,
            int startPort = DebugConnectionServer.DefaultPort,
            int portCount = DebugConnectionServer.DefaultPortScanCount,
            float reconnectIntervalSeconds = 2f)
        {
            return ConnectInternal(host, startPort, true, portCount, reconnectIntervalSeconds);
        }

        public static DebugConnectionClientConnectResult ConnectAdb(
            string deviceSerial = null,
            int localStartPort = DefaultAdbLocalPortStart,
            int remotePort = DebugConnectionServer.DefaultPort,
            float reconnectIntervalSeconds = 2f)
        {
            if (_Client != null && _Client.IsRunning)
                return DebugConnectionClientConnectResult.AlreadyRunning;

            CleanupConnection();

            if (!DebugConnectionEditorAdbTransport.ForwardAnyLocalPort(
                deviceSerial,
                localStartPort,
                DefaultAdbLocalPortEnd,
                remotePort,
                out int localPort))
            {
                OnError(DebugConnectionEditorAdbTransport.LastError);
                return DebugConnectionClientConnectResult.InvalidHost;
            }

            _CleanupConnection = DebugConnectionEditorAdbTransport.RemoveForwards;
            return ConnectInternal("127.0.0.1", localPort, false, 1, reconnectIntervalSeconds, false);
        }

        public static DebugConnectionClientConnectResult ConnectAdbAutoPort(
            string deviceSerial = null,
            int remoteStartPort = DebugConnectionServer.DefaultPort,
            int portCount = DebugConnectionServer.DefaultPortScanCount,
            float reconnectIntervalSeconds = 2f,
            int localStartPort = DefaultAdbLocalPortStart)
        {
            if (_Client != null && _Client.IsRunning)
                return DebugConnectionClientConnectResult.AlreadyRunning;

            CleanupConnection();

            int count = Math.Max(1, portCount);
            int localPort = localStartPort;
            int lastForwardedLocalPort = 0;
            for (int i = 0; i < count; i++)
            {
                int remotePort = remoteStartPort + i;
                if (!DebugConnectionEditorAdbTransport.ForwardAnyLocalPort(
                    deviceSerial,
                    localPort,
                    DefaultAdbLocalPortEnd,
                    remotePort,
                    out int forwardedLocalPort))
                {
                    DebugConnectionEditorAdbTransport.RemoveForwards();
                    OnError(DebugConnectionEditorAdbTransport.LastError);
                    return DebugConnectionClientConnectResult.InvalidHost;
                }

                lastForwardedLocalPort = forwardedLocalPort;
                localPort = forwardedLocalPort + 1;
            }

            _CleanupConnection = DebugConnectionEditorAdbTransport.RemoveForwards;
            int localPortCount = lastForwardedLocalPort - localStartPort + 1;
            return ConnectInternal("127.0.0.1", localStartPort, true, localPortCount, reconnectIntervalSeconds, false);
        }

        public static string[] GetAdbDeviceSerials()
        {
            string[] serials = DebugConnectionEditorAdbTransport.GetDeviceSerials();
            if (!string.IsNullOrEmpty(DebugConnectionEditorAdbTransport.LastError))
                OnError(DebugConnectionEditorAdbTransport.LastError);

            return serials;
        }

        private static DebugConnectionClientConnectResult ConnectInternal(
            string host,
            int port,
            bool autoPort,
            int portCount,
            float reconnectIntervalSeconds,
            bool cleanupExisting = true)
        {
            if (string.IsNullOrWhiteSpace(host))
                return DebugConnectionClientConnectResult.InvalidHost;

            if (port <= 0 || port > 65535)
                return DebugConnectionClientConnectResult.InvalidPort;

            if (_Client != null && _Client.IsRunning)
                return DebugConnectionClientConnectResult.AlreadyRunning;

            if (cleanupExisting)
                CleanupConnection();

            DebugConnectionMainThread.Initialize();

            _Host = host;
            _Port = port;
            _AutoPort = autoPort;
            _PortCount = Math.Max(1, portCount);
            _TargetDisplayName = string.Empty;
            LastError = string.Empty;

            DebugConnectionTcpClient client = new DebugConnectionTcpClient(
                host,
                port,
                autoPort ? _PortCount : 1,
                reconnectIntervalSeconds,
                DebugConnectionHello.CreatePayload(DebugConnectionHello.RoleEditor),
                DebugConnectionHello.RolePlayer);
            client.Connected += OnConnected;
            client.Disconnected += OnDisconnected;
            client.Error += OnError;
            client.RemoteDisplayNameChanged += OnRemoteDisplayNameChanged;
            client.MessageReceived += OnMessageReceived;

            _Client = client;
            DebugConnectionClient.SetClient(client);
            client.Start();
            return DebugConnectionClientConnectResult.Started;
        }

        public static void Disconnect()
        {
            IDebugConnectionClient client = _Client;
            _Client = null;
            _TargetDisplayName = string.Empty;
            DebugConnectionClient.Disconnect();
            CleanupConnection();
            TargetInfoChanged?.Invoke(GetTargetInfo());
        }

        private static void CleanupConnection()
        {
            Action cleanup = _CleanupConnection;
            _CleanupConnection = null;
            cleanup?.Invoke();
        }

        

        public static bool Send(Guid messageId, byte[] data)
        {
            IDebugConnectionClient client = _Client;
            if (client == null)
                return false;

            return client.Send(messageId, data);
        }

        public static DebugConnectionTargetInfo GetTargetInfo()
        {
            IDebugConnectionClient client = _Client;
            return new DebugConnectionTargetInfo
            {
                Host = _Host ?? string.Empty,
                Port = client != null && client.ConnectedPort > 0 ? client.ConnectedPort : _Port,
                StartPort = _Port,
                AutoPort = _AutoPort,
                PortCount = _PortCount,
                DisplayName = client == null ? (_TargetDisplayName ?? string.Empty) : client.RemoteDisplayName,
                RemoteEndPoint = client == null ? string.Empty : client.RemoteEndPoint,
                ConnectedUtc = client == null ? default : client.ConnectedUtc,
                IsConnected = client != null && client.IsConnected,
            };
        }

        private static void OnConnected()
        {
            Debug.Log("[DebugConnection] Editor connected to " + GetTargetInfo().RemoteEndPoint);
            Connected?.Invoke();
            TargetInfoChanged?.Invoke(GetTargetInfo());
            DebugConnectionClient.ClientEventSet.Connected?.Invoke();
        }

        private static void OnDisconnected()
        {
            Disconnected?.Invoke();
            TargetInfoChanged?.Invoke(GetTargetInfo());
            DebugConnectionClient.ClientEventSet.Disconnected?.Invoke();
        }

        private static void OnError(string message)
        {
            LastError = message;
            Debug.LogWarning("[DebugConnection] Editor connection error: " + message);
            Error?.Invoke(message);
            DebugConnectionClient.ClientEventSet.Error?.Invoke(message);
        }

        private static void OnRemoteDisplayNameChanged(string displayName)
        {
            _TargetDisplayName = displayName ?? string.Empty;
            TargetInfoChanged?.Invoke(GetTargetInfo());
            DebugConnectionClient.ClientEventSet.RemoteDisplayNameChanged?.Invoke(displayName);
        }

        private static void OnMessageReceived(DebugConnectionMessageEventArgs args)
        {
            DebugConnectionClient.ClientEventSet.OnMessage(args);
        }
    }
}
