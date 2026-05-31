/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public delegate void DebugConnectionMessageHandler(DebugConnectionMessageEventArgs args);

    public sealed class DebugConnectionMessageEventArgs : EventArgs
    {
        public readonly Guid MessageId;
        public readonly byte[] Data;
        public readonly int ConnectionId;
        public readonly string RemoteEndPoint;

        public DebugConnectionMessageEventArgs(Guid messageId, byte[] data, int connectionId, string remoteEndPoint)
        {
            MessageId = messageId;
            Data = data ?? Array.Empty<byte>();
            ConnectionId = connectionId;
            RemoteEndPoint = remoteEndPoint ?? string.Empty;
        }
    }
}
