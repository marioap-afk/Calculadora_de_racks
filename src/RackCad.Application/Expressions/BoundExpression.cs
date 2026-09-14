using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Units;

namespace RackCad.Application.Expressions
{
    /// <summary>The closed set of bound nodes (I-49, Proposal V6 P2.1 and P2.4).</summary>
    public enum BoundExpressionKind
    {
        Number = 1,
        Reference = 2,
        Negate = 3,
        Binary = 4,
        Call = 5,
    }

    public enum BoundBinaryOperator
    {
        Add = 1,
        Subtract = 2,
        Multiply = 3,
        Divide = 4,
    }

    /// <summary>
    /// The bound model of V6 P2.1 (ADR-0040 D4): what an expression MEANS, with identities instead of names. It is what
    /// G8 will persist, what the evaluator evaluates and what the formatter writes.
    ///
    /// <para>
    /// Immutable and closed: <c>Number(double, optional unit)</c>, <c>Reference(SymbolId)</c>, <c>Negate(operand)</c>,
    /// <c>Binary(Add | Subtract | Multiply | Divide, left, right)</c> and <c>Call(FunctionId, args)</c>. No positions, no
    /// names, no redundant parentheses, no unary plus and NO type annotations (DR-5): the engine is dimensionless. Nothing
    /// here knows project variables, racks or persistence.
    /// </para>
    /// <para>
    /// Equality is structural by value (P2.3): tokens compare ordinally, numbers with <c>double.Equals</c>, references
    /// with the identity of their namespace, and the unit is part of the equality (<c>100[mm]</c> is not
    /// <c>3.937…</c>). The hash, the node count and the depth are computed once, when a node is built from its children,
    /// so none of them walks the tree; comparing two trees uses an explicit stack, never recursion.
    /// </para>
    /// <para>
    /// A tree is not checked against a context here: existence of references, arity, scope, canonical form and the
    /// normative limits are checked against a context by the binder, or on a persisted tree by its reader (P2.2, P7.5).
    /// </para>
    /// </summary>
    public abstract class BoundExpression : IEquatable<BoundExpression>
    {
        private readonly int _hash;

        private protected BoundExpression(long nodeCount, int depth, int hash)
        {
            NodeCount = nodeCount > int.MaxValue ? int.MaxValue : (int)nodeCount;
            Depth = depth;
            _hash = hash;
        }

        public abstract BoundExpressionKind Kind { get; }

        /// <summary>Nodes of the tree, counted as a tree even if a subtree object is shared; saturates at <c>int.MaxValue</c>.</summary>
        public int NodeCount { get; }

        /// <summary>The normative depth of P1.9: nodes on the longest root→leaf path, counting both ends.</summary>
        public int Depth { get; }

        public static BoundNumber Number(double value, LengthUnit? unit = null) => new BoundNumber(value, unit);

        public static BoundReference Reference(SymbolId symbol) => new BoundReference(symbol);

        public static BoundNegate Negate(BoundExpression operand) => new BoundNegate(operand);

        public static BoundBinary Binary(BoundBinaryOperator binaryOperator, BoundExpression left, BoundExpression right)
            => new BoundBinary(binaryOperator, left, right);

        public static BoundCall Call(FunctionId function, IEnumerable<BoundExpression> arguments) => new BoundCall(function, arguments);

