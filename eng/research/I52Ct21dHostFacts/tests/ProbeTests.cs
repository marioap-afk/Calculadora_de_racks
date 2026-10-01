using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>
/// The PF-1 / PF-2 probe table generator and the decision statistics of design 2.3/2.4 (I-0). These are pure offline computations;
/// the probe itself (I-6) is RS, blocked by Q-O-1, and is not implemented. The expected table comes from gen_vectors.py (Python).
/// </summary>
public class ProbeTests
{
    private static IReadOnlyList<ProbeObservation> Exact(IEnumerable<ProbeRow> rows) => rows.Select(r => new ProbeObservation(r.Id, r.Intended, r.Intended)).ToList();

    [Fact]
    public void Generator_reproduces_the_147_rows_of_the_python_table_bit_for_bit()
    {
        var expected = (JsonArray)Support.Data("probe-table-expected.json")["rows"]!;
        var rows = ProbeTable.Generate();
        Assert.Equal(147, rows.Count);
        Assert.Equal(expected.Count, rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            var e = expected[i]!;
            var r = rows[i];
            Assert.Equal(e["id"]!.GetValue<string>(), r.Id);
            Assert.Equal(e["family"]!.GetValue<string>(), r.Family);
            Assert.Equal(e["signPattern"]!.GetValue<string>(), r.SignPattern);
            Assert.Equal(e["component"]!.GetValue<string>(), r.Component);
            Assert.Equal(e["k"] is null ? (int?)null : e["k"]!.GetValue<int>(), r.K);
            Assert.Equal(e["inScope"]!.GetValue<bool>(), r.InScope);
            Assert.Equal(((JsonArray)e["intended"]!).Select(x => x!.GetValue<string>()).ToArray(), r.Intended.ToArray());
        }
    }

    [Fact]
    public void Table_counts_match_the_design()
    {
        var rows = ProbeTable.Generate();
        Assert.Equal(132, rows.Count(r => r.Family == "PF1"));
        Assert.Equal(15, rows.Count(r => r.Family == "PF2"));
        Assert.Equal(135, rows.Count(r => r.InScope));
        Assert.Equal(146, rows.Select(r => string.Join("|", r.Intended)).Distinct().Count()); // the duplicate triple of the design's row count note
        Assert.Equal(new[] { "PF2-0001", "PF2-0002", "PF2-0003" }, rows.Where(r => r.Family == "PF2" && r.InScope).Select(r => r.Id).ToArray());
        Assert.Equal(rows.Select(r => r.Id).Distinct().Count(), rows.Count);
        Assert.Equal(new[] { 52, 48, 44, 40, 36, 32, 30, 29, 28, 24, 20 }, ProbeTable.LadderLevels.ToArray());
    }

    [Fact]
    public void An_exact_host_gives_DA_zero_and_Kpres_52()
    {
        var rows = ProbeTable.Generate();
        var s = ProbeStatistics.Compute(rows, Exact(rows));
        Assert.True(s.Complete);
        Assert.Equal((0.0, 0.0, 0.0, 0.0), (s.DaOp1, s.DaOp2, s.Da, s.DaChar));
        Assert.Equal(52, s.Kpres);
        Assert.Equal(0, s.BitLevelSignedZeroRows);
        Assert.True(s.DeltaExactEverywhere);
        Assert.True(s.EquivalenceDaZeroIffKpres52);
    }

    [Fact]
    public void A_host_that_snaps_offsets_finer_than_2_pow_minus_30_gives_DA_2_pow_minus_32_and_Kpres_30()
    {
        var rows = ProbeTable.Generate();
        // snap: any in-scope PF-1 row with k > 30 is read back as the sign pattern times (1,1,1)
        var obs = rows.Select(r =>
        {
            if (r.Family == "PF1" && r.K > 30)
            {
                var snapped = new[] { r.SignPattern[0] == '-' ? "BFF0000000000000" : "3FF0000000000000", r.SignPattern[1] == '-' ? "BFF0000000000000" : "3FF0000000000000", "3FF0000000000000" };
                return new ProbeObservation(r.Id, snapped, snapped);
            }
            return new ProbeObservation(r.Id, r.Intended, r.Intended);
        }).ToList();
        var s = ProbeStatistics.Compute(rows, obs);
        // the largest snapped offset is the coarsest snapped level, k = 32: 2^-32 = 0x3DF0000000000000 (python)
        Assert.Equal(DoubleBits.FromHex("3DF0000000000000"), s.Da);
        Assert.Equal(30, s.Kpres);
        Assert.True(s.EquivalenceDaZeroIffKpres52);
        Assert.True(s.DeltaExactEverywhere); // Sterbenz: every difference is exact
    }

