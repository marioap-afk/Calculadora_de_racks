using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace I52Ctda.ControlPlane.V35;

// RUN-ENV-01 launch plan for exactly one ProbeId: a dedicated scratch AutoCAD 2025 process on a fresh scratch DWG,
// DRIVER-SCRIPT-01 as its only input and the exact R3 artifacts of one run directory. An unknown ProbeId is rejected
// here, before any process starts.
public sealed record V35ProbeLaunch(
    string ProbeId, string AcadExecutable, string RunDirectory, string NativeHelper, string PayloadArx, string ManagedObserver,
    string ScratchDrawing, string OutputRoot, string EventLog, string ScriptPath, string? Profile, bool LoadsManaged, string Script)
{
    public const string NativeFile = "I52CtdaNative.arx";
    public const string PayloadFile = "I52CtdaPayload.arx";
    public const string ManagedFile = "I52Ctda.ManagedObserver.dll";

    public static V35ProbeLaunch Create(V35Authority authority, string acadExecutable, string nativeHelper, string scratchDrawing, string outputRoot, string probeId, string? profile)
    {
        if (!authority.ByProbe.TryGetValue(probeId, out V35RowPlan? plan)) throw new ArgumentException($"Unknown ProbeId '{probeId}': rejected before launch.", nameof(probeId));
        string native = Path.GetFullPath(nativeHelper);
        if (!File.Exists(native) || !string.Equals(Path.GetFileName(native), NativeFile, StringComparison.OrdinalIgnoreCase)) throw new FileNotFoundException("R-NATIVE-ARX not found.", native);
        string runDirectory = Path.GetDirectoryName(native)!;
        string payload = Path.Combine(runDirectory, PayloadFile);
        string managed = Path.Combine(runDirectory, ManagedFile);
        if (!File.Exists(payload)) throw new FileNotFoundException("R-PAYLOAD-ARX must sit next to R-NATIVE-ARX (NL-ARX-PATH).", payload);
        bool loadsManaged = plan.ObserverRegistrationIds.Contains("RR-MANAGED-CMD");
        if (loadsManaged && !File.Exists(managed)) throw new FileNotFoundException("R-MANAGED-OBSERVER must sit next to R-NATIVE-ARX.", managed);
        string drawing = Path.GetFullPath(scratchDrawing);
        if (!File.Exists(drawing) || !string.Equals(Path.GetExtension(drawing), ".dwg", StringComparison.OrdinalIgnoreCase)) throw new FileNotFoundException("An explicit fresh scratch DWG is required.", drawing);
        string root = Path.GetFullPath(outputRoot);
        return new(probeId, acadExecutable, runDirectory, native, payload, managed, drawing, root, Path.Combine(root, "events.jsonl"), Path.Combine(root, "driver-script-01.scr"),
            profile, loadsManaged, DriverScript(plan, runDirectory, loadsManaged));
    }

    // DRIVER-SCRIPT-01: BOOT, PROBE and the script-issued trigger command only; never FINISH or QUIT.
    public static string DriverScript(V35RowPlan plan, string runDirectory, bool loadsManaged)
    {
        var script = new StringBuilder();
        script.Append("_.FILEDIA\n0\n");
        script.Append("(arxload \"").Append(Path.Combine(runDirectory, NativeFile).Replace('\\', '/')).Append("\")\n");
        if (loadsManaged) script.Append("_.NETLOAD\n").Append(Path.Combine(runDirectory, ManagedFile)).Append('\n');
        script.Append("I52CTDA_BOOT\n");
        script.Append("I52CTDA_PROBE ").Append(plan.ProbeId).Append('\n');
        if (plan.TriggerActionId == "TRG-RUN-FIXTURE-CMD") script.Append("I52CTDA_FIXTURE\n");
        if (plan.TriggerActionId == "TRG-CANCEL-CMDCTX") script.Append("HFV34_CANCEL\n");
        return script.ToString();
    }

    public string Arguments => $"\"{ScratchDrawing}\" /nologo /nossm" + (Profile is null ? "" : $" /p \"{Profile}\"") + $" /b \"{ScriptPath}\"";

    public IReadOnlyDictionary<string, string?> Environment => new Dictionary<string, string?>
    {
        ["I52_CTDA_PROBE_ID"] = ProbeId,
        ["I52_CTDA_EVENT_LOG"] = EventLog,
        ["I52_CTDA_SCRATCH_DWG"] = ScratchDrawing,
        ["I52_CTDA_PAYLOAD_SHA256"] = V35Files.Sha256(PayloadArx),
        ["I52_CTDA_OUTPUT"] = null,
    };
}

