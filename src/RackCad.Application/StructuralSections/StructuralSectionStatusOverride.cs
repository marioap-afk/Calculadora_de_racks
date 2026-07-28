namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// One line of the hand-editable status overlay (<c>structural-section-status.csv</c>).
    ///
    /// The overlay holds EXCEPTIONS ONLY: every section is enabled by default and absence from this file means
    /// enabled. That is what keeps a 983-row generated catalog and a human decision in separate files — the
    /// generated ones can be regenerated at any time without losing the decision.
    ///
    /// It is also the ONLY user override the neutral catalog admits (decision of the owner). No overridden
    /// dimension, no overridden weight, no renamed designation: those would fork the source silently.
    /// </summary>
    public sealed class StructuralSectionStatusOverride
    {
        public StructuralSectionId SectionId { get; init; }

        /// <summary>False withdraws the section from NEW selections. It is never deleted and still resolves by id.</summary>
        public bool IsEnabled { get; init; }

        /// <summary>Optional reason, for whoever reads the file in a year.</summary>
        public string Notes { get; init; }

        public override string ToString() =>
            SectionId.Value + " -> " + (IsEnabled ? "habilitada" : "deshabilitada");
    }
}
