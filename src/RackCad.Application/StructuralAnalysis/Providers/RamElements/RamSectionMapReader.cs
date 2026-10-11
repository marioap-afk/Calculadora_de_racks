using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// The strict reader of <c>rackcad-ram-elements-section-map/v1</c> (I-65 D-06, D-07). It receives TEXT: it does no I/O.
    ///
    /// <para>
    /// The shape is EXACT. A duplicate or unknown property, a wrong type, a repeated or invalid section id, an empty or
    /// non-printable name, a unit outside the closed tables (<c>psi</c>, <c>ksi</c>; <c>pcf</c>, <c>lbf/in3</c>) or a verified row
    /// with a missing field makes the table Invalid, and every finding is reported, not only the first. An Invalid table is never
    /// projected: <see cref="RamSectionMapReadResult.Table"/> is <c>null</c>. A proposed row may be incomplete and assigns nothing.
    /// </para>
    /// <para>
    /// The file is read with the BCL only (<c>System.Text.Json</c>), through a token reader that sees duplicate properties, which a
    /// document model hides.
    /// </para>
    /// </summary>
    public static class RamSectionMapReader
    {
        public const string SchemaId = "rackcad-ram-elements-section-map/v1";

        private const string Proposed = "proposed";
        private const string Verified = "verified";
        private const string DateFormat = "yyyy-MM-dd";

        private static readonly string[] RootProperties = { "schema", "rows" };

        private static readonly string[] RowProperties =
            { "sectionId", "ramSection", "materialId", "ramMaterial", "expectedMaterial", "status", "verifiedBy", "verifiedOn" };

        private static readonly string[] SectionProperties = { "name", "table", "country" };

        private static readonly string[] ExpectedProperties = { "elasticModulus", "poissonRatio", "shearModulus", "unitWeight", "source" };

        private static readonly string[] QuantityProperties = { "value", "unit" };

        private static readonly string[] ModulusUnits = { "psi", "ksi" };

        private static readonly string[] WeightUnits = { "pcf", "lbf/in3" };

        public static RamSectionMapReadResult Read(string json)
        {
            var diagnostics = new List<StructuralDiagnostic>();

            if (string.IsNullOrWhiteSpace(json))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapJsonMalformed, "$");
                return Result(diagnostics, null);
            }

            var text = json.Length > 0 && json[0] == '﻿' ? json.Substring(1) : json;
            object root;

            try
            {
                root = Parse(new UTF8Encoding(false).GetBytes(text), diagnostics);
            }
            catch (JsonException)
            {
                diagnostics.Clear();
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapJsonMalformed, "$");
                return Result(diagnostics, null);
            }
            catch (InvalidOperationException)
            {
                diagnostics.Clear();
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapJsonMalformed, "$");
                return Result(diagnostics, null);
            }

            var rows = new List<RamSectionMapRow>();
            ValidateRoot(root, rows, diagnostics);

            return diagnostics.Count == 0
                ? Result(diagnostics, new RamSectionMaterialTable(rows))
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
            public JsonNumberValue(double value)
            {
                Value = value;
            }

            public double Value { get; }

            public bool IsFinite => !double.IsNaN(Value) && !double.IsInfinity(Value);
        }

        private sealed class JsonNullValue
        {
            public static readonly JsonNullValue Instance = new JsonNullValue();
        }

        private static object Parse(byte[] content, List<StructuralDiagnostic> diagnostics)
        {
            var options = new JsonReaderOptions
            {
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false,
                MaxDepth = 16,
            };

            var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(content), isFinalBlock: true, state: new JsonReaderState(options));

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
                            Invalid(diagnostics, RamElementsDiagnosticCodes.MapDuplicateProperty, path + "." + name);
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
                    return new JsonNumberValue(reader.TryGetDouble(out var parsed) ? parsed : double.PositiveInfinity);

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

        private static void ValidateRoot(object root, List<RamSectionMapRow> rows, List<StructuralDiagnostic> diagnostics)
        {
            if (!(root is JsonObjectValue rootObject))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, "$");
                return;
            }

            CheckKeys(rootObject, RootProperties, "$", diagnostics);

            if (rootObject.TryGetValue("schema", out var schema))
            {
                if (!(schema is string schemaText))
                {
                    Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, "$.schema");
                }
                else if (!string.Equals(schemaText, SchemaId, StringComparison.Ordinal))
                {
                    Invalid(diagnostics, RamElementsDiagnosticCodes.MapSchemaMismatch, "$.schema");
                }
            }

            if (!rootObject.TryGetValue("rows", out var rowsValue))
            {
                return;
            }

            if (!(rowsValue is List<object> items))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, "$.rows");
                return;
            }

            var sectionIds = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < items.Count; index++)
            {
                var row = ValidateRow(items[index], "$.rows[" + index + "]", sectionIds, diagnostics);

                if (row != null)
                {
                    rows.Add(row);
                }
            }
        }

        private static RamSectionMapRow ValidateRow(object item, string path, HashSet<string> sectionIds, List<StructuralDiagnostic> diagnostics)
        {
            if (!(item is JsonObjectValue row))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, path);
                return null;
            }

            var before = diagnostics.Count;
            CheckKeys(row, RowProperties, path, diagnostics);

            var sectionId = ReadSectionId(row, path, sectionIds, diagnostics);
            var materialId = ReadText(row, "materialId", path, required: true, diagnostics);
            var ramMaterial = ReadText(row, "ramMaterial", path, required: true, diagnostics);
            var status = ReadStatus(row, path, diagnostics, out var verified);
            var verifiedBy = ReadText(row, "verifiedBy", path, required: false, diagnostics);
            var verifiedOn = ReadDate(row, path, diagnostics);

            var ramName = (string)null;
            var ramTable = (string)null;
            var ramCountry = (string)null;

            if (row.TryGetValue("ramSection", out var sectionValue))
            {
                if (sectionValue is JsonObjectValue section)
                {
                    var sectionPath = path + ".ramSection";
                    CheckKeys(section, SectionProperties, sectionPath, diagnostics);
                    ramName = ReadText(section, "name", sectionPath, required: true, diagnostics);
                    ramTable = ReadText(section, "table", sectionPath, required: false, diagnostics);
                    ramCountry = ReadText(section, "country", sectionPath, required: false, diagnostics);
                }
                else
                {
                    Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, path + ".ramSection");
                }
            }

            var expected = ReadExpected(row, path, diagnostics);

            if (verified)
            {
                CheckComplete(row, path, expected, diagnostics);
            }

            if (diagnostics.Count != before || sectionId.IsEmpty || expected == null)
            {
                return null;
            }

            return new RamSectionMapRow(
                sectionId,
                ramName,
                ramTable,
                ramCountry,
                StructuralMaterialId.Create(materialId),
                ramMaterial,
                expected,
                status,
                verifiedBy,
                verifiedOn);
        }

        private static StructuralSectionId ReadSectionId(
            JsonObjectValue row, string path, HashSet<string> sectionIds, List<StructuralDiagnostic> diagnostics)
        {
            if (!row.TryGetValue("sectionId", out var value))
            {
                return default;
            }

            var subject = path + ".sectionId";

            if (!(value is string text))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject);
                return default;
            }

            if (!StructuralSectionId.TryParse(text, out var id))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapSectionIdInvalid, subject);
                return default;
            }

            if (!sectionIds.Add(text))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapSectionIdDuplicate, subject);
            }

            return id;
        }

        private static RamExpectedMaterial ReadExpected(JsonObjectValue row, string path, List<StructuralDiagnostic> diagnostics)
        {
            if (!row.TryGetValue("expectedMaterial", out var value))
            {
                return null;
            }

            var subject = path + ".expectedMaterial";

            if (!(value is JsonObjectValue expected))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject);
                return null;
            }

            CheckKeys(expected, ExpectedProperties, subject, diagnostics);

            var modulus = ReadQuantity(expected, "elasticModulus", subject, ModulusUnits, diagnostics);
            var poisson = ReadPoisson(expected, subject, diagnostics);
            var shear = ReadQuantity(expected, "shearModulus", subject, ModulusUnits, diagnostics);
            var weight = ReadQuantity(expected, "unitWeight", subject, WeightUnits, diagnostics);
            var source = ReadText(expected, "source", subject, required: false, diagnostics);

            return new RamExpectedMaterial(modulus, poisson, shear, weight, source);
        }

        private static RamQuantity ReadQuantity(
            JsonObjectValue owner, string name, string path, string[] units, List<StructuralDiagnostic> diagnostics)
        {
            if (!owner.TryGetValue(name, out var value) || value is JsonNullValue)
            {
                return null;
            }

            var subject = path + "." + name;

            if (!(value is JsonObjectValue quantity))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject);
                return null;
            }

            CheckKeys(quantity, QuantityProperties, subject, diagnostics);

            var amount = 0.0;
            var hasAmount = false;

            if (quantity.TryGetValue("value", out var amountValue))
            {
                if (!(amountValue is JsonNumberValue number))
                {
                    Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject + ".value");
                }
                else if (!number.IsFinite || !(number.Value > 0))
                {
                    Invalid(diagnostics, RamElementsDiagnosticCodes.MapValueInvalid, subject + ".value");
                }
                else
                {
                    amount = number.Value;
                    hasAmount = true;
                }
            }

            var unit = (string)null;

            if (quantity.TryGetValue("unit", out var unitValue))
            {
                if (!(unitValue is string unitText))
                {
                    Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject + ".unit");
                }
                else if (!units.Contains(unitText, StringComparer.Ordinal))
                {
                    Invalid(diagnostics, RamElementsDiagnosticCodes.MapUnitUnknown, subject + ".unit");
                }
                else
                {
                    unit = unitText;
                }
            }

            return hasAmount && unit != null ? new RamQuantity(amount, unit) : null;
        }

        private static double? ReadPoisson(JsonObjectValue expected, string path, List<StructuralDiagnostic> diagnostics)
        {
            if (!expected.TryGetValue("poissonRatio", out var value) || value is JsonNullValue)
            {
                return null;
            }

            var subject = path + ".poissonRatio";

            if (!(value is JsonNumberValue number))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject);
                return null;
            }

            // An isotropic material: the ratio lies strictly between -1 and one half.
            if (!number.IsFinite || !(number.Value > -1) || !(number.Value < 0.5))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapValueInvalid, subject);
                return null;
            }

            return number.Value;
        }

        private static RamSectionMapStatus ReadStatus(
            JsonObjectValue row, string path, List<StructuralDiagnostic> diagnostics, out bool verified)
        {
            verified = false;

            if (!row.TryGetValue("status", out var value))
            {
                return RamSectionMapStatus.Proposed;
            }

            var subject = path + ".status";

            if (!(value is string text))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject);
                return RamSectionMapStatus.Proposed;
            }

            if (string.Equals(text, Verified, StringComparison.Ordinal))
            {
                verified = true;
                return RamSectionMapStatus.Verified;
            }

            if (!string.Equals(text, Proposed, StringComparison.Ordinal))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapStatusInvalid, subject);
            }

            return RamSectionMapStatus.Proposed;
        }

        private static string ReadDate(JsonObjectValue row, string path, List<StructuralDiagnostic> diagnostics)
        {
            if (!row.TryGetValue("verifiedOn", out var value) || value is JsonNullValue)
            {
                return null;
            }

            var subject = path + ".verifiedOn";

            if (!(value is string text))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject);
                return null;
            }

            if (text.Length != DateFormat.Length
                || !DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapDateInvalid, subject);
                return null;
            }

            return text;
        }

        /// <summary>A text property: a string that is not blank and has no non-printable character. Optional ones may be <c>null</c>.</summary>
        private static string ReadText(
            JsonObjectValue owner, string name, string path, bool required, List<StructuralDiagnostic> diagnostics)
        {
            if (!owner.TryGetValue(name, out var value))
            {
                return null;
            }

            var subject = path + "." + name;

            if (value is JsonNullValue)
            {
                if (required)
                {
                    Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject);
                }

                return null;
            }

            if (!(value is string text))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapWrongType, subject);
                return null;
            }

            if (!IsPrintable(text))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapTextInvalid, subject);
                return null;
            }

            return text;
        }

        private static bool IsPrintable(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            foreach (var character in text)
            {
                var category = char.GetUnicodeCategory(character);

                if (category == UnicodeCategory.Control
                    || category == UnicodeCategory.Format
                    || category == UnicodeCategory.LineSeparator
                    || category == UnicodeCategory.ParagraphSeparator
                    || (char.IsWhiteSpace(character) && character != ' '))
                {
                    return false;
                }
            }

            return true;
        }

        // ---------------------------------------------------------------- a verified row needs every field

        private static void CheckComplete(
            JsonObjectValue row, string path, RamExpectedMaterial expected, List<StructuralDiagnostic> diagnostics)
        {
            var section = row.TryGetValue("ramSection", out var sectionValue) ? sectionValue as JsonObjectValue : null;

            Required(section, "table", path + ".ramSection", diagnostics);
            Required(section, "country", path + ".ramSection", diagnostics);
            Required(row, "verifiedBy", path, diagnostics);
            Required(row, "verifiedOn", path, diagnostics);

            var expectedObject = row.TryGetValue("expectedMaterial", out var expectedValue) ? expectedValue as JsonObjectValue : null;
            var expectedPath = path + ".expectedMaterial";

            Required(expectedObject, "elasticModulus", expectedPath, diagnostics);
            Required(expectedObject, "unitWeight", expectedPath, diagnostics);
            Required(expectedObject, "source", expectedPath, diagnostics);

            // At least one of the Poisson ratio and the shear modulus.
            if (expectedObject != null
                && IsNullLiteral(expectedObject, "poissonRatio")
                && IsNullLiteral(expectedObject, "shearModulus"))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapVerifiedIncomplete, expectedPath + ".poissonRatio");
            }
        }

        private static void Required(JsonObjectValue owner, string name, string path, List<StructuralDiagnostic> diagnostics)
        {
            if (owner != null && IsNullLiteral(owner, name))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapVerifiedIncomplete, path + "." + name);
            }
        }

        private static bool IsNullLiteral(JsonObjectValue owner, string name) =>
            owner.TryGetValue(name, out var value) && value is JsonNullValue;

        // ---------------------------------------------------------------- helpers

        private static void CheckKeys(JsonObjectValue owner, string[] allowed, string path, List<StructuralDiagnostic> diagnostics)
        {
            foreach (var key in owner.Keys.Where(k => !allowed.Contains(k, StringComparer.Ordinal)).OrderBy(k => k, StringComparer.Ordinal))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapUnknownProperty, path + "." + key);
            }

            foreach (var key in allowed.Where(k => !owner.ContainsKey(k)))
            {
                Invalid(diagnostics, RamElementsDiagnosticCodes.MapMissingProperty, path + "." + key);
            }
        }

        private static void Invalid(List<StructuralDiagnostic> diagnostics, string code, string subject) =>
            diagnostics.Add(StructuralDiagnostic.Blocking(code, StructuralExportOutcome.Invalid, subject));

        private static RamSectionMapReadResult Result(List<StructuralDiagnostic> diagnostics, RamSectionMaterialTable table) =>
            new RamSectionMapReadResult(
                StructuralModelResult.Aggregate(diagnostics),
                new ReadOnlyCollection<StructuralDiagnostic>(diagnostics),
                table);
    }
}
