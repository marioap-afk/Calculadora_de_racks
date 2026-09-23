namespace I52Ctda.ControlPlane;

public sealed record FixtureObject(string Id, string Role, string InitialState, bool Disposable);

public static class FixtureSpecification
{
    public static IReadOnlyList<FixtureObject> Objects { get; } =
    [
        new("F-TRIGGER-MOD", "database-modification trigger", "live/unmodified", true),
        new("F-TRIGGER-ERASE-DB", "database-erasure trigger", "live/not-erased", true),
        new("F-TRIGGER-ERASE-OBJ", "object-reactor erasure trigger", "live/not-erased", true),
        new("F-XR", "protected semantic Xrecord", "STATE-S-0", false),
        new("F-REF-B", "protected material reference", "STATE-M-0", false),
        new("F-REF-A", "protected mixed sibling A", "STATE-SM-0", false),
        new("F-REF-C", "protected mixed sibling C", "absent", false)
    ];

    public static void Validate()
    {
        if (Objects.Select(o => o.Id).Distinct(StringComparer.Ordinal).Count() != Objects.Count)
            throw new InvalidDataException("Fixture identities must be unique.");
        string[] required = ["F-TRIGGER-MOD", "F-TRIGGER-ERASE-DB", "F-TRIGGER-ERASE-OBJ", "F-XR", "F-REF-B", "F-REF-A", "F-REF-C"];
        if (required.Except(Objects.Select(o => o.Id), StringComparer.Ordinal).Any())
            throw new InvalidDataException("Fixture specification is incomplete.");
        if (Objects.Where(o => o.Id.StartsWith("F-TRIGGER-", StringComparison.Ordinal)).Any(o => !o.Disposable))
            throw new InvalidDataException("Trigger objects must be disposable.");
    }
}

public sealed class ReentrancyGuard
{
    private int armed;
    public string GuardId { get; }
    public string TriggerObjectId { get; }
    public ReentrancyGuard(string guardId, string triggerObjectId) => (GuardId, TriggerObjectId) = (guardId, triggerObjectId);
    public bool TryArm() => Interlocked.CompareExchange(ref armed, 1, 0) == 0;
    public void Disarm() => Interlocked.Exchange(ref armed, 0);
    public bool IsArmed => Volatile.Read(ref armed) != 0;
}
