using System.Text.RegularExpressions;

namespace I52Ctda.ControlPlane;

public enum ImplementationStatus
{
    ImplementedManaged,
    ImplementedNativeSourceUncompiled,
    ReadyForNativeLink,
    RuntimeOnlyPending,
    BlockedContract,
    Stub
}

public enum ResultState { Pass, Fail, Unknown }

public enum FailureClassification
{
    None,
    HarnessDefect,
    HelperDefect,
    FixtureDefect,
    EnvironmentInvalid,
    HostFail,
    HostUnknown,
    InstrumentNondeterminism,
    HostNondeterminism
}

public sealed record ProbeDefinition(
    string ProbeId,
    string PrimaryAuthorityId,
    string ScheduleOriginEventId,
    string HeaderAuthority,
    string Setup,
    string Trigger,
    string PrimaryTransactionId,
    string TopTransactionId,
    string DocumentLock,
    string ThreadContext,
    string Mutation,
    string Surface,
    string SchedulePoint,
    string ExecutionPoint,
    string ExpectedBefore,
    string ExpectedAfter,
    string Verifier,
    string LifecycleMarkers,
    string Threats,
    string Properties,
    string Pass,
    string Fail,
    string Unknown,
    string Cleanup)
{
    public static ProbeDefinition Parse(string row)
    {
        string[] f = row.Split(';', StringSplitOptions.TrimEntries);
        if (f.Length != 24) throw new InvalidDataException($"Probe row has {f.Length} fields: {row}");
        return new(f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7], f[8], f[9], f[10], f[11],
            f[12], f[13], f[14], f[15], f[16], f[17], f[18], f[19], f[20], f[21], f[22], f[23]);
    }
}

public sealed record ImplementationDescriptor(
    string ProbeId,
    string HandlerId,
    string PrimaryAuthorityId,
    string ScheduleOriginEventId,
    string? EventId,
    string? SchedulerId,
    IReadOnlyList<string> SupportActions,
    IReadOnlyList<string> FixtureTargets,
    string Verifier,
    string Cleanup,
    ImplementationStatus Status);

public sealed class ContractCatalog
{
    private static readonly Regex IdToken = new(@"\b(?:SA|F)-[A-Z0-9-]+\b", RegexOptions.Compiled);
    public IReadOnlyList<ProbeDefinition> Probes { get; }
    public IReadOnlyDictionary<string, ImplementationDescriptor> Implementations { get; }
    public IReadOnlyList<string> ReachableEventIds { get; }

    private ContractCatalog(IReadOnlyList<ProbeDefinition> probes, IReadOnlyList<string> events)
    {
        Probes = probes;
        ReachableEventIds = events;
        Implementations = probes.ToDictionary(p => p.ProbeId, CreateDescriptor, StringComparer.Ordinal);
    }

    public static ContractCatalog Load(string repository)
    {
        string npm = File.ReadAllText(Path.Combine(repository, "docs/initiatives/I-52-native-probe-matrix-v34.md"));
        string nec = File.ReadAllText(Path.Combine(repository, "docs/initiatives/I-52-native-event-catalog-v34.md"));
        string matrix = Between(npm, "## 10. Full normative matrix", "## 11. Closure rules");
        ProbeDefinition[] probes = matrix.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(line => Regex.IsMatch(line, @"^[A-Za-z0-9][A-Za-z0-9-]*;"))
            .Select(ProbeDefinition.Parse)
            .ToArray();
        string inventory = Between(nec, "## 1. Exact event and capability inventory", "## 2. Event-to-probe projection");
        string[] events = Regex.Matches(inventory, @"\| `(N-[^`]+)` \|")
            .Select(m => m.Groups[1].Value).Where(id => id != "N-ED-FAIL").Distinct(StringComparer.Ordinal).ToArray();
        if (probes.Length != 100 || probes.Select(p => p.ProbeId).Distinct(StringComparer.Ordinal).Count() != 100)
            throw new InvalidDataException("NPM-V34 must contain 100 unique ProbeIds.");
        if (events.Length != 26) throw new InvalidDataException("NEC-V34 must contain 26 reachable EventIds.");
        return new ContractCatalog(probes, events);
    }

