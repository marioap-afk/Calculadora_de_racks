using System.Text;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using I52Ct21d.HostFacts.Rs.Tools;
using I52Ct21d.HostFacts.Tools.Scan;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

public class R0IsUntouchedTests
{
    [Theory]
    [InlineData("allowed-apis.txt", "b2cb0d26ac8dd5ce53a70845d62b2c3237e678aa586e4cc2f361687884a9d880")]
    [InlineData("privileged-surface.txt", "7a56a8e77c841e0f481dba07dc0297714124ea716e978d3f2bbe497a1887949c")]
    [InlineData("privileged-callers.txt", "278e8ccfa5da25a53614d4c54d958f0bf4c212f6e07eea62303da046899a3c07")]
    [InlineData("expected-commands.txt", "f3465afd52bc856a1852a9881a2130c2f801c54d0ebbc9428f95c85fb0267a9f")]
    public void The_four_R0_lists_keep_the_hashes_that_decisions_section_240_registered(string file, string sha) =>
        Assert.Equal(sha, Support.Sha(Path.Combine(Support.R0Folder(), file)));

    [Fact]
    public void The_RS_folder_is_a_sibling_of_the_R0_folder_and_the_R0_folder_has_no_RS_file()
    {
        Assert.True(Directory.Exists(Support.RsFolder()));
        Assert.NotEqual(Support.R0Folder(), Support.RsFolder());
        var sep = Path.DirectorySeparatorChar;
        var files = Directory.EnumerateFiles(Support.R0Folder(), "*", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{sep}bin{sep}") && !p.Contains($"{sep}obj{sep}")).ToList();
        Assert.DoesNotContain(files, p => Path.GetFileName(p).Contains("-rs", StringComparison.OrdinalIgnoreCase) || Path.GetFileName(p).Contains(".rs.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Nothing_of_the_RS_folder_is_referenced_by_a_product_project_or_the_solution()
    {
        var root = Support.RepoRoot();
        foreach (var f in Directory.EnumerateFiles(root, "*.sln").Concat(Directory.EnumerateFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories)))
            Assert.DoesNotContain("I52Ct21dHostFactsRs", File.ReadAllText(f));
    }

    [Fact]
    public void The_RS_projects_are_not_in_the_solution_deploy_or_CI()
    {
        var root = Support.RepoRoot();
        foreach (var f in Directory.EnumerateFiles(root, "*.sln")) Assert.DoesNotContain("HostFactsRs", File.ReadAllText(f));
        var deploy = Path.Combine(root, "deploy");
        if (Directory.Exists(deploy))
            foreach (var f in Directory.EnumerateFiles(deploy, "*", SearchOption.AllDirectories).Where(p => p.EndsWith(".ps1") || p.EndsWith(".xml") || p.EndsWith(".template.xml")))
                Assert.DoesNotContain("I52Ct21dHostFactsRs", File.ReadAllText(f));
        var gh = Path.Combine(root, ".github");
        if (Directory.Exists(gh))
            foreach (var f in Directory.EnumerateFiles(gh, "*", SearchOption.AllDirectories)) Assert.DoesNotContain("I52Ct21dHostFactsRs", File.ReadAllText(f));
    }
}

public class ListFormatTests
{
    public static IEnumerable<object[]> ListFiles() => new[]
    {
        new object[] { "allowed-apis-rs.txt" }, new object[] { "rs-waivers.txt" }, new object[] { "privileged-surface-rs.txt" },
        new object[] { "privileged-callers-rs.txt" }, new object[] { "expected-commands-rs.txt" },
    };

    [Theory]
    [MemberData(nameof(ListFiles))]
    public void A_list_is_UTF8_without_BOM_LF_only_without_tabs_and_ends_with_a_newline(string file)
    {
        var bytes = File.ReadAllBytes(Path.Combine(Support.RsFolder(), file));
        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, "BOM");
        var text = new UTF8Encoding(false, true).GetString(bytes);
        Assert.DoesNotContain("\r", text);
        Assert.DoesNotContain("\t", text);
        Assert.EndsWith("\n", text);
    }

    [Theory]
    [MemberData(nameof(ListFiles))]
    public void A_list_is_ASCII_only(string file) => Assert.All(File.ReadAllBytes(Path.Combine(Support.RsFolder(), file)), b => Assert.True(b < 0x80));

    [Fact]
    public void The_five_lists_parse_with_the_engine_that_the_scan_uses()
    {
        Assert.NotEmpty(Lists.Allowed().Entries);
        Assert.NotEmpty(Lists.Waivers().Entries);
        var p = Lists.Policy();
        Assert.NotEmpty(p.Surface);
        Assert.NotEmpty(p.Callers);
        Assert.NotEmpty(p.Commands);
    }

    [Fact]
    public void Every_allowlist_entry_carries_a_justification_of_a_sentence_not_a_marker() =>
        Assert.All(Lists.Allowed().Entries, e => Assert.True(e.Justification.Length >= 12, "line " + e.Line));

    [Fact]
    public void The_allowlist_has_no_wildcard_member()
    {
        Assert.DoesNotContain(Lists.Allowed().Entries, e => e.Item.EndsWith("::*", StringComparison.Ordinal));
    }

    [Fact]
    public void The_allowlist_has_no_store_opcode_approval_because_the_RS_code_has_no_by_ref_store()
    {
        Assert.DoesNotContain(Lists.Allowed().Entries, e => e.Kind == AllowKind.Opcode);
    }

    [Fact]
    public void The_waiver_file_has_exactly_the_ten_expected_lines() => Assert.Equal(10, Lists.Waivers().Entries.Count);

    [Fact]
    public void The_command_list_has_the_three_design_names_and_the_one_command_class()
    {
        var c = Lists.Policy().Commands;
        Assert.Equal(4, c.Count);
        Assert.Equal(new[] { "CT21DHG_CENSUS_DYN", "CT21DHG_DIMWRITEBACK", "CT21DHG_TOLSCALE" }, c.Where(x => x.Kind == "CommandMethod").Select(x => x.Args).OrderBy(x => x));
        Assert.Single(c, x => x.Kind == "CommandClass");
        Assert.All(c, x => Assert.StartsWith("I52Ct21d.HostFacts.Rs::", x.Owner));
    }

    [Fact]
    public void The_surface_list_pins_exactly_the_four_privileged_types()
    {
        Assert.Equal(RsLayer.PrivilegedTypes.OrderBy(x => x), Lists.Policy().Surface.Keys.OrderBy(x => x));
    }
}

/// <summary>The write members of the host are approved ONLY with a scope and ONLY for the two privileged types (the task's allowed-apis-rs rule).</summary>
public class AllowlistWriteScopeTests
{
    private static readonly IReadOnlyList<AllowEntry> Entries = Lists.Allowed().Entries;

    private static string Sc(AllowEntry e) => e.Scope.Length == 0 ? "" : e.Scope[(e.Scope.IndexOf("::", StringComparison.Ordinal) + 2)..];

    private const string Rs = "I52Ct21d.HostFacts.Rs.";

    private static IEnumerable<AllowEntry> Autodesk() => Entries.Where(e => e.Kind == AllowKind.Member && e.Item.StartsWith("Autodesk.", StringComparison.Ordinal));

    [Fact]
    public void Every_Autodesk_member_is_scoped_except_the_command_attributes() =>
        Assert.All(Autodesk().Where(e => !e.Item.Contains("Runtime.Command", StringComparison.Ordinal)), e => Assert.NotEqual("", e.Scope));

    [Fact]
    public void Every_Autodesk_scope_is_one_of_the_known_types()
    {
        var known = new[] { Rs + "SideDbWriter", Rs + "SideDbReader", Rs + "ScratchSaver", Rs + "SideDbHandle", Rs + "RsGateCommands", Rs + "RsSystemVariables" };
        Assert.All(Autodesk().Where(e => e.Scope.Length > 0), e => Assert.Contains(Sc(e), known));
    }

    [Theory]
    [InlineData("Database::SaveAs", "ScratchSaver")]
    [InlineData("Database::WblockCloneObjects", "SideDbWriter")]
    [InlineData("Database::ReadDwgFile", "SideDbWriter")]
    [InlineData("Database::CloseInput", "SideDbWriter")]
    [InlineData("Database::.ctor", "SideDbWriter")]
    [InlineData("Transaction::Commit", "SideDbWriter")]
    [InlineData("Transaction::AddNewlyCreatedDBObject", "SideDbWriter")]
    [InlineData("SymbolTable::Add", "SideDbWriter")]
    [InlineData("BlockTableRecord::AppendEntity", "SideDbWriter")]
    [InlineData("BlockReference::set_ScaleFactors", "SideDbWriter")]
    [InlineData("SymbolTableRecord::set_Name", "SideDbWriter")]
    [InlineData("Dimension::RecomputeDimensionBlock", "SideDbWriter")]
    [InlineData("BlockReference::.ctor", "SideDbWriter")]
    [InlineData("Line::.ctor", "SideDbWriter")]
    [InlineData("RotatedDimension::.ctor", "SideDbWriter")]
    [InlineData("BlockTableRecord::.ctor", "SideDbWriter")]
    [InlineData("ObjectIdCollection::.ctor", "SideDbWriter")]
    [InlineData("ObjectIdCollection::Add", "SideDbWriter")]
    [InlineData("IdMapping::.ctor", "SideDbWriter")]
    public void A_write_member_has_exactly_one_scope_and_it_is_the_privileged_type(string member, string type)
    {
        var lines = Autodesk().Where(e => e.Item.EndsWith("." + member, StringComparison.Ordinal)).ToList();
        Assert.NotEmpty(lines);
        Assert.All(lines, e => Assert.Equal(Rs + type, Sc(e)));
    }

    [Theory]
    [InlineData("Dimscale")]
    [InlineData("Dimtxt")]
    [InlineData("Dimasz")]
    [InlineData("Dimexe")]
    [InlineData("Dimexo")]
    [InlineData("Dimgap")]
    [InlineData("Dimtad")]
    [InlineData("Dimdec")]
    public void The_dimension_variable_setters_are_approved_only_in_SideDbWriter(string name)
    {
        var setters = Autodesk().Where(e => e.Item.EndsWith("::set_" + name, StringComparison.Ordinal)).ToList();
        Assert.Single(setters);
        Assert.Equal(Rs + "SideDbWriter", Sc(setters[0]));
    }

    [Fact]
    public void No_setter_Add_AppendEntity_Commit_or_constructor_is_approved_for_the_reader_or_the_adapters()
    {
        var nonWriters = new[] { Rs + "SideDbReader", Rs + "RsGateCommands", Rs + "RsSystemVariables", Rs + "SideDbHandle", Rs + "ScratchSaver" };
        var checkedEntries = 0;
        foreach (var e in Autodesk().Where(x => nonWriters.Contains(Sc(x)) && !x.Item.EndsWith("::SaveAs", StringComparison.Ordinal)))
        {
            checkedEntries++;
            var member = e.Item[(e.Item.IndexOf("::", StringComparison.Ordinal) + 2)..];
            var isWrite = member.StartsWith("set_", StringComparison.Ordinal) || member is "Add" or "AppendEntity" or "Commit" or ".ctor" or "AddNewlyCreatedDBObject" or "Erase";
            Assert.False(isWrite, e.Item + " in " + e.Scope);
        }
        Assert.True(checkedEntries > 30, "entries checked: " + checkedEntries);
    }

    [Fact]
    public void The_reader_has_only_getters_enumerators_and_the_read_plumbing()
    {
        var reader = Autodesk().Where(e => Sc(e) == Rs + "SideDbReader").ToList();
        Assert.True(reader.Count > 30, "reader entries: " + reader.Count);
        foreach (var e in reader)
        {
            var member = e.Item[(e.Item.IndexOf("::", StringComparison.Ordinal) + 2)..];
            Assert.True(member.StartsWith("get_", StringComparison.Ordinal) || member is "GetEnumerator" or "MoveNext" or "GetObject" or "StartOpenCloseTransaction"
                or "GetXDataForApplication" or "GetAllowedValues" or "AsArray" or "ModelSpace", e.Item);
        }
    }

    [Fact]
    public void No_System_IO_write_member_is_approved_anywhere()
    {
        var forbidden = new[]
        {
            "File::Copy", "File::Move", "File::Delete", "File::Create", "File::OpenWrite", "File::Open", "File::WriteAll", "File::AppendAll", "File::Replace", "File::SetAttributes",
            "File::Encrypt", "File::Decrypt", "File::CreateText", "File::AppendText", "Directory::Create", "Directory::Delete", "Directory::Move", "FileStream::", "StreamWriter::",
            "BinaryWriter::", "FileInfo::CopyTo", "FileInfo::MoveTo", "FileInfo::Delete", "FileInfo::Create", "DirectoryInfo::Create", "DirectoryInfo::Delete", "FileSystemInfo::Delete",
            "Path::GetTempFileName", "Path::GetRandomFileName", "FileSystemWatcher",
        };
        foreach (var e in Entries.Where(x => x.Kind == AllowKind.Member && x.Item.StartsWith("System.IO.", StringComparison.Ordinal)))
            foreach (var f in forbidden)
                Assert.DoesNotContain("System.IO." + f, e.Item);
    }

    [Fact]
    public void No_process_registry_network_assembly_load_or_reflection_invoke_member_is_approved()
    {
        var forbiddenPrefixes = new[]
        {
            "System.Diagnostics.Process::Start", "System.Diagnostics.Process::Kill", "System.Diagnostics.Process::GetProcess", "System.Diagnostics.ProcessStartInfo",
            "Microsoft.Win32", "System.Net", "System.Reflection.Assembly::Load", "System.Reflection.Assembly::UnsafeLoad", "System.Activator", "System.Reflection.MethodInfo::Invoke",
            "System.Reflection.MethodBase::Invoke", "System.Environment::Set", "System.Environment::Exit", "System.Runtime.InteropServices", "System.Threading.Thread",
            "System.Threading.Tasks", "System.AppDomain", "System.Runtime.Loader",
        };
        foreach (var e in Entries.Where(x => x.Kind == AllowKind.Member))
            foreach (var f in forbiddenPrefixes)
                Assert.False(e.Item.StartsWith(f, StringComparison.Ordinal), e.Item);
    }

    [Fact]
    public void The_evidence_writer_members_approved_are_the_text_writer_the_root_and_the_ledger_only()
    {
        var members = Entries.Where(e => e.Item.StartsWith("I52Ct21d.HostFacts.Core.EvidenceWriter::", StringComparison.Ordinal))
            .Select(e => e.Item[(e.Item.IndexOf("::", StringComparison.Ordinal) + 2)..]).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { ".ctor", "ValidateName", "WriteNewText", "get_Ledger", "get_Root" }, members);
    }

    [Fact]
    public void The_evidence_writer_constructor_is_approved_only_for_the_command_class()
    {
        var ctor = Entries.Single(e => e.Item == "I52Ct21d.HostFacts.Core.EvidenceWriter::.ctor");
        Assert.Equal("I52Ct21d.HostFacts.Rs::I52Ct21d.HostFacts.Rs.RsGateCommands", ctor.Scope);
    }

    [Fact]
    public void SaveAs_is_approved_nowhere_else_and_the_save_is_in_the_one_saver_type()
    {
        var saves = Entries.Where(e => e.Item.Contains("::SaveAs", StringComparison.Ordinal) || e.Item.Contains("::Save", StringComparison.Ordinal) && e.Item.StartsWith("Autodesk.", StringComparison.Ordinal)).ToList();
        Assert.Single(saves);
        Assert.Equal("I52Ct21d.HostFacts.Rs::I52Ct21d.HostFacts.Rs.ScratchSaver", saves[0].Scope);
    }

    [Fact]
    public void The_database_object_is_never_reachable_through_the_document_or_the_working_database()
    {
        Assert.DoesNotContain(Entries, e => e.Item.Contains("HostApplicationServices", StringComparison.Ordinal));
        Assert.DoesNotContain(Entries, e => e.Item.Contains("Document::get_Database", StringComparison.Ordinal));
        Assert.DoesNotContain(Entries, e => e.Item.Contains("Document::get_TransactionManager", StringComparison.Ordinal));
        Assert.DoesNotContain(Entries, e => e.Item.Contains("DocumentCollection::Open", StringComparison.Ordinal) || e.Item.Contains("DocumentCollection::Add", StringComparison.Ordinal));
        Assert.DoesNotContain(Entries, e => e.Item.Contains("SetSystemVariable", StringComparison.Ordinal));
        Assert.DoesNotContain(Entries, e => e.Item.Contains("Overrule", StringComparison.Ordinal));
        Assert.DoesNotContain(Entries, e => e.Item.Contains("LockDocument", StringComparison.Ordinal));
    }

    [Fact]
    public void The_only_document_members_approved_are_the_editor_status_line_of_the_command_class()
    {
        var doc = Entries.Where(e => e.Kind == AllowKind.Member && (e.Item.StartsWith("Autodesk.AutoCAD.ApplicationServices.", StringComparison.Ordinal) || e.Item.StartsWith("Autodesk.AutoCAD.EditorInput.", StringComparison.Ordinal))).ToList();
        Assert.All(doc, e => Assert.True(Sc(e) is Rs + "RsGateCommands" or Rs + "RsSystemVariables", e.Item));
        Assert.Equal(new[]
        {
            "Autodesk.AutoCAD.ApplicationServices.Core.Application::GetSystemVariable",
            "Autodesk.AutoCAD.ApplicationServices.Core.Application::get_DocumentManager",
            "Autodesk.AutoCAD.ApplicationServices.Document::get_Editor",
            "Autodesk.AutoCAD.ApplicationServices.DocumentCollection::get_MdiActiveDocument",
            "Autodesk.AutoCAD.EditorInput.Editor::WriteMessage",
        }, doc.Select(e => e.Item).OrderBy(x => x, StringComparer.Ordinal));
    }
}

public class LedgerFileTests
{
    [Fact]
    public void The_checked_in_hashes_ledger_matches_every_file_of_the_folder()
    {
        // HASHES-RS.txt is the review record: any byte changed after it was written is a difference. Regenerate it ONLY after a new independent review.
        var ledger = Path.Combine(Support.RsFolder(), "HASHES-RS.txt");
        Assert.True(File.Exists(ledger), "HASHES-RS.txt is missing");
        Assert.Empty(HashesLedger.Verify(File.ReadAllText(ledger), Support.RsFolder()));
    }

