using System;

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
        public double SquareInches => throw new NotImplementedException();

        public static StructuralArea FromSquareInches(double squareInches) => throw new NotImplementedException();

        public bool Equals(StructuralArea other) => throw new NotImplementedException();

        public override bool Equals(object obj) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        public override string ToString() => throw new NotImplementedException();

        public static bool operator ==(StructuralArea left, StructuralArea right) => left.Equals(right);

        public static bool operator !=(StructuralArea left, StructuralArea right) => !left.Equals(right);
    }
}
