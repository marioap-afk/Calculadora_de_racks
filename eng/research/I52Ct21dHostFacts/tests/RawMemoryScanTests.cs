using System.Reflection.Metadata;
using I52Ct21d.HostFacts.Tools.Scan;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>
/// Negative controls for RAW MEMORY and UNSAFE CODE (independent audit: a write through a pointer uses only IL opcodes and no external member,
/// so neither the allowlist nor the member-name rules can see it). Each shape is checked by
///  (a) the deny rules ALONE (no allowlist),
///  (b) the REAL allowed-apis.txt, and
///  (c) an allowlist that approves every import of the fixture (only the structural rules can object).
/// Two kinds of fixture: assemblies COMPILED by csc with AllowUnsafeBlocks (fixtures/UnsafeFixture, never loaded, read as bytes), and IL emitted
/// with <see cref="FixtureBuilder"/> for the opcode and signature shapes a compiler does not produce on demand.
/// </summary>
public class RawMemoryScanTests
{
    private const string Rt = "System.Runtime";
    private const string Why = "approved for this test only";
    private const string FixtureAsm = "Fixture";

    private static ApiAllowlist Real() => ApiAllowlist.LoadFile(ForbiddenApiScanTests.AllowlistPath());

    private static IReadOnlyList<Violation> Deny(byte[] image, ScanRole role = ScanRole.R0) =>
        ForbiddenApiScan.Scan(image, "fixture.dll", ScanConfig.For(role));

    private static IReadOnlyList<Violation> WithReal(byte[] image, ScanRole role = ScanRole.R0) =>
        ForbiddenApiScan.Scan(image, "fixture.dll", ScanConfig.For(role, Real()));

    /// <summary>An allowlist that approves everything the fixture imports (so that only structural and deny rules can object), plus extra lines.</summary>
    private static ApiAllowlist ApproveAllImports(byte[] image, params string[] extraLines)
    {
        var lines = ForbiddenApiScan.ListImports(image).Select(i => ApiAllowlistTestText.Line(i.Kind, i.Item)).Concat(extraLines);
        return ApiAllowlist.Parse(string.Join("\n", lines) + "\n");
    }

    private static IReadOnlyList<Violation> WithApprovedImports(byte[] image, ScanRole role = ScanRole.R0, params string[] extraLines) =>
        ForbiddenApiScan.Scan(image, "fixture.dll", ScanConfig.For(role, ApproveAllImports(image, extraLines)));

    // =====================================================================================================================
    // COMPILED fixtures (csc, AllowUnsafeBlocks in the fixture project only)
    // =====================================================================================================================
#if DEBUG
    private const string Cfg = "Debug";
#else
    private const string Cfg = "Release";
#endif

    private static string FixturePath() =>
        Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts", "fixtures", "UnsafeFixture", "bin", Cfg, "net8.0", "UnsafeFixture.dll");

    private static readonly (string Class, string[] Deny, string? ImportMarker)[] Compiled =
    {
        ("PointerWrite", new[] { "R-POINTER-TYPE", "R-RAW-STORE" }, null),                                         // *p = 5 through int*
        // Release builds elide the int* local (ldloca; stind.i4): ONLY the store opcode betrays the write. Debug builds also keep the pointer local.
        ("PointerLocalWrite", new[] { "R-RAW-STORE" }, null),                                                       // &local, write through it
        ("StackAllocPointerWrite", new[] { "R-RAW-MEMORY", "R-RAW-STORE" }, null),                                 // stackalloc + pointer write
        ("StackAllocSpan", new[] { "R-RAW-MEMORY", "R-POINTER-TYPE" }, null),                                      // Span<int> s = stackalloc: localloc + Span(void*, int)
        ("UnsafeWritePointer", new[] { "R-UNSAFE-API", "R-POINTER-TYPE" }, "Unsafe::Write"),                       // Unsafe.Write(void*, T)
        ("UnsafeWriteManaged", new[] { "R-UNSAFE-API" }, "Unsafe::WriteUnaligned"),                                // Unsafe.WriteUnaligned over a ref
        ("MarshalCopyToIntPtr", new[] { "R-PINVOKE" }, "Marshal::Copy"),                                           // Marshal.Copy(byte[], 0, IntPtr, n)
        ("BufferMemoryCopy", new[] { "R-UNSAFE-API", "R-POINTER-TYPE" }, "Buffer::MemoryCopy"),                    // Buffer.MemoryCopy
        ("GcHandleAlloc", new[] { "R-UNSAFE-API" }, "GCHandle::Alloc"),                                            // GCHandle.Alloc
        ("FixedBuffer", new[] { "R-POINTER-TYPE", "R-RAW-STORE" }, null),                                          // fixed (byte* p = a) { *p = 1; }
    };

