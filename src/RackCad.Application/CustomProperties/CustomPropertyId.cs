using System;

namespace RackCad.Application.CustomProperties
{
    /// <summary>
    /// The stable identity of one custom property (I-54 D-02 / ADR-0039 §1): a GUID that Application mints once and
    /// NEVER derives from the property's name.
    ///
    /// <para>
    /// The identity is the GUID <b>value</b>, not its text. Another build may spell the same GUID in upper case, and
    /// both spellings are the same property; what this build writes is always the lower-case <c>D</c> form, so the text
    /// converges without the identity ever depending on it.
    /// </para>
    /// <para>
    /// The accepted text is deliberately narrower than <see cref="Guid.TryParseExact(string, string, out Guid)"/>,
    /// which tolerates white space around the value: a persisted identity with a stray space is corruption, not a
    /// variant spelling. So the exact 36-character shape is checked first and the value obtained afterwards. And
    /// <see cref="Guid.Empty"/> is not an identity at all: it is what an id nobody assigned looks like.
    /// </para>
    /// </summary>
    public readonly struct CustomPropertyId : IEquatable<CustomPropertyId>
    {
        private const int TextLength = 36;

        private readonly Guid _value;

        private CustomPropertyId(Guid value)
        {
            _value = value;
        }

        /// <summary>The GUID. <see cref="Guid.Empty"/> only for a default-constructed id, which is NOT a usable identity.</summary>
        public Guid Value => _value;

        public bool IsEmpty => _value == Guid.Empty;

        /// <summary>Mints a brand-new identity: the only way a property is born with one.</summary>
        public static CustomPropertyId New() => new CustomPropertyId(Guid.NewGuid());

        /// <summary>
        /// Accepts exactly the persisted shape (D-02.1): 36 characters, hyphens at positions 8, 13, 18 and 23, hexadecimal
        /// digits of either case everywhere else, and a value other than <see cref="Guid.Empty"/>. Returns false, never
        /// throws, so a reader can classify a malformed id instead of crashing on it.
        /// </summary>
        public static bool TryParse(string text, out CustomPropertyId id)
        {
            id = default;

            if (text == null || text.Length != TextLength)
            {
                return false;
            }

            for (var i = 0; i < TextLength; i++)
            {
                var isHyphenPosition = i == 8 || i == 13 || i == 18 || i == 23;

                if (isHyphenPosition ? text[i] != '-' : !IsHexDigit(text[i]))
                {
                    return false;
                }
            }

            if (!Guid.TryParseExact(text, "D", out var value) || value == Guid.Empty)
            {
                return false;
            }

            id = new CustomPropertyId(value);
            return true;
        }

        public bool Equals(CustomPropertyId other) => _value == other._value;

        public override bool Equals(object obj) => obj is CustomPropertyId other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        /// <summary>The persisted form: <c>D</c>, lower case (D-02.3).</summary>
        public override string ToString() => _value.ToString("D");

        public static bool operator ==(CustomPropertyId left, CustomPropertyId right) => left.Equals(right);

        public static bool operator !=(CustomPropertyId left, CustomPropertyId right) => !left.Equals(right);

        private static bool IsHexDigit(char c)
            => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }
}
