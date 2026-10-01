using System.Text.Json.Nodes;

namespace I52Ct21d.HostFacts.Core;

/// <summary>What a command run produced: the status of each fact record, the files written (with SHA-256) and the INVALID reasons.</summary>
public sealed record RunOutcome(
    IReadOnlyDictionary<string, string> FactStatuses,
    IReadOnlyList<WrittenFile> Written,
    IReadOnlyList<string> InvalidReasons);

/// <summary>
/// The orchestration of the three R0 commands. Everything here is AutoCAD-free: the host is reached only through
/// <see cref="IDrawingSource"/> and <see cref="ISystemVariables"/>; the only write is through the <see cref="EvidenceWriter"/>.
/// The checks are the host-run checks of design 5.3 item 6 (hashes of the private file and DBMOD equal before and after; the
/// self-pin) and any failure makes every fact of the run INVALID (retained, never repaired; design 2.5 INVALID).
/// </summary>
public static class Runners
{
    public const string CensusRawFile = "census-raw.json";
    public const string InventoryRawFile = "inventory-raw.json";
    public const string CtxVarsRawFile = "ctxvars-raw.json";

    public static string FactFile(string factId) => "hostfact-" + factId + ".json";

    // --- I-2 CT21DHG_CENSUS_R: HF-C1, HF-C3, HF-G1 (M1) ------------------------------------------------------------------
    public static RunOutcome RunCensus(
        RunDesignation d, InstrumentInfo instrument, IRunEnvironment env, IDrawingSource source, ISystemVariables vars, EvidenceWriter writer)
    {
        d.EnsureSeparation();
        var invalid = new List<string>();
        var before = PrivateCopyBefore(d, instrument, vars, invalid, out var dbmodBefore);

        IReadOnlyList<BlockRecordInfo> blocks;
        StoredBufferScan stored;
        using (var reader = source.OpenPrivateCopy(d.PrivateCopyPath))
        {
            blocks = reader.ReadBlockRecords();
            stored = reader.ReadStoredBuffers();
        }

        var after = PrivateCopyAfter(d, vars, before, dbmodBefore, invalid);
        var (body, summary) = CensusBuilder.Build(blocks, stored, d.LibraryFileSha256, d.LibraryPath, instrument);
        var tuple = RecordJson.Tuple(d, instrument, before, after, privateCopyApplies: true);
        body["tuple"] = tuple.DeepClone();
        RecordJson.Seal(body, env);
        RecordJson.ValidateOrThrow("ct21d.census.v1.json", body);
        var raw = writer.WriteNewText(CensusRawFile, RecordJson.ToFileText(body));

        var statuses = new Dictionary<string, string>(StringComparer.Ordinal);

        // HF-C1 (ROW3): classes, nested blocks, symbol records, proxy/custom presence, anonymous flags.
        string c1;
        string c1Reason;
        if (invalid.Count > 0) { c1 = "INVALID"; c1Reason = string.Join(";", invalid); }
        else if (summary.Blocks == 0) { c1 = "UNKNOWN"; c1Reason = "NO_BLOCK_RECORDS"; }
        else if (summary.BlocksUnknown > 0) { c1 = "INVALID"; c1Reason = "INCOMPLETE_CENSUS: every block is required (BA-05 V5 section 3)"; }
        else { c1 = "OBSERVED"; c1Reason = ""; }
        var c1Obs = new JsonObject
        {
            ["blocks"] = summary.Blocks, ["namedBlocks"] = summary.NamedBlocks, ["layoutBlocks"] = summary.LayoutBlocks,
            ["anonymousBlocks"] = summary.AnonymousBlocks, ["blocksUnknown"] = summary.BlocksUnknown, ["entities"] = summary.Entities,
            ["distinctClasses"] = summary.DistinctClasses, ["classesWithoutTable"] = summary.ClassesWithoutTable,
            ["blocksWithProxyOrCustom"] = summary.BlocksWithProxyOrCustom,
            ["blocksReferencingAnonymousStatic"] = summary.BlocksReferencingAnonymousStatic,
            ["dynamicPropertyMethod"] = "NOT_READ_BY_R0",
        };
        statuses["HF-C1"] = WriteFact("HF-C1", tuple, c1Obs, c1, c1Reason, raw, env, writer);

        // HF-C3 (ROW3): the stored DSTYLE section of dimension overrides, pairs in host order.
        string c3;
        string c3Reason;
        if (invalid.Count > 0) { c3 = "INVALID"; c3Reason = string.Join(";", invalid); }
        else if (summary.DimensionsMalformed > 0) { c3 = "UNKNOWN"; c3Reason = "MALFORMED_DSTYLE_SECTION"; }
        else if (summary.DimensionsWithDstyle == 0) { c3 = "NOT_OBSERVED"; c3Reason = "NO_DIMENSION_WITH_A_DSTYLE_SECTION"; }
        else { c3 = "OBSERVED"; c3Reason = ""; }
        var c3Obs = new JsonObject
        {
            ["dimensions"] = summary.Dimensions, ["dimensionsWithDstyleSection"] = summary.DimensionsWithDstyle,
            ["dimensionsMalformed"] = summary.DimensionsMalformed, ["pairs"] = summary.DstylePairs,
            ["storage"] = "ACAD extended data, DSTYLE section (read; pairs are (code, host value type) in host order, no variable name)",
        };
        statuses["HF-C3"] = WriteFact("HF-C3", tuple, c3Obs, c3, c3Reason, raw, env, writer);

        // HF-G1 (M1, G6): host value type per RB code range, read from STORED data only.
        string g1;
        string g1Reason;
        var ranges = summary.Rb.Ranges;
        if (invalid.Count > 0) { g1 = "INVALID"; g1Reason = string.Join(";", invalid); }
        else if (ranges.Any(r => r.Status == "OBSERVED_DIFFERS")) { g1 = "OBSERVED_DIFFERS"; g1Reason = "AT_LEAST_ONE_RANGE_DIFFERS"; }
        else if (ranges.Any(r => r.Status == "OBSERVED")) { g1 = "OBSERVED"; g1Reason = ""; }
        else { g1 = "NOT_OBSERVED"; g1Reason = "NO_RANGE_HAS_AN_OCCURRENCE"; }
        if (stored.ReadFailures > 0 && invalid.Count == 0) g1Reason = (g1Reason.Length > 0 ? g1Reason + ";" : "") + "STORED_BUFFER_READ_FAILURES=" + stored.ReadFailures;
        var g1Obs = new JsonObject
        {
            ["rangesExpected"] = ranges.Count,
            ["rangesObserved"] = ranges.Count(r => r.Status == "OBSERVED"),
            ["rangesDiffer"] = ranges.Count(r => r.Status == "OBSERVED_DIFFERS"),
            ["rangesNotObserved"] = ranges.Count(r => r.Status == "NOT_OBSERVED"),
            ["codesOutsideTable"] = summary.Rb.OutsideTable.Count,
            ["storedBuffersRead"] = stored.BuffersRead,
            ["storedBufferReadFailures"] = stored.ReadFailures,
            ["note"] = "a NOT_OBSERVED range is not confirmed; stored data cannot be assumed to cover all ranges (design HF-G1)",
        };
        statuses["HF-G1"] = WriteFact("HF-G1", tuple, g1Obs, g1, g1Reason, raw, env, writer);

        return new RunOutcome(statuses, writer.Ledger.ToList(), invalid);
    }

