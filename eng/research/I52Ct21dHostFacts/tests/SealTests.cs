using System.Text;
using I52Ct21d.HostFacts.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>I-9 evidence seal and the self-pin comparison (design 5.1). The manifest / pin generator (I-8) is not in this folder.</summary>
public class SealTests
{
    private static void Put(string dir, string name, string content) => File.WriteAllText(Path.Combine(dir, name), content, new UTF8Encoding(false));

    [Fact]
    public void Seal_writes_sorted_hashes_a_digest_and_marks_every_file_read_only()
    {
        using var t = new TempDir();
        Put(t.Path, "b.json", "bee\n");
        Put(t.Path, "a.json", "ay\n");
        var r = EvidenceSealer.SealOrVerify(t.Path);
        Assert.True(r.Sealed);
        Assert.False(r.NoChange);
        var hashes = File.ReadAllText(Path.Combine(t.Path, "HASHES.sha256"));
        var expected = Support.Sha(Encoding.UTF8.GetBytes("ay\n")) + "  a.json\n" + Support.Sha(Encoding.UTF8.GetBytes("bee\n")) + "  b.json\n";
        Assert.Equal(expected, hashes);
        Assert.Equal(Support.Sha(Encoding.UTF8.GetBytes(expected)) + "  HASHES.sha256\n", File.ReadAllText(Path.Combine(t.Path, "HASHES.sha256.digest")));
        Assert.Equal(Support.Sha(Encoding.UTF8.GetBytes(expected)), r.HashesFileSha256);
        foreach (var f in Directory.EnumerateFiles(t.Path))
            Assert.True((File.GetAttributes(f) & FileAttributes.ReadOnly) != 0, f);
        Assert.Equal(2, r.Written.Count);
    }

    [Fact]
    public void Rerun_on_a_sealed_folder_reports_no_change_and_writes_nothing()
    {
        using var t = new TempDir();
        Put(t.Path, "a.json", "x\n");
        EvidenceSealer.SealOrVerify(t.Path);
        var before = Directory.EnumerateFiles(t.Path).OrderBy(x => x, StringComparer.Ordinal).Select(f => (f, Support.Sha(f), File.GetLastWriteTimeUtc(f))).ToArray();
        var again = EvidenceSealer.SealOrVerify(t.Path);
        Assert.True(again.NoChange);
        Assert.Empty(again.Written);
        Assert.Empty(again.Differences);
        var after = Directory.EnumerateFiles(t.Path).OrderBy(x => x, StringComparer.Ordinal).Select(f => (f, Support.Sha(f), File.GetLastWriteTimeUtc(f))).ToArray();
        Assert.Equal(before, after);
    }

    [Fact]
    public void Rerun_reports_a_changed_a_missing_and_an_extra_file_and_a_tampered_hashes_file()
    {
        using var t = new TempDir();
        Put(t.Path, "a.json", "x\n");
        Put(t.Path, "b.json", "y\n");
        EvidenceSealer.SealOrVerify(t.Path);
        File.SetAttributes(Path.Combine(t.Path, "a.json"), FileAttributes.Normal);
        File.WriteAllText(Path.Combine(t.Path, "a.json"), "tampered\n");
        File.SetAttributes(Path.Combine(t.Path, "b.json"), FileAttributes.Normal);
        File.Delete(Path.Combine(t.Path, "b.json"));
        Put(t.Path, "c.json", "new\n");
        var r = EvidenceSealer.SealOrVerify(t.Path);
        Assert.False(r.NoChange);
        Assert.Equal(new[] { "CHANGED a.json", "EXTRA c.json", "MISSING b.json" }, r.Differences.ToArray());

        File.SetAttributes(Path.Combine(t.Path, "HASHES.sha256"), FileAttributes.Normal);
        File.AppendAllText(Path.Combine(t.Path, "HASHES.sha256"), "");
        File.WriteAllText(Path.Combine(t.Path, "HASHES.sha256"), File.ReadAllText(Path.Combine(t.Path, "HASHES.sha256")) + "# x\n");
        Assert.Contains(EvidenceSealer.SealOrVerify(t.Path).Differences, d => d.StartsWith("CHANGED HASHES.sha256", StringComparison.Ordinal) || d.StartsWith("MALFORMED_LINE", StringComparison.Ordinal));
    }

    [Fact]
    public void A_folder_with_a_subdirectory_is_not_sealed()
    {
        using var t = new TempDir();
        Put(t.Path, "a.json", "x\n");
        Directory.CreateDirectory(Path.Combine(t.Path, "sub"));
        var r = EvidenceSealer.SealOrVerify(t.Path);
        Assert.False(r.Sealed);
        Assert.Equal(new[] { "UNDECLARED_DIRECTORY sub" }, r.Differences.ToArray());
        Assert.False(File.Exists(Path.Combine(t.Path, "HASHES.sha256")));
    }

    [Fact]
    public void Seal_never_overwrites_an_existing_hashes_file()
    {
        using var t = new TempDir();
        Put(t.Path, "a.json", "x\n");
        EvidenceSealer.SealOrVerify(t.Path);
        var hashes = File.ReadAllBytes(Path.Combine(t.Path, "HASHES.sha256"));
        EvidenceSealer.SealOrVerify(t.Path);
        Assert.Equal(hashes, File.ReadAllBytes(Path.Combine(t.Path, "HASHES.sha256")));
    }

    [Fact]
    public void Self_pin_matches_only_the_exact_file_hash_and_reports_a_missing_or_wrong_sidecar()
    {
        using var t = new TempDir();
        var dll = Path.Combine(t.Path, "X.dll");
        File.WriteAllBytes(dll, new byte[] { 1, 2, 3 });
        Assert.Equal("PIN_FILE_MISSING", SelfPin.Verify("X", dll).SelfPinStatus);
        File.WriteAllText(SelfPin.PinFileFor(dll), Support.Sha(dll) + "\n");
        var ok = SelfPin.Verify("X", dll);
        Assert.Equal(InstrumentInfo.PinMatch, ok.SelfPinStatus);
        Assert.Equal(Support.Sha(dll), ok.Sha256);
        File.WriteAllBytes(dll, new byte[] { 1, 2, 4 });
        Assert.Equal("PIN_MISMATCH", SelfPin.Verify("X", dll).SelfPinStatus);
    }
}
