using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.StructuralAnalysis;

namespace RackCad.Tests
{
    /// <summary>
    /// The inputs of the I-65 F4-T4 comparator tests: synthetic neutral readbacks built in the test (never read from a program), so each case
    /// changes ONE thing on a fresh copy. The values are fixture values, not engineering values.
    /// </summary>
    internal sealed class StructuralAnalysisComparisonModel
    {
        public const string Fingerprint = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        public const string OtherFingerprint = "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210";
        public const string AdapterVersion = "1.0.0-fixture";

        public sealed class NodeSpec
        {
            public string Id;
            public double X;
            public double Y;
            public double Z;
            public bool[] Flags = new bool[6];
        }

        public sealed class MemberSpec
        {
            public string Id;
            public string NodeI;
            public string NodeJ;
            public string Section = "W 12X26";
            public string Material = "A992 Gr50";
            public double[] L1;
            public double[] L2;
            public double[] L3;
            public int CardinalPoint;
            public double[] OffsetI = { 0, 0, 0 };
            public double[] OffsetJ = { 0, 0, 0 };
            public double[] Factors = { 1, 1, 1, 1 };
            public StructuralReadbackAxialBehavior Axial = StructuralReadbackAxialBehavior.Normal;
            public double SpringI = 0;
            public double SpringJ;
            public bool[] Releases = new bool[10];
        }

        public sealed class LoadSpec
        {
            public string MemberId;
            public StructuralReadbackLoadAxis Axis = StructuralReadbackLoadAxis.GlobalZ;
            public double Value;
            public double Distance;
        }

        public List<NodeSpec> Nodes = new List<NodeSpec>();
        public List<MemberSpec> Members = new List<MemberSpec>();
        public List<LoadSpec> Loads = new List<LoadSpec>();
        public string CaseId = "REF1";
        public string Category = "OTHER";
        public double[] SelfWeight = { 0, 0, 0 };
        public List<string> EmptyCases = new List<string> { "CM" };
        public List<(string Id, (string Case, double Factor)[] Terms)> Combinations = new List<(string, (string, double)[])>();

        /// <summary>
        /// A column of 100 in from a fixed base and an arm of 50 in from its top, both along global axes; one load of -1000 lbf along Z at
        /// 25 in on the arm. <paramref name="prefix"/> renames the identities and <paramref name="reverse"/> reverses the order of the lists,
        /// so that two copies agree only through the geometry.
        /// </summary>
        public static StructuralAnalysisComparisonModel Reference(string prefix = "", bool reverse = false)
        {
            var model = new StructuralAnalysisComparisonModel();

            model.Nodes.Add(new StructuralAnalysisComparisonModel.NodeSpec { Id = prefix + "1", X = 0, Y = 0, Z = 0, Flags = new[] { true, true, true, true, true, true } });
            model.Nodes.Add(new StructuralAnalysisComparisonModel.NodeSpec { Id = prefix + "2", X = 0, Y = 0, Z = 100 });
            model.Nodes.Add(new StructuralAnalysisComparisonModel.NodeSpec { Id = prefix + "3", X = 50, Y = 0, Z = 100 });

            model.Members.Add(new MemberSpec
            {
                Id = prefix + "C1",
                NodeI = prefix + "1",
                NodeJ = prefix + "2",
                L1 = new double[] { 0, 0, 1 },
                L2 = new double[] { 1, 0, 0 },
                L3 = new double[] { 0, 1, 0 },
            });
            model.Members.Add(new MemberSpec
            {
                Id = prefix + "A1",
                NodeI = prefix + "2",
                NodeJ = prefix + "3",
                Section = "HSS 4X4X1/4",
                L1 = new double[] { 1, 0, 0 },
                L2 = new double[] { 0, 1, 0 },
                L3 = new double[] { 0, 0, 1 },
            });

            model.Loads.Add(new LoadSpec { MemberId = prefix + "A1", Axis = StructuralReadbackLoadAxis.GlobalZ, Value = -1000, Distance = 25 });

            if (reverse)
            {
                model.Nodes.Reverse();
                model.Members.Reverse();
            }

            return model;
        }

