using System.Text;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using I52Ct21d.HostFacts.Rs.Tools;
using I52Ct21d.HostFacts.Tools.Scan;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

[CollectionDefinition("Console", DisableParallelization = true)]
public sealed class ConsoleCollection
{
}

[Collection("Console")]
public class ToolsCliTests
{
    private static (int Code, string Out, string Err) Run(params string[] args)
    {
        var oldOut = Console.Out;
        var oldErr = Console.Error;
        using var o = new StringWriter();
        using var e = new StringWriter();
        Console.SetOut(o);
        Console.SetError(e);
        try { return (Program.Main(args), o.ToString(), e.ToString()); }
        finally
        {
            Console.SetOut(oldOut);
            Console.SetError(oldErr);
        }
    }

    private static string[] ScanArgs(params string[] specs) =>
        new[] { "scan-rs", "--allowed", Lists.AllowedPath, "--waivers", Lists.WaiversPath, "--privileged-surface", Lists.SurfacePath, "--privileged-callers", Lists.CallersPath, "--expected-commands", Lists.CommandsPath }
            .Concat(specs).ToArray();

    [Fact]
    public void No_arguments_and_unknown_commands_print_the_usage_and_exit_1()
    {
        var (c1, _, e1) = Run();
        Assert.Equal(1, c1);
        Assert.Contains("usage:", e1);
        Assert.Equal(1, Run("frobnicate").Code);
    }

    [Fact]
    public void Hash_prints_the_sha256_of_a_file()
    {
        using var t = new TempDir();
        var f = Path.Combine(t.Path, "a.txt");
        File.WriteAllText(f, "abc");
        var (code, o, _) = Run("hash", f);
        Assert.Equal(0, code);
        Assert.StartsWith("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad  ", o);
        Assert.Equal(1, Run("hash").Code);
    }

    [Fact]
    public void Scan_of_the_two_real_offline_assemblies_exits_0_and_prints_the_hashes_of_the_five_lists()
    {
        var (code, o, _) = Run(ScanArgs("core:" + Lists.CorePath, "tools:" + Lists.ToolsPath));
        Assert.Equal(0, code);
        Assert.Contains("CLEAN", o);
        Assert.Contains("allowed-apis-rs.txt sha256 " + Support.Sha(Lists.AllowedPath), o);
        Assert.Contains("rs-waivers.txt sha256 " + Support.Sha(Lists.WaiversPath), o);
        Assert.Contains("privileged-surface-rs.txt sha256 " + Support.Sha(Lists.SurfacePath), o);
        Assert.Contains("privileged-callers-rs.txt sha256 " + Support.Sha(Lists.CallersPath), o);
        Assert.Contains("expected-commands-rs.txt sha256 " + Support.Sha(Lists.CommandsPath), o);
    }

#if HAVE_RS
    [Fact]
    public void Scan_of_the_three_real_assemblies_exits_0()
    {
        var (code, o, _) = Run(ScanArgs("core:" + Lists.CorePath, "tools:" + Lists.ToolsPath, "rs:" + Lists.RsPath));
        Assert.Equal(0, code);
        Assert.Contains("lists: CLEAN", o);
    }
#endif

