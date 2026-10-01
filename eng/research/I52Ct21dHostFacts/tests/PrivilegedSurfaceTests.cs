using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using I52Ct21d.HostFacts.Tools.Scan;
using Reflection = System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>
/// LAYER 3 of the forbidden-API scan: the privileged surface (see <see cref="ForbiddenApiScan"/> partial "Privileged"). Negative controls:
/// the five reproductions of the audit that the scan used to pass (injection 16, P1a, P1b, P2, a new command), each of which must now FAIL,
/// plus the checks that the real tree stays CLEAN. P1a / P1b are done on the REAL Core DLL (a constant of the IL is patched in memory); the
/// others add code, so they are IL fixtures (a Core clone built with <see cref="FixtureBuilder"/>; never loaded, only read).
/// </summary>
public class PrivilegedSurfaceTests
{
    private const string CoreAsm = "I52Ct21d.HostFacts.Core";
    private const string WriterNs = "I52Ct21d.HostFacts.Core";
    private const string Writer = "I52Ct21d.HostFacts.Core.EvidenceWriter";
    private const string Rt = "System.Runtime";
    private const string FileModeSpec = "vt:System.Runtime|System.IO.FileMode";
    private const string FileAccessSpec = "vt:System.Runtime|System.IO.FileAccess";
    private const string FileShareSpec = "vt:System.Runtime|System.IO.FileShare";
    private const string FileAttrSpec = "vt:System.Runtime|System.IO.FileAttributes";

    private readonly ITestOutputHelper _out;

    public PrivilegedSurfaceTests(ITestOutputHelper output) => _out = output;

    private static string Dir() => Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts");

    private static ScanPolicy RealPolicy() =>
        ScanPolicy.LoadFiles(Path.Combine(Dir(), "privileged-surface.txt"), Path.Combine(Dir(), "privileged-callers.txt"), Path.Combine(Dir(), "expected-commands.txt"));

    private static ApiAllowlist RealList() => ApiAllowlist.LoadFile(ForbiddenApiScanTests.AllowlistPath());

    private static byte[] CoreImage() => File.ReadAllBytes(typeof(Core.EvidenceWriter).Assembly.Location);

    private static byte[] ToolsImage() => File.ReadAllBytes(typeof(Tools.Program).Assembly.Location);

    private static IReadOnlyList<Violation> ScanCore(byte[] image, ScanPolicy policy, bool withList = true) =>
        ForbiddenApiScan.Scan(image, "core.dll", ScanConfig.For(ScanRole.Core, withList ? RealList() : null, policy));

    private static string[] RuleSet(IEnumerable<Violation> v) => v.Select(x => x.Rule).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();

    private void Show(string title, IEnumerable<Violation> violations)
    {
        var list = violations.ToList();
        _out.WriteLine("REPRODUCTION " + title + ": " + (list.Count == 0 ? "PASS (scan did NOT fail)" : "scan FAIL, " + list.Count + " violation(s)"));
        foreach (var v in list) _out.WriteLine("  " + v);
    }

    // ---- the real tree stays CLEAN ----------------------------------------------------------------------------------------------
    [Fact]
    public void The_real_core_and_tools_are_clean_with_the_pinned_lists_and_the_scan_really_checked_the_callers()
    {
        var policy = RealPolicy();
        Assert.Empty(ScanCore(CoreImage(), policy));
        Assert.Empty(ForbiddenApiScan.Scan(ToolsImage(), "tools.dll", ScanConfig.For(ScanRole.Tools, RealList(), policy)));
        // the callers of Core and Tools were matched against the list (not vacuous); the R0 ones need the R0 DLL (see the HAVE_R0 test)
        Assert.Contains(policy.UsedCallers, c => c.Caller.EndsWith("Runners::RunCensus", StringComparison.Ordinal));
        Assert.Contains(policy.UsedCallers, c => c.Caller.EndsWith("EvidenceSealer::SealOrVerify", StringComparison.Ordinal));
        Assert.Contains(policy.UsedCallers, c => c.Caller.EndsWith("Program::Seal", StringComparison.Ordinal));
    }

