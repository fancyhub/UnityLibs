using System;

namespace FH
{
    public enum EPBFieldType
    {
        Auto,
        Double,
        Float,
        Int32,
        Int64,
        UInt32,
        UInt64,
        SInt32,
        SInt64,
        Fixed32,
        Fixed64,
        SFixed32,
        SFixed64,
        Bool,
        String,
        Bytes,
        Enum,
        Message,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class PBMessageAttribute : Attribute
    {
        public bool GenerateSerialize { get; set; } = true;
        public bool GenerateUnserialize { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class PBFieldAttribute : Attribute
    {
        public int Number { get; }
        public EPBFieldType Type { get; set; } = EPBFieldType.Auto;
        public EPBFieldType KeyType { get; set; } = EPBFieldType.Auto;
        public EPBFieldType ValueType { get; set; } = EPBFieldType.Auto;

        public PBFieldAttribute(int number)
        {
            Number = number;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class PBIgnoreAttribute : Attribute
    {
    }
}
