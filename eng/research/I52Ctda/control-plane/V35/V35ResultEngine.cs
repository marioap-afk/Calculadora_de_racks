using System.Globalization;

namespace I52Ctda.ControlPlane.V35;

public enum V35Tri { True, False, Unavailable }

public sealed record V35Result(
    string ProbeId,
    string Result,
    string ResultClass,
    int Step,
    bool EvidenceComplete,
    bool SafetyEvidenceComplete,
    IReadOnlyList<string> EvidenceGaps,
    IReadOnlyDictionary<string, V35Tri> Observations,
    IReadOnlyList<string> UnknownsHolding,
    IReadOnlyList<string> FailsHolding,
    string VerifierComparison,
    IReadOnlyList<string> Reasons);

// CONTROL-PLANE-RESULT-01: the only authority that classifies a row. It runs outside AutoCAD after the exact PID
// exited (or was terminated) and applies RESULT-RULE-V35 in its five ordered steps to the single LOG-SEQ-01 log, the
// exact-PID exit evidence, the scratch-DWG integrity evidence and the cleanup/fence records. Evaluation is
// conservative: missing or ambiguous evidence is UNKNOWN; only an explicit contradiction with complete evidence is FAIL.
public sealed class V35ResultEngine(V35Authority authority)
{
    private static readonly string[] OrderPairs =
    [
        "N-TR-ABOUT-START>N-TR-STARTED", "N-TR-ABOUT-ABORT>N-TR-ABORTED", "N-TR-ABOUT-END>N-TR-ENDED", "N-DOC-LOCK-WILL>N-DOC-LOCK-CHANGED",
        "N-DOC-LOCK-WILL>N-DOC-LOCK-VETO", "N-ED-WILL>N-ED-END", "N-ED-WILL>N-ED-CANCEL", "N-RX-WILL-LOAD>N-RX-LOADED"
    ];

    public V35Result Evaluate(V35RowPlan plan, V35RunEvidence evidence)
    {
        var log = new V35Log(evidence.Records.Where(r => r.ProbeId == plan.ProbeId || r.ProbeId == "").ToArray());
        var ctx = new Context(authority, plan, log, evidence);
        var reasons = new List<string>();

        (bool complete, List<string> gaps) = ctx.EvidenceComplete();
        bool safetyComplete = ctx.SafetyEvidenceComplete();
        var observations = plan.ObservationPredicateIds.ToDictionary(id => id, ctx.Observation, StringComparer.Ordinal);
        string comparison = ctx.VerifierComparison();

        // Step 1 (safety): SAFETY-EVIDENCE-COMPLETE and FP-CLEANUP-SAFETY give the row's FailClass before UNKNOWN.
        if (safetyComplete && ctx.CleanupSafetyViolated(out string safetyReason))
        {
            reasons.Add("FP-CLEANUP-SAFETY: " + safetyReason);
            return new(plan.ProbeId, "FAIL", plan.FailClass, 1, complete, true, gaps, observations, [], ["FP-CLEANUP-SAFETY"], comparison, reasons);
        }

        // Step 2 (UNKNOWN).
        var unknowns = new List<string>();
        if (!complete) reasons.Add("EVIDENCE-COMPLETE false: " + string.Join("; ", gaps));
        if (ctx.UnknownCommon(out string commonReason)) { unknowns.Add("UNKNOWN-COMMON"); reasons.Add("UNKNOWN-COMMON: " + commonReason); }
        foreach (string id in plan.UnknownPredicateIds.Where(id => id != "UNKNOWN-COMMON"))
            if (ctx.Unknown(id, out string why)) { unknowns.Add(id); reasons.Add(id + ": " + why); }
        var unavailable = observations.Where(o => o.Value == V35Tri.Unavailable).Select(o => o.Key).ToArray();
        foreach (string id in unavailable) reasons.Add(id + ": unavailable");
        if (!complete || unknowns.Count > 0 || unavailable.Length > 0)
            return new(plan.ProbeId, "UNKNOWN", "UNKNOWN", 2, complete, safetyComplete, gaps, observations, unknowns, [], comparison, reasons);

        // Step 3 (FAIL): any other listed FailPredicateId, with complete evidence.
        var fails = plan.FailPredicateIds.Where(id => id != "FP-CLEANUP-SAFETY" && ctx.Fail(id, observations, comparison)).ToArray();
        if (fails.Length > 0)
        {
            reasons.AddRange(fails.Select(f => f + ": holds"));
            return new(plan.ProbeId, "FAIL", plan.FailClass, 3, true, safetyComplete, gaps, observations, [], fails, comparison, reasons);
        }

        // Step 4 (PASS): PassClass, every observation true, verifier comparison equals ExpectedAfter, cleanup and fence complete.
        bool allTrue = observations.Values.All(v => v == V35Tri.True);
        bool fence = ctx.CleanupAndFenceComplete(out string fenceReason);
        if (allTrue && comparison == "EQUAL" && fence)
            return new(plan.ProbeId, "PASS", plan.PassClass, 4, true, safetyComplete, gaps, observations, [], [], comparison, reasons);

        // Step 5: an available but false observation without a matching FailPredicate establishes no contrary property.
        if (!allTrue) reasons.AddRange(observations.Where(o => o.Value != V35Tri.True).Select(o => o.Key + ": false"));
        if (comparison != "EQUAL") reasons.Add("verifier comparison: " + comparison);
        if (!fence) reasons.Add("cleanup/fence: " + fenceReason);
        return new(plan.ProbeId, "UNKNOWN", "UNKNOWN", 5, true, safetyComplete, gaps, observations, [], [], comparison, reasons);
    }

    private sealed class Context(V35Authority authority, V35RowPlan plan, V35Log log, V35RunEvidence evidence)
    {
        private const string S0 = "HFV30:S:0", S1 = "HFV30:S:1";

        private static bool Ok(V35LogRecord? r, string key = "status") => r is not null && r.P(key) == "0";
        private static int Int(string value) => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : int.MinValue;
        private static double Dbl(string value) => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : double.NaN;
        private bool Governed(string stage) => authority.Entries.TryGetValue(stage, out V35Entry? e) && e.Attr("namespace") == "GOVERNED";
        private bool HasToken(string token) => plan.CompletionTokenIds.Contains(token);
        private bool TokenAccepted(string token) => log.Any(token, r => r.Is("status", "ACCEPTED"));
        private long CleanupStart => log.First(plan.CleanupActionId, r => r.Is("phase", "START"))?.Sequence ?? long.MaxValue;
        private bool PayloadLoaded => log.Any("MARK-LOAD-RESULT", r => r.Is("loaded", "1"));
        private bool ManagedRegistered => plan.ObserverRegistrationIds.Contains("RR-MANAGED-CMD");
        private IEnumerable<string> CleanupSteps => authority[plan.CleanupActionId].List("steps");

