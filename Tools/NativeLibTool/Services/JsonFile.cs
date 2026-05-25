using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NativeLibTool.Services
{
    internal static class JsonFile
    {
        public static T Read<T>(string path) where T : class
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using (var stream = File.OpenRead(path))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return serializer.ReadObject(stream) as T;
            }
        }

        public static void Write<T>(string path, T value) where T : class
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, ToPrettyJson(value), new UTF8Encoding(false));
        }

        private static string ToPrettyJson<T>(T value) where T : class
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, value);
                var compact = Encoding.UTF8.GetString(stream.ToArray());
                return PrettyPrint(compact);
            }
        }

        private static string PrettyPrint(string json)
        {
            var builder = new StringBuilder();
            var indent = 0;
            var inString = false;
            var escaping = false;

            foreach (var c in json)
            {
                if (escaping)
                {
                    builder.Append(c);
                    escaping = false;
                    continue;
                }

                if (c == '\\' && inString)
                {
                    builder.Append(c);
                    escaping = true;
                    continue;
                }

                if (c == '"')
                {
                    builder.Append(c);
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    builder.Append(c);
                    continue;
                }

                switch (c)
                {
                    case '{':
                    case '[':
                        builder.Append(c);
                        builder.AppendLine();
                        indent++;
                        AppendIndent(builder, indent);
                        break;
                    case '}':
                    case ']':
                        builder.AppendLine();
                        indent--;
                        AppendIndent(builder, indent);
                        builder.Append(c);
                        break;
                    case ',':
                        builder.Append(c);
                        builder.AppendLine();
                        AppendIndent(builder, indent);
                        break;
                    case ':':
                        builder.Append(": ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(c))
                        {
                            builder.Append(c);
                        }
                        break;
                }
            }

            builder.AppendLine();
            return builder.ToString();
        }

        private static void AppendIndent(StringBuilder builder, int indent)
        {
            builder.Append(new string(' ', Math.Max(0, indent) * 2));
        }
    }
}
