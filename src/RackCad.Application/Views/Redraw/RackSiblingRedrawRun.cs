using System;
using System.Collections.Generic;

namespace RackCad.Application.Views.Redraw
{
    public enum RackSiblingRedrawOutcome
    {
        PrepareFailed,
        RedrawNotRequired,
        Committed,
        Discarded
    }

    public sealed class RackSiblingRedrawResult<TUnit>
    {
        internal RackSiblingRedrawResult(RackSiblingRedrawOutcome outcome, RackSiblingRedrawPlan<TUnit> plan, string diagnostic)
        {
            Outcome = outcome;
            Plan = plan;
            Diagnostic = diagnostic;
        }

        public RackSiblingRedrawOutcome Outcome { get; }
        public RackSiblingRedrawPlan<TUnit> Plan { get; }
        public string Diagnostic { get; }
    }

    public static class RackSiblingRedrawRun
    {
        public static RackSiblingRedrawResult<TUnit> Execute<TUnit>(
            RackSiblingMembershipSnapshot membership,
            ISiblingRedrawPort<TUnit> port)
        {
            if (membership == null) throw new ArgumentNullException(nameof(membership));
            if (port == null) throw new ArgumentNullException(nameof(port));

            foreach (var member in membership.Members)
            {
                if (member.Kind == RackSiblingMembershipKind.BlockingUnreadable)
                {
                    return new RackSiblingRedrawResult<TUnit>(
                        RackSiblingRedrawOutcome.PrepareFailed,
                        null,
                        "SIBLING_GATE_FAILED: " + member.Fact.DefinitionKey);
                }
            }

            var prepared = new List<RackSiblingPreparedUnit<TUnit>>();
            foreach (var member in membership.Members)
            {
                if (member.Kind != RackSiblingMembershipKind.Redraw && member.Kind != RackSiblingMembershipKind.Erase)
                {
                    continue;
                }

                RackSiblingUnitPreparation<TUnit> preparation;
                try
                {
                    preparation = port.Prepare(member);
                }
                catch (Exception ex)
                {
                    return PrepareFailed<TUnit>(member, ex.Message);
                }

                if (preparation == null || !preparation.IsSuccess)
                {
                    return PrepareFailed<TUnit>(member, preparation?.Diagnostic ?? "PREPARE_RETURNED_NULL");
                }

                foreach (var layer in preparation.Layers)
                {
                    if (layer != null && layer.IsLocked)
                    {
                        return PrepareFailed<TUnit>(member, "LOCKED_LAYER: " + layer.Layer);
                    }
                }

                prepared.Add(new RackSiblingPreparedUnit<TUnit>(member, preparation.Unit));
            }

            var plan = RackSiblingRedrawPlan<TUnit>.Create(membership, prepared);
            if (plan.Disposition != RackSiblingRedrawDisposition.MutateNow)
            {
                return new RackSiblingRedrawResult<TUnit>(RackSiblingRedrawOutcome.RedrawNotRequired, plan, plan.Disposition.ToString());
            }

            var units = new List<TUnit>(plan.RedrawUnits.Count + plan.EraseUnits.Count);
            foreach (var item in plan.RedrawUnits) units.Add(item.Unit);
            foreach (var item in plan.EraseUnits) units.Add(item.Unit);

            RackSiblingMutationResult mutation;
            try
            {
                mutation = port.Mutate(units);
            }
            catch (Exception ex)
            {
                mutation = RackSiblingMutationResult.Discarded(null, ex.Message);
            }

            if (mutation == null || mutation.Kind == RackSiblingMutationKind.Discarded)
            {
                return new RackSiblingRedrawResult<TUnit>(
                    RackSiblingRedrawOutcome.Discarded,
                    plan,
                    mutation?.Diagnostic ?? "MUTATE_RETURNED_NULL");
            }

            try
            {
                port.Post(plan, mutation);
                return new RackSiblingRedrawResult<TUnit>(RackSiblingRedrawOutcome.Committed, plan, null);
            }
            catch (Exception ex)
            {
                return new RackSiblingRedrawResult<TUnit>(RackSiblingRedrawOutcome.Committed, plan, "POST_FAILED: " + ex.Message);
            }
        }

        private static RackSiblingRedrawResult<TUnit> PrepareFailed<TUnit>(RackSiblingMember member, string diagnostic)
            => new RackSiblingRedrawResult<TUnit>(
                RackSiblingRedrawOutcome.PrepareFailed,
                null,
                member.Fact.View + " [" + member.Fact.DefinitionKey + "]: " + diagnostic);
    }
}
