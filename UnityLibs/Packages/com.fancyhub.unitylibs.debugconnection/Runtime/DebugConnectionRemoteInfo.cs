/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

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
