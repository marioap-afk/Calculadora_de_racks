using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G16 C16-06 at the level of the command run (G14 plan + G15 run, AutoCAD faked by the write scope). Selective rack family
    /// frames (Planta: X = Depth, Y = Run; Frontal: X = Run; Lateral: X = Depth). Frozen design and obligations O-2..O-8:
    /// docs/initiatives/I-55-c16-06-projected-orientation.md. Only the placement (position + rotation of the BlockReference) changes
    /// between modes; the prepared view (definition plan, envelope, BaseName) never does.
    /// </summary>
    public class G16ProjectedOrientationTests
    {
        public static IEnumerable<object[]> Rotations() => new[]
        {
            new object[] { 0.0 }, new object[] { 90.0 }, new object[] { 180.0 }, new object[] { 270.0 },
        };

        // ================================================================ Projected, orthographic (P-01..P-16)

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_06_P_PlantaToFrontalTurnsTheReferenceSoItsRunFollowsThePlanta(double degrees)
            => AssertProjected(DimensionViewKind.Planta, G14.Planta, DimensionViewKind.Frontal, degrees, degrees + 90.0);

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_06_P_PlantaToLateralTurnsTheReferenceSoItsDepthFollowsThePlanta(double degrees)
            => AssertProjected(DimensionViewKind.Planta, G14.Planta, DimensionViewKind.Lateral, degrees, degrees);

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_06_P_FrontalToPlantaTurnsTheReferenceSoItsRunFollowsTheFrontal(double degrees)
            => AssertProjected(DimensionViewKind.Frontal, G14.Frontal0, DimensionViewKind.Planta, degrees, degrees - 90.0);

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_06_P_LateralToPlantaTurnsTheReferenceSoItsDepthFollowsTheLateral(double degrees)
            => AssertProjected(DimensionViewKind.Lateral, G14.Lateral0, DimensionViewKind.Planta, degrees, degrees);

        // ================================================================ P-17, P-18: several racks

        [Fact]
        public void C16_06_P17_CompatibleRacksShareOneRotationAndLieOnOneProjectionLine()
        {
            var scenario = Scenario(DimensionViewKind.Frontal, G14.Planta, RackProjectionOrientationMode.Projected);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(90.0));
            scenario.AddRack(G14.RackB, "REF-2", -150, 30, rotation: Radians(90.0));
            var port = scenario.NewPort(G15.Points(0, 0, 500, 300));

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            Assert.Equal(2, port.Scopes.Placed.Count);
            Assert.All(port.Scopes.Placed, p => Assert.Equal(Normalize(Radians(180.0)), Normalize(p.RotationRadians), 9));
            // Planta at 90° has its Run along -X: the projected frontals lie on the horizontal line through the target point.
            Assert.All(port.Scopes.Placed, p => Assert.Equal(300.0, p.Position.Y, 6));
        }

        [Fact]
        public void C16_06_P18_OppositeRacksAreRefusedBeforeAnyPointImportOrWrite()
        {
            var scenario = Scenario(DimensionViewKind.Frontal, G14.Planta, RackProjectionOrientationMode.Projected);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(0.0));
            scenario.AddRack(G14.RackB, "REF-2", 200, 0, rotation: Radians(180.0));
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.Equal(RackProjectionStage.Validate, result.PlanResult.FailedStage);
            Assert.All(result.PlanResult.Diagnostics, d => Assert.Equal(RackProjectionFailureCode.SourceOrientationDivergent, d.Code));
            Assert.Equal(
                new[] { G14.RackA, G14.RackB },
                result.PlanResult.Diagnostics.Select(d => d.RackId).Distinct().OrderBy(x => x));
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.DoesNotContain("import", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
            Assert.Contains(port.Printed, line => line.Contains("SourceOrientationDivergent"));
        }

        // ================================================================ Canonical (C-01..C-04)

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_06_C_CanonicalOrthographicIsExactlyTheFrozenG14Result(double degrees)
        {
            foreach (var (sourceKind, sourceAddress, target) in new[]
            {
                (DimensionViewKind.Planta, G14.Planta, DimensionViewKind.Frontal),
                (DimensionViewKind.Planta, G14.Planta, DimensionViewKind.Lateral),
                (DimensionViewKind.Frontal, G14.Frontal0, DimensionViewKind.Planta),
                (DimensionViewKind.Lateral, G14.Lateral0, DimensionViewKind.Planta),
            })
            {
                var canonical = Run(sourceAddress, target, degrees, RackProjectionOrientationMode.Canonical);

                Assert.True(canonical.Result.IsCompleted, sourceKind + " -> " + target + ": " + string.Join(" | ", canonical.Port.Printed));
                var placed = Assert.Single(canonical.Port.Scopes.Placed);
                Assert.Equal(0.0, placed.RotationRadians, 12);
                Assert.Equal(0.0, canonical.Result.PlanResult.Plan.Orthographic.TargetRotationRadians, 12);
            }
        }

        [Fact]
        public void C16_06_C_CanonicalIsNotRefusedForADivergenceThatOnlyProjectedHas()
        {
            var scenario = Scenario(DimensionViewKind.Frontal, G14.Planta, RackProjectionOrientationMode.Canonical);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(0.0));
            scenario.AddRack(G14.RackB, "REF-2", 200, 0, rotation: Radians(180.0));

            var result = RackProjectionCommandRun.Execute(scenario.NewPort(G15.Points()));

            Assert.True(result.IsCompleted);
        }

        // ================================================================ same class

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_06_SameClassProjectedKeepsTheRotationOfTheSource(double degrees)
        {
            var projected = Run(G14.Planta, DimensionViewKind.Planta, degrees, RackProjectionOrientationMode.Projected);

            Assert.Equal(RackProjectionMode.Rigid, projected.Result.PlanResult.Plan.Mode);
            Assert.Equal(Normalize(Radians(degrees)), Normalize(Assert.Single(projected.Port.Scopes.Placed).RotationRadians), 9);
        }

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_06_SameClassCanonicalIsACopyThatLandsTheBasePointOnTheTargetUnrotated(double degrees)
        {
            // Base point = the source insertion point (10, 20): one CommonTransform2D with alpha = -theta brings it onto the target.
            var scenario = Scenario(DimensionViewKind.Planta, G14.Planta, RackProjectionOrientationMode.Canonical);
            scenario.AddRack(G14.RackA, "REF-1", 10, 20, rotation: Radians(degrees));
            var port = scenario.NewPort(G15.Points(10, 20, 500, 300));

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            var placed = Assert.Single(port.Scopes.Placed);
            Assert.Equal(0.0, Normalize(placed.RotationRadians), 9);
            Assert.Equal(500.0, placed.Position.X, 6);
            Assert.Equal(300.0, placed.Position.Y, 6);
        }

        [Fact]
        public void C16_06_SameClassCanonicalKeepsTheLayoutOfARotatedGroup()
        {
            // Two plantas at 90° side by side with an aisle: after Canonical both are at 0° and their layout is the same rigid image.
            var scenario = Scenario(DimensionViewKind.Planta, G14.Planta, RackProjectionOrientationMode.Canonical);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(90.0));
            scenario.AddRack(G14.RackB, "REF-2", 0, 100, rotation: Radians(90.0));
            var port = scenario.NewPort(G15.Points(0, 0, 1000, 0));

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            var a = port.Scopes.Placed.Single(p => p.PhysicalKey == "REF-1");
            var b = port.Scopes.Placed.Single(p => p.PhysicalKey == "REF-2");
            Assert.All(port.Scopes.Placed, p => Assert.Equal(0.0, Normalize(p.RotationRadians), 9));
            Assert.Equal(1000.0, a.Position.X, 6);
            Assert.Equal(0.0, a.Position.Y, 6);
            // R(-90°)·(0, 100) = (100, 0): rack B stays 100 away from rack A, now along X.
            Assert.Equal(1100.0, b.Position.X, 6);
            Assert.Equal(0.0, b.Position.Y, 6);
        }

        [Fact]
        public void C16_06_SameClassCanonicalRefusesRacksWithDifferentRotationsBeforeAnyPoint()
        {
            var scenario = Scenario(DimensionViewKind.Planta, G14.Planta, RackProjectionOrientationMode.Canonical);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(0.0));
            scenario.AddRack(G14.RackB, "REF-2", 0, 200, rotation: Radians(180.0));
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.All(result.PlanResult.Diagnostics, d => Assert.Equal(RackProjectionFailureCode.SourceRotationsDiffer, d.Code));
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
        }

        [Fact]
        public void C16_06_SameClassProjectedCopiesAGroupWithDifferentRotations()
        {
            var scenario = Scenario(DimensionViewKind.Planta, G14.Planta, RackProjectionOrientationMode.Projected);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(0.0));
            scenario.AddRack(G14.RackB, "REF-2", 0, 200, rotation: Radians(180.0));
            var port = scenario.NewPort(G15.Points(0, 0, 500, 300));

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted);
            Assert.Equal(0.0, Normalize(port.Scopes.Placed.Single(p => p.PhysicalKey == "REF-1").RotationRadians), 9);
            Assert.Equal(Normalize(Math.PI), Normalize(port.Scopes.Placed.Single(p => p.PhysicalKey == "REF-2").RotationRadians), 9);
        }

        // ================================================================ parity of the Projected path with the frozen command contract

        [Theory]
        [InlineData(RackProjectionOrientationMode.Projected)]
        [InlineData(RackProjectionOrientationMode.Canonical)]
        public void C16_06_PerpendicularSourcesAreNonParallelInBothModes(RackProjectionOrientationMode mode)
        {
            var scenario = Scenario(DimensionViewKind.Frontal, G14.Planta, mode);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(0.0));
            scenario.AddRack(G14.RackB, "REF-2", 300, 0, rotation: Radians(90.0));

            var result = RackProjectionCommandRun.Execute(scenario.NewPort(G15.Points()));

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.All(result.PlanResult.Diagnostics, d => Assert.Equal(RackProjectionFailureCode.NonParallelSources, d.Code));
        }

        [Fact]
        public void C16_06_ProjectedWarnsAboutOverlapsBeforeAnyPoint()
        {
            // Two plantas at 90° whose runs cover the same interval along the common line: their frontals overlap.
            var scenario = Scenario(DimensionViewKind.Frontal, G14.Planta, RackProjectionOrientationMode.Projected);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(90.0));
            scenario.AddRack(G14.RackB, "REF-2", 0, 200, rotation: Radians(90.0));
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted);
            Assert.Contains(result.PlanResult.Warnings, w => w.Code == RackProjectionWarningCode.Overlap);
            var report = port.Events.IndexOf("report");
            Assert.True(report >= 0 && report < port.Events.IndexOf("pick-base"));
        }

        [Fact]
        public void C16_06_ProjectedWritesOnceAndCancellingWritesNothing()
        {
            var ok = Scenario(DimensionViewKind.Frontal, G14.Planta, RackProjectionOrientationMode.Projected);
            ok.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(90.0));
            var okPort = ok.NewPort(G15.Points());
            Assert.True(RackProjectionCommandRun.Execute(okPort).IsCompleted);
            Assert.Equal(1, okPort.Scopes.Begun);
            Assert.Equal(1, okPort.Scopes.Commits);

            var cancel = Scenario(DimensionViewKind.Frontal, G14.Planta, RackProjectionOrientationMode.Projected);
            cancel.AddRack(G14.RackA, "REF-1", 0, 0, rotation: Radians(90.0));
            var cancelPort = cancel.NewPort(RackProjectionPick.Cancel());
            Assert.Equal(RackProjectionCommandStatus.Cancelled, RackProjectionCommandRun.Execute(cancelPort).Status);
            Assert.Equal(0, cancelPort.Scopes.Begun);
            Assert.DoesNotContain("import", cancelPort.Events);
        }

        // ================================================================ O-8: only the placement changes

        [Theory]
        [InlineData(DimensionViewKind.Frontal)]
        [InlineData(DimensionViewKind.Lateral)]
        [InlineData(DimensionViewKind.Planta)]
        public void C16_06_TheModeNeverChangesTheDefinitionPlanTheEnvelopeOrTheBaseName(DimensionViewKind target)
        {
            var projected = Run(G14.Planta, target, 90.0, RackProjectionOrientationMode.Projected);
            var canonical = Run(G14.Planta, target, 90.0, RackProjectionOrientationMode.Canonical);

            var a = Assert.Single(projected.Port.Scopes.Created);
            var b = Assert.Single(canonical.Port.Scopes.Created);
            Assert.Equal(b.BaseName, a.BaseName);
            Assert.Equal(b.TargetAddress, a.TargetAddress);
            Assert.Equal(b.RackId, a.RackId);
            Assert.Equal(b.Envelope.Id, a.Envelope.Id);
            Assert.Equal(b.Envelope.View, a.Envelope.View);
            Assert.Equal(b.Envelope.Section, a.Envelope.Section);
            Assert.Equal(b.Envelope.Design, a.Envelope.Design);
        }

        [Theory]
        [InlineData(RackProjectionOrientationMode.Projected, "Proyectada")]
        [InlineData(RackProjectionOrientationMode.Canonical, "Predeterminada")]
        public void C16_06_TheReportNamesTheOrientationUsed(RackProjectionOrientationMode mode, string word)
        {
            var run = Run(G14.Planta, DimensionViewKind.Frontal, 0.0, mode);

            Assert.True(run.Result.IsCompleted);
            Assert.Contains(run.Port.Printed, line => line.Contains("Orientacion " + word));
        }

        // ================================================================ helpers

        private static void AssertProjected(
            DimensionViewKind sourceKind, RackViewAddress source, DimensionViewKind target, double degrees, double expectedDegrees)
        {
            var run = Run(source, target, degrees, RackProjectionOrientationMode.Projected);

            Assert.True(run.Result.IsCompleted, sourceKind + " -> " + target + " at " + degrees + ": " + string.Join(" | ", run.Port.Printed));
            var plan = run.Result.PlanResult.Plan;
            Assert.Equal(RackProjectionMode.Orthographic, plan.Mode);
            var placed = Assert.Single(run.Port.Scopes.Placed);
            Assert.Equal(Normalize(Radians(expectedDegrees)), Normalize(placed.RotationRadians), 9);

            // O-2: a pure projection along the source axis: e = w = d, alpha = 0.
            var view = plan.Views.Single();
            RackViewFrameSemantics.TryAxisDirection(view.SourceFrame, plan.ConservedAxis, out var s);
            var d = view.Placement.Linear.Apply(s).Normalized();
            Assert.Equal(d.X, plan.Orthographic.CommonDirection.X, 9);
            Assert.Equal(d.Y, plan.Orthographic.CommonDirection.Y, 9);
            Assert.Equal(d.X, plan.Orthographic.TargetDirection.X, 9);
            Assert.Equal(d.Y, plan.Orthographic.TargetDirection.Y, 9);
            Assert.Equal(0.0, Normalize(plan.Orthographic.AlphaRadians), 9);
        }

        private static (RackProjectionCommandResult Result, G15.Port Port) Run(
            RackViewAddress source, DimensionViewKind target, double degrees, RackProjectionOrientationMode mode)
        {
            var scenario = Scenario(target, source, mode);
            scenario.AddRack(G14.RackA, "REF-1", 10, 20, rotation: Radians(degrees));
            var port = scenario.NewPort(G15.Points(0, 0, 500, 300));
            return (RackProjectionCommandRun.Execute(port), port);
        }

        private static G15.Scenario Scenario(DimensionViewKind target, RackViewAddress source, RackProjectionOrientationMode mode)
            => new G15.Scenario(RackSystemKind.SelectiveRack, target) { SourceAddress = source, Orientation = mode };

        private static Point2D Anchor(RackProjectedPlacement placement, Point2D local)
        {
            var rotated = Transform2D.Rotation(placement.RotationRadians).Apply(new Vector2D(local.X, local.Y));
            return new Point2D(placement.Position.X + rotated.X, placement.Position.Y + rotated.Y);
        }

        private static double Radians(double degrees) => degrees * Math.PI / 180.0;

        private static double Normalize(double radians)
        {
            var twoPi = 2.0 * Math.PI;
            var value = radians % twoPi;
            if (value < 0.0) value += twoPi;
            return Math.Abs(value - twoPi) < 1e-9 ? 0.0 : value;
        }
    }
}
