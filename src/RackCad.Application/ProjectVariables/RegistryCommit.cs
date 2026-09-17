using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>What the commit re-read decided about a planned register change.</summary>
    public enum RegistryCommitOutcome
    {
        /// <summary>The plan changes no variable. Nothing is read, accredited or written.</summary>
        Unchanged = 1,

        /// <summary>The re-read was accredited and the changed document is ready to write.</summary>
        Ready = 2,

        /// <summary>The re-read is not a usable authority. Nothing is applied and nothing is written.</summary>
        Blocked = 3,
    }

    /// <summary>
    /// The document a commit may write, or the visible reason there is none.
    ///
    /// <para>
    /// <see cref="Changed"/> is non-null ONLY for <see cref="RegistryCommitOutcome.Ready"/>. On every other
    /// outcome there is no product at all, which is also the evidence that the change was never applied: a
    /// changed document can only come out of <see cref="RegistryMutation.ApplyTo"/>.
    /// </para>
    /// </summary>
    public sealed class RegistryCommitPreparation
    {
        private RegistryCommitPreparation(
            RegistryCommitOutcome outcome, ProjectVariablesDocument changed, string error,
            int registryWrites = 0, int rackWrites = 0, int semanticReads = 0)
        {
            Outcome = outcome;
            Changed = changed;
            Error = error;
            RegistryWrites = registryWrites;
            RackWrites = rackWrites;
            SemanticReads = semanticReads;
        }

        public RegistryCommitOutcome Outcome { get; }

        /// <summary>The document to write. Null unless <see cref="IsReady"/>.</summary>
        public ProjectVariablesDocument Changed { get; }

        /// <summary>The visible reason nothing may be written. Null unless <see cref="IsBlocked"/>.</summary>
        public string Error { get; }

        public bool IsReady => Outcome == RegistryCommitOutcome.Ready;

        public bool IsBlocked => Outcome == RegistryCommitOutcome.Blocked;

        public bool Succeeded => !IsBlocked;
        public int RegistryWrites { get; }
        public int RackWrites { get; }
        public int SemanticReads { get; }

        internal static RegistryCommitPreparation Unchanged()
            => new RegistryCommitPreparation(RegistryCommitOutcome.Unchanged, null, null);

        internal static RegistryCommitPreparation Ready(ProjectVariablesDocument changed)
            => new RegistryCommitPreparation(RegistryCommitOutcome.Ready, changed, null, 1);

        internal static RegistryCommitPreparation Completed(
            RegistryCommitOutcome outcome, ProjectVariablesDocument changed, int registryWrites, int rackWrites, int semanticReads)
            => new RegistryCommitPreparation(outcome, changed, null, registryWrites, rackWrites, semanticReads);

        internal static RegistryCommitPreparation Blocked(string error)
            => new RegistryCommitPreparation(RegistryCommitOutcome.Blocked, null, error);
    }

    /// <summary>
    /// The ONE seam through which a register change reaches the drawing (I-48 G4B, Proposal V8 R-01).
    ///
    /// <para>
    /// A plan is decided against the register that was read when the window opened; a commit writes against the
    /// register that is there NOW. Those are two different documents, so the re-read is a genuinely new one and
    /// needs its own accreditation — an accreditation belongs to the read it was given. Before this seam existed
    /// the executor re-read and applied the change straight to the raw result, which meant the whole identity
    /// precondition was enforced only on the planning path: a register that grew a duplicated
    /// <see cref="VariableId"/> in between was mutated anyway, and a Remove over a duplicated id deletes BOTH
    /// entries.
    /// </para>
    /// <para>
    /// The order is <b>read, accredit that read, validate repair decisions and Before observations, apply purely,
    /// validate After observations, write</b>. It is enforced by construction and not by comment:
    /// <see cref="RegistryMutation.ApplyTo"/> is internal, so the Plugin cannot call it, and the document it is
    /// applied to here is the one the accreditation vouches for.
    /// </para>
    /// <para>
    /// A plan with <see cref="RegistryMutationKind.None"/> returns <see cref="RegistryCommitOutcome.Unchanged"/>
    /// without touching the read. A rack-only operation must not acquire a new failure mode — and must not force
    /// a re-read it has no use for.
    /// </para>
    /// </summary>
    public static class RegistryCommit
    {
        /// <summary>
        /// Accredits the commit re-read and produces the document to write, or blocks with the reason.
        /// </summary>
        /// <param name="mutation">The planned register change. Null or <c>None</c> means there is nothing to do.</param>
        /// <param name="lastRead">The register as it is NOW, read inside the write transaction.</param>
        public static RegistryCommitPreparation Prepare(
            RegistryMutation mutation, ProjectVariablesReadResult lastRead)
        {
            if (mutation == null || mutation.Kind == RegistryMutationKind.None)
            {
                return RegistryCommitPreparation.Unchanged();
            }

            var accreditation = UsableProjectVariablesRegistry.Accredit(lastRead);

            if (!accreditation.IsUsable)
            {
                // Fail-closed BEFORE applying anything. There is no changed document to hand back, so a caller
                // cannot write "most of" the operation: the transaction unwinds with the drawing untouched.
                return RegistryCommitPreparation.Blocked(accreditation.Error);
            }

            return RegistryCommitPreparation.Ready(mutation.ApplyTo(accreditation.Document));
        }

        /// <summary>Reaccredits every semantic observation before exposing the first permitted write.</summary>
        public static RegistryCommitPreparation Prepare(
            MutationPlan plan,
            ProjectVariablesDocument currentRegistry,
            IReadOnlyList<ProjectVariableScanEntry> currentRacks)
            => Prepare(
                plan,
                currentRegistry == null ? ProjectVariablesReadResult.Absent() : ProjectVariablesReadResult.Readable(currentRegistry),
                currentRacks);

        /// <summary>Read-result form used by the physical executor inside its transaction.</summary>
        public static RegistryCommitPreparation Prepare(
            MutationPlan plan,
            ProjectVariablesReadResult lastRead,
            IReadOnlyList<ProjectVariableScanEntry> currentRacks)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            var needsRegistry = plan.RegistryMutation.Kind != RegistryMutationKind.None || !plan.PlanReadSet.IsEmpty;
            if (!needsRegistry)
            {
                return RegistryCommitPreparation.Completed(
                    RegistryCommitOutcome.Unchanged, null, 0, plan.RackMutations.Count, 0);
            }

            var accreditation = UsableProjectVariablesRegistry.Accredit(lastRead);
            if (!accreditation.IsUsable)
            {
                return RegistryCommitPreparation.Blocked(accreditation.Error);
            }

            if (plan.RegistryMutation.Kind == RegistryMutationKind.ChangeValue &&
                plan.RegistryMutation.ExpectedDefinition != null &&
                (!accreditation.Registry.TryGetTarget(plan.RegistryMutation.VariableId, out var currentTarget) ||
                 !plan.RegistryMutation.ExpectedDefinition.Equals(currentTarget.Definition)))
            {
                return RegistryCommitPreparation.Blocked(
                    "La definicion objetivo cambio despues de crear el plan.");
            }

            var repairObservations = plan.PlanReadSet.RepairDecisionObservations;
            var beforeObservations = plan.PlanReadSet.SymbolResultObservations
                .Where(observation => observation.Phase == SymbolObservationPhase.Before).ToArray();
            var afterObservations = plan.PlanReadSet.SymbolResultObservations
                .Where(observation => observation.Phase == SymbolObservationPhase.After).ToArray();
            RegistryEvaluation before = null;
            if (repairObservations.Count > 0 || beforeObservations.Length > 0)
            {
                before = RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(accreditation.Registry));
            }

            foreach (var observation in repairObservations)
            {
                var target = ProjectVariableConsumerDiscovery.ResolveTargetRack(currentRacks, observation.RackId);
                if (!target.IsSuccess || target.Consumers.Count != 1 ||
                    target.Consumers[0].Authored.PropertyValues == null ||
                    !target.Consumers[0].Authored.PropertyValues.TryGetValue(observation.PropertyId.Value, out var source))
                {
                    return RegistryCommitPreparation.Blocked("El origen reparado cambio antes del commit.");
                }
                var inspection = LinkedPropertyInspection.InspectBinding(
                    observation.PropertyId.Value, source, SelectiveLinkedProperties.All, accreditation.Registry, before);
                if (inspection.RepairReason == null || !observation.Matches(inspection.RepairReason))
                {
                    return RegistryCommitPreparation.Blocked("La razon de reparacion cambio antes del commit.");
                }
            }

            foreach (var observation in beforeObservations)
            {
                if (!before.Results.TryGetValue(observation.Symbol, out var actual) || !observation.Matches(actual))
                {
                    return RegistryCommitPreparation.Blocked(
                        "El estado semantico observado cambio antes del commit: " + observation.SymbolId + ".");
                }
            }

            ProjectVariablesDocument changed = accreditation.Document;
            UsableProjectVariablesRegistry afterRegistry = accreditation.Registry;
            if (plan.RegistryMutation.Kind != RegistryMutationKind.None)
            {
                changed = plan.RegistryMutation.ApplyTo(accreditation.Document);
                var afterAccreditation = UsableProjectVariablesRegistry.Accredit(ProjectVariablesReadResult.Readable(changed));
                if (!afterAccreditation.IsUsable)
                {
                    return RegistryCommitPreparation.Blocked(afterAccreditation.Error);
                }
                afterRegistry = afterAccreditation.Registry;
            }

            if (afterObservations.Length > 0)
            {
                var after = plan.RegistryMutation.Kind == RegistryMutationKind.None && before != null
                    ? before
                    : RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(afterRegistry));
                foreach (var observation in afterObservations)
                {
                    if (!after.Results.TryGetValue(observation.Symbol, out var actual) || !observation.Matches(actual))
                    {
                        return RegistryCommitPreparation.Blocked(
                            "El estado semantico observado cambio antes del commit: " + observation.SymbolId + ".");
                    }
                }
            }

            var registryWrites = plan.RegistryMutation.Kind == RegistryMutationKind.None ? 0 : 1;
            var outcome = registryWrites == 0 ? RegistryCommitOutcome.Unchanged : RegistryCommitOutcome.Ready;
            return RegistryCommitPreparation.Completed(
                outcome, registryWrites == 0 ? null : changed,
                registryWrites, plan.RackMutations.Count, plan.PlanReadSet.IsEmpty ? 0 : 1);
        }
    }
}
