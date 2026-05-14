/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/03/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FH
{
    /// <summary>
    /// 这个是为了解决序列化的时候使用的
    /// 尤其是PB, 或者其他的格式, 需要现写变长的长度, 再写block
    /// 这个类是通过先写到缓存Stream里面, 再重新调整位置, 写会到目标stream里面
    /// </summary>
    public sealed class BlockWriter
    {
        private MemoryStream _Stream;

        private enum ESegment
        {
            Block,
            Size,
        }

        private struct Segment
        {
            public ESegment Type;
            public int Offset;
            public int Length;

            public int BlockStartPos;
        }

        private Stack<int> _SizeSegmentStack = new();
        private List<Segment> _Segments = new();

        public BlockWriter()
        {
            _Stream = new MemoryStream();
        }

        public BlockWriter(int initCapacity)
        {
            _Stream = new MemoryStream(initCapacity);
        }

        public void Begin()
        {
            _Clear();

            _Segments.Add(new Segment()
            {
                Type = ESegment.Block,
                Offset = 0,
                Length = 0,
            });
        }

        public void End(Stream target)
        {
            //1. 修正最后的block seg
            var last = _Segments[_Segments.Count - 1];
            if (last.Type == ESegment.Block)
            {
                last.Length = (int)(_Stream.Position - last.Offset);
                _Segments[_Segments.Count - 1] = last;
            }

            if (_SizeSegmentStack.Count != 0)
                throw new InvalidOperationException("BlockWriter has unclosed block");

            byte[] buf = _Stream.GetBuffer();
            int write_count = 0;
            for (int i = 0; i < _Segments.Count; i++)
            {
                var seg = _Segments[i];
                if (seg.Length == 0)
                    continue;

                target.Write(buf, seg.Offset, seg.Length);
                write_count += seg.Length;
            }

            Log.Assert(write_count == _Stream.Length, "size is not correct {0}:{1}", write_count, _Stream.Length);
            _Clear();
        }

        public void WriteByte(byte b)
        {
            _Stream.WriteByte(b);
        }

        public void Write(byte[] buff, int offset, int count)
        {
            _Stream.Write(buff, offset, count);
        }

        public void Write(ReadOnlySpan<byte> buff)
        {
            _Stream.Write(buff);
        }

        public void WriteVariant(uint value)
        {
            while (value > 127)
            {
                byte vv = (byte)((value & 0x7F) | 0x80);
                _Stream.WriteByte(vv);
                value >>= 7;
            }
            _Stream.WriteByte((byte)value);
        }

        public void WriteVariant(ulong value)
        {
            while (value > 127)
            {
                byte vv = (byte)((value & 0x7F) | 0x80);
                _Stream.WriteByte(vv);
                value >>= 7;
            }
            _Stream.WriteByte((byte)value);
        }


        public void WriteRaw(uint value, bool littleEndian = true)
        {
            Span<byte> buff = stackalloc byte[4];
            BitConverter.TryWriteBytes(buff, value);
            if (BitConverter.IsLittleEndian != littleEndian)
                _Reverse4(buff);
            _Stream.Write(buff);
        }

        public void WriteRaw(ulong value, bool littleEndian = true)
        {
            Span<byte> buff = stackalloc byte[8];
            BitConverter.TryWriteBytes(buff, value);
            if (BitConverter.IsLittleEndian != littleEndian)
                _Reverse8(buff);
            _Stream.Write(buff);
        }


        public void WriteRaw(float value, bool littleEndian = true)
        {
            Span<byte> buff = stackalloc byte[4];
            BitConverter.TryWriteBytes(buff, value);
            if (BitConverter.IsLittleEndian != littleEndian)
                _Reverse4(buff);
            _Stream.Write(buff);
        }

        public void WriteRaw(double value, bool littleEndian = true)
        {
            Span<byte> buff = stackalloc byte[8];
            BitConverter.TryWriteBytes(buff, value);
            if (BitConverter.IsLittleEndian != littleEndian)
                _Reverse8(buff);
            _Stream.Write(buff);
        }

        public void BeginBlock()
        {
            //1. 修正上一个Block的size
            var last = _Segments[_Segments.Count - 1];
            if (last.Type == ESegment.Block)
            {
                last.Length = (int)(_Stream.Position - last.Offset);
                _Segments[_Segments.Count - 1] = last;
            }

            //2. 把size的写入
            _SizeSegmentStack.Push(_Segments.Count);
            _Segments.Add(new Segment()
            {
                Type = ESegment.Size,
                BlockStartPos = (int)_Stream.Position,
            });


            //3. 写入新的block
            _Segments.Add(new Segment()
            {
                Type = ESegment.Block,
                Offset = (int)_Stream.Position,
                Length = 0,
            });
        }

        public void EndBlock()
        {
            //1. 修正最后的block seg
            var last = _Segments[_Segments.Count - 1];
            if (last.Type == ESegment.Block)
            {
                last.Length = (int)(_Stream.Position - last.Offset);
                _Segments[_Segments.Count - 1] = last;
            }

            //2. 修正对应的size seg
            int index = _SizeSegmentStack.Pop();
            var sizeSeg = _Segments[index];
            sizeSeg.Offset = (int)_Stream.Position;
            int blockSize = sizeSeg.Offset - sizeSeg.BlockStartPos;
            WriteVariant((uint)blockSize);
            sizeSeg.Length = (int)(_Stream.Position - sizeSeg.Offset);
            _Segments[index] = sizeSeg;


            //3. 插入新的block
            _Segments.Add(new Segment()
            {
                Type = ESegment.Block,
                Offset = (int)_Stream.Position,
                Length = 0,
            });
        }

        private void _Clear()
        {
            _Stream.SetLength(0);
            _Stream.Position = 0;
            _SizeSegmentStack.Clear();
            _Segments.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void _Reverse4(Span<byte> buff)
        {
            (buff[0], buff[3]) = (buff[3], buff[0]);
            (buff[1], buff[2]) = (buff[2], buff[1]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void _Reverse8(Span<byte> buff)
        {
            (buff[0], buff[7]) = (buff[7], buff[0]);
            (buff[1], buff[6]) = (buff[6], buff[1]);
            (buff[2], buff[5]) = (buff[5], buff[2]);
            (buff[3], buff[4]) = (buff[4], buff[3]);
        }
    }
}
