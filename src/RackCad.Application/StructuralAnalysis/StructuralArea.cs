using System;
using System.Globalization;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// A cross-section area of the neutral analysis model, stored in SQUARE INCHES (I-65 D-03). Catalogue rows published in
    /// the US customary system are already in this unit, so no conversion exists here; no constructor and no conversion from
    /// a bare <c>double</c> exist either.
    /// </summary>
    public readonly struct StructuralArea : IEquatable<StructuralArea>
    {
        private readonly double _squareInches;

        private StructuralArea(double squareInches)
        {
            _squareInches = squareInches;
        }

        /// <summary>The magnitude in square inches.</summary>
        public double SquareInches => _squareInches;

        public static StructuralArea FromSquareInches(double squareInches)
        {
            if (double.IsNaN(squareInches) || double.IsInfinity(squareInches))
            {
                throw new ArgumentOutOfRangeException(nameof(squareInches), squareInches, "An area must be finite.");
            }

            return new StructuralArea(squareInches);
        }

        public bool Equals(StructuralArea other) => _squareInches.Equals(other._squareInches);

        public override bool Equals(object obj) => obj is StructuralArea other && Equals(other);

        public override int GetHashCode() => _squareInches.GetHashCode();

        public override string ToString() => _squareInches.ToString("R", CultureInfo.InvariantCulture) + " in2";

        public static bool operator ==(StructuralArea left, StructuralArea right) => left.Equals(right);

        public static bool operator !=(StructuralArea left, StructuralArea right) => !left.Equals(right);
    }
}
