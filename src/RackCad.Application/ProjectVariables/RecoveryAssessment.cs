using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    public enum RecoveryBlockingReason
    {
        OtherInvalidSources = 1,
        SeveralRecoveryUnits = 2,
        NonSimpleCycle = 3,
        DiscoveryIndeterminate = 4,
    }

    public enum RecoveryDiscoveryAbortKind
    {
        EnvelopeUnclassifiable = 1,
        ProbeIndeterminate = 2,
        PartialPositives = 3,
        NonSingleAuthority = 4,
    }

    public sealed class RecoveryDiscoveryAbortCause
    {
        internal RecoveryDiscoveryAbortCause(
            RecoveryDiscoveryAbortKind kind, string identity, IReadOnlyList<string> recoveryUnits)
        {
            Kind = kind;
            Identity = identity;
            RecoveryUnits = recoveryUnits;
        }

        public RecoveryDiscoveryAbortKind Kind { get; }
        public string Identity { get; }
        public IReadOnlyList<string> RecoveryUnits { get; }

        public override string ToString() => Kind + "(" + Identity + ")";
    }

    /// <summary>Structured recovery eligibility for one failed property source.</summary>
    public sealed class RecoveryAssessment
    {
        private RecoveryAssessment(
            bool isCandidate,
            string candidate,
            IReadOnlyList<string> sourceRoots,
            IReadOnlyList<string> recoveryUnits,
            IReadOnlyList<RecoveryBlockingReason> blockingReasons,
            IReadOnlyList<string> blockingReasonData,
            RackRepairability rackRepairability,
            IReadOnlyList<RecoveryDiscoveryAbortCause> discoveryAbortCauses,
            bool hasRepairPlan,
            string sourceClassification)
        {
            IsCandidate = isCandidate;
            Candidate = candidate;
            SourceRoots = sourceRoots;
            RecoveryUnits = recoveryUnits;
            BlockingReasons = blockingReasons;
            BlockingReasonData = blockingReasonData;
            RackRepairability = rackRepairability;
            DiscoveryAbortCauses = discoveryAbortCauses;
            HasRepairPlan = hasRepairPlan;
            SourceClassification = sourceClassification;
        }

        public bool IsCandidate { get; }
        public string Candidate { get; }
        public IReadOnlyList<string> SourceRoots { get; }
        public IReadOnlyList<string> RecoveryUnits { get; }
        public IReadOnlyList<RecoveryBlockingReason> BlockingReasons { get; }
        public IReadOnlyList<string> BlockingReasonData { get; }
        public RackRepairability RackRepairability { get; }
        public IReadOnlyList<RecoveryDiscoveryAbortCause> DiscoveryAbortCauses { get; }
        public bool HasRepairPlan { get; }
        public string SourceClassification { get; }

        public static RecoveryAssessment Assess(
            ProjectVariablesDocument registry,
            IReadOnlyList<ProjectVariableScanEntry> entries,
            string rackId,
            string propertyId)
        {
            var accreditation = UsableProjectVariablesRegistry.Accredit(
                registry == null ? ProjectVariablesReadResult.Absent() : ProjectVariablesReadResult.Readable(registry));
            if (!accreditation.IsUsable)
            {
                return Empty("FatalRegistry", RackRepairability.Blocked, accreditation.Error);
            }

            var targetGroup = ProjectVariableConsumerDiscovery.GroupSelectiveByRack(entries)
                .FirstOrDefault(group => string.Equals(group.Key, rackId, StringComparison.OrdinalIgnoreCase));
            if (targetGroup.Value == null)
            {
                return Empty("FatalMissingRack", RackRepairability.Blocked, rackId);
            }

            var targetAuthority = SelectiveAuthoredAuthority.Resolve(targetGroup.Key, targetGroup.Value);
            if (!targetAuthority.IsSingle)
            {
                return Empty("FatalNonSingleAuthority", RackRepairability.Blocked, targetAuthority.Error);
            }

            var evaluation = RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(accreditation.Registry));
            var recoveryRank = (registry.Variables ?? new List<ProjectVariableDocument>())
                .Select((item, index) => (Id: SymbolId.ProjectVariable(item.VariableId), Index: index))
                .ToDictionary(item => item.Id, item => item.Index);
            var inspections = Inspect(targetAuthority.Authored, accreditation.Registry, evaluation);
            var source = inspections.FirstOrDefault(item =>
                string.Equals(item.PropertyToken, propertyId, StringComparison.Ordinal));
            if (source == null)
            {
                return Empty("Healthy", RackRepairability.Healthy, propertyId);
            }

            var repairability = RackRepairabilityAssessment.Of(inspections).Outcome;
            var roots = GetSourceRoots(source);
            var units = RecoveryUnitsOf(roots, recoveryRank);
            var unitNames = units.Select(item => item.Name).ToArray();
            var aborts = Discover(entries, evaluation.DependencyGraph, units);
            var reasonData = new List<string>();
            reasonData.AddRange(DescribeReason(source));
            reasonData.AddRange(aborts.Select(cause => cause.ToString()));

            var otherInvalid = FindBlockingSources(
                entries, targetGroup.Key, source.PropertyToken, accreditation.Registry, evaluation, recoveryRank,
                units, aborts, reasonData);

            var reasons = new List<RecoveryBlockingReason>();
            if (otherInvalid || source.Outcome != BindingInspectionOutcome.RepairableUpstream)
            {
                reasons.Add(RecoveryBlockingReason.OtherInvalidSources);
            }
            if (units.Count != 1 && source.Outcome == BindingInspectionOutcome.RepairableUpstream)
            {
                reasons.Add(RecoveryBlockingReason.SeveralRecoveryUnits);
            }
            if (units.Count == 1 && units[0].IsCycle &&
                !evaluation.DependencyGraph.IsSimpleCycle(units[0].Members))
            {
                reasons.Add(RecoveryBlockingReason.NonSimpleCycle);
            }
            if (aborts.Count > 0)
            {
                reasons.Add(RecoveryBlockingReason.DiscoveryIndeterminate);
            }

            var isCandidate = source.Outcome == BindingInspectionOutcome.RepairableUpstream &&
                units.Count == 1 && reasons.Count == 0;
            return new RecoveryAssessment(
                isCandidate,
                isCandidate ? units[0].Name : null,
                new ReadOnlyCollection<string>(roots.Select(FormatRoot).ToArray()),
                new ReadOnlyCollection<string>(unitNames),
                new ReadOnlyCollection<RecoveryBlockingReason>(reasons),
                new ReadOnlyCollection<string>(reasonData.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray()),
                repairability,
                new ReadOnlyCollection<RecoveryDiscoveryAbortCause>(aborts),
                repairability == RackRepairability.Repairable,
                Classification(source.Outcome));
        }

        private static RecoveryAssessment Empty(string classification, RackRepairability state, string detail)
            => new RecoveryAssessment(false, null, Array.Empty<string>(), Array.Empty<string>(),
                new[] { RecoveryBlockingReason.OtherInvalidSources },
                string.IsNullOrEmpty(detail) ? Array.Empty<string>() : new[] { detail },
                state, Array.Empty<RecoveryDiscoveryAbortCause>(), false, classification);

        internal static IReadOnlyList<BindingInspection> Inspect(
            SelectivePalletDesignDocument authored,
            UsableProjectVariablesRegistry registry,
            RegistryEvaluation evaluation)
        {
            var result = new List<BindingInspection>();
            if (authored?.PropertyValues == null)
            {
                return result;
            }

            foreach (var token in authored.PropertyValues.Keys.OrderBy(value => value, StringComparer.Ordinal))
            {
                result.Add(LinkedPropertyInspection.InspectBinding(
                    token, authored.PropertyValues[token], SelectiveLinkedProperties.All, registry, evaluation));
            }
            return result;
        }

        internal static IReadOnlyList<RootSignature> GetSourceRoots(BindingInspection inspection)
        {
            if (inspection?.Outcome != BindingInspectionOutcome.RepairableUpstream)
            {
                return Array.Empty<RootSignature>();
            }

            return inspection.RepairReason.FailedReads.Values.SelectMany(value => value)
                .Distinct().OrderBy(root => root).ToArray();
        }

        private static List<RecoveryUnit> RecoveryUnitsOf(
            IReadOnlyList<RootSignature> roots,
            IReadOnlyDictionary<SymbolId, int> rank)
        {
            var units = new Dictionary<string, RecoveryUnit>(StringComparer.OrdinalIgnoreCase);
            foreach (var root in roots)
            {
                var unit = root.Code == ExpressionDiagnosticCode.Cycle
                    ? RecoveryUnit.Cycle(root.Members, rank)
                    : RecoveryUnit.Owner(root.Owner, rank[root.Owner]);
                units[unit.Name] = unit;
            }

            return units.Values.OrderBy(unit => unit.SortRank).ThenBy(unit => unit.IsCycle ? 1 : 0).ToList();
        }

        private static List<RecoveryDiscoveryAbortCause> Discover(
            IReadOnlyList<ProjectVariableScanEntry> entries,
            DependencyGraph graph,
            IReadOnlyList<RecoveryUnit> units)
        {
            var envelopes = (entries ?? Array.Empty<ProjectVariableScanEntry>())
                .Where(entry => entry != null && !entry.OuterEnvelopeInterpretable)
                .Select(entry => entry.DefinitionId).Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (envelopes.Length > 0)
            {
                var allUnits = units.Select(unit => unit.Name).ToArray();
                return envelopes.Select(id => new RecoveryDiscoveryAbortCause(
                    RecoveryDiscoveryAbortKind.EnvelopeUnclassifiable, id, allUnits)).ToList();
            }

            var causes = new List<RecoveryDiscoveryAbortCause>();
            foreach (var unit in units)
            {
                var closure = unit.Members.Select(member => VariableId.Parse(member.Key))
                    .Concat(unit.Members.SelectMany(graph.TransitiveDependents).Select(id => VariableId.Parse(id.Key)))
                    .Distinct().ToArray();
                foreach (var group in ProjectVariableConsumerDiscovery.GroupSelectiveByRack(entries))
                {
                    var rack = group.Value.Select(view => view.RackId).Where(value => value != null)
                        .OrderBy(value => value, StringComparer.Ordinal).FirstOrDefault() ?? group.Key;
                    var probes = group.Value.Select(view => ProjectVariableConsumerProbe.Probe(view, closure)).ToArray();
                    if (probes.Any(probe => probe == ConsumerProbeOutcome.Indeterminate))
                    {
                        causes.Add(new RecoveryDiscoveryAbortCause(
                            RecoveryDiscoveryAbortKind.ProbeIndeterminate, rack, new[] { unit.Name }));
                        continue;
                    }
                    var positives = probes.Count(probe => probe == ConsumerProbeOutcome.Positive);
                    if (positives == 0)
                    {
                        continue;
                    }
                    if (positives != probes.Length)
                    {
                        causes.Add(new RecoveryDiscoveryAbortCause(
                            RecoveryDiscoveryAbortKind.PartialPositives, rack, new[] { unit.Name }));
                        continue;
                    }
                    if (!SelectiveAuthoredAuthority.Resolve(rack, group.Value).IsSingle)
                    {
                        causes.Add(new RecoveryDiscoveryAbortCause(
                            RecoveryDiscoveryAbortKind.NonSingleAuthority, rack, new[] { unit.Name }));
                    }
                }
            }

            return causes.GroupBy(cause => (cause.Kind, cause.Identity, Unit: string.Join("|", cause.RecoveryUnits)))
                .Select(group => group.First())
                .OrderBy(cause => cause.Kind).ThenBy(cause => cause.Identity, StringComparer.OrdinalIgnoreCase)
                .ThenBy(cause => cause.Identity, StringComparer.Ordinal).ToList();
        }

        private static bool FindBlockingSources(
            IReadOnlyList<ProjectVariableScanEntry> entries,
            string targetRack,
            string targetProperty,
            UsableProjectVariablesRegistry registry,
            RegistryEvaluation evaluation,
            IReadOnlyDictionary<SymbolId, int> recoveryRank,
            IReadOnlyList<RecoveryUnit> targetUnits,
            IReadOnlyList<RecoveryDiscoveryAbortCause> aborts,
            List<string> reasonData)
        {
            var targetNames = new HashSet<string>(targetUnits.Select(unit => unit.Name), StringComparer.OrdinalIgnoreCase);
            var relevantRacks = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { targetRack };
            foreach (var unit in targetUnits)
            {
                var closure = unit.Members.Select(member => VariableId.Parse(member.Key))
                    .Concat(unit.Members.SelectMany(evaluation.DependencyGraph.TransitiveDependents)
                        .Select(id => VariableId.Parse(id.Key))).Distinct().ToArray();
                var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, closure);
                if (discovery.IsSuccess)
                {
                    foreach (var consumer in discovery.Consumers)
                    {
                        relevantRacks.Add(consumer.RackId);
                    }
                }
            }

            var blocked = false;
            foreach (var group in ProjectVariableConsumerDiscovery.GroupSelectiveByRack(entries)
                .Where(group => relevantRacks.Contains(group.Key))
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                var authority = SelectiveAuthoredAuthority.Resolve(group.Key, group.Value);
                if (!authority.IsSingle)
                {
                    continue;
                }
                foreach (var inspection in Inspect(authority.Authored, registry, evaluation))
                {
                    if (string.Equals(group.Key, targetRack, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(inspection.PropertyToken, targetProperty, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    if (inspection.IsHealthy)
                    {
                        continue;
                    }

                    var blocker = inspection.Outcome != BindingInspectionOutcome.RepairableUpstream;
                    if (!blocker)
                    {
                        var sourceUnits = RecoveryUnitsOf(GetSourceRoots(inspection), recoveryRank);
                        blocker = sourceUnits.Any(unit => !targetNames.Contains(unit.Name));
                    }
                    if (blocker)
                    {
                        blocked = true;
                        reasonData.Add(group.Key + ":" + inspection.PropertyToken + ":" +
                            Classification(inspection.Outcome) + ":" + string.Join(",", DescribeReason(inspection)));
                    }
                }
            }
            return blocked;
        }

        private static IEnumerable<string> DescribeReason(BindingInspection inspection)
        {
            if (inspection.RepairReason == null)
            {
                return new[] { Classification(inspection.Outcome) + ":" + (inspection.Detail ?? string.Empty) };
            }
            if (inspection.RepairReason.Kind == "Upstream")
            {
                return inspection.RepairReason.FailedReads.SelectMany(read => read.Value.Select(root =>
                    "Upstream:" + read.Key.Key + ":" + FormatRoot(root)));
            }
            return inspection.RepairReason.StableData.Count == 0
                ? new[] { inspection.RepairReason.Kind }
                : inspection.RepairReason.StableData.Select(data => inspection.RepairReason.Kind + ":" + data);
        }

        private static string Classification(BindingInspectionOutcome outcome)
        {
            switch (outcome)
            {
                case BindingInspectionOutcome.Healthy: return "Healthy";
                case BindingInspectionOutcome.RepairableMissingTarget: return "MissingTarget";
                case BindingInspectionOutcome.RepairableIntrinsic: return "Intrinsic";
                case BindingInspectionOutcome.RepairableUpstream: return "Upstream";
                case BindingInspectionOutcome.RepairableDomain: return "Domain";
                default: return outcome.ToString();
            }
        }

        private static string FormatRoot(RootSignature root)
        {
            if (root.Code == ExpressionDiagnosticCode.Cycle)
            {
                return "CycleRoot[" + string.Join(",", root.Members.Select(member => member.Key)) + "]";
            }
            var data = root.Code == ExpressionDiagnosticCode.BrokenReference
                ? string.Join(",", root.MissingSymbols.Select(id => id.Key))
                : root.Code == ExpressionDiagnosticCode.InvalidArguments
                    ? string.Join(",", root.InvalidArguments.Select(item => item.Token + ":" + item.ArgumentCount))
                    : string.Empty;
            return root.Owner.Key + ":" + root.Code + (data.Length == 0 ? string.Empty : "(" + data + ")");
        }

        private sealed class RecoveryUnit
        {
            private RecoveryUnit(string name, int sortRank, IReadOnlyList<SymbolId> members, bool isCycle)
            {
                Name = name;
                SortRank = sortRank;
                Members = members;
                IsCycle = isCycle;
            }
            internal string Name { get; }
            internal int SortRank { get; }
            internal IReadOnlyList<SymbolId> Members { get; }
            internal bool IsCycle { get; }
            internal static RecoveryUnit Owner(SymbolId owner, int rank) => new RecoveryUnit(owner.Key, rank, new[] { owner }, false);
            internal static RecoveryUnit Cycle(IReadOnlyList<SymbolId> members, IReadOnlyDictionary<SymbolId, int> rank)
            {
                var ordered = members.OrderBy(member => rank[member]).ToArray();
                return new RecoveryUnit("CycleRoot[" + string.Join(",", ordered.Select(member => member.Key)) + "]",
                    rank[ordered[0]], ordered, true);
            }
        }
    }
}