    [Fact]
    public void The_pinned_surface_is_exactly_what_the_assembly_has()
    {
        var observed = ForbiddenApiScan.ScanDetailed(CoreImage(), "core.dll", ScanConfig.For(ScanRole.Core)).Observed;
        var policy = RealPolicy();
        Assert.Equal(new[] { "I52Ct21d.HostFacts.Core.EvidenceSealer", "I52Ct21d.HostFacts.Core.EvidenceWriter" }, observed.Surface.Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray());
        foreach (var (type, lines) in observed.Surface)
            Assert.Equal(policy.Surface[type].OrderBy(x => x, StringComparer.Ordinal), lines.OrderBy(x => x, StringComparer.Ordinal));
        Assert.Contains(observed.Surface[Writer], l => l.Contains("::WriteNew(", StringComparison.Ordinal));
    }

    [Fact]
    public void Editing_the_pinned_surface_file_or_the_assembly_is_reported_with_the_explicit_review_message()
    {
        var dir = Dir();
        var surface = File.ReadAllLines(Path.Combine(dir, "privileged-surface.txt")).ToList();
        var line = surface.Single(l => l.Contains("::MarkReadOnly(", StringComparison.Ordinal));
        // (a) the file lacks a method the assembly has: "added"
        var withoutLine = ScanPolicy.Parse(string.Join("\n", surface.Where(l => l != line)) + "\n", File.ReadAllText(Path.Combine(dir, "privileged-callers.txt")), File.ReadAllText(Path.Combine(dir, "expected-commands.txt")));
        var added = ScanCore(CoreImage(), withoutLine).Where(v => v.Rule == "R-PRIVILEGED-SURFACE").ToList();
        Assert.Contains(added, v => v.Detail.Contains("added or changed", StringComparison.Ordinal) && v.Detail.Contains("MarkReadOnly", StringComparison.Ordinal));
        Assert.All(added, v => Assert.Contains("explicit independent review", v.Detail, StringComparison.Ordinal));
        // (b) the file carries a method the assembly lacks: "removed"
        var withExtra = ScanPolicy.Parse(string.Join("\n", surface) + "\n" + Writer + "::Clobber(System.String,System.Byte[])->System.Void [public instance]\n",
            File.ReadAllText(Path.Combine(dir, "privileged-callers.txt")), File.ReadAllText(Path.Combine(dir, "expected-commands.txt")));
        Assert.Contains(ScanCore(CoreImage(), withExtra), v => v.Rule == "R-PRIVILEGED-SURFACE" && v.Detail.Contains("removed or changed", StringComparison.Ordinal));
    }

    [Fact]
    public void A_call_inside_Core_is_checked_against_the_callers_list_dropping_a_real_caller_makes_the_scan_fail()
    {
        var dir = Dir();
        var callers = File.ReadAllLines(Path.Combine(dir, "privileged-callers.txt")).ToList();
        var drop = callers.Single(l => l.StartsWith(CoreAsm + "::" + CoreAsm + ".Runners::RunCensus|" + Writer + "::WriteNewText|", StringComparison.Ordinal));
        var policy = ScanPolicy.Parse(File.ReadAllText(Path.Combine(dir, "privileged-surface.txt")), string.Join("\n", callers.Where(l => l != drop)) + "\n", File.ReadAllText(Path.Combine(dir, "expected-commands.txt")));
        var v = ScanCore(CoreImage(), policy).Single(x => x.Rule == "R-PRIVILEGED-CALLER");
        Assert.Equal("Runners", v.Type.Split('.').Last());
        Assert.Equal("RunCensus", v.Method);
    }

