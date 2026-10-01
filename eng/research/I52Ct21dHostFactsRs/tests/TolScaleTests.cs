using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

public class TolScaleRowMathTests
{
    private static readonly IReadOnlyList<ProbeRow> Rows = ProbeTable.Generate();

    public static IEnumerable<object[]> RowIds() => ProbeTable.Generate().Select(r => new object[] { r.Id });

    private static ProbeRow Row(string id) => Rows.First(r => r.Id == id);

    [Fact]
    public void The_table_has_147_rows_135_in_scope_and_12_characterization()
    {
        Assert.Equal(147, Rows.Count);
        Assert.Equal(135, Rows.Count(r => r.InScope));
        Assert.Equal(12, Rows.Count(r => !r.InScope));
        Assert.Equal(132, Rows.Count(r => r.Family == "PF1"));
        Assert.Equal(15, Rows.Count(r => r.Family == "PF2"));
    }

    [Fact]
    public void The_147_rows_have_146_distinct_triples_as_the_design_states() =>
        Assert.Equal(146, Rows.Select(r => string.Join(",", r.Intended)).Distinct().Count());

    [Theory]
    [MemberData(nameof(RowIds))]
    public void Every_intended_value_is_finite_and_not_zero(string id)
    {
        foreach (var x in TolScaleRowMath.Values(Row(id).Intended))
        {
            Assert.True(double.IsFinite(x));
            Assert.NotEqual(0.0, x);
        }
    }

    [Theory]
    [MemberData(nameof(RowIds))]
    public void A_perfect_readback_is_observed_exact_and_bit_identical(string id)
    {
        var row = Row(id);
        var host = new FakeReading(row);
        var results = TolScaleAnalyzer.AnalyzeRows(new[] { row }, new[] { new RowConstruction(row.Id, "A1", null) }, new[] { host.Reading("A1") }, new[] { host.Reading("A1") });
        var r = Assert.Single(results);
        Assert.True(r.Observed, r.Reason);
        Assert.True(r.DeltaExact);
        Assert.True(r.BitIdentical);
        Assert.False(r.ContradictsDesign);
        Assert.False(r.RejectedByHost);
        Assert.All(r.DeltaOp1!, h => Assert.Equal(DoubleBits.Hex(0.0), h));
        Assert.Equal(TolScaleRowMath.WitnessApplies(row) ? "AGREE" : "NOT_APPLICABLE", r.Witness);
    }

    private sealed class FakeReading
    {
        private readonly ProbeRow _row;
        public FakeReading(ProbeRow row) => _row = row;

