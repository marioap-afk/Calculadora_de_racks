using System.Globalization;

namespace I52Ct21d.HostFacts.Core;

/// <summary>One intended scale triple of the TOL_SCALE probe tables (design 2.4). Values are binary64 patterns, never decimal text.</summary>
public sealed record ProbeRow(
    string Id,
    string Family,
    string SignPattern,
    string Component,
    int? K,
    bool InScope,
    IReadOnlyList<string> Intended);

/// <summary>
/// The probe tables PF-1 (resolution ladder, 132 rows) and PF-2 (magnitude, 15 rows) of design 2.4 as a pure generator (I-0). The
/// values are TEST INPUTS defined by bit pattern (FM-6: no decimal parsing); they are not tolerances. NOTHING here writes to a
/// host: the probe itself (I-6, class RS) is blocked by Q-O-1 and is NOT implemented in this folder.
/// </summary>
public static class ProbeTable
{
    public static readonly IReadOnlyList<int> LadderLevels = new[] { 52, 48, 44, 40, 36, 32, 30, 29, 28, 24, 20 };

    private static readonly (string Sign, double Sx, double Sy)[] Signs =
    {
        ("++", 1.0, 1.0), ("-+", -1.0, 1.0), ("+-", 1.0, -1.0), ("--", -1.0, -1.0),
    };

    // design 2.4: base, exact bit patterns; ONLY base 1 is in scope, the other four are characterization.
    private static readonly (string Name, ulong Bits, bool InScope)[] Bases =
    {
        ("1", 0x3FF0000000000000UL, true),
        ("0.5", 0x3FE0000000000000UL, false),
        ("2", 0x4000000000000000UL, false),
        ("25.4", 0x4039666666666666UL, false),
        ("1/25.4", 0x3FA42850A142850AUL, false),
    };

    public static IReadOnlyList<ProbeRow> Generate()
    {
        var rows = new List<ProbeRow>();
        var n = 0;
        foreach (var (sign, sx, sy) in Signs)
        {
            var sigma = new[] { sx, sy, 1.0 };
            for (var c = 0; c < 3; c++)
            {
                foreach (var k in LadderLevels)
                {
                    n++;
                    var triple = new[] { sx, sy, 1.0 };
                    triple[c] = sigma[c] * (1.0 + Math.Pow(2.0, -k)); // 2^-k and 1 + 2^-k are exact binary64 for k <= 52
                    rows.Add(new ProbeRow("PF1-" + n.ToString("D4", CultureInfo.InvariantCulture), "PF1", sign, "XYZ"[c].ToString(), k, true,
                        triple.Select(DoubleBits.Hex).ToList()));
                }
            }
        }

        n = 0;
        foreach (var (_, bits, inScope) in Bases)
        {
            var b = BitConverter.UInt64BitsToDouble(bits);
            var up = BitConverter.UInt64BitsToDouble(bits + 1);
            var down = BitConverter.UInt64BitsToDouble(bits - 1);
            foreach (var (component, x) in new[] { ("ALL", (double?)null), ("X", up), ("X", down) })
            {
                n++;
                var triple = x is null
                    ? new[] { DoubleBits.Hex(b), DoubleBits.Hex(b), DoubleBits.Hex(b) }
                    : new[] { DoubleBits.Hex(x.Value), DoubleBits.Hex(b), DoubleBits.Hex(b) };
                rows.Add(new ProbeRow("PF2-" + n.ToString("D4", CultureInfo.InvariantCulture), "PF2", "++", component, null, inScope, triple));
            }
        }
        return rows;
    }
}

/// <summary>What a probe read back at the two observation points (design 2.3 OP1 / OP2), by row id. A null triple is an unread row.</summary>
public sealed record ProbeObservation(string Id, IReadOnlyList<string>? Op1, IReadOnlyList<string>? Op2);

public sealed record ProbeStatisticsResult(
    bool Complete,
    double? DaOp1,
    double? DaOp2,
    double? Da,
    double? DaChar,
    int Kpres,
    int BitLevelSignedZeroRows,
    bool DeltaExactEverywhere,
    bool EquivalenceDaZeroIffKpres52);

