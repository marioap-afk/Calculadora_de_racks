using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

namespace I52Ct21d.HostFacts.Tools.Scan;

public enum ScanRole
{
    /// <summary>The pure library: no AutoCAD reference of any kind, no file write outside the EvidenceWriter.</summary>
    Core,

    /// <summary>The offline tools: same as Core.</summary>
    Tools,

    /// <summary>The R0 plugin DLL (STRICT READ-ONLY, design 5.1).</summary>
    R0,
}

/// <summary>
/// Which types hold the two privileges of the design (5.3 item 3): file writes, and the side-database operations. A privilege belongs to a
/// top-level type DEFINED IN a named assembly (<see cref="CoreAssemblyName"/> for the writer, <see cref="SideDbAssemblyName"/> for the
/// side-database reader): the same type name in another assembly gets nothing and is itself a violation (redefinition).
/// </summary>
public sealed record ScanConfig(ScanRole Role, string EvidenceWriterType, string SideDbType, string CoreAssemblyName, string SideDbAssemblyName = "I52Ct21d.HostFacts.R0")
{
    /// <summary>
    /// The approved imports (design 5.3 item 2). When set, the scan is DENY-BY-DEFAULT: every assembly, type and member reference of the
    /// assembly must be listed in it. When null only the deny rules (second layer) run; the CLI never runs without it.
    /// </summary>
    public ApiAllowlist? Allowlist { get; init; }

    /// <summary>The single EvidenceSealer (a privileged type like the writer: its surface and its callers are pinned).</summary>
    public string EvidenceSealerType { get; init; } = "I52Ct21d.HostFacts.Core.EvidenceSealer";

    /// <summary>
    /// The pinned privileged surface (layer 3): <c>privileged-surface.txt</c>, <c>privileged-callers.txt</c>, <c>expected-commands.txt</c>. When null
    /// only layers 1 and 2 run; the CLI never runs without it.
    /// </summary>
    public ScanPolicy? Policy { get; init; }

    public static ScanConfig For(ScanRole role) => new(
        role, "I52Ct21d.HostFacts.Core.EvidenceWriter", "I52Ct21d.HostFacts.R0.AcadSideDbReader", "I52Ct21d.HostFacts.Core");

    public static ScanConfig For(ScanRole role, ApiAllowlist? allowlist) => For(role) with { Allowlist = allowlist };

    public static ScanConfig For(ScanRole role, ApiAllowlist? allowlist, ScanPolicy? policy) => For(role) with { Allowlist = allowlist, Policy = policy };
}

public sealed record Violation(string Assembly, string Type, string Method, int IlOffset, string Rule, string Detail)
{
    public override string ToString() => $"{Rule} {Assembly} {Type}::{Method} @IL_{IlOffset:X4}: {Detail}";
}

/// <summary>What the scan examined (so a CLEAN verdict can be shown not to be vacuous) and what it found.</summary>
public sealed record ScanOutcome(IReadOnlyList<Violation> Violations, int ExternalCallSites, int AutodeskCallSites, int OpenModeCallSites, int MethodBodies, int ImportRowsChecked = 0)
{
    /// <summary>The privileged surface, callers and command registrations the scan observed (empty unless the assembly has any).</summary>
    public PrivilegedObservations Observed { get; init; } = new(new Dictionary<string, IReadOnlyList<string>>(), Array.Empty<string>(), Array.Empty<string>());
}

/// <summary>
/// The FORBIDDEN-API SEMANTIC SCAN (design 5.3 item 1; decisions section 239 condition 1). It reads the METADATA and the IL of an
/// assembly (reflection over IL, <c>System.Reflection.Metadata</c>): it resolves every member reference, type reference and base type,
/// and, for a call that takes an <c>OpenMode</c>, it resolves the CONSTANT operand with a compiler-independent walk over the IL, so
/// <c>OpenMode.ForWrite</c>, <c>(OpenMode)1</c> and a named constant are all seen as a write, and an <c>OpenMode</c> that is not a
/// compile-time constant is rejected as unresolvable (design 5.3 item 1).
///
/// It is a REVIEW AID with negative controls (the tests build assemblies that call each forbidden API and require the scan to fail); it
/// does not replace the independent reviewer of decisions section 239 condition 1. The mutator-name rule is a deliberately conservative
/// HEURISTIC over the AutoCAD namespaces: a false positive is resolved by the reviewer, a false negative is the risk that the reviewer and
/// the host-run checks (hashes, DBMOD, evidence folder content) are there to catch (design 5.3 item 4).
/// </summary>
public static partial class ForbiddenApiScan
{
    private const string Db = "Autodesk.AutoCAD.DatabaseServices.Database";
    private const string OpenModeType = "Autodesk.AutoCAD.DatabaseServices.OpenMode";
    private const string DbNs = "Autodesk.AutoCAD.DatabaseServices.";