        public NodeSpec Node(string id) => Nodes.Single(n => n.Id == id);

        public MemberSpec Member(string id) => Members.Single(m => m.Id == id);

        public StructuralModelReadback Build()
        {
            var nodes = Nodes.Select(n => new StructuralReadbackNode(n.Id, n.Id, Position(n.X, n.Y, n.Z), n.Flags)).ToList();

            var members = Members.Select(m => new StructuralReadbackMember(
                m.Id,
                m.Id,
                m.NodeI,
                m.NodeJ,
                m.Section,
                m.Material,
                0,
                Direction(m.L1),
                Direction(m.L2),
                Direction(m.L3),
                m.CardinalPoint,
                Position(m.OffsetI[0], m.OffsetI[1], m.OffsetI[2]),
                Position(m.OffsetJ[0], m.OffsetJ[1], m.OffsetJ[2]),
                m.Factors,
                m.Axial,
                m.SpringI,
                m.SpringJ,
                m.Releases)).ToList();

            var cases = new List<StructuralReadbackLoadCase>
            {
                new StructuralReadbackLoadCase(
                    CaseId,
                    Category,
                    Direction(SelfWeight),
                    Loads.Select(l => new StructuralReadbackPointLoad(
                        l.MemberId, l.Axis, StructuralForce.FromPoundsForce(l.Value), StructuralLength.FromInches(l.Distance)))),
            };

            cases.AddRange(EmptyCases.Select(id => new StructuralReadbackLoadCase(id, "DEAD", Direction(new double[] { 0, 0, 0 }), new StructuralReadbackPointLoad[0])));

            var combinations = Combinations.Select(c => new StructuralReadbackCombination(
                c.Id, c.Terms.Select(t => new StructuralReadbackCombinationTerm(t.Case, t.Factor)))).ToList();

            return new StructuralModelReadback(AdapterVersion, "LB_IN", false, nodes, members, cases, combinations);
        }

        public static StructuralPosition Position(double x, double y, double z) =>
            new StructuralPosition(StructuralLength.FromInches(x), StructuralLength.FromInches(y), StructuralLength.FromInches(z));

        public static StructuralDirection Direction(double[] v) => new StructuralDirection(v[0], v[1], v[2]);
    }

    /// <summary>One thing the values of a read-back can get wrong against the dump of the generated model (A65-F4-03).</summary>
    public enum StructuralAnalysisValuesMismatch
    {
        None = 0,
        NodeCount,
        NodeId,
        NodeCoordinate,
        NodeSupport,
        MemberCount,
        MemberId,
        MemberSection,
        MemberMaterial,
        LoadCount,
        LoadValue,
        LoadDistance,
        LoadDirection,
        LoadCaseId,
        LoadCaseCategory,
        NoLoadCase,
    }

    internal static class StructuralAnalysisComparisonSupport
    {
        /// <summary>
        /// An approved read-back whose values agree with the sample dump of the T3 tests (the one the tool tests and one row test read).
        /// For the models of this support use <see cref="Approved(StructuralModelReadback, StructuralAnalysisValuesMismatch)"/>.
        /// </summary>
        public static StructuralTransferReadback Approved() =>
            Approved(StructuralAnalysisRamElementsDumpSupport.Read(StructuralAnalysisRamElementsDumpSupport.Valid()).Readback);

        /// <summary>An approved read-back whose values are what the build would have read from the model of <paramref name="generated"/>, or a variation.</summary>
        public static StructuralTransferReadback Approved(
            StructuralModelReadback generated, StructuralAnalysisValuesMismatch mismatch = StructuralAnalysisValuesMismatch.None) =>
            new StructuralTransferReadback(
                true,
                StructuralAnalysisComparisonModel.Fingerprint,
                StructuralAnalysisComparisonModel.AdapterVersion,
                new string[0],
                ValuesOf(generated, mismatch));

