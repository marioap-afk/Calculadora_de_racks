using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace I52Ctda.ControlPlane.V35;

public sealed record V35ConformanceReport(
    int Plans, int NativePlanRows, int DistinctProbeIds, int EventIds, int ReachableCallbacks, int Schedulers, int SupportActions, int FixtureIdentities,
    IReadOnlyList<string> GeneratedDrift, IReadOnlyList<string> Violations)
{
    public bool Holds => Violations.Count == 0 && GeneratedDrift.Count == 0 && Plans == 100 && NativePlanRows == 100 && DistinctProbeIds == 100
        && EventIds == 27 && ReachableCallbacks == 26 && Schedulers == 3 && SupportActions == 7 && FixtureIdentities == 8;
}

// Static conformance of the R3 runtime to the frozen V35 authority. It is not a second oracle: plans are checked
// against the Architect-approved row hashes (v35-oracle.json Z) and the runtime sources are checked for an
// implementation of every identifier the 100 plans use.
public static class V35Conformance
{
    public static V35ConformanceReport Check(string repository, V35Authority authority)
    {
        var violations = new List<string>();
        string research = Path.Combine(repository, "eng", "research", "I52Ctda");
        string Read(params string[] parts) => File.ReadAllText(Path.Combine(research, Path.Combine(parts)));
        string native = string.Join('\n', Directory.GetFiles(Path.Combine(research, "native"), "*.cpp").Concat(Directory.GetFiles(Path.Combine(research, "native"), "*.h")).Select(File.ReadAllText));
        string payload = Read("payload", "I52CtdaPayload.cpp");
        string managed = string.Join('\n', Directory.GetFiles(Path.Combine(research, "managed-observer"), "*.cs").Select(File.ReadAllText));
        string engine = Read("control-plane", "V35", "V35ResultEngine.cs");
        string planTable = Read("native", "V35PlanTable.inc");

        // Plans: generated freshness, 100 rows, row hashes equal the approval freeze.
        IReadOnlyList<string> drift = V35PlanCompiler.Drift(repository, V35PlanCompiler.Generate(authority, repository));
        JsonObject oracle = JsonNode.Parse(File.ReadAllText(Path.Combine(repository, V35Authority.OraclePath)))!.AsObject();
        JsonObject approvedRows = oracle["Z_approvalFreeze"]!["rows"]!.AsObject();
        foreach (V35RowPlan r in authority.Rows)
            if (approvedRows[r.ProbeId]?.GetValue<string>() != r.RowApprovalHash) violations.Add($"{r.ProbeId}: plan hash differs from the approved row hash");
        if (approvedRows.Count != authority.Rows.Count) violations.Add("approved row count differs");
        int nativeRows = Regex.Matches(planTable, @"^    \{ L""", RegexOptions.Multiline).Count;
        int distinct = Regex.Matches(planTable, @"^    \{ L""([^""]+)""", RegexOptions.Multiline).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal).Count();
        if (!Regex.IsMatch(planTable, @"kI52PlanCount = 100;")) violations.Add("kI52PlanCount is not 100");

        // Every identifier kind the native interpreter dispatches on has a case for every id the plans use.
        void Cases(string label, IEnumerable<string> ids)
        {
            foreach (string id in ids.Distinct())
                if (!native.Contains("case I52Id::" + V35PlanCompiler.NativeName(id) + ":", StringComparison.Ordinal)) violations.Add($"native has no case for {label} {id}");
        }
        Cases("TriggerAction", authority.Rows.Select(r => r.TriggerActionId));
        Cases("SetupAction", authority.Rows.SelectMany(r => r.SetupActionIds));
        Cases("ExecutionContext", authority.Rows.Select(r => r.ExecutionContextId));
        Cases("MutationAction", authority.Rows.Select(r => r.MutationActionId));
        Cases("ObserverRegistration", authority.Rows.SelectMany(r => r.ObserverRegistrationIds));
        Cases("Verifier", authority.Rows.SelectMany(r => r.VerifierIds));
        Cases("CleanupStep", authority.Rows.SelectMany(r => authority[r.CleanupActionId].List("steps")));

