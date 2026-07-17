using RackCad.Application.Geometry;

namespace RackCad.Application.Systems
{
    /// <summary>Resolved origins for a post derived at the boundary between consecutive separators.</summary>
    public readonly struct DynamicDerivedPostPlacement
    {
        public DynamicDerivedPostPlacement(
            Point2D primaryOrigin,
            bool hasReinforcement,
            Point2D reinforcementOrigin)
        {
            PrimaryOrigin = primaryOrigin;
            HasReinforcement = hasReinforcement;
            ReinforcementOrigin = reinforcementOrigin;
        }

        public Point2D PrimaryOrigin { get; }
        public bool HasReinforcement { get; }
        public Point2D ReinforcementOrigin { get; }
    }

    /// <summary>
    /// Centers a derived reinforced post on its separator boundary. FIN_POSTE is the physical interface where the
    /// primary profile ends and the reinforcement begins, so that mate — not the primary origin — must coincide
    /// with the boundary. A non-reinforced post keeps its origin directly on the boundary.
    /// </summary>
    public static class DynamicDerivedPostGeometry
    {
        public static DynamicDerivedPostPlacement Resolve(
            double boundaryX,
            bool reinforced,
            Point2D finPoste)
        {
            var boundary = new Point2D(boundaryX, 0.0);
            if (!reinforced)
            {
                return new DynamicDerivedPostPlacement(boundary, false, boundary);
            }

            var primary = new Point2D(boundary.X - finPoste.X, boundary.Y - finPoste.Y);
            return new DynamicDerivedPostPlacement(primary, true, boundary);
        }
    }
}
