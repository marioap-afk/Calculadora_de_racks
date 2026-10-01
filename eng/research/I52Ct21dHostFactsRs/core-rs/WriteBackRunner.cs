using System.Globalization;
using System.Text.Json.Nodes;
using I52Ct21d.HostFacts.Core;

namespace I52Ct21d.HostFacts.Rs.Core;

/// <summary>What was observed for one scenario at both observation points.</summary>
public sealed class ScenarioObservation
{
    public WriteBackScenario Scenario { get; }
    public ScenarioConstruction? Construction { get; }
    public DimensionReading? Raw1 { get; }
    public DimensionReading? Raw2 { get; }
    public ParsedDstyle? Op1 { get; }
    public ParsedDstyle? Op2 { get; }
    public bool Observed { get; }
    public string Reason { get; }

    public ScenarioObservation(WriteBackScenario scenario, ScenarioConstruction? construction, DimensionReading? raw1, DimensionReading? raw2,
        ParsedDstyle? op1, ParsedDstyle? op2, bool observed, string reason)
    {
        Scenario = scenario;
        Construction = construction;
        Raw1 = raw1;
        Raw2 = raw2;
        Op1 = op1;
        Op2 = op2;
        Observed = observed;
        Reason = reason;
    }

    /// <summary>The stored overrides are equal at OP1 and OP2 (same codes, same value types, same value bits or text, same order).</summary>
    public bool ConsistentOp1Op2 =>
        Op1 is not null && Op2 is not null && Op1.Overrides.Count == Op2.Overrides.Count &&
        Op1.Overrides.Zip(Op2.Overrides).All(p => p.First.Code == p.Second.Code && p.First.ValueTypeCode == p.Second.ValueTypeCode &&
            p.First.ValueHostType == p.Second.ValueHostType && p.First.ValueText == p.Second.ValueText && p.First.ValueBitsHex == p.Second.ValueBitsHex);
}

/// <summary>The per-variable conclusions of HF-C4 and the designation of HF-C5. Each flag is null when it cannot be decided from the observation (never guessed).</summary>
public sealed class VariableFinding
{
    public string Variable { get; }
    public int Ba05Code { get; }
    public int? ObservedCode { get; }
    public bool? StoredWhenDifferent { get; }
    public bool? StoredWhenEqualToStyle { get; }
    public bool? ValueStoredAsWritten { get; }

    public VariableFinding(string variable, int ba05Code, int? observedCode, bool? storedWhenDifferent, bool? storedWhenEqualToStyle, bool? valueStoredAsWritten)
    {
        Variable = variable;
        Ba05Code = ba05Code;
        ObservedCode = observedCode;
        StoredWhenDifferent = storedWhenDifferent;
        StoredWhenEqualToStyle = storedWhenEqualToStyle;
        ValueStoredAsWritten = valueStoredAsWritten;
    }

    public bool? MatchesBa05Code => ObservedCode is null ? null : ObservedCode == Ba05Code;
}

/// <summary>The product-order scenario as read at OP1 (see <see cref="WriteBackAnalyzer.ProductOrder"/>).</summary>
public sealed class ProductOrderView
{
    public IReadOnlyList<int> StoredCodes { get; }
    public IReadOnlyList<string> StoredVars { get; }
    public bool? SameOrder { get; }
    public int? Pairs { get; }

    public ProductOrderView(IReadOnlyList<int> storedCodes, IReadOnlyList<string> storedVars, bool? sameOrder, int? pairs)
    {
        StoredCodes = storedCodes;
        StoredVars = storedVars;
        SameOrder = sameOrder;
        Pairs = pairs;
    }
}

