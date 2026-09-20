using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit.Sdk;

namespace RackCad.Tests
{
    /// <summary>
    /// Compile-safe bridge to the still-missing G9 behavior. The bridge never compares, plans, or assesses a
    /// recovery itself: it passes real product values to a product factory/operation and projects its result.
    /// Consequently a missing operation is an intentional RED, while an implemented operation must satisfy the
    /// semantic assertions in the tests.
    /// </summary>
    internal static class G9BehavioralContractSupport
    {
        internal sealed class RecoveryView
        {
            internal RecoveryView(object raw) { Raw = raw; }
            internal object Raw { get; }
            internal bool IsCandidate => Bool(Raw, "IsCandidate", "HasCandidate", "CanRecover");
            internal string Candidate => Text(Raw, "Candidate", "RecoveryCandidate");
            internal string[] Units => Strings(Raw, "RecoveryUnits", "Units");
            internal string[] Reasons => Strings(Raw, "BlockingReasons", "Reasons");
            internal string[] ReasonData => Strings(Raw, "BlockingReasonData", "ReasonData", "Details");
            internal string[] SourceRoots => Strings(Raw, "SourceRoots", "Roots");
            internal string[] AbortCauses => Strings(Raw, "DiscoveryAbortCauses", "AbortCauses");
            internal string[] StructuredAbortCauses
                => Items(Member(Raw, "DiscoveryAbortCauses", "AbortCauses")).Select(StructuredAbortCause).ToArray();
            internal string Classification => Text(Raw, "SourceClassification", "Classification", "InspectionOutcome");
            internal string RackRepairability => Text(Raw, "RackRepairability", "Repairability", "RackState");
            internal bool HasRepairPlan => Bool(Raw, "HasRepairPlan", "CanRepair", "IsRepairable");

            internal string StableSignature => string.Join("|", new[]
            {
                "candidate=" + IsCandidate,
                "candidateValue=" + Candidate,
                "units=" + string.Join(",", Units),
                "reasons=" + string.Join(",", Reasons),
                "roots=" + string.Join(",", SourceRoots),
                "abort=" + string.Join(",", StructuredAbortCauses),
                "repairability=" + RackRepairability,
                "repairPlan=" + HasRepairPlan,
            });
        }

        internal sealed class AttemptFailureView
        {
            internal AttemptFailureView(VariableMutationPreflightResult result)
            {
                Result = result;
                RawFailure = Member(result, "AttemptedStateFailure", "AttemptFailure", "Failure");
            }

            internal VariableMutationPreflightResult Result { get; }
            internal object RawFailure { get; }
            internal string RackId => Text(RawFailure, "RackId", "FailedRackId");
            internal string Category => Text(RawFailure, "Category", "Code", "Classification");
            internal string[] DiagnosticCodes => Strings(RawFailure, "DiagnosticCodes", "Diagnostics");
            internal bool HasRecoveryCandidate => Bool(RawFailure, "HasRecoveryCandidate", "RecoveryCandidatePresent");
            internal string[] PriorBlockingReasons => Strings(Result, "PriorBlockingReasons", "PriorStateReasons");
        }

        internal sealed class CommitView
        {
            internal CommitView(object raw) { Raw = raw; }
            internal object Raw { get; }
            internal bool Succeeded => Bool(Raw, "Succeeded", "IsSuccess", "IsApplied");
            internal int RegistryWrites => Int(Raw, "RegistryWrites", "RegistryWriteCount");
            internal int RackWrites => Int(Raw, "RackWrites", "RackWriteCount");
            internal int SemanticReads => Int(Raw, "SemanticReads", "SemanticReadCount");
        }

        internal sealed class MutationWriteCounter
        {
            internal int RegistryWrites { get; private set; }
            internal int RackWrites { get; private set; }
            internal void WriteRegistry() => RegistryWrites++;
            internal void WriteRack() => RackWrites++;
        }

