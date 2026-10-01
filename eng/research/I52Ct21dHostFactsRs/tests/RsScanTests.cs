using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using I52Ct21d.HostFacts.Rs.Tools;
using I52Ct21d.HostFacts.Tools.Scan;
using Xunit;
using Il = I52Ct21d.HostFacts.Tests.FixtureBuilder.Il;

namespace I52Ct21d.HostFacts.Rs.Tests;

/// <summary>
/// Builds tiny IL assemblies shaped like the RS DLL (assembly <c>I52Ct21d.HostFacts.Rs</c>, the four privileged types, external references to the AutoCAD
/// assemblies and to the RS core). They are only READ by the scan; no AutoCAD file is used and nothing is loaded. The CLEAN fixture follows every rule;
/// each NEGATIVE control changes one thing (a violation) and the scan MUST report the expected rule.
/// </summary>
internal sealed class Fx
{
    public const string Asm = "I52Ct21d.HostFacts.Rs";
    public const string Ns = "I52Ct21d.HostFacts.Rs";
    public const string Db = "Acdbmgd";
    public const string Core = "accoremgd";
    public const string RsCore = "I52Ct21d.HostFacts.Rs.Core";
    public const string D = "Autodesk.AutoCAD.DatabaseServices.";
    public const string DbT = D + "Database";
    public const string Tr = D + "Transaction";
    public const string Oid = "vt:" + Db + "|" + D + "ObjectId";
    public const string Om = "vt:" + Db + "|" + D + "OpenMode";
    public const string Fs = "vt:System.Runtime|System.IO.FileShare";
    public const string Dwg = "vt:" + Db + "|" + D + "DwgVersion";
    public const string Guard = "I52Ct21d.HostFacts.Rs.Core.ScratchRootGuard";
    public const string Target = "I52Ct21d.HostFacts.Rs.Core.ScratchTarget";
    public const string Source = "I52Ct21d.HostFacts.Rs.Core.ReadableSource";
    public const string App = "Autodesk.AutoCAD.ApplicationServices.";
    public const string Rt = "System.Runtime";

    public const string W = "SideDbWriter";
    public const string R = "SideDbReader";
    public const string S = "ScratchSaver";
    public const string H = "SideDbHandle";
    public const string Adapter = "TolScaleHostAdapter";

    public List<(string Type, string Method, Action<Il> Body)> Methods { get; } = Clean();

    private static List<(string, string, Action<Il>)> Clean() => new()
    {
        (W, "Create", il => il.LdcI4(1).LdcI4(1).NewObj(Db, DbT, "bool", "bool").Pop()),
        (W, "Open", il =>
        {
            il.CallVirt(RsCore, Guard, "AssertReadable", "void", "cls:" + RsCore + "|" + Source);
            il.LdcI4(0).LdcI4(1).NewObj(Db, DbT, "bool", "bool").Pop();
            il.CallVirt(RsCore, Source, "get_FullPath", "string").LdcI4(1).LdcI4(1).LdcI4(0).CallVirt(Db, DbT, "ReadDwgFile", "void", "string", Fs, "bool", "string");
        }),
        (W, "Write", il =>
        {
            il.CallVirt(Db, D + "Database", "get_TransactionManager", "cls:" + Db + "|" + D + "TransactionManager");
            il.CallVirt(Db, D + "TransactionManager", "StartTransaction", "cls:" + Db + "|" + Tr);
            il.LdcI4(1).CallVirt(Db, Tr, "GetObject", "obj", Oid, Om);
            il.CallVirt(Db, Tr, "Commit", "void");
        }),
        (W, "Clone", il => il.CallVirt(Db, DbT, "WblockCloneObjects", "void")),
        (R, "Read", il =>
        {
            il.CallVirt(Db, D + "Database", "get_TransactionManager", "cls:" + Db + "|" + D + "TransactionManager");
            il.CallVirt(Db, D + "TransactionManager", "StartOpenCloseTransaction", "obj");
            il.LdcI4(0).CallVirt(Db, Tr, "GetObject", "obj", Oid, Om);
        }),
        (S, "Save", il =>
        {
            il.CallVirt(RsCore, Guard, "AssertCreatable", "void", "cls:" + RsCore + "|" + Target);
            il.CallVirt(RsCore, Target, "get_FullPath", "string").LdcI4(0).CallVirt(Db, DbT, "SaveAs", "void", "string", Dwg);
        }),
        (H, "Dispose", il => il.CallVirt(Db, "Autodesk.AutoCAD.Runtime.DisposableWrapper", "Dispose", "void")),
    };