        public bool Equals(BoundExpression other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            var pending = new Stack<(BoundExpression Left, BoundExpression Right)>();
            pending.Push((this, other));

            while (pending.Count > 0)
            {
                var (left, right) = pending.Pop();

                if (ReferenceEquals(left, right))
                {
                    continue;
                }

                if (left._hash != right._hash || left.Kind != right.Kind || left.NodeCount != right.NodeCount || left.Depth != right.Depth)
                {
                    return false;
                }

                switch (left)
                {
                    case BoundNumber number:
                        var otherNumber = (BoundNumber)right;
                        if (!number.Value.Equals(otherNumber.Value) || number.Unit != otherNumber.Unit)
                        {
                            return false;
                        }

                        break;

                    case BoundReference reference:
                        if (!reference.Symbol.Equals(((BoundReference)right).Symbol))
                        {
                            return false;
                        }

                        break;

                    case BoundNegate negate:
                        pending.Push((negate.Operand, ((BoundNegate)right).Operand));
                        break;

                    case BoundBinary binary:
                        var otherBinary = (BoundBinary)right;
                        if (binary.Operator != otherBinary.Operator)
                        {
                            return false;
                        }

                        pending.Push((binary.Right, otherBinary.Right));
                        pending.Push((binary.Left, otherBinary.Left));
                        break;

                    case BoundCall call:
                        var otherCall = (BoundCall)right;
                        if (call.Function != otherCall.Function || call.Arguments.Count != otherCall.Arguments.Count)
                        {
                            return false;
                        }

                        for (var index = call.Arguments.Count - 1; index >= 0; index--)
                        {
                            pending.Push((call.Arguments[index], otherCall.Arguments[index]));
                        }

                        break;

                    default:
                        throw new InvalidOperationException("Unknown bound node " + left.GetType().Name + ".");
                }
            }

            return true;
        }

        public override bool Equals(object obj) => obj is BoundExpression other && Equals(other);

        public override int GetHashCode() => _hash;

        /// <summary>
        /// A diagnostic S-expression for assertion messages and debugging, cut after a few thousand characters. It is not
        /// the formatter and not a persisted form: those are the formatter and the token tables.
        /// </summary>
        public override string ToString()
        {
            const int budget = 4000;
            var builder = new StringBuilder();
            var pending = new Stack<object>();
            pending.Push(this);

            while (pending.Count > 0 && builder.Length < budget)
            {
                var item = pending.Pop();

                if (item is string text)
                {
                    builder.Append(text);
                    continue;
                }

                var node = (BoundExpression)item;
                builder.Append('(').Append(BoundExpressionTokens.NodeToken(node));

                switch (node)
                {
                    case BoundNumber number:
                        builder.Append(' ').Append(number.Value.ToString("R", CultureInfo.InvariantCulture));
                        if (number.Unit.HasValue)
                        {
                            builder.Append(' ').Append(LengthUnits.Authority.Token(number.Unit.Value));
                        }

                        builder.Append(')');
                        break;

                    case BoundReference reference:
                        builder.Append(' ').Append(reference.Symbol).Append(')');
                        break;

                    case BoundNegate negate:
                        pending.Push(")");
                        pending.Push(negate.Operand);
                        pending.Push(" ");
                        break;

                    case BoundBinary binary:
                        pending.Push(")");
                        pending.Push(binary.Right);
                        pending.Push(" ");
                        pending.Push(binary.Left);
                        pending.Push(" ");
                        break;

                    case BoundCall call:
                        builder.Append(' ').Append(FunctionRegistry.Productive.Token(call.Function));
                        pending.Push(")");
                        for (var index = call.Arguments.Count - 1; index >= 0; index--)
                        {
                            pending.Push(call.Arguments[index]);
                            pending.Push(" ");
                        }

                        break;
                }
            }

            return pending.Count > 0 ? builder.Append("…").ToString() : builder.ToString();
        }
    }

