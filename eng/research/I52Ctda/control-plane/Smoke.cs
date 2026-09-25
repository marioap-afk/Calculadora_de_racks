using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace I52Ctda.ControlPlane;

public static class SmokeContract
{
    public const string CommandIdentity = "I52CTDA_SMOKE";
    public const string CompletionToken = "I52CTDA_SMOKE_COMPLETE";
    public const int DeclaredFixtureIdentities = 7;

    // Same rule as I52CtdaRuntime::eventLogPath: an explicit I52_CTDA_EVENT_LOG wins, otherwise the path is derived
    // from I52_CTDA_OUTPUT; with neither the logger is unavailable and the smoke fails closed.
    public const string EventLogSuffix = ".events.jsonl";

    public static string? EventLogFor(string? output, string? explicitEventLog)
    {
        if (!string.IsNullOrWhiteSpace(explicitEventLog)) return explicitEventLog;
        if (string.IsNullOrWhiteSpace(output)) return null;
        return output + EventLogSuffix;
    }
}

public sealed record SmokeLaunchPlan(string AcadExecutable, string NativeHelper, string ScratchDrawing, string OutputRoot, string ReportPath, string EventLogPath, string ScriptPath, string? Profile)
{
    public static SmokeLaunchPlan Create(string acadExecutable, string nativeHelper, string? scratchDrawing, string outputRoot, string? profile)
    {
        if (string.IsNullOrWhiteSpace(scratchDrawing)) throw new ArgumentException("An explicit scratch DWG path is required.", nameof(scratchDrawing));
        string drawing = Path.GetFullPath(scratchDrawing);
        if (!string.Equals(Path.GetExtension(drawing), ".dwg", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The scratch drawing must be a .dwg file.", nameof(scratchDrawing));
        if (!File.Exists(drawing)) throw new FileNotFoundException("The scratch DWG does not exist.", drawing);
        string helper = Path.GetFullPath(nativeHelper);
        if (!File.Exists(helper)) throw new FileNotFoundException("The native helper does not exist.", helper);
        string root = Path.GetFullPath(outputRoot);
        string report = Path.Combine(root, "native-smoke.json");
        return new(acadExecutable, helper, drawing, root, report, SmokeContract.EventLogFor(report, null)!, Path.Combine(root, "native-smoke.scr"), profile);
    }

    // FILEDIA 0 keeps the run non-interactive; the helper is unloaded before QUIT so global reactors are removed
    // explicitly, and QUIT discards the scratch drawing without saving.
    public string Script => $"_.FILEDIA\n0\n(arxload \"{NativeHelper.Replace('\\', '/')}\")\n{SmokeContract.CommandIdentity}\n(arxunload \"{Path.GetFileName(NativeHelper)}\")\n_.QUIT\n_N\n";

    public string Arguments => $"\"{ScratchDrawing}\" /nologo /nossm" + (Profile is null ? "" : $" /p \"{Profile}\"") + $" /b \"{ScriptPath}\"";
}

public sealed record SmokeProcessRun(int ProcessId, DateTimeOffset StartedAtUtc, DateTimeOffset? ExitedAtUtc, int? ExitCode, bool TimedOut, bool ProcessGone, string Executable, string Arguments);

public static class SmokeProcessRunner
{
    public static async Task<SmokeProcessRun> RunAsync(string executable, string arguments, string workingDirectory, IReadOnlyDictionary<string, string?> environment, TimeSpan timeout)
    {
        var start = new ProcessStartInfo(executable, arguments) { UseShellExecute = false, WorkingDirectory = workingDirectory };
        foreach ((string key, string? value) in environment)
        {
            if (value is null) start.Environment.Remove(key); else start.Environment[key] = value;
        }
        using var process = new Process { StartInfo = start };
        process.Start();
        int pid = process.Id;
        DateTimeOffset started = new(process.StartTime.ToUniversalTime(), TimeSpan.Zero);
        bool timedOut = false;
        using (var cts = new CancellationTokenSource(timeout))
        {
            try { await process.WaitForExitAsync(cts.Token); }
            catch (OperationCanceledException)
            {
                timedOut = true;
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
        DateTimeOffset? exited = process.HasExited ? new DateTimeOffset(process.ExitTime.ToUniversalTime(), TimeSpan.Zero) : null;
        int? exitCode = process.HasExited ? process.ExitCode : null;
        return new(pid, started, exited, exitCode, timedOut, ProcessExitVerifier.IsGone(pid, started), executable, arguments);
    }
}

public static class ProcessExitVerifier
{
    // A PID counts as gone when no process has it, or when the PID was reused by a process with a different start time.
    public static bool IsGone(int processId, DateTimeOffset startedAtUtc)
    {
        try
        {
            using Process candidate = Process.GetProcessById(processId);
            if (candidate.HasExited) return true;
            return Math.Abs((candidate.StartTime.ToUniversalTime() - startedAtUtc.UtcDateTime).TotalSeconds) > 1;
        }
        catch (ArgumentException) { return true; }
        catch (InvalidOperationException) { return true; }
    }
}

public sealed record NativeSmokeReport(
    string? CommandIdentity, string? CompletionToken, string? Result, IReadOnlyList<string> Failures, int ProcessId,
    bool LoggerReady, string? EventLog, long SequenceFirst, long SequenceLast, bool FixtureBound, int DeclaredIdentities,
    int ResolvedIdentities, string FixtureSnapshot, bool CleanupComplete, bool FixtureReactorsAttached, int OwnedTransactions,
    int OwnedLocks, int QueuedWork, int ActiveGuards, int GovernedProbesDispatched)
{
    public static NativeSmokeReport Parse(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        JsonElement fixture = root.GetProperty("fixture");
        JsonElement cleanup = root.GetProperty("cleanup");
        return new(
            root.GetProperty("commandIdentity").GetString(), root.GetProperty("completionToken").GetString(), root.GetProperty("result").GetString(),
            root.GetProperty("failures").EnumerateArray().Select(x => x.GetString() ?? "").ToArray(), root.GetProperty("processId").GetInt32(),
            root.GetProperty("loggerReady").GetBoolean(), root.GetProperty("eventLog").GetString(), root.GetProperty("sequenceFirst").GetInt64(),
            root.GetProperty("sequenceLast").GetInt64(), fixture.GetProperty("bound").GetBoolean(), fixture.GetProperty("declaredIdentities").GetInt32(),
            fixture.GetProperty("resolvedIdentities").GetInt32(), fixture.GetProperty("snapshot").GetString() ?? "",
            cleanup.GetProperty("cleanupComplete").GetBoolean(), cleanup.GetProperty("fixtureReactorsAttached").GetBoolean(),
            cleanup.GetProperty("ownedTransactions").GetInt32(), cleanup.GetProperty("ownedLocks").GetInt32(), cleanup.GetProperty("queuedWork").GetInt32(),
            cleanup.GetProperty("activeGuards").GetInt32(), root.GetProperty("governedProbesDispatched").GetInt32());
    }

    public string SnapshotSha256 => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(FixtureSnapshot)));
}

public sealed record EventLogSummary(int Records, bool SequenceMonotonic, bool SingleProcess, int? ProcessId, bool HasSmokeBegin, bool HasSmokeEnd, bool HasUnload, bool SmokeRecordsCarryCommandIdentity)
{
    public static EventLogSummary Read(IEnumerable<string> lines)
    {
        var records = lines.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => JsonDocument.Parse(l).RootElement.Clone()).ToArray();
        long[] sequences = records.Select(r => r.GetProperty("sequence").GetInt64()).ToArray();
        int[] pids = records.Select(r => r.GetProperty("processId").GetInt32()).Distinct().ToArray();
        string Event(JsonElement r) => r.GetProperty("eventId").GetString() ?? "";
        var smoke = records.Where(r => Event(r).StartsWith("MARK-SMOKE-", StringComparison.Ordinal)).ToArray();
        return new(
            records.Length,
            sequences.Length > 0 && sequences.Zip(sequences.Skip(1)).All(p => p.Second == p.First + 1),
            pids.Length == 1, pids.Length == 1 ? pids[0] : null,
            smoke.Any(r => Event(r) == "MARK-SMOKE-BEGIN"), smoke.Any(r => Event(r) == "MARK-SMOKE-END"),
            records.Any(r => Event(r) == "MARK-UNLOAD-REACTORS"),
            smoke.Length > 0 && smoke.All(r => r.GetProperty("commandIdentity").GetString() == SmokeContract.CommandIdentity));
    }
}

public sealed record SmokeVerdict(string Result, IReadOnlyList<string> Failures)
{
    public bool Pass => Result == "PASS";
}

public static class SmokeEvaluator
{
    // PASS only when the native report, the ordered event log and the external process fence all agree.
    public static SmokeVerdict Evaluate(NativeSmokeReport? report, EventLogSummary? log, SmokeProcessRun run)
    {
        var failures = new List<string>();
        if (run.TimedOut) failures.Add("PROCESS_TIMEOUT");
        if (!run.ProcessGone) failures.Add("PROCESS_STILL_PRESENT");
        if (report is null) failures.Add("NATIVE_REPORT_MISSING");
        else
        {
            if (report.CommandIdentity != SmokeContract.CommandIdentity) failures.Add("COMMAND_IDENTITY_MISSING");
            if (report.CompletionToken != SmokeContract.CompletionToken) failures.Add("COMPLETION_TOKEN_MISSING");
            if (report.ProcessId != run.ProcessId) failures.Add("REPORT_FROM_OTHER_PROCESS");
            if (report.Result != "PASS" || report.Failures.Count != 0) failures.Add("NATIVE_RESULT_NOT_PASS");
            if (!report.LoggerReady) failures.Add("LOGGER_UNAVAILABLE");
            if (!report.FixtureBound) failures.Add("FIXTURE_NOT_BOUND");
            if (report.DeclaredIdentities != SmokeContract.DeclaredFixtureIdentities || report.ResolvedIdentities != SmokeContract.DeclaredFixtureIdentities) failures.Add("FIXTURE_IDENTITY_COUNT");
            if (!report.CleanupComplete) failures.Add("CLEANUP_INCOMPLETE");
            if (report.FixtureReactorsAttached) failures.Add("FIXTURE_REACTORS_ATTACHED");
            if (report.OwnedTransactions != 0 || report.OwnedLocks != 0 || report.QueuedWork != 0 || report.ActiveGuards != 0) failures.Add("CLEANUP_OBLIGATION_OPEN");
            if (report.GovernedProbesDispatched != 0) failures.Add("GOVERNED_PROBE_DISPATCHED");
        }
        if (log is null) failures.Add("EVENT_LOG_MISSING");
        else
        {
            if (!log.SequenceMonotonic) failures.Add("EVENT_LOG_SEQUENCE");
            if (!log.SingleProcess || log.ProcessId != run.ProcessId) failures.Add("EVENT_LOG_PROCESS");
            if (!log.HasSmokeBegin || !log.HasSmokeEnd) failures.Add("EVENT_LOG_SMOKE_MARKERS");
            if (!log.SmokeRecordsCarryCommandIdentity) failures.Add("EVENT_LOG_COMMAND_IDENTITY");
            if (!log.HasUnload) failures.Add("EVENT_LOG_UNLOAD_MISSING");
        }
        return new(failures.Count == 0 ? "PASS" : "FAIL", failures);
    }
}
