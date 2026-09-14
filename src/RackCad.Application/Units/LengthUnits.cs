using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.Units
{
    /// <summary>
    /// The closed set of length units an expression may write explicitly after a numeral (I-49, Proposal V6 P1.6 and
    /// P9.2; ADR-0040 D3): millimetres, inches and feet. Any other token is not a unit.
    /// </summary>
    public enum LengthUnit
    {
        Millimeter = 1,
        Inch = 2,
        Foot = 3,
    }

    /// <summary>
    /// The ONE neutral authority of generic length conversions and of the unit tokens (V6 P9.5 and P9.7; ADR-0040 D3).
    ///
    /// <para>
    /// It declares the closed unit table, its tokens in both directions and ONLY the generic conversions millimetres ↔
    /// inches and feet ↔ inches. The inch stays RackCad's internal unit: every conversion here goes TO inches, and nothing
    /// converts the drawing (ADR-0005). It depends on nothing —not on the expression core, not on
    /// <c>StructuralSections</c>, not on any system— so the core and the catalogue can both lean on it
    /// (<c>StructuralSectionUnits</c> declares its two conversion constants as aliases of these).
    /// </para>
    /// <para>
    /// A constant belongs here because its ROLE is to convert a magnitude between units, never because it happens to be
    /// worth 12: the Selective's height rounding step, the Dynamic's commercial foot and the Cantilever's per-12 slope
    /// notation are domain rules and stay local (P9.6).
    /// </para>
    /// <para>
    /// The tokens are compared ordinally and declared by hand in both directions: no token is ever derived from an enum
    /// member name (P2.4, P17.10).
    /// </para>
    /// </summary>
    public sealed class LengthUnits
    {
        /// <summary>Exact by definition: 1 in = 25.4 mm. Converting millimetres to inches DIVIDES by it (P9.3).</summary>
        public const double MillimetersPerInch = 25.4;

        /// <summary>Exact by definition: 1 ft = 12 in. Converting feet to inches MULTIPLIES by it (P9.3).</summary>
        public const double InchesPerFoot = 12;

        private static readonly IReadOnlyList<LengthUnit> DeclaredUnits =
            new ReadOnlyCollection<LengthUnit>(new[] { LengthUnit.Millimeter, LengthUnit.Inch, LengthUnit.Foot });

        private LengthUnits()
        {
        }

        /// <summary>The single instance: an operation context carries it, and there is no other (P6.1).</summary>
        public static LengthUnits Authority { get; } = new LengthUnits();

        /// <summary>The closed table, in declaration order.</summary>
        public IReadOnlyList<LengthUnit> Units => DeclaredUnits;

        /// <summary>The token written between brackets after a numeral: <c>mm</c>, <c>in</c> or <c>ft</c>.</summary>
        public string Token(LengthUnit unit)
        {
            switch (unit)
            {
                case LengthUnit.Millimeter: return "mm";
                case LengthUnit.Inch: return "in";
                case LengthUnit.Foot: return "ft";
                default: throw new ArgumentOutOfRangeException(nameof(unit), unit, "Undeclared length unit.");
            }
        }

        /// <summary>The unit of an exact token, compared ordinally: <c>MM</c> or <c>cm</c> are not units (P1.6).</summary>
        public bool TryParseToken(string token, out LengthUnit unit)
        {
            switch (token)
            {
                case "mm":
                    unit = LengthUnit.Millimeter;
                    return true;

                case "in":
                    unit = LengthUnit.Inch;
                    return true;

                case "ft":
                    unit = LengthUnit.Foot;
                    return true;

                default:
                    unit = default;
                    return false;
            }
        }

        /// <summary>
        /// The numeric conversion to inches, with the operation fixed so that it is deterministic bit for bit: feet are
        /// multiplied by 12 and millimetres divided by 25.4 (P9.3). The result may be non-finite for a huge value; deciding
        /// what that means is the caller's job, not this table's.
        /// </summary>
        public double ToInches(double value, LengthUnit unit)
        {
            switch (unit)
            {
                case LengthUnit.Inch: return value;
                case LengthUnit.Foot: return value * InchesPerFoot;
                case LengthUnit.Millimeter: return value / MillimetersPerInch;
                default: throw new ArgumentOutOfRangeException(nameof(unit), unit, "Undeclared length unit.");
            }
        }
    }
}
