using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using I52Ct21d.HostFacts.Tools.Scan;

namespace I52Ct21d.HostFacts.Rs.Tools;

/// <summary>What the RS layer observed in one assembly (the review aid <c>privileged-list-rs</c> prints it).</summary>
public sealed class RsLayerObservations
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Surface { get; }
    public IReadOnlyList<string> Callers { get; }

    public RsLayerObservations(IReadOnlyDictionary<string, IReadOnlyList<string>> surface, IReadOnlyList<string> callers)
    {
        Surface = surface;
        Callers = callers;
    }
}

public sealed class RsLayerOutcome
{
    public IReadOnlyList<Violation> Violations { get; }
    public RsLayerObservations Observed { get; }
    public int BodiesExamined { get; }
    public int PrivilegedCallSites { get; }

    public RsLayerOutcome(IReadOnlyList<Violation> violations, RsLayerObservations observed, int bodiesExamined, int privilegedCallSites)
    {
        Violations = violations;
        Observed = observed;
        BodiesExamined = bodiesExamined;
        PrivilegedCallSites = privilegedCallSites;
    }
}

/// <summary>
/// THE RS LAYER of the forbidden-API scan (class RS, Owner Q-O-1). The R0 scan (deny-by-default imports + deny rules + the pinned surface of the
/// evidence writer) runs first and unchanged; its rules that the RS DLL must trip (database creation, object creation, property setters, commit,
/// write open, save) are waived only inside the types and only for the details that <c>rs-waivers.txt</c> names. This layer then makes the waiver
/// SAFE with checks on the IL of the RS assembly itself:
///
///  * <c>R-RS-DB-CTOR</c>: every <c>new Database(...)</c> is inside <c>SideDbWriter</c>, with the constructor <c>(bool, bool)</c> and CONSTANT arguments whose
///    second one (noDocument) is true: a database is never attached to a document;
///  * <c>R-RS-SAVE-SCOPE</c>: <c>Database.SaveAs</c> only inside <c>ScratchSaver</c>, only the overload <c>(string, DwgVersion)</c> with a constant version,
///    whose path argument is the result of <c>ScratchTarget.get_FullPath</c> (a path the guard authorized), after a straight-line call to
///    <c>ScratchRootGuard.AssertCreatable</c>;
///  * <c>R-RS-READDWG-SCOPE</c>: <c>Database.ReadDwgFile</c> only inside <c>SideDbWriter</c>, the path being the result of <c>ReadableSource.get_FullPath</c>, after a
///    straight-line call to <c>ScratchRootGuard.AssertReadable</c>;
///  * <c>R-RS-PRIVILEGED-SURFACE</c>: the exact method set of the four privileged RS types equals <c>privileged-surface-rs.txt</c>; no non-private method
///    of <c>SideDbWriter</c>, <c>SideDbReader</c>, <c>ScratchSaver</c> mentions an Autodesk type (<c>SideDbHandle</c> excepted: it is the wrapper);
///  * <c>R-RS-PRIVILEGED-CALLER</c>: every call, in the RS assembly, to a method of a privileged RS type comes from a caller in <c>privileged-callers-rs.txt</c>.
///
/// It is a REVIEW AID over the compiled IL. It does not prove that a straight-line guard call dominates every path or that the validated
/// path is the one that reaches the file system (README, residual limits).
/// </summary>
public static class RsLayer
{
    public const string RsAssembly = "I52Ct21d.HostFacts.Rs";
    public const string RsCoreAssembly = "I52Ct21d.HostFacts.Rs.Core";

    public const string SideDbWriterType = "I52Ct21d.HostFacts.Rs.SideDbWriter";
    public const string SideDbReaderType = "I52Ct21d.HostFacts.Rs.SideDbReader";
    public const string ScratchSaverType = "I52Ct21d.HostFacts.Rs.ScratchSaver";
    public const string SideDbHandleType = "I52Ct21d.HostFacts.Rs.SideDbHandle";

    public static readonly IReadOnlyList<string> PrivilegedTypes = new[] { SideDbWriterType, SideDbReaderType, ScratchSaverType, SideDbHandleType };

    private const string DatabaseType = "Autodesk.AutoCAD.DatabaseServices.Database";
    private const string GuardType = "I52Ct21d.HostFacts.Rs.Core.ScratchRootGuard";
    private const string ScratchTargetType = "I52Ct21d.HostFacts.Rs.Core.ScratchTarget";
    private const string ReadableSourceType = "I52Ct21d.HostFacts.Rs.Core.ReadableSource";

