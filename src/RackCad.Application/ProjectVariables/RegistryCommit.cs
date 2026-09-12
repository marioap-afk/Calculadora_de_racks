using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>What the commit re-read decided about a planned register change.</summary>
    public enum RegistryCommitOutcome
    {
        /// <summary>The plan changes no variable. Nothing is read, accredited or written.</summary>
        Unchanged = 1,

        /// <summary>The re-read was accredited and the changed document is ready to write.</summary>
        Ready = 2,

        /// <summary>The re-read is not a usable authority. Nothing is applied and nothing is written.</summary>
        Blocked = 3,
    }

    /// <summary>
    /// The document a commit may write, or the visible reason there is none.
    ///
    /// <para>
    /// <see cref="Changed"/> is non-null ONLY for <see cref="RegistryCommitOutcome.Ready"/>. On every other
    /// outcome there is no product at all, which is also the evidence that the change was never applied: a
    /// changed document can only come out of <see cref="RegistryMutation.ApplyTo"/>.
    /// </para>
    /// </summary>
    public sealed class RegistryCommitPreparation
    {
        private RegistryCommitPreparation(
            RegistryCommitOutcome outcome, ProjectVariablesDocument changed, string error)
        {
            Outcome = outcome;
            Changed = changed;
            Error = error;
        }

        public RegistryCommitOutcome Outcome { get; }

        /// <summary>The document to write. Null unless <see cref="IsReady"/>.</summary>
        public ProjectVariablesDocument Changed { get; }

        /// <summary>The visible reason nothing may be written. Null unless <see cref="IsBlocked"/>.</summary>
        public string Error { get; }

        public bool IsReady => Outcome == RegistryCommitOutcome.Ready;

        public bool IsBlocked => Outcome == RegistryCommitOutcome.Blocked;

        internal static RegistryCommitPreparation Unchanged()
            => new RegistryCommitPreparation(RegistryCommitOutcome.Unchanged, null, null);

        internal static RegistryCommitPreparation Ready(ProjectVariablesDocument changed)
            => new RegistryCommitPreparation(RegistryCommitOutcome.Ready, changed, null);

        internal static RegistryCommitPreparation Blocked(string error)
            => new RegistryCommitPreparation(RegistryCommitOutcome.Blocked, null, error);
    }

    /// <summary>
    /// The ONE seam through which a register change reaches the drawing (I-48 G4B, Proposal V8 R-01).
    ///
    /// <para>
    /// A plan is decided against the register that was read when the window opened; a commit writes against the
    /// register that is there NOW. Those are two different documents, so the re-read is a genuinely new one and
    /// needs its own accreditation — an accreditation belongs to the read it was given. Before this seam existed
    /// the executor re-read and applied the change straight to the raw result, which meant the whole identity
    /// precondition was enforced only on the planning path: a register that grew a duplicated
    /// <see cref="VariableId"/> in between was mutated anyway, and a Remove over a duplicated id deletes BOTH
    /// entries.
    /// </para>
    /// <para>
    /// The order is <b>read, accredit that read, apply to the accredited document, write</b>. It is enforced by
    /// construction and not by comment: <see cref="RegistryMutation.ApplyTo"/> is internal, so the Plugin cannot
    /// call it, and the document it is applied to here is the one the accreditation vouches for.
    /// </para>
    /// <para>
    /// A plan with <see cref="RegistryMutationKind.None"/> returns <see cref="RegistryCommitOutcome.Unchanged"/>
    /// without touching the read. A rack-only operation must not acquire a new failure mode — and must not force
    /// a re-read it has no use for.
    /// </para>
    /// </summary>
    public static class RegistryCommit
    {
        /// <summary>
        /// Accredits the commit re-read and produces the document to write, or blocks with the reason.
        /// </summary>
        /// <param name="mutation">The planned register change. Null or <c>None</c> means there is nothing to do.</param>
        /// <param name="lastRead">The register as it is NOW, read inside the write transaction.</param>
        public static RegistryCommitPreparation Prepare(
            RegistryMutation mutation, ProjectVariablesReadResult lastRead)
        {
            if (mutation == null || mutation.Kind == RegistryMutationKind.None)
            {
                return RegistryCommitPreparation.Unchanged();
            }

            var accreditation = UsableProjectVariablesRegistry.Accredit(lastRead);

            if (!accreditation.IsUsable)
            {
                // Fail-closed BEFORE applying anything. There is no changed document to hand back, so a caller
                // cannot write "most of" the operation: the transaction unwinds with the drawing untouched.
                return RegistryCommitPreparation.Blocked(accreditation.Error);
            }

            return RegistryCommitPreparation.Ready(mutation.ApplyTo(accreditation.Document));
        }
    }
}
