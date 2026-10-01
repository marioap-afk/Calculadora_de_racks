using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

public class TolScaleSchemaTests
{
    [Fact]
    public void The_embedded_schema_file_is_byte_equal_to_the_schema_block_of_the_design_document()
    {
        var design = File.ReadAllText(Path.Combine(Support.RepoRoot(), "docs", "initiatives", "I-52-ct21d-tol-scale-design-and-host-fact-matrix-v1.md"));
        var i = design.IndexOf("**Formal schema (JSON Schema 2020-12) of `tolscale-raw.json`.**", StringComparison.Ordinal);
        Assert.True(i > 0);
        var m = Regex.Match(design[i..], "```json\n(.*?)\n```", RegexOptions.Singleline);
        Assert.True(m.Success);
        var file = File.ReadAllText(Path.Combine(Support.RsFolder(), "schemas", "ct21d.tolscale.v1.json"));
        Assert.Equal(m.Groups[1].Value.Replace("\r\n", "\n") + "\n", file.Replace("\r\n", "\n"));
    }

    [Fact]
    public void The_design_text_lists_147_rows_and_135_in_scope_like_the_schema_constants()
    {
        var schema = File.ReadAllText(Path.Combine(Support.RsFolder(), "schemas", "ct21d.tolscale.v1.json"));
        Assert.Contains("\"minItems\": 147", schema);
        Assert.Contains("\"maxItems\": 147", schema);
        Assert.Contains("\"inScopeRows\": { \"const\": 135 }", schema);
    }

    [Fact]
    public void The_schema_loads_in_the_closed_validator() => Assert.NotNull(RsSchemas.Load("ct21d.tolscale.v1.json"));
}

