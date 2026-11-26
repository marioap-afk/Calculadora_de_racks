using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>One view of a rack as the drawing holds it: the identity of its block definition (unique in the drawing) and its envelope.</summary>
    public sealed record RackSiblingView(string SourceIdentity, RackEmbedDocument Envelope);

    /// <summary>What the views of one rack say about its design.</summary>
    public enum RackSiblingDesignsOutcome
    {
        /// <summary>Every view carries the same authored design, or the rack has only one view.</summary>
        Same = 1,

        /// <summary>The comparator read every view and the designs differ.</summary>
        Divergent = 2,

        /// <summary>The comparator could not read every view, so it is not demonstrated that they carry the same design.</summary>
        Unverifiable = 3,
    }

    /// <summary>The verdict on the views of one rack: <see cref="Reason"/> is what the window says when they do not coincide (null when they do).</summary>
    public sealed record RackSiblingDesignsVerdict(RackSiblingDesignsOutcome Outcome, string Reason, string Diagnostic)
    {
        public bool Coincide => Outcome == RackSiblingDesignsOutcome.Same;
    }

    /// <summary>
    /// Whether the sibling views of a rack carry the same design (I-64 A-14, line D; M-02 of the review of Selectivo). The door of the
    /// RackCad window reads the design of one view and writes the draft in all of them, so it may only do so when they coincide: otherwise
    /// it would open any of them and «Actualizar» would write it everywhere. The rule is generic and pure: it hands the envelopes to the
    /// authored comparator that already decides AUTHORED_DIVERGENT for that kind (<see cref="RackAuthoredComparatorPorts"/>; the same one
    /// the insertion of views uses) and invents no equality of its own, so what that comparator ignores (the view and section of each
    /// block, its custom properties, the documented legacy defaults) it ignores too, and what it compares (for the four AUTH-13 kinds, the
    /// name as well) counts. A kind without a demonstrated comparator, or a view it cannot read, is not equality (fail closed). One view has
    /// nothing to disagree with, so it coincides without being read here: the door reads it as it always did.
    /// </summary>
    public static class RackSiblingDesigns
    {
        /// <summary>The views were read and differ. RACKEDITAR opens the view that was picked, and its «Actualizar» redraws every view with it.</summary>
        public const string DivergentText =
            "Las vistas de este rack no tienen el mismo diseño: ábrelo con RACKEDITAR desde la vista que quieras conservar; «Actualizar» las iguala.";

        /// <summary>Some view could not be read by the comparator: it is not known whether they coincide.</summary>
        public const string UnverifiableText =
            "No se pudo comprobar que las vistas de este rack tengan el mismo diseño: ábrelo con RACKEDITAR desde la vista que quieras conservar.";

        // The caller hands every view it found by the identity of the rack, the same set it reads from and writes to.
        private const string CompletenessEvidence = "RackSiblingDesigns: every view the caller found by the identity of the rack";

        /// <summary>
        /// Compares the designs of <paramref name="views"/>, every view the caller found for <paramref name="rackId"/>. The kind is the one
        /// the envelopes declare; views of different kinds never coincide. It never throws.
        /// </summary>
        public static RackSiblingDesignsVerdict Compare(string rackId, IReadOnlyList<RackSiblingView> views)
        {
            if (views == null || views.Count <= 1)
                return new RackSiblingDesignsVerdict(RackSiblingDesignsOutcome.Same, null, null);

            RackAuthoredComparisonOutcome outcome;
            string diagnostic;
            try
            {
                (outcome, diagnostic) = Run(rackId, views);
            }
            catch (Exception ex)
            {
                (outcome, diagnostic) = (RackAuthoredComparisonOutcome.Unreadable, ex.Message);
            }

            switch (outcome)
            {
                case RackAuthoredComparisonOutcome.Single:
                    return new RackSiblingDesignsVerdict(RackSiblingDesignsOutcome.Same, null, diagnostic);
                case RackAuthoredComparisonOutcome.Divergent:
                    return new RackSiblingDesignsVerdict(RackSiblingDesignsOutcome.Divergent, DivergentText, diagnostic);
                default:
                    return new RackSiblingDesignsVerdict(RackSiblingDesignsOutcome.Unverifiable, UnverifiableText, diagnostic);
            }
        }

        private static (RackAuthoredComparisonOutcome, string) Run(string rackId, IReadOnlyList<RackSiblingView> views)
        {
            var kind = views.Select(view => view?.Envelope?.Kind).FirstOrDefault(k => !string.IsNullOrWhiteSpace(k));

            if (Is(kind, RackEmbedDocument.KindSelective))
            {
                // A view of another kind projects as foreign, which the Selective authority never takes as readable.
                var entries = views.Select(view => ProjectVariableScanProjection.Project(view?.SourceIdentity, view?.Envelope, 0)).ToList();
                return From(RackAuthoredComparatorPorts.Selective().Compare(new SelectiveAuthoredComparisonInput(rackId, entries)));
            }

            var input = Input(rackId, views);
            if (Is(kind, RackEmbedDocument.KindDynamic)) return From(RackAuthoredComparatorPorts.Dynamic().Compare(input));
            if (Is(kind, RackEmbedDocument.KindPushBack)) return From(RackAuthoredComparatorPorts.PushBack().Compare(input));
            if (Is(kind, RackEmbedDocument.KindCantilever)) return From(RackAuthoredComparatorPorts.Cantilever().Compare(input));
            if (Is(kind, RackEmbedDocument.KindCabecera)) return From(RackAuthoredComparatorPorts.Cabecera().Compare(input));
            if (Is(kind, RackEmbedDocument.KindCama)) return From(RackAuthoredComparatorPorts.Cama<RackAuthoredInput, object>().Compare(input));
            return (RackAuthoredComparisonOutcome.Unreadable, "No authored comparator is demonstrated for kind '" + (kind ?? "<none>") + "'.");
        }

        // The original evidence of each view, as the projection of views builds it (the envelope as the drawing holds it, and its design).
        private static RackAuthoredInput Input(string rackId, IReadOnlyList<RackSiblingView> views)
        {
            var store = new RackEmbedStore();
            var siblings = views
                .Select(view => view?.Envelope == null
                    ? new RackAuthoredSibling(view?.SourceIdentity, string.Empty, null, null)
                    : new RackAuthoredSibling(view.SourceIdentity, view.Envelope.Kind ?? string.Empty, store.Serialize(view.Envelope), view.Envelope.Design))
                .ToList();
            return new RackAuthoredInput(rackId, siblings, true, CompletenessEvidence);
        }

        private static (RackAuthoredComparisonOutcome, string) From<TAuthored>(RackAuthoredComparisonResult<TAuthored> result)
            => result == null ? (RackAuthoredComparisonOutcome.Unreadable, "The comparator gave no result.") : (result.Outcome, result.Diagnostic);

        private static bool Is(string kind, string expected) => string.Equals(kind, expected, StringComparison.OrdinalIgnoreCase);
    }
}
