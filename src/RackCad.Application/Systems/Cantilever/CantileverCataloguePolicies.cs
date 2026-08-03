using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.StructuralSections;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The column–base eligibility the PRODUCT uses: any pair of catalogued sections whose families are declared.
    ///
    /// <para><b>Why not a list of ids.</b> ADR-0024 D6 chose exact-id variants and said, in the same breath, that
    /// "el producto no adquiere ids arbitrarios en esta iniciativa" — the id lists live in the tests. Shipping a
    /// hard-coded list of five ids would be precisely the arbitrary acquisition it refused, and it would be a
    /// product decision — which sections a Cantilever may be built from — that nobody has made.</para>
    ///
    /// <para>What I-37A actually declared is a FAMILY restriction: "I-37A restringe además la familia a W. No es
    /// una regla permanente: es el alcance del primer diseño aprobado, declarado en un solo sitio y ampliable."
    /// This is that rule, in that one site. The design still names its two sections explicitly and the user still
    /// chooses them; the policy only answers whether the pair is a shape this system knows how to connect.</para>
    ///
    /// <para>It stays a policy of the SYSTEM and not of the catalogue. The catalogue continues not to know that
    /// Cantilever exists, which is what ADR-0020 protects.</para>
    /// </summary>
    public sealed class CantileverCatalogueColumnBasePolicy : ICantileverColumnBaseSectionPolicy
    {
        private readonly StructuralSectionCatalog _catalogue;
        private readonly StructuralSectionFamily[] _families;

        public CantileverCatalogueColumnBasePolicy(
            StructuralSectionCatalog catalogue, IEnumerable<StructuralSectionFamily> allowedFamilies = null)
        {
            _catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
            _families = (allowedFamilies ?? new[] { StructuralSectionFamily.W }).Distinct().ToArray();

            if (_families.Length == 0)
            {
                throw new ArgumentException(
                    "Una politica sin familias admitidas rechazaria todo diseno.", nameof(allowedFamilies));
            }
        }

        public IReadOnlyList<StructuralSectionFamily> AllowedFamilies => _families;

        /// <summary>
        /// Empty, and that is not an oversight.
        ///
        /// <see cref="Variants"/> is "every registered combination", and this policy registers none: it admits a
        /// family. Enumerating the cross product of every W section with every other would be tens of thousands of
        /// pairs, none of which anybody registered. A consumer that wants to offer choices reads the CATALOGUE
        /// filtered by <see cref="AllowedFamilies"/>, which is the real answer to the question it is asking.
        /// </summary>
        public IReadOnlyList<CantileverColumnBaseVariant> Variants =>
            Array.Empty<CantileverColumnBaseVariant>();

        public bool TryGetVariant(
            StructuralSectionId columnSectionId,
            StructuralSectionId baseSectionId,
            out CantileverColumnBaseVariant variant)
        {
            variant = null;

            if (!IsAdmitted(columnSectionId) || !IsAdmitted(baseSectionId))
            {
                return false;
            }

            // The connection kind follows from the family, and only W has one declared. A family admitted without
            // a kind would have to be given one here, which is exactly the invention this class avoids: adding a
            // family means declaring its kind, not defaulting to the W one.
            variant = new CantileverColumnBaseVariant(
                CantileverColumnBaseVariantKind.WFlangeConnected, columnSectionId, baseSectionId);

            return true;
        }

        private bool IsAdmitted(StructuralSectionId id) =>
            _catalogue.TryGetById(id, out var section) && _families.Contains(section.Family);
    }

    /// <summary>
    /// The arm eligibility the PRODUCT uses: any catalogued section as a single body, and a CHANNEL for the two
    /// paired arrangements.
    ///
    /// The channel restriction is not this class's rule — <c>CantileverArmBodyArrangementResolver</c> already
    /// requires one, because the placement of a pair is derived from where the back of a web is. Restating it here
    /// as a policy means a design that asks for paired W sections is rejected at the boundary, with a message
    /// about eligibility, instead of deeper down with a message about geometry.
    ///
    /// Nothing else is restricted, and nothing else needs to be: the arm resolver validates the cut, the slope,
    /// the connection and the plates, and it reports each in its own words.
    /// </summary>
    public sealed class CantileverCatalogueArmPolicy : ICantileverArmSectionPolicy
    {
        private readonly StructuralSectionCatalog _catalogue;

        public CantileverCatalogueArmPolicy(StructuralSectionCatalog catalogue)
        {
            _catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
        }

        /// <summary>Empty, for the reason <see cref="CantileverCatalogueColumnBasePolicy.Variants"/> gives.</summary>
        public IReadOnlyList<CantileverArmVariant> Variants => Array.Empty<CantileverArmVariant>();

        public bool TryGetVariant(
            StructuralSectionId sectionId,
            CantileverArmBodyArrangement arrangement,
            out CantileverArmVariant variant)
        {
            variant = null;

            if (!CantileverArmBodyArrangementResolver.IsSupported(arrangement))
            {
                return false;
            }

            if (!_catalogue.TryGetById(sectionId, out var section))
            {
                return false;
            }

            if (CantileverArmBodyArrangementResolver.RequiresChannel(arrangement) &&
                section.Family != StructuralSectionFamily.Channel)
            {
                return false;
            }

            variant = new CantileverArmVariant(sectionId, arrangement);
            return true;
        }
    }

    /// <summary>
    /// The two policies a production caller needs, built from one catalogue.
    ///
    /// It exists so the Plugin and the editor do not each decide what a valid Cantilever section is. Before it,
    /// only the tests built policies, and the first production call site would have had to invent one.
    /// </summary>
    public static class CantileverCataloguePolicies
    {
        public static ICantileverColumnBaseSectionPolicy ColumnBase(StructuralSectionCatalog catalogue) =>
            new CantileverCatalogueColumnBasePolicy(catalogue);

        public static ICantileverArmSectionPolicy Arm(StructuralSectionCatalog catalogue) =>
            new CantileverCatalogueArmPolicy(catalogue);
    }
}
