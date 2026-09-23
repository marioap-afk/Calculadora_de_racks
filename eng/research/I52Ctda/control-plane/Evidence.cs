using System.Text.Json;

namespace I52Ctda.ControlPlane;

public sealed record AttemptIdentity(string ProbeId, int Attempt, string TupleHash);
public sealed record ProcessIdentity(int ProcessId, DateTimeOffset StartedAtUtc, string ExecutableSha256, string CommandLine);
public sealed record CleanupRecord(IReadOnlyDictionary<string, bool> Obligations, bool Complete);
public sealed record EvidenceRecord(
    AttemptIdentity Attempt,
    ProcessIdentity? Process,
    ResultState Result,
    FailureClassification Classification,
    string Expected,
    string Actual,
    IReadOnlyList<TotalOrderRecord> Events,
    CleanupRecord Cleanup,
    IReadOnlyDictionary<string, string> Artifacts);

public sealed record TotalOrderRecord(
    long Sequence,
    long TimestampUtcTicks,
    int ProcessId,
    int ThreadId,
    string Document,
    string Database,
    string ProbeId,
    string EventId,
    string SchedulerId,
    string TransactionId,
    int TransactionDepth,
    string LockState,
    string Context,
    string SchedulePoint,
    string ExecutionPoint,
    IReadOnlyList<string> Targets,
    string State,
    string CleanupState);

public sealed class TotalOrderLogger
{
    private long sequence;
    private readonly List<TotalOrderRecord> records = [];
    private readonly object gate = new();

    public TotalOrderRecord Append(TotalOrderRecord template)
    {
        long next = Interlocked.Increment(ref sequence);
        var item = template with { Sequence = next, TimestampUtcTicks = DateTimeOffset.UtcNow.UtcTicks };
        lock (gate) records.Add(item);
        return item;
    }

    public IReadOnlyList<TotalOrderRecord> Snapshot()
    {
        lock (gate) return records.OrderBy(r => r.Sequence).ToArray();
    }

    public async Task WriteJsonLinesAsync(string path, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        await using var stream = File.Create(path);
        await using var writer = new StreamWriter(stream);
        foreach (TotalOrderRecord item in Snapshot())
            await writer.WriteLineAsync(JsonSerializer.Serialize(item).AsMemory(), cancellationToken);
    }
}

public static class EvidenceSerializer
{
    public static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public static void Write(string path, EvidenceRecord evidence)
    {
        if (evidence.Result == ResultState.Pass && !evidence.Cleanup.Complete)
            throw new InvalidDataException("PASS is impossible before cleanup completes.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(evidence, Options));
    }

    public static ResultState Aggregate(IEnumerable<EvidenceRecord> attempts)
    {
        EvidenceRecord[] all = attempts.ToArray();
        if (all.Length == 0 || all.Any(a => a.Result == ResultState.Unknown)) return ResultState.Unknown;
        if (all.Any(a => a.Result == ResultState.Fail)) return ResultState.Fail;
        return ResultState.Pass;
    }
}
