using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.Units
{
    /// <summary>
    /// The closed set of force units the structural-analysis input boundary accepts (I-65 D-03 and D-10b).
    /// </summary>
    public enum ForceUnit
    {
        Newton = 1,
        Kilonewton = 2,
        PoundForce = 3,
        Kip = 4,
        KilogramForce = 5,
    }

    /// <summary>
    /// The ONE owner of the standard gravity (9.80665 m/s2, exact by definition) and of the conversions to the canonical
    /// force of the analysis, the pound-force (I-65 D-03, INV-03 and INV-16).
    ///
    /// <para>
    /// lbf = 1 lb x gn; kgf = 1 kg x gn; kip = 1000 lbf; kN = 1000 N. The factors are derived by multiplication of the
    /// owners (<see cref="MassUnits"/> and <see cref="StandardGravity"/>), so no other literal exists. The table is
    /// closed and its symbols are compared ordinally: <c>lb</c>, <c>LBF</c> or <c>kips</c> are not units.
    /// </para>
    /// </summary>
    public static class ForceUnits
    {
        /// <summary>Exact by definition: gn = 9.80665 m/s2.</summary>
        public const double StandardGravity = 9.80665;

        /// <summary>1 lbf = 1 lb x gn, in newtons.</summary>
        public const double NewtonsPerPoundForce = MassUnits.KilogramsPerPound * StandardGravity;

        /// <summary>1 kip = 1000 lbf.</summary>
        public const double PoundsForcePerKip = 1000;

        /// <summary>1 kN = 1000 N.</summary>
        public const double NewtonsPerKilonewton = 1000;

        private static readonly IReadOnlyList<ForceUnit> DeclaredUnits = new ReadOnlyCollection<ForceUnit>(
            new[]
            {
                ForceUnit.Newton, ForceUnit.Kilonewton, ForceUnit.PoundForce, ForceUnit.Kip, ForceUnit.KilogramForce,
            });

        /// <summary>The closed table, in declaration order.</summary>
        public static IReadOnlyList<ForceUnit> Units => DeclaredUnits;

        /// <summary>The ordinal symbol of a unit: <c>N</c>, <c>kN</c>, <c>lbf</c>, <c>kip</c> or <c>kgf</c>.</summary>
        public static string Token(ForceUnit unit)
        {
            switch (unit)
            {
                case ForceUnit.Newton: return "N";
                case ForceUnit.Kilonewton: return "kN";
                case ForceUnit.PoundForce: return "lbf";
                case ForceUnit.Kip: return "kip";
                case ForceUnit.KilogramForce: return "kgf";
                default: throw new ArgumentOutOfRangeException(nameof(unit), unit, "Undeclared force unit.");
            }
        }

        /// <summary>The unit of an exact symbol, compared ordinally.</summary>
        public static bool TryParseToken(string token, out ForceUnit unit)
        {
            switch (token)
            {
                case "N":
                    unit = ForceUnit.Newton;
                    return true;

                case "kN":
                    unit = ForceUnit.Kilonewton;
                    return true;

                case "lbf":
                    unit = ForceUnit.PoundForce;
                    return true;

                case "kip":
                    unit = ForceUnit.Kip;
                    return true;

                case "kgf":
                    unit = ForceUnit.KilogramForce;
                    return true;

                default:
                    unit = default;
                    return false;
            }
        }

        /// <summary>The single conversion of an input magnitude to pounds-force.</summary>
        public static double ToPoundsForce(double value, ForceUnit unit)
        {
            switch (unit)
            {
                case ForceUnit.Newton: return value / NewtonsPerPoundForce;
                case ForceUnit.Kilonewton: return (value * NewtonsPerKilonewton) / NewtonsPerPoundForce;
                case ForceUnit.PoundForce: return value;
                case ForceUnit.Kip: return value * PoundsForcePerKip;
                case ForceUnit.KilogramForce: return (value * StandardGravity) / NewtonsPerPoundForce;
                default: throw new ArgumentOutOfRangeException(nameof(unit), unit, "Undeclared force unit.");
            }
        }

        /// <summary>The inverse of <see cref="ToPoundsForce"/>, with the same factors.</summary>
        public static double FromPoundsForce(double poundsForce, ForceUnit unit)
        {
            switch (unit)
            {
                case ForceUnit.Newton: return poundsForce * NewtonsPerPoundForce;
                case ForceUnit.Kilonewton: return (poundsForce * NewtonsPerPoundForce) / NewtonsPerKilonewton;
                case ForceUnit.PoundForce: return poundsForce;
                case ForceUnit.Kip: return poundsForce / PoundsForcePerKip;
                case ForceUnit.KilogramForce: return (poundsForce * NewtonsPerPoundForce) / StandardGravity;
                default: throw new ArgumentOutOfRangeException(nameof(unit), unit, "Undeclared force unit.");
            }
        }
    }
}