    // ---- REPRODUCTIONS P1a / P1b on the REAL Core DLL ---------------------------------------------------------------------------
    /// <summary>Patches in place the <c>ldc.i4</c> that loads <c>FileMode</c> (three instructions before the <c>FileStream</c> constructor) inside EvidenceWriter.WriteNew.</summary>
    private static int PatchFileMode(byte[] image, byte to)
    {
        using var pe = new PEReader(System.Collections.Immutable.ImmutableArray.Create(image));
        var md = pe.GetMetadataReader();
        var patched = 0;
        foreach (var th in md.TypeDefinitions)
        {
            var td = md.GetTypeDefinition(th);
            if (md.GetString(td.Name) != "EvidenceWriter") continue;
            foreach (var mh in td.GetMethods())
            {
                var m = md.GetMethodDefinition(mh);
                if (md.GetString(m.Name) != "WriteNew" || m.RelativeVirtualAddress == 0) continue;
                var il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILBytes()!;
                var code = IlReader.Decode(il);
                var block = pe.GetSectionData(m.RelativeVirtualAddress).GetContent(0, 2);
                var headerSize = (block[0] & 3) == 2 ? 1 : (block[1] >> 4) * 4;
                for (var i = 3; i < code.Count; i++)
                {
                    if (code[i].Op != Reflection.Emit.OpCodes.Newobj) continue;
                    var handle = MetadataTokens.EntityHandle((int)code[i].Operand);
                    if (handle.Kind != HandleKind.MemberReference) continue;
                    var mr = md.GetMemberReference((MemberReferenceHandle)handle);
                    if (mr.Parent.Kind != HandleKind.TypeReference || md.GetString(md.GetTypeReference((TypeReferenceHandle)mr.Parent).Name) != "FileStream") continue;
                    Assert.Equal(0x17, il[code[i - 3].Offset]); // ldc.i4.1 = FileMode.CreateNew
                    var rva = m.RelativeVirtualAddress + headerSize + code[i - 3].Offset;
                    foreach (var s in pe.PEHeaders.SectionHeaders)
                        if (rva >= s.VirtualAddress && rva < s.VirtualAddress + s.VirtualSize)
                        {
                            image[rva - s.VirtualAddress + s.PointerToRawData] = to;
                            patched++;
                        }
                }
            }
        }
        return patched;
    }

    [Theory]
    [InlineData("P1a FileMode.CreateNew -> FileMode.Create", 0x18, 2)]
    [InlineData("P1b FileMode.CreateNew -> FileMode.Append", 0x1C, 6)]
    public void Reproduction_P1_changing_the_FileMode_of_the_real_EvidenceWriter_makes_the_scan_fail(string title, int opcode, int mode)
    {
        var image = CoreImage();
        Assert.Empty(ScanCore((byte[])image.Clone(), RealPolicy())); // control: the unmodified DLL is clean
        Assert.Equal(1, PatchFileMode(image, (byte)opcode));
        var withEverything = ScanCore(image, RealPolicy());
        Show(title, withEverything);
        Assert.Contains(withEverything, v => v.Rule == "R-FILESTREAM-ARGS" && v.Type.EndsWith("EvidenceWriter", StringComparison.Ordinal) && v.Detail.Contains("FileMode constant " + mode, StringComparison.Ordinal));
        // the arguments rule does not depend on the imports list
        Assert.Contains(RuleSet(ScanCore(image, RealPolicy(), withList: false)), r => r == "R-FILESTREAM-ARGS");
        Assert.Contains("R-FILESTREAM-ARGS", RuleSet(ForbiddenApiScan.Scan(image, "core.dll", ScanConfig.For(ScanRole.Core))));
    }

    // ---- fixtures: a Core clone ---------------------------------------------------------------------------------------------------
    private enum Variant { Baseline, NewTypeCallsWriter, ClobberMethod, FileModeConstant, FileModeUnresolved, SetAttributesNormal }

    private static byte[] CoreClone(Variant v, int mode = 1)
    {
        var fb = new FixtureBuilder(CoreAsm);
        var w = fb.Type(WriterNs, "EvidenceWriter");
        w.Method("WriteNew", il =>
        {
            il.LdArg0();
            if (v == Variant.FileModeUnresolved) il.LdArg0(); // a mode that is not loaded by a constant
            else il.LdcI4(v == Variant.FileModeConstant ? mode : 1);
            il.LdcI4(2).LdcI4(0).NewObj(Rt, "System.IO.FileStream", "string", FileModeSpec, FileAccessSpec, FileShareSpec).Pop().Ret();
        }, "void", new[] { "string" });
        w.Method("MarkReadOnly", il =>
        {
            il.LdArg0();                                                   // path
            if (v == Variant.SetAttributesNormal) il.LdcI4(128);           // FileAttributes.Normal: clears the read-only bit
            else
            {
                il.LdArg0().Call(Rt, "System.IO.File", "GetAttributes", FileAttrSpec, "string");
                il.LdcI4(1).Op(ILOpCode.Or);           // GetAttributes(p) | ReadOnly
            }
            il.Call(Rt, "System.IO.File", "SetAttributes", "void", "string", FileAttrSpec).Ret();
        }, "void", new[] { "string" });
        if (v == Variant.ClobberMethod)
            w.Method("Clobber", il =>
            {
                il.LdArg0().LdcI4(128).Call(Rt, "System.IO.File", "SetAttributes", "void", "string", FileAttrSpec); // File.SetAttributes(anyPath, Normal)
                il.LdArg0().LdcI4(2).LdcI4(2).LdcI4(0).NewObj(Rt, "System.IO.FileStream", "string", FileModeSpec, FileAccessSpec, FileShareSpec).Pop().Ret(); // new FileStream(anyPath, FileMode.Create, Write, None)
            }, "void", new[] { "string" });
        fb.Type(WriterNs, "EvidenceSealer").Method("SealOrVerify", il =>
            il.LdArg0().CallOwn(Writer, "WriteNew").LdArg0().CallOwn(Writer, "MarkReadOnly").Ret(), "void", new[] { "string" });
        if (v == Variant.NewTypeCallsWriter)
            fb.Type(WriterNs, "NewCoreType").Method("Run", il =>
                il.LdArg0().CallOwn(Writer, "WriteNew").LdArg0().CallOwn(Writer, "MarkReadOnly").Ret(), "void", new[] { "string" }); // new EvidenceWriter(anyExistingFolder).WriteNew(...) / MarkReadOnly(...)
        return fb.Build();
    }