    [Fact]
    public void The_ledger_lists_every_source_file_except_itself_and_the_build_output()
    {
        var root = Support.RsFolder();
        var sep = Path.DirectorySeparatorChar;
        var listed = File.ReadAllLines(Path.Combine(root, "HASHES-RS.txt")).Where(l => l.Length > 66 && l[0] != '#').Select(l => l[66..]).ToHashSet(StringComparer.Ordinal);
        foreach (var f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(root, f).Replace(sep, '/');
            if (rel == "HASHES-RS.txt" || rel.Contains("/bin/", StringComparison.Ordinal) || rel.Contains("/obj/", StringComparison.Ordinal)
                || rel.StartsWith("bin/", StringComparison.Ordinal) || rel.StartsWith("obj/", StringComparison.Ordinal)) continue;
            Assert.Contains(rel, listed);
        }
    }
}

public class WaiverFileTests
{
    private const string W = "I52Ct21d.HostFacts.Rs.SideDbWriter";

    [Theory]
    [InlineData("R-COMMAND")]
    [InlineData("R-PRODUCT-DATABASE")]
    [InlineData("R-SYSVAR-SET")]
    [InlineData("R-FILE-WRITE")]
    [InlineData("R-PROCESS")]
    [InlineData("R-OVERRULE")]
    [InlineData("R-ASSEMBLY-LOAD")]
    [InlineData("R-IMPORT-MEMBER")]
    [InlineData("R-RS-DB-CTOR")]
    [InlineData("R-RS-SAVE-SCOPE")]
    [InlineData("R-OPENMODE-UNRESOLVABLE")]
    [InlineData("R-RAW-STORE")]
    public void A_rule_outside_the_waivable_table_can_never_be_waived(string rule)
    {
        var ex = Assert.Throws<FormatException>(() => RsWaivers.Parse(rule + "|" + W + "|x|a justification long enough\n"));
        Assert.Contains("can never be waived", ex.Message);
    }

