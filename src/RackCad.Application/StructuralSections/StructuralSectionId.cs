using System;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The stable identity of a catalogued cross section:
    /// <c>{ID_NAMESPACE}-{FAMILIA}-{DESIGNACION_NORMALIZADA}</c> (ADR-0021).
    ///
    /// The first segment is the **id namespace**: the authority that names the section, declared by its
    /// <see cref="StructuralSectionSource.IdNamespace"/>. It is a parameter and not a constant precisely
    /// because the catalog is NEUTRAL: baking <c>AISC</c> into the identity would make the one source that
    /// exists today the only one that can ever exist, and two publishers naming the same profile would
    /// collide. AISC declares the namespace <c>AISC</c>, so its 983 ids are exactly what they were.
    ///
    /// What the id deliberately does NOT contain, and why:
    /// - the source REVISION, because an id that said <c>V16</c> would break every stored design the day
    ///   v17 ships; the revision lives in its own field;
    /// - the material grade, which is optional, never inferred and not part of what a section IS;
    /// - any magnitude. Nothing derives dimensions or weight by reading the text of a designation.
    ///
    /// A normalization collision is a fatal error, never an automatic disambiguation: over AISC v16.0 the
    /// 983 designations of the four imported families produce 983 distinct ids, and the validator proves it
    /// instead of assuming it.
    /// </summary>
    public readonly struct StructuralSectionId : IEquatable<StructuralSectionId>
    {
        private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";

        /// <summary>
        /// What an id namespace may look like: one or more ASCII upper-case letters or digits.
        ///
        /// The hyphen is excluded on purpose. It is the id's own separator, so a namespace containing one
        /// would let two different (namespace, family) pairs render to the same text — the exact ambiguity
        /// this segment exists to prevent.
        /// </summary>
        private const string AllowedNamespaceCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly string _value;

        private StructuralSectionId(string value)
        {
            _value = value;
        }

        /// <summary>The id text. Never null: a default-constructed id reports <see cref="string.Empty"/>.</summary>
        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        /// <summary>True when <paramref name="idNamespace"/> may be used as the authority segment of an id.</summary>
        public static bool IsValidNamespace(string idNamespace)
        {
            if (string.IsNullOrEmpty(idNamespace))
            {
                return false;
            }

            foreach (var ch in idNamespace)
            {
                if (AllowedNamespaceCharacters.IndexOf(ch) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Builds the id of a section from the authority that names it, its family and its ORIGINAL published
        /// designation (the EDI form for AISC).
        /// </summary>
        public static StructuralSectionId Create(
            string idNamespace,
            StructuralSectionFamily family,
            string designation)
        {
            if (!IsValidNamespace(idNamespace))
            {
                throw new ArgumentException(
                    "Namespace de id invalido: '" + (idNamespace ?? "<null>") +
                    "'. Solo se admiten A-Z y 0-9, y no puede estar vacio.",
                    nameof(idNamespace));
            }

            if (!TryCreate(idNamespace, family, designation, out var id))
            {
                throw new ArgumentException(
                    "No se puede construir el id de la seccion para la designacion '" +
                    (designation ?? "<null>") + "'.",
                    nameof(designation));
            }

            return id;
        }

        public static bool TryCreate(
            string idNamespace,
            StructuralSectionFamily family,
            string designation,
            out StructuralSectionId id)
        {
            id = default;

            if (!IsValidNamespace(idNamespace) ||
                !StructuralSectionDesignationNormalizer.TryNormalize(designation, out var normalized))
            {
                return false;
            }

            return TryParse(
                idNamespace + "-" + StructuralSectionFamilies.ToToken(family) + "-" + normalized,
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
