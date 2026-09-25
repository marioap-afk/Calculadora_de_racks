using I52Ctda.ControlPlane;

string repo = args.Length == 1 ? Path.GetFullPath(args[0]) : throw new ArgumentException("Repository path required.");
var tests = new (string Name, Action Run)[]
{
    ("registry completeness", RegistryCompleteness), ("traceability", Traceability),
    ("logger ordering", LoggerOrdering), ("result aggregation", ResultAggregation),
    ("cleanup pass guard", CleanupPassGuard), ("cleanup completion", CleanupCompletion),
    ("tuple blocked fields", TupleBlockedFields), ("fixture identity", FixtureIdentity),
    ("scheduler descriptors", SchedulerDescriptors), ("support action mapping", SupportActionMapping),
    ("reentrancy guard", Reentrancy), ("static native source", StaticNative),
    ("process controller", ProcessController), ("header authority signatures", HeaderAuthority),
    ("smoke pass control", SmokePassControl), ("smoke rejects unbound fixture", SmokeRejectsUnboundFixture),
    ("smoke rejects fixture count", SmokeRejectsFixtureCount), ("smoke rejects incomplete cleanup", SmokeRejectsIncompleteCleanup),
    ("smoke rejects unavailable logger", SmokeRejectsUnavailableLogger), ("logger fallback path", LoggerFallbackPath),
    ("smoke command identity", SmokeCommandIdentity), ("smoke event log checks", SmokeEventLogChecks),
    ("harness requires scratch dwg", HarnessRequiresScratchDrawing), ("harness process fence", HarnessProcessFence),
    ("smoke rejects governed dispatch", SmokeRejectsGovernedDispatch),
    ("quit script discards changes", QuitScriptDiscardsChanges), ("smoke rejects mutated scratch dwg", SmokeRejectsMutatedScratchDrawing),
    ("smoke rejects new scratch bak", SmokeRejectsNewScratchBackup), ("smoke passes with untouched scratch", SmokePassesWithUntouchedScratch),
    ("scratch integrity capture", ScratchIntegrityCapture), ("native source matches canonical build", NativeSourceMatchesCanonicalBuild)
};
int passed = 0;
foreach ((string name, Action run) in tests) { run(); Console.WriteLine($"PASS {name}"); passed++; }
Console.WriteLine($"TOTAL {passed}/{tests.Length}");
return;

