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
    ("scratch integrity capture", ScratchIntegrityCapture), ("native source matches canonical build", NativeSourceMatchesCanonicalBuild),
    ("sm precondition accepts SM-0", SmPreconditionAcceptsSm0), ("sm carrier anomalies are unknown", SmCarrierAnomaliesAreUnknown),
    ("sm precondition requires C absent and B live", SmPreconditionRequiresCAbsent), ("ver-sm classification", VerSmClassification),
    ("smoke requires sm-link binding", SmokeRequiresSmLinkBinding), ("snapshot shape", SnapshotShapeChecks),
    ("sm binding source guards", SmBindingSourceGuards), ("cleanup requires link carrier removal", CleanupRequiresLinkCarrierRemoval),
    ("fixture spec unchanged", FixtureSpecUnchanged)
};
int passed = 0;
foreach ((string name, Action run) in tests) { run(); Console.WriteLine($"PASS {name}"); passed++; }
foreach ((string name, Action<string> run) in R3Tests.All) { run(repo); Console.WriteLine($"PASS {name}"); passed++; }
Console.WriteLine($"TOTAL {passed}/{tests.Length + R3Tests.All.Length}");
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

// Smoke fixtures mirror the legacy V34 I52CTDA_SMOKE report and event-log formats (V34 harness smoke contract, historical).
NativeSmokeReport SmokeReport() => NativeSmokeReport.Parse("""
{
  "schemaVersion": 3,
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
    "smLinkBinding": "RESOLVED",
    "smPrecondition": {"link": {"keyPresent": true, "identityMatches": true, "erased": false, "isXrecord": true, "resbufCount": 1, "isText": true, "carriersFound": 1, "value": "HFV30:LINK:A,B"}, "anchorALive": true, "bRead": true, "bLive": true, "foreignReferenceInserts": 0, "cBound": false, "cFound": false, "cLive": false, "cAtCreationPosition": false, "aEvidence": "matrix=1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;layer=2B", "cEvidence": ""},
    "snapshot": "F-TRIGGER-MOD|2E|PRESENT|AcDbPoint|RESOLVED\nF-TRIGGER-ERASE-DB|2F|PRESENT|AcDbPoint|RESOLVED\nF-TRIGGER-ERASE-OBJ|30|PRESENT|AcDbPoint|RESOLVED\nF-XR|33|PRESENT|STATE-S-0|RESOLVED\nF-REF-B|32|PRESENT|STATE-M-0|RESOLVED\nF-REF-A|31|PRESENT|STATE-SM-0 sibling A|RESOLVED\nF-REF-C|NULL|ABSENT|declared absent (not created)|RESOLVED\nBINDING:SM-LINK|34|PRESENT|STATE-SM-0|RESOLVED\n"
  },
  "cleanup": {
    "ownedTransactions": 0,
    "ownedLocks": 0,
    "queuedWork": 0,
    "activeGuards": 0,
    "fixtureReactorsAttached": false,
    "activeTransactionsObserved": 0,
    "linkCarrierRemoved": true,
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
    // R3: the V35 native log path is only the explicit I52_CTDA_EVENT_LOG of RUN-ENV-01 (no derived fallback), and every
    // V35 launch plan sets it; the derivation rule above remains the legacy V34 smoke harness contract.
    string log = File.ReadAllText(Path.Combine(repo, "eng/research/I52Ctda/native/I52CtdaLog.cpp"));
    Require(log.Contains("path_ = environment(L\"I52_CTDA_EVENT_LOG\");", StringComparison.Ordinal) && !log.Contains("kEventLogSuffix", StringComparison.Ordinal), "V35 native log path");
    string run = File.ReadAllText(Path.Combine(repo, "eng/research/I52Ctda/control-plane/V35/V35ProbeRun.cs"));
    Require(System.Text.RegularExpressions.Regex.Matches(run, @"\[""I52_CTDA_EVENT_LOG""\] = EventLog").Count == 2, "V35 launch plans must set I52_CTDA_EVENT_LOG");
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
    // SHA-256 of the LF-normalized native sources of the V34 binding rebuild (decisions section 164); the pre-V35 canonical
    // ARX (7F9C9C05...9ED421) was built from them at 8bf3733f. R3 supersedes those sources in the working tree, so the
    // binding is now checked against git history: the historical canonical build stays exactly reproducible.
    var canonical = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["I52CtdaFixture.cpp"] = "0C014E27C7AC1C6810EFC88A03B34A69E3B781059F1760246C8CFBF94328628D",
        ["I52CtdaFixture.h"] = "3E690E43ED3673ED920A37D20CF9F5F33AEB8593E66D638CE3E74AEB998FDE59",
        ["I52CtdaNative.cpp"] = "648A4FB838B94E100077B316042874DDF1E1E6EE5F44FF218E9C5A7ACB6540DA",
        ["I52CtdaNative.vcxproj"] = "C0762B295E91736EC22EFBD63087E357FA74EEDCFF796DAE5A4D99BA0B2592FF",
        ["I52CtdaRuntime.cpp"] = "D194DF58FB17A4678E53DE0725C4B6257FCFB40CB9B538D03451F6D4807B0BB6",
        ["I52CtdaRuntime.h"] = "C09F5D1011CF3CC087691DE383B4967EA5B36B2ADCDE23994DF85698ABEC38AB",
        ["ProbeDispatchTable.inc"] = "4F8ACD9F06F35ED59B9D385058FFEDAA19951F4D5A5683A78089F18EFF285A0A"
    };
    const string canonicalBuildSource = "8bf3733f07ebaa2724a1c72335f100acad8905e4";
    string Historical(string file)
    {
        var git = new System.Diagnostics.ProcessStartInfo("git", $"-C \"{repo}\" show {canonicalBuildSource}:eng/research/I52Ctda/native/{file}") { RedirectStandardOutput = true, UseShellExecute = false };
        using var process = System.Diagnostics.Process.Start(git)!;
        using var memory = new MemoryStream();
        process.StandardOutput.BaseStream.CopyTo(memory);
        process.WaitForExit();
        byte[] bytes = memory.ToArray();
        var normalized = new List<byte>(bytes.Length);
        for (int i = 0; i < bytes.Length; i++) if (!(bytes[i] == 13 && i + 1 < bytes.Length && bytes[i + 1] == 10)) normalized.Add(bytes[i]);
        return process.ExitCode == 0 ? Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(normalized.ToArray())) : "MISSING";
    }
    Require(canonical.All(kv => Historical(kv.Key) == kv.Value), "historical canonical build source changed: " + string.Join(',', canonical.Keys.Where(k => Historical(k) != canonical[k])));
    // R3 superseded the V34 runtime and dispatch table in the working tree.
    string native = Path.Combine(repo, "eng", "research", "I52Ctda", "native");
    Require(!File.Exists(Path.Combine(native, "I52CtdaRuntime.cpp")) && !File.Exists(Path.Combine(native, "ProbeDispatchTable.inc")), "superseded V34 runtime still present");
}

// V34 binding clarifications (decisions section 164): SM-LINK storage and OPEN-SM-B.
SmFacts Sm0() => new(SmLinkFacts.Valid(SmBindingContract.StateSm0), true, true, true, 0, false, false, false, false, "", "");
SmFacts Sm1() => new(SmLinkFacts.Valid(SmBindingContract.StateSm1), true, false, false, 1, true, true, true, true, "", "");
void SmPreconditionAcceptsSm0() => Require(SmRules.ClassifyPrecondition(Sm0()) == SmRules.Ok, "SM-0 precondition");
void SmCarrierAnomaliesAreUnknown()
{
    SmLinkFacts valid = SmLinkFacts.Valid(SmBindingContract.StateSm0);
    var anomalies = new (SmLinkFacts Link, string Code)[]
    {
        (valid with { KeyPresent = false }, "UNKNOWN:LINK-MISSING"),
        (valid with { Erased = true }, "UNKNOWN:LINK-ERASED"),
        (valid with { IsXrecord = false }, "UNKNOWN:LINK-WRONG-TYPE"),
        (valid with { ResbufCount = 2 }, "UNKNOWN:LINK-WRONG-TYPE"),
        (valid with { IsText = false }, "UNKNOWN:LINK-WRONG-TYPE"),
        (valid with { CarriersFound = 2 }, "UNKNOWN:LINK-DUPLICATE"),
        (valid with { IdentityMatches = false }, "UNKNOWN:LINK-IDENTITY-DRIFT"),
        (valid with { Value = SmBindingContract.StateSm1 }, "UNKNOWN:LINK-PRECONDITION-MISMATCH"),
        (valid with { Value = "HFV30:LINK:A,B " }, "UNKNOWN:LINK-PRECONDITION-MISMATCH")
    };
    foreach ((SmLinkFacts link, string code) in anomalies)
    {
        string actual = SmRules.ClassifyPrecondition(Sm0() with { Link = link });
        Require(actual == code, $"precondition {code} classified {actual}");
        RequireFailure(Judge(SmokeReport() with { SmPrecondition = Sm0() with { Link = link } }), "SM_PRECONDITION_" + code);
        if (code != "UNKNOWN:LINK-PRECONDITION-MISMATCH")
            Require(SmRules.ClassifyAfter(Sm1() with { Link = link with { Value = SmBindingContract.StateSm1 } }) == code, $"post-mutation {code} must stay UNKNOWN");
    }
}
void SmPreconditionRequiresCAbsent()
{
    Require(SmRules.ClassifyPrecondition(Sm0() with { CFound = true, ForeignReferenceInserts = 1 }) == "UNKNOWN:C-PRESENT", "C present");
    Require(SmRules.ClassifyPrecondition(Sm0() with { CBound = true }) == "UNKNOWN:C-PRESENT", "C bound");
    Require(SmRules.ClassifyPrecondition(Sm0() with { ForeignReferenceInserts = 1 }) == "UNKNOWN:C-PRESENT", "extra REF insert");
    Require(SmRules.ClassifyPrecondition(Sm0() with { BLive = false }) == "UNKNOWN:B-NOT-LIVE", "B not live");
    Require(SmRules.ClassifyPrecondition(Sm0() with { BRead = false }) == "UNKNOWN:B-NOT-LIVE", "B unread");
    Require(SmRules.ClassifyPrecondition(Sm0() with { AnchorALive = false }) == "UNKNOWN:A-NOT-LIVE", "A not live");
}
void VerSmClassification()
{
    Require(SmRules.ClassifyAfter(Sm1()) == SmRules.Ok, "valid SM-1");
    Require(SmRules.ClassifyAfter(Sm1() with { Link = SmLinkFacts.Valid(SmBindingContract.StateSm0) }) == "FAIL-SM:LINK-VALUE", "wrong bytes on valid structure is FAIL-SM");
    var anomalies = new (SmFacts Facts, string Code)[]
    {
        (Sm1() with { ForeignReferenceInserts = 0, CFound = false, CLive = false }, "UNKNOWN:C-NONE"),
        (Sm1() with { ForeignReferenceInserts = 2 }, "UNKNOWN:C-MULTIPLE"),
        (Sm1() with { CFound = false }, "UNKNOWN:C-IDENTITY-DRIFT"),
        (Sm1() with { CBound = false }, "UNKNOWN:C-IDENTITY-DRIFT"),
        (Sm1() with { CAtCreationPosition = false }, "UNKNOWN:C-IDENTITY-DRIFT"),
        (Sm1() with { CLive = false }, "UNKNOWN:C-NOT-LIVE"),
        (Sm1() with { AnchorALive = false }, "UNKNOWN:A-NOT-LIVE"),
        (Sm1() with { BRead = true, BLive = true }, "UNKNOWN:B-OPENED-AFTER-TRIGGER"),
        (Sm1() with { Link = SmLinkFacts.Valid(SmBindingContract.StateSm1) with { KeyPresent = false } }, "UNKNOWN:LINK-MISSING"),
        (Sm1() with { Link = SmLinkFacts.Valid(SmBindingContract.StateSm1) with { CarriersFound = 2 } }, "UNKNOWN:LINK-DUPLICATE"),
        (Sm1() with { Link = SmLinkFacts.Valid(SmBindingContract.StateSm1) with { IdentityMatches = false } }, "UNKNOWN:LINK-IDENTITY-DRIFT")
    };
    foreach ((SmFacts facts, string code) in anomalies)
    {
        string actual = SmRules.ClassifyAfter(facts);
        Require(actual == code, $"VER-SM {code} classified {actual}");
        Require(actual != SmRules.Ok, $"anomaly {code} reached OK");
        Require(SmRules.ClassifyAfter(facts with { Link = facts.Link with { Value = SmBindingContract.StateSm0 } }) != SmRules.Ok, $"anomaly {code} with wrong bytes reached OK");
    }
}
void SmokeRequiresSmLinkBinding()
{
    RequireFailure(Judge(SmokeReport() with { SmLinkBinding = "MISMATCH" }), "SM_LINK_BINDING_UNRESOLVED");
    RequireFailure(Judge(SmokeReport() with { SmLinkBinding = null }), "SM_LINK_BINDING_UNRESOLVED");
    RequireFailure(Judge(SmokeReport() with { SmPrecondition = null }), "SM_PRECONDITION_MISSING");
    RequireFailure(Judge(SmokeReport() with { LinkCarrierRemoved = false }), "LINK_CARRIER_NOT_REMOVED");
    RequireFailure(Judge(SmokeReport() with { SmPrecondition = Sm0() with { CFound = true, ForeignReferenceInserts = 1 } }), "SM_PRECONDITION_UNKNOWN:C-PRESENT");
}
void SnapshotShapeChecks()
{
    string good = SmokeReport().FixtureSnapshot;
    Require(SmRules.SnapshotShape(good) is null, "valid snapshot: " + SmRules.SnapshotShape(good));
    string binding = "BINDING:SM-LINK|34|PRESENT|STATE-SM-0|RESOLVED\n";
    RequireFailure(Judge(SmokeReport() with { FixtureSnapshot = good.Replace(binding, "F-SM-LINK|34|PRESENT|STATE-SM-0|RESOLVED\n") }), "SNAPSHOT_IDENTITY_LINES");
    RequireFailure(Judge(SmokeReport() with { FixtureSnapshot = good.Replace(binding, "") }), "SNAPSHOT_BINDING_LINES");
    RequireFailure(Judge(SmokeReport() with { FixtureSnapshot = good + binding }), "SNAPSHOT_BINDING_LINES");
    RequireFailure(Judge(SmokeReport() with { FixtureSnapshot = good.Replace("|STATE-SM-0|RESOLVED", "|STATE-SM-0|MISMATCH") }), "SNAPSHOT_BINDING_INVALID");
    RequireFailure(Judge(SmokeReport() with { FixtureSnapshot = good.Replace("F-REF-C|NULL|ABSENT|", "F-REF-C|35|PRESENT|") }), "SNAPSHOT_C_NOT_ABSENT");
    Require(FixtureSpecification.Objects.Count == 7 && FixtureSpecification.Objects.All(o => !o.Id.Contains("LINK", StringComparison.Ordinal)), "carrier counted as fixture identity");
}
void SmBindingSourceGuards()
{
    string native = Path.Combine(repo, "eng", "research", "I52Ctda", "native");
    string source = File.ReadAllText(Path.Combine(native, "I52CtdaFixture.cpp"));
    string header = File.ReadAllText(Path.Combine(native, "I52CtdaFixture.h"));
    Require(SmBindingGuard.Check(source).Count == 0, "current source: " + string.Join(';', SmBindingGuard.Check(source)));
    Require(SmBindingGuard.CheckHeader(header).Count == 0, "current header");
    string mixed = "Acad::ErrorStatus I52CtdaFixture::mutateMixed()";
    void Detects(string mutated, string violation)
    {
        IReadOnlyList<string> found = SmBindingGuard.Check(mutated);
        Require(found.Contains(violation), $"guard missed '{violation}': {string.Join(';', found)}");
    }
    Require(source.Contains(mixed + "\n{", StringComparison.Ordinal) || source.Contains(mixed + "\r\n{", StringComparison.Ordinal), "mutateMixed definition not found");
    string Inject(string statement) => System.Text.RegularExpressions.Regex.Replace(source, System.Text.RegularExpressions.Regex.Escape(mixed) + @"\r?\n\{", m => m.Value + "\n    " + statement);
    Detects(Inject("AcDbObjectPointer<AcDbBlockReference> b(ids_.materialReference, AcDb::kForWrite); b->erase(true);"), "MUT-SM erases an object");
    Detects(Inject("AcDbObjectPointer<AcDbBlockReference> b(ids_.materialReference, AcDb::kForRead);"), "MUT-SM references F-REF-B");
    Detects(Inject("AcDbObjectPointer<AcDbObject> c(ids_.siblingC, AcDb::kForWrite, true);"), "MUT-SM opens erased objects");
    Detects(Inject("mutateSemantic();"), "MUT-SM writes F-XR");
    Detects(Inject("AcDbObjectPointer<AcDbXrecord> x(ids_.semanticXrecord, AcDb::kForWrite);"), "MUT-SM writes F-XR");
    Detects(Inject("AcDbObjectId extra; appendReference(database, nullptr, nullptr, kSiblingCCreationPosition, ids_.referenceBlock, ids_.layerA, extra);"), "MUT-SM must append exactly one reference");
    Detects(source.Replace("    Acad::ErrorStatus status = mutateSemantic();", "    Acad::ErrorStatus status = mutateSemantic();\n    mutateSemantic();"), "MUT-ALL must run MUT-S, MUT-M and MUT-SM exactly once");
    Detects(source.Replace("    // F-REF-C is declared absent: it is not created here; MUT-SM appends it.",
        "    if ((status = appendReference(database, modelSpace, manager, kSiblingCCreationPosition, created.referenceBlock, created.layerA, created.siblingC)) != Acad::eOk) return status;"), "bootstrap precreates F-REF-C");
    Require(SmBindingGuard.CheckHeader(header.Replace("kDeclaredIdentities = 8;", "kDeclaredIdentities = 7;")).Count == 1, "identity count guard");
}
void CleanupRequiresLinkCarrierRemoval()
{
    var cleanup = new CleanupStateMachine();
    foreach (CleanupObligation item in Enum.GetValues<CleanupObligation>().Where(o => o != CleanupObligation.LinkCarrierRemoved)) cleanup.Satisfy(item);
    Require(!cleanup.Snapshot().Complete, "cleanup complete without LINK-CARRIER-REMOVED");
    cleanup.Satisfy(CleanupObligation.LinkCarrierRemoved);
    Require(cleanup.Snapshot().Complete, "cleanup incomplete after LINK-CARRIER-REMOVED");
    // R3: CLN-BASE verifies R-SM-LINK removal (LINK-CARRIER-REMOVED) as part of its success condition.
    string driver = File.ReadAllText(Path.Combine(repo, "eng", "research", "I52Ctda", "native", "I52CtdaDriver.cpp"));
    Require(System.Text.RegularExpressions.Regex.IsMatch(driver, @"const bool carrierRemoved = fixture_\.linkCarrierRemoved\(\);[\s\S]*ok = ok && removed == Acad::eOk && closed == Acad::eOk && carrierRemoved;"), "native CLN-BASE ignores LINK-CARRIER-REMOVED");
}
void FixtureSpecUnchanged()
{
    byte[] bytes = File.ReadAllBytes(Path.Combine(repo, "eng", "research", "I52Ctda", "fixture-v34.json"));
    var normalized = new List<byte>(bytes.Length);
    for (int i = 0; i < bytes.Length; i++) if (!(bytes[i] == 13 && i + 1 < bytes.Length && bytes[i + 1] == 10)) normalized.Add(bytes[i]);
    Require(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(normalized.ToArray())) == "11411BA8858A4CCA1AB638645FEC5D2C0AA3263C239BC9F0C8A10153AF6AD9FA", "fixture-v34.json changed");
}
