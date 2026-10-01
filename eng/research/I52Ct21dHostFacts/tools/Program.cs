using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Tools.Scan;

namespace I52Ct21d.HostFacts.Tools;

/// <summary>
/// Offline tools (design 5.1 tools/). The package-manifest / pin generator (I-8) is NOT here: it belongs to package v2. Nothing here starts, loads or talks to AutoCAD; every file it writes goes through the single
/// <see cref="EvidenceWriter"/> (create-new, flat names). Exit codes: 0 ok, 1 usage, 2 scan violations, 3 label UNSET, 4 seal differences.
///
///   scan --allowed apis.txt --privileged-surface s.txt --privileged-callers c.txt --expected-commands e.txt role:path...
///                                      role = core | tools | r0. The forbidden-API semantic scan (5.3 item 1) plus the
///                                      deny-by-default imports check against the approved list (5.3 item 2) plus the pinned privileged
///                                      surface (layer 3). All four options are mandatory.
///   imports-allowlist-check apis.txt role:path...   only the imports layer, plus the approvals that nothing uses (stale entries).
///   privileged-list role:path...       prints the observed privileged surface, callers and command registrations (a review aid: nothing is approved).
///   imports-list role:path...          drafts allowed-apis.txt lines from an assembly (a review aid: nothing is approved by listing).
///   label folder [--record]            I-1 label helper: recomputes HF-M7 from the six raw files (and optionally records it).
///   seal folder                        I-9: writes HASHES.sha256 + digest and marks read-only; on a sealed folder verifies only.
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0) return Usage();
        switch (args[0])
        {
            case "scan": return Scan(args.Skip(1).ToList());
            case "label": return Label(args.Skip(1).ToList());
            case "seal": return Seal(args.Skip(1).ToList());
            case "imports-allowlist-check": return ImportsCheck(args.Skip(1).ToList());
            case "imports-list": return ImportsList(args.Skip(1).ToList());
            case "privileged-list": return PrivilegedList(args.Skip(1).ToList());
            default: return Usage();
        }
    }

    private static int Usage()
    {
        Console.Error.WriteLine("usage: scan --allowed apis.txt --privileged-surface s.txt --privileged-callers c.txt --expected-commands e.txt role:path... | imports-allowlist-check apis.txt role:path... | imports-list role:path... | privileged-list role:path... | label folder [--record] | seal folder");
        return 1;
    }

    private static ScanRole? ParseRole(string role) => role.ToLowerInvariant() switch
    {
        "core" => ScanRole.Core,
        "tools" => ScanRole.Tools,
        "r0" => ScanRole.R0,
        _ => (ScanRole?)null,
    };

    private static bool TryParseSpecs(IEnumerable<string> specs, out List<(ScanRole Role, string Path)> parsed)
    {
        parsed = new List<(ScanRole, string)>();
        foreach (var spec in specs)
        {
            var colon = spec.IndexOf(':');
            if (colon <= 0) return false;
            var role = ParseRole(spec[..colon]);
            if (role is null) return false;
            parsed.Add((role.Value, spec[(colon + 1)..]));
        }
        return parsed.Count > 0;
    }

    private static int Scan(List<string> args)
    {
        if (args.Count < 9 || args[0] != "--allowed" || args[2] != "--privileged-surface" || args[4] != "--privileged-callers" || args[6] != "--expected-commands")
            return Usage();
        ApiAllowlist allowlist;
        ScanPolicy policy;
        try
        {
            allowlist = ApiAllowlist.LoadFile(args[1]);
            policy = ScanPolicy.LoadFiles(args[3], args[5], args[7]);
        }
        catch (Exception ex) when (ex is FormatException or IOException or System.Text.DecoderFallbackException)
        {
            Console.Error.WriteLine("scan lists: " + ex.Message);
            return 1;
        }
        if (!TryParseSpecs(args.Skip(8), out var specs)) return Usage();
        var results = new List<(string, IReadOnlyList<Violation>)>();
        foreach (var (role, path) in specs)
            results.Add((Path.GetFileName(path), ForbiddenApiScan.ScanFile(path, ScanConfig.For(role, allowlist, policy))));
        // stale entries of the pinned lists are reported only when all three assemblies were scanned (otherwise the others' entries look unused)
        var seenRoles = new HashSet<ScanRole>();
        foreach (var (role, _) in specs) seenRoles.Add(role);
        if (seenRoles.Count == 3)
        {
            var stale = new List<Violation>();
            foreach (var e in policy.UnusedCallers())
                stale.Add(new Violation("privileged-callers.txt", "<list>", "", -1, "R-PRIVILEGED-CALLERS-UNUSED", "line " + e.Line + ": " + e.Key + " is listed but no scanned assembly makes that call"));
            foreach (var e in policy.UnusedCommands())
                stale.Add(new Violation("expected-commands.txt", "<list>", "", -1, "R-COMMAND-UNUSED", "line " + e.Line + ": " + e.Key + " is listed but no scanned assembly registers it"));
            results.Add(("privileged lists", stale));
        }
        Console.Out.Write(ScanReport.ToText(results));
        Console.Out.WriteLine("allowed-apis.txt sha256 " + allowlist.Sha256 + " (" + allowlist.Entries.Count + " entries)");
        Console.Out.WriteLine("privileged-surface.txt sha256 " + policy.SurfaceSha256 + " (" + SurfaceCount(policy) + " methods)");
        Console.Out.WriteLine("privileged-callers.txt sha256 " + policy.CallersSha256 + " (" + policy.Callers.Count + " entries)");
        Console.Out.WriteLine("expected-commands.txt sha256 " + policy.CommandsSha256 + " (" + policy.Commands.Count + " entries)");
        return results.Any(r => r.Item2.Count > 0) ? 2 : 0;
    }

    private static int SurfaceCount(ScanPolicy policy)
    {
        var n = 0;
        foreach (var pair in policy.Surface) n += pair.Value.Count;
        return n;
    }

    private static int PrivilegedList(List<string> args)
    {
        if (!TryParseSpecs(args, out var specs)) return Usage();
        foreach (var (role, path) in specs)
        {
            var o = ForbiddenApiScan.ScanDetailed(File.ReadAllBytes(path), Path.GetFileName(path), ScanConfig.For(role)).Observed;
            foreach (var (_, lines) in o.Surface.OrderBy(p => p.Key, StringComparer.Ordinal))
                foreach (var line in lines) Console.Out.WriteLine("SURFACE " + line);
            foreach (var c in o.Callers) Console.Out.WriteLine("CALLER " + c + "|REVIEW");
            foreach (var c in o.Commands) Console.Out.WriteLine("COMMAND " + c + "|REVIEW");
        }
        return 0;
    }

    private static int ImportsCheck(List<string> args)
    {
        if (args.Count < 2) return Usage();
        ApiAllowlist allowlist;
        try { allowlist = ApiAllowlist.LoadFile(args[0]); }
        catch (Exception ex) when (ex is FormatException or IOException or System.Text.DecoderFallbackException)
        {
            Console.Error.WriteLine("allowed-apis: " + ex.Message);
            return 1;
        }
        if (!TryParseSpecs(args.Skip(1), out var specs)) return Usage();
        var results = new List<(string, IReadOnlyList<Violation>)>();
        foreach (var (role, path) in specs)
        {
            var outcome = ForbiddenApiScan.ScanDetailed(File.ReadAllBytes(path), Path.GetFileName(path), ScanConfig.For(role, allowlist));
            results.Add((Path.GetFileName(path), outcome.Violations.Where(v => v.Rule.StartsWith("R-IMPORT-", StringComparison.Ordinal)).ToList()));
        }
        var stale = allowlist.Unused()
            .Select(e => new Violation("allowed-apis.txt", "<allowlist>", "", -1, "R-ALLOWLIST-UNUSED", "line " + e.Line + ": " + e.Key + " is approved but no scanned assembly uses it"))
            .ToList();
        results.Add(("allowed-apis.txt", stale));
        Console.Out.Write(ScanReport.ToText(results));
        Console.Out.WriteLine("allowed-apis.txt sha256 " + allowlist.Sha256 + " (" + allowlist.Entries.Count + " entries)");
        return results.Any(r => r.Item2.Count > 0) ? 2 : 0;
    }

    private static int ImportsList(List<string> args)
    {
        if (!TryParseSpecs(args, out var specs)) return Usage();
        var merged = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        foreach (var (_, path) in specs)
            foreach (var (kind, item, scopes) in ForbiddenApiScan.ListImports(File.ReadAllBytes(path)))
            {
                var key = AllowEntry.KindLetter(kind) + "|" + item;
                if (!merged.TryGetValue(key, out var set)) merged[key] = set = new SortedSet<string>(StringComparer.Ordinal);
                foreach (var sc in scopes) set.Add(sc);
            }
        foreach (var (key, scopes) in merged)
            Console.Out.WriteLine(key + "|" + (scopes.Count == 1 && key[0] == 'M' ? scopes.Single() : "") + "|REVIEW" + (scopes.Count > 1 ? " (used in " + scopes.Count + " types: " + string.Join(" ; ", scopes) + ")" : ""));
        return 0;
    }

    private static int Label(List<string> args)
    {
        if (args.Count == 0) return Usage();
        var folder = args[0];
        var outcome = LabelHelper.ComputeFromFolder(folder);
        foreach (var (key, r) in outcome.Attributes.OrderBy(p => p.Key, StringComparer.Ordinal))
            Console.Out.WriteLine($"{key}: {r.Status}" + (r.Reason.Length > 0 ? " (" + r.Reason + ")" : ""));
        Console.Out.WriteLine("label: " + outcome.Label.Label);
        if (outcome.Label.IsSet) Console.Out.WriteLine("serializationSha256: " + outcome.Label.SerializationSha256);
        if (args.Contains("--record"))
        {
            var record = new JsonObject
            {
                ["schema"] = "ct21d.machine-label.v1",
                ["fact"] = "HF-M7",
                ["gate"] = RecordJson.Gate,
                ["governing"] = false,
                ["label"] = outcome.Label.Label,
                ["serializationSha256"] = outcome.Label.IsSet ? outcome.Label.SerializationSha256 : null,
                ["attributes"] = RecordJson.Arr(outcome.Attributes.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => (JsonNode?)new JsonObject
                {
                    ["key"] = p.Key,
                    ["status"] = p.Value.Status == RawAttributeStatus.Observed ? "OBSERVED" : "UNKNOWN",
                    ["reason"] = p.Value.Reason,
                    ["characters"] = p.Value.RuneCount,
                })),
            };
            RecordJson.ValidateOrThrow("ct21d.machine-label.v1.json", record);
            var writer = new EvidenceWriter(Path.GetFullPath(folder));
            var written = writer.WriteNewText("machine-label.json", Jcs.Serialize(record) + "\n");
            Console.Out.WriteLine("recorded machine-label.json sha256 " + written.Sha256);
        }
        return outcome.Label.IsSet ? 0 : 3;
    }

    private static int Seal(List<string> args)
    {
        if (args.Count == 0) return Usage();
        var result = EvidenceSealer.SealOrVerify(Path.GetFullPath(args[0]));
        foreach (var w in result.Written) Console.Out.WriteLine($"wrote {w.Name} {w.Sha256}");
        foreach (var d in result.Differences) Console.Out.WriteLine("difference: " + d);
        if (result.Written.Count == 0 && result.Differences.Count == 0) Console.Out.WriteLine("sealed folder: no change");
        return result.Differences.Count == 0 ? 0 : 4;
    }
}