    public static IEnumerable<object[]> CompiledCases() => Compiled.Select(c => new object[] { c.Class });

    private static byte[] FixtureBytes() => File.ReadAllBytes(FixturePath());

    [Theory]
    [MemberData(nameof(CompiledCases))]
    public void Compiled_unsafe_fixture_the_deny_rules_alone_fail_the_scan(string cls)
    {
        var (_, deny, _) = Compiled.Single(c => c.Class == cls);
        var violations = Deny(FixtureBytes());
        foreach (var rule in deny)
            Assert.True(violations.Any(v => v.Type == "Fx." + cls && v.Rule == rule), cls + ": expected " + rule + " but got " + string.Join(",", violations.Where(v => v.Type == "Fx." + cls).Select(v => v.Rule).Distinct()));
    }

    [Theory]
    [MemberData(nameof(CompiledCases))]
    public void Compiled_unsafe_fixture_the_real_allowlist_fails_the_scan_too(string cls)
    {
        var (_, deny, marker) = Compiled.Single(c => c.Class == cls);
        var violations = WithReal(FixtureBytes());
        // the structural rules do not depend on the allowlist: they fire with it as well
        foreach (var rule in deny)
            Assert.True(violations.Any(v => v.Type == "Fx." + cls && v.Rule == rule), cls + ": expected " + rule + " with the real allowlist");
        // and, where an external member is involved, the first layer refuses the import itself
        if (marker is not null)
            Assert.Contains(violations, v => v.Rule.StartsWith("R-IMPORT-", StringComparison.Ordinal) && v.Detail.Contains(marker, StringComparison.Ordinal));
    }

    [Fact]
    public void Compiled_unsafe_fixture_the_assembly_that_was_compiled_with_unsafe_code_is_refused_as_such()
    {
        var image = FixtureBytes();
        Assert.Contains(Deny(image), v => v.Rule == "R-UNSAFE-CODE" && v.Type == "<metadata>");
        Assert.Contains(WithReal(image), v => v.Rule == "R-UNSAFE-CODE");
        Assert.Contains(WithApprovedImports(image), v => v.Rule == "R-UNSAFE-CODE");
    }

    [Fact]
    public void Compiled_unsafe_fixture_with_every_import_approved_the_raw_memory_rules_still_fail_each_class()
    {
        var violations = WithApprovedImports(FixtureBytes());
        foreach (var (cls, deny, _) in Compiled)
            foreach (var rule in deny.Where(r => r is "R-POINTER-TYPE" or "R-RAW-MEMORY" or "R-RAW-STORE"))
                Assert.True(violations.Any(v => v.Type == "Fx." + cls && v.Rule == rule), cls + ": expected " + rule);
        // the managed-reference Unsafe call has no pointer at all: only the member rule can see it, and it does
        Assert.Contains(violations, v => v.Type == "Fx.UnsafeWriteManaged" && v.Rule == "R-UNSAFE-API");
    }

    [Fact]
    public void Compiled_unsafe_fixture_plain_managed_code_next_to_the_unsafe_code_is_not_flagged_by_the_raw_memory_rules()
    {
        var violations = WithApprovedImports(FixtureBytes());
        Assert.DoesNotContain(violations, v => v.Type == "Fx.PlainManaged");
    }

