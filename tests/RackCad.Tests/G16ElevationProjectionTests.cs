using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;
using Authority = RackCad.Tests.G16OrientationAuthorityTests;

namespace RackCad.Tests
{
    /// <summary>
    /// G16 C16-07: RACKPROYECTAR between the two elevations (Frontal ↔ Lateral). The Owner revoked the OD-7.a A exclusion after
    /// Candidate 68115269 stopped a Selective Frontal → Lateral at AVAILABLE / PairNotExposed. The two elevations share only the
    /// Height axis (AUTH-05: Frontal = Run × Height, Lateral = Depth × Height, +Y local, floor at 0), so the pair is orthographic on
    /// Height and the C16-06 authority derives rho = theta without any table. Frozen design and obligations Q-1..Q-8:
    /// docs/initiatives/I-55-c16-07-elevation-projection.md.
    /// </summary>
    public class G16ElevationProjectionTests
    {
        public static IEnumerable<object[]> Rotations() => new[]
        {
            new object[] { 0.0 }, new object[] { 90.0 }, new object[] { 180.0 }, new object[] { 270.0 },
        };

        public static IEnumerable<object[]> Elevations()
        {
            foreach (var system in new[] { "Selective", "Dynamic", "PushBack", "Cantilever" })
            {
                foreach (var (source, target) in new[]
                {
                    (DimensionViewKind.Frontal, DimensionViewKind.Lateral),
                    (DimensionViewKind.Lateral, DimensionViewKind.Frontal),
                })
                {
                    foreach (var degrees in new[] { 0.0, 90.0, 180.0, 270.0 })
                    {
                        yield return new object[] { system, source, target, degrees };
                    }
                }
            }
        }

        // ================================================================ Q-1: the closed pair policy

        [Theory]
        [InlineData(DimensionViewKind.Frontal, DimensionViewKind.Lateral)]
        [InlineData(DimensionViewKind.Lateral, DimensionViewKind.Frontal)]
        public void C16_07_Q1_TheElevationsAreOrthographicOnTheOnlyAxisTheyShare(DimensionViewKind source, DimensionViewKind target)
        {
            Assert.True(RackProjectionClassMapping.TryMap(source, target, out var mode, out var axis));
            Assert.Equal(RackProjectionMode.Orthographic, mode);
            Assert.Equal(RackPhysicalAxis.Height, axis);
        }

        [Fact]
        public void C16_07_Q1_TheRestOfThePairTableIsUnchanged()
        {
            foreach (var kind in new[] { DimensionViewKind.Frontal, DimensionViewKind.Lateral, DimensionViewKind.Planta })
            {
                Assert.True(RackProjectionClassMapping.TryMap(kind, kind, out var mode, out _));
                Assert.Equal(RackProjectionMode.Rigid, mode);
            }

            Assert.True(RackProjectionClassMapping.TryMap(DimensionViewKind.Planta, DimensionViewKind.Frontal, out _, out var run));
            Assert.Equal(RackPhysicalAxis.Run, run);
            Assert.True(RackProjectionClassMapping.TryMap(DimensionViewKind.Lateral, DimensionViewKind.Planta, out _, out var depth));
            Assert.Equal(RackPhysicalAxis.Depth, depth);
        }

        // ================================================================ Q-2: the Owner's observation, then acceptance

        [Theory]
        [InlineData(DimensionViewKind.Lateral)]
        [InlineData(DimensionViewKind.Frontal)]
        public void C16_07_Q2_SelectiveElevationToElevationIsNoLongerPairNotExposed(DimensionViewKind target)
        {
            var source = target == DimensionViewKind.Lateral ? G14.Frontal0 : G14.Lateral0;
            var run = Run(source, target, 0.0, RackProjectionOrientationMode.Projected);

            Assert.True(run.Result.IsCompleted, Explain(run));
            Assert.DoesNotContain(run.Result.PlanResult.Diagnostics, d => d.Code == RackProjectionFailureCode.PairNotExposed);
            var created = Assert.Single(run.Port.Scopes.Created);
            Assert.Equal(G14.RackA, created.RackId);
            Assert.Equal(target, created.TargetAddress.Kind);
            Assert.Single(run.Port.Scopes.Placed);
            Assert.Equal(1, run.Port.Scopes.Begun);
            Assert.Equal(1, run.Port.Scopes.Commits);
        }

