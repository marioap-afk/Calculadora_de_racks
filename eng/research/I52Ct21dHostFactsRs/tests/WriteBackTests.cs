using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

/// <summary>A fake write-back host: emulates how a host may store (or drop) the overrides, with knobs for every finding the runner must classify.</summary>
internal static class WriteBackFakes
{
    public sealed class Host
    {
        public Dictionary<string, double> Style { get; } = new()
        {
            ["DIMSCALE"] = 1.0, ["DIMTXT"] = 0.18, ["DIMASZ"] = 0.18, ["DIMEXE"] = 0.18, ["DIMEXO"] = 0.0625, ["DIMGAP"] = 0.09, ["DIMTAD"] = 0, ["DIMDEC"] = 4,
        };

        public Dictionary<string, int> Codes { get; } = WriteBackSpecs.Product.ToDictionary(p => p.Variable, p => p.Ba05Code);
        public bool DropEqualToStyle { get; set; }
        public HashSet<string> NeverStore { get; } = new();
        public bool StoreExtraPair { get; set; }
        public bool MalformedPairs { get; set; }
        public string? RejectScenario { get; set; }
        public Func<List<(string Variable, double Value)>, List<(string Variable, double Value)>> HostOrder { get; set; } = x => x;
        public Func<int, List<(string Variable, double Value)>, List<(string Variable, double Value)>> Persist { get; set; } = (_, x) => x;
        public bool LeaveUndeclaredFile { get; set; }
        public bool DoNotDispose { get; set; }
        public bool ThrowOnReopen { get; set; }
        public Func<string, double, double> ValueTransform { get; set; } = (_, v) => v;
        public HashSet<string> DropInProductOrder { get; set; } = new();
        public bool OpaqueValues { get; set; }
    }

    public sealed class HostFor : IWriteBackHost
    {
        private readonly ScratchRootGuard _guard;
        private readonly SideDbLedger _ledger;
        private readonly Host _cfg;

        public HostFor(ScratchRootGuard guard, SideDbLedger ledger, Host cfg)
        {
            _guard = guard;
            _ledger = ledger;
            _cfg = cfg;
        }

