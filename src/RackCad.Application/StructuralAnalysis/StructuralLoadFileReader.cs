using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Units;

namespace RackCad.Application.StructuralAnalysis
{
    public enum StructuralLoadSide
    {
        PositiveY = 1,
        NegativeY = 2,
    }

    /// <summary>One load of the file, with its magnitude already converted to pounds-force.</summary>
    public sealed class StructuralLoadEntry
    {
        public StructuralLoadEntry(string id, int level, StructuralLoadSide side, double positionFraction, StructuralForce magnitude)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "The level starts at 1.");
            }

            if (!(positionFraction >= 0 && positionFraction <= 1))
            {
                throw new ArgumentOutOfRangeException(nameof(positionFraction), positionFraction, "The fraction lies in [0, 1].");
            }

            Id = StructuralRequire.Text(id, nameof(id));
            Level = level;
            Side = side;
            PositionFraction = positionFraction;
            Magnitude = magnitude;
        }

        public string Id { get; }

        public int Level { get; }

        public StructuralLoadSide Side { get; }

        /// <summary>The fraction, in [0, 1], of the physical arm measured from its start on the column side.</summary>
        public double PositionFraction { get; }

        public StructuralForce Magnitude { get; }
    }

    public sealed class StructuralLoadFile
    {
        public StructuralLoadFile(string sha256, IEnumerable<StructuralLoadEntry> loads)
        {
            Sha256 = StructuralRequire.Text(sha256, nameof(sha256));
            Loads = StructuralRequire.Items(loads, nameof(loads));
        }

        /// <summary>SHA-256, in lowercase hexadecimal, of the exact bytes of the file.</summary>
        public string Sha256 { get; }

        public IReadOnlyList<StructuralLoadEntry> Loads { get; }
    }

    public sealed class StructuralLoadFileReadResult
    {
        internal StructuralLoadFileReadResult(
            StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, StructuralLoadFile loadFile)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            LoadFile = loadFile;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        /// <summary>The parsed file when, and only when, the outcome is Valid.</summary>
        public StructuralLoadFile LoadFile { get; }
    }

    /// <summary>
    /// The strict reader of <c>rackcad-structural-loads/v1</c> (I-65 D-10b, CD-I65-F1-05). A number without a unit is never
    /// accepted.
    ///
    /// <para>
    /// The shape is EXACT: an object with <c>schema</c> and <c>loads</c>; each load with <c>id</c>, <c>target</c>
    /// (<c>level</c>, <c>side</c>), <c>positionBasis</c>, <c>position</c>, <c>direction</c>, <c>magnitude</c> and
    /// <c>unit</c>. A duplicate or unknown property, a wrong type or a missing unit is Invalid, and every finding of the
    /// file is reported, not only the first. The file is read with the BCL only (<c>System.Text.Json</c>), through a
    /// token reader that sees duplicate properties, which a document model hides.
    /// </para>
    /// <para>
    /// The conversion of a magnitude to pounds-force happens HERE, once, through <see cref="ForceUnits"/>. The target
    /// (<c>level</c>, <c>side</c>) is only parsed: whether it exists is the source's decision.
    /// </para>
    /// </summary>
    public static class StructuralLoadFileReader
    {
        public const string SchemaId = "rackcad-structural-loads/v1";

        private const string PhysicalArmFraction = "physicalArmFraction";
        private const string NegativeZ = "-Z";

        private static readonly string[] RootProperties = { "schema", "loads" };
        private static readonly string[] LoadProperties = { "id", "target", "positionBasis", "position", "direction", "magnitude", "unit" };
        private static readonly string[] TargetProperties = { "level", "side" };

        public static StructuralLoadFileReadResult Read(byte[] bytes)
        {
            var diagnostics = new List<StructuralDiagnostic>();

            if (bytes == null || bytes.Length == 0)
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsJsonMalformed, "$");
                return Result(diagnostics, null);
            }

            var sha256 = StructuralFingerprints.Sha256Hex(bytes);
            var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            var content = new ReadOnlySpan<byte>(bytes, hasBom ? 3 : 0, bytes.Length - (hasBom ? 3 : 0));
            object root;

            try
            {
                root = Parse(content, diagnostics);
            }
            catch (JsonException)
            {
                diagnostics.RemoveAll(d => true);
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsJsonMalformed, "$");
                return Result(diagnostics, null);
            }
            catch (InvalidOperationException)
            {
                diagnostics.RemoveAll(d => true);
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsJsonMalformed, "$");
                return Result(diagnostics, null);
            }

            var loads = new List<StructuralLoadEntry>();
            ValidateRoot(root, loads, diagnostics);

            return diagnostics.Count == 0
                ? Result(diagnostics, new StructuralLoadFile(sha256, loads))
                : Result(diagnostics, null);
        }

        // ---------------------------------------------------------------- the parse (sees duplicates)

        private sealed class JsonObjectValue : Dictionary<string, object>
        {
            public JsonObjectValue()
                : base(StringComparer.Ordinal)
            {
            }
        }

        private sealed class JsonNumberValue
        {
            public JsonNumberValue(double value, int? integer)
            {
                Value = value;
                Integer = integer;
            }

            public double Value { get; }

            public int? Integer { get; }
        }

        private sealed class JsonNullValue
        {
            public static readonly JsonNullValue Instance = new JsonNullValue();
        }

        private static object Parse(ReadOnlySpan<byte> content, List<StructuralDiagnostic> diagnostics)
        {
            var options = new JsonReaderOptions
            {
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false,
                MaxDepth = 16,
            };

            var reader = new Utf8JsonReader(content, isFinalBlock: true, state: new JsonReaderState(options));

            if (!reader.Read())
            {
                throw new JsonException("Empty document.");
            }

            var root = ReadValue(ref reader, "$", diagnostics);

            if (reader.Read())
            {
                throw new JsonException("More than one root value.");
            }

            return root;
        }

        private static object ReadValue(ref Utf8JsonReader reader, string path, List<StructuralDiagnostic> diagnostics)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    var obj = new JsonObjectValue();

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        var name = reader.GetString();
                        reader.Read();
                        var value = ReadValue(ref reader, path + "." + name, diagnostics);

                        if (obj.ContainsKey(name))
                        {
                            Invalid(diagnostics, StructuralDiagnosticCodes.LoadsDuplicateProperty, path + "." + name);
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
                        list.Add(ReadValue(ref reader, path + "[" + list.Count + "]", diagnostics));
                    }

                    return list;

                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.Number:
                    int? integer = reader.TryGetInt32(out var parsedInteger) ? parsedInteger : (int?)null;
                    var number = reader.TryGetDouble(out var parsedDouble) ? parsedDouble : double.PositiveInfinity;
                    return new JsonNumberValue(number, integer);

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Null:
                    return JsonNullValue.Instance;

                default:
                    throw new JsonException("Unexpected token " + reader.TokenType + ".");
            }
        }

        // ---------------------------------------------------------------- the shape

        private static void ValidateRoot(object root, List<StructuralLoadEntry> loads, List<StructuralDiagnostic> diagnostics)
        {
            if (!(root is JsonObjectValue rootObject))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, "$");
                return;
            }

            CheckKeys(rootObject, RootProperties, "$", diagnostics);

            if (rootObject.TryGetValue("schema", out var schema))
            {
                if (!(schema is string schemaText))
                {
                    Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, "$.schema");
                }
                else if (!string.Equals(schemaText, SchemaId, StringComparison.Ordinal))
                {
                    Invalid(diagnostics, StructuralDiagnosticCodes.LoadsSchemaMismatch, "$.schema");
                }
            }

            if (!rootObject.TryGetValue("loads", out var loadsValue))
            {
                return;
            }

            if (!(loadsValue is List<object> items))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, "$.loads");
                return;
            }

            if (items.Count == 0)
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsEmpty, "$.loads");
                return;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < items.Count; index++)
            {
                var entry = ValidateLoad(items[index], "$.loads[" + index + "]", ids, diagnostics);

                if (entry != null)
                {
                    loads.Add(entry);
                }
            }
        }

        private static StructuralLoadEntry ValidateLoad(object item, string path, HashSet<string> ids, List<StructuralDiagnostic> diagnostics)
        {
            if (!(item is JsonObjectValue load))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, path);
                return null;
            }

            var before = diagnostics.Count;
            CheckKeys(load, LoadProperties, path, diagnostics);

            var id = ReadId(load, path, ids, diagnostics);
            var target = ReadTarget(load, path, diagnostics, out var level, out var side);
            var basis = ReadText(load, "positionBasis", path, diagnostics);
            var position = ReadNumber(load, "position", path, diagnostics);
            var direction = ReadText(load, "direction", path, diagnostics);
            var magnitude = ReadNumber(load, "magnitude", path, diagnostics);
            var unit = ReadText(load, "unit", path, diagnostics);

            if (basis != null && !string.Equals(basis, PhysicalArmFraction, StringComparison.Ordinal))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsPositionBasis, path + ".positionBasis");
            }

            if (position.HasValue && !(position.Value >= 0 && position.Value <= 1))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsPositionRange, path + ".position");
            }

            if (direction != null && !string.Equals(direction, NegativeZ, StringComparison.Ordinal))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsDirection, path + ".direction");
            }

            var unitKnown = ForceUnits.TryParseToken(unit, out var forceUnit);

            if (unit != null && !unitKnown)
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsUnitUnknown, path + ".unit");
            }

            if (magnitude.HasValue && (double.IsNaN(magnitude.Value) || double.IsInfinity(magnitude.Value) || magnitude.Value <= 0))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsMagnitude, path + ".magnitude");
            }

            if (diagnostics.Count != before || !target || id == null || !unitKnown)
            {
                return null;
            }

            var pounds = ForceUnits.ToPoundsForce(magnitude.Value, forceUnit);

            if (double.IsNaN(pounds) || double.IsInfinity(pounds) || pounds <= 0)
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsMagnitude, path + ".magnitude");
                return null;
            }

            return new StructuralLoadEntry(id, level, side, position.Value, StructuralForce.FromPoundsForce(pounds));
        }

        private static string ReadId(JsonObjectValue load, string path, HashSet<string> ids, List<StructuralDiagnostic> diagnostics)
        {
            if (!load.TryGetValue("id", out var value))
            {
                return null;
            }

            if (!(value is string id))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, path + ".id");
                return null;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsIdEmpty, path + ".id");
                return null;
            }

            if (!ids.Add(id))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsIdDuplicate, path + ".id");
                return null;
            }

            return id;
        }

        private static bool ReadTarget(
            JsonObjectValue load, string path, List<StructuralDiagnostic> diagnostics, out int level, out StructuralLoadSide side)
        {
            level = 0;
            side = default;

            if (!load.TryGetValue("target", out var value))
            {
                return false;
            }

            var targetPath = path + ".target";

            if (!(value is JsonObjectValue target))
            {
                Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, targetPath);
                return false;
            }

            var ok = !CheckKeys(target, TargetProperties, targetPath, diagnostics);

            if (target.TryGetValue("level", out var levelValue))
            {
                if (levelValue is JsonNumberValue levelNumber && levelNumber.Integer.HasValue)
                {
                    level = levelNumber.Integer.Value;

                    if (level < 1)
                    {
                        Invalid(diagnostics, StructuralDiagnosticCodes.LoadsTargetInvalid, targetPath + ".level");
                        ok = false;
                    }
                }
                else
                {
                    Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, targetPath + ".level");
                    ok = false;
                }
            }
            else
            {
                ok = false;
            }

            if (target.TryGetValue("side", out var sideValue))
            {
                if (sideValue is string sideText)
                {
                    if (string.Equals(sideText, "PositiveY", StringComparison.Ordinal))
                    {
                        side = StructuralLoadSide.PositiveY;
                    }
                    else if (string.Equals(sideText, "NegativeY", StringComparison.Ordinal))
                    {
                        side = StructuralLoadSide.NegativeY;
                    }
                    else
                    {
                        Invalid(diagnostics, StructuralDiagnosticCodes.LoadsTargetInvalid, targetPath + ".side");
                        ok = false;
                    }
                }
                else
                {
                    Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, targetPath + ".side");
                    ok = false;
                }
            }
            else
            {
                ok = false;
            }

            return ok;
        }

        private static string ReadText(JsonObjectValue load, string name, string path, List<StructuralDiagnostic> diagnostics)
        {
            if (!load.TryGetValue(name, out var value))
            {
                return null;
            }

            if (value is string text)
            {
                return text;
            }

            Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, path + "." + name);
            return null;
        }

        private static double? ReadNumber(JsonObjectValue load, string name, string path, List<StructuralDiagnostic> diagnostics)
        {
            if (!load.TryGetValue(name, out var value))
            {
                return null;
            }

            if (value is JsonNumberValue number)
            {
                return number.Value;
            }

            Invalid(diagnostics, StructuralDiagnosticCodes.LoadsWrongType, path + "." + name);
            return null;
        }

        /// <summary>Reports unknown and missing properties; returns whether it reported any.</summary>
        private static bool CheckKeys(JsonObjectValue obj, string[] allowed, string path, List<StructuralDiagnostic> diagnostics)
        {
            var reported = false;

            foreach (var key in obj.Keys.OrderBy(k => k, StringComparer.Ordinal))
            {
                if (!allowed.Contains(key, StringComparer.Ordinal))
                {
                    Invalid(diagnostics, StructuralDiagnosticCodes.LoadsUnknownProperty, path + "." + key);
                    reported = true;
                }
            }

            foreach (var key in allowed)
            {
                if (!obj.ContainsKey(key))
                {
                    var code = string.Equals(key, "unit", StringComparison.Ordinal)
                        ? StructuralDiagnosticCodes.LoadsUnitMissing
                        : StructuralDiagnosticCodes.LoadsMissingProperty;

                    Invalid(diagnostics, code, path + "." + key);
                    reported = true;
                }
            }

            return reported;
        }

        private static void Invalid(List<StructuralDiagnostic> diagnostics, string code, string subject) =>
            diagnostics.Add(StructuralDiagnostic.Blocking(code, StructuralExportOutcome.Invalid, subject));

        private static StructuralLoadFileReadResult Result(List<StructuralDiagnostic> diagnostics, StructuralLoadFile file)
        {
            var list = new ReadOnlyCollection<StructuralDiagnostic>(diagnostics.ToList());

            return new StructuralLoadFileReadResult(StructuralModelResult.Aggregate(list), list, file);
        }
    }
}