ContractCatalog Catalog() => ContractCatalog.Load(repo);
void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
void RegistryCompleteness() { var c = Catalog(); c.AssertComplete(); Require(c.Implementations.Count == 100, "registry"); }
void Traceability() { var c = Catalog(); Require(c.Probes.All(p => c.Implementations.ContainsKey(p.ProbeId)), "missing trace"); }
void LoggerOrdering() { var log = new TotalOrderLogger(); Parallel.For(0, 100, _ => log.Append(Template())); long[] seq = log.Snapshot().Select(x => x.Sequence).ToArray(); Require(seq.SequenceEqual(Enumerable.Range(1, 100).Select(x => (long)x)), "non-monotonic"); }
void ResultAggregation() { Require(EvidenceSerializer.Aggregate([Evidence(ResultState.Pass, true), Evidence(ResultState.Pass, true)]) == ResultState.Pass, "pass aggregate"); Require(EvidenceSerializer.Aggregate([Evidence(ResultState.Pass, true), Evidence(ResultState.Fail, true)]) == ResultState.Fail, "fail aggregate"); Require(EvidenceSerializer.Aggregate([Evidence(ResultState.Pass, true), Evidence(ResultState.Unknown, false)]) == ResultState.Unknown, "unknown aggregate"); }
void CleanupPassGuard() { string temp = Path.GetTempFileName(); try { EvidenceSerializer.Write(temp, Evidence(ResultState.Pass, false)); throw new InvalidDataException("blind PASS"); } catch (InvalidDataException e) when (e.Message.Contains("cleanup", StringComparison.OrdinalIgnoreCase)) { } finally { File.Delete(temp); } }
void CleanupCompletion() { var cleanup = new CleanupStateMachine(); foreach (CleanupObligation item in Enum.GetValues<CleanupObligation>()) cleanup.Satisfy(item); Require(cleanup.Snapshot().Complete, "cleanup incomplete"); }
void TupleBlockedFields() { Require(ExactTupleCollector.Collect(repo, null, "TEST").Status.Contains("BLOCKED", StringComparison.Ordinal), "tuple status"); }
void FixtureIdentity() { FixtureSpecification.Validate(); Require(FixtureSpecification.Objects.Count == 7, "fixture count"); }
void SchedulerDescriptors() { var c = Catalog(); Require(c.Implementations.Values.Where(i => i.SchedulerId is not null).Select(i => i.SchedulerId).Distinct().Count() == 3, "scheduler count"); }
void SupportActionMapping() { var c = Catalog(); Require(c.Implementations.Values.SelectMany(i => i.SupportActions).Distinct().Count() == 7, "support actions"); }
void Reentrancy() { var g = new ReentrancyGuard("RG-DB-MOD", "F-TRIGGER-MOD"); Require(g.TryArm() && !g.TryArm(), "guard"); g.Disarm(); Require(g.TryArm(), "reset"); }
void StaticNative() { Require(StaticNativeValidator.Validate(repo, Catalog()).IsValid, "native static validation"); }
void ProcessController() { ChildProcessResult r = ScratchProcessController.RunAsync(Environment.GetEnvironmentVariable("COMSPEC")!, "/c exit 0", repo, TimeSpan.FromSeconds(10)).GetAwaiter().GetResult(); Require(!r.TimedOut && r.ExitCode == 0, "child process"); }
void HeaderAuthority() { Require(HeaderAuthorityValidator.Validate(repo, @"D:\Downloads\CDROM1").IsValid, "header signature mismatch"); }
TotalOrderRecord Template() => new(0, 0, Environment.ProcessId, Environment.CurrentManagedThreadId, "D", "DB", "P", "E", "", "T", 1, "LOCK", "CTX", "SP", "EP", [], "STATE", "CLEAN");
EvidenceRecord Evidence(ResultState state, bool complete) => new(new("P", 1, "T"), null, state, FailureClassification.None, "E", "A", [], new(new Dictionary<string, bool> { ["x"] = complete }, complete), new Dictionary<string, string>());

// Smoke fixtures mirror the native I52CTDA_SMOKE report and event-log formats written by I52CtdaNative.cpp/I52CtdaRuntime.cpp.
NativeSmokeReport SmokeReport() => NativeSmokeReport.Parse("""
{
  "schemaVersion": 2,
  "instrument": "I52CtdaNative",
  "stage": "A_SMOKE",
  "commandIdentity": "I52CTDA_SMOKE",
  "completionToken": "I52CTDA_SMOKE_COMPLETE",
  "result": "PASS",
  "failures": [],
  "timestampUnixMicros": 1,
  "processId": 4242,
  "threadId": 7,
  "applicationContext": false,
  "documentIdentity": "0000000000000001",
  "databaseIdentity": "0000000000000002",
  "documentName": "D:\\scratch\\smoke.dwg",
  "eventLog": "D:\\scratch\\native-smoke.json.events.jsonl",
  "loggerReady": true,
  "sequenceFirst": 1,
  "sequenceLast": 3,
  "fixture": {
    "materializeStatus": "0",
    "registerFixtureObjectsStatus": "0",
    "bound": true,
    "declaredIdentities": 7,
    "resolvedIdentities": 7,
    "snapshot": "F-TRIGGER-MOD|2A|PRESENT|AcDbPoint|RESOLVED\n"
  },
  "cleanup": {
    "ownedTransactions": 0,
    "ownedLocks": 0,
    "queuedWork": 0,
    "activeGuards": 0,
    "fixtureReactorsAttached": false,
    "activeTransactionsObserved": 0,
    "cleanupComplete": true,
    "globalReactors": "RETAINED_UNTIL_ARX_UNLOAD"
  },
  "governedProbesDispatched": 0
}
""");
string LogLine(long sequence, string eventId, string command, int pid = 4242) =>
    $$"""{"sequence":{{sequence}},"timestampUnixMicros":1,"processId":{{pid}},"threadId":7,"eventId":"{{eventId}}","notifier":"","transactionDepth":-1,"documentIdentity":"1","databaseIdentity":"2","applicationContext":false,"commandIdentity":"{{command}}","detail":""}""";
