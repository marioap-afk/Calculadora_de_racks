using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

/// <summary>Arithmetic of one probe row (design 2.3 and 2.4). Binary64 patterns in, binary64 patterns out; no decimal parsing anywhere (FM-6).</summary>
public static class TolScaleRowMath
{
    /// <summary>
    /// The witness bound (design 2.4 S5: "the witness bound is the witness's own arithmetic error and is stated in the instrument
    /// specification"): the column length of <c>BlockTransform</c> is a square root of a sum of three squares, whose error is below 2 ulp;
    /// 8 ulp of <c>max(1, |s|)</c> leaves room for the matrix product. The smallest offset the witness is asked to resolve is 2^-36
    /// (about 1.5e-11), four orders of magnitude above this bound.
    /// </summary>
    public const double WitnessUlps = 8.0;

    public const int WitnessMaxLevel = 36;

    /// <summary>The offset (as a binary64) a PF-1 row adds at its level, and for a PF-2 row the coarsest offset of the ladder (2^-20).</summary>
    public static double OwnLevelOffset(ProbeRow row) => Math.Pow(2.0, -(row.K ?? 20));

    public static double[] Values(IReadOnlyList<string> hex)
    {
        var v = new double[hex.Count];
        for (var i = 0; i < hex.Count; i++) v[i] = DoubleBits.FromHex(hex[i]);
        return v;
    }

    public static bool AllFinite(IReadOnlyList<string> hex)
    {
        foreach (var x in Values(hex))
            if (!double.IsFinite(x)) return false;
        return true;
    }

    /// <summary>|read - intended| per component, as binary64 patterns. The difference of two finite doubles never overflows here (|values| are near 1).</summary>
    public static string[] DeltaHex(IReadOnlyList<string> intended, IReadOnlyList<string> read)
    {
        var a = Values(intended);
        var b = Values(read);
        var result = new string[a.Length];
        for (var i = 0; i < a.Length; i++) result[i] = DoubleBits.Hex(Math.Abs(b[i] - a[i]));
        return result;
    }

    /// <summary>Sterbenz: the subtraction is exact when both have the same sign and a/2 &lt;= b &lt;= 2a (or the difference is zero).</summary>
    public static bool SterbenzExact(double a, double b)
    {
        var delta = Math.Abs(b - a);
        if (delta == 0.0) return true;
        return Math.Sign(a) == Math.Sign(b) && Math.Abs(a) / 2 <= Math.Abs(b) && Math.Abs(b) <= 2 * Math.Abs(a);
    }

    public static bool AllSterbenzExact(IReadOnlyList<string> intended, IReadOnlyList<string> read)
    {
        var a = Values(intended);
        var b = Values(read);
        for (var i = 0; i < a.Length; i++)
            if (!SterbenzExact(a[i], b[i])) return false;
        return true;
    }

    public static bool BitsEqual(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        if (a.Count != b.Count) return false;
        for (var i = 0; i < a.Count; i++)
            if (!string.Equals(a[i], b[i], StringComparison.Ordinal)) return false;
        return true;
    }

    /// <summary>Rotation equal to 0 (either zero) and normal equal to +Z, as numbers (design FM-7, V3).</summary>
    public static bool IsIdentityPose(string rotationHex, IReadOnlyList<string> normalHex)
    {
        if (normalHex.Count != 3) return false;
        var n = Values(normalHex);
        return DoubleBits.FromHex(rotationHex) == 0.0 && n[0] == 0.0 && n[1] == 0.0 && n[2] == 1.0;
    }

    public static bool WitnessApplies(ProbeRow row) => row.Family == "PF1" && row.K is { } k && k <= WitnessMaxLevel;

