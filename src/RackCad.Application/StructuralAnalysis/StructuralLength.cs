using System;
using RackCad.Application.Units;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// A length of the neutral analysis model, stored in INCHES (ADR-0005; I-65 D-03).
    ///
    /// There is no public constructor from a bare <c>double</c> and no conversion from one: a number only becomes a length
    /// through a factory that names its unit. Conversions between units are delegated to <see cref="LengthUnits"/>.
    /// </summary>
    public readonly struct StructuralLength : IEquatable<StructuralLength>
    {
        private readonly double _inches;

        private StructuralLength(double inches)
        {
            _inches = inches;
        }

        public static StructuralLength Zero => throw new NotImplementedException();

        /// <summary>The magnitude in inches.</summary>
        public double Inches => throw new NotImplementedException();

        public static StructuralLength FromInches(double inches) => throw new NotImplementedException();

        public static StructuralLength FromMillimeters(double millimeters) => throw new NotImplementedException();

        public static StructuralLength From(double value, LengthUnit unit) => throw new NotImplementedException();

        public bool Equals(StructuralLength other) => throw new NotImplementedException();

        public override bool Equals(object obj) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        public override string ToString() => throw new NotImplementedException();

        public static bool operator ==(StructuralLength left, StructuralLength right) => left.Equals(right);

        public static bool operator !=(StructuralLength left, StructuralLength right) => !left.Equals(right);
    }
}
