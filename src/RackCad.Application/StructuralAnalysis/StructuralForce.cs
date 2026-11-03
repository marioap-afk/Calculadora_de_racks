using System;
using System.Globalization;
using RackCad.Application.Units;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// A force of the neutral analysis model, stored in POUNDS-FORCE (I-65 D-03). Same shape as
    /// <see cref="StructuralLength"/>: no constructor and no conversion from a bare <c>double</c>; the single conversion of
    /// an input magnitude is <see cref="ForceUnits.ToPoundsForce"/>.
    /// </summary>
    public readonly struct StructuralForce : IEquatable<StructuralForce>
    {
        private readonly double _poundsForce;

        private StructuralForce(double poundsForce)
        {
            _poundsForce = poundsForce;
        }

        /// <summary>The magnitude in pounds-force.</summary>
        public double PoundsForce => _poundsForce;

        public static StructuralForce FromPoundsForce(double poundsForce) => From(poundsForce, ForceUnit.PoundForce);

        public static StructuralForce From(double value, ForceUnit unit)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "A force must be finite.");
            }

            var poundsForce = ForceUnits.ToPoundsForce(value, unit);

            if (double.IsInfinity(poundsForce))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The force does not fit in a double once converted.");
            }

            return new StructuralForce(poundsForce);
        }

        /// <summary>The magnitude expressed in <paramref name="unit"/>.</summary>
        public double In(ForceUnit unit) => ForceUnits.FromPoundsForce(_poundsForce, unit);

        public bool Equals(StructuralForce other) => _poundsForce.Equals(other._poundsForce);

        public override bool Equals(object obj) => obj is StructuralForce other && Equals(other);

        public override int GetHashCode() => _poundsForce.GetHashCode();

        public override string ToString() => _poundsForce.ToString("R", CultureInfo.InvariantCulture) + " lbf";

        public static bool operator ==(StructuralForce left, StructuralForce right) => left.Equals(right);

        public static bool operator !=(StructuralForce left, StructuralForce right) => !left.Equals(right);
    }
}