public static class WriteBackAnalyzer
{
    public static IReadOnlyList<ScenarioObservation> Observe(
        IReadOnlyList<WriteBackScenario> scenarios, IReadOnlyList<ScenarioConstruction> constructions,
        IReadOnlyList<DimensionReading> read1, IReadOnlyList<DimensionReading> read2)
    {
        var result = new List<ScenarioObservation>();
        foreach (var s in scenarios)
        {
            var c = constructions.FirstOrDefault(x => x.ScenarioId == s.Id);
            if (c is null) { result.Add(new ScenarioObservation(s, null, null, null, null, null, false, "NO_CONSTRUCTION_RESULT")); continue; }
            if (c.Rejection is not null || c.Handle is null)
            {
                result.Add(new ScenarioObservation(s, c, null, null, null, null, false, "HOST_REJECTED_CONSTRUCTION:" + (c.Rejection ?? "NO_HANDLE")));
                continue;
            }
            var r1 = read1.FirstOrDefault(x => x.Handle == c.Handle);
            var r2 = read2.FirstOrDefault(x => x.Handle == c.Handle);
            var reasons = new List<string>();
            ParsedDstyle? p1 = Parse(r1, "OP1", reasons);
            ParsedDstyle? p2 = Parse(r2, "OP2", reasons);
            result.Add(new ScenarioObservation(s, c, r1, r2, p1, p2, reasons.Count == 0, string.Join(";", reasons)));
        }
        return result;
    }

    private static ParsedDstyle? Parse(DimensionReading? r, string point, List<string> reasons)
    {
        if (r is null) { reasons.Add(point + "_NOT_READ"); return null; }
        if (r.ReadError is not null) { reasons.Add(point + "_READ_ERROR:" + r.ReadError); return null; }
        if (!r.HasDstyleSection) return new ParsedDstyle(true, Array.Empty<StoredOverride>(), ""); // no DSTYLE section: an empty stored set
        if (!r.WellFormed) { reasons.Add(point + "_DSTYLE_SECTION_MALFORMED"); return null; }
        var parsed = DstyleOverrideParser.Parse(r.Entries);
        if (!parsed.WellFormed) { reasons.Add(point + "_DSTYLE_PAIRS_MALFORMED:" + parsed.Problem); return null; }
        return parsed;
    }

