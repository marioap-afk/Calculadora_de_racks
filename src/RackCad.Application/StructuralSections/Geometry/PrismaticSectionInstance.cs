using System;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// A length of a catalogued section, placed and oriented.
    ///
    /// This is where LENGTH lives, and the reason it lives here rather than in
    /// <see cref="StructuralSectionDefinition"/> is not tidiness: a cross-section does not have a length, and
    /// putting one in the catalogue would turn <c>W12X26</c> from a row into a row per size (ADR-0020, §4 of
    /// ADR-0022).
    ///
    /// It is NOT a member. There is no role, no material, no connection, no end preparation and no fabrication
    /// rule here, and none of those will be added: a prismatic instance is geometry with a length, and
    /// <c>StructuralMember</c> belongs to the configurators I-37 will build.
    ///
    /// Axes follow ADR-0022: the section spans local X and Y, Z is the longitudinal axis, and the piece runs
    /// from z = 0 to z = <see cref="Length"/> in its own frame.
    /// </summary>
    public sealed class PrismaticSectionInstance
    {
        private PrismaticSectionInstance(
            StructuralSectionId sectionId,
            double length,
            LocalFrame3D frame,
            double rotationRadians,
            bool mirrored)
        {
            SectionId = sectionId;
            Length = length;
            Frame = frame;
            RotationRadians = rotationRadians;
            Mirrored = mirrored;
        }

        public StructuralSectionId SectionId { get; }

        /// <summary>Length along the local Z axis, in inches. Always strictly positive and finite.</summary>
        public double Length { get; }

        /// <summary>Where the piece sits and how it is oriented. Its Z is the longitudinal axis.</summary>
        public LocalFrame3D Frame { get; }

        /// <summary>Rotation of the SECTION about the longitudinal axis, counter-clockwise, in radians.</summary>
        public double RotationRadians { get; }

        public double RotationDegrees => RotationRadians * 180.0 / Math.PI;

        /// <summary>Whether the section is mirrored about its local Y axis before being placed.</summary>
        public bool Mirrored { get; }

        public static PrismaticSectionInstance Create(
            StructuralSectionId sectionId,
            double length,
            LocalFrame3D frame = null,
            double rotationDegrees = 0.0,
            bool mirrored = false)
        {
            if (sectionId.IsEmpty)
            {
                throw new ArgumentException("Una instancia prismatica necesita el id de su seccion.", nameof(sectionId));
            }

            if (!GeometryTolerance.IsFinite(length))
            {
                throw new ArgumentException(
                    "La longitud debe ser un numero finito; se recibio " + length + ".", nameof(length));
            }

            if (length <= 0.0)
            {
                throw new ArgumentException(
                    "La longitud debe ser positiva; se recibio " + length + ".", nameof(length));
            }

            if (!GeometryTolerance.IsFinite(rotationDegrees))
            {
                throw new ArgumentException("La rotacion debe ser finita.", nameof(rotationDegrees));
            }

            return new PrismaticSectionInstance(
                sectionId, length, frame ?? LocalFrame3D.World,
                rotationDegrees * Math.PI / 180.0, mirrored);
        }

        /// <summary>
        /// How the section's own coordinates are mapped before being placed: the mirror first, then the
        /// rotation. Neither changes area or weight — they move material, they do not create or destroy it.
        /// </summary>
        public Transform2D SectionTransform()
        {
            var transform = Mirrored ? Transform2D.MirrorAboutY : Transform2D.Identity;
            return transform.Then(Transform2D.Rotation(RotationRadians));
        }

        /// <summary>
        /// Total weight of the run, in the section's native mass unit.
        ///
        /// It reuses I-36A's authority — <c>WeightPerLength × Length</c> — instead of deriving anything from
        /// the geometry. The contour is a representation; the weight is a published datum, and computing it
        /// from an area that deliberately omits toe fillets would be worse than useless.
        /// </summary>
        public double Weight(StructuralSectionDefinition section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (section.SectionId != SectionId)
            {
                throw new ArgumentException(
                    "La seccion '" + section.SectionId + "' no es la de esta instancia ('" + SectionId + "').",
                    nameof(section));
            }

            return StructuralSectionUnits.Weight(section, Length);
        }

        public PrismaticSectionInstance WithLength(double length) =>
            Create(SectionId, length, Frame, RotationDegrees, Mirrored);

        public PrismaticSectionInstance WithRotation(double rotationDegrees) =>
            Create(SectionId, Length, Frame, rotationDegrees, Mirrored);

        public PrismaticSectionInstance WithMirror(bool mirrored) =>
            Create(SectionId, Length, Frame, RotationDegrees, mirrored);

        public PrismaticSectionInstance WithFrame(LocalFrame3D frame) =>
            Create(SectionId, Length, frame, RotationDegrees, Mirrored);

        public override string ToString() =>
            SectionId.Value + " L=" + Length.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) +
            (Mirrored ? " espejeada" : string.Empty) +
            (Math.Abs(RotationRadians) > GeometryTolerance.Angle
                ? " rot=" + RotationDegrees.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "deg"
                : string.Empty);
    }
}