        // Driver phases come from the generated phaseOrder; every phase name has a handler.
        foreach (string driver in new[] { "DRIVER-CMD-01", "DRIVER-APP-01" })
            foreach (string phase in authority[driver].List("phaseOrder"))
                if (!native.Contains($"phase == L\"{phase}\"", StringComparison.Ordinal)) violations.Add($"native has no handler for phase {phase}");

        // Guards: every guard id and every keySchema key are generated authority; the special guards are handled.
        foreach (string guard in authority.Rows.SelectMany(r => r.GuardIds).Distinct())
            if (!Read("native", "V35Authority.inc").Contains("I52_GUARD(I52Id::" + V35PlanCompiler.NativeName(guard), StringComparison.Ordinal)) violations.Add("guard not generated: " + guard);
        foreach (string key in authority.IdsOfKind("Guard").SelectMany(g => KeyNames(authority[g])).Distinct())
            if (key is not ("ProbeId" or "GuardId" or "DatabaseId" or "DocumentId" or "EventId" or "CallbackMember" or "TargetObjectId" or "ReactorInstance" or "TransactionManagerId" or "StageId"
                or "TransactionIdentity" or "CommandName" or "RequestId" or "ModulePath" or "TargetInstancePointer" or "ErasingFlag"))
                violations.Add("unhandled guard key " + key);

        // Completion tokens, stages and markers are produced by some runtime module.
        string runtime = native + payload + managed;
        foreach (string token in authority.IdsOfKind("CompletionToken"))
            if (!runtime.Contains("I52Id::" + V35PlanCompiler.NativeName(token), StringComparison.Ordinal) && !runtime.Contains($"\"{token}\"", StringComparison.Ordinal)) violations.Add("token never set: " + token);
        foreach (string stage in authority.IdsOfKind("Stage"))
            if (!runtime.Contains("I52Id::" + V35PlanCompiler.NativeName(stage), StringComparison.Ordinal) && !runtime.Contains($"\"{stage}\"", StringComparison.Ordinal)) violations.Add("stage never opened: " + stage);
        foreach (string marker in authority.Rows.SelectMany(r => r.Markers).Select(m => m.Id).Where(m => m.StartsWith("MARK-", StringComparison.Ordinal)).Distinct())
            if (!runtime.Contains("I52Id::" + V35PlanCompiler.NativeName(marker), StringComparison.Ordinal) && !runtime.Contains($"\"{marker}\"", StringComparison.Ordinal)) violations.Add("marker never recorded: " + marker);
        foreach (string eventId in authority.IdsOfKind("EventId").Where(e => authority[e].Flag("reachable")))
            if (!native.Contains("I52Id::" + V35PlanCompiler.NativeName(eventId), StringComparison.Ordinal)) violations.Add("reachable EventId without a native callback: " + eventId);

        // Result authority: every observation and fail predicate the plans use, and every ExpectedAfter state, is evaluated.
        foreach (string obs in authority.Rows.SelectMany(r => r.ObservationPredicateIds).Distinct())
            if (!engine.Contains($"case \"{obs}\"", StringComparison.Ordinal)) violations.Add("engine does not evaluate " + obs);
        foreach (string fail in authority.Rows.SelectMany(r => r.FailPredicateIds).Distinct().Where(f => f != "FP-CLEANUP-SAFETY"))
            if (!engine.Contains($"case \"{fail}\"", StringComparison.Ordinal)) violations.Add("engine does not evaluate " + fail);
        foreach (string state in authority.Rows.SelectMany(r => r.ExpectedAfter).Distinct())
            if (!engine.Contains($"case \"{state}\"", StringComparison.Ordinal)) violations.Add("engine does not compare " + state);
        if (Regex.IsMatch(engine, @"""(0[0-9A-Z]|1[0-9]|C[A-Z0-9])[A-Z0-9-]*-(S|M|SM|ALL)""")) violations.Add("engine special-cases a ProbeId");

