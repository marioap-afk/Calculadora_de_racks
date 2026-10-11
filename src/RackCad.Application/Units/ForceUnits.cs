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
        public static IReadOnlyList<ForceUnit> Units => throw new NotImplementedException();

        /// <summary>The ordinal symbol of a unit: <c>N</c>, <c>kN</c>, <c>lbf</c>, <c>kip</c> or <c>kgf</c>.</summary>
        public static string Token(ForceUnit unit) => throw new NotImplementedException();

        /// <summary>The unit of an exact symbol, compared ordinally.</summary>
        public static bool TryParseToken(string token, out ForceUnit unit) => throw new NotImplementedException();

        /// <summary>The single conversion of an input magnitude to pounds-force.</summary>
        public static double ToPoundsForce(double value, ForceUnit unit) => throw new NotImplementedException();

        /// <summary>The inverse of <see cref="ToPoundsForce"/>, with the same factors.</summary>
        public static double FromPoundsForce(double poundsForce, ForceUnit unit) => throw new NotImplementedException();
    }
}
