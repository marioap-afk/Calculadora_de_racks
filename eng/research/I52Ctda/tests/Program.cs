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
    ("process controller", ProcessController), ("header authority signatures", HeaderAuthority)
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