        public IWriteBackDatabase CreateSideDatabase() => new Db(_guard, _ledger, _cfg, _ledger.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", "DIMENSION_WRITEBACK_PROBE"), 1);

        private sealed class Db : IWriteBackDatabase
        {
            private readonly ScratchRootGuard _guard;
            private readonly SideDbLedger _ledger;
            private readonly Host _cfg;
            private readonly int _point;
            private List<(string Id, string? Handle, List<(string Variable, double Value)> Stored)> _scenarios = new();

            public Db(ScratchRootGuard guard, SideDbLedger ledger, Host cfg, string id, int point)
            {
                _guard = guard;
                _ledger = ledger;
                _cfg = cfg;
                Id = id;
                _point = point;
            }

            public string Id { get; }

            public IReadOnlyList<StyleValue> ReadStyleValues() => WriteBackSpecs.Product.Select(p =>
                p.IsInt16 ? new StyleValue(p.Variable, "System.Int32", _cfg.Style[p.Variable], _cfg.Style[p.Variable].ToString(CultureInfo.InvariantCulture), null)
                          : new StyleValue(p.Variable, "System.Double", _cfg.Style[p.Variable], _cfg.Style[p.Variable].ToString("R", CultureInfo.InvariantCulture), DoubleBits.Hex(_cfg.Style[p.Variable]))).ToList();

            public IReadOnlyList<ScenarioConstruction> AppendScenarios(IReadOnlyList<WriteBackScenario> scenarios)
            {
                var result = new List<ScenarioConstruction>();
                _scenarios = new();
                for (var i = 0; i < scenarios.Count; i++)
                {
                    var s = scenarios[i];
                    if (_cfg.RejectScenario == s.Id)
                    {
                        _scenarios.Add((s.Id, null, new()));
                        result.Add(new ScenarioConstruction(s.Id, null, "eInvalidInput"));
                        continue;
                    }
                    var stored = new List<(string, double)>();
                    foreach (var a in s.Assignments)
                    {
                        if (_cfg.NeverStore.Contains(a.Variable)) continue;
                        if (_cfg.DropEqualToStyle && a.Value == _cfg.Style[a.Variable]) continue;
                        if (s.Kind == ScenarioKind.ProductOrder && _cfg.DropInProductOrder.Contains(a.Variable)) continue;
                        stored.Add((a.Variable, _cfg.ValueTransform(a.Variable, a.Value)));
                    }
                    stored = _cfg.HostOrder(stored);
                    var handle = (0x200 + i).ToString("X");
                    _scenarios.Add((s.Id, handle, stored));
                    result.Add(new ScenarioConstruction(s.Id, handle, null));
                }
                return result;
            }

            public IReadOnlyList<DimensionReading> ReadDimensions()
            {
                var list = new List<DimensionReading>();
                foreach (var (_, handle, stored0) in _scenarios)
                {
                    if (handle is null) continue;
                    var stored = _cfg.Persist(_point, stored0.ToList());
                    if (stored.Count == 0) { list.Add(new DimensionReading(handle, false, false, true, Array.Empty<StoredEntryReading>(), null)); continue; }
                    var entries = new List<StoredEntryReading>();
                    foreach (var (variable, value) in stored)
                    {
                        var spec = WriteBackSpecs.Find(variable);
                        entries.Add(new StoredEntryReading(1070, "System.Int16", _cfg.Codes[variable].ToString(CultureInfo.InvariantCulture), null));
                        if (_cfg.MalformedPairs) continue;
                        entries.Add(_cfg.OpaqueValues && !spec.IsInt16 ? new StoredEntryReading(1040, "System.Double", "opaque", null)
                            : spec.IsInt16
                            ? new StoredEntryReading(1070, "System.Int16", ((int)value).ToString(CultureInfo.InvariantCulture), null)
                            : new StoredEntryReading(1040, "System.Double", value.ToString("R", CultureInfo.InvariantCulture), DoubleBits.Hex(value)));
                        if (_cfg.StoreExtraPair)
                        {
                            entries.Add(new StoredEntryReading(1070, "System.Int16", "999", null));
                            entries.Add(new StoredEntryReading(1070, "System.Int16", "1", null));
                        }
                    }
                    list.Add(new DimensionReading(handle, true, true, true, entries, null));
                }
                return list;
            }

            public ScratchFileEntry SaveToScratch(ScratchTarget target)
            {
                _guard.AssertCreatable(target);
                File.WriteAllBytes(target.FullPath, Encoding.ASCII.GetBytes("AC1032-fake-writeback:" + target.Name));
                if (_cfg.LeaveUndeclaredFile) File.WriteAllText(Path.Combine(_guard.Root, "extra.sv$"), "x");
                return _guard.RecordSaved(target);
            }

            public IWriteBackDatabase Reopen(ReadableSource source)
            {
                if (_cfg.ThrowOnReopen) throw new IOException("cannot reopen");
                _guard.AssertReadable(source);
                return new Db(_guard, _ledger, _cfg, _ledger.Register("SAVED_REOPEN_READ", "new Database(false, true)", "ReadDwgFile", Path.GetFileName(source.FullPath)), 2) { _scenarios = _scenarios };
            }

            public void Dispose()
            {
                if (_cfg.DoNotDispose) return;
                _ledger.MarkDisposed(Id);
            }
        }
    }
}

public class DstyleParserTests
{
    private static StoredEntryReading Code(int c) => new(1070, "System.Int16", c.ToString(CultureInfo.InvariantCulture), null);
    private static StoredEntryReading Dbl(double v) => new(1040, "System.Double", v.ToString("R", CultureInfo.InvariantCulture), DoubleBits.Hex(v));

    [Fact]
    public void An_empty_section_is_well_formed_with_no_overrides()
    {
        var p = DstyleOverrideParser.Parse(Array.Empty<StoredEntryReading>());
        Assert.True(p.WellFormed);
        Assert.Empty(p.Overrides);
    }

    [Fact]
    public void Pairs_are_read_in_host_order()
    {
        var p = DstyleOverrideParser.Parse(new[] { Code(140), Dbl(3.0), Code(40), Dbl(1.0), Code(77), new StoredEntryReading(1070, "System.Int16", "1", null) });
        Assert.True(p.WellFormed);
        Assert.Equal(new[] { 140, 40, 77 }, p.Overrides.Select(o => o.Code));
        Assert.Equal(1040, p.Overrides[0].ValueTypeCode);
        Assert.Equal(1070, p.Overrides[2].ValueTypeCode);
        Assert.Equal(DoubleBits.Hex(3.0), p.Overrides[0].ValueBitsHex);
    }

