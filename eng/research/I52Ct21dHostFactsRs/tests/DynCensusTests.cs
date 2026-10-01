using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

internal sealed class FakeDynHost : IDynCensusHost
{
    public ScratchRootGuard Guard { get; }
    public SideDbLedger Ledger { get; }
    public List<DynamicBlockProbe> Probes { get; set; } = new();
    public List<string> Failing { get; } = new();
    public ReadableSource? LastSource { get; private set; }
    public int BlockRecords { get; set; } = 7;
    public bool ThrowOnOpen { get; set; }
    public bool SaveAScratchFile { get; set; }
    public bool DoNotDispose { get; set; }
    public List<string> Probed { get; } = new();

    public FakeDynHost(ScratchRootGuard guard, SideDbLedger ledger)
    {
        Guard = guard;
        Ledger = ledger;
    }

    public IDynCensusSession OpenPrivateCopy(ReadableSource source)
    {
        if (ThrowOnOpen) throw new InvalidOperationException("cannot open");
        LastSource = source;
        Guard.AssertReadable(source);
        var a = Ledger.Register("PRIVATE_COPY_READ", "new Database(false, true)", "ReadDwgFile", Path.GetFileName(source.FullPath));
        var b = Ledger.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", "DYNAMIC_CENSUS_SCRATCH");
        if (SaveAScratchFile)
        {
            var t = Guard.AuthorizeNew("CT21D_UNEXPECTED.dwg", "X");
            File.WriteAllText(t.FullPath, "x");
            Guard.RecordSaved(t);
        }
        return new Session(this, a, b);
    }

    private sealed class Session : IDynCensusSession
    {
        private readonly FakeDynHost _host;
        private readonly string _a;
        private readonly string _b;

        public Session(FakeDynHost host, string a, string b)
        {
            _host = host;
            _a = a;
            _b = b;
        }

        public IReadOnlyList<string> ListDynamicDefinitionNames() => _host.Probes.Select(p => p.BlockName).ToList();

        public int CountBlockRecords() => _host.BlockRecords;

        public DynamicBlockProbe Probe(string blockName)
        {
            _host.Probed.Add(blockName);
            if (_host.Failing.Contains(blockName)) throw new InvalidOperationException("clone failed");
            return _host.Probes.First(p => p.BlockName == blockName);
        }

        public void Dispose()
        {
            if (_host.DoNotDispose) return;
            _host.Ledger.MarkDisposed(_a);
            _host.Ledger.MarkDisposed(_b);
        }
    }
}

public class DynCensusBuilderTests
{
    private static DynamicPropertyReading P(string name, bool ro = false, string type = "System.Double", string units = "Distance", IReadOnlyList<AllowedValueReading>? allowed = null, bool unreadable = false) =>
        new(name, ro, type, units, "1", DoubleBits.Hex(1.0), unreadable ? null : (allowed ?? Array.Empty<AllowedValueReading>()), unreadable ? "eNotApplicable" : null);

    [Fact]
    public void A_property_without_a_value_set_is_NONE_and_a_listed_one_is_LISTED()
    {
        var probe = new DynamicBlockProbe("B", new[] { P("A"), P("L", allowed: new[] { new AllowedValueReading("System.Double", "1", DoubleBits.Hex(1.0)) }) }, null);
        var json = DynCensusBuilder.BlockToJson(probe);
        var props = (JsonArray)json["dynamicProperties"]!;
        Assert.Equal("NONE", props[0]!["valueSetStatus"]!.GetValue<string>());
        Assert.Equal("LISTED", props[1]!["valueSetStatus"]!.GetValue<string>());
        Assert.Null(props[0]!["allowedValues"]);
        Assert.Single((JsonArray)props[1]!["allowedValues"]!);
    }

