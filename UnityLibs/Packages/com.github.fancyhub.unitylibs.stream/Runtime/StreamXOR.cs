/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/03/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace FH.FileManagement
{
    public sealed class StreamXOR : System.IO.Stream
    {
        private const int TempWriteBuffLen = 512;
        private Stream _OrigStream;
        private byte[] _Keys;
        private int _KeyLen;

        public StreamXOR(Stream orig, byte[] xor_keys)
        {
            _OrigStream = orig;
            _Keys = xor_keys;
            if (_Keys != null)
                _KeyLen = xor_keys.Length;
        }

        public StreamXOR(Stream orig, string xor_keys)
        {
            System.IO.BufferedStream s;
            _OrigStream = orig;
            if (!string.IsNullOrEmpty(xor_keys))
            {
                _Keys = System.Text.Encoding.UTF8.GetBytes(xor_keys);
                _KeyLen = xor_keys.Length;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && _OrigStream != null)
                {
                    try
                    {
                        Flush();
                    }
                    finally
                    {
                        _OrigStream.Close();
                    }
                }
            }
            finally
            {
                _OrigStream = null;
                // Call base.Dispose(bool) to cleanup async IO resources
                base.Dispose(disposing);
            }
        }

        public override bool CanRead => _OrigStream.CanRead;

        public override bool CanSeek => _OrigStream.CanSeek;

        public override bool CanWrite => _OrigStream.CanWrite;

        public override long Length => _OrigStream.Length;

        public override long Position { get => _OrigStream.Position; set => _OrigStream.Position = value; }

        public override void Flush()
        {
            _OrigStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_KeyLen == 0)
                return _OrigStream.Read(buffer, offset, count);


            long pos = _OrigStream.Position;
            int ret = _OrigStream.Read(buffer, offset, count);
            if (ret == 0)
                return 0;

            for (int i = 0; i < ret; i++)
            {
                buffer[i + offset] ^= _Keys[(pos + i) % _KeyLen];
            }
            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _OrigStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _OrigStream.SetLength(value);
        }

        public unsafe override void Write(byte[] buffer, int offset, int count)
        {
            if (_KeyLen == 0 || count <= 0)
            {
                _OrigStream.Write(buffer, offset, count);
                return;
            }

            int temp_buff_count = Math.Min(count, TempWriteBuffLen);
            Span<byte> temp_buff = stackalloc byte[temp_buff_count];

            long pos = _OrigStream.Position;
            for (; ; )
            {
                int copy_count = Math.Min(temp_buff_count, count);
                if (copy_count <= 0)
                    return;

                for (int i = 0; i < copy_count; i++)
                {
                    temp_buff[i] = buffer[offset + i];
                    temp_buff[i] ^= _Keys[(pos + i) % _KeyLen];
                }

                _OrigStream.Write(temp_buff.Slice(0, copy_count));

                offset += copy_count;
                count -= copy_count;
                pos += copy_count;
            }
        }
    }
}