    [Fact]
    public void A_group_code_without_a_value_is_malformed()
    {
        var p = DstyleOverrideParser.Parse(new[] { Code(140) });
        Assert.False(p.WellFormed);
        Assert.Contains("GROUP_CODE_140_HAS_NO_VALUE", p.Problem);
    }

    [Fact]
    public void A_group_code_that_is_not_an_int16_entry_is_malformed()
    {
        Assert.False(DstyleOverrideParser.Parse(new[] { Dbl(1.0), Dbl(2.0) }).WellFormed);
        Assert.False(DstyleOverrideParser.Parse(new[] { new StoredEntryReading(1070, "System.Int32", "40", null), Dbl(1.0) }).WellFormed);
        Assert.False(DstyleOverrideParser.Parse(new[] { new StoredEntryReading(1000, "System.Int16", "40", null), Dbl(1.0) }).WellFormed);
    }

    [Fact]
    public void A_group_code_that_is_not_an_integer_is_malformed() =>
        Assert.False(DstyleOverrideParser.Parse(new[] { new StoredEntryReading(1070, "System.Int16", "abc", null), Dbl(1.0) }).WellFormed);

    [Fact]
    public void A_handle_valued_override_is_kept_as_a_value_not_guessed()
    {
        var p = DstyleOverrideParser.Parse(new[] { Code(340), new StoredEntryReading(1005, "System.String", "1A2B", null) });
        Assert.True(p.WellFormed);
        Assert.Equal(1005, p.Overrides[0].ValueTypeCode);
    }
}

public class WriteBackPlanTests
{
    private static IReadOnlyList<StyleValue> StyleOf(Dictionary<string, double> s) => WriteBackSpecs.Product.Select(p =>
        new StyleValue(p.Variable, p.IsInt16 ? "System.Int32" : "System.Double", s[p.Variable], s[p.Variable].ToString("R", CultureInfo.InvariantCulture), null)).ToList();

    private static Dictionary<string, double> DefaultStyle() => new WriteBackFakes.Host().Style;

    [Fact]
    public void The_product_list_is_the_eight_variables_in_the_product_order_with_the_BA05_codes()
    {
        Assert.Equal(new[] { "DIMSCALE", "DIMTXT", "DIMASZ", "DIMEXE", "DIMEXO", "DIMGAP", "DIMTAD", "DIMDEC" }, WriteBackSpecs.Product.Select(p => p.Variable));
        Assert.Equal(new[] { 40, 140, 41, 44, 42, 147, 77, 271 }, WriteBackSpecs.Product.Select(p => p.Ba05Code));
        Assert.Equal(new[] { false, false, false, false, false, false, true, true }, WriteBackSpecs.Product.Select(p => p.IsInt16));
    }

    [Theory]
    [InlineData("DIMSCALE", 1.0)]
    [InlineData("DIMTXT", 3.0)]
    [InlineData("DIMASZ", 3.0 * 0.7)]
    [InlineData("DIMEXE", 3.0 * 0.4)]
    [InlineData("DIMEXO", 3.0 * 0.4)]
    [InlineData("DIMGAP", 3.0 * 0.3)]
    [InlineData("DIMTAD", 1.0)]
    [InlineData("DIMDEC", 2.0)]
    public void The_product_value_is_the_product_arithmetic_for_a_text_height_of_three(string variable, double expected) =>
        Assert.Equal(DoubleBits.Hex(expected), DoubleBits.Hex(WriteBackSpecs.ProductValue(variable)));

    [Fact]
    public void An_unknown_variable_is_an_error()
    {
        Assert.Throws<ArgumentException>(() => WriteBackSpecs.ProductValue("DIMLFAC"));
        Assert.Throws<ArgumentException>(() => WriteBackSpecs.Find("DIMLFAC"));
    }

