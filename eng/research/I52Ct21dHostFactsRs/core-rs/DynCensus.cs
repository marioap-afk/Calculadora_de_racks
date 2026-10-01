using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

/// <summary>One value of the allowed-value set a dynamic property reports (host runtime type and value, as the host returned them).</summary>
public sealed class AllowedValueReading
{
    public string HostTypeFullName { get; }
    public string ValueText { get; }
    public string? ValueBitsHex { get; }

    public AllowedValueReading(string hostTypeFullName, string valueText, string? valueBitsHex)
    {
        HostTypeFullName = hostTypeFullName;
        ValueText = valueText;
        ValueBitsHex = valueBitsHex;
    }
}

/// <summary>One entry of the dynamic-property collection of a reference (BA-05 V5 section 5 step 2): name, read-only flag, host value type, units type, value set.</summary>
public sealed class DynamicPropertyReading
{
    public string Name { get; }
    public bool ReadOnly { get; }
    public string HostValueType { get; }
    public string UnitsType { get; }
    public string ValueText { get; }
    public string? ValueBitsHex { get; }
    public IReadOnlyList<AllowedValueReading>? AllowedValues { get; }
    public string? AllowedValuesError { get; }

    public DynamicPropertyReading(string name, bool readOnly, string hostValueType, string unitsType, string valueText, string? valueBitsHex,
        IReadOnlyList<AllowedValueReading>? allowedValues, string? allowedValuesError)
    {
        Name = name;
        ReadOnly = readOnly;
        HostValueType = hostValueType;
        UnitsType = unitsType;
        ValueText = valueText;
        ValueBitsHex = valueBitsHex;
        AllowedValues = allowedValues;
        AllowedValuesError = allowedValuesError;
    }
}

/// <summary>The outcome of probing one dynamic block definition: its properties as a reference in the scratch side database exposes them, or an error (UNKNOWN).</summary>
public sealed class DynamicBlockProbe
{
    public string BlockName { get; }
    public IReadOnlyList<DynamicPropertyReading>? Properties { get; }
    public string? Error { get; }

    public DynamicBlockProbe(string blockName, IReadOnlyList<DynamicPropertyReading>? properties, string? error)
    {
        BlockName = blockName;
        Properties = properties;
        Error = error;
    }
}

public interface IDynCensusSession : IDisposable
{
    /// <summary>Names of the named, non-layout, non-anonymous block table records that are dynamic block definitions, in table order.</summary>
    IReadOnlyList<string> ListDynamicDefinitionNames();

    /// <summary>Total block table records of the private copy (context for the record).</summary>
    int CountBlockRecords();

    /// <summary>Clones the definition into the scratch side database, inserts a reference there and reads its dynamic-property collection (never in the private copy).</summary>
    DynamicBlockProbe Probe(string blockName);
}

public interface IDynCensusHost
{
    /// <summary>Opens the authorized private copy into a side database and creates the scratch side database (two side databases, both disposed with the session).</summary>
    IDynCensusSession OpenPrivateCopy(ReadableSource source);
}

public sealed class RsRunOutcome
{
    public string Status { get; }
    public IReadOnlyList<string> Reasons { get; }
    public IReadOnlyList<WrittenFile> Written { get; }

    public RsRunOutcome(string status, IReadOnlyList<string> reasons, IReadOnlyList<WrittenFile> written)
    {
        Status = status;
        Reasons = reasons;
        Written = written;
    }
}

/// <summary>Builds the dynamic-property census body (<c>ct21d.dyncensus.v1</c>) from the probes. Pure.</summary>
public static class DynCensusBuilder
{
    public const string Method = "CLONE_DEFINITION_TO_SCRATCH_SIDE_DATABASE_AND_READ_REFERENCE_PROPERTY_COLLECTION";

