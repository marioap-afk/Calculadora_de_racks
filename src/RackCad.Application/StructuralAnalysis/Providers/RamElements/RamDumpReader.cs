using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The result of reading a dump: the readback when, and only when, the outcome is Valid. There is never a partial readback.</summary>
    public sealed class RamDumpReadResult
    {
        internal RamDumpReadResult(StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, StructuralModelReadback readback)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            Readback = readback;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        /// <summary>The model as read when, and only when, <see cref="Outcome"/> is Valid; otherwise <c>null</c>.</summary>
        public StructuralModelReadback Readback { get; }
    }

    /// <summary>
    /// The strict reader of <c>rackcad-ram-dump/v1</c> (I-65 D-11, A65-F4P-02). It receives TEXT: it does no I/O.
    ///
    /// <para>
    /// The shape is EXACT. A duplicate or unknown property, a wrong type, a bad count or an id that repeats or points nowhere makes the dump
    /// Invalid, and every finding is reported. A status other than <c>DUMP OK</c> (a failure, the minimal record of a failure included) is
    /// rejected under its own code, whatever else the file says. A dump that was not read in the pair LB_IN is Invalid under the code of the
    /// unit pair.
    /// </para>
    /// <para>
    /// What a valid dump gives is the neutral readback: points and vectors go back to the neutral frame with the inverse of D-04
    /// (X = X of the program, Y = minus Z of the program, Z = Y of the program), the support flags to the order Tx Ty Tz Rx Ry Rz
    /// (Tx = dx, Ty = dz, Tz = dy, Rx = rx, Ry = rz, Rz = ry), lengths and forces become typed inches and pounds-force, and the position of
    /// every load becomes the absolute distance from node I (a percentage is the percentage of the length of the member).
    /// </para>
    /// </summary>
    public static class RamDumpReader
    {
        public const string Schema = "rackcad-ram-dump/v1";

        private const string UnitPair = "LB_IN";

        private static readonly RamJsonCodes JsonCodes = new RamJsonCodes(
            RamDumpDiagnosticCodes.DumpJsonMalformed,
            RamDumpDiagnosticCodes.DumpSchemaMismatch,
            RamDumpDiagnosticCodes.DumpDuplicateProperty,
            RamDumpDiagnosticCodes.DumpUnknownProperty,
            RamDumpDiagnosticCodes.DumpMissingProperty,
            RamDumpDiagnosticCodes.DumpWrongType,
            RamDumpDiagnosticCodes.DumpValueInvalid);

        private static readonly string[] RootKeys =
        {
            "schema", "status", "adapterVersion", "initialUnits", "readUnits", "unitsRestored", "nodes", "members", "loadCases", "combinations", "errors",
        };

        private static readonly string[] NodeKeys = { "id", "description", "coordinates", "restraints" };

        private static readonly string[] MemberKeys =
        {
            "id", "description", "nodeI", "nodeJ", "section", "material", "rotation", "l1", "l2", "l3", "cardinalPoint", "offsetI", "offsetJ",
            "stiffnessFactors", "axialBehavior", "springI", "springJ", "releases",
        };

        private static readonly string[] CaseKeys = { "id", "category", "selfWeight", "memberLoads" };

        private static readonly string[] MemberLoadKeys = { "member", "loads" };

        private static readonly string[] LoadKeys = { "direction", "value", "distance", "isPercentage" };

        private static readonly string[] CombinationKeys = { "id", "terms" };

        private static readonly string[] TermKeys = { "case", "factor" };

        // ---------------------------------------------------------------- the dump before conversion

        private sealed class NodeRow
        {
            public int Id;
            public string Description;
            public double[] Coordinates;
            public bool[] Restraints;
        }

        private sealed class MemberRow
        {
            public int Id;
            public string Description;
            public int NodeI;
            public int NodeJ;
            public string Section;
            public string Material;
            public double Rotation;
            public double[] L1;
            public double[] L2;
            public double[] L3;
            public int CardinalPoint;
            public double[] OffsetI;
            public double[] OffsetJ;
            public double[] Factors;
            public int Axial;
            public double SpringI;
            public double SpringJ;
            public bool[] Releases;
        }

        private sealed class LoadRow
        {
            public int Member;
            public string Direction;
            public double Value;
            public double Distance;
            public bool IsPercentage;
        }

        private sealed class CaseRow
        {
            public string Id;
            public string Category;
            public double[] SelfWeight;
            public List<LoadRow> Loads = new List<LoadRow>();
        }

        private sealed class CombinationRow
        {
            public string Id;
            public List<(string Case, double Factor)> Terms = new List<(string Case, double Factor)>();
        }

        public static RamDumpReadResult Read(string json)
        {
            var scan = new RamJsonScan(JsonCodes);
            var parsed = scan.Parse(json);

            if (parsed == null)
            {
                return Result(scan, null);
            }

            var root = scan.Object(parsed, "$");

            if (root == null)
            {
                return Result(scan, null);
            }

            // the schema and the status first: the record of a failure may carry nothing else
            var schema = scan.String(root, "schema", "$");

            if (schema != null && !string.Equals(schema, Schema, StringComparison.Ordinal))
            {
                scan.Invalid(JsonCodes.Schema, "$.schema");
            }

            var status = scan.String(root, "status", "$");

            if (status != null && !string.Equals(status, RamDumpScriptTemplate.StatusOk, StringComparison.Ordinal))
            {
                scan.Invalid(RamDumpDiagnosticCodes.DumpFailed, "$.status");
                return Result(scan, null);
            }

            scan.Keys(root, "$", RootKeys);

            var adapterVersion = scan.String(root, "adapterVersion", "$", nonBlank: true);
            var initialUnits = scan.String(root, "initialUnits", "$");
            var readUnits = scan.String(root, "readUnits", "$");
            var restored = scan.Bool(root, "unitsRestored", "$");
            var errors = scan.Strings(root, "errors", "$");

            if (errors != null && errors.Count > 0)
            {
                scan.Invalid(JsonCodes.Value, "$.errors");
            }

            if (readUnits != null && !string.Equals(readUnits, UnitPair, StringComparison.Ordinal))
            {
                scan.Invalid(RamDumpDiagnosticCodes.DumpUnitPair, "$.readUnits");
            }
            else if (readUnits != null && initialUnits != null && restored.HasValue
                     && restored.Value != !string.Equals(initialUnits, UnitPair, StringComparison.Ordinal))
            {
                scan.Invalid(RamDumpDiagnosticCodes.DumpUnitsInconsistent, "$.unitsRestored");
            }

            var nodes = ReadNodes(scan, root);
            var members = ReadMembers(scan, root, nodes);
            var cases = new List<CaseRow>();
            var combinations = new List<CombinationRow>();
            ReadCases(scan, root, members, cases, combinations);
            CheckGeometry(scan, nodes, members, cases);

            if (scan.HasFindings || adapterVersion == null || initialUnits == null || !restored.HasValue)
            {
                return Result(scan, null);
            }

            try
            {
                return Result(scan, ToReadback(adapterVersion, initialUnits, restored.Value, nodes, members, cases, combinations));
            }
            catch (ArgumentException)
            {
                // a name that the validation lets through but the neutral type refuses: still a finding, never an exception
                scan.Invalid(JsonCodes.Value, "$");
                return Result(scan, null);
            }
        }

        private static RamDumpReadResult Result(RamJsonScan scan, StructuralModelReadback readback)
        {
            var outcome = StructuralModelResult.Aggregate(scan.Diagnostics);

            return new RamDumpReadResult(
                outcome,
                new ReadOnlyCollection<StructuralDiagnostic>(scan.Diagnostics),
                outcome == StructuralExportOutcome.Valid ? readback : null);
        }

        // ---------------------------------------------------------------- the shape

        private static string At(string path, int index) => path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]";

        private static List<NodeRow> ReadNodes(RamJsonScan scan, RamJsonObject root)
        {
            var rows = new List<NodeRow>();
            var items = scan.Array(root, "nodes", "$");

            if (items == null)
            {
                return rows;
            }

            var ids = new HashSet<int>();

            for (var index = 0; index < items.Count; index++)
            {
                var path = At("$.nodes", index);
                var node = scan.Object(items[index], path);

                if (node == null)
                {
                    continue;
                }

                scan.Keys(node, path, NodeKeys);

                var id = scan.Integer(node, "id", path, 0, int.MaxValue);
                var description = scan.String(node, "description", path);
                var coordinates = scan.Numbers(node, "coordinates", path, 3);
                var restraints = scan.Bools(node, "restraints", path, StructuralSupport.FlagCount);

                if (id.HasValue && !ids.Add(id.Value))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpIdDuplicate, path + ".id");
                }

                if (id.HasValue && description != null && coordinates != null && restraints != null)
                {
                    rows.Add(new NodeRow { Id = id.Value, Description = description, Coordinates = coordinates, Restraints = restraints });
                }
            }

            return rows;
        }

        private static List<MemberRow> ReadMembers(RamJsonScan scan, RamJsonObject root, List<NodeRow> nodes)
        {
            var rows = new List<MemberRow>();
            var items = scan.Array(root, "members", "$");

            if (items == null)
            {
                return rows;
            }

            var nodeIds = new HashSet<int>(nodes.Select(n => n.Id));
            var ids = new HashSet<int>();

            for (var index = 0; index < items.Count; index++)
            {
                var path = At("$.members", index);
                var member = scan.Object(items[index], path);

                if (member == null)
                {
                    continue;
                }

                scan.Keys(member, path, MemberKeys);

                var id = scan.Integer(member, "id", path, 0, int.MaxValue);
                var description = scan.String(member, "description", path);
                var nodeI = scan.Integer(member, "nodeI", path, 0, int.MaxValue);
                var nodeJ = scan.Integer(member, "nodeJ", path, 0, int.MaxValue);
                var section = scan.String(member, "section", path, nonBlank: true);
                var material = scan.String(member, "material", path, nonBlank: true);
                var rotation = scan.Number(member, "rotation", path);
                var l1 = scan.Numbers(member, "l1", path, 3);
                var l2 = scan.Numbers(member, "l2", path, 3);
                var l3 = scan.Numbers(member, "l3", path, 3);
                var cardinal = scan.Integer(member, "cardinalPoint", path, 0, 9);
                var offsetI = scan.Numbers(member, "offsetI", path, 3);
                var offsetJ = scan.Numbers(member, "offsetJ", path, 3);
                var factors = scan.Numbers(member, "stiffnessFactors", path, 4);
                var axial = scan.Integer(member, "axialBehavior", path, 0, 3);
                var springI = scan.Number(member, "springI", path);
                var springJ = scan.Number(member, "springJ", path);
                var releases = scan.Bools(member, "releases", path, StructuralMemberReleases.FlagCount);

                if (id.HasValue && !ids.Add(id.Value))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpIdDuplicate, path + ".id");
                }

                if (nodeI.HasValue && !nodeIds.Contains(nodeI.Value))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpReference, path + ".nodeI");
                }

                if (nodeJ.HasValue && !nodeIds.Contains(nodeJ.Value))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpReference, path + ".nodeJ");
                }

                if (id.HasValue && description != null && nodeI.HasValue && nodeJ.HasValue && section != null && material != null && rotation.HasValue
                    && l1 != null && l2 != null && l3 != null && cardinal.HasValue && offsetI != null && offsetJ != null && factors != null
                    && axial.HasValue && springI.HasValue && springJ.HasValue && releases != null)
                {
                    rows.Add(new MemberRow
                    {
                        Id = id.Value,
                        Description = description,
                        NodeI = nodeI.Value,
                        NodeJ = nodeJ.Value,
                        Section = section,
                        Material = material,
                        Rotation = rotation.Value,
                        L1 = l1,
                        L2 = l2,
                        L3 = l3,
                        CardinalPoint = cardinal.Value,
                        OffsetI = offsetI,
                        OffsetJ = offsetJ,
                        Factors = factors,
                        Axial = axial.Value,
                        SpringI = springI.Value,
                        SpringJ = springJ.Value,
                        Releases = releases,
                    });
                }
            }

            return rows;
        }

        private static void ReadCases(RamJsonScan scan, RamJsonObject root, List<MemberRow> members, List<CaseRow> cases, List<CombinationRow> combinations)
        {
            var memberIds = new HashSet<int>(members.Select(m => m.Id));
            var caseIds = new HashSet<string>(StringComparer.Ordinal);
            var items = scan.Array(root, "loadCases", "$");

            for (var index = 0; items != null && index < items.Count; index++)
            {
                var path = At("$.loadCases", index);
                var item = scan.Object(items[index], path);

                if (item == null)
                {
                    continue;
                }

                scan.Keys(item, path, CaseKeys);

                var row = new CaseRow
                {
                    Id = scan.String(item, "id", path, nonBlank: true),
                    Category = scan.String(item, "category", path, nonBlank: true),
                    SelfWeight = scan.Numbers(item, "selfWeight", path, 3),
                };

                if (row.Id != null && !caseIds.Add(row.Id))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpIdDuplicate, path + ".id");
                }

                ReadMemberLoads(scan, item, path, memberIds, row);

                if (row.Id != null && row.Category != null && row.SelfWeight != null)
                {
                    cases.Add(row);
                }
            }

            var combos = scan.Array(root, "combinations", "$");

            for (var index = 0; combos != null && index < combos.Count; index++)
            {
                var path = At("$.combinations", index);
                var item = scan.Object(combos[index], path);

                if (item == null)
                {
                    continue;
                }

                scan.Keys(item, path, CombinationKeys);

                var row = new CombinationRow();
                var id = scan.String(item, "id", path, nonBlank: true);
                var terms = scan.Array(item, "terms", path);

                if (id != null && !caseIds.Add(id))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpIdDuplicate, path + ".id");
                }

                for (var t = 0; terms != null && t < terms.Count; t++)
                {
                    var termPath = At(path + ".terms", t);
                    var term = scan.Object(terms[t], termPath);

                    if (term == null)
                    {
                        continue;
                    }

                    scan.Keys(term, termPath, TermKeys);

                    var caseId = scan.String(term, "case", termPath, nonBlank: true);
                    var factor = scan.Number(term, "factor", termPath);

                    if (caseId != null && factor.HasValue)
                    {
                        row.Terms.Add((caseId, factor.Value));
                    }
                }

                if (id != null)
                {
                    row.Id = id;
                    combinations.Add(row);
                }
            }

            // a term points to a case or to another combination of the same dump
            foreach (var combination in combinations)
            {
                foreach (var term in combination.Terms.Where(term => !caseIds.Contains(term.Case)))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpReference, "$.combinations." + combination.Id + ".terms." + term.Case);
                }
            }
        }

        private static void ReadMemberLoads(RamJsonScan scan, RamJsonObject item, string path, HashSet<int> memberIds, CaseRow row)
        {
            var groups = scan.Array(item, "memberLoads", path);

            for (var g = 0; groups != null && g < groups.Count; g++)
            {
                var groupPath = At(path + ".memberLoads", g);
                var group = scan.Object(groups[g], groupPath);

                if (group == null)
                {
                    continue;
                }

                scan.Keys(group, groupPath, MemberLoadKeys);

                var member = scan.Integer(group, "member", groupPath, 0, int.MaxValue);
                var loads = scan.Array(group, "loads", groupPath);

                if (member.HasValue && !memberIds.Contains(member.Value))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpReference, groupPath + ".member");
                }

                for (var l = 0; loads != null && l < loads.Count; l++)
                {
                    var loadPath = At(groupPath + ".loads", l);
                    var load = scan.Object(loads[l], loadPath);

                    if (load == null)
                    {
                        continue;
                    }

                    scan.Keys(load, loadPath, LoadKeys);

                    var direction = scan.String(load, "direction", loadPath);
                    var value = scan.Number(load, "value", loadPath);
                    var distance = scan.Number(load, "distance", loadPath);
                    var percentage = scan.Bool(load, "isPercentage", loadPath);

                    if (direction != null && DirectionOf(direction) == null)
                    {
                        scan.Invalid(RamDumpDiagnosticCodes.DumpLoadDirection, loadPath + ".direction");
                    }

                    if (member.HasValue && direction != null && value.HasValue && distance.HasValue && percentage.HasValue)
                    {
                        row.Loads.Add(new LoadRow
                        {
                            Member = member.Value,
                            Direction = direction,
                            Value = value.Value,
                            Distance = distance.Value,
                            IsPercentage = percentage.Value,
                        });
                    }
                }
            }
        }

        // ---------------------------------------------------------------- the conversion (inverse of D-04)

        /// <summary>The axis and the sign of a direction of the program, in the neutral frame; <c>null</c> when it is unknown.</summary>
        private static (StructuralReadbackLoadAxis Axis, bool Flip)? DirectionOf(string direction)
        {
            switch (direction)
            {
                case "X":
                    return (StructuralReadbackLoadAxis.GlobalX, false);
                case "Y":
                    return (StructuralReadbackLoadAxis.GlobalZ, false);
                case "Z":
                    return (StructuralReadbackLoadAxis.GlobalY, true);
                case "1":
                    return (StructuralReadbackLoadAxis.Local1, false);
                case "2":
                    return (StructuralReadbackLoadAxis.Local2, false);
                case "3":
                    return (StructuralReadbackLoadAxis.Local3, false);
                default:
                    return null;
            }
        }

        // + 0.0 turns a negative zero into zero, so that the same value is always read the same way
        private static StructuralPosition ToPosition(double[] p) =>
            new StructuralPosition(StructuralLength.FromInches(p[0] + 0.0), StructuralLength.FromInches(-p[2] + 0.0), StructuralLength.FromInches(p[1] + 0.0));

        private static StructuralDirection ToDirection(double[] v) => new StructuralDirection(v[0] + 0.0, -v[2] + 0.0, v[1] + 0.0);

        /// <summary>The neutral order Tx Ty Tz Rx Ry Rz from the order of the program dx dy dz rx ry rz.</summary>
        private static bool[] ToFlags(bool[] f) => new[] { f[0], f[2], f[1], f[3], f[5], f[4] };

        /// <summary>The length of each member, from the coordinates of its nodes in the frame of the dump (the frame change keeps lengths).</summary>
        private static Dictionary<int, double> LengthsOf(List<NodeRow> nodes, List<MemberRow> members)
        {
            var coordinates = nodes.ToDictionary(n => n.Id, n => n.Coordinates);
            var lengths = new Dictionary<int, double>();

            foreach (var member in members)
            {
                var a = coordinates[member.NodeI];
                var b = coordinates[member.NodeJ];

                lengths[member.Id] = Math.Sqrt(Square(b[0] - a[0]) + Square(b[1] - a[1]) + Square(b[2] - a[2]));
            }

            return lengths;
        }

        /// <summary>A load placed as a percentage needs a member that has a length.</summary>
        private static void CheckGeometry(RamJsonScan scan, List<NodeRow> nodes, List<MemberRow> members, List<CaseRow> cases)
        {
            if (scan.HasFindings)
            {
                return;
            }

            var lengths = LengthsOf(nodes, members);
            var reported = new HashSet<int>();

            foreach (var load in cases.SelectMany(c => c.Loads).Where(l => l.IsPercentage))
            {
                var length = lengths[load.Member];

                if ((!(length > 0) || double.IsInfinity(length)) && reported.Add(load.Member))
                {
                    scan.Invalid(RamDumpDiagnosticCodes.DumpMemberGeometry, "$.members." + Name(load.Member));
                }
            }
        }

        private static StructuralModelReadback ToReadback(
            string adapterVersion,
            string initialUnits,
            bool restored,
            List<NodeRow> nodes,
            List<MemberRow> members,
            List<CaseRow> cases,
            List<CombinationRow> combinations)
        {
            var lengths = LengthsOf(nodes, members);

            var readNodes = nodes.Select(n => new StructuralReadbackNode(Name(n.Id), n.Description, ToPosition(n.Coordinates), ToFlags(n.Restraints))).ToList();

            var readMembers = members.Select(m => new StructuralReadbackMember(
                Name(m.Id),
                m.Description,
                Name(m.NodeI),
                Name(m.NodeJ),
                m.Section,
                m.Material,
                m.Rotation,
                ToDirection(m.L1),
                ToDirection(m.L2),
                ToDirection(m.L3),
                m.CardinalPoint,
                ToPosition(m.OffsetI),
                ToPosition(m.OffsetJ),
                m.Factors,
                (StructuralReadbackAxialBehavior)m.Axial,
                m.SpringI,
                m.SpringJ,
                m.Releases)).ToList();

            var readCases = cases.Select(c => new StructuralReadbackLoadCase(
                c.Id,
                c.Category,
                ToDirection(c.SelfWeight),
                c.Loads.Select(l => ToLoad(l, lengths[l.Member])))).ToList();

            var readCombinations = combinations.Select(c => new StructuralReadbackCombination(
                c.Id,
                c.Terms.Select(t => new StructuralReadbackCombinationTerm(t.Case, t.Factor)))).ToList();

            return new StructuralModelReadback(adapterVersion, initialUnits, restored, readNodes, readMembers, readCases, readCombinations);
        }

        private static StructuralReadbackPointLoad ToLoad(LoadRow load, double memberLength)
        {
            var direction = DirectionOf(load.Direction).Value;
            var value = direction.Flip ? -load.Value + 0.0 : load.Value + 0.0;
            var distance = load.IsPercentage ? (load.Distance * memberLength) / 100.0 : load.Distance;

            return new StructuralReadbackPointLoad(
                Name(load.Member),
                direction.Axis,
                StructuralForce.FromPoundsForce(value),
                StructuralLength.FromInches(distance + 0.0));
        }

        private static double Square(double value) => value * value;

        private static string Name(int id) => id.ToString(CultureInfo.InvariantCulture);
    }
}
