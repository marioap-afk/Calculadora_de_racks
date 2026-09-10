namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// WHAT a project variable's value is. ID22A supports exactly one — <see cref="Length"/>, in inches,
    /// coherent with ADR-0005 and introducing no conversion.
    ///
    /// <para>
    /// Having a single case today does NOT make the discriminator redundant, and dropping it would be the
    /// expensive kind of mistake. Two reasons, both already identified: the clearest candidate after the pilot
    /// is a non-numeric one (<c>DimensionStyle</c>, a drawing-level string), and a future formula cannot
    /// evaluate <c>a + b</c> without knowing whether it adds lengths or concatenates text. The type is a
    /// PRECONDITION of formulas, even though formulas add no types.
    /// </para>
    /// <para>
    /// There is no zero member on purpose: <c>default(VariableType)</c> is not a valid type, and
    /// <see cref="ProjectVariable.Create"/> rejects it rather than letting an undeclared value travel.
    /// </para>
    /// </summary>
    public enum VariableType
    {
        /// <summary>A length, in inches.</summary>
        Length = 1,
    }

    /// <summary>Which <see cref="VariableType"/> values this build can actually resolve.</summary>
    public static class VariableTypes
    {
        /// <summary>True when <paramref name="type"/> is a type ID22A declares. A value outside the enum is never supported.</summary>
        public static bool IsSupported(VariableType type) => type == VariableType.Length;
    }
}
