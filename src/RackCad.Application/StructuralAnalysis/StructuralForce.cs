using System;
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
        public double PoundsForce => throw new NotImplementedException();

        public static StructuralForce FromPoundsForce(double poundsForce) => throw new NotImplementedException();

        public static StructuralForce From(double value, ForceUnit unit) => throw new NotImplementedException();

        /// <summary>The magnitude expressed in <paramref name="unit"/>.</summary>
        public double In(ForceUnit unit) => throw new NotImplementedException();

        public bool Equals(StructuralForce other) => throw new NotImplementedException();

        public override bool Equals(object obj) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        public override string ToString() => throw new NotImplementedException();

        public static bool operator ==(StructuralForce left, StructuralForce right) => left.Equals(right);

        public static bool operator !=(StructuralForce left, StructuralForce right) => !left.Equals(right);
    }
}