public static class V35Files
{
    public static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    public static string? Sha256OrNull(string path) => File.Exists(path) ? Sha256(path) : null;
}

// The process-run evidence (exit code, control-plane termination, PID gone, post-FINISH deadline, interactive state)
// and the scratch integrity are persisted as soon as the PID is gone, before events.jsonl is read, so they survive any
// failure to read or parse the log.
public static class V35RunArtifacts
{
    public const string ProcessRunFile = "process-run.json";
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static string WriteProcessRun(string outputRoot, V35ProcessRun run, ScratchDrawingIntegrity scratch)
    {
        string path = Path.Combine(outputRoot, ProcessRunFile);
        var evidence = new
        {
            schemaVersion = 1,
            persistedBefore = "events.jsonl read",
            process = run,
            scratch = new { scratch.Before, scratch.After, scratch.Unchanged, scratch.BackupCreated },
        };
        File.WriteAllText(path, JsonSerializer.Serialize(evidence, Json), new UTF8Encoding(false));
        return path;
    }
}

public sealed record V35ProcessRun(int ProcessId, DateTimeOffset StartedAtUtc, DateTimeOffset? ExitedAtUtc, int? ExitCode, bool FinishRecordSeen, bool TerminatedByControlPlane, bool ProcessGone, bool ExitedWithinPostFinishDeadline,
    bool InteractiveStateObserved = false, string? InteractiveState = null, DateTimeOffset? ObservedExitAtUtc = null);

// External fence: FIN-GATE-01.externalDeadlineSeconds without a FINISH record, or postFinishDeadlineSeconds after it,
// terminates the exact PID (UNKNOWN). D-3: an interactive (modal) state terminates it at once, before anyone can answer.
public static class V35ProcessRunner
{
    public static async Task<V35ProcessRun> RunAsync(string executable, string arguments, string workingDirectory, IReadOnlyDictionary<string, string?> environment,
        string eventLog, TimeSpan externalDeadline, TimeSpan postFinishDeadline, Func<Process, V35WindowSnapshot>? windows = null)
    {
        windows ??= V35Windows.Snapshot;
        var watch = new V35InteractiveWatch();
        var start = new ProcessStartInfo(executable, arguments) { UseShellExecute = false, WorkingDirectory = workingDirectory };
        foreach ((string key, string? value) in environment) { if (value is null) start.Environment.Remove(key); else start.Environment[key] = value; }
        using var process = new Process { StartInfo = start };
        process.Start();
        int pid = process.Id;
        DateTimeOffset started = new(process.StartTime.ToUniversalTime(), TimeSpan.Zero);
        DateTimeOffset? finishSeen = null;
        bool terminated = false;
        while (!process.HasExited)
        {
            await Task.Delay(250);
            if (finishSeen is null && FinishRecorded(eventLog)) finishSeen = DateTimeOffset.UtcNow;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            bool interactive = false;
            try { interactive = !process.HasExited && watch.Sample(windows(process)); } catch (InvalidOperationException) { }
            if (interactive || (finishSeen is null && now - started > externalDeadline) || (finishSeen is not null && now - finishSeen > postFinishDeadline))
            {
                terminated = true;
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
        if (finishSeen is null && FinishRecorded(eventLog)) finishSeen = DateTimeOffset.UtcNow;
        // H-1: the runner records when it observed the exit; Process.ExitTime can be the FILETIME-zero sentinel (1601-01-01),
        // which is reported as null. Neither value is a classification input.
        DateTimeOffset observedExit = DateTimeOffset.UtcNow;
        DateTimeOffset? exited = ExitTimeOrNull(process);
        bool withinDeadline = finishSeen is not null && !terminated;
        return new(pid, started, exited, process.ExitCode, finishSeen is not null, terminated, ProcessExitVerifier.IsGone(pid, started), withinDeadline,
            watch.Observed is not null, watch.Observed, observedExit);
    }

    public static DateTimeOffset? ExitTimeOrNull(Process process)
    {
        try { return ExitTimeOrNull(process.ExitTime); }
        catch (InvalidOperationException) { return null; }
    }

    public static DateTimeOffset? ExitTimeOrNull(DateTime exitTime)
    {
        DateTime utc = exitTime.Kind == DateTimeKind.Utc ? exitTime : exitTime.ToUniversalTime();
        return utc.Year <= 1601 ? null : new DateTimeOffset(utc, TimeSpan.Zero);
    }

    private static bool FinishRecorded(string eventLog)
    {
        try
        {
            if (!File.Exists(eventLog)) return false;
            using var stream = new FileStream(eventLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);
            string? line;
            while ((line = reader.ReadLine()) is not null)
                if (line.Contains("\"EventOrMarkerId\":\"CMD-FINISH\"", StringComparison.Ordinal)) return true;
            return false;
        }
        catch (IOException) { return false; }
    }
}

// Single-ProbeId harness path: launch, external fence, evidence collection and CONTROL-PLANE-RESULT-01.
public static class V35ProbeHarness
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };

