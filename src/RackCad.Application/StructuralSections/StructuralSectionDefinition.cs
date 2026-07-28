namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// One catalogued cross section: what it is called, what it measures, what it weighs and whether it may be
    /// offered for a NEW selection. Composed, not inherited — identity, dimensions and properties are three
    /// separate concerns and each has its own type (ADR-0020).
    ///
    /// What a <c>StructuralSection</c> is NOT, by decision of the owner and not by omission: it is not a post,
    /// a beam, a truss member, a spacer, a cantilever arm or a column. It carries no member role, no punching,
    /// no connector, no bracket, no end plate and no fabrication rule. Those belong to the FUTURE member
    /// configurators (I-37 onwards), which will compose a section rather than extend it.
    /// </summary>
    public sealed class StructuralSectionDefinition
    {
        public StructuralSectionIdentity Identity { get; init; }

        /// <summary>
        /// Linear weight in the SOURCE's native unit — <c>lb/ft</c> for AISC v16.0. It is kept as published and
        /// never replaced by its converted equivalent (ADR-0021 §8). The equivalent is computed on demand by
        /// <see cref="StructuralSectionUnits"/>.
        ///
        /// This field is the ONLY authority for weight. Reading the "28" out of <c>W12X28</c> would work for a
        /// W and fail for every HSS, so nothing in this catalog derives a magnitude from a designation.
        /// </summary>
        public double WeightPerLength { get; init; }

        /// <summary>The unit system <see cref="WeightPerLength"/> and the dimensions are expressed in.</summary>
        public StructuralSectionUnitSystem NativeUnitSystem { get; init; }

        /// <summary>The family-specific measurements. Concrete type matches <see cref="StructuralSectionIdentity.Family"/>.</summary>
        public IStructuralSectionDimensions Dimensions { get; init; }

        /// <summary>Tabulated section properties; may be incomplete. Never null — use <see cref="StructuralSectionProperties.Empty"/>.</summary>
        public StructuralSectionProperties Properties { get; init; }

        /// <summary>
        /// Whether the section is offered in NEW selections. A disabled section is not deleted and
        /// <see cref="StructuralSectionCatalog.TryGetById"/> keeps resolving it, so an existing design that
        /// already references it still loads (owner decisions 14 and 15).
        /// </summary>
        public bool IsEnabled { get; init; } = true;

        /// <summary>Free note from the status overlay explaining why a section was disabled. Usually null.</summary>
        public string StatusNotes { get; init; }

        /// <summary>
        /// Optional material grade. It is NOT part of the id and is NEVER inferred: the AISC Shapes Database
        /// publishes no grade, so every row imported from it leaves this null (owner decision 13).
        /// </summary>
        public string MaterialGrade { get; init; }

        /// <summary>
        /// AISC <c>T_F</c>: the source flags shapes carrying a special note (for W shapes, <c>tf &gt; 2 in.</c>).
        /// Null when the source does not publish the flag for that family.
        /// </summary>
        public bool? SourceSpecialNote { get; init; }

        public StructuralSectionId SectionId => Identity == null ? default : Identity.SectionId;

        public StructuralSectionFamily Family => Identity.Family;

        /// <summary>What a UI shows: the AISC Manual label, in its original published form.</summary>
        public string DisplayName => Identity.ManualLabel;

        public override string ToString() => Identity == null ? base.ToString() : Identity.ToString();
    }
}
