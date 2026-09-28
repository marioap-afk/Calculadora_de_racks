using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using I52Ctda.ControlPlane;
using I52Ctda.ControlPlane.V35;
using I52Ctda.ManagedObserver;

// R3 (frozen V35) tests: freeze guard, plan authority, conformance, RESULT-RULE-V35 on synthetic logs, managed observer
// logic and the R3 smoke evaluator. No AutoCAD, no ProbeId execution.
internal static class R3Tests
{
    public static (string Name, Action<string> Run)[] All =>
    [
        ("r3 freeze package hash holds", FreezeHolds),
        ("r3 freeze guard detects normative drift", FreezeDetectsDrift),
        ("r3 plans 100/100 with approved row hashes", PlansResolve),
        ("r3 generated native authority has no drift", GeneratedNoDrift),
        ("r3 static conformance", ConformanceHolds),
        ("r3 unknown ProbeId rejected before launch", UnknownProbeRejected),
        ("r3 driver script never finishes or quits", DriverScripts),
        ("r3 derived plan shape", DerivedPlanShape),
        ("r3 result PASS on complete evidence", ResultPass),
        ("r3 result UNKNOWN on missing token", ResultMissingToken),
        ("r3 result FAIL on verifier contradiction", ResultStateFail),
        ("r3 result FAIL-first on cleanup safety", ResultCleanupSafety),
        ("r3 result UNKNOWN on late delivery", ResultLateDelivery),
        ("r3 result UNKNOWN in drain mode", ResultDrain),
        ("r3 result UNKNOWN on sequence gap or foreign PID", ResultSequence),
        ("r3 result UNKNOWN through UNKNOWN-COMMON for unlisted host unknown", ResultStructuralPath),
        ("r3 result requires process fence for FENCE-PROCESS-EXIT", ResultProcessFence),
        ("r3 order rules pair by identity (no false FP-ORDER)", OrderByIdentity),
        ("r3 INFRA records never satisfy governed markers", InfraMarkers),
        ("r3 late delivery past a module fence is a safety FAIL", ModuleFenceLate),
        ("r3 managed observer fence and marker", ManagedObserverLogic),
        ("r3 managed record layout matches the C ABI", ManagedRecordLayout),
        ("r3 smoke evaluator", SmokeEvaluator),
        ("r3 smoke FIN-GATE initializes without FINISH, tokens or classification", SmokeFinishGate),
        ("r3 smoke managed observer reads the fence", SmokeManagedFenceRead),
        ("r3 D-1 N-TR-ENDED subject depth matches the qualified host", TransactionEndedDepth),
        ("r3 D-2 corrupted or unknown CommandIdentity is never PASS", CorruptedCommandIdentity),
        ("r3 D-2 host canary token records keep I52CTDA_PROBE", HostCanaryTokenIdentity),
        ("r3 D-2 no record field points into a by-value temporary", NoTemporaryRecordPointers),
        ("r3 D-2 smoke rejects an unknown CommandIdentity", SmokeCommandIdentity),
        ("r3 D-3 automated discard exit is required evidence", AutomatedDiscardExit),
        ("r3 D-3 native exit needs no human input and cannot save", NativeExitNeedsNoHuman),
        ("r3 D-3 interactive state is terminated and stays UNKNOWN", InteractiveStateFailsClosed),
        ("r3 D-3 smoke leaves through the automated exit", SmokeAutomatedExit),
        ("r3 harness log read retries a transient lock", LogReadTransientLock),
        ("r3 harness permanent log lock fails closed", LogReadPermanentLockFailsClosed),
        ("r3 harness process evidence survives a log failure", ProcessEvidenceSurvivesLogFailure),
        ("r3 harness interactive-state evidence is preserved", InteractiveEvidencePreserved),
    ];

    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
    private static V35Authority Authority(string repo) => V35Authority.Load(repo);

    private static void FreezeHolds(string repo)
    {
        V35FreezeVerification v = V35Freeze.Verify(repo);
        Require(v.Holds && v.ComputedPackageHash == "43DCE809AA5B124E73B67E2B8B76EB78DC921FCE0BE961B2906BC21BE9D3B6DF" && v.Blobs == 24, "freeze: " + string.Join(";", v.Mismatches));
    }

