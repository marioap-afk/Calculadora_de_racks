using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace I52Ct21d.HostFacts.Tools.Scan;

/// <summary>What the scan observed about the privileged types (the review aid <c>tools privileged-list</c> prints it).</summary>
public sealed class PrivilegedObservations
{
    public Dictionary<string, IReadOnlyList<string>> Surface { get; }

    public IReadOnlyList<string> Callers { get; }

    public IReadOnlyList<string> Commands { get; }

    public PrivilegedObservations(Dictionary<string, IReadOnlyList<string>> surface, IReadOnlyList<string> callers, IReadOnlyList<string> commands)
    {
        Surface = surface;
        Callers = callers;
        Commands = commands;
    }
}

/// <summary>
/// LAYER 3: the privileged surface. The first two layers decide WHICH external APIs may be referenced and where; they cannot see a call
/// between two methods of the same assembly (a call to a method definition) and an approval such as <c>FileStream::.ctor</c> inside the
/// <c>EvidenceWriter</c> says nothing about its arguments. This layer closes both gaps:
///
///  (a) <c>R-FILESTREAM-ARGS</c> / <c>R-SETATTRIBUTES-ARGS</c>: inside the writer, the only <c>FileStream</c> constructor is
///      <c>(string, FileMode.CreateNew, FileAccess.Write, FileShare.None)</c> with CONSTANT operands, and <c>File.SetAttributes</c> may only
///      SET the ReadOnly flag (never <c>Normal</c>, never an unresolved value);
///  (b) <c>R-PRIVILEGED-SURFACE</c>: the exact set of methods of <c>EvidenceWriter</c> and <c>EvidenceSealer</c> (nested types included)
///      equals the checked-in <c>privileged-surface.txt</c>, so a new method (a "Clobber(path, bytes)") cannot be added without an edit of
///      a reviewed file;
///  (c) <c>R-PRIVILEGED-CALLER</c>: every call, in ANY assembly (Core included), to a method of a privileged type comes from a caller listed
///      in <c>privileged-callers.txt</c>;
///  (d) <c>R-COMMAND-UNEXPECTED</c>: every <c>[CommandMethod]</c> / <c>[CommandClass]</c> registration is listed in <c>expected-commands.txt</c>.
/// </summary>
public static partial class ForbiddenApiScan
{
    private const string FileModeType = "System.IO.FileMode";
    private const string FileAccessType = "System.IO.FileAccess";
    private const string FileShareType = "System.IO.FileShare";
    private const string CommandMethodAttr = "Autodesk.AutoCAD.Runtime.CommandMethodAttribute";
    private const string CommandClassAttr = "Autodesk.AutoCAD.Runtime.CommandClassAttribute";

    private const int FileModeCreateNew = 1;
    private const int FileAccessWrite = 2;
    private const int FileShareNone = 0;
    private const int FileAttributesReadOnly = 1;

    private const string ReviewMessage = " This is a PRIVILEGED type: a change here needs an explicit independent review before the checked-in list is edited.";

    private static string[] PrivilegedTypes(ScanConfig cfg) => new[] { cfg.EvidenceWriterType, cfg.EvidenceSealerType };