    public static async Task<(int ExitCode, V35Result? Result)> RunAsync(V35Authority authority, V35ProbeLaunch launch)
    {
        Directory.CreateDirectory(launch.OutputRoot);
        if (File.Exists(launch.EventLog)) throw new InvalidOperationException("The output directory already holds an event log; use a fresh directory per run.");
        ScratchDrawingState before = ScratchDrawingState.Capture(launch.ScratchDrawing);
        if (before.BackupExists) throw new InvalidOperationException("A fresh scratch DWG is required: its .bak sibling already exists.");
        await File.WriteAllTextAsync(launch.ScriptPath, launch.Script, new UTF8Encoding(false));
        V35Entry gate = authority["FIN-GATE-01"];
        TimeSpan external = TimeSpan.FromSeconds(gate.Raw["externalDeadlineSeconds"]!.GetValue<long>());
        TimeSpan postFinish = TimeSpan.FromSeconds(gate.Raw["postFinishDeadlineSeconds"]!.GetValue<long>());
        V35ProcessRun run = await V35ProcessRunner.RunAsync(launch.AcadExecutable, launch.Arguments, launch.OutputRoot, launch.Environment, launch.EventLog, external, postFinish);
        // CLN-PROCESS-EXIT: scratch integrity only after the exact PID is gone.
        var scratch = new ScratchDrawingIntegrity(before, ScratchDrawingState.Capture(launch.ScratchDrawing));
        return (0, await CompleteAsync(authority, launch, run, scratch));
    }

    // Evidence first, then the log: process-run.json is written before events.jsonl is read (retry-safe); an unreadable
    // log leaves the evidence incomplete, so the result is UNKNOWN and never PASS.
    public static async Task<V35Result> CompleteAsync(V35Authority authority, V35ProbeLaunch launch, V35ProcessRun run, ScratchDrawingIntegrity scratch, V35LogReadOptions? options = null)
    {
        V35RunArtifacts.WriteProcessRun(launch.OutputRoot, run, scratch);
        var evidence = V35RunEvidence.Load(launch.EventLog,
            new V35ProcessEvidence(run.ProcessId, run.ProcessGone, run.TerminatedByControlPlane, false, run.ExitedWithinPostFinishDeadline, run.InteractiveStateObserved),
            new V35ScratchEvidence(true, scratch.Unchanged, scratch.BackupCreated), options);
        V35Result result = new V35ResultEngine(authority).Evaluate(authority.ByProbe[launch.ProbeId], evidence);
        var manifest = new
        {
            schemaVersion = 1,
            probeId = launch.ProbeId,
            freezePackageHash = V35Freeze.PackageHash,
            rowApprovalHash = authority.ByProbe[launch.ProbeId].RowApprovalHash,
            modules = new { native = V35Files.Sha256OrNull(launch.NativeHelper), payload = V35Files.Sha256OrNull(launch.PayloadArx), managed = launch.LoadsManaged ? V35Files.Sha256OrNull(launch.ManagedObserver) : null },
            acadExecutableSha256 = V35Files.Sha256OrNull(launch.AcadExecutable),
            processRunEvidence = V35RunArtifacts.ProcessRunFile,
            process = run,
            scratch = new { before = scratch.Before, after = scratch.After, scratch.Unchanged, scratch.BackupCreated },
            eventLog = launch.EventLog,
            logRead = new { evidence.LogRead?.Readable, evidence.LogRead?.Attempts, evidence.LogRead?.Error },
            records = evidence.Records.Count,
            result,
        };
        await File.WriteAllTextAsync(Path.Combine(launch.OutputRoot, "probe-result.json"), JsonSerializer.Serialize(manifest, Json), new UTF8Encoding(false));
        return result;
    }
}

