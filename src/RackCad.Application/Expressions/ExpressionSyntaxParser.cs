using System.Collections.Generic;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// Recursive descent over a token stream that the lexer accepted, one method per production of P1.2:
    ///
    /// <code>
    /// expression     = additive
    /// additive       = multiplicative { ("+" | "-") multiplicative }
    /// multiplicative = unary { ("*" | "/") unary }
    /// unary          = ("+" | "-") unary | primary
    /// primary        = quantity | call | reference | "(" expression ")"
    /// </code>
    ///
    /// <para>
    /// Binary chains are loops, not recursion, so a long flat sum costs no stack and does not count as nesting. The
    /// only recursion is through parentheses, unary operators and calls, and the syntactic nesting guard is checked
    /// before each of those descents, so the stack is bounded by <see cref="ExpressionParser.MaxSyntacticNesting"/>
    /// whatever the text.
    /// </para>
    /// <para>
    /// It stops at the first syntax error and returns no tree: a guessed recovery would report errors the user did not
    /// make. Being handed only valid tokens, it never needs to repeat a lexical judgement.
    /// </para>
    /// </summary>
    internal sealed class ExpressionSyntaxParser
    {
        private readonly IReadOnlyList<ExpressionToken> _tokens;
        private int _index;
        private int _openParentheses;
        private ExpressionDiagnostic _failure;

        private ExpressionSyntaxParser(IReadOnlyList<ExpressionToken> tokens)
        {
            _tokens = tokens;
        }

        internal static ExpressionParseResult Parse(string text, IReadOnlyList<ExpressionToken> tokens)
        {
            var parser = new ExpressionSyntaxParser(tokens);
            var syntax = parser.ParseExpression(0);

            if (syntax != null && parser.Current.Kind != ExpressionTokenKind.EndOfText)
            {
                // Top level: nothing is open, so a closing parenthesis here has no partner.
                syntax = parser.Fail(
                    parser.Current.Kind == ExpressionTokenKind.RightParenthesis
                        ? ExpressionDiagnosticCode.UnbalancedParenthesis
                        : ExpressionDiagnosticCode.UnexpectedToken,
                    parser.Current.Span);
            }

            return syntax != null
                ? ExpressionParseResult.Success(text, syntax)
                : ExpressionParseResult.Failure(text, new[] { parser._failure });
        }

        private ExpressionToken Current => _tokens[_index < _tokens.Count ? _index : _tokens.Count - 1];

        private ExpressionToken PeekNext
            => _tokens[_index + 1 < _tokens.Count ? _index + 1 : _tokens.Count - 1];

        private ExpressionToken Advance()
        {
            var token = Current;

            if (_index < _tokens.Count - 1)
            {
                _index++;
            }

            return token;
        }

        private ExpressionSyntax ParseExpression(int nesting) => ParseAdditive(nesting);

        private ExpressionSyntax ParseAdditive(int nesting)
        {
            var left = ParseMultiplicative(nesting);

            while (left != null
                   && (Current.Kind == ExpressionTokenKind.Plus || Current.Kind == ExpressionTokenKind.Minus))
            {
                var op = Advance();
                var right = ParseMultiplicative(nesting);

                if (right == null)
                {
                    return null;
                }

                left = new BinaryExpressionSyntax(
                    op.Kind == ExpressionTokenKind.Plus ? SyntaxBinaryOperator.Add : SyntaxBinaryOperator.Subtract,
                    op.Span,
                    left,
                    right);
            }

            return left;
        }

        private ExpressionSyntax ParseMultiplicative(int nesting)
        {
            var left = ParseUnary(nesting);

            while (left != null
                   && (Current.Kind == ExpressionTokenKind.Star || Current.Kind == ExpressionTokenKind.Slash))
            {
                var op = Advance();
                var right = ParseUnary(nesting);

                if (right == null)
                {
                    return null;
                }

                left = new BinaryExpressionSyntax(
                    op.Kind == ExpressionTokenKind.Star ? SyntaxBinaryOperator.Multiply : SyntaxBinaryOperator.Divide,
                    op.Span,
                    left,
                    right);
            }

            return left;
        }

        private ExpressionSyntax ParseUnary(int nesting)
        {
            if (Current.Kind != ExpressionTokenKind.Plus && Current.Kind != ExpressionTokenKind.Minus)
            {
                return ParsePrimary(nesting);
            }

            if (!Enter(nesting, Current.Span))
            {
                return null;
            }

            var op = Advance();
            var operand = ParseUnary(nesting + 1);

            return operand == null
                ? null
                : new UnaryExpressionSyntax(
                    op.Kind == ExpressionTokenKind.Plus ? SyntaxUnaryOperator.Plus : SyntaxUnaryOperator.Minus,
                    op.Span,
                    operand);
        }

        private ExpressionSyntax ParsePrimary(int nesting)
        {
            var token = Current;

            switch (token.Kind)
            {
                case ExpressionTokenKind.Number:
                    return ParseQuantity();

                case ExpressionTokenKind.Name:
                    return PeekNext.Kind == ExpressionTokenKind.LeftParenthesis && token.Text.IndexOf(' ') < 0
                        ? ParseCall(nesting)
                        : ParseBareNameReference();

                case ExpressionTokenKind.BracedName:
                    Advance();
                    return WithOptionalQualifier(new NameSyntax(token.Text, true, token.Span));

                case ExpressionTokenKind.Qualifier:
                    Advance();
                    return new ReferenceSyntax(null, new QualifierSyntax(token.Id, token.Text, token.Span));

                case ExpressionTokenKind.LeftParenthesis:
                    return ParseParenthesized(nesting);

                case ExpressionTokenKind.RightParenthesis:
                    return Fail(
                        _openParentheses == 0
                            ? ExpressionDiagnosticCode.UnbalancedParenthesis
                            : ExpressionDiagnosticCode.UnexpectedToken,
                        token.Span);

                default:
                    // An operator, a comma, a dot or the end of the text where an operand is needed.
                    return Fail(ExpressionDiagnosticCode.UnexpectedToken, token.Span);
            }
        }

        /// <summary><c>number [ "[" unit "]" ]</c>: the lexer already accepted the unit, so it only has to be attached.</summary>
        private ExpressionSyntax ParseQuantity()
        {
            var number = Advance();

            if (Current.Kind != ExpressionTokenKind.UnitSuffix)
            {
                return new NumberSyntax(number.Span, number.Text, number.Value, number.Span, null, null);
            }

            var unit = Advance();

            return new NumberSyntax(
                SourceSpan.FromBounds(number.Span.Start, unit.Span.End),
                number.Text,
                number.Value,
                number.Span,
                unit.Text,
                unit.Span);
        }

        /// <summary>
        /// A bare name: a reference, possibly qualified, or the reserved namespace form <c>palabra.</c>. Like the name
        /// of a call, the namespace is ONE word: a dot after a multi-word name is not namespace syntax.
        /// </summary>
        private ExpressionSyntax ParseBareNameReference()
        {
            var token = Advance();
            var name = new NameSyntax(token.Text, false, token.Span);

            if (Current.Kind != ExpressionTokenKind.Dot || token.Text.IndexOf(' ') >= 0)
            {
                return WithOptionalQualifier(name);
            }

            var dot = Advance();

            if (Current.Kind != ExpressionTokenKind.Name)
            {
                return new NamespaceReferenceSyntax(name, dot.Span, null);
            }

            var member = Advance();
            return new NamespaceReferenceSyntax(name, dot.Span, new NameSyntax(member.Text, false, member.Span));
        }

        private ExpressionSyntax WithOptionalQualifier(NameSyntax name)
        {
            if (Current.Kind != ExpressionTokenKind.Qualifier)
            {
                return new ReferenceSyntax(name, null);
            }

            var qualifier = Advance();
            return new ReferenceSyntax(name, new QualifierSyntax(qualifier.Id, qualifier.Text, qualifier.Span));
        }

        /// <summary><c>word "(" [ expression { "," expression } ] ")"</c>, one nesting level for the whole call.</summary>
        private ExpressionSyntax ParseCall(int nesting)
        {
            var function = Current;
            var open = PeekNext;

            if (!Enter(nesting, SourceSpan.FromBounds(function.Span.Start, open.Span.End)))
            {
                return null;
            }

            Advance();
            Advance();
            _openParentheses++;

            var name = new NameSyntax(function.Text, false, function.Span);
            var arguments = new List<ExpressionSyntax>();

            if (Current.Kind == ExpressionTokenKind.RightParenthesis)
            {
                return CloseCall(name, arguments);
            }

            while (true)
            {
                var argument = ParseExpression(nesting + 1);

                if (argument == null)
                {
                    return null;
                }

                arguments.Add(argument);

                if (Current.Kind == ExpressionTokenKind.Comma)
                {
                    Advance();
                    continue;
                }

                if (Current.Kind == ExpressionTokenKind.RightParenthesis)
                {
                    return CloseCall(name, arguments);
                }

                return Current.Kind == ExpressionTokenKind.EndOfText
                    ? Fail(ExpressionDiagnosticCode.UnbalancedParenthesis, open.Span)
                    : Fail(ExpressionDiagnosticCode.UnexpectedToken, Current.Span);
            }
        }

        private ExpressionSyntax CloseCall(NameSyntax name, List<ExpressionSyntax> arguments)
        {
            var close = Advance();
            _openParentheses--;
            return new CallExpressionSyntax(name, arguments, SourceSpan.FromBounds(name.Span.Start, close.Span.End));
        }

        private ExpressionSyntax ParseParenthesized(int nesting)
        {
            var open = Current;

            if (!Enter(nesting, open.Span))
            {
                return null;
            }

            Advance();
            _openParentheses++;

            var inner = ParseExpression(nesting + 1);

            if (inner == null)
            {
                return null;
            }

            if (Current.Kind == ExpressionTokenKind.RightParenthesis)
            {
                var close = Advance();
                _openParentheses--;
                return new ParenthesizedExpressionSyntax(inner, SourceSpan.FromBounds(open.Span.Start, close.Span.End));
            }

            return Current.Kind == ExpressionTokenKind.EndOfText
                ? Fail(ExpressionDiagnosticCode.UnbalancedParenthesis, open.Span)
                : Fail(ExpressionDiagnosticCode.UnexpectedToken, Current.Span);
        }

        /// <summary>The nesting guard of P1.9, checked before descending into one more level.</summary>
        private bool Enter(int nesting, SourceSpan opener)
        {
            if (nesting + 1 <= ExpressionParser.MaxSyntacticNesting)
            {
                return true;
            }

            Fail(ExpressionDiagnostic.LimitExceeded(ExpressionLimitKind.SyntacticNesting, ExpressionParser.MaxSyntacticNesting, opener));
            return false;
        }

        private ExpressionSyntax Fail(ExpressionDiagnosticCode code, SourceSpan span)
            => Fail(new ExpressionDiagnostic(code, span));

        private ExpressionSyntax Fail(ExpressionDiagnostic diagnostic)
        {
            if (_failure == null)
            {
                _failure = diagnostic;
            }

            return null;
        }
    }
}