    [Fact]
    public void The_plan_has_18_scenarios_none_then_diff_and_equal_per_variable_then_the_product_order()
    {
        var plan = WriteBackPlan.Build(StyleOf(DefaultStyle()));
        Assert.Equal(18, plan.Count);
        Assert.Equal("WB-NONE", plan[0].Id);
        Assert.Equal("WB-PRODUCT-ORDER", plan[17].Id);
        Assert.Equal(8, plan.Count(s => s.Kind == ScenarioKind.Different));
        Assert.Equal(8, plan.Count(s => s.Kind == ScenarioKind.EqualToStyle));
        Assert.Empty(plan[0].Assignments);
        Assert.Equal(8, plan[17].Assignments.Count);
        Assert.Equal(WriteBackSpecs.Product.Select(p => p.Variable), plan[17].Assignments.Select(a => a.Variable));
    }

    [Fact]
    public void Every_DIFFERENT_scenario_differs_from_the_style_and_every_EQUAL_scenario_equals_it()
    {
        var style = DefaultStyle();
        var plan = WriteBackPlan.Build(StyleOf(style));
        foreach (var s in plan.Where(x => x.Kind == ScenarioKind.Different)) Assert.NotEqual(style[s.Variable!], Assert.Single(s.Assignments).Value);
        foreach (var s in plan.Where(x => x.Kind == ScenarioKind.EqualToStyle)) Assert.Equal(style[s.Variable!], Assert.Single(s.Assignments).Value);
    }

    [Fact]
    public void When_the_style_already_holds_the_product_value_the_different_value_is_moved()
    {
        var style = DefaultStyle();
        style["DIMSCALE"] = 1.0;           // the product value of DIMSCALE is 1.0
        style["DIMTAD"] = 1;               // and of DIMTAD is 1
        var plan = WriteBackPlan.Build(StyleOf(style));
        Assert.Equal(2.0, plan.Single(s => s.Id == "WB-DIFF-DIMSCALE").Assignments[0].Value);
        Assert.Equal(2.0, plan.Single(s => s.Id == "WB-DIFF-DIMTAD").Assignments[0].Value);
        Assert.Equal(1.0, plan.Single(s => s.Id == "WB-EQ-DIMSCALE").Assignments[0].Value);
    }

    [Fact]
    public void A_missing_style_value_is_an_error() => Assert.Throws<ArgumentException>(() => WriteBackPlan.Build(Array.Empty<StyleValue>()));
}

public class WriteBackRunnerTests
{
    private static RsRunOutcome Run(Rig rig, Action<WriteBackFakes.Host>? configure = null, FakeVars? vars = null)
    {
        var cfg = new WriteBackFakes.Host();
        configure?.Invoke(cfg);
        return WriteBackRunner.Run(rig.D, Rig.Match, new FakeEnv(), vars ?? FakeVars.Default(), (g, l) => new WriteBackFakes.HostFor(g, l, cfg), rig.Writer());
    }

    private static JsonObject Raw(Rig rig) => (JsonObject)rig.ReadEvidence("writeback-raw.json");

    private static string FactStatus(Rig rig, string fact) => rig.ReadEvidence("hostfact-" + fact + ".json")["status"]!.GetValue<string>();

    [Fact]
    public void A_faithful_host_gives_OBSERVED_for_HF_C4_and_HF_C5_and_writes_the_four_files()
    {
        using var rig = new Rig();
        var o = Run(rig);
        Assert.Equal("OBSERVED/OBSERVED", o.Status);
        Assert.Equal(new[] { "hostfact-HF-C4.json", "hostfact-HF-C5.json", "rs-run-writeback.json", "writeback-raw.json" }, rig.EvidenceFiles());
        Assert.Equal(new[] { "CT21D_DIMWRITEBACK_OP2.dwg" }, rig.ScratchEntries());
    }

    [Fact]
    public void The_raw_record_validates_and_lists_18_scenarios_with_both_observation_points()
    {
        using var rig = new Rig();
        Run(rig);
        var raw = Raw(rig);
        Assert.Empty(RsSchemas.Load("ct21d.writeback.v1.json").Validate(raw));
        var scenarios = (JsonArray)raw["scenarios"]!;
        Assert.Equal(18, scenarios.Count);
        Assert.All(scenarios, s =>
        {
            Assert.True(s!["op1"]!["read"]!.GetValue<bool>());
            Assert.True(s["op2"]!["read"]!.GetValue<bool>());
            Assert.True(s["consistentOp1Op2"]!.GetValue<bool>());
        });
        Assert.False(raw["governing"]!.GetValue<bool>());
    }

