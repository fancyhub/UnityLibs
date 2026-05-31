using System;

namespace FH
{
    public sealed class DebugConnectionTargetInfo
    {
        public string Host;
        public int Port;
        public int StartPort;
        public bool AutoPort;
        public int PortCount;
        public string DisplayName;
        public string RemoteEndPoint;
        public DateTime ConnectedUtc;
        public bool IsConnected;
    }
}