    [Fact]
    public void An_unreadable_value_set_is_recorded_as_UNREADABLE_with_the_error()
    {
        var json = DynCensusBuilder.BlockToJson(new DynamicBlockProbe("B", new[] { P("A", unreadable: true) }, null));
        var p = ((JsonArray)json["dynamicProperties"]!)[0]!;
        Assert.Equal("UNREADABLE", p["valueSetStatus"]!.GetValue<string>());
        Assert.Equal("eNotApplicable", p["valueSetError"]!.GetValue<string>());
    }

    [Fact]
    public void Two_properties_with_the_same_name_are_flagged_exactly_and_ignoring_case()
    {
        var json = DynCensusBuilder.BlockToJson(new DynamicBlockProbe("B", new[] { P("Alto"), P("Alto"), P("ALTO"), P("Largo") }, null));
        var props = (JsonArray)json["dynamicProperties"]!;
        Assert.True(props[0]!["sharesNameWithAnother"]!.GetValue<bool>());
        Assert.True(props[1]!["sharesNameWithAnother"]!.GetValue<bool>());
        Assert.False(props[2]!["sharesNameWithAnother"]!.GetValue<bool>());
        Assert.True(props[2]!["sharesNameIgnoringCase"]!.GetValue<bool>());
        Assert.False(props[3]!["sharesNameIgnoringCase"]!.GetValue<bool>());
    }

    [Fact]
    public void A_block_that_could_not_be_probed_is_UNKNOWN_with_the_reason()
    {
        var json = DynCensusBuilder.BlockToJson(new DynamicBlockProbe("B", null, "PROBE_FAILED:X"));
        Assert.Equal("UNKNOWN", json["status"]!.GetValue<string>());
        Assert.Equal("PROBE_FAILED:X", json["reason"]!.GetValue<string>());
        Assert.Empty((JsonArray)json["dynamicProperties"]!);
    }

    [Fact]
    public void The_property_order_of_the_host_is_kept_with_an_explicit_index()
    {
        var json = DynCensusBuilder.BlockToJson(new DynamicBlockProbe("B", new[] { P("Z"), P("A"), P("M") }, null));
        var props = (JsonArray)json["dynamicProperties"]!;
        Assert.Equal(new[] { "Z", "A", "M" }, props.Select(p => p!["name"]!.GetValue<string>()));
        Assert.Equal(new[] { 0, 1, 2 }, props.Select(p => p!["index"]!.GetValue<int>()));
    }

    [Fact]
    public void The_summary_counts_properties_writable_constrained_and_unreadable()
    {
        var listed = new[] { new AllowedValueReading("System.Double", "1", DoubleBits.Hex(1.0)) };
        var probes = new[]
        {
            new DynamicBlockProbe("B1", new[] { P("A"), P("B", ro: true), P("C", allowed: listed) }, null),
            new DynamicBlockProbe("B2", new[] { P("A", unreadable: true), P("A") }, null),
            new DynamicBlockProbe("B3", null, "x"),
        };
        var s = DynCensusBuilder.Summarize(probes);
        Assert.Equal(3, s.Definitions);
        Assert.Equal(2, s.Observed);
        Assert.Equal(1, s.Unknown);
        Assert.Equal(5, s.Properties);
        Assert.Equal(4, s.Writable);
        Assert.Equal(1, s.Constrained);
        Assert.Equal(1, s.ValueSetUnreadable);
        Assert.Equal(1, s.DuplicateNames);
    }

    [Fact]
    public void FactStatus_is_NOT_OBSERVED_without_dynamic_blocks_UNKNOWN_with_an_unprobed_one_else_OBSERVED()
    {
        Assert.Equal("NOT_OBSERVED", DynCensusBuilder.FactStatus(DynCensusBuilder.Summarize(Array.Empty<DynamicBlockProbe>())).Status);
        Assert.Equal("UNKNOWN", DynCensusBuilder.FactStatus(DynCensusBuilder.Summarize(new[] { new DynamicBlockProbe("B", null, "x") })).Status);
        Assert.Equal("OBSERVED", DynCensusBuilder.FactStatus(DynCensusBuilder.Summarize(new[] { new DynamicBlockProbe("B", new[] { P("A") }, null) })).Status);
    }

