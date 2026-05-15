using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FH.Protobuf.SourceGenerator
{
    [Generator]
    public sealed class PBAttributeSourceGenerator : ISourceGenerator
    {
        private const string MessageAttributeName = "FH.PBMessageAttribute";
        private const string FieldAttributeName = "FH.PBFieldAttribute";
        private const string IgnoreAttributeName = "FH.PBIgnoreAttribute";
        private const string MessageInterfaceName = "FH.IPBMessage";

        private static readonly DiagnosticDescriptor PartialTypeRule = new DiagnosticDescriptor(
            "FHPB001",
            "PB message type must be partial",
            "PB message type '{0}' must be a non-generic partial class",
            "FH.Protobuf",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor FieldNumberRule = new DiagnosticDescriptor(
            "FHPB002",
            "Invalid protobuf field number",
            "PB field '{0}' on '{1}' must use a positive, unique field number",
            "FH.Protobuf",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor UnsupportedTypeRule = new DiagnosticDescriptor(
            "FHPB003",
            "Unsupported protobuf field type",
            "PB field '{0}' on '{1}' has unsupported type '{2}'",
            "FH.Protobuf",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor WritableMemberRule = new DiagnosticDescriptor(
            "FHPB004",
            "PB field must be writable",
            "PB field '{0}' on '{1}' must be writable so deserialization can assign it",
            "FH.Protobuf",
            DiagnosticSeverity.Error,
            true);

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (IsDisabled(context))
                return;

            SyntaxReceiver receiver = context.SyntaxReceiver as SyntaxReceiver;
            if (receiver == null || receiver.Candidates.Count == 0)
                return;

            INamedTypeSymbol messageAttribute = context.Compilation.GetTypeByMetadataName(MessageAttributeName);
            INamedTypeSymbol fieldAttribute = context.Compilation.GetTypeByMetadataName(FieldAttributeName);
            INamedTypeSymbol ignoreAttribute = context.Compilation.GetTypeByMetadataName(IgnoreAttributeName);
            INamedTypeSymbol messageInterface = context.Compilation.GetTypeByMetadataName(MessageInterfaceName);
            if (messageAttribute == null || fieldAttribute == null)
                return;

            HashSet<ISymbol> processedTypes = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            foreach (TypeDeclarationSyntax candidate in receiver.Candidates)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                INamedTypeSymbol typeSymbol = model.GetDeclaredSymbol(candidate) as INamedTypeSymbol;
                if (typeSymbol == null)
                    continue;
                if (!processedTypes.Add(typeSymbol))
                    continue;

                AttributeData messageAttr = FindAttribute(typeSymbol.GetAttributes(), messageAttribute);
                if (messageAttr == null)
                    continue;

                if (!IsValidMessageType(typeSymbol))
                {
                    Report(context, PartialTypeRule, candidate.Identifier.GetLocation(), typeSymbol.Name);
                    continue;
                }

                MessageOptions options = ReadMessageOptions(messageAttr);
                List<FieldSpec> fields = CollectFields(context, typeSymbol, fieldAttribute, ignoreAttribute, messageAttribute, messageInterface);
                if (fields == null)
                    continue;

                string source = PBSerializationCodeGenTool.GenerateSource(CreateSerializationMessage(typeSymbol, options, fields, messageInterface));
                context.AddSource(GetHintName(typeSymbol, candidate.SyntaxTree.FilePath), SourceText.From(source, Encoding.UTF8));
            }
        }

        private static bool IsDisabled(GeneratorExecutionContext context)
        {
            string value;
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.FHProtobufAttributeGeneration", out value)
                || context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("FHProtobufAttributeGeneration", out value))
            {
                return string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(value, "0", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static List<FieldSpec> CollectFields(
            GeneratorExecutionContext context,
            INamedTypeSymbol typeSymbol,
            INamedTypeSymbol fieldAttribute,
            INamedTypeSymbol ignoreAttribute,
            INamedTypeSymbol messageAttribute,
            INamedTypeSymbol messageInterface)
        {
            List<FieldSpec> fields = new List<FieldSpec>();
            HashSet<int> usedNumbers = new HashSet<int>();
            foreach (ISymbol member in typeSymbol.GetMembers())
            {
                if (ignoreAttribute != null && FindAttribute(member.GetAttributes(), ignoreAttribute) != null)
                    continue;

                AttributeData fieldAttr = FindAttribute(member.GetAttributes(), fieldAttribute);
                if (fieldAttr == null)
                    continue;

                ITypeSymbol memberType = GetMemberType(member);
                if (memberType == null)
                    continue;

                int number = ReadFieldNumber(fieldAttr);
                if (number <= 0 || !usedNumbers.Add(number))
                {
                    Report(context, FieldNumberRule, GetLocation(member), member.Name, typeSymbol.Name);
                    return null;
                }

                if (!IsWritable(member))
                {
                    Report(context, WritableMemberRule, GetLocation(member), member.Name, typeSymbol.Name);
                    return null;
                }

                FieldOptions fieldOptions = ReadFieldOptions(fieldAttr);
                TypeSpec typeSpec = ResolveType(memberType, fieldOptions, messageAttribute, messageInterface);
                if (typeSpec == null)
                {
                    Report(context, UnsupportedTypeRule, GetLocation(member), member.Name, typeSymbol.Name, memberType.ToDisplayString());
                    return null;
                }

                fields.Add(new FieldSpec(member.Name, number, typeSpec));
            }

            fields.Sort((a, b) => a.Number.CompareTo(b.Number));
            return fields;
        }

        private static TypeSpec ResolveType(
            ITypeSymbol type,
            FieldOptions fieldOptions,
            INamedTypeSymbol messageAttribute,
            INamedTypeSymbol messageInterface)
        {
            PBFieldType requestedType = fieldOptions.Type;
            if (IsByteArray(type) && (requestedType == PBFieldType.Auto || requestedType == PBFieldType.Bytes))
                return TypeSpec.Scalar(type, "WriteBytes", "ReadBytes");

            INamedTypeSymbol namedType = type as INamedTypeSymbol;
            if (namedType != null && namedType.IsGenericType)
            {
                if (IsGenericType(namedType, "System.Collections.Generic.List<T>"))
                {
                    PBFieldType elementTypeRequest = fieldOptions.ValueType != PBFieldType.Auto ? fieldOptions.ValueType : requestedType;
                    TypeSpec elementType = ResolveSingleType(namedType.TypeArguments[0], elementTypeRequest, messageAttribute, messageInterface);
                    return elementType == null ? null : TypeSpec.Repeated(type, elementType);
                }

                if (IsGenericType(namedType, "System.Collections.Generic.Dictionary<TKey, TValue>"))
                {
                    PBFieldType valueTypeRequest = fieldOptions.ValueType != PBFieldType.Auto ? fieldOptions.ValueType : requestedType;
                    TypeSpec keyType = ResolveSingleType(namedType.TypeArguments[0], fieldOptions.KeyType, messageAttribute, messageInterface);
                    TypeSpec valueType = ResolveSingleType(namedType.TypeArguments[1], valueTypeRequest, messageAttribute, messageInterface);
                    if (keyType == null || valueType == null || keyType.Kind == PBTypeKind.Message)
                        return null;
                    return TypeSpec.Map(type, keyType, valueType);
                }
            }

            return ResolveSingleType(type, requestedType, messageAttribute, messageInterface);
        }

        private static TypeSpec ResolveSingleType(
            ITypeSymbol type,
            PBFieldType requestedType,
            INamedTypeSymbol messageAttribute,
            INamedTypeSymbol messageInterface)
        {
            PBFieldType effectiveType = requestedType == PBFieldType.Auto ? InferFieldType(type, messageAttribute, messageInterface) : requestedType;
            switch (effectiveType)
            {
                case PBFieldType.Double:
                    return TypeSpec.Scalar(type, "WriteDouble", "ReadDouble");
                case PBFieldType.Float:
                    return TypeSpec.Scalar(type, "WriteFloat", "ReadFloat");
                case PBFieldType.Int32:
                    return TypeSpec.Scalar(type, "WriteInt32", "ReadInt32");
                case PBFieldType.Int64:
                    return TypeSpec.Scalar(type, "WriteInt64", "ReadInt64");
                case PBFieldType.UInt32:
                    return TypeSpec.Scalar(type, "WriteUInt32", "ReadUInt32");
                case PBFieldType.UInt64:
                    return TypeSpec.Scalar(type, "WriteUInt64", "ReadUInt64");
                case PBFieldType.SInt32:
                    return TypeSpec.Scalar(type, "WriteSInt32", "ReadSInt32");
                case PBFieldType.SInt64:
                    return TypeSpec.Scalar(type, "WriteSInt64", "ReadSInt64");
                case PBFieldType.Fixed32:
                    return TypeSpec.Scalar(type, "WriteFixed32", "ReadFixed32");
                case PBFieldType.Fixed64:
                    return TypeSpec.Scalar(type, "WriteFixed64", "ReadFixed64");
                case PBFieldType.SFixed32:
                    return TypeSpec.Scalar(type, "WriteSFixed32", "ReadSFixed32");
                case PBFieldType.SFixed64:
                    return TypeSpec.Scalar(type, "WriteSFixed64", "ReadSFixed64");
                case PBFieldType.Bool:
                    return TypeSpec.Scalar(type, "WriteBool", "ReadBool");
                case PBFieldType.String:
                    return TypeSpec.Scalar(type, "WriteString", "ReadString");
                case PBFieldType.Bytes:
                    return IsByteArray(type) ? TypeSpec.Scalar(type, "WriteBytes", "ReadBytes") : null;
                case PBFieldType.Enum:
                    return type.TypeKind == TypeKind.Enum ? TypeSpec.Enum(type) : null;
                case PBFieldType.Message:
                    return IsMessageType(type, messageAttribute, messageInterface) ? TypeSpec.Message(type) : null;
                default:
                    return null;
            }
        }

        private static PBFieldType InferFieldType(
            ITypeSymbol type,
            INamedTypeSymbol messageAttribute,
            INamedTypeSymbol messageInterface)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Double:
                    return PBFieldType.Double;
                case SpecialType.System_Single:
                    return PBFieldType.Float;
                case SpecialType.System_Int32:
                    return PBFieldType.Int32;
                case SpecialType.System_Int64:
                    return PBFieldType.Int64;
                case SpecialType.System_UInt32:
                    return PBFieldType.UInt32;
                case SpecialType.System_UInt64:
                    return PBFieldType.UInt64;
                case SpecialType.System_Boolean:
                    return PBFieldType.Bool;
                case SpecialType.System_String:
                    return PBFieldType.String;
            }

            if (IsByteArray(type))
                return PBFieldType.Bytes;
            if (type.TypeKind == TypeKind.Enum)
                return PBFieldType.Enum;
            if (IsMessageType(type, messageAttribute, messageInterface))
                return PBFieldType.Message;
            return PBFieldType.Auto;
        }

        private static PBSerializationMessage CreateSerializationMessage(INamedTypeSymbol typeSymbol, MessageOptions options, List<FieldSpec> fields, INamedTypeSymbol messageInterface)
        {
            PBSerializationMessage message = new PBSerializationMessage
            {
                NamespaceName = GetNamespace(typeSymbol),
                Accessibility = GetAccessibility(typeSymbol.DeclaredAccessibility),
                Name = typeSymbol.Name,
                IsSealed = typeSymbol.IsSealed,
                ImplementIPBMessage = !ImplementsMessageInterface(typeSymbol, messageInterface),
                GenerateSerialize = options.GenerateSerialize,
                GenerateUnserialize = options.GenerateUnserialize,
            };

            foreach (INamedTypeSymbol containingType in GetContainingTypes(typeSymbol))
            {
                message.ContainingTypes.Add(new PBSerializationContainingType
                {
                    Accessibility = GetAccessibility(containingType.DeclaredAccessibility),
                    Kind = GetTypeKeyword(containingType),
                    Name = containingType.Name,
                    IsSealed = containingType.IsSealed,
                });
            }

            foreach (FieldSpec field in fields)
            {
                message.Fields.Add(new PBSerializationField
                {
                    Name = field.Name,
                    Number = field.Number,
                    Type = ToSerializationType(field.Type),
                });
            }

            return message;
        }

        private static bool ImplementsMessageInterface(INamedTypeSymbol typeSymbol, INamedTypeSymbol messageInterface)
        {
            if (messageInterface == null)
                return false;

            foreach (INamedTypeSymbol iface in typeSymbol.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(iface, messageInterface))
                    return true;
            }

            return false;
        }

        private static PBSerializationTypeRef ToSerializationType(TypeSpec type)
        {
            switch (type.Kind)
            {
                case PBTypeKind.Enum:
                    return PBSerializationTypeRef.Enum(type.CSharpType);
                case PBTypeKind.Message:
                    return PBSerializationTypeRef.Message(type.CSharpType);
                case PBTypeKind.Repeated:
                    return PBSerializationTypeRef.Repeated(type.CSharpType, ToSerializationType(type.ElementType));
                case PBTypeKind.Map:
                    return PBSerializationTypeRef.Map(type.CSharpType, ToSerializationType(type.KeyType), ToSerializationType(type.ValueType));
                case PBTypeKind.Scalar:
                default:
                    return PBSerializationTypeRef.Scalar(type.CSharpType, type.WriteMethod, type.ReadMethod);
            }
        }

        private static AttributeData FindAttribute(ImmutableArray<AttributeData> attributes, INamedTypeSymbol attributeType)
        {
            foreach (AttributeData attribute in attributes)
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
                    return attribute;
            }

            return null;
        }

        private static MessageOptions ReadMessageOptions(AttributeData attribute)
        {
            MessageOptions options = new MessageOptions();
            foreach (KeyValuePair<string, TypedConstant> item in attribute.NamedArguments)
            {
                if (item.Key == "GenerateSerialize" && item.Value.Value is bool)
                    options.GenerateSerialize = (bool)item.Value.Value;
                if (item.Key == "GenerateUnserialize" && item.Value.Value is bool)
                    options.GenerateUnserialize = (bool)item.Value.Value;
            }

            return options;
        }

        private static int ReadFieldNumber(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length == 0)
                return 0;
            object value = attribute.ConstructorArguments[0].Value;
            return value is int ? (int)value : 0;
        }

        private static FieldOptions ReadFieldOptions(AttributeData attribute)
        {
            FieldOptions ret = new FieldOptions();
            foreach (KeyValuePair<string, TypedConstant> item in attribute.NamedArguments)
            {
                if (item.Key == "Type" && item.Value.Value is int)
                    ret.Type = (PBFieldType)(int)item.Value.Value;
                else if (item.Key == "KeyType" && item.Value.Value is int)
                    ret.KeyType = (PBFieldType)(int)item.Value.Value;
                else if (item.Key == "ValueType" && item.Value.Value is int)
                    ret.ValueType = (PBFieldType)(int)item.Value.Value;
            }

            return ret;
        }

        private static ITypeSymbol GetMemberType(ISymbol member)
        {
            IFieldSymbol field = member as IFieldSymbol;
            if (field != null && !field.IsStatic)
                return field.Type;

            IPropertySymbol property = member as IPropertySymbol;
            if (property != null && !property.IsStatic)
                return property.Type;

            return null;
        }

        private static bool IsWritable(ISymbol member)
        {
            IFieldSymbol field = member as IFieldSymbol;
            if (field != null)
                return !field.IsStatic && !field.IsConst && !field.IsReadOnly;

            IPropertySymbol property = member as IPropertySymbol;
            if (property != null)
                return !property.IsStatic && property.SetMethod != null;

            return false;
        }

        private static bool IsValidMessageType(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind != TypeKind.Class || typeSymbol.TypeParameters.Length > 0)
                return false;

            INamedTypeSymbol current = typeSymbol;
            while (current != null)
            {
                if (current.TypeParameters.Length > 0)
                    return false;

                TypeDeclarationSyntax syntax = current.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax())
                    .OfType<TypeDeclarationSyntax>()
                    .FirstOrDefault();
                if (syntax == null || !syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                    return false;
                current = current.ContainingType;
            }

            return true;
        }

        private static bool IsByteArray(ITypeSymbol type)
        {
            IArrayTypeSymbol arrayType = type as IArrayTypeSymbol;
            return arrayType != null && arrayType.ElementType.SpecialType == SpecialType.System_Byte;
        }

        private static bool IsGenericType(INamedTypeSymbol type, string metadataName)
        {
            return type.ConstructedFrom != null && type.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) == metadataName;
        }

        private static bool IsMessageType(ITypeSymbol type, INamedTypeSymbol messageAttribute, INamedTypeSymbol messageInterface)
        {
            INamedTypeSymbol namedType = type as INamedTypeSymbol;
            if (namedType == null)
                return false;

            if (messageAttribute != null && FindAttribute(namedType.GetAttributes(), messageAttribute) != null)
                return true;

            if (messageInterface != null)
            {
                foreach (INamedTypeSymbol iface in namedType.AllInterfaces)
                {
                    if (SymbolEqualityComparer.Default.Equals(iface, messageInterface))
                        return true;
                }
            }

            return false;
        }

        private static List<INamedTypeSymbol> GetContainingTypes(INamedTypeSymbol typeSymbol)
        {
            List<INamedTypeSymbol> ret = new List<INamedTypeSymbol>();
            INamedTypeSymbol current = typeSymbol.ContainingType;
            while (current != null)
            {
                ret.Add(current);
                current = current.ContainingType;
            }

            ret.Reverse();
            return ret;
        }

        private static string GetNamespace(INamedTypeSymbol typeSymbol)
        {
            INamespaceSymbol ns = typeSymbol.ContainingNamespace;
            if (ns == null || ns.IsGlobalNamespace)
                return string.Empty;
            return ns.ToDisplayString();
        }

        private static string GetAccessibility(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Public:
                    return "public";
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.Private:
                    return "private";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.ProtectedAndInternal:
                    return "private protected";
                case Accessibility.ProtectedOrInternal:
                    return "protected internal";
                default:
                    return "internal";
            }
        }

        private static string GetTypeKeyword(INamedTypeSymbol type)
        {
            switch (type.TypeKind)
            {
                case TypeKind.Struct:
                    return "struct";
                case TypeKind.Interface:
                    return "interface";
                default:
                    return "class";
            }
        }

        private static Location GetLocation(ISymbol symbol)
        {
            return symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        }

        private static void Report(GeneratorExecutionContext context, DiagnosticDescriptor rule, Location location, params object[] args)
        {
            context.ReportDiagnostic(Diagnostic.Create(rule, location, args));
        }

        private static string GetHintName(INamedTypeSymbol type, string sourcePath)
        {
            string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .Replace("global::", string.Empty)
                .Replace(".", "_")
                .Replace("+", "_");
            return typeName + "_" + GetStableHash(sourcePath ?? string.Empty).ToString("x8") + ".pb.g.cs";
        }

        private static uint GetStableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                for (int i = 0; i < value.Length; i++)
                    hash = (hash ^ value[i]) * 16777619;
                return hash;
            }
        }

        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            public readonly List<TypeDeclarationSyntax> Candidates = new List<TypeDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                TypeDeclarationSyntax typeDeclaration = syntaxNode as TypeDeclarationSyntax;
                if (typeDeclaration != null && typeDeclaration.AttributeLists.Count > 0)
                    Candidates.Add(typeDeclaration);
            }
        }

        private sealed class MessageOptions
        {
            public bool GenerateSerialize = true;
            public bool GenerateUnserialize = true;
        }

        private sealed class FieldOptions
        {
            public PBFieldType Type = PBFieldType.Auto;
            public PBFieldType KeyType = PBFieldType.Auto;
            public PBFieldType ValueType = PBFieldType.Auto;
        }

        private sealed class FieldSpec
        {
            public readonly string Name;
            public readonly int Number;
            public readonly TypeSpec Type;

            public FieldSpec(string name, int number, TypeSpec type)
            {
                Name = name;
                Number = number;
                Type = type;
            }
        }

        private sealed class TypeSpec
        {
            public PBTypeKind Kind;
            public string CSharpType;
            public string WriteMethod;
            public string ReadMethod;
            public TypeSpec ElementType;
            public TypeSpec KeyType;
            public TypeSpec ValueType;

            public static TypeSpec Scalar(ITypeSymbol type, string writeMethod, string readMethod)
            {
                return new TypeSpec
                {
                    Kind = PBTypeKind.Scalar,
                    CSharpType = GetTypeName(type),
                    WriteMethod = writeMethod,
                    ReadMethod = readMethod,
                };
            }

            public static TypeSpec Enum(ITypeSymbol type)
            {
                return new TypeSpec
                {
                    Kind = PBTypeKind.Enum,
                    CSharpType = GetTypeName(type),
                    WriteMethod = "WriteEnum",
                    ReadMethod = "ReadEnum",
                };
            }

            public static TypeSpec Message(ITypeSymbol type)
            {
                return new TypeSpec
                {
                    Kind = PBTypeKind.Message,
                    CSharpType = GetTypeName(type),
                    WriteMethod = "WriteMessage",
                    ReadMethod = "ReadMessage",
                };
            }

            public static TypeSpec Repeated(ITypeSymbol type, TypeSpec elementType)
            {
                return new TypeSpec
                {
                    Kind = PBTypeKind.Repeated,
                    CSharpType = GetTypeName(type),
                    ElementType = elementType,
                };
            }

            public static TypeSpec Map(ITypeSymbol type, TypeSpec keyType, TypeSpec valueType)
            {
                return new TypeSpec
                {
                    Kind = PBTypeKind.Map,
                    CSharpType = GetTypeName(type),
                    KeyType = keyType,
                    ValueType = valueType,
                };
            }

            private static string GetTypeName(ITypeSymbol type)
            {
                return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
        }

        private enum PBTypeKind
        {
            Scalar,
            Enum,
            Message,
            Repeated,
            Map,
        }

        private enum PBFieldType
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

    }
}
