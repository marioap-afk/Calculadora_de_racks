using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

/// <summary>
/// Snapshots of host state that must not change during a run: DBMOD (the active document is untouched) and the settings whose change makes a
/// run INVALID (design 2.5: SECURELOAD, TRUSTEDPATHS, profile). Read only (<c>GetSystemVariable</c>).
/// </summary>
public static class SysSnapshot
{
    public static readonly IReadOnlyList<string> Names = new[] { "DBMOD", "SECURELOAD", "TRUSTEDPATHS", "CPROFILE", "DWGPREFIX", "DWGNAME" };

    public static SortedDictionary<string, string> Take(ISystemVariables vars)
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var name in Names)
        {
            var r = vars.Read(name);
            result[name] = r.Error is not null ? "ERROR:" + r.Error : (r.ValueText ?? "");
        }
        return result;
    }

    public static IReadOnlyList<string> Differences(IReadOnlyDictionary<string, string> before, IReadOnlyDictionary<string, string> after)
    {
        var result = new List<string>();
        foreach (var name in Names)
            if (!string.Equals(before[name], after[name], StringComparison.Ordinal)) result.Add(name + "_CHANGED");
        return result;
    }

    /// <summary>The DBMOD value as an integer, or null when the read failed or is not an integer.</summary>
    public static int? Dbmod(IReadOnlyDictionary<string, string> snapshot) =>
        snapshot.TryGetValue("DBMOD", out var t) && int.TryParse(t, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : null;

    public static JsonObject ToJson(IReadOnlyDictionary<string, string> snapshot)
    {
        var o = new JsonObject();
        foreach (var name in Names) o[name] = snapshot[name];
        return o;
    }
}

/// <summary>The scratch files that earlier commands of the same evidence root declared (their <c>rs-run-*.json</c> records).</summary>
public static class PriorDeclarations
{
    private static readonly Regex RunFile = new(@"^rs-run-[a-z0-9-]+\.json\z", RegexOptions.CultureInvariant);

    /// <summary>Reads and validates every <c>rs-run-*.json</c> of the evidence root. A record that does not validate is an error (INVALID evidence).</summary>
    public static IReadOnlyList<ScratchFileEntry> Load(string evidenceRoot, List<string> problems)
    {
        var result = new List<ScratchFileEntry>();
        foreach (var path in Directory.EnumerateFiles(evidenceRoot).OrderBy(p => p, StringComparer.Ordinal))
        {
            var name = Path.GetFileName(path);
            if (!RunFile.IsMatch(name)) continue;
            try
            {
                var node = Jcs.ParseStrict(File.ReadAllText(path));
                RsSchemas.ValidateOrThrow("ct21d.rs-run.v1.json", node);
                foreach (var f in (JsonArray)((JsonObject)((JsonObject)node)["scratch"]!)["files"]!)
                    result.Add(new ScratchFileEntry(f!["name"]!.GetValue<string>(), f["length"]!.GetValue<long>(), f["sha256"]!.GetValue<string>(), f["role"]!.GetValue<string>()));
            }
            catch (Exception ex) when (ex is FormatException or InvalidOperationException or IOException or System.Text.Json.JsonException)
            {
                problems.Add("PRIOR_RUN_RECORD_UNREADABLE:" + name);
            }
        }
        return result;
    }
}

/// <summary>What the preflight found: the problems (empty means the run may start) and the scratch files earlier commands declared.</summary>
public sealed class PreflightResult
{
    public IReadOnlyList<string> Problems { get; }
    public IReadOnlyList<ScratchFileEntry> PriorScratch { get; }

    public PreflightResult(IReadOnlyList<string> problems, IReadOnlyList<ScratchFileEntry> priorScratch)
    {
        Problems = problems;
        PriorScratch = priorScratch;
    }
}

