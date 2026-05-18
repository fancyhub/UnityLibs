/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace FH
{
    [TestFixture]
    public sealed class PBTest
    {

        public enum PBTestEnum
        {
            None = 0,
            A = 1,
            B = -1,
        }

        public sealed class PBTestChild : IPBMessage
        {
            public int Id;
            public string Name;

            public void Serialize(PBWriter writer)
            {
                writer.WriteInt32(1, Id);
                writer.WriteString(2, Name);
            }

            public bool Unserialize(PBReader reader)
            {
                while (true)
                {
                    if (reader.HasError())
                        return false;

                    var r = reader.ReadTag(out _, out int fieldIndex, out EPBWireType wireType);
                    if (r == PBReader.EReadTagResult.End)
                        return !reader.HasError();

                    if (r == PBReader.EReadTagResult.Fail)
                        return false;

                    switch (fieldIndex)
                    {
                        case 1: reader.ReadInt32(fieldIndex, wireType, out Id); break;
                        case 2: reader.ReadString(fieldIndex, wireType, out Name); break;
                        default: reader.SkipField(fieldIndex, wireType); break;
                    }
                }
            }


            public override string ToString()
            {
                return $"Id={Id}, Name={Name}";
            }
        }

        public sealed class PBTestAllTypes : IPBMessage
        {
            public double DoubleValue = 123.456;
            public float FloatValue = 78.5f;
            public int Int32Value = -123;
            public long Int64Value = -1234567890123L;
            public uint UInt32Value = 123u;
            public ulong UInt64Value = 1234567890123UL;
            public int SInt32Value = -456;
            public long SInt64Value = -456789012345L;
            public uint Fixed32Value = 0x12345678u;
            public ulong Fixed64Value = 0x123456789ABCDEF0UL;
            public int SFixed32Value = -789;
            public long SFixed64Value = -9876543210L;
            public bool BoolValue = true;
            public PBTestEnum EnumValue = PBTestEnum.B;
            public string StringValue = "hello protobuf 中文";
            public byte[] BytesValue = { 0x01, 0x02, 0xFE, 0xFF };
            public PBTestChild Child = new PBTestChild { Id = 7, Name = "child message" };

            public void Serialize(PBWriter writer)
            {
                writer.WriteDouble(1, DoubleValue);
                writer.WriteFloat(2, FloatValue);
                writer.WriteInt32(3, Int32Value);
                writer.WriteInt64(4, Int64Value);
                writer.WriteUInt32(5, UInt32Value);
                writer.WriteUInt64(6, UInt64Value);
                writer.WriteSInt32(7, SInt32Value);
                writer.WriteSInt64(8, SInt64Value);
                writer.WriteFixed32(9, Fixed32Value);
                writer.WriteFixed64(10, Fixed64Value);
                writer.WriteSFixed32(11, SFixed32Value);
                writer.WriteSFixed64(12, SFixed64Value);
                writer.WriteBool(13, BoolValue);
                writer.WriteEnum(14, (int)EnumValue);
                writer.WriteString(15, StringValue);
                writer.WriteBytes(16, BytesValue);
                writer.WriteMessage(17, Child);

                writer.WriteInt32(100, 999); // 测试未知字段 skip
            }

            public bool Unserialize(PBReader reader)
            {
                while (true)
                {
                    if (reader.HasError())
                        return false;

                    var r = reader.ReadTag(out _, out int fieldIndex, out EPBWireType wireType);
                    if (r == PBReader.EReadTagResult.End)
                        return !reader.HasError();

                    if (r == PBReader.EReadTagResult.Fail)
                        return false;

                    switch (fieldIndex)
                    {
                        case 1: reader.ReadDouble(fieldIndex, wireType, out DoubleValue); break;
                        case 2: reader.ReadFloat(fieldIndex, wireType, out FloatValue); break;
                        case 3: reader.ReadInt32(fieldIndex, wireType, out Int32Value); break;
                        case 4: reader.ReadInt64(fieldIndex, wireType, out Int64Value); break;
                        case 5: reader.ReadUInt32(fieldIndex, wireType, out UInt32Value); break;
                        case 6: reader.ReadUInt64(fieldIndex, wireType, out UInt64Value); break;
                        case 7: reader.ReadSInt32(fieldIndex, wireType, out SInt32Value); break;
                        case 8: reader.ReadSInt64(fieldIndex, wireType, out SInt64Value); break;
                        case 9: reader.ReadFixed32(fieldIndex, wireType, out Fixed32Value); break;
                        case 10: reader.ReadFixed64(fieldIndex, wireType, out Fixed64Value); break;
                        case 11: reader.ReadSFixed32(fieldIndex, wireType, out SFixed32Value); break;
                        case 12: reader.ReadSFixed64(fieldIndex, wireType, out SFixed64Value); break;
                        case 13: reader.ReadBool(fieldIndex, wireType, out BoolValue); break;
                        case 14: reader.ReadEnum(fieldIndex, wireType, out int enumValue); EnumValue = (PBTestEnum)enumValue; break;
                        case 15: reader.ReadString(fieldIndex, wireType, out StringValue); break;
                        case 16: reader.ReadBytes(fieldIndex, wireType, out BytesValue); break;
                        case 17: reader.ReadMessage<PBTestChild>(fieldIndex, wireType, out Child); break;
                        default: reader.SkipField(fieldIndex, wireType); break;
                    }

                }
            }


            public override string ToString()
            {
                return $"Double={DoubleValue}, Float={FloatValue}, Int32={Int32Value}, Int64={Int64Value}, " +
                       $"UInt32={UInt32Value}, UInt64={UInt64Value}, SInt32={SInt32Value}, SInt64={SInt64Value}, " +
                       $"Fixed32=0x{Fixed32Value:X8}, Fixed64=0x{Fixed64Value:X16}, SFixed32={SFixed32Value}, " +
                       $"SFixed64={SFixed64Value}, Bool={BoolValue}, Enum={EnumValue}, String={StringValue}, " +
                       $"Bytes={BitConverter.ToString(BytesValue ?? Array.Empty<byte>())}, Child=({Child})";
            }
        }


        [Test]
        public void RoundTrip_AllTypes_PreservesValues()
        {
            var src = new PBTestAllTypes();

            var stream = new MemoryStream();
            var writer = new PBWriter();

            writer.SetStream(stream);
            PBSerializer.Serialize(src, writer);


            byte[] bytes = stream.ToArray();
            Assert.Greater(bytes.Length, 0);

            var reader = new PBReader();
            reader.SetStream(bytes);

            var dst = PBSerializer.Unserialize<PBTestAllTypes>(reader);
            Assert.IsTrue(dst != null);

            bool sameOk = IsSame(src, dst);            
            Assert.IsFalse(reader.HasError());
            Assert.IsTrue(sameOk, $"Src = {src}\nDst = {dst}\nHex = {BitConverter.ToString(bytes)}");
        }

        [Test]
        public void TryRead_WrongWireType_SetsError()
        {
            var old = PBLog._.AllowMask;
            PBLog._.AllowMask = ELogMask.None;
            try
            {
                var stream = new MemoryStream();
                var writer = new PBWriter();

                writer.SetStream(stream);
                writer.Begin();
                writer.WriteInt32(3, 123);
                writer.End();

                var reader = new PBReader();
                reader.SetStream(stream.ToArray());

                var tagResult = reader.ReadTag(out _, out int fieldIndex, out EPBWireType wireType);
                Assert.AreEqual(PBReader.EReadTagResult.OK, tagResult);
                Assert.AreEqual(3, fieldIndex);
                Assert.AreEqual(EPBWireType.Variant, wireType);

                Assert.IsFalse(reader.ReadString(fieldIndex, wireType, out _));
                Assert.IsTrue(reader.HasError());
            }            
            finally
            {
                PBLog._.AllowMask = old;
            }
        }

        [Test]
        public void FixedWidthValues_AreWrittenLittleEndian()
        {
            var stream = new MemoryStream();
            var writer = new PBWriter();

            writer.SetStream(stream);
            writer.Begin();
            writer.WriteFixed32(1, 0x12345678u);
            writer.WriteFixed64(2, 0x123456789ABCDEF0ul);
            writer.WriteFloat(3, 1.0f);
            writer.WriteDouble(4, 1.0d);
            writer.End();

            byte[] expected =
            {
                0x0D, 0x78, 0x56, 0x34, 0x12,
                0x11, 0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12,
                0x1D, 0x00, 0x00, 0x80, 0x3F,
                0x21, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F,
            };

            CollectionAssert.AreEqual(expected, stream.ToArray());
        }

        [Test]
        public void MessageFieldScope_CanReadInlineMapEntry()
        {
            var stream = new MemoryStream();
            var writer = new PBWriter();

            writer.SetStream(stream);
            writer.Begin();
            writer.BeginMessageField(1);
            writer.WriteString(1, "key");
            writer.WriteInt32(2, 42);
            writer.EndMessageField();
            writer.End();

            var reader = new PBReader();
            reader.SetStream(stream.ToArray());

            var tagResult = reader.ReadTag(out _, out int fieldIndex, out EPBWireType wireType);
            Assert.AreEqual(PBReader.EReadTagResult.OK, tagResult);
            Assert.AreEqual(1, fieldIndex);
            Assert.AreEqual(EPBWireType.Length_delimited, wireType);
            Assert.IsTrue(reader.TryBeginMessageField(fieldIndex, wireType));

            string key = string.Empty;
            int value = 0;
            while (true)
            {
                var entryTagResult = reader.ReadTag(out _, out int entryFieldIndex, out EPBWireType entryWireType);
                if (entryTagResult == PBReader.EReadTagResult.End)
                    break;

                Assert.AreEqual(PBReader.EReadTagResult.OK, entryTagResult);
                switch (entryFieldIndex)
                {
                    case 1:
                        reader.ReadString(entryFieldIndex, entryWireType, out key);
                        break;
                    case 2:
                        reader.ReadInt32(entryFieldIndex, entryWireType, out value);
                        break;
                    default:
                        reader.SkipField(entryFieldIndex, entryWireType);
                        break;
                }
            }

            Assert.IsTrue(reader.EndMessageField(fieldIndex));
            Assert.AreEqual("key", key);
            Assert.AreEqual(42, value);
            Assert.IsFalse(reader.HasError());
            Assert.AreEqual(PBReader.EReadTagResult.End, reader.ReadTag(out _, out _, out _));
        }

        private static bool IsSame(PBTestAllTypes a, PBTestAllTypes b)
        {
            return a.DoubleValue == b.DoubleValue
                && a.FloatValue == b.FloatValue
                && a.Int32Value == b.Int32Value
                && a.Int64Value == b.Int64Value
                && a.UInt32Value == b.UInt32Value
                && a.UInt64Value == b.UInt64Value
                && a.SInt32Value == b.SInt32Value
                && a.SInt64Value == b.SInt64Value
                && a.Fixed32Value == b.Fixed32Value
                && a.Fixed64Value == b.Fixed64Value
                && a.SFixed32Value == b.SFixed32Value
                && a.SFixed64Value == b.SFixed64Value
                && a.BoolValue == b.BoolValue
                && a.EnumValue == b.EnumValue
                && a.StringValue == b.StringValue
                && (a.BytesValue ?? Array.Empty<byte>()).SequenceEqual(b.BytesValue ?? Array.Empty<byte>())
                && a.Child.Id == b.Child.Id
                && a.Child.Name == b.Child.Name;
        }

    }
}