    public Fx Replace(string type, string method, Action<Il> body)
    {
        var i = Methods.FindIndex(m => m.Type == type && m.Method == method);
        if (i < 0) throw new InvalidOperationException("no such fixture method " + type + "::" + method);
        Methods[i] = (type, method, body);
        return this;
    }

    public Fx Add(string type, string method, Action<Il> body)
    {
        Methods.Add((type, method, body));
        return this;
    }

    public Fx Remove(string type, string method)
    {
        Methods.RemoveAll(m => m.Type == type && m.Method == method);
        return this;
    }

    public byte[] Build(Action<I52Ct21d.HostFacts.Tests.FixtureBuilder>? extra = null, (string Type, string Name, string[] Params, string Ret)[]? publicMethods = null)
    {
        var fb = new I52Ct21d.HostFacts.Tests.FixtureBuilder(Asm);
        foreach (var g in Methods.GroupBy(m => m.Type))
        {
            var t = fb.Type(Ns, g.Key);
            foreach (var m in g) t.Method(m.Method, m.Body);
            if (publicMethods is not null)
                foreach (var p in publicMethods.Where(x => x.Type == g.Key))
                    t.Method(p.Name, il => il.Pop(), p.Ret, p.Params);
        }
        extra?.Invoke(fb);
        return fb.Build();
    }

    public static ScanPolicy PolicyOf(byte[] image, string callers = "", string commands = "")
    {
        var surface = RsLayer.Check(image, "clean.dll", null).Observed.Surface;
        var text = string.Join("\n", surface.SelectMany(p => p.Value)) + "\n";
        return ScanPolicy.Parse(text, callers, commands);
    }

    public static ApiAllowlist Permissive(byte[] image)
    {
        var lines = new List<string>();
        foreach (var (kind, item, _) in ForbiddenApiScan.ListImports(image))
            lines.Add(AllowEntry.KindLetter(kind) + "|" + item + "||permissive allowlist of a negative control");
        return ApiAllowlist.Parse(string.Join("\n", lines) + "\n");
    }
}

public class RsFixtureControlTests
{
    private static readonly RsWaivers Waivers = RsWaivers.LoadFile(Path.Combine(Support.RsFolder(), "rs-waivers.txt"));

    private static ApiAllowlist Real() => ApiAllowlist.LoadFile(Path.Combine(Support.RsFolder(), "allowed-apis-rs.txt"));

    private static RsWaivers FreshWaivers() => RsWaivers.LoadFile(Path.Combine(Support.RsFolder(), "rs-waivers.txt"));

    /// <summary>Scans a fixture and returns the distinct rules found.</summary>
    internal static string[] Rules(byte[] image, ApiAllowlist allow, ScanPolicy policy, ScanRole role = ScanRole.R0) =>
        RsScan.ScanOne(image, "fixture.dll", role, allow, FreshWaivers(), policy).Violations.Select(v => v.Rule).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();

    // ---- POSITIVE CONTROL: a fixture that follows every rule is CLEAN, with the real allowlist, the real waivers and a policy derived from it ----
    [Fact]
    public void Positive_control_the_clean_fixture_passes_the_real_allowlist_and_the_real_waivers()
    {
        var image = new Fx().Build();
        var rules = Rules(image, Real(), Fx.PolicyOf(image));
        Assert.Empty(rules);
    }

    [Fact]
    public void Positive_control_the_clean_fixture_passes_a_permissive_allowlist_too()
    {
        var image = new Fx().Build();
        Assert.Empty(Rules(image, Fx.Permissive(image), Fx.PolicyOf(image)));
    }

