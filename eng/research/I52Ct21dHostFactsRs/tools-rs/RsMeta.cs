using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace I52Ct21d.HostFacts.Rs.Tools;

/// <summary>A member that an IL instruction names: its declaring type (full name), the assembly that defines that type, its name and parameter types.</summary>
public sealed class CalleeInfo
{
    public string TypeFull { get; }
    public string Name { get; }
    public IReadOnlyList<string> Params { get; }
    public string DefiningAssembly { get; }
    public bool IsDefinition { get; }

    public string TopType => TypeFull.Split('/')[0];

    public CalleeInfo(string typeFull, string name, IReadOnlyList<string> parameters, string definingAssembly, bool isDefinition)
    {
        TypeFull = typeFull;
        Name = name;
        Params = parameters;
        DefiningAssembly = definingAssembly;
        IsDefinition = isDefinition;
    }
}

/// <summary>
/// Resolves metadata handles to full type names and to the member an instruction names. The RS layer of the scan (RsLayer) needs this for the
/// scoping rules and the pinned surface; it is a compact counterpart of the name provider of the R0 scan (which is private to that class).
/// </summary>
public sealed class RsNames : ISignatureTypeProvider<string, object?>
{
    private readonly MetadataReader _r;
    private readonly string _ownAssembly;

    public RsNames(MetadataReader reader)
    {
        _r = reader;
        _ownAssembly = reader.GetString(reader.GetAssemblyDefinition().Name);
    }

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

    private string? ScopeAssembly(TypeReferenceHandle h)
    {
        var tr = _r.GetTypeReference(h);
        while (tr.ResolutionScope.Kind == HandleKind.TypeReference) tr = _r.GetTypeReference((TypeReferenceHandle)tr.ResolutionScope);
        return tr.ResolutionScope.Kind == HandleKind.AssemblyReference
            ? _r.GetString(_r.GetAssemblyReference((AssemblyReferenceHandle)tr.ResolutionScope).Name)
            : null;
    }

    /// <summary>The member that an instruction operand names (a method definition, a member reference, a method specification), or null.</summary>
    public CalleeInfo? Resolve(EntityHandle handle)
    {
        if (handle.Kind == HandleKind.MethodSpecification) handle = _r.GetMethodSpecification((MethodSpecificationHandle)handle).Method;
        switch (handle.Kind)
        {
            case HandleKind.MethodDefinition:
            {
                var md = _r.GetMethodDefinition((MethodDefinitionHandle)handle);
                MethodSignature<string> sig;
                try { sig = md.DecodeSignature(this, null); }
                catch (BadImageFormatException) { return new CalleeInfo(Full(md.GetDeclaringType()), _r.GetString(md.Name), new[] { "<undecodable>" }, _ownAssembly, true); }
                return new CalleeInfo(Full(md.GetDeclaringType()), _r.GetString(md.Name), sig.ParameterTypes.ToList(), _ownAssembly, true);
            }
            case HandleKind.MemberReference:
            {
                var mr = _r.GetMemberReference((MemberReferenceHandle)handle);
                if (mr.Parent.Kind != HandleKind.TypeReference && mr.Parent.Kind != HandleKind.TypeSpecification) return null;
                var type = Full(mr.Parent);
                var asm = mr.Parent.Kind == HandleKind.TypeReference ? ScopeAssembly((TypeReferenceHandle)mr.Parent) ?? "" : "";
                var parameters = new List<string>();
                if (mr.GetKind() == MemberReferenceKind.Method)
                {
                    try { parameters = mr.DecodeMethodSignature(this, null).ParameterTypes.ToList(); }
                    catch (BadImageFormatException) { parameters = new List<string> { "<undecodable>" }; }
                }
                return new CalleeInfo(type, _r.GetString(mr.Name), parameters, asm, false);
            }
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

    public string GetFunctionPointerType(MethodSignature<string> signature) => "<fnptr>";

    public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;

    public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;

    public string GetPinnedType(string elementType) => "pinned " + elementType;

    public static EntityHandle Handle(long operand) => MetadataTokens.EntityHandle((int)operand);
}
