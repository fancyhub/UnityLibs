/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public static class DebugConnectionEditorClient
    {
        private static readonly object _MessageLock = new object();
        private static readonly Dictionary<Guid, DebugConnectionMessageHandler> _MessageHandlers =
            new Dictionary<Guid, DebugConnectionMessageHandler>();

        private static DebugConnectionClient _Client;
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
                DebugConnectionClient client = _Client;
                return client != null && client.IsRunning;
            }
        }

        public static bool IsConnected
        {
            get
            {
                DebugConnectionClient client = _Client;
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

            DebugConnectionClient client = new DebugConnectionClient(
                host,
                port,
                autoPort ? _PortCount : 1,
                reconnectIntervalSeconds,
                DebugConnectionHello.CreatePayload(DebugConnectionHello.RoleEditor),
                DebugConnectionHello.RolePlayer);
            client.Connected += OnConnected;
            client.Disconnected += OnDisconnected;
            client.Error += OnError;
            client.FrameReceived += OnFrameReceived;

            _Client = client;
            client.Start();
            return DebugConnectionClientConnectResult.Started;
        }

        public static void Disconnect()
        {
            DebugConnectionClient client = _Client;
            _Client = null;
            _TargetDisplayName = string.Empty;
            client?.Dispose();
            CleanupConnection();
            TargetInfoChanged?.Invoke(GetTargetInfo());
        }

        private static void CleanupConnection()
        {
            Action cleanup = _CleanupConnection;
            _CleanupConnection = null;
            cleanup?.Invoke();
        }

        public static void Register(Guid messageId, DebugConnectionMessageHandler handler)
        {
            if (handler == null)
                return;

            lock (_MessageLock)
            {
                if (_MessageHandlers.TryGetValue(messageId, out DebugConnectionMessageHandler oldHandler))
                    _MessageHandlers[messageId] = oldHandler + handler;
                else
                    _MessageHandlers.Add(messageId, handler);
            }
        }

        public static void Unregister(Guid messageId, DebugConnectionMessageHandler handler)
        {
            if (handler == null)
                return;

            lock (_MessageLock)
            {
                if (!_MessageHandlers.TryGetValue(messageId, out DebugConnectionMessageHandler oldHandler))
                    return;

                oldHandler -= handler;
                if (oldHandler == null)
                    _MessageHandlers.Remove(messageId);
                else
                    _MessageHandlers[messageId] = oldHandler;
            }
        }

        public static bool Send(Guid messageId, byte[] data)
        {
            DebugConnectionClient client = _Client;
            if (client == null)
                return false;

            return client.Send(messageId, data);
        }

        public static DebugConnectionTargetInfo GetTargetInfo()
        {
            DebugConnectionClient client = _Client;
            return new DebugConnectionTargetInfo
            {
                Host = _Host ?? string.Empty,
                Port = client != null && client.ConnectedPort > 0 ? client.ConnectedPort : _Port,
                StartPort = _Port,
                AutoPort = _AutoPort,
                PortCount = _PortCount,
                DisplayName = _TargetDisplayName ?? string.Empty,
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
        }

        private static void OnDisconnected()
        {
            Disconnected?.Invoke();
            TargetInfoChanged?.Invoke(GetTargetInfo());
        }

        private static void OnError(string message)
        {
            LastError = message;
            Debug.LogWarning("[DebugConnection] Editor connection error: " + message);
            Error?.Invoke(message);
        }

        private static void OnFrameReceived(DebugConnectionFrame frame)
        {
            if (frame.FrameType == DebugConnectionFrameType.Hello)
            {
                _TargetDisplayName = DebugConnectionHello.ParsePayload(frame.Payload);
                DebugConnectionMainThread.Post(delegate { TargetInfoChanged?.Invoke(GetTargetInfo()); });
                return;
            }

            if (frame.FrameType != DebugConnectionFrameType.Message)
                return;

            DebugConnectionMainThread.Post(delegate
            {
                DebugConnectionMessageHandler handler;
                lock (_MessageLock)
                {
                    _MessageHandlers.TryGetValue(frame.MessageId, out handler);
                }

                handler?.Invoke(new DebugConnectionMessageEventArgs(
                    frame.MessageId,
                    frame.Payload,
                    0,
                    GetTargetInfo().RemoteEndPoint));
            });
        }
    }
}
