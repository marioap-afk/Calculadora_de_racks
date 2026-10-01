using System.Text;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

public class ScratchNameTests
{
    [Theory]
    [InlineData("CT21D_TOLSCALE_OP2.dwg")]
    [InlineData("CT21D_A.dwg")]
    [InlineData("CT21D_DIMWRITEBACK_OP2.dwg")]
    [InlineData("CT21D_a-b_c1.dwg")]
    public void A_valid_scratch_name_is_accepted(string name) => ScratchRootGuard.ValidateName(name);

    [Theory]
    [InlineData("")]
    [InlineData("x.dwg")]
    [InlineData("CT21D_.dwg")]
    [InlineData("CT21D_A.DWG")]
    [InlineData("CT21D_A.dxf")]
    [InlineData("CT21D_A.dwg.bak")]
    [InlineData("CT21D_A")]
    [InlineData("ct21d_A.dwg")]
    [InlineData("CT21D_A\\B.dwg")]
    [InlineData("CT21D_A/B.dwg")]
    [InlineData("..\\CT21D_A.dwg")]
    [InlineData("CT21D_A..B.dwg")]
    [InlineData("CT21D_A B.dwg")]
    [InlineData("CT21D_A:stream.dwg")]
    [InlineData("C:\\CT21D_A.dwg")]
    [InlineData("CT21D_A.dwg\n")]
    [InlineData("CT21D_NUL.dwg.")]
    public void An_invalid_scratch_name_is_refused(string name)
    {
        var ex = Assert.Throws<ScratchGuardException>(() => ScratchRootGuard.ValidateName(name));
        Assert.Equal(ScratchGuardError.NameInvalid, ex.Error);
    }

    [Fact]
    public void A_name_longer_than_the_pattern_is_refused() =>
        Assert.Throws<ScratchGuardException>(() => ScratchRootGuard.ValidateName("CT21D_" + new string('a', 90) + ".dwg"));
}

public class ScratchRootGuardTests
{
    private static (ScratchRootGuard Guard, Rig Rig) NewGuard()
    {
        var rig = new Rig();
        return (new ScratchRootGuard(rig.Scratch, rig.PrivateCopy), rig);
    }

    private static void Save(ScratchTarget t, string text = "bytes") => File.WriteAllBytes(t.FullPath, Encoding.ASCII.GetBytes(text));

    [Fact]
    public void The_root_must_be_an_existing_absolute_directory()
    {
        using var rig = new Rig();
        Assert.Throws<ScratchGuardException>(() => new ScratchRootGuard("relative", rig.PrivateCopy));
        Assert.Throws<ScratchGuardException>(() => new ScratchRootGuard(Path.Combine(rig.Temp.Path, "missing"), rig.PrivateCopy));
        Assert.Throws<ScratchGuardException>(() => new ScratchRootGuard(rig.Library, rig.PrivateCopy)); // a file is not a directory
        Assert.Throws<ScratchGuardException>(() => new ScratchRootGuard("C:\\a\\..\\b", rig.PrivateCopy));
    }

    [Fact]
    public void A_reparse_point_root_is_refused()
    {
        using var rig = new Rig();
        var link = Path.Combine(rig.Temp.Path, "link");
        if (!Junction.TryCreate(link, rig.Scratch)) return;
        var ex = Assert.Throws<ScratchGuardException>(() => new ScratchRootGuard(link, rig.PrivateCopy));
        Assert.Equal(ScratchGuardError.ReparsePoint, ex.Error);
    }