    [Fact]
    public void Scan_of_a_violating_fixture_exits_2_and_names_the_rule()
    {
        using var t = new TempDir();
        var f = Path.Combine(t.Path, "I52Ct21d.HostFacts.Rs.dll");
        File.WriteAllBytes(f, new Fx().Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)).Build());
        var (code, o, _) = Run(ScanArgs("rs:" + f));
        Assert.Equal(2, code);
        Assert.Contains("R-RS-SAVE-SCOPE", o);
    }

    [Fact]
    public void A_malformed_list_a_bad_role_and_missing_options_exit_1()
    {
        using var t = new TempDir();
        var bad = Path.Combine(t.Path, "bad.txt");
        File.WriteAllText(bad, "R-COMMAND|x|y|a justification long enough\n");
        Assert.Equal(1, Run("scan-rs", "--allowed", Lists.AllowedPath, "--waivers", bad, "--privileged-surface", Lists.SurfacePath, "--privileged-callers", Lists.CallersPath, "--expected-commands", Lists.CommandsPath, "core:" + Lists.CorePath).Code);
        Assert.Equal(1, Run(ScanArgs("weird:" + Lists.CorePath)).Code);
        Assert.Equal(1, Run(ScanArgs("nocolon")).Code);
        Assert.Equal(1, Run("scan-rs", "--allowed", Lists.AllowedPath).Code);
        Assert.Equal(1, Run(ScanArgs()).Code);
    }

    [Fact]
    public void Imports_list_prints_review_lines_and_approves_nothing()
    {
        var (code, o, _) = Run("imports-list-rs", "core:" + Lists.CorePath);
        Assert.Equal(0, code);
        Assert.Contains("|REVIEW", o);
        Assert.Contains("T|System.String||REVIEW", o);
        Assert.Equal(1, Run("imports-list-rs").Code);
    }

    [Fact]
    public void Privileged_list_prints_the_surface_of_a_fixture()
    {
        using var t = new TempDir();
        var f = Path.Combine(t.Path, "I52Ct21d.HostFacts.Rs.dll");
        File.WriteAllBytes(f, new Fx().Build());
        var (code, o, _) = Run("privileged-list-rs", "rs:" + f);
        Assert.Equal(0, code);
        Assert.Contains("SURFACE I52Ct21d.HostFacts.Rs.ScratchSaver::Save()", o);
        Assert.Contains("SURFACE I52Ct21d.HostFacts.Rs.SideDbWriter::Create()", o);
    }

    [Fact]
    public void Scratch_verify_exits_0_for_declared_files_and_2_for_an_undeclared_one()
    {
        using var rig = new Rig();
        var run = TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeTolHost(g, l), rig.Writer());
        Assert.Equal("PASS", run.Result);
        var (c0, o0, _) = Run("scratch-verify", rig.Scratch, rig.Evidence);
        Assert.Equal(0, c0);
        Assert.Contains("CLEAN (1 declared file(s))", o0);
        File.WriteAllText(Path.Combine(rig.Scratch, "stray.txt"), "x");
        var (c2, o2, _) = Run("scratch-verify", rig.Scratch, rig.Evidence);
        Assert.Equal(2, c2);
        Assert.Contains("UNDECLARED stray.txt", o2);
        Assert.Contains("NOT CLEAN", o2);
    }

    [Fact]
    public void Scratch_verify_exits_2_for_a_changed_declared_file_and_for_a_missing_one()
    {
        using var rig = new Rig();
        TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeTolHost(g, l), rig.Writer());
        var scratchFile = Path.Combine(rig.Scratch, "CT21D_TOLSCALE_OP2.dwg");
        File.AppendAllText(scratchFile, "x");
        var (c1, o1, _) = Run("scratch-verify", rig.Scratch, rig.Evidence);
        Assert.Equal(2, c1);
        Assert.Contains("HASH_MISMATCH CT21D_TOLSCALE_OP2.dwg", o1);
        File.Delete(scratchFile);
        var (c2, o2, _) = Run("scratch-verify", rig.Scratch, rig.Evidence);
        Assert.Equal(2, c2);
        Assert.Contains("MISSING CT21D_TOLSCALE_OP2.dwg", o2);
    }

    [Fact]
    public void Scratch_verify_exits_1_for_missing_folders_and_wrong_arguments()
    {
        using var t = new TempDir();
        Assert.Equal(1, Run("scratch-verify", Path.Combine(t.Path, "nope"), t.Path).Code);
        Assert.Equal(1, Run("scratch-verify", t.Path).Code);
    }

    [Fact]
    public void Tolscale_verify_accepts_the_instruments_record_and_rejects_a_tampered_one()
    {
        using var rig = new Rig();
        TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeTolHost(g, l), rig.Writer());
        var file = Path.Combine(rig.Evidence, "tolscale-raw.json");
        Assert.Equal(0, Run("tolscale-verify", file).Code);
        var text = File.ReadAllText(file);
        var tampered = Path.Combine(rig.Temp.Path, "tampered.json");
        File.WriteAllText(tampered, text.Replace("\"Kpres\":52", "\"Kpres\":30", StringComparison.Ordinal));
        var (code, o, _) = Run("tolscale-verify", tampered);
        Assert.Equal(2, code);
        Assert.Contains("SUMMARY_MISMATCH:Kpres", o);
        Assert.Equal(1, Run("tolscale-verify").Code);
    }

    [Fact]
    public void Ledger_verify_exits_0_4_and_1()
    {
        using var t = new TempDir();
        File.WriteAllText(Path.Combine(t.Path, "a.txt"), "abc");
        var ledger = Path.Combine(t.Path, "ledger.txt");
        File.WriteAllText(ledger, "# ledger\nba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad  a.txt\n");
        Assert.Equal(0, Run("ledger-verify", ledger, t.Path).Code);
        File.WriteAllText(Path.Combine(t.Path, "a.txt"), "abd");
        var (c4, o4, _) = Run("ledger-verify", ledger, t.Path);
        Assert.Equal(4, c4);
        Assert.Contains("HASH_DIFFERS:a.txt", o4);
        Assert.Equal(1, Run("ledger-verify", ledger).Code);
    }
}