public class TolScaleRecordTests
{
    private static (JsonObject Record, IReadOnlyList<TolScaleRowResult> Rows, ProbeStatisticsResult Stats) PerfectRecord(string result = "PASS")
    {
        using var rig = new Rig();
        var rows = ProbeTable.Generate();
        var constructions = rows.Select((r, i) => new RowConstruction(r.Id, "H" + i, null)).ToList();
        var readings = rows.Select((r, i) => new ReferenceReading("H" + i, r.Intended, DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) },
            r.Intended.Select(h => DoubleBits.Hex(Math.Abs(DoubleBits.FromHex(h)))).ToList(), null)).ToList();
        var results = TolScaleAnalyzer.AnalyzeRows(rows, constructions, readings, readings);
        var stats = TolScaleAnalyzer.Statistics(results);
        var table = TolScaleProbeTableFile.Build(rows);
        var record = TolScaleRecord.Build(rig.D, Rig.Match, new FakeEnv(), results, stats, new TolScaleChecks(0, 0, true, false, false), result, TolScaleProbeTableFile.Sha256(table));
        return (record, results, stats);
    }

    [Fact]
    public void The_record_has_147_rows_and_validates_against_the_design_schema()
    {
        var (record, _, _) = PerfectRecord();
        Assert.Equal(147, ((JsonArray)record["rows"]!).Count);
        Assert.Empty(RsSchemas.Load("ct21d.tolscale.v1.json").Validate(record));
    }

    [Fact]
    public void The_summary_of_a_perfect_host_is_DA_zero_and_Kpres_52()
    {
        var (record, _, _) = PerfectRecord();
        var s = (JsonObject)record["summary"]!;
        Assert.Equal("0000000000000000", s["DA"]!.GetValue<string>());
        Assert.Equal("0000000000000000", s["DA_op1"]!.GetValue<string>());
        Assert.Equal("0000000000000000", s["DA_op2"]!.GetValue<string>());
        Assert.Equal("0000000000000000", s["DA_char"]!.GetValue<string>());
        Assert.Equal(52, s["Kpres"]!.GetValue<int>());
        Assert.Equal(147, s["rowsObserved"]!.GetValue<int>());
        Assert.Equal(135, s["inScopeRows"]!.GetValue<int>());
        Assert.Equal(0, s["bitLevelSignedZeroRows"]!.GetValue<int>());
    }

    [Fact]
    public void The_record_is_not_governing_and_names_the_gate()
    {
        var (record, _, _) = PerfectRecord();
        Assert.False(record["governing"]!.GetValue<bool>());
        Assert.Equal("HOST_GATE_BA11_3_3", record["gate"]!.GetValue<string>());
        Assert.Equal("ct21d.tolscale.v1", record["schema"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("PASS", true)]
    [InlineData("FAIL", false)]
    [InlineData("UNKNOWN", false)]
    [InlineData("INVALID", false)]
    public void DecisionEligible_is_true_exactly_for_PASS(string result, bool eligible)
    {
        var (record, _, _) = PerfectRecord(result);
        Assert.Equal(eligible, record["decisionEligible"]!.GetValue<bool>());
        Assert.Equal(result, record["result"]!.GetValue<string>());
    }

    [Fact]
    public void The_tuple_carries_the_designation_bindings_and_the_instrument()
    {
        using var rig = new Rig();
        var (record, _, _) = PerfectRecord();
        var t = (JsonObject)record["tuple"]!;
        Assert.Equal(rig.D.RunId, t["runId"]!.GetValue<string>());
        Assert.Equal("MC-0123456789ab", t["machineClassLabel"]!.GetValue<string>());
        Assert.Equal(1, t["attempt"]!.GetValue<int>());
        Assert.Equal("I52Ct21d.HostFacts.Rs", t["instrument"]!["name"]!.GetValue<string>());
        Assert.Equal(new string('3', 64), t["instrument"]!["declaredSetSha256"]!.GetValue<string>());
        Assert.Equal("b74af94ece4f0901a071ef5176f3c58bff01cfdc", record["design"]!["ba05Blob"]!.GetValue<string>());
    }

    [Fact]
    public void The_volatile_part_is_excluded_from_the_content_hash()
    {
        var (record, _, _) = PerfectRecord();
        var a = TolScaleRecord.ContentSha256(record);
        ((JsonObject)record["volatile"]!)["pid"] = 1;
        Assert.Equal(a, TolScaleRecord.ContentSha256(record));
        record["result"] = "FAIL";
        Assert.NotEqual(a, TolScaleRecord.ContentSha256(record));
    }

    [Fact]
    public void The_probe_table_hash_is_stable_and_depends_on_the_table()
    {
        var rows = ProbeTable.Generate();
        var a = TolScaleProbeTableFile.Sha256(TolScaleProbeTableFile.Build(rows));
        Assert.Equal(a, TolScaleProbeTableFile.Sha256(TolScaleProbeTableFile.Build(ProbeTable.Generate())));
        var changed = rows.ToList();
        changed[0] = new ProbeRow(changed[0].Id, changed[0].Family, changed[0].SignPattern, changed[0].Component, changed[0].K, changed[0].InScope,
            new[] { DoubleBits.Hex(2.0), changed[0].Intended[1], changed[0].Intended[2] });
        Assert.NotEqual(a, TolScaleProbeTableFile.Sha256(TolScaleProbeTableFile.Build(changed)));
    }

    [Fact]
    public void The_probe_table_file_ends_with_one_LF_and_has_no_CR()
    {
        var text = TolScaleProbeTableFile.ToFileText(TolScaleProbeTableFile.Build(ProbeTable.Generate()));
        Assert.EndsWith("\n", text);
        Assert.DoesNotContain("\r", text);
    }

    // ---- the offline recomputation (design 2.4: a mismatch is INVALID evidence) ----------------------------------------------------

    [Fact]
    public void The_offline_verifier_accepts_the_instruments_own_record()
    {
        var (record, _, _) = PerfectRecord();
        Assert.Empty(TolScaleOfflineVerifier.Verify(Jcs.Serialize(record)));
    }

    [Theory]
    [InlineData("DA")]
    [InlineData("DA_op1")]
    [InlineData("DA_op2")]
    [InlineData("DA_char")]
    public void The_offline_verifier_detects_a_tampered_statistic(string field)
    {
        var (record, _, _) = PerfectRecord();
        ((JsonObject)record["summary"]!)[field] = DoubleBits.Hex(1e-9);
        var problems = TolScaleOfflineVerifier.Verify(Jcs.Serialize(record));
        Assert.Contains("SUMMARY_MISMATCH:" + field, problems);
    }

    [Fact]
    public void The_offline_verifier_accepts_a_host_that_only_loses_the_PF2_nextDown_base_1_row_review_attack_MAJOR_1()
    {
        // reviewer attack: every value bit-exact except nextDown(1.0) = 0x3FEFFFFFFFFFFFFF returned as 1.0. All 132 PF-1 rows are identical
        // (K_pres = 52) while D_A over the 135 in-scope rows is 2^-53 > 0: a valid, complete observation that must NOT be turned into INVALID.
        using var rig = new Rig();
        var rows = ProbeTable.Generate();
        var nextDown = DoubleBits.Hex(BitConverter.UInt64BitsToDouble(0x3FEFFFFFFFFFFFFFUL));
        var one = DoubleBits.Hex(1.0);
        var constructions = rows.Select((r, i) => new RowConstruction(r.Id, "H" + i, null)).ToList();
        var readings = rows.Select((r, i) =>
        {
            var scale = r.Intended.Select(h => h == nextDown ? one : h).ToList();
            return new ReferenceReading("H" + i, scale, DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) },
                scale.Select(h => DoubleBits.Hex(Math.Abs(DoubleBits.FromHex(h)))).ToList(), null);
        }).ToList();
        var results = TolScaleAnalyzer.AnalyzeRows(rows, constructions, readings, readings);
        var stats = TolScaleAnalyzer.Statistics(results);
        var table = TolScaleProbeTableFile.Build(rows);
        var record = TolScaleRecord.Build(rig.D, Rig.Match, new FakeEnv(), results, stats, new TolScaleChecks(0, 0, true, false, false), "PASS", TolScaleProbeTableFile.Sha256(table));
        var summary = (JsonObject)record["summary"]!;
        Assert.Equal(52, summary["Kpres"]!.GetValue<int>());
        Assert.NotEqual("0000000000000000", summary["DA"]!.GetValue<string>());
        Assert.Empty(TolScaleOfflineVerifier.Verify(Jcs.Serialize(record)));
    }

    [Fact]
    public void The_offline_verifier_detects_a_tampered_Kpres()
    {
        var (record, _, _) = PerfectRecord();
        ((JsonObject)record["summary"]!)["Kpres"] = 40;
        Assert.Contains("SUMMARY_MISMATCH:Kpres", TolScaleOfflineVerifier.Verify(Jcs.Serialize(record)));
    }

    [Fact]
    public void The_offline_verifier_detects_a_row_changed_after_the_statistics()
    {
        var (record, _, _) = PerfectRecord();
        var row = (JsonObject)((JsonArray)record["rows"]!)[40]!;
        var op1 = (JsonArray)row["op1"]!;
        op1[0] = DoubleBits.Hex(DoubleBits.FromHex(op1[0]!.GetValue<string>()) + 1e-9);
        var problems = TolScaleOfflineVerifier.Verify(Jcs.Serialize(record));
        Assert.NotEmpty(problems);
    }

    [Fact]
    public void The_offline_verifier_detects_a_wrong_decision_flag()
    {
        var (record, _, _) = PerfectRecord("FAIL");
        record["decisionEligible"] = true;
        Assert.NotEmpty(TolScaleOfflineVerifier.Verify(Jcs.Serialize(record)));
    }

    [Fact]
    public void The_offline_verifier_refuses_PASS_with_an_unobserved_row()
    {
        var (record, _, _) = PerfectRecord();
        var row = (JsonObject)((JsonArray)record["rows"]!)[3]!;
        row["status"] = "UNKNOWN";
        var problems = TolScaleOfflineVerifier.Verify(Jcs.Serialize(record));
        Assert.Contains("PASS_WITH_UNOBSERVED_ROWS", problems);
    }

    [Fact]
    public void The_offline_verifier_rejects_a_record_outside_the_schema()
    {
        var (record, _, _) = PerfectRecord();
        record["unexpected"] = 1;
        var problems = TolScaleOfflineVerifier.Verify(Jcs.Serialize(record));
        Assert.StartsWith("SCHEMA:", problems[0]);
    }

    [Fact]
    public void The_offline_verifier_rejects_text_that_is_not_json() =>
        Assert.StartsWith("NOT_JSON", TolScaleOfflineVerifier.Verify("{not json")[0]);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(20)]
    public void The_two_independent_implementations_of_the_statistics_agree_on_random_hosts(int seed)
    {
        // a host that keeps a random number of mantissa bits per component and sometimes loses a whole row
        var rng = new Random(seed);
        var rows = ProbeTable.Generate();
        var constructions = new List<RowConstruction>();
        var op1 = new List<ReferenceReading>();
        var op2 = new List<ReferenceReading>();
        var keep = rng.Next(20, 53);
        for (var i = 0; i < rows.Count; i++)
        {
            constructions.Add(new RowConstruction(rows[i].Id, "H" + i, null));
            IReadOnlyList<string> Make(int bits) => rows[i].Intended.Select(h =>
            {
                var v = DoubleBits.FromHex(h);
                if (bits >= 52) return h;
                var mask = ~((1UL << (52 - bits)) - 1UL);
                return DoubleBits.Hex(BitConverter.UInt64BitsToDouble(BitConverter.DoubleToUInt64Bits(v) & mask));
            }).ToList();
            var s1 = Make(keep);
            var s2 = Make(rng.Next(0, 4) == 0 ? keep - 1 : keep);
            ReferenceReading R(string h, IReadOnlyList<string> s) => new(h, s, DoubleBits.Hex(0.0), new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) },
                s.Select(x => DoubleBits.Hex(Math.Abs(DoubleBits.FromHex(x)))).ToList(), null);
            op1.Add(R("H" + i, s1));
            if (rng.Next(0, 40) != 0) op2.Add(R("H" + i, s2)); // sometimes a row is not read at OP2
        }
        var results = TolScaleAnalyzer.AnalyzeRows(rows, constructions, op1, op2);
        var stats = TolScaleAnalyzer.Statistics(results);
        using var rig = new Rig();
        var record = TolScaleRecord.Build(rig.D, Rig.Match, new FakeEnv(), results, stats, new TolScaleChecks(0, 0, true, false, false),
            results.All(r => r.Observed) && !results.Any(r => r.ContradictsDesign) ? "PASS" : results.Any(r => r.ContradictsDesign) ? "FAIL" : "UNKNOWN",
            TolScaleProbeTableFile.Sha256(TolScaleProbeTableFile.Build(rows)));
        Assert.Empty(TolScaleOfflineVerifier.Verify(Jcs.Serialize(record)));
    }
}