    [Fact]
    public void AuthorizeNew_returns_a_flat_path_under_the_root()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            var t = guard.AuthorizeNew("CT21D_X.dwg", "ROLE");
            Assert.Equal(Path.Combine(guard.Root, "CT21D_X.dwg"), t.FullPath);
            Assert.Equal("ROLE", t.Role);
            Assert.False(File.Exists(t.FullPath)); // the guard writes nothing
            Assert.Empty(rig.ScratchEntries());
        }
    }

    [Fact]
    public void AuthorizeNew_refuses_an_existing_file_in_any_letter_case()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            File.WriteAllText(Path.Combine(rig.Scratch, "CT21D_X.dwg"), "x");
            var ex = Assert.Throws<ScratchGuardException>(() => guard.AuthorizeNew("CT21D_X.dwg", "R"));
            Assert.Equal(ScratchGuardError.AlreadyExists, ex.Error);
            File.WriteAllText(Path.Combine(rig.Scratch, "ct21d_y.DWG"), "y");
            Assert.Throws<ScratchGuardException>(() => guard.AuthorizeNew("CT21D_Y.dwg", "R")); // never matches the pattern, still refused
        }
    }

    [Fact]
    public void AuthorizeNew_refuses_an_existing_directory_of_that_name()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            Directory.CreateDirectory(Path.Combine(rig.Scratch, "CT21D_D.dwg"));
            Assert.Throws<ScratchGuardException>(() => guard.AuthorizeNew("CT21D_D.dwg", "R"));
        }
    }

    [Fact]
    public void AuthorizeNew_refuses_the_same_name_twice()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            guard.AuthorizeNew("CT21D_X.dwg", "R");
            var ex = Assert.Throws<ScratchGuardException>(() => guard.AuthorizeNew("CT21D_X.dwg", "R"));
            Assert.Equal(ScratchGuardError.AlreadyAuthorized, ex.Error);
        }
    }

    [Fact]
    public void AuthorizeNew_is_bounded()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            for (var i = 0; i < ScratchRootGuard.MaxFiles; i++) guard.AuthorizeNew("CT21D_F" + i + ".dwg", "R");
            var ex = Assert.Throws<ScratchGuardException>(() => guard.AuthorizeNew("CT21D_ONE_TOO_MANY.dwg", "R"));
            Assert.Equal(ScratchGuardError.TooManyFiles, ex.Error);
        }
    }

    [Fact]
    public void AssertCreatable_accepts_a_fresh_target_and_refuses_a_foreign_one()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        using (var other = new Rig())
        {
            var t = guard.AuthorizeNew("CT21D_X.dwg", "R");
            guard.AssertCreatable(t);
            var otherGuard = new ScratchRootGuard(other.Scratch, other.PrivateCopy);
            var ex = Assert.Throws<ScratchGuardException>(() => otherGuard.AssertCreatable(t));
            Assert.Equal(ScratchGuardError.ForeignTarget, ex.Error);
        }
    }

    [Fact]
    public void AssertCreatable_refuses_a_target_that_appeared_after_the_authorization()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            var t = guard.AuthorizeNew("CT21D_X.dwg", "R");
            File.WriteAllText(t.FullPath, "somebody else wrote it");
            var ex = Assert.Throws<ScratchGuardException>(() => guard.AssertCreatable(t));
            Assert.Equal(ScratchGuardError.AlreadyExists, ex.Error);
        }
    }

    [Fact]
    public void RecordSaved_records_the_length_and_the_sha256_and_refuses_a_second_save()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            var t = guard.AuthorizeNew("CT21D_X.dwg", "ROLE");
            guard.AssertCreatable(t);
            Save(t, "hello");
            var entry = guard.RecordSaved(t);
            Assert.Equal("CT21D_X.dwg", entry.Name);
            Assert.Equal(5, entry.Length);
            Assert.Equal(Support.Sha(Encoding.ASCII.GetBytes("hello")), entry.Sha256);
            Assert.Equal("ROLE", entry.Role);
            Assert.Single(guard.Ledger);
            Assert.Equal(ScratchGuardError.AlreadySaved, Assert.Throws<ScratchGuardException>(() => guard.RecordSaved(t)).Error);
            Assert.Equal(ScratchGuardError.AlreadySaved, Assert.Throws<ScratchGuardException>(() => guard.AssertCreatable(t)).Error);
        }
    }

    [Fact]
    public void RecordSaved_refuses_when_the_file_was_not_written()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            var t = guard.AuthorizeNew("CT21D_X.dwg", "R");
            Assert.Equal(ScratchGuardError.NotSaved, Assert.Throws<ScratchGuardException>(() => guard.RecordSaved(t)).Error);
            Assert.Empty(guard.Ledger);
        }
    }

    [Fact]
    public void RecordSaved_refuses_a_foreign_target()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        using (var other = new Rig())
        {
            var og = new ScratchRootGuard(other.Scratch, other.PrivateCopy);
            var t = og.AuthorizeNew("CT21D_X.dwg", "R");
            Save(t);
            Assert.Equal(ScratchGuardError.ForeignTarget, Assert.Throws<ScratchGuardException>(() => guard.RecordSaved(t)).Error);
        }
    }

    [Fact]
    public void The_private_copy_is_readable_only_at_the_designated_path()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            var src = guard.AuthorizePrivateCopy(Support.Sha(rig.PrivateCopy));
            Assert.Equal(ReadableKind.PrivateCopy, src.Kind);
            guard.AssertReadable(src);
            Assert.Equal(PathRegions.Norm(rig.PrivateCopy), src.FullPath);
        }
    }

    [Fact]
    public void The_private_copy_with_a_changed_hash_is_not_readable()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            var src = guard.AuthorizePrivateCopy(Support.Sha(rig.PrivateCopy));
            File.AppendAllText(rig.PrivateCopy, "tamper");
            Assert.Equal(ScratchGuardError.HashMismatch, Assert.Throws<ScratchGuardException>(() => guard.AssertReadable(src)).Error);
        }
    }

    [Fact]
    public void The_library_itself_is_never_a_readable_source()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            // there is no API that authorizes the library path: the only private source is the designated copy
            var src = guard.AuthorizePrivateCopy(Support.Sha(rig.PrivateCopy));
            Assert.False(PathRegions.Same(src.FullPath, rig.Library));
        }
    }

    [Fact]
    public void A_missing_private_copy_is_not_authorized()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            File.Delete(rig.PrivateCopy);
            Assert.Equal(ScratchGuardError.SourceNotAuthorized, Assert.Throws<ScratchGuardException>(() => guard.AuthorizePrivateCopy(Z())).Error);
        }
    }

    private static string Z() => new('0', 64);

    [Fact]
    public void A_saved_scratch_file_is_readable_back_while_its_hash_holds()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            var t = guard.AuthorizeNew("CT21D_X.dwg", "R");
            Save(t, "payload");
            var entry = guard.RecordSaved(t);
            var src = guard.AuthorizeReadBack(entry);
            Assert.Equal(ReadableKind.SavedScratch, src.Kind);
            guard.AssertReadable(src);
            File.WriteAllText(t.FullPath, "changed after the save");
            Assert.Equal(ScratchGuardError.HashMismatch, Assert.Throws<ScratchGuardException>(() => guard.AssertReadable(src)).Error);
        }
    }

    [Fact]
    public void A_ledger_entry_of_another_guard_is_not_readable()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        using (var other = new Rig())
        {
            var og = new ScratchRootGuard(other.Scratch, other.PrivateCopy);
            var t = og.AuthorizeNew("CT21D_X.dwg", "R");
            Save(t);
            var entry = og.RecordSaved(t);
            Assert.Equal(ScratchGuardError.SourceNotAuthorized, Assert.Throws<ScratchGuardException>(() => guard.AuthorizeReadBack(entry)).Error);
        }
    }

    [Fact]
    public void A_source_of_another_guard_is_not_readable()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        using (var other = new Rig())
        {
            var og = new ScratchRootGuard(other.Scratch, other.PrivateCopy);
            var src = og.AuthorizePrivateCopy(Support.Sha(other.PrivateCopy));
            Assert.Equal(ScratchGuardError.SourceNotAuthorized, Assert.Throws<ScratchGuardException>(() => guard.AssertReadable(src)).Error);
        }
    }

    [Fact]
    public void A_scratch_file_deleted_before_the_read_is_not_readable()
    {
        var (guard, rig) = NewGuard();
        using (rig)
        {
            var t = guard.AuthorizeNew("CT21D_X.dwg", "R");
            Save(t);
            var src = guard.AuthorizeReadBack(guard.RecordSaved(t));
            File.Delete(t.FullPath);
            Assert.Equal(ScratchGuardError.SourceNotAuthorized, Assert.Throws<ScratchGuardException>(() => guard.AssertReadable(src)).Error);
        }
    }
}

