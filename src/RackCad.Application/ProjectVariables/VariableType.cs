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

        /// <summary>
        /// THE single mapping from a persisted type token to a supported <see cref="VariableType"/> (I-48 G4A,
        /// Proposal V8 R-04/V7-R04).
        ///
        /// <para>
        /// One table, several consumers, and that is the point. The store composes it with its other checks to
        /// decide whether a document is readable; the registry accreditation consumes it MECHANICALLY on a
        /// document the store already accredited. Keeping a second copy — a switch here and a string compare
        /// there — is how two builds of the same token start disagreeing about what a variable is.
        /// </para>
        /// <para>
        /// Sharing the mapping does NOT make its consumers authorities of readability: that verdict stays with
        /// the store. And after a readable verdict, a failure of this same conversion is an invariant
        /// violation to report, never a silent reinterpretation of the document.
        /// </para>
        /// <para>
        /// The comparison is case-insensitive because the token is written by a serializer, not authored by
        /// hand — the same reason <see cref="VariableId"/> compares OrdinalIgnoreCase and
        /// <see cref="PropertyId"/> does not.
        /// </para>
        /// </summary>
        public static bool TryParseToken(string token, out VariableType type)
        {
            type = default;

            if (!System.Enum.TryParse(token, ignoreCase: true, out VariableType parsed) || !IsSupported(parsed))
            {
                return false;
            }

            type = parsed;
            return true;
        }
    }
}
