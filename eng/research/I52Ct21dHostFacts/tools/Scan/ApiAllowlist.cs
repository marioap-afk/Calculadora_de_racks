using System.Security.Cryptography;
using System.Text;

namespace I52Ct21d.HostFacts.Tools.Scan;

public enum AllowKind
{
    /// <summary>A referenced assembly (<c>A|System.Runtime|||why</c>).</summary>
    Assembly,

    /// <summary>A referenced type, generic arguments stripped (<c>T|System.String|why</c>).</summary>
    Type,

    /// <summary>A referenced member (<c>M|Ns.Type::Member|scope|why</c>), or every member of a pure type (<c>M|Ns.Type::*|scope|why</c>).</summary>
    Member,

    /// <summary>
    /// An IL opcode that stores through a pointer or a by-ref (<c>O|stind.i4|Assembly::Type|why</c>). The scope is mandatory. Raw-memory
    /// opcodes (<c>localloc</c>, <c>cpblk</c>, <c>initblk</c>) can never be approved.
    /// </summary>
    Opcode,
}

/// <summary>
/// One approved import. <see cref="Scope"/> is empty (anywhere) or <c>Assembly::Full.Name.Of.TopLevelType</c>: the approval holds only for a
/// top-level type of that exact name DEFINED IN the assembly of that exact simple name, so a type of the same name in another assembly
/// (a spoof) gets nothing.
/// </summary>
public sealed record AllowEntry(AllowKind Kind, string Item, string Scope, string Justification, int Line)
{
    public string Key => KindLetter(Kind) + "|" + Item;

    public static char KindLetter(AllowKind k) => k switch { AllowKind.Assembly => 'A', AllowKind.Type => 'T', AllowKind.Opcode => 'O', _ => 'M' };

    /// <summary>The canonical text of a scope.</summary>
    public static string ScopeOf(string assembly, string topLevelType) => assembly + "::" + topLevelType;

    /// <summary>True when the entry applies to code defined in <paramref name="assembly"/> inside the top-level type <paramref name="topLevelType"/>.</summary>
    public bool AppliesTo(string assembly, string topLevelType) => Scope.Length == 0 || Scope == ScopeOf(assembly, topLevelType);
}

/// <summary>
/// The approved-imports list of design 5.3 item 2 (<c>allowed-apis.txt</c>). File format, one entry per line, UTF-8, LF:
/// <c>KIND|ITEM|SCOPE|JUSTIFICATION</c> with KIND in <c>A</c> (assembly), <c>T</c> (type), <c>M</c> (member) or <c>O</c> (a by-ref store opcode, always scoped). Blank lines and lines
/// starting with <c>#</c> are ignored. The justification is mandatory. A wildcard member (<c>Type::*</c>) is refused for every
/// Autodesk type and for the namespaces that can touch the disk, the process, the registry, the network, native code or reflection.
/// An entry that no assembly uses is reported by the checker (the list must stay minimal), except the few marked <c>[debug-only]</c>
/// (compiler attributes that only Debug builds emit).
/// </summary>
public sealed class ApiAllowlist
{
    private static readonly string[] WildcardForbiddenPrefixes =
    {
        "Autodesk.", "System.IO", "System.Reflection", "System.Diagnostics.Process", "System.Runtime.InteropServices", "System.Runtime.Loader",
        "System.Net", "Microsoft.Win32", "System.Threading", "System.Environment", "System.AppDomain", "System.Console", "System.Activator",
        "System.Globalization.CultureInfo", "System.Type", "System.Delegate", "System.AppContext", "System.GC", "System.Linq.Expressions",
        "System.Xml", "System.Data", "System.Security", "System.Diagnostics.Debugger",
        "System.Buffer", "System.Runtime.CompilerServices.Unsafe", "System.Runtime.CompilerServices.RuntimeHelpers", "System.Runtime.Intrinsics",
    };

    /// <summary>The store opcodes that an <c>O</c> entry may approve (a store through a pointer or a by-ref).</summary>
    public static readonly string[] ApprovableOpcodes =
        { "stind.i", "stind.i1", "stind.i2", "stind.i4", "stind.i8", "stind.r4", "stind.r8", "stind.ref", "stobj", "cpobj" };

    private readonly Dictionary<string, List<AllowEntry>> _index = new(StringComparer.Ordinal);

    public IReadOnlyList<AllowEntry> Entries { get; }

    /// <summary>SHA-256 of the list file bytes (printed by the checker so a review can name the exact list).</summary>
    public string Sha256 { get; }

    /// <summary>The entries a scan matched. Mutable on purpose: one list instance is used for the scan of all assemblies of a run.</summary>
    public HashSet<AllowEntry> Used { get; } = new();

    private ApiAllowlist(IReadOnlyList<AllowEntry> entries, string sha)
    {
        Entries = entries;
        Sha256 = sha;
        foreach (var e in entries)
        {
            if (!_index.TryGetValue(e.Key, out var list)) _index[e.Key] = list = new List<AllowEntry>();
            list.Add(e);
        }
    }

