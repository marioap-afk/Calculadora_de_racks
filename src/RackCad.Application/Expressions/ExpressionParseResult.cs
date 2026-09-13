using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The outcome of parsing: a syntax tree with no diagnostics, or diagnostics with no tree — never both, never a
    /// partial tree (P15.5).
    ///
    /// <para>
    /// Reading <see cref="Syntax"/> on a failure throws instead of returning null, with the same discipline as
    /// <c>VariableDefinition.LiteralValue</c>: a caller that forgot to look at <see cref="Succeeded"/> must not carry a
    /// null forward as if it were an expression.
    /// </para>
    /// </summary>
    public sealed class ExpressionParseResult
    {
        private readonly ExpressionSyntax _syntax;

        private ExpressionParseResult(string text, ExpressionSyntax syntax, IReadOnlyList<ExpressionDiagnostic> diagnostics)
        {
            Text = text;
            _syntax = syntax;
            Diagnostics = diagnostics;
        }

        /// <summary>The text that was parsed, verbatim: spans index into it.</summary>
        public string Text { get; }

        public bool Succeeded => _syntax != null;

        public ExpressionSyntax Syntax
        {
            get
            {
                if (_syntax == null)
                {
                    throw new InvalidOperationException("The text is not a valid expression; read Diagnostics instead.");
                }

                return _syntax;
            }
        }

        /// <summary>Empty on success; on failure, in the deterministic order of P15.7.</summary>
        public IReadOnlyList<ExpressionDiagnostic> Diagnostics { get; }

        internal static ExpressionParseResult Success(string text, ExpressionSyntax syntax)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (syntax == null)
            {
                throw new ArgumentNullException(nameof(syntax));
            }

            return new ExpressionParseResult(text, syntax, Array.Empty<ExpressionDiagnostic>());
        }

        internal static ExpressionParseResult Failure(string text, IEnumerable<ExpressionDiagnostic> diagnostics)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            var ordered = diagnostics.ToList();

            if (ordered.Count == 0 || ordered.Any(diagnostic => diagnostic == null))
            {
                throw new ArgumentException("A failed parse needs at least one diagnostic and no null ones.", nameof(diagnostics));
            }

            // OrderBy is stable: diagnostics that compare equal keep the order in which they were found.
            var sorted = ordered.OrderBy(diagnostic => diagnostic, Comparer<ExpressionDiagnostic>.Create(ExpressionDiagnostic.Compare)).ToList();

            return new ExpressionParseResult(text, null, new ReadOnlyCollection<ExpressionDiagnostic>(sorted));
        }
    }
}