        // ---------------------------------------------------------------- EVIDENCE-COMPLETE (closed, 8 items)
        public (bool, List<string>) EvidenceComplete()
        {
            var gaps = new List<string>();
            // (1) phase records
            if (!log.Any("RUN-ENV-01")) gaps.Add("(1) RUN-ENV-01 record");
            if (!log.Any("BOOT-01")) gaps.Add("(1) I52CTDA_BOOT record");
            if (!log.Any("CMD-PROBE", r => r.Is("phase", "ENTRY")) || !log.Any("CMD-PROBE", r => r.Is("phase", "RETURN"))) gaps.Add("(1) I52CTDA_PROBE records");
            if (plan.TriggerActionId == "TRG-RUN-FIXTURE-CMD" && !log.Any("CMD-FIXTURE")) gaps.Add("(1) I52CTDA_FIXTURE record");
            if (plan.TriggerActionId == "TRG-CANCEL-CMDCTX" && !log.Any("CMD-CANCEL")) gaps.Add("(1) HFV34_CANCEL record");
            if (!log.Any("FIN-GATE-01", r => r.Is("phase", "ISSUE"))) gaps.Add("(1) FIN-GATE-01 record");
            if (!log.Any("CMD-FINISH", r => r.Is("phase", "ENTRY"))) gaps.Add("(1) I52CTDA_FINISH record");
            // (2) every CompletionTokenId set
            foreach (string t in plan.CompletionTokenIds) if (!TokenAccepted(t)) gaps.Add("(2) token " + t);
            // (3) markers
            foreach (V35Marker m in plan.Markers)
            {
                int n = MarkerMatches(m.Id).Count();
                if (m.MustBePresent && (Bound(m.Id) ? n != 1 : n < 1)) gaps.Add($"(3) +{m.Id} matched {n}");
                if (!m.MustBePresent && n != 0) gaps.Add($"(3) -{m.Id} observed {n}");
            }
            // (4) verifier reads
            foreach (string v in plan.VerifierIds) if (!log.Any(v, r => r.Is("phase", "FRESH"))) gaps.Add("(4) verifier " + v);
            // (5) cleanup steps
            foreach (string step in CleanupSteps)
                if (!log.Any(step, r => r.Is("phase", "START")) || !log.Any(step, r => r.Is("phase", "END"))) gaps.Add("(5) cleanup step " + step);
            // (6) exact-PID exit and scratch integrity evidence
            if (evidence.Process is null || !evidence.Process.ProcessGone) gaps.Add("(6) exact-PID exit evidence");
            if (evidence.Scratch is null || !evidence.Scratch.Captured) gaps.Add("(6) scratch-DWG integrity evidence");
            // (7) contiguous Sequence from 1 and records from every loaded module
            gaps.AddRange(SequenceGaps(log.All).Select(g => "(7) " + g));
            // (8) well-formed records whose authority ids resolve
            gaps.AddRange(Malformed(log.All).Select(g => "(8) " + g));
            if (evidence.ParseErrors.Count > 0) gaps.Add("(8) unparsable log lines: " + evidence.ParseErrors.Count);
            return (gaps.Count == 0, gaps);
        }

        public bool SafetyEvidenceComplete()
        {
            if (evidence.Process is null || !evidence.Process.ProcessGone || evidence.Scratch is null || !evidence.Scratch.Captured) return false;
            if (SequenceGaps(log.All).Any()) return false;
            if (Malformed(log.All.Where(r => r.Sequence >= CleanupStart)).Any()) return false;
            return CleanupSteps.All(step => log.Any(step, r => r.Is("phase", "START")));
        }

        private IEnumerable<string> SequenceGaps(IReadOnlyList<V35LogRecord> all)
        {
            if (all.Count == 0) { yield return "empty log"; yield break; }
            for (int i = 0; i < all.Count; i++)
                if (all[i].Sequence != i + 1) { yield return $"sequence gap at {i + 1} (found {all[i].Sequence})"; yield break; }
            if (all.Select(r => r.Pid).Distinct().Count() != 1) yield return "records from more than one PID";
            if (evidence.Process is not null && all.Any(r => r.Pid != evidence.Process.ProcessId)) yield return "records from a foreign PID";
            if (PayloadLoaded && !all.Any(r => r.ModuleId == "R-PAYLOAD-ARX")) yield return "no record from loaded R-PAYLOAD-ARX";
            if (ManagedRegistered && !all.Any(r => r.ModuleId == "R-MANAGED-OBSERVER")) yield return "no record from R-MANAGED-OBSERVER";
        }

        private IEnumerable<string> Malformed(IEnumerable<V35LogRecord> records)
        {
            foreach (V35LogRecord r in records)
            {
                if (r.Malformed || r.DeliveryId <= 0 || r.ProbeId.Length == 0 || r.CommandIdentity.Length == 0) { yield return $"record {r.Sequence} malformed"; continue; }
                if (!authority.Entries.TryGetValue(r.StageId, out V35Entry? stage) || stage.Kind != "Stage") yield return $"record {r.Sequence} StageId {r.StageId}";
                if (!authority.Entries.ContainsKey(r.EventOrMarkerId)) yield return $"record {r.Sequence} EventOrMarkerId {r.EventOrMarkerId}";
                if (!authority.Entries.ContainsKey(r.DriverOrSchedulerId)) yield return $"record {r.Sequence} DriverOrSchedulerId {r.DriverOrSchedulerId}";
                if (!authority.Entries.TryGetValue(r.ModuleId, out V35Entry? module) || (module.Kind != "RuntimeModule" && module.Kind != "PayloadResource")) yield return $"record {r.Sequence} ModuleId {r.ModuleId}";
            }
        }

        // ---------------------------------------------------------------- markers (MARKER-STAGE-BIND-01)
        private bool Bound(string marker) => plan.MarkerStageBindings.Any(b => b.Marker == marker);