    private static readonly string[] AutoCadAssemblies = { "AcDbMgd", "AcMgd", "AcCoreMgd", "AcCui", "AcWindows", "AdWindows", "AcTcMgd" };

    private static readonly HashSet<string> DbSaveMembers = new(StringComparer.Ordinal)
    {
        "Save", "SaveAs", "Wblock", "WblockCloneObjects", "DeepCloneObjects", "Insert", "AuditDatabase", "Purge", "RestoreForwardingXrefs",
        "DxfOut", "DxfIn", "DwgOut", "AttachXref", "DetachXref", "BindXrefs", "ResolveXrefs", "ReloadXrefs", "UnloadXrefs",
        "RestoreOriginalXrefSymbols", "ForceWblockDatabaseCopy",
    };

    /// <summary>Members that save, convert or restructure a database or object whatever AutoCAD type declares them (R-DB-SAVE).</summary>
    private static readonly HashSet<string> AnyAutodeskTypeDenyNames = new(StringComparer.Ordinal)
    {
        "DxfOut", "DxfIn", "DwgOut", "AttachXref", "DetachXref", "BindXrefs", "ResolveXrefs", "ReloadXrefs", "UnloadXrefs", "SwapIdWith",
        "WriteDwgFile", "SaveAs", "Wblock", "WblockCloneObjects", "DeepCloneObjects",
    };

    private static readonly string[] MutatorPrefixes =
    {
        "Add", "Append", "Erase", "Remove", "Delete", "Clear", "Transform", "Copy", "Clone", "DeepClone", "Save", "Insert", "Purge",
        "Explode", "Join", "Mirror", "Offset", "Set", "Upgrade", "Wblock", "Create", "Update", "Reverse", "Scale", "Rotate", "Trim", "Extend",
        "Break", "Fillet", "Chamfer", "Apply", "Commit", "Rename", "Replace",
        "Write", "Load", "Unload", "Swap", "Bind", "Detach", "Attach", "Dxf", "Dwg", "Regen", "Highlight", "Unhighlight", "Resolve", "Reload",
        "Import", "Export", "Plot", "Publish", "Flush", "Zoom", "Pan", "Draw", "Move", "Stretch", "Unlock", "Lock", "Register", "Unregister",
        "Invoke", "Execute", "Run", "Send", "Post", "Terminate", "Quit", "Exit", "Kill", "Start", "Begin", "End", "Cancel",
    };

    /// <summary>Autodesk members whose name looks like a mutator but that are judged by another rule or only print to the command line (section 239, R-I).</summary>
    private static readonly HashSet<string> MutatorNameExceptions = new(StringComparer.Ordinal)
    {
        "Autodesk.AutoCAD.EditorInput.Editor::WriteMessage",
        // the one transaction pair of the side-database reader is judged by R-TRANSACTION-SCOPE, not by name
        "Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartOpenCloseTransaction",
        "Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction",
    };

    private static readonly string[] BlockedFileSinkNamespaces =
    {
        "System.Xml", "System.IO.Compression", "System.IO.MemoryMappedFiles", "System.IO.Pipes", "System.IO.Ports", "System.Data", "System.Printing",
    };

    private static readonly string[] DynamicCodeNamespaces =
    {
        "System.Linq.Expressions", "Microsoft.CSharp.RuntimeBinder", "System.Dynamic", "System.CodeDom", "Microsoft.CodeAnalysis",
    };

    /// <summary>Types that hold process-wide or thread-wide state: any setter or mutator of theirs is refused (R-GLOBAL-STATE).</summary>
    private static readonly HashSet<string> GlobalStateTypes = new(StringComparer.Ordinal)
    {
        "System.Globalization.CultureInfo", "System.Console", "System.Environment", "System.AppDomain",
        "System.Threading.Thread", "System.Threading.ThreadPool", "System.AppContext", "System.GC", "System.Runtime.GCSettings",
        "System.Diagnostics.Trace", "System.Diagnostics.Debug", "System.Diagnostics.Debugger", "System.TimeZoneInfo",
        "System.Security.Principal.WindowsIdentity",
    };

    private static readonly string[] GlobalStateMutatorPrefixes =
    {
        "set_", "Set", "Add", "Remove", "Register", "Unregister", "Reset", "Clear", "Close", "Open", "Load", "Unload", "Attach", "Break", "Launch", "Log",
        "Collect", "Suppress", "Exit", "FailFast", "Write", "Redirect", "Impersonate", "Switch",
    };

    private static readonly HashSet<string> AllowedAutodeskCtors = new(StringComparer.Ordinal)
    {
        DbNs + "TypedValue", DbNs + "ObjectId", DbNs + "Handle",
    };

    private static readonly string[] ReflectionBuilderTypes =
    {
        "AssemblyBuilder", "ModuleBuilder", "TypeBuilder", "ILGenerator", "DynamicMethod", "MethodBuilder", "ConstructorBuilder",
        "FieldBuilder", "PropertyBuilder", "EnumBuilder",
    };