    public static JsonObject BlockToJson(DynamicBlockProbe probe)
    {
        var o = new JsonObject { ["blockName"] = probe.BlockName, ["isDynamic"] = true };
        if (probe.Properties is null)
        {
            o["status"] = "UNKNOWN";
            o["reason"] = probe.Error ?? "NOT_PROBED";
            o["dynamicProperties"] = new JsonArray();
            return o;
        }
        o["status"] = "OBSERVED";
        o["reason"] = "";
        var arr = new JsonArray();
        for (var i = 0; i < probe.Properties.Count; i++)
        {
            var p = probe.Properties[i];
            var sameName = probe.Properties.Count(x => string.Equals(x.Name, p.Name, StringComparison.Ordinal)) > 1;
            var sameNameIgnoringCase = probe.Properties.Count(x => string.Equals(x.Name, p.Name, StringComparison.OrdinalIgnoreCase)) > 1;
            var pj = new JsonObject
            {
                ["index"] = i, ["name"] = p.Name, ["readOnly"] = p.ReadOnly, ["hostValueType"] = p.HostValueType, ["unitsType"] = p.UnitsType,
                ["valueText"] = p.ValueText, ["valueBitsHex"] = p.ValueBitsHex,
                ["valueSetStatus"] = p.AllowedValues is null ? "UNREADABLE" : p.AllowedValues.Count == 0 ? "NONE" : "LISTED",
                ["sharesNameWithAnother"] = sameName, ["sharesNameIgnoringCase"] = sameNameIgnoringCase,
            };
            if (p.AllowedValues is not null && p.AllowedValues.Count > 0)
                pj["allowedValues"] = RecordJson.Arr(p.AllowedValues.Select(a => (JsonNode?)new JsonObject
                {
                    ["hostValueType"] = a.HostTypeFullName, ["valueText"] = a.ValueText, ["valueBitsHex"] = a.ValueBitsHex,
                }));
            if (p.AllowedValues is null) pj["valueSetError"] = p.AllowedValuesError ?? "UNREADABLE";
            arr.Add(pj);
        }
        o["dynamicProperties"] = arr;
        return o;
    }

    public sealed class Summary
    {
        public int Definitions { get; }
        public int Observed { get; }
        public int Unknown { get; }
        public int Properties { get; }
        public int Writable { get; }
        public int Constrained { get; }
        public int ValueSetUnreadable { get; }
        public int DuplicateNames { get; }

        public Summary(int definitions, int observed, int unknown, int properties, int writable, int constrained, int valueSetUnreadable, int duplicateNames)
        {
            Definitions = definitions;
            Observed = observed;
            Unknown = unknown;
            Properties = properties;
            Writable = writable;
            Constrained = constrained;
            ValueSetUnreadable = valueSetUnreadable;
            DuplicateNames = duplicateNames;
        }
    }

    public static Summary Summarize(IReadOnlyList<DynamicBlockProbe> probes)
    {
        var props = probes.Where(p => p.Properties is not null).SelectMany(p => p.Properties!).ToList();
        var duplicates = probes.Where(p => p.Properties is not null)
            .Count(p => p.Properties!.GroupBy(x => x.Name, StringComparer.Ordinal).Any(g => g.Count() > 1));
        return new Summary(
            probes.Count, probes.Count(p => p.Properties is not null), probes.Count(p => p.Properties is null), props.Count,
            props.Count(p => !p.ReadOnly), props.Count(p => p.AllowedValues is not null && p.AllowedValues.Count > 0),
            props.Count(p => p.AllowedValues is null), duplicates);
    }

    /// <summary>HF-C2 status: NOT_OBSERVED when the library has no dynamic definition; UNKNOWN when any definition could not be probed; else OBSERVED.</summary>
    public static (string Status, string Reason) FactStatus(Summary s)
    {
        if (s.Definitions == 0) return ("NOT_OBSERVED", "NO_DYNAMIC_BLOCK_DEFINITION_IN_THE_LIBRARY");
        if (s.Unknown > 0) return ("UNKNOWN", "DEFINITIONS_NOT_PROBED=" + s.Unknown);
        return ("OBSERVED", s.ValueSetUnreadable > 0 ? "VALUE_SET_UNREADABLE_FOR=" + s.ValueSetUnreadable + "_PROPERTIES(constrained-unknown)" : "");
    }
}

