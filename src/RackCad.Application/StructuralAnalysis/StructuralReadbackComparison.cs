using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>The verdict of one row of the comparison.</summary>
    public enum StructuralComparisonVerdict
    {
        Pass = 0,
        Fail = 1,
        Informative = 2,
        NotEvaluated = 3,
    }

    /// <summary>The overall outcome of the comparison.</summary>
    public enum StructuralComparisonOutcome
    {
        Pass = 0,
        Fail = 1,
        Stale = 2,
        Rejected = 3,
    }

    /// <summary>The stable identifiers of the rows of the report, in report order.</summary>
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

    /// <summary>What a dump could read; by default everything.</summary>
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

    /// <summary>One row of the report.</summary>
    public sealed class StructuralComparisonRow
    {
        internal StructuralComparisonRow(
            string id, string name, StructuralComparisonVerdict verdict, double? maxDelta, string unit, int differenceCount, IReadOnlyList<string> differences, string note)
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

        public double? MaxDelta { get; }

        public string Unit { get; }

        public int DifferenceCount { get; }

        public IReadOnlyList<string> Differences { get; }

        public string Note { get; }
    }

    /// <summary>The report of a comparison.</summary>
    public sealed class StructuralComparisonReport
    {
        public const string Schema = "rackcad-structural-comparison/v1";

        internal StructuralComparisonReport(StructuralComparisonOutcome outcome, string reason, IReadOnlyList<StructuralComparisonRow> rows)
        {
            Outcome = outcome;
            Reason = reason;
            Rows = rows;
        }

        public StructuralComparisonOutcome Outcome { get; }

        public string Reason { get; }

        public IReadOnlyList<StructuralComparisonRow> Rows { get; }

        public StructuralComparisonRow Row(string id) => throw new NotImplementedException();

        public string ToJson() => throw new NotImplementedException();
    }

    /// <summary>The comparator of two readbacks (I-65 D-11, D-13). Pure.</summary>
    public static class StructuralReadbackComparison
    {
        public const double NodePositionToleranceInches = 0.02;
        public const double AxisToleranceDegrees = 0.1;
        public const double LoadValueTolerancePercent = 0.1;
        public const double LoadPositionToleranceInches = 0.02;

        public static StructuralComparisonReport CompareReadbacks(
            StructuralModelReadback generated,
            StructuralModelReadback manual,
            StructuralTransferReadback transfer,
            string manifestAnalysisInputFingerprint,
            StructuralComparisonOptions options = null)
        {
            throw new NotImplementedException();
        }

        public static StructuralComparisonReport Rejected(string reason) => throw new NotImplementedException();
    }
}