// Zero-ProbeId R3 host smoke (next gate): modules load, shared sequencer, fence read ABI, fixture-v35 bootstrap 8/8,
// FIN-GATE-01 initialization without CMD-FINISH or tokens, payload and managed lifecycle, the managed fence read,
// cleanup, PID fence and an unchanged scratch DWG. The smoke classifies no ProbeId result.
public sealed record V35SmokeLaunch(string AcadExecutable, string RunDirectory, string NativeHelper, string PayloadArx, string ManagedObserver, string ScratchDrawing,
    string OutputRoot, string ReportPath, string EventLog, string ScriptPath, string? Profile)
{
    public const string SmokeProbeId = "I52CTDA_SMOKE";

    public static V35SmokeLaunch Create(string acadExecutable, string nativeHelper, string scratchDrawing, string outputRoot, string? profile)
    {
        string native = Path.GetFullPath(nativeHelper);
        if (!File.Exists(native)) throw new FileNotFoundException("R-NATIVE-ARX not found.", native);
        string run = Path.GetDirectoryName(native)!;
        string payload = Path.Combine(run, V35ProbeLaunch.PayloadFile), managed = Path.Combine(run, V35ProbeLaunch.ManagedFile);
        if (!File.Exists(payload) || !File.Exists(managed)) throw new FileNotFoundException("R-PAYLOAD-ARX and R-MANAGED-OBSERVER must sit next to R-NATIVE-ARX.");
        string drawing = Path.GetFullPath(scratchDrawing);
        if (!File.Exists(drawing)) throw new FileNotFoundException("An explicit fresh scratch DWG is required.", drawing);
        string root = Path.GetFullPath(outputRoot);
        return new(acadExecutable, run, native, payload, managed, drawing, root, Path.Combine(root, "r3-smoke.json"), Path.Combine(root, "events.jsonl"), Path.Combine(root, "r3-smoke.scr"), profile);
    }

    // D-3: no QUIT in the script; I52CTDA_SMOKE leaves through CMD-FINISH's automated QUIT-DISCARD after the script returned.
    public string Script => "_.FILEDIA\n0\n(arxload \"" + NativeHelper.Replace('\\', '/') + "\")\n_.NETLOAD\n" + ManagedObserver + "\nI52CTDA_SMOKE\n";
    public string Arguments => $"\"{ScratchDrawing}\" /nologo /nossm" + (Profile is null ? "" : $" /p \"{Profile}\"") + $" /b \"{ScriptPath}\"";

    public IReadOnlyDictionary<string, string?> Environment => new Dictionary<string, string?>
    {
        ["I52_CTDA_PROBE_ID"] = SmokeProbeId,
        ["I52_CTDA_EVENT_LOG"] = EventLog,
        ["I52_CTDA_OUTPUT"] = ReportPath,
        ["I52_CTDA_SCRATCH_DWG"] = ScratchDrawing,
        ["I52_CTDA_PAYLOAD_SHA256"] = V35Files.Sha256(PayloadArx),
    };
}

public sealed record V35SmokeVerdict(string Result, IReadOnlyList<string> Failures);