        private IEnumerable<V35LogRecord> MarkerMatches(string marker)
        {
            V35MarkerBinding? binding = plan.MarkerStageBindings.FirstOrDefault(b => b.Marker == marker);
            IEnumerable<V35LogRecord> window = log.RunWindow.Where(r => r.EventOrMarkerId == marker);
            if (binding is null) return window.Where(r => Governed(r.StageId));
            window = window.Where(r => r.StageId == binding.Stage && Governed(r.StageId));
            if (marker == "MARK-LOCK-RELEASE") window = window.Where(r => !r.Is("candidate", "SECOND"));
            if (marker is "N-ED-WILL" or "N-ED-END")
            {
                string? command = binding.Stage switch
                {
                    "STG-SEND-DELIVERY" => "I52CTDA_QUEUED",
                    "STG-FIXTURE-CMD" => "I52CTDA_FIXTURE",
                    "STG-CANCEL-CMD" => "HFV34_CANCEL",
                    "STG-PROBE-CMD" => "I52CTDA_PROBE",
                    _ => null,
                };
                window = command is null
                    ? window.Where(r => r.P("command") is not ("I52CTDA_BOOT" or "I52CTDA_PROBE" or "I52CTDA_FIXTURE" or "I52CTDA_QUEUED" or "I52CTDA_FINISH" or "HFV34_CANCEL"))
                    : window.Where(r => string.Equals(r.P("command"), command, StringComparison.OrdinalIgnoreCase));
            }
            return window;
        }

        // ---------------------------------------------------------------- UNKNOWN predicates
        public bool UnknownCommon(out string reason)
        {
            if (log.Any("BOOT-01", r => !r.Is("status", "OK"))) { reason = "BOOT-01 failed"; return true; }
            if (log.Any("CMD-PROBE", r => r.Is("phase", "RETURN") && r.Is("aborted", "1"))) { reason = "I52CTDA_PROBE aborted: " + log.First("CMD-PROBE", r => r.Is("phase", "RETURN"))!.P("reason"); return true; }
            if (log.Any("UNKNOWN-COMMON")) { reason = "UNKNOWN-COMMON record: " + log.First("UNKNOWN-COMMON")!.Payload.FirstOrDefault().Value; return true; }
            // A native UNKNOWN record whose predicate the row does not list is the generic UNKNOWN-COMMON condition.
            V35LogRecord? unlisted = log.All.FirstOrDefault(r => r.EventOrMarkerId.StartsWith("UNK-", StringComparison.Ordinal) && !plan.UnknownPredicateIds.Contains(r.EventOrMarkerId));
            if (unlisted is not null) { reason = "unlisted " + unlisted.EventOrMarkerId; return true; }
            if (plan.MutationActionId != "MUT-NONE" && !log.Any(plan.MutationActionId, r => r.Is("status", "0"))) { reason = "mutation not performed or not observed"; return true; }
            reason = "";
            return false;
        }

        public bool Unknown(string id, out string why)
        {
            why = "";
            if (log.Any(id)) { why = "record present"; return true; }
            switch (id)
            {
            case "UNK-FINISH-TIMEOUT":
                if (log.Any("CMD-FINISH", r => r.Is("phase", "ENTRY") && r.Is("mode", "FINISH-MODE-DRAIN"))) { why = "FINISH-MODE-DRAIN"; return true; }
                if (!log.Any("CMD-FINISH") && evidence.Process is { TerminatedByControlPlane: true }) { why = "PID terminated before a FINISH record"; return true; }
                return false;
            case "UNK-LOG-BINDING":
                if (Malformed(log.All).Any()) { why = "malformed record"; return true; }
                if (PayloadLoaded && !log.Any("PAYLOAD-DB-BINDING")) { why = "loaded payload did not bind LOG-SEQ-01"; return true; }
                if (ManagedRegistered && !log.Any("RR-MANAGED-CMD", r => r.ModuleId == "R-MANAGED-OBSERVER")) { why = "managed observer did not bind LOG-SEQ-01"; return true; }
                return false;
            case "UNK-LATE-DELIVERY":
                V35LogRecord? late = log.Of("FINISH-FENCE-01").FirstOrDefault(r => r.Is("kind", "LATE-DELIVERY") && !r.Is("removedInstance", "1"));
                if (late is not null) { why = "LATE-DELIVERY in " + late.StageId; return true; }
                return false;
            case "UNK-MARKER-BINDING":
                foreach (V35MarkerBinding b in plan.MarkerStageBindings)
                    if (MarkerMatches(b.Marker).Count() >= 2) { why = $"{b.Marker}@{b.Stage} matched twice"; return true; }
                if (log.Any("MARK-LOCK-RELEASE", r => r.Is("candidate", "SECOND") && plan.MarkerStageBindings.Any(b => b.Marker == "MARK-LOCK-RELEASE" && b.Stage == r.StageId)))
                { why = "second lock-release candidate in the bound window"; return true; }
                return false;
            case "UNK-NONDELIVERY":
                V35LogRecord? finish = log.First("CMD-FINISH", r => r.Is("phase", "ENTRY"));
                if (finish is null || finish.P("missing") != "NONE") { why = "tokens missing at FINISH: " + (finish?.P("missing") ?? "no FINISH"); return true; }
                return false;
            case "UNK-REJECTION":
                V35LogRecord? rejected = log.All.FirstOrDefault(r => r.EventOrMarkerId is "NS-SEND" or "NS-BEGIN-APPCTX" or "NS-BEGIN-CMDCTX" && r.P("status") is { Length: > 0 } s && s != "0");
                if (rejected is not null) { why = rejected.EventOrMarkerId + " status " + rejected.P("status"); return true; }
                return false;
            case "UNK-ILLEGAL-MUTATION":
                if (plan.MutationActionId != "MUT-NONE" && !log.Any(plan.MutationActionId, r => r.Is("status", "0"))) { why = "mutation not observed"; return true; }
                return false;
            case "UNK-RECURSION":
                if (log.All.Any(r => r.EventOrMarkerId.StartsWith("RG-", StringComparison.Ordinal) && r.Is("phase", "RECURSION"))) { why = "guard recursion"; return true; }
                return false;
            case "UNK-MISSING-CALLBACK":
                if (!log.RunWindow.Any(r => r.EventOrMarkerId == plan.PrimaryAuthorityId)) { why = "primary callback not observed"; return true; }
                return false;
            case "UNK-ORDER-INCOMPLETE":
                foreach (V35Marker m in plan.Markers.Where(m => m.MustBePresent && m.Id.StartsWith("N-", StringComparison.Ordinal)))
                    if (!log.RunWindow.Any(r => r.EventOrMarkerId == m.Id)) { why = m.Id + " not captured"; return true; }
                return false;
            case "UNK-LOAD":
                if (!log.Any("MARK-LOAD-RESULT", r => r.Is("rtnorm", "1") && r.Is("loaded", "1"))) { why = "MARK-LOAD-RESULT not RTNORM+loaded"; return true; }
                return false;
            case "UNK-REGISTRATION":
                if (log.Of("MARK-REGISTRATION").Count(r => r.ModuleId == "R-PAYLOAD-ARX") != 1) { why = "MARK-REGISTRATION count"; return true; }
                return false;
            case "UNK-CLEANUP-PROOF":
                if (!log.Any(plan.CleanupActionId, r => r.Is("phase", "END") && r.Is("ok", "1")) || evidence.Process is not { ProcessGone: true }) { why = "cleanup or process-exit proof missing"; return true; }
                return false;
            case "UNK-PAYLOAD-DB":
                if (!log.Any("PAYLOAD-DB-BINDING", r => r.Is("holds", "1"))) { why = "PAYLOAD-DB-BINDING not TRUE"; return true; }
                return false;
            case "UNK-CANCELLATION-MISSING":
                if (Observation("OBS-CANCELLATION") != V35Tri.True) { why = "exact N-ED-CANCEL not observed before the queued delivery"; return true; }
                return false;
            case "UNK-UNDRAINED":
                if (log.Any("CLN-DEFER-DRAIN", r => r.Is("drained", "0"))) { why = "undrained work"; return true; }
                return false;
            default:
                return false;  // the remaining predicates hold only through their native record (checked above)
            }
        }