        // ================================================================ Q-3: Projected, the minimum matrix of the Owner

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_07_Q3_ProjectedFrontalToLateralKeepsTheSourceRotation(double degrees)
            => AssertProjected(G14.Frontal0, DimensionViewKind.Lateral, degrees);

        [Theory]
        [MemberData(nameof(Rotations))]
        public void C16_07_Q3_ProjectedLateralToFrontalKeepsTheSourceRotation(double degrees)
            => AssertProjected(G14.Lateral0, DimensionViewKind.Frontal, degrees);

        // ================================================================ Q-4: Canonical, representative rows

        [Theory]
        [InlineData(DimensionViewKind.Lateral, 0.0, 500.0, 320.0)]
        [InlineData(DimensionViewKind.Lateral, 90.0, 500.0, 290.0)]
        [InlineData(DimensionViewKind.Frontal, 0.0, 500.0, 320.0)]
        [InlineData(DimensionViewKind.Frontal, 90.0, 500.0, 290.0)]
        public void C16_07_Q4_CanonicalPlacesTheNormalPresentationOnTheCommonHeightLine(
            DimensionViewKind target, double degrees, double x, double y)
        {
            var source = target == DimensionViewKind.Lateral ? G14.Frontal0 : G14.Lateral0;
            var run = Run(source, target, degrees, RackProjectionOrientationMode.Canonical);

            Assert.True(run.Result.IsCompleted, Explain(run));
            var placed = Assert.Single(run.Port.Scopes.Placed);
            Assert.Equal(0.0, placed.RotationRadians, 12);
            Assert.Equal(0.0, run.Result.PlanResult.Plan.Orthographic.TargetRotationRadians, 12);
            // The floor anchor of the new view sits on the target point, moved along +Y (its Height) by the floor coordinate of the
            // source over the common line (base point at the origin; source floor anchor at (10, 20)).
            Assert.Equal(x, placed.Position.X, 6);
            Assert.Equal(y, placed.Position.Y, 6);
        }

        [Theory]
        [InlineData(DimensionViewKind.Lateral)]
        [InlineData(DimensionViewKind.Frontal)]
        public void C16_07_Q4_TheModeOnlyChangesTheReferenceNotThePreparedView(DimensionViewKind target)
        {
            var source = target == DimensionViewKind.Lateral ? G14.Frontal0 : G14.Lateral0;
            var projected = Run(source, target, 90.0, RackProjectionOrientationMode.Projected);
            var canonical = Run(source, target, 90.0, RackProjectionOrientationMode.Canonical);

            var a = Assert.Single(projected.Port.Scopes.Created);
            var b = Assert.Single(canonical.Port.Scopes.Created);
            Assert.Equal(b.BaseName, a.BaseName);
            Assert.Equal(b.TargetAddress, a.TargetAddress);
            Assert.Equal(b.RackId, a.RackId);
            Assert.Equal(b.Envelope.Design, a.Envelope.Design);
            Assert.NotEqual(
                Normalize(Assert.Single(canonical.Port.Scopes.Placed).RotationRadians),
                Normalize(Assert.Single(projected.Port.Scopes.Placed).RotationRadians));
        }

        // ================================================================ Q-5: real frames of every system with both elevations

        [Theory]
        [MemberData(nameof(Elevations))]
        public void C16_07_Q5_ProjectedDerivesRhoFromTheSharedHeightAxis(
            string system, DimensionViewKind source, DimensionViewKind target, double degrees)
        {
            var view = Authority.View(system, source, target, degrees);
            Assert.True(RackProjectionClassMapping.TryMap(source, target, out _, out var axis), "the pair must be exposed");
            Assert.Equal(RackPhysicalAxis.Height, axis);

            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { view }, axis, RackProjectionOrientationMode.Projected);

