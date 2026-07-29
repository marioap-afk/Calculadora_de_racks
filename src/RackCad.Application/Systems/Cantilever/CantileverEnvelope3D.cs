using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// A CONSERVATIVE axis-aligned box around a Cantilever sub-assembly, in the world frame.
    ///
    /// It lives here, in the Cantilever namespace, and NOT as a new 3D primitive of
    /// <c>RackCad.Application.Geometry</c>, on purpose. Adding a cross-cutting 3D bounds type would put a
    /// contract in a shared namespace on the strength of a single consumer; when a second one appears with
    /// its own requirements, promoting this type is a move, whereas retiring a premature shared abstraction
    /// is a migration.
    ///
    /// It is called an ENVELOPE and not "bounds" because it is not a tight projection: it is built from the
    /// outlines and axes the sub-assembly knows, and a piece's true silhouette in a given view is what
    /// <c>StructuralSectionRepresentationPlan.Bounds</c> reports for that view. Naming it "bounds" would
    /// invite somebody to zoom to it and wonder why it does not fit.
    /// </summary>
    public readonly struct CantileverEnvelope3D
    {
        public CantileverEnvelope3D(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            if (!GeometryTolerance.IsFinite(minX) || !GeometryTolerance.IsFinite(minY) ||
                !GeometryTolerance.IsFinite(minZ) || !GeometryTolerance.IsFinite(maxX) ||
                !GeometryTolerance.IsFinite(maxY) || !GeometryTolerance.IsFinite(maxZ))
            {
                throw new ArgumentException("Los limites de la envolvente deben ser finitos.");
            }

            if (maxX < minX || maxY < minY || maxZ < minZ)
            {
                throw new ArgumentException("Los limites de la envolvente estan invertidos.");
            }

            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }

        public double MinX { get; }

        public double MinY { get; }

        public double MinZ { get; }

        public double MaxX { get; }

        public double MaxY { get; }

        public double MaxZ { get; }

        public double Width => MaxX - MinX;

        public double Depth => MaxY - MinY;

        public double Height => MaxZ - MinZ;

        public static CantileverEnvelope3D FromPoints(IEnumerable<Point3D> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var minZ = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;
            var maxZ = double.NegativeInfinity;
            var any = false;

            foreach (var point in points)
            {
                any = true;
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.Z < minZ) minZ = point.Z;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
                if (point.Z > maxZ) maxZ = point.Z;
            }

            if (!any)
            {
                throw new ArgumentException("No se puede calcular una envolvente de un conjunto vacio.", nameof(points));
            }

            return new CantileverEnvelope3D(minX, minY, minZ, maxX, maxY, maxZ);
        }

        public CantileverEnvelope3D Union(CantileverEnvelope3D other) =>
            new CantileverEnvelope3D(
                Math.Min(MinX, other.MinX),
                Math.Min(MinY, other.MinY),
                Math.Min(MinZ, other.MinZ),
                Math.Max(MaxX, other.MaxX),
                Math.Max(MaxY, other.MaxY),
                Math.Max(MaxZ, other.MaxZ));

        public bool ApproxEquals(CantileverEnvelope3D other, double tolerance) =>
            GeometryTolerance.AreClose(MinX, other.MinX, tolerance) &&
            GeometryTolerance.AreClose(MinY, other.MinY, tolerance) &&
            GeometryTolerance.AreClose(MinZ, other.MinZ, tolerance) &&
            GeometryTolerance.AreClose(MaxX, other.MaxX, tolerance) &&
            GeometryTolerance.AreClose(MaxY, other.MaxY, tolerance) &&
            GeometryTolerance.AreClose(MaxZ, other.MaxZ, tolerance);

        public override string ToString() =>
            "[" + MinX.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            MinY.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            MinZ.ToString("0.####", CultureInfo.InvariantCulture) + " .. " +
            MaxX.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            MaxY.ToString("0.####", CultureInfo.InvariantCulture) + ", " +
            MaxZ.ToString("0.####", CultureInfo.InvariantCulture) + "]";
    }
}
