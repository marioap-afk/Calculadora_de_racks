using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Tests
{
    /// <summary>
    /// Builders for the pure ID19 contract of G14. Everything here feeds the product plan with Foundation facts;
    /// no helper reimplements a Foundation authority.
    /// </summary>
    internal static class G14
    {
        internal const string RackA = "11111111-1111-4111-8111-111111111111";
        internal const string RackB = "22222222-2222-4222-8222-222222222222";
        internal const string RackC = "33333333-3333-4333-8333-333333333333";

        internal static readonly RackSourceTransformTolerance Tolerance =
            new RackSourceTransformTolerance(1e-9, GeometryTolerance.Angle, 1e-9);

        internal static RackPhysicalDefinitionSnapshot Definition(
            string key, string rackId, string kind = RackEmbedDocument.KindSelective,
            string view = RackEmbedDocument.ViewPlanta, int section = -1, int references = 1)
            => new RackPhysicalDefinitionSnapshot(
                key,
                new RackEmbedStore().Serialize(new RackEmbedDocument
                {
                    Kind = kind,
                    Id = rackId,
                    Name = "Rack",
                    View = view,
                    Section = section,
                    Design = "{}",
                }),
                "BLOCK_" + key,
                references);

        internal static RackPhysicalReferenceSnapshot Reference(string key, string definitionKey)
            => new RackPhysicalReferenceSnapshot(
                key, true, RackPhysicalSpace.ModelSpace, false, definitionKey, new Point2D(0.0, 0.0));

        internal static RackPhysicalKindDisposition Known(string kind)
            => new[]
            {
                RackEmbedDocument.KindSelective,
                RackEmbedDocument.KindDynamic,
                RackEmbedDocument.KindPushBack,
                RackEmbedDocument.KindCantilever,
                RackEmbedDocument.KindCabecera,
                RackEmbedDocument.KindCama,
            }.Contains(kind, StringComparer.OrdinalIgnoreCase)
                ? RackPhysicalKindDisposition.Known
                : RackPhysicalKindDisposition.Unknown;

        internal static RackPhysicalSelection Selection(
            IEnumerable<RackPhysicalReferenceSnapshot> references,
            IEnumerable<RackPhysicalDefinitionSnapshot> definitions)
            => RackPhysicalSelection.Classify(references.ToArray(), definitions.ToArray(), Known);

        /// <summary>AUTH-08 V2 facts for one reference. Only the Foundation classifier produces them.</summary>
        internal static RackSourceTransformFactsResult Facts(
            double x, double y, double rotation = 0.0, double z = 0.0,
            double scaleX = 1.0, double scaleY = 1.0, double scaleZ = 1.0,
            double originX = 0.0, double originY = 0.0,
            double normalX = 0.0, double normalY = 0.0, double normalZ = 1.0)
            => RackSourceTransformClassifier.Classify(
                new RackSourcePlacementInput(
                    x, y, z, rotation, scaleX, scaleY, scaleZ, normalX, normalY, normalZ, originX, originY, 0.0),
                Tolerance);

        /// <summary>Planta frame of a rack family: local X is Depth, local Y is Run.</summary>
        internal static RackViewFrame PlantaFrame(double depth = 48.0, double runOrigin = 0.0, double depthOrigin = 0.0)
            => RackViewFrame.TryCreate(
                RackViewAxisMap.DepthRun,
                new RackPhysicalPoint(runOrigin, depthOrigin, 0.0),
                0.0,
                depth,
                RackFrameEndpointConvention.PhysicalDepthFaces,
                RackPhysicalVector.Zero).Frame;

        /// <summary>Frontal frame of a rack family: local X is Run, local Y is Height.</summary>
        internal static RackViewFrame FrontalFrame(double runOrigin = 0.0, double length = 90.0, double depthOrigin = 0.0)
            => RackViewFrame.TryCreate(
                RackViewAxisMap.RunHeight,
                new RackPhysicalPoint(runOrigin, depthOrigin, 0.0),
                0.0,
                length,
                RackFrameEndpointConvention.PostAxisClosedInterval,
                new RackPhysicalVector(0.0, depthOrigin, 0.0)).Frame;

        /// <summary>Lateral frame of a rack family: local X is Depth, local Y is Height.</summary>
        internal static RackViewFrame LateralFrame(double runOrigin = 0.0, double depth = 48.0)
            => RackViewFrame.TryCreate(
                RackViewAxisMap.DepthHeight,
                new RackPhysicalPoint(runOrigin, 0.0, 0.0),
                0.0,
                depth,
                RackFrameEndpointConvention.StorageDepthFaces,
                new RackPhysicalVector(runOrigin, 0.0, 0.0)).Frame;

        internal static RackViewAddress Planta => RackViewAddress.Whole(DimensionViewKind.Planta);
        internal static RackViewAddress Frontal0 => RackViewAddress.Fondo(0);
        internal static RackViewAddress Lateral0 => RackViewAddress.Post(0);

        internal static LibraryPieceAvailabilityFact Piece(
            string pieceId,
            RequirementRole role,
            string key,
            LibraryAvailability availability = LibraryAvailability.Ok,
            LibraryBlockPresence presence = LibraryBlockPresence.Present,
            RackViewAddress? address = null)
            => Build(new LibraryPieceRequirement(pieceId, address ?? Frontal0, role, key), availability, presence);

        private static LibraryPieceAvailabilityFact Build(
            LibraryPieceRequirement requirement,
            LibraryAvailability availability,
            LibraryBlockPresence presence)
        {
            var query = new StubQuery(requirement, availability, presence);
            var flow = LibraryPieceAvailabilityFlowV2.Observe(
                new[] { requirement }, false, query, null);
            return flow.Facts[0];
        }

        private sealed class StubQuery : ILibraryPieceAvailabilityQuery
        {
            private readonly LibraryPieceRequirement requirement;
            private readonly LibraryAvailability availability;
            private readonly LibraryBlockPresence presence;

            internal StubQuery(
                LibraryPieceRequirement requirement,
                LibraryAvailability availability,
                LibraryBlockPresence presence)
            {
                this.requirement = requirement;
                this.availability = availability;
                this.presence = presence;
            }

            public IReadOnlyList<LibraryKeyAvailabilityObservation> Query(
                IReadOnlyList<LibraryBlockRequirement> requirements)
                => requirement.KeyState == RequirementKeyState.Present
                    ? new[]
                    {
                        new LibraryKeyAvailabilityObservation(requirement.LibraryKey, availability, presence),
                    }
                    : Array.Empty<LibraryKeyAvailabilityObservation>();
        }

        internal static RackProjectionTargetPreparation Preparation(
            params LibraryPieceAvailabilityFact[] pieces)
            => new RackProjectionTargetPreparation(
                true, PieceRequirementExtractionOutcome.Extracted, pieces ?? Array.Empty<LibraryPieceAvailabilityFact>());

        /// <summary>Recording services so a test can count Resolve calls and vary one authority at a time.</summary>
        internal sealed class Services
        {
            internal Services(RackSystemKind systemKind, RackViewAvailabilityFacts availability)
            {
                SystemKind = systemKind;
                Availability = availability;
                Frames = new Dictionary<(string, RackViewAddress), RackViewFrame>();
                Preparations = new Dictionary<(string, RackViewAddress), RackProjectionTargetPreparation>();
                ResolveCalls = new List<string>();
                PrepareCalls = new List<(string, RackViewAddress)>();
                Authored = _ => RackProjectionAuthoredState.Single;
                Properties = _ => RackProjectionPropertiesState.Equivalent;
                EditPreflight = _ => RackProjectionEditPreflight.Accepted;
                ResolveState = _ => RackProjectionResolveState.Resolved;
                TargetAddresses = new List<RackViewAddress>();
            }

            internal RackSystemKind SystemKind { get; }
            internal RackViewAvailabilityFacts Availability { get; }
            internal Dictionary<(string, RackViewAddress), RackViewFrame> Frames { get; }
            internal Dictionary<(string, RackViewAddress), RackProjectionTargetPreparation> Preparations { get; }
            internal List<string> ResolveCalls { get; }
            internal List<(string, RackViewAddress)> PrepareCalls { get; }
            internal Func<string, RackProjectionAuthoredState> Authored { get; set; }
            internal Func<string, RackProjectionPropertiesState> Properties { get; set; }
            internal Func<string, RackProjectionEditPreflight> EditPreflight { get; set; }
            internal Func<string, RackProjectionResolveState> ResolveState { get; set; }
            internal Func<string, RackProjectionResolveFailure> ResolveFailure { get; set; } =
                _ => RackProjectionResolveFailure.None;
            internal List<RackViewAddress> TargetAddresses { get; }
            internal RackProjectionTargetPreparation DefaultPreparation { get; set; } = Preparation();

            internal Services WithFrame(string rackId, RackViewAddress address, RackViewFrame frame)
            {
                Frames[(rackId, address)] = frame;
                return this;
            }

            internal Services WithPreparation(
                string rackId, RackViewAddress address, RackProjectionTargetPreparation preparation)
            {
                Preparations[(rackId, address)] = preparation;
                return this;
            }

            internal RackProjectionServices Build()
                => new RackProjectionServices
                {
                    Resolve = rackId =>
                    {
                        ResolveCalls.Add(rackId);
                        return new RackProjectionResolveOutcome(
                            ResolveState(rackId), Availability, TargetAddresses, "code", ResolveFailure(rackId));
                    },
                    Frame = (rackId, address) => Frames.TryGetValue((rackId, address), out var frame)
                        ? RackViewFrameResult.Available(frame)
                        : RackViewFrameResult.Unavailable(RackViewFrameFailure.UnsupportedAddress),
                    Prepare = (rackId, address) =>
                    {
                        PrepareCalls.Add((rackId, address));
                        return Preparations.TryGetValue((rackId, address), out var preparation)
                            ? preparation
                            : DefaultPreparation;
                    },
                    Authored = rackId => Authored(rackId),
                    Properties = rackId => Properties(rackId),
                    EditPreflight = rackId => EditPreflight(rackId),
                };
        }

        internal static SelectiveViewAvailabilityFacts SelectiveFacts(int fondos = 1, params int[] posts)
            => new SelectiveViewAvailabilityFacts(fondos, posts.Length == 0 ? new[] { 0 } : posts);
    }
}