    private static string AccessText(MethodAttributes a) => (a & MethodAttributes.MemberAccessMask) switch
    {
        MethodAttributes.Public => "public",
        MethodAttributes.Private => "private",
        MethodAttributes.Assembly => "internal",
        MethodAttributes.Family => "protected",
        MethodAttributes.FamANDAssem => "private-protected",
        MethodAttributes.FamORAssem => "protected-internal",
        _ => "other",
    };

    private static bool IsCallLike(Instr ins) =>
        ins.Op.OperandType == OperandType.InlineMethod || ins.Op.OperandType == OperandType.InlineTok;

    private static bool IsCallTo(Instr ins, RsNames names, string type, string method)
    {
        if (ins.Op.OperandType != OperandType.InlineMethod) return false;
        var c = names.Resolve(RsNames.Handle(ins.Operand));
        return c is not null && c.TypeFull == type && c.Name == method;
    }

    /// <summary>A guard call earlier in the same method with no branch, branch target, <c>ret</c> or <c>throw</c> between it and the call at <paramref name="callIndex"/>.</summary>
    private static bool GuardBefore(IReadOnlyList<Instr> code, HashSet<int> targets, int callIndex, RsNames names, string guardMethod)
    {
        for (var g = callIndex - 1; g >= 0; g--)
        {
            if (!IsCallTo(code[g], names, GuardType, guardMethod)) continue;
            for (var k = g + 1; k <= callIndex; k++)
            {
                if (targets.Contains(code[k].Offset)) return false;
                var ot = code[k].Op.OperandType;
                if (ot == OperandType.InlineBrTarget || ot == OperandType.ShortInlineBrTarget || ot == OperandType.InlineSwitch) return false;
                if (code[k].Op == OpCodes.Ret || code[k].Op == OpCodes.Throw || code[k].Op == OpCodes.Rethrow) return false;
            }
            return true;
        }
        return false;
    }

