using System;
using System.Collections.Generic;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>The identity of a material of the neutral model. It is never derived from a catalogue grade (I-65 D-07).</summary>
    public readonly struct StructuralMaterialId : IEquatable<StructuralMaterialId>
    {
        private readonly string _value;

        private StructuralMaterialId(string value)
        {
            _value = value;
        }

        public string Value => throw new NotImplementedException();

        public bool IsEmpty => throw new NotImplementedException();

        public static bool TryCreate(string value, out StructuralMaterialId id) => throw new NotImplementedException();

        public static StructuralMaterialId Create(string value) => throw new NotImplementedException();

        public bool Equals(StructuralMaterialId other) => throw new NotImplementedException();

        public override bool Equals(object obj) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        public override string ToString() => throw new NotImplementedException();

        public static bool operator ==(StructuralMaterialId left, StructuralMaterialId right) => left.Equals(right);

        public static bool operator !=(StructuralMaterialId left, StructuralMaterialId right) => !left.Equals(right);
    }

    /// <summary>
    /// One row of the explicit material assignment: section id to material id, who verified it and when (CD-I65-F1-06).
    /// A row without verifier or without date is not verified and assigns nothing.
    /// </summary>
    public sealed class StructuralMaterialRow
    {
        public StructuralMaterialRow(
            StructuralSectionId sectionId, StructuralMaterialId materialId, string verifier, DateTime? verifiedOn)
        {
            throw new NotImplementedException();
        }

        public StructuralSectionId SectionId => throw new NotImplementedException();

        public StructuralMaterialId MaterialId => throw new NotImplementedException();

        public string Verifier => throw new NotImplementedException();

        public DateTime? VerifiedOn => throw new NotImplementedException();

        public bool IsVerified => throw new NotImplementedException();

        /// <summary>The canonical text of the row; it is what the mapping-rows hash is computed from.</summary>
        public string CanonicalText => throw new NotImplementedException();
    }

    /// <summary>The neutral abstraction through which a material reaches the model: explicit verified rows only.</summary>
    public interface IStructuralMaterialProfile
    {
        bool TryGetRow(StructuralSectionId sectionId, out StructuralMaterialRow row);
    }

    /// <summary>
    /// An in-memory profile. The product ships NO rows in F1 (<see cref="Empty"/>): rows used by tests are fixtures, not
    /// an engineering verification.
    /// </summary>
    public sealed class StructuralMaterialProfile : IStructuralMaterialProfile
    {
        public StructuralMaterialProfile(IEnumerable<StructuralMaterialRow> rows)
        {
            throw new NotImplementedException();
        }

        public static StructuralMaterialProfile Empty => throw new NotImplementedException();

        public IReadOnlyList<StructuralMaterialRow> Rows => throw new NotImplementedException();

        public bool TryGetRow(StructuralSectionId sectionId, out StructuralMaterialRow row) =>
            throw new NotImplementedException();
    }
}
