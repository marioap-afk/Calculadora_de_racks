using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The ARRAY grouper: identical pieces collapse into one nested definition referenced N times, singletons and
    /// annotations stay loose, and <see cref="DynamicSystemPlan.Flatten"/> reproduces the input exactly.
    /// </summary>
    public class HeaderInstanceGrouperTests
    {
        private static HeaderBlockInstance Beam(double x, double y, double length)
        {
            var beam = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = "BEAM",
                BlockName = "BEAM_BLOCK",
                View = "frontal",
                Insertion = new Point2D(x, y),
                ConnectionAnchor = new Point2D(x, y)
            };
            beam.DynamicParameters["LONGITUD"] = length;
            return beam;
        }

        [Fact]
        public void Group_CollapsesIdenticalPieces_IntoOneDefinitionWithManyPlacements()
        {
            var instances = new[] { Beam(0, 10, 96), Beam(50, 10, 96), Beam(100, 10, 96) };

            var plan = HeaderInstanceGrouper.Group(instances, "T");

            var group = Assert.Single(plan.Headers);
            Assert.Single(group.Instances);                 // one shared definition
            Assert.Equal(3, group.Placements.Count);        // referenced at all three positions
            Assert.Empty(plan.LooseInstances);
        }

        [Fact]
        public void Group_KeepsSingletonsLoose()
        {
            var instances = new[] { Beam(0, 10, 96), Beam(50, 10, 120) }; // different lengths → distinct signatures

            var plan = HeaderInstanceGrouper.Group(instances, "T");

            Assert.Empty(plan.Headers);                     // neither repeats, so no nested def is worth creating
            Assert.Equal(2, plan.LooseInstances.Count);
        }

        [Fact]
        public void Group_KeepsAnnotationsLoose()
        {
            var annotation = new HeaderBlockInstance { Role = HeaderBlockRole.Annotation, Text = "1", Insertion = new Point2D(5, 5) };
            var instances = new[] { Beam(0, 10, 96), Beam(50, 10, 96), annotation };

            var plan = HeaderInstanceGrouper.Group(instances, "T");

            Assert.Single(plan.Headers);                    // the two beams group
            Assert.Contains(plan.LooseInstances, i => i.Role == HeaderBlockRole.Annotation);
        }

        [Fact]
        public void Group_DoesNotMergePiecesDifferingOnlyInMirroredY()
        {
            // The protector lateral flips its guide via MirroredY (Y scale -1). Two otherwise-identical pieces that
            // differ only in MirroredY are NOT interchangeable and must land in separate nested definitions.
            HeaderBlockInstance Guard(double x, bool mirroredY) => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = "GUARD",
                BlockName = "GUARD_BLOCK",
                View = "planta",
                Insertion = new Point2D(x, 0),
                ConnectionAnchor = new Point2D(x, 0),
                MirroredY = mirroredY
            };

            var instances = new[] { Guard(0, false), Guard(50, false), Guard(0, true), Guard(50, true) };

            var plan = HeaderInstanceGrouper.Group(instances, "T");

            Assert.Equal(2, plan.Headers.Count);                        // one group per MirroredY value, not a merge
            Assert.All(plan.Headers, g => Assert.Single(g.Instances));
            Assert.All(plan.Headers, g => Assert.Equal(2, g.Placements.Count));
        }

        [Fact]
        public void Group_PreservesMirroredY_InDefinitionAndThroughFlatten()
        {
            HeaderBlockInstance Guard(double x) => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = "GUARD",
                BlockName = "GUARD_BLOCK",
                View = "planta",
                Insertion = new Point2D(x, 0),
                ConnectionAnchor = new Point2D(x, 0),
                MirroredY = true
            };

            var plan = HeaderInstanceGrouper.Group(new[] { Guard(0), Guard(50) }, "T");

            var group = Assert.Single(plan.Headers);
            Assert.True(group.Instances[0].MirroredY);                  // the nested definition bakes in the Y-mirror
            Assert.All(plan.Flatten().Instances, i => Assert.True(i.MirroredY)); // and it survives reconstruction
        }

        [Fact]
        public void Group_PreservesInsertionAnchorOffset_ForPlateLikePieces()
        {
            // A base plate: its Insertion is offset from its ConnectionAnchor by the mate point. Grouping must land
            // every placement back at the ORIGINAL Insertion, not on the anchor.
            HeaderBlockInstance Plate(double anchorX) => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.BasePlate,
                PieceId = "PLATE",
                BlockName = "PLATE_BLOCK",
                View = "frontal",
                ConnectionAnchor = new Point2D(anchorX, 0),
                Insertion = new Point2D(anchorX - 4.0, -1.0) // mate offset (−4, −1)
            };

            var instances = new[] { Plate(0), Plate(50) };

            var plan = HeaderInstanceGrouper.Group(instances, "T");
            var flat = plan.Flatten().Instances.OrderBy(i => i.ConnectionAnchor.X).ToList();

            Assert.Equal(2, flat.Count);
            Assert.Equal(0.0, flat[0].ConnectionAnchor.X, 6);
            Assert.Equal(-4.0, flat[0].Insertion.X, 6);   // anchor 0 − mate 4
            Assert.Equal(-1.0, flat[0].Insertion.Y, 6);
            Assert.Equal(50.0, flat[1].ConnectionAnchor.X, 6);
            Assert.Equal(46.0, flat[1].Insertion.X, 6);   // anchor 50 − mate 4
        }
    }
}
