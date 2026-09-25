using System.Text.Json.Nodes;

namespace I52Ctda.ControlPlane.V35;

public sealed record V35Entry(string Id, string Kind, JsonObject Raw)
{
    public string? Attr(string name) => Raw[name] is JsonValue v && v.TryGetValue(out string? s) ? s : null;
    public IReadOnlyList<string> List(string name) => Raw[name] is JsonArray a ? a.Select(x => x!.GetValue<string>()).ToArray() : [];
    public bool Flag(string name) => Raw[name] is JsonValue v && v.TryGetValue(out bool b) && b;
}

public sealed record V35Marker(bool MustBePresent, string Id);
public sealed record V35MarkerBinding(string Marker, string Stage);

// One immutable NPM-V35 row, parsed field by field from the frozen text; nothing is inferred from prose.
public sealed record V35RowPlan(
    string ProbeId,
    string PrimaryAuthorityId,
    string ScheduleOriginEventId,
    string SchedulerChainId,
    IReadOnlyList<string> HeaderAuthority,
    string DriverId,
    IReadOnlyList<string> ObserverRegistrationIds,
    string? BodyObserverId,
    IReadOnlyList<string> GuardIds,
    IReadOnlyList<string> SetupActionIds,
    string TriggerActionId,
    string ExecutionContextId,
    string PrimaryTransactionId,
    string ExecutionTransactionId,
    IReadOnlyList<string> TopTransactionAuthorityIds,
    IReadOnlyList<string> DocumentLockIds,
    IReadOnlyList<string> ThreadContextIds,
    string MutationActionId,
    string Surface,
    string SchedulePoint,
    string ExecutionPoint,
    IReadOnlyList<string> ExpectedBefore,
    IReadOnlyList<string> ExpectedAfter,
    IReadOnlyList<string> VerifierIds,
    IReadOnlyList<V35Marker> Markers,
    IReadOnlyList<V35MarkerBinding> MarkerStageBindings,
    IReadOnlyList<string> ObservationPredicateIds,
    IReadOnlyList<string> UnknownPredicateIds,
    IReadOnlyList<string> FailPredicateIds,
    string PassClass,
    string FailClass,
    string CleanupActionId,
    string CompletionFence,
    IReadOnlyList<string> CompletionTokenIds,
    string Threats,
    string DaPhP,
    string RowApprovalHash)
{
    public bool HasScheduleOrigin => !ScheduleOriginEventId.StartsWith("NOT_APPLICABLE", StringComparison.Ordinal);
    public bool RunsBody => BodyObserverId is not null;
}

public sealed class V35Authority
{
    public const string CatalogPath = "docs/initiatives/I-52-execution-catalog-v35.json";
    public const string MatrixPath = "docs/initiatives/I-52-native-probe-matrix-v35.md";
    public const string FixturePath = "eng/research/I52Ctda/fixture-v35.json";
    public const string TraceabilityPath = "eng/research/I52Ctda/traceability-v35.json";
    public const string OraclePath = "eng/research/I52Ctda/v35-oracle.json";

    private V35Authority(IReadOnlyDictionary<string, V35Entry> entries, IReadOnlyList<string> rowSchema, IReadOnlyList<V35RowPlan> rows, JsonObject catalog)
    {
        Entries = entries;
        RowSchema = rowSchema;
        Rows = rows;
        Catalog = catalog;
        ByProbe = rows.ToDictionary(r => r.ProbeId, StringComparer.Ordinal);
    }

    public IReadOnlyDictionary<string, V35Entry> Entries { get; }
    public IReadOnlyList<string> RowSchema { get; }
    public IReadOnlyList<V35RowPlan> Rows { get; }
    public IReadOnlyDictionary<string, V35RowPlan> ByProbe { get; }
    public JsonObject Catalog { get; }

    public V35Entry this[string id] => Entries.TryGetValue(id, out V35Entry? e) ? e : throw new KeyNotFoundException($"V35 authority id not in EXEC-CATALOG-V35-1: {id}");
    public string KindOf(string id) => this[id].Kind;
    public IEnumerable<string> IdsOfKind(string kind) => Entries.Values.Where(e => e.Kind == kind).Select(e => e.Id);

