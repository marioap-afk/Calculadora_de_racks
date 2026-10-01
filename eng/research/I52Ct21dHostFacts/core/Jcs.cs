using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace I52Ct21d.HostFacts.Core;

/// <summary>
/// RFC 8785 (JSON Canonicalization Scheme) serializer for the JSON values of this package.
/// Supported: objects (keys sorted by UTF-16 code units), arrays, strings, booleans, null and INTEGER numbers.
/// A non-integer number is refused (<see cref="NotSupportedException"/>): every binary64 of the records is carried as a
/// 16-hex-digit pattern string, so no ECMAScript double formatting is needed (fail closed instead of approximating).
/// A string with a lone surrogate is refused (RFC 8785 requires well-formed UTF-16 / I-JSON).
/// </summary>
public static class Jcs
{
    public static string Serialize(JsonNode? node)
    {
        var sb = new StringBuilder();
        Write(sb, node);
        return sb.ToString();
    }

    public static byte[] SerializeUtf8(JsonNode? node) => new UTF8Encoding(false, true).GetBytes(Serialize(node));

    private static void Write(StringBuilder sb, JsonNode? node)
    {
        switch (node)
        {
            case null:
                sb.Append("null");
                break;
            case JsonObject obj:
                sb.Append('{');
                var keys = obj.Select(p => p.Key).ToList();
                keys.Sort(string.CompareOrdinal); // ordinal = UTF-16 code unit order, as RFC 8785 3.2.3 requires
                for (var i = 0; i < keys.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    WriteString(sb, keys[i]);
                    sb.Append(':');
                    Write(sb, obj[keys[i]]);
                }
                sb.Append('}');
                break;
            case JsonArray arr:
                sb.Append('[');
                for (var i = 0; i < arr.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    Write(sb, arr[i]);
                }
                sb.Append(']');
                break;
            case JsonValue val:
                WriteValue(sb, val);
                break;
            default:
                throw new NotSupportedException("unsupported JSON node " + node.GetType().Name);
        }
    }

    private static void WriteValue(StringBuilder sb, JsonValue val)
    {
        if (val.TryGetValue<JsonElement>(out var el))
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.String: WriteString(sb, el.GetString()!); return;
                case JsonValueKind.True: sb.Append("true"); return;
                case JsonValueKind.False: sb.Append("false"); return;
                case JsonValueKind.Null: sb.Append("null"); return;
                case JsonValueKind.Number:
                    if (el.TryGetInt64(out var l)) { sb.Append(l.ToString(CultureInfo.InvariantCulture)); return; }
                    throw new NotSupportedException("non-integer or out-of-range number: " + el.GetRawText());
                default: throw new NotSupportedException("unsupported JSON element " + el.ValueKind);
            }
        }
        if (val.TryGetValue<string>(out var s)) { WriteString(sb, s); return; }
        if (val.TryGetValue<bool>(out var b)) { sb.Append(b ? "true" : "false"); return; }
        if (val.TryGetValue<long>(out var i64)) { sb.Append(i64.ToString(CultureInfo.InvariantCulture)); return; }
        if (val.TryGetValue<int>(out var i32)) { sb.Append(i32.ToString(CultureInfo.InvariantCulture)); return; }
        if (val.TryGetValue<short>(out var i16)) { sb.Append(i16.ToString(CultureInfo.InvariantCulture)); return; }
        if (val.TryGetValue<byte>(out var u8)) { sb.Append(u8.ToString(CultureInfo.InvariantCulture)); return; }
        throw new NotSupportedException("unsupported JSON value (only strings, booleans and integers are canonicalized)");
    }

    private static void WriteString(StringBuilder sb, string s)
    {
        sb.Append('"');
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (char.IsHighSurrogate(c))
            {
                if (i + 1 >= s.Length || !char.IsLowSurrogate(s[i + 1]))
                    throw new ArgumentException("string with a lone surrogate cannot be canonicalized (RFC 8785)");
                sb.Append(c).Append(s[i + 1]);
                i++;
                continue;
            }
            if (char.IsLowSurrogate(c)) throw new ArgumentException("string with a lone surrogate cannot be canonicalized (RFC 8785)");
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\t': sb.Append("\\t"); break;
                case '\n': sb.Append("\\n"); break;
                case '\f': sb.Append("\\f"); break;
                case '\r': sb.Append("\\r"); break;
                default:
                    if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    else sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
    }

    /// <summary>Strict parse: duplicate member names and comments are refused.</summary>
    public static JsonNode ParseStrict(string json)
    {
        RejectDuplicateMembers(json);
        var node = JsonNode.Parse(json, nodeOptions: null, documentOptions: new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
        });
        return node ?? throw new FormatException("JSON null document");
    }

    // The framework parser keeps the last of two equal member names without an error; a record with a duplicate member is ambiguous.
    private static void RejectDuplicateMembers(string json)
    {
        var reader = new Utf8JsonReader(new UTF8Encoding(false, true).GetBytes(json), new JsonReaderOptions { CommentHandling = JsonCommentHandling.Disallow });
        var scopes = new Stack<HashSet<string>?>();
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject: scopes.Push(new HashSet<string>(StringComparer.Ordinal)); break;
                case JsonTokenType.StartArray: scopes.Push(null); break;
                case JsonTokenType.EndObject:
                case JsonTokenType.EndArray: scopes.Pop(); break;
                case JsonTokenType.PropertyName:
                    if (!scopes.Peek()!.Add(reader.GetString()!))
                        throw new FormatException("duplicate JSON member name: " + reader.GetString());
                    break;
            }
        }
    }
}