        public ReferenceReading Reading(string handle, IReadOnlyList<string>? scale = null)
        {
            var s = scale ?? _row.Intended;
            return new ReferenceReading(handle, s, DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) },
                s.Select(h => DoubleBits.Hex(Math.Abs(DoubleBits.FromHex(h)))).ToList(), null);
        }
    }

    [Theory]
    [InlineData(1.0, 1.0, true)]
    [InlineData(1.0, 0.5, true)]
    [InlineData(1.0, 2.0, true)]
    [InlineData(1.0, 2.0000001, false)]
    [InlineData(1.0, 0.4999999, false)]
    [InlineData(1.0, -1.0, false)]
    [InlineData(25.4, 25.4, true)]
    [InlineData(-1.0, -1.0, true)]
    [InlineData(-1.0, -1.5, true)]
    public void SterbenzExact_follows_the_lemma(double a, double b, bool expected) => Assert.Equal(expected, TolScaleRowMath.SterbenzExact(a, b));

    [Fact]
    public void SterbenzExact_is_true_for_a_zero_difference_even_across_a_signed_zero() => Assert.True(TolScaleRowMath.SterbenzExact(0.0, -0.0));

    [Theory]
    [InlineData("0000000000000000", true)]
    [InlineData("8000000000000000", true)]
    [InlineData("3FF0000000000000", false)]
    public void The_rotation_must_be_zero_either_zero(string rotation, bool expected)
    {
        var normal = new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) };
        Assert.Equal(expected, TolScaleRowMath.IsIdentityPose(rotation, normal));
    }

    [Theory]
    [InlineData(0.0, 0.0, 1.0, true)]
    [InlineData(0.0, 0.0, -1.0, false)]
    [InlineData(0.0, 1.0, 0.0, false)]
    [InlineData(1.0, 0.0, 0.0, false)]
    [InlineData(0.0, 0.0, 0.9999999999, false)]
    public void The_normal_must_be_plus_Z(double x, double y, double z, bool expected) =>
        Assert.Equal(expected, TolScaleRowMath.IsIdentityPose(DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(x), DoubleBits.Hex(y), DoubleBits.Hex(z) }));

    [Fact]
    public void A_normal_with_two_components_is_not_identity() =>
        Assert.False(TolScaleRowMath.IsIdentityPose(DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) }));

    [Theory]
    [InlineData("PF1-0001", false)] // ++ X k=52
    [InlineData("PF1-0004", false)] // k=40
    [InlineData("PF1-0005", true)]  // k=36
    [InlineData("PF1-0006", true)]  // k=32
    [InlineData("PF1-0011", true)]  // k=20
    [InlineData("PF2-0001", false)]
    public void The_witness_applies_to_PF1_rows_with_k_at_most_36(string id, bool expected) =>
        Assert.Equal(expected, TolScaleRowMath.WitnessApplies(Row(id)));

    [Fact]
    public void The_witness_covers_exactly_the_levels_36_and_coarser()
    {
        var covered = Rows.Where(TolScaleRowMath.WitnessApplies).Select(r => r.K!.Value).Distinct().OrderBy(k => k).ToArray();
        Assert.Equal(new[] { 20, 24, 28, 29, 30, 32, 36 }, covered);
        Assert.Equal(7 * 12, Rows.Count(TolScaleRowMath.WitnessApplies)); // 4 sign patterns x 3 components x 7 levels
    }

    private static ReferenceReading WithColumns(ProbeRow row, double delta)
    {
        var s = TolScaleRowMath.Values(row.Intended);
        return new ReferenceReading("H", row.Intended, DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) },
            s.Select(x => DoubleBits.Hex(Math.Abs(x) + delta)).ToList(), null);
    }

    [Fact]
    public void The_witness_agrees_within_8_ulp_and_disagrees_beyond()
    {
        var row = Rows.First(r => r.Id == "PF1-0006");
        Assert.True(TolScaleRowMath.WitnessApplies(row));
        var ulp = Math.Pow(2.0, -52);
        Assert.Equal("AGREE", TolScaleRowMath.Witness(row, WithColumns(row, 0.0)));
        Assert.Equal("AGREE", TolScaleRowMath.Witness(row, WithColumns(row, 7.0 * ulp)));
        Assert.Equal("DISAGREE", TolScaleRowMath.Witness(row, WithColumns(row, 9.0 * ulp)));
        Assert.Equal("DISAGREE", TolScaleRowMath.Witness(row, WithColumns(row, 1e-9)));
    }

    [Fact]
    public void The_witness_disagrees_when_its_data_is_missing()
    {
        var row = Rows.First(r => r.Id == "PF1-0006");
        Assert.Equal("DISAGREE", TolScaleRowMath.Witness(row, null));
        Assert.Equal("DISAGREE", TolScaleRowMath.Witness(row, new ReferenceReading("H", row.Intended, null, null, null, null)));
    }

    [Fact]
    public void The_witness_is_not_applicable_to_a_fine_level()
    {
        var row = Rows.First(r => r.K == 52);
        Assert.Equal("NOT_APPLICABLE", TolScaleRowMath.Witness(row, null));
    }

    [Theory]
    [InlineData(52, 1.0 / 4503599627370496.0)]
    [InlineData(30, 1.0 / 1073741824.0)]
    [InlineData(20, 1.0 / 1048576.0)]
    public void OwnLevelOffset_is_two_to_the_minus_k(int k, double expected)
    {
        var row = Rows.First(r => r.K == k);
        Assert.Equal(expected, TolScaleRowMath.OwnLevelOffset(row));
    }

    [Fact]
    public void A_PF2_row_uses_the_coarsest_offset_of_the_ladder()
    {
        var row = Rows.First(r => r.Family == "PF2");
        Assert.Equal(Math.Pow(2.0, -20), TolScaleRowMath.OwnLevelOffset(row));
    }

    [Fact]
    public void Contradicts_is_false_for_a_deviation_equal_to_the_level_offset()
    {
        var row = Rows.First(r => r.K == 30 && r.SignPattern == "++" && r.Component == "X");
        var read = new[] { DoubleBits.Hex(1.0), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) }; // snapped to 1: delta == 2^-30 == the offset
        Assert.False(TolScaleRowMath.Contradicts(row, read));
    }

    [Fact]
    public void Contradicts_is_true_for_a_deviation_beyond_the_level_offset()
    {
        var row = Rows.First(r => r.K == 30 && r.SignPattern == "++" && r.Component == "X");
        var read = new[] { DoubleBits.Hex(1.0 + 3 * Math.Pow(2.0, -30)), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) };
        Assert.True(TolScaleRowMath.Contradicts(row, read));
    }

    [Theory]
    [InlineData("++", "-+")]
    [InlineData("-+", "++")]
    [InlineData("+-", "++")]
    [InlineData("--", "++")]
    public void Contradicts_is_true_when_a_sign_flips(string sign, string readSign)
    {
        var row = Rows.First(r => r.SignPattern == sign && r.Family == "PF1" && r.K == 52 && r.Component == "Z");
        var s = TolScaleRowMath.Values(row.Intended);
        var f = readSign == "++" ? new[] { 1.0, 1.0 } : readSign == "-+" ? new[] { -1.0, 1.0 } : readSign == "+-" ? new[] { 1.0, -1.0 } : new[] { -1.0, -1.0 };
        var read = new[] { DoubleBits.Hex(Math.Abs(s[0]) * f[0]), DoubleBits.Hex(Math.Abs(s[1]) * f[1]), DoubleBits.Hex(s[2]) };
        Assert.True(TolScaleRowMath.Contradicts(row, read));
    }

    [Fact]
    public void Contradicts_is_true_for_a_zero_read_back()
    {
        var row = Rows[0];
        Assert.True(TolScaleRowMath.Contradicts(row, new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) }));
    }

    [Fact]
    public void DeltaHex_is_the_absolute_difference_per_component()
    {
        var d = TolScaleRowMath.DeltaHex(new[] { DoubleBits.Hex(1.0), DoubleBits.Hex(-1.0), DoubleBits.Hex(1.0) }, new[] { DoubleBits.Hex(1.5), DoubleBits.Hex(-1.25), DoubleBits.Hex(1.0) });
        Assert.Equal(new[] { DoubleBits.Hex(0.5), DoubleBits.Hex(0.25), DoubleBits.Hex(0.0) }, d);
    }

    [Fact]
    public void DeltaHex_never_reports_a_negative_zero()
    {
        var d = TolScaleRowMath.DeltaHex(new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) }, new[] { DoubleBits.Hex(-0.0), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) });
        Assert.Equal(DoubleBits.Hex(0.0), d[0]);
    }

    [Fact]
    public void AllFinite_detects_nan_and_infinity()
    {
        Assert.True(TolScaleRowMath.AllFinite(new[] { DoubleBits.Hex(1.0) }));
        Assert.False(TolScaleRowMath.AllFinite(new[] { DoubleBits.Hex(double.NaN) }));
        Assert.False(TolScaleRowMath.AllFinite(new[] { DoubleBits.Hex(1.0), DoubleBits.Hex(double.PositiveInfinity) }));
    }
}

