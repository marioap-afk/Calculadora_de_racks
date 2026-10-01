using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>
/// Emits a tiny .NET assembly whose methods call EXTERNAL members that exist only as metadata references (for example a member of
/// <c>AcDbMgd</c>), exactly the shape an instrument compiled against AutoCAD has. It needs no AutoCAD file and never loads or runs the
/// result: the assembly is only READ by the scan (negative and positive controls of the forbidden-API scan).
/// Signature types are written as <c>void bool int string obj</c>, <c>vt:Asm|Ns.Type</c> (value type) or <c>cls:Asm|Ns.Type</c>.
/// </summary>
public sealed class FixtureBuilder
{
    private readonly string _assemblyName;
    private readonly List<TypeSpec> _types = new();
    private readonly List<(string Asm, string Type, bool OnModule)> _attributes = new();
    private bool _notIlOnly;

    public FixtureBuilder(string assemblyName) => _assemblyName = assemblyName;

    /// <summary>A custom attribute (no arguments) of the module (<c>onModule</c>) or of the assembly, such as <c>UnverifiableCodeAttribute</c>.</summary>
    public FixtureBuilder Attribute(string asm, string type, bool onModule = true)
    {
        _attributes.Add((asm, type, onModule));
        return this;
    }

    /// <summary>Clears the IL-only flag of the CLI header (mixed-mode / native-code image).</summary>
    public FixtureBuilder NotIlOnly()
    {
        _notIlOnly = true;
        return this;
    }

    public TypeSpec Type(string ns, string name, string? externalBase = null)
    {
        var t = new TypeSpec(ns, name, externalBase);
        _types.Add(t);
        return t;
    }

    public sealed class TypeSpec
    {
        public string Ns { get; }
        public string Name { get; }
        public string? ExternalBase { get; }
        public List<MethodSpec> Methods { get; } = new();
        public List<(string Name, string Dll)> PInvokes { get; } = new();
        public List<(string Name, string Spec)> Fields { get; } = new();
        public List<(string Name, string Spec)> Properties { get; } = new();
        public List<(string Name, string Spec)> Events { get; } = new();

        public TypeSpec(string ns, string name, string? externalBase) { Ns = ns; Name = name; ExternalBase = externalBase; }

        public TypeSpec Method(string name, Action<Il> body) { Methods.Add(new MethodSpec(name, body)); return this; }

        /// <summary>A static method with a declared return type, parameters and locals (<c>int*</c>, <c>void*</c>, <c>fnptr</c>, <c>pinned:int</c>, <c>ref:int</c>, <c>intptr</c>...).</summary>
        public TypeSpec Method(string name, Action<Il> body, string ret, string[]? ps = null, string[]? locals = null, bool native = false)
        {
            Methods.Add(new MethodSpec(name, body) { Ret = ret, Params = ps ?? Array.Empty<string>(), Locals = locals ?? Array.Empty<string>(), Native = native });
            return this;
        }

        /// <summary>Adds a custom attribute with one string argument to the LAST method added (for example a command registration).</summary>
        public TypeSpec MethodAttribute(string asm, string type, string arg)
        {
            Methods[^1].StringAttributes.Add((asm, type, arg));
            return this;
        }

        public TypeSpec Field(string name, string spec) { Fields.Add((name, spec)); return this; }

        public TypeSpec Property(string name, string spec) { Properties.Add((name, spec)); return this; }

        /// <summary>An event whose delegate type is the (pointer) type <paramref name="spec"/> (only the signature shape matters to the scan).</summary>
        public TypeSpec Event(string name, string spec) { Events.Add((name, spec)); return this; }

        public TypeSpec PInvoke(string name, string dll) { PInvokes.Add((name, dll)); return this; }
    }

    public sealed class MethodSpec
    {
        public string Name { get; }
        public Action<Il> Body { get; }
        public string Ret { get; init; } = "void";
        public string[] Params { get; init; } = Array.Empty<string>();
        public string[] Locals { get; init; } = Array.Empty<string>();
        public bool Native { get; init; }

        /// <summary>Custom attributes of the method with ONE string argument: (assembly, type, argument), such as <c>[CommandMethod("X")]</c>.</summary>
        public List<(string Asm, string Type, string Arg)> StringAttributes { get; } = new();

        public MethodSpec(string name, Action<Il> body) { Name = name; Body = body; }
    }

    /// <summary>The instruction emitter handed to a method body.</summary>
    public sealed class Il
    {
        private readonly FixtureBuilder.Context _c;
        public InstructionEncoder Enc { get; }

        internal Il(Context c, InstructionEncoder enc) { _c = c; Enc = enc; }

        public Il LdcI4(int v) { Enc.LoadConstantI4(v); return this; }