    // =====================================================================================================================
    // EMITTED fixtures: opcodes
    // =====================================================================================================================
    private static byte[] InType(Action<FixtureBuilder.Il> body, string ns = "Fx", string name = "User", string asm = FixtureAsm)
    {
        var fb = new FixtureBuilder(asm);
        fb.Type(ns, name).Method("M", body);
        return fb.Build();
    }

    private static readonly (string Name, Func<byte[]> Build, string Rule)[] Structural = BuildStructural();

    private static (string Name, Func<byte[]> Build, string Rule)[] BuildStructural()
    {
        var list = new List<(string, Func<byte[]>, string)>
        {
            ("localloc", () => InType(il => il.LdcI4(4).Op(ILOpCode.Localloc).Pop()), "R-RAW-MEMORY"),
            ("cpblk", () => InType(il => il.LdArg0().LdArg0().LdcI4(4).Op(ILOpCode.Cpblk)), "R-RAW-MEMORY"),
            ("initblk", () => InType(il => il.LdArg0().LdcI4(0).LdcI4(4).Op(ILOpCode.Initblk)), "R-RAW-MEMORY"),
        };
        foreach (var op in new[]
        {
            ILOpCode.Stind_i, ILOpCode.Stind_i1, ILOpCode.Stind_i2, ILOpCode.Stind_i4, ILOpCode.Stind_i8, ILOpCode.Stind_r4, ILOpCode.Stind_r8, ILOpCode.Stind_ref,
        })
        {
            var captured = op;
            list.Add((captured.ToString().ToLowerInvariant().Replace('_', '.'), () => InType(il => il.LdArg0().LdcI4(1).Op(captured)), "R-RAW-STORE"));
        }
        list.Add(("stobj", () => InType(il => il.LdArg0().LdArg0().OpType(ILOpCode.Stobj, Rt, "System.Int32")), "R-RAW-STORE"));
        list.Add(("cpobj", () => InType(il => il.LdArg0().LdArg0().OpType(ILOpCode.Cpobj, Rt, "System.Int32")), "R-RAW-STORE"));

        // pointer and function-pointer types in every signature position
        FixtureBuilder With(Action<FixtureBuilder.TypeSpec> shape, string asm = FixtureAsm)
        {
            var fb = new FixtureBuilder(asm);
            shape(fb.Type("Fx", "User"));
            return fb;
        }
        list.Add(("int* parameter", () => With(t => t.Method("M", il => il.Ret(), "void", new[] { "int*" })).Build(), "R-POINTER-TYPE"));
        list.Add(("void* parameter", () => With(t => t.Method("M", il => il.Ret(), "void", new[] { "void*" })).Build(), "R-POINTER-TYPE"));
        list.Add(("int** parameter", () => With(t => t.Method("M", il => il.Ret(), "void", new[] { "int**" })).Build(), "R-POINTER-TYPE"));
        list.Add(("int* return type", () => With(t => t.Method("M", il => il.Ret(), "int*")).Build(), "R-POINTER-TYPE"));
        list.Add(("delegate* parameter", () => With(t => t.Method("M", il => il.Ret(), "void", new[] { "fnptr" })).Build(), "R-POINTER-TYPE"));
        list.Add(("int* local", () => With(t => t.Method("M", il => il.Ret(), "void", null, new[] { "int*" })).Build(), "R-POINTER-TYPE"));
        list.Add(("void* local", () => With(t => t.Method("M", il => il.Ret(), "void", null, new[] { "void*" })).Build(), "R-POINTER-TYPE"));
        list.Add(("delegate* local", () => With(t => t.Method("M", il => il.Ret(), "void", null, new[] { "fnptr" })).Build(), "R-POINTER-TYPE"));
        list.Add(("pinned local", () => With(t => t.Method("M", il => il.Ret(), "void", null, new[] { "pinned:int" })).Build(), "R-POINTER-TYPE"));
        list.Add(("int* field", () => With(t => t.Field("F", "int*").Method("M", il => il.Ret())).Build(), "R-POINTER-TYPE"));
        list.Add(("delegate* field", () => With(t => t.Field("F", "fnptr").Method("M", il => il.Ret())).Build(), "R-POINTER-TYPE"));
        list.Add(("int* property", () => With(t => t.Property("P", "int*").Method("M", il => il.Ret())).Build(), "R-POINTER-TYPE"));
        list.Add(("int* event type", () => With(t => t.Event("E", "int*").Method("M", il => il.Ret())).Build(), "R-POINTER-TYPE"));
        list.Add(("call of an external member with a pointer parameter", () => InType(il => il.Call(Rt, "Ext.Native", "Poke", "void", "void*", "int")), "R-POINTER-TYPE"));
        list.Add(("call of an external member with a pointer return", () => InType(il => il.Call(Rt, "Ext.Native", "Alloc", "int*")), "R-POINTER-TYPE"));

        // unsafe-code markers, non-IL images, native methods
        list.Add(("module UnverifiableCodeAttribute", () =>
        {
            var fb = new FixtureBuilder(FixtureAsm).Attribute(Rt, "System.Security.UnverifiableCodeAttribute");
            fb.Type("Fx", "User").Method("M", il => il.Ret());
            return fb.Build();
        }, "R-UNSAFE-CODE"));
        list.Add(("assembly UnverifiableCodeAttribute", () =>
        {
            var fb = new FixtureBuilder(FixtureAsm).Attribute(Rt, "System.Security.UnverifiableCodeAttribute", onModule: false);
            fb.Type("Fx", "User").Method("M", il => il.Ret());
            return fb.Build();
        }, "R-UNSAFE-CODE"));
        list.Add(("a self-defined UnverifiableCodeAttribute", () =>
        {
            var fb = new FixtureBuilder(FixtureAsm);
            fb.Type("System.Security", "UnverifiableCodeAttribute").Method("M", il => il.Ret());
            return fb.Build();
        }, "R-UNSAFE-CODE"));
        list.Add(("not an IL-only image", () =>
        {
            var fb = new FixtureBuilder(FixtureAsm).NotIlOnly();
            fb.Type("Fx", "User").Method("M", il => il.Ret());
            return fb.Build();
        }, "R-NOT-ILONLY"));
        list.Add(("native method implementation", () => With(t => t.Method("M", il => il.Ret(), "void", null, null, native: true)).Build(), "R-NATIVE-METHOD"));
        return list.ToArray();
    }