    // --- I-2 CT21DHG_INVENTORY: HF-T2 -------------------------------------------------------------------------------------
    public static RunOutcome RunInventory(
        RunDesignation d, InstrumentInfo instrument, IRunEnvironment env, IDrawingSource source, ISystemVariables vars, EvidenceWriter writer)
    {
        d.EnsureSeparation();
        var invalid = new List<string>();
        var before = PrivateCopyBefore(d, instrument, vars, invalid, out var dbmodBefore);
        IReadOnlyList<BlockRecordInfo> blocks;
        using (var reader = source.OpenPrivateCopy(d.PrivateCopyPath))
            blocks = reader.ReadBlockRecords();
        var after = PrivateCopyAfter(d, vars, before, dbmodBefore, invalid);

        var (body, summary) = InventoryBuilder.Build(blocks, d.LibraryFileSha256, d.LibraryPath, instrument);
        var tuple = RecordJson.Tuple(d, instrument, before, after, privateCopyApplies: true);
        body["tuple"] = tuple.DeepClone();
        RecordJson.Seal(body, env);
        RecordJson.ValidateOrThrow("ct21d.inventory.v1.json", body);
        var raw = writer.WriteNewText(InventoryRawFile, RecordJson.ToFileText(body));

        var status = invalid.Count > 0 ? "INVALID" : summary.Status;
        var reason = invalid.Count > 0 ? string.Join(";", invalid) : summary.Status == "NOT_OBSERVED" ? "NO_BLOCK_REFERENCE_IN_THE_LIBRARY" : "";
        var obs = new JsonObject
        {
            ["count"] = summary.Count, ["observed"] = summary.Observed, ["unknown"] = summary.Unknown,
            ["distinctTriples"] = summary.DistinctTriples,
            ["ImaxHex"] = summary.Imax is null ? null : DoubleBits.Hex(summary.Imax.Value),
            ["note"] = "informative only (design 2.3 symbol I_max); no block extents are read",
        };
        var statuses = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["HF-T2"] = WriteFact("HF-T2", tuple, obs, status, reason, raw, env, writer),
        };
        return new RunOutcome(statuses, writer.Ledger.ToList(), invalid);
    }

    // --- I-4 CT21DHG_CTXVARS: HF-G3 ---------------------------------------------------------------------------------------
    public static RunOutcome RunCtxVars(
        RunDesignation d, InstrumentInfo instrument, IRunEnvironment env, ISystemVariables vars, EvidenceWriter writer,
        IEnumerable<string>? extraNames = null)
    {
        d.EnsureSeparation();
        var invalid = new List<string>();
        if (instrument.SelfPinStatus != InstrumentInfo.PinMatch) invalid.Add("SELF_PIN_" + instrument.SelfPinStatus);

        string Identity() => Text(vars.Read("DWGPREFIX")) + "|" + Text(vars.Read("DWGNAME"));
        var dbmodBefore = Text(vars.Read("DBMOD"));
        var identityBefore = Identity();

        var names = ContextVariableTable.Variables.Select(v => v.Name).Concat(extraNames ?? Array.Empty<string>()).Distinct(StringComparer.Ordinal).ToList();
        var readings = names.Select(vars.Read).ToList();

        var dbmodAfter = Text(vars.Read("DBMOD"));
        var identityAfter = Identity();
        if (dbmodBefore != dbmodAfter) invalid.Add("DBMOD_CHANGED");
        if (identityBefore != identityAfter) invalid.Add("DOCUMENT_IDENTITY_CHANGED");

        var (list, factStatus, factReason) = CtxVarsBuilder.Build(readings);
        var tuple = RecordJson.Tuple(d, instrument, null, null, privateCopyApplies: false);
        var body = new JsonObject
        {
            ["schema"] = "ct21d.ctxvars.v1",
            ["governing"] = false,
            ["gate"] = RecordJson.Gate,
            ["tuple"] = tuple.DeepClone(),
            ["ctxVars"] = list,
            ["checks"] = new JsonObject
            {
                ["dbmodBefore"] = dbmodBefore, ["dbmodAfter"] = dbmodAfter,
                ["documentIdentityBefore"] = identityBefore, ["documentIdentityAfter"] = identityAfter,
                ["selfPin"] = instrument.SelfPinStatus,
            },
        };
        RecordJson.Seal(body, env);
        RecordJson.ValidateOrThrow("ct21d.ctxvars.v1.json", body);
        var raw = writer.WriteNewText(CtxVarsRawFile, RecordJson.ToFileText(body));

        var status = invalid.Count > 0 ? "INVALID" : factStatus;
        var reason = invalid.Count > 0 ? string.Join(";", invalid) : factReason;
        var obs = new JsonObject
        {
            ["variables"] = names.Count,
            ["matching"] = readings.Count(r => ContextVariableTable.Find(r.Name) is { } e && r.HostTypeFullName == e.ExpectedHostType),
            ["note"] = "the TYPE is the fact; the value is document state (design HF-G3)",
        };
        var statuses = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["HF-G3"] = WriteFact("HF-G3", tuple, obs, status, reason, raw, env, writer),
        };
        return new RunOutcome(statuses, writer.Ledger.ToList(), invalid);
    }

    // --- shared checks -----------------------------------------------------------------------------------------------------
    private static string PrivateCopyBefore(RunDesignation d, InstrumentInfo instrument, ISystemVariables vars, List<string> invalid, out string dbmodBefore)
    {
        if (instrument.SelfPinStatus != InstrumentInfo.PinMatch) invalid.Add("SELF_PIN_" + instrument.SelfPinStatus);
        var before = Sha256Hex.OfFile(d.PrivateCopyPath);
        if (!string.Equals(before, d.PrivateCopySha256, StringComparison.Ordinal)) invalid.Add("PRIVATE_COPY_HASH_DIFFERS_FROM_DESIGNATION");
        if (!string.Equals(d.PrivateCopySha256, d.LibraryFileSha256, StringComparison.Ordinal)) invalid.Add("PRIVATE_COPY_HASH_DIFFERS_FROM_LIBRARY_HASH");
        dbmodBefore = Text(vars.Read("DBMOD"));
        return before;
    }

    private static string PrivateCopyAfter(RunDesignation d, ISystemVariables vars, string before, string dbmodBefore, List<string> invalid)
    {
        var after = Sha256Hex.OfFile(d.PrivateCopyPath);
        if (!string.Equals(before, after, StringComparison.Ordinal)) invalid.Add("PRIVATE_COPY_CHANGED");
        if (!string.Equals(dbmodBefore, Text(vars.Read("DBMOD")), StringComparison.Ordinal)) invalid.Add("DBMOD_CHANGED");
        return after;
    }

    private static string Text(SysVarReading r) => r.Error is not null ? "ERROR:" + r.Error : r.ValueText ?? "";

    private static string WriteFact(string factId, JsonObject tuple, JsonObject observation, string status, string reason, WrittenFile raw, IRunEnvironment env, EvidenceWriter writer)
    {
        var record = HostFactRecord.Build(factId, tuple, observation, status, reason, new[] { raw }, env);
        writer.WriteNewText(FactFile(factId), RecordJson.ToFileText(record));
        return status;
    }
}
