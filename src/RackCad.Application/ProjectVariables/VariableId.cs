using System;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// The stable identity of a project variable (ADR-0034 / I-47 D-02): a GUID that is generated once and
    /// NEVER derived from the variable's name.
    ///
    /// <para>
    /// The independence from <see cref="ProjectVariable.Name"/> is the whole point, not a nicety. A slug
    /// identity would make renaming break every reference that points at the variable, which is exactly what
    /// the requirement forbids — so renaming has to be an operation with no consumers, and that is only true
    /// while nothing resolves by name.
    /// </para>
    /// <para>
    /// Comparison is <b>OrdinalIgnoreCase</b>, matching the policy the rack GUID already uses in
    /// <c>RackCommandSupport.FindRackBlocks</c>. That is deliberate and asymmetric with
    /// <see cref="PropertyId"/>, which compares <c>Ordinal</c>: this one is a machine-generated GUID whose
    /// hexadecimal case varies between writers, while a <see cref="PropertyId"/> is a token AUTHORED in code,
    /// where a difference in case means somebody wrote something else.
    /// </para>
    /// <para>
    /// The text is stored exactly as it was written and is never normalized: a stored document round-trips to
    /// the same bytes, and the case-insensitive comparison is what makes two spellings the same identity.
    /// A value that is not a parseable GUID cannot be constructed at all — that is what lets a later gate
    /// tell a MALFORMED reference (unknown, must abort) from an absent one (nothing to resolve), instead of
    /// letting an unreadable id travel as if it were a real one.
    /// </para>
    /// </summary>
    public readonly struct VariableId : IEquatable<VariableId>
    {
        private readonly string _value;

        private VariableId(string value)
        {
            _value = value;
        }

        /// <summary>The id text, exactly as authored. Never null: a default-constructed id reports <see cref="string.Empty"/>.</summary>
        public string Value => _value ?? string.Empty;

        /// <summary>True for a default-constructed id, which is NOT a usable identity.</summary>
        public bool IsEmpty => string.IsNullOrEmpty(_value);

        /// <summary>Mints a brand-new identity. The only way a variable is born with one.</summary>
        public static VariableId New() => new VariableId(Guid.NewGuid().ToString());

        /// <summary>
        /// Accepts an id that already exists (a stored document, a UI intent) after checking it is a GUID.
        /// Returns false — never throws — so a caller reading persisted data can classify a malformed value
        /// instead of crashing on it.
        /// </summary>
        public static bool TryParse(string value, out VariableId id)
        {
            id = default;

            if (string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out _))
            {
                return false;
            }

            id = new VariableId(value.Trim());
            return true;
        }

        public static VariableId Parse(string value)
        {
            if (!TryParse(value, out var id))
            {
                throw new ArgumentException(
                    "Id de variable de proyecto invalido: '" + (value ?? "<null>") + "'. Debe ser un GUID.",
                    nameof(value));
            }

            return id;
        }

        public bool Equals(VariableId other)
            => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is VariableId other && Equals(other);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public override string ToString() => Value;

        public static bool operator ==(VariableId left, VariableId right) => left.Equals(right);

        public static bool operator !=(VariableId left, VariableId right) => !left.Equals(right);
    }
}
