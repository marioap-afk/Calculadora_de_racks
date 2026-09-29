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
    /// G15 (C, D, E, F, G, H, I, M, N, O, P, Q, R, S, U, V, W, X, Y, Z, AA, AB, AC, AD, AE, AF, AG, AH): the ID19
    /// command as ONE operation over a faked AutoCAD. The plan is the real G14 pipeline; only the drawing is faked.
    /// </summary>
    public class RackProjectionCommandRunTests
    {
        // ------------------------------------------------------------ before the points

        [Fact]
        public void G15_C_THE_SNAPSHOT_IS_TAKEN_ONCE_AND_BEFORE_ANY_POINT()
        {
            var port = G15.Rigid().NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted);
            Assert.Equal(1, port.CaptureCalls);
            Assert.Equal("capture", port.Events.First());
            Assert.True(port.Events.IndexOf("capture") < port.Events.IndexOf("pick-base"));
        }

        [Fact]
        public void G15_D_A_PURE_BLOCKER_ASKS_NO_POINT_AND_WRITES_NOTHING()
        {
            var scenario = G15.Rigid();
            scenario.Services.Authored = _ => RackProjectionAuthoredState.Divergent;
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.DoesNotContain("pick-target", port.Events);
            Assert.DoesNotContain("import", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
            Assert.Contains(port.Printed, line => line.Contains("No se pidio ningun punto"));
        }

        [Theory]
        [InlineData(LibraryAvailability.Ok, LibraryBlockPresence.BlockMissing)]
        [InlineData(LibraryAvailability.FileMissing, LibraryBlockPresence.Unknown)]
        [InlineData(LibraryAvailability.Unknown, LibraryBlockPresence.Unknown)]
        public void G15_E_A_REQUIRED_PIECE_WITHOUT_ITS_BLOCK_ASKS_NO_POINT(
            LibraryAvailability availability, LibraryBlockPresence presence)
        {
            var scenario = G15.Rigid(
                RackSystemKind.SelectiveRack,
                G14.Piece("P-1", RequirementRole.Required, "POSTE", availability, presence));
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.Equal(RackProjectionStage.Plans, result.PlanResult.FailedStage);
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
        }

        [Fact]
        public void G15_E_A_REQUIRED_PIECE_WITHOUT_A_KEY_ASKS_NO_POINT()
        {
            var scenario = G15.Rigid(
                RackSystemKind.SelectiveRack,
                G14.Piece("P-1", RequirementRole.Required, "  "));
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.Contains(result.PlanResult.Diagnostics, d => d.Code == RackProjectionFailureCode.RequiredKeyMissing);
            Assert.DoesNotContain("pick-base", port.Events);
        }

        [Fact]
        public void G15_E_AN_OPTIONAL_VISUAL_IS_A_WARNING_SHOWN_BEFORE_THE_POINT_AND_DOES_NOT_BLOCK()
        {
            var scenario = G15.Rigid(
                RackSystemKind.SelectiveRack,
                G14.Piece("P-1", RequirementRole.Required, "POSTE"),
                G14.Piece("P-2", RequirementRole.OptionalVisual, "TARIMA", LibraryAvailability.Ok, LibraryBlockPresence.BlockMissing));
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted);
            var warningAt = port.Events.IndexOf("report");
            Assert.True(warningAt >= 0 && warningAt < port.Events.IndexOf("pick-base"));
            Assert.Contains(port.Printed, line => line.Contains("aviso") && line.Contains("opcional"));
        }

        [Fact]
        public void G15_E_A_NOT_APPLICABLE_PIECE_IS_IGNORED()
        {
            var scenario = G15.Rigid(
                RackSystemKind.SelectiveRack,
                G14.Piece("P-1", RequirementRole.NotApplicable, "NADA", LibraryAvailability.FileMissing, LibraryBlockPresence.Unknown));

            var result = RackProjectionCommandRun.Execute(scenario.NewPort(G15.Points()));

            Assert.True(result.IsCompleted);
        }

        [Fact]
        public void G15_AE_FLOW_BED_IS_REJECTED_BEFORE_ANY_POINT()
        {
            var scenario = new G15.Scenario(RackSystemKind.Cama, DimensionViewKind.Lateral);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.False(result.IsCompleted);
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
        }

        [Fact]
        public void G15_A_NON_RACK_SELECTION_STOPS_THE_SNAPSHOT_WITHOUT_POINTS()
        {
            var port = G15.Rigid().NewPort(G15.Points());
            port.SnapshotToReturn = RackProjectionSnapshot.Unavailable(RackProjectionSnapshotFailure.NoRackMembers, null);

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.SnapshotFailed, result.Status);
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
        }

        // ------------------------------------------------------------ point semantics

        [Fact]
        public void G15_F_CANCEL_ON_THE_BASE_POINT_WRITES_NOTHING()
        {
            var port = G15.Rigid().NewPort(RackProjectionPick.Cancel());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Cancelled, result.Status);
            AssertNothingWritten(port);
        }

        [Fact]
        public void G15_F_CANCEL_ON_THE_TARGET_POINT_WRITES_NOTHING()
        {
            var port = G15.Rigid().NewPort(RackProjectionPick.Ok(new Point3D(0, 0, 0)), RackProjectionPick.Cancel());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Cancelled, result.Status);
            AssertNothingWritten(port);
        }

        [Fact]
        public void G15_G_NONE_ON_THE_BASE_POINT_WRITES_NOTHING_AND_DOES_NOT_ASK_AGAIN()
        {
            var port = G15.Rigid().NewPort(RackProjectionPick.None());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Cancelled, result.Status);
            Assert.Equal(1, port.Events.Count(e => e == "pick-base"));
            AssertNothingWritten(port);
        }

        [Fact]
        public void G15_G_NONE_ON_THE_TARGET_POINT_WRITES_NOTHING()
        {
            var port = G15.Rigid().NewPort(RackProjectionPick.Ok(new Point3D(0, 0, 0)), RackProjectionPick.None());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Cancelled, result.Status);
            Assert.Equal(1, port.Events.Count(e => e == "pick-target"));
            AssertNothingWritten(port);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void G15_H_A_POINT_ERROR_IS_A_FAILURE_WITH_NOTHING_WRITTEN(bool onBase)
        {
            var picks = onBase
                ? new[] { RackProjectionPick.Error("PROMPT_STATUS_Error") }
                : new[] { RackProjectionPick.Ok(new Point3D(0, 0, 0)), RackProjectionPick.Error("PROMPT_STATUS_Error") };
            var port = G15.Rigid().NewPort(picks);

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.PointFailed, result.Status);
            AssertNothingWritten(port);
            Assert.Contains(port.Printed, line => line.Contains("PROMPT_STATUS_Error"));
        }

        // ------------------------------------------------------------ after the points

        [Fact]
        public void G15_I_THE_WRITE_STARTS_ONLY_AFTER_THE_PLAN_THE_POINTS_AND_THE_IMPORT()
        {
            var port = G15.Rigid().NewPort(G15.Points());

            RackProjectionCommandRun.Execute(port);

            var order = new[] { "capture", "pick-base", "pick-target", "before-write", "import", "begin" }
                .Select(name => port.Events.IndexOf(name)).ToArray();
            Assert.All(order, index => Assert.True(index >= 0));
            Assert.Equal(order.OrderBy(x => x), order);
        }

        [Fact]
        public void G15_M_N_ONE_SCOPE_ONE_COMMIT_FOR_MANY_RACKS_AND_VIEWS()
        {
            var port = G15.Rigid().NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted);
            Assert.Equal(1, port.Scopes.Begun);
            Assert.Equal(1, port.Scopes.Commits);
            Assert.Equal(2, port.Scopes.Created.Count);
            Assert.Equal(2, port.Scopes.Placed.Count);
            Assert.Equal("dispose:committed", port.Events.Last(e => e.StartsWith("dispose", StringComparison.Ordinal)));
        }

        [Fact]
        public void G15_Q_R_EACH_DEFINITION_IS_CREATED_BEFORE_ITS_REFERENCE_AND_THE_REFERENCE_IS_PLACED_BY_THE_RUN()
        {
            var port = G15.Rigid().NewPort(G15.Points());

            RackProjectionCommandRun.Execute(port);

            var writes = port.Events.Where(e => e.StartsWith("create:") || e.StartsWith("place:")).ToList();
            Assert.Equal(
                new[] { "create:" + G14.RackA, "place:REF-1", "create:" + G14.RackB, "place:REF-2" },
                writes);
        }

        [Fact]
        public void G15_O_A_FAILURE_OF_THE_DEFINITION_CREATOR_NEVER_COMMITS()
        {
            var port = G15.Rigid().NewPort(G15.Points());
            port.Scopes.OnCreate = view => view.RackId == G14.RackB
                ? RackProjectionDefinitionResult.Failed(RackProjectionWriteFailure.DefinitionCreationFailed, "WriteFailed")
                : RackProjectionDefinitionResult.Created(view.BaseName, view.RackId);

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.MaterializationFailed, result.Status);
            Assert.Equal(0, port.Scopes.Commits);
            Assert.Equal("dispose:rolledback", port.Events.Last(e => e.StartsWith("dispose", StringComparison.Ordinal)));
            Assert.Equal(1, port.Scopes.Placed.Count); // nothing after the failure, no later group
            Assert.Contains(port.Printed, line => line.Contains("deshizo la operacion completa"));
        }

        [Fact]
        public void G15_P_AN_INCOMPLETE_DEFINITION_FROM_THE_FAMILY_NEVER_COMMITS()
        {
            var port = G15.Rigid().NewPort(G15.Points());
            port.Scopes.OnCreate = view => RackProjectionDefinitionResult.Failed(
                RackProjectionWriteFailure.DefinitionIncomplete, "MissingLibraryBlocks");

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.MaterializationFailed, result.Status);
            Assert.Equal(RackProjectionWriteFailure.DefinitionIncomplete, result.Materialization.Failure);
            Assert.Equal(0, port.Scopes.Commits);
            Assert.Empty(port.Scopes.Placed);
        }

        [Fact]
        public void G15_O_A_FAILED_REFERENCE_NEVER_COMMITS_AND_STOPS_LATER_GROUPS()
        {
            var port = G15.Rigid().NewPort(G15.Points());
            port.Scopes.OnPlace = placement => RackProjectionReferenceResult.Failed("locked layer");

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionWriteFailure.ReferencePlacementFailed, result.Materialization.Failure);
            Assert.Equal(0, port.Scopes.Commits);
            Assert.Single(port.Scopes.Created);
        }

        [Fact]
        public void G15_O_AN_EXCEPTION_INSIDE_THE_SCOPE_ROLLS_BACK()
        {
            var port = G15.Rigid().NewPort(G15.Points());
            port.Scopes.OnCreate = view => throw new InvalidOperationException("boom");

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionWriteFailure.UnexpectedException, result.Materialization.Failure);
            Assert.Equal(0, port.Scopes.Commits);
        }

        [Fact]
        public void G15_A_MISSING_REQUIRED_BLOCK_AFTER_THE_IMPORT_WRITES_NOTHING_OF_THE_RACKS()
        {
            var port = G15.Rigid().NewPort(G15.Points());
            port.Observe = requirements => new RackProjectionLibraryObservation(
                requirements.Select(r => new LibraryBlockAvailabilityFact(r, LibraryBlockAvailability.Missing)).ToList(),
                false);

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.PrerequisitesFailed, result.Status);
            Assert.Contains("import", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
            Assert.Contains(port.Printed, line => line.Contains("faltan bloques"));
        }

        [Fact]
        public void G15_A_A_LIBRARY_FILE_THAT_VANISHED_NAMES_ITS_OWN_CAUSE()
        {
            var port = G15.Rigid().NewPort(G15.Points());
            port.Observe = requirements => new RackProjectionLibraryObservation(
                Array.Empty<LibraryBlockAvailabilityFact>(), true);

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.PrerequisitesFailed, result.Status);
            Assert.Contains(port.Printed, line => line.Contains("biblioteca no disponible"));
            Assert.Equal(0, port.Scopes.Begun);
        }

        [Fact]
        public void G15_A_THE_IMPORT_DOES_NOT_PROVE_A_REQUIREMENT_BY_ITSELF()
        {
            var port = G15.Rigid().NewPort(G15.Points());
            port.Observe = requirements => new RackProjectionLibraryObservation(
                Array.Empty<LibraryBlockAvailabilityFact>(), false); // an observation that says nothing

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.PrerequisitesFailed, result.Status);
        }

        [Fact]
        public void G15_A_AN_OPTIONAL_VISUAL_MISSING_AFTER_THE_IMPORT_DOES_NOT_BLOCK()
        {
            var scenario = G15.Rigid(
                RackSystemKind.SelectiveRack,
                G14.Piece("P-1", RequirementRole.Required, "POSTE"),
                G14.Piece("P-2", RequirementRole.OptionalVisual, "TARIMA"));
            var port = scenario.NewPort(G15.Points());
            port.Observe = requirements => new RackProjectionLibraryObservation(
                requirements
                    .Select(r => new LibraryBlockAvailabilityFact(
                        r, r.Key == "TARIMA" ? LibraryBlockAvailability.Missing : LibraryBlockAvailability.Found))
                    .ToList(),
                false);

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted);
        }

        // ------------------------------------------------------------ identity and geometry

        [Fact]
        public void G15_S_EVERY_PROJECTED_VIEW_KEEPS_THE_RACKID_OF_ITS_SOURCE()
        {
            var port = G15.Rigid().NewPort(G15.Points());

            RackProjectionCommandRun.Execute(port);

            Assert.Equal(new[] { G14.RackA, G14.RackB }, port.Scopes.Created.Select(v => v.RackId).ToArray());
            Assert.All(port.Scopes.Created, view => Assert.Equal(view.RackId, view.Envelope.Id));
            Assert.All(port.Scopes.Placed, placement =>
                Assert.Contains(placement.RackId, new[] { G14.RackA, G14.RackB }));
        }

        [Fact]
        public void G15_S_THE_LOGICAL_RACK_COUNT_DOES_NOT_CHANGE()
        {
            var port = G15.Rigid().NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            var before = result.PlanResult.Plan.Groups.Select(g => g.RackId).Distinct().Count();
            var after = port.Scopes.Created.Select(v => v.Envelope.Id).Distinct().Count();
            Assert.Equal(before, after);
        }

        [Fact]
        public void G15_U_ONE_COMMON_TRANSFORM_SERVES_EVERY_RACK()
        {
            var port = G15.Rigid().NewPort(G15.Points(0, 0, 500, 300));

            RackProjectionCommandRun.Execute(port);

            // Rigid, alpha = 0: every placement is its source translated by the SAME vector (T - B) = (500, 300).
            var a = port.Scopes.Placed.Single(p => p.PhysicalKey == "REF-1");
            var b = port.Scopes.Placed.Single(p => p.PhysicalKey == "REF-2");
            Assert.Equal(500.0, a.Position.X, 9);
            Assert.Equal(300.0, a.Position.Y, 9);
            Assert.Equal(500.0, b.Position.X, 9);
            Assert.Equal(400.0, b.Position.Y, 9);
            Assert.Equal(a.RotationRadians, b.RotationRadians, 9);
        }

        [Fact]
        public void G15_V_A_SOURCE_WITH_A_DEFINITION_ORIGIN_IS_PLACED_AT_THE_TRANSLATED_ANCHOR_WITHOUT_THE_ORIGIN_OFFSET()
        {
            var plain = G15.Rigid();
            var shifted = G15.Rigid();
            var shiftedScenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Planta);
            shiftedScenario.AddRack(G14.RackA, "REF-1", 0, 0, originX: 12.0);
            shiftedScenario.AddRack(G14.RackB, "REF-2", 0, 100, originX: 12.0);

            var plainPort = plain.NewPort(G15.Points());
            var shiftedPort = shiftedScenario.NewPort(G15.Points());
            RackProjectionCommandRun.Execute(plainPort);
            var result = RackProjectionCommandRun.Execute(shiftedPort);

            Assert.True(result.IsCompleted);
            var first = shiftedPort.Scopes.Placed.Single(p => p.PhysicalKey == "REF-1");
            var reference = plainPort.Scopes.Placed.Single(p => p.PhysicalKey == "REF-1");
            // The source anchor moves with M_r; the target anchor is the translated source anchor, so the new view
            // (definition Origin = 0) sits displaced by the origin exactly once, along the source X.
            Assert.Equal(reference.Position.Y, first.Position.Y, 9);
            Assert.NotEqual(reference.Position.X, first.Position.X);
        }

        [Fact]
        public void G15_W_THE_PROJECTED_VIEW_IS_PLACED_ABOUT_A_TARGET_DEFINITION_WHOSE_ORIGIN_IS_ZERO()
        {
            // The placement value carries a position and a rotation only: nothing of the source DefinitionOrigin can be
            // baked into the definition, because a definition has no offset in the prepared view.
            var members = typeof(RackProjectionPreparedView).GetProperties().Select(p => p.Name).ToArray();
            Assert.DoesNotContain(members, name => name.IndexOf("Origin", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public void G15_X_THE_SAME_CLASS_IS_PROJECTED_RIGID_WITH_A_ZERO_ANGLE()
        {
            var port = G15.Rigid().NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionMode.Rigid, result.PlanResult.Plan.Mode);
            Assert.All(port.Scopes.Placed, placement => Assert.Equal(0.0, placement.RotationRadians, 9));
        }

        [Fact]
        public void G15_Y_A_DIFFERENT_CLASS_IS_PROJECTED_ORTHOGRAPHIC_WITH_ONE_COMMON_DIRECTION()
        {
            var port = G15.Orthographic().NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted);
            Assert.Equal(RackProjectionMode.Orthographic, result.PlanResult.Plan.Mode);
            Assert.Equal(2, port.Scopes.Placed.Count);
            Assert.All(port.Scopes.Placed, p => Assert.Equal(DimensionViewKind.Frontal, p.TargetAddress.Kind));
            Assert.Equal(1, port.Scopes.Commits);
        }

        // ------------------------------------------------------------ families

        [Theory]
        [InlineData(RackSystemKind.SelectiveRack, RackProjectionMaterializationFamily.HeaderRun)]
        [InlineData(RackSystemKind.PalletFlow, RackProjectionMaterializationFamily.HeaderRun)]
        [InlineData(RackSystemKind.PushBack, RackProjectionMaterializationFamily.HeaderRun)]
        [InlineData(RackSystemKind.Selective, RackProjectionMaterializationFamily.HeaderRun)]
        [InlineData(RackSystemKind.Cantilever, RackProjectionMaterializationFamily.Cantilever)]
        [InlineData(RackSystemKind.Cama, RackProjectionMaterializationFamily.Unsupported)]
        public void G15_FAMILY_EVERY_EXPOSED_KIND_MAPS_TO_AN_EXISTING_AUTH15_FAMILY(
            RackSystemKind kind, RackProjectionMaterializationFamily expected)
        {
            Assert.Equal(expected, RackProjectionMaterializationFamilies.Of(kind));
        }

        [Theory]
        [InlineData(RackSystemKind.PushBack)]
        [InlineData(RackSystemKind.Cantilever)]
        [InlineData(RackSystemKind.Selective)]
        [InlineData(RackSystemKind.PalletFlow)]
        [InlineData(RackSystemKind.SelectiveRack)]
        public void G15_Z_AA_AB_AC_AD_EVERY_KIND_PROJECTS_THROUGH_ITS_OWN_FAMILY(RackSystemKind kind)
        {
            var port = G15.Rigid(kind).NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            Assert.Equal(1, port.Scopes.Commits);
            Assert.All(port.Scopes.Created, view =>
            {
                Assert.Equal(RackProjectionMaterializationFamilies.Of(kind), view.Family);
                Assert.Equal(view.Family == RackProjectionMaterializationFamily.HeaderRun, view.HeaderPlan != null);
                Assert.Equal(view.Family == RackProjectionMaterializationFamily.Cantilever, view.CantileverPlan != null);
            });
        }

        [Fact]
        public void G15_A_PREPARED_VIEW_WITH_A_FOREIGN_RACKID_IS_REFUSED()
        {
            var envelope = G15.Envelope(G14.RackB, RackSystemKind.SelectiveRack);
            Assert.Throws<ArgumentException>(() => new RackProjectionPreparedView(
                G14.RackA, RackSystemKind.SelectiveRack, G14.Planta, "n", envelope,
                new RackCad.Application.Drawing.HeaderRunPlan(
                    Array.Empty<RackCad.Application.Drawing.HeaderGroup>(),
                    Array.Empty<RackCad.Application.Drawing.HeaderBlockInstance>()),
                null));
        }

        // ------------------------------------------------------------ single authority per RackId

        [Fact]
        public void G15_AF_AG_RESOLVE_AND_PREPARE_RUN_BEFORE_THE_POINTS_AND_NEVER_AGAIN()
        {
            var scenario = G15.Rigid();
            var port = scenario.NewPort(G15.Points());
            scenario.Services.EditPreflight = rackId => RackProjectionEditPreflight.Accepted;

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted);
            Assert.Equal(2, scenario.Services.ResolveCalls.Count);
            Assert.Equal(2, scenario.Services.ResolveCalls.Distinct().Count());
            Assert.Equal(2, scenario.Services.PrepareCalls.Count);
        }

        // ------------------------------------------------------------ no partial-batch semantics

        [Fact]
        public void G15_AH_THE_COMMAND_HAS_NO_PARTIAL_BATCH_STATUS()
        {
            var names = Enum.GetNames(typeof(RackProjectionCommandStatus))
                .Concat(Enum.GetNames(typeof(RackProjectionMaterializationStatus)))
                .ToArray();

            Assert.DoesNotContain(names, name => name.IndexOf("Partial", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.Equal(new[] { "Committed", "RolledBack" }, Enum.GetNames(typeof(RackProjectionMaterializationStatus)));
        }

        [Fact]
        public void G15_AH_A_COMMITTED_OPERATION_REPORTS_LINKED_VIEWS_NOT_COPIES()
        {
            var port = G15.Rigid().NewPort(G15.Points());

            RackProjectionCommandRun.Execute(port);

            Assert.Contains(port.Printed, line => line.Contains("vistas enlazadas") && line.Contains("no copias")
                && line.Contains("RACKDUPLICAR"));
        }

        private static void AssertNothingWritten(G15.Port port)
        {
            Assert.Equal(0, port.Scopes.Begun);
            Assert.Equal(0, port.Scopes.Commits);
            Assert.Empty(port.Scopes.Created);
            Assert.Empty(port.Scopes.Placed);
            Assert.DoesNotContain("import", port.Events);
            Assert.DoesNotContain("before-write", port.Events);
        }
    }
}
