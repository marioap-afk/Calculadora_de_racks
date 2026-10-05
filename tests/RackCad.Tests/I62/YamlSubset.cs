#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>A rejection of the fail-closed YAML subset reader: the line of the offending construct and the reason.</summary>
    public sealed class YamlSubsetException : Exception
    {
        public YamlSubsetException(int line, string reason) : base("line " + line + ": " + reason)
        {
            Line = line;
            Reason = reason;
        }

        public int Line { get; }

        public string Reason { get; }
    }

    /// <summary>
    /// An ordered mapping of the subset. Order is kept because the canonical writer must reproduce it; keys are unique because the reader rejects a
    /// duplicate key instead of letting the last one win silently.
    /// </summary>
    public sealed class YamlMap : IEnumerable<KeyValuePair<string, object?>>
    {
        private readonly List<KeyValuePair<string, object?>> items = new List<KeyValuePair<string, object?>>();
        private readonly Dictionary<string, int> index = new Dictionary<string, int>(StringComparer.Ordinal);

        public int Count => items.Count;

        public IEnumerable<string> Keys => items.Select(i => i.Key);

        public object? this[string key]
        {
            get => index.TryGetValue(key, out var i) ? items[i].Value : throw new KeyNotFoundException(key);
            set
            {
                if (index.TryGetValue(key, out var i))
                {
                    items[i] = new KeyValuePair<string, object?>(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public void Add(string key, object? value)
        {
            if (index.ContainsKey(key))
            {
                throw new ArgumentException("duplicate key: " + key, nameof(key));
            }

            index[key] = items.Count;
            items.Add(new KeyValuePair<string, object?>(key, value));
        }

        public bool ContainsKey(string key) => index.ContainsKey(key);

        public bool TryGetValue(string key, out object? value)
        {
            if (index.TryGetValue(key, out var i))
            {
                value = items[i].Value;
                return true;
            }

            value = null;
            return false;
        }

        public bool Remove(string key)
        {
            if (!index.TryGetValue(key, out var i))
            {
                return false;
            }

            items.RemoveAt(i);
            index.Clear();
            for (var k = 0; k < items.Count; k++)
            {
                index[items[k].Key] = k;
            }

            return true;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>How much of a file the reader consumes.</summary>
    public enum YamlReadMode
    {
        /// <summary>The whole file must be in the subset (rackcad-automation-state/v2).</summary>
        Strict,

        /// <summary>
        /// rackcad-automation-state/v1 for the classifier of Proposal V14 Anexo E.2: only <c>schema</c> and three direct children of
        /// <c>automation_state</c> (initiative, branch, claim_id) are parsed strictly; everything else is skipped and never consumed.
        /// </summary>
        Header,
    }

    /// <summary>In-memory mutations of the reader, used only by the guard tests to show that each rejection is load-bearing.</summary>
    [Flags]
    public enum YamlReaderMutation
    {
        None = 0,
        AcceptDuplicateKeys = 1,
        AcceptPlainColon = 2,
        AcceptBlockScalars = 4,
        AcceptMultilinePlain = 8,
    }

    /// <summary>
    /// The minimal FAIL-CLOSED reader and the canonical writer of the exact YAML subset of <c>rackcad-automation-state/v2</c> (I-62 F4; port of the
    /// measured preparation prototype f4/yaml-subset, so no package dependency is needed). It is NOT a general YAML parser: anything outside the subset
    /// is rejected with its line instead of being normalized, because a silently re-interpreted state would let a durable point validate against data
    /// that nobody wrote.
    /// Subset: block mappings with snake_case keys; block sequences indented under their key, whose items are scalars or block mappings; spaces only,
    /// a consistent step per block; single-line scalars (plain, double-quoted with \\ \" \/ \n \t \uXXXX, or single-quoted); typed plain scalars null,
    /// true, false and canonical integers; empty collections only as [] and {}; full-line comments and blank lines.
    /// </summary>
    public static class YamlSubset
    {
        private static readonly Regex KeyLine = new Regex(@"^([A-Za-z_][A-Za-z0-9_]*):(?: (.*))?$", RegexOptions.CultureInvariant);
        private static readonly Regex Integer = new Regex(@"^(0|-?[1-9][0-9]*)$", RegexOptions.CultureInvariant);
        private static readonly Regex LeadingZero = new Regex(@"^-?0[0-9]+$", RegexOptions.CultureInvariant);
        private static readonly Regex FloatLike = new Regex(@"^[-+]?(\.[0-9]+|[0-9]+\.[0-9]*)([eE][-+]?[0-9]+)?$", RegexOptions.CultureInvariant);
        private static readonly Regex Hex4 = new Regex(@"^[0-9A-Fa-f]{4}$", RegexOptions.CultureInvariant);
        private const string Indicators = "[]{}&*!|>'\"%@`#,?";
        private static readonly HashSet<string> Ambiguous = new HashSet<string>(StringComparer.Ordinal)
        {
            "~", "Null", "NULL", "True", "TRUE", "False", "FALSE", "yes", "no", "on", "off",
        };

        private static readonly string[] HeaderConsumed = { "initiative", "branch", "claim_id" };

        public static YamlMap Read(string text, YamlReadMode mode = YamlReadMode.Strict) => Read(text, mode, YamlReaderMutation.None);

        public static YamlMap Read(string text, YamlReadMode mode, YamlReaderMutation mutation)
        {
            var reader = new Reader(mutation);
            return reader.Load(text, mode);
        }

        /// <summary>
        /// The canonical serialization: two-space steps, sequences indented under their key, mapping items continued at the item indentation, and a
        /// string quoted whenever the reader would otherwise give it another type or reject it. Write(Read(x)) is a fixed point for canonical input.
        /// </summary>
        public static string Write(YamlMap map)
        {
            var lines = new List<string>();
            WriteMap(map, 0, lines);
            return string.Join("\n", lines) + "\n";
        }

        public static bool DeepEquals(object? a, object? b)
        {
            if (a is null || b is null)
            {
                return a is null && b is null;
            }

            if (a is YamlMap ma && b is YamlMap mb)
            {
                return ma.Count == mb.Count && ma.All(kv => mb.TryGetValue(kv.Key, out var v) && DeepEquals(kv.Value, v));
            }

            if (a is List<object?> la && b is List<object?> lb)
            {
                return la.Count == lb.Count && la.Zip(lb, DeepEquals).All(x => x);
            }

            return a.GetType() == b.GetType() && a.Equals(b);
        }

        public static object? DeepClone(object? v)
        {
            switch (v)
            {
                case YamlMap m:
                    var c = new YamlMap();
                    foreach (var kv in m)
                    {
                        c.Add(kv.Key, DeepClone(kv.Value));
                    }

                    return c;
                case List<object?> l:
                    return l.Select(DeepClone).ToList();
                default:
                    return v;
            }
        }

        /// <summary>The JSON model of a subset value (mapping → object, sequence → array, integer, bool, string or null), for schema validation.</summary>
        public static System.Text.Json.Nodes.JsonNode? ToJson(object? v)
        {
            switch (v)
            {
                case null:
                    return null;
                case YamlMap m:
                    var o = new System.Text.Json.Nodes.JsonObject();
                    foreach (var kv in m)
                    {
                        o[kv.Key] = ToJson(kv.Value);
                    }

                    return o;
                case List<object?> l:
                    return new System.Text.Json.Nodes.JsonArray(l.Select(ToJson).ToArray());
                case long n:
                    return System.Text.Json.Nodes.JsonValue.Create(n);
                case bool b:
                    return System.Text.Json.Nodes.JsonValue.Create(b);
                case string s:
                    return System.Text.Json.Nodes.JsonValue.Create(s);
                default:
                    throw new ArgumentException("not a value of the YAML subset: " + v.GetType().Name);
            }
        }

        private static void WriteMap(YamlMap map, int indent, List<string> lines)
        {
            var pad = new string(' ', indent);
            foreach (var kv in map)
            {
                switch (kv.Value)
                {
                    case YamlMap m when m.Count > 0:
                        lines.Add(pad + kv.Key + ":");
                        WriteMap(m, indent + 2, lines);
                        break;
                    case List<object?> l when l.Count > 0:
                        lines.Add(pad + kv.Key + ":");
                        foreach (var item in l)
                        {
                            if (item is YamlMap im && im.Count > 0)
                            {
                                var sub = new List<string>();
                                WriteMap(im, indent + 4, sub);
                                lines.Add(pad + "  - " + sub[0].TrimStart(' '));
                                lines.AddRange(sub.Skip(1));
                            }
                            else if (item is List<object?> il && il.Count > 0)
                            {
                                throw new InvalidOperationException("nested sequences are outside the subset");
                            }
                            else
                            {
                                lines.Add(pad + "  - " + Scalar(item));
                            }
                        }

                        break;
                    default:
                        lines.Add(pad + kv.Key + ": " + Scalar(kv.Value));
                        break;
                }
            }
        }

        private static string Scalar(object? x)
        {
            switch (x)
            {
                case null:
                    return "null";
                case bool b:
                    return b ? "true" : "false";
                case long or int:
                    return Convert.ToString(x, CultureInfo.InvariantCulture)!;
                case YamlMap m when m.Count == 0:
                    return "{}";
                case List<object?> l when l.Count == 0:
                    return "[]";
                case string s:
                    try
                    {
                        // A control character can never be written plain: the reader splits lines before it sees a scalar, so a raw line break
                        // would turn one value into a malformed entry.
                        if (!s.Any(c => c < 0x20 || c == 0x7F) && new Reader(YamlReaderMutation.None).ParseScalar(s, 0) is string back && back == s)
                        {
                            return s;
                        }
                    }
                    catch (YamlSubsetException)
                    {
                        // falls through to the quoted form
                    }

                    return Quote(s);
                default:
                    throw new InvalidOperationException("value outside the subset: " + x.GetType().Name);
            }
        }

        private static string Quote(string s)
        {
            var sb = new StringBuilder("\"");
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20 || c == 0x7F)
                        {
                            sb.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            return sb.Append('"').ToString();
        }

        private readonly struct Line
        {
            public Line(int number, int indent, string body)
            {
                Number = number;
                Indent = indent;
                Body = body;
            }

            public int Number { get; }

            public int Indent { get; }

            public string Body { get; }
        }

        private sealed class Reader
        {
            private readonly YamlReaderMutation mutation;

            public Reader(YamlReaderMutation mutation)
            {
                this.mutation = mutation;
            }

            private bool Mutated(YamlReaderMutation m) => (mutation & m) != 0;

            public YamlMap Load(string text, YamlReadMode mode)
            {
                var lines = Lines(text);
                if (lines.Count == 0)
                {
                    throw new YamlSubsetException(1, "empty document");
                }

                if (lines[0].Indent != 0)
                {
                    throw new YamlSubsetException(lines[0].Number, "the document must start at column 0");
                }

                if (mode == YamlReadMode.Strict)
                {
                    var (map, next) = ParseMap(lines, 0, 0);
                    if (next != lines.Count)
                    {
                        throw new YamlSubsetException(lines[next].Number, "trailing content");
                    }

                    return map;
                }

                return LoadHeader(lines);
            }

            public object? ParseScalar(string t, int ln)
            {
                if (t.Length == 0)
                {
                    throw new YamlSubsetException(ln, "empty scalar");
                }

                if (t == "[]")
                {
                    return new List<object?>();
                }

                if (t == "{}")
                {
                    return new YamlMap();
                }

                if (t[0] == '"')
                {
                    if (t.Length < 2 || t[t.Length - 1] != '"')
                    {
                        throw new YamlSubsetException(ln, "unterminated or multi-line double-quoted scalar");
                    }

                    var body = t.Substring(1, t.Length - 2);
                    var sb = new StringBuilder();
                    for (var i = 0; i < body.Length;)
                    {
                        var c = body[i];
                        if (c == '"')
                        {
                            throw new YamlSubsetException(ln, "unescaped quote inside a double-quoted scalar");
                        }

                        if (c != '\\')
                        {
                            sb.Append(c);
                            i++;
                            continue;
                        }

                        if (i + 1 >= body.Length)
                        {
                            throw new YamlSubsetException(ln, "dangling escape");
                        }

                        var e = body[i + 1];
                        if (e == '"' || e == '\\' || e == '/')
                        {
                            sb.Append(e);
                            i += 2;
                        }
                        else if (e == 'n')
                        {
                            sb.Append('\n');
                            i += 2;
                        }
                        else if (e == 't')
                        {
                            sb.Append('\t');
                            i += 2;
                        }
                        else if (e == 'u' && i + 6 <= body.Length && Hex4.IsMatch(body.Substring(i + 2, 4)))
                        {
                            sb.Append((char)int.Parse(body.Substring(i + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            i += 6;
                        }
                        else
                        {
                            throw new YamlSubsetException(ln, "unsupported escape \\" + e);
                        }
                    }

                    return sb.ToString();
                }

                if (t[0] == '\'')
                {
                    if (t.Length < 2 || t[t.Length - 1] != '\'')
                    {
                        throw new YamlSubsetException(ln, "unterminated or multi-line single-quoted scalar");
                    }

                    var body = t.Substring(1, t.Length - 2);
                    if (body.Replace("''", string.Empty).Contains('\''))
                    {
                        throw new YamlSubsetException(ln, "unescaped quote inside a single-quoted scalar");
                    }

                    return body.Replace("''", "'");
                }

                if (Indicators.IndexOf(t[0]) >= 0 || t.StartsWith("- ", StringComparison.Ordinal) || t == "-")
                {
                    throw new YamlSubsetException(ln, "plain scalar starts with an indicator: " + Truncate(t, 20));
                }

                if ((t.Contains(": ") || t.EndsWith(":", StringComparison.Ordinal)) && !Mutated(YamlReaderMutation.AcceptPlainColon))
                {
                    throw new YamlSubsetException(ln, "plain scalar contains ': ' (quote it)");
                }

                if (t.Contains(" #"))
                {
                    throw new YamlSubsetException(ln, "plain scalar contains ' #' (comment or ambiguity: quote it)");
                }

                if (t != t.Trim())
                {
                    throw new YamlSubsetException(ln, "plain scalar with surrounding spaces");
                }

                switch (t)
                {
                    case "null": return null;
                    case "true": return true;
                    case "false": return false;
                }

                if (Integer.IsMatch(t))
                {
                    if (long.TryParse(t, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n))
                    {
                        return n;
                    }

                    throw new YamlSubsetException(ln, "integer out of range: " + Truncate(t, 20));
                }

                if (Ambiguous.Contains(t) || LeadingZero.IsMatch(t) || FloatLike.IsMatch(t))
                {
                    throw new YamlSubsetException(ln, "ambiguous plain scalar (YAML would type it differently): " + t);
                }

                return t;
            }

            private static string Truncate(string s, int n) => s.Length <= n ? s : s.Substring(0, n);

            private static List<Line> Lines(string text)
            {
                var result = new List<Line>();
                var raw = text.Replace("\r\n", "\n").Split('\n');
                for (var k = 0; k < raw.Length; k++)
                {
                    var number = k + 1;
                    var r = raw[k];
                    var leading = r.Length - r.TrimStart(' ', '\t').Length;
                    if (r.Substring(0, leading).Contains('\t'))
                    {
                        throw new YamlSubsetException(number, "tab in indentation");
                    }

                    var s = r.TrimEnd(' ');
                    if (s.Trim().Length == 0 || s.TrimStart().StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (s.Trim() == "---" || s.Trim() == "...")
                    {
                        throw new YamlSubsetException(number, "document markers are not supported");
                    }

                    if (s.StartsWith("%", StringComparison.Ordinal))
                    {
                        throw new YamlSubsetException(number, "directives are not supported");
                    }

                    var indent = s.Length - s.TrimStart(' ').Length;
                    result.Add(new Line(number, indent, s.Substring(indent)));
                }

                return result;
            }

            private static bool IsItem(string body) => body.StartsWith("- ", StringComparison.Ordinal) || body == "-";

            private (object? Value, int Next) ParseBlock(List<Line> lines, int i, int indent)
            {
                if (IsItem(lines[i].Body))
                {
                    var (seq, next) = ParseSeq(lines, i, indent);
                    return (seq, next);
                }

                var (map, n) = ParseMap(lines, i, indent);
                return (map, n);
            }

            private (object? Value, int Next) ValueAfterKey(List<Line> lines, int i, int indent, string? rest, int ln)
            {
                if (rest != null)
                {
                    if ((rest.StartsWith("|", StringComparison.Ordinal) || rest.StartsWith(">", StringComparison.Ordinal))
                        && !Mutated(YamlReaderMutation.AcceptBlockScalars))
                    {
                        throw new YamlSubsetException(ln, "block scalars (| >) are not supported");
                    }

                    if (rest.StartsWith("&", StringComparison.Ordinal) || rest.StartsWith("*", StringComparison.Ordinal) || rest.StartsWith("!", StringComparison.Ordinal))
                    {
                        throw new YamlSubsetException(ln, "anchors, aliases and tags are not supported");
                    }

                    if ((rest.StartsWith("[", StringComparison.Ordinal) || rest.StartsWith("{", StringComparison.Ordinal)) && rest != "[]" && rest != "{}")
                    {
                        throw new YamlSubsetException(ln, "flow collections other than [] and {} are not supported");
                    }

                    object? v;
                    if ((rest.StartsWith("|", StringComparison.Ordinal) || rest.StartsWith(">", StringComparison.Ordinal)) && Mutated(YamlReaderMutation.AcceptBlockScalars))
                    {
                        v = rest;
                        var j = i + 1;
                        while (j < lines.Count && lines[j].Indent > indent)
                        {
                            j++;
                        }

                        return (v, j);
                    }

                    v = ParseScalar(rest, ln);
                    if (i + 1 < lines.Count && lines[i + 1].Indent > indent)
                    {
                        if (!Mutated(YamlReaderMutation.AcceptMultilinePlain))
                        {
                            throw new YamlSubsetException(lines[i + 1].Number, "indented content after a scalar value (multi-line scalar)");
                        }

                        var j = i + 1;
                        while (j < lines.Count && lines[j].Indent > indent)
                        {
                            j++;
                        }

                        return (v, j);
                    }

                    return (v, i + 1);
                }

                if (i + 1 >= lines.Count || lines[i + 1].Indent <= indent)
                {
                    if (i + 1 < lines.Count && lines[i + 1].Indent == indent && lines[i + 1].Body.StartsWith("- ", StringComparison.Ordinal))
                    {
                        throw new YamlSubsetException(lines[i + 1].Number, "sequence must be indented under its key");
                    }

                    throw new YamlSubsetException(ln, "empty value without a nested block (write null, [] or {})");
                }

                return ParseBlock(lines, i + 1, lines[i + 1].Indent);
            }

            private (YamlMap Map, int Next) ParseMap(List<Line> lines, int i, int indent)
            {
                var map = new YamlMap();
                while (i < lines.Count)
                {
                    var line = lines[i];
                    if (line.Indent < indent)
                    {
                        break;
                    }

                    if (line.Indent > indent)
                    {
                        throw new YamlSubsetException(line.Number, "unexpected indentation");
                    }

                    if (IsItem(line.Body))
                    {
                        throw new YamlSubsetException(line.Number, "sequence item where a mapping key was expected");
                    }

                    if (line.Body.StartsWith("?", StringComparison.Ordinal))
                    {
                        throw new YamlSubsetException(line.Number, "complex keys are not supported");
                    }

                    var m = KeyLine.Match(line.Body);
                    if (!m.Success)
                    {
                        if (line.Body.StartsWith("\"", StringComparison.Ordinal) || line.Body.StartsWith("'", StringComparison.Ordinal))
                        {
                            throw new YamlSubsetException(line.Number, "quoted keys are not supported");
                        }

                        throw new YamlSubsetException(line.Number, "malformed mapping entry: " + Truncate(line.Body, 40));
                    }

                    var key = m.Groups[1].Value;
                    var rest = m.Groups[2].Success ? m.Groups[2].Value : null;
                    if (rest != null && rest.Trim().Length == 0)
                    {
                        rest = null;
                    }

                    var duplicate = map.ContainsKey(key);
                    if (duplicate && !Mutated(YamlReaderMutation.AcceptDuplicateKeys))
                    {
                        throw new YamlSubsetException(line.Number, "duplicate key: " + key);
                    }

                    var (value, next) = ValueAfterKey(lines, i, indent, rest, line.Number);
                    map[key] = value;
                    i = next;
                }

                return (map, i);
            }

            private (List<object?> Seq, int Next) ParseSeq(List<Line> lines, int i, int indent)
            {
                var seq = new List<object?>();
                while (i < lines.Count)
                {
                    var line = lines[i];
                    if (line.Indent < indent)
                    {
                        break;
                    }

                    if (line.Indent > indent)
                    {
                        throw new YamlSubsetException(line.Number, "unexpected indentation inside a sequence");
                    }

                    if (!IsItem(line.Body))
                    {
                        throw new YamlSubsetException(line.Number, "mapping key where a sequence item was expected");
                    }

                    if (line.Body == "-")
                    {
                        throw new YamlSubsetException(line.Number, "empty sequence item");
                    }

                    var item = line.Body.Substring(2);
                    if (item.StartsWith("- ", StringComparison.Ordinal))
                    {
                        throw new YamlSubsetException(line.Number, "nested inline sequences are not supported");
                    }

                    if (KeyLine.IsMatch(item))
                    {
                        var sub = new List<Line> { new Line(line.Number, indent + 2, item) };
                        var j = i + 1;
                        while (j < lines.Count && lines[j].Indent > indent)
                        {
                            sub.Add(lines[j]);
                            j++;
                        }

                        var (value, k) = ParseMap(sub, 0, indent + 2);
                        if (k != sub.Count)
                        {
                            throw new YamlSubsetException(sub[k].Number, "malformed sequence item");
                        }

                        seq.Add(value);
                        i = j;
                    }
                    else
                    {
                        if (item.StartsWith("&", StringComparison.Ordinal) || item.StartsWith("*", StringComparison.Ordinal) || item.StartsWith("!", StringComparison.Ordinal)
                            || item.StartsWith("|", StringComparison.Ordinal) || item.StartsWith(">", StringComparison.Ordinal))
                        {
                            throw new YamlSubsetException(line.Number, "unsupported construct in a sequence item");
                        }

                        if ((item.StartsWith("[", StringComparison.Ordinal) || item.StartsWith("{", StringComparison.Ordinal)) && item != "[]" && item != "{}")
                        {
                            throw new YamlSubsetException(line.Number, "flow collections other than [] and {} are not supported");
                        }

                        seq.Add(ParseScalar(item, line.Number));
                        if (i + 1 < lines.Count && lines[i + 1].Indent > indent)
                        {
                            throw new YamlSubsetException(lines[i + 1].Number, "indented content after a scalar item (multi-line scalar)");
                        }

                        i++;
                    }
                }

                return (seq, i);
            }

            private YamlMap LoadHeader(List<Line> lines)
            {
                var blocks = new List<List<Line>>();
                List<Line>? current = null;
                foreach (var line in lines)
                {
                    if (line.Indent == 0)
                    {
                        current = new List<Line> { line };
                        blocks.Add(current);
                    }
                    else
                    {
                        if (current == null)
                        {
                            throw new YamlSubsetException(line.Number, "indented content before any key");
                        }

                        current.Add(line);
                    }
                }

                var result = new YamlMap();
                var skipped = new List<object?>();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var block in blocks)
                {
                    var m = KeyLine.Match(block[0].Body);
                    if (!m.Success)
                    {
                        throw new YamlSubsetException(block[0].Number, "malformed top-level entry");
                    }

                    var key = m.Groups[1].Value;
                    if (!seen.Add(key))
                    {
                        throw new YamlSubsetException(block[0].Number, "duplicate key: " + key);
                    }

                    var rest = m.Groups[2].Success ? m.Groups[2].Value : null;
                    if (key == "schema")
                    {
                        if (block.Count != 1 || rest == null)
                        {
                            throw new YamlSubsetException(block[0].Number, "schema must be a single-line scalar");
                        }

                        result.Add("schema", ParseScalar(rest, block[0].Number));
                    }
                    else if (key == "automation_state")
                    {
                        if (!string.IsNullOrEmpty(rest))
                        {
                            throw new YamlSubsetException(block[0].Number, "automation_state must be a block mapping");
                        }

                        if (block.Count < 2)
                        {
                            throw new YamlSubsetException(block[0].Number, "empty automation_state");
                        }

                        var childIndent = block[1].Indent;
                        var state = new YamlMap();
                        var stateSkipped = new List<object?>();
                        var stateSeen = new HashSet<string>(StringComparer.Ordinal);
                        string? currentKey = null;
                        foreach (var line in block.Skip(1))
                        {
                            if (line.Indent == childIndent)
                            {
                                var cm = KeyLine.Match(line.Body);
                                if (!cm.Success)
                                {
                                    throw new YamlSubsetException(line.Number, "malformed automation_state entry");
                                }

                                var ck = cm.Groups[1].Value;
                                if (!stateSeen.Add(ck))
                                {
                                    throw new YamlSubsetException(line.Number, "duplicate key: " + ck);
                                }

                                currentKey = ck;
                                var crest = cm.Groups[2].Success ? cm.Groups[2].Value : null;
                                if (HeaderConsumed.Contains(ck))
                                {
                                    if (string.IsNullOrEmpty(crest))
                                    {
                                        throw new YamlSubsetException(line.Number, ck + " must be a single-line scalar");
                                    }

                                    state.Add(ck, ParseScalar(crest!, line.Number));
                                }
                                else
                                {
                                    stateSkipped.Add(ck);
                                }
                            }
                            else if (line.Indent > childIndent)
                            {
                                if (currentKey != null && HeaderConsumed.Contains(currentKey))
                                {
                                    throw new YamlSubsetException(line.Number, currentKey + " must be a single-line scalar (continuation found)");
                                }
                            }
                            else
                            {
                                throw new YamlSubsetException(line.Number, "unexpected indentation in automation_state");
                            }
                        }

                        state.Add("__skipped__", stateSkipped);
                        result.Add("automation_state", state);
                    }
                    else
                    {
                        skipped.Add(key);
                    }
                }

                result.Add("__skipped__", skipped);
                return result;
            }
        }
    }
}
