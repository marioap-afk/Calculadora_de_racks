using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14 (A, E, F, G, H, I, J, K, L, M, N, Z, AA, AB, AC, AD, AF, AG): the pure ID19 pipeline. Every blocking
    /// failure happens before any point is requested and nothing here creates a command or a new identity.
    /// </summary>
    public class RackGroupPlacementPlanTests
    {
        [Fact]
        public void G14_A_ONE_COMMON_TRANSFORM_IS_SHARED_BY_EVERY_RACKID_GROUP()
        {
            var plan = Assert.IsType<RackGroupPlacementPlan>(TwoRacks().Plan);

            var placement = plan.Place(new Point3D(0, 0, 0), new Point3D(500, 0, 0));

            Assert.True(placement.IsAvailable);
            Assert.Equal(2, plan.Groups.Count);
            Assert.Equal(2, placement.Placements.Count);
            Assert.NotNull(placement.Transform);
            Assert.Equal(1.0, placement.Transform.Determinant, 9);
            Assert.Equal(1.0, placement.Transform.ScaleFactor, 9);
        }

        [Fact]
        public void G14_E_A_SELECTION_PERMUTATION_DOES_NOT_CHANGE_THE_OUTCOME()
        {
            var direct = TwoRacks();
            var permuted = TwoRacks(reverse: true);

            Assert.True(direct.IsAvailable);
            Assert.True(permuted.IsAvailable);
            Assert.Equal(
                direct.Plan.Groups.Select(group => group.RackId),
                permuted.Plan.Groups.Select(group => group.RackId));

            var first = direct.Plan.Place(new Point3D(0, 0, 0), new Point3D(500, 0, 0));
            var second = permuted.Plan.Place(new Point3D(0, 0, 0), new Point3D(500, 0, 0));

            Assert.Equal(
                first.Placements.Select(x => (x.RackId, Math.Round(x.Position.X, 6), Math.Round(x.Position.Y, 6))),
                second.Placements.Select(x => (x.RackId, Math.Round(x.Position.X, 6), Math.Round(x.Position.Y, 6))));
        }

        [Fact]
        public void G14_F_A_BLANK_RACKID_FAILS_CLOSED_BEFORE_ANY_POINT()
        {
            var services = SelectiveServices();
            var request = Request(
                G14.Selection(
                    new[] { G14.Reference("REF-1", "DEF-1") },
                    new[] { G14.Definition("DEF-1", rackId: null) }),
                services);

            var result = RackGroupPlacementPlan.Create(request);

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.Classify, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.BlankRackId);
            Assert.Empty(services.ResolveCalls);
        }

        [Fact]
        public void G14_G_MIXED_SOURCE_TYPES_FAIL_CLOSED()
        {
            var services = SelectiveServices();
            var request = Request(
                G14.Selection(
                    new[] { G14.Reference("REF-1", "DEF-1"), G14.Reference("REF-2", "DEF-2") },
                    new[]
                    {
                        G14.Definition("DEF-1", G14.RackA),
                        G14.Definition("DEF-2", G14.RackB, view: RackEmbedDocument.ViewFrontal, section: 0),
                    }),
                services);

            var result = RackGroupPlacementPlan.Create(request);

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.Group, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.MixedSourceTypes);
        }

        [Fact]
        public void G14_H_SEVERAL_DEFINITIONS_OF_ONE_RACKID_FAIL_CLOSED()
        {
            var services = SelectiveServices();
            var request = Request(
                G14.Selection(
                    new[] { G14.Reference("REF-1", "DEF-1"), G14.Reference("REF-2", "DEF-2") },
                    new[] { G14.Definition("DEF-1", G14.RackA), G14.Definition("DEF-2", G14.RackA) }),
                services);

            var result = RackGroupPlacementPlan.Create(request);

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.Group, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.MultipleSourceDefinitions);
        }

        [Fact]
        public void G14_I_AUTHORED_SINGLE_PROCEEDS()
        {
            Assert.True(TwoRacks().IsAvailable);
        }

        [Fact]
        public void G14_J_AUTHORED_DIVERGENT_FAILS_AT_THE_AUTHORITY_GATE()
        {
            var services = SelectiveServices();
            services.Authored = _ => RackProjectionAuthoredState.Divergent;

            var result = RackGroupPlacementPlan.Create(Request(OneRackSelection(), services));

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.AuthorityGates, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.AuthoredDivergent);
            Assert.Empty(services.ResolveCalls);
        }

        [Fact]
        public void G14_K_AUTHORED_UNREADABLE_FAILS_AT_THE_AUTHORITY_GATE()
        {
            var services = SelectiveServices();
            services.Authored = _ => RackProjectionAuthoredState.Unreadable;

            var result = RackGroupPlacementPlan.Create(Request(OneRackSelection(), services));

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.AuthorityGates, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.AuthoredUnreadable);
        }

        [Fact]
        public void G14_L_DIVERGENT_PROPERTIES_FAIL_AT_THE_AUTHORITY_GATE_AND_STAY_INDEPENDENT()
        {
            var services = SelectiveServices();
            services.Properties = _ => RackProjectionPropertiesState.Divergent;

            var result = RackGroupPlacementPlan.Create(Request(OneRackSelection(), services));

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.AuthorityGates, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.PropertiesDivergent);
            Assert.DoesNotContain(result.Diagnostics, x => x.Code == RackProjectionFailureCode.AuthoredDivergent);
        }

        [Fact]
        public void G14_M_RESOLVE_RUNS_EXACTLY_ONCE_PER_RACKID()
        {
            var services = SelectiveServices();
            var result = RackGroupPlacementPlan.Create(Request(TwoRackSelection(), services));

            Assert.True(result.IsAvailable);
            Assert.Equal(new[] { G14.RackA, G14.RackB }, services.ResolveCalls.OrderBy(x => x, StringComparer.Ordinal));
            Assert.Equal(2, services.ResolveCalls.Count);
        }

        [Fact]
        public void G14_N_A_BLOCKING_RESOLVE_DIAGNOSTIC_FAILS_CLOSED()
        {
            var services = SelectiveServices();
            services.ResolveState = _ => RackProjectionResolveState.ResolvedWithOutputBlocking;

            var result = RackGroupPlacementPlan.Create(Request(OneRackSelection(), services));

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.Resolve, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.ResolveOutputBlocking);
            Assert.Empty(services.PrepareCalls);
        }

        [Fact]
        public void G14_N_A_FAILED_RESOLVE_FAILS_CLOSED()
        {
            var services = SelectiveServices();
            services.ResolveState = _ => RackProjectionResolveState.Failed;

            var result = RackGroupPlacementPlan.Create(Request(OneRackSelection(), services));

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.Resolve, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.ResolveFailed);
        }

        [Fact]
        public void G14_EDIT_PREFLIGHT_REJECTION_FAILS_BEFORE_FRAMES()
        {
            var services = SelectiveServices();
            services.EditPreflight = _ => new RackProjectionEditPreflight(true, xrefDependent: true, code: "xref");

            var result = RackGroupPlacementPlan.Create(Request(OneRackSelection(), services));

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.EditPreflight, result.FailedStage);
            Assert.Contains(
                result.Diagnostics,
                x => x.Code == RackProjectionFailureCode.EditPreflightRejected
                    && x.Remedy.Kind == RackProjectionRemedyKind.FixInXrefSource);
        }

        [Fact]
        public void G14_Z_A_REQUIRED_PIECE_WITHOUT_KEY_BLOCKS_BEFORE_POINTS()
        {
            var result = WithPieces(G14.Piece("P-1", RequirementRole.Required, "   "));

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.Plans, result.FailedStage);
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(RackProjectionFailureCode.RequiredKeyMissing, diagnostic.Code);
            Assert.Equal("P-1", diagnostic.Piece.PieceId);
            Assert.Equal(RequirementKeyState.KeyMissing, diagnostic.Piece.KeyState);
        }

        [Fact]
        public void G14_AA_A_REQUIRED_PIECE_WITH_A_MISSING_BLOCK_BLOCKS()
        {
            var result = WithPieces(G14.Piece(
                "P-2", RequirementRole.Required, "POSTE", LibraryAvailability.Ok, LibraryBlockPresence.BlockMissing));

            Assert.False(result.IsAvailable);
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(RackProjectionFailureCode.RequiredBlockMissing, diagnostic.Code);
            Assert.Equal(LibraryBlockPresence.BlockMissing, diagnostic.Piece.BlockPresence);
            Assert.Equal("POSTE", diagnostic.Piece.LibraryKey);
        }

        [Fact]
        public void G14_AB_A_REQUIRED_PIECE_WITH_A_MISSING_LIBRARY_BLOCKS()
        {
            var result = WithPieces(G14.Piece(
                "P-3", RequirementRole.Required, "POSTE", LibraryAvailability.FileMissing, LibraryBlockPresence.BlockMissing));

            Assert.False(result.IsAvailable);
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(RackProjectionFailureCode.RequiredLibraryMissing, diagnostic.Code);
            Assert.Equal(LibraryAvailability.FileMissing, diagnostic.Piece.LibraryAvailability);
        }

        [Fact]
        public void G14_AB_A_REQUIRED_PIECE_WITH_UNKNOWN_AVAILABILITY_BLOCKS()
        {
            var result = WithPieces(G14.Piece(
                "P-4", RequirementRole.Required, "POSTE", LibraryAvailability.Unknown, LibraryBlockPresence.Unknown));

            Assert.False(result.IsAvailable);
            Assert.Equal(
                RackProjectionFailureCode.RequiredUnknownAvailability,
                Assert.Single(result.Diagnostics).Code);
        }

        [Fact]
        public void G14_AC_AN_OPTIONAL_VISUAL_PIECE_ONLY_WARNS()
        {
            var result = WithPieces(
                G14.Piece("P-5", RequirementRole.Required, "POSTE"),
                G14.Piece("P-6", RequirementRole.OptionalVisual, "TARIMA", LibraryAvailability.Ok, LibraryBlockPresence.BlockMissing));

            Assert.True(result.IsAvailable);
            var warning = Assert.Single(result.Warnings, x => x.Code == RackProjectionWarningCode.OptionalVisualMissing);
            Assert.Equal("P-6", warning.Piece.PieceId);
        }

        [Fact]
        public void G14_AC_A_NOT_APPLICABLE_PIECE_IS_IGNORED()
        {
            var result = WithPieces(
                G14.Piece("P-7", RequirementRole.NotApplicable, null, LibraryAvailability.Unknown, LibraryBlockPresence.Unknown));

            Assert.True(result.IsAvailable);
            Assert.Empty(result.Warnings);
        }

        [Fact]
        public void G14_AD_AN_UNKNOWN_SOURCE_ROLE_FAILS_CLOSED()
        {
            var services = SelectiveServices();
            services.DefaultPreparation = new RackProjectionTargetPreparation(
                true, PieceRequirementExtractionOutcome.UnknownSourceRole, Array.Empty<LibraryPieceAvailabilityFact>());

            var result = RackGroupPlacementPlan.Create(Request(OneRackSelection(), services));

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.Plans, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.UnknownSourceRole);
        }

        [Fact]
        public void G14_AF_AG_IDENTITY_AND_LOGICAL_RACK_COUNT_ARE_PRESERVED()
        {
            var result = TwoRacks();
            var placement = result.Plan.Place(new Point3D(0, 0, 0), new Point3D(500, 0, 0));

            Assert.Equal(
                new[] { G14.RackA, G14.RackB }.OrderBy(x => x, StringComparer.Ordinal),
                placement.Placements.Select(x => x.RackId).OrderBy(x => x, StringComparer.Ordinal));
            Assert.Equal(2, placement.Placements.Select(x => x.RackId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
            Assert.Equal(2, result.Plan.Groups.Count);
            Assert.All(placement.Placements, x => Assert.Equal(1.0, x.Scale, 12));
        }

        [Fact]
        public void G14_TARGET_ADDRESS_IS_THE_FROZEN_CANONICAL_ONE_FOR_THE_REQUESTED_CLASS()
        {
            var plan = TwoRacks().Plan;

            Assert.All(plan.Groups, group => Assert.Equal(G14.Frontal0, group.TargetAddress));
            Assert.All(plan.Groups, group => Assert.Equal(G14.Planta, group.SourceAddress));
            Assert.Equal(RackProjectionMode.Orthographic, plan.Mode);
            Assert.Equal(RackPhysicalAxis.Run, plan.ConservedAxis);
        }

        [Fact]
        public void G14_AN_UNSUPPORTED_MEMBER_FAILS_THE_WHOLE_SELECTION()
        {
            var services = SelectiveServices();
            var request = Request(
                G14.Selection(
                    new[]
                    {
                        G14.Reference("REF-1", "DEF-1"),
                        new RackPhysicalReferenceSnapshot("REF-X", true, RackPhysicalSpace.ModelSpace, true, "DEF-X", new Point2D(0, 0)),
                    },
                    new[] { G14.Definition("DEF-1", G14.RackA), G14.Definition("DEF-X", G14.RackB) }),
                services);

            var result = RackGroupPlacementPlan.Create(request);

            Assert.False(result.IsAvailable);
            Assert.Equal(RackProjectionStage.Classify, result.FailedStage);
            Assert.Contains(result.Diagnostics, x => x.Code == RackProjectionFailureCode.UnsupportedMember);
        }

        [Fact]
        public void G14_PLAN_EXPOSES_NO_AUTOCAD_TYPES()
        {
            var assemblies = typeof(RackGroupPlacementPlan).Assembly
                .GetReferencedAssemblies()
                .Select(name => name.Name)
                .ToArray();

            Assert.DoesNotContain(assemblies, name => name.StartsWith("Autodesk", StringComparison.Ordinal));
            Assert.DoesNotContain(assemblies, name => name.StartsWith("accore", StringComparison.OrdinalIgnoreCase));
        }

        private static RackGroupPlacementPlanResult WithPieces(params LibraryPieceAvailabilityFact[] pieces)
        {
            var services = SelectiveServices();
            services.DefaultPreparation = G14.Preparation(pieces);
            return RackGroupPlacementPlan.Create(Request(OneRackSelection(), services));
        }

        private static RackGroupPlacementPlanResult TwoRacks(bool reverse = false)
            => RackGroupPlacementPlan.Create(Request(TwoRackSelection(reverse), SelectiveServices()));

        private static RackPhysicalSelection OneRackSelection()
            => G14.Selection(
                new[] { G14.Reference("REF-1", "DEF-1") },
                new[] { G14.Definition("DEF-1", G14.RackA) });

        private static RackPhysicalSelection TwoRackSelection(bool reverse = false)
        {
            var references = new List<RackPhysicalReferenceSnapshot>
            {
                G14.Reference("REF-1", "DEF-1"),
                G14.Reference("REF-2", "DEF-2"),
            };

            if (reverse)
            {
                references.Reverse();
            }

            return G14.Selection(
                references,
                new[] { G14.Definition("DEF-1", G14.RackA), G14.Definition("DEF-2", G14.RackB) });
        }

        private static G14.Services SelectiveServices()
        {
            var services = new G14.Services(RackSystemKind.SelectiveRack, G14.SelectiveFacts(fondos: 1, posts: 0));
            services.TargetAddresses.Add(G14.Frontal0);
            services.TargetAddresses.Add(G14.Planta);
            foreach (var rackId in new[] { G14.RackA, G14.RackB, G14.RackC })
            {
                services.WithFrame(rackId, G14.Planta, G14.PlantaFrame());
                services.WithFrame(rackId, G14.Frontal0, G14.FrontalFrame());
            }

            services.DefaultPreparation = G14.Preparation(G14.Piece("P-1", RequirementRole.Required, "POSTE"));
            return services;
        }

        private static RackProjectionRequest Request(RackPhysicalSelection selection, G14.Services services)
        {
            var facts = new Dictionary<string, RackSourceTransformFactsResult>
            {
                ["REF-1"] = G14.Facts(x: 0, y: 0),
                ["REF-2"] = G14.Facts(x: 0, y: 100),
                ["REF-X"] = G14.Facts(x: 0, y: 200),
            };

            return new RackProjectionRequest(selection, DimensionViewKind.Frontal, facts, services.Build());
        }
    }
}
