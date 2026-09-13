using System;

namespace RackCad.Application.Expressions
{
    internal enum ExpressionTokenKind
    {
        /// <summary>A numeral: ASCII digits with at most one fractional part.</summary>
        Number,

        /// <summary>A bare name: words joined by single spaces.</summary>
        Name,

        /// <summary>A braced name, already unescaped.</summary>
        BracedName,

        /// <summary><c>#</c> and a complete D-format GUID.</summary>
        Qualifier,

        /// <summary><c>[</c> unit <c>]</c>; its text is the trimmed content between the brackets.</summary>
        UnitSuffix,

        Plus,

        Minus,

        Star,

        Slash,

        LeftParenthesis,

        RightParenthesis,

        Comma,

        Dot,

        /// <summary>Stands in for characters the grammar has no token for, so context checks never look past them.</summary>
        Invalid,

        EndOfText,
    }

    /// <summary>
    /// One lexeme, with everything the parser and the context checks need and nothing else. A token for which a
    /// lexical diagnostic was already reported is <see cref="IsValid"/> = false, and no later check reasons from it.
    /// </summary>
    internal readonly struct ExpressionToken
    {
        internal ExpressionToken(
            ExpressionTokenKind kind,
            SourceSpan span,
            string text,
            double value,
            Guid id,
            bool precededByWhitespace,
            bool isValid)
        {
            Kind = kind;
            Span = span;
            Text = text;
            Value = value;
            Id = id;
            PrecededByWhitespace = precededByWhitespace;
            IsValid = isValid;
        }

        internal ExpressionTokenKind Kind { get; }

        internal SourceSpan Span { get; }

        /// <summary>The numeral, the name, the unit content or the GUID as typed; null for punctuation.</summary>
        internal string Text { get; }

        /// <summary>The invariant value of a valid numeral.</summary>
        internal double Value { get; }

        /// <summary>The identity of a valid qualifier.</summary>
        internal Guid Id { get; }

        internal bool PrecededByWhitespace { get; }

        internal bool IsValid { get; }
    }
}