public class HashesLedgerTests
{
    [Fact]
    public void A_missing_file_a_malformed_line_and_a_CR_are_differences()
    {
        using var t = new TempDir();
        var d = HashesLedger.Verify("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad  gone.txt\nnot a hash line\n", t.Path);
        Assert.Contains("MISSING:gone.txt", d);
        Assert.Contains("MALFORMED_LINE:not a hash line", d);
        Assert.Contains("LEDGER_HAS_CR", HashesLedger.Verify("x\r\n", t.Path));
    }

    [Fact]
    public void A_path_with_forward_slashes_is_resolved_under_the_root()
    {
        using var t = new TempDir();
        Directory.CreateDirectory(Path.Combine(t.Path, "sub"));
        File.WriteAllText(Path.Combine(t.Path, "sub", "a.txt"), "abc");
        Assert.Empty(HashesLedger.Verify("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad  sub/a.txt\n", t.Path));
    }
}

public class SysSnapshotTests
{
    [Fact]
    public void The_snapshot_reads_the_six_guarded_variables_in_ordinal_order()
    {
        var s = SysSnapshot.Take(FakeVars.Default());
        Assert.Equal(new[] { "CPROFILE", "DBMOD", "DWGNAME", "DWGPREFIX", "SECURELOAD", "TRUSTEDPATHS" }, s.Keys);
    }

    [Fact]
    public void A_variable_that_cannot_be_read_is_recorded_as_an_error_text()
    {
        var v = FakeVars.Default();
        v.Values.Remove("TRUSTEDPATHS");
        Assert.Equal("ERROR:NOT_FOUND", SysSnapshot.Take(v)["TRUSTEDPATHS"]);
    }

    [Fact]
    public void Differences_names_each_changed_variable()
    {
        var a = SysSnapshot.Take(FakeVars.Default());
        var v = FakeVars.Default().Set("CPROFILE", "System.String", "other").Set("DBMOD", "System.Int16", "4");
        var d = SysSnapshot.Differences(a, SysSnapshot.Take(v));
        Assert.Equal(new[] { "DBMOD_CHANGED", "CPROFILE_CHANGED" }.OrderBy(x => x), d.OrderBy(x => x));
        Assert.Empty(SysSnapshot.Differences(a, SysSnapshot.Take(FakeVars.Default())));
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("12", 12)]
    [InlineData("-1", -1)]
    public void Dbmod_parses_an_integer(string text, int expected) =>
        Assert.Equal(expected, SysSnapshot.Dbmod(new Dictionary<string, string> { ["DBMOD"] = text }));

    [Theory]
    [InlineData("x")]
    [InlineData("")]
    [InlineData("ERROR:NOT_FOUND")]
    public void Dbmod_is_null_when_unreadable(string text) =>
        Assert.Null(SysSnapshot.Dbmod(new Dictionary<string, string> { ["DBMOD"] = text }));

    [Fact]
    public void Dbmod_is_null_when_absent() => Assert.Null(SysSnapshot.Dbmod(new Dictionary<string, string>()));

    [Fact]
    public void The_snapshot_serializes_all_six_names()
    {
        var j = SysSnapshot.ToJson(SysSnapshot.Take(FakeVars.Default()));
        Assert.Equal(6, j.Count);
        Assert.Equal("0", j["DBMOD"]!.GetValue<string>());
    }
}

public class PreflightTests
{
    private static PreflightResult Check(Rig rig, InstrumentInfo? inst = null, params string[] outputs) =>
        RsPreflight.Check(rig.D, inst ?? Rig.Match, outputs.Length == 0 ? new[] { "out.json" } : outputs);

    [Fact]
    public void A_clean_rig_has_no_problem() => Assert.Empty(Check(new Rig()).Problems);