            Assert.True(frames.IsAvailable, frames.Failure.ToString());
            var d = Authority.WorldAxis(view, axis);
            Authority.AssertSame(d, Transform2D.Rotation(frames.Target.OrientationRadians).Apply(Authority.LocalAxis(view.TargetFrame, axis)), "R(rho)·t = d");
            // Derived, not assumed: both elevations carry Height on +Y, so rho is the source rotation (no +90°).
            Assert.Equal(Normalize(degrees * Math.PI / 180.0), Normalize(frames.Target.OrientationRadians), 9);

            var projection = RackOrthographicPlacementPolicy.Project(
                new[] { view }, axis, RackProjectionClassMapping.FamilyOf(Authority.SystemKind(system)), frames.Source, frames.Target);
            Assert.True(projection.IsAvailable, projection.Failure.ToString());
            Authority.AssertSame(d, projection.Projection.CommonDirection, "e = d");
            Authority.AssertSame(d, projection.Projection.TargetDirection, "w = d");
            Assert.Equal(0.0, Normalize(projection.Projection.AlphaRadians), 9);

            var placed = RackOrthographicPlacementPolicy.Place(
                projection.Projection, projection.Projection.References[0], new Point3D(0, 0, 0), new Point3D(500, 300, 0));
            Assert.Equal(Normalize(frames.Target.OrientationRadians), Normalize(placed.RotationRadians), 9);
        }

        [Theory]
        [MemberData(nameof(Elevations))]
        public void C16_07_Q5_CanonicalKeepsTheUniversalFramesAndProjects(
            string system, DimensionViewKind source, DimensionViewKind target, double degrees)
        {
            var view = Authority.View(system, source, target, degrees);
            Assert.True(RackProjectionClassMapping.TryMap(source, target, out _, out var axis), "the pair must be exposed");

            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { view }, axis, RackProjectionOrientationMode.Canonical);

