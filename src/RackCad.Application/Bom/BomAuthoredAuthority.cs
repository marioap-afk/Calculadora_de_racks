using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;

namespace RackCad.Application.Bom
{
    /// <summary>Whether a rack's views agree well enough to be quoted at all.</summary>
    public enum BomAuthorityOutcome
    {
        Success = 1,

        /// <summary>At least one view's design could not be read, so no authority can be established.</summary>
        UnreadableSibling = 2,

        /// <summary>Every view was readable and they do not describe the same rack.</summary>
        DivergentSiblings = 3,
    }

    /// <summary>The approved authority of a rack, or the reason it has none.</summary>
    public sealed class BomAuthorityResult
    {
        private BomAuthorityResult(
            BomAuthorityOutcome outcome,
            SelectivePalletDesignDocument authored,
            string representativeDefinitionId,
            string error)
        {
            Outcome = outcome;
            Authored = authored;
            RepresentativeDefinitionId = representativeDefinitionId;
            Error = error;
        }

        public BomAuthorityOutcome Outcome { get; }

        /// <summary>
        /// The single authored state, for a Selective rack. Null for other kinds — which have no inner
        /// authored document in this slice — and null on every failure.
        /// </summary>
        public SelectivePalletDesignDocument Authored { get; }

        /// <summary>
        /// The view the caller may quote from: the FIRST in sweep order, once every view was proven to say
        /// the same thing. Deterministic on purpose — a reproducible answer beats a clever one.
        /// </summary>
        public string RepresentativeDefinitionId { get; }

        public string Error { get; }

        public bool IsSuccess => Outcome == BomAuthorityOutcome.Success;

        public static BomAuthorityResult Success(
            SelectivePalletDesignDocument authored, string representativeDefinitionId)
            => new BomAuthorityResult(BomAuthorityOutcome.Success, authored, representativeDefinitionId, null);

        public static BomAuthorityResult NoAuthority(BomAuthorityOutcome outcome, string error)
            => new BomAuthorityResult(outcome, null, null, error);
    }

    /// <summary>
    /// Picks the view a rack may be quoted from — after proving that picking one changes nothing (I-47 G13).
    ///
    /// <para>
    /// <c>RACKBOMTOTAL</c> used to take the FIRST view of each rack and quote it. Every view stores the whole
    /// design and they are supposed to agree, so that reads as harmless — until they do not agree, and then
    /// the price of a rack depends on which block the block table happened to hand over first. Nothing failed;
    /// the number was simply one of the possible ones.
    /// </para>
    /// <para>
    /// So the representative is still the first view, and it is only approved once EVERY view was read and
    /// shown to describe the same authored state. The comparison is the structural one from G5, over the
    /// persisted tree rather than field by field, because the contract is INCLUDE-BY-DEFAULT: a field a later
    /// gate adds has to participate without anyone remembering to add it. That is why divergence in
    /// <c>SchemaVersion</c>, in <c>PropertyValues</c> or in <c>ExtensionData</c> alone is divergence — each of
    /// them is the trace of something that happened to one view and not the others.
    /// </para>
    /// <para>
    /// A kind that is not Selective has no inner authored document here, so its first view is approved without
    /// further questions. That is not an exception to the rule: it is the rule with nothing to compare.
    /// </para>
    /// </summary>
    public static class BomAuthoredAuthority
    {
        public static BomAuthorityResult Resolve(string rackId, IReadOnlyList<ProjectVariableScanEntry> siblings)
        {
            if (siblings == null || siblings.Count == 0)
            {
                return BomAuthorityResult.NoAuthority(
                    BomAuthorityOutcome.UnreadableSibling,
                    "El rack " + rackId + " no tiene ninguna vista de la que leer su diseño.");
            }

            foreach (var sibling in siblings)
            {
                if (sibling == null || !sibling.OuterEnvelopeInterpretable)
                {
                    return BomAuthorityResult.NoAuthority(
                        BomAuthorityOutcome.UnreadableSibling,
                        "El rack " + rackId + " tiene una vista cuyo sobre no se puede interpretar (definición '" +
                        (sibling?.DefinitionId ?? "<desconocida>") + "').");
                }
            }

            var representative = siblings[0].DefinitionId;

            if (!siblings[0].IsSelective)
            {
                // No inner authored document to compare in this slice; the historic representative stands.
                return BomAuthorityResult.Success(null, representative);
            }

            var authority = SelectiveAuthoredAuthority.Resolve(rackId, siblings);

            switch (authority.Outcome)
            {
                case AuthoredAuthorityOutcome.Single:
                    return BomAuthorityResult.Success(authority.Authored, representative);

                case AuthoredAuthorityOutcome.Divergent:
                    return BomAuthorityResult.NoAuthority(BomAuthorityOutcome.DivergentSiblings, authority.Error);

                default:
                    return BomAuthorityResult.NoAuthority(BomAuthorityOutcome.UnreadableSibling, authority.Error);
            }
        }
    }
}
