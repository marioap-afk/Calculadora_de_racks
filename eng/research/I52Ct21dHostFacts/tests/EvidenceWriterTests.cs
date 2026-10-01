using System.Reflection;
using System.Text;
using I52Ct21d.HostFacts.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>The single EvidenceWriter: create-new, flat, write-once, hashed, no overwrite, no append, nothing outside its folder.</summary>
public class EvidenceWriterTests
{
    private static string[] Listing(string dir) =>
        Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.AllDirectories).Select(p => Path.GetRelativePath(dir, p)).OrderBy(x => x, StringComparer.Ordinal).ToArray();

    [Fact]
    public void Writes_a_new_file_and_records_its_sha256()
    {
        using var t = new TempDir();
        var w = new EvidenceWriter(t.Path);
        var f = w.WriteNew("abc.bin", Encoding.ASCII.GetBytes("abc"));
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", f.Sha256); // FIPS 180 test vector for "abc"
        Assert.Equal(3, f.Length);
        Assert.Equal(f.Sha256, Support.Sha(Path.Combine(t.Path, "abc.bin")));
        Assert.Single(w.Ledger);
        Assert.Equal(new[] { "abc.bin" }, Listing(t.Path));
    }

    [Fact]
    public void Refuses_to_overwrite_a_file_it_wrote()
    {
        using var t = new TempDir();
        var w = new EvidenceWriter(t.Path);
        w.WriteNew("a.json", Encoding.UTF8.GetBytes("first"));
        var ex = Assert.Throws<EvidenceWriterException>(() => w.WriteNew("a.json", Encoding.UTF8.GetBytes("second")));
        Assert.Equal(EvidenceWriteError.AlreadyWrittenByThisWriter, ex.Error);
        Assert.Equal("first", File.ReadAllText(Path.Combine(t.Path, "a.json")));
        Assert.Single(w.Ledger);
    }

    [Fact]
    public void Refuses_to_overwrite_or_append_to_a_file_another_writer_or_a_person_created()
    {
        using var t = new TempDir();
        File.WriteAllText(Path.Combine(t.Path, "existing.txt"), "original");
        var w = new EvidenceWriter(t.Path);
        var ex = Assert.Throws<EvidenceWriterException>(() => w.WriteNew("existing.txt", Encoding.UTF8.GetBytes("replacement")));
        Assert.Equal(EvidenceWriteError.AlreadyExists, ex.Error);
        Assert.Equal("original", File.ReadAllText(Path.Combine(t.Path, "existing.txt"))); // neither truncated nor appended
        Assert.Empty(w.Ledger);

        var second = new EvidenceWriter(t.Path);
        second.WriteNew("fresh.txt", new byte[] { 1 });
        var again = new EvidenceWriter(t.Path);
        Assert.Equal(EvidenceWriteError.AlreadyExists, Assert.Throws<EvidenceWriterException>(() => again.WriteNew("fresh.txt", new byte[] { 2 })).Error);
        Assert.Equal(new byte[] { 1 }, File.ReadAllBytes(Path.Combine(t.Path, "fresh.txt")));
    }

    [Fact]
    public void A_case_insensitive_twin_counts_as_existing()
    {
        using var t = new TempDir();
        File.WriteAllText(Path.Combine(t.Path, "Raw.txt"), "x");
        var w = new EvidenceWriter(t.Path);
        Assert.Equal(EvidenceWriteError.AlreadyExists, Assert.Throws<EvidenceWriterException>(() => w.WriteNew("raw.txt", new byte[] { 1 })).Error);
        Assert.Equal(new[] { "Raw.txt" }, Listing(t.Path));
    }

    [Theory]
    [InlineData("..\\x.txt")]
    [InlineData("../x.txt")]
    [InlineData("sub/x.txt")]
    [InlineData("sub\\x.txt")]
    [InlineData("C:\\x.txt")]
    [InlineData("C:x.txt")]
    [InlineData("x.txt:stream")]
    [InlineData("\\\\server\\share\\x.txt")]
    [InlineData("/etc/x")]
    [InlineData("")]
    [InlineData(".hidden")]
    [InlineData("..")]
    [InlineData(".")]
    [InlineData("a..b")]
    [InlineData("trailing.")]
    [InlineData("with space.txt")]
    [InlineData("CON")]
    [InlineData("nul.txt")]
    [InlineData("COM1.json")]
    public void Refuses_a_name_that_is_not_a_flat_name_inside_the_folder(string name)
    {
        using var t = new TempDir();
        var inner = t.Sub("evidence");
        var before = Listing(t.Path);
        var w = new EvidenceWriter(inner);
        var ex = Assert.Throws<EvidenceWriterException>(() => w.WriteNew(name, new byte[] { 1 }));
        Assert.Equal(EvidenceWriteError.InvalidName, ex.Error);
        Assert.Equal(before, Listing(t.Path)); // nothing was created anywhere, and no directory either
        Assert.Empty(w.Ledger);
    }

    [Fact]
    public void Refuses_a_name_longer_than_120_characters()
    {
        using var t = new TempDir();
        var w = new EvidenceWriter(t.Path);
        Assert.Equal(EvidenceWriteError.InvalidName, Assert.Throws<EvidenceWriterException>(() => w.WriteNew(new string('a', 121), new byte[] { 1 })).Error);
        w.WriteNew(new string('a', 120), new byte[] { 1 });
    }

    [Fact]
    public void Refuses_a_root_that_is_relative_missing_or_not_a_folder_and_never_creates_it()
    {
        using var t = new TempDir();
        Assert.Equal(EvidenceWriteError.RootInvalid, Assert.Throws<EvidenceWriterException>(() => new EvidenceWriter("relative\\folder")).Error);
        var missing = Path.Combine(t.Path, "does-not-exist");
        Assert.Equal(EvidenceWriteError.RootInvalid, Assert.Throws<EvidenceWriterException>(() => new EvidenceWriter(missing)).Error);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(EvidenceWriteError.RootInvalid, Assert.Throws<EvidenceWriterException>(() => new EvidenceWriter("")).Error);
    }

    [Fact]
    public void Text_is_utf8_without_bom_and_lf_only()
    {
        using var t = new TempDir();
        var w = new EvidenceWriter(t.Path);
        w.WriteNewText("t.txt", "a\u00e9\n");
        Assert.Equal(new byte[] { 0x61, 0xC3, 0xA9, 0x0A }, File.ReadAllBytes(Path.Combine(t.Path, "t.txt")));
        Assert.Equal(EvidenceWriteError.InvalidText, Assert.Throws<EvidenceWriterException>(() => w.WriteNewText("u.txt", "a\r\nb")).Error);
        Assert.False(File.Exists(Path.Combine(t.Path, "u.txt")));
    }

    [Fact]
    public void Public_surface_has_no_way_to_append_overwrite_delete_or_write_elsewhere()
    {
        var methods = typeof(EvidenceWriter).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(m => m.Name).Where(n => !n.StartsWith("get_", StringComparison.Ordinal)).OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "MarkReadOnly", "ValidateName", "WriteNew", "WriteNewText" }, methods);
        Assert.All(typeof(EvidenceWriter).GetConstructors(), c => Assert.Single(c.GetParameters()));
    }

    [Fact]
    public void MarkReadOnly_changes_only_the_attribute_of_an_existing_flat_file()
    {
        using var t = new TempDir();
        var w = new EvidenceWriter(t.Path);
        w.WriteNew("a.txt", new byte[] { 1 });
        w.MarkReadOnly("a.txt");
        Assert.True((File.GetAttributes(Path.Combine(t.Path, "a.txt")) & FileAttributes.ReadOnly) != 0);
        Assert.Equal(new byte[] { 1 }, File.ReadAllBytes(Path.Combine(t.Path, "a.txt")));
        Assert.Throws<EvidenceWriterException>(() => w.MarkReadOnly("missing.txt"));
        Assert.Throws<EvidenceWriterException>(() => w.MarkReadOnly("..\\a.txt"));
    }

    [Fact]
    public void A_read_only_file_can_not_be_replaced_either()
    {
        using var t = new TempDir();
        var w = new EvidenceWriter(t.Path);
        w.WriteNew("a.txt", new byte[] { 1 });
        w.MarkReadOnly("a.txt");
        var again = new EvidenceWriter(t.Path);
        Assert.Throws<EvidenceWriterException>(() => again.WriteNew("a.txt", new byte[] { 2 }));
        Assert.Equal(new byte[] { 1 }, File.ReadAllBytes(Path.Combine(t.Path, "a.txt")));
    }
}