    /// <summary>AGREE / DISAGREE / NOT_APPLICABLE (design 2.4 S5): the column lengths of <c>BlockTransform</c> against the absolute value of the scale read back.</summary>
    public static string Witness(ProbeRow row, ReferenceReading? op1)
    {
        if (!WitnessApplies(row)) return "NOT_APPLICABLE";
        if (op1?.ScaleHex is null || op1.ColumnLengthsHex is null || op1.ColumnLengthsHex.Count != 3 || op1.ScaleHex.Count != 3) return "DISAGREE";
        var s = Values(op1.ScaleHex);
        var c = Values(op1.ColumnLengthsHex);
        for (var i = 0; i < 3; i++)
        {
            if (!double.IsFinite(s[i]) || !double.IsFinite(c[i])) return "DISAGREE";
            var bound = WitnessUlps * Math.Pow(2.0, -52) * Math.Max(1.0, Math.Abs(s[i]));
            if (Math.Abs(c[i] - Math.Abs(s[i])) > bound) return "DISAGREE";
        }
        return "AGREE";
    }

    /// <summary>
    /// The FAIL criterion of design 2.5: a row read back with a different sign, or a magnitude that differs from the intended one by MORE than
    /// the offset of its own level (the host transforms or canonizes the value; this is not noise). The magnitude test applies to the in-scope rows only; the
    /// sign test applies to every row.
    /// </summary>
    public static bool Contradicts(ProbeRow row, IReadOnlyList<string> read)
    {
        var a = Values(row.Intended);
        var b = Values(read);
        var offset = OwnLevelOffset(row);
        for (var i = 0; i < a.Length; i++)
        {
            if (Math.Sign(a[i]) != Math.Sign(b[i])) return true; // the sign test covers all 147 rows
            if (row.InScope && Math.Abs(b[i] - a[i]) > offset) return true; // the magnitude test covers the in-scope rows only (design 2.3: the 12 characterization rows enter no branch of the rule)
        }
        return false;
    }
}

/// <summary>The full result for one probe row, ready for the record and for the statistics.</summary>
public sealed class TolScaleRowResult
{
    public ProbeRow Row { get; }
    public IReadOnlyList<string>? Op1 { get; }
    public IReadOnlyList<string>? Op2 { get; }
    public string? RotationHex { get; }
    public IReadOnlyList<string>? NormalHex { get; }
    public IReadOnlyList<string>? DeltaOp1 { get; }
    public IReadOnlyList<string>? DeltaOp2 { get; }
    public bool DeltaExact { get; }
    public bool BitIdentical { get; }
    public string Witness { get; }
    public bool Observed { get; }
    public string Reason { get; }
    public bool RejectedByHost { get; }
    public bool ContradictsDesign { get; }

    public TolScaleRowResult(ProbeRow row, IReadOnlyList<string>? op1, IReadOnlyList<string>? op2, string? rotationHex, IReadOnlyList<string>? normalHex,
        IReadOnlyList<string>? deltaOp1, IReadOnlyList<string>? deltaOp2, bool deltaExact, bool bitIdentical, string witness, bool observed, string reason,
        bool rejectedByHost, bool contradictsDesign)
    {
        Row = row;
        Op1 = op1;
        Op2 = op2;
        RotationHex = rotationHex;
        NormalHex = normalHex;
        DeltaOp1 = deltaOp1;
        DeltaOp2 = deltaOp2;
        DeltaExact = deltaExact;
        BitIdentical = bitIdentical;
        Witness = witness;
        Observed = observed;
        Reason = reason;
        RejectedByHost = rejectedByHost;
        ContradictsDesign = contradictsDesign;
    }
}