    [Fact]
    public void The_designation_comes_only_from_the_HF_C4_probe_and_has_the_eight_codes()
    {
        using var rig = new Rig();
        Run(rig);
        var d = (JsonArray)Raw(rig)["designation"]!;
        Assert.Equal(8, d.Count);
        Assert.All(d, x => Assert.Equal("HF-C4", x!["source"]!.GetValue<string>()));
        Assert.Equal(40, d.First(x => x!["variable"]!.GetValue<string>() == "DIMSCALE")!["code"]!.GetValue<int>());
        Assert.Equal(271, d.First(x => x!["variable"]!.GetValue<string>() == "DIMDEC")!["code"]!.GetValue<int>());
        var f5 = rig.ReadEvidence("hostfact-HF-C5.json");
        Assert.Equal(8, ((JsonArray)f5["observation"]!["designation"]!).Count);
    }

    [Fact]
    public void A_host_that_drops_an_override_equal_to_the_style_is_still_OBSERVED_with_the_finding_recorded()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.DropEqualToStyle = true);
        // the per-variable scenarios are not a difference; the product-order scenario then stores fewer than 8 pairs (its values equal the style for some variables): REQ-2 reads that as OBSERVED_DIFFERS
        Assert.Equal("OBSERVED_DIFFERS/OBSERVED", o.Status);
        Assert.Contains(o.Reasons, r => r.Contains("PRODUCT_ORDER:PAIRS_STORED="));
        Assert.DoesNotContain(o.Reasons, r => r.Contains("NOT_STORED_WHEN_DIFFERENT"));
        var findings = (JsonArray)Raw(rig)["findings"]!;
        Assert.All(findings, f => Assert.False(f!["storedWhenEqualToStyle"]!.GetValue<bool>()));
        Assert.All(findings, f => Assert.True(f!["storedWhenDifferent"]!.GetValue<bool>()));
        Assert.Equal(8, ((JsonObject)rig.ReadEvidence("hostfact-HF-C4.json")["observation"]!)["variablesDroppedWhenEqualToStyle"]!.GetValue<int>());
    }

    [Fact]
    public void A_host_that_keeps_an_override_equal_to_the_style_records_that_too()
    {
        using var rig = new Rig();
        Run(rig);
        var findings = (JsonArray)Raw(rig)["findings"]!;
        Assert.All(findings, f => Assert.True(f!["storedWhenEqualToStyle"]!.GetValue<bool>()));
        Assert.All(findings, f => Assert.True(f!["valueStoredAsWritten"]!.GetValue<bool>()));
    }

    [Fact]
    public void A_variable_that_is_never_stored_gives_OBSERVED_DIFFERS_for_HF_C4_and_UNKNOWN_for_HF_C5()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.NeverStore.Add("DIMGAP"));
        Assert.Equal("OBSERVED_DIFFERS/UNKNOWN", o.Status);
        Assert.Contains(o.Reasons, r => r.Contains("DIMGAP:NOT_STORED_WHEN_DIFFERENT"));
        Assert.Contains(o.Reasons, r => r.Contains("CODE_NOT_DETERMINED_FOR=DIMGAP"));
    }

    [Fact]
    public void A_different_group_code_gives_OBSERVED_DIFFERS_for_HF_C5()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Codes["DIMTAD"] = 78);
        Assert.Equal("OBSERVED/OBSERVED_DIFFERS", o.Status);
        Assert.Contains(o.Reasons, r => r.Contains("DIMTAD=78(BA-05:77)"));
        var f = ((JsonArray)Raw(rig)["findings"]!).First(x => x!["variable"]!.GetValue<string>() == "DIMTAD")!;
        Assert.False(f["matchesBa05Code"]!.GetValue<bool>());
    }

    [Fact]
    public void Two_variables_with_the_same_code_make_the_designation_UNKNOWN()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Codes["DIMEXE"] = 42);
        Assert.Equal("UNKNOWN/UNKNOWN", o.Status); // the shared code leaves the host order of the product scenario undecided (REQ-2)
        Assert.Contains(o.Reasons, r => r.Contains("PRODUCT_ORDER_HOST_ORDER_NOT_DECIDED"));
        Assert.Contains(o.Reasons, r => r.Contains("TWO_VARIABLES_SHARE_A_CODE"));
    }

    [Fact]
    public void A_persisted_state_that_differs_from_the_in_memory_state_gives_OBSERVED_DIFFERS()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Persist = (point, stored) => point == 2 ? stored.Where(s => s.Variable != "DIMDEC").ToList() : stored);
        Assert.StartsWith("OBSERVED_DIFFERS/", o.Status);
        Assert.Contains(o.Reasons, r => r.Contains("OP1_DIFFERS_FROM_OP2"));
        Assert.Contains(((JsonArray)Raw(rig)["scenarios"]!), s => !s!["consistentOp1Op2"]!.GetValue<bool>());
    }

    [Fact]
    public void A_value_not_stored_as_written_on_a_different_scenario_gives_OBSERVED_DIFFERS_for_HF_C4()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.ValueTransform = (v, x) => v == "DIMEXO" ? x + 1.0 : x);
        Assert.StartsWith("OBSERVED_DIFFERS/", o.Status);
        Assert.Contains(o.Reasons, r => r.Contains("DIMEXO:VALUE_NOT_STORED_AS_WRITTEN"));
    }

    [Fact]
    public void A_host_order_different_from_the_assignment_order_gives_OBSERVED_DIFFERS_for_HF_C4()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.HostOrder = x => Enumerable.Reverse(x).ToList());
        Assert.StartsWith("OBSERVED_DIFFERS/", o.Status);
        Assert.Contains(o.Reasons, r => r.Contains("PRODUCT_ORDER:HOST_ORDER_DIFFERS_FROM_ASSIGNMENT_ORDER"));
    }

    [Fact]
    public void A_product_order_scenario_that_stores_other_than_8_pairs_gives_OBSERVED_DIFFERS_for_HF_C4()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.DropInProductOrder.Add("DIMTAD"));
        Assert.StartsWith("OBSERVED_DIFFERS/", o.Status);
        Assert.Contains(o.Reasons, r => r.Contains("PRODUCT_ORDER:PAIRS_STORED=7_EXPECTED=8"));
        Assert.DoesNotContain(o.Reasons, r => r.Contains("NOT_STORED_WHEN_DIFFERENT") || r.Contains("HOST_ORDER_DIFFERS"));
    }

    [Fact]
    public void A_value_that_cannot_be_compared_with_the_assigned_one_gives_UNKNOWN_for_HF_C4()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.OpaqueValues = true);
        Assert.StartsWith("UNKNOWN/", o.Status);
        Assert.Contains(o.Reasons, r => r.Contains("VALUE_AS_WRITTEN_NOT_DECIDED"));
    }

    [Fact]
    public void A_malformed_pair_list_gives_UNKNOWN()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.MalformedPairs = true);
        Assert.Equal("UNKNOWN/UNKNOWN", o.Status);
        Assert.Contains(((JsonArray)Raw(rig)["scenarios"]!), s => s!["reason"]!.GetValue<string>().Contains("DSTYLE_PAIRS_MALFORMED"));
    }

    [Fact]
    public void A_rejected_scenario_gives_UNKNOWN_and_is_recorded()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.RejectScenario = "WB-DIFF-DIMASZ");
        Assert.Equal("UNKNOWN/UNKNOWN", o.Status);
        var s = ((JsonArray)Raw(rig)["scenarios"]!).First(x => x!["id"]!.GetValue<string>() == "WB-DIFF-DIMASZ")!;
        Assert.Equal("eInvalidInput", s["rejection"]!.GetValue<string>());
        Assert.Equal("UNKNOWN", s["status"]!.GetValue<string>());
    }

    [Fact]
    public void More_than_one_stored_pair_for_one_assignment_is_not_guessed()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.StoreExtraPair = true);
        Assert.Equal("OBSERVED_DIFFERS/UNKNOWN", o.Status); // HF-C4: the product scenario stores 16 pairs, not 8 (REQ-2); the per-variable scenarios stay undecided
        Assert.Contains(o.Reasons, r => r.Contains("PRODUCT_ORDER:PAIRS_STORED=16_EXPECTED=8"));
    }

    [Fact]
    public void The_host_order_of_the_product_scenario_is_recorded_against_the_assignment_order()
    {
        using var rig = new Rig();
        Run(rig);
        var order = (JsonObject)Raw(rig)["productOrder"]!;
        Assert.True(order["hostOrderEqualsAssignmentOrder"]!.GetValue<bool>());
        Run2Reversed();

        void Run2Reversed()
        {
            using var rig2 = new Rig();
            Run(rig2, h => h.HostOrder = x => Enumerable.Reverse(x).ToList());
            var o2 = (JsonObject)Raw(rig2)["productOrder"]!;
            Assert.False(o2["hostOrderEqualsAssignmentOrder"]!.GetValue<bool>());
            Assert.Equal(new[] { 271, 77, 147, 42, 44, 41, 140, 40 }, ((JsonArray)o2["storedCodes"]!).Select(c => c!.GetValue<int>()));
        }
    }

    [Fact]
    public void The_NONE_scenario_stores_nothing()
    {
        using var rig = new Rig();
        Run(rig);
        var none = ((JsonArray)Raw(rig)["scenarios"]!).First(s => s!["id"]!.GetValue<string>() == "WB-NONE")!;
        Assert.Empty((JsonArray)none["op1"]!["overrides"]!);
        Assert.False(none["op1"]!["hasAcadXData"]!.GetValue<bool>());
    }

    [Fact]
    public void An_exception_while_reopening_gives_INVALID_for_both_facts()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.ThrowOnReopen = true);
        Assert.Equal("INVALID/INVALID", o.Status);
        Assert.Contains(o.Reasons, r => r.StartsWith("INSTRUMENT_ERROR:IOException"));
        Assert.Equal("INVALID", FactStatus(rig, "HF-C4"));
        Assert.Equal("INVALID", FactStatus(rig, "HF-C5"));
    }

    [Fact]
    public void An_undeclared_scratch_file_gives_INVALID()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.LeaveUndeclaredFile = true);
        Assert.Equal("INVALID/INVALID", o.Status);
        Assert.Contains(o.Reasons, r => r.StartsWith("SCRATCH_VERIFICATION_FAILED:undeclared=1"));
    }

    [Fact]
    public void A_side_database_left_open_gives_INVALID()
    {
        using var rig = new Rig();
        Assert.Equal("INVALID/INVALID", Run(rig, h => h.DoNotDispose = true).Status);
    }

    [Fact]
    public void A_change_of_DBMOD_gives_INVALID()
    {
        using var rig = new Rig();
        var vars = FakeVars.Default();
        var reads = 0;
        vars.OnRead = (n, v) => { if (n == "DBMOD" && ++reads == 2) v.Set("DBMOD", "System.Int16", "8"); };
        Assert.Equal("INVALID/INVALID", Run(rig, null, vars).Status);
    }

    [Fact]
    public void A_second_run_is_refused_with_no_side_effect()
    {
        using var rig = new Rig();
        Run(rig);
        var files = rig.EvidenceFiles();
        var ex = Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Contains(ex.Reasons, r => r.StartsWith("OUTPUT_ALREADY_EXISTS:writeback-raw.json"));
        Assert.Equal(files, rig.EvidenceFiles());
    }

    [Fact]
    public void The_run_does_not_need_an_H4_run_id()
    {
        using var rig = new Rig(Rig.OtherRunId);
        Assert.Equal("OBSERVED/OBSERVED", Run(rig).Status);
    }

    [Fact]
    public void A_manual_load_route_is_refused()
    {
        using var rig = new Rig(scripted: false);
        Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Empty(rig.EvidenceFiles());
    }

    [Fact]
    public void The_fact_records_validate_against_the_common_host_fact_schema_and_are_not_governing()
    {
        using var rig = new Rig();
        Run(rig);
        foreach (var f in new[] { "HF-C4", "HF-C5" })
        {
            var rec = rig.ReadEvidence("hostfact-" + f + ".json");
            Assert.Equal("ct21d.hostfact.v1", rec["schema"]!.GetValue<string>());
            Assert.False(rec["governing"]!.GetValue<bool>());
            Assert.Equal(f, rec["factId"]!.GetValue<string>());
        }
    }
}
