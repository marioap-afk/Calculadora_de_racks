using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// How an arm's section is turned in its own frame.
    ///
    /// One value today, and an enum anyway for the same reason the column has one: adding the second is a
    /// value plus a branch in an authority, not a reinterpretation of what the first meant.
    /// </summary>
    public enum CantileverArmBodyOrientation
    {
        /// <summary>
        /// The section's local Y — its depth — is the perpendicular to the arm's axis that lies in the
        /// vertical plane and points up; its local X runs across the run. For a channel that puts <c>d</c>
        /// vertical-ish and <c>bf</c> transverse, which is what makes a pair sit side by side.
        /// </summary>
        DepthPerpendicularToAxis = 0
    }

    /// <summary>
    /// A registered, approved arm body: which section, in which arrangement, turned how, at which contour
    /// detail.
    ///
    /// It does NOT carry the per-member mirror and offset. Those are derived by
    /// <see cref="CantileverArmBodyArrangementResolver"/> from <c>Bounds</c>; a mirror written by hand in a
    /// variant would be a second authority for the thing ADR-0025 D2 put in one place.
    /// </summary>
    public sealed class CantileverArmVariant
    {
        public CantileverArmVariant(
            StructuralSectionId sectionId,
            CantileverArmBodyArrangement arrangement,
            CantileverArmBodyOrientation orientation = CantileverArmBodyOrientation.DepthPerpendicularToAxis,
            SectionDetailLevel detail = SectionDetailLevel.Tabulated)
        {
            if (sectionId.IsEmpty)
            {
                throw new ArgumentException("Una variante de brazo necesita el id de su seccion.", nameof(sectionId));
            }

            SectionId = sectionId;
            Arrangement = arrangement;
            Orientation = orientation;
            Detail = detail;
        }

        public StructuralSectionId SectionId { get; }

        public CantileverArmBodyArrangement Arrangement { get; }

        public CantileverArmBodyOrientation Orientation { get; }

        /// <summary>The contour detail this variant's geometry is derived at. A visual parameter of the variant.</summary>
        public SectionDetailLevel Detail { get; }

        public override string ToString() => Arrangement + " " + SectionId.Value;
    }

    /// <summary>
    /// Which arm bodies are a product.
    ///
    /// Injectable, and matched by EXACT id plus arrangement — never by a rule derived from the family, the
    /// designation or a dimension. Same reasoning as the column–base policy: this belongs to the SYSTEM, and
    /// the catalogue still does not know Cantilever exists.
    ///
    /// I-37B registers NO production id of its own. The single profile is family-independent and the product
    /// default will be HSS, but fixing a visible default belongs to the initiative that brings the editor and
    /// the approved designs (ADR-0025, D8).
    /// </summary>
    public interface ICantileverArmSectionPolicy
    {
        /// <summary>The registered variant for a (section, arrangement) pair, if any.</summary>
        bool TryGetVariant(
            StructuralSectionId sectionId,
            CantileverArmBodyArrangement arrangement,
            out CantileverArmVariant variant);

        /// <summary>Every registered variant, in registration order.</summary>
        IReadOnlyList<CantileverArmVariant> Variants { get; }
    }

    /// <summary>The registry-backed arm policy. Immutable once built.</summary>
    public sealed class CantileverArmSectionPolicy : ICantileverArmSectionPolicy
    {
        private readonly Dictionary<(string Section, CantileverArmBodyArrangement Arrangement), CantileverArmVariant> _byKey;
        private readonly List<CantileverArmVariant> _order;

        private CantileverArmSectionPolicy(IEnumerable<CantileverArmVariant> variants)
        {
            _order = new List<CantileverArmVariant>();
            _byKey = new Dictionary<(string, CantileverArmBodyArrangement), CantileverArmVariant>();

            foreach (var variant in variants ?? Enumerable.Empty<CantileverArmVariant>())
            {
                if (variant == null)
                {
                    throw new ArgumentException("Una variante registrada no puede ser nula.", nameof(variants));
                }

                var key = (variant.SectionId.Value, variant.Arrangement);

                if (_byKey.ContainsKey(key))
                {
                    throw new ArgumentException(
                        "La variante de brazo '" + variant + "' esta registrada dos veces.", nameof(variants));
                }

                _byKey.Add(key, variant);
                _order.Add(variant);
            }
        }

        public static CantileverArmSectionPolicy Empty { get; } =
            new CantileverArmSectionPolicy(Array.Empty<CantileverArmVariant>());

        public static CantileverArmSectionPolicy Create(IEnumerable<CantileverArmVariant> variants) =>
            new CantileverArmSectionPolicy(variants);

        /// <summary>Returns a NEW policy with one more registered variant.</summary>
        public CantileverArmSectionPolicy With(CantileverArmVariant variant)
        {
            if (variant == null)
            {
                throw new ArgumentNullException(nameof(variant));
            }

            return new CantileverArmSectionPolicy(_order.Concat(new[] { variant }));
        }

        public IReadOnlyList<CantileverArmVariant> Variants => _order;

        public bool TryGetVariant(
            StructuralSectionId sectionId,
            CantileverArmBodyArrangement arrangement,
            out CantileverArmVariant variant) =>
            _byKey.TryGetValue((sectionId.Value, arrangement), out variant);

        /// <summary>
        /// The sections a NEW design may offer for an arrangement: registered, present in the catalogue and
        /// ENABLED. The other half of owner decision 15 — a design already saved keeps resolving a disabled
        /// section — lives in the resolver, which looks it up by id.
        /// </summary>
        public IReadOnlyList<StructuralSectionDefinition> EligibleForNewDesign(
            StructuralSectionCatalog catalog,
            CantileverArmBodyArrangement arrangement)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var wanted = new List<StructuralSectionDefinition>();

            foreach (var variant in _order.Where(v => v.Arrangement == arrangement))
            {
                if (catalog.TryGetById(variant.SectionId, out var section) && section.IsEnabled)
                {
                    wanted.Add(section);
                }
            }

            return wanted;
        }
    }
}
