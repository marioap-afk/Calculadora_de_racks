#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>The result of one drain, tagged with the session it belongs to.</summary>
    public readonly record struct DrainOutcome(SessionId Session, DrainResult Result);

    /// <summary>What the close-begin handler decides from in-memory state only.</summary>
    public enum CloseDecision
    {
        Allow,
        Veto,
    }
}