    public static IEnumerable<object[]> StructuralCases() => Structural.Select((c, i) => new object[] { i, c.Name });

    [Theory]
    [MemberData(nameof(StructuralCases))]
    public void Structural_negative_control_fails_with_the_deny_rules_alone_with_the_real_allowlist_and_with_every_import_approved(int index, string name)
    {
        var (caseName, build, rule) = Structural[index];
        Assert.Equal(name, caseName);
        var image = build();
        Assert.Contains(Deny(image), v => v.Rule == rule);
        Assert.Contains(WithReal(image), v => v.Rule == rule);
        var approved = WithApprovedImports(image);
        Assert.Contains(approved, v => v.Rule == rule);
        Assert.DoesNotContain(approved, v => v.Rule.StartsWith("R-IMPORT-", StringComparison.Ordinal));
    }

    [Fact]
    public void Structural_controls_are_reported_for_the_method_that_contains_them()
    {
        var local = new FixtureBuilder(FixtureAsm);
        local.Type("Fx", "User").Method("Clean", il => il.Ret()).Method("Dirty", il => il.Ret(), "void", null, new[] { "int*" });
        var v = Deny(local.Build()).Single(x => x.Rule == "R-POINTER-TYPE");
        Assert.Equal("Fx.User", v.Type);
        Assert.Equal("Dirty", v.Method);
    }