    private static readonly string[] DocumentManagerPrefixes = { "Open", "Close", "Add", "Recover", "Create", "Remove", "Load" };

    /// <summary>Types that start or schedule concurrent work (a thread, a timer, a task): their constructors and starters are refused (R-THREAD).</summary>
    private static readonly HashSet<string> ConcurrencyTypes = new(StringComparer.Ordinal)
    {
        "System.Threading.Thread", "System.Threading.ThreadPool", "System.Threading.Timer", "System.Threading.PeriodicTimer", "System.Timers.Timer",
        "System.Threading.Tasks.Task", "System.Threading.Tasks.TaskFactory", "System.Threading.Tasks.Parallel", "System.Threading.ThreadPoolBoundHandle",
    };

    private static readonly string[] ConcurrencyStarterPrefixes =
        { ".ctor", "Start", "Run", "Queue", "UnsafeQueue", "ContinueWith", "FromAsync", "Delay", "Change", "For", "Invoke", "Register", "UnsafeRegister", "Schedule", "set_" };

    private static readonly string[] ReadOnlyIoPrefixes = { "Read", "Exists", "Get", "get_", "Enumerate", "OpenRead", "OpenText" };

    public static IReadOnlyList<Violation> ScanFile(string path, ScanConfig cfg) =>
        Scan(File.ReadAllBytes(path), Path.GetFileName(path), cfg);

    public static IReadOnlyList<Violation> Scan(byte[] image, string displayName, ScanConfig cfg) =>
        ScanDetailed(image, displayName, cfg).Violations;

