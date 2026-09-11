namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// The bindable properties ID22A knows, declared in ONE place.
    ///
    /// <para>
    /// The tokens are constants and not literals scattered across the code, which is the same discipline
    /// <c>CatalogBlockParameters</c> already applies to dynamic-block parameter names: a C# rename must not be
    /// able to break a persisted key in silence.
    /// </para>
    /// <para>
    /// ID22A bound exactly ONE property; I-48 G4E adds the second. <see cref="IsKnown"/> tells an
    /// unrecognised key from a recognised one — the distinction that turns "this build does not understand
    /// the document" into a visible error instead of a quiet fall back to the literal.
    /// </para>
    /// </summary>
    public static class ProjectPropertyIds
    {
        /// <summary>The pilot property of ID22A: the selective rack's vertical clearance.</summary>
        public const string SelectiveVerticalClearanceToken = "selective.verticalClearance";

        /// <summary>The pilot property of ID22A, as a comparable id.</summary>
        public static PropertyId SelectiveVerticalClearance { get; } = PropertyId.Parse(SelectiveVerticalClearanceToken);

        /// <summary>
        /// The SECOND real bindable property (I-48 G4E): the selective rack's horizontal pallet tolerance.
        ///
        /// <para>
        /// It is system-qualified like the first and compared Ordinal, case-sensitive: the token is a
        /// persistence key, so <c>selective.pallettolerance</c> is a DIFFERENT property and not a spelling of
        /// this one. There is no alias and no normalisation, because either would make two keys mean one thing
        /// and leave nobody able to say which was written.
        /// </para>
        /// </summary>
        public const string SelectivePalletToleranceToken = "selective.palletTolerance";

        /// <summary>The second real bindable property, as a comparable id.</summary>
        public static PropertyId SelectivePalletTolerance { get; } = PropertyId.Parse(SelectivePalletToleranceToken);

        /// <summary>
        /// True when this build knows what property <paramref name="id"/> names.
        ///
        /// <para>
        /// Since I-48 G4A the answer is DERIVED from the closed catalogue of linked properties by membership,
        /// instead of being an equality against a single constant. The behaviour is identical while the
        /// catalogue holds one property — which it does, deliberately, until the proof gate — but the authority
        /// has moved: what this build knows is now whatever the catalogue declares, in one place.
        /// </para>
        /// </summary>
        public static bool IsKnown(PropertyId id) => SelectiveLinkedProperties.IsKnown(id);
    }
}
