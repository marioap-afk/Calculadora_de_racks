#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T2B2A (D-T2B2-03) — what the host core needs from the host to read the edit intention: ONE logical read of the
    /// drawing, started by the user, whose answer is a value. The Plugin implements it over the navigation host; tests implement
    /// it with a double. It is assigned to the host core by <see cref="WorkspaceHostCore.SetEditIntentReader"/>, never through a
    /// constructor, and the core never calls it from the drain or from the idle pass.
    ///
    /// The implementation, in this order: the active document, no command and the editor at rest, the catalog outside the lock,
    /// the document lock, the authorization of the session (INV-22), the revalidation of the targets with the choice of the origin,
    /// and the one read. It writes nothing.
    /// </summary>
    public interface IWorkspaceEditIntentReader
    {
        /// <param name="session">The session that asks; it is authorized against the active document.</param>
        /// <param name="rackId">The rack to read.</param>
        /// <param name="chosenReference">The handle of the reference the user came from, or null for the first target of the rack.</param>
        /// <param name="witness">What the preview showed, or null when there was no preview.</param>
        EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness? witness);
    }
}