        // ---------------------------------------------------------------- observation predicates
        public V35Tri Observation(string id)
        {
            switch (id)
            {
            case "OBS-PRIMARY-CALLBACK":
            {
                if (plan.BodyObserverId is null)
                {
                    long armed = log.First(plan.TriggerActionId, r => r.Is("phase", "BEGIN"))?.Sequence ?? long.MaxValue;
                    return log.RunWindow.Any(r => r.EventOrMarkerId == plan.PrimaryAuthorityId && r.Sequence > armed) ? V35Tri.True : V35Tri.Unavailable;
                }
                int n = log.Of("EP-CALLBACK", "STG-PRIMARY-CALLBACK").Count(r => r.P("event") == plan.PrimaryAuthorityId);
                return n == 1 ? V35Tri.True : n == 0 ? V35Tri.Unavailable : V35Tri.False;
            }
            case "OBS-ORIGIN-CALLBACK":
            {
                int n = log.Of("EP-CALLBACK", "STG-ORIGIN").Count(r => r.P("event") == plan.ScheduleOriginEventId);
                return n == 1 ? V35Tri.True : n == 0 ? V35Tri.Unavailable : V35Tri.False;
            }
            case "OBS-ENQUEUE-OK":
            {
                IReadOnlyList<string> steps = authority[plan.SchedulerChainId].List("sequence").Where(s => s.StartsWith("NS-", StringComparison.Ordinal)).ToArray();
                foreach (string step in steps)
                {
                    V35LogRecord? call = log.Of(step).FirstOrDefault(r => !r.P("schedulerUse").EndsWith("INFRA", StringComparison.Ordinal));
                    if (call is null) return V35Tri.Unavailable;
                    if (!call.Is("status", "0")) return V35Tri.False;
                }
                return V35Tri.True;
            }
            case "OBS-DELIVERY":
            {
                foreach ((string entry, long callerReturn) in Deliveries())
                {
                    V35LogRecord? ep = log.First(entry);
                    if (ep is null || callerReturn == long.MaxValue) return V35Tri.Unavailable;
                    if (ep.Sequence <= callerReturn) return V35Tri.False;
                }
                return V35Tri.True;
            }
            case "OBS-CAUSAL-ORDER":
            {
                long trigger = log.First(plan.TriggerActionId, r => r.Is("phase", "BEGIN"))?.Sequence ?? -1;
                long origin = plan.HasScheduleOrigin ? log.Of("EP-CALLBACK", "STG-ORIGIN").FirstOrDefault()?.Sequence ?? -1 : trigger;
                string firstStep = authority[plan.SchedulerChainId].List("sequence").FirstOrDefault(s => s.StartsWith("NS-", StringComparison.Ordinal)) ?? "";
                long enqueue = log.First(firstStep)?.Sequence ?? -1;
                long originReturn = plan.HasScheduleOrigin ? log.First("MARK-ORIGIN-RETURN")?.Sequence ?? -1 : log.First("MARK-APPCTX-RETURN")?.Sequence ?? -1;
                long delivery = Deliveries().Select(d => log.First(d.Entry)?.Sequence ?? -1).DefaultIfEmpty(-1).Max();
                long mutation = log.First(plan.MutationActionId)?.Sequence ?? -1;
                long token = plan.CompletionTokenIds.Where(TokenAccepted).Select(t => log.First(t, r => r.Is("status", "ACCEPTED"))!.Sequence).DefaultIfEmpty(-1).Max();
                long[] chain = [trigger, origin, enqueue, originReturn, delivery, mutation, token];
                if (chain.Any(s => s < 0)) return V35Tri.Unavailable;
                for (int i = 1; i < chain.Length; i++) if (chain[i] < chain[i - 1]) return V35Tri.False;
                return V35Tri.True;
            }
            case "OBS-ORDER-RULES":
                return OrderViolation() is null ? V35Tri.True : V35Tri.False;
            case "OBS-ORDER-COMPLETE":
            {
                string[] required = plan.Markers.Where(m => m.MustBePresent).Select(m => m.Id).Append(plan.PrimaryAuthorityId).Where(x => x.StartsWith("N-TR-", StringComparison.Ordinal)).ToArray();
                return required.All(e => log.RunWindow.Any(r => r.EventOrMarkerId == e && r.P("numTransactions").Length > 0)) ? V35Tri.True : V35Tri.Unavailable;
            }
            case "OBS-ORDER-INVARIANT-09N-O":
                return log.RunWindow.Any(r => r.EventOrMarkerId == "N-TR-OUTERMOST-END-CALLED" && r.Is("registration", "RR-TX")) ? V35Tri.True : V35Tri.Unavailable;
            case "OBS-T-AFFILIATION":
                return Affiliation();
            case "OBS-COUNT":
            {
                V35LogRecord? evidenceAtCallback = log.First("VER-T", r => r.Is("phase", "CALLBACK"));
                if (evidenceAtCallback is null) return V35Tri.Unavailable;
                int count = Int(evidenceAtCallback.P("activeTransactions"));
                if (count == int.MinValue) return V35Tri.Unavailable;
                bool expected = plan.ExpectedAfter.FirstOrDefault(s => s.StartsWith("STATE-T-", StringComparison.Ordinal)) switch
                {
                    "STATE-T-NESTED-PREEND" => count > 1,
                    "STATE-T-OUTERMOST-PREEND" => count == 1,
                    "STATE-T-OUTERMOST-END-CALLED" => count == 1,
                    "STATE-T-ENDED" => count == 0,
                    _ => false,
                };
                return expected ? V35Tri.True : V35Tri.False;
            }
            case "OBS-VISIBILITY":
            {
                V35LogRecord? atCallback = log.First("VER-T", r => r.Is("phase", "CALLBACK"));
                if (atCallback is null || atCallback.P("readThroughTop") != "0") return V35Tri.Unavailable;
                bool bytes = atCallback.P("bytes") == S0;
                bool nested = !plan.ExpectedAfter.Contains("STATE-T-NESTED-PREEND") || Int(atCallback.P("activeTransactions")) > 1;
                return bytes && nested ? V35Tri.True : V35Tri.False;
            }
            case "OBS-REREAD":
                return log.Any("VER-T", r => r.Is("phase", "FRESH") && r.P("reread").Length > 0) ? V35Tri.True : V35Tri.Unavailable;
            case "OBS-GUARD-RESET":
            {
                var armed = log.All.Where(r => r.EventOrMarkerId.StartsWith("RG-", StringComparison.Ordinal) && r.Is("phase", "ARM")).Select(r => r.EventOrMarkerId + "|" + r.P("family")).ToList();
                var disarmed = log.All.Where(r => r.EventOrMarkerId.StartsWith("RG-", StringComparison.Ordinal) && r.Is("phase", "DISARM")).Select(r => r.EventOrMarkerId + "|" + r.P("family")).ToList();
                foreach (string g in armed) { if (!disarmed.Remove(g)) return V35Tri.False; }
                return V35Tri.True;
            }
            case "OBS-LOCK-MODES":
            {
                var locks = log.RunWindow.Where(r => r.EventOrMarkerId is "N-DOC-LOCK-WILL" or "N-DOC-LOCK-CHANGED" && r.Is("scratch", "1")).ToArray();
                return locks.Length > 0 && locks.All(r => r.P("myCurrent").Length > 0 && r.P("current").Length > 0) ? V35Tri.True : V35Tri.Unavailable;
            }
            case "OBS-VETO-OK":
            {
                V35LogRecord? veto = log.First("SA-VETO");
                V35LogRecord? lockCall = log.First("SA-LOCK", r => r.Is("authority", "DRIVER-LOCK-01"));
                bool vetoed = log.RunWindow.Any(r => r.EventOrMarkerId == "N-DOC-LOCK-VETO" && r.Is("requestId", "RACKCAD_CTDA_V35#1"));
                if (veto is null || lockCall is null) return V35Tri.Unavailable;
                return veto.Is("status", "0") && vetoed && !lockCall.Is("status", "0") ? V35Tri.True : V35Tri.False;
            }
            case "OBS-ERASE-FLAG":
            {
                V35LogRecord? erased = log.RunWindow.FirstOrDefault(r => r.EventOrMarkerId == "N-OBJ-ERASE" && r.Is("registration", "OR-REF-B") && r.Is("erasing", "1"));
                if (erased is null) return V35Tri.Unavailable;
                return log.RunWindow.Any(r => r.EventOrMarkerId == "N-DB-ERASE" && r.P("object") == erased.P("object")) ? V35Tri.True : V35Tri.Unavailable;
            }
            case "OBS-CONTEXT":
                return ContextObservation();
            case "OBS-SYNC-ENTRY-RETURN":
            {
                V35LogRecord? entry = log.First("MARK-APPCTX-ENTRY", r => r.StageId == "STG-SYNC-APPCTX");
                V35LogRecord? ret = log.First("MARK-APPCTX-RETURN", r => r.StageId == "STG-SYNC-APPCTX");
                if (entry is null || ret is null) return V35Tri.Unavailable;
                return entry.Sequence < ret.Sequence && entry.DeliveryId == ret.DeliveryId ? V35Tri.True : V35Tri.False;
            }
            case "OBS-CANCELLATION":
            {
                V35LogRecord? cancelled = log.RunWindow.FirstOrDefault(r => r.EventOrMarkerId == "N-ED-CANCEL" && string.Equals(r.P("command"), "HFV34_CANCEL", StringComparison.OrdinalIgnoreCase));
                V35LogRecord? delivery = log.First("CANCEL-CMDCTX-01", r => r.Is("phase", "COMPLETION"));
                if (cancelled is null || delivery is null) return V35Tri.Unavailable;
                return cancelled.Sequence < delivery.Sequence ? V35Tri.True : V35Tri.False;
            }
            case "OBS-LOAD":
                return log.Any("MARK-LOAD-RESULT", r => r.Is("rtnorm", "1") && r.Is("loaded", "1")) && log.RunWindow.Any(r => r.EventOrMarkerId == "N-RX-LOADED" && r.Is("exactModule", "1"))
                    ? V35Tri.True : V35Tri.Unavailable;
            case "OBS-REGISTRATION":
            {
                var registrations = log.Of("MARK-REGISTRATION").Where(r => r.ModuleId == "R-PAYLOAD-ARX").ToArray();
                V35LogRecord? body = log.Of("EP-CALLBACK", "STG-ORIGIN").FirstOrDefault(r => r.ModuleId == "R-PAYLOAD-ARX");
                if (registrations.Length != 1 || body is null) return V35Tri.Unavailable;
                return registrations[0].P("instance") == body.P("instance") ? V35Tri.True : V35Tri.False;
            }
            case "OBS-MARKERS":
                foreach (V35Marker m in plan.Markers)
                {
                    int n = MarkerMatches(m.Id).Count();
                    if (m.MustBePresent && n == 0) return V35Tri.Unavailable;
                    if (m.MustBePresent && Bound(m.Id) && n != 1) return V35Tri.Unavailable;
                    if (!m.MustBePresent && n != 0) return V35Tri.False;
                }
                return V35Tri.True;
            case "OBS-OUTCOME-COMMIT":
                return log.Any("SET-OUTCOME-COMMIT", r => r.Is("status", "0")) ? V35Tri.True : V35Tri.Unavailable;
            case "OBS-CANDIDATE-BOUNDARY":
                return CandidateBoundary() switch { "ACCEPTED" => V35Tri.True, "CONTRADICTION" => V35Tri.False, _ => V35Tri.Unavailable };
            case "OBS-NOTIFIER-WRITE":
            {
                V35LogRecord? read = log.First("F-TRIGGER-XR", r => r.Is("phase", "FRESH"));
                if (read is null) return V35Tri.Unavailable;
                return read.P("bytes") == "HFV35:TRIGGER-XR:1" && log.Any(plan.MutationActionId, r => r.Is("status", "0")) ? V35Tri.True : V35Tri.False;
            }
            case "OBS-EWASNOTIFYING":
                return log.Any("OBS-EWASNOTIFYING") ? V35Tri.True : V35Tri.Unavailable;
            case "OBS-STAGED":
                return log.Any("SET-STAGE-S-IN-PRIMARY", r => r.Is("staged", "1") && r.P("readBack") == S1) ? V35Tri.True : V35Tri.Unavailable;
            case "OBS-EXEC-COMPLETE":
            {
                bool exec = plan.ExecutionContextId == "EXEC-OBSERVE" || log.Any(plan.ExecutionContextId, r => r.Is("phase", "EXIT") && r.Is("ok", "1"));
                bool tokens = plan.CompletionTokenIds.All(TokenAccepted);
                if (!tokens) return V35Tri.Unavailable;
                return exec ? V35Tri.True : log.Any(plan.ExecutionContextId, r => r.Is("phase", "EXIT")) ? V35Tri.False : V35Tri.Unavailable;
            }
            case "OBS-LOCK-RELEASE-BOUND":
            {
                V35MarkerBinding? binding = plan.MarkerStageBindings.FirstOrDefault(b => b.Marker == "MARK-LOCK-RELEASE");
                if (binding is null) return V35Tri.Unavailable;
                int n = MarkerMatches("MARK-LOCK-RELEASE").Count();
                bool second = log.Any("MARK-LOCK-RELEASE", r => r.Is("candidate", "SECOND") && r.StageId == binding.Stage);
                return n == 1 && !second ? V35Tri.True : V35Tri.Unavailable;
            }
            case "OBS-PAYLOAD-DB-BINDING":
                return log.Any("PAYLOAD-DB-BINDING", r => r.StageId == "STG-PAYLOAD-INIT" && r.Is("holds", "1")) ? V35Tri.True : V35Tri.Unavailable;
            case "OBS-CANCEL-RESTORED":
            {
                V35LogRecord? read = log.First("OBS-CANCEL-RESTORED");
                if (read is null || !read.Is("read", "0")) return V35Tri.Unavailable;
                return read.P("bytes") == S0 ? V35Tri.True : V35Tri.False;
            }
            default:
                return V35Tri.Unavailable;
            }
        }