        public Il LdArg0() { Enc.LoadArgument(0); return this; }

        public Il Pop() { Enc.OpCode(ILOpCode.Pop); return this; }

        /// <summary>A static call to a method DEFINED in this fixture assembly (a method definition, not a member reference): the shape of a call between two types of one assembly.</summary>
        public Il CallOwn(string type, string method)
        {
            Enc.OpCode(ILOpCode.Call);
            Enc.Token(_c.OwnMethod(type, method));
            return this;
        }

        /// <summary>Any opcode without an operand (<c>localloc</c>, <c>cpblk</c>, <c>initblk</c>, <c>stind.i4</c>, <c>conv.u</c>...).</summary>
        public Il Op(ILOpCode op) { Enc.OpCode(op); return this; }

        /// <summary>An opcode whose operand is a type token (<c>stobj</c>, <c>cpobj</c>).</summary>
        public Il OpType(ILOpCode op, string asm, string type)
        {
            Enc.OpCode(op);
            Enc.Token(_c.TypeRef(asm, type));
            return this;
        }

        public Il LdLoc(int i) { Enc.LoadLocal(i); return this; }

        public Il StLoc(int i) { Enc.StoreLocal(i); return this; }

        public Il Ret() { Enc.OpCode(ILOpCode.Ret); return this; }

        /// <summary>A static call to <c>asm|Ns.Type::name(params) -> ret</c>.</summary>
        public Il Call(string asm, string type, string name, string ret, params string[] ps) => Emit(ILOpCode.Call, asm, type, name, ret, ps, instance: false);

        /// <summary>An instance call (<c>callvirt</c>).</summary>
        public Il CallVirt(string asm, string type, string name, string ret, params string[] ps) => Emit(ILOpCode.Callvirt, asm, type, name, ret, ps, instance: true);

        /// <summary>A constructor call (<c>newobj</c>) of <c>asm|Ns.Type</c>.</summary>
        public Il NewObj(string asm, string type, params string[] ps) => Emit(ILOpCode.Newobj, asm, type, ".ctor", "void", ps, instance: true);

        /// <summary>An indirect call (<c>calli</c>) through a function pointer with a <c>void()</c> signature.</summary>
        public Il Calli()
        {
            Enc.CallIndirect(_c.VoidSignature());
            return this;
        }

        /// <summary>A store to an external static field of type int (<c>stsfld</c>).</summary>
        public Il StsFld(string asm, string type, string name)
        {
            Enc.OpCode(ILOpCode.Stsfld);
            Enc.Token(_c.FieldRef(asm, type, name));
            return this;
        }

        /// <summary>A <c>jmp</c> to an external method.</summary>
        public Il Jmp(string asm, string type, string name)
        {
            Enc.OpCode(ILOpCode.Jmp);
            Enc.Token(_c.MemberRef(asm, type, name, "void", Array.Empty<string>(), instance: false));
            return this;
        }

        private Il Emit(ILOpCode op, string asm, string type, string name, string ret, string[] ps, bool instance)
        {
            var handle = _c.MemberRef(asm, type, name, ret, ps, instance);
            Enc.OpCode(op);
            Enc.Token(handle);
            return this;
        }
    }

    public byte[] Build()
    {
        var mb = new MetadataBuilder();
        var ilStream = new BlobBuilder();
        var bodies = new MethodBodyStreamEncoder(ilStream);
        var ctx = new Context(mb);
        var rowCounter = 0;
        foreach (var t in _types)
        {
            foreach (var m in t.Methods) ctx.OwnMethods[t.Ns + "." + t.Name + "::" + m.Name] = MetadataTokens.MethodDefinitionHandle(++rowCounter);
            rowCounter += t.PInvokes.Count;
        }

        mb.AddModule(0, mb.GetOrAddString(_assemblyName + ".dll"), mb.GetOrAddGuid(new Guid("11111111-2222-3333-4444-555555555555")), default, default);
        mb.AddAssembly(mb.GetOrAddString(_assemblyName), new Version(1, 0, 0, 0), default, default, 0, AssemblyHashAlgorithm.None);

        // <Module> type: owns nothing
        mb.AddTypeDefinition(0, default, mb.GetOrAddString("<Module>"), default, MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));

