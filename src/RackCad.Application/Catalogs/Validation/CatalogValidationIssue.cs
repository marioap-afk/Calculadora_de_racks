namespace RackCad.Application.Catalogs.Validation
{
    /// <summary>How bad an issue is. Ordered so that a higher value is worse (used to roll up a report).</summary>
    public enum CatalogValidationSeverity
    {
        /// <summary>Worth surfacing but harmless (e.g. a library block the catalog never references).</summary>
        Info = 0,

        /// <summary>The catalog loads and draws, but something is silently lost or ambiguous.</summary>
        Warning = 1,

        /// <summary>The catalog is inconsistent: a lookup will miss or take the wrong row.</summary>
        Error = 2
    }

    /// <summary>
    /// The five families the validator groups issues into — one per requirement of I-19. The precise problem
    /// is the stable <see cref="CatalogValidationIssue.Code"/>; the category is the coarse bucket for the UI.
    /// </summary>
    public enum CatalogValidationCategory
    {
        /// <summary>An id repeats within a catalog (or within the unified secciones sheet).</summary>
        DuplicateId,

        /// <summary>A foreign key points nowhere, or a relationship key repeats and shadows a row.</summary>
        InvalidReference,

        /// <summary>A block has no name, or a referenced view does not exist.</summary>
        MissingBlockOrView,

        /// <summary>A secciones row was dropped because its "rol" is unknown/blank.</summary>
        DiscardedRow,

        /// <summary>The expected blocks-library.dwg manifest disagrees with the shipped library.</summary>
        Manifest
    }

    /// <summary>
    /// One finding of the catalog validator. Immutable. The <see cref="Code"/> is a stable machine token
    /// (tests and UI filters key on it, never on the localized <see cref="Message"/>); <see cref="Location"/>
    /// pins WHERE it is (an id, a file, or a composite key) so the reader can jump straight to the row.
    /// </summary>
    public sealed class CatalogValidationIssue
    {
        public CatalogValidationIssue(
            CatalogValidationSeverity severity,
            CatalogValidationCategory category,
            string code,
            string message,
            string location = null)
        {
            Severity = severity;
            Category = category;
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            Location = location ?? string.Empty;
        }

        public CatalogValidationSeverity Severity { get; }

        public CatalogValidationCategory Category { get; }

        /// <summary>Stable code, e.g. <c>DUPLICATE_ID</c> or <c>MANIFEST_MISSING_BLOCK</c>.</summary>
        public string Code { get; }

        /// <summary>User-facing Spanish description of the problem.</summary>
        public string Message { get; }

        /// <summary>The id / file / key the issue lives at; may be empty.</summary>
        public string Location { get; }

        public override string ToString()
        {
            var head = "[" + Severity.ToString().ToUpperInvariant() + "][" + Category + "] " + Code;
            var body = string.IsNullOrEmpty(Location) ? Message : Location + ": " + Message;
            return head + " — " + body;
        }
    }
}
