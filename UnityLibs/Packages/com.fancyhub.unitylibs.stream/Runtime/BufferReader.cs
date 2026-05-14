/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/05/13
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.IO;

namespace FH
{
    public sealed class BufferReader : Stream
    {
        private static readonly byte[] CEmptyBuffer = Array.Empty<byte>();

        private byte[] _Buffer = CEmptyBuffer;
        private int _Offset;
        private int _Length;
        private int _Position;

        public BufferReader()
        {
        }

        public BufferReader(byte[] buffer)
        {
            SetBuffer(buffer);
        }

        public BufferReader(byte[] buffer, int offset, int count)
        {
            SetBuffer(buffer, offset, count);
        }

        public void SetBuffer(byte[] buffer)
        {
            if (buffer == null)
            {
                _Buffer = CEmptyBuffer;
                _Offset = 0;
                _Length = 0;
                _Position = 0;
                return;
            }

            SetBuffer(buffer, 0, buffer.Length);
        }

        public void SetBuffer(byte[] buffer, int count)
        {
            SetBuffer(buffer, 0, count);
        }

        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                if (offset != 0 || count != 0)
                    throw new ArgumentNullException(nameof(buffer));

                _Buffer = CEmptyBuffer;
                _Offset = 0;
                _Length = 0;
                _Position = 0;
                return;
            }

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count)
                throw new ArgumentException("offset and count are out of range");

            _Buffer = buffer;
            _Offset = offset;
            _Length = count;
            _Position = 0;
        }

        public byte[] Buffer => _Buffer;

        public int BufferOffset => _Offset;

        public int BufferCount => _Length;

        public int Remaining => _Position >= _Length ? 0 : _Length - _Position;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _Length;

        public override long Position
        {
            get => _Position;
            set
            {
                if (value < 0 || value > int.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _Position = (int)value;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count)
                throw new ArgumentException("offset and count are out of range");

            int read_count = Math.Min(count, Remaining);
            if (read_count <= 0)
                return 0;

            System.Buffer.BlockCopy(_Buffer, _Offset + _Position, buffer, offset, read_count);
            _Position += read_count;
            return read_count;
        }

        public override int ReadByte()
        {
            if (_Position >= _Length)
                return -1;

            return _Buffer[_Offset + _Position++];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    pos = offset;
                    break;

                case SeekOrigin.Current:
                    pos = _Position + offset;
                    break;

                case SeekOrigin.End:
                    pos = _Length + offset;
                    break;

                default:
                    throw new ArgumentException("Invalid seek origin", nameof(origin));
            }

            if (pos < 0)
                throw new IOException("Cannot seek before the beginning of the buffer");
            if (pos > int.MaxValue)
                throw new IOException("Cannot seek beyond Int32.MaxValue");

            _Position = (int)pos;
            return _Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("BufferReader is read-only");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("BufferReader is read-only");
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException("BufferReader is read-only");
        }
    }
}