        internal static VariableMutationPreflightResult ChangeDefinition(
            ProjectVariablesDocument registry, VariableId id, VariableDefinition definition,
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            var method = typeof(ProjectVariableMutationPreflight).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .SingleOrDefault(candidate => candidate.Name == "ChangeDefinition");
            if (method == null) throw Missing("ProjectVariableMutationPreflight.ChangeDefinition behavior");
            var raw = method.Invoke(null, new object[] { registry, id, definition, entries });
            return raw as VariableMutationPreflightResult ?? throw Missing("ChangeDefinition result");
        }

        internal static AttemptFailureView ChangeDefinitionAttempt(
            ProjectVariablesDocument registry, VariableId id, VariableDefinition definition,
            IReadOnlyList<ProjectVariableScanEntry> entries)
            => new AttemptFailureView(ChangeDefinition(registry, id, definition, entries));

        internal static RegistrySymbolResult Success(double value)
            => new RegistrySymbolResult(true, value, Array.Empty<RegistryDiagnostic>(), Array.Empty<RootSignature>());

        internal static RegistrySymbolResult Failed(
            IEnumerable<RegistryDiagnostic> diagnostics,
            params RootSignature[] roots)
            => new RegistrySymbolResult(false, 0, RegistryDiagnostic.Ordered(diagnostics), roots.OrderBy(root => root).ToArray());

        internal static RootSignature Broken(string owner, params string[] missing)
            => RootSignature.Broken(Symbol(owner), missing.Select(Symbol));

        internal static RootSignature Invalid(string owner, string token, int count)
            => RootSignature.Invalid(Symbol(owner), new[] { new InvalidArgumentSignature(token, count) });

        internal static RootSignature Intrinsic(string owner, ExpressionDiagnosticCode code)
            => RootSignature.Intrinsic(Symbol(owner), code);

        internal static RootSignature Cycle(params string[] members)
            => RootSignature.Cycle(members.Select(Symbol).OrderBy(id => id).ToArray());

        internal static RegistryDiagnostic Dependency(string cause, RootSignature root, params string[] chain)
            => RegistryDiagnostic.DependencyFailed(Symbol(cause), chain.Select(Symbol).ToArray(), root);

        internal static RegistryDiagnostic InvalidDiagnostic(string token, int count)
            => RegistryDiagnostic.Invalid(new InvalidArgumentSignature(token, count));

        internal static RegistryDiagnostic IntrinsicDiagnostic(ExpressionDiagnosticCode code)
            => RegistryDiagnostic.Intrinsic(code);

        internal static bool SymbolMatches(RegistrySymbolResult expected, RegistrySymbolResult actual)
            => SymbolResultObservation.Capture(
                Symbol(G9ContractTestSupport.IdA), SymbolObservationPhase.Before, expected).Matches(actual);

        internal static bool UpstreamMatches(
            IReadOnlyDictionary<SymbolId, RegistrySymbolResult> expected,
            IReadOnlyDictionary<SymbolId, RegistrySymbolResult> actual)
            => RepairDecisionReason.Upstream(expected).Equals(RepairDecisionReason.Upstream(actual));

        internal static bool RepairReasonMatches(object expected, object actual, string reason)
            => ((RepairDecisionReason)expected).Equals((RepairDecisionReason)actual);

        internal static object MissingTarget(params string[] missing)
            => CreateReason("MissingTarget", missing);

        internal static object IntrinsicReason(params object[] signatures)
            => CreateReason("Intrinsic", signatures);

        internal static object DomainReason(double presentationValue, string presentation)
            => CreateReason("Domain", presentationValue, presentation);

        internal static object HealthyReason() => CreateReason("Healthy");

        internal static RecoveryView AssessRecovery(
            ProjectVariablesDocument registry,
            IReadOnlyList<ProjectVariableScanEntry> entries,
            string rackId,
            string propertyId)
        {
            var type = ProductType("RecoveryAssessment", "RecoveryEligibility");
            var raw = InvokeCompatibleStatic(type, new object[] { registry, entries, rackId, propertyId },
                "Assess", "Evaluate", "Create");
            return new RecoveryView(raw);
        }