public class TolScaleAnalyzerTests
{
    private static readonly IReadOnlyList<ProbeRow> Rows = ProbeTable.Generate();

    private static (IReadOnlyList<RowConstruction>, IReadOnlyList<ReferenceReading>) Perfect(Func<ProbeRow, IReadOnlyList<string>>? change = null)
    {
        var c = new List<RowConstruction>();
        var r = new List<ReferenceReading>();
        for (var i = 0; i < Rows.Count; i++)
        {
            var scale = change?.Invoke(Rows[i]) ?? Rows[i].Intended;
            c.Add(new RowConstruction(Rows[i].Id, "H" + i, null));
            r.Add(new ReferenceReading("H" + i, scale, DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) },
                scale.Select(h => DoubleBits.Hex(Math.Abs(DoubleBits.FromHex(h)))).ToList(), null));
        }
        return (c, r);
    }

    [Fact]
    public void A_perfect_host_gives_DA_zero_and_Kpres_52_for_all_147_rows()
    {
        var (c, r) = Perfect();
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, r, r);
        Assert.All(results, x => Assert.True(x.Observed, x.Row.Id + " " + x.Reason));
        var s = TolScaleAnalyzer.Statistics(results);
        Assert.True(s.Complete);
        Assert.Equal(0.0, s.Da);
        Assert.Equal(0.0, s.DaChar);
        Assert.Equal(52, s.Kpres);
        Assert.Equal(0, s.BitLevelSignedZeroRows);
        Assert.True(s.EquivalenceDaZeroIffKpres52);
    }

    [Fact]
    public void A_host_that_snaps_to_2_to_the_minus_30_has_Kpres_below_52_and_DA_equal_to_the_finest_lost_offset()
    {
        // snapping every scale to a multiple of 2^-30: the levels k = 52..32 are lost (their offsets vanish), 30, 29, 28, 24, 20 survive
        var (c, r) = Perfect(row => row.Intended.Select(h => TolHelpers.RoundTo(h, 30)).ToList());
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, r, r);
        var s = TolScaleAnalyzer.Statistics(results);
        Assert.True(s.Complete);
        Assert.True(s.Da > 0);
        Assert.Equal(Math.Pow(2.0, -32), s.Da!.Value); // the largest lost offset: level 32 (exact in binary64)
        Assert.Equal(30, s.Kpres);
        Assert.DoesNotContain(results, x => x.ContradictsDesign); // a lost offset is never larger than its own level
        Assert.True(s.EquivalenceDaZeroIffKpres52);
    }

    [Fact]
    public void A_host_that_snaps_to_one_bit_of_fraction_contradicts_the_design_at_the_coarse_levels()
    {
        // 2^-1 steps turn 1 + 2^-20 into 1.0 (delta 2^-20 = offset, no contradiction) but 1.0 itself stays; sign patterns survive
        var (c, r) = Perfect(row => row.Intended.Select(h => TolHelpers.RoundTo(h, 1)).ToList());
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, r, r);
        Assert.DoesNotContain(results, x => x.ContradictsDesign && x.Row.Family == "PF1");
        // 25.4 -> 25.5 differs by more than 2^-20, but the characterization rows enter no branch of the rule (REQ-1): no FAIL, and DA_char records it
        // the only contradictions left are SIGN contradictions on characterization rows (1/25.4 snapped to 0), never a magnitude one
        Assert.All(results.Where(x => x.ContradictsDesign), x => Assert.False(x.Row.InScope));
        Assert.DoesNotContain(results, x => x.ContradictsDesign && x.Row.Intended.Select(DoubleBits.FromHex).Any(v => v > 20.0));
        Assert.Contains(results, x => !x.Row.InScope && x.DeltaOp1!.Any(h => DoubleBits.FromHex(h) > Math.Pow(2.0, -20)));
    }

    private static TolScaleClassification ClassifyFrom(IReadOnlyList<TolScaleRowResult> results) =>
        TolScaleClassifier.Classify(new TolScaleFacts(Array.Empty<string>(), results.Count(x => x.RejectedByHost), results.Count(x => x.ContradictsDesign),
            results.Count(x => !x.Observed && !x.RejectedByHost), TolScaleAnalyzer.Statistics(results).Complete));

    [Fact]
    public void A_magnitude_deviation_on_a_characterization_row_does_not_FAIL_and_gives_DA_char_above_zero()
    {
        // PF-2 base 25.4 style rows: the host moves the value by far more than 2^-20
        var (c, r) = Perfect(row => !row.InScope && row.Intended.Select(DoubleBits.FromHex).All(v => Math.Abs(v) > 20.0 && Math.Abs(v) < 30.0)
            ? row.Intended.Select(h => DoubleBits.Hex(DoubleBits.FromHex(h) + (DoubleBits.FromHex(h) > 0 ? 0.1 : -0.1))).ToList() : row.Intended);
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, r, r);
        var moved = results.Where(x => !x.Row.InScope && !x.BitIdentical).ToList();
        Assert.NotEmpty(moved);
        Assert.All(moved, x => Assert.False(x.ContradictsDesign));
        var s = TolScaleAnalyzer.Statistics(results);
        Assert.True(s.DaChar > 0);
        Assert.Equal(0.0, s.Da);
        Assert.Equal(TolScaleResult.Pass, ClassifyFrom(results).Result);
    }

    [Fact]
    public void A_magnitude_contradiction_on_an_in_scope_row_FAILs()
    {
        var target = Rows.First(x => x.InScope && x.Family == "PF1" && x.K == 30 && x.SignPattern == "++" && x.Component == "X");
        var (c, r) = Perfect(row => row.Id == target.Id ? new[] { DoubleBits.Hex(1.0 + 3 * Math.Pow(2.0, -30)), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) } : row.Intended);
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, r, r);
        Assert.Contains(results, x => x.Row.Id == target.Id && x.ContradictsDesign);
        Assert.Equal(TolScaleResult.Fail, ClassifyFrom(results).Result);
    }

    [Fact]
    public void A_sign_flip_on_a_characterization_row_FAILs()
    {
        var target = Rows.First(x => !x.InScope);
        var (c, r) = Perfect(row => row.Id == target.Id ? row.Intended.Select(h => DoubleBits.Hex(-DoubleBits.FromHex(h))).ToList() : row.Intended);
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, r, r);
        Assert.Contains(results, x => x.Row.Id == target.Id && x.ContradictsDesign);
        Assert.Equal(TolScaleResult.Fail, ClassifyFrom(results).Result);
    }

    [Fact]
    public void Contradicts_applies_the_magnitude_test_to_in_scope_rows_only_and_the_sign_test_to_all()
    {
        var chr = Rows.First(x => !x.InScope);
        var inScope = Rows.First(x => x.InScope && x.Family == "PF2");
        foreach (var row in new[] { chr, inScope })
        {
            var moved = row.Intended.Select(h => DoubleBits.Hex(DoubleBits.FromHex(h) + (DoubleBits.FromHex(h) > 0 ? 0.5 : -0.5))).ToArray();
            Assert.Equal(row.InScope, TolScaleRowMath.Contradicts(row, moved));
            var flipped = row.Intended.Select(h => DoubleBits.Hex(-DoubleBits.FromHex(h))).ToArray();
            Assert.True(TolScaleRowMath.Contradicts(row, flipped));
        }
    }

    [Fact]
    public void A_missing_construction_result_makes_the_row_unknown()
    {
        var (c, r) = Perfect();
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c.Skip(1).ToList(), r, r);
        Assert.False(results[0].Observed);
        Assert.Equal("NO_CONSTRUCTION_RESULT", results[0].Reason);
        Assert.False(TolScaleAnalyzer.Statistics(results).Complete);
    }

    [Fact]
    public void A_rejected_construction_is_flagged_and_unknown()
    {
        var (c, r) = Perfect();
        var list = c.ToList();
        list[3] = new RowConstruction(Rows[3].Id, null, "eInvalidInput");
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, list, r, r);
        Assert.True(results[3].RejectedByHost);
        Assert.False(results[3].Observed);
        Assert.StartsWith("HOST_REJECTED_CONSTRUCTION:eInvalidInput", results[3].Reason);
    }

    [Fact]
    public void A_row_not_read_at_OP2_is_unknown()
    {
        var (c, r) = Perfect();
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, r, r.Skip(1).ToList());
        Assert.False(results[0].Observed);
        Assert.Contains("OP2_NOT_READ", results[0].Reason);
    }

    [Fact]
    public void A_row_not_read_at_OP1_is_unknown()
    {
        var (c, r) = Perfect();
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, r.Skip(1).ToList(), r);
        Assert.Contains("OP1_NOT_READ", results[0].Reason);
    }

    [Fact]
    public void A_non_finite_read_back_is_unknown_and_never_a_number()
    {
        var (c, r) = Perfect();
        var bad = r.ToList();
        bad[5] = new ReferenceReading("H5", new[] { DoubleBits.Hex(double.NaN), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) }, DoubleBits.Hex(0.0),
            new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) }, null, null);
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, bad, r);
        Assert.False(results[5].Observed);
        Assert.Contains("OP1_NON_FINITE_VALUE", results[5].Reason);
        Assert.Null(results[5].DeltaOp1);
    }

    [Fact]
    public void A_read_error_is_unknown()
    {
        var (c, r) = Perfect();
        var bad = r.ToList();
        bad[7] = new ReferenceReading("H7", null, null, null, null, "eAtMaxReaders");
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, bad, r);
        Assert.Contains("OP1_READ_ERROR:eAtMaxReaders", results[7].Reason);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void A_rotated_or_tilted_reference_is_unknown_not_noise(int which)
    {
        var (c, r) = Perfect();
        var bad = r.ToList();
        var rot = which == 1 ? DoubleBits.Hex(0.5) : DoubleBits.Hex(0.0);
        var normal = which == 2 ? new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(-1.0) } : new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) };
        bad[9] = new ReferenceReading("H9", Rows[9].Intended, rot, normal, bad[9].ColumnLengthsHex, null);
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, bad, r);
        Assert.False(results[9].Observed);
        Assert.Contains("OP1_ROTATION_OR_NORMAL_NOT_IDENTITY", results[9].Reason);
    }

    [Fact]
    public void A_witness_disagreement_makes_the_row_unknown()
    {
        var (c, r) = Perfect();
        var bad = r.ToList();
        var idx = Rows.ToList().FindIndex(x => x.K == 30 && x.Family == "PF1");
        bad[idx] = new ReferenceReading("H" + idx, Rows[idx].Intended, DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) },
            new[] { DoubleBits.Hex(1.5), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) }, null);
        var results = TolScaleAnalyzer.AnalyzeRows(Rows, c, bad, r);
        Assert.False(results[idx].Observed);
        Assert.Contains("WITNESS_DISAGREES", results[idx].Reason);
        Assert.Equal("DISAGREE", results[idx].Witness);
    }

    [Fact]
    public void A_signed_zero_difference_is_counted_as_a_secondary_finding_not_noise()
    {
        // a row whose intended component is read as the other zero cannot exist (no zero scale is assigned): the case is built on a synthetic row
        var rows = new[] { new ProbeRow("PF1-0001", "PF1", "++", "X", 52, true, new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) }) };
        var obs = new[] { new ProbeObservation("PF1-0001", new[] { DoubleBits.Hex(-0.0), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) }, new[] { DoubleBits.Hex(-0.0), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) }) };
        var s = ProbeStatistics.Compute(rows, obs);
        Assert.Equal(1, s.BitLevelSignedZeroRows);
        Assert.Equal(0.0, s.Da);
    }
}

