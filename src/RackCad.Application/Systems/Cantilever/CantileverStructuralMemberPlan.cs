using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// A resolved MEMBER: a catalogued section, placed, with a role.
    ///
    /// This is the type ADR-0020 anticipated when it said a section "is not a post, a beam, a spacer, a
    /// cantilever arm or a column" and that the role belongs to the configurators. One type covers every
    /// piece whose material is a catalogue section — column and base today, arm and brace later — because
    /// they differ in ROLE, not in nature.
    ///
    /// <b><see cref="Placement"/> is the single authority of placement.</b> Section, length, frame, rotation
    /// and mirror live there and nowhere else; <see cref="SectionId"/>, <see cref="GeometricLength"/>,
    /// <see cref="Start"/> and <see cref="End"/> are computed from it with no backing field. Storing any of
    /// them would be a second truth that goes stale the moment somebody calls <c>WithLength</c>.
    ///
    /// What is deliberately absent: weight (I-37A does not compute it; <c>WeightPerLength</c> is the only
    /// authority and its consumer, the BOM, does not exist yet), a BOM flag (if it is here, it is steel),
    /// and any fabrication concept.
    /// </summary>
    public sealed class CantileverStructuralMemberPlan
    {
        private CantileverStructuralMemberPlan(
            CantileverPieceId id,
            CantileverMemberRole role,
            string owner,
            PrismaticSectionInstance placement,
            double nominalCutLength,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            Id = id;
            Role = role;
            Owner = owner;
            Placement = placement;
            NominalCutLength = nominalCutLength;
            Diagnostics = diagnostics;
        }

        public CantileverPieceId Id { get; }

        public CantileverMemberRole Role { get; }

        /// <summary>The sub-assembly this member belongs to. One string, not a back-pointer to a mutable object.</summary>
        public string Owner { get; }

        /// <summary>The ONE authority: section, length, frame, rotation and mirror.</summary>
        public PrismaticSectionInstance Placement { get; }

        /// <summary>
        /// The length the future BOM will quote, inches.
        ///
        /// In the MVP it EQUALS <see cref="GeometricLength"/>, by definition and with a test. It carries no
        /// end preparation, no cut allowance, no assembly tolerance and no kerf, and it is NOT released for
        /// CNC or for a shop drawing — every one of those is outside I-37 by the owner's decision. It exists
        /// with a name of its own, rather than as a silent alias, so that the day fabrication enters scope
        /// there is a field to put it in and a test that notices it stopped being an identity.
        /// </summary>
        public double NominalCutLength { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        // ---- derived: no backing fields, no second truth -------------------------------------------------

        public StructuralSectionId SectionId => Placement.SectionId;

        /// <summary>Length along the member's own axis, inches.</summary>
        public double GeometricLength => Placement.Length;

        public Point3D Start => Placement.Frame.Origin;

        public Point3D End => Placement.Frame.ToWorld(new Point3D(0.0, 0.0, Placement.Length));

        /// <summary>The member's longitudinal direction in world coordinates.</summary>
        public Vector3D Direction => Placement.Frame.AxisZ;

        public static CantileverStructuralMemberPlan Create(
            CantileverPieceId id,
            CantileverMemberRole role,
            string owner,
            PrismaticSectionInstance placement,
            IEnumerable<CantileverDiagnostic> diagnostics = null)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException("Un miembro necesita su id.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("Un miembro necesita su propietario.", nameof(owner));
            }

            if (placement == null)
            {
                throw new ArgumentNullException(nameof(placement));
            }

            var list = diagnostics == null
                ? (IReadOnlyList<CantileverDiagnostic>)Array.Empty<CantileverDiagnostic>()
                : diagnostics.ToList();

            // The MVP identity, asserted where it is created rather than trusted at the call sites.
            return new CantileverStructuralMemberPlan(id, role, owner, placement, placement.Length, list);
        }

        public override string ToString() =>
            Id.Value + " " + Role + " " + Placement;
    }
}
