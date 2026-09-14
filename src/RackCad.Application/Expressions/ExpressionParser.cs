using System;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The single entry point from typed text to expression syntax (I-49, Proposal V6 P1 and P2.1). Hand-written,
    /// deterministic and culture-independent; no third-party or framework expression engine is involved (P1.10).
    ///
    /// <para>
    /// The text is the part AFTER the surface mark: the <c>=</c> that tells RACKVARIABLES or the linked-property editor
    /// "this is an expression" is not part of the grammar (P1.8), so a leading <c>=</c> here is an unexpected character.
    /// </para>
    /// <para>
    /// Parsing decides SYNTAX only. Whether a name exists, is reserved or is ambiguous, whether a function exists and
    /// takes those arguments, what a unit converts to and whether the tree is within the normative limits are decided
    /// by the binder, against a context this type never sees. That is why <c>MIN</c>, <c>Rack.Frentes</c> or
    /// <c>UNKNOWN(A)</c> parse: the parser hands over the form, and the binder owns the answer.
    /// </para>
    /// <para>
    /// Pipeline, each step fail-closed: blank check → lexer (the syntactic token guard runs while lexing; every lexical
    /// error is collected) → parser (stops at the first syntax error). A lexical error stops the pipeline before parsing,
    /// so a user sees what was mistyped rather than the consequences of the mistake.
    /// </para>
    /// <para>
    /// There is no limit on the number of CHARACTERS (Amendment A1.1, ADR-0040 D8): a legal name has no maximum length,
    /// so any finite character guard would reject the canonical text of a legal tree and break P2.5. The parser is
    /// protected by the number of tokens and by the syntactic nesting instead.
    /// </para>
    /// </summary>
    public static class ExpressionParser
    {
        /// <summary>
        /// Parser guard on the number of lexemes (Amendment A1.2–A1.4). An implementation value, initially 4096 and never
        /// below 6 × the normative maximum of nodes (1536), so the canonical text of any legal tree —at most 6n - 3 tokens
        /// (A1 §4)— always lexes. Every lexeme counts one, whatever its length: a numeral, a unit suffix, a whole bare name
        /// with all its words, a whole braced name, a qualifier, each operator, parenthesis, comma and namespace dot, and
        /// each malformed lexeme. Whitespace and the internal end-of-text mark do not count.
        /// </summary>
        public const int MaxSyntacticTokens = 4096;

        /// <summary>
        /// Parser guard on nested parentheses, unary operators and calls, checked BEFORE descending (P1.9). An
        /// implementation value, initially 64 and never below 24, so that the canonical text of any legal tree parses
        /// again. It is not <c>MaxBoundExpressionDepth</c>: a flat binary chain does not nest syntactically at all.
        /// </summary>
        public const int MaxSyntacticNesting = 64;

        public static ExpressionParseResult Parse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (IsBlank(text))
            {
                return ExpressionParseResult.Failure(
                    text,
                    new[] { new ExpressionDiagnostic(ExpressionDiagnosticCode.EmptyExpression, new SourceSpan(0, text.Length)) });
            }

            var lexed = ExpressionLexer.Tokenize(text);

            if (lexed.Diagnostics.Count > 0)
            {
                return ExpressionParseResult.Failure(text, lexed.Diagnostics);
            }

            return ExpressionSyntaxParser.Parse(text, lexed.Tokens);
        }

        private static bool IsBlank(string text)
        {
            foreach (var character in text)
            {
                if (!ExpressionLexer.IsInsignificantWhitespace(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
