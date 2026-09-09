using System;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// The persistent identity of a PROPERTY — what property it is, independently of the C# field that holds
    /// it today and of the text box that edits it (I-47 §1-bis, C4-14).
    ///
    /// <para>
    /// It exists for two reasons, and the second is what makes it mandatory now rather than later: it is the
    /// key of the persisted binding map, and it is one half of the <c>(RackId, PropertyId)</c> pair a future
    /// rack-to-rack reference needs. Introducing it afterwards would mean rewriting what is already on disk.
    /// </para>
    /// <para>
    /// Comparison is <b>Ordinal</b>: exact, case-sensitive, with no normalization, no aliases and no semantic
    /// trimming. The asymmetry with <see cref="VariableId"/> (OrdinalIgnoreCase) is deliberate and is
    /// explained there. The consequence is the one that matters: an unrecognised <see cref="PropertyId"/> is
    /// a value THIS BUILD does not understand, and a build that cannot name a property cannot claim the
    /// literal beside it is the right value — so it is a visible error later, never a silent fall back.
    /// </para>
    /// </summary>
    public readonly struct PropertyId : IEquatable<PropertyId>
    {
        private readonly string _value;

        private PropertyId(string value)
        {
            _value = value;
        }

        /// <summary>The token, verbatim. Never null: a default-constructed id reports <see cref="string.Empty"/>.</summary>
        public string Value => _value ?? string.Empty;

        /// <summary>True for a default-constructed id, which names no property.</summary>
        public bool IsEmpty => string.IsNullOrEmpty(_value);

        /// <summary>
        /// Accepts a token. Returns false — never throws — so a caller reading a persisted map can classify an
        /// empty key instead of crashing. The token is NOT trimmed: trimming is normalization, and this type
        /// exists precisely to compare what was written.
        /// </summary>
        public static bool TryParse(string value, out PropertyId id)
        {
            id = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            id = new PropertyId(value);
            return true;
        }

        public static PropertyId Parse(string value)
        {
            if (!TryParse(value, out var id))
            {
                throw new ArgumentException(
                    "Id de propiedad invalido: '" + (value ?? "<null>") + "'. No puede estar vacio.",
                    nameof(value));
            }

            return id;
        }

        public bool Equals(PropertyId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is PropertyId other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;

        public static bool operator ==(PropertyId left, PropertyId right) => left.Equals(right);

        public static bool operator !=(PropertyId left, PropertyId right) => !left.Equals(right);
    }
}