public class TolScaleRunnerTests
{
    private static TolScaleOutcome Run(Rig rig, Action<FakeTolHost>? configure = null, FakeVars? vars = null, InstrumentInfo? instrument = null)
    {
        FakeTolHost? host = null;
        var outcome = TolScaleRunner.Run(rig.D, instrument ?? Rig.Match, new FakeEnv(), vars ?? FakeVars.Default(), (g, l) =>
        {
            host = new FakeTolHost(g, l);
            configure?.Invoke(host);
            return host;
        }, rig.Writer());
        Assert.NotNull(host);
        return outcome;
    }

    [Fact]
    public void A_perfect_host_gives_PASS_and_writes_exactly_the_three_declared_files_and_one_scratch_file()
    {
        using var rig = new Rig();
        var o = Run(rig);
        Assert.Equal("PASS", o.Result);
        Assert.Equal(new[] { "rs-run-tolscale.json", "tolscale-probe-table.json", "tolscale-raw.json" }, rig.EvidenceFiles());
        Assert.Equal(new[] { "CT21D_TOLSCALE_OP2.dwg" }, rig.ScratchEntries());
    }

    [Fact]
    public void The_records_written_validate_and_the_offline_verifier_agrees()
    {
        using var rig = new Rig();
        Run(rig);
        var text = File.ReadAllText(Path.Combine(rig.Evidence, "tolscale-raw.json"));
        Assert.Empty(RsSchemas.Load("ct21d.tolscale.v1.json").Validate(Jcs.ParseStrict(text)));
        Assert.Empty(TolScaleOfflineVerifier.Verify(text));
        Assert.Empty(RsSchemas.Load("ct21d.rs-run.v1.json").Validate(rig.ReadEvidence("rs-run-tolscale.json")));
    }

