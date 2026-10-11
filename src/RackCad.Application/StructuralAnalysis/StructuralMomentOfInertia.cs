using System;
using System.Globalization;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// A second moment of area (or torsional constant) of the neutral analysis model, stored in INCHES TO THE FOURTH POWER
    /// (I-65 D-03). Same shape as <see cref="StructuralArea"/>.
    /// </summary>
    public readonly struct StructuralMomentOfInertia : IEquatable<StructuralMomentOfInertia>
    {
        private readonly double _inchesToTheFourth;

        private StructuralMomentOfInertia(double inchesToTheFourth)
        {
            _inchesToTheFourth = inchesToTheFourth;
        }

        /// <summary>The magnitude in inches to the fourth power.</summary>
        public double InchesToTheFourth => _inchesToTheFourth;

        public static StructuralMomentOfInertia FromInchesToTheFourth(double inchesToTheFourth)
        {
            if (double.IsNaN(inchesToTheFourth) || double.IsInfinity(inchesToTheFourth))
            {
                throw new ArgumentOutOfRangeException(nameof(inchesToTheFourth), inchesToTheFourth, "A moment of inertia must be finite.");
            }

            return new StructuralMomentOfInertia(inchesToTheFourth);
        }

        public bool Equals(StructuralMomentOfInertia other) => _inchesToTheFourth.Equals(other._inchesToTheFourth);

        public override bool Equals(object obj) => obj is StructuralMomentOfInertia other && Equals(other);

        public override int GetHashCode() => _inchesToTheFourth.GetHashCode();

        public override string ToString() => _inchesToTheFourth.ToString("R", CultureInfo.InvariantCulture) + " in4";

        public static bool operator ==(StructuralMomentOfInertia left, StructuralMomentOfInertia right) => left.Equals(right);

        public static bool operator !=(StructuralMomentOfInertia left, StructuralMomentOfInertia right) => !left.Equals(right);
    }
}
