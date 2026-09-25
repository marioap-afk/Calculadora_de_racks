using System;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14 (O, P, Q, R, S, T): same class projection. The layout of semantic anchors is reproduced by the single
    /// common transform; DefinitionOrigin never contaminates the placement and AUTH-08 V2 decides what is admissible.
    /// </summary>
    public class RackRigidPlacementPolicyTests
    {
        private static readonly Point3D Base = new Point3D(0, 0, 1);
        private static readonly Point3D Target = new Point3D(1000, 0, 5);

        [Fact]
        public void G14_O_RIGID_WITH_ALPHA_ZERO_TRANSLATES_THE_SOURCE_ANCHOR()
        {
            var view = View("REF-1", G14.RackA, G14.Facts(x: 100, y: 50, z: 3));
            var transform = CommonTransform2D.TryCreate(Base, Target, 0.0).Transform;

            var placement = RackRigidPlacementPolicy.Place(view, transform);

            Assert.Equal(1100.0, placement.Position.X, 9);
            Assert.Equal(50.0, placement.Position.Y, 9);
            Assert.Equal(7.0, placement.Position.Z, 9);
            Assert.Equal(0.0, placement.RotationRadians, 9);
            Assert.Equal(1.0, placement.Scale, 12);
            Assert.Equal(G14.RackA, placement.RackId);
        }

        [Fact]
        public void G14_P_DEFINITION_ORIGIN_DOES_NOT_CONTAMINATE_THE_SEMANTIC_PLACEMENT()
        {
            var facts = G14.Facts(x: 100, y: 50, z: 3, rotation: Math.PI / 6.0, originX: 12, originY: -7);
            var view = View("REF-1", G14.RackA, facts, sourceAnchorRun: 40.0);
            var transform = CommonTransform2D.TryCreate(Base, Target, 0.0).Transform;

            var placement = RackRigidPlacementPolicy.Place(view, transform);

            var expected = Oracle(facts, localAnchor: new Point2D(0, 40), alpha: 0.0);
            Assert.Equal(expected.X, placement.Position.X, 6);
            Assert.Equal(expected.Y, placement.Position.Y, 6);
            Assert.Equal(Math.PI / 6.0, placement.RotationRadians, 9);
        }

        [Fact]
        public void G14_RIGID_KEEPS_THE_HALF_TURN_OF_A_180_DEGREE_SOURCE()
        {
            var facts = G14.Facts(x: 100, y: 50, rotation: 0.0, scaleX: -1, scaleY: -1, originX: 12, originY: -7);
            var view = View("REF-1", G14.RackA, facts, sourceAnchorRun: 40.0);
            var transform = CommonTransform2D.TryCreate(Base, Target, 0.0).Transform;

            var placement = RackRigidPlacementPolicy.Place(view, transform);

            Assert.Equal(Math.PI, Math.Abs(placement.RotationRadians), 9);
            var expected = Oracle(facts, localAnchor: new Point2D(0, 40), alpha: 0.0);
            Assert.Equal(expected.X, placement.Position.X, 6);
            Assert.Equal(expected.Y, placement.Position.Y, 6);
        }

        [Theory]
        [InlineData(-1.0, 1.0, 1.0, RackProjectionFailureCode.ReflectionNotAllowed)]
        [InlineData(1.0, -1.0, 1.0, RackProjectionFailureCode.ReflectionNotAllowed)]
        [InlineData(2.0, 2.0, 2.0, RackProjectionFailureCode.NonUnitScale)]
        [InlineData(1.0, 2.0, 1.0, RackProjectionFailureCode.NonUniformScale)]
        [InlineData(1.0, 1.0, -1.0, RackProjectionFailureCode.NegativeZScale)]
        public void G14_Q_R_S_T_AUTH08_FACTS_REJECT_TRANSFORMS_ID19_DOES_NOT_ADMIT(
            double scaleX, double scaleY, double scaleZ, RackProjectionFailureCode expected)
        {
            var acceptance = RackProjectionSourceTransform.Accept(
                G14.Facts(x: 0, y: 0, scaleX: scaleX, scaleY: scaleY, scaleZ: scaleZ));

            Assert.False(acceptance.IsAccepted);
            Assert.Equal(expected, acceptance.Failure);
        }

        [Fact]
        public void G14_AUTH08_FACTS_REJECT_A_NORMAL_THAT_IS_NOT_WORLD_Z()
        {
            var acceptance = RackProjectionSourceTransform.Accept(
                G14.Facts(x: 0, y: 0, normalX: 0.0, normalY: 1.0, normalZ: 0.0));

            Assert.False(acceptance.IsAccepted);
            Assert.Equal(RackProjectionFailureCode.NormalNotWorldZ, acceptance.Failure);
        }

        [Fact]
        public void G14_AUTH08_OUTCOMES_TRAVEL_AS_PRODUCT_FAILURES_WITHOUT_LOCAL_CLASSIFICATION()
        {
            Assert.Equal(
                RackProjectionFailureCode.SourceFactsNonFinite,
                RackProjectionSourceTransform.Accept(G14.Facts(x: double.NaN, y: 0)).Failure);
            Assert.Equal(
                RackProjectionFailureCode.SourceFactsDegenerate,
                RackProjectionSourceTransform.Accept(G14.Facts(x: 0, y: 0, scaleX: 0.0)).Failure);
            Assert.Equal(
                RackProjectionFailureCode.SourceFactsInvalidTolerance,
                RackProjectionSourceTransform.Accept(
                    RackSourceTransformClassifier.Classify(
                        new RackSourcePlacementInput(0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0),
                        new RackSourceTransformTolerance(-1, -1, -1))).Failure);
        }

        [Fact]
        public void G14_RIGID_PRESERVES_THE_RELATIVE_LAYOUT_OF_TWO_RACKS()
        {
            var first = View("REF-1", G14.RackA, G14.Facts(x: 0, y: 0));
            var second = View("REF-2", G14.RackB, G14.Facts(x: 0, y: 100));
            var transform = CommonTransform2D.TryCreate(Base, Target, 0.0).Transform;

            var a = RackRigidPlacementPolicy.Place(first, transform);
            var b = RackRigidPlacementPolicy.Place(second, transform);

            Assert.Equal(0.0, b.Position.X - a.Position.X, 9);
            Assert.Equal(100.0, b.Position.Y - a.Position.Y, 9);
        }

        internal static RackProjectionSourceView View(
            string key, string rackId, RackSourceTransformFactsResult facts, double sourceAnchorRun = 0.0)
        {
            var frame = G14.PlantaFrame(depth: 48.0, runOrigin: sourceAnchorRun);
            return new RackProjectionSourceView(
                key, rackId, G14.Planta, frame, G14.Planta, frame, RackProjectionSourceTransform.Accept(facts));
        }

        /// <summary>Independent oracle: world = Position + R(rotation) * Scale * (local - DefinitionOrigin).</summary>
        internal static Point2D Oracle(
            RackSourceTransformFactsResult factsResult, Point2D localAnchor, double alpha)
        {
            var facts = factsResult.Facts;
            var half = facts.SignX == RackScaleSign.Negative && facts.SignY == RackScaleSign.Negative;
            var phi = facts.RotationRadians + (half ? Math.PI : 0.0);
            var dx = localAnchor.X - facts.DefinitionOrigin.X;
            var dy = localAnchor.Y - facts.DefinitionOrigin.Y;
            var worldX = facts.ReferencePosition.X + (Math.Cos(phi) * dx) - (Math.Sin(phi) * dy);
            var worldY = facts.ReferencePosition.Y + (Math.Sin(phi) * dx) + (Math.Cos(phi) * dy);

            var movedX = Target.X + (Math.Cos(alpha) * (worldX - Base.X)) - (Math.Sin(alpha) * (worldY - Base.Y));
            var movedY = Target.Y + (Math.Sin(alpha) * (worldX - Base.X)) + (Math.Cos(alpha) * (worldY - Base.Y));

            var rho = phi + alpha;
            return new Point2D(
                movedX - ((Math.Cos(rho) * localAnchor.X) - (Math.Sin(rho) * localAnchor.Y)),
                movedY - ((Math.Sin(rho) * localAnchor.X) + (Math.Cos(rho) * localAnchor.Y)));
        }
    }
}