    [Fact]
    public void A_waiver_for_another_type_is_a_format_error()
    {
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-COMMIT|I52Ct21d.HostFacts.Rs.SideDbReader|Transaction::Commit|a justification long enough\n"));
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-COMMIT|I52Ct21d.HostFacts.Rs.TolScaleHostAdapter|Transaction::Commit|a justification long enough\n"));
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-DB-SAVE|I52Ct21d.HostFacts.Rs.SideDbReader|Database::WblockCloneObjects|a justification long enough\n"));
    }

    [Fact]
    public void A_waiver_with_a_wider_fragment_is_a_format_error()
    {
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-DB-SAVE|" + W + "|Database::|a justification long enough\n"));
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-DB-SAVE|" + W + "||a justification long enough\n"));
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-DB-SAVE|" + W + "|Database::SaveAs|a justification long enough\n"));
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-WRITE-OPEN|" + W + "|OpenMode constant 2|a justification long enough\n"));
    }

    [Fact]
    public void The_save_waiver_belongs_to_the_saver_only()
    {
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-DB-SAVE|" + W + "|Database::SaveAs|a justification long enough\n"));
        RsWaivers.Parse("R-DB-SAVE|I52Ct21d.HostFacts.Rs.ScratchSaver|Database::SaveAs|a justification long enough\n");
    }

