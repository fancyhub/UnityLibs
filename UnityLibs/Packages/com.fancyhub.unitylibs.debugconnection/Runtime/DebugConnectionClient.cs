/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace FH
{
    internal sealed class DebugConnectionClient : IDisposable
    {
        private readonly string _Host;
        private readonly int _Port;
        private readonly int _PortCount;
        private readonly int _ReconnectIntervalMilliseconds;
        private readonly byte[] _HelloPayload;
        private readonly string _ExpectedRemoteRole;

        private Thread _Thread;
        private volatile bool _Running;
        private DebugConnectionPeer _Peer;
        private int _ConnectedPort;

        public bool IsRunning
        {
            get { return _Running; }
        }

        public bool IsConnected
        {
            get
            {
                DebugConnectionPeer peer = _Peer;
                return peer != null && peer.IsConnected;
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                DebugConnectionPeer peer = _Peer;
                return peer == null ? string.Empty : peer.RemoteEndPoint;
            }
        }

        public DateTime ConnectedUtc
        {
            get
            {
                DebugConnectionPeer peer = _Peer;
                return peer == null ? default : peer.ConnectedUtc;
            }
        }

        public int ConnectedPort
        {
            get { return _ConnectedPort; }
        }

        public event Action Connected;
        public event Action Disconnected;
        public event Action<DebugConnectionFrame> FrameReceived;
        public event Action<string> Error;

        public DebugConnectionClient(
            string host,
            int port,
            int portCount,
            float reconnectIntervalSeconds,
            byte[] helloPayload,
            string expectedRemoteRole)
        {
            _Host = host;
            _Port = port;
            _PortCount = Math.Max(1, portCount);
            _ReconnectIntervalMilliseconds = Math.Max(500, (int)(reconnectIntervalSeconds * 1000));
            _HelloPayload = helloPayload ?? Array.Empty<byte>();
            _ExpectedRemoteRole = expectedRemoteRole;
        }

        public void Start()
        {
            if (_Running)
                return;

            _Running = true;
            _Thread = new Thread(ConnectionLoop);
            _Thread.IsBackground = true;
            _Thread.Name = "FH.DebugConnection.Client";
            _Thread.Start();
        }

        public bool Send(Guid messageId, byte[] data)
        {
            DebugConnectionPeer peer = _Peer;
            if (peer == null)
                return false;

            return peer.SendMessage(messageId, data);
        }

        public void Dispose()
        {
            _Running = false;

            DebugConnectionPeer peer = _Peer;
            _Peer = null;
            peer?.Close();
        }

        private void ConnectionLoop()
        {
            while (_Running)
            {
                DebugConnectionPeer peer = null;

                try
                {
                    if (!TryConnect(out peer, out string error))
                    {
                        if (_Running)
                            DebugConnectionMainThread.Post(delegate { Error?.Invoke(error); });

                        WaitReconnectInterval();
                        continue;
                    }

                    peer.FrameReceived += OnFrameReceived;
                    peer.Closed += OnPeerClosed;
                    peer.Error += OnPeerError;

                    _Peer = peer;
                    peer.Start();

                    DebugConnectionMainThread.Post(delegate { Connected?.Invoke(); });
                    peer.WaitForClosed();
                }
                catch (Exception e)
                {
                    if (_Running)
                        DebugConnectionMainThread.Post(delegate { Error?.Invoke(e.Message); });

                    peer?.Close();
                }
                finally
                {
                    if (ReferenceEquals(_Peer, peer))
                        _Peer = null;
                }

                if (_Running)
                    WaitReconnectInterval();
            }
        }

        private bool TryConnect(out DebugConnectionPeer peer, out string error)
        {
            peer = null;
            error = string.Empty;

            int portCount = Math.Max(1, _PortCount);
            for (int i = 0; i < portCount && _Running; i++)
            {
                int port = _Port + i;
                if (port <= 0 || port > 65535)
                    continue;

                if (TryConnectPort(port, out peer, out error))
                    return true;
            }

            if (string.IsNullOrEmpty(error))
                error = "No debug connection server found";
            else if (portCount > 1)
                error = "No debug connection server found from port " + _Port + " to " + (_Port + portCount - 1);

            return false;
        }

        private bool TryConnectPort(int port, out DebugConnectionPeer peer, out string error)
        {
            peer = null;
            error = string.Empty;

            TcpClient client = null;
            try
            {
                client = new TcpClient();
                client.NoDelay = true;
                ConnectWithTimeout(client, _Host, port, DebugConnectionProtocol.ConnectTimeoutMilliseconds);
                client.ReceiveTimeout = DebugConnectionProtocol.HandshakeTimeoutMilliseconds;
                client.SendTimeout = DebugConnectionProtocol.HandshakeTimeoutMilliseconds;

                NetworkStream stream = client.GetStream();
                stream.ReadTimeout = DebugConnectionProtocol.HandshakeTimeoutMilliseconds;
                stream.WriteTimeout = DebugConnectionProtocol.HandshakeTimeoutMilliseconds;

                DebugConnectionProtocol.WriteFrame(
                    stream,
                    DebugConnectionFrame.Create(DebugConnectionFrameType.Hello, Guid.Empty, _HelloPayload));

                DebugConnectionFrame frame = DebugConnectionProtocol.ReadFrame(stream);
                if (frame.FrameType != DebugConnectionFrameType.Hello
                    || !DebugConnectionHello.IsRole(frame.Payload, _ExpectedRemoteRole))
                {
                    error = "Port " + port + " is not a debug connection player. Remote hello: "
                        + DebugConnectionHello.ParsePayload(frame.Payload);
                    client.Close();
                    return false;
                }

                _ConnectedPort = port;
                peer = new DebugConnectionPeer(client, 0);
                peer.SetDisplayName(DebugConnectionHello.ParsePayload(frame.Payload));
                return true;
            }
            catch (Exception e)
            {
                error = "Port " + port + ": " + e.Message;

                try
                {
                    client?.Close();
                }
                catch
                {
                }

                return false;
            }
        }

        private static void ConnectWithTimeout(TcpClient client, string host, int port, int timeoutMilliseconds)
        {
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            bool connected = result.AsyncWaitHandle.WaitOne(timeoutMilliseconds);
            result.AsyncWaitHandle.Close();

            if (!connected)
                throw new TimeoutException("Connect timeout");

            client.EndConnect(result);
        }

        private void OnFrameReceived(DebugConnectionPeer peer, DebugConnectionFrame frame)
        {
            FrameReceived?.Invoke(frame);
        }

        private void OnPeerClosed(DebugConnectionPeer peer)
        {
            if (ReferenceEquals(_Peer, peer))
                _Peer = null;
            _ConnectedPort = 0;

            if (_Running)
                DebugConnectionMainThread.Post(delegate { Disconnected?.Invoke(); });
        }

        private void OnPeerError(DebugConnectionPeer peer, string message)
        {
            DebugConnectionMainThread.Post(delegate { Error?.Invoke(message); });
        }

        private void WaitReconnectInterval()
        {
            int elapsed = 0;
            while (_Running && elapsed < _ReconnectIntervalMilliseconds)
            {
                Thread.Sleep(100);
                elapsed += 100;
            }
        }
    }


    [System.Serializable]
    public struct DebugConnectionButton
    {
    }
}
