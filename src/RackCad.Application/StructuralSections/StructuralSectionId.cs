using System;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The stable identity of a catalogued cross section: <c>AISC-{FAMILIA}-{EDI_NORMALIZADO}</c> (ADR-0021).
    ///
    /// What it deliberately does NOT contain, and why:
    /// - the source REVISION, because an id that said <c>V16</c> would break every stored design the day
    ///   v17 ships; the revision lives in its own field;
    /// - the material grade, which is optional, never inferred and not part of what a section IS;
    /// - any magnitude. Nothing derives dimensions or weight by reading the text of a designation.
    ///
    /// The prefix carries the SOURCE (<c>AISC</c>) and the FAMILY token so two sources may one day publish the
    /// same designation without colliding. A normalization collision is a fatal error, never an automatic
    /// disambiguation: over AISC v16.0 the 983 designations of the four imported families produce 983 distinct
    /// ids, and the validator proves it instead of assuming it.
    /// </summary>
    public readonly struct StructuralSectionId : IEquatable<StructuralSectionId>
    {
        /// <summary>Source prefix. Only AISC exists today; the segment is what keeps a second source possible.</summary>
        public const string AiscPrefix = "AISC";

        private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";

        private readonly string _value;

        private StructuralSectionId(string value)
        {
            _value = value;
        }

        /// <summary>The id text. Never null: a default-constructed id reports <see cref="string.Empty"/>.</summary>
        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        /// <summary>Builds the id of a section from its family and its ORIGINAL EDI designation.</summary>
        public static StructuralSectionId Create(StructuralSectionFamily family, string ediDesignation)
        {
            if (!TryCreate(family, ediDesignation, out var id))
            {
                throw new ArgumentException(
                    "No se puede construir el id de la seccion para la designacion EDI '" +
                    (ediDesignation ?? "<null>") + "'.",
                    nameof(ediDesignation));
            }

            return id;
        }

        public static bool TryCreate(StructuralSectionFamily family, string ediDesignation, out StructuralSectionId id)
        {
            id = default;

            if (!StructuralSectionDesignationNormalizer.TryNormalize(ediDesignation, out var normalized))
            {
                return false;
            }

            return TryParse(
                AiscPrefix + "-" + StructuralSectionFamilies.ToToken(family) + "-" + normalized,
                out id);
        }

        /// <summary>Accepts an id that already exists (a CSV cell, a stored design) after validating its shape.</summary>
        public static bool TryParse(string value, out StructuralSectionId id)
        {
            id = default;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (var ch in value)
            {
                if (AllowedCharacters.IndexOf(ch) < 0)
                {
                    return false;
                }
            }

            id = new StructuralSectionId(value);
            return true;
        }

        public static StructuralSectionId Parse(string value)
        {
            if (!TryParse(value, out var id))
            {
                throw new ArgumentException(
                    "Id de seccion estructural invalido: '" + (value ?? "<null>") +
                    "'. Solo se admiten A-Z, 0-9, '_' y '-'.",
                    nameof(value));
            }

            return id;
        }

        public bool Equals(StructuralSectionId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is StructuralSectionId other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;

        public static bool operator ==(StructuralSectionId left, StructuralSectionId right) => left.Equals(right);

        public static bool operator !=(StructuralSectionId left, StructuralSectionId right) => !left.Equals(right);
    }
}