    [Fact]
    public void A_short_justification_a_wrong_field_count_a_duplicate_and_CR_are_format_errors()
    {
        var line = "R-COMMIT|" + W + "|Transaction::Commit|a justification long enough";
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-COMMIT|" + W + "|Transaction::Commit|short\n"));
        Assert.Throws<FormatException>(() => RsWaivers.Parse("R-COMMIT|" + W + "|Transaction::Commit\n"));
        Assert.Throws<FormatException>(() => RsWaivers.Parse(line + "\n" + line + "\n"));
        Assert.Throws<FormatException>(() => RsWaivers.Parse(line + "\r\n"));
    }

    [Fact]
    public void Comments_and_blank_lines_are_ignored_and_the_hash_covers_the_bytes()
    {
        var text = "# a comment\n\nR-COMMIT|" + W + "|Transaction::Commit|a justification long enough\n";
        var w = RsWaivers.Parse(text);
        Assert.Single(w.Entries);
        Assert.Equal(Support.Sha(Encoding.UTF8.GetBytes(text)), w.Sha256);
    }

    [Fact]
    public void A_waiver_is_found_only_for_the_RS_assembly_the_type_and_the_fragment()
    {
        var w = RsWaivers.Parse("R-COMMIT|" + W + "|Transaction::Commit|a justification long enough\n");
        var v = new Violation("x", W + "/Nested", "M", 1, "R-COMMIT", "Autodesk.AutoCAD.DatabaseServices.Transaction::Commit");
        Assert.NotNull(w.Find("I52Ct21d.HostFacts.Rs", v));
        Assert.Null(w.Find("Some.Other.Assembly", v));
        Assert.Null(w.Find("I52Ct21d.HostFacts.Rs", new Violation("x", "I52Ct21d.HostFacts.Rs.SideDbReader", "M", 1, "R-COMMIT", "Autodesk.AutoCAD.DatabaseServices.Transaction::Commit")));
        Assert.Null(w.Find("I52Ct21d.HostFacts.Rs", new Violation("x", W, "M", 1, "R-COMMIT", "Autodesk.AutoCAD.DatabaseServices.Other::Commit")));
        Assert.Null(w.Find("I52Ct21d.HostFacts.Rs", new Violation("x", W, "M", 1, "R-COMMITX", "Autodesk.AutoCAD.DatabaseServices.Transaction::Commit")));
    }