    // =====================================================================================================================
    // approvals of the store opcodes: per (assembly, type), never for raw memory, never in a method that can forge an address
    // =====================================================================================================================
    private static byte[] StoreIn(string asm, string type, Action<FixtureBuilder.Il>? body = null, string[]? locals = null)
    {
        var fb = new FixtureBuilder(asm);
        fb.Type("Fx", type).Method("M", body ?? (il => il.LdArg0().LdcI4(1).Op(ILOpCode.Stind_i4)), "void", null, locals);
        return fb.Build();
    }

    private static string Approve(string op, string asm, string type) => "O|" + op + "|" + AllowEntry.ScopeOf(asm, "Fx." + type) + "|" + Why;

    [Fact]
    public void A_store_opcode_is_refused_without_an_approval_and_accepted_with_the_exact_assembly_and_type()
    {
        var image = StoreIn(FixtureAsm, "User");
        Assert.Contains(Deny(image), v => v.Rule == "R-RAW-STORE");
        Assert.Contains(WithApprovedImports(image), v => v.Rule == "R-RAW-STORE");                                       // imports approved, store not
        Assert.Empty(WithApprovedImports(image, ScanRole.R0, Approve("stind.i4", FixtureAsm, "User")));
    }

    [Fact]
    public void A_store_approval_does_not_hold_for_another_opcode_another_type_or_the_same_type_name_in_another_assembly()
    {
        Assert.Contains(WithApprovedImports(StoreIn(FixtureAsm, "User"), ScanRole.R0, Approve("stind.i8", FixtureAsm, "User")), v => v.Rule == "R-RAW-STORE");
        Assert.Contains(WithApprovedImports(StoreIn(FixtureAsm, "User"), ScanRole.R0, Approve("stind.i4", FixtureAsm, "Other")), v => v.Rule == "R-RAW-STORE");
        // a SPOOF: the same type name, defined in a different assembly
        var spoof = StoreIn("Spoofed.Assembly", "User");
        Assert.Contains(WithApprovedImports(spoof, ScanRole.R0, Approve("stind.i4", FixtureAsm, "User")), v => v.Rule == "R-RAW-STORE");
        // nested types of the approved top-level type share its approval
        var fb = new FixtureBuilder(FixtureAsm);
        fb.Type("Fx", "User").Method("M", il => il.Ret());
        Assert.Empty(WithApprovedImports(fb.Build(), ScanRole.R0, Approve("stind.i4", FixtureAsm, "User")));
    }

    [Fact]
    public void An_approved_store_is_refused_in_a_method_that_can_forge_a_native_address()
    {
        var approval = Approve("stind.i4", FixtureAsm, "User");
        var conv = StoreIn(FixtureAsm, "User", il => il.LdArg0().Op(ILOpCode.Conv_u).LdcI4(1).Op(ILOpCode.Stind_i4));
        var convRules = WithApprovedImports(conv, ScanRole.R0, approval).Select(v => v.Rule).Distinct().ToArray();
        Assert.Contains("R-RAW-STORE-NATIVE-ADDRESS", convRules);
        Assert.DoesNotContain("R-RAW-STORE", convRules);

        var convI = StoreIn(FixtureAsm, "User", il => il.LdArg0().Op(ILOpCode.Conv_i).LdcI4(1).Op(ILOpCode.Stind_i4));
        Assert.Contains(WithApprovedImports(convI, ScanRole.R0, approval), v => v.Rule == "R-RAW-STORE-NATIVE-ADDRESS");

        var intPtrLocal = StoreIn(FixtureAsm, "User", null, new[] { "intptr" });
        Assert.Contains(WithApprovedImports(intPtrLocal, ScanRole.R0, approval), v => v.Rule == "R-RAW-STORE-NATIVE-ADDRESS");

        var pointerLocal = StoreIn(FixtureAsm, "User", null, new[] { "int*" });
        Assert.Contains(WithApprovedImports(pointerLocal, ScanRole.R0, approval), v => v.Rule == "R-POINTER-TYPE");
    }

