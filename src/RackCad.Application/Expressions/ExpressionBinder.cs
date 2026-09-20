using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The outcome of binding: a bound tree with no diagnostics, or diagnostics with no tree — never both, never a partial
    /// tree (P7.4, P15.5). Reading <see cref="Expression"/> on a failure throws.
    /// </summary>
    public sealed class ExpressionBindResult
    {
        private static readonly IReadOnlyList<ExpressionDiagnostic> NoDiagnostics =
            new ReadOnlyCollection<ExpressionDiagnostic>(Array.Empty<ExpressionDiagnostic>());

        private readonly BoundExpression _expression;

        private ExpressionBindResult(BoundExpression expression, IReadOnlyList<ExpressionDiagnostic> diagnostics)
        {
            _expression = expression;
            Diagnostics = diagnostics;
        }

        public bool Succeeded => _expression != null;

        public BoundExpression Expression
            => _expression ?? throw new InvalidOperationException("The expression did not bind; read Diagnostics instead.");

        /// <summary>Empty on success; on failure, at most 20, in the deterministic order of P15.7.</summary>
        public IReadOnlyList<ExpressionDiagnostic> Diagnostics { get; }

        internal static ExpressionBindResult Success(BoundExpression expression)
            => new ExpressionBindResult(expression ?? throw new ArgumentNullException(nameof(expression)), NoDiagnostics);

        internal static ExpressionBindResult Failure(IReadOnlyList<ExpressionDiagnostic> diagnostics)
        {
            if (diagnostics == null || diagnostics.Count == 0)
            {
                throw new ArgumentException("A failed binding needs at least one diagnostic.", nameof(diagnostics));
            }

            return new ExpressionBindResult(null, diagnostics);
        }
    }

    /// <summary>
    /// The binder and <c>SymbolResolver</c> of V6 P7 (ADR-0040 D5, D6, D8): the ONLY production path from a name to an
    /// identity, used only while writing and only against the snapshot the user sees (P7.3, P7.7).
    ///
    /// <para>
    /// <c>SyntaxExpression + ExpressionContext + consumer scope → BoundExpression</c>, only with zero diagnostics.
    /// Resolution follows P7.1 exactly: with a qualifier the id rules and the current name is only validated; <c>#id</c>
    /// without a name never binds; an unqualified name binds only when it matches exactly one symbol, ignoring case
    /// ordinally, with no trimming, no Unicode normalization and no partial match; a homonym is <c>AmbiguousName</c>
    /// with every candidate. A reserved bare name is <c>ReservedName</c>, namespace syntax is <c>UnknownNamespace</c>, and
    /// a contiguous run with operators that equals a symbol name is <c>OperatorInName</c> — never an operation.
    /// </para>
    /// <para>
    /// It also canonicalizes the form: a unary plus and parentheses bind to nothing, a unary minus binds to
    /// <c>Negate</c>; resolves functions (case-insensitive input, canonical <see cref="FunctionId"/>) and their arity; maps
    /// unit tokens through the units authority without converting them; and validates the normative limits of the tree
    /// (P1.9, P7.5). It is total and fail-closed: every finding is reported, in deterministic order, up to 20 (P7.4).
    /// </para>
    /// <para>
    /// It walks the syntax with an explicit stack, so a long left-deep chain costs no recursion, and it never builds
    /// graphs, detects cycles, evaluates, persists or adapts Project Variables: those belong to G7 and G8.
    /// </para>
    /// </summary>
    public static class ExpressionBinder
    {
        public static ExpressionBindResult Bind(ExpressionSyntax syntax, ExpressionContext context, SymbolScope consumerScope)
        {
            if (syntax == null)
            {
                throw new ArgumentNullException(nameof(syntax));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (consumerScope != SymbolScope.Project && consumerScope != SymbolScope.Rack)
            {
                throw new ArgumentOutOfRangeException(nameof(consumerScope), consumerScope, "Undeclared consumer scope.");
            }

            return new Binder(context, consumerScope).Run(syntax);
        }

        private readonly struct Bound
        {
            internal Bound(BoundExpression expression, long nodes, int depth)
            {
                Expression = expression;
                Nodes = nodes;
                Depth = depth;
            }

            /// <summary>Null when this part could not bind; its shape still counts for the normative limits.</summary>
            internal BoundExpression Expression { get; }

            internal long Nodes { get; }

            internal int Depth { get; }
        }

        private sealed class Binder
        {
            private readonly ExpressionContext _context;
            private readonly SymbolScope _consumerScope;
            private readonly List<ExpressionDiagnostic> _diagnostics = new List<ExpressionDiagnostic>();

            internal Binder(ExpressionContext context, SymbolScope consumerScope)
            {
                _context = context;
                _consumerScope = consumerScope;
            }

            internal ExpressionBindResult Run(ExpressionSyntax root)
            {
                var runs = OperatorInNameDetector.Find(root, _context.Symbols);

                foreach (var run in runs)
                {
                    _diagnostics.Add(ExpressionDiagnostic.About(ExpressionDiagnosticCode.OperatorInName, run.Span, run.Symbols));
                }

                var bound = BindTree(root);
                var limits = _context.Limits;

                if (bound.Nodes > limits.NodeCount)
                {
                    _diagnostics.Add(ExpressionDiagnostic.LimitExceeded(ExpressionLimitKind.NodeCount, limits.NodeCount, root.Span));
                }

                if (bound.Depth > limits.BoundExpressionDepth)
                {
                    _diagnostics.Add(ExpressionDiagnostic.LimitExceeded(
                        ExpressionLimitKind.BoundExpressionDepth,
                        limits.BoundExpressionDepth,
                        root.Span));
                }

                var findings = runs.Count == 0 ? _diagnostics : WithoutFindingsInsideRuns(_diagnostics, runs);

                if (findings.Count == 0)
                {
                    if (bound.Expression == null)
                    {
                        throw new InvalidOperationException("Binding produced neither a tree nor a diagnostic.");
                    }

                    return ExpressionBindResult.Success(bound.Expression);
                }

                return ExpressionBindResult.Failure(ExpressionDiagnostic.Ordered(findings, limits.BinderDiagnostics));
            }

            /// <summary>Post-order over the syntax with an explicit stack: children are bound before their parent.</summary>
            private Bound BindTree(ExpressionSyntax root)
            {
                var work = new Stack<(ExpressionSyntax Node, bool Expanded)>();
                var results = new Stack<Bound>();
                work.Push((root, false));

                while (work.Count > 0)
                {
                    var (node, expanded) = work.Pop();
                    var children = ChildrenOf(node);

                    if (!expanded && children.Count > 0)
                    {
                        work.Push((node, true));

                        for (var index = children.Count - 1; index >= 0; index--)
                        {
                            work.Push((children[index], false));
                        }

                        continue;
                    }

                    var bound = new Bound[children.Count];

                    for (var index = children.Count - 1; index >= 0; index--)
                    {
                        bound[index] = results.Pop();
                    }

                    results.Push(Combine(node, bound));
                }

                return results.Pop();
            }

            private static IReadOnlyList<ExpressionSyntax> ChildrenOf(ExpressionSyntax node)
            {
                switch (node)
                {
                    case UnaryExpressionSyntax unary: return new[] { unary.Operand };
                    case BinaryExpressionSyntax binary: return new[] { binary.Left, binary.Right };
                    case ParenthesizedExpressionSyntax parenthesized: return new[] { parenthesized.Expression };
                    case CallExpressionSyntax call: return call.Arguments;
                    default: return Array.Empty<ExpressionSyntax>();
                }
            }

            private Bound Combine(ExpressionSyntax node, Bound[] children)
            {
                switch (node)
                {
                    case NumberSyntax number:
                        return new Bound(BindNumber(number), 1, 1);

                    case ReferenceSyntax reference:
                        return new Bound(Resolve(reference), 1, 1);

                    case NamespaceReferenceSyntax reserved:
                        Report(ExpressionDiagnosticCode.UnknownNamespace, reserved.Span);
                        return new Bound(null, 1, 1);

                    case UnaryExpressionSyntax unary:
                        var operand = children[0];

                        if (unary.Operator == SyntaxUnaryOperator.Plus)
                        {
                            return operand;
                        }

                        return new Bound(
                            operand.Expression == null ? null : BoundExpression.Negate(operand.Expression),
                            operand.Nodes + 1,
                            operand.Depth + 1);

                    case ParenthesizedExpressionSyntax _:
                        return children[0];

                    case BinaryExpressionSyntax binary:
                        var left = children[0];
                        var right = children[1];

                        return new Bound(
                            left.Expression == null || right.Expression == null
                                ? null
                                : BoundExpression.Binary(OperatorOf(binary.Operator), left.Expression, right.Expression),
                            left.Nodes + right.Nodes + 1,
                            Math.Max(left.Depth, right.Depth) + 1);

                    case CallExpressionSyntax call:
                        return BindCall(call, children);

                    default:
                        throw new InvalidOperationException("Unknown syntax node " + node.GetType().Name + ".");
                }
            }

            private BoundExpression BindNumber(NumberSyntax number)
            {
                if (!number.HasUnit)
                {
                    return BoundExpression.Number(number.Value);
                }

                if (!_context.Units.TryParseToken(number.UnitToken, out var unit))
                {
                    // The lexer and the binder read the same table: this is a broken invariant, not a user error.
                    throw new InvalidOperationException("The lexer accepted a unit token the units authority does not declare.");
                }

                return BoundExpression.Number(number.Value, unit);
            }

            private Bound BindCall(CallExpressionSyntax call, Bound[] arguments)
            {
                var functions = _context.Functions;
                var known = functions.TryResolveName(call.Function.Text, out var function);
                var arityOk = known && functions.AcceptsArity(function, arguments.Length);

                if (!known)
                {
                    Report(ExpressionDiagnosticCode.UnknownFunction, call.Function.Span);
                }
                else if (!arityOk)
                {
                    Report(ExpressionDiagnosticCode.InvalidArguments, call.Span);
                }

                if (arguments.Length > _context.Limits.ArgumentCount)
                {
                    _diagnostics.Add(ExpressionDiagnostic.LimitExceeded(
                        ExpressionLimitKind.ArgumentCount,
                        _context.Limits.ArgumentCount,
                        call.Span));
                }

                var nodes = 1L;
                var depth = 0;
                var complete = true;

                foreach (var argument in arguments)
                {
                    nodes += argument.Nodes;
                    depth = Math.Max(depth, argument.Depth);
                    complete &= argument.Expression != null;
                }

                var expression = arityOk && complete && arguments.Length <= _context.Limits.ArgumentCount
                    ? BoundExpression.Call(function, arguments.Select(argument => argument.Expression))
                    : null;

                return new Bound(expression, nodes, depth + 1);
            }

            /// <summary>The table of P7.1, row by row.</summary>
            private BoundExpression Resolve(ReferenceSyntax reference)
            {
                var name = reference.Name;
                var qualifier = reference.Qualifier;

                if (name != null && !name.IsBraced && ExpressionReservedNames.IsReserved(name.Text))
                {
                    Report(ExpressionDiagnosticCode.ReservedName, name.Span);
                    return null;
                }

                SymbolEntry entry;

                if (qualifier != null)
                {
                    // The key text and the comparer of its namespace, nothing else (Amendment A2 §3.5): the entry found
                    // gives the identity back with the spelling of the registry.
                    var id = SymbolId.ProjectVariable(qualifier.Key);

                    if (!_context.Symbols.TryGet(id, out entry))
                    {
                        Report(ExpressionDiagnosticCode.BrokenReference, reference.Span, id);
                        return null;
                    }

                    if (name == null)
                    {
                        Report(ExpressionDiagnosticCode.NameRequired, reference.Span, entry.Id);
                        return null;
                    }

                    if (!string.Equals(name.Text, entry.DisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        Report(ExpressionDiagnosticCode.QualifiedNameMismatch, reference.Span, entry.Id);
                        return null;
                    }
                }
                else
                {
                    var matches = _context.Symbols.FindByDisplayName(name.Text);

                    if (matches.Count == 0)
                    {
                        Report(ExpressionDiagnosticCode.UnknownSymbol, name.Span);
                        return null;
                    }

                    if (matches.Count > 1)
                    {
                        _diagnostics.Add(ExpressionDiagnostic.About(
                            ExpressionDiagnosticCode.AmbiguousName,
                            name.Span,
                            matches.Select(match => match.Id)));
                        return null;
                    }

                    entry = matches[0];
                }

                if (entry.Scope == SymbolScope.Rack && _consumerScope == SymbolScope.Project)
                {
                    Report(ExpressionDiagnosticCode.ScopeViolation, reference.Span, entry.Id);
                    return null;
                }

                return BoundExpression.Reference(entry.Id);
            }

            private static BoundBinaryOperator OperatorOf(SyntaxBinaryOperator syntaxOperator)
            {
                switch (syntaxOperator)
                {
                    case SyntaxBinaryOperator.Add: return BoundBinaryOperator.Add;
                    case SyntaxBinaryOperator.Subtract: return BoundBinaryOperator.Subtract;
                    case SyntaxBinaryOperator.Multiply: return BoundBinaryOperator.Multiply;
                    case SyntaxBinaryOperator.Divide: return BoundBinaryOperator.Divide;
                    default: throw new InvalidOperationException("Unknown syntax operator " + (int)syntaxOperator + ".");
                }
            }

            private void Report(ExpressionDiagnosticCode code, SourceSpan span, SymbolId related = null)
                => _diagnostics.Add(ExpressionDiagnostic.About(code, span, related == null ? null : new[] { related }));

            /// <summary>
            /// An <c>OperatorInName</c> run is never read as an operation, so what binding its pieces as references would
            /// say is not reported: the user sees the run, not the consequences of misreading it. Limits and the runs
            /// themselves stay.
            /// </summary>
            private static List<ExpressionDiagnostic> WithoutFindingsInsideRuns(
                List<ExpressionDiagnostic> diagnostics,
                IReadOnlyList<OperatorInNameDetector.Run> runs)
            {
                var spans = runs.Select(run => run.Span).OrderBy(span => span.Start).ToArray();
                var maxEnd = new int[spans.Length];

                for (var index = 0; index < spans.Length; index++)
                {
                    maxEnd[index] = Math.Max(spans[index].End, index == 0 ? int.MinValue : maxEnd[index - 1]);
                }

                bool InsideARun(SourceSpan span)
                {
                    // The last run that starts at or before the finding; a run containing it has start ≤ and end ≥.
                    int low = 0, high = spans.Length - 1, found = -1;

                    while (low <= high)
                    {
                        var middle = low + ((high - low) / 2);

                        if (spans[middle].Start <= span.Start)
                        {
                            found = middle;
                            low = middle + 1;
                        }
                        else
                        {
                            high = middle - 1;
                        }
                    }

                    return found >= 0 && maxEnd[found] >= span.End;
                }

                return diagnostics
                    .Where(diagnostic => diagnostic.Code == ExpressionDiagnosticCode.OperatorInName
                                         || diagnostic.Code == ExpressionDiagnosticCode.LimitExceeded
                                         || !diagnostic.Span.HasValue
                                         || !InsideARun(diagnostic.Span.Value))
                    .ToList();
            }
        }
    }
}
