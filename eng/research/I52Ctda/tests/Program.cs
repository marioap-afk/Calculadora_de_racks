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
    ("smoke rejects governed dispatch", SmokeRejectsGovernedDispatch)
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
SmokeVerdict Judge(NativeSmokeReport report) => SmokeEvaluator.Evaluate(report, EventLogSummary.Read(SmokeLog()), SmokeRun());
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
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog().Take(3)), SmokeRun()), "EVENT_LOG_UNLOAD_MISSING");
}
void SmokeRejectsUnavailableLogger()
{
    RequireFailure(Judge(SmokeReport() with { LoggerReady = false }), "LOGGER_UNAVAILABLE");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), null, SmokeRun()), "EVENT_LOG_MISSING");
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
    RequireFailure(SmokeEvaluator.Evaluate(report, EventLogSummary.Read(anonymous), SmokeRun()), "EVENT_LOG_COMMAND_IDENTITY");
}
void SmokeEventLogChecks()
{
    string[] gap = [LogLine(1, "MARK-SMOKE-BEGIN", "I52CTDA_SMOKE"), LogLine(3, "MARK-SMOKE-END", "I52CTDA_SMOKE"), LogLine(4, "MARK-UNLOAD-REACTORS", "")];
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(gap), SmokeRun()), "EVENT_LOG_SEQUENCE");
    string[] foreign = [LogLine(1, "MARK-SMOKE-BEGIN", "I52CTDA_SMOKE"), LogLine(2, "MARK-SMOKE-END", "I52CTDA_SMOKE", 99), LogLine(3, "MARK-UNLOAD-REACTORS", "")];
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(foreign), SmokeRun()), "EVENT_LOG_PROCESS");
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
        Require(commands.SequenceEqual(["_.FILEDIA", "0", "(arxload", "I52CTDA_SMOKE", "(arxunload", "_.QUIT", "_N"]), "smoke script must run only the smoke command: " + string.Join('|', commands));
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
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(timedOut: true)), "PROCESS_TIMEOUT");
    RequireFailure(SmokeEvaluator.Evaluate(SmokeReport(), EventLogSummary.Read(SmokeLog()), SmokeRun(gone: false)), "PROCESS_STILL_PRESENT");
    using var self = System.Diagnostics.Process.GetCurrentProcess();
    Require(!ProcessExitVerifier.IsGone(self.Id, new DateTimeOffset(self.StartTime.ToUniversalTime(), TimeSpan.Zero)), "live process reported gone");
}
void SmokeRejectsGovernedDispatch()
{
    RequireFailure(Judge(SmokeReport() with { GovernedProbesDispatched = 1 }), "GOVERNED_PROBE_DISPATCHED");
    var catalog = Catalog();
    Require(catalog.Probes.Count == 100 && catalog.Implementations.Count == 100, "governed universe unchanged");
}
