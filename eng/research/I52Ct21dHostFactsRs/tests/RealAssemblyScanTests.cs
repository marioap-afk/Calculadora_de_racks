using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using I52Ct21d.HostFacts.Rs.Core;
using I52Ct21d.HostFacts.Rs.Tools;
using I52Ct21d.HostFacts.Tools.Scan;
using Xunit;

namespace I52Ct21d.HostFacts.Rs.Tests;

/// <summary>The checked-in lists as the CAD manager and the reviewer see them.</summary>
internal static class Lists
{
    public static string Dir => Support.RsFolder();
    public static string AllowedPath => Path.Combine(Dir, "allowed-apis-rs.txt");
    public static string WaiversPath => Path.Combine(Dir, "rs-waivers.txt");
    public static string SurfacePath => Path.Combine(Dir, "privileged-surface-rs.txt");
    public static string CallersPath => Path.Combine(Dir, "privileged-callers-rs.txt");
    public static string CommandsPath => Path.Combine(Dir, "expected-commands-rs.txt");

    public static ApiAllowlist Allowed() => ApiAllowlist.LoadFile(AllowedPath);
    public static RsWaivers Waivers() => RsWaivers.LoadFile(WaiversPath);
    public static ScanPolicy Policy() => ScanPolicy.LoadFiles(SurfacePath, CallersPath, CommandsPath);

    public static ScanPolicy PolicyWith(Func<string, string>? surface = null, Func<string, string>? callers = null, Func<string, string>? commands = null) =>
        ScanPolicy.Parse(surface?.Invoke(File.ReadAllText(SurfacePath)) ?? File.ReadAllText(SurfacePath),
            callers?.Invoke(File.ReadAllText(CallersPath)) ?? File.ReadAllText(CallersPath),
            commands?.Invoke(File.ReadAllText(CommandsPath)) ?? File.ReadAllText(CommandsPath));

    public static string CorePath => typeof(RsSchemas).Assembly.Location;
    public static string ToolsPath => typeof(RsScan).Assembly.Location;

#if DEBUG
    public const string Cfg = "Debug";
#else
    public const string Cfg = "Release";
#endif

    public static string RsPath => Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFactsRs", "rs", "bin", Cfg, "net8.0-windows", "I52Ct21d.HostFacts.Rs.dll");

    public static IReadOnlyList<RsScanInput> AllInputs() => new[]
    {
        new RsScanInput(ScanRole.Core, CorePath), new RsScanInput(ScanRole.Tools, ToolsPath), new RsScanInput(ScanRole.R0, RsPath),
    };
}

public class RealAssemblyCoreAndToolsTests
{
    [Fact]
    public void The_RS_core_assembly_is_clean_and_the_scan_really_examined_it()
    {
        var r = RsScan.ScanOne(File.ReadAllBytes(Lists.CorePath), "core-rs", ScanRole.Core, Lists.Allowed(), Lists.Waivers(), Lists.Policy());
        Assert.Empty(r.Violations);
        Assert.True(r.ImportRowsChecked > 300, "import rows: " + r.ImportRowsChecked);
        Assert.True(r.BodiesExamined > 200, "bodies: " + r.BodiesExamined);
        Assert.Equal(0, r.WaivedFindings);
    }

    [Fact]
    public void The_RS_tools_assembly_is_clean_and_the_scan_really_examined_it()
    {
        var r = RsScan.ScanOne(File.ReadAllBytes(Lists.ToolsPath), "tools-rs", ScanRole.Tools, Lists.Allowed(), Lists.Waivers(), Lists.Policy());
        Assert.Empty(r.Violations);
        Assert.True(r.ImportRowsChecked > 300, "import rows: " + r.ImportRowsChecked);
        Assert.True(r.BodiesExamined > 80, "bodies: " + r.BodiesExamined);
    }

