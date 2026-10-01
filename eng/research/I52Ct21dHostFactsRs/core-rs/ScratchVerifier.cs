using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

/// <summary>The result of listing the scratch root against its declarations. <see cref="Ok"/> is true only when nothing is wrong.</summary>
public sealed class ScratchVerification
{
    public IReadOnlyList<string> Undeclared { get; }
    public IReadOnlyList<string> Missing { get; }
    public IReadOnlyList<string> HashMismatches { get; }
    public IReadOnlyList<string> ReparsePoints { get; }

    public bool Ok => Undeclared.Count == 0 && Missing.Count == 0 && HashMismatches.Count == 0 && ReparsePoints.Count == 0;

    public ScratchVerification(IReadOnlyList<string> undeclared, IReadOnlyList<string> missing, IReadOnlyList<string> hashMismatches, IReadOnlyList<string> reparsePoints)
    {
        Undeclared = undeclared;
        Missing = missing;
        HashMismatches = hashMismatches;
        ReparsePoints = reparsePoints;
    }

    public JsonObject ToJson() => new()
    {
        ["ok"] = Ok,
        ["undeclared"] = RecordJson.Strings(Undeclared),
        ["missing"] = RecordJson.Strings(Missing),
        ["hashMismatches"] = RecordJson.Strings(HashMismatches),
        ["reparsePoints"] = RecordJson.Strings(ReparsePoints),
    };
}

/// <summary>
/// The verifier of the cleanup contract (Owner Q-O-1: no undeclared persistent state between runs). It LISTS the scratch root, every
/// entry at every depth (hidden and system files included), and fails on: an entry that no declaration names, a declared file that is
/// missing, a declared file whose SHA-256 differs, a reparse point. It reads only. Alternate data streams cannot be listed by this API
/// (README, residual limits).
/// </summary>
public static class ScratchVerifier
{
    public static ScratchVerification Verify(string root, IReadOnlyCollection<ScratchFileEntry> declared)
    {
        var undeclared = new List<string>();
        var missing = new List<string>();
        var mismatches = new List<string>();
        var reparse = new List<string>();
        var full = PathRegions.Norm(root);
        var byName = new Dictionary<string, ScratchFileEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in declared) byName[d.Name] = d;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in Directory.EnumerateFileSystemEntries(full, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(full, entry);
            if ((File.GetAttributes(entry) & FileAttributes.ReparsePoint) != 0) reparse.Add(rel);
            var isFlatFile = !rel.Contains(Path.DirectorySeparatorChar) && File.Exists(entry);
            if (!isFlatFile || !byName.TryGetValue(rel, out var decl))
            {
                undeclared.Add(rel);
                continue;
            }
            seen.Add(rel);
            if (!string.Equals(Sha256Hex.OfFile(entry), decl.Sha256, StringComparison.Ordinal)) mismatches.Add(rel);
        }
        foreach (var d in declared)
            if (!seen.Contains(d.Name)) missing.Add(d.Name);
        undeclared.Sort(StringComparer.Ordinal);
        missing.Sort(StringComparer.Ordinal);
        mismatches.Sort(StringComparer.Ordinal);
        reparse.Sort(StringComparer.Ordinal);
        return new ScratchVerification(undeclared, missing, mismatches, reparse);
    }
}

/// <summary>One side database the instrument created: identified in the evidence, never attached to a document, disposed at the end.</summary>
public sealed class SideDbEntry
{
    public string Id { get; }
    public string Role { get; }
    public string Constructor { get; }
    public string Fill { get; }
    public string Source { get; }
    public bool Disposed { get; internal set; }

    internal SideDbEntry(string id, string role, string constructor, string fill, string source)
    {
        Id = id;
        Role = role;
        Constructor = constructor;
        Fill = fill;
        Source = source;
    }

    public JsonObject ToJson() => new()
    {
        ["id"] = Id, ["role"] = Role, ["constructor"] = Constructor, ["fill"] = Fill, ["source"] = Source, ["disposed"] = Disposed,
    };
}

/// <summary>The roster of side databases of a run (identification in the evidence; Owner Q-O-1 "identificadas en evidencia").</summary>
public sealed class SideDbLedger
{
    private readonly List<SideDbEntry> _entries = new();

    public IReadOnlyList<SideDbEntry> Entries => _entries;

    public string Register(string role, string constructor, string fill, string source)
    {
        var id = "SDB-" + (_entries.Count + 1).ToString("D2", System.Globalization.CultureInfo.InvariantCulture);
        _entries.Add(new SideDbEntry(id, role, constructor, fill, source));
        return id;
    }

    public void MarkDisposed(string id)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == id) ?? throw new InvalidOperationException("unknown side database " + id);
        entry.Disposed = true;
    }

    public bool AllDisposed => _entries.All(e => e.Disposed);
}