    /// <summary>The three pinned lists of a baseline clone (what a reviewer would have checked in for it).</summary>
    private static ScanPolicy PolicyOf(byte[] baseline, string commands = "")
    {
        var o = ForbiddenApiScan.ScanDetailed(baseline, "baseline.dll", ScanConfig.For(ScanRole.Core)).Observed;
        var surface = string.Join("\n", o.Surface.Values.SelectMany(x => x)) + "\n";
        var callers = string.Join("\n", o.Callers.Select(c => c + "|baseline caller of the control")) + "\n";
        return ScanPolicy.Parse(surface, callers, commands);
    }

    private static IReadOnlyList<Violation> ScanClone(byte[] image, ScanPolicy policy) =>
        ForbiddenApiScan.Scan(image, "clone.dll", ScanConfig.For(ScanRole.Core, null, policy));

    [Fact]
    public void Positive_control_the_baseline_clone_is_clean_under_its_own_lists()
    {
        var baseline = CoreClone(Variant.Baseline);
        var policy = PolicyOf(baseline);
        Assert.Equal(2, policy.Callers.Count);
        Assert.Empty(ScanClone(baseline, policy));
    }

    [Fact]
    public void Reproduction_16_a_new_Core_type_calling_WriteNew_and_MarkReadOnly_makes_the_scan_fail()
    {
        var policy = PolicyOf(CoreClone(Variant.Baseline));
        var violations = ScanClone(CoreClone(Variant.NewTypeCallsWriter), policy);
        Show("16 new Core type NewCoreType::Run calls EvidenceWriter.WriteNew / MarkReadOnly", violations);
        var callers = violations.Where(v => v.Rule == "R-PRIVILEGED-CALLER").ToList();
        Assert.Equal(2, callers.Count);
        Assert.All(callers, v => { Assert.Equal("NewCoreType", v.Type.Split('.').Last()); Assert.Equal("Run", v.Method); });
        Assert.Equal(new[] { "R-PRIVILEGED-CALLER" }, RuleSet(violations)); // the surface of the writer did not change: only the callers rule fires
        // the same call from ANOTHER assembly (a member reference to Core) is refused by the same list
        var fb = new FixtureBuilder("I52Ct21d.HostFacts.Tools");
        fb.Type("Fx", "Other").Method("M", il => il.LdArg0().Call(CoreAsm, Writer, "WriteNew", "void", "string"), "void", new[] { "string" });
        Assert.Contains(ForbiddenApiScan.Scan(fb.Build(), "other.dll", ScanConfig.For(ScanRole.Tools, null, RealPolicy())), v => v.Rule == "R-PRIVILEGED-CALLER");
    }