public static class V35SmokeEvaluator
{
    public static V35SmokeVerdict Evaluate(JsonElement? report, IReadOnlyList<V35LogRecord> records, int processId, bool processGone, bool timedOut, ScratchDrawingIntegrity scratch,
        bool interactiveStateObserved = false, string? logError = null)
    {
        var failures = new List<string>();
        if (logError is not null) failures.Add("EVENT_LOG_UNREADABLE");
        if (!scratch.Unchanged || scratch.BackupCreated) failures.Add("SCRATCH_DWG_MUTATED");
        if (timedOut) failures.Add("PROCESS_TIMEOUT");
        if (interactiveStateObserved) failures.Add("INTERACTIVE_STATE");
        if (!processGone) failures.Add("PROCESS_STILL_PRESENT");
        if (report is not { } r) failures.Add("NATIVE_REPORT_MISSING");
        else
        {
            if (r.GetProperty("result").GetString() != "PASS") failures.Add("NATIVE_RESULT_NOT_PASS");
            if (r.GetProperty("processId").GetInt32() != processId) failures.Add("REPORT_FROM_OTHER_PROCESS");
            if (r.GetProperty("governedProbesDispatched").GetInt32() != 0) failures.Add("GOVERNED_PROBE_DISPATCHED");
            if (r.GetProperty("fixture").GetProperty("declared").GetInt32() != 8) failures.Add("FIXTURE_DECLARED_NOT_8");
            JsonElement gate = r.TryGetProperty("finGate", out JsonElement g) ? g : default;
            if (!True(gate, "idleHook") || Num(gate, "timer") <= 0 || Num(gate, "stageDelivery") <= 0 || Num(gate, "registeredRecords") != 1) failures.Add("FIN_GATE_NOT_INITIALIZED");
            if (!False(gate, "finishIssued") || Num(gate, "finishRecords") != 0) failures.Add("SPONTANEOUS_FINISH");
            if (Num(gate, "tokensAccepted") != 0) failures.Add("TOKEN_FABRICATED");
            if (!True(gate, "hookRemoved") || !True(gate, "timerKilled")) failures.Add("FIN_GATE_TEARDOWN");
            JsonElement managed = r.TryGetProperty("managed", out JsonElement m) ? m : default;
            if (Num(managed, "fenceReadBefore") != 0 || Num(managed, "fenceReadAfter") != 1 || Num(managed, "fenceReadRecords") != 2) failures.Add("MANAGED_FENCE_READ_NOT_OBSERVED");
            if (!True(r.TryGetProperty("exit", out JsonElement x) ? x : default, "queued")) failures.Add("EXIT_NOT_QUEUED");
        }
        if (records.Count == 0) failures.Add("EVENT_LOG_MISSING");
        else
        {
            long[] seq = records.Select(x => x.Sequence).OrderBy(x => x).ToArray();
            if (seq[0] != 1 || seq.Zip(seq.Skip(1)).Any(p => p.Second != p.First + 1)) failures.Add("EVENT_LOG_SEQUENCE");
            if (records.Any(x => x.Pid != processId)) failures.Add("EVENT_LOG_PROCESS");
            if (!records.Any(x => x.ModuleId == "R-PAYLOAD-ARX")) failures.Add("PAYLOAD_NOT_IN_SHARED_LOG");
            if (!records.Any(x => x.ModuleId == "R-MANAGED-OBSERVER")) failures.Add("MANAGED_NOT_IN_SHARED_LOG");
            if (records.Any(x => x.Malformed)) failures.Add("EVENT_LOG_MALFORMED");
            if (records.Any(x => x.CommandIdentity is not ("I52CTDA_SMOKE" or "NONE"))) failures.Add("EVENT_LOG_COMMAND_IDENTITY");  // D-2
            // The log itself (not only the native self-report) shows the gate initialized, no FINISH, no token accepted
            // and the managed observer reading the fence unset, then set.
            if (!records.Any(x => x.ModuleId == "R-NATIVE-ARX" && x.EventOrMarkerId == "FIN-GATE-01" && x.StageId == "STG-FIN-GATE" && x.Is("phase", "REGISTERED")
                && x.Is("idleHook", "1") && x.P("timer") is not ("" or "0"))) failures.Add("FIN_GATE_NOT_IN_LOG");
            if (records.Any(x => x.EventOrMarkerId == "CMD-FINISH" && !x.P("phase").StartsWith("EXIT-", StringComparison.Ordinal) || x.EventOrMarkerId == "FIN-GATE-01" && x.Is("phase", "ISSUE")
                || x.EventOrMarkerId == "FINISH-FENCE-01" && x.Is("phase", "SET")))
                failures.Add("SPONTANEOUS_FINISH_IN_LOG");
            // D-3: the process left through the automated QUIT-DISCARD, queued over an unmodified drawing.
            var exits = records.Where(x => x.EventOrMarkerId == "CMD-FINISH" && x.P("phase").StartsWith("EXIT-", StringComparison.Ordinal)).ToArray();
            if (exits.Length != 1 || !exits[0].Is("phase", "EXIT-QUEUED") || !exits[0].Is("status", "0") || !exits[0].Is("dbmodAfter", "0")) failures.Add("EXIT_NOT_AUTOMATED");
            if (records.Any(x => x.EventOrMarkerId.StartsWith("TOK-", StringComparison.Ordinal) && x.Is("status", "ACCEPTED"))) failures.Add("TOKEN_FABRICATED_IN_LOG");
            string[] managedFence = records.Where(x => x.ModuleId == "R-MANAGED-OBSERVER" && x.EventOrMarkerId == "FINISH-FENCE-01" && x.Is("phase", "SMOKE-READ"))
                .OrderBy(x => x.Sequence).Select(x => x.P("fenceIsSet")).ToArray();
            if (!managedFence.SequenceEqual(["0", "1"])) failures.Add("MANAGED_FENCE_READ_NOT_IN_LOG");
        }
        return new(failures.Count == 0 ? "PASS" : "FAIL", failures);
    }

