#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4B PB-T4a (D-PBT4-03, A-T3-8) — the reader of the edit intention that can read again at the origin of an open draft,
    /// found by its text. It is optional: a reader without it is read as today.
    /// </summary>
    public interface IWorkspaceEditIntentOriginReader
    {
        EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness? witness);
    }
}