        /// <summary>An approved read-back that carries no values at all (the constructor without values).</summary>
        public static StructuralTransferReadback ApprovedWithoutValues() =>
            new StructuralTransferReadback(true, StructuralAnalysisComparisonModel.Fingerprint, StructuralAnalysisComparisonModel.AdapterVersion, new string[0]);

        public static StructuralTransferReadback Failed(params string[] differences) =>
            new StructuralTransferReadback(false, StructuralAnalysisComparisonModel.Fingerprint, StructuralAnalysisComparisonModel.AdapterVersion, differences);

        /// <summary>The values the build would have read back from the model of the dump <paramref name="generated"/>, with at most ONE variation.</summary>
        public static StructuralTransferValues ValuesOf(
            StructuralModelReadback generated, StructuralAnalysisValuesMismatch mismatch = StructuralAnalysisValuesMismatch.None)
        {
            var nodes = generated.Nodes.Select(n => new StructuralTransferNodeValue(
                mismatch == StructuralAnalysisValuesMismatch.NodeId ? n.Description + "-x" : n.Description,
                n.Id,
                mismatch == StructuralAnalysisValuesMismatch.NodeCoordinate
                    ? StructuralAnalysisComparisonModel.Position(n.Position.X.Inches + 0.001, n.Position.Y.Inches, n.Position.Z.Inches)
                    : n.Position,
                mismatch == StructuralAnalysisValuesMismatch.NodeSupport ? n.SupportFlags.Select(f => !f).ToArray() : n.SupportFlags.ToArray())).ToList();

            var members = generated.Members.Select(m => new StructuralTransferMemberValue(
                mismatch == StructuralAnalysisValuesMismatch.MemberId ? m.Description + "-x" : m.Description,
                m.Id,
                mismatch == StructuralAnalysisValuesMismatch.MemberSection ? m.SectionName + " X" : m.SectionName,
                mismatch == StructuralAnalysisValuesMismatch.MemberMaterial ? m.MaterialName + " X" : m.MaterialName)).ToList();

            var loadCase = generated.LoadCases.FirstOrDefault(c => c.Loads.Count > 0);
            var loads = new List<StructuralTransferLoadValue>();

            if (loadCase != null)
            {
                foreach (var group in loadCase.Loads.GroupBy(l => l.MemberId, StringComparer.Ordinal))
                {
                    var member = generated.Members.Single(m => m.Id == group.Key);
                    var index = 0;

                    foreach (var load in group)
                    {
                        loads.Add(new StructuralTransferLoadValue(
                            member.Description,
                            index++,
                            mismatch == StructuralAnalysisValuesMismatch.LoadDirection ? StructuralReadbackLoadAxis.Local3 : load.Axis,
                            StructuralForce.FromPoundsForce(load.Value.PoundsForce + (mismatch == StructuralAnalysisValuesMismatch.LoadValue ? 0.5 : 0.0)),
                            load.DistanceFromNodeI.Inches + (mismatch == StructuralAnalysisValuesMismatch.LoadDistance ? 0.01 : 0.0),
                            false));
                    }
                }
            }

            switch (mismatch)
            {
                case StructuralAnalysisValuesMismatch.NodeCount:
                    nodes.RemoveAt(nodes.Count - 1);
                    break;
                case StructuralAnalysisValuesMismatch.MemberCount:
                    members.RemoveAt(members.Count - 1);
                    break;
                case StructuralAnalysisValuesMismatch.LoadCount:
                    loads.RemoveAt(loads.Count - 1);
                    break;
            }

            var caseValue = loadCase == null || mismatch == StructuralAnalysisValuesMismatch.NoLoadCase
                ? null
                : new StructuralTransferCaseValue(
                    mismatch == StructuralAnalysisValuesMismatch.LoadCaseId ? loadCase.Id + "-x" : loadCase.Id,
                    mismatch == StructuralAnalysisValuesMismatch.LoadCaseCategory ? loadCase.Category + "-x" : loadCase.Category);

            return new StructuralTransferValues(nodes, members, loads, caseValue);
        }