    public void AssertComplete()
    {
        if (Implementations.Count != 100) throw new InvalidDataException("Implementation registry is incomplete.");
        if (Implementations.Values.Any(i => i.Status == ImplementationStatus.Stub || i.Status == ImplementationStatus.BlockedContract))
            throw new InvalidDataException("Implementation registry contains a stub or contract blocker.");
        string[] covered = Implementations.Values.Where(i => i.EventId is not null).Select(i => i.EventId!).Distinct(StringComparer.Ordinal).ToArray();
        string[] missing = ReachableEventIds.Except(covered, StringComparer.Ordinal).ToArray();
        if (missing.Length != 0) throw new InvalidDataException($"Missing primary handlers: {string.Join(',', missing)}");
    }

    private static ImplementationDescriptor CreateDescriptor(ProbeDefinition p)
    {
        string all = string.Join(' ', p.Setup, p.Trigger, p.DocumentLock, p.Mutation, p.LifecycleMarkers, p.Cleanup);
        string[] tokens = IdToken.Matches(all).Select(m => m.Value).Distinct(StringComparer.Ordinal).ToArray();
        var supportSet = tokens.Where(id => id.StartsWith("SA-", StringComparison.Ordinal)).ToHashSet(StringComparer.Ordinal);
        if (all.Contains("APPCTX", StringComparison.Ordinal)) supportSet.UnionWith(["SA-LOCK", "SA-UNLOCK", "SA-TX-RESOLVE", "SA-TX-START", "SA-TX-END", "SA-TX-ABORT"]);
        if (all.Contains("CMDCTX", StringComparison.Ordinal) || all.Contains("T-FRESH", StringComparison.Ordinal)) supportSet.UnionWith(["SA-TX-RESOLVE", "SA-TX-START", "SA-TX-END", "SA-TX-ABORT"]);
        if (all.Contains("VETO", StringComparison.Ordinal)) supportSet.Add("SA-VETO");
        var targetSet = tokens.Where(id => id.StartsWith("F-", StringComparison.Ordinal)).ToHashSet(StringComparer.Ordinal);
        if (p.Surface is "S" or "ALL") targetSet.Add("F-XR");
        if (p.Surface is "M" or "ALL") targetSet.Add("F-REF-B");
        if (p.Surface is "SM" or "ALL") targetSet.UnionWith(["F-REF-A", "F-REF-B", "F-REF-C"]);
        string? eventId = p.PrimaryAuthorityId.StartsWith("N-", StringComparison.Ordinal) ? p.PrimaryAuthorityId : null;
        string? schedulerId = p.PrimaryAuthorityId.StartsWith("NS-", StringComparison.Ordinal) ? p.PrimaryAuthorityId : null;
        return new(p.ProbeId, $"Probe_{Sanitize(p.ProbeId)}", p.PrimaryAuthorityId, p.ScheduleOriginEventId,
            eventId, schedulerId, supportSet.Order(StringComparer.Ordinal).ToArray(), targetSet.Order(StringComparer.Ordinal).ToArray(), p.Verifier, p.Cleanup, ImplementationStatus.ReadyForNativeLink);
    }

    public static string Sanitize(string id) => Regex.Replace(id, "[^A-Za-z0-9]", "_");

    private static string Between(string text, string start, string end)
    {
        int first = text.IndexOf(start, StringComparison.Ordinal);
        int last = text.IndexOf(end, first + start.Length, StringComparison.Ordinal);
        if (first < 0 || last < 0) throw new InvalidDataException($"Missing section {start} / {end}.");
        return text[(first + start.Length)..last];
    }
}
