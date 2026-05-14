/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2019/8/7
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;
using System.Text;


namespace FH
{
    public sealed class PBWriter
    {
        private const int CStackallocStringMaxCharCount = 256;
        private BlockWriter _BlockWriter = new BlockWriter();
        private Stream _stream;
        public PBWriter()
        {
        }

        public void SetStream(Stream stream)
        {
            _stream = stream;
        }

        public void Begin()
        {
            _BlockWriter.Begin();
        }

        public void End()
        {
            if (_stream == null)
                throw new InvalidOperationException("PBWriter stream is null");

            _BlockWriter.End(_stream);
        }

        public void WriteTag(uint tag)
        {
            _BlockWriter.WriteVariant(tag);
        }

        public void WriteTag(int fieldIndex, EPBWireType wireType)
        {
            WriteTag(PBUtil.MakeTag(fieldIndex, wireType));
        }

        #region Write Funcs

        /// <summary>
        /// ProtoType: float<para/>
        /// WireType : Fixed32<para/>
        /// C# Type : float
        /// </summary>
        public void WriteFloat(float value)
        {
            _BlockWriter.WriteRaw(value);
        }

        /// <summary>
        /// ProtoType: float<para/>
        /// WireType : Fixed32<para/>
        /// C# Type : float
        /// </summary>
        public void WriteFloat(int fieldIndex, float value, float defaultValue = 0)
        {
            if (value.Equals(defaultValue))
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Fixed32));
            _BlockWriter.WriteRaw(value);
        }

        /// <summary>
        /// ProtoType: Fixed32<para/>
        /// WireType: Fixed32<para/>
        /// C# Type: uint 
        /// </summary>
        public void WriteFixed32(uint value)
        {
            _BlockWriter.WriteRaw(value);
        }

        /// <summary>
        /// ProtoType: Fixed32<para/>
        /// WireType: Fixed32<para/>
        /// C# Type: uint 
        /// </summary>
        public void WriteFixed32(int fieldIndex, uint value, uint defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Fixed32));
            _BlockWriter.WriteRaw(value);
        }

        /// <summary>
        /// ProtoType: SFixed32<para/>
        /// WireType: Fixed32<para/>
        /// C# Type: int 
        /// </summary>
        public void WriteSFixed32(int value)
        {
            _BlockWriter.WriteRaw((uint)value);
        }

        /// <summary>
        /// ProtoType: SFixed32<para/>
        /// WireType: Fixed32<para/>
        /// C# Type: int 
        /// </summary>
        public void WriteSFixed32(int fieldIndex, int value, int defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Fixed32));
            _BlockWriter.WriteRaw((uint)value);
        }


        /// <summary>
        /// ProtoType: double <para/>
        /// WireType : Fixed64  <para/>
        /// C# Type : double
        /// </summary>
        public void WriteDouble(double value)
        {
            _BlockWriter.WriteRaw(value);
        }

        /// <summary>
        /// ProtoType: double <para/>
        /// WireType : Fixed64  <para/>
        /// C# Type : double
        /// </summary>
        public void WriteDouble(int fieldIndex, double value, double defaultValue = 0)
        {
            if (value.Equals(defaultValue))
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Fixed64));
            _BlockWriter.WriteRaw(value);
        }


        /// <summary>
        /// ProtoType: int32<para/>
        /// WireType: Variant<para/>
        /// C# Type: int
        /// </summary>
        public void WriteInt32(int value)
        {
            if (value >= 0)
            {
                _BlockWriter.WriteVariant((uint)value);
            }
            else
            {
                // Must sign-extend.
                _BlockWriter.WriteVariant((ulong)value);
            }
        }

        /// <summary>
        /// ProtoType: int32<para/>
        /// WireType: Variant<para/>
        /// C# Type: int
        /// </summary>
        public void WriteInt32(int fieldIndex, int value, int defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Variant));
            if (value >= 0)
            {
                _BlockWriter.WriteVariant((uint)value);
            }
            else
            {
                // Must sign-extend.
                _BlockWriter.WriteVariant((ulong)value);
            }
        }

        /// <summary>
        /// ProtoType: Fixed64<para/>
        /// WireType: Fixed64 <para/>
        /// C# Type: ulong/uint64 
        /// </summary>
        public void WriteFixed64(ulong value)
        {
            _BlockWriter.WriteRaw(value);
        }

        /// <summary>
        /// ProtoType: Fixed64<para/>
        /// WireType: Fixed64 <para/>
        /// C# Type: ulong/uint64 
        /// </summary>
        public void WriteFixed64(int fieldIndex, ulong value, ulong defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Fixed64));
            _BlockWriter.WriteRaw(value);
        }

        /// <summary>
        /// ProtoType: SFixed64<para/>
        /// WireType: Fixed64 <para/>
        /// C# Type: long/int64
        /// </summary>
        public void WriteSFixed64(long value)
        {
            _BlockWriter.WriteRaw((ulong)value);
        }

        /// <summary>
        /// ProtoType: SFixed64<para/>
        /// WireType: Fixed64 <para/>
        /// C# Type: long/int64
        /// </summary>
        public void WriteSFixed64(int fieldIndex, long value, long defaultValue = 0)
        {
            if (value == defaultValue)
                return;
            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Fixed64));
            _BlockWriter.WriteRaw((ulong)value);
        }


        /// <summary>
        /// ProtoType: int64<para/>
        /// WireType: Variant<para/>
        /// C# Type: long/int64
        /// </summary>
        public void WriteInt64(long value)
        {
            _BlockWriter.WriteVariant((ulong)value);
        }

        /// <summary>
        /// ProtoType: int64<para/>
        /// WireType: Variant<para/>
        /// C# Type: long/int64
        /// </summary>
        public void WriteInt64(int fieldIndex, long value, long defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Variant));
            _BlockWriter.WriteVariant((ulong)value);
        }

        /// <summary>
        /// ProtoType: uint64<para/>
        /// WireType: Variant <para/>
        /// C# Type: ulong/int64
        /// </summary>
        public void WriteUInt64(ulong value)
        {
            _BlockWriter.WriteVariant(value);
        }

        /// <summary>
        /// ProtoType: uint64<para/>
        /// WireType: Variant <para/>
        /// C# Type: ulong/int64
        /// </summary>
        public void WriteUInt64(int fieldIndex, ulong value, ulong defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Variant));
            _BlockWriter.WriteVariant(value);
        }

        /// <summary>
        /// ProtoType: bool<para/>
        /// WireType: Variant <para/>
        /// C# Type: bool
        /// </summary>
        public void WriteBool(bool value)
        {
            _BlockWriter.WriteByte(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// ProtoType: bool<para/>
        /// WireType: Variant <para/>
        /// C# Type: bool
        /// </summary>
        public void WriteBool(int fieldIndex, bool value, bool defaultValue = false)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Variant));
            _BlockWriter.WriteByte(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// ProtoType: uint32<para/>
        /// WireType: Variant <para/>
        /// C# Type: uint
        /// </summary>
        public void WriteUInt32(uint value)
        {
            _BlockWriter.WriteVariant(value);
        }

        /// <summary>
        /// ProtoType: uint32<para/>
        /// WireType: Variant <para/>
        /// C# Type: uint
        /// </summary>
        public void WriteUInt32(int fieldIndex, uint value, uint defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Variant));
            _BlockWriter.WriteVariant(value);
        }


        /// <summary>
        /// ProtoType: enum<para/>
        /// WireType: Variant
        /// C# Type: Enum/int<para/>
        /// </summary>
        public void WriteEnum(int value)
        {
            if (value >= 0)
            {
                _BlockWriter.WriteVariant((uint)value);
            }
            else
            {
                // Must sign-extend.
                _BlockWriter.WriteVariant((ulong)value);
            }
        }

        /// <summary>
        /// ProtoType: enum<para/>
        /// WireType: Variant
        /// C# Type: Enum/int<para/>
        /// </summary>
        public void WriteEnum(int fieldIndex, int value, int defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Variant));
            if (value >= 0)
            {
                _BlockWriter.WriteVariant((uint)value);
            }
            else
            {
                // Must sign-extend.
                _BlockWriter.WriteVariant((ulong)value);
            }
        }

        /// <summary>
        /// ProtoType: sint32<para/>
        /// WireType: Variant<para/>
        /// C# Type: int
        /// </summary>
        public void WriteSInt32(int value)
        {
            _BlockWriter.WriteVariant(PBUtil.EncodeZigzag32(value));
        }

        /// <summary>
        /// ProtoType: sint32<para/>
        /// WireType: Variant<para/>
        /// C# Type: int
        /// </summary>
        public void WriteSInt32(int fieldIndex, int value, int defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Variant));
            _BlockWriter.WriteVariant(PBUtil.EncodeZigzag32(value));
        }

        /// <summary>
        /// ProtoType: sint64<para/>
        /// WireType: Variant<para/>
        /// C# Type: long/int64
        /// </summary>
        public void WriteSInt64(long value)
        {
            _BlockWriter.WriteVariant(PBUtil.EncodeZigzag64(value));
        }

        /// <summary>
        /// ProtoType: sint64<para/>
        /// WireType: Variant<para/>
        /// C# Type: long/int64
        /// </summary>
        public void WriteSInt64(int fieldIndex, long value, long defaultValue = 0)
        {
            if (value == defaultValue)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Variant));
            _BlockWriter.WriteVariant(PBUtil.EncodeZigzag64(value));
        }

        /// <summary>
        /// ProtoType: bytes<para/>
        /// WireType: Length_delimited<para/>
        /// C# Type: byte[]
        /// </summary>
        public void WriteBytes(byte[] value)
        {
            _BlockWriter.BeginBlock();
            if (value != null && value.Length > 0)
                _BlockWriter.Write(value, 0, value.Length);
            _BlockWriter.EndBlock();
        }

        /// <summary>
        /// ProtoType: bytes<para/>
        /// WireType: Length_delimited<para/>
        /// C# Type: byte[]
        /// </summary>
        public void WriteBytes(int fieldIndex, byte[] value)
        {
            if (value == null || value.Length == 0)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Length_delimited));
            WriteBytes(value);
        }

        /// <summary>
        /// ProtoType: string<para/>
        /// WireType: Length_delimited<para/>
        /// C# Type: string
        /// </summary>
        public void WriteString(string value)
        {
            _BlockWriter.BeginBlock();
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length <= CStackallocStringMaxCharCount)
                {
                    // UTF8 BMP chars can use up to 3 bytes; surrogate pairs use 4 bytes for 2 chars.
                    Span<byte> bytes = stackalloc byte[value.Length * 3];
                    int byteCount = Encoding.UTF8.GetBytes(value.AsSpan(), bytes);
                    _BlockWriter.Write(bytes.Slice(0, byteCount));
                }
                else
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(value);
                    _BlockWriter.Write(bytes, 0, bytes.Length);
                }
            }
            _BlockWriter.EndBlock();
        }

        /// <summary>
        /// ProtoType: string<para/>
        /// WireType: Length_delimited<para/>
        /// C# Type: string
        /// </summary>
        public void WriteString(int fieldIndex, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Length_delimited));
            WriteString(value);
        }


        /// <summary>
        /// ProtoType: message<para/>
        /// WireType: Length_delimited
        /// </summary>
        public void WriteMessage(int fieldIndex, IPBMessage message)
        {
            if (message == null)
                return;

            _BlockWriter.WriteVariant(PBUtil.MakeTag(fieldIndex, EPBWireType.Length_delimited));
            _BlockWriter.BeginBlock();
            message.Serialize(this);
            _BlockWriter.EndBlock();

        }

        public void WriteMessage(uint tag, IPBMessage message)
        {
            if (message == null)
                return;

            _BlockWriter.WriteVariant(tag);
            _BlockWriter.BeginBlock();
            message.Serialize(this);
            _BlockWriter.EndBlock();
        }
        #endregion

    }
}
