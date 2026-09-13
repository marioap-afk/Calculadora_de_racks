namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The closed catalogue of expression diagnostics (I-49, Proposal V6 P15.3), in catalogue order.
    ///
    /// <para>
    /// The numeric value IS the order used to sort diagnostics that share a position (P15.7), so members are only
    /// ever appended. G5 declares the "syntax and limits" class, the only one a lexer and a parser can produce.
    /// Binding codes (<c>UnknownSymbol</c>, <c>ReservedName</c>, <c>UnknownNamespace</c>, <c>UnknownFunction</c>…) and
    /// semantic codes belong to the binder and the evaluator: declaring them here would invite the parser to decide
    /// them, and a second authority over the same question is exactly what the core must not have.
    /// </para>
    /// <para>
    /// These are codes, not messages. The Spanish text a user reads is produced by one text layer outside the core
    /// (P15.4).
    /// </para>
    /// </summary>
    public enum ExpressionDiagnosticCode
    {
        /// <summary>The text is empty or only whitespace.</summary>
        EmptyExpression = 1,

        /// <summary>A character that belongs to no token of the grammar, such as <c>=</c>, <c>@</c> or a non-breaking space.</summary>
        UnexpectedCharacter = 2,

        /// <summary>A well-formed token where the grammar does not allow it, including a missing token at the end.</summary>
        UnexpectedToken = 3,

        /// <summary>A parenthesis that is never closed, or a closing one with nothing open.</summary>
        UnbalancedParenthesis = 4,

        /// <summary>A braced name whose closing brace never arrives (an escaped <c>}}</c> does not close it).</summary>
        UnterminatedName = 5,

        /// <summary>A numeral outside the invariant form: <c>.5</c>, <c>5.</c>, an exponent, glued letters or a non-finite value.</summary>
        InvalidNumber = 6,

        /// <summary>
        /// A comma with a digit right before it and another right after, whatever those digits belong to: never read as
        /// a decimal point nor as an argument separator (P1.5). A separator is written with a space: <c>MAX(A1, 5)</c>.
        /// </summary>
        AmbiguousDecimalComma = 7,

        /// <summary>A bracketed suffix after a numeral whose token is not exactly <c>mm</c>, <c>in</c> or <c>ft</c>.</summary>
        UnknownUnit = 8,

        /// <summary>A bracketed unit suffix that does not follow a numeral: after a reference, a call or a parenthesis.</summary>
        UnitNotAllowedHere = 9,

        /// <summary>A unit written any other way: <c>100 mm</c>, <c>100mm</c>, <c>12"</c>, <c>10'6"</c> or <c>1 1/8</c>.</summary>
        UnitSyntaxNotSupported = 10,

        /// <summary>A <c>#</c> not followed by a complete D-format GUID: a fragment is never parsed (P1.7, P3.9).</summary>
        InvalidQualifier = 11,

        /// <summary>A guard or a limit was exceeded; <see cref="ExpressionDiagnostic.Limit"/> says which.</summary>
        LimitExceeded = 12,
    }
}
