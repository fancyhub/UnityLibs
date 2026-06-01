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

        private string _StringData;
        public string StringData
        {
            get
            {
                if (_StringData == null)
                {
                    if (Data == null || Data.Length == 0)
                        return string.Empty;
                    try
                    {
                        _StringData = System.Text.Encoding.UTF8.GetString(Data);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }
                return _StringData;
            }
        }


        public DebugConnectionMessageEventArgs(Guid messageId, byte[] data, int connectionId, string remoteEndPoint)
        {
            MessageId = messageId;
            Data = data ?? Array.Empty<byte>();
            ConnectionId = connectionId;
            RemoteEndPoint = remoteEndPoint ?? string.Empty;
        }
    }
}