    [Fact]
    public void An_unreadable_value_set_is_reported_in_the_OBSERVED_reason()
    {
        var (status, reason) = DynCensusBuilder.FactStatus(DynCensusBuilder.Summarize(new[] { new DynamicBlockProbe("B", new[] { P("A", unreadable: true) }, null) }));
        Assert.Equal("OBSERVED", status);
        Assert.Contains("constrained-unknown", reason);
    }
}

public class DynCensusRunnerTests
{
    private static DynamicPropertyReading P(string name, bool ro = false) => new(name, ro, "System.Double", "Distance", "1", DoubleBits.Hex(1.0), Array.Empty<AllowedValueReading>(), null);

    private static RsRunOutcome Run(Rig rig, Action<FakeDynHost>? configure = null, FakeVars? vars = null)
    {
        return DynCensusRunner.Run(rig.D, Rig.Match, new FakeEnv(), vars ?? FakeVars.Default(), (g, l) =>
        {
            var h = new FakeDynHost(g, l)
            {
                Probes = new List<DynamicBlockProbe> { new("LARGUERO", new[] { P("Longitud"), P("Origen1", ro: true) }, null), new("TARIMA", new[] { P("Alto") }, null) },
            };
            configure?.Invoke(h);
            return h;
        }, rig.Writer());
    }

    [Fact]
    public void Two_probed_definitions_give_OBSERVED_and_the_three_files_and_no_scratch_file()
    {
        using var rig = new Rig();
        var o = Run(rig);
        Assert.Equal("OBSERVED", o.Status);
        Assert.Equal(new[] { "dyncensus-raw.json", "hostfact-HF-C2.json", "rs-run-dyncensus.json" }, rig.EvidenceFiles());
        Assert.Empty(rig.ScratchEntries());
    }

    [Fact]
    public void The_raw_record_validates_and_describes_the_definitions()
    {
        using var rig = new Rig();
        Run(rig);
        var raw = rig.ReadEvidence("dyncensus-raw.json");
        Assert.Empty(RsSchemas.Load("ct21d.dyncensus.v1.json").Validate(raw));
        Assert.Equal(DynCensusBuilder.Method, raw["dynamicPropertyMethod"]!.GetValue<string>());
        Assert.Equal(2, ((JsonArray)raw["blocks"]!).Count);
        Assert.Equal(7, raw["blockRecords"]!.GetValue<int>());
        Assert.Equal(3, raw["summary"]!["properties"]!.GetValue<int>());
        Assert.Equal(2, raw["summary"]!["writableProperties"]!.GetValue<int>());
    }

    [Fact]
    public void The_private_copy_is_handed_to_the_host_as_a_PrivateCopy_source_never_the_library()
    {
        using var rig = new Rig();
        FakeDynHost? host = null;
        Run(rig, h => host = h);
        Assert.Equal(ReadableKind.PrivateCopy, host!.LastSource!.Kind);
        Assert.Equal(PathRegions.Norm(rig.PrivateCopy), host.LastSource.FullPath);
        Assert.False(PathRegions.Same(host.LastSource.FullPath, rig.Library));
    }

    [Fact]
    public void Each_definition_is_probed_once_in_table_order()
    {
        using var rig = new Rig();
        FakeDynHost? host = null;
        Run(rig, h => host = h);
        Assert.Equal(new[] { "LARGUERO", "TARIMA" }, host!.Probed);
    }

