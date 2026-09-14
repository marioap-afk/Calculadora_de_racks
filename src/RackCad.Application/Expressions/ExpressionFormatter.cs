using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RackCad.Application.Units;

namespace RackCad.Application.Expressions
{
    public enum CanonicalShapeKind
    {
        Literal = 1,
        DirectReference = 2,
        Expression = 3,
    }

    /// <summary>
    /// The neutral classification of a bound tree into its canonical shape (I-49, Proposal V6 P2.8; ADR-0040 D4): one
    /// representation per meaning.
    ///
    /// <list type="bullet">
    /// <item><description><c>Reference(projectVariable X)</c> alone → <see cref="CanonicalShapeKind.DirectReference"/>
    /// (X), so <c>=Holgura</c> and choosing Holgura in the list are the same thing;</description></item>
    /// <item><description>a number without unit, or the negation of one → <see cref="CanonicalShapeKind.Literal"/>, so
    /// <c>=6</c> is the same as <c>6</c>;</description></item>
    /// <item><description>anything else —an operation, a function or an explicit unit, even <c>=4[in]</c>— →
    /// <see cref="CanonicalShapeKind.Expression"/>.</description></item>
    /// </list>
    ///
    /// <para>
    /// Only the classification lives here. What each surface stores for each shape —the cases of
    /// <c>VariableDefinition</c> and <c>LinkedPropertySource</c>— and the literal rules of each surface belong to G8 and
    /// later. Reading a value of another shape throws.
    /// </para>
    /// </summary>
    public sealed class CanonicalShape
    {
        private readonly double _literal;
        private readonly SymbolId _reference;
        private readonly BoundExpression _expression;

        private CanonicalShape(CanonicalShapeKind kind, double literal, SymbolId reference, BoundExpression expression)
        {
            Kind = kind;
            _literal = literal;
            _reference = reference;
            _expression = expression;
        }

        public CanonicalShapeKind Kind { get; }

        public double LiteralValue
            => Kind == CanonicalShapeKind.Literal ? _literal : throw new InvalidOperationException("This shape is not a literal.");

        public SymbolId Reference
            => Kind == CanonicalShapeKind.DirectReference ? _reference : throw new InvalidOperationException("This shape is not a direct reference.");

        public BoundExpression Expression
            => Kind == CanonicalShapeKind.Expression ? _expression : throw new InvalidOperationException("This shape is not an expression.");

        public static CanonicalShape Classify(BoundExpression expression)
        {
            switch (expression ?? throw new ArgumentNullException(nameof(expression)))
            {
                case BoundReference reference when reference.Symbol.Namespace == SymbolNamespace.ProjectVariable:
                    return new CanonicalShape(CanonicalShapeKind.DirectReference, 0, reference.Symbol, null);

                case BoundNumber number when !number.Unit.HasValue:
                    return new CanonicalShape(CanonicalShapeKind.Literal, number.Value, null, null);

                case BoundNegate negate when negate.Operand is BoundNumber operand && !operand.Unit.HasValue:
                    return new CanonicalShape(CanonicalShapeKind.Literal, -operand.Value, null, null);

                default:
                    return new CanonicalShape(CanonicalShapeKind.Expression, 0, null, expression);
            }
        }
    }

