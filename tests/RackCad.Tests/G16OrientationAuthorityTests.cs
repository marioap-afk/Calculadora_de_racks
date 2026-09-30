using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// G16 C16-06, obligation O-1: the pure orientation authority over the REAL frames of the five ID19 systems (AUTH-05 adapters)
    /// and AUTH-08 V2 accepted placements. Projected: the target BlockReference rotation rho makes the target +K axis point, in the
    /// drawing, where the source +K axis points (R(rho)·t = L·s), and the group frames make the existing orthographic policy a pure
    /// projection (e = w = d, alpha = 0). Canonical: the universal frames of the frozen G14 contract. Frozen design:
    /// docs/initiatives/I-55-c16-06-projected-orientation.md.
    /// </summary>
    public class G16OrientationAuthorityTests
    {
        private static readonly double[] Rotations = { 0.0, 90.0, 180.0, 270.0 };

        public static IEnumerable<object[]> Pairs()
        {
            foreach (var system in new[] { "Selective", "Dynamic", "PushBack", "Cabecera", "Cantilever" })
            {
                foreach (var pair in PairsOf(system))
                {
                    foreach (var degrees in Rotations)
                    {
                        yield return new object[] { system, pair.Source, pair.Target, degrees };
                    }
                }
            }
        }

        // ================================================================ O-1: the derived rotation, every system

        [Theory]
        [MemberData(nameof(Pairs))]
        public void C16_06_O1_ProjectedKeepsTheSharedPhysicalAxisPointingWhereTheSourceAxisPoints(
            string system, DimensionViewKind source, DimensionViewKind target, double degrees)
        {
            var view = View(system, source, target, degrees);
            Assert.True(RackProjectionClassMapping.TryMap(source, target, out var mode, out var axis));
            Assert.Equal(RackProjectionMode.Orthographic, mode);

            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { view }, axis, RackProjectionOrientationMode.Projected);

            Assert.True(frames.IsAvailable, frames.Failure.ToString());
            var d = WorldAxis(view, axis);
            var t = LocalAxis(view.TargetFrame, axis);
            AssertSame(d, Transform2D.Rotation(frames.Target.OrientationRadians).Apply(t), "R(rho)·t = d");
            AssertSame(d, Transform2D.Rotation(frames.Source.OrientationRadians).Apply(LocalAxis(view.SourceFrame, axis)), "R(phi_s)·s = d");
        }

        [Theory]
        [MemberData(nameof(Pairs))]
        public void C16_06_O1_ProjectedMakesTheOrthographicPolicyAPureProjectionAlongTheSourceAxis(
            string system, DimensionViewKind source, DimensionViewKind target, double degrees)
        {
            var view = View(system, source, target, degrees);
            RackProjectionClassMapping.TryMap(source, target, out _, out var axis);
            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { view }, axis, RackProjectionOrientationMode.Projected);

            var projection = RackOrthographicPlacementPolicy.Project(
                new[] { view }, axis, RackProjectionClassMapping.FamilyOf(SystemKind(system)), frames.Source, frames.Target);

            Assert.True(projection.IsAvailable, projection.Failure.ToString());
            var d = WorldAxis(view, axis);
            AssertSame(d, projection.Projection.CommonDirection, "e = d");
            AssertSame(d, projection.Projection.TargetDirection, "w = d");
            Assert.Equal(0.0, Normalize(projection.Projection.AlphaRadians), 9);
            Assert.False(projection.Projection.NearWindowLimit);

            var placed = RackOrthographicPlacementPolicy.Place(
                projection.Projection, projection.Projection.References[0], new Point3D(0, 0, 0), new Point3D(500, 300, 0));
            Assert.Equal(Normalize(frames.Target.OrientationRadians), Normalize(placed.RotationRadians), 9);
        }

        [Theory]
        [MemberData(nameof(Pairs))]
        public void C16_06_O1_CanonicalKeepsTheUniversalFramesOfTheFrozenG14Contract(
            string system, DimensionViewKind source, DimensionViewKind target, double degrees)
        {
            var view = View(system, source, target, degrees);
            RackProjectionClassMapping.TryMap(source, target, out _, out var axis);

            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { view }, axis, RackProjectionOrientationMode.Canonical);

            Assert.True(frames.IsAvailable);
            Assert.Equal(0.0, frames.Source.OrientationRadians, 12);
            Assert.Equal(0.0, frames.Target.OrientationRadians, 12);
        }

        // ================================================================ the closed-form table of the frozen design (section 4)

        [Theory]
        [InlineData("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 90.0)]
        [InlineData("Selective", DimensionViewKind.Planta, DimensionViewKind.Lateral, 0.0)]
        [InlineData("Selective", DimensionViewKind.Frontal, DimensionViewKind.Planta, -90.0)]
        [InlineData("Selective", DimensionViewKind.Lateral, DimensionViewKind.Planta, 0.0)]
        [InlineData("Dynamic", DimensionViewKind.Planta, DimensionViewKind.Frontal, 90.0)]
        [InlineData("Dynamic", DimensionViewKind.Frontal, DimensionViewKind.Planta, -90.0)]
        [InlineData("PushBack", DimensionViewKind.Planta, DimensionViewKind.Frontal, 90.0)]
        [InlineData("PushBack", DimensionViewKind.Lateral, DimensionViewKind.Planta, 0.0)]
        [InlineData("Cabecera", DimensionViewKind.Planta, DimensionViewKind.Lateral, 0.0)]
        [InlineData("Cabecera", DimensionViewKind.Lateral, DimensionViewKind.Planta, 0.0)]
        [InlineData("Cantilever", DimensionViewKind.Planta, DimensionViewKind.Frontal, 0.0)]
        [InlineData("Cantilever", DimensionViewKind.Planta, DimensionViewKind.Lateral, 90.0)]
        [InlineData("Cantilever", DimensionViewKind.Frontal, DimensionViewKind.Planta, 0.0)]
        [InlineData("Cantilever", DimensionViewKind.Lateral, DimensionViewKind.Planta, -90.0)]
        public void C16_06_TheTargetRotationIsTheSourceRotationPlusTheClassOffsetOfTheFamily(
            string system, DimensionViewKind source, DimensionViewKind target, double offsetDegrees)
        {
            foreach (var degrees in Rotations)
            {
                var view = View(system, source, target, degrees);
                RackProjectionClassMapping.TryMap(source, target, out _, out var axis);

                var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { view }, axis, RackProjectionOrientationMode.Projected);

                Assert.Equal(Normalize((degrees + offsetDegrees) * Math.PI / 180.0), Normalize(frames.Target.OrientationRadians), 9);
            }
        }

        // ================================================================ E: one common orientation or a typed refusal

        [Fact]
        public void C16_06_ParallelCoDirectedSourcesShareOneProjectedOrientation()
        {
            var a = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 90.0, x: 0.0, y: 0.0);
            var b = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 90.0, x: 300.0, y: 40.0, key: "REF-2");

            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { a, b }, RackPhysicalAxis.Run, RackProjectionOrientationMode.Projected);

            Assert.True(frames.IsAvailable);
            Assert.Equal(Normalize(Math.PI), Normalize(frames.Target.OrientationRadians), 9);
        }

        [Fact]
        public void C16_06_OppositeSourcesHaveNoCommonProjectedOrientationAndAreRefused()
        {
            var a = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 0.0);
            var b = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 180.0, x: 200.0, key: "REF-2");

            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { a, b }, RackPhysicalAxis.Run, RackProjectionOrientationMode.Projected);

            Assert.False(frames.IsAvailable);
            Assert.Equal(RackProjectionFailureCode.SourceOrientationDivergent, frames.Failure);
        }

        [Fact]
        public void C16_06_NonParallelSourcesKeepTheirExistingFailureInBothModes()
        {
            var a = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 0.0);
            var b = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 90.0, x: 200.0, key: "REF-2");

            var projected = RackProjectionOrientationResolver.ResolveOrthographic(new[] { a, b }, RackPhysicalAxis.Run, RackProjectionOrientationMode.Projected);

            Assert.False(projected.IsAvailable);
            Assert.Equal(RackProjectionFailureCode.NonParallelSources, projected.Failure);
        }

        [Fact]
        public void C16_06_CanonicalIsNeverRefusedForADivergenceThatOnlyProjectedWouldHave()
        {
            var a = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 0.0);
            var b = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 180.0, x: 200.0, key: "REF-2");

            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { a, b }, RackPhysicalAxis.Run, RackProjectionOrientationMode.Canonical);

            Assert.True(frames.IsAvailable);
        }

        [Fact]
        public void C16_06_AHalfTurnSourceIsReadFromTheAcceptedLinearPartOfAuth08()
        {
            // AUTH-08 reports a planar half turn with the sign pair (-1, -1) and rotation 0: the accepted linear part is a half turn.
            var halfTurn = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 0.0, scaleX: -1.0, scaleY: -1.0);
            var turned = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 180.0);

            var a = RackProjectionOrientationResolver.ResolveOrthographic(new[] { halfTurn }, RackPhysicalAxis.Run, RackProjectionOrientationMode.Projected);
            var b = RackProjectionOrientationResolver.ResolveOrthographic(new[] { turned }, RackPhysicalAxis.Run, RackProjectionOrientationMode.Projected);

            Assert.True(a.IsAvailable, a.Failure.ToString());
            Assert.Equal(Normalize(b.Target.OrientationRadians), Normalize(a.Target.OrientationRadians), 9);
        }

        [Theory]
        [InlineData(30.0)]
        [InlineData(210.0)]
        [InlineData(-120.0)]
        public void C16_06_O1_ANonCardinalRotationFollowsTheSameDerivation(double degrees)
        {
            var view = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, degrees);

            var frames = RackProjectionOrientationResolver.ResolveOrthographic(new[] { view }, RackPhysicalAxis.Run, RackProjectionOrientationMode.Projected);

            Assert.True(frames.IsAvailable);
            Assert.Equal(Normalize((degrees + 90.0) * Math.PI / 180.0), Normalize(frames.Target.OrientationRadians), 9);
            Assert.Equal(Normalize(view.Placement.RotationRadians), Normalize(frames.Source.OrientationRadians), 9);
        }

        [Fact]
        public void C16_06_O1_SeveralReferencesProjectAlongTheirNormalisedCommonLine()
        {
            var views = new[]
            {
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 30.0, x: 0.0, y: 0.0, key: "REF-1"),
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 30.0, x: -150.0, y: 260.0, key: "REF-2"),
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Frontal, 30.0, x: 400.0, y: -50.0, key: "REF-3"),
            };
            var frames = RackProjectionOrientationResolver.ResolveOrthographic(views, RackPhysicalAxis.Run, RackProjectionOrientationMode.Projected);

            var projection = RackOrthographicPlacementPolicy.Project(
                views, RackPhysicalAxis.Run, RackProjectionFamily.Rack, frames.Source, frames.Target);

            Assert.True(projection.IsAvailable);
            AssertSame(WorldAxis(views[0], RackPhysicalAxis.Run), projection.Projection.CommonDirection, "e = d");
            Assert.All(projection.Projection.References, r => Assert.Equal(1, r.SourceSign));
            Assert.Equal(0.0, Normalize(projection.Projection.AlphaRadians), 9);
        }

        // ================================================================ same class (Rigid): alpha = phi_t - phi_s

        [Theory]
        [InlineData(0.0)]
        [InlineData(90.0)]
        [InlineData(180.0)]
        [InlineData(270.0)]
        [InlineData(30.0)]
        public void C16_06_SameClassProjectedIsTheFrozenRigidCopy(double degrees)
        {
            var views = new[]
            {
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Planta, degrees),
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Planta, degrees + 180.0, x: 300.0, key: "REF-2"),
            };

            var frames = RackProjectionOrientationResolver.ResolveRigid(views, RackProjectionOrientationMode.Projected);

            Assert.True(frames.IsAvailable);
            Assert.Equal(0.0, frames.Source.OrientationRadians, 12);
            Assert.Equal(0.0, frames.Target.OrientationRadians, 12);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(90.0)]
        [InlineData(180.0)]
        [InlineData(270.0)]
        [InlineData(30.0)]
        public void C16_06_SameClassCanonicalTurnsTheWholeGroupBackToTheUniversalFrame(double degrees)
        {
            var views = new[]
            {
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Planta, degrees),
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Planta, degrees, x: 300.0, key: "REF-2"),
            };

            var frames = RackProjectionOrientationResolver.ResolveRigid(views, RackProjectionOrientationMode.Canonical);

            Assert.True(frames.IsAvailable, frames.Failure.ToString());
            Assert.Equal(Normalize(views[0].Placement.RotationRadians), Normalize(frames.Source.OrientationRadians), 9);
            Assert.Equal(0.0, frames.Target.OrientationRadians, 12);
        }

        [Fact]
        public void C16_06_SameClassCanonicalReadsTheHalfTurnOfAuth08()
        {
            var halfTurn = View("Selective", DimensionViewKind.Planta, DimensionViewKind.Planta, 0.0, scaleX: -1.0, scaleY: -1.0);

            var frames = RackProjectionOrientationResolver.ResolveRigid(new[] { halfTurn }, RackProjectionOrientationMode.Canonical);

            Assert.True(frames.IsAvailable);
            Assert.Equal(Normalize(Math.PI), Normalize(frames.Source.OrientationRadians), 9);
        }

        [Fact]
        public void C16_06_SameClassCanonicalRefusesDifferentRotations()
        {
            var views = new[]
            {
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Planta, 0.0),
                View("Selective", DimensionViewKind.Planta, DimensionViewKind.Planta, 180.0, x: 300.0, key: "REF-2"),
            };

            var frames = RackProjectionOrientationResolver.ResolveRigid(views, RackProjectionOrientationMode.Canonical);

            Assert.False(frames.IsAvailable);
            Assert.Equal(RackProjectionFailureCode.SourceRotationsDiffer, frames.Failure);
        }

        // ================================================================ helpers: REAL frames of the five systems

        private static IEnumerable<(DimensionViewKind Source, DimensionViewKind Target)> PairsOf(string system)
        {
            if (system != "Cabecera")
            {
                yield return (DimensionViewKind.Planta, DimensionViewKind.Frontal);
                yield return (DimensionViewKind.Frontal, DimensionViewKind.Planta);
            }

            yield return (DimensionViewKind.Planta, DimensionViewKind.Lateral);
            yield return (DimensionViewKind.Lateral, DimensionViewKind.Planta);
        }

        private static RackSystemKind SystemKind(string system)
        {
            switch (system)
            {
                case "Dynamic": return RackSystemKind.PalletFlow;
                case "PushBack": return RackSystemKind.PushBack;
                case "Cabecera": return RackSystemKind.Selective;
                case "Cantilever": return RackSystemKind.Cantilever;
                default: return RackSystemKind.SelectiveRack;
            }
        }

        private static RackProjectionSourceView View(
            string system, DimensionViewKind source, DimensionViewKind target, double degrees,
            double x = 10.0, double y = 20.0, string key = "REF-1", double scaleX = 1.0, double scaleY = 1.0)
        {
            var sourceAddress = Address(system, source);
            var targetAddress = Address(system, target);
            var sourceFrame = Frame(system, sourceAddress);
            var targetFrame = Frame(system, targetAddress);
            Assert.True(sourceFrame.IsAvailable, system + " " + sourceAddress + ": " + sourceFrame.Failure);
            Assert.True(targetFrame.IsAvailable, system + " " + targetAddress + ": " + targetFrame.Failure);
            var accepted = RackProjectionSourceTransform.Accept(
                G14.Facts(x, y, rotation: degrees * Math.PI / 180.0, scaleX: scaleX, scaleY: scaleY));
            Assert.True(accepted.IsAccepted, accepted.Failure.ToString());

            return new RackProjectionSourceView(
                key, G14.RackA, sourceAddress, sourceFrame.Frame, targetAddress, targetFrame.Frame, accepted);
        }

        private static RackViewAddress Address(string system, DimensionViewKind kind)
        {
            if (kind == DimensionViewKind.Planta) return RackViewAddress.Whole(DimensionViewKind.Planta);
            switch (system)
            {
                case "Dynamic":
                    return kind == DimensionViewKind.Frontal ? RackViewAddress.FlowEnd(RackFlowEnd.Exit) : RackViewAddress.Post(0);
                case "PushBack":
                    return kind == DimensionViewKind.Frontal
                        ? RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A)
                        : RackViewAddress.Post(0);
                case "Cabecera":
                    return RackViewAddress.Whole(DimensionViewKind.Lateral);
                case "Cantilever":
                    return kind == DimensionViewKind.Frontal ? RackViewAddress.Whole(DimensionViewKind.Frontal) : RackViewAddress.Station(0);
                default:
                    return kind == DimensionViewKind.Frontal ? RackViewAddress.Fondo(0) : RackViewAddress.Post(0);
            }
        }

        private static RackViewFrameResult Frame(string system, RackViewAddress address)
        {
            switch (system)
            {
                case "Dynamic":
                    return DynamicViewFrameAdapter.Resolve(Dynamic(DimensionDetail.Standard, Catalog), Catalog, address);
                case "PushBack":
                    return PushBackViewFrameAdapter.Resolve(PushBackSingleSided(DimensionDetail.Standard, Catalog), Catalog, address);
                case "Cabecera":
                    return CabeceraViewFrameAdapter.Resolve(new RackFrameConfiguration { Depth = 48.0 }, address);
                case "Cantilever":
                    var (line, factory) = CantileverLine();
                    var kind = address.Kind == DimensionViewKind.Planta
                        ? CantileverViewKind.Planta
                        : address.Kind == DimensionViewKind.Frontal ? CantileverViewKind.Frontal : CantileverViewKind.Lateral;
                    var plan = address.Kind == DimensionViewKind.Lateral
                        ? CantileverViewPlanBuilder.Build(line, kind, factory, stationIndex: address.Variant.Index)
                        : CantileverViewPlanBuilder.Build(line, kind, factory);
                    return CantileverViewFrameAdapter.Resolve(line, plan, address);
                default:
                    return SelectiveViewFrameAdapter.Resolve(Selective(DimensionDetail.Standard, false, Catalog), Catalog, address);
            }
        }

        private static (CantileverLineAssembly Line, StructuralSectionGeometryFactory Factory) CantileverLine()
        {
            var catalog = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();
            var factory = new StructuralSectionGeometryFactory(catalog);
            var topology = new CantileverLineStationTopologyDesign
            {
                FaceMode = CantileverStationFaceMode.Single,
                SingleSide = CantileverArmSide.PositiveY,
                LevelCount = 2,
                RequestedClearHeight = 24.0,
                ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                {
                    ColumnSectionId = "AISC-W-W10X33",
                    Base = new CantileverBaseDesign { SectionId = "AISC-W-W12X26", Length = 48.0 }
                }
            };
            topology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            topology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 4.0;
            var design = new CantileverLineDesign
            {
                Name = "C16-06",
                StationCount = 3,
                ColumnCentreSpacing = 96.0,
                StationTopology = topology,
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign
                    {
                        Arrangement = CantileverArmBodyArrangement.Single,
                        SectionId = "AISC-HSS-RECT-HSS4X4X_250",
                        CutLength = 36.0
                    },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5
                    }
                },
                Bracing = new CantileverBracingDesign()
            };
            var line = CantileverLineResolver.Resolve(
                design, catalog, factory,
                CantileverCataloguePolicies.ColumnBase(catalog),
                CantileverCataloguePolicies.Arm(catalog));
            Assert.False(line.IsBlocked, string.Join(" | ", line.Diagnostics.Select(item => item.Message)));
            return (line, factory);
        }

        private static Vector2D LocalAxis(RackViewFrame frame, RackPhysicalAxis axis)
        {
            Assert.True(RackViewFrameSemantics.TryAxisDirection(frame, axis, out var direction), frame.AxisMap + " has no " + axis);
            return direction;
        }

        private static Vector2D WorldAxis(RackProjectionSourceView view, RackPhysicalAxis axis)
            => view.Placement.Linear.Apply(LocalAxis(view.SourceFrame, axis)).Normalized();

        private static void AssertSame(Vector2D expected, Vector2D actual, string what)
        {
            Assert.True(
                Math.Abs(expected.X - actual.X) < 1e-9 && Math.Abs(expected.Y - actual.Y) < 1e-9,
                what + ": expected (" + expected.X + ", " + expected.Y + "), actual (" + actual.X + ", " + actual.Y + ")");
        }

        /// <summary>An angle in [0, 2 pi), so equal orientations compare equal.</summary>
        private static double Normalize(double radians)
        {
            var twoPi = 2.0 * Math.PI;
            var value = radians % twoPi;
            if (value < 0.0) value += twoPi;
            return Math.Abs(value - twoPi) < 1e-9 ? 0.0 : value;
        }
    }
}