/// <summary>
/// U-RS-1 / I-3: the dynamic-property census (HF-C2). The private copy is read into a side database; each dynamic definition is CLONED into a
/// scratch side database, a reference is inserted THERE, and its property collection is read (BA-05 V5 section 5 step 2: "never in the library copy").
/// No scratch file is saved. The files written: <c>dyncensus-raw.json</c>, <c>hostfact-HF-C2.json</c>, <c>rs-run-dyncensus.json</c>.
/// </summary>
public static class DynCensusRunner
{
    public const string Command = "CT21DHG_CENSUS_DYN";
    public const string RawFile = "dyncensus-raw.json";
    public const string FactFile = "hostfact-HF-C2.json";
    public const string RunFile = "rs-run-dyncensus.json";

    public static IReadOnlyList<string> OutputNames { get; } = new[] { RawFile, FactFile, RunFile };

    public static RsRunOutcome Run(
        RsDesignation d,
        InstrumentInfo instrument,
        IRunEnvironment env,
        ISystemVariables vars,
        Func<ScratchRootGuard, SideDbLedger, IDynCensusHost> hostFactory,
        EvidenceWriter writer)
    {
        var refusal = new List<string>();
        var pre = RsPreflight.Check(d, instrument, OutputNames);
        refusal.AddRange(pre.Problems);
        if (!PathRegions.Same(writer.Root, d.EvidenceRoot)) refusal.Add("WRITER_ROOT_IS_NOT_THE_DESIGNATED_EVIDENCE_ROOT");
        if (refusal.Count > 0) throw new RsRefusedException(refusal);

        var guard = new ScratchRootGuard(d.ScratchRoot, d.PrivateCopyPath);
        var sideDbs = new SideDbLedger();
        var invalid = new List<string>();
        var before = SysSnapshot.Take(vars);
        var privateBefore = Sha256Hex.OfFile(d.PrivateCopyPath);
        var libraryBefore = Sha256Hex.OfFile(d.LibraryPath);

        var probes = new List<DynamicBlockProbe>();
        var blockRecords = 0;
        try
        {
            var host = hostFactory(guard, sideDbs);
            using var session = host.OpenPrivateCopy(guard.AuthorizePrivateCopy(d.PrivateCopySha256));
            blockRecords = session.CountBlockRecords();
            foreach (var name in session.ListDynamicDefinitionNames())
            {
                DynamicBlockProbe probe;
                try { probe = session.Probe(name); }
                catch (Exception ex) when (ex is not OutOfMemoryException) { probe = new DynamicBlockProbe(name, null, "PROBE_FAILED:" + ex.GetType().Name + ":" + ex.Message); }
                probes.Add(probe);
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            invalid.Add("INSTRUMENT_ERROR:" + ex.GetType().Name + ":" + ex.Message);
        }

        var after = SysSnapshot.Take(vars);
        invalid.AddRange(SysSnapshot.Differences(before, after));
        var dbmodBefore = SysSnapshot.Dbmod(before);
        var dbmodAfter = SysSnapshot.Dbmod(after);
        if (dbmodBefore is null || dbmodAfter is null) invalid.Add("DBMOD_UNREADABLE");
        var privateAfter = Sha256Hex.OfFile(d.PrivateCopyPath);
        var libraryAfter = Sha256Hex.OfFile(d.LibraryPath);
        if (privateBefore != privateAfter || libraryBefore != libraryAfter) invalid.Add("PRIVATE_FILES_CHANGED");
        if (!sideDbs.AllDisposed) invalid.Add("SIDE_DATABASE_NOT_DISPOSED");
        var declared = new List<ScratchFileEntry>(pre.PriorScratch);
        declared.AddRange(guard.Ledger);
        if (guard.Ledger.Count > 0) invalid.Add("UNEXPECTED_SCRATCH_FILE_SAVED");
        var verification = ScratchVerifier.Verify(d.ScratchRoot, declared);
        if (!verification.Ok)
            invalid.Add("SCRATCH_VERIFICATION_FAILED:undeclared=" + verification.Undeclared.Count + ",missing=" + verification.Missing.Count +
                        ",hashMismatches=" + verification.HashMismatches.Count + ",reparsePoints=" + verification.ReparsePoints.Count);

        try
        {
            var summary = DynCensusBuilder.Summarize(probes);
            var (factStatus, factReason) = DynCensusBuilder.FactStatus(summary);
            var status = invalid.Count > 0 ? "INVALID" : factStatus;
            var reason = invalid.Count > 0 ? string.Join(";", invalid) : factReason;

            var tuple = RecordJson.Tuple(d.ToCore(), instrument, privateBefore, privateAfter, privateCopyApplies: true);
            var body = new JsonObject
            {
                ["schema"] = "ct21d.dyncensus.v1",
                ["governing"] = false,
                ["gate"] = RecordJson.Gate,
                ["tuple"] = tuple.DeepClone(),
                ["libraryFileSha256"] = d.LibraryFileSha256,
                ["libraryPath"] = d.LibraryPath,
                ["dynamicPropertyMethod"] = DynCensusBuilder.Method,
                ["blockRecords"] = blockRecords,
                ["blocks"] = RecordJson.Arr(probes.Select(p => (JsonNode?)DynCensusBuilder.BlockToJson(p))),
                ["summary"] = new JsonObject
                {
                    ["dynamicDefinitions"] = summary.Definitions, ["observed"] = summary.Observed, ["unknown"] = summary.Unknown,
                    ["properties"] = summary.Properties, ["writableProperties"] = summary.Writable, ["constrainedProperties"] = summary.Constrained,
                    ["valueSetUnreadableProperties"] = summary.ValueSetUnreadable, ["definitionsWithDuplicateNames"] = summary.DuplicateNames,
                },
            };
            RecordJson.Seal(body, env);
            RsSchemas.ValidateOrThrow("ct21d.dyncensus.v1.json", body);
            var raw = writer.WriteNewText(RawFile, RecordJson.ToFileText(body));

            var obs = new JsonObject
            {
                ["dynamicDefinitions"] = summary.Definitions, ["observed"] = summary.Observed, ["unknown"] = summary.Unknown,
                ["properties"] = summary.Properties, ["writableProperties"] = summary.Writable, ["constrainedProperties"] = summary.Constrained,
                ["valueSetUnreadableProperties"] = summary.ValueSetUnreadable,
                ["method"] = DynCensusBuilder.Method,
                ["note"] = "the reference was created in a scratch side database; the private copy was only read (BA-05 V5 section 5 step 2)",
            };
            var fact = HostFactRecord.Build("HF-C2", tuple, obs, status, reason, new[] { raw }, env);
            writer.WriteNewText(FactFile, RecordJson.ToFileText(fact));
            var runRecord = RsRunRecord.Build(Command, "U-RS-1", d, instrument, env, sideDbs, guard, verification, pre.PriorScratch, before, after, invalid, status);
            writer.WriteNewText(RunFile, RecordJson.ToFileText(runRecord));
            return new RsRunOutcome(status, invalid.Count > 0 ? invalid : (reason.Length > 0 ? new[] { reason } : Array.Empty<string>()), writer.Ledger.ToList());
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not RsRefusedException)
        {
            var declaration = RsFailSafe.TryBuild(Command, "U-RS-1", d, instrument, env, sideDbs, guard, verification, pre.PriorScratch, before, after, invalid, ex);
            if (declaration is not null)
            {
                try { writer.WriteNewText(RunFile, declaration); }
                catch (Exception wex) when (wex is IOException or EvidenceWriterException) { } // the record exists already (create-new): nothing is overwritten
            }
            throw;
        }
    }
}
