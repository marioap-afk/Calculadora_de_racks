using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using I52Ct21d.HostFacts.Tools.Scan;

namespace I52Ct21d.HostFacts.Rs.Tools;

/// <summary>
/// Offline RS tools. Nothing here starts, loads or talks to AutoCAD, and NOTHING here writes a file: every command reads and prints.
/// Exit codes: 0 ok, 1 usage or malformed list, 2 scan or verification violations, 4 ledger differences.
///
///   scan-rs --allowed a.txt --waivers w.txt --privileged-surface s.txt --privileged-callers c.txt --expected-commands e.txt role:path...
///                                      role = core | tools | rs. The R0 scan configured for the RS assemblies, the scoped waivers, the RS layer
///                                      and the stale-entry checks. All five options are mandatory.
///   imports-list-rs role:path...       drafts allowed-apis-rs.txt lines (a review aid: nothing is approved by listing).
///   privileged-list-rs role:path...    prints the observed RS privileged surface and callers, and the command registrations (a review aid).
///   scratch-verify scratchRoot evidenceRoot   lists the scratch root and fails on any entry that the rs-run-*.json records do not declare.
///   tolscale-verify tolscale-raw.json  the offline recomputation of design 2.4 (schema + independent statistics).
///   ledger-verify ledger.txt root      verifies the SHA-256 lines of a hashes ledger against the files under root.
///   hash path...                       prints the SHA-256 of files.
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0) return Usage();
        var rest = args.Skip(1).ToList();
        switch (args[0])
        {
            case "scan-rs": return ScanRs(rest);
            case "imports-list-rs": return ImportsList(rest);
            case "privileged-list-rs": return PrivilegedList(rest);
            case "scratch-verify": return ScratchVerify(rest);
            case "tolscale-verify": return TolScaleVerify(rest);
            case "ledger-verify": return LedgerVerify(rest);
            case "hash": return Hash(rest);
            default: return Usage();
        }
    }

    private static int Usage()
    {
        Console.Error.WriteLine("usage: scan-rs --allowed a --waivers w --privileged-surface s --privileged-callers c --expected-commands e role:path... | imports-list-rs role:path... | privileged-list-rs role:path... | scratch-verify scratchRoot evidenceRoot | tolscale-verify file | ledger-verify ledger root | hash path...");
        return 1;
    }

    private static ScanRole? ParseRole(string role) => role.ToLowerInvariant() switch
    {
        "core" => ScanRole.Core,
        "tools" => ScanRole.Tools,
        "rs" => ScanRole.R0,
        _ => (ScanRole?)null,
    };

    private static List<RsScanInput>? ParseSpecs(IEnumerable<string> specs)
    {
        var parsed = new List<RsScanInput>();
        foreach (var spec in specs)
        {
            var colon = spec.IndexOf(':');
            if (colon <= 0) return null;
            var role = ParseRole(spec[..colon]);
            if (role is null) return null;
            parsed.Add(new RsScanInput(role.Value, spec[(colon + 1)..]));
        }
        return parsed.Count > 0 ? parsed : null;
    }

    private static int ScanRs(List<string> args)
    {
        if (args.Count < 11 || args[0] != "--allowed" || args[2] != "--waivers" || args[4] != "--privileged-surface" || args[6] != "--privileged-callers" || args[8] != "--expected-commands")
            return Usage();
        ApiAllowlist allowlist;
        RsWaivers waivers;
        ScanPolicy policy;
        try
        {
            allowlist = ApiAllowlist.LoadFile(args[1]);
            waivers = RsWaivers.LoadFile(args[3]);
            policy = ScanPolicy.LoadFiles(args[5], args[7], args[9]);
        }
        catch (Exception ex) when (ex is FormatException or IOException or System.Text.DecoderFallbackException)
        {
            Console.Error.WriteLine("scan lists: " + ex.Message);
            return 1;
        }
        var specs = ParseSpecs(args.Skip(10));
        if (specs is null) return Usage();
        var result = RsScan.Run(specs, allowlist, waivers, policy);
        Console.Out.Write(RsScan.ToText(result));
        Console.Out.WriteLine("allowed-apis-rs.txt sha256 " + allowlist.Sha256 + " (" + allowlist.Entries.Count + " entries)");
        Console.Out.WriteLine("rs-waivers.txt sha256 " + waivers.Sha256 + " (" + waivers.Entries.Count + " entries)");
        Console.Out.WriteLine("privileged-surface-rs.txt sha256 " + policy.SurfaceSha256 + " (" + policy.Surface.Values.Sum(v => v.Count) + " methods)");
        Console.Out.WriteLine("privileged-callers-rs.txt sha256 " + policy.CallersSha256 + " (" + policy.Callers.Count + " entries)");
        Console.Out.WriteLine("expected-commands-rs.txt sha256 " + policy.CommandsSha256 + " (" + policy.Commands.Count + " entries)");
        return result.Clean ? 0 : 2;
    }

    private static int ImportsList(List<string> args)
    {
        var specs = ParseSpecs(args);
        if (specs is null) return Usage();
        var merged = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        foreach (var input in specs)
            foreach (var (kind, item, scopes) in ForbiddenApiScan.ListImports(File.ReadAllBytes(input.Path)))
            {
                var key = AllowEntry.KindLetter(kind) + "|" + item;
                if (!merged.TryGetValue(key, out var set)) merged[key] = set = new SortedSet<string>(StringComparer.Ordinal);
                foreach (var sc in scopes) set.Add(sc);
            }
        foreach (var (key, scopes) in merged)
            Console.Out.WriteLine(key + "|" + (scopes.Count == 1 && key[0] == 'M' ? scopes.Single() : "") + "|REVIEW" + (scopes.Count > 1 ? " (used in " + scopes.Count + " types: " + string.Join(" ; ", scopes) + ")" : ""));
        return 0;
    }

    private static int PrivilegedList(List<string> args)
    {
        var specs = ParseSpecs(args);
        if (specs is null) return Usage();
        foreach (var input in specs)
        {
            var image = File.ReadAllBytes(input.Path);
            var layer = RsLayer.Check(image, System.IO.Path.GetFileName(input.Path), null);
            foreach (var pair in layer.Observed.Surface.OrderBy(p => p.Key, StringComparer.Ordinal))
                foreach (var line in pair.Value) Console.Out.WriteLine("SURFACE " + line);
            foreach (var c in layer.Observed.Callers) Console.Out.WriteLine("CALLER " + c + "|REVIEW");
            var baseObserved = ForbiddenApiScan.ScanDetailed(image, System.IO.Path.GetFileName(input.Path), ScanConfig.For(input.Role)).Observed;
            foreach (var c in baseObserved.Callers) Console.Out.WriteLine("CALLER " + c + "|REVIEW");
            foreach (var c in baseObserved.Commands) Console.Out.WriteLine("COMMAND " + c + "|REVIEW");
        }
        return 0;
    }

    private static int ScratchVerify(List<string> args)
    {
        if (args.Count != 2) return Usage();
        var scratchRoot = args[0];
        var evidenceRoot = args[1];
        if (!Directory.Exists(scratchRoot) || !Directory.Exists(evidenceRoot))
        {
            Console.Error.WriteLine("scratch-verify: both folders must exist");
            return 1;
        }
        var problems = new List<string>();
        var declared = PriorDeclarations.Load(evidenceRoot, problems);
        var verification = ScratchVerifier.Verify(scratchRoot, declared);
        foreach (var p in problems) Console.Out.WriteLine("problem: " + p);
        foreach (var u in verification.Undeclared) Console.Out.WriteLine("UNDECLARED " + u);
        foreach (var m in verification.Missing) Console.Out.WriteLine("MISSING " + m);
        foreach (var h in verification.HashMismatches) Console.Out.WriteLine("HASH_MISMATCH " + h);
        foreach (var r in verification.ReparsePoints) Console.Out.WriteLine("REPARSE_POINT " + r);
        Console.Out.WriteLine(verification.Ok && problems.Count == 0 ? "scratch root: CLEAN (" + declared.Count + " declared file(s))" : "scratch root: NOT CLEAN");
        return verification.Ok && problems.Count == 0 ? 0 : 2;
    }

    private static int TolScaleVerify(List<string> args)
    {
        if (args.Count != 1) return Usage();
        var problems = TolScaleOfflineVerifier.Verify(File.ReadAllText(args[0]));
        foreach (var p in problems) Console.Out.WriteLine("problem: " + p);
        Console.Out.WriteLine(problems.Count == 0 ? "tolscale record: offline recomputation EQUAL" : "tolscale record: INVALID");
        return problems.Count == 0 ? 0 : 2;
    }

    private static int LedgerVerify(List<string> args)
    {
        if (args.Count != 2) return Usage();
        var differences = HashesLedger.Verify(File.ReadAllText(args[0]), args[1]);
        foreach (var d in differences) Console.Out.WriteLine("difference: " + d);
        Console.Out.WriteLine(differences.Count == 0 ? "ledger: no difference" : "ledger: DIFFERENT");
        return differences.Count == 0 ? 0 : 4;
    }

    private static int Hash(List<string> args)
    {
        if (args.Count == 0) return Usage();
        foreach (var path in args) Console.Out.WriteLine(Sha256Hex.OfFile(path) + "  " + path);
        return 0;
    }
}

/// <summary>The hashes ledger (<c>HASHES-RS.txt</c>): one <c>sha256  relative/path</c> line per file, LF, paths in ordinal order. Verified read-only.</summary>
public static class HashesLedger
{
    public static IReadOnlyList<string> Verify(string ledgerText, string root)
    {
        var differences = new List<string>();
        if (ledgerText.Contains('\r')) differences.Add("LEDGER_HAS_CR");
        var listed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var raw in ledgerText.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            var sep = line.IndexOf("  ", StringComparison.Ordinal);
            if (sep != 64) { differences.Add("MALFORMED_LINE:" + line); continue; }
            var hash = line[..64];
            var rel = line[66..];
            listed.Add(rel);
            var full = System.IO.Path.Combine(root, rel.Replace('/', System.IO.Path.DirectorySeparatorChar));
            if (!File.Exists(full)) { differences.Add("MISSING:" + rel); continue; }
            if (!string.Equals(Sha256Hex.OfFile(full), hash, StringComparison.Ordinal)) differences.Add("HASH_DIFFERS:" + rel);
        }
        return differences;
    }
}