    private static bool True(JsonElement o, string name) => o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out JsonElement e) && e.ValueKind == JsonValueKind.True;
    private static bool False(JsonElement o, string name) => o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out JsonElement e) && e.ValueKind == JsonValueKind.False;
    private static long Num(JsonElement o, string name) => o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out JsonElement e) && e.ValueKind == JsonValueKind.Number ? e.GetInt64() : -1;
}

// Smoke completion: process-run evidence first, then the native report and events.jsonl (both retry-safe), then the
// verdict. An unreadable log or report fails the smoke; it is never silently ignored.
public static class V35SmokeCompletion
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static async Task<V35SmokeVerdict> CompleteAsync(V35SmokeLaunch launch, V35ProcessRun run, ScratchDrawingIntegrity scratch, V35LogReadOptions? options = null)
    {
        V35RunArtifacts.WriteProcessRun(launch.OutputRoot, run, scratch);
        JsonElement? report = null;
        V35LogRead reportRead = V35LogReader.Read(launch.ReportPath, options);
        if (reportRead.Readable)
            try { report = JsonDocument.Parse(string.Join('\n', reportRead.Lines)).RootElement.Clone(); } catch (JsonException) { }
        V35RunEvidence evidence = V35RunEvidence.Load(launch.EventLog, null, null, options);
        string? logError = evidence.LogRead is { Readable: false } read ? read.Error : evidence.ParseErrors.Count > 0 ? string.Join("; ", evidence.ParseErrors) : null;
        V35SmokeVerdict verdict = V35SmokeEvaluator.Evaluate(report, evidence.Records, run.ProcessId, run.ProcessGone, run.TerminatedByControlPlane, scratch, run.InteractiveStateObserved, logError);
        var result = new
        {
            schemaVersion = 2, stage = "R3_SMOKE", verdict.Result, verdict.Failures, run,
            processRunEvidence = V35RunArtifacts.ProcessRunFile,
            logRead = new { evidence.LogRead?.Readable, evidence.LogRead?.Attempts, evidence.LogRead?.Error, parseErrors = evidence.ParseErrors },
            reportRead = new { reportRead.Readable, reportRead.Attempts, reportRead.Error },
            modules = new { native = V35Files.Sha256OrNull(launch.NativeHelper), payload = V35Files.Sha256OrNull(launch.PayloadArx), managed = V35Files.Sha256OrNull(launch.ManagedObserver) },
            scratch = new { scratch.Before, scratch.After, scratch.Unchanged, scratch.BackupCreated }, records = evidence.Records.Count, governedProbesExecuted = 0,
        };
        await File.WriteAllTextAsync(Path.Combine(launch.OutputRoot, "r3-smoke-result.json"), JsonSerializer.Serialize(result, Json), new UTF8Encoding(false));
        return verdict;
    }
}