    [Fact]
    public void An_existing_output_is_a_problem_in_any_letter_case()
    {
        using var rig = new Rig();
        File.WriteAllText(Path.Combine(rig.Evidence, "OUT.json"), "x");
        Assert.Contains(Check(rig).Problems, p => p.StartsWith("OUTPUT_ALREADY_EXISTS:out.json"));
    }

    [Theory]
    [InlineData("PIN_FILE_MISSING")]
    [InlineData("PIN_MISMATCH")]
    public void A_pin_that_does_not_match_is_a_problem(string status)
    {
        using var rig = new Rig();
        Assert.Contains("SELF_PIN_" + status, Check(rig, new InstrumentInfo("x", new string('a', 64), status)).Problems);
    }

    [Fact]
    public void Path_problems_are_reported_before_anything_is_touched()
    {
        using var rig = new Rig();
        rig.With(d => new RsDesignation(d.RunId, d.Attempt, d.SessionId, "relative", d.ScratchRoot, d.PrivateCopyPath, d.PrivateCopySha256, d.LibraryPath, d.LibraryFileSha256,
            d.GovernedDocumentPath, d.ProductPaths, d.HostChecks, d.MachineClassLabel, d.BuildTupleDigest, d.PackageManifestSha256, d.DeclaredSetSha256, d.DesignBlob, d.Ba05Blob, d.DeclaredSet));
        var r = Check(rig);
        Assert.Contains("PATH_NOT_ABSOLUTE:evidenceRoot", r.Problems);
    }

    [Fact]
    public void The_scratch_files_declared_by_earlier_records_are_returned()
    {
        using var rig = new Rig();
        WriteBackRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new WriteBackFakes.HostFor(g, l, new WriteBackFakes.Host()), rig.Writer());
        var r = Check(rig);
        Assert.Empty(r.Problems);
        Assert.Equal(new[] { "CT21D_DIMWRITEBACK_OP2.dwg" }, r.PriorScratch.Select(e => e.Name));
    }

    [Fact]
    public void A_junction_as_scratch_root_is_a_problem()
    {
        using var rig = new Rig();
        var link = Path.Combine(rig.Temp.Path, "scratch-link");
        if (!Junction.TryCreate(link, rig.Scratch)) return;
        rig.With(d => new RsDesignation(d.RunId, d.Attempt, d.SessionId, d.EvidenceRoot, link, d.PrivateCopyPath, d.PrivateCopySha256, d.LibraryPath, d.LibraryFileSha256,
            d.GovernedDocumentPath, d.ProductPaths, d.HostChecks, d.MachineClassLabel, d.BuildTupleDigest, d.PackageManifestSha256, d.DeclaredSetSha256, d.DesignBlob, d.Ba05Blob, d.DeclaredSet));
        Assert.Contains("SCRATCH_ROOT_REPARSE_POINT", Check(rig).Problems);
    }

    [Fact]
    public void A_missing_private_copy_and_a_missing_library_are_problems()
    {
        using var rig = new Rig();
        File.Delete(rig.PrivateCopy);
        File.Delete(rig.Library);
        var p = Check(rig).Problems;
        Assert.Contains("PRIVATE_COPY_MISSING", p);
        Assert.Contains("LIBRARY_FILE_MISSING", p);
    }
}