    public static ScanOutcome ScanDetailed(byte[] image, string displayName, ScanConfig cfg)
    {
        var counters = new Counters();
        using var pe = new PEReader(ImmutableArray.Create(image));
        var reader = pe.GetMetadataReader();
        var asmName = reader.GetString(reader.GetAssemblyDefinition().Name);
        var sink = new List<Violation>();
        var names = new NameProvider(reader);

        // --- metadata level rules: assembly references, type references, type definitions ----------------------------------
        foreach (var h in reader.AssemblyReferences)
        {
            var an = reader.GetString(reader.GetAssemblyReference(h).Name);
            if (cfg.Role != ScanRole.R0 && AutoCadAssemblies.Contains(an, StringComparer.OrdinalIgnoreCase))
                sink.Add(new Violation(displayName, "<metadata>", "", -1, "R-AUTODESK-IN-NONHOST", "assembly reference " + an));
        }
        foreach (var h in reader.TypeReferences)
        {
            var full = names.Full(h);
            var violation = TypeRule(full, cfg);
            if (violation is not null) sink.Add(new Violation(displayName, "<metadata>", "", -1, violation, "type reference " + full));
        }
        foreach (var th in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(th);
            var full = names.Full(th);
            if (full == cfg.EvidenceWriterType && asmName != cfg.CoreAssemblyName)
                sink.Add(new Violation(displayName, full, "", -1, "R-WRITER-REDEFINED", "the single EvidenceWriter may be defined only in " + cfg.CoreAssemblyName));
            if (full == cfg.SideDbType && (cfg.Role != ScanRole.R0 || asmName != cfg.SideDbAssemblyName))
                sink.Add(new Violation(displayName, full, "", -1, "R-SIDEDB-REDEFINED", "the side-database reader may be defined only in " + cfg.SideDbAssemblyName));
            if (full == "System.Security.UnverifiableCodeAttribute")
                sink.Add(new Violation(displayName, full, "", -1, "R-UNSAFE-CODE", "the assembly defines UnverifiableCodeAttribute"));
            if (!td.BaseType.IsNil)
            {
                var b = names.Full(td.BaseType);
                var v = TypeRule(b, cfg);
                if (v is not null) sink.Add(new Violation(displayName, full, "", -1, v, "base type " + b));
            }
        }

        // --- layer 3: command registrations and the pinned surface of the privileged types -------------------------------------
        var observedCallers = new List<string>();
        var observedCommands = new List<string>();
        CollectCommands(reader, names, asmName, displayName, cfg, observedCommands, sink);
        var observedSurface = CollectSurface(reader, names, asmName, cfg);
        CheckSurface(observedSurface, asmName, displayName, cfg, sink);

        // --- unsafe code, native code, pointer types (independent of the allowlist) -----------------------------------------------
        CheckUnsafeMetadata(pe, reader, names, displayName, sink);

        // --- member level rules: every method body ----------------------------------------------------------------------------
        foreach (var th in reader.TypeDefinitions)
        {
            var typeName = names.Full(th);
            var top = typeName.Split('/')[0];
            foreach (var mh in reader.GetTypeDefinition(th).GetMethods())
            {
                var m = reader.GetMethodDefinition(mh);
                var methodName = reader.GetString(m.Name);
                if ((m.Attributes & MethodAttributes.PinvokeImpl) != 0)
                    sink.Add(new Violation(displayName, typeName, methodName, -1, "R-PINVOKE", "DllImport / P/Invoke declaration"));
                if (m.RelativeVirtualAddress == 0) continue;
                if ((m.ImplAttributes & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL) continue; // refused by CheckUnsafeMetadata (R-NATIVE-METHOD)
                var il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILBytes();
                if (il is null) continue;
                counters.MethodBodies++;
                var code = IlReader.Decode(il);
                var targets = IlReader.BranchTargets(code);
                var approvedStores = new List<int>();
                for (var i = 0; i < code.Count; i++)
                {
                    var ins = code[i];
                    if (ins.Op == System.Reflection.Emit.OpCodes.Calli)
                    {
                        sink.Add(new Violation(displayName, typeName, methodName, ins.Offset, "R-CALLI", "calli (indirect call through a function pointer) is never allowed"));
                        continue;
                    }
                    if (ins.Op == System.Reflection.Emit.OpCodes.Jmp)
                    {
                        sink.Add(new Violation(displayName, typeName, methodName, ins.Offset, "R-JMP", "jmp is never allowed"));
                        continue;
                    }
                    if (CheckRawMemoryOpcode(ins, asmName, top, displayName, typeName, methodName, cfg, sink)) approvedStores.Add(ins.Offset);
                    if (ins.Op == System.Reflection.Emit.OpCodes.Stsfld || ins.Op == System.Reflection.Emit.OpCodes.Stfld)
                    {
                        var field = names.Resolve(MetadataTokens.EntityHandle((int)ins.Operand));
                        if (field is not null && field.Params is null)
                            sink.Add(new Violation(displayName, typeName, methodName, ins.Offset, "R-EXTERNAL-FIELD-STORE", StripGenerics(field.DeclType) + "::" + field.Name + " (store to an external field)"));
                    }
                    if (cfg.Allowlist is not null) CheckImportsAtInstruction(ins, names, displayName, asmName, typeName, methodName, top, cfg.Allowlist, sink);
                    ObserveCaller(ins, reader, names, displayName, asmName, typeName, methodName, top, cfg, observedCallers, sink);
                    if (!IlReader.IsCall(ins)) continue;
                    var target = names.Resolve(MetadataTokens.EntityHandle((int)ins.Operand));
                    if (target is null) continue;
                    if (target.Params is not null && target.Params.Any(HasPointer))
                        sink.Add(new Violation(displayName, typeName, methodName, ins.Offset, "R-POINTER-TYPE",
                            StripGenerics(target.DeclType) + "::" + target.Name + " takes a pointer or function-pointer parameter"));
                    counters.ExternalCallSites++;
                    if (StripGenerics(target.DeclType).StartsWith("Autodesk.", StringComparison.Ordinal)) counters.AutodeskCallSites++;
                    if (target.Params is not null && target.Params.Contains(OpenModeType)) counters.OpenModeCallSites++;
                    var ctx = new Site(displayName, asmName, typeName, methodName, top, code, i, targets, ins.Op == System.Reflection.Emit.OpCodes.Newobj, names);
                    CheckCall(target, ctx, cfg, sink);
                }
                if (approvedStores.Count > 0) CheckApprovedStores(code, approvedStores, NativeSignatureText(pe, reader, names, m), displayName, typeName, methodName, sink);
            }
        }
        if (cfg.Allowlist is not null) counters.ImportRows = CheckImportsMetadata(reader, names, cfg.Allowlist, displayName, sink);
        var ordered = sink.OrderBy(v => v.Type, StringComparer.Ordinal).ThenBy(v => v.Method, StringComparer.Ordinal)
            .ThenBy(v => v.IlOffset).ThenBy(v => v.Rule, StringComparer.Ordinal).ToList();
        return new ScanOutcome(ordered, counters.ExternalCallSites, counters.AutodeskCallSites, counters.OpenModeCallSites, counters.MethodBodies, counters.ImportRows)
        {
            Observed = new PrivilegedObservations(observedSurface, observedCallers.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList(),
                observedCommands.OrderBy(x => x, StringComparer.Ordinal).ToList()),
        };
    }

    private sealed class Counters
    {
        public int ExternalCallSites;
        public int AutodeskCallSites;
        public int OpenModeCallSites;
        public int MethodBodies;
        public int ImportRows;
    }

    // -----------------------------------------------------------------------------------------------------------------------
    private sealed record Site(string Assembly, string AsmName, string Type, string Method, string TopType, IReadOnlyList<Instr> Code, int Index, HashSet<int> Targets, bool IsNewobj, NameProvider? Names = null);

    private static string? TypeRule(string full, ScanConfig cfg)
    {
        var t = StripGenerics(full);
        if (t.StartsWith("Autodesk.", StringComparison.Ordinal))
        {
            if (cfg.Role != ScanRole.R0) return "R-AUTODESK-IN-NONHOST";
            if (t.StartsWith("Autodesk.AutoCAD.Internal", StringComparison.Ordinal) || t.StartsWith("Autodesk.AutoCAD.Interop", StringComparison.Ordinal))
                return "R-AUTODESK-INTERNAL-OR-COM";
            if (LastSegment(t).EndsWith("Overrule", StringComparison.Ordinal)) return "R-OVERRULE";
            if (LastSegment(t) == "DocumentLock") return "R-COMMAND";
            if (LastSegment(t) == "DynamicLinker") return "R-ASSEMBLY-LOAD";
            return null;
        }
        foreach (var ns in BlockedFileSinkNamespaces)
            if (t == ns || t.StartsWith(ns + ".", StringComparison.Ordinal)) return "R-FILE-WRITE";
        if (t == "System.IO.FileSystemWatcher") return "R-FILE-WRITE";
        foreach (var ns in DynamicCodeNamespaces)
            if (t == ns || t.StartsWith(ns + ".", StringComparison.Ordinal)) return "R-DYNAMIC-CODE";
        if (t.StartsWith("System.Runtime.CompilerServices.CallSite", StringComparison.Ordinal)) return "R-DYNAMIC-CODE";
        if (t.StartsWith("System.Net.", StringComparison.Ordinal)) return "R-NETWORK";
        if (t == "System.Diagnostics.ProcessStartInfo") return "R-PROCESS";
        if (t == "System.Runtime.InteropServices.NativeLibrary" || t == "System.Runtime.InteropServices.Marshal") return "R-PINVOKE";
        if (t is "System.Runtime.CompilerServices.Unsafe" or "System.Runtime.InteropServices.GCHandle" or "System.Runtime.InteropServices.NativeMemory"
            or "System.Runtime.InteropServices.MemoryMarshal") return "R-UNSAFE-API";
        if (t == "System.Security.UnverifiableCodeAttribute") return "R-UNSAFE-CODE";
        if (t == "System.Runtime.Loader.AssemblyLoadContext") return "R-ASSEMBLY-LOAD";
        if (t.StartsWith("System.Reflection.Emit.", StringComparison.Ordinal) && ReflectionBuilderTypes.Contains(LastSegment(t))) return "R-DYNAMIC-CODE";
        return null;
    }

    private static void CheckCall(Target t, Site s, ScanConfig cfg, List<Violation> sink)
    {
        var dt = StripGenerics(t.DeclType);
        var n = t.Name;
        var inSideDb = s.TopType == cfg.SideDbType && s.AsmName == cfg.SideDbAssemblyName;
        var inWriter = s.TopType == cfg.EvidenceWriterType && s.AsmName == cfg.CoreAssemblyName;
        void Add(string rule, string detail) => sink.Add(new Violation(s.Assembly, s.Type, s.Method, s.Code[s.Index].Offset, rule, dt + "::" + n + (detail.Length > 0 ? " (" + detail + ")" : "")));

        if (dt.StartsWith("Autodesk.", StringComparison.Ordinal))
        {
            if (cfg.Role != ScanRole.R0) { Add("R-AUTODESK-IN-NONHOST", ""); return; }
            var last = LastSegment(dt);
            if (last.EndsWith("Overrule", StringComparison.Ordinal)) Add("R-OVERRULE", "");
            if (n is "SetSystemVariable" or "TrySetSystemVariable") Add("R-SYSVAR-SET", "");
            if (dt == Db && DbSaveMembers.Contains(n)) Add("R-DB-SAVE", "");
            else if (AnyAutodeskTypeDenyNames.Contains(n)) Add("R-DB-SAVE", "database / object save, conversion or restructuring");
            if (last == "DynamicLinker") Add("R-ASSEMBLY-LOAD", "native or managed module load");
            if (n == "UpgradeOpen") Add("R-WRITE-OPEN", "UpgradeOpen");
            if (n == "Commit" && dt.StartsWith(DbNs, StringComparison.Ordinal)) Add("R-COMMIT", "");
            if (n is "StartTransaction" or "StartOpenCloseTransaction" or "get_TransactionManager" && !inSideDb) Add("R-TRANSACTION-SCOPE", "outside the side-database reader");
            if (last == "Document" && n is "get_Database" or "get_TransactionManager") Add("R-PRODUCT-DATABASE", "handle on the database of an open document");
            if (last == "HostApplicationServices" && n is "get_WorkingDatabase" or "set_WorkingDatabase") Add("R-PRODUCT-DATABASE", "WorkingDatabase");
            if (last == "Editor" && n.StartsWith("Command", StringComparison.Ordinal)) Add("R-COMMAND", "");
            if (n == "SendStringToExecute" || n == "LockDocument" || n == "Quit") Add("R-COMMAND", "");
            if (last == "DocumentCollection" && n is "Open" or "Add" or "CloseAll" or "set_MdiActiveDocument") Add("R-COMMAND", "document manager");
            else if (last is "DocumentCollection" or "DocumentCollectionExtension" && DocumentManagerPrefixes.Any(p => n.StartsWith(p, StringComparison.Ordinal)))
                Add("R-COMMAND", "document manager (Open / Close / Add / Recover / Create prefix, extension methods included)");
            if (last == "Document" && n.StartsWith("Close", StringComparison.Ordinal)) Add("R-COMMAND", "document close");
            if (n.StartsWith("add_", StringComparison.Ordinal) || n.StartsWith("remove_", StringComparison.Ordinal)) Add("R-EVENT", "event subscription");
            if (n.StartsWith("set_", StringComparison.Ordinal)) Add("R-AUTODESK-SETTER", "property write");
            if (dt == Db && n.StartsWith("ReadDwgFile", StringComparison.Ordinal) && !inSideDb) Add("R-READDWG-SCOPE", "outside the side-database reader");

            if (s.IsNewobj)
            {
                if (dt == Db) CheckDatabaseCtor(t, s, cfg, inSideDb, Add);
                else if (dt.StartsWith(DbNs, StringComparison.Ordinal) && !AllowedAutodeskCtors.Contains(dt)) Add("R-NEWOBJ-OBJECT", "object or entity creation");
            }
            else if (IsAcadNamespace(dt) && MutatorPrefixes.Any(p => n.StartsWith(p, StringComparison.Ordinal)) && !n.StartsWith("set_", StringComparison.Ordinal)
                     && n != "MoveNext" && !MutatorNameExceptions.Contains(dt + "::" + n))
            {
                Add("R-AUTODESK-MUTATOR-NAME", "conservative name heuristic");
            }

            CheckOpenMode(t, s, Add);
            return;
        }

        switch (dt)
        {
            case "System.Diagnostics.Process" when !(n.StartsWith("get_", StringComparison.Ordinal) || n == "GetCurrentProcess"):
                Add("R-PROCESS", "every Process member other than GetCurrentProcess and getters is refused");
                break;
            case "System.Reflection.Assembly" when n.StartsWith("Load", StringComparison.Ordinal) || n.StartsWith("UnsafeLoad", StringComparison.Ordinal):
            case "System.AppDomain" when n.StartsWith("Load", StringComparison.Ordinal):
                Add("R-ASSEMBLY-LOAD", "");
                break;
            case "System.Reflection.MethodBase" or "System.Reflection.MethodInfo" or "System.Reflection.ConstructorInfo" when n == "Invoke":
            case "System.Reflection.FieldInfo" or "System.Reflection.PropertyInfo" when n == "SetValue":
            case "System.Type" when n == "InvokeMember":
            case "System.Activator" when n.StartsWith("CreateInstance", StringComparison.Ordinal):
            case "System.Delegate" when n is "DynamicInvoke" or "CreateDelegate":
            case "System.Reflection.MethodInfo" or "System.Reflection.MethodBase" when n == "CreateDelegate":
                Add("R-REFLECTION-INVOKE", "");
                break;
            case "System.IO.Path" when n is "GetTempFileName" or "GetRandomFileName":
                Add("R-FILE-WRITE", "creates a file on disk");
                break;
            case "System.Environment" when n is "SetEnvironmentVariable" or "Exit" or "FailFast":
                Add("R-ENVIRONMENT", "");
                break;
            case "System.Runtime.InteropServices.Marshal":
                Add("R-PINVOKE", "every Marshal member is refused (Copy, StructureToPtr, Write*, GetFunctionPointerFor*, GetDelegateFor*...)");
                break;
            case "System.Runtime.InteropServices.GCHandle" or "System.Runtime.InteropServices.NativeMemory" or "System.Runtime.InteropServices.MemoryMarshal"
                or "System.Runtime.InteropServices.ComWrappers":
                Add("R-UNSAFE-API", "pinning, native memory or raw memory access");
                break;
            case "System.Buffer" when n.StartsWith("MemoryCopy", StringComparison.Ordinal) || n.StartsWith("SetByte", StringComparison.Ordinal):
                Add("R-UNSAFE-API", "raw memory copy");
                break;
        }
        if (dt == "System.Runtime.CompilerServices.Unsafe") Add("R-UNSAFE-API", "System.Runtime.CompilerServices.Unsafe: every member is refused");
        if (ConcurrencyTypes.Contains(StripArity(dt)) && ConcurrencyStarterPrefixes.Any(p => n.StartsWith(p, StringComparison.Ordinal)))
            Add("R-THREAD", "constructs, starts or schedules concurrent work");
        if (GlobalStateTypes.Contains(dt) && GlobalStateMutatorPrefixes.Any(p => n.StartsWith(p, StringComparison.Ordinal)))
            Add("R-GLOBAL-STATE", "process-wide or thread-wide state");
        if (dt.StartsWith("Microsoft.Win32.Registry", StringComparison.Ordinal) && n is "SetValue" or "CreateSubKey" or "DeleteValue" or "DeleteSubKey" or "DeleteSubKeyTree")
            Add("R-REGISTRY", "registry write");
        if (dt.StartsWith("System.Net.", StringComparison.Ordinal)) Add("R-NETWORK", "");

        if (inWriter) CheckWriterPrimitive(t, dt, s, Add);
        if (!inWriter)
        {
            if (dt is "System.IO.File" or "System.IO.Directory" or "System.IO.FileInfo" or "System.IO.DirectoryInfo" or "System.IO.FileSystemInfo")
            {
                var readOnly = ReadOnlyIoPrefixes.Any(p => n.StartsWith(p, StringComparison.Ordinal)) || (n == ".ctor" && dt is "System.IO.FileInfo" or "System.IO.DirectoryInfo");
                if (!readOnly) Add("R-FILE-WRITE", "file write outside the single EvidenceWriter");
            }
            else if (dt is "System.IO.FileStream" or "System.IO.StreamWriter" or "System.IO.BinaryWriter" && n == ".ctor")
            {
                Add("R-FILE-WRITE", "stream over a file outside the single EvidenceWriter");
            }
        }
    }

    private static void CheckDatabaseCtor(Target t, Site s, ScanConfig cfg, bool inSideDb, Action<string, string> add)
    {
        // the ONLY database construction of R0: new Database(false, true) inside the side-database reader, then ReadDwgFile of a private copy
        var ok = cfg.Role == ScanRole.R0 && inSideDb && t.Params is { Count: 2 } p
                 && p[0] == "System.Boolean" && p[1] == "System.Boolean"
                 && s.Index >= 2
                 && IlReader.ConstantI4(s.Code[s.Index - 2]) == 0
                 && IlReader.ConstantI4(s.Code[s.Index - 1]) == 1
                 && !s.Targets.Contains(s.Code[s.Index - 1].Offset) && !s.Targets.Contains(s.Code[s.Index].Offset);
        if (!ok) add("R-DB-CTOR", "only new Database(false, true) inside the side-database reader is allowed");
    }

    private static void CheckOpenMode(Target t, Site s, Action<string, string> add)
    {
        if (t.Params is null) return;
        var p = -1;
        for (var k = 0; k < t.Params.Count; k++)
            if (t.Params[k] == OpenModeType) { p = k; break; }
        if (p < 0) return;

        // walk back over the arguments that follow the OpenMode (each must be a one-instruction load), then read the OpenMode's own load
        var after = t.Params.Count - 1 - p;
        var idx = s.Index - 1 - after;
        for (var k = 0; k < after; k++)
            if (idx + 1 + k < 0 || !IlReader.IsSimpleLoad(s.Code[idx + 1 + k]))
            {
                add("R-OPENMODE-UNRESOLVABLE", "trailing argument is not a simple load");
                return;
            }
        if (idx < 0) { add("R-OPENMODE-UNRESOLVABLE", "no instruction before the call"); return; }
        for (var k = idx + 1; k <= s.Index; k++)
            if (s.Targets.Contains(s.Code[k].Offset)) { add("R-OPENMODE-UNRESOLVABLE", "a branch target lies between the OpenMode load and the call"); return; }
        var mode = IlReader.ConstantI4(s.Code[idx]);
        if (mode is null) add("R-OPENMODE-UNRESOLVABLE", "OpenMode is not a compile-time constant");
        else if (mode != 0) add("R-WRITE-OPEN", "OpenMode constant " + mode + " (ForWrite or ForNotify); only ForRead (0) is allowed");
    }

    /// <summary>Every Autodesk namespace (DatabaseServices, ApplicationServices, EditorInput, Runtime, Customization, PlottingServices, Publishing...).</summary>
    private static bool IsAcadNamespace(string t) => t.StartsWith("Autodesk.", StringComparison.Ordinal);

    private static string StripGenerics(string t)
    {
        var i = t.IndexOf('<');
        return i < 0 ? t : t[..i];
    }

    private static string StripArity(string t)
    {
        var i = t.IndexOf('`');
        return i < 0 ? t : t[..i];
    }

    private static string LastSegment(string t)
    {
        var i = Math.Max(t.LastIndexOf('.'), t.LastIndexOf('/'));
        return i < 0 ? t : t[(i + 1)..];
    }

    // -----------------------------------------------------------------------------------------------------------------------
    private sealed record Target(string DeclType, string Name, IReadOnlyList<string>? Params);

    /// <summary>Resolves handles to full type names and member targets. Signature types are decoded to full names.</summary>
    private sealed class NameProvider : ISignatureTypeProvider<string, object?>
    {
        private readonly MetadataReader _r;

        public NameProvider(MetadataReader reader) => _r = reader;

        public string Full(EntityHandle h) => h.Kind switch
        {
            HandleKind.TypeDefinition => Full((TypeDefinitionHandle)h),
            HandleKind.TypeReference => Full((TypeReferenceHandle)h),
            HandleKind.TypeSpecification => GetTypeFromSpecification(_r, null, (TypeSpecificationHandle)h, 0),
            _ => "<" + h.Kind + ">",
        };

        public string Full(TypeDefinitionHandle h)
        {
            var td = _r.GetTypeDefinition(h);
            var declaring = td.GetDeclaringType();
            var name = _r.GetString(td.Name);
            if (!declaring.IsNil) return Full(declaring) + "/" + name;
            var ns = _r.GetString(td.Namespace);
            return ns.Length == 0 ? name : ns + "." + name;
        }

        public string Full(TypeReferenceHandle h)
        {
            var tr = _r.GetTypeReference(h);
            var name = _r.GetString(tr.Name);
            if (tr.ResolutionScope.Kind == HandleKind.TypeReference) return Full((TypeReferenceHandle)tr.ResolutionScope) + "/" + name;
            var ns = _r.GetString(tr.Namespace);
            return ns.Length == 0 ? name : ns + "." + name;
        }

        public Target? Resolve(EntityHandle h)
        {
            switch (h.Kind)
            {
                case HandleKind.MethodDefinition:
                    return null; // a call into the assembly's own code: its body is scanned on its own
                case HandleKind.MethodSpecification:
                    return Resolve(_r.GetMethodSpecification((MethodSpecificationHandle)h).Method);
                case HandleKind.MemberReference:
                    var mr = _r.GetMemberReference((MemberReferenceHandle)h);
                    var declType = mr.Parent.Kind == HandleKind.MethodDefinition ? "<vararg>" : Full(mr.Parent);
                    var name = _r.GetString(mr.Name);
                    if (mr.GetKind() == MemberReferenceKind.Method)
                    {
                        var sig = mr.DecodeMethodSignature(this, null);
                        return new Target(declType, name, sig.ParameterTypes.ToList());
                    }
                    return new Target(declType, name, null);
                default:
                    return null;
            }
        }

        public string GetPrimitiveType(PrimitiveTypeCode c) => c switch
        {
            PrimitiveTypeCode.Boolean => "System.Boolean",
            PrimitiveTypeCode.Byte => "System.Byte",
            PrimitiveTypeCode.SByte => "System.SByte",
            PrimitiveTypeCode.Char => "System.Char",
            PrimitiveTypeCode.Int16 => "System.Int16",
            PrimitiveTypeCode.UInt16 => "System.UInt16",
            PrimitiveTypeCode.Int32 => "System.Int32",
            PrimitiveTypeCode.UInt32 => "System.UInt32",
            PrimitiveTypeCode.Int64 => "System.Int64",
            PrimitiveTypeCode.UInt64 => "System.UInt64",
            PrimitiveTypeCode.Single => "System.Single",
            PrimitiveTypeCode.Double => "System.Double",
            PrimitiveTypeCode.String => "System.String",
            PrimitiveTypeCode.Object => "System.Object",
            PrimitiveTypeCode.Void => "System.Void",
            PrimitiveTypeCode.IntPtr => "System.IntPtr",
            PrimitiveTypeCode.UIntPtr => "System.UIntPtr",
            _ => c.ToString(),
        };

        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => Full(handle);

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => Full(handle);

        public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
            reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

        public string GetSZArrayType(string elementType) => elementType + "[]";

        public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', shape.Rank - 1) + "]";

        public string GetByReferenceType(string elementType) => elementType + "&";

        public string GetPointerType(string elementType) => elementType + "*";

        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) =>
            genericType + "<" + string.Join(",", typeArguments) + ">";

        public string GetFunctionPointerType(MethodSignature<string> signature) => FnPtr;

        public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;

        public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;

        public string GetPinnedType(string elementType) => "pinned " + elementType;

        public const string FnPtr = "<fnptr>";
    }
}

/// <summary>Plain text of a scan, one violation per line, deterministic order.</summary>
public static class ScanReport
{
    public static string ToText(IEnumerable<(string Assembly, IReadOnlyList<Violation> Violations)> results)
    {
        var sb = new StringBuilder();
        foreach (var (asm, violations) in results)
        {
            sb.Append(asm).Append(": ").Append(violations.Count == 0 ? "CLEAN" : violations.Count + " violation(s)").Append('\n');
            foreach (var v in violations) sb.Append("  ").Append(v).Append('\n');
        }
        return sb.ToString();
    }
}
