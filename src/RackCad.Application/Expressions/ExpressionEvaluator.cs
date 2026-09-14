using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    public enum EvaluationOutcome
    {
        Success = 1,
        Failed = 2,
    }

    /// <summary>
    /// The minimal, opt-in trace of one evaluation (P16.3): the tree that was evaluated and the value supplied for each
    /// present symbol it reads, in first-occurrence order. It is not an explanation for a user, it is not persisted, and
    /// asking for it never changes a result.
    /// </summary>
    public sealed class EvaluationTrace
    {
        internal EvaluationTrace(BoundExpression expression, IReadOnlyList<KeyValuePair<SymbolId, double>> inputs)
        {
            Expression = expression;
            Inputs = inputs;
        }

        public BoundExpression Expression { get; }

        public IReadOnlyList<KeyValuePair<SymbolId, double>> Inputs { get; }
    }

    /// <summary>
    /// The result of evaluating one tree (P14.4, P15.5): a finite value and no diagnostics, or diagnostics and NO value.
    /// Reading the value of a failed result throws: there is no partial value, no last good value and no fallback.
    /// </summary>
    public sealed class EvaluationResult
    {
        private static readonly IReadOnlyList<ExpressionDiagnostic> NoDiagnostics =
            new ReadOnlyCollection<ExpressionDiagnostic>(Array.Empty<ExpressionDiagnostic>());

        private readonly double _value;

        private EvaluationResult(EvaluationOutcome outcome, double value, IReadOnlyList<ExpressionDiagnostic> diagnostics, EvaluationTrace trace)
        {
            Outcome = outcome;
            _value = value;
            Diagnostics = diagnostics;
            Trace = trace;
        }

        public EvaluationOutcome Outcome { get; }

        public bool Succeeded => Outcome == EvaluationOutcome.Success;

        public double Value => Succeeded ? _value : throw new InvalidOperationException("A failed evaluation has no value.");

        public IReadOnlyList<ExpressionDiagnostic> Diagnostics { get; }

        /// <summary>Null unless the caller asked for it.</summary>
        public EvaluationTrace Trace { get; }

        internal static EvaluationResult Success(double value, EvaluationTrace trace)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "A successful evaluation is finite.");
            }

            return new EvaluationResult(EvaluationOutcome.Success, value, NoDiagnostics, trace);
        }

        internal static EvaluationResult Failure(IEnumerable<ExpressionDiagnostic> diagnostics, EvaluationTrace trace)
        {
            var ordered = ExpressionDiagnostic.Ordered(diagnostics, null);

            if (ordered.Count == 0)
            {
                throw new ArgumentException("A failed evaluation has at least one diagnostic.", nameof(diagnostics));
            }

            return new EvaluationResult(EvaluationOutcome.Failed, 0, ordered, trace);
        }
    }

    /// <summary>
    /// The evaluator of ONE bound tree against values supplied by the caller (I-49, Proposal V6 P5.6, P9.3, P14.4, P14.5;
    /// ADR-0040 D2).
    ///
    /// <list type="bullet">
    /// <item><description>dimensionless: any finite <c>double</c> is a value, and no operation is refused because of a
    /// dimension or a sign; a negative intermediate is legal (P5.6, P28.3);</description></item>
    /// <item><description>a unit is converted when its literal is evaluated, through the single units authority: feet
    /// multiplied by 12, millimetres divided by 25.4 (P9.3);</description></item>
    /// <item><description>fail-closed: a divisor equal to zero, positive or negative, is <c>DivisionByZero</c>, and any
    /// intermediate or final value that is not finite is <c>NonFiniteResult</c>; the first failure ends the evaluation,
    /// without a partial value;</description></item>
    /// <item><description>a reference to an id absent from the snapshot is <c>BrokenReference</c>, and then nothing is
    /// evaluated; a known function with a wrong arity is <c>InvalidArguments</c> before its arguments are
    /// evaluated;</description></item>
    /// <item><description>deterministic bit for bit: operands left to right, fixed operations, no culture, no clock and no
    /// recursion.</description></item>
    /// </list>
    ///
    /// <para>
    /// It is NOT the evaluation of a registry: it never evaluates the definition of a symbol, builds no graph, orders
    /// nothing and detects no cycle (that is G7). The value of every present symbol the tree reads is supplied by the
    /// caller; a missing or non-finite supplied value is a programming error of the caller, not a diagnostic.
    /// </para>
    /// </summary>
    public static class ExpressionEvaluator
    {
        private sealed class Frame
        {
            internal Frame(BoundExpression node)
            {
                Node = node;
            }

            internal BoundExpression Node { get; }

            internal int NextChild { get; set; }
        }

        public static EvaluationResult Evaluate(
            BoundExpression expression,
            ExpressionContext context,
            IReadOnlyDictionary<SymbolId, double> symbolValues,
            bool includeTrace = false)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (symbolValues == null)
            {
                throw new ArgumentNullException(nameof(symbolValues));
            }

            var inputs = new List<KeyValuePair<SymbolId, double>>();
            var broken = new List<ExpressionDiagnostic>();

            foreach (var symbol in ReferencesOf(expression))
            {
                if (!context.Symbols.TryGet(symbol, out _))
                {
                    broken.Add(ExpressionDiagnostic.About(ExpressionDiagnosticCode.BrokenReference, null, new[] { symbol }));
                    continue;
                }

                if (!symbolValues.TryGetValue(symbol, out var value))
                {
                    throw new ArgumentException("No value was supplied for the present symbol " + symbol + ".", nameof(symbolValues));
                }

                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    throw new ArgumentException("The value supplied for " + symbol + " is not finite.", nameof(symbolValues));
                }

                inputs.Add(new KeyValuePair<SymbolId, double>(symbol, value));
            }

            var trace = includeTrace
                ? new EvaluationTrace(expression, new ReadOnlyCollection<KeyValuePair<SymbolId, double>>(inputs))
                : null;

            if (broken.Count > 0)
            {
                return EvaluationResult.Failure(broken, trace);
            }

            return Run(expression, context, symbolValues, trace);
        }

        /// <summary>The distinct references of the tree, in first-occurrence order from left to right.</summary>
        private static IReadOnlyList<SymbolId> ReferencesOf(BoundExpression expression)
        {
            var seen = new HashSet<SymbolId>();
            var found = new List<SymbolId>();
            var pending = new Stack<BoundExpression>();
            pending.Push(expression);

            while (pending.Count > 0)
            {
                switch (pending.Pop())
                {
                    case BoundReference reference:
                        if (seen.Add(reference.Symbol))
                        {
                            found.Add(reference.Symbol);
                        }

                        break;

                    case BoundNegate negate:
                        pending.Push(negate.Operand);
                        break;

                    case BoundBinary binary:
                        pending.Push(binary.Right);
                        pending.Push(binary.Left);
                        break;

                    case BoundCall call:
                        for (var index = call.Arguments.Count - 1; index >= 0; index--)
                        {
                            pending.Push(call.Arguments[index]);
                        }

                        break;
                }
            }

            return found;
        }

        /// <summary>Post-order evaluation with an explicit stack of frames and a stack of values.</summary>
        private static EvaluationResult Run(
            BoundExpression expression,
            ExpressionContext context,
            IReadOnlyDictionary<SymbolId, double> symbolValues,
            EvaluationTrace trace)
        {
            var frames = new Stack<Frame>();
            var values = new Stack<double>();
            frames.Push(new Frame(expression));

            while (frames.Count > 0)
            {
                var frame = frames.Peek();
                double result;

                switch (frame.Node)
                {
                    case BoundNumber number:
                        result = number.Unit.HasValue ? context.Units.ToInches(number.Value, number.Unit.Value) : number.Value;
                        break;

                    case BoundReference reference:
                        result = symbolValues[reference.Symbol];
                        break;

                    case BoundNegate negate:
                        if (frame.NextChild == 0)
                        {
                            frame.NextChild = 1;
                            frames.Push(new Frame(negate.Operand));
                            continue;
                        }

                        result = -values.Pop();
                        break;

                    case BoundBinary binary:
                        if (frame.NextChild < 2)
                        {
                            frames.Push(new Frame(frame.NextChild == 0 ? binary.Left : binary.Right));
                            frame.NextChild++;
                            continue;
                        }

                        var right = values.Pop();
                        var left = values.Pop();

                        if (binary.Operator == BoundBinaryOperator.Divide && right == 0)
                        {
                            return Failed(ExpressionDiagnosticCode.DivisionByZero, trace);
                        }

                        result = Apply(binary.Operator, left, right);
                        break;

                    case BoundCall call:
                        if (frame.NextChild == 0 && !context.Functions.AcceptsArity(call.Function, call.Arguments.Count))
                        {
                            return Failed(ExpressionDiagnosticCode.InvalidArguments, trace);
                        }

                        if (frame.NextChild < call.Arguments.Count)
                        {
                            frames.Push(new Frame(call.Arguments[frame.NextChild]));
                            frame.NextChild++;
                            continue;
                        }

                        var arguments = new double[call.Arguments.Count];
                        for (var index = arguments.Length - 1; index >= 0; index--)
                        {
                            arguments[index] = values.Pop();
                        }

                        result = context.Functions.Invoke(call.Function, arguments);
                        break;

                    default:
                        throw new InvalidOperationException("Unknown bound node " + frame.Node.GetType().Name + ".");
                }

                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    return Failed(ExpressionDiagnosticCode.NonFiniteResult, trace);
                }

                frames.Pop();
                values.Push(result);
            }

            return EvaluationResult.Success(values.Single(), trace);
        }

        private static double Apply(BoundBinaryOperator binaryOperator, double left, double right)
        {
            switch (binaryOperator)
            {
                case BoundBinaryOperator.Add: return left + right;
                case BoundBinaryOperator.Subtract: return left - right;
                case BoundBinaryOperator.Multiply: return left * right;
                case BoundBinaryOperator.Divide: return left / right;
                default: throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, "Undeclared binary operator.");
            }
        }

        private static EvaluationResult Failed(ExpressionDiagnosticCode code, EvaluationTrace trace)
            => EvaluationResult.Failure(new[] { ExpressionDiagnostic.About(code, null, null) }, trace);
    }
}
