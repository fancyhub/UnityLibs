/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;

namespace FH.Protobuf.Ed
{
    public enum PBProtoLabel
    {
        None,
        Optional,
        Repeated,
        Required,
    }

    public sealed class PBProtoFile
    {
        public string FilePath;
        public string PackageName;
        public string CSharpNamespace;
        public readonly List<PBProtoEnum> Enums = new List<PBProtoEnum>();
        public readonly List<PBProtoMessage> Messages = new List<PBProtoMessage>();
    }

    public sealed class PBProtoEnum
    {
        public string Name;
        public readonly List<string> Comments = new List<string>();
        public readonly List<PBProtoEnumValue> Values = new List<PBProtoEnumValue>();
    }

    public sealed class PBProtoEnumValue
    {
        public string Name;
        public int Value;
        public readonly List<string> Comments = new List<string>();
    }

    public sealed class PBProtoMessage
    {
        public string Name;
        public readonly List<string> Comments = new List<string>();
        public readonly List<PBProtoEnum> Enums = new List<PBProtoEnum>();
        public readonly List<PBProtoMessage> Messages = new List<PBProtoMessage>();
        public readonly List<PBProtoField> Fields = new List<PBProtoField>();
    }

    public sealed class PBProtoField
    {
        public PBProtoLabel Label;
        public string TypeName;
        public string Name;
        public int Number;
        public bool IsMap;
        public string MapKeyType;
        public string MapValueType;
        public readonly List<string> Comments = new List<string>();

        public bool IsRepeated => Label == PBProtoLabel.Repeated;
    }
}
