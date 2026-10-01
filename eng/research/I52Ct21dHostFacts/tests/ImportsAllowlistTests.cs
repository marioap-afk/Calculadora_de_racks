using System.Text;
using I52Ct21d.HostFacts.Tools;
using I52Ct21d.HostFacts.Tools.Scan;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>The deny-by-default imports layer (design 5.3 item 2): the list parser, the checks, the CLI and the content of the real list.</summary>
[Collection("console")]
public class ImportsAllowlistTests
{
    private const string A = "AcDbMgd";
    private const string Ent = "Autodesk.AutoCAD.DatabaseServices.Entity";

    private static byte[] Fixture(Action<FixtureBuilder.Il> body, string ns = "Fx", string name = "User")
    {
        var fb = new FixtureBuilder("Fixture");
        fb.Type(ns, name).Method("M", body);
        return fb.Build();
    }

    private static string[] RulesOf(byte[] image, string list) =>
        ForbiddenApiScan.Scan(image, "f.dll", ScanConfig.For(ScanRole.R0, ApiAllowlist.Parse(list))).Select(v => v.Rule).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();

    private const string Why = "approved for this test only";

    // ---- the parser --------------------------------------------------------------------------------------------------------
    [Fact]
    public void The_parser_accepts_the_format_and_refuses_each_malformed_line()
    {
        var ok = ApiAllowlist.Parse("# c\n\nA|AcDbMgd||" + Why + "\nT|Autodesk.AutoCAD.DatabaseServices.Entity||" + Why + "\nM|Autodesk.AutoCAD.DatabaseServices.Entity::get_Layer|Fixture::Fx.User|" + Why + "\n");
        Assert.Equal(3, ok.Entries.Count);
        Assert.Equal(64, ok.Sha256.Length);
        Assert.Equal("Fixture::Fx.User", ok.Entries[2].Scope);
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("M|Autodesk.AutoCAD.DatabaseServices.Entity::get_Layer|Fx.User|" + Why + "\n"));   // a scope without its assembly
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("M|Autodesk.AutoCAD.DatabaseServices.Entity::get_Layer|::Fx.User|" + Why + "\n"));
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("M|Autodesk.AutoCAD.DatabaseServices.Entity::get_Layer|Fixture::|" + Why + "\n"));
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("O|stind.i4||" + Why + "\n"));                                                      // an opcode approval must be scoped
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("O|localloc|Fixture::Fx.User|" + Why + "\n"));                                      // raw-memory opcodes can never be approved
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("O|cpblk|Fixture::Fx.User|" + Why + "\n"));
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("O|initblk|Fixture::Fx.User|" + Why + "\n"));
        Assert.Equal(AllowKind.Opcode, ApiAllowlist.Parse("O|stind.i4|Fixture::Fx.User|" + Why + "\n").Entries.Single().Kind);

        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("A|X||short\n"));                       // justification too short
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("A|X|\n"));                              // wrong column count
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("Q|X||" + Why + "\n"));                  // unknown kind
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("T|X|Scope|" + Why + "\n"));             // scope only on members
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("M|NoSeparator||" + Why + "\n"));        // not Type::Member
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("A|X||" + Why + "\r\n"));                // CRLF
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("A|X||" + Why + "\nA|X||" + Why + "\n")); // duplicate
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("A||" + "|" + Why + "\n"));              // empty item
    }

    [Theory]
    [InlineData("Autodesk.AutoCAD.DatabaseServices.Database")]
    [InlineData("System.IO.File")]
    [InlineData("System.IO.Path")]
    [InlineData("System.Reflection.Assembly")]
    [InlineData("System.Diagnostics.Process")]
    [InlineData("System.Runtime.InteropServices.Marshal")]
    [InlineData("Microsoft.Win32.Registry")]
    [InlineData("System.Net.Http.HttpClient")]
    [InlineData("System.Threading.Thread")]
    [InlineData("System.Environment")]
    [InlineData("System.Globalization.CultureInfo")]
    [InlineData("System.Console")]
    [InlineData("System.Xml.Linq.XDocument")]
    public void A_wildcard_is_refused_for_every_type_that_can_touch_the_host(string type)
    {
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("M|" + type + "::*||" + Why + "\n"));
    }

    [Fact]
    public void A_wildcard_is_accepted_for_a_pure_type()
    {
        var l = ApiAllowlist.Parse("M|System.String::*||" + Why + "\n");
        Assert.Single(l.FindMember("System.String", "Concat"));
        Assert.Empty(l.FindMember("System.Text.StringBuilder", "Append"));
    }

    // ---- the checks ----------------------------------------------------------------------------------------------------------
    private static string Full(string member = "get_Layer") =>
        "A|AcDbMgd||" + Why + "\nA|System.Runtime||" + Why + "\nT|" + Ent + "||" + Why + "\nT|System.Object||" + Why + "\nM|" + Ent + "::" + member + "||" + Why + "\n";

    [Fact]
    public void A_fully_approved_fixture_is_clean_and_each_missing_approval_is_reported_under_its_own_rule()
    {
        var image = Fixture(il => il.CallVirt(A, Ent, "get_Layer", "string"));
        Assert.Empty(RulesOf(image, Full()));
        Assert.Equal(new[] { "R-IMPORT-ASSEMBLY" }, RulesOf(image, Full().Replace("A|AcDbMgd||" + Why + "\n", "")));
        Assert.Equal(new[] { "R-IMPORT-TYPE" }, RulesOf(image, Full().Replace("T|" + Ent + "||" + Why + "\n", "")));
        Assert.Equal(new[] { "R-IMPORT-MEMBER" }, RulesOf(image, Full("get_Linetype")));
    }

    [Fact]
    public void A_member_approved_for_one_type_is_refused_in_any_other_type()
    {
        var image = Fixture(il => il.CallVirt(A, Ent, "get_Layer", "string"), "Fx", "Other");
        var scoped = Full().Replace("::get_Layer||", "::get_Layer|Fixture::Fx.User|");
        Assert.Equal(new[] { "R-IMPORT-SCOPE" }, RulesOf(image, scoped));
        Assert.Empty(RulesOf(Fixture(il => il.CallVirt(A, Ent, "get_Layer", "string"), "Fx", "User"), scoped));
        // the same type name defined in ANOTHER assembly is not the approved type: scoped approvals match (assembly, type), not the type name alone
        var spoof = new FixtureBuilder("Spoofed.Assembly");
        spoof.Type("Fx", "User").Method("M", il => il.CallVirt(A, Ent, "get_Layer", "string"));
        Assert.Equal(new[] { "R-IMPORT-SCOPE" }, RulesOf(spoof.Build(), scoped));
    }

    [Fact]
    public void The_type_wildcard_approves_members_of_a_pure_type_only()
    {
        var image = Fixture(il => il.Call("System.Runtime", "System.String", "Concat", "string", "string", "string"));
        var list = "A|System.Runtime||" + Why + "\nT|System.String||" + Why + "\nT|System.Object||" + Why + "\nM|System.String::*||" + Why + "\n";
        Assert.Empty(RulesOf(image, list));
    }

    [Fact]
    public void Array_accessors_are_intrinsic_and_not_an_import()
    {
        var fb = new FixtureBuilder("Fixture");
        fb.Type("Fx", "User").Method("M", il => il.Ret());
        Assert.Empty(RulesOf(fb.Build(), "A|System.Runtime||" + Why + "\nT|System.Object||" + Why + "\n"));
    }

    [Fact]
    public void Unused_approvals_are_reported_by_the_list()
    {
        var image = Fixture(il => il.CallVirt(A, Ent, "get_Layer", "string"));
        var list = ApiAllowlist.Parse(Full() + "M|" + Ent + "::get_Linetype||" + Why + "\n");
        ForbiddenApiScan.Scan(image, "f.dll", ScanConfig.For(ScanRole.R0, list));
        Assert.Equal(new[] { Ent + "::get_Linetype" }, list.Unused().Select(e => e.Item).ToArray());
    }

    // ---- the CLI ---------------------------------------------------------------------------------------------------------------
    private static int Run(params string[] args)
    {
        var o = Console.Out;
        var e = Console.Error;
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
        try { return Program.Main(args); }
        finally { Console.SetOut(o); Console.SetError(e); }
    }

    /// <summary>The CLI scan with the given allowlist and the three real privileged lists (all four options are mandatory).</summary>
    private static int RunScanWith(string allowlist, params string[] specs)
    {
        var d = Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts");
        return Run(new[] { "scan", "--allowed", allowlist, "--privileged-surface", Path.Combine(d, "privileged-surface.txt"),
            "--privileged-callers", Path.Combine(d, "privileged-callers.txt"), "--expected-commands", Path.Combine(d, "expected-commands.txt") }.Concat(specs).ToArray());
    }

    [Fact]
    public void Imports_allowlist_check_exits_0_when_every_import_is_approved_and_used_and_2_otherwise_and_scan_needs_the_list()
    {
        using var t = new TempDir();
        var dll = Path.Combine(t.Path, "r0.dll");
        File.WriteAllBytes(dll, Fixture(il => il.CallVirt(A, Ent, "get_Layer", "string")));
        var good = Path.Combine(t.Path, "good.txt");
        File.WriteAllText(good, Full(), new UTF8Encoding(false));
        Assert.Equal(0, Run("imports-allowlist-check", good, "r0:" + dll));
        Assert.Equal(0, RunScanWith(good, "r0:" + dll));

        var missing = Path.Combine(t.Path, "missing.txt");
        File.WriteAllText(missing, Full("get_Linetype"), new UTF8Encoding(false));
        Assert.Equal(2, Run("imports-allowlist-check", missing, "r0:" + dll));   // the import is not approved (and the approval is unused)
        Assert.Equal(2, RunScanWith(missing, "r0:" + dll));

        var extra = Path.Combine(t.Path, "extra.txt");
        File.WriteAllText(extra, Full() + "M|" + Ent + "::get_Linetype||" + Why + "\n", new UTF8Encoding(false));
        Assert.Equal(2, Run("imports-allowlist-check", extra, "r0:" + dll));     // everything approved, but one approval is not used
        Assert.Equal(0, RunScanWith(extra, "r0:" + dll));           // scan alone does not judge the list's minimality

        var bad = Path.Combine(t.Path, "bad.txt");
        File.WriteAllText(bad, "A|X||x\n", new UTF8Encoding(false));
        Assert.Equal(1, RunScanWith(bad, "r0:" + dll));             // a malformed list is a usage error, never a silent pass
        Assert.Equal(1, RunScanWith(Path.Combine(t.Path, "absent.txt"), "r0:" + dll));
        Assert.Equal(1, Run("scan", "r0:" + dll));                               // the list is mandatory
        Assert.Equal(1, Run("imports-allowlist-check", good));
    }

    [Fact]
    public void Imports_list_drafts_lines_with_the_scope_of_single_type_use_and_approves_nothing()
    {
        using var t = new TempDir();
        var dll = Path.Combine(t.Path, "r0.dll");
        File.WriteAllBytes(dll, Fixture(il => il.CallVirt(A, Ent, "get_Layer", "string")));
        var o = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        try { Assert.Equal(0, Program.Main(new[] { "imports-list", "r0:" + dll })); }
        finally { Console.SetOut(o); }
        var text = sw.ToString();
        Assert.Contains("M|" + Ent + "::get_Layer|Fixture::Fx.User|REVIEW", text);
        Assert.Contains("A|AcDbMgd||REVIEW", text);
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse(text.Replace("\r", ""))); // a drafted list is not loadable until a reviewer writes justifications
    }

    // ---- the real list ----------------------------------------------------------------------------------------------------------
    [Fact]
    public void The_real_list_parses_has_LF_endings_and_a_justification_on_every_entry()
    {
        var path = ForbiddenApiScanTests.AllowlistPath();
        var bytes = File.ReadAllBytes(path);
        Assert.DoesNotContain((byte)'\r', bytes);
        var list = ApiAllowlist.Parse(bytes);
        Assert.True(list.Entries.Count > 500);
        Assert.All(list.Entries, e => Assert.True(e.Justification.Length >= 20, "line " + e.Line));
    }

    // Members that write, save, load, start, change global state or open a document. Nothing of this may be approved; the few exact
    // exceptions are the reviewed ones, each scoped to the single type that holds the privilege.
    private static readonly HashSet<string> ForbiddenMemberNames = new(StringComparer.Ordinal)
    {
        "Save", "SaveAs", "Wblock", "DxfOut", "DxfIn", "Insert", "Commit", "UpgradeOpen", "SetSystemVariable", "SendStringToExecute", "LockDocument",
        "Open", "Start", "Kill", "Close", "Invoke", "DynamicInvoke", "CreateDelegate", "CreateInstance", "InvokeMember", "Delete", "Move", "Copy",
        "Replace", "Create", "Append", "AppendEntity", "AddOverrule", "Command", "GetTempFileName", "Compile", "Regen", "Quit", "Exit", "CloseMainWindow",
    };

    private static readonly string[] ForbiddenMemberPrefixes = { "set_", "Set", "Write", "Load", "Delete", "Dxf", "Add", "Erase", "Remove" };

    private static readonly HashSet<string> ReviewedExceptions = new(StringComparer.Ordinal)
    {
        // the one write stream of the EvidenceWriter
        "System.IO.Stream::Write", "System.IO.File::SetAttributes", "System.IO.FileStream::.ctor",
        // command-line status line and the standard writers of the offline tools
        "Autodesk.AutoCAD.EditorInput.Editor::WriteMessage", "System.IO.TextWriter::Write", "System.IO.TextWriter::WriteLine",
        // newobj of the in-memory side database, and Database.ReadDwgFile of the private copy (scoped, constant arguments checked by the scan)
        "Autodesk.AutoCAD.DatabaseServices.Database::.ctor",
        // attribute constructors (the host registers the commands)
        "Autodesk.AutoCAD.Runtime.CommandClassAttribute::.ctor", "Autodesk.AutoCAD.Runtime.CommandMethodAttribute::.ctor",
        // in-memory option structs and JSON nodes of the core (not process-wide state)
        "System.Text.Json.JsonDocumentOptions::set_AllowTrailingCommas", "System.Text.Json.JsonDocumentOptions::set_CommentHandling",
        "System.Text.Json.JsonReaderOptions::set_CommentHandling", "System.Text.Json.Nodes.JsonNode::set_Item",
        // in-memory collections
        "System.Collections.Generic.Dictionary`2::set_Item", "System.Collections.Generic.SortedDictionary`2::set_Item",
        "System.Collections.Generic.Dictionary`2::Remove", "System.Text.Json.Nodes.JsonObject::Remove",
        "System.IO.Path::Combine", // pure string combination (no I/O)
        // the single privileged writer of the evidence folder (create-new, flat names), called by the runners and the label helper
        "I52Ct21d.HostFacts.Core.EvidenceWriter::WriteNewText",
        // delegate invocation of the command wrappers and of the offline scan (internal lambdas)
        "System.Action`2::Invoke", "System.Func`5::Invoke",
        // in-memory builders
        "System.Collections.Immutable.ImmutableArray::Create", "System.Text.Json.Nodes.JsonValue::Create", "System.Text.StringBuilder::Append",
        "System.Collections.Generic.List`1::Add", "System.Collections.Generic.HashSet`1::Add", "System.Collections.Generic.SortedSet`1::Add",
        "System.Collections.Generic.List`1::AddRange", "System.Text.Json.Nodes.JsonArray::Add",
        "System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted",
        "System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral",
    };

    [Fact]
    public void The_real_list_approves_nothing_that_writes_saves_loads_starts_or_changes_global_state_except_the_reviewed_exceptions()
    {
        var list = ApiAllowlist.LoadFile(ForbiddenApiScanTests.AllowlistPath());
        var offenders = list.Entries
            .Where(e => e.Kind == AllowKind.Member)
            .Where(e => { var m = e.Item[(e.Item.IndexOf("::", StringComparison.Ordinal) + 2)..]; return ForbiddenMemberNames.Contains(m) || ForbiddenMemberPrefixes.Any(p => m.StartsWith(p, StringComparison.Ordinal)); })
            .Where(e => !ReviewedExceptions.Contains(e.Item))
            .Select(e => e.Item)
            .ToList();
        Assert.True(offenders.Count == 0, "unreviewed approvals: " + string.Join(", ", offenders));

        // the privileged members are scoped to the one type that holds the privilege
        string Scope(string item) => list.Entries.Single(e => e.Kind == AllowKind.Member && e.Item == item).Scope;
        Assert.Equal("I52Ct21d.HostFacts.Core::I52Ct21d.HostFacts.Core.EvidenceWriter", Scope("System.IO.FileStream::.ctor"));
        Assert.Equal("I52Ct21d.HostFacts.Core::I52Ct21d.HostFacts.Core.EvidenceWriter", Scope("System.IO.Stream::Write"));
        Assert.Equal("I52Ct21d.HostFacts.Core::I52Ct21d.HostFacts.Core.EvidenceWriter", Scope("System.IO.File::SetAttributes"));
        Assert.Equal("I52Ct21d.HostFacts.R0::I52Ct21d.HostFacts.R0.AcadSideDbReader", Scope("Autodesk.AutoCAD.DatabaseServices.Database::ReadDwgFile"));
        Assert.Equal("I52Ct21d.HostFacts.R0::I52Ct21d.HostFacts.R0.AcadSideDbReader", Scope("Autodesk.AutoCAD.DatabaseServices.Database::.ctor"));
        Assert.Equal("I52Ct21d.HostFacts.R0::I52Ct21d.HostFacts.R0.AcadSideDbReader", Scope("Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartOpenCloseTransaction"));
        Assert.Equal("I52Ct21d.HostFacts.R0::I52Ct21d.HostFacts.R0.AcadSideDbReader", Scope("Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject"));
        // and no type of the dangerous families is approved at all
        var types = list.Entries.Where(e => e.Kind == AllowKind.Type).Select(e => e.Item).ToHashSet(StringComparer.Ordinal);
        foreach (var banned in new[]
        {
            "System.Diagnostics.ProcessStartInfo", "System.Runtime.InteropServices.Marshal", "System.Runtime.Loader.AssemblyLoadContext", "System.Activator",
            "System.Net.Http.HttpClient", "Microsoft.Win32.Registry", "System.IO.Compression.ZipFile", "System.Xml.Linq.XDocument",
            "System.IO.MemoryMappedFiles.MemoryMappedFile", "System.Linq.Expressions.Expression", "System.AppDomain", "System.Delegate",
            "System.Reflection.MethodInfo", "System.Reflection.Emit.DynamicMethod", "Autodesk.AutoCAD.Runtime.DynamicLinker", "System.Runtime.CompilerServices.Unsafe", "System.Runtime.InteropServices.GCHandle", "System.Runtime.InteropServices.NativeMemory", "System.Threading.Thread", "System.Threading.Tasks.Task", "System.Buffer", "Autodesk.AutoCAD.ApplicationServices.DocumentCollectionExtension",
            "Autodesk.AutoCAD.DatabaseServices.Line", "Autodesk.AutoCAD.ApplicationServices.DocumentLock",
        })
            Assert.DoesNotContain(banned, types);
    }
}
