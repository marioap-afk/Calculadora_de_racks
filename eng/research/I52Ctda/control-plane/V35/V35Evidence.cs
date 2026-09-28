using System.Text;
using System.Text.Json;

namespace I52Ctda.ControlPlane.V35;

// One LOG-RECORD-01 record of the single LOG-SEQ-01 log, as written by R-NATIVE-ARX for every module.
public sealed record V35LogRecord(
    long Sequence, string ProbeId, string StageId, long DeliveryId, string DriverOrSchedulerId, string ModuleId, int Pid, int Tid,
    string DocumentId, string DatabaseId, string CommandIdentity, string EventOrMarkerId, IReadOnlyDictionary<string, string> Payload,
    long TimestampUnixMicros, bool Malformed)
{
    public string P(string key) => Payload.TryGetValue(key, out string? v) ? v : "";
    public bool Is(string key, string value) => Payload.TryGetValue(key, out string? v) && v == value;

    public static V35LogRecord Parse(string line)
    {
        using JsonDocument doc = JsonDocument.Parse(line);
        JsonElement r = doc.RootElement;
        string S(string name) => r.TryGetProperty(name, out JsonElement e) && e.ValueKind == JsonValueKind.String ? e.GetString() ?? "" : "";
        long L(string name) => r.TryGetProperty(name, out JsonElement e) && e.ValueKind == JsonValueKind.Number ? e.GetInt64() : -1;
        var payload = new Dictionary<string, string>(StringComparer.Ordinal);
        if (r.TryGetProperty("Payload", out JsonElement p) && p.ValueKind == JsonValueKind.Object)
            foreach (JsonProperty prop in p.EnumerateObject()) payload[prop.Name] = prop.Value.GetString() ?? "";
        bool malformed = r.TryGetProperty("Malformed", out JsonElement m) && m.ValueKind == JsonValueKind.True;
        return new(L("Sequence"), S("ProbeId"), S("StageId"), L("DeliveryId"), S("DriverOrSchedulerId"), S("ModuleId"), (int)L("PID"), (int)L("TID"),
            S("DocumentId"), S("DatabaseId"), S("CommandIdentity"), S("EventOrMarkerId"), payload, L("TimestampUnixMicros"), malformed);
    }
}

// Process fence and scratch-DWG evidence held by the control plane (CLN-PROCESS-EXIT, EVIDENCE-COMPLETE item 6).
public sealed record V35ProcessEvidence(int ProcessId, bool ProcessGone, bool TerminatedByControlPlane, bool TimedOut, bool ExitedWithinPostFinishDeadline,
    bool InteractiveStateObserved = false);
public sealed record V35ScratchEvidence(bool Captured, bool Unchanged, bool BackupCreated);

public sealed record V35RunEvidence(IReadOnlyList<V35LogRecord> Records, V35ProcessEvidence? Process, V35ScratchEvidence? Scratch, IReadOnlyList<string> ParseErrors,
    V35LogRead? LogRead = null)
{
    public static V35RunEvidence Load(string logPath, V35ProcessEvidence? process, V35ScratchEvidence? scratch, V35LogReadOptions? options = null)
    {
        var records = new List<V35LogRecord>();
        var errors = new List<string>();
        V35LogRead read = V35LogReader.Read(logPath, options);
        if (!read.Readable) errors.Add(read.Error ?? "event log unreadable");
        int line = 0;
        foreach (string text in read.Lines)
        {
            line++;
            if (string.IsNullOrWhiteSpace(text)) continue;
            try { records.Add(V35LogRecord.Parse(text)); }
            catch (JsonException e) { errors.Add($"line {line}: {e.Message}"); }
        }
        return new(records, process, scratch, errors, read);
    }
}

// Retry-safe read of a file another process may still be closing (events.jsonl after the PID exits). A sharing or lock
// violation is retried with bounded exponential backoff; a file that stays unreadable is reported, never ignored: the
// run's evidence is then incomplete (EVIDENCE-COMPLETE items 7 and 8) and the result cannot be PASS.
public sealed record V35LogReadOptions(int MaxAttempts = 10, int InitialDelayMilliseconds = 100, int MaxDelayMilliseconds = 2000);

public sealed record V35LogRead(bool Exists, bool Readable, int Attempts, IReadOnlyList<string> Lines, string? Error);

public static class V35LogReader
{
    public static V35LogRead Read(string path, V35LogReadOptions? options = null)
    {
        options ??= new();
        int delay = options.InitialDelayMilliseconds;
        string? error = null;
        for (int attempt = 1; attempt <= options.MaxAttempts; attempt++)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
                var lines = new List<string>();
                string? text;
                while ((text = reader.ReadLine()) is not null) lines.Add(text);
                return new(true, true, attempt, lines, null);
            }
            catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException) { return new(false, false, attempt, [], "event log missing"); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                error = $"{e.GetType().Name} 0x{e.HResult:X8}: {e.Message}";
                if (attempt == options.MaxAttempts) break;
                Thread.Sleep(delay);
                delay = Math.Min(delay * 2, options.MaxDelayMilliseconds);
            }
        }
        return new(true, false, options.MaxAttempts, [], $"event log unreadable after {options.MaxAttempts} attempts: {error}");
    }
}

// Read-only queries over the ordered log.
public sealed class V35Log(IReadOnlyList<V35LogRecord> records)
{
    public IReadOnlyList<V35LogRecord> All { get; } = records.OrderBy(r => r.Sequence).ToArray();

    public IEnumerable<V35LogRecord> Of(string eventOrMarker) => All.Where(r => r.EventOrMarkerId == eventOrMarker);
    public IEnumerable<V35LogRecord> Of(string eventOrMarker, string stage) => Of(eventOrMarker).Where(r => r.StageId == stage);
    public V35LogRecord? First(string eventOrMarker) => Of(eventOrMarker).FirstOrDefault();
    public V35LogRecord? First(string eventOrMarker, Func<V35LogRecord, bool> filter) => Of(eventOrMarker).FirstOrDefault(filter);
    public bool Any(string eventOrMarker) => Of(eventOrMarker).Any();
    public bool Any(string eventOrMarker, Func<V35LogRecord, bool> filter) => Of(eventOrMarker).Any(filter);

    public long FenceSequence => First("FINISH-FENCE-01", r => r.Is("phase", "SET"))?.Sequence ?? long.MaxValue;
    public long ProbeEntrySequence => First("CMD-PROBE", r => r.Is("phase", "ENTRY"))?.Sequence ?? long.MinValue;

    // Run window of marker observation: from the probe entry up to FINISH-FENCE-01.
    public IEnumerable<V35LogRecord> RunWindow => All.Where(r => r.Sequence >= ProbeEntrySequence && r.Sequence < FenceSequence);
}
