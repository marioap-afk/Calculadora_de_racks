using System.Security.Cryptography;
using System.Text;
using I52Ct21d.HostFacts.Tools.Scan;

namespace I52Ct21d.HostFacts.Rs.Tools;

/// <summary>One line of rs-waivers.txt: a rule of the R0 scan that is waived inside ONE top-level type of the RS assembly, for the violations whose detail contains a fragment.</summary>
public sealed class WaiverEntry
{
    public string Rule { get; }
    public string Type { get; }
    public string Fragment { get; }
    public string Justification { get; }
    public int Line { get; }

    public WaiverEntry(string rule, string type, string fragment, string justification, int line)
    {
        Rule = rule;
        Type = type;
        Fragment = fragment;
        Justification = justification;
        Line = line;
    }

    public string Key => Rule + "|" + Type + "|" + Fragment;
}

/// <summary>
/// The WAIVERS of the RS scan (<c>rs-waivers.txt</c>, format <c>RULE|Full.Type.Name|DETAIL_FRAGMENT|WHY</c>, UTF-8, LF). The R0 scan forbids the write
/// members outright; the RS DLL needs a few of them, in two types. A waiver does not approve an import (that is <c>allowed-apis-rs.txt</c>, scoped
/// to the same types): it removes ONE scan finding, in ONE type, when its detail names the expected member. The maximum a file can waive is fixed
/// in the source of the scan (<see cref="Waivable"/>): a line outside it is a format error, so editing the file alone cannot widen the privilege.
/// </summary>
public sealed class RsWaivers
{
    /// <summary>For each waivable rule: the types that may carry it and the detail fragments that may be waived. THIS TABLE IS THE LIMIT of what a waiver file can do.</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<(string Type, string Fragment)>> Waivable =
        new Dictionary<string, IReadOnlyList<(string, string)>>(StringComparer.Ordinal)
        {
            ["R-DB-CTOR"] = new[] { (RsLayer.SideDbWriterType, "Database::.ctor") },
            ["R-NEWOBJ-OBJECT"] = new[] { (RsLayer.SideDbWriterType, "::.ctor (object or entity creation)") },
            ["R-AUTODESK-SETTER"] = new[] { (RsLayer.SideDbWriterType, "(property write)") },
            ["R-AUTODESK-MUTATOR-NAME"] = new[] { (RsLayer.SideDbWriterType, "(conservative name heuristic)"), (RsLayer.ScratchSaverType, "Database::SaveAs") },
            ["R-COMMIT"] = new[] { (RsLayer.SideDbWriterType, "Transaction::Commit") },
            ["R-WRITE-OPEN"] = new[] { (RsLayer.SideDbWriterType, "Transaction::GetObject (OpenMode constant 1 (ForWrite or ForNotify)") },
            ["R-DB-SAVE"] = new[] { (RsLayer.SideDbWriterType, "Database::WblockCloneObjects"), (RsLayer.ScratchSaverType, "Database::SaveAs") },
            ["R-TRANSACTION-SCOPE"] = new[] { (RsLayer.SideDbReaderType, "(outside the side-database reader)") },
        };

    public IReadOnlyList<WaiverEntry> Entries { get; }

    public string Sha256 { get; }

    public HashSet<WaiverEntry> Used { get; } = new();

    private RsWaivers(IReadOnlyList<WaiverEntry> entries, string sha)
    {
        Entries = entries;
        Sha256 = sha;
    }

    public static RsWaivers LoadFile(string path) => Parse(File.ReadAllBytes(path));

    public static RsWaivers Parse(string text) => Parse(new UTF8Encoding(false).GetBytes(text));

    public static RsWaivers Parse(byte[] bytes)
    {
        var text = new UTF8Encoding(false, true).GetString(bytes);
        if (text.Contains('\r')) throw new FormatException("rs-waivers: the file must use LF line endings");
        var entries = new List<WaiverEntry>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var no = 0;
        foreach (var raw in text.Split('\n'))
        {
            no++;
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            var parts = line.Split('|');
            if (parts.Length != 4) throw new FormatException("rs-waivers line " + no + ": expected RULE|TYPE|DETAIL_FRAGMENT|WHY");
            var rule = parts[0].Trim();
            var type = parts[1].Trim();
            var fragment = parts[2].Trim();
            var why = parts[3].Trim();
            if (!Waivable.TryGetValue(rule, out var allowed)) throw new FormatException("rs-waivers line " + no + ": the rule " + rule + " can never be waived");
            if (!allowed.Any(a => a.Type == type && a.Fragment == fragment))
                throw new FormatException("rs-waivers line " + no + ": " + rule + " may be waived only for the (type, fragment) pairs fixed in the scan: " + string.Join(" ; ", allowed.Select(a => a.Type + " / " + a.Fragment)));
            if (why.Length < 8) throw new FormatException("rs-waivers line " + no + ": a justification of at least 8 characters is required");
            var entry = new WaiverEntry(rule, type, fragment, why, no);
            if (!seen.Add(entry.Key)) throw new FormatException("rs-waivers line " + no + ": duplicate entry");
            entries.Add(entry);
        }
        return new RsWaivers(entries, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    /// <summary>The waiver that covers a violation of the assembly <paramref name="assembly"/>, or null (marks it used).</summary>
    public WaiverEntry? Find(string assembly, Violation v)
    {
        if (assembly != RsLayer.RsAssembly) return null;
        var top = v.Type.Split('/')[0];
        foreach (var e in Entries)
        {
            if (e.Rule == v.Rule && e.Type == top && v.Detail.Contains(e.Fragment, StringComparison.Ordinal))
            {
                Used.Add(e);
                return e;
            }
        }
        return null;
    }

    public IReadOnlyList<WaiverEntry> Unused() => Entries.Where(e => !Used.Contains(e)).ToList();
}