    public static ApiAllowlist LoadFile(string path) => Parse(File.ReadAllBytes(path));

    public static ApiAllowlist Parse(string text) => Parse(new UTF8Encoding(false).GetBytes(text));

    public static ApiAllowlist Parse(byte[] bytes)
    {
        var text = new UTF8Encoding(false, true).GetString(bytes);
        if (text.Contains('\r')) throw new FormatException("allowed-apis: the file must use LF line endings");
        var entries = new List<AllowEntry>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var lineNo = 0;
        foreach (var raw in text.Split('\n'))
        {
            lineNo++;
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            var parts = line.Split('|');
            if (parts.Length != 4) throw new FormatException($"allowed-apis line {lineNo}: expected KIND|ITEM|SCOPE|JUSTIFICATION");
            var kind = parts[0].Trim() switch { "A" => AllowKind.Assembly, "T" => AllowKind.Type, "M" => AllowKind.Member, "O" => AllowKind.Opcode, _ => throw new FormatException($"allowed-apis line {lineNo}: unknown kind '{parts[0]}'") };
            var item = parts[1].Trim();
            var scope = parts[2].Trim();
            var why = parts[3].Trim();
            if (item.Length == 0) throw new FormatException($"allowed-apis line {lineNo}: empty item");
            if (why.Length < 8) throw new FormatException($"allowed-apis line {lineNo}: a justification of at least 8 characters is required");
            if (kind is AllowKind.Assembly or AllowKind.Type && scope.Length > 0) throw new FormatException($"allowed-apis line {lineNo}: only member and opcode entries may carry a scope");
            if (scope.Length > 0)
            {
                var ssep = scope.IndexOf("::", StringComparison.Ordinal);
                if (ssep <= 0 || ssep + 2 >= scope.Length || scope.IndexOf("::", ssep + 2, StringComparison.Ordinal) >= 0)
                    throw new FormatException($"allowed-apis line {lineNo}: a scope must be Assembly::Full.Type.Name (the assembly that defines the type is part of the scope)");
            }
            if (kind == AllowKind.Opcode)
            {
                if (scope.Length == 0) throw new FormatException($"allowed-apis line {lineNo}: an opcode approval must be scoped to Assembly::Type");
                if (!ApprovableOpcodes.Contains(item)) throw new FormatException($"allowed-apis line {lineNo}: opcode '{item}' can never be approved (approvable: {string.Join(", ", ApprovableOpcodes)})");
            }
            if (kind == AllowKind.Member)
            {
                var sep = item.IndexOf("::", StringComparison.Ordinal);
                if (sep <= 0 || sep + 2 >= item.Length) throw new FormatException($"allowed-apis line {lineNo}: a member must be Type::Member");
                if (item.EndsWith("::*", StringComparison.Ordinal))
                {
                    var type = item[..sep];
                    if (WildcardForbiddenPrefixes.Any(p => type.StartsWith(p, StringComparison.Ordinal)))
                        throw new FormatException($"allowed-apis line {lineNo}: a wildcard is not allowed for {type}");
                }
            }
            var entry = new AllowEntry(kind, item, scope, why, lineNo);
            if (!seen.Add(entry.Key + "|" + scope)) throw new FormatException($"allowed-apis line {lineNo}: duplicate entry {entry.Key} ({scope})");
            entries.Add(entry);
        }
        return new ApiAllowlist(entries, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    /// <summary>The entries that approve an assembly or a type (any scope is irrelevant for them).</summary>
    public AllowEntry? FindSimple(AllowKind kind, string item)
    {
        if (!_index.TryGetValue(AllowEntry.KindLetter(kind) + "|" + item, out var l)) return null;
        Used.Add(l[0]);
        return l[0];
    }

    /// <summary>
    /// The member entries that approve <c>type::member</c> (the exact entry, or the type wildcard). Marks them used.
    /// </summary>
    public IReadOnlyList<AllowEntry> FindMember(string type, string member)
    {
        var found = new List<AllowEntry>();
        if (_index.TryGetValue("M|" + type + "::" + member, out var exact)) found.AddRange(exact);
        if (_index.TryGetValue("M|" + type + "::*", out var wild)) found.AddRange(wild);
        foreach (var e in found) Used.Add(e);
        return found;
    }

    /// <summary>The opcode approvals for <paramref name="opcode"/> (marks them used).</summary>
    public IReadOnlyList<AllowEntry> FindOpcode(string opcode)
    {
        if (!_index.TryGetValue("O|" + opcode, out var found)) return Array.Empty<AllowEntry>();
        foreach (var e in found) Used.Add(e);
        return found;
    }

    /// <summary>Entries nothing used: stale approvals widen the allowed surface for nothing.</summary>
    public IReadOnlyList<AllowEntry> Unused() =>
        Entries.Where(e => !Used.Contains(e) && !e.Justification.StartsWith(DebugOnlyMarker, StringComparison.Ordinal)).ToList();

    /// <summary>An entry whose justification starts with this marker is only needed by Debug builds and is exempt from the unused-entry check.</summary>
    public const string DebugOnlyMarker = "[debug-only]";
}
