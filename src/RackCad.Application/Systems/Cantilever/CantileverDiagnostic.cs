using System;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>How much a diagnostic matters. Mirrors the shape of <c>SectionDiagnosticSeverity</c>.</summary>
    public enum CantileverDiagnosticSeverity
    {
        /// <summary>Worth knowing; the result is what it claims to be.</summary>
        Info = 0,

        /// <summary>The result is usable but something about it must be shown to the user.</summary>
        Warning = 1,

        /// <summary>The design cannot be resolved. Nothing is drawn and nothing is counted.</summary>
        Blocking = 2
    }

    /// <summary>
    /// One thing the resolver wants the caller to know. <see cref="Code"/> is the stable token: tests key on
    /// it and never on the message, which is Spanish prose for the user and may be reworded.
    /// </summary>
    public sealed class CantileverDiagnostic
    {
        public CantileverDiagnostic(CantileverDiagnosticSeverity severity, string code, string message)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Un diagnostico necesita su codigo estable.", nameof(code));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Un diagnostico necesita su mensaje.", nameof(message));
            }

            Severity = severity;
            Code = code;
            Message = message;
        }

        public CantileverDiagnosticSeverity Severity { get; }

        /// <summary>Stable machine token, e.g. <c>CANT_SECTION_UNKNOWN</c>.</summary>
        public string Code { get; }

        /// <summary>Spanish description for the user.</summary>
        public string Message { get; }

        public bool IsBlocking => Severity == CantileverDiagnosticSeverity.Blocking;

        public static CantileverDiagnostic Info(string code, string message) =>
            new CantileverDiagnostic(CantileverDiagnosticSeverity.Info, code, message);

        public static CantileverDiagnostic Warning(string code, string message) =>
            new CantileverDiagnostic(CantileverDiagnosticSeverity.Warning, code, message);

        public static CantileverDiagnostic Blocking(string code, string message) =>
            new CantileverDiagnostic(CantileverDiagnosticSeverity.Blocking, code, message);

        public override string ToString() => "[" + Severity + "] " + Code + " — " + Message;
    }

    /// <summary>
    /// The stable diagnostic tokens of the Cantilever resolver.
    ///
    /// They exist as constants, and not as literals at the throw site, because a test that keys on a string
    /// typed twice is a test that passes while the product is broken.
    /// </summary>
    public static class CantileverDiagnostics
    {
        /// <summary>The design left a section id empty.</summary>
        public const string SectionIdMissing = "CANT_SECTION_ID_MISSING";

        /// <summary>The stored text is not a well-formed section id.</summary>
        public const string SectionIdInvalid = "CANT_SECTION_ID_INVALID";

        /// <summary>The id is well formed but the catalogue does not have it.</summary>
        public const string SectionUnknown = "CANT_SECTION_UNKNOWN";

        /// <summary>
        /// The section resolved but is disabled. WARNING, never blocking: a design saved months ago must
        /// keep opening (owner decision 15 of I-36A), and substituting the section silently would change
        /// its geometry behind the user's back.
        /// </summary>
        public const string SectionDisabled = "CANT_SECTION_DISABLED";

        /// <summary>The section's family is not one this Cantilever design admits.</summary>
        public const string SectionFamilyNotEligible = "CANT_SECTION_FAMILY_NOT_ELIGIBLE";

        /// <summary>Column and base resolve, but the policy has no registered variant for the pair.</summary>
        public const string CombinationNotEligible = "CANT_COMBINATION_NOT_ELIGIBLE";

        /// <summary>The registered variant is not the one the design declares.</summary>
        public const string VariantMismatch = "CANT_VARIANT_MISMATCH";

        /// <summary>The contour of a section carries a declared RackCad convention (ADR-0023).</summary>
        public const string SectionVisualDerived = "CANT_SECTION_VISUAL_DERIVED";

        /// <summary>A required parameter has no approved default and the design did not supply it.</summary>
        public const string RequiredParameterMissing = "CANT_REQUIRED_PARAMETER_MISSING";

        /// <summary>A length, thickness, pitch or offset is not a usable positive number.</summary>
        public const string ParameterNotPositive = "CANT_PARAMETER_NOT_POSITIVE";

        /// <summary>Not even the first connection punch fits inside the base section envelope.</summary>
        public const string NoPunchFitsInBase = "CANT_NO_PUNCH_FITS_IN_BASE";

        /// <summary>
        /// The punch columns the COLUMN governs fall outside the rear plate the BASE can offer.
        ///
        /// Blocking on purpose: widening the plate would be inventing a rule nobody approved, and the
        /// combination is simply not a product.
        /// </summary>
        public const string PunchOutsideRearPlate = "CANT_PUNCH_OUTSIDE_REAR_PLATE";

        /// <summary>The column is too short to hold a single regular punch after the connection region.</summary>
        public const string NoRegularPunchFits = "CANT_NO_REGULAR_PUNCH_FITS";

        /// <summary>The column bottom plate is too shallow to hold a symmetric pair at the required offset.</summary>
        public const string NoBottomPlatePunchFits = "CANT_NO_BOTTOM_PLATE_PUNCH_FITS";
    }
}
