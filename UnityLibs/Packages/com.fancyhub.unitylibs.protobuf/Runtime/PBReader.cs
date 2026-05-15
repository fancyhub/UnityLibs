/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2019/8/7
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace FH
{

    internal sealed class PBSegReader
    {
        private const ulong C_VARIANT_LAST = ~(ulong)0x01;
        private Stack<int> _limit_stack = new Stack<int>();
        private int _cur_end_pos = int.MaxValue;
        private int _cur_pos = 0;

        private Stream _Stream;

        public void SetStream(Stream stream, int len)
        {
            _Stream = stream;
            _cur_end_pos = len;
            _cur_pos = 0;
            _limit_stack.Clear();
        }

        public bool TryPopLimit(out int remain_len)
        {
            remain_len = 0;
            if (_limit_stack.Count == 0)
            {
                PBLog._.E("PopLimit: Stack is empty");
                return false;
            }

            remain_len = BytesUntilLimit();
            bool ret = true;
            if (remain_len > 0 && !Skip(remain_len))
            {
                PBLog._.E("PopLimit: Remain: {0}", remain_len);
                ret = false;
            }

            _cur_end_pos = _limit_stack.Pop();
            PBLog._.D("PushLimit: Being StackCount: {0} -> {1}, Remain:{2}", _limit_stack.Count + 1, _limit_stack.Count, remain_len);
            return ret;
        }

        public bool TryPushLimit(int len)
        {

            if (len < 0)
            {
                PBLog._.E("PushLimit: Len is 0");
                return false;
            }

            int remain = _cur_end_pos - _cur_pos;
            if (remain < len)
            {
                PBLog._.E("PushLimit: Len: {0} > {1}", len, remain);
                return false;
            }
            PBLog._.D("PushLimit: Being StackCount: {0} -> {1}, Length:{2}", _limit_stack.Count, _limit_stack.Count + 1, len);
            _limit_stack.Push(_cur_end_pos);
            _cur_end_pos = _cur_pos + len;
            return true;
        }

        public int BytesUntilLimit()
        {
            return _cur_end_pos - _cur_pos;
        }

        public bool TryReadVariant(out ulong v)
        {
            v = 0;
            ulong chunk = 0;


            if (!TryReadByte(out chunk)) return false;
            v = chunk & 0x7FU;
            if ((chunk & 0x80) == 0) return true;


            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return true;


            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return true;

            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return true;

            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 28;
            if ((chunk & 0x80) == 0) return true;


            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 35;
            if ((chunk & 0x80) == 0) return true;


            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 42;
            if ((chunk & 0x80) == 0) return true;

            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 49;
            if ((chunk & 0x80) == 0) return true;

            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 56;
            if ((chunk & 0x80) == 0) return true;


            if (!TryReadByte(out chunk)) return false;
            v |= (chunk & 0x7F) << 63;
            if ((chunk & C_VARIANT_LAST) != 0) return false;


            return true;
        }

        public bool TryReadByte(out ulong c)
        {
            Span<byte> temp = stackalloc byte[1];
            if (!ReadBuff(temp))
            {
                c = default;
                return false;
            }
            c = temp[0];
            return true;
        }

        public bool TryReadByte(out byte c)
        {
            Span<byte> temp = stackalloc byte[1];
            if (!ReadBuff(temp))
            {
                c = default;
                return false;
            }
            c = temp[0];
            return true;
        }

        public bool ReadBuff(byte[] buff, int offset, int count)
        {
            return ReadBuff(new Span<byte>(buff, offset, count));
        }

        public bool ReadBuff(Span<byte> buff)
        {
            if (buff.Length == 0)
                return true;

            int reamin = _cur_end_pos - _cur_pos;
            if (reamin < buff.Length)
                return false;

            int readed_count = 0;
            while (readed_count < buff.Length)
            {
                var t = buff.Slice(readed_count, buff.Length - readed_count);
                int read_size = _Stream.Read(t);
                if (read_size <= 0)
                    return false;

                readed_count += read_size;
                _cur_pos += read_size;
            }
            return true;
        }

        public bool Skip(int len)
        {
            if (len < 0)
                return false;
            if (len == 0)
                return true;

            int reamin = _cur_end_pos - _cur_pos;
            if (reamin < len)
                return false;

            if (_Stream.CanSeek)
            {
                _Stream.Seek(len, SeekOrigin.Current);
                _cur_pos += len;
                return true;
            }

            Span<byte> buff = stackalloc byte[256];

            int ret = 0;
            while (ret < len)
            {
                int read_len = Math.Min(len - ret, buff.Length);
                int c = _Stream.Read(buff.Slice(0, read_len));
                if (c <= 0)
                    return false;

                ret += c;
                _cur_pos += c;
            }
            return true;
        }
    }

    public sealed class PBReader
    {
        public enum EReadTagResult
        {
            OK,
            Fail,
            End,
        }
        private PBSegReader _Reader;
        private BufferReader _InnerStream;
        private bool _hasError = false;

        public PBReader()
        {
            _Reader = new PBSegReader();
        }

        public void SetStream(Stream stream, int stream_len)
        {
            _Reader.SetStream(stream, stream_len);
            _ClearError();
        }

        public void SetStream(byte[] buff, int offset, int count)
        {
            if (_InnerStream == null)
                _InnerStream = new BufferReader();
            _InnerStream.SetBuffer(buff, offset, count);

            SetStream(_InnerStream, (int)_InnerStream.Length);
        }

        public void SetStream(byte[] buff)
        {
            if (_InnerStream == null)
                _InnerStream = new BufferReader();
            _InnerStream.SetBuffer(buff);

            SetStream(_InnerStream, (int)_InnerStream.Length);
        }

        public EReadTagResult ReadTag(out uint tag, out int field_index, out EPBWireType wire_type)
        {
            tag = 0;
            field_index = 0;
            wire_type = default;

            if (_Reader.BytesUntilLimit() == 0)
                return EReadTagResult.End;

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadTag");
                _SetError();
                return EReadTagResult.Fail;
            }

            if (l_v > uint.MaxValue)
            {
                PBLog._.E("ReadTag: Tag:{0}", l_v);
                _SetError();
                return EReadTagResult.Fail;
            }

            tag = (uint)l_v;
            (field_index, wire_type) = PBUtil.SplitTag(tag);
            if (field_index <= 0)
            {
                PBLog._.E("ReadTag:  Tag:{0}, FieldIndex:{1}, WireType:{2}", l_v, field_index, wire_type);
                _SetError();
                return EReadTagResult.Fail;
            }
            if (wire_type > EPBWireType.Fixed32 || wire_type < EPBWireType.Variant)
            {
                PBLog._.E("ReadTag:  Tag:{0}, FieldIndex:{1}, WireType:{2}", l_v, field_index, wire_type);
                _SetError();
                return EReadTagResult.Fail;
            }

            if (wire_type == EPBWireType.EndGroup || wire_type == EPBWireType.StartGroup)
            {
                PBLog._.E("ReadTag:  Tag:{0}, FieldIndex:{1}, WireType:{2}, wireType unsupport", l_v, field_index, wire_type);
                _SetError();
                return EReadTagResult.Fail;
            }

            PBLog._.D("ReadTag:  Tag:{0}, FieldIndex:{1}, WireType:{2}", l_v, field_index, wire_type);
            return EReadTagResult.OK;
        }


        /// <summary>
        /// ProtoType: float<para/>
        /// WireType : Fixed32<para/>
        /// C# Type : float
        /// </summary>
        public bool ReadFloat(int field_index, EPBWireType wire_type, out float ret)
        {
            ret = 0;
            if (EPBWireType.Fixed32 != wire_type)
            {
                PBLog._.E("ReadFloat:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Fixed32, wire_type);
                return _SetError();
            }

            Span<byte> buffer = stackalloc byte[4];
            if (!_Reader.ReadBuff(buffer))
            {
                PBLog._.E("ReadFloat:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }

            if (!BitConverter.IsLittleEndian)
                _Reverse4(buffer);
            ret = BitConverter.ToSingle(buffer);
            return true;
        }

        /// <summary>
        /// ProtoType: Fixed32<para/>
        /// WireType: Fixed32<para/>
        /// C# Type: uint
        /// </summary>
        public bool ReadFixed32(int field_index, EPBWireType wire_type, out uint ret)
        {
            ret = 0;
            if (EPBWireType.Fixed32 != wire_type)
            {
                PBLog._.E("ReadFixed32:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Fixed32, wire_type);
                return _SetError();
            }

            Span<byte> buffer = stackalloc byte[4];
            if (!_Reader.ReadBuff(buffer))
            {
                PBLog._.E("ReadFixed32:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }

            if (!BitConverter.IsLittleEndian)
                _Reverse4(buffer);

            ret = BitConverter.ToUInt32(buffer);
            return true;
        }

        /// <summary>
        /// ProtoType: SFixed32<para/>
        /// WireType: Fixed32<para/>
        /// C# Type: int
        /// </summary>
        public bool ReadSFixed32(int field_index, EPBWireType wire_type, out int ret)
        {
            ret = 0;
            if (EPBWireType.Fixed32 != wire_type)
            {
                PBLog._.E("ReadSFixed32:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Fixed32, wire_type);
                return _SetError();
            }

            Span<byte> buffer = stackalloc byte[4];
            if (!_Reader.ReadBuff(buffer))
            {
                PBLog._.E("ReadSFixed32:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }
            if (!BitConverter.IsLittleEndian)
                _Reverse4(buffer);
            ret = BitConverter.ToInt32(buffer);
            return true;
        }

        /// <summary>
        /// ProtoType: Fixed64<para/>
        /// WireType: Fixed64<para/>
        /// C# Type: ulong/uint64
        /// </summary>
        public bool ReadFixed64(int field_index, EPBWireType wire_type, out ulong ret)
        {
            ret = 0;
            if (EPBWireType.Fixed64 != wire_type)
            {
                PBLog._.E("ReadFixed64:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Fixed64, wire_type);
                return _SetError();
            }

            Span<byte> buffer = stackalloc byte[8];
            if (!_Reader.ReadBuff(buffer))
            {
                PBLog._.E("ReadFixed64:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }
            if (!BitConverter.IsLittleEndian)
                _Reverse8(buffer);
            ret = BitConverter.ToUInt64(buffer);
            return true;
        }

        /// <summary>
        /// ProtoType: SFixed64<para/>
        /// WireType: Fixed64<para/>
        /// C# Type: long/int64
        /// </summary>
        public bool ReadSFixed64(int field_index, EPBWireType wire_type, out long ret)
        {
            ret = 0;
            if (EPBWireType.Fixed64 != wire_type)
            {
                PBLog._.E("ReadSFixed64:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Fixed64, wire_type);
                return _SetError();
            }

            Span<byte> buffer = stackalloc byte[8];
            if (!_Reader.ReadBuff(buffer))
            {
                PBLog._.E("ReadSFixed64:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }
            if (!BitConverter.IsLittleEndian)
                _Reverse8(buffer);
            ret = BitConverter.ToInt64(buffer);
            return true;
        }

        /// <summary>
        /// ProtoType: double<para/>
        /// WireType : Fixed64<para/>
        /// C# Type : double
        /// </summary>
        public bool ReadDouble(int field_index, EPBWireType wire_type, out double ret)
        {
            ret = 0;
            if (EPBWireType.Fixed64 != wire_type)
            {
                PBLog._.E("ReadDouble:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Fixed64, wire_type);
                return _SetError();
            }

            Span<byte> buffer = stackalloc byte[8];
            if (!_Reader.ReadBuff(buffer))
            {
                PBLog._.E("ReadDouble:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }
            if (!BitConverter.IsLittleEndian)
                _Reverse8(buffer);
            ret = BitConverter.ToDouble(buffer);
            return true;
        }

        /// <summary>
        /// ProtoType: int32<para/>
        /// WireType: Variant<para/>
        /// C# Type: int
        /// </summary>
        public bool ReadInt32(int field_index, EPBWireType wire_type, out int ret)
        {
            ret = 0;
            if (EPBWireType.Variant != wire_type)
            {
                PBLog._.E("ReadInt32:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Variant, wire_type);
                return _SetError();
            }

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadInt32:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }

            ret = (int)((uint)l_v);
            return true;
        }

        /// <summary>
        /// ProtoType: uint32<para/>
        /// WireType: Variant<para/>
        /// C# Type: uint
        /// </summary>
        public bool ReadUInt32(int field_index, EPBWireType wire_type, out uint ret)
        {
            ret = 0;
            if (EPBWireType.Variant != wire_type)
            {
                PBLog._.E("ReadUInt32:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Variant, wire_type);
                return _SetError();
            }

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadUInt32:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }

            if (l_v > uint.MaxValue)
            {
                PBLog._.E("ReadUInt32:  FieldIndex:{0}, ReadDataError, RawData:{1} > uint32.MaxValue", field_index, l_v);
                return _SetError();
            }

            ret = (uint)l_v;
            return true;
        }

        /// <summary>
        /// ProtoType: int64<para/>
        /// WireType: Variant<para/>
        /// C# Type: long/int64
        /// </summary>
        public bool ReadInt64(int field_index, EPBWireType wire_type, out long ret)
        {
            ret = 0;
            if (EPBWireType.Variant != wire_type)
            {
                PBLog._.E("ReadInt64:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Variant, wire_type);
                return _SetError();
            }

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadInt64:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }
            ret = (long)l_v;
            return true;
        }

        /// <summary>
        /// ProtoType: uint64<para/>
        /// WireType: Variant<para/>
        /// C# Type: ulong/uint64
        /// </summary>
        public bool ReadUInt64(int field_index, EPBWireType wire_type, out ulong ret)
        {
            ret = 0;
            if (EPBWireType.Variant != wire_type)
            {
                PBLog._.E("ReadUInt64:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Variant, wire_type);
                return _SetError();
            }

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadUInt64:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }
            ret = l_v;
            return true;
        }

        /// <summary>
        /// ProtoType: sint32<para/>
        /// WireType: Variant<para/>
        /// C# Type: int
        /// </summary>
        public bool ReadSInt32(int field_index, EPBWireType wire_type, out int ret)
        {
            ret = 0;
            if (EPBWireType.Variant != wire_type)
            {
                PBLog._.E("ReadSInt32:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Variant, wire_type);
                return _SetError();
            }

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadSInt32:  FieldIndex:{0}, ReadDataError", field_index);
                return _SetError();
            }

            if (l_v > uint.MaxValue)
            {
                PBLog._.E("ReadSInt32:  FieldIndex:{0}, ReadDataError, RawData:{1} > uint32.MaxValue", field_index, l_v);
                return _SetError();
            }

            //decode zig zag 32
            ret = PBUtil.DecodeZigzag32((uint)l_v);
            return true;

        }

        /// <summary>
        /// ProtoType: sint64<para/>
        /// WireType: Variant<para/>
        /// C# Type: long/int64
        /// </summary>
        public bool ReadSInt64(int field_index, EPBWireType wire_type, out long ret)
        {
            ret = 0;
            if (EPBWireType.Variant != wire_type)
            {
                PBLog._.E("ReadSInt64:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Variant, wire_type);
                return _SetError();
            }

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadUInt32:  FieldIndex:{0}, ReadData Error", field_index);
                return _SetError();
            }

            //decode zig zag 32
            ret = PBUtil.DecodeZigzag64(l_v);
            return true;
        }

        /// <summary>
        /// ProtoType: bool<para/>
        /// WireType: Variant<para/>
        /// C# Type: bool
        /// </summary>
        public bool ReadBool(int field_index, EPBWireType wire_type, out bool ret)
        {
            ret = false;
            if (EPBWireType.Variant != wire_type)
            {
                PBLog._.E("ReadBool:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Variant, wire_type);
                return _SetError();
            }

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadBool:  FieldIndex:{0}, ReadData Error", field_index);
                return _SetError();
            }
            ret = l_v != 0;
            return true;
        }

        /// <summary>
        /// ProtoType: enum<para/>
        /// WireType: Variant<para/>
        /// C# Type: Enum/int
        /// </summary>
        public bool ReadEnum(int field_index, EPBWireType wire_type, out int ret)
        {
            ret = default;
            if (EPBWireType.Variant != wire_type)
            {
                PBLog._.E("ReadEnum:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Variant, wire_type);
                return _SetError();
            }

            if (!_Reader.TryReadVariant(out var l_v))
            {
                PBLog._.E("ReadEnum:  FieldIndex:{0}, ReadData Error", field_index);
                return _SetError();
            }

            ret = (int)((uint)l_v);
            return true;
        }

        /// <summary>
        /// ProtoType: bytes<para/>
        /// WireType: Length_delimited<para/>
        /// C# Type: byte[]
        /// </summary>
        public bool ReadBytes(int field_index, EPBWireType wire_type, out byte[] ret)
        {
            ret = Array.Empty<byte>();
            if (EPBWireType.Length_delimited != wire_type)
            {
                PBLog._.E("ReadBytes:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Length_delimited, wire_type);
                return _SetError();
            }

            if (!_TryReadLength(out var len))
            {
                PBLog._.E("ReadBytes:  FieldIndex:{0}, ReadLength Error", field_index);
                return _SetError();
            }

            if (len == 0)
                return true;

            if (len > _Reader.BytesUntilLimit())
            {
                PBLog._.E("ReadBytes:  FieldIndex:{0}, Len:{1} > {2}", field_index, len, _Reader.BytesUntilLimit());
                return _SetError();
            }

            ret = new byte[len];
            if (!_Reader.ReadBuff(new Span<byte>(ret)))
            {
                PBLog._.E("ReadBytes:  FieldIndex:{0}, Read Data Error ,Len {1}", field_index, len);
                return _SetError();
            }

            return true;
        }

        /// <summary>
        /// ProtoType: string<para/>
        /// WireType: Length_delimited<para/>
        /// C# Type: string
        /// </summary>
        public bool ReadString(int field_index, EPBWireType wire_type, out string ret)
        {
            ret = string.Empty;
            if (EPBWireType.Length_delimited != wire_type)
            {
                PBLog._.E("ReadString:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Length_delimited, wire_type);
                return _SetError();
            }


            if (!_TryReadLength(out var len))
            {
                PBLog._.E("ReadString:  FieldIndex:{0}, ReadLength Error", field_index);
                return _SetError();
            }

            if (len == 0)
                return true;

            if (len > _Reader.BytesUntilLimit())
            {
                PBLog._.E("ReadString:  FieldIndex:{0}, Len:{1} > {2}", field_index, len, _Reader.BytesUntilLimit());
                return _SetError();
            }

            if (len < 512)
            {
                Span<byte> buff = stackalloc byte[len];
                if (!_Reader.ReadBuff(buff))
                {
                    PBLog._.E("ReadString:  FieldIndex:{0}, Read Data Error ,Len {1}", field_index, len);
                    return _SetError();
                }
                ret = System.Text.Encoding.UTF8.GetString(buff);
                return true;
            }
            else
            {
                byte[] buff = new byte[len];
                if (!_Reader.ReadBuff(new Span<byte>(buff)))
                {
                    PBLog._.E("ReadString:  FieldIndex:{0}, Read Data Error ,Len {1}", field_index, len);
                    return _SetError();
                }
                ret = System.Text.Encoding.UTF8.GetString(buff);
                return true;
            }
        }

        /// <summary>
        /// Enters a length-delimited message field. Use this for generated inline sub-message bodies, such as protobuf map entries.
        /// </summary>
        public bool TryBeginMessageField(int field_index, EPBWireType wire_type)
        {
            if (EPBWireType.Length_delimited != wire_type)
            {
                PBLog._.E("BeginMessageField:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Length_delimited, wire_type);
                return _SetError();
            }

            if (!_TryReadLength(out var len))
            {
                PBLog._.E("BeginMessageField:  FieldIndex:{0}, ReadLength Error", field_index);
                return _SetError();
            }

            if (len > _Reader.BytesUntilLimit())
            {
                PBLog._.E("BeginMessageField:  FieldIndex:{0}, Len:{1} > {2}", field_index, len, _Reader.BytesUntilLimit());
                return _SetError();
            }

            if (!_Reader.TryPushLimit(len))
            {
                PBLog._.E("BeginMessageField:  FieldIndex:{0}, Push Limit error, Len:{1} remain:{2}", field_index, len, _Reader.BytesUntilLimit());
                return _SetError();
            }

            return true;
        }

        public bool EndMessageField(int field_index)
        {
            bool ret = true;
            if (_Reader.BytesUntilLimit() != 0)
            {
                PBLog._.E("EndMessageField:  FieldIndex:{0}, remain:{1}", field_index, _Reader.BytesUntilLimit());
                ret = _SetError();
            }

            if (!_Reader.TryPopLimit(out var _))
            {
                PBLog._.E("EndMessageField:  FieldIndex:{0}, TryPopLimitError", field_index);
                ret = _SetError();
            }

            return ret && !_hasError;
        }

        /// <summary>
        /// ProtoType: message<para/>
        /// WireType: Length_delimited<para/>
        /// C# Type: IPBMessage
        /// </summary>
        public bool ReadMessage(int field_index, EPBWireType wire_type, IPBMessage msg)
        {
            if (EPBWireType.Length_delimited != wire_type)
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Length_delimited, wire_type);
                return _SetError();
            }

            if (msg == null)
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0}, Param msg is null", field_index);
                return _SetError();
            }

            if (!_TryReadLength(out var len))
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0}, ReadLength Error", field_index);
                return _SetError();
            }

            if (len > _Reader.BytesUntilLimit())
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0}, Len:{1} > {2}", field_index, len, _Reader.BytesUntilLimit());
                return _SetError();
            }

            if (!_Reader.TryPushLimit(len))
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0},Push Limit error, Len:{1} remain:{2}", field_index, len, _Reader.BytesUntilLimit());
                return _SetError();
            }

            try
            {
                var ret = msg.Unserialize(this);
                if (!ret || HasError())
                {
                    _SetError();
                }
                else if (_Reader.BytesUntilLimit() != 0)
                {
                    PBLog._.E("ReadMessage:  FieldIndex:{0},remain:{1}", field_index, _Reader.BytesUntilLimit());
                    _SetError();
                }
            }
            finally
            {
                if (!_Reader.TryPopLimit(out var _))
                {
                    PBLog._.E("ReadMessage:  FieldIndex:{0}, TryPopLimitError", field_index);
                    _SetError();
                }
            }

            return !_hasError;
        }

        public bool ReadMessage<T>(int field_index, EPBWireType wire_type, out T msg) where T : class, IPBMessage, new()
        {
            msg = default;
            if (EPBWireType.Length_delimited != wire_type)
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0}, NeedWireType:{1}, GivenWireType:{2}", field_index, EPBWireType.Length_delimited, wire_type);
                return _SetError();
            }

            if (!_TryReadLength(out var len))
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0}, ReadLength Error", field_index);
                return _SetError();
            }

            if (len > _Reader.BytesUntilLimit())
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0}, Len:{1} > {2}", field_index, len, _Reader.BytesUntilLimit());
                return _SetError();
            }

            if (!_Reader.TryPushLimit(len))
            {
                PBLog._.E("ReadMessage:  FieldIndex:{0},Push Limit error, Len:{1} remain:{2}", field_index, len, _Reader.BytesUntilLimit());
                return _SetError();
            }

            if (msg == null)
                msg = new T();

            try
            {
                var ret = msg.Unserialize(this);
                if (!ret || HasError())
                {
                    _SetError();
                }
                else if (_Reader.BytesUntilLimit() != 0)
                {
                    PBLog._.E("ReadMessage:  FieldIndex:{0},remain:{1}", field_index, _Reader.BytesUntilLimit());
                    _SetError();
                }
            }
            finally
            {
                if (!_Reader.TryPopLimit(out var _))
                {
                    PBLog._.E("ReadMessage:  FieldIndex:{0}, TryPopLimitError", field_index);
                    _SetError();
                }
            }

            return !_hasError;
        }

        public bool SkipField(int field_index, EPBWireType wire_type)
        {
            PBLog._.W("SkipField: FieldIndex:{0} WireType:{1}", field_index, wire_type);
            switch (wire_type)
            {
                case EPBWireType.Fixed32:
                    if (!_Reader.Skip(4))
                        return _SetError();
                    return true;

                case EPBWireType.Fixed64:
                    if (!_Reader.Skip(8))
                        return _SetError();
                    return true;

                case EPBWireType.SignedVariant:
                case EPBWireType.Variant:
                    if (!_Reader.TryReadVariant(out var _))
                        return _SetError();
                    return true;

                case EPBWireType.Length_delimited:
                    if (!_TryReadLength(out var len))
                        return _SetError();
                    if (!_Reader.Skip(len))
                        return _SetError();
                    return true;

                default:
                    return _SetError();
            }
        }

        private bool _TryReadLength(out int len)
        {
            len = default;
            if (!_Reader.TryReadVariant(out ulong raw_len))
                return false;

            if (raw_len > int.MaxValue)
                return false;

            len = (int)raw_len;
            return true;
        }

        public bool HasError() => _hasError;

        private void _ClearError()
        {
            _hasError = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _SetError()
        {
            _hasError = true;
            return false;
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