    /// <summary>
    /// The ONE formatter (I-49, Proposal V6 P4.2–P4.7, §4.3; ADR-0040 D6): deterministic and invariant, from a bound tree
    /// and the CURRENT names of a snapshot.
    ///
    /// <list type="bullet">
    /// <item><description>numbers in their shortest round-trip form, WITHOUT an exponent and never with <c>"0.###"</c>;
    /// the unit glued (<c>100[mm]</c>);</description></item>
    /// <item><description>one space on each side of a binary operator; <c>MIN(a, b)</c>, functions upper case;</description></item>
    /// <item><description>the minimum parentheses that keep the exact tree: a left operand of the same precedence needs
    /// none, a right one does, because the parser associates to the left;</description></item>
    /// <item><description>each reference in its minimum unambiguous form within the snapshot: <c>#id</c> for an absent id,
    /// <c>Name</c> for a unique safe name, <c>{Name}</c> with <c>}}</c> for any other or reserved name, and the name form
    /// plus <c>#id</c> for a homonym, with the complete D-form GUID in lower case.</description></item>
    /// </list>
    ///
    /// <para>
    /// A name is "safe" only when THE lexer of the core reads it alone as one bare-name token covering all of it and it is
    /// not reserved: no second set of lexical rules exists to disagree with the parser. Together with the parser guards of
    /// Amendment A1 and the normative limits, this is what makes P2.5 unconditional: the text of every canonical tree
    /// without broken references binds back to the same tree in the same snapshot, whatever the length of its names. The
    /// shown form depends on the snapshot (P4.7); what is persisted never does.
    /// </para>
    /// </summary>
    public static class ExpressionFormatter
    {
        private const int AdditivePrecedence = 1;
        private const int MultiplicativePrecedence = 2;
        private const int NegatePrecedence = 3;
        private const int PrimaryPrecedence = 4;

