using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The deterministic graph and semantic evaluation of one immutable expression context. It delegates eligible
    /// numeric trees to <see cref="ExpressionEvaluator"/> and owns the snapshot-level blockers that evaluator cannot see.
    /// </summary>
    public sealed class RegistryEvaluation
    {
        private sealed class SymbolState
        {
            internal SymbolState(SymbolEntry entry)
            {
                Entry = entry;
                Missing = Array.Empty<SymbolId>();
                InvalidArguments = Array.Empty<InvalidArgumentSignature>();
                StaticDiagnostics = Array.Empty<RegistryDiagnostic>();
                Diagnostics = Array.Empty<RegistryDiagnostic>();
            }

            internal SymbolEntry Entry { get; }
            internal IReadOnlyList<SymbolId> Missing { get; set; }
            internal IReadOnlyList<InvalidArgumentSignature> InvalidArguments { get; set; }
            internal IReadOnlyList<RegistryDiagnostic> StaticDiagnostics { get; set; }
            internal IReadOnlyList<RegistryDiagnostic> Diagnostics { get; set; }
            internal bool Succeeded { get; set; }
            internal double Value { get; set; }
        }

        private readonly IReadOnlyDictionary<SymbolId, RegistrySymbolResult> _results;

        private RegistryEvaluation(ExpressionContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DependencyGraph = new DependencyGraph(context.Symbols);
            EvaluationOrder = DependencyGraph.EvaluationOrder;
            Cycles = DependencyGraph.Cycles;

            var states = context.Symbols.Entries.ToDictionary(entry => entry.Id, entry => new SymbolState(entry));
            BuildStaticState(context, DependencyGraph, states);

            // Cycle members are known failures before acyclic nodes are processed. Their first diagnostic is already
            // stable because every later external DependencyFailed sorts after BrokenReference/Cycle.
            foreach (var state in states.Values.Where(state => DependencyGraph.IsCyclicComponent(DependencyGraph.ComponentOf(state.Entry.Id))))
            {
                state.Diagnostics = state.StaticDiagnostics;
            }

            foreach (var symbol in EvaluationOrder)
            {
                EvaluateAcyclic(context, DependencyGraph, states, states[symbol]);
            }

            foreach (var state in states.Values.Where(state => DependencyGraph.IsCyclicComponent(DependencyGraph.ComponentOf(state.Entry.Id))))
            {
                var diagnostics = state.StaticDiagnostics.Concat(DependencyFailures(DependencyGraph, states, state));
                state.Diagnostics = RegistryDiagnostic.Ordered(diagnostics);
                state.Succeeded = false;
            }

            var rootsByComponent = BuildRootCauses(DependencyGraph, states);
            var results = new Dictionary<SymbolId, RegistrySymbolResult>();
            foreach (var entry in context.Symbols.Entries)
            {
                var state = states[entry.Id];
                results.Add(entry.Id, new RegistrySymbolResult(
                    state.Succeeded,
                    state.Value,
                    state.Diagnostics,
                    rootsByComponent[DependencyGraph.ComponentOf(entry.Id)]));
            }

            _results = new ReadOnlyDictionary<SymbolId, RegistrySymbolResult>(results);
        }

        public ExpressionContext Context { get; }

        public DependencyGraph DependencyGraph { get; }

        public IReadOnlyList<SymbolId> EvaluationOrder { get; }

        public IReadOnlyList<DependencyCycle> Cycles { get; }

        public IReadOnlyDictionary<SymbolId, RegistrySymbolResult> Results => _results;

        public static RegistryEvaluation Evaluate(ExpressionContext context) => new RegistryEvaluation(context);

        public RegistrySymbolResult Result(SymbolId symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (!_results.TryGetValue(symbol, out var result))
            {
                throw new KeyNotFoundException("The symbol does not belong to this registry snapshot: " + symbol + ".");
            }

            return result;
        }

        private static void BuildStaticState(
            ExpressionContext context,
            DependencyGraph graph,
            IReadOnlyDictionary<SymbolId, SymbolState> states)
        {
            foreach (var state in states.Values)
            {
                if (state.Entry.Definition.Kind == SymbolDefinitionKind.Literal)
                {
                    continue;
                }

                var expression = state.Entry.Definition.Expression;
                state.Missing = ReadOnly(graph.DirectDependencies(state.Entry.Id)
                    .Where(dependency => !context.Symbols.TryGet(dependency, out _)));
                state.InvalidArguments = InvalidArgumentsOf(expression, context.Functions);

                var diagnostics = new List<RegistryDiagnostic>();
                diagnostics.AddRange(state.Missing.Select(RegistryDiagnostic.Broken));

                var component = graph.ComponentOf(state.Entry.Id);
                if (graph.IsCyclicComponent(component))
                {
                    diagnostics.Add(RegistryDiagnostic.Cycle(graph.Components[component]));
                }

                diagnostics.AddRange(state.InvalidArguments.Select(RegistryDiagnostic.Invalid));
                if (IsNonCanonical(expression))
                {
                    diagnostics.Add(RegistryDiagnostic.Intrinsic(ExpressionDiagnosticCode.NonCanonicalForm));
                }

                state.StaticDiagnostics = RegistryDiagnostic.Ordered(diagnostics);
            }
        }

        private static void EvaluateAcyclic(
            ExpressionContext context,
            DependencyGraph graph,
            IReadOnlyDictionary<SymbolId, SymbolState> states,
            SymbolState state)
        {
            if (state.Entry.Definition.Kind == SymbolDefinitionKind.Literal)
            {
                state.Succeeded = true;
                state.Value = state.Entry.Definition.LiteralValue;
                state.Diagnostics = Array.Empty<RegistryDiagnostic>();
                return;
            }

            var diagnostics = RegistryDiagnostic.Ordered(
                state.StaticDiagnostics.Concat(DependencyFailures(graph, states, state)));
            if (diagnostics.Count > 0)
            {
                state.Succeeded = false;
                state.Diagnostics = diagnostics;
                return;
            }

            var values = graph.PresentDependencies(state.Entry.Id)
                .ToDictionary(dependency => dependency, dependency => states[dependency].Value);
            var numeric = ExpressionEvaluator.Evaluate(state.Entry.Definition.Expression, context, values);
            if (numeric.Succeeded)
            {
                state.Succeeded = true;
                state.Value = numeric.Value;
                state.Diagnostics = Array.Empty<RegistryDiagnostic>();
                return;
            }

            state.Succeeded = false;
            state.Diagnostics = RegistryDiagnostic.Ordered(
                numeric.Diagnostics.Select(diagnostic => RegistryDiagnostic.Intrinsic(diagnostic.Code)));
        }

        private static IEnumerable<RegistryDiagnostic> DependencyFailures(
            DependencyGraph graph,
            IReadOnlyDictionary<SymbolId, SymbolState> states,
            SymbolState owner)
        {
            var ownerComponent = graph.ComponentOf(owner.Entry.Id);
            foreach (var dependency in graph.PresentDependencies(owner.Entry.Id))
            {
                if (graph.ComponentOf(dependency) == ownerComponent || states[dependency].Succeeded)
                {
                    continue;
                }

                var failed = states[dependency];
                var first = failed.Diagnostics.First();
                IReadOnlyList<SymbolId> chain;
                RootSignature root;
                if (first.Code == ExpressionDiagnosticCode.DependencyFailed)
                {
                    chain = CauseChain.Prepend(dependency, first.Chain);
                    root = first.Root;
                }
                else
                {
                    chain = CauseChain.Single(dependency);
                    root = RootForDiagnostic(graph, failed, first);
                }

                yield return RegistryDiagnostic.DependencyFailed(dependency, chain, root);
            }
        }

        private static RootSignature RootForDiagnostic(DependencyGraph graph, SymbolState owner, RegistryDiagnostic diagnostic)
        {
            switch (diagnostic.Code)
            {
                case ExpressionDiagnosticCode.BrokenReference:
                    return RootSignature.Broken(owner.Entry.Id, owner.Missing);
                case ExpressionDiagnosticCode.Cycle:
                    return RootSignature.Cycle(graph.Components[graph.ComponentOf(owner.Entry.Id)]);
                case ExpressionDiagnosticCode.InvalidArguments:
                    return RootSignature.Invalid(owner.Entry.Id, owner.InvalidArguments);
                case ExpressionDiagnosticCode.DivisionByZero:
                case ExpressionDiagnosticCode.NonFiniteResult:
                case ExpressionDiagnosticCode.NonCanonicalForm:
                    return RootSignature.Intrinsic(owner.Entry.Id, diagnostic.Code);
                default:
                    throw new InvalidOperationException("Diagnostic " + diagnostic.Code + " cannot be a cause root.");
            }
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<RootSignature>> BuildRootCauses(
            DependencyGraph graph,
            IReadOnlyDictionary<SymbolId, SymbolState> states)
        {
            var own = new Dictionary<int, SortedSet<RootSignature>>();
            var dependencies = new Dictionary<int, SortedSet<int>>();
            var dependents = new Dictionary<int, SortedSet<int>>();
            for (var component = 0; component < graph.Components.Count; component++)
            {
                own.Add(component, new SortedSet<RootSignature>());
                dependencies.Add(component, new SortedSet<int>());
                dependents.Add(component, new SortedSet<int>());
            }

            foreach (var state in states.Values)
            {
                var component = graph.ComponentOf(state.Entry.Id);
                var representedCodes = new HashSet<ExpressionDiagnosticCode>();
                foreach (var diagnostic in state.Diagnostics)
                {
                    if (diagnostic.Code == ExpressionDiagnosticCode.DependencyFailed || !representedCodes.Add(diagnostic.Code))
                    {
                        continue;
                    }

                    own[component].Add(RootForDiagnostic(graph, state, diagnostic));
                }

                foreach (var dependency in graph.PresentDependencies(state.Entry.Id))
                {
                    var dependencyComponent = graph.ComponentOf(dependency);
                    if (dependencyComponent != component)
                    {
                        dependencies[component].Add(dependencyComponent);
                        dependents[dependencyComponent].Add(component);
                    }
                }
            }

            var pending = dependencies.ToDictionary(pair => pair.Key, pair => pair.Value.Count);
            var ready = new SortedSet<int>(pending.Where(pair => pair.Value == 0).Select(pair => pair.Key));
            var roots = new Dictionary<int, IReadOnlyList<RootSignature>>();
            while (ready.Count > 0)
            {
                var component = ready.Min;
                ready.Remove(component);
                var accumulated = own[component];
                foreach (var dependency in dependencies[component])
                {
                    accumulated.UnionWith(roots[dependency]);
                }

                roots.Add(component, new ReadOnlyCollection<RootSignature>(accumulated.ToList()));
                foreach (var dependent in dependents[component])
                {
                    pending[dependent]--;
                    if (pending[dependent] == 0)
                    {
                        ready.Add(dependent);
                    }
                }
            }

            return new ReadOnlyDictionary<int, IReadOnlyList<RootSignature>>(roots);
        }

        private static IReadOnlyList<InvalidArgumentSignature> InvalidArgumentsOf(
            BoundExpression expression,
            FunctionRegistry functions)
        {
            var invalid = new SortedSet<InvalidArgumentSignature>();
            var pending = new Stack<BoundExpression>();
            pending.Push(expression);
            while (pending.Count > 0)
            {
                switch (pending.Pop())
                {
                    case BoundNegate negate:
                        pending.Push(negate.Operand);
                        break;
                    case BoundBinary binary:
                        pending.Push(binary.Right);
                        pending.Push(binary.Left);
                        break;
                    case BoundCall call:
                        if (!functions.AcceptsArity(call.Function, call.Arguments.Count))
                        {
                            invalid.Add(new InvalidArgumentSignature(functions.Token(call.Function), call.Arguments.Count));
                        }

                        for (var index = call.Arguments.Count - 1; index >= 0; index--)
                        {
                            pending.Push(call.Arguments[index]);
                        }

                        break;
                }
            }

            return new ReadOnlyCollection<InvalidArgumentSignature>(invalid.ToList());
        }

        private static bool IsNonCanonical(BoundExpression expression)
            => expression is BoundNumber number && !number.Unit.HasValue
               || expression is BoundNegate negate
               && negate.Operand is BoundNumber negatedNumber
               && !negatedNumber.Unit.HasValue;

        private static IReadOnlyList<T> ReadOnly<T>(IEnumerable<T> values)
            => new ReadOnlyCollection<T>(values.ToList());
    }
}
