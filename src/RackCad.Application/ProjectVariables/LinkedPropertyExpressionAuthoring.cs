using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.Expressions;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>Typed findings produced while a linked-property formula is still a draft.</summary>
    public enum LinkedPropertyEditDiagnosticCode
    {
        UnknownSymbol,
        AmbiguousName,
        InvalidQualifier,
        UnterminatedName,
        ResourceLimit,
        DependencyFailed,
        Domain,
        Syntax,
        Evaluation,
    }

    public sealed class LinkedPropertyEditDiagnostic
    {
        internal LinkedPropertyEditDiagnostic(LinkedPropertyEditDiagnosticCode code)
            : this(code, null, null)
        {
        }

        internal LinkedPropertyEditDiagnostic(
            LinkedPropertyEditDiagnosticCode code,
            SymbolId failedSymbol,
            RegistrySymbolResult registryResult)
        {
            Code = code;
            FailedSymbol = failedSymbol;
            RegistryResult = registryResult;
        }

        public LinkedPropertyEditDiagnosticCode Code { get; }

        public SymbolId FailedSymbol { get; }

        /// <summary>The original G7 result; diagnostics and root causes are never reconstructed from text.</summary>
        public RegistrySymbolResult RegistryResult { get; }
    }

    /// <summary>
    /// One coherent authoring pipeline for linked properties: parse, bind, canonicalize, evaluate and validate the
    /// property domain. The UI only supplies text and renders this result.
    /// </summary>
    internal sealed class LinkedPropertyExpressionAuthoring
    {
        private readonly IReadOnlyList<LinkedPropertyOption> _options;
        private readonly ExpressionContext _context;
        private readonly IReadOnlyDictionary<SymbolId, double> _values;
        private readonly HashSet<SymbolId> _failed;
        private readonly IReadOnlyDictionary<SymbolId, RegistrySymbolResult> _failedResults;

        internal LinkedPropertyExpressionAuthoring(IReadOnlyList<LinkedPropertyOption> options)
        {
            _options = options ?? Array.Empty<LinkedPropertyOption>();
            var entries = new List<SymbolEntry>();
            var values = new Dictionary<SymbolId, double>();
            _failed = new HashSet<SymbolId>();

            foreach (var option in _options)
            {
                var id = SymbolId.ProjectVariable(option.VariableId.Value);
                entries.Add(new SymbolEntry(
                    id,
                    SymbolScope.Project,
                    option.Name ?? string.Empty,
                    SymbolDefinition.FromLiteral(option.LiteralValue)));
                values.Add(id, option.LiteralValue);
            }

            _context = ExpressionContext.Create(SymbolTable.Create(entries));
            _values = new ReadOnlyDictionary<SymbolId, double>(values);
            _failedResults = new ReadOnlyDictionary<SymbolId, RegistrySymbolResult>(
                new Dictionary<SymbolId, RegistrySymbolResult>());
        }

        internal LinkedPropertyExpressionAuthoring(LinkedPropertyAuthoringContext authoringContext)
        {
            if (authoringContext == null) throw new ArgumentNullException(nameof(authoringContext));

            _options = Array.Empty<LinkedPropertyOption>();
            _context = authoringContext.ExpressionContext;
            var values = new Dictionary<SymbolId, double>();
            _failed = new HashSet<SymbolId>();
            var failedResults = new Dictionary<SymbolId, RegistrySymbolResult>();
            foreach (var symbol in authoringContext.Symbols)
            {
                var id = SymbolId.ProjectVariable(symbol.VariableId.Value);
                if (symbol.Evaluation.Succeeded)
                {
                    values.Add(id, symbol.Evaluation.Value);
                }
                else
                {
                    _failed.Add(id);
                    failedResults.Add(id, symbol.Evaluation);
                }
            }

            _values = new ReadOnlyDictionary<SymbolId, double>(values);
            _failedResults = new ReadOnlyDictionary<SymbolId, RegistrySymbolResult>(failedResults);
        }

        internal SymbolTable Symbols => _context.Symbols;

        internal LinkedPropertyAuthoringResult Run(string text)
        {
            var source = (text ?? string.Empty).TrimStart();
            if (source.StartsWith("=", StringComparison.Ordinal))
            {
                source = source.Substring(1);
            }

            var parsed = ExpressionParser.Parse(source);
            if (!parsed.Succeeded)
            {
                return LinkedPropertyAuthoringResult.Failure(parsed.Diagnostics.Select(Map));
            }

            var bound = ExpressionBinder.Bind(parsed.Syntax, _context, SymbolScope.Rack);
            if (!bound.Succeeded)
            {
                return LinkedPropertyAuthoringResult.Failure(bound.Diagnostics.Select(Map));
            }

            var expression = bound.Expression;
            var failedDependency = BoundExpressionDependencies.DirectDependencies(expression)
                .FirstOrDefault(id => _failed.Contains(id));
            if (failedDependency != null)
            {
                return _failedResults.TryGetValue(failedDependency, out var failedResult)
                    ? LinkedPropertyAuthoringResult.Failure(
                        new LinkedPropertyEditDiagnostic(
                            LinkedPropertyEditDiagnosticCode.DependencyFailed,
                            failedDependency,
                            failedResult))
                    : LinkedPropertyAuthoringResult.Failure(LinkedPropertyEditDiagnosticCode.DependencyFailed);
            }

            var evaluated = ExpressionEvaluator.Evaluate(expression, _context, _values);
            if (!evaluated.Succeeded)
            {
                return LinkedPropertyAuthoringResult.Failure(evaluated.Diagnostics.Select(Map));
            }

            // Every currently linked editor in RackCad edits a positive length. Keep this boundary check here so a
            // semantically valid expression cannot bypass the same domain the literal path must ultimately satisfy.
            if (evaluated.Value <= 0.0)
            {
                return LinkedPropertyAuthoringResult.Failure(LinkedPropertyEditDiagnosticCode.Domain);
            }

            return LinkedPropertyAuthoringResult.Success(expression, evaluated.Value, CanonicalShape.Classify(expression));
        }

        internal string Format(BoundExpression expression) => ExpressionFormatter.Format(expression, _context.Symbols);

        internal bool TryEvaluate(BoundExpression expression, out double value)
        {
            value = 0.0;
            if (expression == null ||
                BoundExpressionDependencies.DirectDependencies(expression).Any(id => _failed.Contains(id)))
            {
                return false;
            }

            var evaluated = ExpressionEvaluator.Evaluate(expression, _context, _values);
            if (!evaluated.Succeeded || evaluated.Value <= 0.0)
            {
                return false;
            }

            value = evaluated.Value;
            return true;
        }

        private static LinkedPropertyEditDiagnosticCode Map(ExpressionDiagnostic diagnostic)
        {
            switch (diagnostic.Code)
            {
                case ExpressionDiagnosticCode.UnknownSymbol: return LinkedPropertyEditDiagnosticCode.UnknownSymbol;
                case ExpressionDiagnosticCode.AmbiguousName: return LinkedPropertyEditDiagnosticCode.AmbiguousName;
                case ExpressionDiagnosticCode.InvalidQualifier: return LinkedPropertyEditDiagnosticCode.InvalidQualifier;
                case ExpressionDiagnosticCode.UnterminatedName: return LinkedPropertyEditDiagnosticCode.UnterminatedName;
                case ExpressionDiagnosticCode.LimitExceeded: return LinkedPropertyEditDiagnosticCode.ResourceLimit;
                case ExpressionDiagnosticCode.DependencyFailed: return LinkedPropertyEditDiagnosticCode.DependencyFailed;
                case ExpressionDiagnosticCode.DivisionByZero:
                case ExpressionDiagnosticCode.NonFiniteResult:
                    return LinkedPropertyEditDiagnosticCode.Evaluation;
                default:
                    return LinkedPropertyEditDiagnosticCode.Syntax;
            }
        }
    }

    internal sealed class LinkedPropertyAuthoringResult
    {
        private LinkedPropertyAuthoringResult(
            BoundExpression expression,
            double value,
            CanonicalShape shape,
            IReadOnlyList<LinkedPropertyEditDiagnostic> diagnostics)
        {
            Expression = expression;
            Value = value;
            Shape = shape;
            Diagnostics = diagnostics;
        }

        internal bool Succeeded => Expression != null;
        internal BoundExpression Expression { get; }
        internal double Value { get; }
        internal CanonicalShape Shape { get; }
        internal IReadOnlyList<LinkedPropertyEditDiagnostic> Diagnostics { get; }

        internal static LinkedPropertyAuthoringResult Success(BoundExpression expression, double value, CanonicalShape shape)
            => new LinkedPropertyAuthoringResult(expression, value, shape, Array.Empty<LinkedPropertyEditDiagnostic>());

        internal static LinkedPropertyAuthoringResult Failure(LinkedPropertyEditDiagnosticCode code)
            => Failure(new[] { code });

        internal static LinkedPropertyAuthoringResult Failure(LinkedPropertyEditDiagnostic diagnostic)
            => new LinkedPropertyAuthoringResult(
                null,
                0.0,
                null,
                new ReadOnlyCollection<LinkedPropertyEditDiagnostic>(new[] { diagnostic }));

        internal static LinkedPropertyAuthoringResult Failure(IEnumerable<LinkedPropertyEditDiagnosticCode> codes)
            => new LinkedPropertyAuthoringResult(
                null,
                0.0,
                null,
                new ReadOnlyCollection<LinkedPropertyEditDiagnostic>(
                    codes.Select(code => new LinkedPropertyEditDiagnostic(code)).ToArray()));
    }
}
