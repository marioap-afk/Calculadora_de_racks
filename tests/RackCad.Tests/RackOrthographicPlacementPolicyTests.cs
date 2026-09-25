using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14 (U, V, W, X, Y): planta against elevations. Owner decision OD-7.e A froze the Relative Frame Window,
    /// so the sense of the common line is read from the source frame of the family and never from a majority.
    /// </summary>
    public class RackOrthographicPlacementPolicyTests
    {
        private static readonly Point3D Base = new Point3D(0, 0, 0);
        private static readonly Point3D Target = new Point3D(500, 0, 0);

        [Fact]
        public void G14_U_FROZEN_CLASS_MAPPINGS_KEEP_THE_CONSERVED_AXIS()
        {
            Assert.True(RackProjectionClassMapping.TryMap(
                DimensionViewKind.Planta, DimensionViewKind.Frontal, out var mode, out var axis));
            Assert.Equal(RackProjectionMode.Orthographic, mode);
            Assert.Equal(RackPhysicalAxis.Run, axis);

            Assert.True(RackProjectionClassMapping.TryMap(
                DimensionViewKind.Planta, DimensionViewKind.Lateral, out _, out axis));
            Assert.Equal(RackPhysicalAxis.Depth, axis);

            Assert.True(RackProjectionClassMapping.TryMap(
                DimensionViewKind.Frontal, DimensionViewKind.Planta, out _, out axis));
            Assert.Equal(RackPhysicalAxis.Run, axis);

            Assert.True(RackProjectionClassMapping.TryMap(
                DimensionViewKind.Lateral, DimensionViewKind.Planta, out _, out axis));
            Assert.Equal(RackPhysicalAxis.Depth, axis);

            Assert.True(RackProjectionClassMapping.TryMap(
                DimensionViewKind.Planta, DimensionViewKind.Planta, out mode, out _));
            Assert.Equal(RackProjectionMode.Rigid, mode);

            Assert.False(RackProjectionClassMapping.TryMap(
                DimensionViewKind.Frontal, DimensionViewKind.Lateral, out _, out _));
            Assert.False(RackProjectionClassMapping.TryMap(
                DimensionViewKind.Lateral, DimensionViewKind.Frontal, out _, out _));
        }

        [Fact]
        public void G14_U_PLANTA_TO_FRONTAL_PLACES_EVERY_RACK_ON_ITS_OWN_RUN_INTERVAL()
        {
            var projection = Project(Row(new[] { 0.0, 100.0, 200.0 }, rotation: 0.0));

            Assert.Equal(new Vector2D(0, 1), Round(projection.Projection.CommonDirection));
            Assert.Equal(new Vector2D(1, 0), Round(projection.Projection.TargetDirection));
            Assert.Equal(-Math.PI / 2.0, projection.Projection.AlphaRadians, 9);
            Assert.Equal(new[] { 500.0, 600.0, 700.0 }, Intervals(projection.Projection));
        }

        [Fact]
        public void G14_V_RELATIVE_FRAME_WINDOW_KEEPS_THE_ORDER_WHEN_EVERY_RACK_IS_TURNED()
        {
            var projection = Project(Row(new[] { 90.0, 190.0, 290.0 }, rotation: Math.PI));

            Assert.Equal(new Vector2D(0, 1), Round(projection.Projection.CommonDirection));
            Assert.Equal(new[] { 500.0, 600.0, 700.0 }, Intervals(projection.Projection));
        }

        [Fact]
        public void G14_V_A_MAJORITY_OF_TURNED_RACKS_DOES_NOT_FLIP_THE_ELEVATION()
        {
            var views = new List<RackProjectionSourceView>
            {
                PlantaView("REF-1", G14.RackA, runPosition: 0.0, rotation: 0.0),
                PlantaView("REF-2", G14.RackB, runPosition: 190.0, rotation: Math.PI),
                PlantaView("REF-3", G14.RackC, runPosition: 290.0, rotation: Math.PI),
            };

            var projection = Project(views);

            Assert.Equal(new Vector2D(0, 1), Round(projection.Projection.CommonDirection));
            Assert.Equal(new[] { 500.0, 600.0, 700.0 }, Intervals(projection.Projection));
        }

        [Theory]
        [InlineData(0.0, 1)]
        [InlineData(Math.PI, -1)]
        [InlineData(-Math.PI, -1)]
        [InlineData((3.0 * Math.PI / 4.0) - 1e-7, 1)]
        [InlineData(3.0 * Math.PI / 4.0, -1)]
        [InlineData((3.0 * Math.PI / 4.0) + 1e-7, -1)]
        [InlineData((2.0 * Math.PI) - 1e-7, 1)]
        public void G14_W_WINDOW_LIMITS_ARE_DETERMINISTIC_AROUND_THREE_QUARTERS_OF_PI(
            double rotation, int expectedSense)
        {
            var projection = Project(Row(new[] { 0.0, 100.0 }, rotation));

            var sourceAxis = Rotate(new Vector2D(0, 1), rotation);
            var expected = Round(new Vector2D(sourceAxis.X * expectedSense, sourceAxis.Y * expectedSense));
            Assert.Equal(expected, Round(projection.Projection.CommonDirection));
        }

        [Fact]
        public void G14_X_RACKS_WITH_DIFFERENT_SPANS_KEEP_THEIR_OWN_LENGTH()
        {
            var views = new List<RackProjectionSourceView>
            {
                PlantaView("REF-1", G14.RackA, runPosition: 0.0, rotation: 0.0, targetLength: 90.0),
                PlantaView("REF-2", G14.RackB, runPosition: 250.0, rotation: Math.PI, targetLength: 150.0),
            };

            var projection = Project(views);
            var placements = Place(projection.Projection);

            Assert.Equal(500.0, placements[0].Position.X, 6);
            Assert.Equal(600.0, placements[1].Position.X, 6);
            Assert.Equal(new[] { 90.0, 150.0 }, projection.Projection.References.Select(x => Math.Round(x.SpanLength, 6)));
        }

        [Fact]
        public void G14_Y_OVERLAP_IS_EVALUATED_ON_SEMANTIC_INTERVALS()
        {
            Assert.False(Overlaps(0.0, 100.0));
            Assert.False(Overlaps(0.0, 90.0));
            Assert.True(Overlaps(0.0, 50.0));
        }

        [Fact]
        public void G14_NON_PARALLEL_SOURCES_FAIL_CLOSED()
        {
            var views = new List<RackProjectionSourceView>
            {
                PlantaView("REF-1", G14.RackA, runPosition: 0.0, rotation: 0.0),
                PlantaView("REF-2", G14.RackB, runPosition: 100.0, rotation: 0.3),
            };

            var projection = Project(views);

            Assert.False(projection.IsAvailable);
            Assert.Equal(RackProjectionFailureCode.NonParallelSources, projection.Failure);
        }

        [Fact]
        public void G14_ORTHOGRAPHIC_TARGET_ROTATION_IS_THE_FROZEN_TARGET_FRAME()
        {
            var projection = Project(Row(new[] { 0.0, 100.0 }, rotation: Math.PI));
            var placements = Place(projection.Projection);

            Assert.All(placements, placement => Assert.Equal(0.0, placement.RotationRadians, 9));
            Assert.All(placements, placement => Assert.Equal(Target.Z, placement.Position.Z, 9));
        }

        private static bool Overlaps(double firstRun, double secondRun)
        {
            var views = new List<RackProjectionSourceView>
            {
                PlantaView("REF-1", G14.RackA, runPosition: firstRun, rotation: 0.0),
                PlantaView("REF-2", G14.RackB, runPosition: secondRun, rotation: 0.0),
            };

            var projection = Project(views).Projection;
            var first = projection.References[0];
            var second = projection.References[1];
            var lowA = Math.Min(first.SourceCoordinate, first.SourceCoordinate + first.SpanLength);
            var highA = Math.Max(first.SourceCoordinate, first.SourceCoordinate + first.SpanLength);
            var lowB = Math.Min(second.SourceCoordinate, second.SourceCoordinate + second.SpanLength);
            var highB = Math.Max(second.SourceCoordinate, second.SourceCoordinate + second.SpanLength);
            return Math.Min(highA, highB) - Math.Max(lowA, lowB) > GeometryTolerance.Length;
        }

        internal static List<RackProjectionSourceView> Row(double[] runPositions, double rotation)
            => runPositions
                .Select((run, index) => PlantaView(
                    "REF-" + index,
                    index == 0 ? G14.RackA : index == 1 ? G14.RackB : G14.RackC,
                    run,
                    rotation))
                .ToList();

        internal static RackProjectionSourceView PlantaView(
            string key, string rackId, double runPosition, double rotation, double targetLength = 90.0)
        {
            var facts = G14.Facts(x: 0.0, y: runPosition, rotation: rotation);
            return new RackProjectionSourceView(
                key,
                rackId,
                G14.Planta,
                G14.PlantaFrame(),
                G14.Frontal0,
                G14.FrontalFrame(runOrigin: 0.0, length: targetLength),
                RackProjectionSourceTransform.Accept(facts));
        }

        private static RackOrthographicProjectionResult Project(IReadOnlyList<RackProjectionSourceView> views)
            => RackOrthographicPlacementPolicy.Project(
                views,
                RackPhysicalAxis.Run,
                RackProjectionFamily.Rack,
                SourceGroupFrame.Universal(Base),
                TargetGroupFrame.Universal(Target));

        private static IReadOnlyList<RackProjectedPlacement> Place(RackOrthographicProjection projection)
            => projection.References
                .Select(reference => RackOrthographicPlacementPolicy.Place(projection, reference, Base, Target))
                .ToList();

        private static double[] Intervals(RackOrthographicProjection projection)
            => Place(projection).Select(placement => Math.Round(placement.Position.X, 6)).ToArray();

        private static Vector2D Rotate(Vector2D vector, double angle)
            => new Vector2D(
                (Math.Cos(angle) * vector.X) - (Math.Sin(angle) * vector.Y),
                (Math.Sin(angle) * vector.X) + (Math.Cos(angle) * vector.Y));

        private static Vector2D Round(Vector2D vector)
            => new Vector2D(Math.Round(vector.X, 6), Math.Round(vector.Y, 6));
    }
}
