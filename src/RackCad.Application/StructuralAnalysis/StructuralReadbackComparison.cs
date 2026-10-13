using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>The verdict of one row of the comparison.</summary>
    public enum StructuralComparisonVerdict
    {
        Pass = 0,
        Fail = 1,

        /// <summary>The row is reported but it is not a criterion: it never decides the overall verdict.</summary>
        Informative = 2,

        /// <summary>The comparator does not evaluate the row; the note says who does, or why it could not.</summary>
        NotEvaluated = 3,
    }

    /// <summary>The overall outcome of the comparison.</summary>
    public enum StructuralComparisonOutcome
    {
        /// <summary>Every evaluated row passes and none fails.</summary>
        Pass = 0,

        /// <summary>At least one row fails.</summary>
        Fail = 1,

        /// <summary>The read-back belongs to another input than the manifest: nothing is approved.</summary>
        Stale = 2,

        /// <summary>The read-back is not approved (a failure, a gate rejection) or a dump failed: nothing is approved.</summary>
        Rejected = 3,

        /// <summary>
        /// No row fails but the combinations of a dump could not be read, so the comparison is not complete and is never a PASS (A65-F4-02):
        /// the engineer decides, and the decision is written in the report.
        /// </summary>
        Incomplete = 4,
    }

    /// <summary>The stable identifiers of the rows of the report, in report order. An identifier is never renamed or reused.</summary>
    public static class StructuralComparisonRowIds
    {
        public const string NodeCoordinates = "NODE-COORDINATES";
        public const string Connectivity = "CONNECTIVITY";
        public const string LocalAxes = "LOCAL-AXES";
        public const string SectionName = "SECTION-NAME";
        public const string MaterialName = "MATERIAL-NAME";
        public const string SectionProperties = "SECTION-PROPERTIES";
        public const string MaterialProperties = "MATERIAL-PROPERTIES";
        public const string MemberDefaults = "MEMBER-DEFAULTS";
        public const string ReleasesSupports = "RELEASES-SUPPORTS";
        public const string Loads = "LOADS";
        public const string Combinations = "COMBINATIONS";
        public const string CaseCategory = "CASE-CATEGORY";
        public const string ManualCaseCategory = "MANUAL-CASE-CATEGORY";
        public const string SelfWeight = "SELF-WEIGHT";
    }

    /// <summary>
    /// What a dump could read. The combinations are read when the API exposes them; the neutral readback cannot tell "none" from "not
    /// read", so the caller says it. By default everything was read.
    /// </summary>
    public sealed class StructuralComparisonOptions
    {
        public StructuralComparisonOptions(bool generatedCombinationsRead = true, bool manualCombinationsRead = true)
        {
            GeneratedCombinationsRead = generatedCombinationsRead;
            ManualCombinationsRead = manualCombinationsRead;
        }

        public bool GeneratedCombinationsRead { get; }

        public bool ManualCombinationsRead { get; }
    }

    /// <summary>One row of the report: a verdict, the largest delta found and the first differences in the order of the model.</summary>
    public sealed class StructuralComparisonRow
    {
        internal StructuralComparisonRow(
            string id,
            string name,
            StructuralComparisonVerdict verdict,
            double? maxDelta,
            string unit,
            int differenceCount,
            IReadOnlyList<string> differences,
            string note)
        {
            Id = id;
            Name = name;
            Verdict = verdict;
            MaxDelta = maxDelta;
            Unit = unit;
            DifferenceCount = differenceCount;
            Differences = differences;
            Note = note;
        }

        public string Id { get; }

        public string Name { get; }

        public StructuralComparisonVerdict Verdict { get; }

        /// <summary>The largest delta of the row in <see cref="Unit"/>; <c>null</c> when the row has no numeric delta.</summary>
        public double? MaxDelta { get; }

        public string Unit { get; }

        /// <summary>The number of differences found, listed or not.</summary>
        public int DifferenceCount { get; }

        /// <summary>The first differences (at most <see cref="StructuralComparisonReport.MaxListedDifferences"/>), in the order of the model.</summary>
        public IReadOnlyList<string> Differences { get; }

        public string Note { get; }
    }

    /// <summary>
    /// The inputs a caller can attach to a report (A65-F4-03): the fingerprints of the manifest and of the read-back and the SHA-256 of the
    /// four files compared, in lower-case hexadecimal. The comparator is pure and reads no file: it only writes what the caller gives.
    /// </summary>
    public sealed class StructuralComparisonInputs
    {
        public StructuralComparisonInputs(
            string manifestFingerprint,
            string readbackFingerprint,
            string manifestSha256,
            string readbackSha256,
            string generatedDumpSha256,
            string manualDumpSha256)
        {
            ManifestFingerprint = manifestFingerprint;
            ReadbackFingerprint = readbackFingerprint;
            ManifestSha256 = Hash(manifestSha256, nameof(manifestSha256));
            ReadbackSha256 = Hash(readbackSha256, nameof(readbackSha256));
            GeneratedDumpSha256 = Hash(generatedDumpSha256, nameof(generatedDumpSha256));
            ManualDumpSha256 = Hash(manualDumpSha256, nameof(manualDumpSha256));
        }

        /// <summary>The fingerprint of the input that the manifest records; <c>null</c> when it could not be read.</summary>
        public string ManifestFingerprint { get; }

        /// <summary>The fingerprint of the input that the read-back records; <c>null</c> when it could not be read.</summary>
        public string ReadbackFingerprint { get; }

        public string ManifestSha256 { get; }

        public string ReadbackSha256 { get; }

        public string GeneratedDumpSha256 { get; }

        public string ManualDumpSha256 { get; }

        private static string Hash(string value, string name)
        {
            if (value == null || value.Length != 64 || !value.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
            {
                throw new ArgumentException("A SHA-256 is 64 lower-case hexadecimal characters.", name);
            }

            return value;
        }
    }

    /// <summary>
    /// The report of a comparison (<c>rackcad-structural-comparison/v1</c>): one entry per row of the matrix, in a stable order. It compares
    /// the transfer of a model; it does not validate the design.
    /// </summary>
    public sealed class StructuralComparisonReport
    {
        public const string Schema = "rackcad-structural-comparison/v1";

        /// <summary>How many differences of a row the report lists.</summary>
        public const int MaxListedDifferences = 5;

        internal StructuralComparisonReport(StructuralComparisonOutcome outcome, string reason, IReadOnlyList<StructuralComparisonRow> rows)
            : this(outcome, reason, rows, null, null)
        {
        }

        private StructuralComparisonReport(
            StructuralComparisonOutcome outcome,
            string reason,
            IReadOnlyList<StructuralComparisonRow> rows,
            string combinationsDecision,
            StructuralComparisonInputs inputs)
        {
            Outcome = outcome;
            Reason = reason;
            Rows = rows;
            CombinationsDecision = combinationsDecision;
            Inputs = inputs;
        }

        public StructuralComparisonOutcome Outcome { get; }

        /// <summary>Why the comparison was rejected or stale; <c>null</c> otherwise.</summary>
        public string Reason { get; }

        public IReadOnlyList<StructuralComparisonRow> Rows { get; }

        /// <summary>The decision of the engineer about the combinations nobody could read; <c>null</c> when there is none (A65-F4-02).</summary>
        public string CombinationsDecision { get; }

        /// <summary>The inputs the caller attached; <c>null</c> when it attached none (A65-F4-03).</summary>
        public StructuralComparisonInputs Inputs { get; }

        /// <summary>The same report with the decision about the combinations written in it. The text is required and not blank.</summary>
        public StructuralComparisonReport WithCombinationsDecision(string decision)
        {
            if (string.IsNullOrWhiteSpace(decision))
            {
                throw new ArgumentException("A decision needs a text.", nameof(decision));
            }

            return new StructuralComparisonReport(Outcome, Reason, Rows, decision, Inputs);
        }

        /// <summary>The same report with the inputs it was made from. A report without inputs serializes exactly as before.</summary>
        public StructuralComparisonReport WithInputs(StructuralComparisonInputs inputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }

            return new StructuralComparisonReport(Outcome, Reason, Rows, CombinationsDecision, inputs);
        }

        public StructuralComparisonRow Row(string id)
        {
            var row = Rows.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.Ordinal));

            if (row == null)
            {
                throw new ArgumentException("The report has no row '" + id + "'.", nameof(id));
            }

            return row;
        }

        /// <summary>The report as deterministic JSON: stable order, invariant culture, LF, no clock, ending with a line feed.</summary>
        public string ToJson()
        {
            var text = new StringBuilder();

            text.Append("{\n");
            text.Append("  \"schema\": ").Append(Quote(Schema)).Append(",\n");
            text.Append("  \"verdict\": ").Append(Quote(OutcomeText(Outcome))).Append(",\n");
            text.Append("  \"reason\": ").Append(Reason == null ? "null" : Quote(Reason)).Append(",\n");

            if (CombinationsDecision != null)
            {
                text.Append("  \"combinationsDecision\": ").Append(Quote(CombinationsDecision)).Append(",\n");
            }

            if (Inputs != null)
            {
                text.Append("  \"inputs\": {\n");
                text.Append("    \"manifestFingerprint\": ").Append(Inputs.ManifestFingerprint == null ? "null" : Quote(Inputs.ManifestFingerprint)).Append(",\n");
                text.Append("    \"readbackFingerprint\": ").Append(Inputs.ReadbackFingerprint == null ? "null" : Quote(Inputs.ReadbackFingerprint)).Append(",\n");
                text.Append("    \"manifestSha256\": ").Append(Quote(Inputs.ManifestSha256)).Append(",\n");
                text.Append("    \"readbackSha256\": ").Append(Quote(Inputs.ReadbackSha256)).Append(",\n");
                text.Append("    \"generatedDumpSha256\": ").Append(Quote(Inputs.GeneratedDumpSha256)).Append(",\n");
                text.Append("    \"manualDumpSha256\": ").Append(Quote(Inputs.ManualDumpSha256)).Append("\n");
                text.Append("  },\n");
            }

            text.Append("  \"tolerances\": {\n");
            text.Append("    \"nodePositionInches\": ").Append(Number(StructuralReadbackComparison.NodePositionToleranceInches)).Append(",\n");
            text.Append("    \"axisDegrees\": ").Append(Number(StructuralReadbackComparison.AxisToleranceDegrees)).Append(",\n");
            text.Append("    \"loadValuePercent\": ").Append(Number(StructuralReadbackComparison.LoadValueTolerancePercent)).Append(",\n");
            text.Append("    \"loadPositionInches\": ").Append(Number(StructuralReadbackComparison.LoadPositionToleranceInches)).Append("\n");
            text.Append("  },\n");
            text.Append("  \"rows\": [");

            for (var index = 0; index < Rows.Count; index++)
            {
                var row = Rows[index];
                text.Append(index == 0 ? "\n" : ",\n");
                text.Append("    {\n");
                text.Append("      \"id\": ").Append(Quote(row.Id)).Append(",\n");
                text.Append("      \"name\": ").Append(Quote(row.Name)).Append(",\n");
                text.Append("      \"verdict\": ").Append(Quote(VerdictText(row.Verdict))).Append(",\n");
                text.Append("      \"maxDelta\": ").Append(row.MaxDelta.HasValue ? Number(row.MaxDelta.Value) : "null").Append(",\n");
                text.Append("      \"unit\": ").Append(row.Unit == null ? "null" : Quote(row.Unit)).Append(",\n");
                text.Append("      \"differenceCount\": ").Append(row.DifferenceCount.ToString(CultureInfo.InvariantCulture)).Append(",\n");
                text.Append("      \"differences\": [");

                for (var d = 0; d < row.Differences.Count; d++)
                {
                    text.Append(d == 0 ? "\n" : ",\n");
                    text.Append("        ").Append(Quote(row.Differences[d]));
                }

                text.Append(row.Differences.Count == 0 ? "],\n" : "\n      ],\n");
                text.Append("      \"note\": ").Append(row.Note == null ? "null" : Quote(row.Note)).Append("\n");
                text.Append("    }");
            }

            text.Append(Rows.Count == 0 ? "]\n" : "\n  ]\n");
            text.Append("}\n");

            return text.ToString();
        }

        private static string OutcomeText(StructuralComparisonOutcome outcome)
        {
            switch (outcome)
            {
                case StructuralComparisonOutcome.Pass:
                    return "PASS";
                case StructuralComparisonOutcome.Fail:
                    return "FAIL";
                case StructuralComparisonOutcome.Stale:
                    return "STALE";
                case StructuralComparisonOutcome.Incomplete:
                    return "INCOMPLETE";
                default:
                    return "REJECTED";
            }
        }

        private static string VerdictText(StructuralComparisonVerdict verdict)
        {
            switch (verdict)
            {
                case StructuralComparisonVerdict.Pass:
                    return "PASS";
                case StructuralComparisonVerdict.Fail:
                    return "FAIL";
                case StructuralComparisonVerdict.Informative:
                    return "INFORMATIVE";
                default:
                    return "NOT_EVALUATED";
            }
        }

        private static string Number(double value) => StructuralReadbackComparison.Format(value);

        private static string Quote(string value)
        {
            var text = new StringBuilder(value.Length + 2);
            text.Append('"');

            foreach (var character in value)
            {
                switch (character)
                {
                    case '"':
                        text.Append("\\\"");
                        break;
                    case '\\':
                        text.Append("\\\\");
                        break;
                    case '\n':
                        text.Append("\\n");
                        break;
                    case '\r':
                        text.Append("\\r");
                        break;
                    case '\t':
                        text.Append("\\t");
                        break;
                    default:
                        if (character < 0x20)
                        {
                            text.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            text.Append(character);
                        }

                        break;
                }
            }

            return text.Append('"').ToString();
        }
    }

    /// <summary>
    /// The comparator of two readbacks (I-65 D-11 and D-13, INV-13): the dump of the generated model against the dump of the manual one,
    /// with the read-back of the build and the fingerprint of the manifest. It is PURE: no input or output, no clock, no state; the same
    /// inputs give the same report and the same serialized bytes. It compares the transfer of a model; it does not validate the design.
    ///
    /// <para>
    /// Nodes are paired by position (a candidate is a node whose Euclidean distance is within the tolerance); a node with two candidates, or a
    /// candidate that two nodes claim, is never resolved in silence: the row fails. Members are paired by their pair of paired nodes, in
    /// any order, and then the sense I to J must agree. The tolerances are the ones of the Freeze and are not altered. The value of a load
    /// is compared relative to the value of the MANUAL model, and in absolute value when that value is zero.
    /// </para>
    /// <para>
    /// Before any row, the dump of the GENERATED model is bound to the values that the build read back from the model it wrote: if they do
    /// not agree the comparison is rejected. A comparison whose combinations nobody could read is INCOMPLETE, never a PASS.
    /// </para>
    /// </summary>
    public static class StructuralReadbackComparison
    {
        public const double NodePositionToleranceInches = 0.02;
        public const double AxisToleranceDegrees = 0.1;
        public const double LoadValueTolerancePercent = 0.1;
        public const double LoadPositionToleranceInches = 0.02;

        /// <summary>
        /// The tolerance, in pounds-force, of a load whose manual value is zero: the value is compared in absolute value and never divided.
        /// It is the noise of a double in the range of V1, not an engineering criterion.
        /// </summary>
        public const double ZeroLoadAbsoluteToleranceLbf = 1e-6;

        /// <summary>The tolerance, in inches and pounds-force, with which the dump of the generated model must agree with the read-back of the build.</summary>
        public const double ReadbackBindingTolerance = 1e-6;

        /// <summary>The category the generated case with loads must have.</summary>
        public const string GeneratedCaseCategory = "OTHER";

        // absorbs the noise of the last bits of a double at the very edge of a tolerance
        private const double Epsilon = 1e-9;

        private static readonly (string Id, string Name)[] RowTable =
        {
            (StructuralComparisonRowIds.NodeCoordinates, "Node coordinates (paired by position)"),
            (StructuralComparisonRowIds.Connectivity, "Connectivity (end nodes of the members, I to J)"),
            (StructuralComparisonRowIds.LocalAxes, "Local axes l1, l2 and l3"),
            (StructuralComparisonRowIds.SectionName, "Section name"),
            (StructuralComparisonRowIds.MaterialName, "Material name"),
            (StructuralComparisonRowIds.SectionProperties, "Section properties (A, Ix, Iy, J)"),
            (StructuralComparisonRowIds.MaterialProperties, "Material properties (E, nu or G, unit weight)"),
            (StructuralComparisonRowIds.MemberDefaults, "Cardinal point, end offsets, stiffness factors, axial behaviour and springs"),
            (StructuralComparisonRowIds.ReleasesSupports, "Releases and support flags"),
            (StructuralComparisonRowIds.Loads, "Point loads on the members, by case with loads"),
            (StructuralComparisonRowIds.Combinations, "Load combinations (none expected)"),
            (StructuralComparisonRowIds.CaseCategory, "Category of the generated case with loads"),
            (StructuralComparisonRowIds.ManualCaseCategory, "Category of the manual case with loads"),
            (StructuralComparisonRowIds.SelfWeight, "Self weight of the cases with loads"),
        };

        private const string SignedSheetNote =
            "Evaluated by the signed verification sheet of the engineer; the program API does not expose section or material properties.";

        public static StructuralComparisonReport CompareReadbacks(
            StructuralModelReadback generated,
            StructuralModelReadback manual,
            StructuralTransferReadback transfer,
            string manifestAnalysisInputFingerprint,
            StructuralComparisonOptions options = null)
        {
            if (generated == null)
            {
                throw new ArgumentNullException(nameof(generated));
            }

            if (manual == null)
            {
                throw new ArgumentNullException(nameof(manual));
            }

            if (transfer == null)
            {
                throw new ArgumentNullException(nameof(transfer));
            }

            options = options ?? new StructuralComparisonOptions();

            if (!transfer.Passed)
            {
                var detail = transfer.Differences.Count == 0 ? "no detail" : string.Join("; ", transfer.Differences);
                return Unapproved(StructuralComparisonOutcome.Rejected, "The read-back of the build is not approved: " + detail);
            }

            if (!string.Equals(transfer.AnalysisInputFingerprint, manifestAnalysisInputFingerprint, StringComparison.Ordinal))
            {
                return Unapproved(
                    StructuralComparisonOutcome.Stale,
                    "The fingerprint of the read-back (" + (transfer.AnalysisInputFingerprint ?? "none") + ") is not the one of the manifest ("
                    + (manifestAnalysisInputFingerprint ?? "none") + ").");
            }

            var mismatch = FirstBindingMismatch(generated, transfer.Values);

            if (mismatch != null)
            {
                return Unapproved(
                    StructuralComparisonOutcome.Rejected,
                    "The dump of the generated model does not agree with the values of the read-back of the build: " + mismatch);
            }

            var comparison = new Comparison(generated, manual, options);
            var rows = comparison.Rows();
            var failed = rows.Any(row => row.Verdict == StructuralComparisonVerdict.Fail);
            var incomplete = rows.Any(row => row.Id == StructuralComparisonRowIds.Combinations && row.Verdict == StructuralComparisonVerdict.NotEvaluated);

            return new StructuralComparisonReport(
                failed ? StructuralComparisonOutcome.Fail : (incomplete ? StructuralComparisonOutcome.Incomplete : StructuralComparisonOutcome.Pass),
                null,
                new ReadOnlyCollection<StructuralComparisonRow>(rows));
        }

        // ------------------------------------------------------------ the dump of the generated model against the read-back of the build

        private static string FirstBindingMismatch(StructuralModelReadback generated, StructuralTransferValues values)
        {
            if (values == null)
            {
                return "the read-back carries no values to compare the dump with";
            }

            if (values.Nodes.Count != generated.Nodes.Count)
            {
                return "the read-back lists " + Count(values.Nodes.Count) + " nodes and the dump " + Count(generated.Nodes.Count);
            }

            if (values.Members.Count != generated.Members.Count)
            {
                return "the read-back lists " + Count(values.Members.Count) + " members and the dump " + Count(generated.Members.Count);
            }

            var casesWithLoads = generated.LoadCases.Where(c => c.Loads.Count > 0).ToList();
            var expectedLoads = values.Loads.Count;
            var dumpLoads = casesWithLoads.Sum(c => c.Loads.Count);

            if (expectedLoads != dumpLoads)
            {
                return "the read-back lists " + Count(expectedLoads) + " loads and the dump " + Count(dumpLoads);
            }

            var nodesById = new Dictionary<string, StructuralReadbackNode>(StringComparer.Ordinal);

            foreach (var node in generated.Nodes)
            {
                nodesById[node.Id] = node;
            }

            foreach (var expected in values.Nodes)
            {
                if (!nodesById.TryGetValue(expected.ModelId, out var node))
                {
                    return "node " + expected.Id + ": the dump has no node with the id " + expected.ModelId;
                }

                if (!string.Equals(node.Description, expected.Id, StringComparison.Ordinal))
                {
                    return "node " + expected.ModelId + ": the dump calls it '" + node.Description + "' and the read-back '" + expected.Id + "'";
                }

                if (Far(node.Position.X.Inches, expected.Position.X.Inches) || Far(node.Position.Y.Inches, expected.Position.Y.Inches)
                    || Far(node.Position.Z.Inches, expected.Position.Z.Inches))
                {
                    return "node " + expected.Id + ": the dump has (" + Format(node.Position.X.Inches) + ", " + Format(node.Position.Y.Inches) + ", "
                           + Format(node.Position.Z.Inches) + ") in and the read-back (" + Format(expected.Position.X.Inches) + ", "
                           + Format(expected.Position.Y.Inches) + ", " + Format(expected.Position.Z.Inches) + ") in";
                }

                if (!node.SupportFlags.SequenceEqual(expected.SupportFlags))
                {
                    return "node " + expected.Id + ": the dump has support flags " + Flags(node.SupportFlags) + " and the read-back " + Flags(expected.SupportFlags);
                }
            }

            var membersById = new Dictionary<string, StructuralReadbackMember>(StringComparer.Ordinal);

            foreach (var member in generated.Members)
            {
                membersById[member.Id] = member;
            }

            foreach (var expected in values.Members)
            {
                if (!membersById.TryGetValue(expected.ModelId, out var member))
                {
                    return "member " + expected.Id + ": the dump has no member with the id " + expected.ModelId;
                }

                if (!string.Equals(member.Description, expected.Id, StringComparison.Ordinal))
                {
                    return "member " + expected.ModelId + ": the dump calls it '" + member.Description + "' and the read-back '" + expected.Id + "'";
                }

                if (!string.Equals(member.SectionName, expected.SectionName, StringComparison.Ordinal))
                {
                    return "member " + expected.Id + ": the dump has section '" + member.SectionName + "' and the read-back '" + expected.SectionName + "'";
                }

                if (!string.Equals(member.MaterialName, expected.MaterialName, StringComparison.Ordinal))
                {
                    return "member " + expected.Id + ": the dump has material '" + member.MaterialName + "' and the read-back '" + expected.MaterialName + "'";
                }
            }

            return FirstLoadMismatch(generated, values, casesWithLoads, nodesById, membersById);
        }

        private static string FirstLoadMismatch(
            StructuralModelReadback generated,
            StructuralTransferValues values,
            List<StructuralReadbackLoadCase> casesWithLoads,
            Dictionary<string, StructuralReadbackNode> nodesById,
            Dictionary<string, StructuralReadbackMember> membersById)
        {
            if (values.LoadCase == null)
            {
                return casesWithLoads.Count == 0 ? null : "the read-back lists no load case and the dump has loads in case " + casesWithLoads[0].Id;
            }

            var expectedCase = values.LoadCase;
            var dumpCase = generated.LoadCases.FirstOrDefault(c => string.Equals(c.Id, expectedCase.Id, StringComparison.Ordinal));

            if (dumpCase == null)
            {
                return "load case " + expectedCase.Id + ": the dump has no such case";
            }

            if (!string.Equals(dumpCase.Category, expectedCase.Category, StringComparison.Ordinal))
            {
                return "load case " + expectedCase.Id + ": the dump has category '" + dumpCase.Category + "' and the read-back '" + expectedCase.Category + "'";
            }

            var other = casesWithLoads.FirstOrDefault(c => !string.Equals(c.Id, expectedCase.Id, StringComparison.Ordinal));

            if (other != null)
            {
                return "the dump has loads in case " + other.Id + ", which the read-back does not list";
            }

            var membersByName = new Dictionary<string, StructuralTransferMemberValue>(StringComparer.Ordinal);

            foreach (var member in values.Members)
            {
                membersByName[member.Id] = member;
            }

            foreach (var expected in values.Loads)
            {
                var label = "load " + expected.Index.ToString(CultureInfo.InvariantCulture) + " of member " + expected.MemberId;

                if (!membersByName.TryGetValue(expected.MemberId, out var memberValue))
                {
                    return label + ": the read-back lists no such member";
                }

                var dumpLoads = dumpCase.Loads.Where(l => string.Equals(l.MemberId, memberValue.ModelId, StringComparison.Ordinal)).ToList();

                if (expected.Index >= dumpLoads.Count)
                {
                    return label + ": the dump has " + Count(dumpLoads.Count) + " loads on that member";
                }

                var load = dumpLoads[expected.Index];

                if (load.Axis != expected.Axis)
                {
                    return label + ": the dump has direction " + load.Axis + " and the read-back " + expected.Axis;
                }

                if (Far(load.Value.PoundsForce, expected.Value.PoundsForce))
                {
                    return label + ": the dump has " + Format(load.Value.PoundsForce) + " lbf and the read-back " + Format(expected.Value.PoundsForce) + " lbf";
                }

                var distance = expected.Distance;

                if (expected.IsPercentage)
                {
                    var member = membersById[memberValue.ModelId];

                    if (!nodesById.TryGetValue(member.NodeIId, out var nodeI) || !nodesById.TryGetValue(member.NodeJId, out var nodeJ))
                    {
                        return label + ": the dump does not list an end node of the member, so a percentage cannot become a distance";
                    }

                    var a = nodeI.Position;
                    var b = nodeJ.Position;
                    var length = Math.Sqrt(
                        Square(b.X.Inches - a.X.Inches) + Square(b.Y.Inches - a.Y.Inches) + Square(b.Z.Inches - a.Z.Inches));
                    distance = (expected.Distance * length) / 100.0;
                }

                if (Far(load.DistanceFromNodeI.Inches, distance))
                {
                    return label + ": the dump has " + Format(load.DistanceFromNodeI.Inches) + " in from node I and the read-back " + Format(distance) + " in";
                }
            }

            return null;
        }

        private static bool Far(double a, double b) => Math.Abs(a - b) > ReadbackBindingTolerance;

        private static double Square(double value) => value * value;

        private static string Count(int count) => count.ToString(CultureInfo.InvariantCulture);

        private static string Flags(IReadOnlyList<bool> flags) => string.Concat(flags.Select(flag => flag ? "1" : "0"));

        /// <summary>A rejected report: no row is approved. For an input that cannot even be compared (a dump in a failed state).</summary>
        public static StructuralComparisonReport Rejected(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("A rejection needs a reason.", nameof(reason));
            }

            return Unapproved(StructuralComparisonOutcome.Rejected, reason);
        }

        internal static string Format(double value)
        {
            var rounded = Math.Round(value, 6) + 0.0;
            return rounded.ToString("R", CultureInfo.InvariantCulture);
        }

        private static StructuralComparisonReport Unapproved(StructuralComparisonOutcome outcome, string reason)
        {
            var rows = RowTable
                .Select(entry => new StructuralComparisonRow(
                    entry.Id, entry.Name, StructuralComparisonVerdict.NotEvaluated, null, null, 0, new string[0], reason))
                .ToList();

            return new StructuralComparisonReport(outcome, reason, new ReadOnlyCollection<StructuralComparisonRow>(rows));
        }

        /// <summary>The working state of one comparison. It lives for the duration of one call.</summary>
        private sealed class Comparison
        {
            private readonly StructuralModelReadback _generated;
            private readonly StructuralModelReadback _manual;
            private readonly StructuralComparisonOptions _options;

            private readonly Dictionary<string, string> _nodeMap = new Dictionary<string, string>(StringComparer.Ordinal);
            private readonly List<(StructuralReadbackMember Generated, StructuralReadbackMember Manual)> _pairs =
                new List<(StructuralReadbackMember, StructuralReadbackMember)>();

            private const string ZeroLoadLimitText = "0.000001";

            private bool _nodesPaired;
            private bool _zeroBaseCompared;
            private double _maxZeroBaseLbf;

            public Comparison(StructuralModelReadback generated, StructuralModelReadback manual, StructuralComparisonOptions options)
            {
                _generated = generated;
                _manual = manual;
                _options = options;
            }

            public List<StructuralComparisonRow> Rows()
            {
                var coordinates = Coordinates();
                var connectivity = Connectivity();

                return new List<StructuralComparisonRow>
                {
                    coordinates,
                    connectivity,
                    LocalAxes(),
                    Names(StructuralComparisonRowIds.SectionName, "section", member => member.SectionName),
                    Names(StructuralComparisonRowIds.MaterialName, "material", member => member.MaterialName),
                    Build(StructuralComparisonRowIds.SectionProperties, StructuralComparisonVerdict.NotEvaluated, null, null, new List<string>(), SignedSheetNote),
                    Build(StructuralComparisonRowIds.MaterialProperties, StructuralComparisonVerdict.NotEvaluated, null, null, new List<string>(), SignedSheetNote),
                    Defaults(),
                    ReleasesAndSupports(),
                    Loads(),
                    Combinations(),
                    CaseCategory(),
                    ManualCaseCategory(),
                    SelfWeight(),
                };
            }

            // ------------------------------------------------------------ nodes

            private StructuralComparisonRow Coordinates()
            {
                var differences = new List<string>();
                var generatedNodes = _generated.Nodes;
                var manualNodes = _manual.Nodes;
                var maxDelta = 0.0;

                var candidatesOfGenerated = generatedNodes.Select(g => manualNodes.Where(m => Delta(g, m) <= NodePositionToleranceInches + Epsilon).ToList()).ToList();
                var candidatesOfManual = manualNodes.Select(m => generatedNodes.Where(g => Delta(g, m) <= NodePositionToleranceInches + Epsilon).ToList()).ToList();

                for (var index = 0; index < generatedNodes.Count; index++)
                {
                    var node = generatedNodes[index];
                    var candidates = candidatesOfGenerated[index];

                    if (manualNodes.Count > 0)
                    {
                        maxDelta = Math.Max(maxDelta, manualNodes.Min(m => Delta(node, m)));
                    }

                    if (candidates.Count == 0)
                    {
                        differences.Add("generated node " + node.Id + " has no manual node within " + Format(NodePositionToleranceInches) + " in");
                    }
                    else if (candidates.Count > 1)
                    {
                        differences.Add(
                            "ambiguous pairing: generated node " + node.Id + " has " + candidates.Count.ToString(CultureInfo.InvariantCulture)
                            + " candidates (" + string.Join(", ", candidates.Select(c => c.Id)) + ")");
                    }
                }

                for (var index = 0; index < manualNodes.Count; index++)
                {
                    var node = manualNodes[index];
                    var candidates = candidatesOfManual[index];

                    if (generatedNodes.Count > 0)
                    {
                        maxDelta = Math.Max(maxDelta, generatedNodes.Min(g => Delta(g, node)));
                    }

                    if (candidates.Count == 0)
                    {
                        differences.Add("manual node " + node.Id + " has no generated node within " + Format(NodePositionToleranceInches) + " in");
                    }
                    else if (candidates.Count > 1)
                    {
                        differences.Add(
                            "generated nodes " + string.Join(", ", candidates.Select(c => c.Id)) + " are paired twice with manual node " + node.Id);
                    }
                }

                if (differences.Count == 0)
                {
                    for (var index = 0; index < generatedNodes.Count; index++)
                    {
                        _nodeMap[generatedNodes[index].Id] = candidatesOfGenerated[index][0].Id;
                    }

                    _nodesPaired = true;
                }

                return Build(
                    StructuralComparisonRowIds.NodeCoordinates,
                    differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail,
                    maxDelta,
                    "in",
                    differences,
                    "The nodes are paired by position; the delta is the Euclidean distance between a node and its nearest counterpart.");
            }

            private static double Delta(StructuralReadbackNode a, StructuralReadbackNode b) =>
                Math.Sqrt(
                    Square(a.Position.X.Inches - b.Position.X.Inches)
                    + Square(a.Position.Y.Inches - b.Position.Y.Inches)
                    + Square(a.Position.Z.Inches - b.Position.Z.Inches));

            // ------------------------------------------------------------ members

            private StructuralComparisonRow Connectivity()
            {
                var differences = new List<string>();

                if (!_nodesPaired)
                {
                    differences.Add(UnpairedNodes);
                    return Build(StructuralComparisonRowIds.Connectivity, StructuralComparisonVerdict.Fail, null, null, differences, null);
                }

                var manualByEnds = new Dictionary<string, List<StructuralReadbackMember>>(StringComparer.Ordinal);

                foreach (var member in _manual.Members)
                {
                    var key = EndsKey(member.NodeIId, member.NodeJId);

                    if (!manualByEnds.TryGetValue(key, out var list))
                    {
                        list = new List<StructuralReadbackMember>();
                        manualByEnds.Add(key, list);
                    }

                    list.Add(member);
                }

                var claimed = new HashSet<string>(StringComparer.Ordinal);

                foreach (var member in _generated.Members)
                {
                    if (!_nodeMap.TryGetValue(member.NodeIId, out var nodeI) || !_nodeMap.TryGetValue(member.NodeJId, out var nodeJ))
                    {
                        differences.Add("generated member " + member.Id + " uses a node that the dump does not list");
                        continue;
                    }

                    var key = EndsKey(nodeI, nodeJ);

                    if (!manualByEnds.TryGetValue(key, out var candidates))
                    {
                        differences.Add("generated member " + member.Id + " has no manual member between its nodes");
                        continue;
                    }

                    if (candidates.Count > 1 || !claimed.Add(key))
                    {
                        differences.Add("generated member " + member.Id + " shares its pair of nodes with another member: pairing is ambiguous");
                        continue;
                    }

                    var counterpart = candidates[0];
                    _pairs.Add((member, counterpart));

                    if (!string.Equals(counterpart.NodeIId, nodeI, StringComparison.Ordinal))
                    {
                        differences.Add(
                            "generated member " + member.Id + " runs from node I to node J in the opposite sense of manual member " + counterpart.Id
                            + ": inverted");
                    }
                }

                foreach (var member in _manual.Members)
                {
                    if (!_pairs.Any(pair => ReferenceEquals(pair.Manual, member)))
                    {
                        differences.Add("manual member " + member.Id + " has no generated counterpart");
                    }
                }

                return Build(
                    StructuralComparisonRowIds.Connectivity,
                    differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail,
                    null,
                    null,
                    differences,
                    null);
            }

            private const string UnpairedNodes = "the nodes could not be paired, so the members cannot be paired";

            private static string EndsKey(string a, string b) =>
                string.CompareOrdinal(a, b) <= 0 ? a + "\u0001" + b : b + "\u0001" + a;

            private StructuralComparisonRow LocalAxes()
            {
                if (!_nodesPaired)
                {
                    return Build(StructuralComparisonRowIds.LocalAxes, StructuralComparisonVerdict.Fail, null, null, new List<string> { UnpairedNodes }, null);
                }

                var differences = new List<string>();
                var maxDelta = 0.0;

                foreach (var pair in _pairs)
                {
                    var axes = new[]
                    {
                        ("l1", pair.Generated.Axis1, pair.Manual.Axis1),
                        ("l2", pair.Generated.Axis2, pair.Manual.Axis2),
                        ("l3", pair.Generated.Axis3, pair.Manual.Axis3),
                    };

                    foreach (var axis in axes)
                    {
                        var angle = AngleDegrees(axis.Item2, axis.Item3);
                        maxDelta = Math.Max(maxDelta, angle);

                        if (angle > AxisToleranceDegrees + Epsilon)
                        {
                            differences.Add(
                                "member " + pair.Generated.Id + " (manual " + pair.Manual.Id + "): " + axis.Item1 + " differs by "
                                + Format(angle) + " deg (limit " + Format(AxisToleranceDegrees) + ")");
                        }
                    }
                }

                return Build(
                    StructuralComparisonRowIds.LocalAxes,
                    differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail,
                    maxDelta,
                    "deg",
                    differences,
                    "The angle between corresponding vectors is the arc cosine of their normalised dot product, clamped to [-1, 1].");
            }

            private static double AngleDegrees(StructuralDirection a, StructuralDirection b)
            {
                var normA = Math.Sqrt((a.X * a.X) + (a.Y * a.Y) + (a.Z * a.Z));
                var normB = Math.Sqrt((b.X * b.X) + (b.Y * b.Y) + (b.Z * b.Z));

                if (normA == 0.0 || normB == 0.0)
                {
                    // an undefined axis agrees only with another undefined axis
                    return normA == 0.0 && normB == 0.0 ? 0.0 : 180.0;
                }

                var cosine = ((a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z)) / (normA * normB);
                cosine = Math.Max(-1.0, Math.Min(1.0, cosine));

                return Math.Acos(cosine) * 180.0 / Math.PI;
            }

            private StructuralComparisonRow Names(string rowId, string what, Func<StructuralReadbackMember, string> read)
            {
                if (!_nodesPaired)
                {
                    return Build(rowId, StructuralComparisonVerdict.Fail, null, null, new List<string> { UnpairedNodes }, null);
                }

                var differences = new List<string>();

                foreach (var pair in _pairs)
                {
                    var generated = read(pair.Generated);
                    var manual = read(pair.Manual);

                    if (!string.Equals(generated, manual, StringComparison.Ordinal))
                    {
                        differences.Add(
                            "member " + pair.Generated.Id + " (manual " + pair.Manual.Id + "): " + what + " '" + generated + "' against '" + manual + "'");
                    }
                }

                return Build(rowId, differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail, null, null, differences, null);
            }

            private StructuralComparisonRow Defaults()
            {
                var differences = new List<string>();

                foreach (var member in _generated.Members)
                {
                    CheckDefaults("generated", member, differences);
                }

                foreach (var member in _manual.Members)
                {
                    CheckDefaults("manual", member, differences);
                }

                return Build(
                    StructuralComparisonRowIds.MemberDefaults,
                    differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail,
                    null,
                    null,
                    differences,
                    "Both models must carry the defaults exactly: centroid, no offsets, stiffness factors of 1, normal axial behaviour, no springs.");
            }

            private static void CheckDefaults(string side, StructuralReadbackMember member, List<string> differences)
            {
                var prefix = side + " member " + member.Id + ": ";

                if (member.CardinalPoint != 0)
                {
                    differences.Add(prefix + "cardinal point " + member.CardinalPoint.ToString(CultureInfo.InvariantCulture) + " (expected 0, the centroid)");
                }

                foreach (var offset in new[] { ("offset I", member.OffsetI), ("offset J", member.OffsetJ) })
                {
                    if (offset.Item2.X.Inches != 0.0 || offset.Item2.Y.Inches != 0.0 || offset.Item2.Z.Inches != 0.0)
                    {
                        differences.Add(
                            prefix + offset.Item1 + " (" + Format(offset.Item2.X.Inches) + ", " + Format(offset.Item2.Y.Inches) + ", "
                            + Format(offset.Item2.Z.Inches) + ") in (expected 0)");
                    }
                }

                if (member.StiffnessFactors.Any(factor => factor != 1.0))
                {
                    differences.Add(prefix + "stiffness factors (" + string.Join(", ", member.StiffnessFactors.Select(Format)) + ") (expected 1)");
                }

                if (member.AxialBehavior != StructuralReadbackAxialBehavior.Normal)
                {
                    differences.Add(prefix + "axial behaviour " + member.AxialBehavior + " (expected Normal)");
                }

                if (member.RotationalSpringI != 0.0 || member.RotationalSpringJ != 0.0)
                {
                    differences.Add(prefix + "rotational springs (" + Format(member.RotationalSpringI) + ", " + Format(member.RotationalSpringJ) + ") (expected 0)");
                }
            }

            private StructuralComparisonRow ReleasesAndSupports()
            {
                if (!_nodesPaired)
                {
                    return Build(StructuralComparisonRowIds.ReleasesSupports, StructuralComparisonVerdict.Fail, null, null, new List<string> { UnpairedNodes }, null);
                }

                var differences = new List<string>();

                foreach (var node in _generated.Nodes)
                {
                    var counterpart = _manual.Nodes.Single(m => string.Equals(m.Id, _nodeMap[node.Id], StringComparison.Ordinal));

                    if (!node.SupportFlags.SequenceEqual(counterpart.SupportFlags))
                    {
                        differences.Add(
                            "node " + node.Id + " (manual " + counterpart.Id + "): support flags " + Flags(node.SupportFlags) + " against "
                            + Flags(counterpart.SupportFlags));
                    }
                }

                foreach (var pair in _pairs)
                {
                    if (!pair.Generated.Releases.SequenceEqual(pair.Manual.Releases))
                    {
                        differences.Add(
                            "member " + pair.Generated.Id + " (manual " + pair.Manual.Id + "): releases " + Flags(pair.Generated.Releases)
                            + " against " + Flags(pair.Manual.Releases));
                    }
                }

                return Build(
                    StructuralComparisonRowIds.ReleasesSupports,
                    differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail,
                    null,
                    null,
                    differences,
                    null);
            }

            private static string Flags(IReadOnlyList<bool> flags) => string.Concat(flags.Select(flag => flag ? "1" : "0"));

            // ------------------------------------------------------------ loads and cases

            private StructuralComparisonRow Loads()
            {
                var generatedCases = _generated.LoadCases.Where(c => c.Loads.Count > 0).ToList();
                var manualCases = _manual.LoadCases.Where(c => c.Loads.Count > 0).ToList();

                if (generatedCases.Count == 0 && manualCases.Count == 0)
                {
                    return Build(StructuralComparisonRowIds.Loads, StructuralComparisonVerdict.Pass, 0.0, "percent", new List<string>(), "Neither model has a case with loads.");
                }

                if (!_nodesPaired)
                {
                    return Build(StructuralComparisonRowIds.Loads, StructuralComparisonVerdict.Fail, null, null, new List<string> { UnpairedNodes }, null);
                }

                var differences = new List<string>();
                var maxValue = 0.0;
                var maxPosition = 0.0;
                var casePairs = new List<(StructuralReadbackLoadCase Generated, StructuralReadbackLoadCase Manual)>();

                if (generatedCases.Count == 1 && manualCases.Count == 1)
                {
                    casePairs.Add((generatedCases[0], manualCases[0]));
                }
                else
                {
                    foreach (var generated in generatedCases)
                    {
                        var counterpart = manualCases.FirstOrDefault(m => string.Equals(m.Id, generated.Id, StringComparison.Ordinal));

                        if (counterpart == null)
                        {
                            differences.Add("generated case " + generated.Id + " has loads and no manual case with the same name has");
                        }
                        else
                        {
                            casePairs.Add((generated, counterpart));
                        }
                    }

                    foreach (var manual in manualCases.Where(m => !generatedCases.Any(g => string.Equals(g.Id, m.Id, StringComparison.Ordinal))))
                    {
                        differences.Add("manual case " + manual.Id + " has loads and no generated case with the same name has");
                    }
                }

                foreach (var pair in casePairs)
                {
                    CompareCaseLoads(pair.Generated, pair.Manual, differences, ref maxValue, ref maxPosition);
                }

                return Build(
                    StructuralComparisonRowIds.Loads,
                    differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail,
                    maxValue,
                    "percent",
                    differences,
                    "Largest difference of a load value, in percent of the manual value"
                    + (_zeroBaseCompared
                        ? " (a manual value of zero is compared in absolute value, limit " + ZeroLoadLimitText + " lbf; largest absolute value against it "
                          + Format(_maxZeroBaseLbf) + " lbf)"
                        : string.Empty)
                    + "; largest difference of a load position " + Format(maxPosition) + " in.");
            }

            private void CompareCaseLoads(
                StructuralReadbackLoadCase generated,
                StructuralReadbackLoadCase manual,
                List<string> differences,
                ref double maxValue,
                ref double maxPosition)
            {
                var manualMembersWithLoads = manual.Loads.Select(l => l.MemberId).Distinct(StringComparer.Ordinal).ToList();
                var visited = new HashSet<string>(StringComparer.Ordinal);

                foreach (var pair in _pairs)
                {
                    var generatedLoads = generated.Loads.Where(l => string.Equals(l.MemberId, pair.Generated.Id, StringComparison.Ordinal)).ToList();
                    var manualLoads = manual.Loads.Where(l => string.Equals(l.MemberId, pair.Manual.Id, StringComparison.Ordinal)).ToList();
                    visited.Add(pair.Manual.Id);

                    if (generatedLoads.Count == 0 && manualLoads.Count == 0)
                    {
                        continue;
                    }

                    var label = "member " + pair.Generated.Id + " (manual " + pair.Manual.Id + ")";

                    if (generatedLoads.Count != manualLoads.Count)
                    {
                        differences.Add(
                            label + ": " + generatedLoads.Count.ToString(CultureInfo.InvariantCulture) + " loads against "
                            + manualLoads.Count.ToString(CultureInfo.InvariantCulture));
                        continue;
                    }

                    var used = new bool[manualLoads.Count];

                    foreach (var load in generatedLoads)
                    {
                        var best = -1;

                        for (var index = 0; index < manualLoads.Count; index++)
                        {
                            if (!used[index]
                                && (best < 0 || Math.Abs(manualLoads[index].DistanceFromNodeI.Inches - load.DistanceFromNodeI.Inches)
                                    < Math.Abs(manualLoads[best].DistanceFromNodeI.Inches - load.DistanceFromNodeI.Inches)))
                            {
                                best = index;
                            }
                        }

                        used[best] = true;
                        var counterpart = manualLoads[best];

                        var positionDelta = Math.Abs(counterpart.DistanceFromNodeI.Inches - load.DistanceFromNodeI.Inches);
                        maxPosition = Math.Max(maxPosition, positionDelta);

                        if (load.Axis != counterpart.Axis)
                        {
                            differences.Add(label + ": direction " + load.Axis + " against " + counterpart.Axis);
                        }

                        var generatedValue = load.Value.PoundsForce;
                        var manualValue = counterpart.Value.PoundsForce;

                        if (manualValue == 0.0)
                        {
                            // the manual value is the base of the comparison: with a base of zero the value is compared in absolute value, never divided
                            var absolute = Math.Abs(generatedValue);
                            _zeroBaseCompared = true;
                            _maxZeroBaseLbf = Math.Max(_maxZeroBaseLbf, absolute);

                            if (absolute > ZeroLoadAbsoluteToleranceLbf)
                            {
                                differences.Add(
                                    label + ": value " + Format(generatedValue) + " lbf against 0 lbf (absolute difference " + Format(absolute)
                                    + " lbf, limit " + ZeroLoadLimitText + " lbf)");
                            }
                        }
                        else
                        {
                            var valueDelta = 100.0 * Math.Abs(generatedValue - manualValue) / Math.Abs(manualValue);
                            maxValue = Math.Max(maxValue, valueDelta);

                            if (valueDelta > LoadValueTolerancePercent + Epsilon)
                            {
                                differences.Add(
                                    label + ": value " + Format(generatedValue) + " lbf against " + Format(manualValue)
                                    + " lbf (" + Format(valueDelta) + " percent of the manual value, limit " + Format(LoadValueTolerancePercent) + ")");
                            }
                        }

                        if (positionDelta > LoadPositionToleranceInches + Epsilon)
                        {
                            differences.Add(
                                label + ": position " + Format(load.DistanceFromNodeI.Inches) + " in against " + Format(counterpart.DistanceFromNodeI.Inches)
                                + " in from node I (limit " + Format(LoadPositionToleranceInches) + ")");
                        }
                    }
                }

                foreach (var memberId in manualMembersWithLoads.Where(id => !visited.Contains(id)))
                {
                    differences.Add("manual member " + memberId + " carries loads but has no generated counterpart");
                }

                foreach (var load in generated.Loads.Where(l => !_pairs.Any(p => string.Equals(p.Generated.Id, l.MemberId, StringComparison.Ordinal))))
                {
                    differences.Add("generated member " + load.MemberId + " carries loads but has no manual counterpart");
                }
            }

            private StructuralComparisonRow Combinations()
            {
                if (!_options.GeneratedCombinationsRead || !_options.ManualCombinationsRead)
                {
                    var which = !_options.GeneratedCombinationsRead && !_options.ManualCombinationsRead
                        ? "either dump"
                        : (!_options.GeneratedCombinationsRead ? "the generated dump" : "the manual dump");

                    return Build(
                        StructuralComparisonRowIds.Combinations,
                        StructuralComparisonVerdict.NotEvaluated,
                        null,
                        null,
                        new List<string>(),
                        "The combinations could not be read by " + which + "; the engineer checks them visually in the program interface.");
                }

                var differences = new List<string>();

                if (_generated.Combinations.Count > 0)
                {
                    differences.Add("the generated model has combinations: " + string.Join(", ", _generated.Combinations.Select(c => c.Id)));
                }

                if (_manual.Combinations.Count > 0)
                {
                    differences.Add("the manual model has combinations: " + string.Join(", ", _manual.Combinations.Select(c => c.Id)));
                }

                return Build(
                    StructuralComparisonRowIds.Combinations,
                    differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail,
                    null,
                    null,
                    differences,
                    null);
            }

            private StructuralComparisonRow CaseCategory()
            {
                var differences = new List<string>();
                var cases = _generated.LoadCases.Where(c => c.Loads.Count > 0).ToList();

                if (cases.Count == 0)
                {
                    differences.Add("the generated model has no case with loads");
                }

                foreach (var loadCase in cases.Where(c => !string.Equals(c.Category, GeneratedCaseCategory, StringComparison.Ordinal)))
                {
                    differences.Add("generated case " + loadCase.Id + " has category '" + loadCase.Category + "' (expected '" + GeneratedCaseCategory + "')");
                }

                return Build(
                    StructuralComparisonRowIds.CaseCategory,
                    differences.Count == 0 ? StructuralComparisonVerdict.Pass : StructuralComparisonVerdict.Fail,
                    null,
                    null,
                    differences,
                    null);
            }

            private StructuralComparisonRow ManualCaseCategory()
            {
                var cases = _manual.LoadCases.Where(c => c.Loads.Count > 0).ToList();
                var note = cases.Count == 0
                    ? "The manual model has no case with loads."
                    : "Manual case category: " + string.Join(", ", cases.Select(c => c.Category))
                      + " (informative; the program never read back a category created in its interface).";

                return Build(StructuralComparisonRowIds.ManualCaseCategory, StructuralComparisonVerdict.Informative, null, null, new List<string>(), note);
            }

            private StructuralComparisonRow SelfWeight()
            {
                var parts = new List<string>();

                foreach (var loadCase in _generated.LoadCases.Where(c => c.Loads.Count > 0))
                {
                    parts.Add("generated case " + loadCase.Id + " " + Vector(loadCase.SelfWeight));
                }

                foreach (var loadCase in _manual.LoadCases.Where(c => c.Loads.Count > 0))
                {
                    parts.Add("manual case " + loadCase.Id + " " + Vector(loadCase.SelfWeight));
                }

                var note = parts.Count == 0 ? "No case with loads." : "Self weight multipliers: " + string.Join("; ", parts) + " (informative).";

                return Build(StructuralComparisonRowIds.SelfWeight, StructuralComparisonVerdict.Informative, null, null, new List<string>(), note);
            }

            private static string Vector(StructuralDirection direction) =>
                "(" + Format(direction.X) + ", " + Format(direction.Y) + ", " + Format(direction.Z) + ")";

            // ------------------------------------------------------------ rows

            private static StructuralComparisonRow Build(
                string id, StructuralComparisonVerdict verdict, double? maxDelta, string unit, List<string> differences, string note)
            {
                var name = RowTable.Single(entry => string.Equals(entry.Id, id, StringComparison.Ordinal)).Name;

                return new StructuralComparisonRow(
                    id,
                    name,
                    verdict,
                    maxDelta,
                    unit,
                    differences.Count,
                    new ReadOnlyCollection<string>(differences.Take(StructuralComparisonReport.MaxListedDifferences).ToList()),
                    note);
            }
        }
    }
}