        // Delivery entry points and the sequence of the scheduling caller's return, per chain step.
        private IEnumerable<(string Entry, long CallerReturn)> Deliveries()
        {
            long originReturn = log.First("MARK-ORIGIN-RETURN")?.Sequence ?? long.MaxValue;
            switch (plan.SchedulerChainId)
            {
            case "CHAIN-SEND": yield return ("EP-COMMAND", originReturn); break;
            case "CHAIN-APPCTX": yield return ("EP-APPCTX", originReturn); break;
            case "CHAIN-APPCTX-CMDCTX":
                yield return ("EP-APPCTX", originReturn);
                yield return ("EP-CMDCTX", log.First("MARK-APPCTX-CALLBACK-RETURN")?.Sequence ?? long.MaxValue);
                break;
            case "CHAIN-SYNC-CMDCTX": yield return ("EP-CMDCTX", log.First("MARK-APPCTX-RETURN")?.Sequence ?? long.MaxValue); break;
            }
        }

        private string? OrderViolation()
        {
            // A pair exists only when both halves of the same identity are present (editor: command; document: lock
            // request; transaction: manager and subject depth; dynamic linker: module).
            static string Identity(V35LogRecord r) => r.EventOrMarkerId[..r.EventOrMarkerId.IndexOf('-', 2)] switch
            {
                "N-ED" => "command:" + r.P("command").ToUpperInvariant(),
                "N-DOC" => "request:" + r.P("requestId"),
                "N-TR" => "tx:" + r.P("manager") + "/" + r.P("subjectDepth"),
                "N-RX" => "module:" + r.P("module"),
                _ => "",
            };
            foreach (string pair in OrderPairs)
            {
                string[] p = pair.Split('>');
                var wills = log.RunWindow.Where(r => r.EventOrMarkerId == p[0]).GroupBy(Identity).ToDictionary(g => g.Key, g => g.Min(r => r.Sequence));
                foreach (var did in log.RunWindow.Where(r => r.EventOrMarkerId == p[1]).GroupBy(Identity))
                    if (wills.TryGetValue(did.Key, out long will) && did.Min(r => r.Sequence) < will) return pair + " " + did.Key;
            }
            return null;
        }