        // Frozen ABI: the four LOG-SEQ-01 exports, a read-only fence query and no exported setter.
        foreach (string export in new[] { "I52Ctda_LogAppend", "I52Ctda_TokenSet", "I52Ctda_QueryC15Arm", "I52Ctda_FinishFenceIsSet" })
            if (!Regex.IsMatch(native, @"extern ""C"" __declspec\(dllexport\) [a-z0-9_]+ " + export + @"\(")) violations.Add("LOG-SEQ-01 export missing: " + export);
        if (Regex.IsMatch(native, @"dllexport\)[^\n]*FinishFenceSet")) violations.Add("fence setter exported");
        if (!Regex.IsMatch(native, @"int32_t I52Ctda_FinishFenceIsSet\(void\)")) violations.Add("fence read ABI signature");
        if (!payload.Contains("\"I52Ctda_FinishFenceIsSet\"", StringComparison.Ordinal) || !managed.Contains("I52Ctda_FinishFenceIsSet", StringComparison.Ordinal)) violations.Add("a reader does not import the fence read ABI");
        if (payload.IndexOf("fenceIsSet() != 0", StringComparison.Ordinal) is var fenceAt && (fenceAt < 0 || fenceAt > payload.IndexOf("object->objectId()", StringComparison.Ordinal))) violations.Add("payload callback does not read the fence first");
        if (payload.IndexOf("PAYLOAD-DB-BINDING", StringComparison.Ordinal) > payload.IndexOf("queryArm(&arm)", StringComparison.Ordinal)) violations.Add("payload queries the C15 arm before PAYLOAD-DB-BINDING");
        if (!payload.Contains("phase=ARM;family=N-DB-MOD", StringComparison.Ordinal) || !payload.Contains("phase=DISARM;family=N-DB-MOD", StringComparison.Ordinal)) violations.Add("payload guard ARM/DISARM not paired by family");
        if (!native.Contains("fact(I52Id::CTX_CALLBACK", StringComparison.Ordinal) || !native.Contains("x.fact(I52Id::CTX_CALLBACK", StringComparison.Ordinal) || !payload.Contains("L\"CTX-CALLBACK\"", StringComparison.Ordinal)) violations.Add("CTX-CALLBACK not recorded by every callback context");

        // Fixture: eight identities; the non-identities are exactly the frozen ones.
        JsonObject fixture = JsonNode.Parse(File.ReadAllText(Path.Combine(repository, V35Authority.FixturePath)))!.AsObject();
        int identities = fixture["identities"]!.AsArray().Count;
        if (!Regex.IsMatch(Read("native", "I52CtdaFixture.h"), @"kDeclaredIdentities = 8;")) violations.Add("native declared identities is not 8");
        foreach (JsonNode? res in fixture["nonIdentityResources"]!.AsArray())
            if (authority.KindOf(res!["id"]!.GetValue<string>()) != res["kind"]!.GetValue<string>()) violations.Add("resource class drift: " + res["id"]);

        // Structural UNKNOWN freeze (15 rows) is recorded, not special-cased.
        JsonObject manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(repository, V35Freeze.ManifestPath)))!.AsObject();
        if (manifest["package"]!["structuralUnknownRows"]!.AsArray().Count != 15) violations.Add("structural UNKNOWN freeze is not 15 rows");

        int eventIds = authority.IdsOfKind("EventId").Count();
        int callbacks = authority.Rows.Select(r => r.PrimaryAuthorityId).Where(p => authority.KindOf(p) == "EventId").Distinct().Count();
        return new(authority.Rows.Count, nativeRows, distinct, eventIds, callbacks, authority.IdsOfKind("SchedulerId").Count(), authority.IdsOfKind("SupportAction").Count(),
            identities, drift, violations);
    }

    private static IEnumerable<string> KeyNames(V35Entry guard) => guard.Raw["keySchema"] switch
    {
        JsonArray flat => flat.Select(k => k!.GetValue<string>()),
        JsonObject families => families.SelectMany(f => f.Value!.AsArray().Select(k => k!.GetValue<string>())),
        _ => [],
    };
}