    [Fact]
    public void The_probe_table_file_hash_equals_the_hash_in_the_record()
    {
        using var rig = new Rig();
        Run(rig);
        var fileSha = Support.Sha(Path.Combine(rig.Evidence, "tolscale-probe-table.json"));
        Assert.Equal(fileSha, rig.ReadEvidence("tolscale-raw.json")["design"]!["probeTableSha256"]!.GetValue<string>());
    }

    [Fact]
    public void The_run_record_identifies_both_side_databases_and_their_disposal()
    {
        using var rig = new Rig();
        Run(rig);
        var run = rig.ReadEvidence("rs-run-tolscale.json");
        var dbs = (JsonArray)run["sideDatabases"]!;
        Assert.Equal(2, dbs.Count);
        Assert.Equal("SCRATCH_WRITE", dbs[0]!["role"]!.GetValue<string>());
        Assert.Equal("SAVED_REOPEN_READ", dbs[1]!["role"]!.GetValue<string>());
        Assert.All(dbs, d => Assert.True(d!["disposed"]!.GetValue<bool>()));
    }

    [Fact]
    public void The_run_record_declares_the_scratch_file_with_its_hash_and_a_clean_verification()
    {
        using var rig = new Rig();
        Run(rig);
        var run = rig.ReadEvidence("rs-run-tolscale.json");
        var file = run["scratch"]!["files"]![0]!;
        Assert.Equal("CT21D_TOLSCALE_OP2.dwg", file["name"]!.GetValue<string>());
        Assert.Equal(Support.Sha(Path.Combine(rig.Scratch, "CT21D_TOLSCALE_OP2.dwg")), file["sha256"]!.GetValue<string>());
        Assert.True(run["scratch"]!["verification"]!["ok"]!.GetValue<bool>());
        Assert.Equal("CAD_MANAGER_DECLARATION", run["declarations"]!["source"]!.GetValue<string>());
        Assert.True(run["noAutomaticRetry"]!.GetValue<bool>());
        Assert.False(run["governing"]!.GetValue<bool>());
    }