        // OBS-T-AFFILIATION: the mutation is logged inside the row's execution transaction and that transaction's
        // end/abort result is recorded before the fresh verifier.
        private V35Tri Affiliation()
        {
            V35LogRecord? mutation = log.First(plan.MutationActionId, r => r.Is("status", "0"));
            if (mutation is null) return V35Tri.Unavailable;
            string owner = plan.ExecutionTransactionId;
            long verifier = log.All.FirstOrDefault(r => r.EventOrMarkerId.StartsWith("VER-", StringComparison.Ordinal) && r.Is("phase", "FRESH"))?.Sequence ?? long.MaxValue;
            if (owner == "T-PRIMARY")
            {
                V35LogRecord? start = log.First("SA-TX-START", r => r.Is("owner", "T-PRIMARY"));
                V35LogRecord? end = log.First("SET-OUTCOME-COMMIT", r => r.P("status").Length > 0);
                if (start is null || end is null) return V35Tri.Unavailable;
                return mutation.P("transaction") == start.P("transaction") && end.Sequence < verifier ? V35Tri.True : V35Tri.False;
            }
            V35LogRecord? owned = log.All.LastOrDefault(r => r.EventOrMarkerId == "SA-TX-START" && r.Is("owner", owner) && r.Sequence < mutation.Sequence);
            V35LogRecord? close = log.All.FirstOrDefault(r => r.EventOrMarkerId is "SA-TX-END" or "SA-TX-ABORT" && r.Is("owner", owner) && r.Sequence > mutation.Sequence);
            if (owned is null || close is null) return V35Tri.Unavailable;
            return mutation.P("transaction") == owned.P("transaction") && close.Sequence < verifier ? V35Tri.True : V35Tri.False;
        }