        /// <summary>Compares a generated model with a manual one; both default to the reference, the manual one with other identities and order.</summary>
        /// <remarks>Without a <paramref name="transfer"/> the read-back is an approved one whose values agree with the generated model (or vary by <paramref name="mismatch"/>).</remarks>
        public static StructuralComparisonReport Compare(
            StructuralAnalysisComparisonModel generated = null,
            StructuralAnalysisComparisonModel manual = null,
            StructuralTransferReadback transfer = null,
            string fingerprint = StructuralAnalysisComparisonModel.Fingerprint,
            StructuralComparisonOptions options = null,
            StructuralAnalysisValuesMismatch mismatch = StructuralAnalysisValuesMismatch.None)
        {
            generated = generated ?? StructuralAnalysisComparisonModel.Reference();
            manual = manual ?? StructuralAnalysisComparisonModel.Reference("M", reverse: true);

            var generatedReadback = generated.Build();

            return StructuralReadbackComparison.CompareReadbacks(
                generatedReadback, manual.Build(), transfer ?? Approved(generatedReadback, mismatch), fingerprint, options);
        }

        public static StructuralComparisonVerdict VerdictOf(StructuralComparisonReport report, string rowId) => report.Row(rowId).Verdict;

        public static string Describe(StructuralComparisonReport report) =>
            report.Outcome + ": " + string.Join("; ", report.Rows.Select(r => r.Id + "=" + r.Verdict));

        /// <summary>The paths at which two JSON documents differ (a leaf, or an object property or array that is on one side only).</summary>
        public static IReadOnlyList<string> Differences(JsonElement left, JsonElement right, string path = "$")
        {
            var found = new List<string>();
            Walk(left, right, path, found);
            return found;
        }

        private static void Walk(JsonElement left, JsonElement right, string path, List<string> found)
        {
            if (left.ValueKind != right.ValueKind)
            {
                found.Add(path);
                return;
            }

            switch (left.ValueKind)
            {
                case JsonValueKind.Object:
                    var names = left.EnumerateObject().Select(p => p.Name).Union(right.EnumerateObject().Select(p => p.Name)).ToList();

                    foreach (var name in names)
                    {
                        var hasLeft = left.TryGetProperty(name, out var l);
                        var hasRight = right.TryGetProperty(name, out var r);

                        if (hasLeft && hasRight)
                        {
                            Walk(l, r, path + "." + name, found);
                        }
                        else
                        {
                            found.Add(path + "." + name);
                        }
                    }

                    break;
                case JsonValueKind.Array:
                    var count = Math.Max(left.GetArrayLength(), right.GetArrayLength());

                    if (left.GetArrayLength() != right.GetArrayLength() && left.EnumerateArray().Select(e => e.ValueKind).All(k => k == JsonValueKind.String))
                    {
                        found.Add(path);
                        break;
                    }

                    for (var index = 0; index < count; index++)
                    {
                        if (index >= left.GetArrayLength() || index >= right.GetArrayLength())
                        {
                            found.Add(path + "[" + index + "]");
                        }
                        else
                        {
                            Walk(left[index], right[index], path + "[" + index + "]", found);
                        }
                    }

                    break;
                case JsonValueKind.String:
                    if (!string.Equals(left.GetString(), right.GetString(), StringComparison.Ordinal))
                    {
                        found.Add(path);
                    }

                    break;
                default:
                    if (left.GetRawText() != right.GetRawText())
                    {
                        found.Add(path);
                    }

                    break;
            }
        }
    }
}