        var objectType = ctx.TypeRef("System.Runtime", "System.Object");
        var methodRow = 0;
        var fieldRow = 0;
        var propertyRow = 0;
        var eventRow = 0;
        var typeRow = 1; // <Module>
        foreach (var t in _types)
        {
            typeRow++;
            var baseType = t.ExternalBase is null ? objectType : ctx.TypeRef(t.ExternalBase.Split('|')[0], t.ExternalBase.Split('|')[1]);
            mb.AddTypeDefinition(TypeAttributes.Public | TypeAttributes.Class, mb.GetOrAddString(t.Ns), mb.GetOrAddString(t.Name), baseType,
                MetadataTokens.FieldDefinitionHandle(fieldRow + 1), MetadataTokens.MethodDefinitionHandle(methodRow + 1));

            foreach (var (fname, fspec) in t.Fields)
            {
                var fb = new BlobBuilder();
                ctx.Encode(new BlobEncoder(fb).FieldSignature(), fspec);
                mb.AddFieldDefinition(FieldAttributes.Public | FieldAttributes.Static, mb.GetOrAddString(fname), mb.GetOrAddBlob(fb));
                fieldRow++;
            }
            foreach (var m in t.Methods)
            {
                var enc = new InstructionEncoder(new BlobBuilder());
                m.Body(new Il(ctx, enc));
                var localSig = m.Locals.Length == 0 ? default : ctx.LocalSignature(m.Locals);
                var offset = bodies.AddMethodBody(enc, 8, localSig, MethodBodyAttributes.None);
                var methodHandle = mb.AddMethodDefinition(MethodAttributes.Public | MethodAttributes.Static, m.Native ? MethodImplAttributes.Native : MethodImplAttributes.IL,
                    mb.GetOrAddString(m.Name), ctx.StaticSignature(m.Ret, m.Params), offset, default);
                methodRow++;
                foreach (var (aAsm, aType, aArg) in m.StringAttributes)
                {
                    var ctor = ctx.MemberRef(aAsm, aType, ".ctor", "void", new[] { "string" }, instance: true);
                    var attrBlob = new BlobBuilder();
                    attrBlob.WriteUInt16(1); // prolog
                    attrBlob.WriteSerializedString(aArg);
                    attrBlob.WriteUInt16(0); // no named arguments
                    mb.AddCustomAttribute(methodHandle, ctor, mb.GetOrAddBlob(attrBlob));
                }
            }
            if (t.Properties.Count > 0)
            {
                mb.AddPropertyMap(MetadataTokens.TypeDefinitionHandle(typeRow), MetadataTokens.PropertyDefinitionHandle(propertyRow + 1));
                foreach (var (pname, pspec) in t.Properties)
                {
                    var pb = new BlobBuilder();
                    new BlobEncoder(pb).PropertySignature(isInstanceProperty: false).Parameters(0, r => ctx.Encode(r.Type(), pspec), _ => { });
                    mb.AddProperty(PropertyAttributes.None, mb.GetOrAddString(pname), mb.GetOrAddBlob(pb));
                    propertyRow++;
                }
            }
            if (t.Events.Count > 0)
            {
                mb.AddEventMap(MetadataTokens.TypeDefinitionHandle(typeRow), MetadataTokens.EventDefinitionHandle(eventRow + 1));
                foreach (var (ename, espec) in t.Events)
                {
                    var tb = new BlobBuilder();
                    ctx.Encode(new BlobEncoder(tb).TypeSpecificationSignature(), espec);
                    mb.AddEvent(EventAttributes.None, mb.GetOrAddString(ename), mb.AddTypeSpecification(mb.GetOrAddBlob(tb)));
                    eventRow++;
                }
            }
            foreach (var (name, dll) in t.PInvokes)
            {
                var m = mb.AddMethodDefinition(MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl, MethodImplAttributes.IL,
                    mb.GetOrAddString(name), ctx.StaticVoidSignature(), -1, default);
                mb.AddMethodImport(m, MethodImportAttributes.None, mb.GetOrAddString(name), mb.AddModuleReference(mb.GetOrAddString(dll)));
                methodRow++;
            }
        }

        foreach (var (asm, type, onModule) in _attributes)
        {
            var ctor = ctx.MemberRef(asm, type, ".ctor", "void", Array.Empty<string>(), instance: true);
            var value = new BlobBuilder();
            value.WriteUInt16(1); // prolog
            value.WriteUInt16(0); // no named arguments
            mb.AddCustomAttribute(onModule ? EntityHandle.ModuleDefinition : EntityHandle.AssemblyDefinition, ctor, mb.GetOrAddBlob(value));
        }

