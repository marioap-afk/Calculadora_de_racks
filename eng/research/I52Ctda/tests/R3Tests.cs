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
                    ["PID"] = i.Pid, ["TID"] = 1, ["DocumentId"] = "0x1", ["DatabaseId"] = "0x2", ["CommandIdentity"] = "I52CTDA_PROBE", ["EventOrMarkerId"] = i.Event,
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
        .Add("CMD-FINISH", "STG-FINISH", 6, "phase=FLUSH");

    private static V35Result Evaluate(string repo, Log log, string probe = "02N", V35ProcessEvidence? process = null, long skip = -1)
    {
        V35Authority a = Authority(repo);
        string path = log.Write(skip);
        try
        {
            var evidence = V35RunEvidence.Load(path, process ?? new V35ProcessEvidence(4242, true, false, false, true), new V35ScratchEvidence(true, true, false));
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
        "{\"result\":\"PASS\",\"processId\":4242,\"governedProbesDispatched\":0,\"fixture\":{\"declared\":8}," + gate + "," + managed + "}";

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
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), [.. SmokeLog(), SmokeRecord(9, "STG-FIN-GATE", "R-NATIVE-ARX", "FIN-GATE-01", "phase=ISSUE;mode=FINISH-MODE-DRAIN")]), "SPONTANEOUS_FINISH_IN_LOG");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), [.. SmokeLog(), SmokeRecord(9, "STG-FINISH", "R-NATIVE-ARX", "CMD-FINISH", "phase=ENTRY")]), "SPONTANEOUS_FINISH_IN_LOG");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(gate: SmokeGate.Replace("\"tokensAccepted\":0", "\"tokensAccepted\":1")), SmokeLog()), "TOKEN_FABRICATED");
        RequireSmokeFailure(JudgeSmoke(SmokeReport(), [.. SmokeLog(), SmokeRecord(9, "STG-PROBE-CMD", "R-NATIVE-ARX", "TOK-EXEC-DONE", "status=ACCEPTED")]), "TOKEN_FABRICATED_IN_LOG");
        Require(JudgeSmoke(SmokeReport(), [.. SmokeLog(), SmokeRecord(9, "STG-PROBE-CMD", "R-NATIVE-ARX", "TOK-EXEC-DONE", "status=NOT-IN-ROW")]).Result == "PASS", "a rejected token is not fabricated");
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
}