            Assert.True(frames.IsAvailable);
            Assert.Equal(0.0, frames.Target.OrientationRadians, 12);
            var projection = RackOrthographicPlacementPolicy.Project(
                new[] { view }, axis, RackProjectionClassMapping.FamilyOf(Authority.SystemKind(system)), frames.Source, frames.Target);
            Assert.True(projection.IsAvailable, projection.Failure.ToString());
            Assert.Equal(0.0, projection.Projection.TargetRotationRadians, 12);
            Authority.AssertSame(new Vector2D(0.0, 1.0), projection.Projection.TargetDirection, "w = +Height of the unrotated target");
        }

        // ================================================================ Q-6: genuinely unsupported combinations stay closed

        [Theory]
        [InlineData(DimensionViewKind.Lateral)]
        [InlineData(DimensionViewKind.Planta)]
        public void C16_07_Q6_ACabeceraHasNoFrontalSoThePairIsNotExposed(DimensionViewKind sourceKind)
        {
            var scenario = new G15.Scenario(RackSystemKind.Selective, DimensionViewKind.Frontal)
            {
                SourceAddress = RackViewAddress.Whole(sourceKind),
                Orientation = RackProjectionOrientationMode.Projected,
            };
            scenario.Services.TargetAddresses.Clear();
            scenario.Services.TargetAddresses.Add(G14.Planta);
            scenario.Services.TargetAddresses.Add(RackViewAddress.Whole(DimensionViewKind.Lateral));
            scenario.AddRack(G14.RackA, "REF-1", 0, 0);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.Equal(RackProjectionStage.Available, result.PlanResult.FailedStage);
            var diagnostic = Assert.Single(result.PlanResult.Diagnostics);
            Assert.Equal(RackProjectionFailureCode.PairNotExposed, diagnostic.Code);
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
        }

        [Fact]
        public void C16_07_Q6_AnExistingButUnavailableTargetVariantIsStillTargetAddressUnavailable()
        {
            // The rack offers a frontal class, but only a fondo the policy does not accept: AVAILABLE validation is not weakened.
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Frontal) { SourceAddress = G14.Lateral0 };
            scenario.Services.TargetAddresses.Clear();
            scenario.Services.TargetAddresses.Add(G14.Planta);
            scenario.Services.TargetAddresses.Add(G14.Lateral0);
            scenario.Services.TargetAddresses.Add(RackViewAddress.Fondo(3));
            scenario.AddRack(G14.RackA, "REF-1", 0, 0);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionStage.Available, result.PlanResult.FailedStage);
            Assert.Equal(RackProjectionFailureCode.TargetAddressUnavailable, Assert.Single(result.PlanResult.Diagnostics).Code);
            Assert.Equal(0, port.Scopes.Begun);
        }

        [Fact]
        public void C16_07_Q6_TheFlowBedStaysOutsideId19ForAnElevationTarget()
        {
            // Self-check note: the pair is now exposed, so the flow bed lateral is refused by the source decision (TargetNotExposed, the same
            // failure it already had for Planta and Lateral targets) at AVAILABLE, before any point and with the same «no se puede proyectar».
            var scenario = new G15.Scenario(RackSystemKind.Cama, DimensionViewKind.Frontal) { SourceAddress = RackViewAddress.Whole(DimensionViewKind.Lateral) };
            scenario.AddRack(G14.RackA, "REF-1", 0, 0);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.Equal(RackProjectionStage.Available, result.PlanResult.FailedStage);
            Assert.Equal(RackProjectionFailureCode.TargetNotExposed, Assert.Single(result.PlanResult.Diagnostics).Code);
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
        }

        [Fact]
        public void C16_07_Q6_PairNotExposedKeepsItsCodeAndRemedy()
        {
            Assert.True(Enum.IsDefined(typeof(RackProjectionFailureCode), RackProjectionFailureCode.PairNotExposed));
            var remedy = RackProjectionRemedySelector.For(
                RackProjectionFailureCode.PairNotExposed, RackSystemKind.Selective, hasAnotherValidSiblingView: false);
            Assert.False(string.IsNullOrWhiteSpace(remedy.Message));
        }

        // ================================================================ Q-7: several racks, one common operation

        [Fact]
        public void C16_07_Q7_FrontalsOfOneRowShareTheFloorLineSoTheirLateralsSuperposeWithAWarning()
        {
            var scenario = Scenario(DimensionViewKind.Lateral, G14.Frontal0, RackProjectionOrientationMode.Projected);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0);
            scenario.AddRack(G14.RackB, "REF-2", 200, 0);
            var port = scenario.NewPort(G15.Points(0, 0, 500, 300));

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            Assert.Equal(2, port.Scopes.Placed.Count);
            Assert.All(port.Scopes.Placed, p => Assert.Equal(0.0, Normalize(p.RotationRadians), 9));
            Assert.All(port.Scopes.Placed, p => Assert.Equal(500.0, p.Position.X, 6));
            Assert.All(port.Scopes.Placed, p => Assert.Equal(300.0, p.Position.Y, 6));
            Assert.Contains(result.PlanResult.Warnings, w => w.Code == RackProjectionWarningCode.Overlap);
            Assert.Equal(1, port.Scopes.Commits);
        }

        [Fact]
        public void C16_07_Q7_FrontalsOnDifferentFloorLinesKeepTheirSeparationAlongTheHeight()
        {
            var scenario = Scenario(DimensionViewKind.Lateral, G14.Frontal0, RackProjectionOrientationMode.Projected);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0);
            scenario.AddRack(G14.RackB, "REF-2", 0, 300);
            var port = scenario.NewPort(G15.Points(0, 0, 500, 300));

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            Assert.Equal(300.0, port.Scopes.Placed.Single(p => p.PhysicalKey == "REF-1").Position.Y, 6);
            Assert.Equal(600.0, port.Scopes.Placed.Single(p => p.PhysicalKey == "REF-2").Position.Y, 6);
            Assert.DoesNotContain(result.PlanResult.Warnings, w => w.Code == RackProjectionWarningCode.Overlap);
        }

        [Fact]
        public void C16_07_Q7_OppositeElevationsHaveNoCommonProjectedOrientationButCanonicalProceeds()
        {
            var projected = Scenario(DimensionViewKind.Lateral, G14.Frontal0, RackProjectionOrientationMode.Projected);
            projected.AddRack(G14.RackA, "REF-1", 0, 0, rotation: 0.0);
            projected.AddRack(G14.RackB, "REF-2", 200, 0, rotation: Math.PI);
            var projectedPort = projected.NewPort(G15.Points());

            var refused = RackProjectionCommandRun.Execute(projectedPort);

            Assert.Equal(RackProjectionCommandStatus.Blocked, refused.Status);
            Assert.Equal(RackProjectionStage.Validate, refused.PlanResult.FailedStage);
            Assert.All(refused.PlanResult.Diagnostics, d => Assert.Equal(RackProjectionFailureCode.SourceOrientationDivergent, d.Code));
            Assert.DoesNotContain("pick-base", projectedPort.Events);
            Assert.Equal(0, projectedPort.Scopes.Begun);

            var canonical = Scenario(DimensionViewKind.Lateral, G14.Frontal0, RackProjectionOrientationMode.Canonical);
            canonical.AddRack(G14.RackA, "REF-1", 0, 0, rotation: 0.0);
            canonical.AddRack(G14.RackB, "REF-2", 200, 0, rotation: Math.PI);
            var canonicalPort = canonical.NewPort(G15.Points());

            Assert.True(RackProjectionCommandRun.Execute(canonicalPort).IsCompleted, string.Join(" | ", canonicalPort.Printed));
            Assert.All(canonicalPort.Scopes.Placed, p => Assert.Equal(0.0, p.RotationRadians, 12));
        }

        [Theory]
        [InlineData(RackProjectionOrientationMode.Projected)]
        [InlineData(RackProjectionOrientationMode.Canonical)]
        public void C16_07_Q7_NonParallelElevationsAreRefusedBeforeAnyPoint(RackProjectionOrientationMode mode)
        {
            var scenario = Scenario(DimensionViewKind.Lateral, G14.Frontal0, mode);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, rotation: 0.0);
            scenario.AddRack(G14.RackB, "REF-2", 200, 0, rotation: Math.PI / 2.0);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.All(result.PlanResult.Diagnostics, d => Assert.Equal(RackProjectionFailureCode.NonParallelSources, d.Code));
            Assert.DoesNotContain("pick-base", port.Events);
        }

        [Fact]
        public void C16_07_Q7_TheFloorLineCoordinateIsTheHeightOfTheSourceOrigin()
        {
            var view = Authority.View("Selective", DimensionViewKind.Frontal, DimensionViewKind.Lateral, 0.0, x: 10.0, y: 20.0);
            var frames = RackProjectionOrientationResolver.ResolveOrthographic(
                new[] { view }, RackPhysicalAxis.Height, RackProjectionOrientationMode.Projected);

            var projection = RackOrthographicPlacementPolicy.Project(
                new[] { view }, RackPhysicalAxis.Height, RackProjectionFamily.Rack, frames.Source, frames.Target);

            Assert.True(projection.IsAvailable, projection.Failure.ToString());
            var reference = Assert.Single(projection.Projection.References);
            Assert.Equal(0.0, reference.SpanLength, 12);
            Assert.Equal(20.0, reference.SourceCoordinate, 9);
        }

        // ================================================================ Q-8: conservations

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void C16_07_Q8_AnUnnamedRackProjectsBetweenElevationsAndKeepsItsIdentity(string name)
        {
            var scenario = Scenario(DimensionViewKind.Lateral, G14.Frontal0, RackProjectionOrientationMode.Projected);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: name);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            Assert.Equal(G14.RackA, Assert.Single(port.Scopes.Created).RackId);
            Assert.Equal(1, port.Scopes.Commits);
        }

        [Fact]
        public void C16_07_Q8_ElevationProjectionUsesTheSameSingleOperation()
        {
            var run = Run(G14.Frontal0, DimensionViewKind.Lateral, 90.0, RackProjectionOrientationMode.Projected);

            Assert.True(run.Result.IsCompleted, Explain(run));
            Assert.Equal(RackProjectionMode.Orthographic, run.Result.PlanResult.Plan.Mode);
            Assert.Equal(RackPhysicalAxis.Height, run.Result.PlanResult.Plan.ConservedAxis);
            var events = run.Port.Events;
            Assert.True(events.IndexOf("pick-base") < events.IndexOf("pick-target"));
            Assert.True(events.IndexOf("pick-target") < events.IndexOf("import"));
            Assert.Equal(1, run.Port.Scopes.Begun);
            Assert.Equal(1, run.Port.Scopes.Commits);
            Assert.Contains(run.Port.Printed, line => line.Contains("Orientacion Proyectada"));
        }

        // ================================================================ helpers

        private static void AssertProjected(RackViewAddress source, DimensionViewKind target, double degrees)
        {
            var run = Run(source, target, degrees, RackProjectionOrientationMode.Projected);

            Assert.True(run.Result.IsCompleted, Explain(run));
            var plan = run.Result.PlanResult.Plan;
            Assert.Equal(RackProjectionMode.Orthographic, plan.Mode);
            Assert.Equal(RackPhysicalAxis.Height, plan.ConservedAxis);
            var placed = Assert.Single(run.Port.Scopes.Placed);
            Assert.Equal(Normalize(degrees * Math.PI / 180.0), Normalize(placed.RotationRadians), 9);

            // A pure projection along the source Height: e = w = d, alpha = 0; the floor anchor (local origin of both elevations of
            // these frames) lands on the target point moved along d by the source floor coordinate (source floor anchor at (10, 20)).
            var view = plan.Views.Single();
            var d = Authority.WorldAxis(view, RackPhysicalAxis.Height);
            Authority.AssertSame(d, plan.Orthographic.CommonDirection, "e = d");
            Authority.AssertSame(d, plan.Orthographic.TargetDirection, "w = d");
            Assert.Equal(0.0, Normalize(plan.Orthographic.AlphaRadians), 9);
            var along = (10.0 * d.X) + (20.0 * d.Y);
            Assert.Equal(500.0 + (along * d.X), placed.Position.X, 6);
            Assert.Equal(300.0 + (along * d.Y), placed.Position.Y, 6);
        }

        private static (RackProjectionCommandResult Result, G15.Port Port) Run(
            RackViewAddress source, DimensionViewKind target, double degrees, RackProjectionOrientationMode mode)
        {
            var scenario = Scenario(target, source, mode);
            scenario.AddRack(G14.RackA, "REF-1", 10, 20, rotation: degrees * Math.PI / 180.0);
            var port = scenario.NewPort(G15.Points(0, 0, 500, 300));
            return (RackProjectionCommandRun.Execute(port), port);
        }

        private static G15.Scenario Scenario(DimensionViewKind target, RackViewAddress source, RackProjectionOrientationMode mode)
            => new G15.Scenario(RackSystemKind.SelectiveRack, target) { SourceAddress = source, Orientation = mode };

        private static string Explain((RackProjectionCommandResult Result, G15.Port Port) run)
            => string.Join(" | ", run.Result.PlanResult?.Diagnostics.Select(d => d.Stage + "/" + d.Code) ?? Array.Empty<string>())
                + " || " + string.Join(" | ", run.Port.Printed);

        private static double Normalize(double radians) => Authority.Normalize(radians);
    }
}