        private V35Tri ContextObservation()
        {
            long last = -1;
            foreach (string ctx in plan.ThreadContextIds)
            {
                V35LogRecord? r = ctx switch
                {
                    "CTX-CALLBACK" => log.First("CTX-CALLBACK"),
                    "CTX-COMMAND" => log.First("CTX-COMMAND"),
                    "CTX-APPLICATION" => log.First("CTX-APPLICATION"),
                    _ => null,
                };
                if (r is null) return V35Tri.Unavailable;
                if (ctx == "CTX-APPLICATION" && !r.Is("isApplicationContext", "1")) return V35Tri.False;
                if (ctx == "CTX-COMMAND" && !r.Is("isApplicationContext", "0")) return V35Tri.False;
                if (r.Sequence < last) return V35Tri.False;
                last = r.Sequence;
            }
            return V35Tri.True;
        }

        // OBS-CANDIDATE-BOUNDARY (Architect B12).
        private string CandidateBoundary()
        {
            V35LogRecord? callback = log.Of("EP-CALLBACK", "STG-PRIMARY-CALLBACK").FirstOrDefault(r => r.P("event") == "N-ED-END");
            V35LogRecord? release = MarkerMatches("MARK-LOCK-RELEASE").FirstOrDefault();
            V35LogRecord? lockObserved = log.First("CB-LOCK-01");
            V35LogRecord? start = log.First("SA-TX-START", r => r.Is("owner", "T_CB"));
            V35LogRecord? end = log.First("SA-TX-END", r => r.Is("owner", "T_CB"));
            V35LogRecord? exit = log.First("CB-EXEC-01", r => r.Is("phase", "EXIT"));
            bool i = callback is not null && release is not null && callback.Sequence < release.Sequence;
            bool ii = lockObserved is not null && lockObserved.Is("writeCapable", "1");
            bool iii = start is not null && start.P("transaction") is { Length: > 0 } t && t != "0x0" && end is not null && end.Is("status", "0") && exit is not null && end.Sequence < exit.Sequence;
            if (!(i && ii && iii)) return "UNAVAILABLE";
            return VerifierComparison() == "EQUAL" ? "ACCEPTED" : "CONTRADICTION";
        }

        // ---------------------------------------------------------------- verifier comparison vs ExpectedAfter
        public string VerifierComparison()
        {
            var results = plan.ExpectedAfter.Select(CompareState).ToArray();
            if (results.Contains("UNAVAILABLE")) return "UNAVAILABLE";
            return results.All(r => r == "EQUAL") ? "EQUAL" : "DIFFERENT";
        }

        private string CompareState(string state)
        {
            V35LogRecord? FreshRecord(string verifier) =>
                log.All.FirstOrDefault(r => (r.EventOrMarkerId == verifier || r.EventOrMarkerId == "VER-ALL" && r.Is("part", verifier)) && r.Is("phase", "FRESH"));
            string Semantic(string expected) => FreshRecord("VER-S") is { } s ? s.P("bytes") == expected ? "EQUAL" : "DIFFERENT"
                : FreshRecord("VER-T") is { } t && t.P("reread").Length > 0 ? t.P("reread") == expected ? "EQUAL" : "DIFFERENT" : "UNAVAILABLE";
            switch (state)
            {
            case "STATE-S-0": case "STATE-T-ABORT": return Semantic(S0);
            case "STATE-S-1": return Semantic(S1);
            case "STATE-M-0": return Material(10, 20, "RACKCAD_CTDA_V30_A");
            case "STATE-M-1": return Material(110, 220, "RACKCAD_CTDA_V30_B");
            case "STATE-SM-1":
            {
                SmFacts? facts = Sm();
                if (facts is null) return "UNAVAILABLE";
                string c = SmRules.ClassifyAfter(facts);
                return c == SmRules.Ok ? "EQUAL" : c.StartsWith("FAIL-SM", StringComparison.Ordinal) ? "DIFFERENT" : "UNAVAILABLE";
            }
            case "STATE-SM-0":
            {
                SmFacts? facts = Sm();
                if (facts is null || SmRules.LinkStructure(facts.Link) is not null || !facts.AnchorALive || facts.BRead) return "UNAVAILABLE";
                if (facts.Link.Value == SmBindingContract.StateSm0 && facts.ForeignReferenceInserts == 0 && !facts.CBound) return "EQUAL";
                return "DIFFERENT";
            }
            case "STATE-ALL-1":
            {
                string[] parts = [Semantic(S1), Material(110, 220, "RACKCAD_CTDA_V30_B"), CompareState("STATE-SM-1")];
                return parts.Contains("UNAVAILABLE") ? "UNAVAILABLE" : parts.All(p => p == "EQUAL") ? "EQUAL" : "DIFFERENT";
            }
            case "STATE-T-NESTED-PREEND": case "STATE-T-OUTERMOST-PREEND": case "STATE-T-OUTERMOST-END-CALLED":
            {
                // STATE-T part: callback-time VER-T event evidence; STATE-S part: fresh VER-S at FINISH equals STATE-S-0.
                V35Tri count = Observation("OBS-COUNT");
                if (count == V35Tri.Unavailable) return "UNAVAILABLE";
                string semantic = Semantic(S0);
                if (semantic == "UNAVAILABLE") return "UNAVAILABLE";
                return count == V35Tri.True && semantic == "EQUAL" ? "EQUAL" : "DIFFERENT";
            }
            case "STATE-T-ENDED":
            {
                V35LogRecord? fresh = FreshRecord("VER-T");
                if (fresh is null || fresh.P("reread").Length == 0) return "UNAVAILABLE";
                return fresh.Is("activeTransactions", "0") ? "EQUAL" : "DIFFERENT";
            }
            default: return "UNAVAILABLE";
            }

            string Material(double x, double y, string layer)
            {
                V35LogRecord? m = FreshRecord("VER-M");
                if (m is null || !m.Is("live", "1")) return "UNAVAILABLE";
                return Math.Abs(Dbl(m.P("x")) - x) < 1e-9 && Math.Abs(Dbl(m.P("y")) - y) < 1e-9 && Math.Abs(Dbl(m.P("z"))) < 1e-9 && m.P("layer") == layer ? "EQUAL" : "DIFFERENT";
            }
        }

        private SmFacts? Sm()
        {
            V35LogRecord? r = log.All.FirstOrDefault(x => (x.EventOrMarkerId == "VER-SM" || x.EventOrMarkerId == "VER-ALL" && x.Is("part", "VER-SM")) && x.Is("phase", "FRESH"));
            if (r is null) return null;
            bool B(string k) => r.P(k) == "1";
            var link = new SmLinkFacts(B("keyPresent"), B("identityMatches"), B("erased"), B("isXrecord"), Int(r.P("resbufCount")), B("isText"), Int(r.P("carriersFound")), r.P("value"));
            return new SmFacts(link, B("anchorALive"), B("bRead"), B("bLive"), Int(r.P("foreignReferenceInserts")), B("cBound"), B("cFound"), B("cLive"), B("cAtCreationPosition"), "", "");
        }