    [Fact]
    public void Positive_control_the_waivers_were_really_used_by_the_clean_fixture()
    {
        var image = new Fx().Build();
        var waivers = FreshWaivers();
        var result = RsScan.ScanOne(image, "fixture.dll", ScanRole.R0, Real(), waivers, Fx.PolicyOf(image));
        Assert.Empty(result.Violations);
        Assert.True(result.WaivedFindings >= 5, "waived findings: " + result.WaivedFindings);
        Assert.NotEmpty(waivers.Used);
    }

    // ---- NEGATIVE CONTROLS ---------------------------------------------------------------------------------------------------------------
    internal sealed record Case(string Name, Func<Fx, Fx> Build, string Rule);

    private static string[] P(params string[] p) => p;

    internal static IEnumerable<Case> Cases()
    {
        // --- the active document and the working database ---
        yield return new("WorkingDatabase getter in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.Call(Fx.Db, Fx.D + "HostApplicationServices", "get_WorkingDatabase", "obj")), "R-PRODUCT-DATABASE");
        yield return new("WorkingDatabase setter in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.Call(Fx.Db, Fx.D + "HostApplicationServices", "set_WorkingDatabase", "void", "obj")), "R-PRODUCT-DATABASE");
        yield return new("WorkingDatabase getter in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.Call(Fx.Db, Fx.D + "HostApplicationServices", "get_WorkingDatabase", "obj")), "R-PRODUCT-DATABASE");
        yield return new("MdiActiveDocument.Database in SideDbWriter", f => f.Add(Fx.W, "Evil", il =>
            il.CallVirt(Fx.Core, Fx.App + "DocumentCollection", "get_MdiActiveDocument", "obj").CallVirt(Fx.Core, Fx.App + "Document", "get_Database", "obj")), "R-PRODUCT-DATABASE");
        yield return new("MdiActiveDocument.Database in the adapter", f => f.Add(Fx.Adapter, "Evil", il =>
            il.CallVirt(Fx.Core, Fx.App + "DocumentCollection", "get_MdiActiveDocument", "obj").CallVirt(Fx.Core, Fx.App + "Document", "get_Database", "obj")), "R-PRODUCT-DATABASE");
        yield return new("Document.TransactionManager in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Core, Fx.App + "Document", "get_TransactionManager", "obj")), "R-PRODUCT-DATABASE");
        // --- document manager ---
        yield return new("DocumentManager.Open", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Core, Fx.App + "DocumentCollection", "Open", "obj", "string")), "R-COMMAND");
        yield return new("DocumentManager.Add", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Core, Fx.App + "DocumentCollection", "Add", "obj", "string")), "R-COMMAND");
        yield return new("DocumentManager.CloseAll", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Core, Fx.App + "DocumentCollection", "CloseAll", "void")), "R-COMMAND");
        yield return new("DocumentManager.set_MdiActiveDocument", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Core, Fx.App + "DocumentCollection", "set_MdiActiveDocument", "void", "obj")), "R-COMMAND");
        yield return new("DocumentCollectionExtension.Open", f => f.Add(Fx.W, "Evil", il => il.Call(Fx.Core, Fx.App + "DocumentCollectionExtension", "Open", "obj", "obj", "string")), "R-COMMAND");
        yield return new("Document.CloseAndDiscard", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Core, Fx.App + "Document", "CloseAndDiscard", "void")), "R-COMMAND");
        yield return new("LockDocument", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Core, Fx.App + "Document", "LockDocument", "obj")), "R-COMMAND");
        yield return new("Editor.Command", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Core, "Autodesk.AutoCAD.EditorInput.Editor", "Command", "obj", "obj")), "R-COMMAND");
        yield return new("SendStringToExecute", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Core, Fx.App + "Document", "SendStringToExecute", "void", "string", "bool", "bool", "bool")), "R-COMMAND");
        // --- saving ---
        yield return new("SaveAs inside SideDbWriter", f => f.Add(Fx.W, "Evil", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target)
              .CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs in the adapter", f => f.Add(Fx.Adapter, "Evil", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target)
              .CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs in the RS core-like type", f => f.Add("Elsewhere", "Evil", il => il.LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs without the guard check", f => f.Replace(Fx.S, "Save", il =>
            il.CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs with the wrong guard method (AssertReadable)", f => f.Replace(Fx.S, "Save", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertReadable", "void", "cls:" + Fx.RsCore + "|" + Fx.Source)
              .CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs to a path that is not ScratchTarget.FullPath", f => f.Replace(Fx.S, "Save", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target)
              .LdArg0().LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs to ReadableSource.FullPath (the library path)", f => f.Replace(Fx.S, "Save", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target)
              .CallVirt(Fx.RsCore, Fx.Source, "get_FullPath", "string").LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs with a non-constant DwgVersion", f => f.Replace(Fx.S, "Save", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target)
              .CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdArg0().CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs(string, bool, DwgVersion, SecurityParameters) overload", f => f.Replace(Fx.S, "Save", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target)
              .CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(0).LdcI4(0).LdcI4(0)
              .CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", "bool", Fx.Dwg, "cls:" + Fx.Db + "|" + Fx.D + "SecurityParameters")), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs(string, SecurityParameters) overload", f => f.Replace(Fx.S, "Save", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target)
              .CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(0)
              .CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", "cls:" + Fx.Db + "|" + Fx.D + "SecurityParameters")), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs after a branch between the guard and the save", f => f.Replace(Fx.S, "Save", il =>
        {
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target);
            il.Op(System.Reflection.Metadata.ILOpCode.Br_s);   // br.s +0: a branch to the very next instruction
            il.Enc.CodeBuilder.WriteByte(0);
            il.CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg);
        }), "R-RS-SAVE-SCOPE");
        yield return new("SaveAs after a ret between the guard and the save", f => f.Replace(Fx.S, "Save", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target).Ret()
              .CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "SaveAs", "void", "string", Fx.Dwg)), "R-RS-SAVE-SCOPE");
        yield return new("Database.Save", f => f.Add(Fx.S, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "Save", "void")), "R-DB-SAVE");
        yield return new("Database.DxfOut", f => f.Add(Fx.S, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "DxfOut", "void", "string", "int", "int")), "R-DB-SAVE");
        yield return new("Database.WriteDwgFile-like (DwgOut)", f => f.Add(Fx.S, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "DwgOut", "void", "string")), "R-DB-SAVE");
        // --- cloning and inserting (Wblock / Insert / DeepClone onto the active document) ---
        yield return new("Wblock in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "Wblock", "cls:" + Fx.Db + "|" + Fx.DbT)), "R-DB-SAVE");
        yield return new("Wblock(string) in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "Wblock", "void", "string")), "R-DB-SAVE");
        yield return new("Database.Insert in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "Insert", "void")), "R-DB-SAVE");
        yield return new("Database.Insert in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "Insert", "void")), "R-DB-SAVE");
        yield return new("DeepCloneObjects in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "DeepCloneObjects", "void")), "R-DB-SAVE");
        yield return new("DeepCloneObjects in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "DeepCloneObjects", "void")), "R-DB-SAVE");
        yield return new("WblockCloneObjects in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "WblockCloneObjects", "void")), "R-DB-SAVE");
        yield return new("WblockCloneObjects in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "WblockCloneObjects", "void")), "R-DB-SAVE");
        yield return new("WblockCloneObjects in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "WblockCloneObjects", "void")), "R-DB-SAVE");
        yield return new("AttachXref in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "AttachXref", "void")), "R-DB-SAVE");
        yield return new("BindXrefs in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "BindXrefs", "void")), "R-DB-SAVE");
        yield return new("SwapIdWith in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "DBObject", "SwapIdWith", "void")), "R-DB-SAVE");
        // --- database construction ---
        yield return new("new Database(true,true) in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.LdcI4(1).LdcI4(1).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop()), "R-RS-DB-CTOR");
        yield return new("new Database(true,true) in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.LdcI4(1).LdcI4(1).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop()), "R-RS-DB-CTOR");
        yield return new("new Database(true,true) in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.LdcI4(1).LdcI4(1).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop()), "R-RS-DB-CTOR");
        yield return new("new Database() in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.NewObj(Fx.Db, Fx.DbT).Pop()), "R-RS-DB-CTOR");
        yield return new("new Database(false,false) in SideDbWriter (attached to a document)", f => f.Add(Fx.W, "Evil", il => il.LdcI4(0).LdcI4(0).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop()), "R-RS-DB-CTOR");
        yield return new("new Database(true,false) in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.LdcI4(1).LdcI4(0).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop()), "R-RS-DB-CTOR");
        yield return new("new Database(non-constant, true) in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.LdArg0().LdcI4(1).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop()), "R-RS-DB-CTOR");
        yield return new("new Database(true, non-constant) in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.LdcI4(1).LdArg0().NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop()), "R-RS-DB-CTOR");
        yield return new("new Database(IntPtr, bool) in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.LdArg0().LdcI4(1).NewObj(Fx.Db, Fx.DbT, "intptr", "bool").Pop()), "R-RS-DB-CTOR");
        // --- reading files ---
        yield return new("ReadDwgFile in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "ReadDwgFile", "void", "string", Fx.Fs, "bool", "string")), "R-RS-READDWG-SCOPE");
        yield return new("ReadDwgFile in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "ReadDwgFile", "void", "string", Fx.Fs, "bool", "string")), "R-RS-READDWG-SCOPE");
        yield return new("ReadDwgFile in SideDbWriter without the guard", f => f.Replace(Fx.W, "Open", il =>
            il.LdcI4(0).LdcI4(1).NewObj(Fx.Db, Fx.DbT, "bool", "bool").Pop()
              .CallVirt(Fx.RsCore, Fx.Source, "get_FullPath", "string").LdcI4(1).LdcI4(1).LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "ReadDwgFile", "void", "string", Fx.Fs, "bool", "string")), "R-RS-READDWG-SCOPE");
        yield return new("ReadDwgFile with the wrong guard method (AssertCreatable)", f => f.Replace(Fx.W, "Open", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertCreatable", "void", "cls:" + Fx.RsCore + "|" + Fx.Target)
              .CallVirt(Fx.RsCore, Fx.Source, "get_FullPath", "string").LdcI4(1).LdcI4(1).LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "ReadDwgFile", "void", "string", Fx.Fs, "bool", "string")), "R-RS-READDWG-SCOPE");
        yield return new("ReadDwgFile of a path that is not ReadableSource.FullPath", f => f.Replace(Fx.W, "Open", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertReadable", "void", "cls:" + Fx.RsCore + "|" + Fx.Source)
              .LdArg0().LdcI4(1).LdcI4(1).LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "ReadDwgFile", "void", "string", Fx.Fs, "bool", "string")), "R-RS-READDWG-SCOPE");
        yield return new("ReadDwgFile of ScratchTarget.FullPath", f => f.Replace(Fx.W, "Open", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertReadable", "void", "cls:" + Fx.RsCore + "|" + Fx.Source)
              .CallVirt(Fx.RsCore, Fx.Target, "get_FullPath", "string").LdcI4(1).LdcI4(1).LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "ReadDwgFile", "void", "string", Fx.Fs, "bool", "string")), "R-RS-READDWG-SCOPE");
        yield return new("ReadDwgFile(IntPtr, bool, string)", f => f.Replace(Fx.W, "Open", il =>
            il.CallVirt(Fx.RsCore, Fx.Guard, "AssertReadable", "void", "cls:" + Fx.RsCore + "|" + Fx.Source)
              .LdArg0().LdcI4(1).LdcI4(0).CallVirt(Fx.Db, Fx.DbT, "ReadDwgFile", "void", "intptr", "bool", "string")), "R-RS-READDWG-SCOPE");
        // --- open modes, commit, creation, setters outside SideDbWriter ---
        yield return new("ForNotify in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.LdcI4(2).CallVirt(Fx.Db, Fx.Tr, "GetObject", "obj", Fx.Oid, Fx.Om)), "R-WRITE-OPEN");
        yield return new("OpenMode 5 in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.LdcI4(5).CallVirt(Fx.Db, Fx.Tr, "GetObject", "obj", Fx.Oid, Fx.Om)), "R-WRITE-OPEN");
        yield return new("UpgradeOpen in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "DBObject", "UpgradeOpen", "void")), "R-WRITE-OPEN");
        yield return new("ForWrite in SideDbReader", f => f.Replace(Fx.R, "Read", il => il.LdcI4(1).CallVirt(Fx.Db, Fx.Tr, "GetObject", "obj", Fx.Oid, Fx.Om)), "R-WRITE-OPEN");
        yield return new("ForWrite in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.LdcI4(1).CallVirt(Fx.Db, Fx.Tr, "GetObject", "obj", Fx.Oid, Fx.Om)), "R-WRITE-OPEN");
        yield return new("ForWrite in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.LdcI4(1).CallVirt(Fx.Db, Fx.Tr, "GetObject", "obj", Fx.Oid, Fx.Om)), "R-WRITE-OPEN");
        yield return new("non-constant OpenMode in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.LdArg0().CallVirt(Fx.Db, Fx.Tr, "GetObject", "obj", Fx.Oid, Fx.Om)), "R-OPENMODE-UNRESOLVABLE");
        yield return new("non-constant OpenMode in SideDbReader", f => f.Replace(Fx.R, "Read", il => il.LdArg0().CallVirt(Fx.Db, Fx.Tr, "GetObject", "obj", Fx.Oid, Fx.Om)), "R-OPENMODE-UNRESOLVABLE");
        yield return new("Commit in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.CallVirt(Fx.Db, Fx.Tr, "Commit", "void")), "R-COMMIT");
        yield return new("Commit in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.CallVirt(Fx.Db, Fx.Tr, "Commit", "void")), "R-COMMIT");
        yield return new("Commit in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.Tr, "Commit", "void")), "R-COMMIT");
        yield return new("Commit of an OpenCloseTransaction in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "OpenCloseTransaction", "Commit", "void")), "R-COMMIT");
        yield return new("StartTransaction in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "TransactionManager", "StartTransaction", "cls:" + Fx.Db + "|" + Fx.Tr)), "R-TRANSACTION-SCOPE");
        yield return new("StartOpenCloseTransaction in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "TransactionManager", "StartOpenCloseTransaction", "obj")), "R-TRANSACTION-SCOPE");
        yield return new("AppendEntity in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "BlockTableRecord", "AppendEntity", "obj", "obj")), "R-AUTODESK-MUTATOR-NAME");
        yield return new("AppendEntity in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "BlockTableRecord", "AppendEntity", "obj", "obj")), "R-AUTODESK-MUTATOR-NAME");
        yield return new("AppendEntity in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "BlockTableRecord", "AppendEntity", "obj", "obj")), "R-AUTODESK-MUTATOR-NAME");
        yield return new("set_ScaleFactors in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "BlockReference", "set_ScaleFactors", "void", "obj")), "R-AUTODESK-SETTER");
        yield return new("set_Name in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Db, Fx.D + "SymbolTableRecord", "set_Name", "void", "string")), "R-AUTODESK-SETTER");
        yield return new("new BlockReference in SideDbReader", f => f.Add(Fx.R, "Evil", il => il.NewObj(Fx.Db, Fx.D + "BlockReference").Pop()), "R-NEWOBJ-OBJECT");
        yield return new("new Line in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.NewObj(Fx.Db, Fx.D + "Line").Pop()), "R-NEWOBJ-OBJECT");
        yield return new("new RotatedDimension in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.NewObj(Fx.Db, Fx.D + "RotatedDimension").Pop()), "R-NEWOBJ-OBJECT");
        // --- host state and commands ---
        yield return new("SetSystemVariable in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.Call(Fx.Core, App_Core, "SetSystemVariable", "void", "string", "obj")), "R-SYSVAR-SET");
        yield return new("SetSystemVariable in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.Call(Fx.Core, App_Core, "SetSystemVariable", "void", "string", "obj")), "R-SYSVAR-SET");
        yield return new("Overrule.AddOverrule", f => f.Add(Fx.W, "Evil", il => il.Call(Fx.Db, "Autodesk.AutoCAD.Runtime.Overrule", "AddOverrule", "void", "obj", "bool")), "R-OVERRULE");
        yield return new("DynamicLinker.LoadModule", f => f.Add(Fx.W, "Evil", il => il.Call(Fx.Db, "Autodesk.AutoCAD.Runtime.DynamicLinker", "LoadModule", "bool", "string", "bool", "bool")), "R-ASSEMBLY-LOAD");
        yield return new("event subscription", f => f.Add(Fx.W, "Evil", il => il.CallVirt(Fx.Db, Fx.DbT, "add_ObjectAppended", "void", "obj")), "R-EVENT");
        // --- files and processes ---
        foreach (var (name, type, member, ret, ps) in new (string, string, string, string, string[])[]
        {
            ("File.Copy over the library", "System.IO.File", "Copy", "void", P("string", "string", "bool")),
            ("File.Move", "System.IO.File", "Move", "void", P("string", "string")),
            ("File.Delete", "System.IO.File", "Delete", "void", P("string")),
            ("File.WriteAllBytes", "System.IO.File", "WriteAllBytes", "void", P("string", "obj")),
            ("File.WriteAllText", "System.IO.File", "WriteAllText", "void", P("string", "string")),
            ("File.AppendAllText", "System.IO.File", "AppendAllText", "void", P("string", "string")),
            ("File.Replace", "System.IO.File", "Replace", "void", P("string", "string", "string")),
            ("File.Create", "System.IO.File", "Create", "obj", P("string")),
            ("File.OpenWrite", "System.IO.File", "OpenWrite", "obj", P("string")),
            ("File.SetAttributes", "System.IO.File", "SetAttributes", "void", P("string", "int")),
            ("File.Encrypt", "System.IO.File", "Encrypt", "void", P("string")),
            ("Directory.Delete", "System.IO.Directory", "Delete", "void", P("string", "bool")),
            ("Directory.CreateDirectory", "System.IO.Directory", "CreateDirectory", "obj", P("string")),
            ("Directory.Move", "System.IO.Directory", "Move", "void", P("string", "string")),
            ("Path.GetTempFileName", "System.IO.Path", "GetTempFileName", "string", P()),
        })
        {
            foreach (var host in new[] { Fx.W, Fx.S, Fx.R, Fx.Adapter })
                yield return new(name + " in " + host, f => f.Add(host, "Evil", il => il.Call(Fx.Rt, type, member, ret, ps)), "R-FILE-WRITE");
        }
        yield return new("new FileStream in ScratchSaver", f => f.Add(Fx.S, "Evil", il => il.NewObj(Fx.Rt, "System.IO.FileStream", "string", "int", "int", "int").Pop()), "R-FILE-WRITE");
        yield return new("new StreamWriter in SideDbWriter", f => f.Add(Fx.W, "Evil", il => il.NewObj(Fx.Rt, "System.IO.StreamWriter", "string").Pop()), "R-FILE-WRITE");
        yield return new("new BinaryWriter in the adapter", f => f.Add(Fx.Adapter, "Evil", il => il.NewObj(Fx.Rt, "System.IO.BinaryWriter", "obj").Pop()), "R-FILE-WRITE");
        yield return new("Process.Start", f => f.Add(Fx.W, "Evil", il => il.Call(Fx.Rt, "System.Diagnostics.Process", "Start", "obj", "string")), "R-PROCESS");
        yield return new("Process.Kill", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Rt, "System.Diagnostics.Process", "Kill", "void")), "R-PROCESS");
        yield return new("Process.GetProcessesByName", f => f.Add(Fx.Adapter, "Evil", il => il.Call(Fx.Rt, "System.Diagnostics.Process", "GetProcessesByName", "obj", "string")), "R-PROCESS");
        yield return new("new ProcessStartInfo", f => f.Add(Fx.Adapter, "Evil", il => il.NewObj(Fx.Rt, "System.Diagnostics.ProcessStartInfo", "string").Pop()), "R-PROCESS");
        yield return new("Assembly.LoadFrom", f => f.Add(Fx.Adapter, "Evil", il => il.Call(Fx.Rt, "System.Reflection.Assembly", "LoadFrom", "obj", "string")), "R-ASSEMBLY-LOAD");
        yield return new("Assembly.Load", f => f.Add(Fx.Adapter, "Evil", il => il.Call(Fx.Rt, "System.Reflection.Assembly", "Load", "obj", "string")), "R-ASSEMBLY-LOAD");
        yield return new("Activator.CreateInstance", f => f.Add(Fx.Adapter, "Evil", il => il.Call(Fx.Rt, "System.Activator", "CreateInstance", "obj", "obj")), "R-REFLECTION-INVOKE");
        yield return new("MethodInfo.Invoke", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Rt, "System.Reflection.MethodInfo", "Invoke", "obj", "obj", "obj")), "R-REFLECTION-INVOKE");
        yield return new("Registry.SetValue", f => f.Add(Fx.Adapter, "Evil", il => il.Call(Fx.Rt, "Microsoft.Win32.Registry", "SetValue", "void", "string", "string", "obj")), "R-REGISTRY");
        yield return new("Environment.SetEnvironmentVariable", f => f.Add(Fx.Adapter, "Evil", il => il.Call(Fx.Rt, "System.Environment", "SetEnvironmentVariable", "void", "string", "string")), "R-ENVIRONMENT");
        yield return new("HttpClient", f => f.Add(Fx.Adapter, "Evil", il => il.CallVirt(Fx.Rt, "System.Net.Http.HttpClient", "GetStringAsync", "obj", "string")), "R-NETWORK");
        yield return new("Marshal.Copy", f => f.Add(Fx.Adapter, "Evil", il => il.Call(Fx.Rt, "System.Runtime.InteropServices.Marshal", "Copy", "void", "obj", "int", "intptr", "int")), "R-PINVOKE");
        yield return new("calli", f => f.Add(Fx.Adapter, "Evil", il => il.Calli()), "R-CALLI");
        yield return new("localloc", f => f.Add(Fx.Adapter, "Evil", il => il.LdcI4(8).Op(System.Reflection.Metadata.ILOpCode.Localloc).Pop()), "R-RAW-MEMORY");
        yield return new("stind.i4", f => f.Add(Fx.Adapter, "Evil", il => il.LdArg0().LdcI4(1).Op(System.Reflection.Metadata.ILOpCode.Stind_i4)), "R-RAW-STORE");
    }

    private const string App_Core = "Autodesk.AutoCAD.ApplicationServices.Core.Application";

    public static IEnumerable<object[]> CaseIndexes()
    {
        var i = 0;
        foreach (var c in Cases()) yield return new object[] { i++, c.Name };
    }

    private static readonly List<Case> All = Cases().ToList();

    [Fact]
    public void There_are_at_least_120_negative_controls() => Assert.True(All.Count >= 120, "cases: " + All.Count);

    [Theory]
    [MemberData(nameof(CaseIndexes))]
    public void Negative_control_with_every_import_approved_the_deny_rules_and_the_RS_layer_still_fail(int index, string name)
    {
        var c = All[index];
        Assert.Equal(name, c.Name);
        var image = c.Build(new Fx()).Build();
        var rules = Rules(image, Fx.Permissive(image), Fx.PolicyOf(new Fx().Build()));
        Assert.NotEmpty(rules);
        Assert.Contains(c.Rule, rules);
    }

    [Theory]
    [MemberData(nameof(CaseIndexes))]
    public void Negative_control_with_the_real_allowlist_the_scan_fails_too(int index, string name)
    {
        var c = All[index];
        Assert.Equal(name, c.Name);
        var image = c.Build(new Fx()).Build();
        var rules = Rules(image, Real(), Fx.PolicyOf(new Fx().Build()));
        Assert.NotEmpty(rules);
    }

    [Theory]
    [MemberData(nameof(CaseIndexes))]
    public void Negative_control_the_violation_is_not_waived_away(int index, string name)
    {
        var c = All[index];
        Assert.Equal(name, c.Name);
        var image = c.Build(new Fx()).Build();
        var result = RsScan.ScanOne(image, "fixture.dll", ScanRole.R0, Fx.Permissive(image), FreshWaivers(), Fx.PolicyOf(new Fx().Build()));
        Assert.NotEmpty(result.Violations);
    }
}
