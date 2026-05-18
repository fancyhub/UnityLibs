/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace FH.Protobuf.Ed
{
    public enum PBProtoCodeGenMode
    {
        ForceGenerate,
        CompileTime,
    }

    public enum PBProtoMemberNameStyle
    {
        KeepProtoName,
        PascalCase,
        CamelCase,
        UnderscoreCamelCase,
        MUnderscoreCamelCase,
    }

    public sealed class PBProtoCodeGenOptions
    {
        public string LanguageId = PBProtoCodeGenerator.CSharpLanguageId;
        public PBProtoCodeGenMode Mode = PBProtoCodeGenMode.ForceGenerate;
        public PBProtoMemberNameStyle MemberNameStyle = PBProtoMemberNameStyle.KeepProtoName;
    }

    public sealed class PBProtoGeneratedFile
    {
        public string RelativePath;
        public string Contents;

        public PBProtoGeneratedFile(string relativePath, string contents)
        {
            RelativePath = relativePath;
            Contents = contents;
        }
    }

    public sealed class PBProtoCodeGenInput
    {
        private readonly Dictionary<PBProtoFile, string> _relativePathWithoutExtension = new Dictionary<PBProtoFile, string>();

        public IReadOnlyList<PBProtoFile> Files { get; }
        public PBProtoProj Proj { get; }
        public PBProtoCodeGenOptions Options { get; }

        internal PBProtoCodeGenInput(PBProtoProj proj, PBProtoCodeGenOptions options)
        {
            Proj = proj;
            Files = proj.Files;
            Options = options;

            foreach (PBProtoFile file in Files)
                _relativePathWithoutExtension.Add(file, Path.ChangeExtension(MakeRelativePath(proj.RootDir, file.FilePath), null));
        }

        public string GetRelativePathWithoutExtension(PBProtoFile file)
        {
            return _relativePathWithoutExtension[file];
        }

        private static string MakeRelativePath(string root, string path)
        {
            Uri rootUri = new Uri(AppendDirectorySeparatorChar(root));
            Uri pathUri = new Uri(path);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                return path + Path.DirectorySeparatorChar;
            return path;
        }
    }

    public interface IPBProtoCodeGenBackend
    {
        string LanguageId { get; }
        IReadOnlyList<PBProtoGeneratedFile> Generate(PBProtoCodeGenInput input);
        IEnumerable<string> GetGeneratedRelativePaths(PBProtoCodeGenInput input, PBProtoFile file);
    }

    public static class PBProtoCodeGenerator
    {
        public const string CSharpLanguageId = "csharp";

        private const string SerializationToolDllPath = "Packages/com.fancyhub.unitylibs.protobuf/Runtime/RoslynAnalyzers/FH.Protobuf.SourceGenerator.dll";

        private static readonly Dictionary<string, IPBProtoCodeGenBackend> Backends = new Dictionary<string, IPBProtoCodeGenBackend>(StringComparer.OrdinalIgnoreCase)
        {
            { CSharpLanguageId, new CSharpCodeGenBackend() },
        };

        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
            "virtual", "void", "volatile", "while",
        };

        private static readonly Dictionary<string, ScalarInfo> Scalars = new Dictionary<string, ScalarInfo>(StringComparer.Ordinal)
        {
            { "double", new ScalarInfo("double", "WriteDouble", "ReadDouble", "Double", false) },
            { "float", new ScalarInfo("float", "WriteFloat", "ReadFloat", "Float", false) },
            { "int32", new ScalarInfo("int", "WriteInt32", "ReadInt32", "Int32", false) },
            { "int64", new ScalarInfo("long", "WriteInt64", "ReadInt64", "Int64", false) },
            { "uint32", new ScalarInfo("uint", "WriteUInt32", "ReadUInt32", "UInt32", false) },
            { "uint64", new ScalarInfo("ulong", "WriteUInt64", "ReadUInt64", "UInt64", false) },
            { "sint32", new ScalarInfo("int", "WriteSInt32", "ReadSInt32", "SInt32", true) },
            { "sint64", new ScalarInfo("long", "WriteSInt64", "ReadSInt64", "SInt64", true) },
            { "fixed32", new ScalarInfo("uint", "WriteFixed32", "ReadFixed32", "Fixed32", true) },
            { "fixed64", new ScalarInfo("ulong", "WriteFixed64", "ReadFixed64", "Fixed64", true) },
            { "sfixed32", new ScalarInfo("int", "WriteSFixed32", "ReadSFixed32", "SFixed32", true) },
            { "sfixed64", new ScalarInfo("long", "WriteSFixed64", "ReadSFixed64", "SFixed64", true) },
            { "bool", new ScalarInfo("bool", "WriteBool", "ReadBool", "Bool", false) },
            { "string", new ScalarInfo("string", "WriteString", "ReadString", "String", false) },
            { "bytes", new ScalarInfo("byte[]", "WriteBytes", "ReadBytes", "Bytes", false) },
        };

        public static int Generate(PBProtoProj proj, string outputDir)
        {
            return Generate(proj, outputDir, null);
        }

        public static int Generate(PBProtoProj proj, string outputDir, PBProtoCodeGenOptions options)
        {
            options ??= new PBProtoCodeGenOptions();            
            IPBProtoCodeGenBackend backend = GetBackend(options.LanguageId);
            outputDir = NormalizeOutputDir(outputDir);

            PBProtoCodeGenInput input = new PBProtoCodeGenInput(proj, options);
            foreach (PBProtoFile protoFile in proj.Files)
            {
                foreach (string relativePath in backend.GetGeneratedRelativePaths(input, protoFile))
                    DeleteFileIfExists(Path.Combine(outputDir, relativePath));
            }

            int genCount = 0;
            foreach (PBProtoGeneratedFile generatedFile in backend.Generate(input))
            {
                WriteGeneratedFile(Path.Combine(outputDir, generatedFile.RelativePath), generatedFile.Contents);
                genCount++;
            }

            return genCount;
        }

        public static void RegisterBackend(IPBProtoCodeGenBackend backend)
        {
            if (backend == null)
                throw new ArgumentNullException(nameof(backend));
            if (string.IsNullOrEmpty(backend.LanguageId))
                throw new ArgumentException("Backend language id is empty", nameof(backend));

            Backends[backend.LanguageId] = backend;
        }

        private static IPBProtoCodeGenBackend GetBackend(string languageId)
        {
            if (string.IsNullOrEmpty(languageId))
                languageId = CSharpLanguageId;
            if (Backends.TryGetValue(languageId, out IPBProtoCodeGenBackend backend))
                return backend;
            throw new ArgumentException("Unsupported protobuf code generation language: " + languageId, nameof(languageId));
        }

        private static void WriteGeneratedFile(string outPath, string code)
        {
            string outDir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(outDir))
                Directory.CreateDirectory(outDir);

            File.WriteAllText(outPath, code, new UTF8Encoding(false));
            Debug.Log("Generated protobuf code: " + outPath);
        }

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static string GenerateDataFile(PBProtoFile file, PBProtoGenerateContext context, PBProtoCodeGenOptions options)
        {
            bool emitAttributes = options.Mode == PBProtoCodeGenMode.CompileTime;
            bool emitCodec = options.Mode == PBProtoCodeGenMode.ForceGenerate;
            CodeWriter writer = BeginFile(file);

            foreach (PBProtoEnum pbEnum in file.Enums)
            {
                WriteEnum(writer, pbEnum);
                writer.Line();
            }

            foreach (PBProtoMessage msg in file.Messages)
            {
                WriteMessageData(writer, file, context, msg, options.MemberNameStyle, emitAttributes, emitCodec);
                writer.Line();
            }

            return EndFile(writer);
        }

        private static CodeWriter BeginFile(PBProtoFile file)
        {
            CodeWriter writer = new CodeWriter();
            writer.Line("// <auto-generated />");
            writer.Line("// Generated from: " + Path.GetFileName(file.FilePath));
            writer.Line("#pragma warning disable 0649");
            writer.Line("#pragma warning disable 0169");
            writer.Line();
            writer.Line("using System;");
            writer.Line("using System.Collections.Generic;");
            writer.Line("using FH;");
            writer.Line();
            writer.Line("namespace " + GetNamespace(file));
            writer.Line("{");
            writer.Indent++;
            return writer;
        }

        private static string EndFile(CodeWriter writer)
        {
            writer.Indent--;
            writer.Line("}");
            return writer.ToString();
        }

        private static void WriteEnum(CodeWriter writer, PBProtoEnum pbEnum)
        {
            WriteComments(writer, pbEnum.Comments);
            writer.Line("public enum " + ToTypeName(pbEnum.Name));
            writer.Line("{");
            writer.Indent++;
            foreach (PBProtoEnumValue value in pbEnum.Values)
            {
                WriteComments(writer, value.Comments);
                writer.Line(ToEnumValueName(value.Name) + " = " + value.Value + ",");
            }
            writer.Indent--;
            writer.Line("}");
        }

        private static void WriteMessageData(CodeWriter writer, PBProtoFile file, PBProtoGenerateContext context, PBProtoMessage msg, PBProtoMemberNameStyle memberNameStyle, bool emitAttributes, bool emitCodec)
        {
            WriteComments(writer, msg.Comments);
            if (emitAttributes)
                writer.Line("[PBMessage]");
            writer.Line("public sealed partial class " + ToTypeName(msg.Name) + " : IPBMessage");
            writer.Line("{");
            writer.Indent++;

            foreach (PBProtoEnum pbEnum in msg.Enums)
            {
                WriteEnum(writer, pbEnum);
                writer.Line();
            }

            foreach (PBProtoMessage child in msg.Messages)
            {
                WriteMessageData(writer, file, context, child, memberNameStyle, emitAttributes, emitCodec);
                writer.Line();
            }

            Dictionary<PBProtoField, string> fieldNames = BuildFieldNameMap(msg, memberNameStyle);
            foreach (PBProtoField field in msg.Fields)
                WriteField(writer, file, context, msg, field, fieldNames[field], emitAttributes);

            if (emitCodec)
            {
                writer.Line();
                writer.Lines(PBSerializationToolBridge.GenerateMembers(CreateSerializationMessage(file, context, msg, memberNameStyle)));
            }

            writer.Indent--;
            writer.Line("}");
        }

        private static void WriteField(CodeWriter writer, PBProtoFile file, PBProtoGenerateContext context, PBProtoMessage msg, PBProtoField field, string fieldName, bool emitAttribute)
        {
            MethodTypeSpec fieldType = ResolveFieldType(file, context, msg, field);
            WriteComments(writer, field.Comments);
            if (emitAttribute)
                writer.Line(GetPBFieldAttribute(field, fieldType));

            writer.Line("public " + fieldType.CSharpType + " " + fieldName + GetFieldInitializer(fieldType) + ";");
        }

        private static void WriteComments(CodeWriter writer, IReadOnlyList<string> comments)
        {
            if (comments == null || comments.Count == 0)
                return;

            foreach (string comment in comments)
            {
                if (string.IsNullOrEmpty(comment))
                {
                    writer.Line("//");
                    continue;
                }

                string[] lines = comment.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                foreach (string line in lines)
                    writer.Line(ToCSharpCommentLine(line));
            }
        }

        private static string ToCSharpCommentLine(string comment)
        {
            if (string.IsNullOrEmpty(comment))
                return "//";

            string text = comment.TrimEnd();
            if (text.Length == 0)
                return "//";

            return text[0] == ' ' || text[0] == '\t' ? "//" + text : "// " + text;
        }

        private static string GetPBFieldAttribute(PBProtoField field, MethodTypeSpec fieldType)
        {
            List<string> args = new List<string> { field.Number.ToString() };
            if (fieldType.Kind == MethodTypeKind.Map)
            {
                AddAttributeTypeArg(args, "KeyType", fieldType.KeyType);
                AddAttributeTypeArg(args, "ValueType", fieldType.ValueType);
            }
            else if (fieldType.Kind == MethodTypeKind.Repeated)
            {
                AddAttributeTypeArg(args, "Type", fieldType.ElementType);
            }
            else
            {
                AddAttributeTypeArg(args, "Type", fieldType);
            }

            return "[PBField(" + Join(", ", args.ToArray()) + ")]";
        }

        private static void AddAttributeTypeArg(List<string> args, string name, MethodTypeSpec type)
        {
            if (type != null && type.RequiresExplicitPBFieldType)
                args.Add(name + " = EPBFieldType." + type.PBFieldTypeName);
        }

        private static string GetFieldInitializer(MethodTypeSpec type)
        {
            switch (type.Kind)
            {
                case MethodTypeKind.Repeated:
                case MethodTypeKind.Map:
                    return " = new " + type.CSharpType + "()";
                case MethodTypeKind.Scalar:
                    if (type.CSharpType == "string")
                        return " = string.Empty";
                    if (type.CSharpType == "byte[]")
                        return " = Array.Empty<byte>()";
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private static SerializationMessageSpec CreateSerializationMessage(
            PBProtoFile file,
            PBProtoGenerateContext context,
            PBProtoMessage msg,
            PBProtoMemberNameStyle memberNameStyle)
        {
            SerializationMessageSpec ret = new SerializationMessageSpec
            {
                NamespaceName = string.Empty,
                Accessibility = "public",
                Name = ToTypeName(msg.Name),
                IsSealed = true,
                ImplementIPBMessage = true,
                GenerateSerialize = true,
                GenerateUnserialize = true,
            };

            Dictionary<PBProtoField, string> fieldNames = BuildFieldNameMap(msg, memberNameStyle);
            foreach (PBProtoField field in msg.Fields)
            {
                ret.Fields.Add(new SerializationFieldSpec
                {
                    Name = fieldNames[field],
                    Number = field.Number,
                    Type = ResolveFieldType(file, context, msg, field),
                });
            }

            return ret;
        }

        private static MethodTypeSpec ResolveFieldType(PBProtoFile file, PBProtoGenerateContext context, PBProtoMessage scopeMsg, PBProtoField field)
        {
            if (field.IsMap)
            {
                MethodTypeSpec keyType = ResolveSingleType(file, context, scopeMsg, field.MapKeyType);
                MethodTypeSpec valueType = ResolveSingleType(file, context, scopeMsg, field.MapValueType);
                return MethodTypeSpec.Map("Dictionary<" + keyType.CSharpType + ", " + valueType.CSharpType + ">", keyType, valueType);
            }

            MethodTypeSpec type = ResolveSingleType(file, context, scopeMsg, field.TypeName);
            if (field.IsRepeated)
                return MethodTypeSpec.Repeated("List<" + type.CSharpType + ">", type);
            return type;
        }

        private static MethodTypeSpec ResolveSingleType(PBProtoFile file, PBProtoGenerateContext context, PBProtoMessage scopeMsg, string protoType)
        {
            if (Scalars.TryGetValue(protoType, out ScalarInfo scalar))
                return MethodTypeSpec.Scalar(scalar.CSharpType, scalar.WriteMethod, scalar.ReadMethod, scalar.PBFieldTypeName, scalar.RequiresExplicitPBFieldType);

            string typeName = context.GetCSharpType(file, scopeMsg, protoType);
            if (context.IsEnum(file, scopeMsg, protoType))
                return MethodTypeSpec.Enum(typeName);

            return MethodTypeSpec.Message(typeName);
        }

        private static string GetNamespace(PBProtoFile file)
        {
            string ns = !string.IsNullOrEmpty(file.CSharpNamespace) ? file.CSharpNamespace : file.PackageName;
            if (string.IsNullOrEmpty(ns))
                return "FH";

            string[] parts = ns.Split('.');
            for (int i = 0; i < parts.Length; i++)
                parts[i] = SanitizeIdentifier(parts[i]);
            return Join(".", parts);
        }

        private static string ToCSharpTypeName(PBProtoFile file, string protoType)
        {
            string ret = protoType.TrimStart('.');
            if (!string.IsNullOrEmpty(file.PackageName) && ret.StartsWith(file.PackageName + ".", StringComparison.Ordinal))
                ret = ret.Substring(file.PackageName.Length + 1);

            string[] parts = ret.Split('.');
            for (int i = 0; i < parts.Length; i++)
                parts[i] = ToTypeName(parts[i]);
            return Join(".", parts);
        }

        private static Dictionary<PBProtoField, string> BuildFieldNameMap(PBProtoMessage msg, PBProtoMemberNameStyle memberNameStyle)
        {
            Dictionary<PBProtoField, string> ret = new Dictionary<PBProtoField, string>();
            HashSet<string> reserved = GetReservedMemberNames(msg);
            HashSet<string> used = new HashSet<string>(reserved, StringComparer.Ordinal);

            foreach (PBProtoField field in msg.Fields)
            {
                string fieldName = ToFieldName(field.Name, memberNameStyle);
                if (used.Contains(fieldName))
                {
                    string baseName = reserved.Contains(fieldName) ? fieldName + "_value" : fieldName;
                    fieldName = MakeUniqueName(baseName, used);
                }

                used.Add(fieldName);
                ret.Add(field, fieldName);
            }

            return ret;
        }

        private static HashSet<string> GetReservedMemberNames(PBProtoMessage msg)
        {
            HashSet<string> ret = new HashSet<string>(StringComparer.Ordinal)
            {
                "Serialize",
                "Unserialize",
            };
            foreach (PBProtoEnum pbEnum in msg.Enums)
                ret.Add(ToTypeName(pbEnum.Name));
            foreach (PBProtoMessage child in msg.Messages)
                ret.Add(ToTypeName(child.Name));
            return ret;
        }

        private static string MakeUniqueName(string baseName, HashSet<string> used)
        {
            string ret = baseName;
            int index = 2;
            while (used.Contains(ret))
                ret = baseName + index++;
            return ret;
        }

        private static string ToFieldName(string name, PBProtoMemberNameStyle memberNameStyle)
        {
            switch (memberNameStyle)
            {
                case PBProtoMemberNameStyle.PascalCase:
                    return SanitizeIdentifier(ToPascalCase(name));
                case PBProtoMemberNameStyle.CamelCase:
                    return SanitizeIdentifier(ToCamelCase(name));
                case PBProtoMemberNameStyle.UnderscoreCamelCase:
                    return SanitizeIdentifier("_" + ToCamelCase(name));
                case PBProtoMemberNameStyle.MUnderscoreCamelCase:
                    return SanitizeIdentifier("m_" + ToCamelCase(name));
                case PBProtoMemberNameStyle.KeepProtoName:
                default:
                    return SanitizeIdentifier(name);
            }
        }

        private static string ToTypeName(string name)
        {
            return SanitizeIdentifier(ToPascalCase(name));
        }

        private static string ToEnumValueName(string name)
        {
            return SanitizeIdentifier(ToPascalCase(name));
        }

        private static string ToCamelCase(string name)
        {
            string pascal = ToPascalCase(name);
            if (string.IsNullOrEmpty(pascal) || pascal == "_")
                return pascal;

            if (pascal[0] == '@')
                return pascal;

            return char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
        }

        private static string ToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "_";

            if (name.IndexOf('_') < 0 && char.IsUpper(name[0]))
                return name;

            if (name.IndexOf('_') < 0)
                return char.ToUpperInvariant(name[0]) + name.Substring(1);

            StringBuilder sb = new StringBuilder();
            bool upperNext = true;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c == '_')
                {
                    upperNext = true;
                    continue;
                }

                if (upperNext)
                {
                    sb.Append(char.ToUpperInvariant(c));
                    upperNext = false;
                }
                else
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }

            return sb.Length == 0 ? "_" : sb.ToString();
        }

        private static string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "_";

            StringBuilder sb = new StringBuilder(name.Length + 1);
            char first = name[0];
            if (!(char.IsLetter(first) || first == '_'))
                sb.Append('_');

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
            }

            string ret = sb.ToString();
            if (CSharpKeywords.Contains(ret))
                ret = "@" + ret;
            return ret;
        }

        private static string NormalizeDir(string dir, string argName)
        {
            if (string.IsNullOrEmpty(dir))
                throw new ArgumentException(argName + " is empty", argName);

            string fullPath = Path.GetFullPath(dir);
            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException(fullPath);
            return fullPath;
        }

        private static string NormalizeOutputDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                throw new ArgumentException("outputDir is empty", nameof(dir));

            string fullPath = Path.GetFullPath(dir);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        private static string Join(string sep, string[] parts)
        {
            if (parts == null || parts.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    sb.Append(sep);
                sb.Append(parts[i]);
            }
            return sb.ToString();
        }

        private sealed class CSharpCodeGenBackend : IPBProtoCodeGenBackend
        {
            public string LanguageId => CSharpLanguageId;

            public IReadOnlyList<PBProtoGeneratedFile> Generate(PBProtoCodeGenInput input)
            {
                PBProtoGenerateContext context = new PBProtoGenerateContext(input.Files);
                List<PBProtoGeneratedFile> ret = new List<PBProtoGeneratedFile>();
                foreach (PBProtoFile protoFile in input.Files)
                {
                    string relNoExt = input.GetRelativePathWithoutExtension(protoFile);
                    ret.Add(new PBProtoGeneratedFile(relNoExt + ".pb.cs", GenerateDataFile(protoFile, context, input.Options)));
                }

                return ret;
            }

            public IEnumerable<string> GetGeneratedRelativePaths(PBProtoCodeGenInput input, PBProtoFile file)
            {
                string relNoExt = input.GetRelativePathWithoutExtension(file);
                yield return relNoExt + ".pb.cs";
                yield return relNoExt + ".pb.codec.cs";
                yield return relNoExt + ".pb.serialize.cs";
                yield return relNoExt + ".pb.unserialize.cs";
            }
        }

        private struct ScalarInfo
        {
            public readonly string CSharpType;
            public readonly string WriteMethod;
            public readonly string ReadMethod;
            public readonly string PBFieldTypeName;
            public readonly bool RequiresExplicitPBFieldType;

            public ScalarInfo(string csharpType, string writeMethod, string readMethod, string pbFieldTypeName, bool requiresExplicitPBFieldType)
            {
                CSharpType = csharpType;
                WriteMethod = writeMethod;
                ReadMethod = readMethod;
                PBFieldTypeName = pbFieldTypeName;
                RequiresExplicitPBFieldType = requiresExplicitPBFieldType;
            }
        }

        private enum MethodTypeKind
        {
            Scalar,
            Enum,
            Message,
            Repeated,
            Map,
        }

        private sealed class MethodTypeSpec
        {
            public MethodTypeKind Kind;
            public string CSharpType;
            public string WriteMethod;
            public string ReadMethod;
            public string PBFieldTypeName;
            public bool RequiresExplicitPBFieldType;
            public MethodTypeSpec ElementType;
            public MethodTypeSpec KeyType;
            public MethodTypeSpec ValueType;

            public static MethodTypeSpec Scalar(string csharpType, string writeMethod, string readMethod, string pbFieldTypeName, bool requiresExplicitPBFieldType)
            {
                return new MethodTypeSpec
                {
                    Kind = MethodTypeKind.Scalar,
                    CSharpType = csharpType,
                    WriteMethod = writeMethod,
                    ReadMethod = readMethod,
                    PBFieldTypeName = pbFieldTypeName,
                    RequiresExplicitPBFieldType = requiresExplicitPBFieldType,
                };
            }

            public static MethodTypeSpec Enum(string csharpType)
            {
                return new MethodTypeSpec
                {
                    Kind = MethodTypeKind.Enum,
                    CSharpType = csharpType,
                    PBFieldTypeName = "Enum",
                    RequiresExplicitPBFieldType = false,
                };
            }

            public static MethodTypeSpec Message(string csharpType)
            {
                return new MethodTypeSpec
                {
                    Kind = MethodTypeKind.Message,
                    CSharpType = csharpType,
                    PBFieldTypeName = "Message",
                    RequiresExplicitPBFieldType = false,
                };
            }

            public static MethodTypeSpec Repeated(string csharpType, MethodTypeSpec elementType)
            {
                return new MethodTypeSpec
                {
                    Kind = MethodTypeKind.Repeated,
                    CSharpType = csharpType,
                    ElementType = elementType,
                };
            }

            public static MethodTypeSpec Map(string csharpType, MethodTypeSpec keyType, MethodTypeSpec valueType)
            {
                return new MethodTypeSpec
                {
                    Kind = MethodTypeKind.Map,
                    CSharpType = csharpType,
                    KeyType = keyType,
                    ValueType = valueType,
                };
            }
        }

        private sealed class SerializationMessageSpec
        {
            public string NamespaceName;
            public string Accessibility;
            public string Name;
            public bool IsSealed;
            public bool ImplementIPBMessage;
            public bool GenerateSerialize;
            public bool GenerateUnserialize;
            public readonly List<SerializationFieldSpec> Fields = new List<SerializationFieldSpec>();
        }

        private sealed class SerializationFieldSpec
        {
            public string Name;
            public int Number;
            public MethodTypeSpec Type;
        }

        private static class PBSerializationToolBridge
        {
            private static Assembly _assembly;
            private static Type _messageType;
            private static Type _fieldType;
            private static Type _typeRefType;
            private static MethodInfo _generatePartialDeclaration;
            private static MethodInfo _generateMembers;
            private static MethodInfo _scalar;
            private static MethodInfo _enum;
            private static MethodInfo _message;
            private static MethodInfo _repeated;
            private static MethodInfo _map;

            public static string GeneratePartialDeclaration(SerializationMessageSpec message)
            {
                EnsureLoaded();
                object toolMessage = CreateMessage(message);
                return (string)_generatePartialDeclaration.Invoke(null, new[] { toolMessage });
            }

            public static string GenerateMembers(SerializationMessageSpec message)
            {
                EnsureLoaded();
                object toolMessage = CreateMessage(message);
                return (string)_generateMembers.Invoke(null, new[] { toolMessage });
            }

            private static void EnsureLoaded()
            {
                if (_assembly != null)
                    return;

                string fullPath = Path.GetFullPath(SerializationToolDllPath);
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("Protobuf Roslyn tool DLL not found", fullPath);

                _assembly = Assembly.LoadFrom(fullPath);
                _messageType = _assembly.GetType("FH.Protobuf.SourceGenerator.PBSerializationMessage", true);
                _fieldType = _assembly.GetType("FH.Protobuf.SourceGenerator.PBSerializationField", true);
                _typeRefType = _assembly.GetType("FH.Protobuf.SourceGenerator.PBSerializationTypeRef", true);
                Type toolType = _assembly.GetType("FH.Protobuf.SourceGenerator.PBSerializationCodeGenTool", true);

                _generatePartialDeclaration = toolType.GetMethod("GeneratePartialDeclaration", BindingFlags.Public | BindingFlags.Static);
                _generateMembers = toolType.GetMethod("GenerateMembers", BindingFlags.Public | BindingFlags.Static);
                _scalar = _typeRefType.GetMethod("Scalar", BindingFlags.Public | BindingFlags.Static);
                _enum = _typeRefType.GetMethod("Enum", BindingFlags.Public | BindingFlags.Static);
                _message = _typeRefType.GetMethod("Message", BindingFlags.Public | BindingFlags.Static);
                _repeated = _typeRefType.GetMethod("Repeated", BindingFlags.Public | BindingFlags.Static);
                _map = _typeRefType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
            }

            private static object CreateMessage(SerializationMessageSpec message)
            {
                object ret = Activator.CreateInstance(_messageType);
                SetField(ret, "NamespaceName", message.NamespaceName);
                SetField(ret, "Accessibility", message.Accessibility);
                SetField(ret, "Name", message.Name);
                SetField(ret, "IsSealed", message.IsSealed);
                SetField(ret, "ImplementIPBMessage", message.ImplementIPBMessage);
                SetField(ret, "GenerateSerialize", message.GenerateSerialize);
                SetField(ret, "GenerateUnserialize", message.GenerateUnserialize);

                IList fields = (IList)_messageType.GetField("Fields").GetValue(ret);
                foreach (SerializationFieldSpec field in message.Fields)
                    fields.Add(CreateField(field));

                return ret;
            }

            private static object CreateField(SerializationFieldSpec field)
            {
                object ret = Activator.CreateInstance(_fieldType);
                SetField(ret, "Name", field.Name);
                SetField(ret, "Number", field.Number);
                SetField(ret, "Type", CreateTypeRef(field.Type));
                return ret;
            }

            private static object CreateTypeRef(MethodTypeSpec type)
            {
                switch (type.Kind)
                {
                    case MethodTypeKind.Enum:
                        return _enum.Invoke(null, new object[] { type.CSharpType });
                    case MethodTypeKind.Message:
                        return _message.Invoke(null, new object[] { type.CSharpType });
                    case MethodTypeKind.Repeated:
                        return _repeated.Invoke(null, new[] { type.CSharpType, CreateTypeRef(type.ElementType) });
                    case MethodTypeKind.Map:
                        return _map.Invoke(null, new[] { type.CSharpType, CreateTypeRef(type.KeyType), CreateTypeRef(type.ValueType) });
                    case MethodTypeKind.Scalar:
                    default:
                        return _scalar.Invoke(null, new object[] { type.CSharpType, type.WriteMethod, type.ReadMethod });
                }
            }

            private static void SetField(object target, string fieldName, object value)
            {
                target.GetType().GetField(fieldName).SetValue(target, value);
            }
        }

        private sealed class PBProtoGenerateContext
        {
            private readonly PBProtoTypeRegistry _typeRegistry;
            private readonly Dictionary<string, string> _typeMap = new Dictionary<string, string>(StringComparer.Ordinal);

            public PBProtoGenerateContext(IReadOnlyList<PBProtoFile> files)
            {
                _typeRegistry = new PBProtoTypeRegistry(files);

                foreach (PBProtoFile file in files)
                {
                    foreach (PBProtoEnum pbEnum in file.Enums)
                        AddType(file, null, null, pbEnum.Name);
                    foreach (PBProtoMessage msg in file.Messages)
                        Collect(file, null, null, msg);
                }
            }

            public string GetCSharpType(PBProtoFile file, PBProtoMessage scopeMsg, string typeName)
            {
                foreach (string key in _typeRegistry.GetTypeLookupKeys(file, scopeMsg, typeName))
                {
                    if (_typeMap.TryGetValue(key, out string ret))
                        return ToGlobalTypeName(ret);
                }

                return ToCSharpTypeName(file, typeName);
            }

            public bool IsEnum(PBProtoFile file, PBProtoMessage scopeMsg, string typeName)
            {
                return _typeRegistry.IsEnum(file, scopeMsg, typeName);
            }

            private void Collect(PBProtoFile file, string parentProtoName, string parentCSharpName, PBProtoMessage msg)
            {
                string msgProtoName = string.IsNullOrEmpty(parentProtoName) ? msg.Name : parentProtoName + "." + msg.Name;
                string msgCSharpName = string.IsNullOrEmpty(parentCSharpName) ? ToTypeName(msg.Name) : parentCSharpName + "." + ToTypeName(msg.Name);
                AddType(file, parentProtoName, parentCSharpName, msg.Name);

                foreach (PBProtoEnum pbEnum in msg.Enums)
                    AddType(file, msgProtoName, msgCSharpName, pbEnum.Name);
                foreach (PBProtoMessage child in msg.Messages)
                    Collect(file, msgProtoName, msgCSharpName, child);
            }

            private void AddType(PBProtoFile file, string parentProtoName, string parentCSharpName, string typeName)
            {
                string protoName = string.IsNullOrEmpty(parentProtoName) ? typeName : parentProtoName + "." + typeName;
                string csharpName = string.IsNullOrEmpty(parentCSharpName) ? ToTypeName(typeName) : parentCSharpName + "." + ToTypeName(typeName);
                string ns = GetNamespace(file);
                string csharpFullName = string.IsNullOrEmpty(ns) ? csharpName : ns + "." + csharpName;

                AddTypeMap(protoName, csharpFullName);
                if (!string.IsNullOrEmpty(file.PackageName))
                    AddTypeMap(file.PackageName + "." + protoName, csharpFullName);

                if (string.IsNullOrEmpty(parentProtoName))
                    AddTypeMap(typeName, csharpFullName);
            }

            private void AddTypeMap(string protoName, string csharpName)
            {
                if (!_typeMap.ContainsKey(protoName))
                    _typeMap.Add(protoName, csharpName);
            }

            private static string ToGlobalTypeName(string fullName)
            {
                return "global::" + fullName;
            }
        }

        private sealed class CodeWriter
        {
            private readonly StringBuilder _sb = new StringBuilder();
            public int Indent;

            public void Line()
            {
                _sb.AppendLine();
            }

            public void Line(string value)
            {
                for (int i = 0; i < Indent; i++)
                    _sb.Append("    ");
                _sb.AppendLine(value);
            }

            public void Lines(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return;

                string[] lines = value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                foreach (string line in lines)
                {
                    if (line.Length == 0)
                        Line();
                    else
                        Line(line);
                }
            }

            public override string ToString()
            {
                return _sb.ToString();
            }
        }
    }
}
