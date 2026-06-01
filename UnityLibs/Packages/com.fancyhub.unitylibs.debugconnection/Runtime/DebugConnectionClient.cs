/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public interface IDebugConnectionClient : IDisposable
    {
        bool IsRunning { get; }
        bool IsConnected { get; }
        string RemoteEndPoint { get; }
        DateTime ConnectedUtc { get; }
        int ConnectedPort { get; }
        string LastError { get; }
        string RemoteDisplayName { get; }

        event Action Connected;
        event Action Disconnected;
        event Action<string> Error;
        event Action<string> RemoteDisplayNameChanged;
        event DebugConnectionMessageHandler MessageReceived;

        bool Send(Guid messageId, byte[] data);
    }

    internal class DebugConnectionClientEventSet
    {
        private readonly object _MessageLock = new object();
        private readonly System.Collections.Generic.Dictionary<Guid, DebugConnectionMessageHandler> _MessageHandlers =
            new System.Collections.Generic.Dictionary<Guid, DebugConnectionMessageHandler>();

        public Action Connected;
        public Action Disconnected;
        public Action<string> Error;
        public Action<string> RemoteDisplayNameChanged;

        public void Register(Guid messageId, DebugConnectionMessageHandler handler)
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

        public void Unregister(Guid messageId, DebugConnectionMessageHandler handler)
        {
            if (handler == null)
                return;

            lock (_MessageLock)
            {
                if (_MessageHandlers.TryGetValue(messageId, out DebugConnectionMessageHandler oldHandler))
                {
                    oldHandler -= handler;
                    if (oldHandler == null)
                        _MessageHandlers.Remove(messageId);
                    else
                        _MessageHandlers[messageId] = oldHandler;
                }
            }
        }

        public void OnMessage(DebugConnectionMessageEventArgs msg)
        {
            DebugConnectionMessageHandler handler;

            lock (_MessageLock)
            {
                _MessageHandlers.TryGetValue(msg.MessageId, out handler);
            }

            handler?.Invoke(msg);
        }
    }

    public static class DebugConnectionClient
    {

        private static IDebugConnectionClient _Client;
        internal static DebugConnectionClientEventSet _ClientEventSet = new DebugConnectionClientEventSet();

        public static bool IsRunning { get { return _Client != null && _Client.IsRunning; } }
        public static bool IsConnected { get { return _Client != null && _Client.IsConnected; } }
        public static string RemoteEndPoint { get { return _Client == null ? string.Empty : _Client.RemoteEndPoint; } }
        public static DateTime ConnectedUtc { get { return _Client == null ? default : _Client.ConnectedUtc; } }
        public static int ConnectedPort { get { return _Client == null ? 0 : _Client.ConnectedPort; } }
        public static string LastError { get { return _Client == null ? string.Empty : _Client.LastError; } }
        public static string RemoteDisplayName { get { return _Client == null ? string.Empty : _Client.RemoteDisplayName; } }

        public static event Action Connected;
        public static event Action Disconnected;
        public static event Action<string> Error;
        public static event Action<string> RemoteDisplayNameChanged;

        static DebugConnectionClient()
        {
            _ClientEventSet.Connected = () =>
            {
                Connected?.Invoke();
            };

            _ClientEventSet.Disconnected = () =>
            {
                Disconnected?.Invoke();
            };

            _ClientEventSet.Error = (msg) =>
            {
                Error?.Invoke(msg);
            };

            _ClientEventSet.RemoteDisplayNameChanged = (msg) =>
            {
                RemoteDisplayNameChanged?.Invoke(msg);
            };
        }

        public static IDebugConnectionClient Current
        {
            get { return _Client; }
        }

        internal static void SetClient(IDebugConnectionClient client)
        {
            Disconnect();
            _Client = client;
        }

        internal static DebugConnectionClientEventSet ClientEventSet => _ClientEventSet;

        public static void Disconnect()
        {
            IDebugConnectionClient client = _Client;
            _Client = null;
            if (client == null)
                return;
            client.Dispose();
        }

        public static bool Send(Guid messageId, byte[] data)
        {
            return _Client != null && _Client.Send(messageId, data);
        }

        public static bool Send(Guid messageId, string data)
        {
            if (_Client == null)
                return false;
            if (string.IsNullOrEmpty(data))
                return Send(messageId, Array.Empty<byte>());
            else
                return Send(messageId, System.Text.Encoding.UTF8.GetBytes(data));
        }

        public static void Register(Guid messageId, DebugConnectionMessageHandler handler)
        {
            if (handler == null)
                return;
            _ClientEventSet.Register(messageId, handler);
        }

        public static void Unregister(Guid messageId, DebugConnectionMessageHandler handler)
        {
            if (handler == null)
                return;
            _ClientEventSet.Unregister(messageId, handler);
        }
    }

    [System.Serializable]
    public struct DebugConnectionButton
    {
    }
}
