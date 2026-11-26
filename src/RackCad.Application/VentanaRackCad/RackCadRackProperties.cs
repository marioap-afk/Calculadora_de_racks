using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>One view of a rack whose views have different custom properties: how it is named, what it holds, and whether it can be the source.</summary>
    public sealed record RackPropertiesViewSummary(string Handle, string Label, string Content, bool CanBeSource,
        IReadOnlyList<CustomPropertyDraftEntry> Entries = null);

    /// <summary>
    /// The custom properties of a rack as the tab «Propiedades» of its editor shows them (I-64 A-14, cuarta tanda, va; VA-07 and VA-08 of
    /// the Owner). Read with the rack by the door (RACKSELECTIVOABRIR, RACKDINAMICOABRIR), from the same authority RACKPROPIEDADES uses.
    /// <list type="bullet">
    /// <item><see cref="Writable"/>: one collection for every view; <see cref="Entries"/> is its draft.</item>
    /// <item><see cref="Divergent"/>: the views differ; <see cref="Views"/> says what each holds, and one can be the source of a unify.</item>
    /// <item>Neither: read only, with <see cref="Reason"/>.</item>
    /// </list>
    /// No identifier is shown (VA-09): the handles travel only to name the source of a unify.
    /// </summary>
    public sealed record RackPropertiesRead(
        bool Writable,
        bool Divergent,
        string Reason,
        IReadOnlyList<CustomPropertyDraftEntry> Entries,
        IReadOnlyList<RackPropertiesViewSummary> Views,
        int ViewCount,
        string Fingerprint);

    /// <summary>What «Actualizar» of the editor writes in the properties of the rack: the whole draft, and the view it was unified from (VA-08).</summary>
    public sealed record RackPropertiesWrite(string Fingerprint, IReadOnlyList<CustomPropertyDraftEntry> Entries, string UnifiedFrom = null);

    /// <summary>The pure rules of the tab «Propiedades» of a rack (cuarta tanda, va), with their tests in Core.</summary>
    public static class RackPropertiesRules
    {
        public const string ChangedText = CustomPropertiesCommit.DraftChangedText;

        /// <summary>The tab of the authority of a rack (RACKPROPIEDADES reads the same one).</summary>
        public static RackPropertiesRead Read(RackCustomPropertiesAuthorityResult authority)
        {
            if (authority == null)
                return new RackPropertiesRead(false, false, "No se pudieron leer las propiedades del rack.", Array.Empty<CustomPropertyDraftEntry>(),
                    Array.Empty<RackPropertiesViewSummary>(), 0, null);

            var fingerprint = Fingerprint(authority);
            var count = authority.Members.Count;
            if (authority.Outcome == RackCustomPropertiesAuthorityOutcome.Single && authority.Collection != null && authority.Collection.CanWrite)
                return new RackPropertiesRead(true, false, null, CustomPropertiesBatch.DraftOf(authority.Collection.Document),
                    Array.Empty<RackPropertiesViewSummary>(), count, fingerprint);

            if (authority.Outcome == RackCustomPropertiesAuthorityOutcome.Divergent)
            {
                var views = authority.Members.Select(member =>
                {
                    var option = authority.UnifyOptions.FirstOrDefault(o => string.Equals(o.SourceHandle, member.Handle, StringComparison.Ordinal));
                    return new RackPropertiesViewSummary(member.Handle, LabelOf(member.View, member.Section), ContentOf(member.Collection),
                        option != null && option.IsAvailable && member.Collection != null && member.Collection.CanWrite,
                        member.Collection?.Document == null ? Array.Empty<CustomPropertyDraftEntry>() : CustomPropertiesBatch.DraftOf(member.Collection.Document));
                }).ToList();
                return new RackPropertiesRead(false, true, null, Array.Empty<CustomPropertyDraftEntry>(), views, count, fingerprint);
            }

            return new RackPropertiesRead(false, false,
                "Las propiedades de este rack son de solo lectura en esta versión: " + (string.IsNullOrWhiteSpace(authority.Error) ? "no se pueden escribir." : authority.Error.Trim()),
                Array.Empty<CustomPropertyDraftEntry>(), Array.Empty<RackPropertiesViewSummary>(), count, fingerprint);
        }

        /// <summary>What the editor read of the properties (<see cref="CustomPropertiesCommit.DraftFingerprint"/>).</summary>
        public static string Fingerprint(RackCustomPropertiesAuthorityResult authority) => CustomPropertiesCommit.DraftFingerprint(authority);

        /// <summary>The payloads «Actualizar» writes over the authority of NOW (<see cref="CustomPropertiesCommit.PlanDraft"/>).</summary>
        public static CustomPropertiesCommitResult Plan(RackCustomPropertiesAuthorityResult now, RackPropertiesWrite write)
            => write == null
                ? CustomPropertiesCommit.PlanDraft(null, null, null, null)
                : CustomPropertiesCommit.PlanDraft(now, write.Fingerprint, write.Entries, write.UnifiedFrom);

        private static string LabelOf(string view, int section)
        {
            if (string.Equals(view, RackEmbedDocument.ViewLateral, StringComparison.OrdinalIgnoreCase))
                return "Vista lateral · corte en el poste " + (section + 1).ToString(CultureInfo.InvariantCulture);
            if (string.Equals(view, RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase)) return "Vista en planta";
            return section > 0 ? "Vista frontal · fondo " + (section + 1).ToString(CultureInfo.InvariantCulture) : "Vista frontal";
        }

        private static string ContentOf(CustomPropertiesReadResult collection)
        {
            if (collection == null || collection.Document == null) return "no se puede leer";
            var entries = collection.Document.Entries;
            return entries.Count == 0 ? "vacío / sin propiedades" : string.Join("; ", entries.Select(e => e.Name + " — " + e.Value));
        }
    }
}