    [Fact]
    public void Reproduction_P2_a_Clobber_method_using_SetAttributes_Normal_and_FileMode_Create_makes_the_scan_fail()
    {
        var policy = PolicyOf(CoreClone(Variant.Baseline));
        var violations = ScanClone(CoreClone(Variant.ClobberMethod), policy);
        Show("P2 EvidenceWriter.Clobber(anyPath, ...) = File.SetAttributes(Normal) + new FileStream(anyPath, FileMode.Create)", violations);
        Assert.Equal(new[] { "R-FILESTREAM-ARGS", "R-PRIVILEGED-SURFACE", "R-SETATTRIBUTES-ARGS" }, RuleSet(violations));
        Assert.Contains(violations, v => v.Rule == "R-PRIVILEGED-SURFACE" && v.Detail.Contains("Clobber", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(0)]
    public void The_file_mode_of_the_writer_must_be_the_constant_CreateNew_in_the_clone(int mode)
    {
        var policy = PolicyOf(CoreClone(Variant.Baseline));
        Assert.Contains(ScanClone(CoreClone(Variant.FileModeConstant, mode), policy), v => v.Rule == "R-FILESTREAM-ARGS");
    }

    [Fact]
    public void An_unresolvable_FileMode_or_a_SetAttributes_Normal_is_refused_even_without_any_other_change()
    {
        var policy = PolicyOf(CoreClone(Variant.Baseline));
        Assert.Contains(ScanClone(CoreClone(Variant.FileModeUnresolved), policy), v => v.Rule == "R-FILESTREAM-ARGS" && v.Detail.Contains("not compile-time constants", StringComparison.Ordinal));
        var normal = ScanClone(CoreClone(Variant.SetAttributesNormal), policy);
        Assert.Equal(new[] { "R-SETATTRIBUTES-ARGS" }, RuleSet(normal));
    }

    // ---- REPRODUCTION: a new [CommandMethod] ------------------------------------------------------------------------------------------
    private static byte[] CommandImage(params string[] commands)
    {
        var fb = new FixtureBuilder("I52Ct21d.HostFacts.R0");
        var t = fb.Type("I52Ct21d.HostFacts.R0", "HostGateCommands");
        var i = 0;
        foreach (var c in commands)
            t.Method("Cmd" + i++, il => il.Ret()).MethodAttribute("AcMgd", "Autodesk.AutoCAD.Runtime.CommandMethodAttribute", c);
        return fb.Build();
    }

    private const string OwnerOf0 = "I52Ct21d.HostFacts.R0::I52Ct21d.HostFacts.R0.HostGateCommands::Cmd0";

    [Fact]
    public void Reproduction_a_new_CommandMethod_that_is_not_in_the_expected_list_makes_the_scan_fail()
    {
        var expected = ScanPolicy.Parse("", "", "CommandMethod|" + OwnerOf0 + "|CT21DHG_CENSUS_R|the one expected control command\n");
        // control: exactly the expected command is clean
        Assert.Empty(ForbiddenApiScan.Scan(CommandImage("CT21DHG_CENSUS_R"), "r0.dll", ScanConfig.For(ScanRole.R0, null, expected)));
        // a second command, a renamed command, and an unlisted owner each fail
        var violations = ForbiddenApiScan.Scan(CommandImage("CT21DHG_CENSUS_R", "CT21DHG_EVIL"), "r0.dll", ScanConfig.For(ScanRole.R0, null, ScanPolicy.Parse("", "", "CommandMethod|" + OwnerOf0 + "|CT21DHG_CENSUS_R|the one expected control command\n")));
        Show("new [CommandMethod(\"CT21DHG_EVIL\")] not in expected-commands.txt", violations);
        var v = Assert.Single(violations);
        Assert.Equal("R-COMMAND-UNEXPECTED", v.Rule);
        Assert.Contains("CT21DHG_EVIL", v.Detail, StringComparison.Ordinal);
        Assert.Contains("R-COMMAND-UNEXPECTED", RuleSet(ForbiddenApiScan.Scan(CommandImage("CT21DHG_RENAMED"), "r0.dll", ScanConfig.For(ScanRole.R0, null, expected))));
        Assert.Contains("R-COMMAND-UNEXPECTED", RuleSet(ForbiddenApiScan.Scan(CommandImage("X", "CT21DHG_CENSUS_R"), "r0.dll", ScanConfig.For(ScanRole.R0, null, expected))));
    }

    [Fact]
    public void A_stale_entry_of_the_lists_is_reported_as_unused_by_the_policy()
    {
        var policy = ScanPolicy.Parse("", "A::B.C::M|B.D::N|an entry nothing uses\n", "CommandMethod|X::Y::Z|GONE|an entry nothing registers\n");
        ForbiddenApiScan.Scan(CommandImage("CT21DHG_CENSUS_R"), "r0.dll", ScanConfig.For(ScanRole.R0, null, policy));
        Assert.Single(policy.UnusedCallers());
        Assert.Single(policy.UnusedCommands());
    }

    // ---- the list files --------------------------------------------------------------------------------------------------------------
    [Fact]
    public void The_list_files_reject_malformed_input()
    {
        Assert.Throws<FormatException>(() => ScanPolicy.Parse("", "A::B::C|D::E|why text long enough\r\n", ""));
        Assert.Throws<FormatException>(() => ScanPolicy.Parse("", "A::B::C|D::E|short\n", ""));
        Assert.Throws<FormatException>(() => ScanPolicy.Parse("", "A::B::C|D::E|why text long enough\nA::B::C|D::E|why text long enough\n", ""));
        Assert.Throws<FormatException>(() => ScanPolicy.Parse("", "B::C|D::E|why text long enough\n", ""));
        Assert.Throws<FormatException>(() => ScanPolicy.Parse("", "", "Other|X::Y::Z|a|why text long enough\n"));
        Assert.Throws<FormatException>(() => ScanPolicy.Parse("nonsense\n", "", ""));
        Assert.Throws<FormatException>(() => ScanPolicy.Parse("A.B::M()->V [x]\nA.B::M()->V [x]\n", "", ""));
    }

    [Fact]
    public void The_checked_in_lists_use_LF_and_have_a_justification_per_caller_and_command()
    {
        foreach (var f in new[] { "privileged-surface.txt", "privileged-callers.txt", "expected-commands.txt" })
        {
            var bytes = File.ReadAllBytes(Path.Combine(Dir(), f));
            Assert.DoesNotContain((byte)'\r', bytes);
            Assert.Equal((byte)'\n', bytes[^1]);
        }
        var policy = RealPolicy();
        Assert.All(policy.Callers, c => Assert.True(c.Justification.Length >= 20));
        Assert.All(policy.Commands, c => Assert.True(c.Justification.Length >= 20));
    }

#if HAVE_R0
#if DEBUG
    private const string Cfg = "Debug";
#else
    private const string Cfg = "Release";
#endif

    private static byte[] R0Image() => File.ReadAllBytes(Path.Combine(Dir(), "r0", "bin", Cfg, "net8.0-windows", "I52Ct21d.HostFacts.R0.dll"));

    [Fact]
    public void The_real_R0_is_clean_and_all_three_assemblies_use_every_line_of_the_callers_and_commands_lists()
    {
        var policy = RealPolicy();
        var list = RealList();
        Assert.Empty(ForbiddenApiScan.Scan(CoreImage(), "core.dll", ScanConfig.For(ScanRole.Core, list, policy)));
        Assert.Empty(ForbiddenApiScan.Scan(ToolsImage(), "tools.dll", ScanConfig.For(ScanRole.Tools, list, policy)));
        Assert.Empty(ForbiddenApiScan.Scan(R0Image(), "r0.dll", ScanConfig.For(ScanRole.R0, list, policy)));
        Assert.Empty(policy.UnusedCallers());
        Assert.Empty(policy.UnusedCommands());
        Assert.Equal(4, policy.UsedCommands.Count);
    }

    [Fact]
    public void Dropping_one_expected_command_line_makes_the_real_R0_fail_with_the_command_rule()
    {
        var dir = Dir();
        var lines = File.ReadAllLines(Path.Combine(dir, "expected-commands.txt")).ToList();
        var drop = lines.Single(l => l.Contains("|CT21DHG_INVENTORY|", StringComparison.Ordinal));
        var policy = ScanPolicy.Parse(File.ReadAllText(Path.Combine(dir, "privileged-surface.txt")), File.ReadAllText(Path.Combine(dir, "privileged-callers.txt")), string.Join("\n", lines.Where(l => l != drop)) + "\n");
        var violations = ForbiddenApiScan.Scan(R0Image(), "r0.dll", ScanConfig.For(ScanRole.R0, RealList(), policy));
        Show("real R0 DLL against expected-commands.txt without CT21DHG_INVENTORY", violations);
        var v = Assert.Single(violations);
        Assert.Equal("R-COMMAND-UNEXPECTED", v.Rule);
        Assert.Contains("CT21DHG_INVENTORY", v.Detail, StringComparison.Ordinal);
    }
#endif
}
