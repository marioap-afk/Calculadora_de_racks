using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>The snapshot side on which a symbol result was used while planning.</summary>
    public enum SymbolObservationPhase
    {
        Before = 1,
        After = 2,
    }

    /// <summary>A comparable symbol result used by a mutation plan.</summary>
    public sealed class SymbolResultObservation
    {
        private SymbolResultObservation(SymbolId symbolId, SymbolObservationPhase phase, RegistrySymbolResult expectedResult)
        {
            Symbol = symbolId;
            Phase = phase;
            ExpectedResult = expectedResult ?? throw new ArgumentNullException(nameof(expectedResult));
        }

        public string SymbolId => Symbol.Key;
        internal SymbolId Symbol { get; }
        public SymbolObservationPhase Phase { get; }
        public RegistrySymbolResult ExpectedResult { get; }

        internal static SymbolResultObservation Capture(
            SymbolId symbolId, SymbolObservationPhase phase, RegistrySymbolResult expected)
            => new SymbolResultObservation(symbolId, phase, expected);

        public bool Matches(RegistrySymbolResult actual) => RegistryResultComparer.Equals(ExpectedResult, actual);
    }

    /// <summary>Stable structured identity of an intrinsic property-expression failure.</summary>
    public sealed class IntrinsicDiagnosticSignature : IEquatable<IntrinsicDiagnosticSignature>, IComparable<IntrinsicDiagnosticSignature>
    {
        private IntrinsicDiagnosticSignature(
            ExpressionDiagnosticCode code,
            string functionToken,
            int? argumentCount)
        {
            if (code == ExpressionDiagnosticCode.InvalidArguments)
            {
                FunctionToken = functionToken ?? throw new ArgumentNullException(nameof(functionToken));
                ArgumentCount = argumentCount ?? throw new ArgumentNullException(nameof(argumentCount));
            }
            else if (functionToken != null || argumentCount.HasValue)
            {
                throw new ArgumentException("Only InvalidArguments carries function and arity data.");
            }

            Code = code;
        }

        public ExpressionDiagnosticCode Code { get; }
        public string FunctionToken { get; }
        public int? ArgumentCount { get; }

        public static IntrinsicDiagnosticSignature InvalidArguments(string functionToken, int argumentCount)
            => new IntrinsicDiagnosticSignature(ExpressionDiagnosticCode.InvalidArguments, functionToken, argumentCount);

        public static IntrinsicDiagnosticSignature ForCode(ExpressionDiagnosticCode code)
        {
            if (code != ExpressionDiagnosticCode.DivisionByZero &&
                code != ExpressionDiagnosticCode.NonFiniteResult &&
                code != ExpressionDiagnosticCode.NonCanonicalForm)
            {
                throw new ArgumentOutOfRangeException(nameof(code), code, "This intrinsic diagnostic requires structured data or is not a source-local failure.");
            }
            return new IntrinsicDiagnosticSignature(code, null, null);
        }

        public int CompareTo(IntrinsicDiagnosticSignature other)
        {
            if (other == null)
            {
                return 1;
            }

            var byCode = ((int)Code).CompareTo((int)other.Code);
            if (byCode != 0 || Code != ExpressionDiagnosticCode.InvalidArguments)
            {
                return byCode;
            }

            var byToken = string.CompareOrdinal(FunctionToken, other.FunctionToken);
            return byToken != 0 ? byToken : ArgumentCount.Value.CompareTo(other.ArgumentCount.Value);
        }

        public bool Equals(IntrinsicDiagnosticSignature other) => other != null && CompareTo(other) == 0;
        public override bool Equals(object obj) => obj is IntrinsicDiagnosticSignature other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Code, FunctionToken, ArgumentCount);
        public override string ToString()
            => Code == ExpressionDiagnosticCode.InvalidArguments
                ? Code + ":" + FunctionToken + ":" + ArgumentCount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : Code.ToString();
    }

    /// <summary>The stable semantic reason why a property source may be removed by explicit rack repair.</summary>
    public sealed class RepairDecisionReason : IEquatable<RepairDecisionReason>
    {
        private RepairDecisionReason(
            string kind,
            IReadOnlyList<string> stableData,
            IReadOnlyDictionary<SymbolId, IReadOnlyList<RootSignature>> failedReads,
            IReadOnlyList<IntrinsicDiagnosticSignature> intrinsicSignatures = null)
        {
            Kind = kind;
            StableData = stableData;
            FailedReads = failedReads;
            IntrinsicSignatures = intrinsicSignatures ?? Array.Empty<IntrinsicDiagnosticSignature>();
        }

        public string Kind { get; }
        public IReadOnlyList<string> StableData { get; }
        public IReadOnlyDictionary<SymbolId, IReadOnlyList<RootSignature>> FailedReads { get; }
        public IReadOnlyList<IntrinsicDiagnosticSignature> IntrinsicSignatures { get; }

        public static RepairDecisionReason Create(string kind)
            => new RepairDecisionReason(kind, Array.Empty<string>(), EmptyFailedReads());

        public static RepairDecisionReason Create(string kind, string[] values)
            => Create(kind, (IReadOnlyList<object>)(values ?? Array.Empty<string>()).Cast<object>().ToArray());

        public static RepairDecisionReason Create(string kind, string code)
            => string.Equals(kind, "Intrinsic", StringComparison.Ordinal)
                ? Intrinsic(new[] { ParseIntrinsic(code) })
                : new RepairDecisionReason(kind, new[] { code }, EmptyFailedReads());

        public static RepairDecisionReason Create(string kind, string code, string token, int argumentCount)
            => string.Equals(kind, "Intrinsic", StringComparison.Ordinal) &&
               string.Equals(code, ExpressionDiagnosticCode.InvalidArguments.ToString(), StringComparison.Ordinal)
                ? Intrinsic(new[] { IntrinsicDiagnosticSignature.InvalidArguments(token, argumentCount) })
                : new RepairDecisionReason(kind, new[] { code, token, argumentCount.ToString(System.Globalization.CultureInfo.InvariantCulture) }, EmptyFailedReads());

        public static RepairDecisionReason Create(string kind, double presentationValue, string presentation)
            => new RepairDecisionReason(kind, Array.Empty<string>(), EmptyFailedReads());

        public static RepairDecisionReason Create(string kind, IReadOnlyList<object> data)
        {
            if (string.Equals(kind, "Domain", StringComparison.Ordinal))
            {
                return Domain();
            }
            if (string.Equals(kind, "MissingTarget", StringComparison.Ordinal))
            {
                return MissingTarget((data ?? Array.Empty<object>()).Select(value =>
                    SymbolId.ProjectVariable(value?.ToString() ?? string.Empty)));
            }
            if (string.Equals(kind, "Intrinsic", StringComparison.Ordinal))
            {
                return Intrinsic(ParseIntrinsicData(data));
            }
            var values = (data ?? Array.Empty<object>()).Select(value => value?.ToString() ?? string.Empty);
            return new RepairDecisionReason(kind, new ReadOnlyCollection<string>(values.ToArray()), EmptyFailedReads());
        }

        internal static RepairDecisionReason MissingTarget(IEnumerable<SymbolId> missing)
        {
            var canonical = new Dictionary<SymbolId, SymbolId>();
            foreach (var id in missing ?? Array.Empty<SymbolId>())
            {
                if (!canonical.TryGetValue(id, out var stored) || string.CompareOrdinal(id.Key, stored.Key) < 0)
                {
                    canonical[id] = id;
                }
            }

            return new RepairDecisionReason(
                "MissingTarget",
                new ReadOnlyCollection<string>(canonical.Values.OrderBy(id => id).Select(id => id.Key).ToArray()),
                EmptyFailedReads());
        }

        internal static RepairDecisionReason Intrinsic(IEnumerable<IntrinsicDiagnosticSignature> signatures)
        {
            var canonical = (signatures ?? Array.Empty<IntrinsicDiagnosticSignature>()).Distinct().OrderBy(value => value).ToArray();
            return new RepairDecisionReason(
                "Intrinsic",
                new ReadOnlyCollection<string>(canonical.Select(value => value.ToString()).ToArray()),
                EmptyFailedReads(),
                new ReadOnlyCollection<IntrinsicDiagnosticSignature>(canonical));
        }

        internal static RepairDecisionReason Upstream(
            IEnumerable<KeyValuePair<SymbolId, RegistrySymbolResult>> failedReads)
        {
            var ordered = new SortedDictionary<SymbolId, IReadOnlyList<RootSignature>>();
            foreach (var read in failedReads)
            {
                ordered[read.Key] = new ReadOnlyCollection<RootSignature>(read.Value.RootCauses.OrderBy(root => root).ToArray());
            }

            return new RepairDecisionReason(
                "Upstream", Array.Empty<string>(),
                new ReadOnlyDictionary<SymbolId, IReadOnlyList<RootSignature>>(ordered));
        }

        internal static RepairDecisionReason Domain()
            => new RepairDecisionReason("Domain", Array.Empty<string>(), EmptyFailedReads());

        public bool Equals(RepairDecisionReason other)
        {
            if (other == null || !string.Equals(Kind, other.Kind, StringComparison.Ordinal) ||
                !StableData.SequenceEqual(other.StableData, StringComparer.Ordinal) ||
                !IntrinsicSignatures.SequenceEqual(other.IntrinsicSignatures) ||
                FailedReads.Count != other.FailedReads.Count)
            {
                return false;
            }

            foreach (var expected in FailedReads)
            {
                if (!other.FailedReads.TryGetValue(expected.Key, out var actual) ||
                    !expected.Value.SequenceEqual(actual))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) => obj is RepairDecisionReason other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Kind);
        public override string ToString()
            => StableData.Count == 0 ? Kind : Kind + "(" + string.Join(",", StableData) + ")";

        private static IReadOnlyDictionary<SymbolId, IReadOnlyList<RootSignature>> EmptyFailedReads()
            => new ReadOnlyDictionary<SymbolId, IReadOnlyList<RootSignature>>(
                new Dictionary<SymbolId, IReadOnlyList<RootSignature>>());

        private static IEnumerable<IntrinsicDiagnosticSignature> ParseIntrinsicData(IReadOnlyList<object> data)
        {
            var values = data ?? Array.Empty<object>();
            if (values.Count == 3 &&
                string.Equals(values[0]?.ToString(), ExpressionDiagnosticCode.InvalidArguments.ToString(), StringComparison.Ordinal) &&
                int.TryParse(values[2]?.ToString(), System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var count))
            {
                return new[] { IntrinsicDiagnosticSignature.InvalidArguments(values[1]?.ToString() ?? string.Empty, count) };
            }

            return values.Select(value => value as IntrinsicDiagnosticSignature ?? ParseIntrinsic(value?.ToString() ?? string.Empty));
        }

        private static IntrinsicDiagnosticSignature ParseIntrinsic(string value)
        {
            var parts = (value ?? string.Empty).Split(':');
            if (parts.Length == 3 &&
                string.Equals(parts[0], ExpressionDiagnosticCode.InvalidArguments.ToString(), StringComparison.Ordinal) &&
                int.TryParse(parts[2], System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var count))
            {
                return IntrinsicDiagnosticSignature.InvalidArguments(parts[1], count);
            }

            if (!Enum.TryParse(value, ignoreCase: false, out ExpressionDiagnosticCode code))
            {
                throw new ArgumentException("Unknown intrinsic diagnostic code: " + value, nameof(value));
            }
            return IntrinsicDiagnosticSignature.ForCode(code);
        }
    }

    /// <summary>A comparable decision to remove one persisted property source.</summary>
    public sealed class RepairDecisionObservation
    {
        private RepairDecisionObservation(
            string rackId,
            PropertyId propertyId,
            SelectivePropertyValueDocument source,
            RepairDecisionReason expectedRepairReason)
        {
            RackId = rackId;
            PropertyId = propertyId;
            Source = source;
            ExpectedRepairReason = expectedRepairReason;
        }

        public string RackId { get; }
        public PropertyId PropertyId { get; }
        public SelectivePropertyValueDocument Source { get; }
        public RepairDecisionReason ExpectedRepairReason { get; }

        internal static RepairDecisionObservation Capture(
            string rackId, PropertyId propertyId, SelectivePropertyValueDocument source, RepairDecisionReason expected)
            => new RepairDecisionObservation(rackId, propertyId, source, expected);

        public bool Matches(IReadOnlyDictionary<SymbolId, RegistrySymbolResult> actual)
            => ExpectedRepairReason.Equals(RepairDecisionReason.Upstream(actual.OrderBy(pair => pair.Key)));

        public bool Matches(RepairDecisionReason actual) => ExpectedRepairReason.Equals(actual);
    }

    /// <summary>The exact semantic reads a plan must reaccredit before its first physical write.</summary>
    public sealed class PlanReadSet
    {
        public PlanReadSet(
            IEnumerable<SymbolResultObservation> symbolResultObservations,
            IEnumerable<RepairDecisionObservation> repairDecisionObservations)
        {
            SymbolResultObservations = new ReadOnlyCollection<SymbolResultObservation>(
                (symbolResultObservations ?? Array.Empty<SymbolResultObservation>())
                .OrderBy(item => item.Phase).ThenBy(item => item.Symbol).ToArray());
            RepairDecisionObservations = new ReadOnlyCollection<RepairDecisionObservation>(
                (repairDecisionObservations ?? Array.Empty<RepairDecisionObservation>())
                .OrderBy(item => item.RackId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.PropertyId.Value, StringComparer.Ordinal).ToArray());

            if (SymbolResultObservations.GroupBy(item => (item.Symbol, item.Phase)).Any(group => group.Count() != 1))
            {
                throw new ArgumentException("A PlanReadSet cannot observe the same symbol twice in one phase.");
            }
        }

        public IReadOnlyList<SymbolResultObservation> SymbolResultObservations { get; }
        public IReadOnlyList<RepairDecisionObservation> RepairDecisionObservations { get; }
        public bool IsEmpty => SymbolResultObservations.Count == 0 && RepairDecisionObservations.Count == 0;
        public static PlanReadSet Empty { get; } = new PlanReadSet(null, null);
    }

    internal static class RegistryResultComparer
    {
        internal static bool Equals(RegistrySymbolResult expected, RegistrySymbolResult actual)
        {
            if (expected == null || actual == null || expected.Succeeded != actual.Succeeded)
            {
                return false;
            }

            if (expected.Succeeded)
            {
                return expected.Value.Equals(actual.Value);
            }

            return DiagnosticsEqual(expected.Diagnostics, actual.Diagnostics) &&
                   expected.RootCauses.SequenceEqual(actual.RootCauses);
        }

        private static bool DiagnosticsEqual(
            IReadOnlyList<RegistryDiagnostic> expected, IReadOnlyList<RegistryDiagnostic> actual)
        {
            if (expected.Count != actual.Count)
            {
                return false;
            }

            for (var index = 0; index < expected.Count; index++)
            {
                var left = expected[index];
                var right = actual[index];
                if (left.Code != right.Code || left.Cause != right.Cause ||
                    !left.RelatedSymbols.SequenceEqual(right.RelatedSymbols) ||
                    !left.Chain.SequenceEqual(right.Chain) || !object.Equals(left.Root, right.Root) ||
                    !string.Equals(left.FunctionToken, right.FunctionToken, StringComparison.Ordinal) ||
                    left.ArgumentCount != right.ArgumentCount)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
