using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The classes of the closed diagnostic catalogue (P15.3). The lexer and the parser produce only
    /// <see cref="SyntaxAndLimits"/>; the binder adds <see cref="Binding"/>, <c>BrokenReference</c> and
    /// <c>InvalidArguments</c>, plus <c>LimitExceeded</c> for the normative limits of the bound tree; the evaluator adds
    /// <see cref="Semantic"/> results. <see cref="BoundaryContract"/> (<c>OutOfRange</c>) belongs to the Project Variables
    /// adapter and to property consumers, never to the core.
    /// </summary>
    public enum ExpressionDiagnosticClass
    {
        SyntaxAndLimits = 1,

        Binding = 2,

        Semantic = 3,

        BoundaryContract = 4,
    }

    /// <summary>
    /// Every diagnostic in V6 is an error (P15.2): there is no warning that lets a caller carry on with a value.
    /// </summary>
    public enum ExpressionDiagnosticSeverity
    {
        Error = 1,
    }

    /// <summary>
    /// Which bound a <see cref="ExpressionDiagnosticCode.LimitExceeded"/> refers to. <see cref="SyntacticNesting"/> and
    /// <see cref="SyntacticTokenCount"/> are the PARSER guards; <see cref="NodeCount"/>, <see cref="BoundExpressionDepth"/>
    /// and <see cref="ArgumentCount"/> are the NORMATIVE limits of the bound tree, validated after binding (P1.9, P7.5,
    /// Amendment A1.5). A parser guard never replaces a normative limit.
    ///
    /// <para>
    /// Value 1 was <c>TextLength</c>, the character guard of G5. Amendment A1 removed it from the contract (A1 §6, option
    /// A) and the value is never reused for another kind of limit.
    /// </para>
    /// </summary>
    public enum ExpressionLimitKind
    {
        /// <summary>Parentheses, unary operators and calls nest deeper than <see cref="ExpressionParser.MaxSyntacticNesting"/>.</summary>
        SyntacticNesting = 2,

        /// <summary>The text has more lexemes than <see cref="ExpressionParser.MaxSyntacticTokens"/> (Amendment A1.2).</summary>
        SyntacticTokenCount = 3,

        /// <summary>The bound tree has more than <see cref="ExpressionLimits.MaxNodeCount"/> nodes.</summary>
        NodeCount = 4,

        /// <summary>The bound tree is deeper than <see cref="ExpressionLimits.MaxBoundExpressionDepth"/>.</summary>
        BoundExpressionDepth = 5,

        /// <summary>A call has more than <see cref="ExpressionLimits.MaxArgumentCount"/> arguments.</summary>
        ArgumentCount = 6,
    }

    /// <summary>
    /// One typed finding about an expression: a stable code, its class and severity, the position while writing, the
    /// identities it concerns and, for a limit, which limit. It deliberately carries NO message (P15.4): the Spanish text
    /// is produced by one text layer outside the core.
    ///
    /// <para>
    /// There is no owner either. The owner of a diagnostic is a project variable or a rack property, and the core
    /// must not know either concept (DR-2); an adapter adds it.
    /// </para>
    /// </summary>
    public sealed class ExpressionDiagnostic
    {
        private static readonly IReadOnlyList<SymbolId> NoSymbols = new ReadOnlyCollection<SymbolId>(Array.Empty<SymbolId>());

        internal ExpressionDiagnostic(ExpressionDiagnosticCode code, SourceSpan span)
            : this(code, span, null, null, null)
        {
            if (code == ExpressionDiagnosticCode.LimitExceeded)
            {
                throw new ArgumentException("A limit diagnostic must say which limit; use LimitExceeded.", nameof(code));
            }
        }

        private ExpressionDiagnostic(
            ExpressionDiagnosticCode code,
            SourceSpan? span,
            ExpressionLimitKind? limit,
            int? limitMaximum,
            IEnumerable<SymbolId> relatedSymbols)
        {
            Code = code;
            Class = ClassOf(code);
            Span = span;
            Limit = limit;
            LimitMaximum = limitMaximum;
            RelatedSymbols = relatedSymbols == null
                ? NoSymbols
                : new ReadOnlyCollection<SymbolId>(relatedSymbols.ToList());
        }

        public ExpressionDiagnosticCode Code { get; }

        public ExpressionDiagnosticClass Class { get; }

        public ExpressionDiagnosticSeverity Severity => ExpressionDiagnosticSeverity.Error;

        /// <summary>Where, in the text being written. Every diagnostic of writing has one; evaluating a tree has no text.</summary>
        public SourceSpan? Span { get; }

        /// <summary>For <see cref="ExpressionDiagnosticCode.LimitExceeded"/>, the bound that was exceeded; otherwise null.</summary>
        public ExpressionLimitKind? Limit { get; }

        /// <summary>For <see cref="ExpressionDiagnosticCode.LimitExceeded"/>, the largest value the bound admits; otherwise null.</summary>
        public int? LimitMaximum { get; }

        /// <summary>
        /// The identities the finding is about (P15.1): the absent id of <c>BrokenReference</c>, the id a qualifier named,
        /// the symbol out of scope, or EVERY candidate of <c>AmbiguousName</c> in deterministic order, whose qualified form
        /// the formatter writes. Empty when there is none.
        /// </summary>
        public IReadOnlyList<SymbolId> RelatedSymbols { get; }

        internal static ExpressionDiagnostic LimitExceeded(ExpressionLimitKind limit, int maximum, SourceSpan span)
            => new ExpressionDiagnostic(ExpressionDiagnosticCode.LimitExceeded, span, limit, maximum, null);

        /// <summary>A binding or semantic finding, with or without a position and with the identities it concerns.</summary>
        internal static ExpressionDiagnostic About(ExpressionDiagnosticCode code, SourceSpan? span, IEnumerable<SymbolId> relatedSymbols)
        {
            if (code == ExpressionDiagnosticCode.LimitExceeded)
            {
                throw new ArgumentException("A limit diagnostic must say which limit; use LimitExceeded.", nameof(code));
            }

            return new ExpressionDiagnostic(code, span, null, null, relatedSymbols);
        }

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

        /// <summary>Stable sort by <see cref="Compare"/>: findings that compare equal keep the order in which they were found.</summary>
        internal static IReadOnlyList<ExpressionDiagnostic> Ordered(IEnumerable<ExpressionDiagnostic> diagnostics, int? cap)
        {
            var sorted = diagnostics.OrderBy(diagnostic => diagnostic, Comparer<ExpressionDiagnostic>.Create(Compare));
            var list = cap.HasValue ? sorted.Take(cap.Value).ToList() : sorted.ToList();
            return new ReadOnlyCollection<ExpressionDiagnostic>(list);
        }

        public override string ToString() => Span.HasValue ? Code + "@" + Span.Value : Code.ToString();

        /// <summary>A closed classification: a code that is not classified here is a programming error, not a default.</summary>
        internal static ExpressionDiagnosticClass ClassOf(ExpressionDiagnosticCode code)
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

                case ExpressionDiagnosticCode.UnknownSymbol:
                case ExpressionDiagnosticCode.AmbiguousName:
                case ExpressionDiagnosticCode.UnknownNamespace:
                case ExpressionDiagnosticCode.UnknownFunction:
                case ExpressionDiagnosticCode.ScopeViolation:
                case ExpressionDiagnosticCode.ReservedName:
                case ExpressionDiagnosticCode.NameRequired:
                case ExpressionDiagnosticCode.QualifiedNameMismatch:
                case ExpressionDiagnosticCode.OperatorInName:
                    return ExpressionDiagnosticClass.Binding;

                case ExpressionDiagnosticCode.BrokenReference:
                case ExpressionDiagnosticCode.Cycle:
                case ExpressionDiagnosticCode.DependencyFailed:
                case ExpressionDiagnosticCode.InvalidArguments:
                case ExpressionDiagnosticCode.DivisionByZero:
                case ExpressionDiagnosticCode.NonFiniteResult:
                case ExpressionDiagnosticCode.NonCanonicalForm:
                    return ExpressionDiagnosticClass.Semantic;

                case ExpressionDiagnosticCode.OutOfRange:
                    return ExpressionDiagnosticClass.BoundaryContract;

                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, "Unclassified expression diagnostic code.");
            }
        }
    }
}
