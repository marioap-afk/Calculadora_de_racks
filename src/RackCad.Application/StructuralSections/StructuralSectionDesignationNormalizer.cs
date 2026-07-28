using System;
using System.Globalization;
using System.Text;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Turns a published shape designation into the ASCII token used for identity and lookup (ADR-0021).
    ///
    /// The rule is deliberately tiny and total: upper-case invariant, drop every whitespace character, and map
    /// the three punctuation marks AISC actually uses inside a designation — <c>.</c> (decimal thickness of an
    /// HSS, <c>HSS34X10X.875</c>), <c>/</c> (fraction, <c>L4X4X1/4</c>) and <c>-</c> (mixed number,
    /// <c>L12X12X1-3/8</c>) — to a single underscore. Nothing else is touched, so the transformation is
    /// reversible enough to audit by eye and, crucially, never DERIVES a dimension from the text.
    ///
    /// It is applied to the EDI designation to build the id and to any designation the user types when
    /// searching, which is why both live in one function: two normalizers would eventually disagree and a
    /// lookup would miss a section that exists.
    /// </summary>
    public static class StructuralSectionDesignationNormalizer
    {
        /// <summary>Characters a normalized designation may contain. The id adds <c>-</c> for its own separators.</summary>
        private const string AllowedAfterNormalization = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";

        public static bool TryNormalize(string designation, out string normalized)
        {
            normalized = null;

            if (string.IsNullOrWhiteSpace(designation))
            {
                return false;
            }

            var builder = new StringBuilder(designation.Length);

            foreach (var raw in designation)
            {
                if (char.IsWhiteSpace(raw))
                {
                    continue;
                }

                var upper = char.ToUpperInvariant(raw);

                switch (upper)
                {
                    case '.':
                    case '/':
                    case '-':
                        builder.Append('_');
                        break;
                    default:
                        if (AllowedAfterNormalization.IndexOf(upper) < 0)
                        {
                            // A character the AISC designations of the four imported families never contain.
                            // Guessing a substitution here is how a normalizer starts inventing identity.
                            return false;
                        }

                        builder.Append(upper);
                        break;
                }
            }

            if (builder.Length == 0)
            {
                return false;
            }

            normalized = builder.ToString();
            return true;
        }

        public static string Normalize(string designation)
        {
            if (!TryNormalize(designation, out var normalized))
            {
                throw new ArgumentException(
                    "No se puede normalizar la designacion '" + (designation ?? "<null>") +
                    "': esta vacia o contiene caracteres fuera de A-Z, 0-9, '.', '/' y '-'.",
                    nameof(designation));
            }

            return normalized;
        }

        /// <summary>True when <paramref name="value"/> is already the output of <see cref="Normalize"/>.</summary>
        public static bool IsNormalized(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (var ch in value)
            {
                if (AllowedAfterNormalization.IndexOf(ch) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Formats a tabulated number the way the catalog writes it: invariant culture, round-trip precision,
        /// no thousands separator and no exponent for the magnitudes AISC publishes. Kept next to the
        /// normalizer because both answer the same question — "what is the canonical TEXT of this datum".
        /// </summary>
        public static string FormatInvariant(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