        var header = new PEHeaderBuilder(imageCharacteristics: Characteristics.Dll | Characteristics.ExecutableImage);
        var pe = new ManagedPEBuilder(header, new MetadataRootBuilder(mb), ilStream, flags: _notIlOnly ? (CorFlags)0 : CorFlags.ILOnly);
        var blob = new BlobBuilder();
        pe.Serialize(blob);
        return blob.ToArray();
    }

    internal sealed class Context
    {
        private readonly MetadataBuilder _mb;
        private readonly Dictionary<string, AssemblyReferenceHandle> _asm = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TypeReferenceHandle> _types = new(StringComparer.Ordinal);

        public Context(MetadataBuilder mb) => _mb = mb;

        /// <summary>The method definitions of the fixture by <c>Ns.Type::Method</c>.</summary>
        public Dictionary<string, MethodDefinitionHandle> OwnMethods { get; } = new(StringComparer.Ordinal);

        public MethodDefinitionHandle OwnMethod(string type, string method) => OwnMethods[type + "::" + method];

        public TypeReferenceHandle TypeRef(string asm, string fullName)
        {
            var key = asm + "|" + fullName;
            if (_types.TryGetValue(key, out var h)) return h;
            if (!_asm.TryGetValue(asm, out var ah))
                _asm[asm] = ah = _mb.AddAssemblyReference(_mb.GetOrAddString(asm), new Version(1, 0, 0, 0), default, default, 0, default);
            var dot = fullName.LastIndexOf('.');
            var ns = dot < 0 ? "" : fullName[..dot];
            var name = dot < 0 ? fullName : fullName[(dot + 1)..];
            return _types[key] = _mb.AddTypeReference(ah, _mb.GetOrAddString(ns), _mb.GetOrAddString(name));
        }

        public StandaloneSignatureHandle VoidSignature()
        {
            var b = new BlobBuilder();
            new BlobEncoder(b).MethodSignature().Parameters(0, r => r.Void(), _ => { });
            return _mb.AddStandaloneSignature(_mb.GetOrAddBlob(b));
        }

        public MemberReferenceHandle FieldRef(string asm, string type, string name)
        {
            var b = new BlobBuilder();
            new BlobEncoder(b).FieldSignature().Int32();
            return _mb.AddMemberReference(TypeRef(asm, type), _mb.GetOrAddString(name), _mb.GetOrAddBlob(b));
        }

        public BlobHandle StaticVoidSignature()
        {
            var b = new BlobBuilder();
            new BlobEncoder(b).MethodSignature().Parameters(0, r => r.Void(), _ => { });
            return _mb.GetOrAddBlob(b);
        }

        public BlobHandle StaticSignature(string ret, string[] ps)
        {
            var b = new BlobBuilder();
            new BlobEncoder(b).MethodSignature().Parameters(ps.Length,
                r => { if (ret == "void") r.Void(); else Encode(r.Type(), ret); },
                p => { foreach (var s in ps) Encode(p.AddParameter().Type(), s); });
            return _mb.GetOrAddBlob(b);
        }

        /// <summary>A local-variable signature. A spec may be prefixed <c>pinned:</c> or <c>ref:</c>.</summary>
        public StandaloneSignatureHandle LocalSignature(string[] locals)
        {
            var b = new BlobBuilder();
            var enc = new BlobEncoder(b).LocalVariableSignature(locals.Length);
            foreach (var l in locals)
            {
                var pinned = l.StartsWith("pinned:", StringComparison.Ordinal);
                var byRef = l.StartsWith("ref:", StringComparison.Ordinal);
                var spec = pinned ? l["pinned:".Length..] : byRef ? l["ref:".Length..] : l;
                Encode(enc.AddVariable().Type(byRef, pinned), spec);
            }
            return _mb.AddStandaloneSignature(_mb.GetOrAddBlob(b));
        }

        public MemberReferenceHandle MemberRef(string asm, string type, string name, string ret, string[] ps, bool instance)
        {
            var parent = TypeRef(asm, type);
            var b = new BlobBuilder();
            new BlobEncoder(b).MethodSignature(isInstanceMethod: instance).Parameters(ps.Length,
                r => { if (ret == "void") r.Void(); else Encode(r.Type(), ret); },
                p => { foreach (var s in ps) Encode(p.AddParameter().Type(), s); });
            return _mb.AddMemberReference(parent, _mb.GetOrAddString(name), _mb.GetOrAddBlob(b));
        }

        internal void Encode(SignatureTypeEncoder t, string spec)
        {
            switch (spec)
            {
                case "bool": t.Boolean(); return;
                case "int": t.Int32(); return;
                case "long": t.Int64(); return;
                case "intptr": t.IntPtr(); return;
                case "string": t.String(); return;
                case "obj": t.Object(); return;
                case "void*": t.VoidPointer(); return;
                case "fnptr": t.FunctionPointer().Parameters(0, r => r.Void(), _ => { }); return;
            }
            if (spec.EndsWith('*')) { Encode(t.Pointer(), spec[..^1]); return; }
            var isValueType = spec.StartsWith("vt:", StringComparison.Ordinal);
            if (!isValueType && !spec.StartsWith("cls:", StringComparison.Ordinal)) throw new ArgumentException("bad signature spec " + spec);
            var parts = spec[(spec.IndexOf(':') + 1)..].Split('|');
            t.Type(TypeRef(parts[0], parts[1]), isValueType);
        }
    }
}
