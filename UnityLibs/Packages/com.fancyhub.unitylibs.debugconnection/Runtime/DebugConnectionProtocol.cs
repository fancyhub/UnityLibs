using System;
using System.IO;

namespace FH
{
    internal static class DebugConnectionProtocol
    {
        public const int DefaultPort = 56020;
        public const int DefaultPortScanCount = 20;
        public const int MaxPayloadSize = 16 * 1024 * 1024;
        public const int HeartbeatIntervalMilliseconds = 5000;
        public const int ReadTimeoutMilliseconds = 20000;
        public const int ConnectTimeoutMilliseconds = 500;
        public const int HandshakeTimeoutMilliseconds = 3000;

        private const int CHeaderSize = 28;
        private const int CMagic = 0x43444846; // FHDC
        private const ushort CVersion = 1;

        public static DebugConnectionFrame ReadFrame(Stream stream)
        {
            byte[] header = new byte[CHeaderSize];
            ReadExact(stream, header, 0, header.Length);

            int magic = ReadInt32(header, 0);
            if (magic != CMagic)
                throw new InvalidDataException("Invalid debug connection frame magic");

            ushort version = ReadUInt16(header, 4);
            if (version != CVersion)
                throw new InvalidDataException("Unsupported debug connection protocol version: " + version);

            DebugConnectionFrameType frameType = (DebugConnectionFrameType)ReadUInt16(header, 6);

            byte[] guidBytes = new byte[16];
            Buffer.BlockCopy(header, 8, guidBytes, 0, guidBytes.Length);
            Guid messageId = new Guid(guidBytes);

            int payloadLength = ReadInt32(header, 24);
            if (payloadLength < 0 || payloadLength > MaxPayloadSize)
                throw new InvalidDataException("Invalid debug connection payload size: " + payloadLength);

            byte[] payload = Array.Empty<byte>();
            if (payloadLength > 0)
            {
                payload = new byte[payloadLength];
                ReadExact(stream, payload, 0, payloadLength);
            }

            return DebugConnectionFrame.Create(frameType, messageId, payload);
        }

        public static void WriteFrame(Stream stream, DebugConnectionFrame frame)
        {
            byte[] payload = frame.Payload ?? Array.Empty<byte>();
            if (payload.Length > MaxPayloadSize)
                throw new InvalidDataException("Debug connection payload too large: " + payload.Length);

            byte[] header = new byte[CHeaderSize];
            WriteInt32(header, 0, CMagic);
            WriteUInt16(header, 4, CVersion);
            WriteUInt16(header, 6, (ushort)frame.FrameType);

            byte[] guidBytes = frame.MessageId.ToByteArray();
            Buffer.BlockCopy(guidBytes, 0, header, 8, guidBytes.Length);

            WriteInt32(header, 24, payload.Length);

            stream.Write(header, 0, header.Length);
            if (payload.Length > 0)
                stream.Write(payload, 0, payload.Length);
            stream.Flush();
        }

        private static void ReadExact(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int readCount = stream.Read(buffer, offset, count);
                if (readCount <= 0)
                    throw new EndOfStreamException("Debug connection stream closed");

                offset += readCount;
                count -= readCount;
            }
        }

        private static int ReadInt32(byte[] buffer, int offset)
        {
            return buffer[offset]
                | (buffer[offset + 1] << 8)
                | (buffer[offset + 2] << 16)
                | (buffer[offset + 3] << 24);
        }

        private static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
        }

        private static void WriteInt32(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        private static void WriteUInt16(byte[] buffer, int offset, ushort value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }
    }
}