    [Fact]
    public void A_library_with_no_dynamic_block_gives_NOT_OBSERVED()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.Probes = new List<DynamicBlockProbe>());
        Assert.Equal("NOT_OBSERVED", o.Status);
        Assert.Equal("NOT_OBSERVED", rig.ReadEvidence("hostfact-HF-C2.json")["status"]!.GetValue<string>());
    }

    [Fact]
    public void A_probe_that_fails_gives_UNKNOWN_and_the_other_blocks_are_still_probed()
    {
        using var rig = new Rig();
        FakeDynHost? host = null;
        var o = Run(rig, h => { host = h; h.Failing.Add("LARGUERO"); });
        Assert.Equal("UNKNOWN", o.Status);
        Assert.Equal(new[] { "LARGUERO", "TARIMA" }, host!.Probed);
        var blocks = (JsonArray)rig.ReadEvidence("dyncensus-raw.json")["blocks"]!;
        Assert.Equal("UNKNOWN", blocks[0]!["status"]!.GetValue<string>());
        Assert.StartsWith("PROBE_FAILED:InvalidOperationException", blocks[0]!["reason"]!.GetValue<string>());
        Assert.Equal("OBSERVED", blocks[1]!["status"]!.GetValue<string>());
    }

    [Fact]
    public void An_exception_while_opening_gives_INVALID()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.ThrowOnOpen = true);
        Assert.Equal("INVALID", o.Status);
        Assert.Contains(o.Reasons, r => r.StartsWith("INSTRUMENT_ERROR:InvalidOperationException"));
    }

    [Fact]
    public void A_scratch_file_saved_by_the_census_is_INVALID_because_the_census_saves_nothing()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.SaveAScratchFile = true);
        Assert.Equal("INVALID", o.Status);
        Assert.Contains("UNEXPECTED_SCRATCH_FILE_SAVED", o.Reasons);
    }

    [Fact]
    public void Side_databases_left_open_give_INVALID()
    {
        using var rig = new Rig();
        var o = Run(rig, h => h.DoNotDispose = true);
        Assert.Equal("INVALID", o.Status);
        Assert.Contains("SIDE_DATABASE_NOT_DISPOSED", o.Reasons);
    }

    [Fact]
    public void The_run_record_identifies_the_private_copy_database_and_the_scratch_database()
    {
        using var rig = new Rig();
        Run(rig);
        var dbs = (JsonArray)rig.ReadEvidence("rs-run-dyncensus.json")["sideDatabases"]!;
        Assert.Equal(new[] { "PRIVATE_COPY_READ", "SCRATCH_WRITE" }, dbs.Select(d => d!["role"]!.GetValue<string>()));
        Assert.All(dbs, d => Assert.True(d!["disposed"]!.GetValue<bool>()));
    }

    [Fact]
    public void The_private_copy_and_the_library_stay_byte_identical()
    {
        using var rig = new Rig();
        var before = Support.Sha(rig.Library);
        Run(rig);
        Assert.Equal(before, Support.Sha(rig.Library));
        Assert.Equal(before, Support.Sha(rig.PrivateCopy));
    }

    [Fact]
    public void A_change_of_DBMOD_gives_INVALID()
    {
        using var rig = new Rig();
        var vars = FakeVars.Default();
        var reads = 0;
        vars.OnRead = (n, v) => { if (n == "DBMOD" && ++reads == 2) v.Set("DBMOD", "System.Int16", "3"); };
        Assert.Equal("INVALID", Run(rig, null, vars).Status);
    }

    [Fact]
    public void A_second_run_is_refused()
    {
        using var rig = new Rig();
        Run(rig);
        var ex = Assert.Throws<RsRefusedException>(() => Run(rig));
        Assert.Contains(ex.Reasons, r => r.StartsWith("OUTPUT_ALREADY_EXISTS:dyncensus-raw.json"));
    }

    [Fact]
    public void The_fact_record_is_a_non_governing_HF_C2_record()
    {
        using var rig = new Rig();
        Run(rig);
        var f = rig.ReadEvidence("hostfact-HF-C2.json");
        Assert.Equal("HF-C2", f["factId"]!.GetValue<string>());
        Assert.False(f["governing"]!.GetValue<bool>());
        Assert.Equal("dyncensus-raw.json", f["rawRefs"]![0]!["path"]!.GetValue<string>());
    }
}
