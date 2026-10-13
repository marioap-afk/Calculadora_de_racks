using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

// Reads type metadata only (no loading, no AutoCAD): lists the public members of the named types in a .NET assembly.
internal sealed class Prov : ISignatureTypeProvider<string, object>
{
    private readonly MetadataReader _md;
    public Prov(MetadataReader md) => _md = md;
    public string GetArrayType(string e, ArrayShape s) => e + "[,]";
    public string GetByReferenceType(string e) => e + "&";
    public string GetFunctionPointerType(MethodSignature<string> s) => "fnptr";
    public string GetGenericInstantiation(string g, System.Collections.Immutable.ImmutableArray<string> a) => g + "<" + string.Join(",", a) + ">";
    public string GetGenericMethodParameter(object c, int i) => "!!" + i;
    public string GetGenericTypeParameter(object c, int i) => "!" + i;
    public string GetModifiedType(string m, string u, bool r) => u;
    public string GetPinnedType(string e) => e;
    public string GetPointerType(string e) => e + "*";
    public string GetPrimitiveType(PrimitiveTypeCode c) => c.ToString();
    public string GetSZArrayType(string e) => e + "[]";
    public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte k) { var t = r.GetTypeDefinition(h); return r.GetString(t.Namespace) + "." + r.GetString(t.Name); }
    public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte k) { var t = r.GetTypeReference(h); return r.GetString(t.Namespace) + "." + r.GetString(t.Name); }
    public string GetTypeFromSpecification(MetadataReader r, object c, TypeSpecificationHandle h, byte k) => "spec";
}

internal static class Program
{
    private static int Main(string[] args)
    {
        using var fs = File.OpenRead(args[0]);
        using var pe = new PEReader(fs);
        var md = pe.GetMetadataReader();
        var wanted = new HashSet<string>(args.Skip(1));
        foreach (var th in md.TypeDefinitions)
        {
            var t = md.GetTypeDefinition(th);
            var full = md.GetString(t.Namespace) + "." + md.GetString(t.Name);
            if (!wanted.Contains(full)) continue;
            Console.WriteLine("TYPE " + full + " attrs=" + t.Attributes);
            var accessors = new Dictionary<MethodDefinitionHandle, string>();
            foreach (var ph in t.GetProperties())
            {
                var p = md.GetPropertyDefinition(ph);
                var acc = p.GetAccessors();
                string Vis(MethodDefinitionHandle h) => h.IsNil ? "-" : ((md.GetMethodDefinition(h).Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public ? "public" : "nonpublic");
                Console.WriteLine("  PROP " + md.GetString(p.Name) + " get=" + Vis(acc.Getter) + " set=" + Vis(acc.Setter));
                if (!acc.Getter.IsNil) accessors[acc.Getter] = "";
                if (!acc.Setter.IsNil) accessors[acc.Setter] = "";
            }
            foreach (var mh in t.GetMethods())
            {
                if (accessors.ContainsKey(mh)) continue;
                var m = md.GetMethodDefinition(mh);
                if ((m.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public) continue;
                var name = md.GetString(m.Name);
                if (name.StartsWith("add_") || name.StartsWith("remove_")) continue;
                Console.WriteLine("  METHOD " + name + " params=" + m.GetParameters().Count + ((m.Attributes & MethodAttributes.Static) != 0 ? " static" : ""));
            }
            foreach (var eh in t.GetEvents())
                Console.WriteLine("  EVENT " + md.GetString(md.GetEventDefinition(eh).Name));
            var prov = new Prov(md);
            foreach (var ph in t.GetProperties())
            {
                var p = md.GetPropertyDefinition(ph);
                var sig = p.DecodeSignature(prov, null);
                Console.WriteLine("  PROPTYPE " + md.GetString(p.Name) + " : " + sig.ReturnType);
            }
            foreach (var mh in t.GetMethods())
            {
                var m = md.GetMethodDefinition(mh);
                var name = md.GetString(m.Name);
                if (name is ".ctor" or "InitializeFloatingPosition" or "AddVisual")
                    Console.WriteLine("  SIG " + name + "(" + string.Join(", ", m.DecodeSignature(prov, null).ParameterTypes) + ")");
            }
            foreach (var fh in t.GetFields())
            {
                var f = md.GetFieldDefinition(fh);
                if ((f.Attributes & FieldAttributes.Static) == 0) continue;
                var c = f.GetDefaultValue();
                string val = "";
                if (!c.IsNil) { var blob = md.GetBlobReader(md.GetConstant(c).Value); val = blob.Length == 4 ? blob.ReadInt32().ToString() : "?"; }
                Console.WriteLine("  FIELD " + md.GetString(f.Name) + " = " + val);
            }
        }
        return 0;
    }
}
