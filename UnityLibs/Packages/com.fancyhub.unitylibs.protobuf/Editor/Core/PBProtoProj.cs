/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace FH.Protobuf.Ed
{
    public class PBProtoProj
    {
        public readonly string RootDir;
        private List<PBProtoFile> _Files;
        private PBProtoProj(string rootDir, List<PBProtoFile> files)
        {
            RootDir = rootDir;
            _Files = files;
        }

        public IReadOnlyList<PBProtoFile> Files => _Files;

        public static PBProtoProj LoadDirectory(string protoDir)
        {
            protoDir = NormalizeDir(protoDir, nameof(protoDir));


            string[] protoFiles = Directory.GetFiles(protoDir, "*.proto", SearchOption.AllDirectories);
            Array.Sort(protoFiles, StringComparer.OrdinalIgnoreCase);
            return LoadFiles(protoDir, protoFiles);
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

        public static PBProtoProj LoadFiles(string rootDir, IReadOnlyList<string> protoFiles)
        {
            if (protoFiles == null)
                throw new ArgumentNullException(nameof(protoFiles));
            rootDir = NormalizeDir(rootDir, nameof(rootDir));
            List<PBProtoFile> files = new List<PBProtoFile>(protoFiles.Count);
            for (int i = 0; i < protoFiles.Count; i++)
                files.Add(PBProtoParser.ParseFile(protoFiles[i]));

            PBProtoProj ret = new PBProtoProj(rootDir, files);
            ret.ValidateTypes();
            return ret;
        }

        public void ValidateTypes()
        {
            if (_Files == null)
                throw new ArgumentNullException(nameof(_Files));

            PBProtoTypeRegistry registry = new PBProtoTypeRegistry(_Files);
            List<string> errors = new List<string>();

            foreach (PBProtoFile file in _Files)
            {
                foreach (PBProtoMessage msg in file.Messages)
                    ValidateMessageTypes(file, registry, msg, msg.Name, errors);
            }

            if (errors.Count > 0)
                throw new PBProtoParseException("Protobuf type validation failed:\n" + string.Join("\n", errors.ToArray()));
        }

        private static void ValidateMessageTypes(
            PBProtoFile file,
            PBProtoTypeRegistry registry,
            PBProtoMessage msg,
            string messagePath,
            List<string> errors)
        {
            foreach (PBProtoField field in msg.Fields)
            {
                if (field.IsMap)
                {
                    if (!PBProtoScalarTypes.IsMapKey(field.MapKeyType))
                        errors.Add(FormatFieldLocation(file, messagePath, field.Name)
                            + " has invalid map key type '" + field.MapKeyType + "'"
                            + "; map keys must use " + PBProtoScalarTypes.MapKeyTypeDescription);

                    ValidateKnownFieldType(file, registry, msg, messagePath, field, "map value type", field.MapValueType, errors);
                    continue;
                }

                ValidateKnownFieldType(file, registry, msg, messagePath, field, "type", field.TypeName, errors);
            }

            foreach (PBProtoMessage child in msg.Messages)
                ValidateMessageTypes(file, registry, child, messagePath + "." + child.Name, errors);
        }

        private static void ValidateKnownFieldType(
            PBProtoFile file,
            PBProtoTypeRegistry registry,
            PBProtoMessage scopeMsg,
            string messagePath,
            PBProtoField field,
            string typeRole,
            string protoType,
            List<string> errors)
        {
            if (IsKnownFieldType(file, registry, scopeMsg, protoType))
                return;

            errors.Add(FormatFieldTypeError(file, messagePath, field.Name, typeRole, protoType));
        }

        private static bool IsKnownFieldType(PBProtoFile file, PBProtoTypeRegistry registry, PBProtoMessage scopeMsg, string protoType)
        {
            if (string.IsNullOrEmpty(protoType))
                return false;
            if (PBProtoScalarTypes.IsScalar(protoType))
                return true;
            return registry.HasType(file, scopeMsg, protoType);
        }

        private static string FormatFieldTypeError(PBProtoFile file, string messagePath, string fieldName, string typeRole, string protoType)
        {
            return FormatFieldLocation(file, messagePath, fieldName)
                + " has unknown " + typeRole
                + " '" + protoType + "'";
        }

        private static string FormatFieldLocation(PBProtoFile file, string messagePath, string fieldName)
        {
            return FormatProtoPath(file.FilePath)
                + ": message '" + messagePath
                + "' field '" + fieldName + "'";
        }

        private static string FormatProtoPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "<unknown>";
            return path.Replace('\\', '/');
        }
    }

    internal sealed class PBProtoTypeRegistry
    {
        private readonly HashSet<string> _types = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _enums = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<PBProtoMessage, string> _messageProtoNames = new Dictionary<PBProtoMessage, string>();

        public PBProtoTypeRegistry(IReadOnlyList<PBProtoFile> files)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            foreach (PBProtoFile file in files)
            {
                foreach (PBProtoEnum pbEnum in file.Enums)
                    AddType(file, null, pbEnum.Name, true);
                foreach (PBProtoMessage msg in file.Messages)
                    Collect(file, null, msg);
            }
        }

        public bool HasType(PBProtoFile file, PBProtoMessage scopeMsg, string typeName)
        {
            foreach (string key in GetTypeLookupKeys(file, scopeMsg, typeName))
            {
                if (_types.Contains(key))
                    return true;
            }

            return false;
        }

        public bool IsEnum(PBProtoFile file, PBProtoMessage scopeMsg, string typeName)
        {
            foreach (string key in GetTypeLookupKeys(file, scopeMsg, typeName))
            {
                if (_enums.Contains(key))
                    return true;
            }

            return false;
        }

        public List<string> GetTypeLookupKeys(PBProtoFile file, PBProtoMessage scopeMsg, string typeName)
        {
            List<string> ret = new List<string>();
            HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(typeName))
                return ret;

            string raw = typeName.TrimStart('.');

            if (typeName.StartsWith(".", StringComparison.Ordinal))
            {
                AddTypeLookupKey(ret, added, raw);
                return ret;
            }

            if (!string.IsNullOrEmpty(file.PackageName) && raw.StartsWith(file.PackageName + ".", StringComparison.Ordinal))
            {
                AddTypeLookupKey(ret, added, raw);
                return ret;
            }

            if (scopeMsg != null && _messageProtoNames.TryGetValue(scopeMsg, out string scope))
            {
                while (!string.IsNullOrEmpty(scope))
                {
                    if (!string.IsNullOrEmpty(file.PackageName))
                        AddTypeLookupKey(ret, added, file.PackageName + "." + scope + "." + raw);
                    AddTypeLookupKey(ret, added, scope + "." + raw);

                    int index = scope.LastIndexOf('.');
                    scope = index < 0 ? string.Empty : scope.Substring(0, index);
                }
            }

            if (!string.IsNullOrEmpty(file.PackageName))
                AddTypeLookupKey(ret, added, file.PackageName + "." + raw);
            AddTypeLookupKey(ret, added, raw);
            return ret;
        }

        private void Collect(PBProtoFile file, string parentProtoName, PBProtoMessage msg)
        {
            string msgProtoName = string.IsNullOrEmpty(parentProtoName) ? msg.Name : parentProtoName + "." + msg.Name;
            _messageProtoNames[msg] = msgProtoName;
            AddType(file, parentProtoName, msg.Name, false);

            foreach (PBProtoEnum pbEnum in msg.Enums)
                AddType(file, msgProtoName, pbEnum.Name, true);
            foreach (PBProtoMessage child in msg.Messages)
                Collect(file, msgProtoName, child);
        }

        private void AddType(PBProtoFile file, string parentProtoName, string typeName, bool isEnum)
        {
            string protoName = string.IsNullOrEmpty(parentProtoName) ? typeName : parentProtoName + "." + typeName;

            AddTypeMap(protoName, isEnum);
            if (!string.IsNullOrEmpty(file.PackageName))
                AddTypeMap(file.PackageName + "." + protoName, isEnum);

            if (string.IsNullOrEmpty(parentProtoName))
                AddTypeMap(typeName, isEnum);
        }

        private void AddTypeMap(string protoName, bool isEnum)
        {
            if (!_types.Contains(protoName))
                _types.Add(protoName);

            if (isEnum)
                _enums.Add(protoName);
        }

        private static void AddTypeLookupKey(List<string> keys, HashSet<string> added, string key)
        {
            if (string.IsNullOrEmpty(key) || added.Contains(key))
                return;

            added.Add(key);
            keys.Add(key);
        }
    }

    internal static class PBProtoScalarTypes
    {
        public const string MapKeyTypeDescription = "int32, int64, uint32, uint64, sint32, sint64, fixed32, fixed64, sfixed32, sfixed64, bool, or string";

        private static readonly HashSet<string> Scalars = new HashSet<string>(StringComparer.Ordinal)
        {
            "double", "float", "int32", "int64", "uint32", "uint64", "sint32", "sint64",
            "fixed32", "fixed64", "sfixed32", "sfixed64", "bool", "string", "bytes",
        };

        private static readonly HashSet<string> MapKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "int32", "int64", "uint32", "uint64", "sint32", "sint64",
            "fixed32", "fixed64", "sfixed32", "sfixed64", "bool", "string",
        };

        public static bool IsScalar(string typeName)
        {
            return !string.IsNullOrEmpty(typeName) && Scalars.Contains(typeName);
        }

        public static bool IsMapKey(string typeName)
        {
            return !string.IsNullOrEmpty(typeName) && MapKeys.Contains(typeName);
        }
    }
}
