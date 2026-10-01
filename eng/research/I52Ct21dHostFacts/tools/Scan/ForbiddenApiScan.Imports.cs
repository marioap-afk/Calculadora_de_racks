using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace I52Ct21d.HostFacts.Tools.Scan;

/// <summary>
/// The IMPORTS-ALLOWLIST BACKSTOP (design 5.3 item 2): the scan is deny-by-default. Every assembly reference, every type reference and
/// every member reference of the assembly (all rows of the metadata tables, so a reference reached by a call, a delegate, a token, a
/// custom attribute or reflection by token is covered) must be approved in <c>allowed-apis.txt</c>; and at every instruction that
/// names an external member the scope of the approval is checked (an approval may be limited to one top-level type, as the file writes
/// are limited to the EvidenceWriter). A reference that is not approved is a violation whatever the deny rules say: the deny rules are the
/// second layer, this is the first.
/// </summary>
public static partial class ForbiddenApiScan
{
    /// <summary>Returns the number of metadata rows examined (assembly, type and member references).</summary>
    private static int CheckImportsMetadata(MetadataReader reader, NameProvider names, ApiAllowlist al, string displayName, List<Violation> sink)
    {
        var rows = 0;
        void Add(string rule, string detail) => sink.Add(new Violation(displayName, "<metadata>", "", -1, rule, detail));

        foreach (var h in reader.AssemblyReferences)
        {
            rows++;
            var an = reader.GetString(reader.GetAssemblyReference(h).Name);
            if (al.FindSimple(AllowKind.Assembly, an) is null) Add("R-IMPORT-ASSEMBLY", "assembly reference " + an + " is not in allowed-apis.txt");
        }
        foreach (var h in reader.TypeReferences)
        {
            rows++;
            var full = StripGenerics(names.Full(h));
            if (al.FindSimple(AllowKind.Type, full) is null) Add("R-IMPORT-TYPE", "type reference " + full + " is not in allowed-apis.txt");
        }
        foreach (var h in reader.MemberReferences)
        {
            rows++;
            var mr = reader.GetMemberReference(h);
            var name = reader.GetString(mr.Name);
            switch (mr.Parent.Kind)
            {
                case HandleKind.TypeReference:
                case HandleKind.TypeSpecification:
                {
                    var type = StripGenerics(names.Full(mr.Parent));
                    if (IsArrayType(type)) break; // array accessors (Get, Set, Address, .ctor) are intrinsic and harmless
                    if (al.FindMember(type, name).Count == 0) Add("R-IMPORT-MEMBER", "member reference " + type + "::" + name + " is not in allowed-apis.txt");
                    break;
                }
                default:
                    Add("R-IMPORT-MEMBER", "member reference " + name + " with a parent of kind " + mr.Parent.Kind + " (vararg or module-level) is never allowed");
                    break;
            }
        }
        return rows;
    }

    /// <summary>The scope check at an instruction that names an external member (the metadata pass already required an approval).</summary>
    private static void CheckImportsAtInstruction(Instr ins, NameProvider names, string displayName, string asmName, string typeName, string methodName, string top, ApiAllowlist al, List<Violation> sink)
    {
        if (ins.Op.OperandType is not (System.Reflection.Emit.OperandType.InlineMethod or System.Reflection.Emit.OperandType.InlineField or System.Reflection.Emit.OperandType.InlineTok))
            return;
        var handle = MetadataTokens.EntityHandle((int)ins.Operand);
        if (handle.Kind is not (HandleKind.MemberReference or HandleKind.MethodSpecification)) return;
        var target = names.Resolve(handle);
        if (target is null) return;
        var type = StripGenerics(target.DeclType);
        if (IsArrayType(type)) return;
        var entries = al.FindMember(type, target.Name);
        if (entries.Count == 0) return; // reported by the metadata pass
        if (entries.Any(e => e.AppliesTo(asmName, top))) return;
        sink.Add(new Violation(displayName, typeName, methodName, ins.Offset, "R-IMPORT-SCOPE",
            type + "::" + target.Name + " is approved only inside " + string.Join(", ", entries.Select(e => e.Scope).Distinct()) + ", used inside " + AllowEntry.ScopeOf(asmName, top)));
    }

    private static bool IsArrayType(string t) => t.EndsWith(']');

    /// <summary>
    /// Lists the distinct imports of an assembly in the allowed-apis.txt syntax, with the scopes in which they occur. This is a
    /// REVIEW AID used once to draft the list from the current code; nothing is approved until a reviewer reads each line.
    /// </summary>
    public static IReadOnlyList<(AllowKind Kind, string Item, IReadOnlyCollection<string> Scopes)> ListImports(byte[] image)
    {
        using var pe = new System.Reflection.PortableExecutable.PEReader(System.Collections.Immutable.ImmutableArray.Create(image));
        var reader = pe.GetMetadataReader();
        var names = new NameProvider(reader);
        var asmName = reader.GetString(reader.GetAssemblyDefinition().Name);
        var result = new Dictionary<(AllowKind, string), SortedSet<string>>();
        void Put(AllowKind k, string item, string? scope)
        {
            if (!result.TryGetValue((k, item), out var set)) result[(k, item)] = set = new SortedSet<string>(StringComparer.Ordinal);
            if (scope is not null) set.Add(scope);
        }
        foreach (var h in reader.AssemblyReferences) Put(AllowKind.Assembly, reader.GetString(reader.GetAssemblyReference(h).Name), null);
        foreach (var h in reader.TypeReferences) Put(AllowKind.Type, StripGenerics(names.Full(h)), null);
        foreach (var h in reader.MemberReferences)
        {
            var mr = reader.GetMemberReference(h);
            if (mr.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeSpecification)) continue;
            var type = StripGenerics(names.Full(mr.Parent));
            if (!IsArrayType(type)) Put(AllowKind.Member, type + "::" + reader.GetString(mr.Name), null);
        }
        foreach (var th in reader.TypeDefinitions)
        {
            var top = names.Full(th).Split('/')[0];
            foreach (var mh in reader.GetTypeDefinition(th).GetMethods())
            {
                var m = reader.GetMethodDefinition(mh);
                if (m.RelativeVirtualAddress == 0) continue;
                var il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILBytes();
                if (il is null) continue;
                foreach (var ins in IlReader.Decode(il))
                {
                    if (ins.Op.OperandType is not (System.Reflection.Emit.OperandType.InlineMethod or System.Reflection.Emit.OperandType.InlineField or System.Reflection.Emit.OperandType.InlineTok)) continue;
                    var handle = MetadataTokens.EntityHandle((int)ins.Operand);
                    if (handle.Kind is not (HandleKind.MemberReference or HandleKind.MethodSpecification)) continue;
                    var t = names.Resolve(handle);
                    if (t is null || IsArrayType(StripGenerics(t.DeclType))) continue;
                    Put(AllowKind.Member, StripGenerics(t.DeclType) + "::" + t.Name, AllowEntry.ScopeOf(asmName, top));
                }
            }
        }
        return result.OrderBy(p => p.Key.Item1).ThenBy(p => p.Key.Item2, StringComparer.Ordinal)
            .Select(p => (p.Key.Item1, p.Key.Item2, (IReadOnlyCollection<string>)p.Value)).ToList();
    }
}
