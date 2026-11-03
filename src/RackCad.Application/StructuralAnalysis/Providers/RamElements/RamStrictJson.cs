using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The diagnostic codes a strict reader reports, by kind of finding.</summary>
    internal sealed class RamJsonCodes
    {
        public RamJsonCodes(string malformed, string schema, string duplicate, string unknown, string missing, string wrongType, string value)
        {
            Malformed = malformed;
            Schema = schema;
            Duplicate = duplicate;
            Unknown = unknown;
            Missing = missing;
            WrongType = wrongType;
            Value = value;
        }

        public string Malformed { get; }

        public string Schema { get; }

        public string Duplicate { get; }

        public string Unknown { get; }

        public string Missing { get; }

        public string WrongType { get; }

        public string Value { get; }
    }

    internal sealed class RamJsonObject : Dictionary<string, object>
    {
        public RamJsonObject()
            : base(StringComparer.Ordinal)
        {
        }
    }

    internal sealed class RamJsonNumber
    {
        public RamJsonNumber(double value)
        {
            Value = value;
        }

        public double Value { get; }

        public bool IsFinite => !double.IsNaN(Value) && !double.IsInfinity(Value);
    }

    internal sealed class RamJsonNull
    {
        public static readonly RamJsonNull Instance = new RamJsonNull();
    }

    /// <summary>
    /// The strict reading of a JSON text shared by the readers of the dump, the read-back and the manifest: a token reader that SEES a duplicate
    /// property (a document model hides it), the exact set of properties of every object, and the types of the values. Every finding is
    /// collected as a blocking diagnostic of the outcome Invalid; the first one does not hide the rest. It does no I/O: it receives TEXT.
    /// </summary>
    internal sealed class RamJsonScan
    {
        private const int MaxDepth = 32;

        private readonly RamJsonCodes _codes;

        public RamJsonScan(RamJsonCodes codes)
        {
            _codes = codes;
            Diagnostics = new List<StructuralDiagnostic>();
        }

        public List<StructuralDiagnostic> Diagnostics { get; }

        public bool HasFindings => Diagnostics.Count > 0;

        public RamJsonCodes Codes => _codes;

        public void Invalid(string code, string subject) =>
            Diagnostics.Add(StructuralDiagnostic.Blocking(code, StructuralExportOutcome.Invalid, subject));

        // ---------------------------------------------------------------- the parse

        /// <summary>The tree of the text, or <c>null</c> (with the diagnostic) when the text is not one single well-formed JSON value.</summary>
        public object Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Invalid(_codes.Malformed, "$");
                return null;
            }

            var text = json[0] == (char)0xFEFF ? json.Substring(1) : json;

            try
            {
                var options = new JsonReaderOptions
                {
                    CommentHandling = JsonCommentHandling.Disallow,
                    AllowTrailingCommas = false,
                    MaxDepth = MaxDepth,
                };

                var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(new UTF8Encoding(false).GetBytes(text)), isFinalBlock: true, state: new JsonReaderState(options));

                if (!reader.Read())
                {
                    throw new JsonException("Empty document.");
                }

                var root = ReadValue(ref reader, "$");

                if (reader.Read())
                {
                    throw new JsonException("More than one root value.");
                }

                return root;
            }
            catch (JsonException)
            {
                Diagnostics.Clear();
                Invalid(_codes.Malformed, "$");
                return null;
            }
            catch (InvalidOperationException)
            {
                Diagnostics.Clear();
                Invalid(_codes.Malformed, "$");
                return null;
            }
        }

        private object ReadValue(ref Utf8JsonReader reader, string path)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    var obj = new RamJsonObject();

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        var name = reader.GetString();
                        reader.Read();
                        var value = ReadValue(ref reader, path + "." + name);

                        if (obj.ContainsKey(name))
                        {
                            Invalid(_codes.Duplicate, path + "." + name);
                        }
                        else
                        {
                            obj.Add(name, value);
                        }
                    }

                    return obj;

                case JsonTokenType.StartArray:
                    var list = new List<object>();

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        list.Add(ReadValue(ref reader, path + "[" + list.Count.ToString(CultureInfo.InvariantCulture) + "]"));
                    }

                    return list;

                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.Number:
                    return new RamJsonNumber(reader.TryGetDouble(out var parsed) ? parsed : double.PositiveInfinity);

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Null:
                    return RamJsonNull.Instance;

                default:
                    throw new JsonException("Unexpected token " + reader.TokenType + ".");
            }
        }

        // ---------------------------------------------------------------- the shape

        public RamJsonObject Object(object value, string path)
        {
            if (value is RamJsonObject obj)
            {
                return obj;
            }

            Invalid(_codes.WrongType, path);
            return null;
        }

        /// <summary>The exact set of properties: each unknown one and each missing one is a finding.</summary>
        public void Keys(RamJsonObject obj, string path, params string[] expected)
        {
            var known = new HashSet<string>(expected, StringComparer.Ordinal);

            foreach (var key in obj.Keys)
            {
                if (!known.Contains(key))
                {
                    Invalid(_codes.Unknown, path + "." + key);
                }
            }

            foreach (var key in expected)
            {
                if (!obj.ContainsKey(key))
                {
                    Invalid(_codes.Missing, path + "." + key);
                }
            }
        }

        public List<object> Array(RamJsonObject obj, string key, string path)
        {
            if (!obj.TryGetValue(key, out var value))
            {
                return null;
            }

            if (value is List<object> list)
            {
                return list;
            }

            Invalid(_codes.WrongType, path + "." + key);
            return null;
        }

        public RamJsonObject Child(RamJsonObject obj, string key, string path)
        {
            return obj.TryGetValue(key, out var value) ? Object(value, path + "." + key) : null;
        }

        /// <summary>A text value; <c>null</c> when it is absent, wrongly typed, JSON null (unless allowed) or blank (when a name is required).</summary>
        public string String(RamJsonObject obj, string key, string path, bool nonBlank = false, bool allowNull = false)
        {
            if (!obj.TryGetValue(key, out var value))
            {
                return null;
            }

            var subject = path + "." + key;

            if (value is RamJsonNull && allowNull)
            {
                return null;
            }

            if (!(value is string text))
            {
                Invalid(_codes.WrongType, subject);
                return null;
            }

            if (nonBlank && string.IsNullOrWhiteSpace(text))
            {
                Invalid(_codes.Value, subject);
                return null;
            }

            return text;
        }

        public double? Number(RamJsonObject obj, string key, string path)
        {
            return obj.TryGetValue(key, out var value) ? NumberOf(value, path + "." + key) : null;
        }

        private double? NumberOf(object value, string subject)
        {
            if (!(value is RamJsonNumber number))
            {
                Invalid(_codes.WrongType, subject);
                return null;
            }

            if (!number.IsFinite)
            {
                Invalid(_codes.Value, subject);
                return null;
            }

            return number.Value;
        }

        public bool? Bool(RamJsonObject obj, string key, string path)
        {
            if (!obj.TryGetValue(key, out var value))
            {
                return null;
            }

            if (value is bool flag)
            {
                return flag;
            }

            Invalid(_codes.WrongType, path + "." + key);
            return null;
        }

        /// <summary>A whole number within the range; a fraction or a value outside it is a finding of value, a non-number one of type.</summary>
        public int? Integer(RamJsonObject obj, string key, string path, int minimum, int maximum)
        {
            var number = Number(obj, key, path);

            if (!number.HasValue)
            {
                return null;
            }

            if (number.Value != Math.Floor(number.Value) || number.Value < minimum || number.Value > maximum)
            {
                Invalid(_codes.Value, path + "." + key);
                return null;
            }

            return (int)number.Value;
        }

        /// <summary>An array of exactly <paramref name="count"/> finite numbers.</summary>
        public double[] Numbers(RamJsonObject obj, string key, string path, int count)
        {
            var list = Array(obj, key, path);

            if (list == null)
            {
                return null;
            }

            var subject = path + "." + key;
            var values = new double[list.Count];
            var ok = true;

            for (var index = 0; index < list.Count; index++)
            {
                var number = NumberOf(list[index], subject + "[" + index.ToString(CultureInfo.InvariantCulture) + "]");

                if (number.HasValue)
                {
                    values[index] = number.Value;
                }
                else
                {
                    ok = false;
                }
            }

            if (list.Count != count)
            {
                Invalid(_codes.Value, subject);
                return null;
            }

            return ok ? values : null;
        }

        /// <summary>An array of exactly <paramref name="count"/> booleans.</summary>
        public bool[] Bools(RamJsonObject obj, string key, string path, int count)
        {
            var list = Array(obj, key, path);

            if (list == null)
            {
                return null;
            }

            var subject = path + "." + key;
            var values = new bool[list.Count];
            var ok = true;

            for (var index = 0; index < list.Count; index++)
            {
                if (list[index] is bool flag)
                {
                    values[index] = flag;
                }
                else
                {
                    Invalid(_codes.WrongType, subject + "[" + index.ToString(CultureInfo.InvariantCulture) + "]");
                    ok = false;
                }
            }

            if (list.Count != count)
            {
                Invalid(_codes.Value, subject);
                return null;
            }

            return ok ? values : null;
        }

        /// <summary>An array of texts.</summary>
        public List<string> Strings(RamJsonObject obj, string key, string path)
        {
            var list = Array(obj, key, path);

            if (list == null)
            {
                return null;
            }

            var values = new List<string>();
            var ok = true;

            for (var index = 0; index < list.Count; index++)
            {
                if (list[index] is string text)
                {
                    values.Add(text);
                }
                else
                {
                    Invalid(_codes.WrongType, path + "." + key + "[" + index.ToString(CultureInfo.InvariantCulture) + "]");
                    ok = false;
                }
            }

            return ok ? values : null;
        }

        /// <summary>A lower-case hexadecimal SHA-256 (64 characters).</summary>
        public static bool IsSha256(string text)
        {
            if (text == null || text.Length != 64)
            {
                return false;
            }

            foreach (var character in text)
            {
                if (!((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f')))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