string[] SmokeLog() => [LogLine(1, "MARK-SMOKE-BEGIN", "I52CTDA_SMOKE"), LogLine(2, "N-DB-APPEND", "I52CTDA_SMOKE"), LogLine(3, "MARK-SMOKE-END", "I52CTDA_SMOKE"), LogLine(4, "MARK-UNLOAD-REACTORS", "")];
SmokeProcessRun SmokeRun(bool gone = true, bool timedOut = false) => new(4242, DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, 0, timedOut, gone, "acad.exe", "args");
ScratchDrawingState ScratchState(string sha, bool backup = false, string? backupSha = null) => new(@"D:\scratch\smoke.dwg", true, 31665, sha, DateTimeOffset.UnixEpoch, @"D:\scratch\smoke.bak", backup, backup ? 31665 : null, backup ? backupSha ?? sha : null);
ScratchDrawingIntegrity Untouched() => new(ScratchState("A"), ScratchState("A"));
SmokeVerdict Judge(NativeSmokeReport report) => SmokeEvaluator.Evaluate(report, EventLogSummary.Read(SmokeLog()), SmokeRun(), Untouched());
void RequireFailure(SmokeVerdict verdict, string failure) { Require(!verdict.Pass, $"blind PASS without {failure}"); Require(verdict.Failures.Contains(failure), $"missing failure {failure}: {string.Join(',', verdict.Failures)}"); }

