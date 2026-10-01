using System.Text;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Tools;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>The command-line entry points of the offline tools (exit codes and the files they write).</summary>
[Collection("console")]
public class ToolsCliTests
{
    private static int Run(params string[] args)
    {
        var o = Console.Out;
        var e = Console.Error;
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
        try { return Program.Main(args); }
        finally { Console.SetOut(o); Console.SetError(e); }
    }

    private static string[] Lists(string? callers = null) => new[]
    {
        "--allowed", ForbiddenApiScanTests.AllowlistPath(),
        "--privileged-surface", Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts", "privileged-surface.txt"),
        "--privileged-callers", callers ?? Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts", "privileged-callers.txt"),
        "--expected-commands", Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts", "expected-commands.txt"),
    };

    private static int RunScan(string? callers, params string[] specs) => Run(new[] { "scan" }.Concat(Lists(callers)).Concat(specs).ToArray());

    [Fact]
    public void Usage_errors_exit_1()
    {
        Assert.Equal(1, Run());
        Assert.Equal(1, Run("nonsense"));
        Assert.Equal(1, Run("scan"));
        Assert.Equal(1, RunScan(null, "nocolon"));
        Assert.Equal(1, RunScan(null, "badrole:x.dll"));
        Assert.Equal(1, Run("scan", "--allowed", ForbiddenApiScanTests.AllowlistPath(), "core:x.dll")); // the three privileged lists are mandatory too
        Assert.Equal(1, Run("manifest", "a")); // the generator is not part of this folder
        Assert.Equal(1, Run("scan", "core:x.dll")); // --allowed is mandatory
    }

    [Fact]
    public void Scan_exits_0_on_the_real_core_and_2_on_a_fixture_that_calls_a_forbidden_api()
    {
        Assert.Equal(0, RunScan(null, "core:" + typeof(EvidenceWriter).Assembly.Location));
        using var t = new TempDir();
        var fb = new FixtureBuilder("Fixture");
        fb.Type("Fx", "V").Method("M", il => il.CallVirt("AcDbMgd", "Autodesk.AutoCAD.DatabaseServices.Database", "SaveAs", "void", "string"));
        var path = Path.Combine(t.Path, "bad.dll");
        File.WriteAllBytes(path, fb.Build());
        Assert.Equal(2, RunScan(null, "r0:" + path));
        Assert.Equal(2, RunScan(null, "core:" + typeof(EvidenceWriter).Assembly.Location, "r0:" + path)); // one bad assembly fails the whole scan
    }

    [Fact]
    public void Scan_exits_2_when_a_privileged_caller_line_is_missing_and_1_when_a_list_is_malformed()
    {
        using var t = new TempDir();
        var real = Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts", "privileged-callers.txt");
        var without = Path.Combine(t.Path, "callers.txt");
        File.WriteAllText(without, string.Join("\n", File.ReadAllLines(real).Where(l => !l.Contains("Runners::RunCensus|", StringComparison.Ordinal))) + "\n");
        Assert.Equal(2, RunScan(without, "core:" + typeof(EvidenceWriter).Assembly.Location));
        var bad = Path.Combine(t.Path, "bad.txt");
        File.WriteAllText(bad, "not a caller line\n");
        Assert.Equal(1, RunScan(bad, "core:" + typeof(EvidenceWriter).Assembly.Location));
    }

    [Fact]
    public void Label_exits_0_when_set_3_when_unset_and_records_a_validated_file_without_overwriting()
    {
        using var t = new TempDir();
        Assert.Equal(3, Run("label", t.Path));
        foreach (var k in MachineLabel.Keys) File.WriteAllBytes(Path.Combine(t.Path, LabelHelper.RawFileName(k)), new UTF8Encoding(false).GetBytes("v\n"));
        foreach (var k in new[] { "AutoCadProduct", "AutoCadProfile", "SECURELOAD", "TRUSTEDPATHS" })
            File.WriteAllText(Path.Combine(t.Path, LabelHelper.DeclaredLengthFileName(k)), "1\n");
        Assert.Equal(0, Run("label", t.Path, "--record"));
        var rec = (JsonObject)Jcs.ParseStrict(File.ReadAllText(Path.Combine(t.Path, "machine-label.json")));
        Assert.Empty(JsonSchemaLite.LoadEmbedded("ct21d.machine-label.v1.json").Validate(rec));
        Assert.StartsWith("MC-", rec["label"]!.GetValue<string>());
        // the clear strings are not copied into the record
        Assert.DoesNotContain("\"v\"", File.ReadAllText(Path.Combine(t.Path, "machine-label.json")));
        Assert.ThrowsAny<EvidenceWriterException>(() => Run("label", t.Path, "--record")); // second record: create-new refuses
    }

    [Fact]
    public void Seal_exits_0_then_4_after_a_change()
    {
        using var t = new TempDir();
        var ev = t.Sub("ev");
        File.WriteAllText(Path.Combine(ev, "a.json"), "x\n");
        Assert.Equal(0, Run("seal", ev));
        Assert.Equal(0, Run("seal", ev));
        File.SetAttributes(Path.Combine(ev, "a.json"), FileAttributes.Normal);
        File.WriteAllText(Path.Combine(ev, "a.json"), "changed\n");
        Assert.Equal(4, Run("seal", ev));
    }
}

[CollectionDefinition("console", DisableParallelization = true)]
public class ConsoleCollection { }
