using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Emit = System.Reflection.Emit;

namespace I52Ct21d.HostFacts.Tools.Scan;

/// <summary>
/// RAW-MEMORY AND UNSAFE-CODE RULES. A write to raw memory in unsafe code (<c>*p = 5</c>, <c>stackalloc</c> plus a pointer write, a block copy)
/// uses only IL opcodes and no external member, so neither the imports allowlist nor the member-name deny rules can see it. These rules look
/// at the OPCODES and at the SIGNATURES instead, and they are independent of the allowlist: they run with and without it.
///
/// * <c>localloc</c>, <c>cpblk</c>, <c>initblk</c> are refused everywhere (R-RAW-MEMORY); they can never be approved.
/// * <c>stind.*</c>, <c>stobj</c>, <c>cpobj</c> (a store through a pointer or a by-ref) are refused (R-RAW-STORE) unless the allowlist carries an
///   <c>O|opcode|Assembly::Type|why</c> approval for the top-level type that contains the store. The scan cannot prove from the IL alone that the
///   target of such a store is a managed by-ref (an <c>out</c> parameter, a <c>ref</c> local) and not an unmanaged address; the approval is that
///   claim, made per type and per opcode and read by the reviewer. The unmanaged address itself cannot exist without one of the refused
///   signature shapes below, or an <c>IntPtr</c> produced by a refused API.
/// * a pointer type (<c>T*</c>), a function-pointer type (<c>delegate*</c>) or a pinned local in ANY signature (method, local, field,
///   property, event, member reference, type specification, method specification, standalone signature) is refused (R-POINTER-TYPE).
/// * an assembly or module carrying <c>System.Security.UnverifiableCodeAttribute</c> (what the compiler emits for unsafe code) is refused (R-UNSAFE-CODE).
/// * an assembly that is not IL-only, and a method that is native / unmanaged / internal-call, is refused (R-NOT-ILONLY, R-NATIVE-METHOD).
/// </summary>
public static partial class ForbiddenApiScan
{
    // by opcode NAME (the scan itself then references no OpCodes.* field of the store family)
    private static readonly HashSet<string> RawMemoryOpcodes = new(StringComparer.Ordinal) { "localloc", "cpblk", "initblk" };

    private static readonly HashSet<string> RawStoreOpcodes = new(ApiAllowlist.ApprovableOpcodes, StringComparer.Ordinal);

    /// <summary>Opcodes that turn a number or a managed reference into a native-size integer / unmanaged pointer (the way an address is forged).</summary>
    private static readonly HashSet<string> NativeAddressOpcodes = new(StringComparer.Ordinal)
    {
        "conv.i", "conv.u", "conv.ovf.i", "conv.ovf.u", "conv.ovf.i.un", "conv.ovf.u.un",
    };

    private static bool HasPointer(string signatureText) =>
        signatureText.Contains('*') || signatureText.Contains(NameProvider.FnPtr, StringComparison.Ordinal) || signatureText.Contains("pinned ", StringComparison.Ordinal);

    /// <summary>
    /// Judges one instruction. Returns true when it is a store opcode that the allowlist APPROVED for this type (the caller then applies the
    /// native-address check of <see cref="CheckApprovedStores"/> to the whole method).
    /// </summary>
    private static bool CheckRawMemoryOpcode(Instr ins, string asmName, string top, string displayName, string typeName, string methodName, ScanConfig cfg, List<Violation> sink)
    {
        var name = ins.Op.Name ?? "?";
        if (RawMemoryOpcodes.Contains(name))
        {
            sink.Add(new Violation(displayName, typeName, methodName, ins.Offset, "R-RAW-MEMORY", name + " (raw memory) is never allowed and can not be approved"));
            return false;
        }
        if (!RawStoreOpcodes.Contains(name)) return false;
        if (cfg.Allowlist is not null && cfg.Allowlist.FindOpcode(name).Any(e => e.AppliesTo(asmName, top))) return true;
        sink.Add(new Violation(displayName, typeName, methodName, ins.Offset, "R-RAW-STORE",
            name + " (store through a pointer or a by-ref) is refused unless the allowlist approves it for " + AllowEntry.ScopeOf(asmName, top)));
        return false;
    }

