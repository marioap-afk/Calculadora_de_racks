using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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

        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        public static bool TryCreate(string value, out StructuralMaterialId id)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                id = default;
                return false;
            }

            id = new StructuralMaterialId(value);
            return true;
        }

        public static StructuralMaterialId Create(string value)
        {
            if (!TryCreate(value, out var id))
            {
                throw new ArgumentException("A material id cannot be blank.", nameof(value));
            }

            return id;
        }

        public bool Equals(StructuralMaterialId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is StructuralMaterialId other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;

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
            if (sectionId.IsEmpty)
            {
                throw new ArgumentException("A row needs a section id.", nameof(sectionId));
            }

            if (materialId.IsEmpty)
            {
                throw new ArgumentException("A row needs a material id.", nameof(materialId));
            }

            SectionId = sectionId;
            MaterialId = materialId;
            Verifier = verifier;
            VerifiedOn = verifiedOn;
        }

        /// <summary>
        /// A row that carries, as part of its canonical text, the COMPLETE row of the table it was projected from (I-65 A65-F1-01):
        /// whatever else that table records about the row then reaches the mapping-rows hash and the analysis input fingerprint.
        /// </summary>
        public StructuralMaterialRow(
            StructuralSectionId sectionId, StructuralMaterialId materialId, string verifier, DateTime? verifiedOn, string sourceCanonicalText)
            : this(sectionId, materialId, verifier, verifiedOn)
        {
            SourceCanonicalText = sourceCanonicalText;
        }

        public StructuralSectionId SectionId { get; }

        public StructuralMaterialId MaterialId { get; }

        public string Verifier { get; }

        public DateTime? VerifiedOn { get; }

        public bool IsVerified => !string.IsNullOrWhiteSpace(Verifier) && VerifiedOn.HasValue;

        /// <summary>The canonical text of the table row this row was projected from, or <c>null</c> when it has none.</summary>
        public string SourceCanonicalText { get; }

        /// <summary>
        /// The canonical text of the row; it is what the mapping-rows hash is computed from. Every field is written as
        /// <c>&lt;length&gt;:&lt;value&gt;</c>, so that moving a character from one field to the next cannot give the same text.
        /// </summary>
        public string CanonicalText
        {
            get
            {
                var fields = new List<string>
                {
                    Prefixed(SectionId.Value),
                    Prefixed(MaterialId.Value),
                    Prefixed(Verifier ?? string.Empty),
                    Prefixed(VerifiedOn.HasValue ? VerifiedOn.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty),
                };

                if (SourceCanonicalText != null)
                {
                    fields.Add(Prefixed(SourceCanonicalText));
                }

                return string.Join("|", fields);
            }
        }

        private static string Prefixed(string value) => value.Length.ToString(CultureInfo.InvariantCulture) + ":" + value;
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
        private readonly Dictionary<StructuralSectionId, StructuralMaterialRow> _bySection;

        public StructuralMaterialProfile(IEnumerable<StructuralMaterialRow> rows)
        {
            var list = StructuralRequire.Items(rows, nameof(rows));
            _bySection = new Dictionary<StructuralSectionId, StructuralMaterialRow>();

            foreach (var row in list)
            {
                if (_bySection.ContainsKey(row.SectionId))
                {
                    throw new ArgumentException("Two rows for the section " + row.SectionId + ".", nameof(rows));
                }

                _bySection.Add(row.SectionId, row);
            }

            Rows = new ReadOnlyCollection<StructuralMaterialRow>(list.OrderBy(row => row.SectionId.Value, StringComparer.Ordinal).ToList());
        }

        public static StructuralMaterialProfile Empty { get; } = new StructuralMaterialProfile(new StructuralMaterialRow[0]);

        public IReadOnlyList<StructuralMaterialRow> Rows { get; }

        public bool TryGetRow(StructuralSectionId sectionId, out StructuralMaterialRow row) =>
            _bySection.TryGetValue(sectionId, out row);
    }
}