    private static bool? AsWritten(StoredOverride o, double assigned)
    {
        if (o.ValueBitsHex is not null) return string.Equals(o.ValueBitsHex, DoubleBits.Hex(assigned), StringComparison.Ordinal);
        return double.TryParse(o.ValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v == assigned : null;
    }

    public static IReadOnlyList<VariableFinding> Findings(IReadOnlyList<ScenarioObservation> obs)
    {
        var result = new List<VariableFinding>();
        foreach (var spec in WriteBackSpecs.Product)
        {
            var diff = obs.FirstOrDefault(o => o.Scenario.Kind == ScenarioKind.Different && o.Scenario.Variable == spec.Variable);
            var eq = obs.FirstOrDefault(o => o.Scenario.Kind == ScenarioKind.EqualToStyle && o.Scenario.Variable == spec.Variable);
            int? code = null;
            bool? storedDiff = null;
            bool? asWritten = null;
            if (diff is { Observed: true, Op1: not null, Op2: not null })
            {
                var n = diff.Op1.Overrides.Count;
                if (n == 0) storedDiff = false;
                else if (n == 1)
                {
                    storedDiff = true;
                    code = diff.Op1.Overrides[0].Code;
                    asWritten = AsWritten(diff.Op1.Overrides[0], diff.Scenario.Assignments[0].Value);
                }
            }
            bool? storedEq = null;
            if (eq is { Observed: true, Op1: not null })
            {
                var n = eq.Op1.Overrides.Count;
                if (n == 0) storedEq = false;
                else if (n == 1) storedEq = true;
            }
            result.Add(new VariableFinding(spec.Variable, spec.Ba05Code, code, storedDiff, storedEq, asWritten));
        }
        return result;
    }

    /// <summary>
    /// The product-order scenario read at OP1: the stored group codes, the variables they designate (HF-C4 is the only source; a code shared by two variables
    /// or unknown is "?", never guessed) and whether the host order equals the assignment order (null when it cannot be decided).
    /// </summary>
    public static ProductOrderView ProductOrder(
        IReadOnlyList<ScenarioObservation> obs, IReadOnlyList<VariableFinding> findings)
    {
        var product = obs.FirstOrDefault(o => o.Scenario.Kind == ScenarioKind.ProductOrder);
        var designation = findings.Where(f => f.ObservedCode is not null).ToList();
        var byCode = new Dictionary<int, string>();
        var ambiguous = new HashSet<int>();
        foreach (var f in designation)
            if (!byCode.TryAdd(f.ObservedCode!.Value, f.Variable)) ambiguous.Add(f.ObservedCode!.Value); // two variables with one code: the designation is ambiguous, never guessed
        var storedCodes = product?.Op1?.Overrides.Select(o => o.Code).ToList() ?? new List<int>();
        var storedVars = storedCodes.Select(c => byCode.TryGetValue(c, out var v) && !ambiguous.Contains(c) ? v : "?").ToList();
        bool? sameOrder = null;
        if (product is { Observed: true } && storedVars.All(v => v != "?"))
        {
            var assigned = product.Scenario.Assignments.Select(a => a.Variable).Where(v => storedVars.Contains(v)).ToList();
            sameOrder = assigned.SequenceEqual(storedVars);
        }
        int? pairs = product is { Observed: true, Op1: not null } ? product.Op1.Overrides.Count : null;
        return new ProductOrderView(storedCodes, storedVars, sameOrder, pairs);
    }

    /// <summary>
    /// HF-C4: INVALID is decided by the caller; UNKNOWN when any scenario is unobserved; OBSERVED_DIFFERS when a variable's DIFFERENT scenario stores nothing or
    /// stores a value other than the one written, a pair set at OP1 differs from OP2, the host order of the product-order scenario differs from the assignment
    /// order, or that scenario stores a number of pairs other than 8; UNKNOWN when one of those flags cannot be decided; else OBSERVED.
    /// </summary>
    public static (string Status, string Reason) FactC4(IReadOnlyList<ScenarioObservation> obs, IReadOnlyList<VariableFinding> findings)
    {
        var unobserved = obs.Where(o => !o.Observed).ToList();
        if (unobserved.Count > 0) return ("UNKNOWN", "SCENARIOS_UNOBSERVED=" + string.Join(",", unobserved.Select(o => o.Scenario.Id)));
        var order = ProductOrder(obs, findings);
        var sameOrder = order.SameOrder;
        var pairs = order.Pairs;
        var differs = new List<string>();
        foreach (var f in findings)
        {
            if (f.StoredWhenDifferent == false) differs.Add(f.Variable + ":NOT_STORED_WHEN_DIFFERENT");
            if (f.ValueStoredAsWritten == false) differs.Add(f.Variable + ":VALUE_NOT_STORED_AS_WRITTEN");
        }
        foreach (var o in obs)
            if (!o.ConsistentOp1Op2) differs.Add(o.Scenario.Id + ":OP1_DIFFERS_FROM_OP2");
        if (sameOrder == false) differs.Add("PRODUCT_ORDER:HOST_ORDER_DIFFERS_FROM_ASSIGNMENT_ORDER");
        if (pairs is { } n && n != WriteBackSpecs.Product.Count) differs.Add("PRODUCT_ORDER:PAIRS_STORED=" + n + "_EXPECTED=" + WriteBackSpecs.Product.Count);
        if (differs.Count > 0) return ("OBSERVED_DIFFERS", string.Join(";", differs));
        var unknown = new List<string>();
        if (findings.Any(f => f.StoredWhenDifferent is null || f.StoredWhenEqualToStyle is null))
            unknown.Add("A_SCENARIO_STORED_MORE_THAN_ONE_PAIR_OR_NONE_COULD_BE_DECIDED");
        if (findings.Any(f => f.ValueStoredAsWritten is null)) unknown.Add("VALUE_AS_WRITTEN_NOT_DECIDED");
        if (sameOrder is null) unknown.Add("PRODUCT_ORDER_HOST_ORDER_NOT_DECIDED");
        if (pairs is null) unknown.Add("PRODUCT_ORDER_PAIRS_NOT_DECIDED");
        if (unknown.Count > 0) return ("UNKNOWN", string.Join(";", unknown));
        return ("OBSERVED", "");
    }

    /// <summary>HF-C5: the code of each variable read from the stored pairs of the DIFFERENT scenarios (the only source: HF-C4).</summary>
    public static (string Status, string Reason) FactC5(IReadOnlyList<VariableFinding> findings)
    {
        var undetermined = findings.Where(f => f.ObservedCode is null).Select(f => f.Variable).ToList();
        if (undetermined.Count > 0) return ("UNKNOWN", "CODE_NOT_DETERMINED_FOR=" + string.Join(",", undetermined));
        var codes = findings.Select(f => f.ObservedCode!.Value).ToList();
        if (codes.Distinct().Count() != codes.Count) return ("UNKNOWN", "TWO_VARIABLES_SHARE_A_CODE");
        var differs = findings.Where(f => f.MatchesBa05Code == false).Select(f => f.Variable + "=" + f.ObservedCode + "(BA-05:" + f.Ba05Code + ")").ToList();
        if (differs.Count > 0) return ("OBSERVED_DIFFERS", string.Join(";", differs));
        return ("OBSERVED", "");
    }
}

/// <summary>
/// U-RS-2 / I-5: the dimension write-back probe (HF-C4) and the group-code designation (HF-C5), in a scratch side database. Writes the eight
/// product variables with the property setters the product uses, once different from the style and once equal to it, reads the stored
/// <c>DSTYLE</c> pairs back at OP1 (in memory, after the commit) and OP2 (save as a NEW scratch file, reopen, read). Files written:
/// <c>writeback-raw.json</c>, <c>hostfact-HF-C4.json</c>, <c>hostfact-HF-C5.json</c>, <c>rs-run-writeback.json</c>.
/// </summary>
public static class WriteBackRunner
{
    public const string Command = "CT21DHG_DIMWRITEBACK";
    public const string RawFile = "writeback-raw.json";
    public const string FactC4File = "hostfact-HF-C4.json";
    public const string FactC5File = "hostfact-HF-C5.json";
    public const string RunFile = "rs-run-writeback.json";
    public const string Op2ScratchName = "CT21D_DIMWRITEBACK_OP2.dwg";