    // ---- (a) the arguments of the two file primitives inside the writer ---------------------------------------------------
    private static void CheckWriterPrimitive(Target t, string declType, Site s, Action<string, string> add)
    {
        if (declType == "System.IO.FileStream" && t.Name == ".ctor")
        {
            if (t.Params is not { Count: 4 } p || p[0] != "System.String" || p[1] != FileModeType || p[2] != FileAccessType || p[3] != FileShareType)
            {
                add("R-FILESTREAM-ARGS", "only FileStream(string, FileMode, FileAccess, FileShare) is allowed in the EvidenceWriter");
                return;
            }
            var c = TrailingConstants(s, 3, out var why);
            if (c is null) { add("R-FILESTREAM-ARGS", "FileMode / FileAccess / FileShare are not compile-time constants (" + why + ")"); return; }
            if (c[0] != FileModeCreateNew) add("R-FILESTREAM-ARGS", "FileMode constant " + c[0] + "; only FileMode.CreateNew (1) is allowed");
            if (c[1] != FileAccessWrite) add("R-FILESTREAM-ARGS", "FileAccess constant " + c[1] + "; only FileAccess.Write (2) is allowed");
            if (c[2] != FileShareNone) add("R-FILESTREAM-ARGS", "FileShare constant " + c[2] + "; only FileShare.None (0) is allowed");
        }
        else if (declType == "System.IO.File" && t.Name == "SetAttributes")
        {
            if (t.Params is not { Count: 2 } p || p[0] != "System.String" || p[1] != "System.IO.FileAttributes")
            {
                add("R-SETATTRIBUTES-ARGS", "only File.SetAttributes(string, FileAttributes) is allowed in the EvidenceWriter");
                return;
            }
            // allowed shapes of the second argument, both ending right before the call: `ldc.i4 1` (ReadOnly alone) or `call File::GetAttributes; ldc.i4 1; or`
            var i = s.Index;
            if (i >= 3 && s.Code[i - 1].Op == System.Reflection.Emit.OpCodes.Or && IlReader.ConstantI4(s.Code[i - 2]) == FileAttributesReadOnly
                && IsCallTo(s, s.Code[i - 3], "System.IO.File", "GetAttributes") && NoBranchTarget(s, i - 3))
                return;
            if (i >= 1 && IlReader.ConstantI4(s.Code[i - 1]) == FileAttributesReadOnly && NoBranchTarget(s, i - 1))
                return;
            add("R-SETATTRIBUTES-ARGS", "the attributes argument must be the constant ReadOnly flag, set (File.GetAttributes(p) | FileAttributes.ReadOnly); Normal, a clear or an unresolved value is refused");
        }
    }

    private static bool NoBranchTarget(Site s, int from)
    {
        for (var k = from; k <= s.Index; k++)
            if (k < 0 || s.Targets.Contains(s.Code[k].Offset)) return false;
        return true;
    }

    /// <summary>The constants of the last <paramref name="count"/> arguments of the call, each pushed by one <c>ldc.i4*</c> right before it (null when any is not).</summary>
    private static int[]? TrailingConstants(Site s, int count, out string why)
    {
        why = "";
        if (s.Index < count) { why = "no instructions before the call"; return null; }
        var result = new int[count];
        for (var k = 0; k < count; k++)
        {
            var v = IlReader.ConstantI4(s.Code[s.Index - count + k]);
            if (v is null) { why = "argument " + (k + 1) + " of " + count + " is not loaded by a constant"; return null; }
            result[k] = v.Value;
        }
        if (!NoBranchTarget(s, s.Index - count)) { why = "a branch target lies between the arguments and the call"; return null; }
        return result;
    }

    private static bool IsCallTo(Site s, Instr ins, string declType, string name)
    {
        if (!IlReader.IsCall(ins) || s.Names is null) return false;
        var target = s.Names.Resolve(MetadataTokens.EntityHandle((int)ins.Operand));
        return target is not null && StripGenerics(target.DeclType) == declType && target.Name == name;
    }

    // ---- (b) the surface of a privileged type -------------------------------------------------------------------------------
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

