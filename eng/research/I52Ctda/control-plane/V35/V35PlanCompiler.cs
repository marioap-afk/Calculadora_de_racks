using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace I52Ctda.ControlPlane.V35;

// Compiles the frozen V35 rows into immutable runtime plans and emits the generated native authority. The native
// executor never parses prose: it switches over generated identifiers and interprets exactly these plans.
public static class V35PlanCompiler
{
    public const string NativeIdsPath = "eng/research/I52Ctda/native/V35Ids.inc";
    public const string NativePlanPath = "eng/research/I52Ctda/native/V35PlanTable.inc";
    public const string PlanManifestPath = "eng/research/I52Ctda/runtime-plan-v35.json";

    // Allowed catalog kinds per NPM-V35 column (closed; a new kind is authority drift and stops compilation).
    public static readonly IReadOnlyDictionary<string, string[]> ColumnKinds = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["PrimaryAuthorityId"] = ["EventId", "SchedulerId", "RetainedContract"],
        ["ScheduleOriginEventId"] = ["EventId", "Sentinel"],
        ["SchedulerChainId"] = ["SchedulerChain"],
        ["HeaderAuthority"] = ["RetainedContract"],
        ["DriverId"] = ["Driver"],
        ["ObserverRegistrationIds"] = ["ObserverRegistration"],
        ["BodyObserverId"] = ["ObserverRegistration"],
        ["GuardIds"] = ["Guard"],
        ["SetupActionIds"] = ["SetupAction"],
        ["TriggerActionId"] = ["TriggerAction"],
        ["ExecutionContextId"] = ["ExecutionContext"],
        ["PrimaryTransactionId"] = ["TransactionId"],
        ["ExecutionTransactionId"] = ["TransactionId"],
        ["TopTransactionAuthorityIds"] = ["TopTransactionAuthority"],
        ["DocumentLockIds"] = ["DocumentLockAuthority"],
        ["ThreadContextIds"] = ["RetainedContract"],
        ["MutationActionId"] = ["MutationAction"],
        ["Surface"] = ["Surface"],
        ["SchedulePoint"] = ["RetainedContract"],
        ["ExecutionPoint"] = ["RetainedContract"],
        ["ExpectedBefore"] = ["RetainedContract"],
        ["ExpectedAfter"] = ["RetainedContract"],
        ["VerifierIds"] = ["RetainedContract"],
        ["Markers"] = ["EventId", "Marker", "RetainedContract"],
        ["MarkerStage"] = ["Stage"],
        ["ObservationPredicateIds"] = ["ObservationPredicate"],
        ["UnknownPredicateIds"] = ["UnknownPredicate", "RetainedContract"],
        ["FailPredicateIds"] = ["FailPredicate"],
        ["PassClass"] = ["RetainedContract", "ResultClass"],
        ["FailClass"] = ["RetainedContract", "ResultClass"],
        ["CleanupActionId"] = ["CleanupAction"],
        ["CompletionFence"] = ["CompletionFence"],
        ["CompletionTokenIds"] = ["CompletionToken"],
    };

    public static IEnumerable<(string Column, string Id)> ColumnIds(V35RowPlan r)
    {
        yield return ("PrimaryAuthorityId", r.PrimaryAuthorityId);
        yield return ("ScheduleOriginEventId", r.ScheduleOriginEventId);
        yield return ("SchedulerChainId", r.SchedulerChainId);
        foreach (string x in r.HeaderAuthority) yield return ("HeaderAuthority", x);
        yield return ("DriverId", r.DriverId);
        foreach (string x in r.ObserverRegistrationIds) yield return ("ObserverRegistrationIds", x);
        if (r.BodyObserverId is not null) yield return ("BodyObserverId", r.BodyObserverId);
        foreach (string x in r.GuardIds) yield return ("GuardIds", x);
        foreach (string x in r.SetupActionIds) yield return ("SetupActionIds", x);
        yield return ("TriggerActionId", r.TriggerActionId);
        yield return ("ExecutionContextId", r.ExecutionContextId);
        yield return ("PrimaryTransactionId", r.PrimaryTransactionId);
        yield return ("ExecutionTransactionId", r.ExecutionTransactionId);
        foreach (string x in r.TopTransactionAuthorityIds) yield return ("TopTransactionAuthorityIds", x);
        foreach (string x in r.DocumentLockIds) yield return ("DocumentLockIds", x);
        foreach (string x in r.ThreadContextIds) yield return ("ThreadContextIds", x);
        yield return ("MutationActionId", r.MutationActionId);
        yield return ("Surface", r.Surface);
        yield return ("SchedulePoint", r.SchedulePoint);
        yield return ("ExecutionPoint", r.ExecutionPoint);
        foreach (string x in r.ExpectedBefore) yield return ("ExpectedBefore", x);
        foreach (string x in r.ExpectedAfter) yield return ("ExpectedAfter", x);
        foreach (string x in r.VerifierIds) yield return ("VerifierIds", x);
        foreach (V35Marker m in r.Markers) yield return ("Markers", m.Id);
        foreach (V35MarkerBinding b in r.MarkerStageBindings) { yield return ("Markers", b.Marker); yield return ("MarkerStage", b.Stage); }
        foreach (string x in r.ObservationPredicateIds) yield return ("ObservationPredicateIds", x);
        foreach (string x in r.UnknownPredicateIds) yield return ("UnknownPredicateIds", x);
        foreach (string x in r.FailPredicateIds) yield return ("FailPredicateIds", x);
        yield return ("PassClass", r.PassClass);
        yield return ("FailClass", r.FailClass);
        yield return ("CleanupActionId", r.CleanupActionId);
        yield return ("CompletionFence", r.CompletionFence);
        foreach (string x in r.CompletionTokenIds) yield return ("CompletionTokenIds", x);
    }

    public static IReadOnlyList<string> Violations(V35Authority authority)
    {
        var violations = new List<string>();
        if (authority.Rows.Count != 100) violations.Add($"rows {authority.Rows.Count} != 100");
        if (authority.RowSchema.Count != 36) violations.Add($"fields {authority.RowSchema.Count} != 36");
        if (authority.ByProbe.Count != authority.Rows.Count) violations.Add("duplicate ProbeId");
        foreach (V35RowPlan r in authority.Rows)
        {
            foreach ((string column, string id) in ColumnIds(r))
            {
                if (!authority.Entries.TryGetValue(id, out V35Entry? e)) { violations.Add($"{r.ProbeId}.{column}: {id} does not resolve"); continue; }
                if (!ColumnKinds[column].Contains(e.Kind)) violations.Add($"{r.ProbeId}.{column}: {id} is {e.Kind}");
            }
            foreach (V35MarkerBinding b in r.MarkerStageBindings)
                if (!r.Markers.Any(m => m.Id == b.Marker)) violations.Add($"{r.ProbeId}: binding {b.Marker}@{b.Stage} has no marker");
            foreach (string step in authority[r.CleanupActionId].List("steps"))
                if (!authority.Entries.ContainsKey(step)) violations.Add($"{r.ProbeId}: cleanup step {step} does not resolve");
        }
        return violations;
    }

    public static void AssertResolved(V35Authority authority)
    {
        IReadOnlyList<string> violations = Violations(authority);
        if (violations.Count != 0) throw new InvalidDataException("V35 plan authority does not resolve: " + string.Join("; ", violations.Take(10)));
    }

    // NPM-V35 section 4: the fixed derived execution plan, every element an identifier.
    public static IReadOnlyList<string> DerivedPlan(V35Authority authority, V35RowPlan r)
    {
        var plan = new List<string> { "RUN-ENV-01", "DRIVER-SCRIPT-01", "BOOT-01", r.DriverId };
        plan.AddRange(r.ObserverRegistrationIds.Select(x => "REGISTER:" + x));
        plan.Add("REGISTER:FIN-GATE-01");
        plan.AddRange(r.SetupActionIds.Select(x => "SETUP:" + x));
        plan.AddRange(r.GuardIds.Select(x => "ARM:" + x));
        plan.Add("TRIGGER:" + r.TriggerActionId);
        plan.AddRange(["CALLBACK-WINDOW", "POST-TRIGGER-OBLIGATIONS", "OUTCOME-RECORD"]);
        plan.AddRange(r.GuardIds.Select(x => "DISARM:" + x));
        plan.Add("TOKEN");
        plan.AddRange(authority[r.SchedulerChainId].List("sequence").Select(x => "DELIVERY:" + x));
        plan.Add("EXEC:" + r.ExecutionContextId);
        plan.Add("FIN-GATE-01:" + string.Join('+', r.CompletionTokenIds));
        plan.Add("I52CTDA_FINISH");
        plan.AddRange(r.VerifierIds.Select(x => "VERIFY:" + x));
        plan.AddRange(authority[r.CleanupActionId].List("steps").Select(x => "CLEANUP:" + x));
        plan.Add("FENCE:" + r.CompletionFence);
        plan.Add("CONTROL-PLANE-RESULT-01:RESULT-RULE-V35");
        return plan;
    }

    public static string NativeName(string id)
    {
        var text = new StringBuilder();
        foreach (char c in id.ToUpperInvariant()) text.Append(char.IsAsciiLetterOrDigit(c) ? c : '_');
        string name = text.ToString();
        while (name.Contains("__", StringComparison.Ordinal)) name = name.Replace("__", "_", StringComparison.Ordinal);
        return name.Trim('_');
    }

    public static string GenerateNativeIds(V35Authority authority)
    {
        var text = new StringBuilder();
        text.Append("// Generated by `harness generate-v35` from EXEC-CATALOG-V35-1 (V35 freeze ").Append(V35Freeze.PackageHash).Append("). Do not edit.\n");
        text.Append("// I52_ID(NativeName, L\"CatalogId\", L\"Kind\")\n");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (V35Entry e in authority.Entries.Values.OrderBy(e => e.Id, StringComparer.Ordinal))
        {
            string name = NativeName(e.Id);
            if (!seen.Add(name)) throw new InvalidDataException($"native identifier collision: {e.Id} -> {name}");
            text.Append("I52_ID(").Append(name).Append(", L\"").Append(e.Id).Append("\", L\"").Append(e.Kind).Append("\")\n");
        }
        return text.ToString();
    }

    // Catalog attributes the native interpreter validates against (token stages, stage namespaces, marker stages,
    // lock-release anchors, guard keys, triggers, chains, execution contexts, registrations, write sets, cleanup steps).
    public static string GenerateNativeAuthority(V35Authority authority)
    {
        var text = new StringBuilder();
        text.Append("// Generated by `harness generate-v35` from EXEC-CATALOG-V35-1 attributes (V35 freeze ").Append(V35Freeze.PackageHash).Append("). Do not edit.\n");
        string n(string id) => "I52Id::" + NativeName(id);
        string s(string? value) => "L\"" + (value ?? "") + "\"";
        IEnumerable<V35Entry> kind(string k) => authority.Entries.Values.Where(e => e.Kind == k).OrderBy(e => e.Id, StringComparer.Ordinal);
        text.Append("#ifdef I52_TOKEN_STAGE\n");
        foreach (V35Entry e in kind("CompletionToken"))
        {
            string stage = e.Attr("stage")!;
            if (stage.StartsWith("BOUND:", StringComparison.Ordinal)) text.Append("I52_TOKEN_BOUND(").Append(n(e.Id)).Append(", ").Append(n(stage["BOUND:".Length..])).Append(")\n");
            else foreach (string part in stage.Split('|')) text.Append("I52_TOKEN_STAGE(").Append(n(e.Id)).Append(", ").Append(n(part)).Append(")\n");
        }
        text.Append("#endif\n#ifdef I52_STAGE_NAMESPACE\n");
        foreach (V35Entry e in kind("Stage")) text.Append("I52_STAGE_NAMESPACE(").Append(n(e.Id)).Append(", ").Append(s(e.Attr("namespace"))).Append(")\n");
        text.Append("#endif\n#ifdef I52_MARKER_STAGE\n");
        foreach ((string marker, JsonNode? stages) in authority["MARKER-STAGE-BIND-01"].Raw["markerStages"]!.AsObject())
            foreach (JsonNode? stage in stages!.AsArray()) text.Append("I52_MARKER_STAGE(").Append(n(marker)).Append(", ").Append(n(stage!.GetValue<string>())).Append(")\n");
        text.Append("#endif\n#ifdef I52_LOCK_ANCHOR\n");
        foreach ((string stage, JsonNode? anchor) in authority["LOCK-RELEASE-BIND-01"].Raw["anchors"]!.AsObject())
            text.Append("I52_LOCK_ANCHOR(").Append(n(stage)).Append(", ").Append(s(anchor!.GetValue<string>())).Append(")\n");
        text.Append("#endif\n#ifdef I52_GUARD\n");
        foreach (V35Entry e in kind("Guard"))
        {
            string target = e.Attr("armTarget")!;
            text.Append("I52_GUARD(").Append(n(e.Id)).Append(", ").Append(s(e.Attr("family"))).Append(", ")
                .Append(authority.Entries.ContainsKey(target) ? n(target) : "I52Id::None").Append(", ").Append(s(target)).Append(")\n");
            if (e.Raw["keySchema"] is JsonArray flat)
                foreach (JsonNode? k in flat) text.Append("I52_GUARD_KEY(").Append(n(e.Id)).Append(", ").Append(s(e.Attr("family"))).Append(", ").Append(s(k!.GetValue<string>())).Append(")\n");
            else if (e.Raw["keySchema"] is JsonObject families)
                foreach ((string family, JsonNode? keys) in families)
                    foreach (JsonNode? k in keys!.AsArray()) text.Append("I52_GUARD_KEY(").Append(n(e.Id)).Append(", ").Append(s(family)).Append(", ").Append(s(k!.GetValue<string>())).Append(")\n");
        }
        text.Append("#endif\n#ifdef I52_TRIGGER\n");
        foreach (V35Entry e in kind("TriggerAction"))
            text.Append("I52_TRIGGER(").Append(n(e.Id)).Append(", ").Append(e.Attr("target") is { } t ? n(t) : "I52Id::None").Append(", ")
                .Append(s(e.Attr("openMode"))).Append(", ").Append(s(e.Attr("targetClass"))).Append(", ").Append(n(e.Attr("driver")!)).Append(", ")
                .Append(e.Flag("modelSpaceWrite") ? "true" : "false").Append(")\n");
        text.Append("#endif\n#ifdef I52_CHAIN_STEP\n");
        foreach (V35Entry e in kind("SchedulerChain"))
        {
            IReadOnlyList<string> seq = e.List("sequence");
            for (int i = 0; i < seq.Count; i++) text.Append("I52_CHAIN_STEP(").Append(n(e.Id)).Append(", ").Append(n(seq[i])).Append(", ").Append(i).Append(")\n");
        }
        text.Append("#endif\n#ifdef I52_EXEC\n");
        foreach (V35Entry e in kind("ExecutionContext"))
        {
            text.Append("I52_EXEC(").Append(n(e.Id)).Append(", ").Append(n(e.Attr("transaction")!)).Append(")\n");
            foreach (string l in e.List("locks")) text.Append("I52_EXEC_LOCK(").Append(n(e.Id)).Append(", ").Append(n(l)).Append(")\n");
        }
        text.Append("#endif\n#ifdef I52_REGISTRATION\n");
        foreach (V35Entry e in kind("ObserverRegistration"))
            text.Append("I52_REGISTRATION(").Append(n(e.Id)).Append(", ").Append(s(e.Attr("reactorClass"))).Append(", ").Append(s(e.Attr("notifier"))).Append(", ")
                .Append(e.Flag("objectReactor") ? "true" : "false").Append(")\n");
        text.Append("#endif\n#ifdef I52_MUTATION_WRITE\n");
        foreach (V35Entry e in kind("MutationAction"))
            foreach (string w in e.List("writeSet")) text.Append("I52_MUTATION_WRITE(").Append(n(e.Id)).Append(", ").Append(n(w)).Append(")\n");
        text.Append("#endif\n#ifdef I52_CLEANUP_STEP\n");
        foreach (V35Entry e in kind("CleanupAction"))
            foreach (string step in e.List("steps")) text.Append("I52_CLEANUP_STEP(").Append(n(e.Id)).Append(", ").Append(n(step)).Append(")\n");
        text.Append("#endif\n#ifdef I52_SETUP_ORDER\n");
        foreach (string setup in authority["DRIVER-CMD-01"].List("setupOrder")) text.Append("I52_SETUP_ORDER(").Append(n(setup)).Append(")\n");
        text.Append("#endif\n#ifdef I52_PHASE\n");
        foreach (string driver in new[] { "DRIVER-CMD-01", "DRIVER-APP-01" })
            foreach (string phase in authority[driver].List("phaseOrder")) text.Append("I52_PHASE(").Append(n(driver)).Append(", ").Append(s(phase)).Append(")\n");
        text.Append("#endif\n#ifdef I52_FINISH_GATE\n");
        V35Entry gate = authority["FIN-GATE-01"];
        text.Append("I52_FINISH_GATE(").Append(gate.Raw["timeoutSeconds"]!.GetValue<long>()).Append(", ").Append(gate.Raw["externalDeadlineSeconds"]!.GetValue<long>())
            .Append(", ").Append(gate.Raw["postFinishDeadlineSeconds"]!.GetValue<long>()).Append(")\n");
        text.Append("#endif\n");
        return text.ToString();
    }

    public const string NativeAuthorityPath = "eng/research/I52Ctda/native/V35Authority.inc";

    public static string GenerateNativePlans(V35Authority authority)
    {
        var text = new StringBuilder();
        text.Append("// Generated by `harness generate-v35` from NPM-V35 (V35 freeze ").Append(V35Freeze.PackageHash).Append("). Do not edit.\n");
        text.Append("// One immutable plan per ProbeId; the row approval hash equals v35-oracle.json Z_approvalFreeze.rows.\n");
        int index = 0;
        var rowsText = new StringBuilder();
        foreach (V35RowPlan r in authority.Rows)
        {
            string p = $"kP{index:D3}";
            string list(string suffix, IReadOnlyList<string> ids)
            {
                if (ids.Count == 0) return "nullptr, 0";
                text.Append("static const I52Id ").Append(p).Append('_').Append(suffix).Append("[] = { ")
                    .Append(string.Join(", ", ids.Select(i => "I52Id::" + NativeName(i)))).Append(" };\n");
                return $"{p}_{suffix}, {ids.Count}";
            }
            string markers()
            {
                if (r.Markers.Count == 0) return "nullptr, 0";
                text.Append("static const I52PlanMarker ").Append(p).Append("_Markers[] = { ")
                    .Append(string.Join(", ", r.Markers.Select(m => $"{{ {(m.MustBePresent ? "true" : "false")}, I52Id::{NativeName(m.Id)} }}"))).Append(" };\n");
                return $"{p}_Markers, {r.Markers.Count}";
            }
            string bindings()
            {
                if (r.MarkerStageBindings.Count == 0) return "nullptr, 0";
                text.Append("static const I52PlanBinding ").Append(p).Append("_Bindings[] = { ")
                    .Append(string.Join(", ", r.MarkerStageBindings.Select(b => $"{{ I52Id::{NativeName(b.Marker)}, I52Id::{NativeName(b.Stage)} }}"))).Append(" };\n");
                return $"{p}_Bindings, {r.MarkerStageBindings.Count}";
            }
            string id(string? value) => value is null ? "I52Id::None" : "I52Id::" + NativeName(value);
            string[] cells =
            [
                $"L\"{r.ProbeId}\"", $"L\"{r.RowApprovalHash}\"", id(r.PrimaryAuthorityId), id(r.ScheduleOriginEventId), id(r.SchedulerChainId),
                list("Header", r.HeaderAuthority), id(r.DriverId), list("Registrations", r.ObserverRegistrationIds), id(r.BodyObserverId),
                list("Guards", r.GuardIds), list("Setup", r.SetupActionIds), id(r.TriggerActionId), id(r.ExecutionContextId),
                id(r.PrimaryTransactionId), id(r.ExecutionTransactionId), list("Top", r.TopTransactionAuthorityIds), list("Locks", r.DocumentLockIds),
                list("Contexts", r.ThreadContextIds), id(r.MutationActionId), id(r.Surface), id(r.SchedulePoint), id(r.ExecutionPoint),
                list("Before", r.ExpectedBefore), list("After", r.ExpectedAfter), list("Verifiers", r.VerifierIds), markers(), bindings(),
                list("Obs", r.ObservationPredicateIds), list("Unk", r.UnknownPredicateIds), list("Fail", r.FailPredicateIds),
                id(r.PassClass), id(r.FailClass), id(r.CleanupActionId), list("CleanupSteps", authority[r.CleanupActionId].List("steps")),
                id(r.CompletionFence), list("Tokens", r.CompletionTokenIds)
            ];
            rowsText.Append("    { ").Append(string.Join(", ", cells)).Append(" },\n");
            index++;
        }
        text.Append("static const I52RowPlan kI52Plans[] = {\n").Append(rowsText).Append("};\n");
        text.Append("static constexpr size_t kI52PlanCount = ").Append(index).Append(";\n");
        return text.ToString();
    }

    public static JsonObject PlanManifest(V35Authority authority, string repository, string nativeIds, string nativePlans, string nativeAuthority)
    {
        var rows = new JsonArray();
        foreach (V35RowPlan r in authority.Rows)
        {
            var row = JsonSerializer.SerializeToNode(r)!.AsObject();
            row["DerivedPlan"] = new JsonArray(DerivedPlan(authority, r).Select(x => (JsonNode)x!).ToArray());
            rows.Add(row);
        }
        return new JsonObject
        {
            ["schemaVersion"] = 1,
            ["initiative"] = "I-52",
            ["contract"] = authority.Catalog["contract"]!.GetValue<string>(),
            ["revision"] = V35Freeze.Revision,
            ["freezePackageHash"] = V35Freeze.PackageHash,
            ["freezeSha"] = V35Freeze.FreezeSha,
            ["catalogBlob"] = V35Freeze.GitBlobId(Path.Combine(repository, V35Authority.CatalogPath)),
            ["matrixBlob"] = V35Freeze.GitBlobId(Path.Combine(repository, V35Authority.MatrixPath)),
            ["generated"] = new JsonObject
            {
                [NativeIdsPath] = Sha256(nativeIds),
                [NativePlanPath] = Sha256(nativePlans),
                [NativeAuthorityPath] = Sha256(nativeAuthority),
            },
            ["planCount"] = authority.Rows.Count,
            ["rows"] = rows,
        };
    }

    public static string Sha256(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    public sealed record GeneratedAuthority(string NativeIds, string NativePlans, string NativeAuthority, string PlanManifest);

    public static GeneratedAuthority Generate(V35Authority authority, string repository)
    {
        string ids = GenerateNativeIds(authority);
        string plans = GenerateNativePlans(authority);
        string nativeAuthority = GenerateNativeAuthority(authority);
        string manifest = PlanManifest(authority, repository, ids, plans, nativeAuthority).ToJsonString(new JsonSerializerOptions { WriteIndented = true }).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
        return new(ids, plans, nativeAuthority, manifest);
    }

    public static void Write(string repository, GeneratedAuthority generated)
    {
        var utf8 = new UTF8Encoding(false);
        File.WriteAllText(Path.Combine(repository, NativeIdsPath), generated.NativeIds, utf8);
        File.WriteAllText(Path.Combine(repository, NativePlanPath), generated.NativePlans, utf8);
        File.WriteAllText(Path.Combine(repository, NativeAuthorityPath), generated.NativeAuthority, utf8);
        File.WriteAllText(Path.Combine(repository, PlanManifestPath), generated.PlanManifest, utf8);
    }

    // Generated files are committed; any difference from a fresh regeneration is drift.
    public static IReadOnlyList<string> Drift(string repository, GeneratedAuthority generated)
    {
        var drift = new List<string>();
        void check(string path, string expected)
        {
            string file = Path.Combine(repository, path);
            if (!File.Exists(file)) { drift.Add(path + " missing"); return; }
            if (File.ReadAllText(file).Replace("\r\n", "\n", StringComparison.Ordinal) != expected) drift.Add(path + " differs from regeneration");
        }
        check(NativeIdsPath, generated.NativeIds);
        check(NativePlanPath, generated.NativePlans);
        check(NativeAuthorityPath, generated.NativeAuthority);
        check(PlanManifestPath, generated.PlanManifest);
        return drift;
    }
}
