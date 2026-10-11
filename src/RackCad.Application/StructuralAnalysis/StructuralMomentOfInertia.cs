using System;

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
        public double InchesToTheFourth => throw new NotImplementedException();

        public static StructuralMomentOfInertia FromInchesToTheFourth(double inchesToTheFourth) => throw new NotImplementedException();

        public bool Equals(StructuralMomentOfInertia other) => throw new NotImplementedException();

        public override bool Equals(object obj) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        public override string ToString() => throw new NotImplementedException();

        public static bool operator ==(StructuralMomentOfInertia left, StructuralMomentOfInertia right) => left.Equals(right);

        public static bool operator !=(StructuralMomentOfInertia left, StructuralMomentOfInertia right) => !left.Equals(right);
    }
}
