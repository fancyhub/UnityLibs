/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FH.Protobuf.Ed
{
    internal sealed class PBProtoParseException : Exception
    {
        public PBProtoParseException(string message) : base(message)
        {
        }
    }

    internal static class PBProtoParser
    {
        public static PBProtoFile ParseFile(string filePath)
        {
            string text = File.ReadAllText(filePath, Encoding.UTF8);
            PBProtoTokenizer tokenizer = new PBProtoTokenizer(text, filePath);
            PBProtoFile file = new PBProtoFile
            {
                FilePath = filePath,
            };

            while (!tokenizer.IsEnd)
            {
                if (tokenizer.TryConsume(";"))
                {
                    tokenizer.DropPendingComments();
                    continue;
                }

                string token = tokenizer.Read();
                switch (token)
                {
                    case "syntax":
                    case "import":
                        tokenizer.DropPendingComments();
                        SkipStatement(tokenizer);
                        break;

                    case "service":
                        tokenizer.DropPendingComments();
                        SkipStatementOrBlock(tokenizer);
                        break;

                    case "package":
                        tokenizer.DropPendingComments();
                        file.PackageName = ReadQualifiedName(tokenizer, false);
                        tokenizer.Expect(";");
                        tokenizer.DropTrailingComments(tokenizer.LastTokenLine);
                        break;

                    case "option":
                        tokenizer.DropPendingComments();
                        ParseOption(tokenizer, file);
                        break;

                    case "message":
                        file.Messages.Add(ParseMessage(tokenizer));
                        break;

                    case "enum":
                        file.Enums.Add(ParseEnum(tokenizer));
                        break;

                    default:
                        tokenizer.DropPendingComments();
                        SkipStatementOrBlock(tokenizer);
                        break;
                }
            }

            return file;
        }

        private static void ParseOption(PBProtoTokenizer tokenizer, PBProtoFile file)
        {
            string optionName = ReadOptionName(tokenizer);
            if (tokenizer.TryConsume("="))
            {
                if (optionName == "csharp_namespace" && tokenizer.PeekKind(PBProtoTokenKind.String))
                    file.CSharpNamespace = tokenizer.ReadString();
                SkipStatement(tokenizer);
                return;
            }

            SkipStatement(tokenizer);
        }

        private static PBProtoMessage ParseMessage(PBProtoTokenizer tokenizer)
        {
            List<string> comments = tokenizer.TakePendingComments();
            PBProtoMessage msg = new PBProtoMessage
            {
                Name = tokenizer.ReadIdentifier(),
            };
            AddComments(msg.Comments, comments);
            tokenizer.Expect("{");

            while (!tokenizer.TryConsume("}"))
            {
                if (tokenizer.IsEnd)
                    throw tokenizer.Error("Unexpected end of message");

                if (tokenizer.TryConsume(";"))
                {
                    tokenizer.DropPendingComments();
                    continue;
                }

                string token = tokenizer.Read();
                switch (token)
                {
                    case "message":
                        msg.Messages.Add(ParseMessage(tokenizer));
                        break;

                    case "enum":
                        msg.Enums.Add(ParseEnum(tokenizer));
                        break;

                    case "oneof":
                        tokenizer.DropPendingComments();
                        ParseOneof(tokenizer, msg);
                        break;

                    case "option":
                    case "reserved":
                    case "extensions":
                        tokenizer.DropPendingComments();
                        SkipStatementOrBlock(tokenizer);
                        break;

                    default:
                        msg.Fields.Add(ParseField(tokenizer, token));
                        break;
                }
            }

            tokenizer.DropPendingComments();
            return msg;
        }

        private static void ParseOneof(PBProtoTokenizer tokenizer, PBProtoMessage msg)
        {
            tokenizer.ReadIdentifier();
            tokenizer.Expect("{");
            while (!tokenizer.TryConsume("}"))
            {
                if (tokenizer.IsEnd)
                    throw tokenizer.Error("Unexpected end of oneof");

                if (tokenizer.TryConsume(";"))
                {
                    tokenizer.DropPendingComments();
                    continue;
                }

                string token = tokenizer.Read();
                if (token == "option")
                {
                    tokenizer.DropPendingComments();
                    SkipStatementOrBlock(tokenizer);
                    continue;
                }

                msg.Fields.Add(ParseField(tokenizer, token));
            }
        }

        private static PBProtoEnum ParseEnum(PBProtoTokenizer tokenizer)
        {
            List<string> comments = tokenizer.TakePendingComments();
            PBProtoEnum ret = new PBProtoEnum
            {
                Name = tokenizer.ReadIdentifier(),
            };
            AddComments(ret.Comments, comments);
            tokenizer.Expect("{");

            while (!tokenizer.TryConsume("}"))
            {
                if (tokenizer.IsEnd)
                    throw tokenizer.Error("Unexpected end of enum");

                if (tokenizer.TryConsume(";"))
                {
                    tokenizer.DropPendingComments();
                    continue;
                }

                string token = tokenizer.Read();
                if (token == "option" || token == "reserved")
                {
                    tokenizer.DropPendingComments();
                    SkipStatementOrBlock(tokenizer);
                    continue;
                }

                List<string> valueComments = tokenizer.TakePendingComments();
                PBProtoEnumValue value = new PBProtoEnumValue
                {
                    Name = token,
                };
                AddComments(value.Comments, valueComments);
                tokenizer.Expect("=");
                value.Value = ReadSignedInt(tokenizer);
                if (tokenizer.TryConsume("["))
                    SkipUntilMatching(tokenizer, "[", "]");
                tokenizer.Expect(";");
                tokenizer.TakeTrailingComments(tokenizer.LastTokenLine, value.Comments);
                tokenizer.DropPendingComments();
                ret.Values.Add(value);
            }

            tokenizer.DropPendingComments();
            return ret;
        }

        private static PBProtoField ParseField(PBProtoTokenizer tokenizer, string firstToken)
        {
            List<string> comments = tokenizer.TakePendingComments();
            PBProtoLabel label = PBProtoLabel.None;
            string typeToken = firstToken;
            switch (firstToken)
            {
                case "optional":
                    label = PBProtoLabel.Optional;
                    typeToken = tokenizer.Read();
                    break;

                case "repeated":
                    label = PBProtoLabel.Repeated;
                    typeToken = tokenizer.Read();
                    break;

                case "required":
                    label = PBProtoLabel.Required;
                    typeToken = tokenizer.Read();
                    break;
            }

            PBProtoField field = new PBProtoField
            {
                Label = label,
            };
            AddComments(field.Comments, comments);

            if (typeToken == "map")
            {
                field.IsMap = true;
                tokenizer.Expect("<");
                field.MapKeyType = ReadQualifiedName(tokenizer, true);
                tokenizer.Expect(",");
                field.MapValueType = ReadQualifiedName(tokenizer, true);
                tokenizer.Expect(">");
                field.Name = tokenizer.ReadIdentifier();
            }
            else
            {
                field.TypeName = ReadQualifiedNameStartingWith(tokenizer, typeToken);
                field.Name = tokenizer.ReadIdentifier();
            }

            tokenizer.Expect("=");
            field.Number = ReadSignedInt(tokenizer);

            if (tokenizer.TryConsume("["))
                SkipUntilMatching(tokenizer, "[", "]");

            tokenizer.Expect(";");
            tokenizer.TakeTrailingComments(tokenizer.LastTokenLine, field.Comments);
            tokenizer.DropPendingComments();
            return field;
        }

        private static string ReadOptionName(PBProtoTokenizer tokenizer)
        {
            StringBuilder sb = new StringBuilder();
            while (!tokenizer.IsEnd && !tokenizer.PeekIs("=") && !tokenizer.PeekIs(";"))
                sb.Append(tokenizer.Read());
            return sb.ToString();
        }

        private static string ReadQualifiedName(PBProtoTokenizer tokenizer, bool allowLeadingDot)
        {
            string first = tokenizer.Read();
            return ReadQualifiedNameStartingWith(tokenizer, first, allowLeadingDot);
        }

        private static string ReadQualifiedNameStartingWith(PBProtoTokenizer tokenizer, string firstToken, bool allowLeadingDot = true)
        {
            StringBuilder sb = new StringBuilder();
            string token = firstToken;
            if (token == ".")
            {
                if (!allowLeadingDot)
                    throw tokenizer.Error("Unexpected leading dot");
                sb.Append(".");
                token = tokenizer.ReadIdentifier();
            }
            else if (!PBProtoTokenizer.IsIdentifierToken(token))
            {
                throw tokenizer.Error("Expected identifier");
            }

            sb.Append(token);
            while (tokenizer.TryConsume("."))
            {
                sb.Append(".");
                sb.Append(tokenizer.ReadIdentifier());
            }

            return sb.ToString();
        }

        private static int ReadSignedInt(PBProtoTokenizer tokenizer)
        {
            bool negative = tokenizer.TryConsume("-");
            string token = tokenizer.Read();
            int value;
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                value = int.Parse(token.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            else
                value = int.Parse(token, CultureInfo.InvariantCulture);
            return negative ? -value : value;
        }

        private static void AddComments(List<string> target, List<string> comments)
        {
            if (comments == null || comments.Count == 0)
                return;

            target.AddRange(comments);
        }

        private static void SkipStatement(PBProtoTokenizer tokenizer)
        {
            while (!tokenizer.IsEnd)
            {
                if (tokenizer.TryConsume(";"))
                {
                    tokenizer.DropTrailingComments(tokenizer.LastTokenLine);
                    tokenizer.DropPendingComments();
                    return;
                }
                tokenizer.Read();
            }
        }

        private static void SkipStatementOrBlock(PBProtoTokenizer tokenizer)
        {
            while (!tokenizer.IsEnd)
            {
                if (tokenizer.TryConsume(";"))
                {
                    tokenizer.DropTrailingComments(tokenizer.LastTokenLine);
                    tokenizer.DropPendingComments();
                    return;
                }

                if (tokenizer.TryConsume("{"))
                {
                    SkipUntilMatching(tokenizer, "{", "}");
                    tokenizer.DropTrailingComments(tokenizer.LastTokenLine);
                    tokenizer.DropPendingComments();
                    return;
                }

                tokenizer.Read();
            }
        }

        private static void SkipUntilMatching(PBProtoTokenizer tokenizer, string open, string close)
        {
            int depth = 1;
            while (!tokenizer.IsEnd)
            {
                string token = tokenizer.Read();
                if (token == open)
                    depth++;
                else if (token == close)
                {
                    depth--;
                    if (depth == 0)
                    {
                        tokenizer.DropPendingComments();
                        return;
                    }
                }
            }

            throw tokenizer.Error("Unexpected end while skipping block");
        }
    }

    internal enum PBProtoTokenKind
    {
        Identifier,
        Number,
        String,
        Symbol,
        Comment,
    }

    internal sealed class PBProtoToken
    {
        public PBProtoTokenKind Kind;
        public string Value;
        public int Line;
        public int Column;
    }

    internal sealed class PBProtoTokenizer
    {
        private readonly string _filePath;
        private readonly List<PBProtoToken> _tokens = new List<PBProtoToken>();
        private readonly List<string> _pendingComments = new List<string>();
        private int _index;

        public PBProtoTokenizer(string text, string filePath)
        {
            _filePath = filePath;
            Tokenize(text);
        }

        public int LastTokenLine { get; private set; }

        public bool IsEnd
        {
            get
            {
                SkipCommentTokens();
                return _index >= _tokens.Count;
            }
        }

        public bool PeekIs(string value)
        {
            SkipCommentTokens();
            return !IsEnd && _tokens[_index].Value == value;
        }

        public bool PeekKind(PBProtoTokenKind kind)
        {
            SkipCommentTokens();
            return !IsEnd && _tokens[_index].Kind == kind;
        }

        public bool TryConsume(string value)
        {
            SkipCommentTokens();
            if (!PeekIs(value))
                return false;
            LastTokenLine = _tokens[_index].Line;
            _index++;
            return true;
        }

        public void Expect(string value)
        {
            string token = Read();
            if (token != value)
                throw Error("Expected '" + value + "', got '" + token + "'");
        }

        public string Read()
        {
            SkipCommentTokens();
            if (IsEnd)
                throw Error("Unexpected end of file");
            PBProtoToken token = _tokens[_index++];
            LastTokenLine = token.Line;
            return token.Value;
        }

        public string ReadIdentifier()
        {
            if (IsEnd || !IsIdentifierToken(_tokens[_index].Value))
                throw Error("Expected identifier");
            return Read();
        }

        public string ReadString()
        {
            SkipCommentTokens();
            if (IsEnd || _tokens[_index].Kind != PBProtoTokenKind.String)
                throw Error("Expected string");
            return Read();
        }

        public List<string> TakePendingComments()
        {
            List<string> ret = new List<string>(_pendingComments);
            _pendingComments.Clear();
            return ret;
        }

        public void DropPendingComments()
        {
            _pendingComments.Clear();
        }

        public void TakeTrailingComments(int line, List<string> target)
        {
            while (_index < _tokens.Count && _tokens[_index].Kind == PBProtoTokenKind.Comment && _tokens[_index].Line == line)
            {
                AddCommentLines(target, _tokens[_index].Value);
                _index++;
            }
        }

        public void DropTrailingComments(int line)
        {
            while (_index < _tokens.Count && _tokens[_index].Kind == PBProtoTokenKind.Comment && _tokens[_index].Line == line)
                _index++;
        }

        public PBProtoParseException Error(string msg)
        {
            if (IsEnd)
                return new PBProtoParseException(_filePath + ": " + msg);

            PBProtoToken token = _tokens[Math.Min(_index, _tokens.Count - 1)];
            return new PBProtoParseException(_filePath + "(" + token.Line + "," + token.Column + "): " + msg);
        }

        public static bool IsIdentifierToken(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            char first = value[0];
            if (!(char.IsLetter(first) || first == '_'))
                return false;
            for (int i = 1; i < value.Length; i++)
            {
                char c = value[i];
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    return false;
            }
            return true;
        }

        private void SkipCommentTokens()
        {
            while (_index < _tokens.Count && _tokens[_index].Kind == PBProtoTokenKind.Comment)
            {
                AddCommentLines(_pendingComments, _tokens[_index].Value);
                _index++;
            }
        }

        private static void AddCommentLines(List<string> target, string value)
        {
            if (value == null)
                return;

            string[] lines = value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            foreach (string line in lines)
                target.Add(line);
        }

        private void Tokenize(string text)
        {
            int line = 1;
            int col = 1;
            int i = 0;
            while (i < text.Length)
            {
                char c = text[i];
                if (char.IsWhiteSpace(c))
                {
                    Advance(text, ref i, ref line, ref col);
                    continue;
                }

                if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
                {
                    int commentLine = line;
                    int commentCol = col;
                    Advance(text, ref i, ref line, ref col);
                    Advance(text, ref i, ref line, ref col);
                    int start = i;
                    while (i < text.Length && text[i] != '\n')
                        Advance(text, ref i, ref line, ref col);
                    _tokens.Add(new PBProtoToken
                    {
                        Kind = PBProtoTokenKind.Comment,
                        Value = text.Substring(start, i - start).TrimEnd(),
                        Line = commentLine,
                        Column = commentCol,
                    });
                    continue;
                }

                if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    int commentLine = line;
                    int commentCol = col;
                    Advance(text, ref i, ref line, ref col);
                    Advance(text, ref i, ref line, ref col);
                    int start = i;
                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                        Advance(text, ref i, ref line, ref col);
                    if (i + 1 >= text.Length)
                        throw new PBProtoParseException(_filePath + ": Unterminated block comment");
                    string comment = NormalizeBlockComment(text.Substring(start, i - start));
                    Advance(text, ref i, ref line, ref col);
                    Advance(text, ref i, ref line, ref col);
                    _tokens.Add(new PBProtoToken
                    {
                        Kind = PBProtoTokenKind.Comment,
                        Value = comment,
                        Line = commentLine,
                        Column = commentCol,
                    });
                    continue;
                }

                int tokenLine = line;
                int tokenCol = col;
                if (c == '"' || c == '\'')
                {
                    _tokens.Add(new PBProtoToken
                    {
                        Kind = PBProtoTokenKind.String,
                        Value = ReadQuotedString(text, ref i, ref line, ref col, c),
                        Line = tokenLine,
                        Column = tokenCol,
                    });
                    continue;
                }

                if (IsSymbol(c))
                {
                    _tokens.Add(new PBProtoToken
                    {
                        Kind = PBProtoTokenKind.Symbol,
                        Value = c.ToString(),
                        Line = tokenLine,
                        Column = tokenCol,
                    });
                    Advance(text, ref i, ref line, ref col);
                    continue;
                }

                if (char.IsDigit(c))
                {
                    int start = i;
                    while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
                        Advance(text, ref i, ref line, ref col);
                    _tokens.Add(new PBProtoToken
                    {
                        Kind = PBProtoTokenKind.Number,
                        Value = text.Substring(start, i - start).Replace("_", string.Empty),
                        Line = tokenLine,
                        Column = tokenCol,
                    });
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    int start = i;
                    while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
                        Advance(text, ref i, ref line, ref col);
                    _tokens.Add(new PBProtoToken
                    {
                        Kind = PBProtoTokenKind.Identifier,
                        Value = text.Substring(start, i - start),
                        Line = tokenLine,
                        Column = tokenCol,
                    });
                    continue;
                }

                throw new PBProtoParseException(_filePath + "(" + line + "," + col + "): Invalid char '" + c + "'");
            }
        }

        private static string NormalizeBlockComment(string value)
        {
            string[] lines = value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int start = 0;
            int end = lines.Length - 1;

            while (start <= end && string.IsNullOrWhiteSpace(lines[start]))
                start++;
            while (end >= start && string.IsNullOrWhiteSpace(lines[end]))
                end--;

            StringBuilder sb = new StringBuilder();
            for (int i = start; i <= end; i++)
            {
                string line = lines[i].TrimEnd();
                string trimmed = line.TrimStart();
                if (trimmed.StartsWith("*", StringComparison.Ordinal))
                {
                    trimmed = trimmed.Substring(1);
                    if (trimmed.StartsWith(" ", StringComparison.Ordinal))
                        trimmed = trimmed.Substring(1);
                    line = trimmed;
                }
                else
                {
                    line = trimmed;
                }

                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(line);
            }

            return sb.ToString();
        }

        private static bool IsSymbol(char c)
        {
            switch (c)
            {
                case '{':
                case '}':
                case '[':
                case ']':
                case '(':
                case ')':
                case ';':
                case '=':
                case ',':
                case '<':
                case '>':
                case '.':
                case '-':
                case ':':
                    return true;
                default:
                    return false;
            }
        }

        private static string ReadQuotedString(string text, ref int i, ref int line, ref int col, char quote)
        {
            StringBuilder sb = new StringBuilder();
            Advance(text, ref i, ref line, ref col);
            while (i < text.Length)
            {
                char c = text[i];
                if (c == quote)
                {
                    Advance(text, ref i, ref line, ref col);
                    return sb.ToString();
                }

                if (c == '\\')
                {
                    Advance(text, ref i, ref line, ref col);
                    if (i >= text.Length)
                        break;
                    c = text[i];
                    switch (c)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '\\': sb.Append('\\'); break;
                        case '"': sb.Append('"'); break;
                        default: sb.Append(c); break;
                    }
                    Advance(text, ref i, ref line, ref col);
                    continue;
                }

                sb.Append(c);
                Advance(text, ref i, ref line, ref col);
            }

            throw new PBProtoParseException("Unterminated string");
        }

        private static void Advance(string text, ref int i, ref int line, ref int col)
        {
            if (text[i] == '\n')
            {
                line++;
                col = 1;
            }
            else
            {
                col++;
            }
            i++;
        }
    }
}
