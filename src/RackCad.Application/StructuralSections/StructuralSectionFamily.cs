using System;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The families of cross section the neutral catalog carries (ADR-0020). A family is a SHAPE, never a
    /// member role: there is no POSTE, CELOSIA, LARGUERO or SEPARADOR here, because the role belongs to the
    /// member, not to the cross section.
    ///
    /// Square HSS is NOT a fifth family: a square tube is the case of a rectangular one whose two walls are
    /// equal (<see cref="HssRectangularSectionDimensions.IsSquare"/>). Splitting it would duplicate every
    /// invariant for a distinction the source itself does not make — AISC tabulates both under Type "HSS"
    /// with the same columns.
    /// </summary>
    public enum StructuralSectionFamily
    {
        /// <summary>Wide-flange shapes (AISC Type <c>W</c>).</summary>
        W = 0,

        /// <summary>Rectangular and square hollow structural sections (AISC Type <c>HSS</c> with walls, not a diameter).</summary>
        HssRectangular = 1,

        /// <summary>American Standard channels (AISC Type <c>C</c>). <c>MC</c> is deliberately NOT imported.</summary>
        Channel = 2,

        /// <summary>Single angles (AISC Type <c>L</c>). Double angles (<c>2L</c>) are deliberately NOT imported.</summary>
        Angle = 3
    }

    /// <summary>
    /// The stable on-disk and in-id token of each family. The token — not the C# enum name — is what travels
    /// into <see cref="StructuralSectionId"/> and into the CSV, so renaming the enum member can never silently
    /// change an identifier that is already stored in someone's design.
    /// </summary>
    public static class StructuralSectionFamilies
    {
        public const string WToken = "W";
        public const string HssRectangularToken = "HSS-RECT";
        public const string ChannelToken = "C";
        public const string AngleToken = "L";

        /// <summary>Every family, in declaration order. Callers rely on this order being stable.</summary>
        public static readonly StructuralSectionFamily[] All =
        {
            StructuralSectionFamily.W,
            StructuralSectionFamily.HssRectangular,
            StructuralSectionFamily.Channel,
            StructuralSectionFamily.Angle
        };

        public static string ToToken(StructuralSectionFamily family)
        {
            switch (family)
            {
                case StructuralSectionFamily.W: return WToken;
                case StructuralSectionFamily.HssRectangular: return HssRectangularToken;
                case StructuralSectionFamily.Channel: return ChannelToken;
                case StructuralSectionFamily.Angle: return AngleToken;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(family), family, "Familia de seccion estructural desconocida.");
            }
        }

        /// <summary>
        /// Parses a family token. The comparison is ORDINAL and case sensitive on purpose: these files are
        /// generated output, and a lenient parse is exactly the kind of tolerance the strict reader exists to
        /// remove (a mistyped token must be an error, not a guess).
        /// </summary>
        public static bool TryParseToken(string token, out StructuralSectionFamily family)
        {
            switch (token)
            {
                case WToken: family = StructuralSectionFamily.W; return true;
                case HssRectangularToken: family = StructuralSectionFamily.HssRectangular; return true;
                case ChannelToken: family = StructuralSectionFamily.Channel; return true;
                case AngleToken: family = StructuralSectionFamily.Angle; return true;
                default: family = default; return false;
            }
        }

        public static StructuralSectionFamily ParseToken(string token)
        {
            if (!TryParseToken(token, out var family))
            {
                throw new ArgumentException(
                    "Familia de seccion estructural desconocida: '" + token + "'. Valores validos: " +
                    WToken + ", " + HssRectangularToken + ", " + ChannelToken + ", " + AngleToken + ".",
                    nameof(token));
            }

            return family;
        }
    }
}