        internal static CommitView Commit(MutationPlan plan, ProjectVariablesDocument currentRegistry,
            IReadOnlyList<ProjectVariableScanEntry> currentRacks, object writeSpy)
            => new CommitView(RegistryCommit.Prepare(plan, currentRegistry, currentRacks));

        internal static object NewWriteSpy()
            => new MutationWriteCounter();

        private static object CreateReason(string kind, params object[] data)
            => RepairDecisionReason.Create(kind, data);

        private static Type ProductType(params string[] names)
            => typeof(MutationPlan).Assembly.GetTypes().FirstOrDefault(type => names.Any(name =>
                string.Equals(type.Name, name, StringComparison.Ordinal)))
                ?? throw Missing(string.Join("/", names) + " product behavior");

        private static object InvokeCompatibleStatic(Type type, object[] args, params string[] names)
            => TryInvokeCompatibleStatic(type, args, names) ?? throw Missing(type.Name + "." + string.Join("/", names));

        private static object TryInvokeCompatibleStatic(Type type, object[] args, params string[] names)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(method => names.Any(name => string.Equals(method.Name, name, StringComparison.OrdinalIgnoreCase))))
            {
                if (TryBind(method.GetParameters(), args, out var bound)) return method.Invoke(null, bound);
            }
            return null;
        }

        private static object TryInvokeCompatibleInstance(object target, object[] args, params string[] names)
        {
            foreach (var method in target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(method => names.Any(name => string.Equals(method.Name, name, StringComparison.OrdinalIgnoreCase))))
            {
                if (TryBind(method.GetParameters(), args, out var bound)) return method.Invoke(target, bound);
            }
            return null;
        }

        private static bool TryBind(ParameterInfo[] parameters, object[] available, out object[] bound)
        {
            bound = new object[parameters.Length]; var used = new bool[available.Length];
            for (var index = 0; index < parameters.Length; index++)
            {
                var found = -1;
                for (var item = 0; item < available.Length; item++)
                    if (!used[item] && available[item] != null && parameters[index].ParameterType.IsInstanceOfType(available[item])) { found = item; break; }
                if (found < 0)
                {
                    if (parameters[index].HasDefaultValue) { bound[index] = parameters[index].DefaultValue; continue; }
                    return false;
                }
                used[found] = true; bound[index] = available[found];
            }
            return used.All(value => value);
        }

        private static SymbolId Symbol(string value) => SymbolId.ProjectVariable(value);

        private static object Member(object target, params string[] names)
        {
            foreach (var name in names)
            {
                var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null) return property.GetValue(target);
            }
            throw Missing(target.GetType().Name + "." + string.Join("/", names));
        }

        private static object MemberOrNull(object target, params string[] names)
        {
            foreach (var name in names)
            {
                var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null) return property.GetValue(target);
            }
            return null;
        }

        private static string StructuredAbortCause(object cause)
        {
            var kind = MemberOrNull(cause, "Kind", "Code", "Cause")?.ToString() ?? cause.GetType().Name;
            var identity = MemberOrNull(cause, "DefinitionId", "RackId", "Identity")?.ToString() ?? string.Empty;
            var unitsValue = MemberOrNull(cause, "RecoveryUnits", "Units");
            var units = unitsValue == null ? Array.Empty<string>() : Items(unitsValue).Select(value => value.ToString()).ToArray();
            return kind + "(" + identity + "){" + string.Join(",", units) + "}";
        }

        private static bool Bool(object target, params string[] names) => Convert.ToBoolean(Member(target, names));
        private static int Int(object target, params string[] names) => Convert.ToInt32(Member(target, names));
        private static string Text(object target, params string[] names) => Member(target, names)?.ToString();
        private static string[] Strings(object target, params string[] names)
            => ((IEnumerable)Member(target, names)).Cast<object>().Select(value => value.ToString()).ToArray();
        private static IEnumerable<object> Items(object value) => ((IEnumerable)value).Cast<object>();
        private static XunitException Missing(string detail) => new XunitException("G9 behavioral RED: product is missing " + detail + ".");
    }
}
