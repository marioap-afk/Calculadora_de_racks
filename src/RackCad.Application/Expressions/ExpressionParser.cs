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
    /// after binding, against a context this type never sees. That is why <c>MIN</c>, <c>Rack.Frentes</c> or
    /// <c>UNKNOWN(A)</c> parse: the parser hands over the form, and the binder owns the answer.
    /// </para>
    /// <para>
    /// Pipeline, each step fail-closed: text guard → blank check → lexer (every lexical error is collected) → parser
    /// (stops at the first syntax error). A lexical error stops the pipeline before parsing, so a user sees what was
    /// mistyped rather than the consequences of the mistake.
    /// </para>
    /// </summary>
    public static class ExpressionParser
    {
        /// <summary>
        /// Parser guard on the text, in Unicode characters (code points), never below 4000 (P1.9). It protects the
        /// parser only: it is not a limit of the tree, and a qualified reference alone is 37 characters long.
        /// </summary>
        public const int MaxTextLength = 4000;

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

            if (TryFindTextExcess(text, out var excessStart))
            {
                return ExpressionParseResult.Failure(
                    text,
                    new[]
                    {
                        ExpressionDiagnostic.LimitExceeded(
                            ExpressionLimitKind.TextLength,
                            MaxTextLength,
                            SourceSpan.FromBounds(excessStart, text.Length)),
                    });
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

        /// <summary>
        /// Finds where the text goes past <see cref="MaxTextLength"/> characters, counting a surrogate pair as one
        /// character. It never reads beyond that point, so an arbitrarily long input costs a bounded scan.
        /// </summary>
        private static bool TryFindTextExcess(string text, out int excessStart)
        {
            excessStart = -1;

            if (text.Length <= MaxTextLength)
            {
                return false;
            }

            var characters = 0;

            for (var index = 0; index < text.Length; index++)
            {
                if (characters == MaxTextLength)
                {
                    excessStart = index;
                    return true;
                }

                characters++;

                if (char.IsHighSurrogate(text[index]) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
                {
                    index++;
                }
            }

            return false;
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