/// <summary>Turns what the host returned into per-row results and the decision statistics (design 2.3), without applying any decision rule.</summary>
public static class TolScaleAnalyzer
{
    public static IReadOnlyList<TolScaleRowResult> AnalyzeRows(
        IReadOnlyList<ProbeRow> rows, IReadOnlyList<RowConstruction> constructions, IReadOnlyList<ReferenceReading> op1, IReadOnlyList<ReferenceReading> op2)
    {
        var byHandle1 = new Dictionary<string, ReferenceReading>(StringComparer.Ordinal);
        foreach (var r in op1) byHandle1[r.Handle] = r;
        var byHandle2 = new Dictionary<string, ReferenceReading>(StringComparer.Ordinal);
        foreach (var r in op2) byHandle2[r.Handle] = r;
        var result = new List<TolScaleRowResult>();
        foreach (var row in rows)
        {
            var c = constructions.FirstOrDefault(x => x.RowId == row.Id);
            if (c is null)
            {
                result.Add(Unknown(row, "NO_CONSTRUCTION_RESULT", false));
                continue;
            }
            if (c.Rejection is not null || c.Handle is null)
            {
                result.Add(Unknown(row, "HOST_REJECTED_CONSTRUCTION:" + (c.Rejection ?? "NO_HANDLE"), true));
                continue;
            }
            byHandle1.TryGetValue(c.Handle, out var r1);
            byHandle2.TryGetValue(c.Handle, out var r2);
            result.Add(AnalyzeOne(row, r1, r2));
        }
        return result;
    }

    private static TolScaleRowResult Unknown(ProbeRow row, string reason, bool rejected) =>
        new(row, null, null, null, null, null, null, false, false, "NOT_APPLICABLE", false, reason, rejected, false);

    private static TolScaleRowResult AnalyzeOne(ProbeRow row, ReferenceReading? r1, ReferenceReading? r2)
    {
        var reasons = new List<string>();
        IReadOnlyList<string>? op1 = null;
        IReadOnlyList<string>? op2 = null;
        if (r1 is null) reasons.Add("OP1_NOT_READ");
        else if (r1.ReadError is not null || r1.ScaleHex is null || r1.ScaleHex.Count != 3) reasons.Add("OP1_READ_ERROR:" + (r1.ReadError ?? "NO_SCALE"));
        else op1 = r1.ScaleHex;
        if (r2 is null) reasons.Add("OP2_NOT_READ");
        else if (r2.ReadError is not null || r2.ScaleHex is null || r2.ScaleHex.Count != 3) reasons.Add("OP2_READ_ERROR:" + (r2.ReadError ?? "NO_SCALE"));
        else op2 = r2.ScaleHex;

        var finite1 = op1 is not null && TolScaleRowMath.AllFinite(op1);
        var finite2 = op2 is not null && TolScaleRowMath.AllFinite(op2);
        if (op1 is not null && !finite1) reasons.Add("OP1_NON_FINITE_VALUE");
        if (op2 is not null && !finite2) reasons.Add("OP2_NON_FINITE_VALUE");

        string? rotation = null;
        IReadOnlyList<string>? normal = null;
        if (r1 is not null && r1.RotationHex is not null && r1.NormalHex is not null)
        {
            rotation = r1.RotationHex;
            normal = r1.NormalHex;
        }
        else if (op1 is not null) reasons.Add("OP1_POSE_NOT_READ");
        var pose1 = rotation is not null && normal is not null && TolScaleRowMath.IsIdentityPose(rotation, normal);
        if (rotation is not null && !pose1) reasons.Add("OP1_ROTATION_OR_NORMAL_NOT_IDENTITY");
        if (r2 is not null && op2 is not null)
        {
            if (r2.RotationHex is null || r2.NormalHex is null) reasons.Add("OP2_POSE_NOT_READ");
            else if (!TolScaleRowMath.IsIdentityPose(r2.RotationHex, r2.NormalHex)) reasons.Add("OP2_ROTATION_OR_NORMAL_NOT_IDENTITY");
        }

        var witness = TolScaleRowMath.Witness(row, r1);
        if (witness == "DISAGREE") reasons.Add("WITNESS_DISAGREES");

        IReadOnlyList<string>? d1 = finite1 ? TolScaleRowMath.DeltaHex(row.Intended, op1!) : null;
        IReadOnlyList<string>? d2 = finite2 ? TolScaleRowMath.DeltaHex(row.Intended, op2!) : null;
        var observed = reasons.Count == 0;
        var exact = observed && TolScaleRowMath.AllSterbenzExact(row.Intended, op1!) && TolScaleRowMath.AllSterbenzExact(row.Intended, op2!);
        var identical = observed && TolScaleRowMath.BitsEqual(row.Intended, op1!) && TolScaleRowMath.BitsEqual(row.Intended, op2!);
        var contradicts = (finite1 && TolScaleRowMath.Contradicts(row, op1!)) || (finite2 && TolScaleRowMath.Contradicts(row, op2!));
        return new TolScaleRowResult(row, op1, op2, rotation, normal, d1, d2, exact, identical, witness, observed, string.Join(";", reasons), false, contradicts);
    }

