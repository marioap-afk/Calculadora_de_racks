using System;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// The standard directions a prismatic instance can be looked at from.
    ///
    /// Deliberately NOT called "frontal", "lateral" or "planta". Those names describe how a member ends up
    /// placed in a system, and a section knows nothing about that: the same channel is "frontal" in one rack
    /// and "lateral" in another. Naming the views after the LOCAL axes keeps the core honest and leaves the
    /// system-specific vocabulary where it belongs.
    /// </summary>
    public enum SectionViewKind
    {
        /// <summary>Looking along Z: the cross-section itself, true shape, no foreshortening.</summary>
        CrossSection = 0,

        /// <summary>Looking along X: the piece seen sideways, its length across the view.</summary>
        LongitudinalX = 1,

        /// <summary>Looking along Y: the other side view.</summary>
        LongitudinalY = 2,

        /// <summary>The standard isometric direction, equally inclined to the three axes.</summary>
        Isometric = 3,

        /// <summary>Any other direction, given by an explicit camera frame.</summary>
        Custom = 4
    }

    /// <summary>
    /// An orthographic camera: where the viewer looks FROM and which way is up.
    ///
    /// Projection is parallel, never perspective. A perspective view of a structural profile would be
    /// prettier and useless for anything measurable, and everything downstream — bounds, fitting the preview,
    /// materialising in AutoCAD — assumes distances survive the projection.
    ///
    /// The camera is a <see cref="LocalFrame3D"/>: its X and Y span the picture plane and its Z is the
    /// viewing direction. Reusing the frame type instead of inventing a camera type is not a shortcut —
    /// orthonormality is exactly the property a camera needs, and it is already enforced there.
    /// </summary>
    public sealed class SectionViewpoint
    {
        private SectionViewpoint(SectionViewKind kind, LocalFrame3D camera)
        {
            Kind = kind;
            Camera = camera;
        }

        public SectionViewKind Kind { get; }

        /// <summary>Camera frame: X right, Y up, Z towards the viewer.</summary>
        public LocalFrame3D Camera { get; }

        /// <summary>Looking down the Z axis: the true cross-section, X right and Y up.</summary>
        public static SectionViewpoint CrossSection { get; } =
            new SectionViewpoint(SectionViewKind.CrossSection, LocalFrame3D.World);

        /// <summary>
        /// Looking along the X axis. The length (model Z) runs across the picture and the section's Y stays
        /// up, which is what makes a side elevation readable.
        /// </summary>
        public static SectionViewpoint LongitudinalX { get; } =
            new SectionViewpoint(
                SectionViewKind.LongitudinalX,
                LocalFrame3D.Camera(new Vector3D(-1.0, 0.0, 0.0), Vector3D.UnitY));

        /// <summary>Looking along the Y axis. The length runs across the picture and the section's X is up.</summary>
        public static SectionViewpoint LongitudinalY { get; } =
            new SectionViewpoint(
                SectionViewKind.LongitudinalY,
                LocalFrame3D.Camera(Vector3D.UnitY, Vector3D.UnitX));

        /// <summary>
        /// The standard isometric direction (1,1,1). Every axis is foreshortened by the same factor, so the
        /// three of them read at the same scale — which is the whole point of an isometric.
        /// </summary>
        public static SectionViewpoint Isometric { get; } =
            new SectionViewpoint(
                SectionViewKind.Isometric,
                LocalFrame3D.Camera(new Vector3D(1.0, 1.0, 1.0), Vector3D.UnitZ));

        /// <summary>An arbitrary orthographic direction. The frame is validated by <see cref="LocalFrame3D"/>.</summary>
        public static SectionViewpoint Custom(Vector3D viewDirection, Vector3D upReference) =>
            new SectionViewpoint(SectionViewKind.Custom, LocalFrame3D.Camera(viewDirection, upReference));

        public static SectionViewpoint Standard(SectionViewKind kind)
        {
            switch (kind)
            {
                case SectionViewKind.CrossSection: return CrossSection;
                case SectionViewKind.LongitudinalX: return LongitudinalX;
                case SectionViewKind.LongitudinalY: return LongitudinalY;
                case SectionViewKind.Isometric: return Isometric;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(kind), kind, "Una vista personalizada necesita su marco de camara.");
            }
        }

        /// <summary>Projects a model point onto the picture plane. Parallel projection: depth is discarded.</summary>
        public Point2D Project(Point3D world) =>
            new Point2D(
                (world.X * Camera.AxisX.X) + (world.Y * Camera.AxisX.Y) + (world.Z * Camera.AxisX.Z),
                (world.X * Camera.AxisY.X) + (world.Y * Camera.AxisY.Y) + (world.Z * Camera.AxisY.Z));

        /// <summary>How far a model point is from the viewer. Not used to hide anything — see the plan's docs.</summary>
        public double Depth(Point3D world) =>
            (world.X * Camera.AxisZ.X) + (world.Y * Camera.AxisZ.Y) + (world.Z * Camera.AxisZ.Z);

        /// <summary>
        /// True when the picture plane is parallel to the section plane of a member laid along WORLD Z, so its
        /// arcs stay circular.
        ///
        /// It answers a question about the camera alone, which is enough while every viewpoint is expressed in
        /// the section's own axes — there, «along world Z» and «along the member» are the same sentence. They
        /// stop being the same the moment a member is laid down: see <see cref="PreservesShapeOf"/>, which is
        /// what a placed member must ask.
        /// </summary>
        public bool PreservesSectionShape =>
            Math.Abs(Math.Abs(Camera.AxisZ.Z) - 1.0) <= 1e-9;

        /// <summary>
        /// True when this camera looks along the given member's OWN longitudinal axis, so that member's section
        /// shape is preserved.
        ///
        /// A camera looking straight down preserves the shape of a standing column and NOT that of a base lying
        /// on the floor, and only the member's own axis can tell the two apart. Asking the camera about world Z
        /// instead drew the base of every Cantilever station as a single 6.49 in line with no length at all —
        /// its two end faces were taken for one and its generatrices were never emitted. That is motivo 2 of the
        /// round-2 rejection of I-37D.
        ///
        /// For a viewpoint expressed in section axes the member's axis IS world Z, so this agrees with
        /// <see cref="PreservesSectionShape"/> exactly and nothing about a standalone section drawing changes.
        /// </summary>
        public bool PreservesShapeOf(Vector3D memberAxis)
        {
            var axis = memberAxis.Normalized();

            var alignment =
                (axis.X * Camera.AxisZ.X) + (axis.Y * Camera.AxisZ.Y) + (axis.Z * Camera.AxisZ.Z);

            return Math.Abs(Math.Abs(alignment) - 1.0) <= 1e-9;
        }

        public override string ToString() => Kind.ToString();
    }
}