    [Fact]
    public void Raw_memory_opcodes_can_not_be_approved_by_any_list_line()
    {
        foreach (var op in new[] { "localloc", "cpblk", "initblk" })
            Assert.Throws<FormatException>(() => ApiAllowlist.Parse("O|" + op + "|" + AllowEntry.ScopeOf(FixtureAsm, "Fx.User") + "|" + Why + "\n"));
        // and a store approval is never used as a wildcard
        Assert.Throws<FormatException>(() => ApiAllowlist.Parse("O|stind.*|" + AllowEntry.ScopeOf(FixtureAsm, "Fx.User") + "|" + Why + "\n"));
    }

    // =====================================================================================================================
    // SCOPED APPROVALS CANNOT BE SPOOFED (assembly + type), checked against EVERY scoped line of the real list
    // =====================================================================================================================
    [Fact]
    public void Every_scoped_member_approval_of_the_real_list_is_refused_to_the_same_type_name_in_another_assembly_and_granted_to_the_real_one()
    {
        var list = Real();
        var scoped = list.Entries.Where(e => e.Kind == AllowKind.Member && e.Scope.Length > 0).ToList();
        // the member also approved with no scope needs no spoof test (anyone may use it)
        var unscopedKeys = list.Entries.Where(e => e.Kind == AllowKind.Member && e.Scope.Length == 0).Select(e => e.Item).ToHashSet(StringComparer.Ordinal);
        var testable = scoped.Where(e => !unscopedKeys.Contains(e.Item) && !e.Item[..e.Item.IndexOf("::", StringComparison.Ordinal)].Contains('/')).ToList();
        Assert.True(testable.Count > 200, "scoped member approvals examined: " + testable.Count);

        foreach (var group in testable.GroupBy(e => e.Scope))
        {
            var (asm, type) = SplitScope(group.Key);
            var dot = type.LastIndexOf('.');
            var ns = dot < 0 ? "" : type[..dot];
            var name = dot < 0 ? type : type[(dot + 1)..];

            byte[] Build(string assemblyName)
            {
                var fb = new FixtureBuilder(assemblyName);
                var t = fb.Type(ns, name);
                foreach (var e in group)
                {
                    var sep = e.Item.IndexOf("::", StringComparison.Ordinal);
                    var declaring = e.Item[..sep];
                    var member = e.Item[(sep + 2)..];
                    if (member == "*") member = "Probe";
                    var captured = (declaring, member);
                    t.Method("M_" + t.Methods.Count, il =>
                    {
                        if (captured.member == ".ctor") il.NewObj(Rt, captured.declaring).Pop();
                        else il.Call(Rt, captured.declaring, captured.member, "void");
                    });
                }
                return fb.Build();
            }

            // the real (assembly, type): no scope violation
            Assert.DoesNotContain(WithReal(Build(asm)), v => v.Rule == "R-IMPORT-SCOPE");
            // the same type name in a different assembly: every member is refused
            var spoofed = WithReal(Build("Spoofed." + asm)).Where(v => v.Rule == "R-IMPORT-SCOPE").ToList();
            Assert.True(spoofed.Count == group.Count(), group.Key + ": expected " + group.Count() + " scope violations in the spoofed assembly but got " + spoofed.Count);
        }
    }

    private static (string Asm, string Type) SplitScope(string scope)
    {
        var sep = scope.IndexOf("::", StringComparison.Ordinal);
        return (scope[..sep], scope[(sep + 2)..]);
    }