    /// <summary>The statistics of design 2.3 over the rows OBSERVED at both points (an unobserved row makes the statistics incomplete, never zero).</summary>
    public static ProbeStatisticsResult Statistics(IReadOnlyList<TolScaleRowResult> results)
    {
        var rows = results.Select(r => r.Row).ToList();
        var observations = results.Select(r => new ProbeObservation(r.Row.Id, r.Observed ? r.Op1 : null, r.Observed ? r.Op2 : null)).ToList();
        return ProbeStatistics.Compute(rows, observations);
    }
}

/// <summary>The four classes of design 2.5. A statement about the measurement, never a verdict on the product.</summary>
public static class TolScaleResult
{
    public const string Pass = "PASS";
    public const string Fail = "FAIL";
    public const string Unknown = "UNKNOWN";
    public const string Invalid = "INVALID";
}

public sealed class TolScaleFacts
{
    public IReadOnlyList<string> InvalidReasons { get; }
    public int RowsRejectedByHost { get; }
    public int RowsContradicting { get; }
    public int RowsUnknown { get; }
    public bool StatisticsComplete { get; }

    public TolScaleFacts(IReadOnlyList<string> invalidReasons, int rowsRejectedByHost, int rowsContradicting, int rowsUnknown, bool statisticsComplete)
    {
        InvalidReasons = invalidReasons;
        RowsRejectedByHost = rowsRejectedByHost;
        RowsContradicting = rowsContradicting;
        RowsUnknown = rowsUnknown;
        StatisticsComplete = statisticsComplete;
    }
}

public sealed class TolScaleClassification
{
    public string Result { get; }
    public IReadOnlyList<string> Reasons { get; }
    public bool DecisionEligible => Result == TolScaleResult.Pass;

    public TolScaleClassification(string result, IReadOnlyList<string> reasons)
    {
        Result = result;
        Reasons = reasons;
    }
}

/// <summary>
/// The classification state machine of design 2.5. Precedence (the implementer's reading, flagged): INVALID (the execution cannot support a
/// conclusion) over FAIL (a valid, complete execution that contradicts a design assumption) over UNKNOWN (a required observation is missing)
/// over PASS. A construction the host rejects is a FAIL ("an error that is part of the data"), not an UNKNOWN.
/// </summary>
public static class TolScaleClassifier
{
    public static TolScaleClassification Classify(TolScaleFacts f)
    {
        if (f.InvalidReasons.Count > 0) return new TolScaleClassification(TolScaleResult.Invalid, f.InvalidReasons);
        var fail = new List<string>();
        if (f.RowsRejectedByHost > 0) fail.Add("ROWS_REJECTED_BY_HOST=" + f.RowsRejectedByHost);
        if (f.RowsContradicting > 0) fail.Add("ROWS_CONTRADICT_THE_DESIGN=" + f.RowsContradicting);
        if (fail.Count > 0) return new TolScaleClassification(TolScaleResult.Fail, fail);
        var unknown = new List<string>();
        if (f.RowsUnknown > 0) unknown.Add("ROWS_UNKNOWN=" + f.RowsUnknown);
        if (!f.StatisticsComplete) unknown.Add("STATISTICS_INCOMPLETE");
        if (unknown.Count > 0) return new TolScaleClassification(TolScaleResult.Unknown, unknown);
        return new TolScaleClassification(TolScaleResult.Pass, Array.Empty<string>());
    }
}
