using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// The frozen ID19 sequence. Each stage either produces facts for the next one or fails closed with every
    /// affected member listed; nothing here asks for a point, writes a drawing or invents an identity.
    /// </summary>
    internal static class RackProjectionPipeline
    {
        internal static RackGroupPlacementPlanResult Run(RackProjectionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Selection == null) throw new ArgumentException("A projection needs selection facts.", nameof(request));
            if (request.Services == null) throw new ArgumentException("A projection needs its authorities.", nameof(request));

            var warnings = new List<RackProjectionWarning>();
            var members = request.Selection.Members
                .OrderBy(member => member.PhysicalKey, StringComparer.Ordinal)
                .ToList();

            // CLASSIFY
            var accepted = new Dictionary<string, RackSourcePlacementAcceptance>(StringComparer.Ordinal);
            var classify = new List<RackProjectionDiagnostic>();
            foreach (var member in members)
            {
                if (!member.IsSelected)
                {
                    classify.Add(Diagnostic(
                        RackProjectionStage.Classify, RackProjectionFailureCode.UnsupportedMember, member));
                    continue;
                }

                if (member.Identity == RackPhysicalIdentityFact.Missing || string.IsNullOrWhiteSpace(member.RackId))
                {
                    classify.Add(Diagnostic(
                        RackProjectionStage.Classify, RackProjectionFailureCode.BlankRackId, member));
                    continue;
                }

                if (!member.HasAddress)
                {
                    classify.Add(Diagnostic(
                        RackProjectionStage.Classify, RackProjectionFailureCode.UnsupportedMember, member));
                    continue;
                }

                if (request.SourceFacts == null
                    || !request.SourceFacts.TryGetValue(member.PhysicalKey, out var facts))
                {
                    classify.Add(Diagnostic(
                        RackProjectionStage.Classify, RackProjectionFailureCode.UnsupportedMember, member));
                    continue;
                }

                var acceptance = RackProjectionSourceTransform.Accept(facts);
                if (!acceptance.IsAccepted)
                {
                    classify.Add(Diagnostic(RackProjectionStage.Classify, acceptance.Failure, member));
                    continue;
                }

                accepted[member.PhysicalKey] = acceptance;
            }

            if (classify.Count > 0)
            {
                return Failure(RackProjectionStage.Classify, classify, warnings);
            }

            // GROUP
            var groups = request.Selection.RackGroups
                .OrderBy(group => group.RackId, StringComparer.Ordinal)
                .ToList();
            var groupDiagnostics = new List<RackProjectionDiagnostic>();
            foreach (var group in groups)
            {
                var ordered = Ordered(group);
                if (ordered.Select(member => member.Kind).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1
                    || ordered.Select(member => member.Address.Kind).Distinct().Count() > 1)
                {
                    groupDiagnostics.AddRange(ordered.Select(member => Diagnostic(
                        RackProjectionStage.Group, RackProjectionFailureCode.MixedSourceTypes, member)));
                    continue;
                }

                if (ordered.Select(member => member.DefinitionKey).Distinct(StringComparer.Ordinal).Count() > 1)
                {
                    groupDiagnostics.AddRange(ordered.Select(member => Diagnostic(
                        RackProjectionStage.Group, RackProjectionFailureCode.MultipleSourceDefinitions, member)));
                }
            }

            var sourceKinds = members
                .Where(member => member.IsSelected && member.HasAddress)
                .Select(member => member.Address.Kind)
                .Distinct()
                .ToList();
            if (sourceKinds.Count > 1)
            {
                groupDiagnostics.AddRange(members.Select(member => Diagnostic(
                    RackProjectionStage.Group, RackProjectionFailureCode.MixedSourceTypes, member)));
            }

            if (groupDiagnostics.Count > 0)
            {
                return Failure(RackProjectionStage.Group, groupDiagnostics, warnings);
            }

            // AUTHORITY GATES
            var gates = new List<RackProjectionDiagnostic>();
            foreach (var group in groups)
            {
                var representative = Ordered(group).First();
                var authored = RackSiblingAuthoredGate.From(Map(request.Services.Authored(group.RackId)));
                if (!authored.Proceeds)
                {
                    gates.Add(Diagnostic(
                        RackProjectionStage.AuthorityGates, authored.Failure, representative,
                        RackSystemKind.SelectiveRack));
                }

                var properties = request.Services.Properties(group.RackId);
                if (properties != RackProjectionPropertiesState.Equivalent)
                {
                    gates.Add(Diagnostic(
                        RackProjectionStage.AuthorityGates,
                        properties == RackProjectionPropertiesState.Divergent
                            ? RackProjectionFailureCode.PropertiesDivergent
                            : RackProjectionFailureCode.PropertiesUnreadable,
                        representative,
                        RackSystemKind.SelectiveRack));
                }
            }

            if (gates.Count > 0)
            {
                return Failure(RackProjectionStage.AuthorityGates, gates, warnings);
            }

            // REPRESENTATIVE and RESOLVE, once per RackId
            var resolved = new Dictionary<string, RackProjectionResolveOutcome>(StringComparer.Ordinal);
            var resolveDiagnostics = new List<RackProjectionDiagnostic>();
            foreach (var group in groups)
            {
                var representative = Ordered(group).First();
                var outcome = request.Services.Resolve(group.RackId);
                resolved[group.RackId] = outcome;

                if (outcome.State == RackProjectionResolveState.ResolvedWithOutputBlocking)
                {
                    resolveDiagnostics.Add(Diagnostic(
                        RackProjectionStage.Resolve,
                        RackProjectionFailureCode.ResolveOutputBlocking,
                        representative,
                        outcome.Availability?.SystemKind ?? RackSystemKind.SelectiveRack,
                        outcome.Code));
                }
                else if (outcome.State == RackProjectionResolveState.Failed)
                {
                    resolveDiagnostics.Add(Diagnostic(
                        RackProjectionStage.Resolve,
                        MapResolveFailure(outcome.Failure),
                        representative,
                        outcome.Availability?.SystemKind ?? RackSystemKind.SelectiveRack,
                        outcome.Code));
                }
            }

            if (resolveDiagnostics.Count > 0)
            {
                return Failure(RackProjectionStage.Resolve, resolveDiagnostics, warnings);
            }

            // EDIT PREFLIGHT
            var preflightDiagnostics = new List<RackProjectionDiagnostic>();
            foreach (var group in groups)
            {
                var preflight = request.Services.EditPreflight(group.RackId);
                if (preflight == null || !preflight.Rejected)
                {
                    continue;
                }

                var representative = Ordered(group).First();
                var systemKind = resolved[group.RackId].Availability?.SystemKind ?? RackSystemKind.SelectiveRack;
                preflightDiagnostics.Add(new RackProjectionDiagnostic(
                    RackProjectionStage.EditPreflight,
                    RackProjectionFailureCode.EditPreflightRejected,
                    group.RackId,
                    representative.PhysicalKey,
                    representative.Address,
                    RackProjectionRemedySelector.For(
                        RackProjectionFailureCode.EditPreflightRejected,
                        systemKind,
                        hasAnotherValidSiblingView: group.Members.Count > 1,
                        isXrefDependent: preflight.XrefDependent,
                        detail: preflight.Code)));
            }

            if (preflightDiagnostics.Count > 0)
            {
                return Failure(RackProjectionStage.EditPreflight, preflightDiagnostics, warnings);
            }

            // AVAILABLE
            var availability = new List<RackProjectionDiagnostic>();
            var targets = new Dictionary<string, RackViewAddress>(StringComparer.Ordinal);
            var mode = RackProjectionMode.Rigid;
            var conservedAxis = RackPhysicalAxis.Run;
            foreach (var group in groups)
            {
                var representative = Ordered(group).First();
                var outcome = resolved[group.RackId];
                var systemKind = outcome.Availability?.SystemKind ?? RackSystemKind.SelectiveRack;

                if (!RackProjectionClassMapping.TryMap(
                        representative.Address.Kind, request.TargetKind, out mode, out conservedAxis))
                {
                    availability.Add(Diagnostic(
                        RackProjectionStage.Available,
                        RackProjectionFailureCode.PairNotExposed,
                        representative,
                        systemKind));
                    continue;
                }

                var sourceDecision = ProjectionAvailabilityPolicy.EvaluateNewView(
                    systemKind,
                    representative.Address,
                    outcome.Availability,
                    RackViewProductOperation.GroupProjection);
                if (sourceDecision.Decision == ProjectionPolicyDecisionKind.Reject)
                {
                    availability.Add(Diagnostic(
                        RackProjectionStage.Available,
                        sourceDecision.ReasonCode == ProjectionPolicyReasonCode.NotExposedForOperation
                            ? RackProjectionFailureCode.TargetNotExposed
                            : RackProjectionFailureCode.SourceAddressUnavailable,
                        representative,
                        systemKind));
                    continue;
                }

                var target = mode == RackProjectionMode.Rigid
                    ? Candidate(representative.Address, systemKind, outcome)
                    : Canonical(request.TargetKind, systemKind, outcome);

                if (target == null)
                {
                    availability.Add(Diagnostic(
                        RackProjectionStage.Available,
                        RackProjectionFailureCode.TargetAddressUnavailable,
                        representative,
                        systemKind));
                    continue;
                }

                targets[group.RackId] = target.Value;
            }

            if (availability.Count > 0)
            {
                return Failure(RackProjectionStage.Available, availability, warnings);
            }

            // FRAMES
            var frameDiagnostics = new List<RackProjectionDiagnostic>();
            var views = new List<RackProjectionSourceView>();
            foreach (var group in groups)
            {
                var target = targets[group.RackId];
                foreach (var member in Ordered(group))
                {
                    var sourceFrame = request.Services.Frame(group.RackId, member.Address);
                    var targetFrame = request.Services.Frame(group.RackId, target);
                    if (!sourceFrame.IsAvailable || !targetFrame.IsAvailable)
                    {
                        frameDiagnostics.Add(Diagnostic(
                            RackProjectionStage.Frames, RackProjectionFailureCode.FrameUnavailable, member));
                        continue;
                    }

                    views.Add(new RackProjectionSourceView(
                        member.PhysicalKey,
                        group.RackId,
                        member.Address,
                        sourceFrame.Frame,
                        target,
                        targetFrame.Frame,
                        accepted[member.PhysicalKey]));
                }
            }

            if (frameDiagnostics.Count > 0)
            {
                return Failure(RackProjectionStage.Frames, frameDiagnostics, warnings);
            }

            // VALIDATE
            var families = groups
                .Select(group => RackProjectionClassMapping.FamilyOf(
                    resolved[group.RackId].Availability?.SystemKind ?? RackSystemKind.SelectiveRack))
                .Distinct()
                .ToList();
            if (families.Count > 1)
            {
                return Failure(
                    RackProjectionStage.Validate,
                    views.Select(view => Diagnostic(
                        RackProjectionStage.Validate, RackProjectionFailureCode.MixedFamilies, view)).ToList(),
                    warnings);
            }

            RackOrthographicProjection orthographic = null;
            if (mode == RackProjectionMode.Orthographic)
            {
                var projection = RackOrthographicPlacementPolicy.Project(
                    views,
                    conservedAxis,
                    families[0],
                    SourceGroupFrame.Universal(new Point3D(0, 0, 0)),
                    TargetGroupFrame.Universal(new Point3D(0, 0, 0)));

                if (!projection.IsAvailable)
                {
                    return Failure(
                        RackProjectionStage.Validate,
                        views.Select(view => Diagnostic(
                            RackProjectionStage.Validate, projection.Failure, view)).ToList(),
                        warnings);
                }

                orthographic = projection.Projection;
                warnings.AddRange(Overlaps(orthographic));
                if (orthographic.NearWindowLimit)
                {
                    warnings.Add(new RackProjectionWarning(
                        RackProjectionWarningCode.DirectionWindowNearLimit,
                        null,
                        null,
                        "La corrida cae junto al limite de la ventana relativa."));
                }
            }

            // PLANS
            var planDiagnostics = new List<RackProjectionDiagnostic>();
            var planGroups = new List<RackProjectionGroup>();
            foreach (var group in groups)
            {
                var target = targets[group.RackId];
                var representative = Ordered(group).First();
                var systemKind = resolved[group.RackId].Availability?.SystemKind ?? RackSystemKind.SelectiveRack;
                var preparation = request.Services.Prepare(group.RackId, target);

                if (preparation == null || !preparation.PlanAvailable)
                {
                    planDiagnostics.Add(Diagnostic(
                        RackProjectionStage.Plans, RackProjectionFailureCode.PlanUnavailable, representative, systemKind));
                    continue;
                }

                if (preparation.RequirementOutcome != PieceRequirementExtractionOutcome.Extracted)
                {
                    planDiagnostics.Add(Diagnostic(
                        RackProjectionStage.Plans, RackProjectionFailureCode.UnknownSourceRole, representative, systemKind));
                    continue;
                }

                foreach (var fact in preparation.PieceFacts)
                {
                    Evaluate(fact, group.RackId, representative.PhysicalKey, systemKind, planDiagnostics, warnings);
                }

                planGroups.Add(new RackProjectionGroup(
                    group.RackId,
                    systemKind,
                    Ordered(group).Select(member => member.PhysicalKey).ToList(),
                    representative.Address,
                    target,
                    preparation));
            }

            if (planDiagnostics.Count > 0)
            {
                return Failure(RackProjectionStage.Plans, planDiagnostics, warnings);
            }

            var plan = new RackGroupPlacementPlan(
                request.TargetKind,
                mode,
                conservedAxis,
                planGroups,
                views,
                orthographic,
                warnings);

            return new RackGroupPlacementPlanResult(plan, RackProjectionStage.Plans, Array.Empty<RackProjectionDiagnostic>(), warnings);
        }

        private static void Evaluate(
            LibraryPieceAvailabilityFact fact,
            string rackId,
            string physicalKey,
            RackSystemKind systemKind,
            List<RackProjectionDiagnostic> diagnostics,
            List<RackProjectionWarning> warnings)
        {
            if (fact?.Requirement == null || fact.Requirement.Role == RequirementRole.NotApplicable)
            {
                return;
            }

            var evidence = new RackProjectionPieceEvidence(
                fact.Requirement.PieceId,
                fact.Requirement.ViewAddress,
                fact.Requirement.Role,
                fact.Requirement.LibraryKey,
                fact.Requirement.KeyState,
                fact.LibraryAvailability,
                fact.BlockPresence);

            var satisfied = fact.Requirement.KeyState == RequirementKeyState.Present
                && fact.LibraryAvailability == LibraryAvailability.Ok
                && fact.BlockPresence == LibraryBlockPresence.Present;

            if (fact.Requirement.Role == RequirementRole.OptionalVisual)
            {
                if (!satisfied)
                {
                    warnings.Add(new RackProjectionWarning(
                        RackProjectionWarningCode.OptionalVisualMissing,
                        rackId,
                        physicalKey,
                        "Pieza visual opcional sin bloque disponible.",
                        evidence));
                }

                return;
            }

            if (satisfied)
            {
                return;
            }

            RackProjectionFailureCode code;
            if (fact.Requirement.KeyState == RequirementKeyState.KeyMissing)
            {
                code = RackProjectionFailureCode.RequiredKeyMissing;
            }
            else if (fact.LibraryAvailability == LibraryAvailability.FileMissing)
            {
                code = RackProjectionFailureCode.RequiredLibraryMissing;
            }
            else if (fact.LibraryAvailability == LibraryAvailability.Unknown
                || fact.BlockPresence == LibraryBlockPresence.Unknown)
            {
                code = RackProjectionFailureCode.RequiredUnknownAvailability;
            }
            else
            {
                code = RackProjectionFailureCode.RequiredBlockMissing;
            }

            diagnostics.Add(new RackProjectionDiagnostic(
                RackProjectionStage.Plans,
                code,
                rackId,
                physicalKey,
                fact.Requirement.ViewAddress,
                RackProjectionRemedySelector.For(code, systemKind, hasAnotherValidSiblingView: false, detail: fact.Requirement.PieceId),
                evidence));
        }

        private static IEnumerable<RackProjectionWarning> Overlaps(RackOrthographicProjection projection)
        {
            for (var first = 0; first < projection.References.Count; first++)
            {
                for (var second = first + 1; second < projection.References.Count; second++)
                {
                    var a = projection.References[first];
                    var b = projection.References[second];
                    var lowA = Math.Min(a.SourceCoordinate, a.SourceCoordinate + a.SpanLength);
                    var highA = Math.Max(a.SourceCoordinate, a.SourceCoordinate + a.SpanLength);
                    var lowB = Math.Min(b.SourceCoordinate, b.SourceCoordinate + b.SpanLength);
                    var highB = Math.Max(b.SourceCoordinate, b.SourceCoordinate + b.SpanLength);

                    if (Math.Min(highA, highB) - Math.Max(lowA, lowB) > GeometryTolerance.Length)
                    {
                        yield return new RackProjectionWarning(
                            RackProjectionWarningCode.Overlap,
                            a.View.RackId,
                            a.View.PhysicalKey,
                            "Se superpone con " + b.View.PhysicalKey + ".");
                    }
                }
            }
        }

        private static RackViewAddress? Candidate(
            RackViewAddress sourceAddress, RackSystemKind systemKind, RackProjectionResolveOutcome outcome)
        {
            // Owner decision OD-2.b A: same class keeps the variant of the source view.
            var decision = ProjectionAvailabilityPolicy.EvaluateNewView(
                systemKind, sourceAddress, outcome.Availability, RackViewProductOperation.GroupProjection);
            return decision.Decision == ProjectionPolicyDecisionKind.Accept ? sourceAddress : (RackViewAddress?)null;
        }

        private static RackViewAddress? Canonical(
            DimensionViewKind targetKind, RackSystemKind systemKind, RackProjectionResolveOutcome outcome)
        {
            // Owner decision OD-2 A: a different class takes the fixed canonical variant that the rack offers.
            var candidates = outcome.AvailableTargetAddresses
                .Where(address => address.Kind == targetKind)
                .OrderBy(Priority)
                .ToList();

            foreach (var candidate in candidates)
            {
                var decision = ProjectionAvailabilityPolicy.EvaluateNewView(
                    systemKind, candidate, outcome.Availability, RackViewProductOperation.GroupProjection);
                if (decision.Decision == ProjectionPolicyDecisionKind.Accept)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static int Priority(RackViewAddress address)
        {
            switch (address.Variant.Kind)
            {
                case RackViewVariantKind.Whole:
                    return 0;
                case RackViewVariantKind.FlowEnd:
                    return address.Variant.FlowEnd == RackFlowEnd.Exit ? 0 : 1;
                case RackViewVariantKind.PushBackCut:
                    return (address.Variant.PushBackEnd == RackPushBackEnd.EntradaSalida ? 0 : 2)
                        + (address.Variant.PushBackSide == RackPushBackSide.A ? 0 : 1);
                default:
                    return address.Variant.Index;
            }
        }

        private static RackAuthoredComparisonOutcome Map(RackProjectionAuthoredState state)
        {
            switch (state)
            {
                case RackProjectionAuthoredState.Single:
                    return RackAuthoredComparisonOutcome.Single;
                case RackProjectionAuthoredState.Divergent:
                    return RackAuthoredComparisonOutcome.Divergent;
                default:
                    return RackAuthoredComparisonOutcome.Unreadable;
            }
        }

        private static RackProjectionFailureCode MapResolveFailure(RackProjectionResolveFailure failure)
        {
            switch (failure)
            {
                case RackProjectionResolveFailure.BrokenReference:
                    return RackProjectionFailureCode.ResolveBrokenReference;
                case RackProjectionResolveFailure.DependencyUnavailable:
                    return RackProjectionFailureCode.ResolveDependencyUnavailable;
                case RackProjectionResolveFailure.LegacyUnsupported:
                    return RackProjectionFailureCode.ResolveLegacyUnsupported;
                case RackProjectionResolveFailure.Unreadable:
                case RackProjectionResolveFailure.UnknownKind:
                    return RackProjectionFailureCode.ResolveUnreadable;
                default:
                    return RackProjectionFailureCode.ResolveFailed;
            }
        }

        private static IReadOnlyList<RackPhysicalMemberFacts> Ordered(RackPhysicalRackGroup group)
            => group.Members.OrderBy(member => member.PhysicalKey, StringComparer.Ordinal).ToList();

        private static RackProjectionDiagnostic Diagnostic(
            RackProjectionStage stage,
            RackProjectionFailureCode code,
            RackPhysicalMemberFacts member,
            RackSystemKind systemKind = RackSystemKind.SelectiveRack,
            string detail = null)
            => new RackProjectionDiagnostic(
                stage,
                code,
                member.RackId,
                member.PhysicalKey,
                member.HasAddress ? member.Address : (RackViewAddress?)null,
                RackProjectionRemedySelector.For(code, systemKind, hasAnotherValidSiblingView: false, detail: detail));

        private static RackProjectionDiagnostic Diagnostic(
            RackProjectionStage stage,
            RackProjectionFailureCode code,
            RackProjectionSourceView view)
            => new RackProjectionDiagnostic(
                stage,
                code,
                view.RackId,
                view.PhysicalKey,
                view.SourceAddress,
                RackProjectionRemedySelector.For(
                    code, RackSystemKind.SelectiveRack, hasAnotherValidSiblingView: false));

        private static RackGroupPlacementPlanResult Failure(
            RackProjectionStage stage,
            IReadOnlyList<RackProjectionDiagnostic> diagnostics,
            IReadOnlyList<RackProjectionWarning> warnings)
            => new RackGroupPlacementPlanResult(null, stage, diagnostics, warnings);
    }
}
