using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

/// <summary>Negative controls for the independent reviews of round 1 (the reviewers' attacks, reproduced).</summary>
public class ReviewRound1Tests
{
    [Theory]
    [InlineData("C:\\")]
    [InlineData("C:/")]
    public void A_drive_root_is_refused_as_a_scratch_root_and_by_the_path_rules(string root)
    {
        using var rig = new Rig();
        Assert.True(PathRegions.IsDriveRoot(root));
        Assert.Equal("DRIVE_ROOT", PathRegions.TextProblem(root));
        Assert.Throws<ScratchGuardException>(() => new ScratchRootGuard(root, rig.PrivateCopy));
    }

    [Fact]
    public void An_ordinary_directory_is_not_a_drive_root()
    {
        using var rig = new Rig();
        Assert.False(PathRegions.IsDriveRoot(rig.Scratch));
    }

    [Fact]
    public void A_private_copy_that_is_a_symbolic_link_is_refused_by_the_guard_and_the_preflight()
    {
        using var rig = new Rig();
        var link = Path.Combine(rig.Temp.Path, "private", "link-copy.dwg");
        try { File.CreateSymbolicLink(link, rig.Library); }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException) { return; } // no privilege to create a symbolic link here
        var guard = new ScratchRootGuard(rig.Scratch, link);
        var e = Assert.Throws<ScratchGuardException>(() => guard.AuthorizePrivateCopy(rig.D.PrivateCopySha256));
        Assert.Equal(ScratchGuardError.ReparsePoint, e.Error);
        rig.With(d => new RsDesignation(d.RunId, d.Attempt, d.SessionId, d.EvidenceRoot, d.ScratchRoot, link, d.PrivateCopySha256, d.LibraryPath, d.LibraryFileSha256,
            d.GovernedDocumentPath, d.ProductPaths, d.HostChecks, d.MachineClassLabel, d.BuildTupleDigest, d.PackageManifestSha256, d.DeclaredSetSha256, d.DesignBlob,
            d.Ba05Blob, d.DeclaredSet));
        Assert.Contains("PRIVATE_COPY_IS_A_REPARSE_POINT", RsPreflight.Check(rig.D, Rig.Match, TolScaleRunner.OutputNames).Problems);
    }

    [Fact]
    public void After_a_failure_following_the_side_effects_the_scratch_ledger_is_still_declared_and_never_overwritten()
    {
        using var rig = new Rig();
        var guard = new ScratchRootGuard(rig.Scratch, rig.PrivateCopy);
        var target = guard.AuthorizeNew("CT21D_TOLSCALE_OP2.dwg", "TOLSCALE_OP2");
        File.WriteAllBytes(target.FullPath, new byte[] { 1, 2, 3 });
        guard.RecordSaved(target);
        var verification = ScratchVerifier.Verify(rig.Scratch, guard.Ledger);
        var snapshot = SysSnapshot.Take(new FakeVars());
        var writer = new EvidenceWriter(rig.Evidence);
        var text = RsFailSafe.TryBuild(TolScaleRunner.Command, "U-RS-3", rig.D, Rig.Match, new FakeEnv(), new SideDbLedger(), guard, verification,
            Array.Empty<ScratchFileEntry>(), snapshot, snapshot, new List<string>(), new InvalidOperationException("x"));
        Assert.NotNull(text);
        writer.WriteNewText(TolScaleRunner.RunFile, text!);
        var path = Path.Combine(rig.Evidence, TolScaleRunner.RunFile);
        var record = (JsonObject)Jcs.ParseStrict(File.ReadAllText(path));
        Assert.Equal("INVALID", record["result"]!.GetValue<string>());
        Assert.Contains("POST_PROCESSING_FAILED:InvalidOperationException", record["invalidReasons"]!.AsArray().Select(n => n!.GetValue<string>()));
        Assert.Equal("CT21D_TOLSCALE_OP2.dwg", record["scratch"]!["files"]![0]!["name"]!.GetValue<string>());
        // the writer is create-new: a second declaration cannot overwrite the first
        var bytes = File.ReadAllBytes(path);
        Assert.Throws<EvidenceWriterException>(() => writer.WriteNewText(TolScaleRunner.RunFile, text!));
        Assert.Equal(bytes, File.ReadAllBytes(path));
        // the next preflight reads the declaration, so the scratch file is declared (the run itself is refused: its output exists)
        var pre = RsPreflight.Check(rig.D, Rig.Match, TolScaleRunner.OutputNames);
        Assert.DoesNotContain(pre.Problems, p => p.StartsWith("SCRATCH_ROOT_HAS_UNDECLARED_ENTRY_AT_START", StringComparison.Ordinal));
        Assert.Contains(pre.Problems, p => p.StartsWith("OUTPUT_ALREADY_EXISTS", StringComparison.Ordinal));
    }
}