    [Fact]
    public void DA_takes_the_maximum_over_both_observation_points_and_the_characterization_rows_stay_apart()
    {
        var rows = ProbeTable.Generate();
        var obs = Exact(rows).ToList();
        // OP2 only differs, on an in-scope row: DA_op1 = 0, DA_op2 > 0, DA = DA_op2
        var i = rows.ToList().FindIndex(r => r.Id == "PF1-0001");
        var altered = rows[i].Intended.ToArray();
        altered[0] = "3FF0000000000002"; // 1 + 2 ulp instead of 1 + 1 ulp: delta = 2^-52
        obs[i] = new ProbeObservation(rows[i].Id, rows[i].Intended, altered);
        var s = ProbeStatistics.Compute(rows, obs);
        Assert.Equal(0.0, s.DaOp1);
        Assert.Equal(Math.Pow(2, -52), s.DaOp2);
        Assert.Equal(Math.Pow(2, -52), s.Da);
        Assert.Equal(0.0, s.DaChar);
        Assert.Equal(48, s.Kpres); // level 52 is not bit identical, so the finest level with every coarser level identical is 48

        // a characterization row (PF2 base 0.5, X nextUp) that differs enters DA_char only
        var obs2 = Exact(rows).ToList();
        var j = rows.ToList().FindIndex(r => r.Id == "PF2-0005");
        obs2[j] = new ProbeObservation(rows[j].Id, new[] { "3FE0000000000000", "3FE0000000000000", "3FE0000000000000" }, rows[j].Intended);
        var s2 = ProbeStatistics.Compute(rows, obs2);
        Assert.Equal(0.0, s2.Da);
        Assert.True(s2.DaChar > 0);
        Assert.Equal(52, s2.Kpres);
    }

    [Fact]
    public void An_unobserved_in_scope_row_makes_the_statistics_incomplete_never_a_value()
    {
        var rows = ProbeTable.Generate();
        var obs = Exact(rows).Where(o => o.Id != "PF1-0010").ToList();
        var s = ProbeStatistics.Compute(rows, obs);
        Assert.False(s.Complete);
        Assert.Null(s.Da);
        Assert.Null(s.DaOp1);
        Assert.True(s.Kpres < 52);

        // a non-finite read is the same as unread
        var obs2 = Exact(rows).ToList();
        obs2[3] = new ProbeObservation(rows[3].Id, new[] { "7FF8000000000000", rows[3].Intended[1], rows[3].Intended[2] }, rows[3].Intended);
        Assert.False(ProbeStatistics.Compute(rows, obs2).Complete);
    }

    [Fact]
    public void Signed_zero_rows_are_counted_apart_and_never_noise()
    {
        var rows = new[] { new ProbeRow("PF2-0099", "PF2", "++", "X", null, true, new[] { "0000000000000000", "3FF0000000000000", "3FF0000000000000" }) };
        var obs = new[] { new ProbeObservation("PF2-0099", new[] { "8000000000000000", "3FF0000000000000", "3FF0000000000000" }, new[] { "8000000000000000", "3FF0000000000000", "3FF0000000000000" }) };
        var s = ProbeStatistics.Compute(rows, obs);
        Assert.Equal(1, s.BitLevelSignedZeroRows);
        Assert.Equal(0.0, s.Da); // -0.0 equals +0.0: the delta is zero
    }

    [Fact]
    public void Sterbenz_exactness_flag_is_false_when_the_operands_are_more_than_a_factor_two_apart()
    {
        var rows = new[] { new ProbeRow("PF2-0098", "PF2", "++", "X", null, true, new[] { "3FF0000000000000", "3FF0000000000000", "3FF0000000000000" }) };
        var obs = new[] { new ProbeObservation("PF2-0098", new[] { "4010000000000000", "3FF0000000000000", "3FF0000000000000" }, rows[0].Intended) }; // 4.0 read for 1.0
        var s = ProbeStatistics.Compute(rows, obs);
        Assert.Equal(3.0, s.Da);
        Assert.False(s.DeltaExactEverywhere);
    }
}
