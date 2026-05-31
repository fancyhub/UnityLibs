using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace FH
{
    internal sealed class DebugConnectionPeer : IDisposable
    {
        private readonly TcpClient _Client;
        private readonly Queue<DebugConnectionFrame> _SendQueue = new Queue<DebugConnectionFrame>();
        private readonly AutoResetEvent _SendSignal = new AutoResetEvent(false);
        private readonly ManualResetEvent _ClosedSignal = new ManualResetEvent(false);

        private NetworkStream _Stream;
        private Thread _ReadThread;
        private Thread _WriteThread;
        private volatile bool _Running;
        private int _CloseSignaled;

        public readonly int PlayerId;
        public readonly DateTime ConnectedUtc;
        public readonly string RemoteEndPoint;

        public string DisplayName { get; private set; }

        public bool IsConnected
        {
            get { return _Running && _Client != null && _Client.Connected; }
        }

        public event Action<DebugConnectionPeer, DebugConnectionFrame> FrameReceived;
        public event Action<DebugConnectionPeer> Closed;
        public event Action<DebugConnectionPeer, string> Error;

        public DebugConnectionPeer(TcpClient client, int playerId)
        {
            _Client = client;
            PlayerId = playerId;
            ConnectedUtc = DateTime.UtcNow;
            RemoteEndPoint = client == null || client.Client == null || client.Client.RemoteEndPoint == null
                ? string.Empty
                : client.Client.RemoteEndPoint.ToString();
            DisplayName = RemoteEndPoint;
        }

        public void Start()
        {
            if (_Client == null)
                throw new InvalidOperationException("Debug connection peer has no TcpClient");

            _Client.NoDelay = true;
            _Client.ReceiveTimeout = DebugConnectionProtocol.ReadTimeoutMilliseconds;
            _Stream = _Client.GetStream();
            _Running = true;

            _ReadThread = new Thread(ReadLoop);
            _ReadThread.IsBackground = true;
            _ReadThread.Name = "FH.DebugConnection.Read";
            _ReadThread.Start();

            _WriteThread = new Thread(WriteLoop);
            _WriteThread.IsBackground = true;
            _WriteThread.Name = "FH.DebugConnection.Write";
            _WriteThread.Start();
        }

        public void SetDisplayName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return;

            DisplayName = displayName;
        }

        public bool SendMessage(Guid messageId, byte[] payload)
        {
            return Enqueue(DebugConnectionFrame.Create(DebugConnectionFrameType.Message, messageId, payload));
        }

        public bool SendControl(DebugConnectionFrameType frameType, byte[] payload)
        {
            return Enqueue(DebugConnectionFrame.Create(frameType, Guid.Empty, payload));
        }

        public void WaitForClosed()
        {
            _ClosedSignal.WaitOne();
        }

        public void Close()
        {
            if (Interlocked.Exchange(ref _CloseSignaled, 1) != 0)
                return;

            _Running = false;

            try
            {
                _Stream?.Close();
            }
            catch
            {
            }

            try
            {
                _Client?.Close();
            }
            catch
            {
            }

            _SendSignal.Set();
            _ClosedSignal.Set();
            Closed?.Invoke(this);
        }

        public void Dispose()
        {
            Close();
            _SendSignal.Dispose();
            _ClosedSignal.Dispose();
        }

        private bool Enqueue(DebugConnectionFrame frame)
        {
            if (!_Running)
                return false;

            byte[] payload = frame.Payload ?? Array.Empty<byte>();
            if (payload.Length > DebugConnectionProtocol.MaxPayloadSize)
            {
                Error?.Invoke(this, "Debug connection payload too large: " + payload.Length);
                return false;
            }

            if (payload.Length > 0)
            {
                byte[] payloadCopy = new byte[payload.Length];
                Buffer.BlockCopy(payload, 0, payloadCopy, 0, payload.Length);
                frame.Payload = payloadCopy;
            }

            lock (_SendQueue)
            {
                _SendQueue.Enqueue(frame);
            }

            _SendSignal.Set();
            return true;
        }

        private void ReadLoop()
        {
            try
            {
                while (_Running)
                {
                    DebugConnectionFrame frame = DebugConnectionProtocol.ReadFrame(_Stream);
                    if (frame.FrameType == DebugConnectionFrameType.Ping)
                    {
                        SendControl(DebugConnectionFrameType.Pong, null);
                        continue;
                    }

                    if (frame.FrameType == DebugConnectionFrameType.Pong)
                        continue;

                    FrameReceived?.Invoke(this, frame);
                }
            }
            catch (Exception e)
            {
                if (_Running && !IsExpectedCloseException(e))
                    Error?.Invoke(this, e.Message);
            }
            finally
            {
                Close();
            }
        }

        private void WriteLoop()
        {
            DateTime nextHeartbeatUtc = DateTime.UtcNow.AddMilliseconds(DebugConnectionProtocol.HeartbeatIntervalMilliseconds);

            try
            {
                while (_Running)
                {
                    if (TryDequeue(out DebugConnectionFrame frame))
                    {
                        DebugConnectionProtocol.WriteFrame(_Stream, frame);
                        continue;
                    }

                    int waitMilliseconds = GetWaitMilliseconds(nextHeartbeatUtc);
                    _SendSignal.WaitOne(waitMilliseconds);

                    if (!_Running)
                        break;

                    if (DateTime.UtcNow >= nextHeartbeatUtc)
                    {
                        DebugConnectionProtocol.WriteFrame(
                            _Stream,
                            DebugConnectionFrame.Create(DebugConnectionFrameType.Ping, Guid.Empty, null));
                        nextHeartbeatUtc = DateTime.UtcNow.AddMilliseconds(DebugConnectionProtocol.HeartbeatIntervalMilliseconds);
                    }
                }
            }
            catch (Exception e)
            {
                if (_Running && !IsExpectedCloseException(e))
                    Error?.Invoke(this, e.Message);
            }
            finally
            {
                Close();
            }
        }

        private bool TryDequeue(out DebugConnectionFrame frame)
        {
            lock (_SendQueue)
            {
                if (_SendQueue.Count > 0)
                {
                    frame = _SendQueue.Dequeue();
                    return true;
                }
            }

            frame = default;
            return false;
        }

        private static int GetWaitMilliseconds(DateTime nextHeartbeatUtc)
        {
            double wait = (nextHeartbeatUtc - DateTime.UtcNow).TotalMilliseconds;
            if (wait < 1)
                return 1;

            if (wait > 1000)
                return 1000;

            return (int)wait;
        }

        private static bool IsExpectedCloseException(Exception e)
        {
            return e is ObjectDisposedException
                || e is EndOfStreamException
                || e is IOException
                || e is SocketException;
        }
    }
}