public class ScratchVerifierTests
{
    private static ScratchFileEntry Entry(string name, string content, string role = "R") =>
        new(name, Encoding.ASCII.GetByteCount(content), Support.Sha(Encoding.ASCII.GetBytes(content)), role);

    [Fact]
    public void An_empty_root_with_no_declaration_is_clean()
    {
        using var t = new TempDir();
        var v = ScratchVerifier.Verify(t.Sub("s"), Array.Empty<ScratchFileEntry>());
        Assert.True(v.Ok);
    }

    [Fact]
    public void A_declared_file_with_its_hash_is_clean()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        File.WriteAllText(Path.Combine(s, "CT21D_A.dwg"), "abc");
        Assert.True(ScratchVerifier.Verify(s, new[] { Entry("CT21D_A.dwg", "abc") }).Ok);
    }

    [Fact]
    public void An_undeclared_file_fails_the_verification()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        File.WriteAllText(Path.Combine(s, "CT21D_A.dwg"), "abc");
        File.WriteAllText(Path.Combine(s, "stray.txt"), "x");
        var v = ScratchVerifier.Verify(s, new[] { Entry("CT21D_A.dwg", "abc") });
        Assert.False(v.Ok);
        Assert.Equal(new[] { "stray.txt" }, v.Undeclared);
    }

    [Fact]
    public void A_hidden_file_is_listed_too()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        var hidden = Path.Combine(s, "hidden.dat");
        File.WriteAllText(hidden, "x");
        File.SetAttributes(hidden, FileAttributes.Hidden | FileAttributes.System);
        Assert.Equal(new[] { "hidden.dat" }, ScratchVerifier.Verify(s, Array.Empty<ScratchFileEntry>()).Undeclared);
    }

    [Fact]
    public void An_undeclared_directory_and_the_files_inside_it_are_listed()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        Directory.CreateDirectory(Path.Combine(s, "sub"));
        File.WriteAllText(Path.Combine(s, "sub", "deep.dwg"), "x");
        var v = ScratchVerifier.Verify(s, Array.Empty<ScratchFileEntry>());
        Assert.False(v.Ok);
        Assert.Contains("sub", v.Undeclared);
        Assert.Contains(Path.Combine("sub", "deep.dwg"), v.Undeclared);
    }

    [Fact]
    public void A_declared_file_inside_a_subfolder_does_not_count_as_declared()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        Directory.CreateDirectory(Path.Combine(s, "sub"));
        File.WriteAllText(Path.Combine(s, "sub", "CT21D_A.dwg"), "abc");
        var v = ScratchVerifier.Verify(s, new[] { Entry("CT21D_A.dwg", "abc") });
        Assert.False(v.Ok);
        Assert.Contains("CT21D_A.dwg", v.Missing);
    }

    [Fact]
    public void A_missing_declared_file_fails()
    {
        using var t = new TempDir();
        var v = ScratchVerifier.Verify(t.Sub("s"), new[] { Entry("CT21D_A.dwg", "abc") });
        Assert.False(v.Ok);
        Assert.Equal(new[] { "CT21D_A.dwg" }, v.Missing);
    }

    [Fact]
    public void A_changed_declared_file_fails()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        File.WriteAllText(Path.Combine(s, "CT21D_A.dwg"), "abc");
        var v = ScratchVerifier.Verify(s, new[] { Entry("CT21D_A.dwg", "different") });
        Assert.False(v.Ok);
        Assert.Equal(new[] { "CT21D_A.dwg" }, v.HashMismatches);
    }

    [Fact]
    public void The_name_comparison_ignores_letter_case_like_the_file_system()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        File.WriteAllText(Path.Combine(s, "ct21d_a.dwg"), "abc");
        Assert.True(ScratchVerifier.Verify(s, new[] { Entry("CT21D_A.dwg", "abc") }).Ok);
    }

    [Fact]
    public void A_junction_inside_the_root_is_reported_as_a_reparse_point()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        var target = t.Sub("target");
        if (!Junction.TryCreate(Path.Combine(s, "j"), target)) return;
        var v = ScratchVerifier.Verify(s, Array.Empty<ScratchFileEntry>());
        Assert.False(v.Ok);
        Assert.Contains("j", v.ReparsePoints);
    }

    [Fact]
    public void The_verification_serializes_to_json_with_its_verdict()
    {
        using var t = new TempDir();
        var s = t.Sub("s");
        File.WriteAllText(Path.Combine(s, "stray.txt"), "x");
        var j = ScratchVerifier.Verify(s, Array.Empty<ScratchFileEntry>()).ToJson();
        Assert.False(j["ok"]!.GetValue<bool>());
        Assert.Equal("stray.txt", j["undeclared"]![0]!.GetValue<string>());
    }
}

