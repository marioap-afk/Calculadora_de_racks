using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Expressions;
using Xunit.Sdk;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// Compile-safe bridge for the G7 RED contract. It discovers capabilities by semantic shape so the tests do not
    /// prescribe an algorithm or a concrete implementation class. Once G7 exists, this bridge projects its public or
    /// test-visible result into assertions over the frozen behavior.
    /// </summary>
    internal static class G7ContractTestSupport
    {
        internal static SymbolEntry Literal(int id, double value = 1)
            => new SymbolEntry(Id(id), SymbolScope.Project, "S" + id, SymbolDefinition.FromLiteral(value));

        internal static SymbolEntry Expression(int id, BoundExpression expression)
            => new SymbolEntry(Id(id), SymbolScope.Project, "S" + id, SymbolDefinition.FromExpression(expression));

        internal static IReadOnlyList<SymbolId> Dependencies(BoundExpression expression)
        {
            var method = ExpressionAssembly()
                .GetTypes()
                .Where(type => type.Namespace == typeof(BoundExpression).Namespace)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(candidate => candidate.Name.IndexOf("Depend", StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(candidate => candidate.GetParameters().Length == 1)
                .Where(candidate => candidate.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(BoundExpression)))
                .FirstOrDefault(candidate => typeof(IEnumerable<SymbolId>).IsAssignableFrom(candidate.ReturnType));

            if (method == null)
            {
                throw Missing("pure dependency extraction over BoundExpression");
            }

            return ((IEnumerable<SymbolId>)method.Invoke(null, new object[] { expression })).ToArray();
        }

        internal static G7EvaluationView Evaluate(params SymbolEntry[] entries)
        {
            var assembly = ExpressionAssembly();
            var resultType = assembly.GetTypes().FirstOrDefault(type => type.Name == "RegistryEvaluation");
            if (resultType == null)
            {
                throw Missing("RegistryEvaluation");
            }

            var context = Context(entries);
            var factory = assembly.GetTypes()
                .Where(type => type.Namespace == typeof(BoundExpression).Namespace)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(method => method.ReturnType == resultType)
                .Select(method => new { Method = method, Parameters = method.GetParameters() })
                .FirstOrDefault(candidate => candidate.Parameters.Length == 1
                    && (candidate.Parameters[0].ParameterType.IsAssignableFrom(typeof(ExpressionContext))
                        || candidate.Parameters[0].ParameterType.IsAssignableFrom(typeof(SymbolTable))));

            object raw;
            if (factory != null)
            {
                var argument = factory.Parameters[0].ParameterType.IsAssignableFrom(typeof(ExpressionContext))
                    ? (object)context
                    : context.Symbols;
                raw = factory.Method.Invoke(null, new[] { argument });
            }
            else
            {
                var constructor = resultType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(candidate => new { Constructor = candidate, Parameters = candidate.GetParameters() })
                    .FirstOrDefault(candidate => candidate.Parameters.Length == 1
                        && (candidate.Parameters[0].ParameterType.IsAssignableFrom(typeof(ExpressionContext))
                            || candidate.Parameters[0].ParameterType.IsAssignableFrom(typeof(SymbolTable))));

                if (constructor == null)
                {
                    throw Missing("a RegistryEvaluation factory over the immutable expression snapshot");
                }

                var argument = constructor.Parameters[0].ParameterType.IsAssignableFrom(typeof(ExpressionContext))
                    ? (object)context
                    : context.Symbols;
                raw = constructor.Constructor.Invoke(new[] { argument });
            }

            return new G7EvaluationView(raw ?? throw Missing("a non-null RegistryEvaluation result"));
        }

        internal static string IdKey(SymbolId id) => id.Key;

        internal static string CycleKey(params int[] members)
            => "CycleRoot[" + string.Join(",", members.Select(member => IdKey(Id(member)))) + "]";

        internal static string RootKey(int owner, ExpressionDiagnosticCode code, string stableData = "")
            => IdKey(Id(owner)) + ":" + code + (string.IsNullOrEmpty(stableData) ? string.Empty : ":" + stableData);

        internal static IDisposable WithCulture(string name)
            => new CultureScope(name);

        private static Assembly ExpressionAssembly() => typeof(BoundExpression).Assembly;

        private static XunitException Missing(string capability)
            => new XunitException("G7 RED: production does not yet expose " + capability + ".");

        private sealed class CultureScope : IDisposable
        {
            private readonly System.Globalization.CultureInfo _culture;
            private readonly System.Globalization.CultureInfo _uiCulture;

            internal CultureScope(string name)
            {
                _culture = System.Globalization.CultureInfo.CurrentCulture;
                _uiCulture = System.Globalization.CultureInfo.CurrentUICulture;
                var next = System.Globalization.CultureInfo.GetCultureInfo(name);
                System.Globalization.CultureInfo.CurrentCulture = next;
                System.Globalization.CultureInfo.CurrentUICulture = next;
            }

            public void Dispose()
            {
                System.Globalization.CultureInfo.CurrentCulture = _culture;
                System.Globalization.CultureInfo.CurrentUICulture = _uiCulture;
            }
        }
    }

    internal sealed class G7EvaluationView
    {
        private readonly object _raw;
        private readonly object _graph;

        internal G7EvaluationView(object raw)
        {
            _raw = raw;
            _graph = ReflectionView.OptionalMember(raw, "DependencyGraph", "Graph") ?? raw;
        }

        internal IReadOnlyList<SymbolId> DirectDependencies(int id)
            => ReflectionView.IdsFromCall(_graph, Id(id), "DirectDependencies", "Dependencies", "DependenciesOf");

        internal IReadOnlyList<SymbolId> DirectDependents(int id)
            => ReflectionView.IdsFromCall(_graph, Id(id), "DirectDependents", "Dependents", "DependentsOf");

        internal IReadOnlyList<SymbolId> TransitiveDependencies(int id)
            => ReflectionView.IdsFromCall(_graph, Id(id), "TransitiveDependencies", "DependencyClosure");

        internal IReadOnlyList<SymbolId> TransitiveDependents(int id)
            => ReflectionView.IdsFromCall(_graph, Id(id), "TransitiveDependents", "DependentClosure", "Affected");

        internal IReadOnlyList<SymbolId> EvaluationOrder
            => ReflectionView.Ids(ReflectionView.RequiredMember(_raw, "EvaluationOrder", "TopologicalOrder"));

        internal IReadOnlyList<IReadOnlyList<SymbolId>> Cycles
        {
            get
            {
                var rawCycles = ReflectionView.Items(ReflectionView.RequiredMember(_raw, "Cycles"));
                return rawCycles.Select(cycle =>
                {
                    if (cycle is SymbolId)
                    {
                        throw new XunitException("G7 projection: a cycle must expose its complete member set.");
                    }

                    var members = ReflectionView.OptionalMember(cycle, "Members", "Symbols") ?? cycle;
                    return (IReadOnlyList<SymbolId>)ReflectionView.Ids(members);
                }).ToArray();
            }
        }

        internal bool IsSimpleCycle(params int[] members)
        {
            var ids = members.Select(Id).ToArray();
            return ReflectionView.BoolFromCall(_graph, ids, "IsSimpleCycle");
        }

        internal G7SymbolView Result(int id)
        {
            var symbol = Id(id);
            var direct = ReflectionView.OptionalCall(_raw, symbol, "Result", "ResultOf", "GetResult");
            if (direct != null)
            {
                return new G7SymbolView(direct);
            }

            var results = ReflectionView.RequiredMember(_raw, "Results", "SymbolResults");
            return new G7SymbolView(ReflectionView.DictionaryValue(results, symbol));
        }

        internal string StableSnapshot(params int[] symbols)
        {
            var cycles = string.Join(";", Cycles.Select(cycle => string.Join(",", cycle.Select(G7ContractTestSupport.IdKey))));
            var order = string.Join(",", EvaluationOrder.Select(G7ContractTestSupport.IdKey));
            var results = string.Join(";", symbols.Select(id => G7ContractTestSupport.IdKey(Id(id)) + "=" + Result(id).StableSnapshot));
            return "cycles=" + cycles + "|order=" + order + "|results=" + results;
        }
    }

    internal sealed class G7SymbolView
    {
        private readonly object _raw;

        internal G7SymbolView(object raw) => _raw = raw;

        internal bool Succeeded
        {
            get
            {
                var value = ReflectionView.OptionalMember(_raw, "Succeeded", "IsSuccess");
                if (value is bool succeeded)
                {
                    return succeeded;
                }

                var outcome = ReflectionView.RequiredMember(_raw, "Outcome").ToString();
                return string.Equals(outcome, "Success", StringComparison.OrdinalIgnoreCase);
            }
        }

        internal double Value => Convert.ToDouble(ReflectionView.RequiredMember(_raw, "Value"), System.Globalization.CultureInfo.InvariantCulture);

        internal IReadOnlyList<G7DiagnosticView> Diagnostics
            => ReflectionView.Items(ReflectionView.RequiredMember(_raw, "Diagnostics"))
                .Select(item => new G7DiagnosticView(item))
                .ToArray();

        internal IReadOnlyList<string> RootCauses
            => ReflectionView.Items(ReflectionView.RequiredMember(_raw, "RootCauses"))
                .Select(ReflectionView.RootKey)
                .ToArray();

        internal string StableSnapshot
            => (Succeeded ? "ok:" + Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture) : "failed")
                + "|diagnostics=" + string.Join(",", Diagnostics.Select(diagnostic => diagnostic.StableKey))
                + "|roots=" + string.Join(",", RootCauses);
    }

    internal sealed class G7DiagnosticView
    {
        private readonly object _raw;

        internal G7DiagnosticView(object raw) => _raw = raw;

        internal ExpressionDiagnosticCode Code
            => ReflectionView.Code(ReflectionView.RequiredMember(_raw, "Code"));

        internal SymbolId Cause
            => ReflectionView.OptionalMember(_raw, "Cause", "DirectCause", "FailedDependency") as SymbolId;

        internal IReadOnlyList<SymbolId> RelatedSymbols
            => ReflectionView.Ids(ReflectionView.OptionalMember(_raw, "RelatedSymbols", "MissingSymbols") ?? Array.Empty<SymbolId>());

        internal IReadOnlyList<SymbolId> Chain
            => ReflectionView.Ids(ReflectionView.OptionalMember(_raw, "Chain", "RepresentativeChain") ?? Array.Empty<SymbolId>());

        internal string Root
        {
            get
            {
                var root = ReflectionView.OptionalMember(_raw, "Root", "RootSignature");
                return root == null ? null : ReflectionView.RootKey(root);
            }
        }

        internal string FunctionToken
            => ReflectionView.OptionalMember(_raw, "FunctionToken", "Token")?.ToString();

        internal int? ArgumentCount
        {
            get
            {
                var value = ReflectionView.OptionalMember(_raw, "ArgumentCount", "Arity");
                return value == null ? null : Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        internal string StableKey
            => Code
                + (Cause == null ? string.Empty : ":cause=" + G7ContractTestSupport.IdKey(Cause))
                + (RelatedSymbols.Count == 0 ? string.Empty : ":related=" + string.Join("+", RelatedSymbols.Select(G7ContractTestSupport.IdKey)))
                + (Chain.Count == 0 ? string.Empty : ":chain=" + string.Join("+", Chain.Select(G7ContractTestSupport.IdKey)))
                + (Root == null ? string.Empty : ":root=" + Root)
                + (FunctionToken == null ? string.Empty : ":fn=" + FunctionToken + "/" + ArgumentCount);
    }

    internal static class ReflectionView
    {
        private const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        internal static object RequiredMember(object target, params string[] names)
            => OptionalMember(target, names)
               ?? throw new XunitException("G7 projection: missing member " + string.Join("/", names) + " on " + target.GetType().FullName + ".");

        internal static object OptionalMember(object target, params string[] names)
        {
            foreach (var name in names)
            {
                var property = target.GetType().GetProperty(name, InstanceFlags);
                if (property != null)
                {
                    return property.GetValue(target);
                }

                var field = target.GetType().GetField(name, InstanceFlags);
                if (field != null)
                {
                    return field.GetValue(target);
                }
            }

            return null;
        }

        internal static object OptionalCall(object target, object argument, params string[] names)
        {
            foreach (var name in names)
            {
                var method = target.GetType().GetMethods(InstanceFlags)
                    .FirstOrDefault(candidate => candidate.Name == name
                        && candidate.GetParameters().Length == 1
                        && candidate.GetParameters()[0].ParameterType.IsInstanceOfType(argument));
                if (method != null)
                {
                    return method.Invoke(target, new[] { argument });
                }
            }

            return null;
        }

        internal static IReadOnlyList<SymbolId> IdsFromCall(object target, SymbolId id, params string[] names)
            => Ids(OptionalCall(target, id, names)
                ?? throw new XunitException("G7 projection: missing graph query " + string.Join("/", names) + "."));

        internal static bool BoolFromCall(object target, IReadOnlyList<SymbolId> ids, params string[] names)
        {
            foreach (var name in names)
            {
                var method = target.GetType().GetMethods(InstanceFlags)
                    .FirstOrDefault(candidate => candidate.Name == name && candidate.GetParameters().Length == 1);
                if (method != null)
                {
                    return (bool)method.Invoke(target, new object[] { ids });
                }
            }

            throw new XunitException("G7 projection: missing simple-cycle predicate.");
        }

        internal static IReadOnlyList<SymbolId> Ids(object value)
            => Items(value).Select(item => item as SymbolId
                ?? throw new XunitException("G7 projection: expected SymbolId, found " + item.GetType().FullName + ".")).ToArray();

        internal static IReadOnlyList<object> Items(object value)
        {
            if (value is string || !(value is IEnumerable enumerable))
            {
                throw new XunitException("G7 projection: expected an enumerable structural value.");
            }

            return enumerable.Cast<object>().ToArray();
        }

        internal static object DictionaryValue(object dictionary, SymbolId key)
        {
            var tryGet = dictionary.GetType().GetMethods(InstanceFlags)
                .FirstOrDefault(method => method.Name == "TryGetValue" && method.GetParameters().Length == 2);
            if (tryGet != null)
            {
                var arguments = new object[] { key, null };
                if ((bool)tryGet.Invoke(dictionary, arguments))
                {
                    return arguments[1];
                }
            }

            var indexer = dictionary.GetType().GetProperty("Item", InstanceFlags);
            if (indexer != null)
            {
                return indexer.GetValue(dictionary, new object[] { key });
            }

            throw new XunitException("G7 projection: result map cannot be queried by SymbolId.");
        }

        internal static ExpressionDiagnosticCode Code(object value)
            => value is ExpressionDiagnosticCode code
                ? code
                : Enum.TryParse(value.ToString(), out ExpressionDiagnosticCode parsed)
                    ? parsed
                    : throw new XunitException("G7 projection: undeclared diagnostic code " + value + ".");

        internal static string RootKey(object root)
        {
            var members = OptionalMember(root, "Members", "CycleMembers");
            if (members != null)
            {
                return "CycleRoot[" + string.Join(",", Ids(members).Select(G7ContractTestSupport.IdKey)) + "]";
            }

            var owner = RequiredMember(root, "Owner", "Symbol", "OwnerId") as SymbolId
                ?? throw new XunitException("G7 projection: an intrinsic root must expose its owner SymbolId.");
            var code = Code(RequiredMember(root, "Code"));
            var missing = OptionalMember(root, "MissingSymbols", "RelatedSymbols");
            var functionToken = OptionalMember(root, "FunctionToken", "Token");
            var argumentCount = OptionalMember(root, "ArgumentCount", "Arity");
            var data = missing != null
                ? string.Join("+", Ids(missing).Select(G7ContractTestSupport.IdKey))
                : functionToken != null
                    ? functionToken + "/" + Convert.ToInt32(argumentCount, System.Globalization.CultureInfo.InvariantCulture)
                    : string.Empty;
            return G7ContractTestSupport.RootKey(int.Parse(owner.Key.Substring(owner.Key.Length - 12), System.Globalization.NumberStyles.HexNumber), code, data);
        }
    }
}