    public static RsLayerOutcome Check(byte[] image, string displayName, ScanPolicy? policy)
    {
        var sink = new List<Violation>();
        using var pe = new PEReader(ImmutableArray.Create(image));
        var reader = pe.GetMetadataReader();
        var asm = reader.GetString(reader.GetAssemblyDefinition().Name);
        var names = new RsNames(reader);
        var isRs = asm == RsAssembly;

        // ---- the pinned surface of the privileged RS types ----------------------------------------------------------------------
        var surface = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        if (isRs)
        {
            foreach (var priv in PrivilegedTypes)
            {
                var lines = new List<string>();
                var found = false;
                foreach (var th in reader.TypeDefinitions)
                {
                    var full = names.Full(th);
                    if (full.Split('/')[0] != priv) continue;
                    found = true;
                    foreach (var mh in reader.GetTypeDefinition(th).GetMethods())
                    {
                        var m = reader.GetMethodDefinition(mh);
                        MethodSignature<string> sig;
                        try { sig = m.DecodeSignature(names, null); }
                        catch (BadImageFormatException) { lines.Add(full + "::" + reader.GetString(m.Name) + "(<undecodable>)->?"); continue; }
                        var flags = AccessText(m.Attributes) + ((m.Attributes & MethodAttributes.Static) != 0 ? " static" : " instance")
                                    + ((m.Attributes & MethodAttributes.Virtual) != 0 ? " virtual" : "")
                                    + ((m.Attributes & MethodAttributes.PinvokeImpl) != 0 ? " pinvoke" : "");
                        var text = full + "::" + reader.GetString(m.Name) + "(" + string.Join(",", sig.ParameterTypes) + ")->" + sig.ReturnType + " [" + flags + "]";
                        lines.Add(text);
                        var accessible = (m.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Private;
                        if (accessible && priv != SideDbHandleType && text.Contains("Autodesk.", StringComparison.Ordinal))
                            sink.Add(new Violation(displayName, priv, reader.GetString(m.Name), -1, "R-RS-SURFACE-AUTODESK",
                                "a non-private method of a privileged RS type mentions an Autodesk type: " + text));
                    }
                }
                if (found) surface[priv] = lines.OrderBy(x => x, StringComparer.Ordinal).ToList();
            }
            if (policy is not null) CompareSurface(surface, policy, displayName, sink);
        }

        // ---- every method body ----------------------------------------------------------------------------------------------------
        var observedCallers = new List<string>();
        var bodies = 0;
        var privilegedSites = 0;
        foreach (var th in reader.TypeDefinitions)
        {
            var typeName = names.Full(th);
            var top = typeName.Split('/')[0];
            foreach (var mh in reader.GetTypeDefinition(th).GetMethods())
            {
                var m = reader.GetMethodDefinition(mh);
                if (m.RelativeVirtualAddress == 0) continue;
                if ((m.ImplAttributes & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL) continue;
                var il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILBytes();
                if (il is null) continue;
                bodies++;
                var methodName = reader.GetString(m.Name);
                var code = IlReader.Decode(il);
                var targets = IlReader.BranchTargets(code);
                for (var i = 0; i < code.Count; i++)
                {
                    var ins = code[i];
                    if (!IsCallLike(ins)) continue;
                    var callee = names.Resolve(RsNames.Handle(ins.Operand));
                    if (callee is null) continue;
                    var offset = ins.Offset;
                    void Add(string rule, string detail) => sink.Add(new Violation(displayName, typeName, methodName, offset, rule, callee.TypeFull + "::" + callee.Name + " (" + detail + ")"));

                    if (callee.TypeFull == DatabaseType)
                    {
                        if (callee.Name == ".ctor") CheckDatabaseCtor(ins, callee, code, i, targets, asm, top, Add);
                        else if (callee.Name.StartsWith("SaveAs", StringComparison.Ordinal)) CheckSaveAs(callee, code, i, targets, names, asm, top, Add);
                        else if (callee.Name.StartsWith("ReadDwgFile", StringComparison.Ordinal)) CheckReadDwg(callee, code, i, targets, names, asm, top, Add);
                    }

                    if (isRs && callee.IsDefinition && PrivilegedTypes.Contains(callee.TopType) && callee.TopType != top)
                    {
                        privilegedSites++;
                        var caller = asm + "::" + typeName + "::" + methodName;
                        var calleeText = callee.TopType + "::" + callee.Name;
                        observedCallers.Add(caller + "|" + calleeText);
                        if (policy is not null && !policy.AllowsCaller(caller, calleeText))
                            Add("R-RS-PRIVILEGED-CALLER", "may be called only from the callers of privileged-callers-rs.txt; " + caller + " is not one of them. This is a PRIVILEGED type: a change here needs an explicit independent review before the list is edited");
                    }
                }
            }
        }
        var observed = new RsLayerObservations(surface, observedCallers.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList());
        var ordered = sink.OrderBy(v => v.Type, StringComparer.Ordinal).ThenBy(v => v.Method, StringComparer.Ordinal).ThenBy(v => v.IlOffset)
            .ThenBy(v => v.Rule, StringComparer.Ordinal).ToList();
        return new RsLayerOutcome(ordered, observed, bodies, privilegedSites);
    }

    private static bool InRs(string asm, string top, string type) => asm == RsAssembly && top == type;

    private static bool Straight(HashSet<int> targets, IReadOnlyList<Instr> code, int from, int to)
    {
        for (var k = from; k <= to; k++)
            if (k < 0 || targets.Contains(code[k].Offset)) return false;
        return true;
    }

    private static void CheckDatabaseCtor(Instr ins, CalleeInfo callee, IReadOnlyList<Instr> code, int i, HashSet<int> targets, string asm, string top, Action<string, string> add)
    {
        if (ins.Op != OpCodes.Newobj) return;
        if (!InRs(asm, top, SideDbWriterType))
        {
            add("R-RS-DB-CTOR", "a database may be constructed only inside " + SideDbWriterType + " (the side-database factory)");
            return;
        }
        var ok = callee.Params.Count == 2 && callee.Params[0] == "System.Boolean" && callee.Params[1] == "System.Boolean" && i >= 2
                 && IlReader.ConstantI4(code[i - 2]) is 0 or 1 && IlReader.ConstantI4(code[i - 1]) == 1 && Straight(targets, code, i - 1, i);
        if (!ok) add("R-RS-DB-CTOR", "only new Database(bool, bool) with constant arguments and noDocument = true is allowed (a database never attached to a document)");
    }

    private static void CheckSaveAs(CalleeInfo callee, IReadOnlyList<Instr> code, int i, HashSet<int> targets, RsNames names, string asm, string top, Action<string, string> add)
    {
        if (!InRs(asm, top, ScratchSaverType))
        {
            add("R-RS-SAVE-SCOPE", "SaveAs may be called only inside " + ScratchSaverType);
            return;
        }
        if (callee.Params.Count != 2 || callee.Params[0] != "System.String" || callee.Params[1] != "Autodesk.AutoCAD.DatabaseServices.DwgVersion")
        {
            add("R-RS-SAVE-SCOPE", "only SaveAs(string, DwgVersion) is allowed");
            return;
        }
        if (i < 2 || IlReader.ConstantI4(code[i - 1]) is null || !Straight(targets, code, i - 2, i))
            add("R-RS-SAVE-SCOPE", "the DwgVersion argument must be a compile-time constant");
        if (i < 2 || !IsCallTo(code[i - 2], names, ScratchTargetType, "get_FullPath"))
            add("R-RS-SAVE-SCOPE", "the path argument must be the result of ScratchTarget.FullPath (a path the scratch-root guard authorized)");
        if (!GuardBefore(code, targets, i, names, "AssertCreatable"))
            add("R-RS-SAVE-SCOPE", "no straight-line call to ScratchRootGuard.AssertCreatable precedes the save");
    }

    private static void CheckReadDwg(CalleeInfo callee, IReadOnlyList<Instr> code, int i, HashSet<int> targets, RsNames names, string asm, string top, Action<string, string> add)
    {
        if (!InRs(asm, top, SideDbWriterType))
        {
            add("R-RS-READDWG-SCOPE", "ReadDwgFile may be called only inside " + SideDbWriterType);
            return;
        }
        if (callee.Params.Count != 4 || callee.Params[0] != "System.String" || callee.Params[1] != "System.IO.FileShare")
        {
            add("R-RS-READDWG-SCOPE", "only ReadDwgFile(string, FileShare, bool, string) is allowed");
            return;
        }
        if (i < 4 || !IsCallTo(code[i - 4], names, ReadableSourceType, "get_FullPath"))
            add("R-RS-READDWG-SCOPE", "the path argument must be the result of ReadableSource.FullPath (a source the scratch-root guard authorized)");
        if (!GuardBefore(code, targets, i, names, "AssertReadable"))
            add("R-RS-READDWG-SCOPE", "no straight-line call to ScratchRootGuard.AssertReadable precedes the read");
    }

    private static void CompareSurface(Dictionary<string, IReadOnlyList<string>> observed, ScanPolicy policy, string displayName, List<Violation> sink)
    {
        const string review = " This is a PRIVILEGED type: a change here needs an explicit independent review before the checked-in list is edited.";
        foreach (var priv in PrivilegedTypes)
        {
            if (!observed.TryGetValue(priv, out var actual))
            {
                sink.Add(new Violation(displayName, priv, "", -1, "R-RS-PRIVILEGED-SURFACE", "the privileged type is not defined in " + RsAssembly + ", but the surface list pins it"));
                continue;
            }
            if (!policy.Surface.TryGetValue(priv, out var expected))
            {
                sink.Add(new Violation(displayName, priv, "", -1, "R-RS-PRIVILEGED-SURFACE", "privileged-surface-rs.txt has no entry for this type." + review));
                continue;
            }
            foreach (var added in actual)
                if (!expected.Contains(added, StringComparer.Ordinal))
                    sink.Add(new Violation(displayName, priv, "", -1, "R-RS-PRIVILEGED-SURFACE", "method not in privileged-surface-rs.txt (added or changed): " + added + "." + review));
            foreach (var removed in expected)
                if (!actual.Contains(removed, StringComparer.Ordinal))
                    sink.Add(new Violation(displayName, priv, "", -1, "R-RS-PRIVILEGED-SURFACE", "method of privileged-surface-rs.txt missing from the assembly (removed or changed): " + removed + "." + review));
        }
        foreach (var pinned in policy.Surface.Keys)
            if (!PrivilegedTypes.Contains(pinned, StringComparer.Ordinal))
                sink.Add(new Violation(displayName, pinned, "", -1, "R-RS-PRIVILEGED-SURFACE", "privileged-surface-rs.txt pins a type that is not one of the privileged RS types"));
    }
}
