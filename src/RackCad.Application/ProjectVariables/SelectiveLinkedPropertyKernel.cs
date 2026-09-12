using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>Whether a rack can be repaired, over its COMPLETE set of bindings.</summary>
    internal enum RackRepairability
    {
        /// <summary>No missing target and no fatal state. Nothing to repair.</summary>
        Healthy = 1,

        /// <summary>At least one missing target and NO fatal state. Only here does an actionable set exist.</summary>
        Repairable = 2,

        /// <summary>At least one FATAL state. The whole rack is unrepairable, missing targets included.</summary>
        Blocked = 3,
    }

    /// <summary>
    /// The rack-level verdict, computed over the WHOLE scan before anything is offered (I-48 G4B, V7-R03).
    ///
    /// <para>
    /// It exists because repairability is a property of the RACK, not of a row. A rack holding one missing
    /// target and one malformed reference cannot be repaired at all — the executor needs a complete effective
    /// design and there is none — so presenting the missing one as an actionable repair would offer a remedy
    /// that structurally cannot succeed. The missing target may still be SHOWN, as a diagnostic.
    /// </para>
    /// <para>
    /// <see cref="Repairable"/> is the only state in which <see cref="Missing"/> is an actionable set. On
    /// <see cref="RackRepairability.Blocked"/> the same rows are diagnostics and nothing more.
    /// </para>
    /// </summary>
    internal sealed class RackRepairabilityAssessment
    {
        private RackRepairabilityAssessment(
            RackRepairability outcome,
            IReadOnlyList<BindingInspection> inspections,
            IReadOnlyList<BindingInspection> missing,
            IReadOnlyList<BindingInspection> fatal)
        {
            Outcome = outcome;
            Inspections = inspections;
            Missing = missing;
            Fatal = fatal;
        }

        internal RackRepairability Outcome { get; }

        /// <summary>Every inspection of the rack, in deterministic order. Nothing is dropped from the scan.</summary>
        internal IReadOnlyList<BindingInspection> Inspections { get; }

        /// <summary>
        /// The missing targets. They are DIAGNOSTICS unless <see cref="CanRepair"/> is true, in which case they
        /// are exactly the set B that a repair must act on — all of them, never a subset.
        /// </summary>
        internal IReadOnlyList<BindingInspection> Missing { get; }

        /// <summary>The fatal states, so the reason a rack is blocked stays visible.</summary>
        internal IReadOnlyList<BindingInspection> Fatal { get; }

        /// <summary>
        /// ANY fatal state makes this false for the ENTIRE rack, even when missing targets are also present.
        /// </summary>
        internal bool CanRepair => Outcome == RackRepairability.Repairable;

        internal bool IsBlocked => Outcome == RackRepairability.Blocked;

        /// <summary>The first fatal reason, for a caller that has to explain the block in one line.</summary>
        internal string BlockingReason => Fatal.Count > 0 ? Fatal[0].Detail : null;

        internal static RackRepairabilityAssessment Of(IReadOnlyList<BindingInspection> inspections)
        {
            var missing = new List<BindingInspection>();
            var fatal = new List<BindingInspection>();

            foreach (var inspection in inspections)
            {
                if (inspection.IsFatal)
                {
                    fatal.Add(inspection);
                }
                else if (inspection.IsRepairable)
                {
                    missing.Add(inspection);
                }
            }

            var outcome = fatal.Count > 0
                ? RackRepairability.Blocked
                : missing.Count > 0
                    ? RackRepairability.Repairable
                    : RackRepairability.Healthy;

            return new RackRepairabilityAssessment(outcome, inspections, missing, fatal);
        }
    }

    /// <summary>
    /// The generic kernel over a rack's bindings (I-48 G4B).
    ///
    /// <para>
    /// <see cref="InspectBindings"/> is a SCANNER and nothing more: it walks the persisted map, calls the one
    /// primitive per entry and keeps every result in a deterministic order. It does not resolve a descriptor,
    /// validate a reference kind, parse a <see cref="VariableId"/>, look a target up or compare types — those
    /// five rules stay in <see cref="LinkedPropertyInspection.InspectBinding"/>, because splitting them across
    /// a caller is how a second notion of the same rule appears.
    /// </para>
    /// <para>
    /// An unknown or malformed binding NEVER disappears from the scan. Dropping it would turn a state this
    /// build cannot interpret into the absence of a binding, which is the exact collapse the whole initiative
    /// exists to prevent.
    /// </para>
    /// </summary>
    internal static class SelectiveLinkedPropertyKernel
    {
        /// <summary>
        /// Inspects every binding the authored document carries, ordered Ordinal by the persisted key so the
        /// result never depends on <see cref="Dictionary{TKey,TValue}"/> enumeration order.
        /// </summary>
        internal static IReadOnlyList<BindingInspection> InspectBindings(
            SelectivePalletDesignDocument authored,
            LinkedPropertyDescriptorSet descriptors,
            UsableProjectVariablesRegistry registry)
        {
            if (authored == null)
            {
                throw new ArgumentNullException(nameof(authored));
            }

            var inspections = new List<BindingInspection>();

            if (authored.PropertyValues == null)
            {
                return inspections;
            }

            var tokens = new List<string>(authored.PropertyValues.Keys);
            tokens.Sort(static (left, right) => string.Compare(left, right, StringComparison.Ordinal));

            foreach (var token in tokens)
            {
                inspections.Add(
                    LinkedPropertyInspection.InspectBinding(
                        token, authored.PropertyValues[token], descriptors, registry));
            }

            return inspections;
        }

        /// <summary>Scan plus rack-level verdict, which is how every consumer should ask the question.</summary>
        internal static RackRepairabilityAssessment Assess(
            SelectivePalletDesignDocument authored,
            LinkedPropertyDescriptorSet descriptors,
            UsableProjectVariablesRegistry registry)
            => RackRepairabilityAssessment.Of(InspectBindings(authored, descriptors, registry));

        /// <summary>
        /// The properties of this rack whose binding points at <paramref name="target"/>, ordered Ordinal.
        ///
        /// <para>
        /// This is the set the contract calls <c>P</c>, and it is derived from ONE authored document — the
        /// single authority a caller has already proven. Deriving it from an arbitrary sibling before that
        /// proof would make it depend on which view was read first.
        /// </para>
        /// <para>
        /// A variable may govern N properties of the same rack: nothing in the model forbids it, so nothing
        /// here assumes otherwise.
        /// </para>
        /// </summary>
        internal static IReadOnlyList<PropertyId> PropertiesBoundTo(
            SelectivePalletDesignDocument authored, VariableId target)
        {
            var properties = new List<PropertyId>();

            if (authored?.PropertyValues == null)
            {
                return properties;
            }

            var tokens = new List<string>(authored.PropertyValues.Keys);
            tokens.Sort(static (left, right) => string.Compare(left, right, StringComparison.Ordinal));

            foreach (var token in tokens)
            {
                var reference = authored.PropertyValues[token];

                if (reference == null ||
                    !string.Equals(
                        reference.Kind, SelectivePropertyValueDocument.ProjectVariableKind, StringComparison.Ordinal) ||
                    !VariableId.TryParse(reference.VariableId, out var id) ||
                    !id.Equals(target) ||
                    !PropertyId.TryParse(token, out var propertyId))
                {
                    continue;
                }

                properties.Add(propertyId);
            }

            return properties;
        }
    }
}