    [Fact]
    public void Unused_reports_the_waivers_nothing_matched()
    {
        var w = RsWaivers.Parse("R-COMMIT|" + W + "|Transaction::Commit|a justification long enough\n");
        Assert.Single(w.Unused());
        w.Find("I52Ct21d.HostFacts.Rs", new Violation("x", W, "M", 1, "R-COMMIT", "Autodesk.AutoCAD.DatabaseServices.Transaction::Commit"));
        Assert.Empty(w.Unused());
    }
}

public class SchemaFileTests
{
    public static IEnumerable<object[]> Schemas() => Directory.EnumerateFiles(Path.Combine(Support.RsFolder(), "schemas"), "*.json").OrderBy(p => p).Select(p => new object[] { Path.GetFileName(p) });

    [Theory]
    [MemberData(nameof(Schemas))]
    public void Every_schema_file_loads_in_the_closed_validator_and_is_closed_at_the_top(string file)
    {
        var text = File.ReadAllText(Path.Combine(Support.RsFolder(), "schemas", file));
        Assert.DoesNotContain("\r", text);
        var node = (JsonObject)Jcs.ParseStrict(text);
        Assert.Equal(Path.GetFileNameWithoutExtension(file), node["$id"]!.GetValue<string>());
        Assert.False(node["additionalProperties"]!.GetValue<bool>());
        Assert.NotNull(RsSchemas.Load(file));
    }

