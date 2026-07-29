using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// Which column–base combinations are a product, and which sections a NEW design may offer.
    ///
    /// It is an interface so the policy is injectable: the product registers what it sells and the tests
    /// register explicit ids of their own. I-37A adds no production ids of its own — a list of arbitrary
    /// designations would be the product deciding its own catalogue from a source nobody approved.
    ///
    /// This does not contradict ADR-0020. ADR-0020 keeps ROLES out of the CATALOGUE, and it still holds:
    /// <c>StructuralSectionFamily</c> says a family is a shape and never a role. This policy belongs to the
    /// SYSTEM, lives under <c>Systems/Cantilever</c>, and the catalogue still does not know Cantilever exists.
    /// </summary>
    public interface ICantileverColumnBaseSectionPolicy
    {
        /// <summary>
        /// The registered variant for a pair, if any. Matching is by EXACT id: no family rule, no
        /// designation parsing, no dimensional threshold.
        /// </summary>
        bool TryGetVariant(
            StructuralSectionId columnSectionId,
            StructuralSectionId baseSectionId,
            out CantileverColumnBaseVariant variant);

        /// <summary>Families this policy admits at all. Used to explain a rejection, not to grant one.</summary>
        IReadOnlyList<StructuralSectionFamily> AllowedFamilies { get; }

        /// <summary>Every registered combination, in registration order.</summary>
        IReadOnlyList<CantileverColumnBaseVariant> Variants { get; }
    }

    /// <summary>
    /// The registry-backed policy. Immutable once built; <see cref="With"/> returns a new instance so a
    /// policy handed to a resolver can never grow a combination behind its back.
    /// </summary>
    public sealed class CantileverColumnBaseSectionPolicy : ICantileverColumnBaseSectionPolicy
    {
        private readonly Dictionary<(string Column, string Base), CantileverColumnBaseVariant> _byPair;
        private readonly List<CantileverColumnBaseVariant> _order;
        private readonly StructuralSectionFamily[] _families;

        private CantileverColumnBaseSectionPolicy(
            IEnumerable<CantileverColumnBaseVariant> variants,
            IEnumerable<StructuralSectionFamily> allowedFamilies)
        {
            _order = new List<CantileverColumnBaseVariant>();
            _byPair = new Dictionary<(string, string), CantileverColumnBaseVariant>();

            foreach (var variant in variants ?? Enumerable.Empty<CantileverColumnBaseVariant>())
            {
                if (variant == null)
                {
                    throw new ArgumentException("Una variante registrada no puede ser nula.", nameof(variants));
                }

                var key = (variant.ColumnSectionId.Value, variant.BaseSectionId.Value);

                if (_byPair.ContainsKey(key))
                {
                    // Same reason StructuralSectionCatalog.Create throws on a duplicate id: resolving to
                    // "the first row" hides the fact that two registrations disagree.
                    throw new ArgumentException(
                        "La combinacion '" + key.Item1 + "' / '" + key.Item2 + "' esta registrada dos veces.",
                        nameof(variants));
                }

                _byPair.Add(key, variant);
                _order.Add(variant);
            }

            _families = (allowedFamilies ?? new[] { StructuralSectionFamily.W }).Distinct().ToArray();

            if (_families.Length == 0)
            {
                throw new ArgumentException("La politica necesita al menos una familia admitida.", nameof(allowedFamilies));
            }
        }

        /// <summary>
        /// An empty policy that admits only family W.
        ///
        /// W is the scope of the FIRST approved design (I-37A), not a permanent rule: widening it is
        /// registering variants of another family, in one place, with the owner's decision behind it.
        /// </summary>
        public static CantileverColumnBaseSectionPolicy Empty { get; } =
            new CantileverColumnBaseSectionPolicy(
                Array.Empty<CantileverColumnBaseVariant>(),
                new[] { StructuralSectionFamily.W });

        public static CantileverColumnBaseSectionPolicy Create(
            IEnumerable<CantileverColumnBaseVariant> variants,
            IEnumerable<StructuralSectionFamily> allowedFamilies = null) =>
            new CantileverColumnBaseSectionPolicy(variants, allowedFamilies);

        /// <summary>Returns a NEW policy with one more registered combination.</summary>
        public CantileverColumnBaseSectionPolicy With(CantileverColumnBaseVariant variant)
        {
            if (variant == null)
            {
                throw new ArgumentNullException(nameof(variant));
            }

            return new CantileverColumnBaseSectionPolicy(_order.Concat(new[] { variant }), _families);
        }

        /// <summary>Returns a NEW policy admitting exactly these families.</summary>
        public CantileverColumnBaseSectionPolicy WithFamilies(params StructuralSectionFamily[] families) =>
            new CantileverColumnBaseSectionPolicy(_order, families);

        public IReadOnlyList<StructuralSectionFamily> AllowedFamilies => _families;

        public IReadOnlyList<CantileverColumnBaseVariant> Variants => _order;

        public bool TryGetVariant(
            StructuralSectionId columnSectionId,
            StructuralSectionId baseSectionId,
            out CantileverColumnBaseVariant variant) =>
            _byPair.TryGetValue((columnSectionId.Value, baseSectionId.Value), out variant);

        /// <summary>
        /// The sections a NEW design may offer for a role: registered, present in the catalogue, of an
        /// admitted family and ENABLED.
        ///
        /// This is the half of owner decision 15 that filters. The other half — a design already saved that
        /// references a disabled section — goes through <c>TryGetById</c> and keeps resolving, because
        /// dropping it would silently change a drawing the owner already approved.
        /// </summary>
        public IReadOnlyList<StructuralSectionDefinition> EligibleForNewDesign(
            StructuralSectionCatalog catalog,
            CantileverMemberRole role)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var wanted = new List<StructuralSectionDefinition>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var variant in _order)
            {
                var id = role == CantileverMemberRole.Column ? variant.ColumnSectionId : variant.BaseSectionId;

                if (!seen.Add(id.Value))
                {
                    continue;
                }

                if (catalog.TryGetById(id, out var section) &&
                    section.IsEnabled &&
                    _families.Contains(section.Family))
                {
                    wanted.Add(section);
                }
            }

            return wanted;
        }
    }
}
