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
}

public sealed record V35ProcessRun(int ProcessId, DateTimeOffset StartedAtUtc, DateTimeOffset? ExitedAtUtc, int? ExitCode, bool FinishRecordSeen, bool TerminatedByControlPlane, bool ProcessGone, bool ExitedWithinPostFinishDeadline);

// External fence: FIN-GATE-01.externalDeadlineSeconds without a FINISH record, or postFinishDeadlineSeconds after it,
// terminates the exact PID (UNKNOWN).
public static class V35ProcessRunner
{
    public static async Task<V35ProcessRun> RunAsync(string executable, string arguments, string workingDirectory, IReadOnlyDictionary<string, string?> environment,
        string eventLog, TimeSpan externalDeadline, TimeSpan postFinishDeadline)
    {
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
            await Task.Delay(1000);
            if (finishSeen is null && FinishRecorded(eventLog)) finishSeen = DateTimeOffset.UtcNow;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if ((finishSeen is null && now - started > externalDeadline) || (finishSeen is not null && now - finishSeen > postFinishDeadline))
            {
                terminated = true;
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
        if (finishSeen is null && FinishRecorded(eventLog)) finishSeen = DateTimeOffset.UtcNow;
        DateTimeOffset exited = new(process.ExitTime.ToUniversalTime(), TimeSpan.Zero);
        bool withinDeadline = finishSeen is not null && !terminated;
        return new(pid, started, exited, process.ExitCode, finishSeen is not null, terminated, ProcessExitVerifier.IsGone(pid, started), withinDeadline);
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
        var evidence = V35RunEvidence.Load(launch.EventLog,
            new V35ProcessEvidence(run.ProcessId, run.ProcessGone, run.TerminatedByControlPlane, false, run.ExitedWithinPostFinishDeadline),
            new V35ScratchEvidence(true, scratch.Unchanged, scratch.BackupCreated));
        V35Result result = new V35ResultEngine(authority).Evaluate(authority.ByProbe[launch.ProbeId], evidence);
        var manifest = new
        {
            schemaVersion = 1,
            probeId = launch.ProbeId,
            freezePackageHash = V35Freeze.PackageHash,
            rowApprovalHash = authority.ByProbe[launch.ProbeId].RowApprovalHash,
            modules = new { native = V35Files.Sha256(launch.NativeHelper), payload = V35Files.Sha256(launch.PayloadArx), managed = launch.LoadsManaged ? V35Files.Sha256(launch.ManagedObserver) : null },
            acadExecutableSha256 = V35Files.Sha256(launch.AcadExecutable),
            process = run,
            scratch = new { before = scratch.Before, after = scratch.After, scratch.Unchanged, scratch.BackupCreated },
            eventLog = launch.EventLog,
            records = evidence.Records.Count,
            result,
        };
        await File.WriteAllTextAsync(Path.Combine(launch.OutputRoot, "probe-result.json"), JsonSerializer.Serialize(manifest, Json), new UTF8Encoding(false));
        return (0, result);
    }
}

// Zero-ProbeId R3 host smoke (next gate): modules load, shared sequencer, fence read ABI, fixture-v35 bootstrap 8/8,
// payload and managed lifecycle, cleanup, PID fence and an unchanged scratch DWG.
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

    public string Script => "_.FILEDIA\n0\n(arxload \"" + NativeHelper.Replace('\\', '/') + "\")\n_.NETLOAD\n" + ManagedObserver + "\nI52CTDA_SMOKE\n_.QUIT\n_Y\n";
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
    public static V35SmokeVerdict Evaluate(JsonElement? report, IReadOnlyList<V35LogRecord> records, int processId, bool processGone, bool timedOut, ScratchDrawingIntegrity scratch)
    {
        var failures = new List<string>();
        if (!scratch.Unchanged || scratch.BackupCreated) failures.Add("SCRATCH_DWG_MUTATED");
        if (timedOut) failures.Add("PROCESS_TIMEOUT");
        if (!processGone) failures.Add("PROCESS_STILL_PRESENT");
        if (report is not { } r) failures.Add("NATIVE_REPORT_MISSING");
        else
        {
            if (r.GetProperty("result").GetString() != "PASS") failures.Add("NATIVE_RESULT_NOT_PASS");
            if (r.GetProperty("processId").GetInt32() != processId) failures.Add("REPORT_FROM_OTHER_PROCESS");
            if (r.GetProperty("governedProbesDispatched").GetInt32() != 0) failures.Add("GOVERNED_PROBE_DISPATCHED");
            if (r.GetProperty("fixture").GetProperty("declared").GetInt32() != 8) failures.Add("FIXTURE_DECLARED_NOT_8");
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
        }
        return new(failures.Count == 0 ? "PASS" : "FAIL", failures);
    }
}
