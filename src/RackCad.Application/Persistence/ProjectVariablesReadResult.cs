namespace RackCad.Application.Persistence
{
    /// <summary>How reading the drawing's project-variable register turned out (I-47 D-01-bis, C4.6-2).</summary>
    public enum ProjectVariablesReadOutcome
    {
        /// <summary>
        /// The register does not exist in this drawing. That is NOT an error: it is a project with zero
        /// variables, and it is valid legacy. Produced by the layer that looks the entry up, never by parsing.
        /// </summary>
        Absent = 1,

        /// <summary>The register was read and this build understands all of it.</summary>
        Readable = 2,

        /// <summary>
        /// The register EXISTS but its content is empty, corrupt, not deserializable, missing its version, or
        /// declaring something this build does not understand. Visible error, no empty register, and NO WRITE.
        /// </summary>
        PresentButUnreadable = 3,

        /// <summary>The register was written by a newer MAJOR. Visible error, and NO WRITE — never a downgrade.</summary>
        IncompatibleMajor = 4,
    }

    /// <summary>
    /// The discriminated result of reading the register.
    ///
    /// <para>
    /// The whole point is the distance between two neighbours that a tolerant reader would collapse:
    /// <b>ABSENT is not PRESENT_BUT_UNREADABLE</b>. Reading a present-but-corrupt register as "empty" would
    /// destroy every variable in the drawing on the next write, silently. So absence resolves to an empty
    /// register and corruption fails closed, and <see cref="CanWrite"/> is what a caller checks before
    /// touching anything.
    /// </para>
    /// </summary>
    public sealed class ProjectVariablesReadResult
    {
        private ProjectVariablesReadResult(
            ProjectVariablesReadOutcome outcome,
            ProjectVariablesDocument document,
            string error)
        {
            Outcome = outcome;
            Document = document;
            Error = error;
        }

        public ProjectVariablesReadOutcome Outcome { get; }

        /// <summary>The register. Null for both failing outcomes — there is nothing safe to hand back.</summary>
        public ProjectVariablesDocument Document { get; }

        /// <summary>The visible reason. Null when the read succeeded.</summary>
        public string Error { get; }

        /// <summary>
        /// True when writing the register back is allowed. False after ANY read failure: overwriting a
        /// register this build could not read is how a drawing loses variables without anyone noticing.
        /// </summary>
        public bool CanWrite
            => Outcome == ProjectVariablesReadOutcome.Absent || Outcome == ProjectVariablesReadOutcome.Readable;

        /// <summary>
        /// The drawing has no register: an EMPTY one, which is valid legacy (C-1). Produced by the layer that
        /// looked the entry up and found nothing — parsing never produces this.
        /// </summary>
        public static ProjectVariablesReadResult Absent()
            => new ProjectVariablesReadResult(
                ProjectVariablesReadOutcome.Absent,
                ProjectVariablesDocument.CreateNew(),
                null);

        public static ProjectVariablesReadResult Readable(ProjectVariablesDocument document)
            => new ProjectVariablesReadResult(ProjectVariablesReadOutcome.Readable, document, null);

        public static ProjectVariablesReadResult Unreadable(string error)
            => new ProjectVariablesReadResult(ProjectVariablesReadOutcome.PresentButUnreadable, null, error);

        public static ProjectVariablesReadResult IncompatibleMajor(string error)
            => new ProjectVariablesReadResult(ProjectVariablesReadOutcome.IncompatibleMajor, null, error);
    }
}
