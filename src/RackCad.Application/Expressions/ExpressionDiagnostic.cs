using System;

namespace RackCad.Application.Expressions
{
    /// <summary>The classes of the diagnostic catalogue (P15.3). G5 produces only the syntax-and-limits class.</summary>
    public enum ExpressionDiagnosticClass
    {
        SyntaxAndLimits = 1,
    }

    /// <summary>
    /// Every diagnostic in V6 is an error (P15.2): there is no warning that lets a caller carry on with a value.
    /// </summary>
    public enum ExpressionDiagnosticSeverity
    {
        Error = 1,
    }

    /// <summary>
    /// Which bound a <see cref="ExpressionDiagnosticCode.LimitExceeded"/> refers to. The two members here are the
    /// PARSER guards of P1.9; the normative limits of the bound tree (nodes, depth, arguments) are validated after
    /// binding and are not parser guards.
    /// </summary>
    public enum ExpressionLimitKind
    {
        /// <summary>The text is longer than <see cref="ExpressionParser.MaxTextLength"/> characters.</summary>
        TextLength = 1,

        /// <summary>Parentheses, unary operators and calls nest deeper than <see cref="ExpressionParser.MaxSyntacticNesting"/>.</summary>
        SyntacticNesting = 2,
    }

    /// <summary>
    /// One typed finding about an expression: a stable code, its class and severity, the position while writing and
    /// the data a text layer needs. It deliberately carries NO message (P15.4).
    ///
    /// <para>
    /// There is no owner either. The owner of a diagnostic is a project variable or a rack property, and the core
    /// must not know either concept (DR-2); an adapter adds it.
    /// </para>
    /// </summary>
    public sealed class ExpressionDiagnostic
    {
        internal ExpressionDiagnostic(ExpressionDiagnosticCode code, SourceSpan span)
            : this(code, span, null, null)
        {
            if (code == ExpressionDiagnosticCode.LimitExceeded)
            {
                throw new ArgumentException("A limit diagnostic must say which limit; use LimitExceeded.", nameof(code));
            }
        }

        private ExpressionDiagnostic(ExpressionDiagnosticCode code, SourceSpan? span, ExpressionLimitKind? limit, int? limitMaximum)
        {
            Code = code;
            Class = ClassOf(code);
            Span = span;
            Limit = limit;
            LimitMaximum = limitMaximum;
        }

        public ExpressionDiagnosticCode Code { get; }

        public ExpressionDiagnosticClass Class { get; }

        public ExpressionDiagnosticSeverity Severity => ExpressionDiagnosticSeverity.Error;

        /// <summary>Where, in the text being written. Every syntax diagnostic has one.</summary>
        public SourceSpan? Span { get; }

        /// <summary>For <see cref="ExpressionDiagnosticCode.LimitExceeded"/>, the bound that was exceeded; otherwise null.</summary>
        public ExpressionLimitKind? Limit { get; }

        /// <summary>For <see cref="ExpressionDiagnosticCode.LimitExceeded"/>, the largest value the bound admits; otherwise null.</summary>
        public int? LimitMaximum { get; }

        internal static ExpressionDiagnostic LimitExceeded(ExpressionLimitKind limit, int maximum, SourceSpan span)
            => new ExpressionDiagnostic(ExpressionDiagnosticCode.LimitExceeded, span, limit, maximum);

        /// <summary>
        /// The deterministic order of P15.7: position first, then the catalogue order of the code, then the length.
        /// A diagnostic without a position sorts before any positioned one.
        /// </summary>
        internal static int Compare(ExpressionDiagnostic left, ExpressionDiagnostic right)
        {
            if (left.Span.HasValue != right.Span.HasValue)
            {
                return left.Span.HasValue ? 1 : -1;
            }

            if (left.Span.HasValue)
            {
                var byStart = left.Span.Value.Start.CompareTo(right.Span.Value.Start);
                if (byStart != 0)
                {
                    return byStart;
                }
            }

            var byCode = ((int)left.Code).CompareTo((int)right.Code);
            if (byCode != 0)
            {
                return byCode;
            }

            return left.Span.HasValue ? left.Span.Value.Length.CompareTo(right.Span.Value.Length) : 0;
        }

        public override string ToString() => Span.HasValue ? Code + "@" + Span.Value : Code.ToString();

        /// <summary>A closed classification: a code that is not classified here is a programming error, not a default.</summary>
        private static ExpressionDiagnosticClass ClassOf(ExpressionDiagnosticCode code)
        {
            switch (code)
            {
                case ExpressionDiagnosticCode.EmptyExpression:
                case ExpressionDiagnosticCode.UnexpectedCharacter:
                case ExpressionDiagnosticCode.UnexpectedToken:
                case ExpressionDiagnosticCode.UnbalancedParenthesis:
                case ExpressionDiagnosticCode.UnterminatedName:
                case ExpressionDiagnosticCode.InvalidNumber:
                case ExpressionDiagnosticCode.AmbiguousDecimalComma:
                case ExpressionDiagnosticCode.UnknownUnit:
                case ExpressionDiagnosticCode.UnitNotAllowedHere:
                case ExpressionDiagnosticCode.UnitSyntaxNotSupported:
                case ExpressionDiagnosticCode.InvalidQualifier:
                case ExpressionDiagnosticCode.LimitExceeded:
                    return ExpressionDiagnosticClass.SyntaxAndLimits;

                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, "Unclassified expression diagnostic code.");
            }
        }
    }
}