    [Fact]
    public void The_RS_core_has_no_AutoCAD_reference_at_all()
    {
        using var pe = new System.Reflection.PortableExecutable.PEReader(System.Collections.Immutable.ImmutableArray.Create(File.ReadAllBytes(Lists.CorePath)));
        var md = pe.GetMetadataReader();
        var names = md.AssemblyReferences.Select(h => md.GetString(md.GetAssemblyReference(h).Name)).ToList();
        Assert.DoesNotContain(names, n => n.StartsWith("Ac", StringComparison.OrdinalIgnoreCase) || n.StartsWith("Autodesk", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void The_RS_tools_write_no_file_so_no_privileged_caller_is_listed_for_them()
    {
        Assert.DoesNotContain(Lists.Policy().Callers, c => c.Caller.Contains("Rs.Tools", StringComparison.Ordinal));
    }

    [Fact]
    public void A_core_that_is_scanned_as_a_plain_RS_assembly_role_is_refused_for_AutoCAD_references()
    {
        // the fixture's AutoCAD reference is a violation for the Core role (R-AUTODESK-IN-NONHOST), whatever the lists say
        var image = new Fx().Build();
        var r = RsScan.ScanOne(image, "fixture", ScanRole.Core, Fx.Permissive(image), Lists.Waivers(), Fx.PolicyOf(image));
        Assert.Contains(r.Violations, v => v.Rule == "R-AUTODESK-IN-NONHOST");
    }
}

#if HAVE_RS
public class RealAssemblyRsTests
{
    private static byte[] Image() => File.ReadAllBytes(Lists.RsPath);

    private static RsScanAssembly Scan(byte[]? image = null, ApiAllowlist? allowed = null, RsWaivers? waivers = null, ScanPolicy? policy = null) =>
        RsScan.ScanOne(image ?? Image(), "rs", ScanRole.R0, allowed ?? Lists.Allowed(), waivers ?? Lists.Waivers(), policy ?? Lists.Policy());

    [Fact]
    public void The_real_RS_assembly_is_clean_and_the_scan_really_examined_its_AutoCAD_call_sites()
    {
        var r = Scan();
        Assert.Empty(r.Violations);
        Assert.True(r.ImportRowsChecked > 250, "import rows: " + r.ImportRowsChecked);
        Assert.True(r.WaivedFindings >= 40, "waived findings: " + r.WaivedFindings);
        Assert.True(r.PrivilegedCallSites >= 40, "privileged call sites: " + r.PrivilegedCallSites);
        Assert.True(r.BodiesExamined >= 60, "bodies: " + r.BodiesExamined);
    }

    [Fact]
    public void The_three_real_assemblies_together_are_clean_and_no_list_entry_is_stale()
    {
        var result = RsScan.Run(Lists.AllInputs(), Lists.Allowed(), Lists.Waivers(), Lists.Policy());
        Assert.Empty(result.ListProblems);
        Assert.All(result.Assemblies, a => Assert.Empty(a.Violations));
        Assert.True(result.Clean);
    }

    [Fact]
    public void The_real_assembly_registers_exactly_the_three_commands_of_the_design()
    {
        var image = Image();
        var o = ForbiddenApiScan.ScanDetailed(image, "rs", RsScan.ConfigFor(ScanRole.R0, Lists.Allowed(), Lists.Policy())).Observed;
        Assert.Equal(4, o.Commands.Count);
        Assert.Contains(o.Commands, c => c.EndsWith("|CT21DHG_CENSUS_DYN", StringComparison.Ordinal));
        Assert.Contains(o.Commands, c => c.EndsWith("|CT21DHG_DIMWRITEBACK", StringComparison.Ordinal));
        Assert.Contains(o.Commands, c => c.EndsWith("|CT21DHG_TOLSCALE", StringComparison.Ordinal));
    }

    [Fact]
    public void The_real_assembly_has_exactly_one_Database_constructor_site_pattern_per_factory_method()
    {
        var layer = RsLayer.Check(Image(), "rs", Lists.Policy());
        Assert.Empty(layer.Violations);
        Assert.Equal(new[] { RsLayer.ScratchSaverType, RsLayer.SideDbHandleType, RsLayer.SideDbReaderType, RsLayer.SideDbWriterType }.OrderBy(x => x),
            layer.Observed.Surface.Keys.OrderBy(x => x));
    }

    // ---- mutation controls on the REAL assembly: each makes the verdict change, so that CLEAN is not vacuous --------------------------------
    [Fact]
    public void Mutation_control_patching_a_ForRead_constant_to_ForWrite_makes_the_scan_fail()
    {
        var image = Image();
        Assert.True(PatchBeforeCall(image, "GetObject", from: 0x16, to: 0x17) > 0, "no GetObject call site patched");
        var r = Scan(image);
        Assert.Contains(r.Violations, v => v.Rule == "R-WRITE-OPEN" && v.Type.Contains("SideDbReader", StringComparison.Ordinal));
    }

    [Fact]
    public void Mutation_control_patching_the_noDocument_argument_of_the_Database_constructor_makes_the_scan_fail()
    {
        var image = Image();
        // new Database(x, true) is ldc.i4 x, ldc.i4.1, newobj: turn the second constant into ldc.i4.0 -> noDocument = false
        var patched = PatchBeforeCall(image, ".ctor", from: 0x17, to: 0x16, onlyDeclaring: "Database");
        Assert.True(patched >= 2, "the Database constructor call sites were not found: " + patched);
        var r = Scan(image);
        Assert.Contains(r.Violations, v => v.Rule == "R-RS-DB-CTOR");
    }

    [Fact]
    public void Mutation_control_a_list_without_the_SaveAs_approval_fails_on_the_import()
    {
        var text = string.Join("\n", File.ReadAllLines(Lists.AllowedPath).Where(l => !l.StartsWith("M|Autodesk.AutoCAD.DatabaseServices.Database::SaveAs|", StringComparison.Ordinal))) + "\n";
        var r = Scan(allowed: ApiAllowlist.Parse(text));
        Assert.Contains(r.Violations, v => v.Rule.StartsWith("R-IMPORT-", StringComparison.Ordinal) && v.Detail.Contains("SaveAs", StringComparison.Ordinal));
    }

    [Fact]
    public void Mutation_control_a_list_that_moves_SaveAs_to_another_type_fails_on_the_scope()
    {
        var lines = File.ReadAllLines(Lists.AllowedPath).Select(l =>
            l.StartsWith("M|Autodesk.AutoCAD.DatabaseServices.Database::SaveAs|", StringComparison.Ordinal)
                ? l.Replace("ScratchSaver", "SideDbWriter", StringComparison.Ordinal) : l);
        var r = Scan(allowed: ApiAllowlist.Parse(string.Join("\n", lines) + "\n"));
        Assert.Contains(r.Violations, v => v.Rule == "R-IMPORT-SCOPE" && v.Detail.Contains("SaveAs", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Database::ReadDwgFile")]
    [InlineData("Database::WblockCloneObjects")]
    [InlineData("Database::.ctor")]
    [InlineData("Transaction::Commit")]
    [InlineData("BlockReference::set_ScaleFactors")]
    [InlineData("BlockTableRecord::AppendEntity")]
    [InlineData("Dimension::set_Dimscale")]
    [InlineData("Transaction::AddNewlyCreatedDBObject")]
    public void Mutation_control_a_list_without_one_write_approval_fails_on_that_import(string member)
    {
        var text = string.Join("\n", File.ReadAllLines(Lists.AllowedPath).Where(l => !l.Contains(member + "|", StringComparison.Ordinal))) + "\n";
        var r = Scan(allowed: ApiAllowlist.Parse(text));
        Assert.Contains(r.Violations, v => v.Rule.StartsWith("R-IMPORT-", StringComparison.Ordinal) && v.Detail.Contains(member.Split("::")[1], StringComparison.Ordinal));
    }

    [Fact]
    public void Mutation_control_without_the_SaveAs_waiver_the_save_is_reported()
    {
        var lines = File.ReadAllLines(Lists.WaiversPath).Where(l => !(l.StartsWith("R-DB-SAVE|", StringComparison.Ordinal) && l.Contains("ScratchSaver", StringComparison.Ordinal)));
        var r = Scan(waivers: RsWaivers.Parse(string.Join("\n", lines) + "\n"));
        Assert.Contains(r.Violations, v => v.Rule == "R-DB-SAVE" && v.Type.Contains("ScratchSaver", StringComparison.Ordinal));
    }

    [Fact]
    public void Mutation_control_without_the_ctor_waiver_the_side_database_factory_is_reported()
    {
        var lines = File.ReadAllLines(Lists.WaiversPath).Where(l => !l.StartsWith("R-DB-CTOR|", StringComparison.Ordinal));
        var r = Scan(waivers: RsWaivers.Parse(string.Join("\n", lines) + "\n"));
        Assert.Contains(r.Violations, v => v.Rule == "R-DB-CTOR");
    }

    [Fact]
    public void Mutation_control_without_the_reader_transaction_waiver_the_reader_is_reported()
    {
        var lines = File.ReadAllLines(Lists.WaiversPath).Where(l => !l.StartsWith("R-TRANSACTION-SCOPE|", StringComparison.Ordinal));
        var r = Scan(waivers: RsWaivers.Parse(string.Join("\n", lines) + "\n"));
        Assert.Contains(r.Violations, v => v.Rule == "R-TRANSACTION-SCOPE" && v.Type.Contains("SideDbReader", StringComparison.Ordinal));
    }

    [Fact]
    public void Mutation_control_a_caller_dropped_from_the_list_is_reported()
    {
        var policy = Lists.PolicyWith(callers: t => string.Join("\n", t.Split('\n').Where(l => !l.Contains("SideDbWriter::CloneDefinition", StringComparison.Ordinal))));
        var r = Scan(policy: policy);
        Assert.Contains(r.Violations, v => v.Rule == "R-RS-PRIVILEGED-CALLER" && v.Detail.Contains("CloneDefinition", StringComparison.Ordinal));
    }

    [Fact]
    public void Mutation_control_a_surface_line_removed_is_reported()
    {
        var policy = Lists.PolicyWith(surface: t => string.Join("\n", t.Split('\n').Where(l => !l.Contains("SideDbWriter::InsertReference", StringComparison.Ordinal))));
        var r = Scan(policy: policy);
        Assert.Contains(r.Violations, v => v.Rule == "R-RS-PRIVILEGED-SURFACE" && v.Detail.Contains("InsertReference", StringComparison.Ordinal));
    }

    [Fact]
    public void Mutation_control_a_surface_line_added_is_reported_as_missing_from_the_assembly()
    {
        var policy = Lists.PolicyWith(surface: t => t + "I52Ct21d.HostFacts.Rs.ScratchSaver::Clobber(System.String,System.Byte[])->System.Void [internal instance]\n");
        var r = Scan(policy: policy);
        Assert.Contains(r.Violations, v => v.Rule == "R-RS-PRIVILEGED-SURFACE" && v.Detail.Contains("Clobber", StringComparison.Ordinal));
    }

    [Fact]
    public void Mutation_control_a_command_dropped_from_the_list_is_reported()
    {
        var policy = Lists.PolicyWith(commands: t => string.Join("\n", t.Split('\n').Where(l => !l.Contains("CT21DHG_TOLSCALE", StringComparison.Ordinal))));
        var r = ForbiddenApiScan.ScanDetailed(Image(), "rs", RsScan.ConfigFor(ScanRole.R0, Lists.Allowed(), policy));
        Assert.Contains(r.Violations, v => v.Rule == "R-COMMAND-UNEXPECTED");
    }

    [Fact]
    public void A_stale_entry_in_each_list_is_reported_by_the_full_run()
    {
        var allowed = ApiAllowlist.Parse(File.ReadAllText(Lists.AllowedPath) + "M|System.Math::Cos||an approval that nothing uses\n");
        var policy = Lists.PolicyWith(
            callers: t => t + "I52Ct21d.HostFacts.Rs::I52Ct21d.HostFacts.Rs.Nobody::Calls|I52Ct21d.HostFacts.Rs.SideDbWriter::CreateScratch|a caller that does not exist\n",
            commands: t => t + "CommandMethod|I52Ct21d.HostFacts.Rs::I52Ct21d.HostFacts.Rs.RsGateCommands::Gone|CT21DHG_GONE|a command that is not registered\n");
        var result = RsScan.Run(Lists.AllInputs(), allowed, Lists.Waivers(), policy);
        var rules = result.ListProblems.Select(v => v.Rule).ToArray();
        Assert.Contains("R-ALLOWLIST-UNUSED", rules);
        Assert.Contains("R-PRIVILEGED-CALLERS-UNUSED", rules);
        Assert.Contains("R-COMMAND-UNUSED", rules);
        Assert.False(result.Clean);
    }

    /// <summary>Patches, in place, the single-byte instruction that precedes each call to <paramref name="member"/>.</summary>
    private static int PatchBeforeCall(byte[] image, string member, byte from, byte to, string? onlyDeclaring = null)
    {
        using var pe = new System.Reflection.PortableExecutable.PEReader(System.Collections.Immutable.ImmutableArray.Create(image));
        var md = pe.GetMetadataReader();
        var patched = 0;
        foreach (var th in md.TypeDefinitions)
            foreach (var mh in md.GetTypeDefinition(th).GetMethods())
            {
                var m = md.GetMethodDefinition(mh);
                if (m.RelativeVirtualAddress == 0) continue;
                var body = pe.GetMethodBody(m.RelativeVirtualAddress);
                var il = body.GetILBytes()!;
                var code = IlReader.Decode(il);
                var block = pe.GetSectionData(m.RelativeVirtualAddress).GetContent(0, 2);
                var headerSize = (block[0] & 3) == 2 ? 1 : (block[1] >> 4) * 4;
                var ilStartRva = m.RelativeVirtualAddress + headerSize;
                for (var i = 1; i < code.Count; i++)
                {
                    if (!IlReader.IsCall(code[i])) continue;
                    var handle = MetadataTokens.EntityHandle((int)code[i].Operand);
                    if (handle.Kind != HandleKind.MemberReference) continue;
                    var mr = md.GetMemberReference((MemberReferenceHandle)handle);
                    if (md.GetString(mr.Name) != member) continue;
                    if (onlyDeclaring is not null && !(mr.Parent.Kind == HandleKind.TypeReference
                        && md.GetString(md.GetTypeReference((TypeReferenceHandle)mr.Parent).Name) == onlyDeclaring)) continue;
                    var prev = code[i - 1];
                    if (il[prev.Offset] != from) continue;
                    var rva = ilStartRva + prev.Offset;
                    foreach (var s in pe.PEHeaders.SectionHeaders)
                        if (rva >= s.VirtualAddress && rva < s.VirtualAddress + s.VirtualSize)
                        {
                            image[rva - s.VirtualAddress + s.PointerToRawData] = to;
                            patched++;
                        }
                }
            }
        return patched;
    }
}
#endif

public class RsFixtureRunLevelTests
{
    [Fact]
    public void An_unused_waiver_is_reported_when_the_three_assemblies_are_scanned()
    {
        using var t = new TempDir();
        var fixturePath = Path.Combine(t.Path, "I52Ct21d.HostFacts.Rs.dll");
        File.WriteAllBytes(fixturePath, new Fx().Build());
        var inputs = new[] { new RsScanInput(ScanRole.Core, Lists.CorePath), new RsScanInput(ScanRole.Tools, Lists.ToolsPath), new RsScanInput(ScanRole.R0, fixturePath) };
        var result = RsScan.Run(inputs, Lists.Allowed(), Lists.Waivers(), Fx.PolicyOf(new Fx().Build()));
        Assert.Contains(result.ListProblems, v => v.Rule == "R-RS-WAIVER-UNUSED");
        Assert.Contains(result.ListProblems, v => v.Rule == "R-ALLOWLIST-UNUSED");
        Assert.False(result.Clean);
    }

    [Fact]
    public void Stale_entries_are_not_reported_when_only_two_assemblies_are_scanned()
    {
        var inputs = new[] { new RsScanInput(ScanRole.Core, Lists.CorePath), new RsScanInput(ScanRole.Tools, Lists.ToolsPath) };
        var result = RsScan.Run(inputs, Lists.Allowed(), Lists.Waivers(), Lists.Policy());
        Assert.Empty(result.ListProblems);
    }

    [Fact]
    public void A_privileged_type_defined_in_another_assembly_name_gets_no_privilege_from_the_waivers()
    {
        // the same type names in an assembly that is NOT called I52Ct21d.HostFacts.Rs: nothing is waived, the write members are all reported
        var fb = new I52Ct21d.HostFacts.Tests.FixtureBuilder("SomeOtherAssembly");
        fb.Type(Fx.Ns, Fx.W).Method("Create", il => il.LdcI4(1).LdcI4(1).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop());
        var image = fb.Build();
        var r = RsScan.ScanOne(image, "spoof.dll", ScanRole.R0, Fx.Permissive(image), RsWaivers.LoadFile(Lists.WaiversPath), Fx.PolicyOf(image));
        Assert.Equal(0, r.WaivedFindings);
        Assert.Contains(r.Violations, v => v.Rule == "R-DB-CTOR");
    }

    [Fact]
    public void A_privileged_type_name_in_a_different_namespace_gets_no_privilege()
    {
        var fb = new I52Ct21d.HostFacts.Tests.FixtureBuilder(Fx.Asm);
        fb.Type("Other.Namespace", Fx.W).Method("Create", il => il.LdcI4(1).LdcI4(1).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop());
        var image = fb.Build();
        var r = RsScan.ScanOne(image, "spoof.dll", ScanRole.R0, Fx.Permissive(image), RsWaivers.LoadFile(Lists.WaiversPath), Fx.PolicyOf(image));
        Assert.Contains(r.Violations, v => v.Rule == "R-RS-DB-CTOR");
    }

    [Fact]
    public void Every_waiver_of_the_checked_in_file_names_a_type_of_the_privileged_set()
    {
        foreach (var w in Lists.Waivers().Entries)
            Assert.Contains(w.Type, RsLayer.PrivilegedTypes);
    }

    [Fact]
    public void The_privileged_set_is_the_four_types_named_in_the_design_of_the_task()
    {
        Assert.Equal(new[]
        {
            "I52Ct21d.HostFacts.Rs.SideDbWriter", "I52Ct21d.HostFacts.Rs.SideDbReader", "I52Ct21d.HostFacts.Rs.ScratchSaver", "I52Ct21d.HostFacts.Rs.SideDbHandle",
        }, RsLayer.PrivilegedTypes);
    }

    [Fact]
    public void A_non_private_method_that_mentions_an_Autodesk_type_on_a_privileged_type_is_reported()
    {
        var f = new Fx();
        var image = f.Build(null, new[] { (Fx.S, "Leak", new[] { "cls:" + Fx.Db + "|" + Fx.DbT }, "void") });
        var rules = RsFixtureControlTests.Rules(image, Fx.Permissive(image), Fx.PolicyOf(image));
        Assert.Contains("R-RS-SURFACE-AUTODESK", rules);
    }

    [Fact]
    public void The_handle_type_may_expose_the_Database_to_the_assembly_by_design()
    {
        var f = new Fx();
        var image = f.Build(null, new[] { (Fx.H, "get_Database", Array.Empty<string>(), "cls:" + Fx.Db + "|" + Fx.DbT) });
        Assert.DoesNotContain("R-RS-SURFACE-AUTODESK", RsFixtureControlTests.Rules(image, Fx.Permissive(image), Fx.PolicyOf(image)));
    }

    [Fact]
    public void An_unexpected_command_registration_is_reported()
    {
        var f = new Fx();
        var image = f.Build();
        var fb = new I52Ct21d.HostFacts.Tests.FixtureBuilder(Fx.Asm);
        fb.Type(Fx.Ns, "RsGateCommands").Method("Evil", il => il.Pop()).MethodAttribute(Fx.Core, "Autodesk.AutoCAD.Runtime.CommandMethodAttribute", "EVIL_COMMAND");
        var bad = fb.Build();
        var outcome = ForbiddenApiScan.ScanDetailed(bad, "rs", RsScan.ConfigFor(ScanRole.R0, Fx.Permissive(bad), Fx.PolicyOf(image)));
        Assert.Contains(outcome.Violations, v => v.Rule == "R-COMMAND-UNEXPECTED");
    }

    [Fact]
    public void A_caller_that_is_not_in_the_list_is_reported_for_a_call_between_two_types_of_the_assembly()
    {
        var f = new Fx().Add(Fx.Adapter, "Calls", il => il.CallOwn(Fx.Ns + "." + Fx.W, "Create"));
        var image = f.Build();
        var rules = RsFixtureControlTests.Rules(image, Fx.Permissive(image), Fx.PolicyOf(new Fx().Build()));
        Assert.Contains("R-RS-PRIVILEGED-CALLER", rules);
    }

    [Fact]
    public void A_listed_caller_is_accepted_for_the_same_call()
    {
        var f = new Fx().Add(Fx.Adapter, "Calls", il => il.CallOwn(Fx.Ns + "." + Fx.W, "Create"));
        var image = f.Build();
        var callers = "I52Ct21d.HostFacts.Rs::I52Ct21d.HostFacts.Rs.TolScaleHostAdapter::Calls|I52Ct21d.HostFacts.Rs.SideDbWriter::Create|a fixture caller listed on purpose\n";
        var rules = RsFixtureControlTests.Rules(image, Fx.Permissive(image), Fx.PolicyOf(image, callers));
        Assert.DoesNotContain("R-RS-PRIVILEGED-CALLER", rules);
    }

    [Fact]
    public void A_privileged_type_calling_itself_needs_no_listing()
    {
        var f = new Fx().Add(Fx.W, "Calls", il => il.CallOwn(Fx.Ns + "." + Fx.W, "Create"));
        var image = f.Build();
        Assert.DoesNotContain("R-RS-PRIVILEGED-CALLER", RsFixtureControlTests.Rules(image, Fx.Permissive(image), Fx.PolicyOf(image)));
    }

    [Fact]
    public void A_surface_change_without_a_list_edit_is_reported_for_a_new_method()
    {
        var clean = new Fx().Build();
        var changed = new Fx().Add(Fx.S, "Clobber", il => il.Pop()).Build();
        Assert.Contains("R-RS-PRIVILEGED-SURFACE", RsFixtureControlTests.Rules(changed, Fx.Permissive(changed), Fx.PolicyOf(clean)));
    }

    [Fact]
    public void A_surface_change_is_reported_for_a_removed_method()
    {
        var clean = new Fx().Build();
        var changed = new Fx().Remove(Fx.S, "Save").Add(Fx.S, "Other", il => il.Pop()).Build();
        Assert.Contains("R-RS-PRIVILEGED-SURFACE", RsFixtureControlTests.Rules(changed, Fx.Permissive(changed), Fx.PolicyOf(clean)));
    }
}
