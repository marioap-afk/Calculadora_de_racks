using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>A deterministic cyclic SCC exposed as one immutable member list.</summary>
    public sealed class DependencyCycle
    {
        internal DependencyCycle(IReadOnlyList<SymbolId> members) => Members = members;

        public IReadOnlyList<SymbolId> Members { get; }
    }

    /// <summary>
    /// The immutable owner-to-dependency graph of one symbol snapshot. Missing references remain visible as direct
    /// broken edges, but only present symbols are graph nodes and participate in traversal or SCC analysis.
    /// </summary>
    public sealed class DependencyGraph
    {
        private readonly IReadOnlyList<SymbolId> _nodes;
        private readonly Dictionary<SymbolId, IReadOnlyList<SymbolId>> _direct;
        private readonly Dictionary<SymbolId, IReadOnlyList<SymbolId>> _presentDirect;
        private readonly Dictionary<SymbolId, IReadOnlyList<SymbolId>> _dependents;
        private readonly Dictionary<SymbolId, int> _componentByNode;
        private readonly IReadOnlyList<IReadOnlyList<SymbolId>> _components;
        private readonly HashSet<int> _cyclicComponents;
        private readonly IReadOnlyList<DependencyCycle> _cycles;
        private readonly IReadOnlyList<SymbolId> _evaluationOrder;

        internal DependencyGraph(SymbolTable symbols)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            _nodes = ReadOnly(symbols.Entries.Select(entry => entry.Id));
            var nodeSet = new HashSet<SymbolId>(_nodes);
            _direct = new Dictionary<SymbolId, IReadOnlyList<SymbolId>>();
            _presentDirect = new Dictionary<SymbolId, IReadOnlyList<SymbolId>>();

            foreach (var entry in symbols.Entries)
            {
                var all = entry.Definition.Kind == SymbolDefinitionKind.Expression
                    ? BoundExpressionDependencies.DirectDependencies(entry.Definition.Expression)
                    : ReadOnly(Array.Empty<SymbolId>());
                _direct.Add(entry.Id, all);
                _presentDirect.Add(entry.Id, ReadOnly(all.Where(nodeSet.Contains)));
            }

            _dependents = BuildDependents(_nodes, _presentDirect);
            (_components, _componentByNode) = FindComponents(_nodes, _presentDirect, _dependents);
            _cyclicComponents = FindCyclicComponents(_components, _presentDirect);
            _cycles = BuildCycles(_components, _cyclicComponents);
            _evaluationOrder = BuildEvaluationOrder(_nodes, _presentDirect, _dependents, _componentByNode, _cyclicComponents);
        }

        public IReadOnlyList<DependencyCycle> Cycles => _cycles;

        public IReadOnlyList<SymbolId> EvaluationOrder => _evaluationOrder;

        public IReadOnlyList<SymbolId> DirectDependencies(SymbolId owner) => Get(_direct, owner);

        public IReadOnlyList<SymbolId> DirectDependents(SymbolId dependency) => Get(_dependents, dependency);

        public IReadOnlyList<SymbolId> TransitiveDependencies(SymbolId owner)
            => Traverse(owner, _presentDirect);

        public IReadOnlyList<SymbolId> TransitiveDependents(SymbolId dependency)
            => Traverse(dependency, _dependents);

        public bool IsSimpleCycle(IReadOnlyList<SymbolId> members)
        {
            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            var ordered = members.Distinct().OrderBy(member => member).ToArray();
            if (ordered.Length == 0 || !_componentByNode.TryGetValue(ordered[0], out var component)
                || !_cyclicComponents.Contains(component)
                || !_components[component].SequenceEqual(ordered))
            {
                return false;
            }

            var memberSet = new HashSet<SymbolId>(ordered);
            var incoming = ordered.ToDictionary(member => member, _ => 0);
            foreach (var owner in ordered)
            {
                var outgoing = _presentDirect[owner].Where(memberSet.Contains).ToArray();
                if (outgoing.Length != 1)
                {
                    return false;
                }

                incoming[outgoing[0]]++;
            }

            return incoming.Values.All(count => count == 1);
        }

        internal IReadOnlyList<IReadOnlyList<SymbolId>> Components => _components;

        internal int ComponentOf(SymbolId node) => _componentByNode[node];

        internal bool IsCyclicComponent(int component) => _cyclicComponents.Contains(component);

        internal IReadOnlyList<SymbolId> PresentDependencies(SymbolId owner) => Get(_presentDirect, owner);

        private static IReadOnlyList<SymbolId> Traverse(
            SymbolId start,
            IReadOnlyDictionary<SymbolId, IReadOnlyList<SymbolId>> adjacency)
        {
            if (start == null || !adjacency.ContainsKey(start))
            {
                return ReadOnly(Array.Empty<SymbolId>());
            }

            var visited = new HashSet<SymbolId>();
            var pending = new Stack<SymbolId>();
            foreach (var next in adjacency[start].Reverse())
            {
                pending.Push(next);
            }

            while (pending.Count > 0)
            {
                var current = pending.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                foreach (var next in adjacency[current].Reverse())
                {
                    if (!visited.Contains(next))
                    {
                        pending.Push(next);
                    }
                }
            }

            return ReadOnly(visited.OrderBy(node => node));
        }

        private static Dictionary<SymbolId, IReadOnlyList<SymbolId>> BuildDependents(
            IReadOnlyList<SymbolId> nodes,
            IReadOnlyDictionary<SymbolId, IReadOnlyList<SymbolId>> dependencies)
        {
            var mutable = nodes.ToDictionary(node => node, _ => new List<SymbolId>());
            foreach (var owner in nodes)
            {
                foreach (var dependency in dependencies[owner])
                {
                    mutable[dependency].Add(owner);
                }
            }

            return mutable.ToDictionary(pair => pair.Key, pair => ReadOnly(pair.Value.OrderBy(node => node)));
        }

        private static (IReadOnlyList<IReadOnlyList<SymbolId>>, Dictionary<SymbolId, int>) FindComponents(
            IReadOnlyList<SymbolId> nodes,
            IReadOnlyDictionary<SymbolId, IReadOnlyList<SymbolId>> dependencies,
            IReadOnlyDictionary<SymbolId, IReadOnlyList<SymbolId>> dependents)
        {
            var visited = new HashSet<SymbolId>();
            var finish = new List<SymbolId>(nodes.Count);

            foreach (var root in nodes)
            {
                if (!visited.Add(root))
                {
                    continue;
                }

                var pending = new Stack<(SymbolId Node, int NextDependency)>();
                pending.Push((root, 0));
                while (pending.Count > 0)
                {
                    var frame = pending.Pop();
                    var adjacent = dependencies[frame.Node];
                    if (frame.NextDependency >= adjacent.Count)
                    {
                        finish.Add(frame.Node);
                        continue;
                    }

                    pending.Push((frame.Node, frame.NextDependency + 1));
                    var next = adjacent[frame.NextDependency];
                    if (visited.Add(next))
                    {
                        pending.Push((next, 0));
                    }
                }
            }

            visited.Clear();
            var raw = new List<IReadOnlyList<SymbolId>>();
            for (var index = finish.Count - 1; index >= 0; index--)
            {
                var root = finish[index];
                if (!visited.Add(root))
                {
                    continue;
                }

                var members = new List<SymbolId>();
                var pending = new Stack<SymbolId>();
                pending.Push(root);
                while (pending.Count > 0)
                {
                    var current = pending.Pop();
                    members.Add(current);
                    foreach (var next in dependents[current].Reverse())
                    {
                        if (visited.Add(next))
                        {
                            pending.Push(next);
                        }
                    }
                }

                members.Sort();
                raw.Add(ReadOnly(members));
            }

            raw.Sort((left, right) => left[0].CompareTo(right[0]));
            var components = new ReadOnlyCollection<IReadOnlyList<SymbolId>>(raw);
            var byNode = new Dictionary<SymbolId, int>();
            for (var component = 0; component < components.Count; component++)
            {
                foreach (var member in components[component])
                {
                    byNode.Add(member, component);
                }
            }

            return (components, byNode);
        }

        private static HashSet<int> FindCyclicComponents(
            IReadOnlyList<IReadOnlyList<SymbolId>> components,
            IReadOnlyDictionary<SymbolId, IReadOnlyList<SymbolId>> dependencies)
        {
            var cyclic = new HashSet<int>();
            for (var index = 0; index < components.Count; index++)
            {
                var members = components[index];
                if (members.Count > 1 || dependencies[members[0]].Contains(members[0]))
                {
                    cyclic.Add(index);
                }
            }

            return cyclic;
        }

        private static IReadOnlyList<DependencyCycle> BuildCycles(
            IReadOnlyList<IReadOnlyList<SymbolId>> components,
            HashSet<int> cyclic)
            => new ReadOnlyCollection<DependencyCycle>(cyclic
                .OrderBy(component => components[component][0])
                .Select(component => new DependencyCycle(components[component]))
                .ToList());

        private static IReadOnlyList<SymbolId> BuildEvaluationOrder(
            IReadOnlyList<SymbolId> nodes,
            IReadOnlyDictionary<SymbolId, IReadOnlyList<SymbolId>> dependencies,
            IReadOnlyDictionary<SymbolId, IReadOnlyList<SymbolId>> dependents,
            IReadOnlyDictionary<SymbolId, int> componentByNode,
            HashSet<int> cyclic)
        {
            var pendingDependencies = new Dictionary<SymbolId, int>();
            var ready = new SortedSet<SymbolId>();
            foreach (var node in nodes)
            {
                if (cyclic.Contains(componentByNode[node]))
                {
                    continue;
                }

                var count = dependencies[node].Count(dependency => !cyclic.Contains(componentByNode[dependency]));
                pendingDependencies.Add(node, count);
                if (count == 0)
                {
                    ready.Add(node);
                }
            }

            var order = new List<SymbolId>(pendingDependencies.Count);
            while (ready.Count > 0)
            {
                var next = ready.Min;
                ready.Remove(next);
                order.Add(next);

                foreach (var dependent in dependents[next])
                {
                    if (!pendingDependencies.ContainsKey(dependent))
                    {
                        continue;
                    }

                    pendingDependencies[dependent]--;
                    if (pendingDependencies[dependent] == 0)
                    {
                        ready.Add(dependent);
                    }
                }
            }

            return ReadOnly(order);
        }

        private static IReadOnlyList<SymbolId> Get(
            IReadOnlyDictionary<SymbolId, IReadOnlyList<SymbolId>> source,
            SymbolId id)
            => id != null && source.TryGetValue(id, out var values) ? values : ReadOnly(Array.Empty<SymbolId>());

        private static IReadOnlyList<SymbolId> ReadOnly(IEnumerable<SymbolId> values)
            => new ReadOnlyCollection<SymbolId>(values.ToList());
    }
}