    /// <summary>
    /// <c>Number(double, optional unit)</c>: the numeral as written, with its unit NOT converted (P17.4). The value is
    /// finite. A binder only ever builds non-negative values —the sign is a <see cref="BoundNegate"/>— which is part of
    /// what makes a tree canonical.
    /// </summary>
    public sealed class BoundNumber : BoundExpression
    {
        internal BoundNumber(double value, LengthUnit? unit)
            : base(1, 1, HashCode.Combine((int)BoundExpressionKind.Number, value, unit.HasValue ? (int)unit.Value : 0))
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "A bound number must be finite.");
            }

            if (unit.HasValue && unit.Value != LengthUnit.Millimeter && unit.Value != LengthUnit.Inch && unit.Value != LengthUnit.Foot)
            {
                throw new ArgumentOutOfRangeException(nameof(unit), unit, "Undeclared length unit.");
            }

            Value = value;
            Unit = unit;
        }

        public override BoundExpressionKind Kind => BoundExpressionKind.Number;

        public double Value { get; }

        /// <summary>The explicit unit, or null for a dimensionless number that a Length consumer reads in inches (P9.4).</summary>
        public LengthUnit? Unit { get; }
    }

    /// <summary><c>Reference(SymbolId)</c>: an identity, never a name (P3.5).</summary>
    public sealed class BoundReference : BoundExpression
    {
        internal BoundReference(SymbolId symbol)
            : base(1, 1, HashCode.Combine((int)BoundExpressionKind.Reference, RequireSymbol(symbol)))
        {
            Symbol = symbol;
        }

        public override BoundExpressionKind Kind => BoundExpressionKind.Reference;

        public SymbolId Symbol { get; }

        private static SymbolId RequireSymbol(SymbolId symbol) => symbol ?? throw new ArgumentNullException(nameof(symbol));
    }

    /// <summary><c>Negate(operand)</c>: the unary minus. A unary plus binds to nothing.</summary>
    public sealed class BoundNegate : BoundExpression
    {
        internal BoundNegate(BoundExpression operand)
            : base(
                (long)RequireOperand(operand).NodeCount + 1,
                SaturatedIncrement(operand.Depth),
                HashCode.Combine((int)BoundExpressionKind.Negate, operand.GetHashCode()))
        {
            Operand = operand;
        }

        public override BoundExpressionKind Kind => BoundExpressionKind.Negate;

        public BoundExpression Operand { get; }

        private static BoundExpression RequireOperand(BoundExpression operand) => operand ?? throw new ArgumentNullException(nameof(operand));

        internal static int SaturatedIncrement(int depth) => depth == int.MaxValue ? depth : depth + 1;
    }

    /// <summary><c>Binary(operator, left, right)</c>: left-associative chains are left-deep trees (P1.3).</summary>
    public sealed class BoundBinary : BoundExpression
    {
        internal BoundBinary(BoundBinaryOperator binaryOperator, BoundExpression left, BoundExpression right)
            : base(
                (long)RequireChild(left, nameof(left)).NodeCount + RequireChild(right, nameof(right)).NodeCount + 1,
                BoundNegate.SaturatedIncrement(Math.Max(left.Depth, right.Depth)),
                HashCode.Combine((int)BoundExpressionKind.Binary, (int)RequireOperator(binaryOperator), left.GetHashCode(), right.GetHashCode()))
        {
            Operator = binaryOperator;
            Left = left;
            Right = right;
        }

        public override BoundExpressionKind Kind => BoundExpressionKind.Binary;

        public BoundBinaryOperator Operator { get; }

        public BoundExpression Left { get; }

        public BoundExpression Right { get; }

        private static BoundExpression RequireChild(BoundExpression child, string name) => child ?? throw new ArgumentNullException(name);

        private static BoundBinaryOperator RequireOperator(BoundBinaryOperator binaryOperator)
            => binaryOperator >= BoundBinaryOperator.Add && binaryOperator <= BoundBinaryOperator.Divide
                ? binaryOperator
                : throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, "Undeclared binary operator.");
    }

    /// <summary>
    /// <c>Call(FunctionId, args)</c>. The arguments are copied. Arity is NOT a construction rule: a persisted tree of a
    /// known function with a wrong arity is a semantic <c>InvalidArguments</c>, not an unreadable tree (P10.4).
    /// </summary>
    public sealed class BoundCall : BoundExpression
    {
        internal BoundCall(FunctionId function, IEnumerable<BoundExpression> arguments)
            : this(RequireFunction(function), CopyArguments(arguments), copied: true)
        {
        }

        private BoundCall(FunctionId function, ReadOnlyCollection<BoundExpression> arguments, bool copied)
            : base(
                1 + arguments.Sum(argument => (long)argument.NodeCount),
                BoundNegate.SaturatedIncrement(arguments.Count == 0 ? 0 : arguments.Max(argument => argument.Depth)),
                HashOf(function, arguments))
        {
            Function = function;
            Arguments = arguments;
        }

        public override BoundExpressionKind Kind => BoundExpressionKind.Call;

        public FunctionId Function { get; }

        public IReadOnlyList<BoundExpression> Arguments { get; }

        private static FunctionId RequireFunction(FunctionId function)
            => function >= FunctionId.Min && function <= FunctionId.Abs
                ? function
                : throw new ArgumentOutOfRangeException(nameof(function), function, "Undeclared function.");

        private static ReadOnlyCollection<BoundExpression> CopyArguments(IEnumerable<BoundExpression> arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var copy = arguments.ToList();

            if (copy.Any(argument => argument == null))
            {
                throw new ArgumentException("A call cannot have a null argument.", nameof(arguments));
            }

            return new ReadOnlyCollection<BoundExpression>(copy);
        }

        private static int HashOf(FunctionId function, IReadOnlyList<BoundExpression> arguments)
        {
            var hash = new HashCode();
            hash.Add((int)BoundExpressionKind.Call);
            hash.Add((int)function);
            hash.Add(arguments.Count);

            foreach (var argument in arguments)
            {
                hash.Add(argument.GetHashCode());
            }

            return hash.ToHashCode();
        }
    }

    /// <summary>
    /// The explicit, closed, two-way token tables of the bound nodes and operators (P2.4, P17.10; ADR-0040 D9): <c>number</c>,
    /// <c>ref</c>, <c>neg</c>, <c>add</c>, <c>sub</c>, <c>mul</c>, <c>div</c> and <c>call</c>, compared ordinally. They are
    /// written by hand: no token comes from <c>ToString()</c>, <c>nameof</c> or a type name, so renaming a C# member cannot
    /// change what is written or read. G6 persists nothing; G8 will persist with these very tables.
    /// </summary>
    public static class BoundExpressionTokens
    {
        private static readonly IReadOnlyList<string> DeclaredNodeTokens =
            new ReadOnlyCollection<string>(new[] { "number", "ref", "neg", "add", "sub", "mul", "div", "call" });

        public static IReadOnlyList<string> NodeTokens => DeclaredNodeTokens;

        public static string NodeToken(BoundExpression expression)
        {
            switch (expression ?? throw new ArgumentNullException(nameof(expression)))
            {
                case BoundNumber _: return "number";
                case BoundReference _: return "ref";
                case BoundNegate _: return "neg";
                case BoundBinary binary: return OperatorToken(binary.Operator);
                case BoundCall _: return "call";
                default: throw new ArgumentOutOfRangeException(nameof(expression), expression.GetType().Name, "Unknown bound node.");
            }
        }

        /// <summary>The kind of a node token and, for the four binary tokens, its operator.</summary>
        public static bool TryParseNodeToken(string token, out BoundExpressionKind kind, out BoundBinaryOperator? binaryOperator)
        {
            binaryOperator = null;

            switch (token)
            {
                case "number":
                    kind = BoundExpressionKind.Number;
                    return true;

                case "ref":
                    kind = BoundExpressionKind.Reference;
                    return true;

                case "neg":
                    kind = BoundExpressionKind.Negate;
                    return true;

                case "call":
                    kind = BoundExpressionKind.Call;
                    return true;
            }

            if (TryParseOperatorToken(token, out var parsed))
            {
                kind = BoundExpressionKind.Binary;
                binaryOperator = parsed;
                return true;
            }

            kind = default;
            return false;
        }

        public static string OperatorToken(BoundBinaryOperator binaryOperator)
        {
            switch (binaryOperator)
            {
                case BoundBinaryOperator.Add: return "add";
                case BoundBinaryOperator.Subtract: return "sub";
                case BoundBinaryOperator.Multiply: return "mul";
                case BoundBinaryOperator.Divide: return "div";
                default: throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, "Undeclared binary operator.");
            }
        }

        public static bool TryParseOperatorToken(string token, out BoundBinaryOperator binaryOperator)
        {
            switch (token)
            {
                case "add":
                    binaryOperator = BoundBinaryOperator.Add;
                    return true;

                case "sub":
                    binaryOperator = BoundBinaryOperator.Subtract;
                    return true;

                case "mul":
                    binaryOperator = BoundBinaryOperator.Multiply;
                    return true;

                case "div":
                    binaryOperator = BoundBinaryOperator.Divide;
                    return true;

                default:
                    binaryOperator = default;
                    return false;
            }
        }
    }
}