/// <summary>The checks made before any side effect (no side database, no file written). Any problem refuses the run.</summary>
public static class RsPreflight
{
    public static PreflightResult Check(RsDesignation d, InstrumentInfo instrument, IReadOnlyCollection<string> outputNames)
    {
        var problems = new List<string>(d.SeparationProblems());
        var prior = new List<ScratchFileEntry>();
        if (instrument.SelfPinStatus != InstrumentInfo.PinMatch) problems.Add("SELF_PIN_" + instrument.SelfPinStatus);
        if (problems.Count > 0) return new PreflightResult(problems, prior); // nothing below may touch a path that failed the path rules

        if (!Directory.Exists(d.EvidenceRoot)) problems.Add("EVIDENCE_ROOT_MISSING");
        if (!Directory.Exists(d.ScratchRoot)) problems.Add("SCRATCH_ROOT_MISSING");
        if (!File.Exists(d.PrivateCopyPath)) problems.Add("PRIVATE_COPY_MISSING");
        if (!File.Exists(d.LibraryPath)) problems.Add("LIBRARY_FILE_MISSING");
        if (problems.Count > 0) return new PreflightResult(problems, prior);

        var reparseEvidence = PathRegions.FirstReparsePoint(d.EvidenceRoot);
        if (reparseEvidence.Length > 0) problems.Add("EVIDENCE_ROOT_REPARSE_POINT");
        if ((File.GetAttributes(d.PrivateCopyPath) & FileAttributes.ReparsePoint) != 0) problems.Add("PRIVATE_COPY_IS_A_REPARSE_POINT");
        var reparseScratch = PathRegions.FirstReparsePoint(d.ScratchRoot);
        if (reparseScratch.Length > 0) problems.Add("SCRATCH_ROOT_REPARSE_POINT");
        if (!string.Equals(Sha256Hex.OfFile(d.PrivateCopyPath), d.PrivateCopySha256, StringComparison.Ordinal)) problems.Add("PRIVATE_COPY_HASH_DIFFERS_FROM_DESIGNATION");
        if (!string.Equals(Sha256Hex.OfFile(d.LibraryPath), d.LibraryFileSha256, StringComparison.Ordinal)) problems.Add("LIBRARY_HASH_DIFFERS_FROM_DESIGNATION");
        if (!string.Equals(d.PrivateCopySha256, d.LibraryFileSha256, StringComparison.Ordinal)) problems.Add("PRIVATE_COPY_HASH_DIFFERS_FROM_LIBRARY_HASH");
        if (d.HostChecks.OtherAcadProcess) problems.Add("OTHER_ACAD_PROCESS_DECLARED");
        if (!d.HostChecks.LoadRouteScripted) problems.Add("MANUAL_INPUT_DECLARED");

        foreach (var existing in Directory.EnumerateFileSystemEntries(d.EvidenceRoot))
            foreach (var output in outputNames)
                if (string.Equals(Path.GetFileName(existing), output, StringComparison.OrdinalIgnoreCase))
                    problems.Add("OUTPUT_ALREADY_EXISTS:" + output + " (no automatic retry: a new attempt needs the Coordinator's authorization and a new evidence root)");

        var priorProblems = new List<string>();
        prior.AddRange(PriorDeclarations.Load(d.EvidenceRoot, priorProblems));
        problems.AddRange(priorProblems);
        var before = ScratchVerifier.Verify(d.ScratchRoot, prior);
        foreach (var u in before.Undeclared) problems.Add("SCRATCH_ROOT_HAS_UNDECLARED_ENTRY_AT_START:" + u);
        foreach (var m in before.Missing) problems.Add("SCRATCH_FILE_DECLARED_BUT_MISSING_AT_START:" + m);
        foreach (var h in before.HashMismatches) problems.Add("SCRATCH_FILE_HASH_CHANGED_SINCE_DECLARED:" + h);
        foreach (var r in before.ReparsePoints) problems.Add("SCRATCH_REPARSE_POINT:" + r);
        return new PreflightResult(problems, prior);
    }
}