    public static IReadOnlyList<string> OutputNames { get; } = new[] { RawFile, FactC4File, FactC5File, RunFile };

    public static RsRunOutcome Run(
        RsDesignation d,
        InstrumentInfo instrument,
        IRunEnvironment env,
        ISystemVariables vars,
        Func<ScratchRootGuard, SideDbLedger, IWriteBackHost> hostFactory,
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

        IReadOnlyList<StyleValue> style = Array.Empty<StyleValue>();
        IReadOnlyList<WriteBackScenario> scenarios = Array.Empty<WriteBackScenario>();
        IReadOnlyList<ScenarioConstruction> constructions = Array.Empty<ScenarioConstruction>();
        IReadOnlyList<DimensionReading> read1 = Array.Empty<DimensionReading>();
        IReadOnlyList<DimensionReading> read2 = Array.Empty<DimensionReading>();
        ScratchFileEntry? saved = null;
        try
        {
            var host = hostFactory(guard, sideDbs);
            using var db = host.CreateSideDatabase();
            style = db.ReadStyleValues();
            scenarios = WriteBackPlan.Build(style);
            constructions = db.AppendScenarios(scenarios);
            read1 = db.ReadDimensions();
            var target = guard.AuthorizeNew(Op2ScratchName, "WRITEBACK_OP2");
            saved = db.SaveToScratch(target);
            using var reopened = db.Reopen(guard.AuthorizeReadBack(saved));
            read2 = reopened.ReadDimensions();
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
        var verification = ScratchVerifier.Verify(d.ScratchRoot, declared);
        if (!verification.Ok)
            invalid.Add("SCRATCH_VERIFICATION_FAILED:undeclared=" + verification.Undeclared.Count + ",missing=" + verification.Missing.Count +
                        ",hashMismatches=" + verification.HashMismatches.Count + ",reparsePoints=" + verification.ReparsePoints.Count);

        try
        {
            var observations = WriteBackAnalyzer.Observe(scenarios, constructions, read1, read2);
            var findings = WriteBackAnalyzer.Findings(observations);
            var (c4, c4Reason) = WriteBackAnalyzer.FactC4(observations, findings);
            var (c5, c5Reason) = WriteBackAnalyzer.FactC5(findings);
            if (scenarios.Count == 0) { c4 = "UNKNOWN"; c4Reason = "NO_SCENARIO_RAN"; c5 = "UNKNOWN"; c5Reason = "NO_SCENARIO_RAN"; }
            var status4 = invalid.Count > 0 ? "INVALID" : c4;
            var status5 = invalid.Count > 0 ? "INVALID" : c5;
            var invalidText = string.Join(";", invalid);

            var tuple = RecordJson.Tuple(d.ToCore(), instrument, privateBefore, privateAfter, privateCopyApplies: true);
            var body = BuildBody(tuple, style, observations, findings, saved, dbmodBefore ?? -1, dbmodAfter ?? -1);
            RecordJson.Seal(body, env);
            RsSchemas.ValidateOrThrow("ct21d.writeback.v1.json", body);
            var raw = writer.WriteNewText(RawFile, RecordJson.ToFileText(body));

            var obs4 = new JsonObject
            {
                ["scenarios"] = observations.Count, ["scenariosObserved"] = observations.Count(o => o.Observed),
                ["variablesStoredWhenDifferent"] = findings.Count(f => f.StoredWhenDifferent == true),
                ["variablesStoredWhenEqualToStyle"] = findings.Count(f => f.StoredWhenEqualToStyle == true),
                ["variablesDroppedWhenEqualToStyle"] = findings.Count(f => f.StoredWhenEqualToStyle == false),
                ["consistentAtOp1AndOp2"] = observations.Count(o => o.ConsistentOp1Op2),
                ["note"] = "storage and order per override; equal-to-style behaviour; written to a scratch side database only (BA-05 V5 section 5 step 5 (a))",
            };
            writer.WriteNewText(FactC4File, RecordJson.ToFileText(HostFactRecord.Build("HF-C4", tuple, obs4, status4, invalid.Count > 0 ? invalidText : c4Reason, new[] { raw }, env)));
            var obs5 = new JsonObject
            {
                ["designation"] = RecordJson.Arr(findings.Where(f => f.ObservedCode is not null).Select(f => (JsonNode?)new JsonObject
                {
                    ["variable"] = f.Variable, ["code"] = f.ObservedCode, ["source"] = "HF-C4", ["ba05Code"] = f.Ba05Code,
                })),
                ["note"] = "the only source of the assignment code -> variable is the stored pair of the HF-C4 write probe",
            };
            writer.WriteNewText(FactC5File, RecordJson.ToFileText(HostFactRecord.Build("HF-C5", tuple, obs5, status5, invalid.Count > 0 ? invalidText : c5Reason, new[] { raw }, env)));
            var runRecord = RsRunRecord.Build(Command, "U-RS-2", d, instrument, env, sideDbs, guard, verification, pre.PriorScratch, before, after, invalid, status4 + "/" + status5);
            writer.WriteNewText(RunFile, RecordJson.ToFileText(runRecord));
            var reasons = invalid.Count > 0 ? invalid.ToList() : new List<string>();
            if (invalid.Count == 0)
            {
                if (c4Reason.Length > 0) reasons.Add("HF-C4:" + c4Reason);
                if (c5Reason.Length > 0) reasons.Add("HF-C5:" + c5Reason);
            }
            return new RsRunOutcome(status4 + "/" + status5, reasons, writer.Ledger.ToList());
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not RsRefusedException)
        {
            var declaration = RsFailSafe.TryBuild(Command, "U-RS-2", d, instrument, env, sideDbs, guard, verification, pre.PriorScratch, before, after, invalid, ex);
            if (declaration is not null)
            {
                try { writer.WriteNewText(RunFile, declaration); }
                catch (Exception wex) when (wex is IOException or EvidenceWriterException) { } // the record exists already (create-new): nothing is overwritten
            }
            throw;
        }
    }

    private static JsonObject ReadingJson(DimensionReading? r, ParsedDstyle? p)
    {
        if (r is null) return new JsonObject { ["read"] = false };
        return new JsonObject
        {
            ["read"] = true,
            ["hasAcadXData"] = r.HasAcadXData,
            ["hasDstyleSection"] = r.HasDstyleSection,
            ["wellFormed"] = r.WellFormed,
            ["entries"] = RecordJson.Arr(r.Entries.Select(e => (JsonNode?)new JsonObject
            {
                ["typeCode"] = e.TypeCode, ["hostType"] = e.HostTypeFullName, ["valueText"] = e.ValueText, ["valueBitsHex"] = e.ValueBitsHex,
            })),
            ["overrides"] = RecordJson.Arr((p?.Overrides ?? Array.Empty<StoredOverride>()).Select(o => (JsonNode?)new JsonObject
            {
                ["code"] = o.Code, ["valueTypeCode"] = o.ValueTypeCode, ["valueHostType"] = o.ValueHostType, ["valueText"] = o.ValueText, ["valueBitsHex"] = o.ValueBitsHex,
            })),
            ["parsed"] = p is not null,
            ["readError"] = r.ReadError,
        };
    }

    private static JsonObject BuildBody(JsonObject tuple, IReadOnlyList<StyleValue> style, IReadOnlyList<ScenarioObservation> observations,
        IReadOnlyList<VariableFinding> findings, ScratchFileEntry? saved, int dbmodBefore, int dbmodAfter)
    {
        var designation = findings.Where(f => f.ObservedCode is not null).ToList();
        var order = WriteBackAnalyzer.ProductOrder(observations, findings);
        var storedCodes = order.StoredCodes;
        var storedVars = order.StoredVars;
        var sameOrder = order.SameOrder;
        return new JsonObject
        {
            ["schema"] = "ct21d.writeback.v1",
            ["governing"] = false,
            ["gate"] = RecordJson.Gate,
            ["tuple"] = tuple.DeepClone(),
            ["styleValues"] = RecordJson.Arr(style.Select(s => (JsonNode?)new JsonObject
            {
                ["variable"] = s.Variable, ["hostType"] = s.HostTypeFullName, ["valueText"] = s.ValueText, ["valueBitsHex"] = s.ValueBitsHex,
            })),
            ["scenarios"] = RecordJson.Arr(observations.Select(o => (JsonNode?)new JsonObject
            {
                ["id"] = o.Scenario.Id,
                ["kind"] = o.Scenario.Kind.ToString(),
                ["variable"] = o.Scenario.Variable,
                ["assigned"] = RecordJson.Arr(o.Scenario.Assignments.Select(a => (JsonNode?)new JsonObject
                {
                    ["variable"] = a.Variable, ["valueBitsHex"] = DoubleBits.Hex(a.Value),
                })),
                ["handle"] = o.Construction?.Handle,
                ["rejection"] = o.Construction?.Rejection,
                ["op1"] = ReadingJson(o.Raw1, o.Op1),
                ["op2"] = ReadingJson(o.Raw2, o.Op2),
                ["consistentOp1Op2"] = o.ConsistentOp1Op2,
                ["status"] = o.Observed ? "OBSERVED" : "UNKNOWN",
                ["reason"] = o.Reason,
            })),
            ["findings"] = RecordJson.Arr(findings.Select(f => (JsonNode?)new JsonObject
            {
                ["variable"] = f.Variable, ["ba05Code"] = f.Ba05Code, ["observedCode"] = f.ObservedCode,
                ["storedWhenDifferent"] = f.StoredWhenDifferent, ["storedWhenEqualToStyle"] = f.StoredWhenEqualToStyle,
                ["valueStoredAsWritten"] = f.ValueStoredAsWritten, ["matchesBa05Code"] = f.MatchesBa05Code,
            })),
            ["productOrder"] = new JsonObject
            {
                ["assignedVariables"] = RecordJson.Strings(WriteBackSpecs.Product.Select(p => p.Variable)),
                ["storedCodes"] = RecordJson.Arr(storedCodes.Select(c => (JsonNode?)JsonValue.Create(c))),
                ["storedVariablesByDesignation"] = RecordJson.Strings(storedVars),
                ["hostOrderEqualsAssignmentOrder"] = sameOrder,
            },
            ["designation"] = RecordJson.Arr(designation.Select(f => (JsonNode?)new JsonObject { ["variable"] = f.Variable, ["code"] = f.ObservedCode, ["source"] = "HF-C4" })),
            ["scratch"] = new JsonObject { ["op2File"] = saved?.ToJson() },
            ["checks"] = new JsonObject { ["dbmodBefore"] = dbmodBefore, ["dbmodAfter"] = dbmodAfter },
        };
    }
}