    [Fact]
    public void There_are_six_RS_schemas()
    {
        Assert.Equal(new[]
        {
            "ct21d.designation.rs.v1.json", "ct21d.dyncensus.v1.json", "ct21d.rs-run.v1.json", "ct21d.tolscale-probe-table.v1.json", "ct21d.tolscale.v1.json", "ct21d.writeback.v1.json",
        }, Schemas().Select(s => (string)s[0]));
    }

    [Fact]
    public void The_designation_schema_extends_the_R0_one_with_the_scratch_root_and_the_declarations()
    {
        var rs = (JsonObject)Jcs.ParseStrict(File.ReadAllText(Path.Combine(Support.RsFolder(), "schemas", "ct21d.designation.rs.v1.json")));
        var r0 = (JsonObject)Jcs.ParseStrict(File.ReadAllText(Path.Combine(Support.R0Folder(), "schemas", "ct21d.designation.v1.json")));
        var rsProps = ((JsonObject)rs["properties"]!).Select(p => p.Key).ToHashSet();
        var r0Props = ((JsonObject)r0["properties"]!).Select(p => p.Key).ToHashSet();
        foreach (var added in new[] { "scratchRoot", "evidenceRoot", "governedDocumentPath", "productPaths", "hostChecks" }) Assert.Contains(added, rsProps);
        foreach (var kept in new[] { "runId", "attempt", "sessionId", "privateCopyPath", "privateCopySha256", "libraryPath", "libraryFileSha256", "declaredSet", "tupleBinding" }) Assert.Contains(kept, rsProps);
        Assert.Contains("evidenceFolder", r0Props);
        Assert.DoesNotContain("evidenceFolder", rsProps); // renamed: evidenceRoot
    }