/// <summary>Builds the <c>ct21d.rs-run.v1</c> record every RS command writes: side databases, scratch ledger, host snapshots, the CAD manager's declarations.</summary>
public static class RsRunRecord
{
    public static JsonObject Build(
        string command,
        string unit,
        RsDesignation d,
        InstrumentInfo instrument,
        IRunEnvironment env,
        SideDbLedger sideDbs,
        ScratchRootGuard? guard,
        ScratchVerification verification,
        IReadOnlyList<ScratchFileEntry> prior,
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        IReadOnlyList<string> invalidReasons,
        string result,
        IReadOnlyDictionary<string, string>? contentHashes = null)
    {
        var tuple = RecordJson.Tuple(d.ToCore(), instrument, null, null, privateCopyApplies: true);
        var record = new JsonObject
        {
            ["schema"] = "ct21d.rs-run.v1",
            ["command"] = command,
            ["unit"] = unit,
            ["governing"] = false,
            ["gate"] = RecordJson.Gate,
            ["classification"] = "CONTROL_PLANE_AUXILIARY_WRITES",
            ["tuple"] = tuple,
            ["sideDatabases"] = RecordJson.Arr(sideDbs.Entries.Select(e => (JsonNode?)e.ToJson())),
            ["scratch"] = new JsonObject
            {
                ["root"] = d.ScratchRoot,
                ["preexisting"] = RecordJson.Arr(prior.Select(e => (JsonNode?)e.ToJson())),
                ["files"] = RecordJson.Arr((guard?.Ledger ?? Array.Empty<ScratchFileEntry>()).Select(e => (JsonNode?)e.ToJson())),
                ["verification"] = verification.ToJson(),
            },
            ["host"] = new JsonObject
            {
                ["before"] = SysSnapshot.ToJson(before),
                ["after"] = SysSnapshot.ToJson(after),
                ["differences"] = RecordJson.Strings(SysSnapshot.Differences(before, after)),
            },
            ["declarations"] = new JsonObject
            {
                ["source"] = "CAD_MANAGER_DECLARATION",
                ["otherAcadProcess"] = d.HostChecks.OtherAcadProcess,
                ["loadRouteScripted"] = d.HostChecks.LoadRouteScripted,
            },
            ["noAutomaticRetry"] = true,
            ["invalidReasons"] = RecordJson.Strings(invalidReasons),
            ["result"] = result,
        };
        if (contentHashes is not null)
        {
            // the content hash (RFC 8785, without the volatile part) of a result record whose closed schema has no field for it (tolscale-raw.json)
            var hashes = new JsonObject();
            foreach (var pair in contentHashes.OrderBy(p => p.Key, StringComparer.Ordinal)) hashes[pair.Key] = pair.Value;
            record["contentHashes"] = hashes;
        }
        RecordJson.Seal(record, env);
        RsSchemas.ValidateOrThrow("ct21d.rs-run.v1.json", record);
        return record;
    }
}

/// <summary>
/// The last-resort declaration (review finding: no undeclared persistent state). When the post-processing of a run throws AFTER the side effects
/// (a schema check, a result-file write), the scratch ledger would otherwise remain undeclared. This builds the text of the <c>rs-run-*.json</c>
/// record (result INVALID, the reason) WITHOUT writing anything: the runner writes it create-new through the evidence writer, which refuses to
/// overwrite an existing record. Best effort: when building fails too, the original exception still propagates and the next preflight refuses on the
/// undeclared scratch file (fail closed). It never retries a run.
/// </summary>
public static class RsFailSafe
{
    public static string? TryBuild(
        string command, string unit, RsDesignation d, InstrumentInfo instrument, IRunEnvironment env, SideDbLedger sideDbs, ScratchRootGuard guard,
        ScratchVerification verification, IReadOnlyList<ScratchFileEntry> prior, IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after, IReadOnlyList<string> invalid, Exception cause)
    {
        try
        {
            var reasons = new List<string>(invalid) { "POST_PROCESSING_FAILED:" + cause.GetType().Name };
            var record = RsRunRecord.Build(command, unit, d, instrument, env, sideDbs, guard, verification, prior, before, after, reasons, "INVALID");
            return RecordJson.ToFileText(record);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return null;
        }
    }
}