    /// <summary>
    /// An approved by-ref store is trusted only in a method that has no way to forge an unmanaged address: no <c>conv.i</c> / <c>conv.u</c>
    /// family opcode and no <c>IntPtr</c> / <c>UIntPtr</c> in its signature or locals (pointer types are refused anyway). Otherwise the approval
    /// does not hold for that method (R-RAW-STORE-NATIVE-ADDRESS).
    /// </summary>
    private static void CheckApprovedStores(IReadOnlyList<Instr> code, IReadOnlyList<int> approvedStoreOffsets, string nativeSignatureText, string displayName, string typeName, string methodName, List<Violation> sink)
    {
        if (approvedStoreOffsets.Count == 0) return;
        var forging = code.FirstOrDefault(i => NativeAddressOpcodes.Contains(i.Op.Name ?? "?"));
        var why = forging is not null ? (forging.Op.Name + " at IL_" + forging.Offset.ToString("X4"))
            : nativeSignatureText.Contains("System.IntPtr", StringComparison.Ordinal) || nativeSignatureText.Contains("System.UIntPtr", StringComparison.Ordinal) ? "IntPtr / UIntPtr in the signature or the locals"
            : null;
        if (why is null) return;
        foreach (var off in approvedStoreOffsets)
            sink.Add(new Violation(displayName, typeName, methodName, off, "R-RAW-STORE-NATIVE-ADDRESS",
                "an approved by-ref store in a method that can forge a native address (" + why + ")"));
    }

    /// <summary>The decoded signature and local types of a method, as one text (searched for IntPtr / UIntPtr).</summary>
    private static string NativeSignatureText(PEReader pe, MetadataReader reader, NameProvider names, MethodDefinition m)
    {
        var sig = m.DecodeSignature(names, null);
        var text = string.Join(" ", sig.ParameterTypes) + " " + sig.ReturnType;
        var local = pe.GetMethodBody(m.RelativeVirtualAddress).LocalSignature;
        return local.IsNil ? text : text + " " + string.Join(" ", reader.GetStandaloneSignature(local).DecodeLocalSignature(names, null));
    }

    private static string AttributeTypeName(MetadataReader reader, NameProvider names, CustomAttribute ca) => ca.Constructor.Kind switch
    {
        HandleKind.MemberReference => names.Full(reader.GetMemberReference((MemberReferenceHandle)ca.Constructor).Parent),
        HandleKind.MethodDefinition => names.Full(reader.GetMethodDefinition((MethodDefinitionHandle)ca.Constructor).GetDeclaringType()),
        _ => "<" + ca.Constructor.Kind + ">",
    };

