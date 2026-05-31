using System;

namespace FH
{
    public sealed class DebugConnectionRemoteInfo
    {
        public int ConnectionId;
        public string DisplayName;
        public string RemoteEndPoint;
        public DateTime ConnectedUtc;
        public bool IsConnected;
    }
}
