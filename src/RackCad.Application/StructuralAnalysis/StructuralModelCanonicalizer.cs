using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// The deterministic canonical form of a model (I-65 D-12, CD-I65-F1-10): ordinal order by id, invariant culture,
    /// round-trip doubles, no timestamps, UTF-8 without BOM and LF line endings.
    ///
    /// Lengths are in inches, forces in pounds-force, areas in in2 and inertias in in4; an absent section property is
    /// <c>null</c>, never 0. The form is written by hand, not by a serializer, so that nothing about the runtime, the
    /// culture or a library version can change a byte of it.
    /// </summary>
    public static class StructuralModelCanonicalizer
    {
        public const string SchemaId = "rackcad-structural-model/1";

        public static string ToCanonicalJson(StructuralModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var builder = new StringBuilder();
            WriteValue(builder, Describe(model), 0);
            builder.Append('\n');
            return builder.ToString();
        }

        public static byte[] ToCanonicalBytes(StructuralModel model) => new UTF8Encoding(false).GetBytes(ToCanonicalJson(model));

        // ---------------------------------------------------------------- the tree

        private static Obj Describe(StructuralModel model)
        {
            var metadata = model.Metadata;

            return new Obj
            {
                { "schema", SchemaId },
                {
                    "metadata", new Obj
                    {
                        {
                            "sourceRef", new Obj
                            {
                                { "rackId", metadata.SourceRef.RackId },
                                { "kind", metadata.SourceRef.Kind },
                                { "stationIndex", metadata.SourceRef.StationIndex },
                                { "sourceFingerprint", metadata.SourceRef.SourceFingerprint },
                            }
                        },
                        { "generatorVersion", metadata.GeneratorVersion },
                        { "loadFileSha256", metadata.LoadFileSha256 },
                        { "mappingRowsSha256", metadata.MappingRowsSha256 },
                        { "assumptionsSha256", metadata.AssumptionsSha256 },
                    }
                },
                {
                    "loadCase", new Obj
                    {
                        { "id", model.LoadCase.Id },
                        { "category", model.LoadCase.Category.ToString() },
                        { "includesSelfWeight", model.LoadCase.IncludesSelfWeight },
                    }
                },
                {
                    "nodes", ArrayOf(model.Nodes.OrderBy(n => n.Id, StringComparer.Ordinal).Select(n => (object)new Obj
                    {
                        { "id", n.Id },
                        { "xIn", n.Position.X.Inches },
                        { "yIn", n.Position.Y.Inches },
                        { "zIn", n.Position.Z.Inches },
                    }))
                },
                {
                    "members", ArrayOf(model.Members.OrderBy(m => m.Id, StringComparer.Ordinal).Select(m => (object)new Obj
                    {
                        { "id", m.Id },
                        { "nodeI", m.NodeIId },
                        { "nodeJ", m.NodeJId },
                        { "section", m.SectionRef.Value },
                        { "material", m.MaterialRef.Value },
                        { "peralteAxis", ArrayOf(new object[] { m.PeralteAxis.X, m.PeralteAxis.Y, m.PeralteAxis.Z }) },
                        {
                            "releases", new Obj
                            {
                                { "flags", ArrayOf(m.Releases.Flags.Select(f => (object)f)) },
                                { "assumptionId", m.Releases.AssumptionId },
                            }
                        },
                        { "role", m.Role.ToString() },
                        { "originPiece", m.OriginPiece },
                    }))
                },
                {
                    "sections", ArrayOf(model.Sections.OrderBy(s => s.Id.Value, StringComparer.Ordinal).Select(s => (object)new Obj
                    {
                        { "id", s.Id.Value },
                        { "area", s.Area.HasValue ? (object)s.Area.Value.SquareInches : null },
                        { "ix", s.Ix.HasValue ? (object)s.Ix.Value.InchesToTheFourth : null },
                        { "iy", s.Iy.HasValue ? (object)s.Iy.Value.InchesToTheFourth : null },
                        { "j", s.J.HasValue ? (object)s.J.Value.InchesToTheFourth : null },
                    }))
                },
                {
                    "materials", ArrayOf(model.Materials.OrderBy(m => m.Id.Value, StringComparer.Ordinal).Select(m => (object)new Obj
                    {
                        { "id", m.Id.Value },
                    }))
                },
                {
                    "supports", ArrayOf(model.Supports.OrderBy(s => s.NodeId, StringComparer.Ordinal).Select(s => (object)new Obj
                    {
                        { "nodeId", s.NodeId },
                        { "flags", ArrayOf(s.Flags.Select(f => (object)f)) },
                        { "assumptionId", s.AssumptionId },
                    }))
                },
                {
                    "loads", ArrayOf(model.Loads.OrderBy(l => l.Id, StringComparer.Ordinal).Select(l => (object)new Obj
                    {
                        { "id", l.Id },
                        { "caseId", model.LoadCase.Id },
                        { "memberId", l.MemberId },
                        { "direction", "-Z" },
                        { "magnitudeLbf", l.Magnitude.PoundsForce },
                        { "distanceFromNodeIIn", l.DistanceFromNodeI.Inches },
                    }))
                },
                {
                    "assumptions", ArrayOf(model.Assumptions.OrderBy(a => a.Id, StringComparer.Ordinal).Select(a => (object)new Obj
                    {
                        { "id", a.Id },
                        { "statement", a.Statement },
                    }))
                },
                {
                    "diagnostics", ArrayOf(model.Diagnostics
                        .OrderBy(d => d.Code, StringComparer.Ordinal)
                        .ThenBy(d => d.Subject, StringComparer.Ordinal)
                        .Select(d => (object)new Obj
                        {
                            { "code", d.Code },
                            { "severity", d.Severity.ToString() },
                            { "subject", d.Subject },
                        }))
                },
            };
        }

        private static Arr ArrayOf(IEnumerable<object> items) => new Arr(items);

        // ---------------------------------------------------------------- the writer

        private sealed class Obj : List<KeyValuePair<string, object>>
        {
            public void Add(string key, object value) => Add(new KeyValuePair<string, object>(key, value));
        }

        private sealed class Arr : List<object>
        {
            public Arr(IEnumerable<object> items)
                : base(items)
            {
            }
        }

        private static void WriteValue(StringBuilder builder, object value, int depth)
        {
            switch (value)
            {
                case null:
                    builder.Append("null");
                    break;

                case string text:
                    WriteString(builder, text);
                    break;

                case bool flag:
                    builder.Append(flag ? "true" : "false");
                    break;

                case int number:
                    builder.Append(number.ToString(CultureInfo.InvariantCulture));
                    break;

                case double number:
                    builder.Append(FormatDouble(number));
                    break;

                case Obj obj:
                    WriteObject(builder, obj, depth);
                    break;

                case Arr arr:
                    WriteArray(builder, arr, depth);
                    break;

                default:
                    throw new InvalidOperationException("Unexpected canonical value " + value.GetType().Name + ".");
            }
        }

        private static void WriteObject(StringBuilder builder, Obj obj, int depth)
        {
            builder.Append("{\n");

            for (var index = 0; index < obj.Count; index++)
            {
                Indent(builder, depth + 1);
                WriteString(builder, obj[index].Key);
                builder.Append(": ");
                WriteValue(builder, obj[index].Value, depth + 1);
                builder.Append(index < obj.Count - 1 ? ",\n" : "\n");
            }

            Indent(builder, depth);
            builder.Append('}');
        }

        private static void WriteArray(StringBuilder builder, Arr arr, int depth)
        {
            if (arr.Count == 0)
            {
                builder.Append("[]");
                return;
            }

            if (arr.All(item => !(item is Obj) && !(item is Arr)))
            {
                builder.Append('[');

                for (var index = 0; index < arr.Count; index++)
                {
                    WriteValue(builder, arr[index], depth);
                    builder.Append(index < arr.Count - 1 ? ", " : string.Empty);
                }

                builder.Append(']');
                return;
            }

            builder.Append("[\n");

            for (var index = 0; index < arr.Count; index++)
            {
                Indent(builder, depth + 1);
                WriteValue(builder, arr[index], depth + 1);
                builder.Append(index < arr.Count - 1 ? ",\n" : "\n");
            }

            Indent(builder, depth);
            builder.Append(']');
        }

        private static void Indent(StringBuilder builder, int depth) => builder.Append(' ', depth * 2);

        /// <summary>Round-trip, invariant culture, and one spelling for zero (negative zero included).</summary>
        private static string FormatDouble(double value) =>
            value == 0 ? "0" : value.ToString("R", CultureInfo.InvariantCulture);

        private static void WriteString(StringBuilder builder, string text)
        {
            builder.Append('"');

            foreach (var character in text)
            {
                switch (character)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (character < 0x20 || character > 0x7E)
                        {
                            builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            builder.Append('"');
        }
    }
}