/// <summary>
/// The decision statistics of design 2.3 computed offline from observed rows (I-0): <c>D_A</c> = max |read - intended| over the
/// IN-SCOPE rows at both points, <c>DA_char</c> over the characterization rows (informative only), <c>K_pres</c> = the finest ladder
/// level k such that every in-scope PF-1 row at every level not finer than k is read back bit-identical at both points (0 when not even
/// the coarsest level is). Sterbenz exactness of each difference is checked. This module computes numbers from data; it chooses no
/// tolerance, applies no decision rule and is not a verdict.
/// </summary>
public static class ProbeStatistics
{
    public static ProbeStatisticsResult Compute(IReadOnlyList<ProbeRow> rows, IReadOnlyList<ProbeObservation> observations)
    {
        var byId = observations.ToDictionary(o => o.Id, StringComparer.Ordinal);
        var complete = true;
        double? daOp1 = 0, daOp2 = 0, daChar = 0;
        var signedZero = 0;
        var exactEverywhere = true;
        var identicalAtLevel = ProbeTable.LadderLevels.ToDictionary(k => k, _ => true);

        foreach (var row in rows)
        {
            if (!byId.TryGetValue(row.Id, out var obs) || !TryParse(obs.Op1, out var op1) || !TryParse(obs.Op2, out var op2))
            {
                complete = false;
                if (row.InScope) { daOp1 = daOp2 = null; }
                else daChar = null;
                if (row.Family == "PF1" && row.K is { } kk) identicalAtLevel[kk] = false;
                continue;
            }
            var intended = row.Intended.Select(DoubleBits.FromHex).ToArray();
            var rowDelta1 = MaxDelta(intended, op1, ref exactEverywhere);
            var rowDelta2 = MaxDelta(intended, op2, ref exactEverywhere);
            var identical = BitsEqual(row.Intended, obs.Op1!) && BitsEqual(row.Intended, obs.Op2!);
            if (!identical && SameNumericValue(intended, op1) && SameNumericValue(intended, op2)) signedZero++;

            if (row.InScope)
            {
                if (daOp1 is not null) daOp1 = Math.Max(daOp1.Value, rowDelta1);
                if (daOp2 is not null) daOp2 = Math.Max(daOp2.Value, rowDelta2);
            }
            else if (daChar is not null)
            {
                daChar = Math.Max(daChar.Value, Math.Max(rowDelta1, rowDelta2));
            }
            if (row.Family == "PF1" && row.InScope && row.K is { } k && !identical) identicalAtLevel[k] = false;
        }

        var kpres = 0;
        foreach (var level in ProbeTable.LadderLevels.OrderBy(x => x)) // coarse (small k) to fine (large k)
        {
            if (!identicalAtLevel[level]) break;
            kpres = level;
        }
        double? da = daOp1 is null || daOp2 is null ? null : Math.Max(daOp1.Value, daOp2.Value);
        var equivalence = da is null || ((da == 0.0) == (kpres == 52));
        return new ProbeStatisticsResult(complete, daOp1, daOp2, da, daChar, kpres, signedZero, exactEverywhere, equivalence);
    }

    private static bool TryParse(IReadOnlyList<string>? hex, out double[] values)
    {
        values = Array.Empty<double>();
        if (hex is null || hex.Count != 3) return false;
        var v = hex.Select(DoubleBits.FromHex).ToArray();
        if (v.Any(x => !double.IsFinite(x))) return false;
        values = v;
        return true;
    }

    private static bool BitsEqual(IReadOnlyList<string> a, IReadOnlyList<string> b) =>
        a.Count == b.Count && a.Zip(b).All(p => string.Equals(p.First, p.Second, StringComparison.Ordinal));

    private static bool SameNumericValue(double[] a, double[] b) => a.Zip(b).All(p => p.First == p.Second);

    private static double MaxDelta(double[] intended, double[] read, ref bool exactEverywhere)
    {
        var max = 0.0;
        for (var i = 0; i < 3; i++)
        {
            var a = intended[i];
            var b = read[i];
            var delta = Math.Abs(b - a);
            // Sterbenz: the subtraction is exact when a and b have the same sign and a/2 <= b <= 2a
            var exact = delta == 0.0 || (Math.Sign(a) == Math.Sign(b) && Math.Abs(a) / 2 <= Math.Abs(b) && Math.Abs(b) <= 2 * Math.Abs(a));
            if (!exact) exactEverywhere = false;
            if (delta > max) max = delta;
        }
        return max;
    }
}