public class SideDbLedgerTests
{
    [Fact]
    public void Entries_are_numbered_and_start_undisposed()
    {
        var l = new SideDbLedger();
        Assert.Equal("SDB-01", l.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", "a"));
        Assert.Equal("SDB-02", l.Register("PRIVATE_COPY_READ", "new Database(false, true)", "ReadDwgFile", "b"));
        Assert.False(l.AllDisposed);
        Assert.All(l.Entries, e => Assert.False(e.Disposed));
    }

    [Fact]
    public void MarkDisposed_sets_the_flag_and_AllDisposed_follows()
    {
        var l = new SideDbLedger();
        var a = l.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", "a");
        var b = l.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", "b");
        l.MarkDisposed(a);
        Assert.False(l.AllDisposed);
        l.MarkDisposed(b);
        Assert.True(l.AllDisposed);
    }

    [Fact]
    public void An_unknown_id_is_an_error() => Assert.Throws<InvalidOperationException>(() => new SideDbLedger().MarkDisposed("SDB-99"));

    [Fact]
    public void An_empty_ledger_counts_as_all_disposed() => Assert.True(new SideDbLedger().AllDisposed);

    [Fact]
    public void The_entry_json_carries_the_identification()
    {
        var l = new SideDbLedger();
        l.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", "purpose");
        var j = l.Entries[0].ToJson();
        Assert.Equal("SDB-01", j["id"]!.GetValue<string>());
        Assert.Equal("new Database(true, true)", j["constructor"]!.GetValue<string>());
        Assert.False(j["disposed"]!.GetValue<bool>());
    }
}