        // ---------------------------------------------------------------- FAIL predicates (complete evidence only)
        public bool Fail(string id, IReadOnlyDictionary<string, V35Tri> observations, string comparison)
        {
            switch (id)
            {
            case "FP-STATE": return comparison == "DIFFERENT";
            case "FP-ORDER": return OrderViolation() is not null;
            case "FP-AFFILIATION": return Affiliation() == V35Tri.True && comparison == "DIFFERENT" && ExecutionTransactionEndedOk();
            case "FP-COUNT": return Observation("OBS-COUNT") == V35Tri.False;
            case "FP-VISIBILITY": return Observation("OBS-VISIBILITY") == V35Tri.False;
            case "FP-INVARIANT-09N-O":
                return log.RunWindow.Any(r => r.EventOrMarkerId == "N-TR-OUTERMOST-END-CALLED" && r.Is("registration", "RR-TX") && r.P("subjectDepth") is { Length: > 0 } d && d != "1");
            case "FP-BOUNDARY": return CandidateBoundary() == "CONTRADICTION";
            case "FP-CAUSAL":
                foreach ((string entry, long callerReturn) in Deliveries())
                {
                    V35LogRecord? ep = log.First(entry);
                    if (ep is not null && callerReturn != long.MaxValue && ep.Sequence < callerReturn) return true;
                    V35LogRecord? mutation = log.First(plan.MutationActionId);
                    if (ep is not null && mutation is not null && mutation.Sequence < ep.Sequence) return true;
                }
                return false;
            case "FP-CONTEXT": return ContextObservation() == V35Tri.False;
            case "FP-SYNC":
            {
                V35LogRecord? entry = log.First("MARK-APPCTX-ENTRY", r => r.StageId == "STG-SYNC-APPCTX");
                V35LogRecord? ret = log.First("MARK-APPCTX-RETURN", r => r.StageId == "STG-SYNC-APPCTX");
                return ret is not null && (entry is null || entry.Sequence > ret.Sequence);
            }
            case "FP-NOTIFIER-WRITE": return Observation("OBS-NOTIFIER-WRITE") == V35Tri.False;
            case "FP-VETO-BYPASS":
                return log.Any("SA-VETO", r => r.Is("status", "0")) && log.Any("SA-LOCK", r => r.Is("authority", "DRIVER-LOCK-01") && r.Is("status", "0"));
            default: return false;
            }
        }

        private bool ExecutionTransactionEndedOk() => plan.ExecutionTransactionId == "T-PRIMARY"
            ? log.Any("SET-OUTCOME-COMMIT", r => r.Is("status", "0"))
            : log.Any("SA-TX-END", r => r.Is("owner", plan.ExecutionTransactionId) && r.Is("status", "0"));

        // ---------------------------------------------------------------- FP-CLEANUP-SAFETY (V34 section 5)
        public bool CleanupSafetyViolated(out string reason)
        {
            long start = CleanupStart;
            reason = "";
            if (start == long.MaxValue) return false;
            var retainedInstances = log.Of("CLN-OBJ-RETAIN-FENCED").Select(r => r.P("instance")).Where(i => i.Length > 0).ToHashSet();
            long payloadFence = log.First("RR-PAYLOAD-DB", r => r.Is("phase", "REMOVE") && r.Is("status", "0"))?.Sequence ?? long.MaxValue;
            long managedFence = log.First("RR-MANAGED-CMD", r => r.Is("phase", "UNSUBSCRIBED"))?.Sequence ?? long.MaxValue;
            foreach (V35LogRecord r in log.All.Where(x => x.Sequence >= start))
            {
                if (r.EventOrMarkerId == "FINISH-FENCE-01" && r.Is("kind", "LATE-DELIVERY") && r.Is("removedInstance", "1")) { reason = $"late delivery to a removed instance at {r.Sequence}"; return true; }
                if (r.EventOrMarkerId == "FINISH-FENCE-01" && r.Is("kind", "LATE-DELIVERY") && (r.ModuleId == "R-MANAGED-OBSERVER" && r.Sequence > managedFence || r.ModuleId == "R-PAYLOAD-ARX" && r.Sequence > payloadFence))
                { reason = $"late delivery to a module past its fence point at {r.Sequence}"; return true; }
                if (!Governed(r.StageId)) continue;
                if (r.EventOrMarkerId == "EP-CALLBACK" || r.EventOrMarkerId.StartsWith("MUT-", StringComparison.Ordinal) && r.EventOrMarkerId != "MUT-NONE" || r.P("phase") == "CONSUME")
                { reason = $"governed body or write after cleanup start at {r.Sequence}"; return true; }
                if (retainedInstances.Contains(r.P("instance")) && !r.Is("notifierAccess", "NONE")) { reason = $"retained reactor touched its notifier at {r.Sequence}"; return true; }
                if (r.ModuleId == "R-PAYLOAD-ARX" && r.Sequence > payloadFence && !r.Is("kind", "LATE-DELIVERY")) { reason = $"payload record after its fence point at {r.Sequence}"; return true; }
                if (r.ModuleId == "R-MANAGED-OBSERVER" && r.Sequence > managedFence && !r.Is("kind", "LATE-DELIVERY")) { reason = $"managed record after its fence point at {r.Sequence}"; return true; }
            }
            return false;
        }

        public bool CleanupAndFenceComplete(out string reason)
        {
            reason = "";
            if (!log.Any(plan.CleanupActionId, r => r.Is("phase", "END") && r.Is("ok", "1"))) { reason = plan.CleanupActionId + " not ok"; return false; }
            if (!log.Any("CLN-BASE", r => r.Is("step", "VERIFY") && r.Is("ok", "1"))) { reason = "CLN-BASE verification"; return false; }
            if (plan.CompletionFence == "FENCE-PROCESS-EXIT" && evidence.Process is not { ProcessGone: true, TerminatedByControlPlane: false, ExitedWithinPostFinishDeadline: true })
            { reason = "exact PID exit not verified"; return false; }
            if (evidence.Scratch is not { Captured: true, Unchanged: true, BackupCreated: false }) { reason = "scratch DWG integrity"; return false; }
            return true;
        }
    }
}
