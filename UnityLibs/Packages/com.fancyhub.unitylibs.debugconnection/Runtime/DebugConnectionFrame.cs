/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    internal enum DebugConnectionFrameType : ushort
    {
        Hello = 1,
        Message = 2,
        Ping = 3,
        Pong = 4,
    }

    internal struct DebugConnectionFrame
    {
        public DebugConnectionFrameType FrameType;
        public Guid MessageId;
        public byte[] Payload;

        public static DebugConnectionFrame Create(DebugConnectionFrameType frameType, Guid messageId, byte[] payload)
        {
            return new DebugConnectionFrame
            {
                FrameType = frameType,
                MessageId = messageId,
                Payload = payload ?? Array.Empty<byte>(),
            };
        }
    }
}