    private static void FreezeDetectsDrift(string repo)
    {
        string temp = Path.Combine(Path.GetTempPath(), "i52-r3-freeze-" + Guid.NewGuid().ToString("N"));
        try
        {
            JsonObject manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(repo, V35Freeze.ManifestPath)))!.AsObject();
            var files = manifest["package"]!["blobs"]!.AsObject().SelectMany(g => g.Value!.AsObject().Select(p => p.Key)).Append(V35Freeze.ManifestPath);
            foreach (string f in files) { string dst = Path.Combine(temp, f); Directory.CreateDirectory(Path.GetDirectoryName(dst)!); File.Copy(Path.Combine(repo, f), dst); }
            Require(V35Freeze.Verify(temp).Holds, "copy should verify");
            string catalog = Path.Combine(temp, V35Authority.CatalogPath);
            File.WriteAllText(catalog, File.ReadAllText(catalog).Replace("HFV35:TRIGGER-XR:0", "HFV35:TRIGGER-XR:9", StringComparison.Ordinal));
            V35FreezeVerification drifted = V35Freeze.Verify(temp);
            Require(!drifted.Holds && drifted.Mismatches.Any(m => m.Contains("execution-catalog-v35.json", StringComparison.Ordinal)), "catalog drift not detected");
            bool stopped = false;
            try { V35Authority.Load(temp); } catch (InvalidDataException e) when (e.Message.Contains("STOP", StringComparison.Ordinal)) { stopped = true; }
            Require(stopped, "authority loaded from a drifted package");
        }
        finally { if (Directory.Exists(temp)) Directory.Delete(temp, true); }
    }

    private static void PlansResolve(string repo)
    {
        V35Authority a = Authority(repo);
        Require(a.Rows.Count == 100 && a.RowSchema.Count == 36 && a.ByProbe.Count == 100, "plan count");
        Require(V35PlanCompiler.Violations(a).Count == 0, "violations");
        JsonObject rows = JsonNode.Parse(File.ReadAllText(Path.Combine(repo, V35Authority.OraclePath)))!["Z_approvalFreeze"]!["rows"]!.AsObject();
        Require(a.Rows.All(r => rows[r.ProbeId]!.GetValue<string>() == r.RowApprovalHash), "row approval hashes");
    }

    private static void GeneratedNoDrift(string repo)
    {
        V35Authority a = Authority(repo);
        IReadOnlyList<string> drift = V35PlanCompiler.Drift(repo, V35PlanCompiler.Generate(a, repo));
        Require(drift.Count == 0, "drift: " + string.Join(";", drift));
        JsonObject manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(repo, V35PlanCompiler.PlanManifestPath)))!.AsObject();
        Require(manifest["freezePackageHash"]!.GetValue<string>() == V35Freeze.PackageHash && manifest["planCount"]!.GetValue<int>() == 100, "plan manifest binding");
    }

    private static void ConformanceHolds(string repo)
    {
        V35ConformanceReport r = V35Conformance.Check(repo, Authority(repo));
        Require(r.Holds, "conformance: " + string.Join("; ", r.Violations.Concat(r.GeneratedDrift)));
    }

    private static void UnknownProbeRejected(string repo)
    {
        V35Authority a = Authority(repo);
        foreach (string bad in new[] { "99X", "02n", "02N ", "", "C15N16N-SM;02N" })
        {
            bool rejected = false;
            try { V35ProbeLaunch.Create(a, "acad.exe", "missing.arx", "missing.dwg", "out", bad, null); }
            catch (ArgumentException) { rejected = true; }
            Require(rejected, $"ProbeId '{bad}' not rejected before launch");
        }
    }

    private static void DriverScripts(string repo)
    {
        V35Authority a = Authority(repo);
        int managed = 0, fixture = 0, cancel = 0;
        foreach (V35RowPlan r in a.Rows)
        {
            bool loads = r.ObserverRegistrationIds.Contains("RR-MANAGED-CMD");
            string script = V35ProbeLaunch.DriverScript(r, @"C:\run", loads);
            Require(!script.Contains("FINISH", StringComparison.Ordinal) && !script.Contains("QUIT", StringComparison.Ordinal), r.ProbeId + " script finishes");
            Require(script.Contains("I52CTDA_BOOT\nI52CTDA_PROBE " + r.ProbeId + "\n", StringComparison.Ordinal), r.ProbeId + " boot/probe order");
            if (loads) managed++;
            if (script.Contains("I52CTDA_FIXTURE", StringComparison.Ordinal)) { fixture++; Require(r.TriggerActionId == "TRG-RUN-FIXTURE-CMD", r.ProbeId + " fixture"); }
            if (script.Contains("HFV34_CANCEL", StringComparison.Ordinal)) { cancel++; Require(r.TriggerActionId == "TRG-CANCEL-CMDCTX", r.ProbeId + " cancel"); }
        }
        Require(managed == 4 && fixture == 9 && cancel == 3, $"managed {managed} fixture {fixture} cancel {cancel}");
    }

    private static void DerivedPlanShape(string repo)
    {
        V35Authority a = Authority(repo);
        foreach (V35RowPlan r in a.Rows)
        {
            IReadOnlyList<string> p = V35PlanCompiler.DerivedPlan(a, r);
            Require(p[0] == "RUN-ENV-01" && p[^1] == "CONTROL-PLANE-RESULT-01:RESULT-RULE-V35", r.ProbeId + " ends");
            int trigger = p.ToList().IndexOf("TRIGGER:" + r.TriggerActionId), finish = p.ToList().IndexOf("I52CTDA_FINISH");
            Require(trigger > 0 && finish > trigger && p.Count(x => x == "CLEANUP:CLN-BASE") == 1, r.ProbeId + " shape");
        }
    }

    // ------------------------------------------------------------------ synthetic LOG-RECORD-01 logs
    private sealed class Log
    {
        private readonly List<(string Event, string Stage, long Delivery, string Payload, string Module, int Pid)> items = [];
        public string ProbeId = "02N";
        public Func<string, string, string>? CommandFor;  // (event, payload) -> CommandIdentity; I52CTDA_PROBE by default
        public Log Add(string e, string stage, long delivery, string payload = "", string module = "R-NATIVE-ARX", int pid = 4242) { items.Add((e, stage, delivery, payload, module, pid)); return this; }
        public Log Remove(Func<string, string, bool> match) { items.RemoveAll(i => match(i.Event, i.Payload)); return this; }
        public Log Replace(string e, string oldPayload, string newPayload) { for (int i = 0; i < items.Count; i++) if (items[i].Event == e && items[i].Payload == oldPayload) items[i] = items[i] with { Payload = newPayload }; return this; }
        public Log InsertBefore(string e, string payloadStart, (string, string, long, string) item)
        {
            int at = items.FindIndex(i => i.Event == e && i.Payload.StartsWith(payloadStart, StringComparison.Ordinal));
            items.Insert(at, (item.Item1, item.Item2, item.Item3, item.Item4, "R-NATIVE-ARX", 4242));
            return this;
        }
        public string Write(long skip = -1)
        {
            string path = Path.GetTempFileName();
            var text = new StringBuilder();
            long seq = 0;
            foreach (var i in items)
            {
                seq++;
                if (seq == skip) seq++;
                var payload = new JsonObject();
                foreach (string pair in i.Payload.Split(';', StringSplitOptions.RemoveEmptyEntries)) { string[] kv = pair.Split('=', 2); payload[kv[0]] = kv.Length > 1 ? kv[1] : ""; }
                var r = new JsonObject
                {
                    ["Sequence"] = seq, ["ProbeId"] = ProbeId, ["StageId"] = i.Stage, ["DeliveryId"] = i.Delivery, ["DriverOrSchedulerId"] = "DRIVER-CMD-01", ["ModuleId"] = i.Module,
                    ["PID"] = i.Pid, ["TID"] = 1, ["DocumentId"] = "0x1", ["DatabaseId"] = "0x2", ["CommandIdentity"] = CommandFor?.Invoke(i.Event, i.Payload) ?? "I52CTDA_PROBE", ["EventOrMarkerId"] = i.Event,
                    ["Payload"] = payload, ["TimestampUnixMicros"] = seq,
                };
                text.Append(r.ToJsonString()).Append('\n');
            }
            File.WriteAllText(path, text.ToString());
            return path;
        }
    }

    // A complete, well-formed run of 02N (CB-PRIMARY-01, MUT-S, CLEAN-BASE, FENCE-CLEANUP-TOKEN).
    private static Log Pass02N() => new Log()
        .Add("RUN-ENV-01", "STG-PROBE-CMD", 1, "pid=4242")
        .Add("BOOT-01", "STG-PROBE-CMD", 1, "status=OK;declared=8;resolved=8")
        .Add("CMD-PROBE", "STG-PROBE-CMD", 1, "phase=ENTRY")
        .Add("DRIVER-CMD-01", "STG-PROBE-CMD", 1, "phase=ENTRY;writeCapable=1")
        .Add("RR-DB", "STG-PROBE-CMD", 1, "phase=REGISTER;status=0")
        .Add("FIN-GATE-01", "STG-FIN-GATE", 2, "phase=REGISTERED")
        .Add("SA-TX-START", "STG-PROBE-CMD", 1, "transaction=0xA;owner=T-PRIMARY;activeTransactions=1")
        .Add("SET-OPEN-PRIMARY-T", "STG-PROBE-CMD", 1, "status=1")
        .Add("SET-OUTCOME-COMMIT", "STG-PROBE-CMD", 1, "probeOutcomeMode=COMMIT")
        .Add("RG-DB-OPEN", "STG-PROBE-CMD", 1, "phase=ARM;family=N-DB-OPEN")
        .Add("TRG-MODIFY-TRIGGER-MOD", "STG-TRIGGER", 3, "phase=BEGIN")
        .Add("N-DB-OPEN", "STG-TRIGGER", 3, "registration=RR-DB;object=0x10")
        .Add("EP-CALLBACK", "STG-PRIMARY-CALLBACK", 4, "event=N-DB-OPEN")
        .Add("RG-DB-OPEN", "STG-PRIMARY-CALLBACK", 4, "phase=CONSUME")
        .Add("CB-PRIMARY-01", "STG-EXEC", 5, "phase=ENTRY")
        .Add("CB-LOCK-01", "STG-EXEC", 5, "lockMode=4;writeCapable=1")
        .Add("MUT-S", "STG-EXEC", 5, "status=0;transaction=0xA")
        .Add("CB-PRIMARY-01", "STG-EXEC", 5, "phase=EXIT;ok=1")
        .Add("N-DB-MOD", "STG-TRIGGER", 3, "registration=RR-DB;object=0x10")
        .Add("TRG-MODIFY-TRIGGER-MOD", "STG-TRIGGER", 3, "phase=END;ok=1")
        .Add("SA-TX-END", "STG-PROBE-CMD", 1, "status=0;owner=T-PRIMARY")
        .Add("SET-OUTCOME-COMMIT", "STG-PROBE-CMD", 1, "status=0")
        .Add("TOK-EXEC-DONE", "STG-EXEC", 5, "status=ACCEPTED")
        .Add("RG-DB-OPEN", "STG-PROBE-CMD", 1, "phase=DISARM;family=N-DB-OPEN")
        .Add("TOK-OUTCOME", "STG-PROBE-CMD", 1, "status=ACCEPTED")
        .Add("CMD-PROBE", "STG-PROBE-CMD", 1, "phase=RETURN;aborted=0")
        .Add("TOK-PROBE-RETURN", "STG-PROBE-CMD", 1, "status=ACCEPTED")
        .Add("FIN-GATE-01", "STG-FIN-GATE", 2, "phase=ISSUE;mode=FINISH-MODE-NORMAL")
        .Add("FINISH-FENCE-01", "STG-FINISH", 6, "phase=SET")
        .Add("CMD-FINISH", "STG-FINISH", 6, "phase=ENTRY;mode=FINISH-MODE-NORMAL;missing=NONE")
        .Add("VER-S", "STG-FINISH", 6, "phase=FRESH;bytes=HFV30:S:1")
        .Add("VER-T", "STG-FINISH", 6, "phase=FRESH;activeTransactions=0;reread=HFV30:S:1")
        .Add("CLEAN-BASE", "STG-FINISH", 6, "phase=START")
        .Add("CLN-BASE", "STG-FINISH", 6, "phase=START")
        .Add("CLN-BASE", "STG-FINISH", 6, "step=VERIFY;ok=1")
        .Add("CLN-BASE", "STG-FINISH", 6, "phase=END;ok=1")
        .Add("CLEAN-BASE", "STG-FINISH", 6, "phase=END;ok=1")
        .Add("CMD-FINISH", "STG-FINISH", 6, "phase=FLUSH")
        .Add("CMD-FINISH", "STG-FINISH", 6, "phase=EXIT-QUEUED;exitAction=QUIT-DISCARD;status=0;origin=CMD-FINISH;dbmodAtBoot=0;dbmodBefore=1;popStatus=0;dbmodAfter=0;schedulerUse=FINISH-INFRA");

    private static V35Result Evaluate(string repo, Log log, string probe = "02N", V35ProcessEvidence? process = null, long skip = -1, V35ScratchEvidence? scratch = null)
    {
        V35Authority a = Authority(repo);
        string path = log.Write(skip);
        try
        {
            var evidence = V35RunEvidence.Load(path, process ?? new V35ProcessEvidence(4242, true, false, false, true), scratch ?? new V35ScratchEvidence(true, true, false));
            return new V35ResultEngine(a).Evaluate(a.ByProbe[probe], evidence);
        }
        finally { File.Delete(path); }
    }

    private static void ResultPass(string repo)
    {
        V35Result r = Evaluate(repo, Pass02N());
        Require(r.Result == "PASS" && r.ResultClass == "PASS-S" && r.Step == 4, $"expected PASS-S: {r.Result} step {r.Step} {string.Join(" | ", r.Reasons)}");
    }

    private static void ResultMissingToken(string repo)
    {
        V35Result r = Evaluate(repo, Pass02N().Remove((e, _) => e == "TOK-OUTCOME"));
        Require(r.Result == "UNKNOWN" && r.Step == 2 && !r.EvidenceComplete && r.EvidenceGaps.Any(g => g.Contains("TOK-OUTCOME", StringComparison.Ordinal)), "missing token must be UNKNOWN");
    }

    private static void ResultStateFail(string repo)
    {
        V35Result r = Evaluate(repo, Pass02N().Replace("VER-S", "phase=FRESH;bytes=HFV30:S:1", "phase=FRESH;bytes=HFV30:S:0").Replace("VER-T", "phase=FRESH;activeTransactions=0;reread=HFV30:S:1", "phase=FRESH;activeTransactions=0;reread=HFV30:S:0"));
        Require(r.Result == "FAIL" && r.ResultClass == "FAIL-S" && r.Step == 3 && r.FailsHolding.Contains("FP-STATE"), $"expected FAIL-S at step 3: {r.Result} {r.Step}");
    }

    private static void ResultCleanupSafety(string repo)
    {
        // A late delivery to an instance whose eOk removal is recorded, after cleanup started: V34 section 5, before UNKNOWN.
        Log log = Pass02N().InsertBefore("CLEAN-BASE", "phase=END", ("FINISH-FENCE-01", "STG-PRIMARY-CALLBACK", 7, "kind=LATE-DELIVERY;notifierAccess=NONE;removedInstance=1"));
        V35Result r = Evaluate(repo, log);
        Require(r.Result == "FAIL" && r.Step == 1 && r.FailsHolding.Contains("FP-CLEANUP-SAFETY"), $"expected safety FAIL at step 1: {r.Result} {r.Step}");
        // The same body record in INFRA (cleanup's own records) is never counted.
        V35Result infra = Evaluate(repo, Pass02N().InsertBefore("CLEAN-BASE", "phase=END", ("EP-CALLBACK", "STG-FINISH", 6, "event=N-DB-OPEN")));
        Require(infra.Step != 1, "INFRA cleanup record counted as a safety contradiction");
    }

    private static void ResultLateDelivery(string repo)
    {
        Log log = Pass02N().InsertBefore("CMD-FINISH", "phase=FLUSH", ("FINISH-FENCE-01", "STG-SEND-DELIVERY", 7, "kind=LATE-DELIVERY;notifierAccess=NONE;removedInstance=0"));
        V35Result r = Evaluate(repo, log);
        Require(r.Result == "UNKNOWN" && r.UnknownsHolding.Contains("UNK-LATE-DELIVERY"), "late delivery must be UNKNOWN");
    }

    private static void ResultDrain(string repo)
    {
        Log log = Pass02N().Replace("CMD-FINISH", "phase=ENTRY;mode=FINISH-MODE-NORMAL;missing=NONE", "phase=ENTRY;mode=FINISH-MODE-DRAIN;missing=NONE")
            .Remove((e, p) => e is "VER-S" or "VER-T");
        V35Result r = Evaluate(repo, log);
        Require(r.Result == "UNKNOWN" && r.UnknownsHolding.Contains("UNK-FINISH-TIMEOUT"), "drain must be UNKNOWN");
    }

    private static void ResultSequence(string repo)
    {
        V35Result gap = Evaluate(repo, Pass02N(), skip: 10);
        Require(gap.Result == "UNKNOWN" && gap.EvidenceGaps.Any(g => g.StartsWith("(7)", StringComparison.Ordinal)), "sequence gap");
        V35Result foreign = Evaluate(repo, Pass02N().Add("N-DB-MOD", "STG-FIN-GATE", 2, "registration=RR-DB", pid: 777));
        Require(foreign.Result == "UNKNOWN", "foreign PID");
    }

    private static void ResultStructuralPath(string repo)
    {
        // 09N-D (structural UNKNOWN row) does not list UNK-NO-WRITE-LOCK: the lock-absent path is UNKNOWN-COMMON.
        Log log = Pass02N().InsertBefore("TRG-MODIFY-TRIGGER-MOD", "phase=END", ("UNK-NO-WRITE-LOCK", "STG-EXEC", 5, "authority=CB-LOCK-01;lockMode=2"));
        log.ProbeId = "09N-D";
        V35Result r = Evaluate(repo, log, "09N-D");
        Require(r.Result == "UNKNOWN" && r.UnknownsHolding.Contains("UNKNOWN-COMMON") && r.Reasons.Any(x => x.Contains("UNK-NO-WRITE-LOCK", StringComparison.Ordinal)), "structural path must report UNKNOWN-COMMON with the host reason");
    }

    private static void ResultProcessFence(string repo)
    {
        V35Authority a = Authority(repo);
        V35RowPlan fenced = a.Rows.First(r => r.CompletionFence == "FENCE-PROCESS-EXIT");
        Log log = Pass02N();
        log.ProbeId = fenced.ProbeId;
        V35Result r = Evaluate(repo, log, fenced.ProbeId, new V35ProcessEvidence(4242, true, true, false, false));
        Require(r.Result != "PASS", "terminated PID reached PASS");
        V35Result gone = Evaluate(repo, Pass02N(), process: new V35ProcessEvidence(4242, false, false, false, false));
        Require(gone.Result == "UNKNOWN" && gone.EvidenceGaps.Any(g => g.StartsWith("(6)", StringComparison.Ordinal)), "missing PID exit evidence");
    }

    private static void OrderByIdentity(string repo)
    {
        // PROBE's own commandEnded (its WILL predates RR-ED) followed by a complete FIXTURE pair is not a violation.
        Log ok = Pass02N().InsertBefore("TRG-MODIFY-TRIGGER-MOD", "phase=END", ("N-ED-END", "STG-PROBE-CMD", 1, "registration=RR-ED;command=I52CTDA_PROBE"))
            .InsertBefore("FIN-GATE-01", "phase=ISSUE", ("N-ED-WILL", "STG-FIXTURE-CMD", 8, "registration=RR-ED;command=I52CTDA_FIXTURE"))
            .InsertBefore("FIN-GATE-01", "phase=ISSUE", ("N-ED-END", "STG-FIXTURE-CMD", 8, "registration=RR-ED;command=I52CTDA_FIXTURE"));
        V35Result r = Evaluate(repo, ok);
        Require(r.Result == "PASS" && !r.FailsHolding.Contains("FP-ORDER"), $"unpaired END produced {r.Result}: {string.Join(" | ", r.Reasons)}");
        // An END of the same command before its WILL is a violation.
        Log bad = Pass02N().InsertBefore("FIN-GATE-01", "phase=ISSUE", ("N-ED-END", "STG-FIXTURE-CMD", 8, "registration=RR-ED;command=I52CTDA_FIXTURE"))
            .InsertBefore("FIN-GATE-01", "phase=ISSUE", ("N-ED-WILL", "STG-FIXTURE-CMD", 8, "registration=RR-ED;command=I52CTDA_FIXTURE"));
        bad.ProbeId = "09N-D";
        V35Authority a = Authority(repo);
        string path = bad.Write();
        try
        {
            var evidence = V35RunEvidence.Load(path, new V35ProcessEvidence(4242, true, false, false, true), new V35ScratchEvidence(true, true, false));
            V35Result d = new V35ResultEngine(a).Evaluate(a.ByProbe["09N-D"], evidence);
            Require(d.Observations.TryGetValue("OBS-ORDER-RULES", out V35Tri o) && o == V35Tri.False, "reversed same-command pair not detected");
        }
        finally { File.Delete(path); }
    }

    private static void InfraMarkers(string repo)
    {
        Log log = Pass02N().Remove((e, p) => e == "N-DB-MOD").InsertBefore("FIN-GATE-01", "phase=ISSUE", ("N-DB-MOD", "STG-FIN-GATE", 2, "registration=RR-DB;object=0x10"));
        V35Result r = Evaluate(repo, log);
        Require(r.Result == "UNKNOWN" && r.Observations["OBS-MARKERS"] == V35Tri.Unavailable, "an INFRA record satisfied +N-DB-MOD");
    }

    private static void ModuleFenceLate(string repo)
    {
        Log log = Pass02N()
            .InsertBefore("CLEAN-BASE", "phase=END", ("RR-MANAGED-CMD", "STG-FINISH", 6, "phase=UNSUBSCRIBED"))
            .InsertBefore("CLEAN-BASE", "phase=END", ("FINISH-FENCE-01", "STG-FIXTURE-CMD", 9, "kind=LATE-DELIVERY;notifierAccess=NONE;registration=RR-MANAGED-CMD"));
        string path = log.Write();
        // The two inserted records come from R-MANAGED-OBSERVER.
        var lines = File.ReadAllLines(path).Select(l => l.Contains("UNSUBSCRIBED", StringComparison.Ordinal) || l.Contains("registration=RR-MANAGED-CMD", StringComparison.Ordinal) || l.Contains("\"registration\":\"RR-MANAGED-CMD\"", StringComparison.Ordinal)
            ? l.Replace("\"ModuleId\":\"R-NATIVE-ARX\"", "\"ModuleId\":\"R-MANAGED-OBSERVER\"", StringComparison.Ordinal) : l).ToArray();
        File.WriteAllLines(path, lines);
        try
        {
            V35Authority a = Authority(repo);
            var evidence = V35RunEvidence.Load(path, new V35ProcessEvidence(4242, true, false, false, true), new V35ScratchEvidence(true, true, false));
            V35Result r = new V35ResultEngine(a).Evaluate(a.ByProbe["02N"], evidence);
            Require(r.Result == "FAIL" && r.Step == 1, $"late delivery past the module fence: {r.Result} step {r.Step}");
        }
        finally { File.Delete(path); }
    }

    // ------------------------------------------------------------------ managed observer
    private sealed class FakeSequencer : ILogSequencer
    {
        public int Fence;
        public readonly List<ManagedRecord> Records = [];
        public readonly List<string> Tokens = [];
        public ulong LogAppend(ManagedRecord record) { Records.Add(record); return (ulong)Records.Count; }
        public int TokenSet(string tokenId, ulong deliveryId) { Tokens.Add(tokenId); return 1; }
        public int FinishFenceIsSet() => Fence;
    }

    private static void ManagedObserverLogic(string repo)
    {
        var log = new FakeSequencer();
        var observer = new ManagedCommandObserver(log, "09N-D");
        observer.OnSubscribed(1, 2, true);
        observer.OnCommandEnded("I52CTDA_PROBE", 1, 2);
        Require(log.Records.Count == 2 && log.Tokens.Count == 0, "non-fixture command must only be observed");
        observer.OnCommandEnded("I52CTDA_FIXTURE", 1, 2);
        ManagedRecord marker = log.Records.Single(r => r.EventOrMarkerId == "MARK-MANAGED-CMD-END");
        Require(marker.StageId == "STG-FIXTURE-CMD" && marker.ModuleId == "R-MANAGED-OBSERVER" && log.Tokens.SequenceEqual(["TOK-MANAGED-CMD-END"]), "fixture marker/token");
        log.Fence = 1;
        int before = log.Records.Count;
        observer.OnCommandEnded("I52CTDA_FIXTURE", 1, 2);
        Require(log.Records.Count == before + 1 && log.Records[^1].EventOrMarkerId == "FINISH-FENCE-01" && log.Records[^1].Payload.Contains("LATE-DELIVERY", StringComparison.Ordinal)
            && log.Tokens.Count == 1, "after the fence only one LATE-DELIVERY record");
        Require(log.Records.All(r => r.CommandIdentity == "*" && r.DriverOrSchedulerId == "DRIVER-CMD-01"), "record identity fields");
    }

    private static void ManagedRecordLayout(string repo) => Require(NativeLogSequencer.RecordSize == 88, "I52CtdaRecord size " + NativeLogSequencer.RecordSize);

    // Synthetic R3 smoke evidence (native report schema 5 plus the shared log) that the evaluator must accept.
    private const string SmokeGate = "\"finGate\":{\"idleHook\":true,\"timer\":17,\"stageDelivery\":2,\"registeredRecords\":1,\"finishIssued\":false,\"finishRecords\":0,\"tokensAccepted\":0,\"hookRemoved\":true,\"timerKilled\":true}";
    private const string SmokeManaged = "\"managed\":{\"fenceReadBefore\":0,\"fenceReadAfter\":1,\"fenceReadRecords\":2}";
    private static string SmokeReport(string gate = SmokeGate, string managed = SmokeManaged) =>
        "{\"result\":\"PASS\",\"processId\":4242,\"governedProbesDispatched\":0,\"fixture\":{\"declared\":8}," + gate + "," + managed + ",\"exit\":{\"queued\":true,\"dbmodAtBoot\":0}}";

    private static V35LogRecord SmokeRecord(long sequence, string stage, string module, string eventId, string payload = "")
    {
        var fields = payload.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(kv => kv.Split('=', 2)).ToDictionary(kv => kv[0], kv => kv[1], StringComparer.Ordinal);
        return new(sequence, "I52CTDA_SMOKE", stage, 1, "DRIVER-CMD-01", module, 4242, 1, "0x1", "0x2", "I52CTDA_SMOKE", eventId, fields, sequence, false);
    }

    private static List<V35LogRecord> SmokeLog() =>
    [
        SmokeRecord(1, "STG-PROBE-CMD", "R-NATIVE-ARX", "RUN-ENV-01", "smoke=1"),
        SmokeRecord(2, "STG-FIN-GATE", "R-NATIVE-ARX", "FIN-GATE-01", "phase=REGISTERED;idleHook=1;timer=17;timeoutSeconds=120;schedulerUse=FINISH-INFRA"),
        SmokeRecord(3, "STG-PAYLOAD-INIT", "R-PAYLOAD-ARX", "PAYLOAD-DB-BINDING"),
        SmokeRecord(4, "STG-PROBE-CMD", "R-MANAGED-OBSERVER", "RR-MANAGED-CMD", "phase=SUBSCRIBED"),
        SmokeRecord(5, "STG-PROBE-CMD", "R-MANAGED-OBSERVER", "FINISH-FENCE-01", "smoke=1;phase=SMOKE-READ;reader=R-MANAGED-OBSERVER;fenceIsSet=0"),
        SmokeRecord(6, "STG-FIN-GATE", "R-NATIVE-ARX", "FIN-GATE-01", "smoke=1;phase=UNREGISTERED;hookRemoved=1;timerKilled=1;finishIssued=0;tokensAccepted=0"),
        SmokeRecord(7, "STG-PROBE-CMD", "R-MANAGED-OBSERVER", "FINISH-FENCE-01", "smoke=1;phase=SMOKE-READ;reader=R-MANAGED-OBSERVER;fenceIsSet=1"),
        SmokeRecord(8, "STG-PROBE-CMD", "R-NATIVE-ARX", "FINISH-FENCE-01", "smoke=1;before=0;after=1;setterExported=0"),
        SmokeRecord(9, "STG-PROBE-CMD", "R-NATIVE-ARX", "CMD-FINISH", "phase=EXIT-QUEUED;exitAction=QUIT-DISCARD;status=0;origin=I52CTDA_SMOKE;dbmodAtBoot=0;dbmodBefore=1;popStatus=0;dbmodAfter=0"),
    ];

    private static readonly ScratchDrawingState SmokeScratch = new("x.dwg", true, 1, "A", DateTimeOffset.UnixEpoch, "x.bak", false, null, null);
    private static V35SmokeVerdict JudgeSmoke(string report, IReadOnlyList<V35LogRecord> records) =>
        V35SmokeEvaluator.Evaluate(JsonDocument.Parse(report).RootElement, records, 4242, true, false, new(SmokeScratch, SmokeScratch));
    private static void RequireSmokeFailure(V35SmokeVerdict v, string failure) =>
        Require(v.Result == "FAIL" && v.Failures.Contains(failure), $"expected {failure}: {v.Result} {string.Join(",", v.Failures)}");
    private static List<V35LogRecord> Renumber(IEnumerable<V35LogRecord> records) => records.Select((r, i) => r with { Sequence = i + 1 }).ToList();

    private static void SmokeEvaluator(string repo)
    {
        var ok = JudgeSmoke(SmokeReport(), SmokeLog());
        Require(ok.Result == "PASS", "smoke pass: " + string.Join(",", ok.Failures));
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), Renumber(SmokeLog().Where(r => r.ModuleId != "R-MANAGED-OBSERVER"))), "MANAGED_NOT_IN_SHARED_LOG");
        var mutated = V35SmokeEvaluator.Evaluate(JsonDocument.Parse(SmokeReport()).RootElement, SmokeLog(), 4242, true, false, new(SmokeScratch, SmokeScratch with { Sha256 = "B" }));
        RequireSmokeFailure(mutated, "SCRATCH_DWG_MUTATED");
    }

    // Smoke coverage 1: FIN-GATE-01 registers its idle hook and timer with zero ProbeIds, never issues CMD-FINISH,
    // accepts no token and the smoke classifies no result.
    private static void SmokeFinishGate(string repo)
    {
        RequireSmokeFailure(JudgeSmoke(SmokeReport(gate: "\"finGate\":{}"), SmokeLog()), "FIN_GATE_NOT_INITIALIZED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(gate: SmokeGate.Replace("\"idleHook\":true", "\"idleHook\":false")), SmokeLog()), "FIN_GATE_NOT_INITIALIZED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(gate: SmokeGate.Replace("\"timer\":17", "\"timer\":0")), SmokeLog()), "FIN_GATE_NOT_INITIALIZED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), Renumber(SmokeLog().Where(r => !r.Is("phase", "REGISTERED")))), "FIN_GATE_NOT_IN_LOG");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), SmokeLog().Select(r => r.Is("phase", "REGISTERED") ? r with { StageId = "STG-PROBE-CMD" } : r).ToList()), "FIN_GATE_NOT_IN_LOG");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(gate: SmokeGate.Replace("\"finishIssued\":false", "\"finishIssued\":true")), SmokeLog()), "SPONTANEOUS_FINISH");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), [.. SmokeLog(), SmokeRecord(10, "STG-FIN-GATE", "R-NATIVE-ARX", "FIN-GATE-01", "phase=ISSUE;mode=FINISH-MODE-DRAIN")]), "SPONTANEOUS_FINISH_IN_LOG");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), [.. SmokeLog(), SmokeRecord(10, "STG-FINISH", "R-NATIVE-ARX", "CMD-FINISH", "phase=ENTRY")]), "SPONTANEOUS_FINISH_IN_LOG");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(gate: SmokeGate.Replace("\"tokensAccepted\":0", "\"tokensAccepted\":1")), SmokeLog()), "TOKEN_FABRICATED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), [.. SmokeLog(), SmokeRecord(10, "STG-PROBE-CMD", "R-NATIVE-ARX", "TOK-EXEC-DONE", "status=ACCEPTED")]), "TOKEN_FABRICATED_IN_LOG");
        Require(JudgeSmoke(SmokeReport(), [.. SmokeLog(), SmokeRecord(10, "STG-PROBE-CMD", "R-NATIVE-ARX", "TOK-EXEC-DONE", "status=NOT-IN-ROW")]).Result == "PASS", "a rejected token is not fabricated");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(gate: SmokeGate.Replace("\"timerKilled\":true", "\"timerKilled\":false")), SmokeLog()), "FIN_GATE_TEARDOWN");

        // Native smoke source: the gate is registered after BOOT-01 and removed before the fence is set; the smoke never
        // selects a plan, issues FINISH or sets a token, and onIdle() returns without a plan (no spontaneous FINISH).
        string driver = File.ReadAllText(Path.Combine(repo, "eng", "research", "I52Ctda", "native", "I52CtdaDriver.cpp"));
        string smoke = driver[driver.IndexOf("void I52Executor::smoke()", StringComparison.Ordinal)..];
        int register = smoke.IndexOf("registerFinishGate()", StringComparison.Ordinal), unregister = smoke.IndexOf("phase=UNREGISTERED", StringComparison.Ordinal),
            fenceSet = smoke.IndexOf("I52FinishFence::set()", StringComparison.Ordinal);
        Require(smoke.IndexOf("boot();", StringComparison.Ordinal) < register && register < unregister && unregister < fenceSet, "gate lifecycle order in smoke");
        Require(smoke.Contains("acedRemoveOnIdleWinMsg(idleThunk)", StringComparison.Ordinal) && smoke.Contains("KillTimer(nullptr, gateTimer_)", StringComparison.Ordinal) && smoke.Contains("gateRegistered_ = false;", StringComparison.Ordinal), "gate teardown");
        foreach (string forbidden in new[] { "issueFinish(", "setPlan(", "I52Ctda_TokenSet", "setToken(", "runVerifiers(" })
            Require(!smoke.Contains(forbidden, StringComparison.Ordinal), "smoke must not call " + forbidden);
        string onIdle = driver[driver.IndexOf("void I52Executor::onIdle()", StringComparison.Ordinal)..driver.IndexOf("void I52Executor::issueFinish(", StringComparison.Ordinal)];
        Require(onIdle.Contains("if (!gateRegistered_ || finishIssued_ || plan_ == nullptr) return;", StringComparison.Ordinal), "onIdle must return without a plan");
        string command = File.ReadAllText(Path.Combine(repo, "eng", "research", "I52Ctda", "harness", "V35SmokeCommand.cs"));
        Require(!command.Contains("V35ResultEngine", StringComparison.Ordinal) && !command.Contains("ProbeResult", StringComparison.Ordinal), "smoke-v35 classifies no result");
    }

    // Smoke coverage 2: R-MANAGED-OBSERVER reads I52Ctda_FinishFenceIsSet through the smoke-only entry mode and records
    // the observed value (unset, then set) in the shared log.
    private static void SmokeManagedFenceRead(string repo)
    {
        var log = new FakeSequencer();
        var observer = new ManagedCommandObserver(log, "I52CTDA_SMOKE");
        Require(observer.OnSmokeFenceRead(1, 2) == 0, "unset fence read");
        log.Fence = 1;
        Require(observer.OnSmokeFenceRead(1, 2) == 1, "set fence read");
        Require(log.Records.Count == 2 && log.Records.All(r => r.ModuleId == "R-MANAGED-OBSERVER" && r.EventOrMarkerId == "FINISH-FENCE-01" && r.StageId == "*")
            && log.Records[0].Payload.EndsWith("fenceIsSet=0", StringComparison.Ordinal) && log.Records[1].Payload.EndsWith("fenceIsSet=1", StringComparison.Ordinal)
            && log.Records.All(r => r.Payload.Contains("phase=SMOKE-READ", StringComparison.Ordinal) && !r.Payload.Contains("LATE-DELIVERY", StringComparison.Ordinal)), "managed fence read records");
        Require(log.Tokens.Count == 0 && !observer.Subscribed, "the fence read sets no token and subscribes nothing");

        RequireSmokeFailure(JudgeSmoke(SmokeReport(managed: "\"managed\":{}"), SmokeLog()), "MANAGED_FENCE_READ_NOT_OBSERVED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(managed: SmokeManaged.Replace("\"fenceReadAfter\":1", "\"fenceReadAfter\":0")), SmokeLog()), "MANAGED_FENCE_READ_NOT_OBSERVED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), Renumber(SmokeLog().Where(r => !r.Is("phase", "SMOKE-READ")))), "MANAGED_FENCE_READ_NOT_IN_LOG");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), SmokeLog().Select(r => r.Is("phase", "SMOKE-READ") ? r with { Payload = new Dictionary<string, string>(r.Payload) { ["fenceIsSet"] = r.Sequence == 5 ? "1" : "0" } } : r).ToList()), "MANAGED_FENCE_READ_NOT_IN_LOG");

        // The entry mode is shared by both sides and used only by the smoke; the governed executor never passes it.
        string research = Path.Combine(repo, "eng", "research", "I52Ctda");
        Require(File.ReadAllText(Path.Combine(research, "native", "I52CtdaAbi.h")).Contains("#define I52CTDA_MANAGED_SMOKE_FENCE_READ " + ManagedCommandObserver.SmokeFenceRead, StringComparison.Ordinal), "entry mode value");
        Require(!File.ReadAllText(Path.Combine(research, "native", "I52CtdaExecutor.cpp")).Contains("I52CTDA_MANAGED_SMOKE_FENCE_READ", StringComparison.Ordinal), "governed executor must not use the smoke fence read");
        Require(File.ReadAllText(Path.Combine(research, "managed-observer", "ObserverApplication.cs")).Contains("if (subscribe == ManagedCommandObserver.SmokeFenceRead) return observer.OnSmokeFenceRead(", StringComparison.Ordinal), "managed entry routes the smoke fence read");
    }

    // subjectDepth(event, n) as implemented by R-NATIVE-ARX: events in the `return n + 1` case, n otherwise.
    private static int NativeSubjectDepth(string executor, string eventId, int n)
    {
        int start = executor.IndexOf("int subjectDepth(I52Id event, int n)", StringComparison.Ordinal);
        string body = executor[start..executor.IndexOf("\n}\n", start, StringComparison.Ordinal)];
        Require(body.Contains("default: return n;", StringComparison.Ordinal), "subjectDepth default");
        string plusOne = body.Split('\n').Single(l => l.Contains("return n + 1;", StringComparison.Ordinal));
        return plusOne.Contains("case I52Id::" + V35PlanCompiler.NativeName(eventId) + ":", StringComparison.Ordinal) ? n + 1 : n;
    }

    // D-1 (first governed canary 09N-B): on the qualified host transactionEnded reports numTransactions = 1 while the
    // active count is already 0, so the ended subject is the outermost transaction (depth n = 1), the depth RG-TX arms
    // for TRG-END-PRIMARY-T. N-TR-ABORTED and every other transaction mapping are unchanged.
    private static void TransactionEndedDepth(string repo)
    {
        string research = Path.Combine(repo, "eng", "research", "I52Ctda");
        string executor = File.ReadAllText(Path.Combine(research, "native", "I52CtdaExecutor.cpp")).Replace("\r\n", "\n");
        string driver = File.ReadAllText(Path.Combine(research, "native", "I52CtdaDriver.cpp"));
        const int hostNumTransactions = 1, hostActiveCountAfterEnd = 0;
        int ended = NativeSubjectDepth(executor, "N-TR-ENDED", hostNumTransactions);
        Require(ended == hostNumTransactions && ended == hostActiveCountAfterEnd + 1, $"N-TR-ENDED subjectDepth {ended} for the host numTransactions=1");
        Require(driver.Contains("extra = L\"depth:\" + std::to_wstring(plan_->trigger == I52Id::TRG_END_NESTED_T ? 2 : 1);", StringComparison.Ordinal), "RG-TX arming depth");
        V35Authority a = Authority(repo);
        foreach (string probe in new[] { "CTRENDED16SND-ALL", "CTRENDED16APP-ALL" })
        {
            V35RowPlan row = a.ByProbe[probe];
            Require(row.ScheduleOriginEventId == "N-TR-ENDED" && row.TriggerActionId == "TRG-END-PRIMARY-T" && row.GuardIds.Contains("RG-TX") && row.BodyObserverId == "RR-TX", probe + " shape");
            // The pre-D-1 mapping (n + 1) keyed the host callback as depth 2, so RG-TX never admitted it (false UNKNOWN).
            Require($"depth:{ended}" == "depth:1", probe + ": the N-TR-ENDED key must match the armed RG-TX depth");
        }
        // Unchanged: about-to-start and aborted are n + 1; started, about-to-end and outermost-end are n.
        Require(NativeSubjectDepth(executor, "N-TR-ABOUT-START", 0) == 1 && NativeSubjectDepth(executor, "N-TR-ABORTED", 1) == 2, "about-start/aborted unchanged");
        foreach (string e in new[] { "N-TR-STARTED", "N-TR-ABOUT-END", "N-TR-OUTERMOST-END-CALLED" })
            Require(NativeSubjectDepth(executor, e, 1) == 1, e + " unchanged");
        // FP-ORDER pairs N-TR by manager and subject depth: the host about-start (n = 0) and ended (n = 1) of one
        // transaction now share the identity.
        Require(NativeSubjectDepth(executor, "N-TR-ABOUT-START", 0) == NativeSubjectDepth(executor, "N-TR-ENDED", 1), "about-start/ended identity");
    }

    // D-2 (second canary 02NDBMOD-S): I52Ctda_TokenSet pointed CommandIdentity into a destroyed temporary, so token
    // records carried corrupted text. A record whose CommandIdentity is not NONE, a governed CommandResource or a host
    // command recorded by N-ED-WILL at or before it is malformed: evidence incomplete, UNKNOWN, never PASS.
    private static readonly string[] CorruptedIdentities = ["\uFFFD\uFFFD\uFFFD\uFFFD\uFFFD\uFFFD\uFFFD\uFFFD", "\u00DD\u00DD\u00DD\u00DD", "I52CTDA_PROBX", "i52ctda_probe", "I52CTDA_PROBE\u0001"];

    private static void CorruptedCommandIdentity(string repo)
    {
        Require(Evaluate(repo, Pass02N()).Result == "PASS", "baseline 02N must PASS");
        foreach (string bad in CorruptedIdentities)
        {
            Log log = Pass02N();
            log.CommandFor = (e, _) => e.StartsWith("TOK-", StringComparison.Ordinal) ? bad : "I52CTDA_PROBE";
            V35Result r = Evaluate(repo, log);
            Require(r.Result == "UNKNOWN" && !r.EvidenceComplete && r.EvidenceGaps.Count(g => g.Contains("CommandIdentity", StringComparison.Ordinal)) == 3,
                $"token CommandIdentity '{bad}' gave {r.Result}: {string.Join(" | ", r.EvidenceGaps)}");
        }
        // One corrupted record anywhere, including the cleanup window, is enough.
        Log cleanup = Pass02N();
        cleanup.CommandFor = (e, p) => e == "CLN-BASE" && p.StartsWith("step=VERIFY", StringComparison.Ordinal) ? "\uFFFD\uFFFD" : "I52CTDA_FINISH";
        V35Result c = Evaluate(repo, cleanup);
        Require(c.Result != "PASS" && !c.EvidenceComplete && !c.SafetyEvidenceComplete, $"corrupted cleanup record gave {c.Result}");
        // Known identities stay valid: NONE, every CommandResource and a host command after its commandWillStart.
        Log known = Pass02N()
            .InsertBefore("TRG-MODIFY-TRIGGER-MOD", "phase=BEGIN", ("N-ED-WILL", "STG-PROBE-CMD", 1, "registration=RR-ED;command=REGEN"))
            .InsertBefore("TRG-MODIFY-TRIGGER-MOD", "phase=BEGIN", ("N-ED-END", "STG-PROBE-CMD", 1, "registration=RR-ED;command=REGEN"));
        known.CommandFor = (e, p) => e.StartsWith("N-ED-", StringComparison.Ordinal) ? "REGEN" : e == "FIN-GATE-01" ? "NONE" : e.StartsWith("CMD-FINISH", StringComparison.Ordinal) ? "I52CTDA_FINISH" : "I52CTDA_PROBE";
        V35Result k = Evaluate(repo, known);
        Require(k.EvidenceGaps.All(g => !g.Contains("CommandIdentity", StringComparison.Ordinal)), "known identities rejected: " + string.Join(" | ", k.EvidenceGaps));
        // A host command identity before its own commandWillStart is not known yet.
        Log early = Pass02N()
            .InsertBefore("TRG-MODIFY-TRIGGER-MOD", "phase=BEGIN", ("N-ED-WILL", "STG-PROBE-CMD", 1, "registration=RR-ED;command=REGEN"));
        early.CommandFor = (e, _) => e == "SA-TX-START" || e == "N-ED-WILL" ? "REGEN" : "I52CTDA_PROBE";
        V35Result x = Evaluate(repo, early);
        Require(x.Result != "PASS" && x.EvidenceGaps.Any(g => g.Contains("CommandIdentity REGEN", StringComparison.Ordinal)), "identity before its commandWillStart accepted");
    }

    // The published host log of the first canary (09N-B, package 6e445fa8): every token record carries exactly
    // I52CTDA_PROBE and no CommandIdentity gap appears; the same records corrupted as in D-2 add exactly two. Since D-3 the
    // log is UNKNOWN through its pre-D-3 exit only (Coordinator ruling: 09N-B is not a valid governed result).
    private static void HostCanaryTokenIdentity(string repo)
    {
        JsonObject canary = JsonNode.Parse(File.ReadAllText(Path.Combine(repo, "docs", "automation", "evidence", "I-52-r3-canary-09N-B.json")))!.AsObject();
        JsonArray events = canary["hostRecord"]!["eventLog"]!.AsArray();
        var tokens = events.Where(e => e!["EventOrMarkerId"]!.GetValue<string>().StartsWith("TOK-", StringComparison.Ordinal)).ToArray();
        Require(tokens.Length == 2 && tokens.All(t => t!["CommandIdentity"]!.GetValue<string>() == "I52CTDA_PROBE"), "09N-B token records keep I52CTDA_PROBE");
        V35Authority a = Authority(repo);
        V35Result Judge(Func<JsonNode, JsonNode> map)
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(path, events.Select(e => map(e!.DeepClone()).ToJsonString()));
                var evidence = V35RunEvidence.Load(path, new V35ProcessEvidence(27448, true, false, false, true), new V35ScratchEvidence(true, true, false));
                return new V35ResultEngine(a).Evaluate(a.ByProbe["09N-B"], evidence);
            }
            finally { File.Delete(path); }
        }
        V35Result host = Judge(e => e);
        Require(host.Result == "UNKNOWN" && host.EvidenceGaps.Count > 0 && host.EvidenceGaps.All(g => g.StartsWith("(6) automated exit", StringComparison.Ordinal)),
            $"09N-B host log gave {host.Result}: {string.Join(" | ", host.EvidenceGaps)}");
        V35Result corrupted = Judge(e => { if (e["EventOrMarkerId"]!.GetValue<string>().StartsWith("TOK-", StringComparison.Ordinal)) e["CommandIdentity"] = "\uFFFD\uFFFD\uFFFD\uFFFD\uFFFD\uFFFD"; return e; });
        Require(corrupted.Result == "UNKNOWN" && corrupted.EvidenceGaps.Count(g => g.Contains("CommandIdentity", StringComparison.Ordinal)) == 2, $"corrupted 09N-B tokens gave {corrupted.Result}");
    }

    // Static guard for the D-2 class of defect: no LOG-RECORD initializer takes .c_str() of a call that returns
    // std::wstring by value (the temporary dies before append). Covers R-NATIVE-ARX and R-PAYLOAD-ARX.
    private static void NoTemporaryRecordPointers(string repo)
    {
        string research = Path.Combine(repo, "eng", "research", "I52Ctda");
        string[] files = Directory.GetFiles(Path.Combine(research, "native"), "*.*").Where(f => f.EndsWith(".cpp", StringComparison.Ordinal) || f.EndsWith(".h", StringComparison.Ordinal))
            .Append(Path.Combine(research, "payload", "I52CtdaPayload.cpp")).ToArray();
        string all = string.Join('\n', files.Select(File.ReadAllText));
        var byValue = System.Text.RegularExpressions.Regex.Matches(all, @"^\s*(?:static\s+|inline\s+)?(?:const\s+)?std::wstring\s+(?:\w+::)?(\w+)\s*\([^;{]*\)\s*(?:const\s*)?(?:\{|;)", System.Text.RegularExpressions.RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        Require(byValue.Contains("commandIdentity"), "commandIdentity() must be found as a by-value accessor");
        foreach (string file in files)
        {
            string text = File.ReadAllText(file);
            foreach (System.Text.RegularExpressions.Match record in System.Text.RegularExpressions.Regex.Matches(text, @"I52CtdaRecord\s+\w+\s*\{[^;]*\};", System.Text.RegularExpressions.RegexOptions.Singleline))
                foreach (string name in byValue)
                    Require(!System.Text.RegularExpressions.Regex.IsMatch(record.Value, @"\b" + name + @"\([^()]*\)\.c_str\(\)"), $"{Path.GetFileName(file)}: record field points into temporary {name}()");
        }
        string log = File.ReadAllText(Path.Combine(research, "native", "I52CtdaLog.cpp"));
        int tokenSet = log.IndexOf("I52Ctda_TokenSet(const wchar_t* tokenId", StringComparison.Ordinal);
        string body = log[tokenSet..log.IndexOf("return accepted;", tokenSet, StringComparison.Ordinal)];
        Require(body.IndexOf("const std::wstring command = log.commandIdentity();", StringComparison.Ordinal) is var named && named > 0
            && named < body.IndexOf("I52CtdaRecord record", StringComparison.Ordinal) && body.Contains("command.c_str()", StringComparison.Ordinal)
            && body.IndexOf("log.append(record);", StringComparison.Ordinal) > named, "I52Ctda_TokenSet keeps CommandIdentity in a named local through append");
    }

    private static void SmokeCommandIdentity(string repo)
    {
        Require(JudgeSmoke(SmokeReport(), SmokeLog()).Result == "PASS", "smoke baseline");
        foreach (string bad in CorruptedIdentities)
            RequireSmokeFailure(JudgeSmoke(SmokeReport(), SmokeLog().Select(r => r.Sequence == 5 ? r with { CommandIdentity = bad } : r).ToList()), "EVENT_LOG_COMMAND_IDENTITY");
    }

    // D-3: the exit is required evidence. PASS needs the automated QUIT-DISCARD queued over an unmodified drawing, a PID
    // that left by itself within the post-FINISH deadline, no interactive state, an unchanged scratch and no .bak.
    private static void AutomatedDiscardExit(string repo)
    {
        Require(Evaluate(repo, Pass02N()).Result == "PASS", "automated discard exit must PASS");
        void Unknown(V35Result r, string what, string gap)
        {
            Require(r.Result != "PASS", what + " reached PASS");
            Require(r.Result == "UNKNOWN" && r.EvidenceGaps.Concat(r.Reasons).Any(g => g.Contains(gap, StringComparison.Ordinal)), $"{what}: {r.Result} {string.Join(" | ", r.EvidenceGaps.Concat(r.Reasons))}");
        }
        Unknown(Evaluate(repo, Pass02N(), scratch: new V35ScratchEvidence(true, false, false)), "scratch mutated", "");
        Unknown(Evaluate(repo, Pass02N(), scratch: new V35ScratchEvidence(true, true, true)), "scratch .bak created", "");
        Unknown(Evaluate(repo, Pass02N(), process: new V35ProcessEvidence(4242, true, true, false, false)), "PID terminated after FINISH", "(6) automated exit: PID terminated");
        Unknown(Evaluate(repo, Pass02N(), process: new V35ProcessEvidence(4242, true, false, false, false)), "PID not gone within the post-FINISH deadline", "(6) automated exit: PID not gone");
        Unknown(Evaluate(repo, Pass02N(), process: new V35ProcessEvidence(4242, true, true, false, false, InteractiveStateObserved: true)), "interactive state", "(6) automated exit: interactive");
        Unknown(Evaluate(repo, Pass02N().Remove((e, p) => e == "CMD-FINISH" && p.StartsWith("phase=EXIT-", StringComparison.Ordinal))), "no exit record", "(6) automated exit: no CMD-FINISH exit record");
        Unknown(Evaluate(repo, Pass02N().Replace("CMD-FINISH", "phase=EXIT-QUEUED;exitAction=QUIT-DISCARD;status=0;origin=CMD-FINISH;dbmodAtBoot=0;dbmodBefore=1;popStatus=0;dbmodAfter=0;schedulerUse=FINISH-INFRA",
            "phase=EXIT-BLOCKED;exitAction=QUIT-DISCARD;reason=DRAWING-MODIFIED;origin=CMD-FINISH;dbmodAtBoot=1;dbmodBefore=1;popStatus=0;dbmodAfter=1")), "exit blocked", "EXIT-BLOCKED");
        Unknown(Evaluate(repo, Pass02N().Replace("CMD-FINISH", "phase=EXIT-QUEUED;exitAction=QUIT-DISCARD;status=0;origin=CMD-FINISH;dbmodAtBoot=0;dbmodBefore=1;popStatus=0;dbmodAfter=0;schedulerUse=FINISH-INFRA",
            "phase=EXIT-QUEUED;exitAction=QUIT-DISCARD;status=0")), "pre-D-3 QUIT over a modified drawing", "dbmodAfter=");
    }

    // D-3 native path: BOOT-01 pushes $DBMOD before the fixture changes the drawing; CMD-FINISH ends with the one exit
    // routine, which pops $DBMOD and queues `_.QUIT` `_Y` only when the drawing reads unmodified (QUIT then has nothing to
    // save and no dialog); otherwise nothing is queued. No script holds QUIT and nothing but the ProbeId is read as input.
    private static void NativeExitNeedsNoHuman(string repo)
    {
        string research = Path.Combine(repo, "eng", "research", "I52Ctda");
        string driver = File.ReadAllText(Path.Combine(research, "native", "I52CtdaDriver.cpp")).Replace("\r\n", "\n");
        string Body(string signature) { int at = driver.IndexOf(signature, StringComparison.Ordinal); Require(at >= 0, "missing " + signature); return driver[at..driver.IndexOf("\n}\n", at, StringComparison.Ordinal)]; }
        string boot = Body("void I52Executor::boot()");
        Require(boot.IndexOf("document_->pushDbmod();", StringComparison.Ordinal) is var push && push > 0 && push < boot.IndexOf("materialize(", StringComparison.Ordinal), "BOOT-01 pushes $DBMOD before materializing");
        string finish = Body("void I52Executor::finish()");
        Require(finish.TrimEnd().EndsWith("queueDiscardExit(L\"CMD-FINISH\");", StringComparison.Ordinal) && !finish.Contains("sendStringToExecute", StringComparison.Ordinal), "CMD-FINISH ends with the discard exit");
        string exit = Body("bool I52Executor::queueDiscardExit(");
        int pop = exit.IndexOf("popDbmod()", StringComparison.Ordinal), guard = exit.IndexOf("if (popped != Acad::eOk || after != 0)", StringComparison.Ordinal),
            blocked = exit.IndexOf("return false;", StringComparison.Ordinal), quit = exit.IndexOf("sendStringToExecute(document_, L\"_.QUIT\\n_Y\\n\"", StringComparison.Ordinal);
        Require(pop > 0 && pop < guard && guard < blocked && blocked < quit, "QUIT is queued only after $DBMOD was popped to 0");
        Require(driver.Split('\n').Count(line => line.Contains("_.QUIT", StringComparison.Ordinal) && !line.TrimStart().StartsWith("//", StringComparison.Ordinal)) == 1, "one QUIT in the native module");
        string smoke = Body("void I52Executor::smoke()");
        Require(smoke.Contains("queueDiscardExit(L\"I52CTDA_SMOKE\")", StringComparison.Ordinal), "smoke leaves through the same exit");
        string native = string.Join('\n', Directory.GetFiles(Path.Combine(research, "native"), "*.cpp").Select(File.ReadAllText));
        foreach (string input in new[] { "acedGetKword", "acedGetInt", "acedGetReal", "acedGetPoint", "acedGetFileD", "acedGetFileNavDialog", "acedInitGet", "acedAlert", "MessageBox" })
            Require(!native.Contains(input, StringComparison.Ordinal), "interactive input API in R-NATIVE-ARX: " + input);
        Require(System.Text.RegularExpressions.Regex.Matches(native, @"acedGetString\(").Count == 2, "acedGetString only reads the ProbeId argument of PROBE and FINISH");
        V35Authority a = Authority(repo);
        foreach (V35RowPlan r in a.Rows)
            Require(!V35ProbeLaunch.DriverScript(r, @"C:\run", r.ObserverRegistrationIds.Contains("RR-MANAGED-CMD")).Contains("QUIT", StringComparison.OrdinalIgnoreCase), r.ProbeId + " script quits");
        string temp = Path.Combine(Path.GetTempPath(), "i52-d3-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            foreach (string f in new[] { V35ProbeLaunch.NativeFile, V35ProbeLaunch.PayloadFile, V35ProbeLaunch.ManagedFile, "scratch.dwg" }) File.WriteAllText(Path.Combine(temp, f), "x");
            V35SmokeLaunch launch = V35SmokeLaunch.Create("acad.exe", Path.Combine(temp, V35ProbeLaunch.NativeFile), Path.Combine(temp, "scratch.dwg"), Path.Combine(temp, "out"), null);
            Require(!launch.Script.Contains("QUIT", StringComparison.OrdinalIgnoreCase) && launch.Script.EndsWith("I52CTDA_SMOKE\n", StringComparison.Ordinal), "smoke script holds no QUIT");
        }
        finally { Directory.Delete(temp, true); }
    }

    // D-3 fail-closed: a main window kept disabled by a modal window is an interactive state; the control plane terminates
    // the exact PID at once and the row stays UNKNOWN. Covers the watch, the runner with a scripted window source, and a
    // real WinForms owner window disabled by a modal dialog (no AutoCAD).
    private static void InteractiveStateFailsClosed(string repo)
    {
        var watch = new V35InteractiveWatch();
        V35WindowSnapshot enabled = new(false, ["Afx:main"]), modal = new(true, ["Afx:main disabled", "#32770:'AutoCAD'"]), hidden = new(false, []);
        foreach (V35WindowSnapshot s in new[] { hidden, hidden, modal, modal, modal, enabled, modal, modal, modal }) Require(!watch.Sample(s), "transient or hidden states are not interactive");
        Require(watch.Sample(modal) && watch.Observed!.Contains("#32770", StringComparison.Ordinal) && watch.Sample(enabled), "four consecutive modal samples are interactive (sticky)");

        string shell = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        string log = Path.GetTempFileName();
        try
        {
            int samples = 0;
            var clock = System.Diagnostics.Stopwatch.StartNew();
            V35ProcessRun run = V35ProcessRunner.RunAsync(shell, "/c ping -n 60 127.0.0.1 >nul", repo, new Dictionary<string, string?>(), log, TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(50),
                _ => ++samples > 2 ? modal : enabled).GetAwaiter().GetResult();
            Require(run.InteractiveStateObserved && run.TerminatedByControlPlane && run.ProcessGone && !run.ExitedWithinPostFinishDeadline && clock.Elapsed < TimeSpan.FromSeconds(20),
                $"scripted modal state not terminated at once ({clock.Elapsed})");
            V35Result r = Evaluate(repo, Pass02N(), process: new V35ProcessEvidence(run.ProcessId, run.ProcessGone, run.TerminatedByControlPlane, false, run.ExitedWithinPostFinishDeadline, run.InteractiveStateObserved) with { ProcessId = 4242 });
            Require(r.Result == "UNKNOWN", "interactive run must stay UNKNOWN, got " + r.Result);
            V35ProcessRun quiet = V35ProcessRunner.RunAsync(shell, "/c exit 0", repo, new Dictionary<string, string?>(), log, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20)).GetAwaiter().GetResult();
            Require(!quiet.InteractiveStateObserved && !quiet.TerminatedByControlPlane && quiet.ExitCode == 0, "a process without windows is not interactive");

            // Real modal window: a WinForms owner (off screen) disabled by ShowDialog of an owned form.
            string script = Path.Combine(Path.GetTempPath(), "i52-d3-modal-" + Guid.NewGuid().ToString("N") + ".ps1");
            File.WriteAllText(script, """
                Add-Type -AssemblyName System.Windows.Forms
                function Offscreen($f, $title) { $f.StartPosition = 'Manual'; $f.Location = New-Object System.Drawing.Point(-3000, -3000); $f.ShowInTaskbar = $false; $f.Text = $title }
                $owner = New-Object System.Windows.Forms.Form; Offscreen $owner 'i52-d3-owner'
                $owner.Add_Shown({ $dialog = New-Object System.Windows.Forms.Form; Offscreen $dialog 'i52-d3-modal'; [void]$dialog.ShowDialog($owner) })
                [System.Windows.Forms.Application]::Run($owner)
                """);
            clock.Restart();
            V35ProcessRun real;
            try
            {
                real = V35ProcessRunner.RunAsync("pwsh.exe", $"-NoProfile -NonInteractive -File \"{script}\"", repo, new Dictionary<string, string?>(), log,
                    TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60)).GetAwaiter().GetResult();
            }
            finally { File.Delete(script); }
            Require(real.InteractiveStateObserved && real.TerminatedByControlPlane && real.ProcessGone && clock.Elapsed < TimeSpan.FromSeconds(45) && real.InteractiveState!.Contains("i52-d3-modal", StringComparison.Ordinal),
                $"real modal window not detected ({clock.Elapsed}): {real.InteractiveState}");

            // Real non-modal window that closes itself after 3 s: never interactive, exits on its own.
            File.WriteAllText(script, """
                Add-Type -AssemblyName System.Windows.Forms
                $owner = New-Object System.Windows.Forms.Form; $owner.StartPosition = 'Manual'; $owner.Location = New-Object System.Drawing.Point(-3000, -3000); $owner.Text = 'i52-d3-plain'
                $timer = New-Object System.Windows.Forms.Timer; $timer.Interval = 3000; $timer.Add_Tick({ $owner.Close() }); $timer.Start()
                [System.Windows.Forms.Application]::Run($owner)
                """);
            try
            {
                V35ProcessRun plain = V35ProcessRunner.RunAsync("pwsh.exe", $"-NoProfile -NonInteractive -File \"{script}\"", repo, new Dictionary<string, string?>(), log,
                    TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60)).GetAwaiter().GetResult();
                Require(!plain.InteractiveStateObserved && !plain.TerminatedByControlPlane && plain.ExitCode == 0, $"non-modal window flagged: {plain.InteractiveState}");
            }
            finally { File.Delete(script); }
        }
        finally { File.Delete(log); }
    }

    private static void SmokeAutomatedExit(string repo)
    {
        Require(JudgeSmoke(SmokeReport(), SmokeLog()).Result == "PASS", "smoke with the automated exit must PASS");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), Renumber(SmokeLog().Where(r => r.EventOrMarkerId != "CMD-FINISH"))), "EXIT_NOT_AUTOMATED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), SmokeLog().Select(r => r.EventOrMarkerId == "CMD-FINISH" ? r with { Payload = new Dictionary<string, string>(r.Payload) { ["phase"] = "EXIT-BLOCKED", ["dbmodAfter"] = "1" } } : r).ToList()), "EXIT_NOT_AUTOMATED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport().Replace("\"queued\":true", "\"queued\":false"), SmokeLog()), "EXIT_NOT_QUEUED");
        RequireSmokeFailure(V35SmokeEvaluator.Evaluate(JsonDocument.Parse(SmokeReport()).RootElement, SmokeLog(), 4242, true, false, new(SmokeScratch, SmokeScratch), interactiveStateObserved: true), "INTERACTIVE_STATE");
        RequireSmokeFailure(V35SmokeEvaluator.Evaluate(JsonDocument.Parse(SmokeReport()).RootElement, SmokeLog(), 4242, true, true, new(SmokeScratch, SmokeScratch)), "PROCESS_TIMEOUT");
        RequireSmokeFailure(V35SmokeEvaluator.Evaluate(JsonDocument.Parse(SmokeReport()).RootElement, SmokeLog(), 4242, true, false, new(SmokeScratch, SmokeScratch with { BackupExists = true })), "SCRATCH_DWG_MUTATED");
    }

    // Harness defect after the D-3 host smoke: events.jsonl may still be held by a closing handle after the PID exits.
    private static readonly V35LogReadOptions FastRetry = new(MaxAttempts: 4, InitialDelayMilliseconds: 20, MaxDelayMilliseconds: 40);

    private static string TempDirectory()
    {
        string dir = Path.Combine(Path.GetTempPath(), "i52-harness-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static FileStream ExclusiveLock(string path) => new(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

    private static V35ProcessRun CleanRun(bool interactive = false) => new(4242, DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddSeconds(30), 0, true,
        TerminatedByControlPlane: interactive, ProcessGone: true, ExitedWithinPostFinishDeadline: !interactive, InteractiveStateObserved: interactive,
        InteractiveState: interactive ? "modal window over a disabled owner; top-level windows: Afx:'AutoCAD' disabled | #32770:'AutoCAD'" : null);

    private static (V35ProbeLaunch Launch, ScratchDrawingIntegrity Scratch, string Dir) ProbeFixture(string repo)
    {
        string dir = TempDirectory();
        foreach (string f in new[] { V35ProbeLaunch.NativeFile, V35ProbeLaunch.PayloadFile }) File.WriteAllText(Path.Combine(dir, f), "x");
        string dwg = Path.Combine(dir, "scratch.dwg");
        File.WriteAllText(dwg, "dwg");
        V35ProbeLaunch launch = V35ProbeLaunch.Create(Authority(repo), Path.Combine(dir, "acad-missing.exe"), Path.Combine(dir, V35ProbeLaunch.NativeFile), dwg, Path.Combine(dir, "out"), "02N", null);
        Directory.CreateDirectory(launch.OutputRoot);
        ScratchDrawingState state = ScratchDrawingState.Capture(dwg);
        return (launch, new ScratchDrawingIntegrity(state, state), dir);
    }

    private static void LogReadTransientLock(string repo)
    {
        string dir = TempDirectory();
        try
        {
            string path = Path.Combine(dir, "events.jsonl");
            File.WriteAllText(path, "{\"a\":1}\n{\"b\":2}\n");
            FileStream held = ExclusiveLock(path);
            var release = Task.Run(async () => { await Task.Delay(300); held.Dispose(); });
            V35LogRead read = V35LogReader.Read(path, new V35LogReadOptions(MaxAttempts: 10, InitialDelayMilliseconds: 50, MaxDelayMilliseconds: 400));
            release.GetAwaiter().GetResult();
            Require(read.Readable && read.Attempts > 1 && read.Lines.Count == 2 && read.Error is null, $"transient lock: readable={read.Readable} attempts={read.Attempts} error={read.Error}");
            // The shared reader also reads while another writer keeps the file open with read sharing (as R-NATIVE-ARX now does).
            using (new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                Require(V35LogReader.Read(path, FastRetry) is { Readable: true, Attempts: 1 }, "read while a sharing writer holds the log");
        }
        finally { Directory.Delete(dir, true); }
    }

    private static void LogReadPermanentLockFailsClosed(string repo)
    {
        string path = Pass02N().Write();
        try
        {
            Require(Evaluate(repo, Pass02N()).Result == "PASS", "baseline");
            using (ExclusiveLock(path))
            {
                V35LogRead read = V35LogReader.Read(path, FastRetry);
                Require(!read.Readable && read.Exists && read.Attempts == FastRetry.MaxAttempts && read.Error!.Contains("unreadable after 4 attempts", StringComparison.Ordinal), "permanent lock reported: " + read.Error);
                V35Authority a = Authority(repo);
                var evidence = V35RunEvidence.Load(path, new V35ProcessEvidence(4242, true, false, false, true), new V35ScratchEvidence(true, true, false), FastRetry);
                Require(evidence.Records.Count == 0 && evidence.ParseErrors.Any(e => e.Contains("unreadable", StringComparison.Ordinal)), "unreadable log is an evidence error");
                V35Result r = new V35ResultEngine(a).Evaluate(a.ByProbe["02N"], evidence);
                Require(r.Result == "UNKNOWN" && !r.EvidenceComplete, "unreadable log gave " + r.Result);
                RequireSmokeFailure(V35SmokeEvaluator.Evaluate(JsonDocument.Parse(SmokeReport()).RootElement, [], 4242, true, false, new(SmokeScratch, SmokeScratch), logError: read.Error), "EVENT_LOG_UNREADABLE");
            }
            V35LogRead missing = V35LogReader.Read(path + ".missing", FastRetry);
            Require(!missing.Exists && !missing.Readable && missing.Attempts == 1, "missing log is not retried");
        }
        finally { File.Delete(path); }
    }

    // process-run.json is written before events.jsonl is read: with the log locked for good, the probe result is UNKNOWN,
    // the smoke FAILs, and both keep the exact process evidence.
    private static void ProcessEvidenceSurvivesLogFailure(string repo)
    {
        (V35ProbeLaunch launch, ScratchDrawingIntegrity scratch, string dir) = ProbeFixture(repo);
        try
        {
            File.WriteAllText(launch.EventLog, File.ReadAllText(Pass02N().Write()));
            V35Result result;
            using (ExclusiveLock(launch.EventLog))
                result = V35ProbeHarness.CompleteAsync(Authority(repo), launch, CleanRun(), scratch, FastRetry).GetAwaiter().GetResult();
            Require(result.Result == "UNKNOWN", "locked log gave " + result.Result);
            JsonElement process = JsonDocument.Parse(File.ReadAllText(Path.Combine(launch.OutputRoot, V35RunArtifacts.ProcessRunFile))).RootElement.GetProperty("process");
            Require(process.GetProperty("ExitCode").GetInt32() == 0 && !process.GetProperty("TerminatedByControlPlane").GetBoolean() && process.GetProperty("ProcessGone").GetBoolean()
                && process.GetProperty("ExitedWithinPostFinishDeadline").GetBoolean() && !process.GetProperty("InteractiveStateObserved").GetBoolean(), "probe process evidence persisted");
            JsonElement manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(launch.OutputRoot, "probe-result.json"))).RootElement;
            Require(!manifest.GetProperty("logRead").GetProperty("Readable").GetBoolean() && manifest.GetProperty("result").GetProperty("Result").GetString() == "UNKNOWN", "probe result records the unreadable log");

            string smokeDir = Path.Combine(dir, "smoke");
            Directory.CreateDirectory(smokeDir);
            foreach (string f in new[] { V35ProbeLaunch.NativeFile, V35ProbeLaunch.PayloadFile, V35ProbeLaunch.ManagedFile }) File.WriteAllText(Path.Combine(smokeDir, f), "x");
            V35SmokeLaunch smoke = V35SmokeLaunch.Create("acad-missing.exe", Path.Combine(smokeDir, V35ProbeLaunch.NativeFile), Path.Combine(dir, "scratch.dwg"), Path.Combine(smokeDir, "out"), null);
            Directory.CreateDirectory(smoke.OutputRoot);
            File.WriteAllText(smoke.ReportPath, SmokeReport());
            File.WriteAllText(smoke.EventLog, "{}\n");
            V35SmokeVerdict verdict;
            using (ExclusiveLock(smoke.EventLog))
                verdict = V35SmokeCompletion.CompleteAsync(smoke, CleanRun(), scratch, FastRetry).GetAwaiter().GetResult();
            Require(verdict.Result == "FAIL" && verdict.Failures.Contains("EVENT_LOG_UNREADABLE"), "locked smoke log: " + string.Join(",", verdict.Failures));
            Require(File.Exists(Path.Combine(smoke.OutputRoot, V35RunArtifacts.ProcessRunFile)) && File.Exists(Path.Combine(smoke.OutputRoot, "r3-smoke-result.json")), "smoke evidence persisted");

            // Order in both completion paths: the process evidence is written before anything reads the report or the log,
            // so even an unexpected read or parse failure cannot lose it.
            string source = File.ReadAllText(Path.Combine(repo, "eng", "research", "I52Ctda", "control-plane", "V35", "V35ProbeRun.cs"));
            foreach (string completion in new[] { "public static async Task<V35Result> CompleteAsync(", "public static async Task<V35SmokeVerdict> CompleteAsync(" })
            {
                int at = source.IndexOf(completion, StringComparison.Ordinal);
                string body = source[at..source.IndexOf("\n    }", at, StringComparison.Ordinal)];
                int persisted = body.IndexOf("V35RunArtifacts.WriteProcessRun(", StringComparison.Ordinal);
                int firstRead = new[] { body.IndexOf("V35RunEvidence.Load(", StringComparison.Ordinal), body.IndexOf("V35LogReader.Read(", StringComparison.Ordinal) }.Where(x => x >= 0).Min();
                Require(persisted > 0 && persisted < firstRead, completion + " persists the process evidence before reading");
            }
            string harness = File.ReadAllText(Path.Combine(repo, "eng", "research", "I52Ctda", "harness", "V35SmokeCommand.cs"));
            Require(!harness.Contains("File.ReadAllText", StringComparison.Ordinal) && !harness.Contains("File.ReadLines", StringComparison.Ordinal) && harness.Contains("V35SmokeCompletion.CompleteAsync(", StringComparison.Ordinal),
                "smoke-v35 reads nothing itself");
        }
        finally { Directory.Delete(dir, true); }
    }

    // An interactive state observed by the runner reaches process-run.json and probe-result.json intact, and the row is UNKNOWN.
    private static void InteractiveEvidencePreserved(string repo)
    {
        (V35ProbeLaunch launch, ScratchDrawingIntegrity scratch, string dir) = ProbeFixture(repo);
        try
        {
            File.WriteAllText(launch.EventLog, File.ReadAllText(Pass02N().Write()));
            V35Result result = V35ProbeHarness.CompleteAsync(Authority(repo), launch, CleanRun(interactive: true), scratch, FastRetry).GetAwaiter().GetResult();
            Require(result.Result == "UNKNOWN" && result.EvidenceGaps.Any(g => g.Contains("interactive modal state", StringComparison.Ordinal)), "interactive run gave " + result.Result);
            foreach (string file in new[] { V35RunArtifacts.ProcessRunFile, "probe-result.json" })
            {
                JsonElement process = JsonDocument.Parse(File.ReadAllText(Path.Combine(launch.OutputRoot, file))).RootElement.GetProperty("process");
                Require(process.GetProperty("InteractiveStateObserved").GetBoolean() && process.GetProperty("TerminatedByControlPlane").GetBoolean()
                    && process.GetProperty("InteractiveState").GetString()!.Contains("#32770:'AutoCAD'", StringComparison.Ordinal), file + " keeps the interactive-state details");
            }
        }
        finally { Directory.Delete(dir, true); }
    }
}