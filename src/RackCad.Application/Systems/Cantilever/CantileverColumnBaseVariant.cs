using System;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// How the COLUMN's section is turned in the world frame.
    ///
    /// It is a property of the VARIANT and not of the design, because the user does not choose it: the
    /// approved product bolts the base to a particular face, and turning the profile would move the holes
    /// to a face that has none.
    /// </summary>
    public enum CantileverColumnOrientation
    {
        /// <summary>
        /// The section's local Y — its depth — runs along the base direction (world +Y), and its local X
        /// runs across (world +X). For a W that puts the flanges across the run and makes their outer faces
        /// the ones the base meets, which is what makes the two punch columns fall inside the flange width.
        /// </summary>
        DepthAlongBase = 0
    }

    /// <summary>How the BASE's section is turned in the world frame.</summary>
    public enum CantileverBaseOrientation
    {
        /// <summary>
        /// The section's local Y — its depth — is VERTICAL, so the base works as a beam laid on the floor.
        /// Its local X runs across the run.
        /// </summary>
        DepthVertical = 0
    }

    /// <summary>
    /// A registered, approved column–base combination: which two sections, how each one is turned, and the
    /// level of contour detail its drawings use.
    ///
    /// The variant is what the policy REGISTERS. It exists so that eligibility is a lookup of exact ids and
    /// never a rule derived from the family, the designation or a dimension — a rule of that kind would be
    /// the product deciding for itself which pieces exist.
    /// </summary>
    public sealed class CantileverColumnBaseVariant
    {
        public CantileverColumnBaseVariant(
            CantileverColumnBaseVariantKind kind,
            StructuralSectionId columnSectionId,
            StructuralSectionId baseSectionId,
            CantileverColumnOrientation columnOrientation = CantileverColumnOrientation.DepthAlongBase,
            CantileverBaseOrientation baseOrientation = CantileverBaseOrientation.DepthVertical,
            SectionDetailLevel detail = SectionDetailLevel.Tabulated)
        {
            if (columnSectionId.IsEmpty)
            {
                throw new ArgumentException("Una variante necesita el id de la seccion de columna.", nameof(columnSectionId));
            }

            if (baseSectionId.IsEmpty)
            {
                throw new ArgumentException("Una variante necesita el id de la seccion de base.", nameof(baseSectionId));
            }

            Kind = kind;
            ColumnSectionId = columnSectionId;
            BaseSectionId = baseSectionId;
            ColumnOrientation = columnOrientation;
            BaseOrientation = baseOrientation;
            Detail = detail;
        }

        public CantileverColumnBaseVariantKind Kind { get; }

        public StructuralSectionId ColumnSectionId { get; }

        public StructuralSectionId BaseSectionId { get; }

        public CantileverColumnOrientation ColumnOrientation { get; }

        public CantileverBaseOrientation BaseOrientation { get; }

        /// <summary>
        /// The contour detail this variant's geometry is derived at. A VISUAL parameter of the variant, not
        /// of the design: two products may legitimately want different contour fidelity, and neither should
        /// have to be edited to get it.
        /// </summary>
        public SectionDetailLevel Detail { get; }

        /// <summary>True when the same catalogued section is used for both pieces.</summary>
        public bool IsSymmetric => ColumnSectionId == BaseSectionId;

        public override string ToString() =>
            Kind + " " + ColumnSectionId.Value + " / " + BaseSectionId.Value;
    }
}