    [Fact]
    public void The_run_record_schema_refuses_a_governing_record_and_any_command_but_the_three()
    {
        using var rig = new Rig();
        TolScaleRunner.Run(rig.D, Rig.Match, new FakeEnv(), FakeVars.Default(), (g, l) => new FakeTolHost(g, l), rig.Writer());
        var good = (JsonObject)rig.ReadEvidence("rs-run-tolscale.json");
        var schema = RsSchemas.Load("ct21d.rs-run.v1.json");
        Assert.Empty(schema.Validate(good));
        JsonObject Mutate(Action<JsonObject> m) { var c = (JsonObject)good.DeepClone(); m(c); return c; }
        Assert.NotEmpty(schema.Validate(Mutate(o => o["governing"] = true)));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["command"] = "CT21DHG_CENSUS_R")));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["classification"] = "READ_ONLY_PRODUCT_STATE")));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["noAutomaticRetry"] = false)));
        Assert.NotEmpty(schema.Validate(Mutate(o => o.Remove("scratch"))));
        Assert.NotEmpty(schema.Validate(Mutate(o => o["extra"] = 1)));
        Assert.NotEmpty(schema.Validate(Mutate(o => ((JsonObject)((JsonObject)o["declarations"]!))["source"] = "INSTRUMENT")));
        Assert.NotEmpty(schema.Validate(Mutate(o => ((JsonArray)((JsonObject)o["scratch"]!)["files"]!)[0]!["name"] = "..\\x.dwg")));
        Assert.NotEmpty(schema.Validate(Mutate(o => ((JsonArray)o["sideDatabases"]!)[0]!["constructor"] = "new Database()")));
    }
}