        public static string Format(BoundExpression expression, SymbolTable symbols)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (symbols == null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            var references = new Dictionary<SymbolId, string>();
            var builder = new StringBuilder();
            var pending = new Stack<object>();
            pending.Push(expression);

            while (pending.Count > 0)
            {
                var item = pending.Pop();

                if (item is string text)
                {
                    builder.Append(text);
                    continue;
                }

                switch ((BoundExpression)item)
                {
                    case BoundNumber number:
                        builder.Append(FormatNumber(number.Value));
                        if (number.Unit.HasValue)
                        {
                            builder.Append('[').Append(LengthUnits.Authority.Token(number.Unit.Value)).Append(']');
                        }

                        break;

                    case BoundReference reference:
                        if (!references.TryGetValue(reference.Symbol, out var written))
                        {
                            written = FormatReference(reference.Symbol, symbols);
                            references.Add(reference.Symbol, written);
                        }

                        builder.Append(written);
                        break;

                    case BoundNegate negate:
                        builder.Append('-');
                        PushOperand(pending, negate.Operand, PrecedenceOf(negate.Operand) < NegatePrecedence);
                        break;

                    case BoundBinary binary:
                        var precedence = PrecedenceOf(binary.Operator);
                        PushOperand(pending, binary.Right, PrecedenceOf(binary.Right) <= precedence);
                        pending.Push(" " + SymbolOf(binary.Operator) + " ");
                        PushOperand(pending, binary.Left, PrecedenceOf(binary.Left) < precedence);
                        break;

                    case BoundCall call:
                        builder.Append(FunctionRegistry.Productive.Token(call.Function)).Append('(');
                        pending.Push(")");

                        for (var index = call.Arguments.Count - 1; index >= 0; index--)
                        {
                            pending.Push(call.Arguments[index]);

                            if (index > 0)
                            {
                                pending.Push(", ");
                            }
                        }

                        break;

                    default:
                        throw new InvalidOperationException("Unknown bound node " + item.GetType().Name + ".");
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// The qualified form of a symbol —its name form plus <c>#id</c>— whatever its homonyms: the form a homonym is
        /// written in, and how every candidate of <c>AmbiguousName</c> is shown (P7.1, §4.2 rule 7).
        /// </summary>
        public static string FormatQualifiedReference(SymbolEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            return NameForm(entry.DisplayName) + "#" + entry.Id.Key.ToLowerInvariant();
        }

        /// <summary>
        /// The shortest decimal that reads back to the same double, without exponent: the digits of the round-trip form,
        /// with the point moved by the exponent. One token however long (Amendment A1.3); <c>double.MaxValue</c> takes 309
        /// characters and <c>double.Epsilon</c> 326.
        /// </summary>
        internal static string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Only finite numbers are formatted.");
            }

            var roundTrip = Math.Abs(value).ToString("R", CultureInfo.InvariantCulture);
            var exponentAt = roundTrip.IndexOfAny(new[] { 'E', 'e' });
            var mantissa = exponentAt < 0 ? roundTrip : roundTrip.Substring(0, exponentAt);
            var exponent = exponentAt < 0
                ? 0
                : int.Parse(roundTrip.Substring(exponentAt + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

            var point = mantissa.IndexOf('.');
            var digits = point < 0 ? mantissa : mantissa.Remove(point, 1);
            var integerDigits = (point < 0 ? mantissa.Length : point) + exponent;

            string plain;

            if (integerDigits <= 0)
            {
                plain = "0." + new string('0', -integerDigits) + digits;
            }
            else if (integerDigits >= digits.Length)
            {
                plain = digits + new string('0', integerDigits - digits.Length);
            }
            else
            {
                plain = digits.Substring(0, integerDigits) + "." + digits.Substring(integerDigits);
            }

            var dot = plain.IndexOf('.');
            var integerPart = (dot < 0 ? plain : plain.Substring(0, dot)).TrimStart('0');
            var fractionPart = dot < 0 ? string.Empty : plain.Substring(dot + 1).TrimEnd('0');
            var normalized = (integerPart.Length == 0 ? "0" : integerPart) + (fractionPart.Length == 0 ? string.Empty : "." + fractionPart);

            return double.IsNegative(value) ? "-" + normalized : normalized;
        }

        private static string FormatReference(SymbolId id, SymbolTable symbols)
        {
            if (!symbols.TryGet(id, out var entry))
            {
                // P4.5: the absent id alone, without taking a name from anywhere else. It displays, it never binds.
                return "#" + id.Key.ToLowerInvariant();
            }

            return symbols.FindByDisplayName(entry.DisplayName).Count > 1
                ? FormatQualifiedReference(entry)
                : NameForm(entry.DisplayName);
        }

        private static string NameForm(string name)
            => IsSafeBareName(name) ? name : "{" + name.Replace("}", "}}") + "}";

        /// <summary>Whether the core lexer reads the name alone as ONE bare-name token covering it all, and it is not reserved.</summary>
        internal static bool IsSafeBareName(string name)
        {
            if (name.Length == 0 || ExpressionReservedNames.IsReserved(name))
            {
                return false;
            }

            var lexed = ExpressionLexer.Tokenize(name);

            return lexed.Diagnostics.Count == 0
                   && lexed.Tokens.Count == 2
                   && lexed.Tokens[0].Kind == ExpressionTokenKind.Name
                   && lexed.Tokens[0].Span.Start == 0
                   && lexed.Tokens[0].Span.Length == name.Length;
        }

        private static void PushOperand(Stack<object> pending, BoundExpression operand, bool parenthesize)
        {
            if (parenthesize)
            {
                pending.Push(")");
                pending.Push(operand);
                pending.Push("(");
            }
            else
            {
                pending.Push(operand);
            }
        }

        private static int PrecedenceOf(BoundExpression expression)
        {
            switch (expression)
            {
                case BoundBinary binary: return PrecedenceOf(binary.Operator);
                case BoundNegate _: return NegatePrecedence;
                case BoundNumber number when double.IsNegative(number.Value): return NegatePrecedence;
                default: return PrimaryPrecedence;
            }
        }

        private static int PrecedenceOf(BoundBinaryOperator binaryOperator)
            => binaryOperator == BoundBinaryOperator.Add || binaryOperator == BoundBinaryOperator.Subtract
                ? AdditivePrecedence
                : MultiplicativePrecedence;

        private static string SymbolOf(BoundBinaryOperator binaryOperator)
        {
            switch (binaryOperator)
            {
                case BoundBinaryOperator.Add: return "+";
                case BoundBinaryOperator.Subtract: return "-";
                case BoundBinaryOperator.Multiply: return "*";
                case BoundBinaryOperator.Divide: return "/";
                default: throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, "Undeclared binary operator.");
            }
        }
    }
}