    private static void CheckUnsafeMetadata(PEReader pe, MetadataReader reader, NameProvider names, string displayName, List<Violation> sink)
    {
        void Add(string type, string member, string rule, string detail) => sink.Add(new Violation(displayName, type, member, -1, rule, detail));
        void Pointer(string type, string member, string where, IEnumerable<string> texts)
        {
            var bad = texts.FirstOrDefault(HasPointer);
            if (bad is not null) Add(type, member, "R-POINTER-TYPE", where + " has a pointer, function-pointer or pinned type: " + bad);
        }

        var cor = pe.PEHeaders.CorHeader;
        if (cor is null || (cor.Flags & CorFlags.ILOnly) == 0)
            Add("<metadata>", "", "R-NOT-ILONLY", "the assembly is not IL-only (mixed-mode or native code)");

        foreach (var owner in new EntityHandle[] { EntityHandle.ModuleDefinition, EntityHandle.AssemblyDefinition })
            foreach (var cah in reader.GetCustomAttributes(owner))
                if (AttributeTypeName(reader, names, reader.GetCustomAttribute(cah)) == "System.Security.UnverifiableCodeAttribute")
                    Add("<metadata>", "", "R-UNSAFE-CODE", "the " + (owner.Kind == HandleKind.ModuleDefinition ? "module" : "assembly") + " carries UnverifiableCodeAttribute (compiled with unsafe code)");

        foreach (var th in reader.TypeDefinitions)
        {
            var type = names.Full(th);
            var td = reader.GetTypeDefinition(th);
            foreach (var fh in td.GetFields())
            {
                var f = reader.GetFieldDefinition(fh);
                Pointer(type, reader.GetString(f.Name), "field", new[] { f.DecodeSignature(names, null) });
            }
            foreach (var mh in td.GetMethods())
            {
                var m = reader.GetMethodDefinition(mh);
                var name = reader.GetString(m.Name);
                var impl = m.ImplAttributes;
                if ((impl & MethodImplAttributes.CodeTypeMask) is MethodImplAttributes.Native or MethodImplAttributes.OPTIL
                    || (impl & MethodImplAttributes.Unmanaged) != 0 || (impl & MethodImplAttributes.InternalCall) != 0)
                    Add(type, name, "R-NATIVE-METHOD", "native, unmanaged or internal-call method implementation");
                var sig = m.DecodeSignature(names, null);
                Pointer(type, name, "method signature", sig.ParameterTypes.Prepend(sig.ReturnType));
                if (m.RelativeVirtualAddress != 0 && (impl & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.IL)
                {
                    var local = pe.GetMethodBody(m.RelativeVirtualAddress).LocalSignature;
                    if (!local.IsNil) Pointer(type, name, "local variable signature", reader.GetStandaloneSignature(local).DecodeLocalSignature(names, null));
                }
            }
            foreach (var ph in td.GetProperties())
            {
                var p = reader.GetPropertyDefinition(ph);
                var sig = p.DecodeSignature(names, null);
                Pointer(type, reader.GetString(p.Name), "property signature", sig.ParameterTypes.Prepend(sig.ReturnType));
            }
            foreach (var eh in td.GetEvents())
            {
                var e = reader.GetEventDefinition(eh);
                Pointer(type, reader.GetString(e.Name), "event type", new[] { names.Full(e.Type) });
            }
        }

        foreach (var h in reader.MemberReferences)
        {
            var mr = reader.GetMemberReference(h);
            var parent = mr.Parent.Kind is HandleKind.TypeReference or HandleKind.TypeSpecification or HandleKind.TypeDefinition ? names.Full(mr.Parent) : "<metadata>";
            var name = reader.GetString(mr.Name);
            if (mr.GetKind() == MemberReferenceKind.Method)
            {
                var sig = mr.DecodeMethodSignature(names, null);
                Pointer(parent, name, "member reference signature", sig.ParameterTypes.Prepend(sig.ReturnType));
            }
            else
            {
                Pointer(parent, name, "member reference (field) signature", new[] { mr.DecodeFieldSignature(names, null) });
            }
        }

        for (var i = 1; i <= reader.GetTableRowCount(TableIndex.StandAloneSig); i++)
        {
            var sa = reader.GetStandaloneSignature(MetadataTokens.StandaloneSignatureHandle(i));
            if (sa.GetKind() != StandaloneSignatureKind.LocalVariables) // locals are judged per method above
            {
                var sig = sa.DecodeMethodSignature(names, null);
                Pointer("<metadata>", "", "standalone method signature #" + i, sig.ParameterTypes.Prepend(sig.ReturnType));
            }
        }
        for (var i = 1; i <= reader.GetTableRowCount(TableIndex.TypeSpec); i++)
            Pointer("<metadata>", "", "type specification #" + i, new[] { reader.GetTypeSpecification(MetadataTokens.TypeSpecificationHandle(i)).DecodeSignature(names, null) });
        for (var i = 1; i <= reader.GetTableRowCount(TableIndex.MethodSpec); i++)
            Pointer("<metadata>", "", "method specification #" + i, reader.GetMethodSpecification(MetadataTokens.MethodSpecificationHandle(i)).DecodeSignature(names, null));
    }
}