void SmokePassControl() { SmokeVerdict v = Judge(SmokeReport()); Require(v.Pass && v.Failures.Count == 0, "pass control: " + string.Join(',', v.Failures)); }
void SmokeRejectsUnboundFixture() => RequireFailure(Judge(SmokeReport() with { FixtureBound = false }), "FIXTURE_NOT_BOUND");
void SmokeRejectsFixtureCount()
{
    RequireFailure(Judge(SmokeReport() with { ResolvedIdentities = 6 }), "FIXTURE_IDENTITY_COUNT");
    RequireFailure(Judge(SmokeReport() with { DeclaredIdentities = 8, ResolvedIdentities = 8 }), "FIXTURE_IDENTITY_COUNT");
}
void SmokeRejectsIncompleteCleanup()
{
    RequireFailure(Judge(SmokeReport() with { CleanupComplete = false }), "CLEANUP_INCOMPLETE");
    RequireFailure(Judge(SmokeReport() with { FixtureReactorsAttached = true }), "FIXTURE_REACTORS_ATTACHED");
    RequireFailure(Judge(SmokeReport() with { OwnedTransactions = 1 }), "CLEANUP_OBLIGATION_OPEN");
    RequireFailure(Judge(SmokeReport() with { QueuedWork = 1 }), "CLEANUP_OBLIGATION_OPEN");
    RequireFailure(Judge(SmokeReport() with { ActiveGuards = 1 }), "CLEANUP_OBLIGATION_OPEN");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog().Take(3)), SmokeRun(), Untouched()), "EVENT_LOG_UNLOAD_MISSING");
}
void SmokeRejectsUnavailableLogger()
{
    RequireFailure(Judge(SmokeReport() with { LoggerReady = false }), "LOGGER_UNAVAILABLE");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), null, SmokeRun(), Untouched()), "EVENT_LOG_MISSING");
    Require(SmokeContract.EventLogFor(null, null) is null && SmokeContract.EventLogFor(" ", "") is null, "no logger path must be unavailable");
}
void LoggerFallbackPath()
{
    Require(SmokeContract.EventLogFor(@"D:\s\native-smoke.json", null) == @"D:\s\native-smoke.json.events.jsonl", "derived path");
    Require(SmokeContract.EventLogFor(@"D:\s\native-smoke.json", @"D:\x\explicit.jsonl") == @"D:\x\explicit.jsonl", "explicit path honored");
    string header = File.ReadAllText(Path.Combine(repo, "eng/research/I52Ctda/native/I52CtdaRuntime.h"));
    Require(header.Contains($"kEventLogSuffix = L\"{SmokeContract.EventLogSuffix}\"", StringComparison.Ordinal), "native and harness derivation rules differ");
}
void SmokeCommandIdentity()
{
    NativeSmokeReport report = SmokeReport();
    Require(report.CommandIdentity == "I52CTDA_SMOKE" && report.CompletionToken == "I52CTDA_SMOKE_COMPLETE", "parsed identity");
    RequireFailure(Judge(report with { CommandIdentity = null }), "COMMAND_IDENTITY_MISSING");
    RequireFailure(Judge(report with { CompletionToken = null }), "COMPLETION_TOKEN_MISSING");
    string[] anonymous = [LogLine(1, "MARK-SMOKE-BEGIN", ""), LogLine(2, "MARK-SMOKE-END", "I52CTDA_SMOKE"), LogLine(3, "MARK-UNLOAD-REACTORS", "")];
    RequireFailure(SmokeEvaluator.Evaluate(report, EventLogSummary.Read(anonymous), SmokeRun(), Untouched()), "EVENT_LOG_COMMAND_IDENTITY");
}
void SmokeEventLogChecks()
{
    string[] gap = [LogLine(1, "MARK-SMOKE-BEGIN", "I52CTDA_SMOKE"), LogLine(3, "MARK-SMOKE-END", "I52CTDA_SMOKE"), LogLine(4, "MARK-UNLOAD-REACTORS", "")];
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(gap), SmokeRun(), Untouched()), "EVENT_LOG_SEQUENCE");
    string[] foreign = [LogLine(1, "MARK-SMOKE-BEGIN", "I52CTDA_SMOKE"), LogLine(2, "MARK-SMOKE-END", "I52CTDA_SMOKE", 99), LogLine(3, "MARK-UNLOAD-REACTORS", "")];
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(foreign), SmokeRun(), Untouched()), "EVENT_LOG_PROCESS");
    RequireFailure(Judge(SmokeReport() with { ProcessId = 99 }), "REPORT_FROM_OTHER_PROCESS");
    RequireFailure(Judge(SmokeReport() with { Result = "FAIL", Failures = ["X"] }), "NATIVE_RESULT_NOT_PASS");
}
void HarnessRequiresScratchDrawing()
{
    string root = Directory.CreateTempSubdirectory("i52-smoke-").FullName;
    try
    {
        string helper = Path.Combine(root, "I52CtdaNative.arx"); File.WriteAllText(helper, "x");
        string text = Path.Combine(root, "scratch.txt"); File.WriteAllText(text, "x");
        string drawing = Path.Combine(root, "scratch.dwg"); File.WriteAllText(drawing, "x");
        void Rejects<TException>(string? dwg) where TException : Exception
        {
            try { SmokeLaunchPlan.Create("acad.exe", helper, dwg, root, null); }
            catch (TException) { return; }
            throw new InvalidDataException($"scratch DWG '{dwg}' accepted");
        }
        Rejects<ArgumentException>(null);
        Rejects<ArgumentException>("");
        Rejects<ArgumentException>(text);
        Rejects<FileNotFoundException>(Path.Combine(root, "missing.dwg"));
        SmokeLaunchPlan plan = SmokeLaunchPlan.Create("acad.exe", helper, drawing, root, null);
        Require(plan.Arguments.StartsWith($"\"{drawing}\" ", StringComparison.Ordinal), "scratch DWG is not the opened drawing");
        Require(plan.EventLogPath == plan.ReportPath + SmokeContract.EventLogSuffix, "harness event log path");
        string[] commands = plan.Script.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.StartsWith("(arx", StringComparison.Ordinal) ? l[..l.IndexOf(' ')] : l).ToArray();
        Require(commands.SequenceEqual(["_.FILEDIA", "0", "(arxload", "I52CTDA_SMOKE", "(arxunload", "_.QUIT", "_Y"]), "smoke script must run only the smoke command: " + string.Join('|', commands));
    }
    finally { Directory.Delete(root, true); }
}
void HarnessProcessFence()
{
    string shell = Environment.GetEnvironmentVariable("COMSPEC")!;
    SmokeProcessRun ok = SmokeProcessRunner.RunAsync(shell, "/c exit 0", repo, new Dictionary<string, string?>(), TimeSpan.FromSeconds(20)).GetAwaiter().GetResult();
    Require(ok.ProcessId > 0 && ok.ExitCode == 0 && !ok.TimedOut && ok.ProcessGone && ok.ExitedAtUtc is not null, "completed child fence");
    SmokeProcessRun hung = SmokeProcessRunner.RunAsync(shell, "/c ping -n 30 127.0.0.1 >nul", repo, new Dictionary<string, string?>(), TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
    Require(hung.TimedOut && hung.ProcessGone, "timed-out child must be killed and verified gone");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(timedOut: true), Untouched()), "PROCESS_TIMEOUT");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(gone: false), Untouched()), "PROCESS_STILL_PRESENT");
    using var self = System.Diagnostics.Process.GetCurrentProcess();
    Require(!ProcessExitVerifier.IsGone(self.Id, new DateTimeOffset(self.StartTime.ToUniversalTime(), TimeSpan.Zero)), "live process reported gone");
}
void SmokeRejectsGovernedDispatch()
{
    RequireFailure(Judge(SmokeReport() with { GovernedProbesDispatched = 1 }), "GOVERNED_PROBE_DISPATCHED");
    var catalog = Catalog();
    Require(catalog.Probes.Count == 100 && catalog.Implementations.Count == 100, "governed universe unchanged");
}
void QuitScriptDiscardsChanges()
{
    string root = Directory.CreateTempSubdirectory("i52-quit-").FullName;
    try
    {
        string helper = Path.Combine(root, "I52CtdaNative.arx"); File.WriteAllText(helper, "x");
        string drawing = Path.Combine(root, "scratch.dwg"); File.WriteAllText(drawing, "x");
        string[] lines = SmokeLaunchPlan.Create("acad.exe", helper, drawing, root, null).Script.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int quit = Array.IndexOf(lines, "_.QUIT");
        Require(quit == lines.Length - 2, "QUIT must be the last command");
        Require(lines[quit + 1] == SmokeContract.DiscardAllChangesAnswer && SmokeContract.DiscardAllChangesAnswer == "_Y", "QUIT must answer Yes to 'discard all changes'");
        Require(!lines.Contains("_N") && !lines.Any(l => l.Contains("SAVE", StringComparison.OrdinalIgnoreCase)), "script must never keep or save changes");
        Require(lines.Count(l => l == SmokeContract.CommandIdentity) == 1 && !lines.Any(l => l.StartsWith("I52CTDA_", StringComparison.Ordinal) && l != SmokeContract.CommandIdentity), "only the smoke command; no governed ProbeId");
    }
    finally { Directory.Delete(root, true); }
}
void SmokeRejectsMutatedScratchDrawing()
{
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(), new(ScratchState("A"), ScratchState("B"))), "SCRATCH_DWG_MUTATED");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(), new(ScratchState("A"), ScratchState("A") with { Bytes = 31570 })), "SCRATCH_DWG_MUTATED");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(), new(ScratchState("A"), ScratchState("A") with { Exists = false, Sha256 = null, Bytes = null })), "SCRATCH_DWG_MUTATED");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(), null), "SCRATCH_INTEGRITY_UNVERIFIED");
}
void SmokeRejectsNewScratchBackup()
{
    // The first host run: DWG rewritten and the original kept as .bak.
    SmokeVerdict v = SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(), new(ScratchState("A"), ScratchState("B", backup: true, backupSha: "A")));
    RequireFailure(v, "SCRATCH_DWG_MUTATED");
    RequireFailure(v, "SCRATCH_BAK_CREATED");
    // A .bak alone, even with the DWG hash unchanged, is still a mutation of the scratch generation.
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(), new(ScratchState("A"), ScratchState("A", backup: true))), "SCRATCH_BAK_CREATED");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(), new(ScratchState("A", backup: true, backupSha: "X"), ScratchState("A", backup: true, backupSha: "Y"))), "SCRATCH_BAK_CREATED");
}
void SmokePassesWithUntouchedScratch()
{
    SmokeVerdict v = SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(), Untouched());
    Require(v.Pass && v.Failures.Count == 0, "untouched scratch must pass: " + string.Join(',', v.Failures));
    Require(SmokeReport().GovernedProbesDispatched == 0, "control report dispatches no governed ProbeId");
}
void ScratchIntegrityCapture()
{
    string root = Directory.CreateTempSubdirectory("i52-scratch-").FullName;
    try
    {
        string drawing = Path.Combine(root, "scratch.dwg"); File.WriteAllBytes(drawing, [1, 2, 3]);
        ScratchDrawingState before = ScratchDrawingState.Capture(drawing);
        Require(before.Exists && before.Bytes == 3 && !before.BackupExists && before.BackupPath == Path.Combine(root, "scratch.bak"), "capture");
        Require(new ScratchDrawingIntegrity(before, ScratchDrawingState.Capture(drawing)) is { Unchanged: true, BackupCreated: false }, "unchanged file");
        File.Copy(drawing, Path.Combine(root, "scratch.bak"));
        File.WriteAllBytes(drawing, [1, 2, 4]);
        var after = new ScratchDrawingIntegrity(before, ScratchDrawingState.Capture(drawing));
        Require(!after.Unchanged && after.BackupCreated, "rewritten file with backup");
    }
    finally { Directory.Delete(root, true); }
}
void NativeSourceMatchesCanonicalBuild()
{
    // SHA-256 of the LF-normalized native sources of build source bf738b1d, which produced ARX
    // B7016A3469F0BA9786691493F051A015EAB94E0DC5A631990E1C2ED97531B22A. A managed-only fix must leave them identical.
    var canonical = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["I52CtdaFixture.cpp"] = "34833A9BD0F331A4F1290610048913704401ACD9D0073A09CDDE58B3FB414075",
        ["I52CtdaFixture.h"] = "E455190867F06A698AF88EF0A9E355DD12BC6E18C902493C73CAB955118273C0",
        ["I52CtdaNative.cpp"] = "66D4195C0F93F7C35A608959559CE532C550CC1D0D6E72126BD9113771AF0BB0",
        ["I52CtdaNative.vcxproj"] = "C0762B295E91736EC22EFBD63087E357FA74EEDCFF796DAE5A4D99BA0B2592FF",
        ["I52CtdaRuntime.cpp"] = "21D86B4B2010262DC1FB345D51E9E3BDB8049FF06781F0BFBC55DB6FBE3DB9F9",
        ["I52CtdaRuntime.h"] = "514148E07FE3E1C84ECF10EBC0CADB38A89F1E3CBF5CD8E3876FA0003BB020D4",
        ["ProbeDispatchTable.inc"] = "4F8ACD9F06F35ED59B9D385058FFEDAA19951F4D5A5683A78089F18EFF285A0A"
    };
    string native = Path.Combine(repo, "eng", "research", "I52Ctda", "native");
    var actual = Directory.GetFiles(native).ToDictionary(f => Path.GetFileName(f)!, f =>
    {
        byte[] bytes = File.ReadAllBytes(f);
        var normalized = new List<byte>(bytes.Length);
        for (int i = 0; i < bytes.Length; i++) if (!(bytes[i] == 13 && i + 1 < bytes.Length && bytes[i + 1] == 10)) normalized.Add(bytes[i]);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(normalized.ToArray()));
    }, StringComparer.Ordinal);
    Require(actual.Count == canonical.Count && canonical.All(kv => actual.TryGetValue(kv.Key, out string? h) && h == kv.Value),
        "native source differs from canonical build source bf738b1d: " + string.Join(',', canonical.Keys.Where(k => !actual.TryGetValue(k, out string? h) || h != canonical[k])));
}
