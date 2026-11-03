using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// A deterministic JSON writer: the same calls always give the same characters, whatever the culture or the platform. Numbers are
    /// written with the round-trip format of the invariant culture; with <c>asciiOnly</c> every character above U+007E is written as
    /// a <c>\uXXXX</c> escape.
    /// </summary>
    internal sealed class RamJsonWriter
    {
        private sealed class Frame
        {
            public int Count;
        }

        private readonly StringBuilder _text = new StringBuilder();
        private readonly Stack<Frame> _stack = new Stack<Frame>();
        private readonly bool _asciiOnly;
        private readonly bool _indented;
        private bool _afterName;

        public RamJsonWriter(bool asciiOnly, bool indented)
        {
            _asciiOnly = asciiOnly;
            _indented = indented;
        }

        public RamJsonWriter BeginObject()
        {
            BeforeValue();
            _text.Append('{');
            _stack.Push(new Frame());
            return this;
        }

        public RamJsonWriter EndObject() => End('}');

        public RamJsonWriter BeginArray()
        {
            BeforeValue();
            _text.Append('[');
            _stack.Push(new Frame());
            return this;
        }

        public RamJsonWriter EndArray() => End(']');

        public RamJsonWriter Name(string name)
        {
            var frame = _stack.Peek();

            if (frame.Count > 0)
            {
                _text.Append(',');
            }

            frame.Count++;
            NewLine(_stack.Count);
            Quote(name);
            _text.Append(':');

            if (_indented)
            {
                _text.Append(' ');
            }

            _afterName = true;
            return this;
        }

        public RamJsonWriter String(string value)
        {
            BeforeValue();

            if (value == null)
            {
                _text.Append("null");
            }
            else
            {
                Quote(value);
            }

            return this;
        }

        public RamJsonWriter Number(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "A JSON number must be finite.");
            }

            BeforeValue();

            // + 0.0 turns a negative zero into zero, so that the same value is always written the same way.
            _text.Append((value + 0.0).ToString("R", CultureInfo.InvariantCulture));
            return this;
        }

        public RamJsonWriter Integer(long value)
        {
            BeforeValue();
            _text.Append(value.ToString(CultureInfo.InvariantCulture));
            return this;
        }

        public RamJsonWriter Bool(bool value)
        {
            BeforeValue();
            _text.Append(value ? "true" : "false");
            return this;
        }

        public RamJsonWriter Null()
        {
            BeforeValue();
            _text.Append("null");
            return this;
        }

        public RamJsonWriter Property(string name, string value) => Name(name).String(value);

        public RamJsonWriter Property(string name, double value) => Name(name).Number(value);

        public RamJsonWriter Property(string name, bool value) => Name(name).Bool(value);

        public RamJsonWriter NumberArray(IEnumerable<double> values)
        {
            BeginArray();

            foreach (var value in values)
            {
                Number(value);
            }

            return EndArray();
        }

        public RamJsonWriter BoolArray(IEnumerable<bool> values)
        {
            BeginArray();

            foreach (var value in values)
            {
                Bool(value);
            }

            return EndArray();
        }

        public RamJsonWriter StringArray(IEnumerable<string> values)
        {
            BeginArray();

            foreach (var value in values)
            {
                String(value);
            }

            return EndArray();
        }

        public override string ToString() => _text.ToString();

        private RamJsonWriter End(char closing)
        {
            var frame = _stack.Pop();

            if (frame.Count > 0)
            {
                NewLine(_stack.Count);
            }

            _text.Append(closing);
            return this;
        }

        private void BeforeValue()
        {
            if (_afterName)
            {
                _afterName = false;
                return;
            }

            if (_stack.Count == 0)
            {
                return;
            }

            var frame = _stack.Peek();

            if (frame.Count > 0)
            {
                _text.Append(',');
            }

            frame.Count++;
            NewLine(_stack.Count);
        }

        private void NewLine(int depth)
        {
            if (!_indented)
            {
                return;
            }

            _text.Append('\n');
            _text.Append(' ', depth * 2);
        }

        private void Quote(string value)
        {
            _text.Append('"');

            foreach (var character in value)
            {
                switch (character)
                {
                    case '"':
                        _text.Append("\\\"");
                        break;
                    case '\\':
                        _text.Append("\\\\");
                        break;
                    case '\n':
                        _text.Append("\\n");
                        break;
                    case '\r':
                        _text.Append("\\r");
                        break;
                    case '\t':
                        _text.Append("\\t");
                        break;
                    default:
                        if (character < 0x20 || (_asciiOnly && character > 0x7E))
                        {
                            _text.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            _text.Append(character);
                        }

                        break;
                }
            }

            _text.Append('"');
        }
    }

    /// <summary>
    /// The payload of the build script (I-65 D-09 rule 2, CD-I65-F3-08): compact JSON in plain ASCII, schema
    /// <c>rackcad-ram-build-payload/v1</c>. It carries everything the script writes, checks and reads back; the script holds no
    /// number of its own. Pure: the same plan, version, folder and fingerprint always give the same characters.
    /// </summary>
    public static class RamBuildPayload
    {
        public const string Schema = "rackcad-ram-build-payload/v1";

        /// <summary>The version of the package of the analysis program that the capability gate requires.</summary>
        public const string RequiredPackageVersion = "0.1.0";

        /// <summary>The tolerance of the read-back of lengths and values: |d| &lt;= tolerance * max(1, |v|).</summary>
        public const double RelativeTolerance = 1e-9;

        /// <summary>The tolerance of the read-back of the position of a load, in percent.</summary>
        public const double PercentTolerance = 1e-9;

        /// <summary>The tolerance of the read-back of the components of the local axes.</summary>
        public const double AxesTolerance = 1e-9;

        /// <summary>
        /// The functions of the core of the package that the script uses, in ordinal order; the capability gate demands all of them.
        /// Every one is among the 207 functions measured in P1.
        /// </summary>
        public static IReadOnlyList<string> CoreFunctions { get; } = new ReadOnlyCollection<string>(
            new[]
            {
                "lc_add_case",
                "lc_count",
                "lc_get_id",
                "lc_get_case_category",
                "lc_get_self_weight",
                "lc_is_combo",
                "members_add",
                "members_add_point_load",
                "members_count",
                "members_get_axial_behavior",
                "members_get_cardinal_point",
                "members_get_description",
                "members_get_local_axes",
                "members_get_material",
                "members_get_point_load",
                "members_get_point_load_count",
                "members_get_releases",
                "members_get_rigid_end_offset",
                "members_get_rotation",
                "members_get_rotational_stiffness",
                "members_get_section",
                "members_get_stiffness_factors",
                "members_set_cardinal_point",
                "members_set_description",
                "members_set_material",
                "members_set_rotation",
                "members_set_section",
                "nodes_add",
                "nodes_count",
                "nodes_get_coordinates",
                "nodes_get_restraints",
                "nodes_set_description",
                "nodes_set_restraints",
                "units_get",
                "units_set",
            }.OrderBy(name => name, StringComparer.Ordinal).ToList());

        public static string Create(RamBuildPlan plan, string adapterVersion, string packageFolder, string analysisInputFingerprint)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var writer = new RamJsonWriter(asciiOnly: true, indented: false);

            writer.BeginObject();
            writer.Property("schema", Schema);
            writer.Property("adapterVersion", adapterVersion);
            writer.Property("units", plan.Units);
            writer.Property("packageFolder", packageFolder);
            writer.Property("analysisInputFingerprint", analysisInputFingerprint);

            writer.Name("loadCase").BeginObject();
            writer.Property("id", plan.LoadCaseId);
            writer.Property("category", plan.LoadCaseCategory);
            writer.EndObject();

            writer.Name("nodes").BeginArray();

            foreach (var node in plan.Nodes)
            {
                writer.BeginObject();
                writer.Property("id", node.Id);
                writer.Property("x", node.Position.X);
                writer.Property("y", node.Position.Y);
                writer.Property("z", node.Position.Z);
                writer.Name("restraints").BoolArray(new[]
                {
                    node.Restraints.Dx, node.Restraints.Dy, node.Restraints.Dz, node.Restraints.Rx, node.Restraints.Ry, node.Restraints.Rz,
                });
                writer.EndObject();
            }

            writer.EndArray();

            writer.Name("members").BeginArray();

            foreach (var member in plan.Members)
            {
                writer.BeginObject();
                writer.Property("id", member.Id);
                writer.Property("nodeI", member.NodeIId);
                writer.Property("nodeJ", member.NodeJId);
                writer.Property("section", member.SectionName);
                writer.Property("material", member.MaterialName);
                writer.Property("rotation", member.ThetaDegrees);
                writer.Name("l1").NumberArray(Components(member.LocalAxes.L1));
                writer.Name("l2").NumberArray(Components(member.LocalAxes.L2));
                writer.Name("l3").NumberArray(Components(member.LocalAxes.L3));
                writer.EndObject();
            }

            writer.EndArray();

            writer.Name("loads").BeginArray();

            foreach (var load in plan.Loads)
            {
                writer.BeginObject();
                writer.Property("id", load.Id);
                writer.Property("member", load.MemberId);
                writer.Property("direction", load.Direction);
                writer.Property("value", load.ValuePoundsForce);
                writer.Property("percent", load.PositionPercent);
                writer.EndObject();
            }

            writer.EndArray();

            writer.Name("tolerances").BeginObject();
            writer.Property("relative", RelativeTolerance);
            writer.Property("percent", PercentTolerance);
            writer.Property("axes", AxesTolerance);
            writer.EndObject();

            writer.Name("capabilities").BeginObject();
            writer.Property("version", RequiredPackageVersion);
            writer.Name("coreFunctions").StringArray(CoreFunctions);
            writer.EndObject();

            writer.EndObject();

            return writer.ToString();
        }

        private static IEnumerable<double> Components(RamVector vector) => new[] { vector.X, vector.Y, vector.Z };
    }
}
