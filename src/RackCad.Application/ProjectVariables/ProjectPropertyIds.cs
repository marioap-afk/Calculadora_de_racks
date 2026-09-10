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
    /// ID22A binds exactly ONE property. <see cref="IsKnown"/> is what a later gate needs to tell an
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

        /// <summary>True when this build knows what property <paramref name="id"/> names.</summary>
        public static bool IsKnown(PropertyId id) => id == SelectiveVerticalClearance;
    }
}
