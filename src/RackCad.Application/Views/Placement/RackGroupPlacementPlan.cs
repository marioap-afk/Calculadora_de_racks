using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    public enum RackProjectionAuthoredState
    {
        Single,
        Divergent,
        Unreadable
    }

    public enum RackProjectionPropertiesState
    {
        Equivalent,
        Divergent,
        UnreadableAttributable
    }

    public enum RackProjectionResolveState
    {
        Resolved,
        ResolvedWithOutputBlocking,
        Failed
    }

    /// <summary>Why AUTH-09 could not hand a system over. The product never invents a fallback for these.</summary>
    public enum RackProjectionResolveFailure
    {
        None,
        BrokenReference,
        DependencyUnavailable,
        LegacyUnsupported,
        Unreadable,
        UnknownKind
    }

    /// <summary>AUTH-09 outcome for one RackId, resolved exactly once.</summary>
    public sealed class RackProjectionResolveOutcome
    {
        public RackProjectionResolveOutcome(
            RackProjectionResolveState state,
            RackViewAvailabilityFacts availability,
            IReadOnlyList<RackViewAddress> availableTargetAddresses,
            string code = null,
            RackProjectionResolveFailure failure = RackProjectionResolveFailure.None)
        {
            State = state;
            Availability = availability;
            AvailableTargetAddresses = availableTargetAddresses ?? Array.Empty<RackViewAddress>();
            Code = code;
            Failure = failure;
        }

        public RackProjectionResolveState State { get; }
        public RackViewAvailabilityFacts Availability { get; }
        public IReadOnlyList<RackViewAddress> AvailableTargetAddresses { get; }
        public string Code { get; }
        public RackProjectionResolveFailure Failure { get; }
    }

    public sealed class RackProjectionEditPreflight
    {
        public RackProjectionEditPreflight(bool rejected, bool xrefDependent = false, string code = null)
        {
            Rejected = rejected;
            XrefDependent = xrefDependent;
            Code = code;
        }

        public bool Rejected { get; }
        public bool XrefDependent { get; }
        public string Code { get; }

        public static RackProjectionEditPreflight Accepted { get; } = new RackProjectionEditPreflight(false);
    }

    /// <summary>AUTH-10 plan plus the AUTH-12 V2 facts of the prepared target view.</summary>
    public sealed class RackProjectionTargetPreparation
    {
        public RackProjectionTargetPreparation(
            bool planAvailable,
            PieceRequirementExtractionOutcome requirementOutcome,
            IReadOnlyList<LibraryPieceAvailabilityFact> pieceFacts,
            string planFailureCode = null)
        {
            PlanAvailable = planAvailable;
            RequirementOutcome = requirementOutcome;
            PieceFacts = pieceFacts ?? Array.Empty<LibraryPieceAvailabilityFact>();
            PlanFailureCode = planFailureCode;
        }

        public bool PlanAvailable { get; }
        public PieceRequirementExtractionOutcome RequirementOutcome { get; }
        public IReadOnlyList<LibraryPieceAvailabilityFact> PieceFacts { get; }
        public string PlanFailureCode { get; }
    }

    /// <summary>Authorities the pure plan consumes. Every one of them is a Foundation or product authority.</summary>
    public sealed class RackProjectionServices
    {
        public Func<string, RackProjectionResolveOutcome> Resolve { get; set; }
        public Func<string, RackViewAddress, RackViewFrameResult> Frame { get; set; }
        public Func<string, RackViewAddress, RackProjectionTargetPreparation> Prepare { get; set; }
        public Func<string, RackProjectionAuthoredState> Authored { get; set; }
        public Func<string, RackProjectionPropertiesState> Properties { get; set; }
        public Func<string, RackProjectionEditPreflight> EditPreflight { get; set; }
    }

    public sealed class RackProjectionRequest
    {
        public RackProjectionRequest(
            RackPhysicalSelection selection,
            DimensionViewKind targetKind,
            IReadOnlyDictionary<string, RackSourceTransformFactsResult> sourceFacts,
            RackProjectionServices services)
        {
            Selection = selection;
            TargetKind = targetKind;
            SourceFacts = sourceFacts;
            Services = services;
        }

        public RackPhysicalSelection Selection { get; }
        public DimensionViewKind TargetKind { get; }
        public IReadOnlyDictionary<string, RackSourceTransformFactsResult> SourceFacts { get; }
        public RackProjectionServices Services { get; }
    }

    public sealed class RackProjectionGroup
    {
        public RackProjectionGroup(
            string rackId,
            RackSystemKind systemKind,
            IReadOnlyList<string> memberKeys,
            RackViewAddress sourceAddress,
            RackViewAddress targetAddress,
            RackProjectionTargetPreparation preparation)
        {
            RackId = rackId;
            SystemKind = systemKind;
            MemberKeys = memberKeys;
            SourceAddress = sourceAddress;
            TargetAddress = targetAddress;
            Preparation = preparation;
        }

        public string RackId { get; }
        public RackSystemKind SystemKind { get; }
        public IReadOnlyList<string> MemberKeys { get; }
        public RackViewAddress SourceAddress { get; }
        public RackViewAddress TargetAddress { get; }
        public RackProjectionTargetPreparation Preparation { get; }
    }

    public sealed class RackGroupPlacementResult
    {
        internal RackGroupPlacementResult(
            CommonTransform2D transform,
            IReadOnlyList<RackProjectedPlacement> placements,
            CommonTransformFailure failure)
        {
            Transform = transform;
            Placements = placements ?? Array.Empty<RackProjectedPlacement>();
            Failure = failure;
        }

        public CommonTransform2D Transform { get; }
        public IReadOnlyList<RackProjectedPlacement> Placements { get; }
        public CommonTransformFailure Failure { get; }
        public bool IsAvailable => Failure == CommonTransformFailure.None;
    }

    /// <summary>
    /// Pure ID19 contract: CLASSIFY, GROUP, AUTHORITY GATES, REPRESENTATIVE, RESOLVE, EDIT PREFLIGHT, AVAILABLE,
    /// FRAMES, VALIDATE and PLANS. Every blocking failure happens before any point is requested, and the plan keeps
    /// what a later command needs to materialise linked views without touching identity.
    /// </summary>
    public sealed class RackGroupPlacementPlan
    {
        internal RackGroupPlacementPlan(
            DimensionViewKind targetKind,
            RackProjectionMode mode,
            RackPhysicalAxis conservedAxis,
            IReadOnlyList<RackProjectionGroup> groups,
            IReadOnlyList<RackProjectionSourceView> views,
            RackOrthographicProjection orthographic,
            IReadOnlyList<RackProjectionWarning> warnings)
        {
            TargetKind = targetKind;
            Mode = mode;
            ConservedAxis = conservedAxis;
            Groups = groups;
            Views = views;
            Orthographic = orthographic;
            Warnings = warnings;
        }

        public DimensionViewKind TargetKind { get; }
        public RackProjectionMode Mode { get; }
        public RackPhysicalAxis ConservedAxis { get; }
        public IReadOnlyList<RackProjectionGroup> Groups { get; }
        public IReadOnlyList<RackProjectionSourceView> Views { get; }
        public RackOrthographicProjection Orthographic { get; }
        public IReadOnlyList<RackProjectionWarning> Warnings { get; }

        public static RackGroupPlacementPlanResult Create(RackProjectionRequest request)
            => RackProjectionPipeline.Run(request);

        /// <summary>Applies the single common transform of the operation to every accepted reference.</summary>
        public RackGroupPlacementResult Place(Point3D basePoint, Point3D targetPoint)
        {
            var alpha = Mode == RackProjectionMode.Rigid ? 0.0 : Orthographic.AlphaRadians;
            var transform = CommonTransform2D.TryCreate(basePoint, targetPoint, alpha);
            if (!transform.IsAvailable)
            {
                return new RackGroupPlacementResult(null, Array.Empty<RackProjectedPlacement>(), transform.Failure);
            }

            var placements = new List<RackProjectedPlacement>(Views.Count);
            if (Mode == RackProjectionMode.Rigid)
            {
                foreach (var view in Views)
                {
                    placements.Add(RackRigidPlacementPolicy.Place(view, transform.Transform));
                }
            }
            else
            {
                foreach (var reference in Orthographic.References)
                {
                    placements.Add(RackOrthographicPlacementPolicy.Place(
                        Orthographic, reference, basePoint, targetPoint));
                }
            }

            return new RackGroupPlacementResult(transform.Transform, placements, CommonTransformFailure.None);
        }
    }

    public sealed class RackGroupPlacementPlanResult
    {
        internal RackGroupPlacementPlanResult(
            RackGroupPlacementPlan plan,
            RackProjectionStage failedStage,
            IReadOnlyList<RackProjectionDiagnostic> diagnostics,
            IReadOnlyList<RackProjectionWarning> warnings)
        {
            Plan = plan;
            FailedStage = failedStage;
            Diagnostics = diagnostics ?? Array.Empty<RackProjectionDiagnostic>();
            Warnings = warnings ?? Array.Empty<RackProjectionWarning>();
        }

        public RackGroupPlacementPlan Plan { get; }
        public bool IsAvailable => Plan != null;
        public RackProjectionStage FailedStage { get; }
        public IReadOnlyList<RackProjectionDiagnostic> Diagnostics { get; }
        public IReadOnlyList<RackProjectionWarning> Warnings { get; }
    }
}
