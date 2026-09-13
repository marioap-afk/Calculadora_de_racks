using System;
using System.Globalization;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// A range of the text the user wrote, measured in UTF-16 code units: <see cref="Start"/> is an index into the
    /// string and <see cref="Length"/> counts code units, so a letter outside the Basic Multilingual Plane spans two.
    ///
    /// <para>
    /// Positions exist only while WRITING (I-49, Proposal V6 P2.1 and P15.1). The syntax tree carries them so a
    /// diagnostic can point at what was typed and so the binder can tell which pieces were contiguous. Nothing that is
    /// bound, persisted or evaluated has a position.
    /// </para>
    /// </summary>
    public readonly struct SourceSpan : IEquatable<SourceSpan>
    {
        public SourceSpan(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start), start, "A span cannot start before the text.");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "A span cannot have a negative length.");
            }

            Start = start;
            Length = length;
        }

        public int Start { get; }

        public int Length { get; }

        /// <summary>The first code unit AFTER the span.</summary>
        public int End => Start + Length;

        internal static SourceSpan FromBounds(int start, int end) => new SourceSpan(start, end - start);

        public bool Equals(SourceSpan other) => Start == other.Start && Length == other.Length;

        public override bool Equals(object obj) => obj is SourceSpan other && Equals(other);

        public override int GetHashCode() => (Start, Length).GetHashCode();

        public override string ToString()
            => "[" + Start.ToString(CultureInfo.InvariantCulture) + ".." + End.ToString(CultureInfo.InvariantCulture) + ")";

        public static bool operator ==(SourceSpan left, SourceSpan right) => left.Equals(right);

        public static bool operator !=(SourceSpan left, SourceSpan right) => !left.Equals(right);
    }
}