    // Loads the frozen authority. The freeze guard runs first: drifted normative files STOP the load.
    public static V35Authority Load(string repository, bool verifyFreeze = true)
    {
        if (verifyFreeze) V35Freeze.AssertHolds(repository);
        JsonObject catalog = JsonNode.Parse(File.ReadAllText(Path.Combine(repository, CatalogPath)))!.AsObject();
        if (catalog["revision"]!.GetValue<string>() != V35Freeze.Revision) throw new InvalidDataException("Catalog revision is not " + V35Freeze.Revision);
        var entries = new Dictionary<string, V35Entry>(StringComparer.Ordinal);
        foreach ((string id, JsonNode? raw) in catalog["entries"]!.AsObject())
            entries.Add(id, new V35Entry(id, raw!["kind"]!.GetValue<string>(), raw.AsObject()));
        string[] schema = catalog["rowSchema"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
        string matrix = File.ReadAllText(Path.Combine(repository, MatrixPath)).Replace("\r\n", "\n", StringComparison.Ordinal);
        var rows = ParseRows(matrix, schema).ToArray();
        var authority = new V35Authority(entries, schema, rows, catalog);
        V35PlanCompiler.AssertResolved(authority);
        return authority;
    }

    public static IEnumerable<V35RowPlan> ParseRows(string matrix, IReadOnlyList<string> schema)
    {
        int section = matrix.IndexOf("## 5. Full normative matrix", StringComparison.Ordinal);
        if (section < 0) throw new InvalidDataException("NPM-V35 section 5 missing");
        int start = matrix.IndexOf("```text\n", section, StringComparison.Ordinal) + "```text\n".Length;
        int end = matrix.IndexOf("```", start, StringComparison.Ordinal);
        foreach (string line in matrix[start..end].Trim('\n').Split('\n'))
        {
            string[] f = line.Split(';');
            if (f.Length != schema.Count) throw new InvalidDataException($"NPM-V35 row has {f.Length} fields, schema has {schema.Count}: {line[..Math.Min(40, line.Length)]}");
            var raw = new JsonObject();
            for (int i = 0; i < schema.Count; i++) raw[schema[i]] = f[i];
            string field(string name) => f[IndexOf(schema, name)];
            IReadOnlyList<string> list(string name, char separator = ',') { string v = field(name); return v is "NONE" or "" ? [] : v.Split(separator); }
            yield return new V35RowPlan(
                field("ProbeId"), field("PrimaryAuthorityId"), field("ScheduleOriginEventId"), field("SchedulerChainId"), list("HeaderAuthority"),
                field("DriverId"), list("ObserverRegistrationIds"), field("BodyObserverId") == "NONE" ? null : field("BodyObserverId"), list("GuardIds"),
                list("SetupActionIds"), field("TriggerActionId"), field("ExecutionContextId"), field("PrimaryTransactionId"), field("ExecutionTransactionId"),
                list("TopTransactionAuthorityIds"), list("DocumentLockIds"), list("ThreadContextIds"), field("MutationActionId"), field("Surface"),
                field("SchedulePoint"), field("ExecutionPoint"), list("ExpectedBefore", '+'), list("ExpectedAfter", '+'), list("VerifierIds"),
                list("Markers").Select(m => new V35Marker(m[0] == '+', m[1..])).ToArray(),
                list("MarkerStageBindings").Select(b => b.Split('@')).Select(p => new V35MarkerBinding(p[0], p[1])).ToArray(),
                list("ObservationPredicateIds"), list("UnknownPredicateIds"), list("FailPredicateIds"), field("PassClass"), field("FailClass"),
                field("CleanupActionId"), field("CompletionFence"), list("CompletionTokenIds"), field("Threats"), field("DA-P/H-P"),
                V35Canon.Sha256(raw));
        }
    }

    private static int IndexOf(IReadOnlyList<string> schema, string name)
    {
        for (int i = 0; i < schema.Count; i++) if (schema[i] == name) return i;
        throw new InvalidDataException("rowSchema lacks " + name);
    }
}