    [Fact]
    public void The_types_that_hold_a_privilege_in_the_real_list_are_all_covered_by_the_spoof_check()
    {
        var scopes = Real().Entries.Where(e => e.Scope.Length > 0).Select(e => e.Scope).ToHashSet(StringComparer.Ordinal);
        foreach (var named in new[]
        {
            "I52Ct21d.HostFacts.Core::I52Ct21d.HostFacts.Core.EvidenceSealer", "I52Ct21d.HostFacts.Core::I52Ct21d.HostFacts.Core.Sha256Hex",
            "I52Ct21d.HostFacts.R0::I52Ct21d.HostFacts.R0.HostEnvironment", "I52Ct21d.HostFacts.R0::I52Ct21d.HostFacts.R0.HostGateCommands",
            "I52Ct21d.HostFacts.Tools::I52Ct21d.HostFacts.Tools.Program", "I52Ct21d.HostFacts.Tools::I52Ct21d.HostFacts.Tools.Scan.IlReader",
            "I52Ct21d.HostFacts.Core::I52Ct21d.HostFacts.Core.JsonSchemaLite", "I52Ct21d.HostFacts.Core::I52Ct21d.HostFacts.Core.EvidenceWriter",
            "I52Ct21d.HostFacts.R0::I52Ct21d.HostFacts.R0.AcadSideDbReader",
        })
            Assert.Contains(named, scopes);
        // every scope names its assembly (no legacy bare type name)
        Assert.All(scopes, s => Assert.Contains("::", s));
    }

    [Fact]
    public void Every_store_approval_of_the_real_list_is_refused_to_the_same_type_name_in_another_assembly()
    {
        var approvals = Real().Entries.Where(e => e.Kind == AllowKind.Opcode).ToList();
        Assert.NotEmpty(approvals);
        Assert.All(approvals, e => Assert.NotEqual("I52Ct21d.HostFacts.R0", SplitScope(e.Scope).Asm)); // the R0 DLL needs no store approval at all
        foreach (var e in approvals)
        {
            var (asm, type) = SplitScope(e.Scope);
            var dot = type.LastIndexOf('.');
            byte[] Build(string assemblyName)
            {
                var fb = new FixtureBuilder(assemblyName);
                fb.Type(type[..dot], type[(dot + 1)..]).Method("M", il =>
                {
                    if (e.Item is "stobj" or "cpobj") il.LdArg0().LdArg0().OpType(e.Item == "stobj" ? ILOpCode.Stobj : ILOpCode.Cpobj, Rt, "System.Int32");
                    else il.LdArg0().LdcI4(1).Op(Enum.Parse<ILOpCode>(e.Item.Replace('.', '_'), ignoreCase: true));
                });
                return fb.Build();
            }
            Assert.DoesNotContain(WithReal(Build(asm)), v => v.Rule == "R-RAW-STORE");
            Assert.Contains(WithReal(Build("Spoofed." + asm)), v => v.Rule == "R-RAW-STORE");
        }
    }

    [Fact]
    public void The_real_assemblies_use_no_pointer_no_raw_memory_opcode_and_no_unsafe_marker()
    {
        var core = ForbiddenApiScan.Scan(File.ReadAllBytes(typeof(Core.EvidenceWriter).Assembly.Location), "core", ScanConfig.For(ScanRole.Core, Real()));
        var tools = ForbiddenApiScan.Scan(File.ReadAllBytes(typeof(Tools.Program).Assembly.Location), "tools", ScanConfig.For(ScanRole.Tools, Real()));
        Assert.Empty(core);
        Assert.Empty(tools);
        // without the allowlist the only thing the deny layer reports on the real code is the (approved) by-ref stores, never raw memory
        var denyOnly = Deny(File.ReadAllBytes(typeof(Core.EvidenceWriter).Assembly.Location), ScanRole.Core);
        Assert.DoesNotContain(denyOnly, v => v.Rule is "R-RAW-MEMORY" or "R-POINTER-TYPE" or "R-UNSAFE-CODE" or "R-NOT-ILONLY" or "R-NATIVE-METHOD" or "R-UNSAFE-API");
        Assert.All(denyOnly, v => Assert.Equal("R-RAW-STORE", v.Rule));
    }
}
