using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Geometry
{
    public sealed class BasePlatePlacement2D
    {
        public PostSide Side { get; set; }
        public string PlateCatalogId { get; set; }
        public string ConnectionPointId { get; set; }

        /// <summary>The post-base world point the plate's connection point is mated to.</summary>
        public Point2D Anchor { get; set; }

        /// <summary>Resolved placement of the plate's local origin in model space.</summary>
        public Placement2D Placement { get; set; }
    }

    /// <summary>
    /// Resolves absolute positions in the abstract model plane by mating named connection points.
    /// The header lies in the depth plane: the left post sits at X=0 and the right post at X=Depth,
    /// both anchored at elevation Y=0. A base plate is positioned by making its catalog connection
    /// point coincide with the corresponding post-base anchor. No AutoCAD involved.
    /// </summary>
    public static class FramePlacementResolver
    {
        public static IReadOnlyList<BasePlatePlacement2D> ResolveBasePlates(RackFrameConfiguration configuration, RackCatalog catalog)
        {
            var results = new List<BasePlatePlacement2D>();

            if (configuration == null)
            {
                return results;
            }

            AddPlate(results, configuration.LeftBasePlate, PostSide.Left, anchorX: 0.0, catalog);
            AddPlate(results, configuration.RightBasePlate, PostSide.Right, anchorX: configuration.Depth, catalog);
            return results;
        }

        private static void AddPlate(List<BasePlatePlacement2D> results, BasePlatePlacement plate, PostSide side, double anchorX, RackCatalog catalog)
        {
            if (plate == null || string.IsNullOrWhiteSpace(plate.PlateCatalogId))
            {
                return;
            }

            var connectionPoint = catalog?.ConnectionPoints.FindConnectionPoint(plate.ConnectionPointId);
            var local = connectionPoint == null
                ? new Point2D(0.0, 0.0)
                : new Point2D(connectionPoint.LocalX, connectionPoint.LocalY);
            var anchor = new Point2D(anchorX, 0.0);

            results.Add(new BasePlatePlacement2D
            {
                Side = side,
                PlateCatalogId = plate.PlateCatalogId,
                ConnectionPointId = plate.ConnectionPointId,
                Anchor = anchor,
                Placement = MateSolver.SolveCoincident(anchor, local)
            });
        }
    }
}
