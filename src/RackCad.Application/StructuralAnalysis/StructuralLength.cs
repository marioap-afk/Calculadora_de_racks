using System;
using System.Globalization;
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

        public static StructuralLength Zero => default;

        /// <summary>The magnitude in inches.</summary>
        public double Inches => _inches;

        public static StructuralLength FromInches(double inches) => From(inches, LengthUnit.Inch);

        public static StructuralLength FromMillimeters(double millimeters) => From(millimeters, LengthUnit.Millimeter);

        public static StructuralLength From(double value, LengthUnit unit)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "A length must be finite.");
            }

            var inches = LengthUnits.Authority.ToInches(value, unit);

            if (double.IsInfinity(inches))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The length does not fit in a double once converted.");
            }

            return new StructuralLength(inches);
        }

        public bool Equals(StructuralLength other) => _inches.Equals(other._inches);

        public override bool Equals(object obj) => obj is StructuralLength other && Equals(other);

        public override int GetHashCode() => _inches.GetHashCode();

        public override string ToString() => _inches.ToString("R", CultureInfo.InvariantCulture) + " in";

        public static bool operator ==(StructuralLength left, StructuralLength right) => left.Equals(right);

        public static bool operator !=(StructuralLength left, StructuralLength right) => !left.Equals(right);
    }
}
