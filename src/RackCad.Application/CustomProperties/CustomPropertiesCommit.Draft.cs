using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;

namespace RackCad.Application.CustomProperties
{
    /// <summary>
    /// The whole draft of the custom properties of a rack as ONE commit (I-64 A-14, cuarta tanda, va; VA-07 and VA-08): the tab «Propiedades»
    /// of an editor of the RackCad window edits the collection freely, and its «Actualizar» writes it in every view of the rack.
    /// </summary>
    public static partial class CustomPropertiesCommit
    {
        public const string DraftChangedText =
            "Las propiedades de este rack cambiaron en el dibujo desde que se abrió el editor: no se escribieron. Cierra el editor y vuelve a abrirlo con «Editar aquí».";

        /// <summary>
        /// What the editor read of the properties: the rack and the distinct collections of its views (not their handles, so a view that the
        /// redraw of the same «Actualizar» removes does not count as a change).
        /// </summary>
        public static string DraftFingerprint(RackCustomPropertiesAuthorityResult authority)
            => authority == null ? null
                : (authority.RackId ?? string.Empty) + "|" + string.Join("|", authority.Members.Select(m => m.CanonicalForm ?? string.Empty)
                    .Distinct(StringComparer.Ordinal).OrderBy(f => f, StringComparer.Ordinal));

        /// <summary>
        /// The payloads of a whole draft on a FRESH projection: the authority is evaluated again, its properties must be the ones the editor
        /// read (<paramref name="fingerprint"/>), the draft is applied with the rules of each mutation (<see cref="CustomPropertiesBatch"/>)
        /// over the one collection of the rack or, for a unify, over the chosen view while it is still a safe source; every member gets it.
        /// </summary>
        public static CustomPropertiesCommitResult ForRackDraft(
            IEnumerable<RackCustomPropertiesDefinition> fresh,
            RackCustomPropertiesSelection selection,
            Func<string, bool> isKnownKind,
            string fingerprint,
            IReadOnlyList<CustomPropertyDraftEntry> entries,
            string unifiedFrom)
        {
            var now = FreshAuthority(fresh, selection, isKnownKind, out var missing);
            if (missing != null) return missing;
            return PlanDraft(now, fingerprint, entries, unifiedFrom);
        }

        /// <summary>The plan of a draft over an authority already evaluated (the pure half of <see cref="ForRackDraft"/>).</summary>
        public static CustomPropertiesCommitResult PlanDraft(
            RackCustomPropertiesAuthorityResult now, string fingerprint, IReadOnlyList<CustomPropertyDraftEntry> entries, string unifiedFrom)
        {
            if (now == null)
                return CustomPropertiesCommitResult.Refused(CustomPropertiesCommitRefusal.SelectionMissing, "No hay propiedades que escribir.", null);
            if (!string.Equals(DraftFingerprint(now), fingerprint, StringComparison.Ordinal))
                return CustomPropertiesCommitResult.Refused(CustomPropertiesCommitRefusal.DisplayedStateChanged, DraftChangedText, now.Outcome);

            CustomPropertiesReadResult over;
            if (unifiedFrom == null)
            {
                if (now.Outcome != RackCustomPropertiesAuthorityOutcome.Single) return NotWritable(now);
                over = now.Collection;
            }
            else
            {
                var option = now.UnifyOptions.FirstOrDefault(o => string.Equals(o.SourceHandle, unifiedFrom, StringComparison.Ordinal));
                var source = now.Members.FirstOrDefault(m => string.Equals(m.Handle, unifiedFrom, StringComparison.Ordinal));
                if (now.Outcome != RackCustomPropertiesAuthorityOutcome.Divergent || option == null || !option.IsAvailable || source == null)
                    return CustomPropertiesCommitResult.Refused(CustomPropertiesCommitRefusal.UnifyUnavailable,
                        "Ya no se puede unificar desde esa vista: no se escribieron las propiedades.", now.Outcome);
                over = source.Collection;
            }

            var applied = CustomPropertiesBatch.Apply(over, entries);
            if (!applied.Succeeded)
                return CustomPropertiesCommitResult.Refused(CustomPropertiesCommitRefusal.MutationRejected, applied.Error, now.Outcome, applied.Rejection);

            var document = applied.Document;
            document.SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(document.SchemaVersion, CustomPropertiesDocument.CurrentSchemaVersion);
            return CustomPropertiesCommitResult.Planned(PlanFor(now.Members, document), document, default, now.Outcome);
        }
    }
}
