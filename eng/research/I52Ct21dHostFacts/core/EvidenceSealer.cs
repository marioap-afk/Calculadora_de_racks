using System.Text;

namespace I52Ct21d.HostFacts.Core;

public sealed record SealResult(
    bool Sealed,
    bool NoChange,
    IReadOnlyList<WrittenFile> Written,
    IReadOnlyList<string> Differences,
    string? HashesFileSha256);

/// <summary>
/// I-9 evidence seal (design 5.2): writes <c>HASHES.sha256</c> (one <c>sha256  name</c> line per file, names in ordinal order, LF) and
/// its own digest file for an evidence folder, then marks the files read-only. A second run on a SEALED folder verifies and reports
/// <c>NoChange</c> or the differences; it never writes. All writing goes through the <see cref="EvidenceWriter"/>.
/// The evidence folder is flat: a subdirectory is reported as a difference (the folder holds only declared outputs, design 5.3 item 6).
/// </summary>
public static class EvidenceSealer
{
    public const string HashesFile = "HASHES.sha256";
    public const string DigestFile = "HASHES.sha256.digest";

    public static SealResult SealOrVerify(string folder, bool markReadOnly = true)
    {
        var differences = new List<string>();
        foreach (var dir in Directory.EnumerateDirectories(folder))
            differences.Add("UNDECLARED_DIRECTORY " + Path.GetFileName(dir));

        var current = Directory.EnumerateFiles(folder)
            .Select(Path.GetFileName)
            .Where(n => n is not (HashesFile or DigestFile))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();
        var hashes = current.ToDictionary(n => n!, n => Sha256Hex.OfFile(Path.Combine(folder, n!)), StringComparer.Ordinal);

        var hashesPath = Path.Combine(folder, HashesFile);
        if (File.Exists(hashesPath))
            return Verify(folder, hashes, differences);

        if (differences.Count > 0)
            return new SealResult(false, false, Array.Empty<WrittenFile>(), differences, null); // not sealed: undeclared content

        var writer = new EvidenceWriter(folder);
        var sb = new StringBuilder();
        foreach (var name in hashes.Keys.OrderBy(k => k, StringComparer.Ordinal))
            sb.Append(hashes[name]).Append("  ").Append(name).Append('\n');
        var file = writer.WriteNewText(HashesFile, sb.ToString());
        writer.WriteNewText(DigestFile, file.Sha256 + "  " + HashesFile + "\n");
        if (markReadOnly)
        {
            foreach (var name in hashes.Keys) writer.MarkReadOnly(name);
            writer.MarkReadOnly(HashesFile);
            writer.MarkReadOnly(DigestFile);
        }
        return new SealResult(true, false, writer.Ledger.ToList(), differences, file.Sha256);
    }

    private static SealResult Verify(string folder, Dictionary<string, string> actual, List<string> differences)
    {
        var recorded = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in File.ReadAllText(Path.Combine(folder, HashesFile)).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var sep = line.IndexOf("  ", StringComparison.Ordinal);
            if (sep != 64) { differences.Add("MALFORMED_LINE " + line); continue; }
            recorded[line[(sep + 2)..]] = line[..sep];
        }
        foreach (var (name, hash) in recorded)
        {
            if (!actual.TryGetValue(name, out var now)) differences.Add("MISSING " + name);
            else if (!string.Equals(now, hash, StringComparison.Ordinal)) differences.Add("CHANGED " + name);
        }
        foreach (var name in actual.Keys)
            if (!recorded.ContainsKey(name)) differences.Add("EXTRA " + name);

        var hashesSha = Sha256Hex.OfFile(Path.Combine(folder, HashesFile));
        var digestPath = Path.Combine(folder, DigestFile);
        if (!File.Exists(digestPath)) differences.Add("MISSING " + DigestFile);
        else if (!File.ReadAllText(digestPath).StartsWith(hashesSha + "  " + HashesFile, StringComparison.Ordinal)) differences.Add("CHANGED " + HashesFile);

        differences.Sort(StringComparer.Ordinal);
        return new SealResult(true, differences.Count == 0, Array.Empty<WrittenFile>(), differences, hashesSha);
    }
}
