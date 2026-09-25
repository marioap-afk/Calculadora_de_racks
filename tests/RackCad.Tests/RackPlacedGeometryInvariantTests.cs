using System;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14 (AE): INV-GRP-1..5 measured against an independent matrix. The expected values are computed here with
    /// the AutoCAD placement formula, never by calling the production policy again.
    /// </summary>
    public class RackPlacedGeometryInvariantTests
    {
        private static readonly Point3D Base = new Point3D(0, 0, 1);
        private static readonly Point3D Target = new Point3D(1000, 0, 5);

        [Fact]
        public void INV_GRP_1_ANCHOR_DIFFERENCES_ARE_THE_COMMON_ROTATION_OF_SOURCE_DIFFERENCES()
        {
            var transform = CommonTransform2D.TryCreate(Base, Target, 0.0).Transform;
            var first = RigidView("REF-1", G14.RackA, x: 10, y: 20, rotation: 0.4);
            var second = RigidView("REF-2", G14.RackB, x: 130, y: -45, rotation: 1.1);

            var a = RackRigidPlacementPolicy.Place(first, transform);
            var b = RackRigidPlacementPolicy.Place(second, transform);

            var sourceA = SourceAnchor(first);
            var sourceB = SourceAnchor(second);
            var targetA = TargetAnchor(first, a);
            var targetB = TargetAnchor(second, b);

            Assert.Equal(sourceB.X - sourceA.X, targetB.X - targetA.X, 6);
            Assert.Equal(sourceB.Y - sourceA.Y, targetB.Y - targetA.Y, 6);
        }

        [Fact]
        public void INV_GRP_2_RIGID_ORIENTATION_IS_THE_SOURCE_ORIENTATION_PLUS_ALPHA()
        {
            var transform = CommonTransform2D.TryCreate(Base, Target, 0.0).Transform;
            var view = RigidView("REF-1", G14.RackA, x: 10, y: 20, rotation: 0.7);

            var placement = RackRigidPlacementPolicy.Place(view, transform);

            Assert.Equal(0.7, placement.RotationRadians, 6);
        }

        [Fact]
        public void INV_GRP_2_ORTHOGRAPHIC_ORIENTATION_DEPENDS_ONLY_ON_THE_TARGET_FRAME()
        {
            var views = new[]
            {
                RackOrthographicPlacementPolicyTests.PlantaView("REF-1", G14.RackA, 0.0, 0.0),
                RackOrthographicPlacementPolicyTests.PlantaView("REF-2", G14.RackB, 190.0, Math.PI),
            };

            var projection = RackOrthographicPlacementPolicy.Project(
                views,
                RackPhysicalAxis.Run,
                RackProjectionFamily.Rack,
                SourceGroupFrame.Universal(new Point3D(0, 0, 0)),
                TargetGroupFrame.Universal(new Point3D(500, 0, 0))).Projection;

            var placements = projection.References
                .Select(reference => RackOrthographicPlacementPolicy.Place(
                    projection, reference, new Point3D(0, 0, 0), new Point3D(500, 0, 0)))
                .ToArray();

            Assert.All(placements, placement => Assert.Equal(0.0, placement.RotationRadians, 9));
        }

        [Fact]
        public void INV_GRP_3_4_ORTHOGRAPHIC_KEEPS_EACH_SEMANTIC_SPAN_OVER_THE_COMMON_AXIS()
        {
            var views = new[]
            {
                RackOrthographicPlacementPolicyTests.PlantaView("REF-1", G14.RackA, 0.0, 0.0, targetLength: 90.0),
                RackOrthographicPlacementPolicyTests.PlantaView("REF-2", G14.RackB, 100.0, 0.0, targetLength: 90.0),
            };

            var projection = RackOrthographicPlacementPolicy.Project(
                views,
                RackPhysicalAxis.Run,
                RackProjectionFamily.Rack,
                SourceGroupFrame.Universal(new Point3D(0, 0, 0)),
                TargetGroupFrame.Universal(new Point3D(500, 0, 0))).Projection;

            var first = projection.References[0];
            var second = projection.References[1];

            Assert.Equal(0.0, first.SourceCoordinate, 6);
            Assert.Equal(100.0, second.SourceCoordinate, 6);
            Assert.Equal(90.0, first.SpanLength, 6);
            Assert.Equal(second.SourceCoordinate - first.SourceCoordinate, 100.0, 6);
        }

        [Fact]
        public void INV_GRP_5_PLACED_GEOMETRY_MATCHES_AN_INDEPENDENT_MATRIX_WITH_ORIGIN_AND_HALF_TURN()
        {
            var transform = CommonTransform2D.TryCreate(Base, Target, 0.0).Transform;

            foreach (var view in new[]
            {
                RigidView("REF-1", G14.RackA, x: 100, y: 50, rotation: Math.PI / 6.0, originX: 12, originY: -7, z: 3),
                RigidView("REF-2", G14.RackB, x: 100, y: 50, rotation: 0.0, originX: 12, originY: -7, z: 3, halfTurn: true),
            })
            {
                var placement = RackRigidPlacementPolicy.Place(view, transform);
                Assert.True(Satisfies(view, placement, transform));
            }
        }

        [Fact]
        public void INV_GRP_5_REJECTS_A_PERTURBED_ANCHOR_ROTATION_TRANSLATION_OR_ORIGIN()
        {
            var transform = CommonTransform2D.TryCreate(Base, Target, 0.0).Transform;
            var view = RigidView("REF-1", G14.RackA, x: 100, y: 50, rotation: Math.PI / 6.0, originX: 12, originY: -7, z: 3);
            var placement = RackRigidPlacementPolicy.Place(view, transform);

            Assert.True(Satisfies(view, placement, transform));

            Assert.False(Satisfies(view, Move(placement, dx: 1.0), transform));
            Assert.False(Satisfies(view, Move(placement, dy: -0.5), transform));
            Assert.False(Satisfies(view, Turn(placement, 0.01), transform));
            Assert.False(Satisfies(view, Move(placement, dz: 2.0), transform));
            Assert.False(Satisfies(view, IgnoreDefinitionOrigin(view, transform), transform));
        }

        /// <summary>Independent check: the placed reference must map its target local anchor onto T_common(A_r).</summary>
        private static bool Satisfies(
            RackProjectionSourceView view, RackProjectedPlacement placement, CommonTransform2D transform)
        {
            var facts = view.Placement.Facts;
            var localAnchor = RackViewFrameSemantics.Anchor(view.SourceFrame);
            var phi = facts.RotationRadians + (facts.HasPlanarHalfTurnSign ? Math.PI : 0.0);
            var dx = localAnchor.X - facts.DefinitionOrigin.X;
            var dy = localAnchor.Y - facts.DefinitionOrigin.Y;
            var worldX = facts.ReferencePosition.X + (Math.Cos(phi) * dx) - (Math.Sin(phi) * dy);
            var worldY = facts.ReferencePosition.Y + (Math.Sin(phi) * dx) + (Math.Cos(phi) * dy);

            var expected = transform.Apply(new Point2D(worldX, worldY));
            var targetLocal = RackViewFrameSemantics.Anchor(view.TargetFrame);
            var actualX = placement.Position.X
                + (Math.Cos(placement.RotationRadians) * targetLocal.X)
                - (Math.Sin(placement.RotationRadians) * targetLocal.Y);
            var actualY = placement.Position.Y
                + (Math.Sin(placement.RotationRadians) * targetLocal.X)
                + (Math.Cos(placement.RotationRadians) * targetLocal.Y);

            var expectedZ = Target.Z + (facts.ReferencePosition.Z - Base.Z);
            var expectedRotation = phi + transform.AlphaRadians;

            return Math.Abs(actualX - expected.X) <= 1e-6
                && Math.Abs(actualY - expected.Y) <= 1e-6
                && Math.Abs(placement.Position.Z - expectedZ) <= 1e-6
                && Math.Abs(NormalizeToPi(placement.RotationRadians - expectedRotation)) <= 1e-6;
        }

        private static double NormalizeToPi(double angle)
        {
            var normalized = angle % (2.0 * Math.PI);
            if (normalized <= -Math.PI) normalized += 2.0 * Math.PI;
            if (normalized > Math.PI) normalized -= 2.0 * Math.PI;
            return normalized;
        }

        private static Point2D SourceAnchor(RackProjectionSourceView view)
        {
            var facts = view.Placement.Facts;
            var localAnchor = RackViewFrameSemantics.Anchor(view.SourceFrame);
            var phi = facts.RotationRadians + (facts.HasPlanarHalfTurnSign ? Math.PI : 0.0);
            var dx = localAnchor.X - facts.DefinitionOrigin.X;
            var dy = localAnchor.Y - facts.DefinitionOrigin.Y;
            return new Point2D(
                facts.ReferencePosition.X + (Math.Cos(phi) * dx) - (Math.Sin(phi) * dy),
                facts.ReferencePosition.Y + (Math.Sin(phi) * dx) + (Math.Cos(phi) * dy));
        }

        private static Point2D TargetAnchor(RackProjectionSourceView view, RackProjectedPlacement placement)
        {
            var targetLocal = RackViewFrameSemantics.Anchor(view.TargetFrame);
            return new Point2D(
                placement.Position.X
                    + (Math.Cos(placement.RotationRadians) * targetLocal.X)
                    - (Math.Sin(placement.RotationRadians) * targetLocal.Y),
                placement.Position.Y
                    + (Math.Sin(placement.RotationRadians) * targetLocal.X)
                    + (Math.Cos(placement.RotationRadians) * targetLocal.Y));
        }

        private static RackProjectedPlacement Move(
            RackProjectedPlacement placement, double dx = 0.0, double dy = 0.0, double dz = 0.0)
            => new RackProjectedPlacement(
                placement.PhysicalKey,
                placement.RackId,
                placement.TargetAddress,
                new Point3D(placement.Position.X + dx, placement.Position.Y + dy, placement.Position.Z + dz),
                placement.RotationRadians);

        private static RackProjectedPlacement Turn(RackProjectedPlacement placement, double delta)
            => new RackProjectedPlacement(
                placement.PhysicalKey,
                placement.RackId,
                placement.TargetAddress,
                placement.Position,
                placement.RotationRadians + delta);

        /// <summary>A mutant that forgets DefinitionOrigin: it anchors on the raw reference position.</summary>
        private static RackProjectedPlacement IgnoreDefinitionOrigin(
            RackProjectionSourceView view, CommonTransform2D transform)
        {
            var facts = view.Placement.Facts;
            var moved = transform.Apply(new Point2D(facts.ReferencePosition.X, facts.ReferencePosition.Y));
            return new RackProjectedPlacement(
                view.PhysicalKey,
                view.RackId,
                view.TargetAddress,
                new Point3D(moved.X, moved.Y, Target.Z + (facts.ReferencePosition.Z - Base.Z)),
                facts.RotationRadians + (facts.HasPlanarHalfTurnSign ? Math.PI : 0.0));
        }

        private static RackProjectionSourceView RigidView(
            string key,
            string rackId,
            double x,
            double y,
            double rotation,
            double originX = 0.0,
            double originY = 0.0,
            double z = 0.0,
            bool halfTurn = false)
        {
            var facts = G14.Facts(
                x: x,
                y: y,
                z: z,
                rotation: rotation,
                scaleX: halfTurn ? -1.0 : 1.0,
                scaleY: halfTurn ? -1.0 : 1.0,
                originX: originX,
                originY: originY);
            var frame = G14.PlantaFrame(depth: 48.0, runOrigin: 40.0);
            return new RackProjectionSourceView(
                key, rackId, G14.Planta, frame, G14.Planta, frame, RackProjectionSourceTransform.Accept(facts));
        }
    }
}