    private static Dictionary<string, IReadOnlyList<string>> CollectSurface(MetadataReader reader, NameProvider names, string asmName, ScanConfig cfg)
    {
        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        if (asmName != cfg.CoreAssemblyName) return result;
        foreach (var priv in PrivilegedTypes(cfg))
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
                    lines.Add(full + "::" + reader.GetString(m.Name) + "(" + string.Join(",", sig.ParameterTypes) + ")->" + sig.ReturnType + " [" + flags + "]");
                }
            }
            if (found) result[priv] = lines.OrderBy(x => x, StringComparer.Ordinal).ToList();
        }
        return result;
    }

    private static bool ContainsLine(IReadOnlyList<string> lines, string line)
    {
        foreach (var l in lines)
            if (string.Equals(l, line, StringComparison.Ordinal)) return true;
        return false;
    }

    private static void CheckSurface(Dictionary<string, IReadOnlyList<string>> observed, string asmName, string displayName, ScanConfig cfg, List<Violation> sink)
    {
        if (cfg.Policy is null || asmName != cfg.CoreAssemblyName) return;
        foreach (var priv in PrivilegedTypes(cfg))
        {
            if (!observed.TryGetValue(priv, out var actual))
            {
                sink.Add(new Violation(displayName, priv, "", -1, "R-PRIVILEGED-SURFACE", "the privileged type is not defined in " + cfg.CoreAssemblyName + ", but privileged-surface.txt pins it"));
                continue;
            }
            if (!cfg.Policy.Surface.TryGetValue(priv, out var expected))
            {
                sink.Add(new Violation(displayName, priv, "", -1, "R-PRIVILEGED-SURFACE", "privileged-surface.txt has no entry for this type." + ReviewMessage));
                continue;
            }
            foreach (var added in actual)
                if (!ContainsLine(expected, added))
                    sink.Add(new Violation(displayName, priv, "", -1, "R-PRIVILEGED-SURFACE", "method not in privileged-surface.txt (added or changed): " + added + "." + ReviewMessage));
            foreach (var removed in expected)
                if (!ContainsLine(actual, removed))
                    sink.Add(new Violation(displayName, priv, "", -1, "R-PRIVILEGED-SURFACE", "method of privileged-surface.txt missing from the assembly (removed or changed): " + removed + "." + ReviewMessage));
        }
    }

    // ---- (c) the callers of a privileged type --------------------------------------------------------------------------------
    /// <summary>The (privileged top-level type, method) that the instruction names, when it names a method of one of the privileged Core types.</summary>
    private static (string Type, string Method)? PrivilegedCallee(Instr ins, MetadataReader reader, NameProvider names, string asmName, ScanConfig cfg)
    {
        if (ins.Op.OperandType is not (System.Reflection.Emit.OperandType.InlineMethod or System.Reflection.Emit.OperandType.InlineTok)) return null;
        var handle = MetadataTokens.EntityHandle((int)ins.Operand);
        if (handle.Kind == HandleKind.MethodSpecification) handle = reader.GetMethodSpecification((MethodSpecificationHandle)handle).Method;
        string top, method, definingAsm;
        switch (handle.Kind)
        {
            case HandleKind.MethodDefinition:
            {
                var md = reader.GetMethodDefinition((MethodDefinitionHandle)handle);
                top = names.Full(md.GetDeclaringType()).Split('/')[0];
                method = reader.GetString(md.Name);
                definingAsm = asmName;
                break;
            }
            case HandleKind.MemberReference:
            {
                var mr = reader.GetMemberReference((MemberReferenceHandle)handle);
                if (mr.Parent.Kind != HandleKind.TypeReference) return null;
                top = names.Full(mr.Parent).Split('/')[0];
                method = reader.GetString(mr.Name);
                definingAsm = ScopeAssembly(reader, (TypeReferenceHandle)mr.Parent) ?? "";
                break;
            }
            default:
                return null; // ldtoken of the type itself and the like name no member
        }
        if (definingAsm != cfg.CoreAssemblyName || Array.IndexOf(PrivilegedTypes(cfg), top) < 0) return null;
        return (top, method);
    }

    private static string? ScopeAssembly(MetadataReader reader, TypeReferenceHandle h)
    {
        var tr = reader.GetTypeReference(h);
        while (tr.ResolutionScope.Kind == HandleKind.TypeReference) tr = reader.GetTypeReference((TypeReferenceHandle)tr.ResolutionScope);
        return tr.ResolutionScope.Kind == HandleKind.AssemblyReference
            ? reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)tr.ResolutionScope).Name)
            : null;
    }

    private static void ObserveCaller(Instr ins, MetadataReader reader, NameProvider names, string displayName, string asmName, string typeName, string methodName, string top,
        ScanConfig cfg, List<string> observedCallers, List<Violation> sink)
    {
        var callee = PrivilegedCallee(ins, reader, names, asmName, cfg);
        if (callee is null) return;
        if (top == callee.Value.Type && asmName == cfg.CoreAssemblyName) return; // a privileged type calling itself: its surface is pinned
        var caller = asmName + "::" + typeName + "::" + methodName;
        var calleeText = callee.Value.Type + "::" + callee.Value.Method;
        observedCallers.Add(caller + "|" + calleeText);
        if (cfg.Policy is not null && !cfg.Policy.AllowsCaller(caller, calleeText))
            sink.Add(new Violation(displayName, typeName, methodName, ins.Offset, "R-PRIVILEGED-CALLER",
                calleeText + " may be called only from the callers of privileged-callers.txt; " + caller + " is not one of them." + ReviewMessage));
    }

    // ---- (d) the command registrations ---------------------------------------------------------------------------------------
    private sealed class AttributeValueProvider : ICustomAttributeTypeProvider<string>
    {
        private readonly NameProvider _names;

        public AttributeValueProvider(NameProvider names) => _names = names;

        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => _names.GetPrimitiveType(typeCode);

        public string GetSystemType() => "System.Type";

        public string GetSZArrayType(string elementType) => elementType + "[]";

        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => _names.Full(handle);

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => _names.Full(handle);

        public string GetTypeFromSerializedName(string name) => name;

        public PrimitiveTypeCode GetUnderlyingEnumType(string type) => PrimitiveTypeCode.Int32;

        public bool IsSystemType(string type) => type == "System.Type";
    }

    private static string AttributeOwner(MetadataReader reader, NameProvider names, string asmName, EntityHandle parent)
    {
        switch (parent.Kind)
        {
            case HandleKind.MethodDefinition:
            {
                var md = reader.GetMethodDefinition((MethodDefinitionHandle)parent);
                return asmName + "::" + names.Full(md.GetDeclaringType()) + "::" + reader.GetString(md.Name);
            }
            case HandleKind.TypeDefinition:
                return asmName + "::" + names.Full((TypeDefinitionHandle)parent) + "::<type>";
            case HandleKind.AssemblyDefinition:
                return asmName + "::<assembly>";
            case HandleKind.ModuleDefinition:
                return asmName + "::<module>";
            default:
                return asmName + "::<" + parent.Kind + ">";
        }
    }

    private static void CollectCommands(MetadataReader reader, NameProvider names, string asmName, string displayName, ScanConfig cfg, List<string> observed, List<Violation> sink)
    {
        var provider = new AttributeValueProvider(names);
        foreach (var th in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(th);
            if (td.BaseType.IsNil) continue;
            var b = names.Full(td.BaseType);
            if (b is CommandMethodAttr or CommandClassAttr)
                sink.Add(new Violation(displayName, names.Full(th), "", -1, "R-COMMAND-UNEXPECTED", "the assembly derives its own command attribute from " + b));
        }
        foreach (var cah in reader.CustomAttributes)
        {
            var ca = reader.GetCustomAttribute(cah);
            if (ca.Constructor.Kind != HandleKind.MemberReference) continue;
            var mr = reader.GetMemberReference((MemberReferenceHandle)ca.Constructor);
            if (mr.Parent.Kind != HandleKind.TypeReference) continue;
            var attrType = names.Full(mr.Parent);
            if (attrType is not (CommandMethodAttr or CommandClassAttr)) continue;
            var kind = attrType == CommandMethodAttr ? "CommandMethod" : "CommandClass";
            var owner = AttributeOwner(reader, names, asmName, ca.Parent);
            string args;
            try
            {
                var value = ca.DecodeValue(provider);
                var fixedArgs = new List<string>();
                for (var k = 0; k < value.FixedArguments.Length; k++) fixedArgs.Add(value.FixedArguments[k].Value?.ToString() ?? "<null>");
                args = string.Join(",", fixedArgs);
                if (value.NamedArguments.Length > 0)
                {
                    var named = new List<string>();
                    for (var k = 0; k < value.NamedArguments.Length; k++) named.Add(value.NamedArguments[k].Name + "=" + value.NamedArguments[k].Value?.ToString());
                    args += ";" + string.Join(",", named);
                }
            }
            catch (BadImageFormatException)
            {
                sink.Add(new Violation(displayName, owner, "", -1, "R-COMMAND-UNEXPECTED", kind + " attribute with an undecodable blob"));
                continue;
            }
            observed.Add(kind + "|" + owner + "|" + args);
            if (cfg.Policy is not null && !cfg.Policy.ExpectsCommand(kind, owner, args))
                sink.Add(new Violation(displayName, owner, "", -1, "R-COMMAND-UNEXPECTED",
                    kind + " " + args + " is not in expected-commands.txt: a new command registration needs an explicit review of the list."));
        }
    }
}
