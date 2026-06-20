namespace RackCad.Application.Geometry
{
    /// <summary>
    /// Resolves piece placement by MATING named points: two points are made coincident (not
    /// "intersected"). Given a world anchor point and the local point that must land on it, the
    /// solver returns the placement of the piece. Axis-aligned only for now (no rotation), which
    /// is all the schematic header model needs; rotation can be added later without changing callers.
    /// </summary>
    public static class MateSolver
    {
        /// <summary>Place a piece so its local point <paramref name="mateLocal"/> coincides with <paramref name="anchorWorld"/>.</summary>
        public static Placement2D SolveCoincident(Point2D anchorWorld, Point2D mateLocal)
        {
            return new Placement2D(anchorWorld.X - mateLocal.X, anchorWorld.Y - mateLocal.Y);
        }

        /// <summary>World coordinate of a local point once the piece is placed.</summary>
        public static Point2D ToWorld(Placement2D placement, Point2D local)
        {
            return placement.Apply(local);
        }
    }
}