public class RunRecordTests
{
    [Fact]
    public void The_run_record_builder_produces_a_schema_valid_sealed_record()
    {
        using var rig = new Rig();
        var ledger = new SideDbLedger();
        ledger.MarkDisposed(ledger.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", "x"));
        var snap = SysSnapshot.Take(FakeVars.Default());
        var record = RsRunRecord.Build("CT21DHG_TOLSCALE", "U-RS-3", rig.D, Rig.Match, new FakeEnv(), ledger, null, new ScratchVerification(new string[0], new string[0], new string[0], new string[0]),
            Array.Empty<ScratchFileEntry>(), snap, snap, Array.Empty<string>(), "PASS");
        Assert.Empty(RsSchemas.Load("ct21d.rs-run.v1.json").Validate(record));
        Assert.Equal(RecordJson.ContentSha256(record), record["contentSha256"]!.GetValue<string>());
        Assert.True(record["noAutomaticRetry"]!.GetValue<bool>());
    }

    [Fact]
    public void The_run_record_carries_the_declarations_of_the_cad_manager_not_observations()
    {
        using var rig = new Rig();
        var snap = SysSnapshot.Take(FakeVars.Default());
        var record = RsRunRecord.Build("CT21DHG_CENSUS_DYN", "U-RS-1", rig.D, Rig.Match, new FakeEnv(), new SideDbLedger(), null,
            new ScratchVerification(new string[0], new string[0], new string[0], new string[0]), Array.Empty<ScratchFileEntry>(), snap, snap, Array.Empty<string>(), "OBSERVED");
        Assert.Equal("CAD_MANAGER_DECLARATION", record["declarations"]!["source"]!.GetValue<string>());
        Assert.Equal("CONTROL_PLANE_AUXILIARY_WRITES", record["classification"]!.GetValue<string>());
    }

    [Fact]
    public void Prior_declarations_are_read_only_from_records_named_rs_run()
    {
        using var rig = new Rig();
        File.WriteAllText(Path.Combine(rig.Evidence, "notes.json"), "not a run record");
        var problems = new List<string>();
        Assert.Empty(PriorDeclarations.Load(rig.Evidence, problems));
        Assert.Empty(problems);
    }
}

public class EvidenceCompositionTests
{
    [Fact]
    public void The_designation_file_name_is_distinct_from_the_R0_one()
    {
        Assert.Equal("run-designation-rs.json", RsDesignation.FileName);
        Assert.NotEqual(RunDesignation.FileName, RsDesignation.FileName);
    }

    [Fact]
    public void The_sealer_of_the_R0_core_seals_the_evidence_folder_of_a_run()
    {
        using var rig = new Rig();
        TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeTolHost(g, l), rig.Writer());
        var sealResult = EvidenceSealer.SealOrVerify(rig.Evidence);
        Assert.True(sealResult.Sealed);
        Assert.Empty(sealResult.Differences);
        Assert.Contains("rs-run-tolscale.json", File.ReadAllText(Path.Combine(rig.Evidence, "HASHES.sha256")));
        // the scratch root is NOT sealed by the evidence seal: it is declared by the run record and checked by the verifier
        Assert.DoesNotContain("CT21D_TOLSCALE_OP2.dwg", File.ReadAllText(Path.Combine(rig.Evidence, "HASHES.sha256")));
    }

    [Fact]
    public void The_scratch_file_hash_in_the_record_is_the_hash_of_the_file_on_disk()
    {
        using var rig = new Rig();
        TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeTolHost(g, l), rig.Writer());
        var run = rig.ReadEvidence("rs-run-tolscale.json");
        var f = run["scratch"]!["files"]![0]!;
        var path = Path.Combine(rig.Scratch, f["name"]!.GetValue<string>());
        Assert.Equal(Support.Sha(path), f["sha256"]!.GetValue<string>());
        Assert.Equal(new FileInfo(path).Length, f["length"]!.GetValue<long>());
    }

    [Fact]
    public void An_evidence_file_written_by_a_run_cannot_be_overwritten_by_the_next_writer()
    {
        using var rig = new Rig();
        WriteBackRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new WriteBackFakes.HostFor(g, l, new WriteBackFakes.Host()), rig.Writer());
        var w = rig.Writer();
        foreach (var f in rig.EvidenceFiles()) Assert.Throws<EvidenceWriterException>(() => w.WriteNewText(f, "x"));
    }

    [Fact]
    public void Three_commands_in_one_evidence_root_declare_their_scratch_files_in_turn()
    {
        using var rig = new Rig();
        var census = DynCensusRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeDynHost(g, l), rig.Writer());
        Assert.Equal("NOT_OBSERVED", census.Status);
        var wb = WriteBackRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new WriteBackFakes.HostFor(g, l, new WriteBackFakes.Host()), rig.Writer());
        Assert.Equal("OBSERVED/OBSERVED", wb.Status);
        var tol = TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeTolHost(g, l), rig.Writer());
        Assert.Equal("PASS", tol.Result);
        Assert.Equal(new[] { "CT21D_DIMWRITEBACK_OP2.dwg", "CT21D_TOLSCALE_OP2.dwg" }, rig.ScratchEntries());
        var problems = new List<string>();
        var declared = PriorDeclarations.Load(rig.Evidence, problems);
        Assert.Empty(problems);
        Assert.True(ScratchVerifier.Verify(rig.Scratch, declared).Ok);
        Assert.Equal(new[] { "CT21D_DIMWRITEBACK_OP2.dwg", "CT21D_TOLSCALE_OP2.dwg" }, declared.Select(d => d.Name).OrderBy(n => n));
    }
}