    [Fact]
    public void The_closed_tolscale_schema_has_no_content_hash_so_the_run_record_carries_it()
    {
        using var rig = new Rig();
        Run(rig);
        var raw = (JsonObject)rig.ReadEvidence("tolscale-raw.json");
        Assert.Null(raw["contentSha256"]);
        var run = rig.ReadEvidence("rs-run-tolscale.json");
        Assert.Equal(TolScaleRecord.ContentSha256(raw), run["contentHashes"]!["tolscale-raw.json"]!.GetValue<string>());
    }

    [Fact]
    public void The_host_calls_follow_the_design_steps_S2_to_S6()
    {
        using var rig = new Rig();
        FakeTolHost? captured = null;
        TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => captured = new FakeTolHost(g, l), rig.Writer());
        Assert.Equal(new[] { "create", "append", "read1", "save", "reopen", "read2" }, captured!.Calls);
        Assert.Equal(1, captured.Creates); // one execution, one side database created by the instrument
    }

    [Fact]
    public void The_private_files_and_the_library_are_untouched()
    {
        using var rig = new Rig();
        var before = Support.Sha(rig.Library);
        Run(rig);
        Assert.Equal(before, Support.Sha(rig.Library));
        Assert.Equal(before, Support.Sha(rig.PrivateCopy));
    }

    [Fact]
    public void A_host_that_stores_fewer_bits_is_still_a_valid_PASS_with_a_positive_DA()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Readback = (r, _) => r.Intended.Select(x => TolHelpers.RoundTo(x, 30)).ToList());
        Assert.Equal("PASS", o.Result);
        var s = o.Record["summary"]!;
        Assert.NotEqual("0000000000000000", s["DA"]!.GetValue<string>());
        Assert.Equal(30, s["Kpres"]!.GetValue<int>());
        Assert.True(o.Record["decisionEligible"]!.GetValue<bool>());
    }

    [Fact]
    public void OP1_and_OP2_both_enter_DA()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Readback = (r, p) => p == 2 ? r.Intended.Select(x => TolHelpers.RoundTo(x, 24)).ToList() : r.Intended);
        var s = o.Record["summary"]!;
        Assert.Equal("0000000000000000", s["DA_op1"]!.GetValue<string>());
        Assert.NotEqual("0000000000000000", s["DA_op2"]!.GetValue<string>());
        Assert.Equal(s["DA_op2"]!.GetValue<string>(), s["DA"]!.GetValue<string>());
    }

    [Fact]
    public void A_host_that_flips_a_sign_gives_FAIL()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Readback = (r, _) => r.SignPattern == "-+" && r.Family == "PF1" ? r.Intended.Select(x => DoubleBits.Hex(Math.Abs(DoubleBits.FromHex(x)))).ToList() : r.Intended);
        Assert.Equal("FAIL", o.Result);
        Assert.False(o.Record["decisionEligible"]!.GetValue<bool>());
    }

    [Fact]
    public void A_host_that_rejects_a_construction_gives_FAIL_with_the_row_unknown()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Reject = r => r.Id == "PF1-0010" ? "eInvalidInput" : null);
        Assert.Equal("FAIL", o.Result);
        var row = ((JsonArray)o.Record["rows"]!).First(x => x!["id"]!.GetValue<string>() == "PF1-0010")!;
        Assert.Equal("UNKNOWN", row["status"]!.GetValue<string>());
        Assert.StartsWith("HOST_REJECTED_CONSTRUCTION:eInvalidInput", row["reason"]!.GetValue<string>());
    }

    [Fact]
    public void A_host_that_cannot_read_a_row_back_gives_UNKNOWN()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Normal = (r, p) => r.Id == "PF1-0020" ? new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(-1.0) } : new[] { DoubleBits.Hex(0.0), DoubleBits.Hex(0.0), DoubleBits.Hex(1.0) });
        Assert.Equal("UNKNOWN", o.Result);
        Assert.Contains("ROWS_UNKNOWN=1", o.Reasons);
        Assert.Null(o.Record["summary"]!["DA"]);
    }

    [Fact]
    public void A_witness_disagreement_gives_UNKNOWN()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Columns = (r, _, s) => r.Id == "PF1-0006" ? new[] { DoubleBits.Hex(2.0), DoubleBits.Hex(1.0), DoubleBits.Hex(1.0) } : s.Select(x => DoubleBits.Hex(Math.Abs(DoubleBits.FromHex(x)))).ToList());
        Assert.Equal("UNKNOWN", o.Result);
    }

    [Fact]
    public void An_exception_in_the_host_gives_INVALID_and_still_records_the_run()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.ThrowOnRead = true);
        Assert.Equal("INVALID", o.Result);
        Assert.Contains(o.Reasons, r => r.StartsWith("INSTRUMENT_ERROR:InvalidOperationException"));
        Assert.Contains("tolscale-raw.json", rig.EvidenceFiles());
        Assert.False(o.Record["decisionEligible"]!.GetValue<bool>());
    }

    [Fact]
    public void An_exception_at_the_save_gives_INVALID_and_no_scratch_file()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.ThrowOnSave = true);
        Assert.Equal("INVALID", o.Result);
        Assert.Empty(rig.ScratchEntries());
    }

    [Fact]
    public void A_change_of_DBMOD_during_the_run_gives_INVALID()
    {
        using var rig = new Rig();
        var vars = FakeVars.Default();
        var reads = 0;
        vars.OnRead = (n, v) => { if (n == "DBMOD" && ++reads == 2) v.Set("DBMOD", "System.Int16", "1"); };
        var o = Run(rig, null, vars);
        Assert.Equal("INVALID", o.Result);
        Assert.Contains("DBMOD_CHANGED", o.Reasons);
    }

    [Theory]
    [InlineData("SECURELOAD")]
    [InlineData("TRUSTEDPATHS")]
    [InlineData("CPROFILE")]
    [InlineData("DWGNAME")]
    [InlineData("DWGPREFIX")]
    public void A_change_of_a_guarded_setting_during_the_run_gives_INVALID(string name)
    {
        using var rig = new Rig();
        var vars = FakeVars.Default();
        var seen = 0;
        vars.OnRead = (n, v) => { if (n == name && ++seen == 2) v.Set(name, "System.String", "changed"); };
        var o = Run(rig, null, vars);
        Assert.Equal("INVALID", o.Result);
        Assert.Contains(name + "_CHANGED", o.Reasons);
    }

    [Fact]
    public void An_unreadable_DBMOD_gives_INVALID()
    {
        using var rig = new Rig();
        var vars = FakeVars.Default();
        vars.Values.Remove("DBMOD");
        var o = Run(rig, null, vars);
        Assert.Equal("INVALID", o.Result);
        Assert.Contains("DBMOD_UNREADABLE", o.Reasons);
    }

    [Fact]
    public void A_private_copy_changed_during_the_run_gives_INVALID()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.OnSaved = () => File.AppendAllText(rig.PrivateCopy, "tamper"));
        Assert.Equal("INVALID", o.Result);
        Assert.Contains("PRIVATE_FILES_CHANGED", o.Reasons);
    }

    [Fact]
    public void An_undeclared_file_left_in_the_scratch_root_gives_INVALID()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.LeaveUndeclaredFile = true);
        Assert.Equal("INVALID", o.Result);
        Assert.Contains(o.Reasons, r => r.StartsWith("SCRATCH_VERIFICATION_FAILED:undeclared=1"));
        var run = rig.ReadEvidence("rs-run-tolscale.json");
        Assert.False(run["scratch"]!["verification"]!["ok"]!.GetValue<bool>());
        Assert.Equal("leftover.bak", run["scratch"]!["verification"]!["undeclared"]![0]!.GetValue<string>());
    }

    [Fact]
    public void A_side_database_that_is_not_disposed_gives_INVALID()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.DoNotDispose = true);
        Assert.Equal("INVALID", o.Result);
        Assert.Contains("SIDE_DATABASE_NOT_DISPOSED", o.Reasons);
    }

    // ---- refusals happen before ANY side effect --------------------------------------------------------------------------------------

    private static void AssertRefusedWithNoSideEffects(Rig rig, Func<TolScaleOutcome> run, string reason)
    {
        var ex = Assert.Throws<RsRefusedException>(run);
        Assert.Contains(ex.Reasons, r => r.StartsWith(reason));
        Assert.Empty(rig.EvidenceFiles());
        Assert.Empty(rig.ScratchEntries());
    }

    [Fact]
    public void A_pin_mismatch_is_refused_before_the_host_is_created()
    {
        using var rig = new Rig();
        var created = false;
        AssertRefusedWithNoSideEffects(rig, () => TolScaleRunner.Run(rig.D, new InstrumentInfo("x", new string('a', 64), "PIN_MISMATCH"), new FakeEnv(), FakeVars.Default(),
            (g, l) => { created = true; return new FakeTolHost(g, l); }, rig.Writer()), "SELF_PIN_PIN_MISMATCH");
        Assert.False(created);
    }

    [Fact]
    public void A_run_id_that_is_not_H4_is_refused()
    {
        using var rig = new Rig(Rig.OtherRunId);
        AssertRefusedWithNoSideEffects(rig, () => Run(rig), "RUN_ID_IS_NOT_AN_H4_RUN");
    }

    [Fact]
    public void A_manual_load_route_is_refused()
    {
        using var rig = new Rig(scripted: false);
        AssertRefusedWithNoSideEffects(rig, () => Run(rig), "MANUAL_INPUT_DECLARED");
    }

    [Fact]
    public void Another_acad_process_is_refused()
    {
        using var rig = new Rig(otherAcad: true);
        AssertRefusedWithNoSideEffects(rig, () => Run(rig), "OTHER_ACAD_PROCESS_DECLARED");
    }

    [Fact]
    public void A_private_copy_whose_hash_differs_from_the_designation_is_refused()
    {
        using var rig = new Rig();
        File.AppendAllText(rig.PrivateCopy, "tamper");
        AssertRefusedWithNoSideEffects(rig, () => Run(rig), "PRIVATE_COPY_HASH_DIFFERS_FROM_DESIGNATION");
    }

    [Fact]
    public void A_library_whose_hash_differs_from_the_designation_is_refused()
    {
        using var rig = new Rig();
        File.AppendAllText(rig.Library, "tamper");
        AssertRefusedWithNoSideEffects(rig, () => Run(rig), "LIBRARY_HASH_DIFFERS_FROM_DESIGNATION");
    }

    [Fact]
    public void A_scratch_root_with_an_undeclared_entry_at_the_start_is_refused()
    {
        using var rig = new Rig();
        File.WriteAllText(Path.Combine(rig.Scratch, "old.dwg"), "x");
        var ex = Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Contains("SCRATCH_ROOT_HAS_UNDECLARED_ENTRY_AT_START:old.dwg", ex.Reasons);
        Assert.Empty(rig.EvidenceFiles());
    }

    [Fact]
    public void A_missing_scratch_root_is_refused()
    {
        using var rig = new Rig();
        Directory.Delete(rig.Scratch);
        Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Empty(rig.EvidenceFiles());
    }

    [Fact]
    public void Overlapping_roots_are_refused()
    {
        using var rig = new Rig();
        rig.With(d => new RsDesignation(d.RunId, d.Attempt, d.SessionId, d.EvidenceRoot, d.EvidenceRoot, d.PrivateCopyPath, d.PrivateCopySha256, d.LibraryPath,
            d.LibraryFileSha256, d.GovernedDocumentPath, d.ProductPaths, d.HostChecks, d.MachineClassLabel, d.BuildTupleDigest, d.PackageManifestSha256,
            d.DeclaredSetSha256, d.DesignBlob, d.Ba05Blob, d.DeclaredSet));
        var ex = Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Contains("EVIDENCE_ROOT_AND_SCRATCH_ROOT_OVERLAP", ex.Reasons);
    }

    [Fact]
    public void A_writer_on_another_folder_is_refused()
    {
        using var rig = new Rig();
        using var other = new TempDir();
        var ex = Assert.Throws<RsRefusedException>(() => TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeTolHost(g, l), new EvidenceWriter(other.Path)));
        Assert.Contains("WRITER_ROOT_IS_NOT_THE_DESIGNATED_EVIDENCE_ROOT", ex.Reasons);
    }

    [Fact]
    public void A_second_run_in_the_same_evidence_root_is_refused_no_automatic_retry()
    {
        using var rig = new Rig();
        Run(rig);
        var files = rig.EvidenceFiles();
        var scratch = rig.ScratchEntries();
        var ex = Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Contains(ex.Reasons, r => r.StartsWith("OUTPUT_ALREADY_EXISTS:tolscale-raw.json"));
        Assert.Equal(files, rig.EvidenceFiles());
        Assert.Equal(scratch, rig.ScratchEntries());
    }

    [Fact]
    public void A_corrupt_prior_run_record_blocks_the_next_run()
    {
        using var rig = new Rig();
        File.WriteAllText(Path.Combine(rig.Evidence, "rs-run-other.json"), "{\"schema\":\"ct21d.rs-run.v1\"}");
        var ex = Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Contains("PRIOR_RUN_RECORD_UNREADABLE:rs-run-other.json", ex.Reasons);
    }

    [Fact]
    public void Scratch_files_declared_by_an_earlier_command_are_accepted_at_the_start()
    {
        using var rig = new Rig();
        // an earlier command (write-back) declared its OP2 file in its own run record
        var wbHost = new WriteBackFakes.Host();
        var wb = WriteBackRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new WriteBackFakes.HostFor(g, l, wbHost), rig.Writer());
        Assert.StartsWith("OBSERVED", wb.Status);
        Assert.Equal(new[] { "CT21D_DIMWRITEBACK_OP2.dwg" }, rig.ScratchEntries());
        var o = Run(rig);
        Assert.Equal("PASS", o.Result);
        Assert.Equal(new[] { "CT21D_DIMWRITEBACK_OP2.dwg", "CT21D_TOLSCALE_OP2.dwg" }, rig.ScratchEntries());
        Assert.Single((JsonArray)rig.ReadEvidence("rs-run-tolscale.json")["scratch"]!["preexisting"]!);
    }

    [Fact]
    public void A_tampered_file_declared_by_an_earlier_command_blocks_the_next_run()
    {
        using var rig = new Rig();
        var wb = WriteBackRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new WriteBackFakes.HostFor(g, l, new WriteBackFakes.Host()), rig.Writer());
        Assert.StartsWith("OBSERVED", wb.Status);
        File.AppendAllText(Path.Combine(rig.Scratch, "CT21D_DIMWRITEBACK_OP2.dwg"), "tamper");
        var ex = Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Contains("SCRATCH_FILE_HASH_CHANGED_SINCE_DECLARED:CT21D_DIMWRITEBACK_OP2.dwg", ex.Reasons);
    }

    [Fact]
    public void The_result_files_are_never_overwritten_by_a_second_writer()
    {
        using var rig = new Rig();
        Run(rig);
        var w = rig.Writer();
        Assert.Throws<EvidenceWriterException>(() => w.WriteNewText("tolscale-raw.json", "x"));
    }
}
