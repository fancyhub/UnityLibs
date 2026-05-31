using System;

namespace FH
{
    [Serializable]
    public sealed class DebugConnectionHistoryRecord
    {
        public string Name;
        public string Host;
        public int Port;
        public bool AutoPort;
        public int PortCount;
        public long UpdatedTicks;

        public string DisplayName
        {
            get
            {
                string name = string.IsNullOrWhiteSpace(Name) ? Host : Name;
                if (AutoPort)
                    return string.Format("{0} ({1}:{2}-{3})", name, Host, Port, Port + GetPortCount() - 1);

                return string.Format("{0} ({1}:{2})", name, Host, Port);
            }
        }

        public int GetPortCount()
        {
            return PortCount <= 0 ? DebugConnection.DefaultPortScanCount : PortCount;
        }

        public DebugConnectionHistoryRecord Clone()
        {
            return new DebugConnectionHistoryRecord
            {
                Name = Name,
                Host = Host,
                Port = Port,
                AutoPort = AutoPort,
                PortCount = PortCount,
                UpdatedTicks = UpdatedTicks,
            };
        }
    }
}