public class TolScaleClassifierTests
{
    private static TolScaleFacts Facts(string[]? invalid = null, int rejected = 0, int contradicting = 0, int unknown = 0, bool complete = true) =>
        new(invalid ?? Array.Empty<string>(), rejected, contradicting, unknown, complete);

    [Fact]
    public void A_clean_execution_passes_and_is_decision_eligible()
    {
        var c = TolScaleClassifier.Classify(Facts());
        Assert.Equal("PASS", c.Result);
        Assert.True(c.DecisionEligible);
        Assert.Empty(c.Reasons);
    }

    [Theory]
    [InlineData(0, 0, 1, true, "UNKNOWN")]
    [InlineData(0, 0, 0, false, "UNKNOWN")]
    [InlineData(1, 0, 0, true, "FAIL")]
    [InlineData(0, 1, 0, true, "FAIL")]
    [InlineData(1, 1, 5, false, "FAIL")]
    [InlineData(0, 1, 5, false, "FAIL")]
    public void The_class_follows_the_precedence_FAIL_over_UNKNOWN(int rejected, int contradicting, int unknown, bool complete, string expected)
    {
        var c = TolScaleClassifier.Classify(Facts(rejected: rejected, contradicting: contradicting, unknown: unknown, complete: complete));
        Assert.Equal(expected, c.Result);
        Assert.False(c.DecisionEligible);
    }

    [Fact]
    public void INVALID_wins_over_everything()
    {
        var c = TolScaleClassifier.Classify(Facts(invalid: new[] { "DBMOD_CHANGED" }, rejected: 3, contradicting: 3, unknown: 3, complete: false));
        Assert.Equal("INVALID", c.Result);
        Assert.Equal(new[] { "DBMOD_CHANGED" }, c.Reasons);
        Assert.False(c.DecisionEligible);
    }

    [Fact]
    public void Only_PASS_is_eligible()
    {
        foreach (var r in new[] { "FAIL", "UNKNOWN", "INVALID" })
            Assert.False(new TolScaleClassification(r, Array.Empty<string>()).DecisionEligible);
        Assert.True(new TolScaleClassification("PASS", Array.Empty<string>()).DecisionEligible);
    }
}
