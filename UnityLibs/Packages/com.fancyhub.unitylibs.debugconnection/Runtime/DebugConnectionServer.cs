using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace FH
{
    public static class DebugConnectionServer
    {
        public const int DefaultPort = DebugConnectionProtocol.DefaultPort;
        public const int DefaultPortScanCount = DebugConnectionProtocol.DefaultPortScanCount;
        public const int MaxPayloadSize = DebugConnectionProtocol.MaxPayloadSize;

        private static readonly object _MessageLock = new object();
        private static readonly Dictionary<Guid, DebugConnectionMessageHandler> _MessageHandlers =
            new Dictionary<Guid, DebugConnectionMessageHandler>();

        private static readonly object _PeerLock = new object();
        private static readonly Dictionary<int, DebugConnectionPeer> _Peers = new Dictionary<int, DebugConnectionPeer>();

        private static TcpListener _Listener;
        private static Thread _AcceptThread;
        private static volatile bool _Running;
        private static int _NextConnectionId;
        private static byte[] _ServerHelloPayload;

        public static event Action Connected;
        public static event Action Disconnected;
        public static event Action<string> Error;
        public static event Action<DebugConnectionRemoteInfo> RemoteConnected;
        public static event Action<DebugConnectionRemoteInfo> RemoteDisconnected;
        public static event Action<DebugConnectionRemoteInfo> RemoteInfoChanged;

        public static int Port { get; private set; }
        public static string LastError { get; private set; }

        public static bool IsRunning
        {
            get { return _Running; }
        }

        public static bool IsConnected
        {
            get
            {
                lock (_PeerLock)
                {
                    return _Peers.Count > 0;
                }
            }
        }

        public static int ConnectionCount
        {
            get
            {
                lock (_PeerLock)
                {
                    return _Peers.Count;
                }
            }
        }

        public static DebugConnectionServerStartResult StartServer(int port = DefaultPort)
        {
            if (port <= 0 || port > 65535)
            {
                LastError = "Invalid port: " + port;
                return DebugConnectionServerStartResult.InvalidPort;
            }

            if (_Running)
                return DebugConnectionServerStartResult.AlreadyRunning;

            DebugConnectionMainThread.Initialize();

            try
            {
                _ServerHelloPayload = DebugConnectionHello.CreatePayload(DebugConnectionHello.RolePlayer);
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                _Listener = listener;
                _Running = true;
                Port = port;
                LastError = string.Empty;

                _AcceptThread = new Thread(AcceptLoop);
                _AcceptThread.IsBackground = true;
                _AcceptThread.Name = "FH.DebugConnection.Server";
                _AcceptThread.Start();

                Debug.Log("[DebugConnection] Server started on port " + port);
                return DebugConnectionServerStartResult.Started;
            }
            catch (SocketException e)
            {
                LastError = e.SocketErrorCode == SocketError.AddressAlreadyInUse
                    ? "Port " + port + " is in use"
                    : e.Message;
                _Listener = null;
                _Running = false;
                Debug.LogWarning("[DebugConnection] Server start failed: " + LastError);

                if (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    return DebugConnectionServerStartResult.PortInUse;

                return DebugConnectionServerStartResult.SocketError;
            }
        }

        public static DebugConnectionServerStartResult StartServerOnNextFreePort(
            int preferredPort,
            int portCount,
            out int actualPort)
        {
            actualPort = 0;

            if (_Running)
            {
                actualPort = Port;
                return DebugConnectionServerStartResult.AlreadyRunning;
            }

            if (portCount <= 0)
                portCount = 1;

            for (int i = 0; i < portCount; i++)
            {
                int port = preferredPort + i;
                DebugConnectionServerStartResult result = StartServer(port);
                if (result == DebugConnectionServerStartResult.Started)
                {
                    actualPort = port;
                    return result;
                }

                if (result != DebugConnectionServerStartResult.PortInUse)
                    return result;
            }

            LastError = "No free port found from " + preferredPort + " to " + (preferredPort + portCount - 1);
            return DebugConnectionServerStartResult.PortInUse;
        }

        public static void Stop()
        {
            _Running = false;
            LastError = string.Empty;

            try
            {
                _Listener?.Stop();
            }
            catch
            {
            }

            _Listener = null;
            _ServerHelloPayload = null;

            List<DebugConnectionPeer> peers = new List<DebugConnectionPeer>();
            lock (_PeerLock)
            {
                foreach (DebugConnectionPeer peer in _Peers.Values)
                    peers.Add(peer);
                _Peers.Clear();
            }

            foreach (DebugConnectionPeer peer in peers)
                peer.Close();
        }

        public static void Update()
        {
            DebugConnectionMainThread.Update();
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
            return Broadcast(messageId, data) > 0;
        }

        public static bool SendTo(int connectionId, Guid messageId, byte[] data)
        {
            DebugConnectionPeer peer;
            lock (_PeerLock)
            {
                if (!_Peers.TryGetValue(connectionId, out peer))
                    return false;
            }

            return peer.SendMessage(messageId, data);
        }

        public static int Broadcast(Guid messageId, byte[] data)
        {
            List<DebugConnectionPeer> peers = new List<DebugConnectionPeer>();
            lock (_PeerLock)
            {
                foreach (DebugConnectionPeer peer in _Peers.Values)
                    peers.Add(peer);
            }

            int count = 0;
            foreach (DebugConnectionPeer peer in peers)
            {
                if (peer.SendMessage(messageId, data))
                    count++;
            }

            return count;
        }

        public static List<DebugConnectionRemoteInfo> GetConnections()
        {
            List<DebugConnectionRemoteInfo> ret = new List<DebugConnectionRemoteInfo>();
            lock (_PeerLock)
            {
                foreach (DebugConnectionPeer peer in _Peers.Values)
                    ret.Add(CreateRemoteInfo(peer));
            }

            return ret;
        }

        public static List<string> GetLocalIPv4Addresses()
        {
            List<string> ret = new List<string>();

            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress address in hostEntry.AddressList)
                {
                    if (address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address))
                        continue;

                    ret.Add(address.ToString());
                }
            }
            catch (Exception e)
            {
                LastError = e.Message;
            }

            return ret;
        }

        private static void AcceptLoop()
        {
            while (_Running)
            {
                try
                {
                    TcpClient client = _Listener.AcceptTcpClient();
                    client.NoDelay = true;
                    client.SendTimeout = DebugConnectionProtocol.HandshakeTimeoutMilliseconds;
                    NetworkStream stream = client.GetStream();
                    stream.WriteTimeout = DebugConnectionProtocol.HandshakeTimeoutMilliseconds;
                    DebugConnectionProtocol.WriteFrame(
                        stream,
                        DebugConnectionFrame.Create(
                            DebugConnectionFrameType.Hello,
                            Guid.Empty,
                            _ServerHelloPayload));

                    int connectionId = Interlocked.Increment(ref _NextConnectionId);
                    DebugConnectionPeer peer = new DebugConnectionPeer(client, connectionId);
                    peer.FrameReceived += OnFrameReceived;
                    peer.Closed += OnPeerClosed;
                    peer.Error += OnPeerError;

                    lock (_PeerLock)
                    {
                        _Peers[connectionId] = peer;
                    }

                    peer.Start();
                    Debug.Log("[DebugConnection] Accepted editor connection: " + peer.RemoteEndPoint);

                    DebugConnectionRemoteInfo info = CreateRemoteInfo(peer);
                    DebugConnectionMainThread.Post(delegate
                    {
                        Connected?.Invoke();
                        RemoteConnected?.Invoke(info);
                    });
                }
                catch (Exception e)
                {
                    if (_Running)
                    {
                        LastError = e.Message;
                        Debug.LogWarning("[DebugConnection] Accept failed: " + e.Message);
                        DebugConnectionMainThread.Post(delegate { Error?.Invoke(e.Message); });
                    }
                }
            }
        }

        private static void OnFrameReceived(DebugConnectionPeer peer, DebugConnectionFrame frame)
        {
            if (frame.FrameType == DebugConnectionFrameType.Hello)
            {
                peer.SetDisplayName(DebugConnectionHello.ParsePayload(frame.Payload));
                DebugConnectionRemoteInfo info = CreateRemoteInfo(peer);
                DebugConnectionMainThread.Post(delegate { RemoteInfoChanged?.Invoke(info); });
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
                    peer.PlayerId,
                    peer.RemoteEndPoint));
            });
        }

        private static void OnPeerClosed(DebugConnectionPeer peer)
        {
            bool removed;
            lock (_PeerLock)
            {
                removed = _Peers.Remove(peer.PlayerId);
            }

            if (!removed)
                return;

            DebugConnectionRemoteInfo info = CreateRemoteInfo(peer);
            DebugConnectionMainThread.Post(delegate
            {
                Disconnected?.Invoke();
                RemoteDisconnected?.Invoke(info);
            });
        }

        private static void OnPeerError(DebugConnectionPeer peer, string message)
        {
            LastError = message;
            Debug.LogWarning("[DebugConnection] Peer error (" + peer.RemoteEndPoint + "): " + message);
            DebugConnectionMainThread.Post(delegate { Error?.Invoke(message); });
        }

        private static DebugConnectionRemoteInfo CreateRemoteInfo(DebugConnectionPeer peer)
        {
            return new DebugConnectionRemoteInfo
            {
                ConnectionId = peer.PlayerId,
                DisplayName = peer.DisplayName,
                RemoteEndPoint = peer.RemoteEndPoint,
                ConnectedUtc = peer.ConnectedUtc,
                IsConnected = peer.IsConnected,
            };
        }
    }
}
