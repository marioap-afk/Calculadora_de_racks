using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G15 (M, N, O, Q, R and the library prerequisites): the single write, the selection filter and the strict reading
    /// of the final library observation, each on its own so a regression names the exact rule it breaks.
    /// </summary>
    public class RackProjectionMaterializationRunTests
    {
        private static RackProjectionMaterializationUnit Unit(string rackId, params string[] referenceKeys)
        {
            var group = new RackProjectionGroup(
                rackId, RackSystemKind.SelectiveRack, referenceKeys, G14.Planta, G14.Planta,
                new RackProjectionTargetPreparation(true, PieceRequirementExtractionOutcome.Extracted, null));
            var view = G15.Prepared(rackId, RackSystemKind.SelectiveRack, G14.Planta);
            var placements = referenceKeys
                .Select((key, index) => new RackProjectedPlacement(
                    key, rackId, G14.Planta, new Point3D(index * 10.0, 0, 0), 0.0))
                .ToList();
            return new RackProjectionMaterializationUnit(group, view, placements);
        }

        [Fact]
        public void G15_N_MANY_REFERENCES_OF_ONE_RACKID_SHARE_ONE_NEW_DEFINITION_AND_ONE_COMMIT()
        {
            var events = new List<string>();
            var scopes = new G15.ScopeFactory(events);

            var result = RackProjectionMaterializationRun.Execute(scopes, new[] { Unit(G14.RackA, "REF-1", "REF-2", "REF-3") });

            Assert.True(result.IsCommitted);
            Assert.Equal(1, result.Definitions);
            Assert.Equal(3, result.References);
            Assert.Equal(1, scopes.Commits);
            Assert.Equal(new[] { "begin", "create:" + G14.RackA, "place:REF-1", "place:REF-2", "place:REF-3", "commit", "dispose:committed" }, events);
        }

        [Fact]
        public void G15_O_A_FAILURE_LEAVES_THE_SCOPE_UNCOMMITTED_AND_DISPOSED()
        {
            var events = new List<string>();
            var scopes = new G15.ScopeFactory(events)
            {
                OnPlace = placement => RackProjectionReferenceResult.Failed("no"),
            };

            var result = RackProjectionMaterializationRun.Execute(scopes, new[] { Unit(G14.RackA, "REF-1") });

            Assert.False(result.IsCommitted);
            Assert.Equal(RackProjectionMaterializationStatus.RolledBack, result.Status);
            Assert.Equal(0, scopes.Commits);
            Assert.Equal(1, scopes.Disposed);
            Assert.DoesNotContain("commit", events);
        }

        [Fact]
        public void G15_O_A_SCOPE_THAT_CANNOT_BEGIN_WRITES_NOTHING()
        {
            var result = RackProjectionMaterializationRun.Execute(new ThrowingScopes(), new[] { Unit(G14.RackA, "REF-1") });

            Assert.Equal(RackProjectionWriteFailure.ScopeUnavailable, result.Failure);
            Assert.False(result.IsCommitted);
        }

        [Fact]
        public void G15_A_THE_RUN_NEVER_CREATES_AN_IDENTITY()
        {
            var events = new List<string>();
            var scopes = new G15.ScopeFactory(events);

            RackProjectionMaterializationRun.Execute(scopes, new[] { Unit(G14.RackA, "REF-1"), Unit(G14.RackB, "REF-2") });

            Assert.Equal(new[] { G14.RackA, G14.RackB }, scopes.Created.Select(v => v.Envelope.Id).ToArray());
            Assert.All(scopes.Placed, p => Assert.Equal(p.RackId, scopes.Created.Single(v => v.RackId == p.RackId).RackId));
        }

        // ------------------------------------------------------------ library prerequisites

        [Fact]
        public void G15_A_ONLY_A_REQUIRED_PIECE_WITHOUT_ITS_BLOCK_IS_A_PREREQUISITE_FAILURE()
        {
            var required = new[]
            {
                new LibraryPieceRequirement("P-1", G14.Planta, RequirementRole.Required, "POSTE"),
                new LibraryPieceRequirement("P-2", G14.Planta, RequirementRole.OptionalVisual, "TARIMA"),
            };
            var observation = new RackProjectionLibraryObservation(
                new[]
                {
                    new LibraryBlockAvailabilityFact(new LibraryBlockRequirement("POSTE"), LibraryBlockAvailability.Missing),
                    new LibraryBlockAvailabilityFact(new LibraryBlockRequirement("TARIMA"), LibraryBlockAvailability.Missing),
                },
                false);

            var missing = RackProjectionPrerequisites.Evaluate(required, observation);

            Assert.Equal(new[] { "P-1" }, missing.Select(m => m.PieceId).ToArray());
        }

        [Fact]
        public void G15_A_A_KEY_THE_OBSERVATION_DOES_NOT_MENTION_IS_NEVER_ASSUMED_PRESENT()
        {
            var required = new[] { new LibraryPieceRequirement("P-1", G14.Planta, RequirementRole.Required, "POSTE") };

            var missing = RackProjectionPrerequisites.Evaluate(required, null);

            Assert.Single(missing);
        }

        [Fact]
        public void G15_A_THE_KEYS_ARE_NEVER_SANITISED()
        {
            var required = new[]
            {
                new LibraryPieceRequirement("P-1", G14.Planta, RequirementRole.Required, "RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL"),
            };

            var keys = RackProjectionPrerequisites.Keys(required);

            Assert.Equal("RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL", Assert.Single(keys).Key);
        }

        // ------------------------------------------------------------ the selection that the plan receives

        [Fact]
        public void G15_A_ENTITIES_THAT_ARE_NOT_RACKS_ARE_IGNORED_WITH_A_NOTICE()
        {
            var references = new[]
            {
                G14.Reference("REF-1", "DEF-1"),
                new RackPhysicalReferenceSnapshot("LINE", false, RackPhysicalSpace.ModelSpace, false, null, new Point2D(0, 0)),
                new RackPhysicalReferenceSnapshot("PAPER", true, RackPhysicalSpace.PaperSpace, false, "DEF-1", new Point2D(0, 0)),
                new RackPhysicalReferenceSnapshot("PLAIN", true, RackPhysicalSpace.ModelSpace, false, "DEF-P", new Point2D(0, 0)),
            };
            var definitions = new[]
            {
                G14.Definition("DEF-1", G14.RackA),
                new RackPhysicalDefinitionSnapshot("DEF-P", null, "PLAIN_BLOCK"),
            };

            var snapshot = RackProjectionSelectionFilter.Build(references, definitions, G14.Known);

            Assert.Single(snapshot.Selection.Members);
            Assert.Single(snapshot.Selection.SelectedMembers);
            Assert.Equal(3, snapshot.Notices.Count);
        }

        [Fact]
        public void G15_A_A_MEMBER_THE_PLAN_CANNOT_PROJECT_STAYS_IN_THE_SELECTION_TO_FAIL_THE_WHOLE_OPERATION()
        {
            var references = new[]
            {
                G14.Reference("REF-1", "DEF-1"),
                new RackPhysicalReferenceSnapshot("XREF", true, RackPhysicalSpace.ModelSpace, true, "DEF-1", new Point2D(0, 0)),
            };

            var snapshot = RackProjectionSelectionFilter.Build(
                references, new[] { G14.Definition("DEF-1", G14.RackA) }, G14.Known);

            Assert.Equal(2, snapshot.Selection.Members.Count);
            Assert.Single(snapshot.Selection.SelectedMembers);
            Assert.Empty(snapshot.Notices);
        }

        private sealed class ThrowingScopes : IRackProjectionWriteScopeFactory
        {
            public IRackProjectionWriteScope Begin() => throw new InvalidOperationException("no document");
        }
    }
}
