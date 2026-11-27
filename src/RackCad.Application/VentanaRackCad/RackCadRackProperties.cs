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
    /// <para>T6 pr (VA-S9, Owner 2026-11-27: «Sí, y se escriben al insertarlo»): <see cref="NewRack"/> is the tab of a rack of «Nuevo»
    /// that is not in the drawing yet (<see cref="RackPropertiesRules.ForNewRack"/>): writable and empty, with no fingerprint, because
    /// nothing was read; its draft is written by the command that inserts the rack.</para>
    /// </summary>
    public sealed record RackPropertiesRead(
        bool Writable,
        bool Divergent,
        string Reason,
        IReadOnlyList<CustomPropertyDraftEntry> Entries,
        IReadOnlyList<RackPropertiesViewSummary> Views,
        int ViewCount,
        string Fingerprint,
        bool NewRack = false); // T6 pr

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

        // ------------------------------------------------------------------ T6 pr: the tab of a new rack (VA-S9)

        /// <summary>What «Insertar en el dibujo…» adds to its answer when it wrote the properties of the new rack too (the text of «Actualizar»).</summary>
        public const string NewRackWrittenText = " También escribió sus propiedades.";

        /// <summary>What it adds when they could not be written but the tab of the rack keeps them, to write with «Actualizar».</summary>
        public const string NewRackKeptText = " Siguen en su pestaña «Propiedades»: escríbelas con «Actualizar».";

        /// <summary>
        /// Correction M-1 of the review of T6 pr: the note of the tab when the rows of a draft were kept after an insertion that could not
        /// read the drawing again. «Actualizar» writes them only over a collection with no property (<see cref="NewRackFingerprint"/>).
        /// </summary>
        public const string NotReadAfterInsertionText =
            "No se pudieron volver a leer del dibujo tras la inserción y estas filas no se escribieron. «Actualizar» las escribe solo si en el dibujo sigue sin haber propiedades.";

        /// <summary>
        /// The tab «Propiedades» of a draft of «Nuevo» (T6 pr; VA-S9, answered by the Owner on 2026-11-27: «Sí, y se escriben al
        /// insertarlo»): the rack is not in the drawing, so nothing is read; the tab is writable and empty, and its draft is written by the
        /// command that inserts the rack (the NUEVO commands of the door), in the same command, after the insertion.
        /// </summary>
        public static RackPropertiesRead ForNewRack()
            => new RackPropertiesRead(true, false, null, Array.Empty<CustomPropertyDraftEntry>(), Array.Empty<RackPropertiesViewSummary>(), 0, null,
                NewRack: true);

        /// <summary>
        /// The tab of a rack just inserted whose properties were not written and could not be read again (correction M-1 of the review of
        /// T6 pr): writable, with no base and no fingerprint, so the rows of the draft are kept («Modificado», D-08) and its «Actualizar»
        /// writes them only over a collection with no property, as at the insertion; <see cref="RackPropertiesRead.Reason"/> is its note.
        /// </summary>
        public static RackPropertiesRead NotReadAfterInsertion()
            => new RackPropertiesRead(true, false, NotReadAfterInsertionText, Array.Empty<CustomPropertyDraftEntry>(),
                Array.Empty<RackPropertiesViewSummary>(), 0, null);

        /// <summary>
        /// The fingerprint a draft with no fingerprint is written over (T6 pr): the one of a NEW rack, or rows kept without a read (M-1).
        /// It is the authority of the rack as it is now (for a new rack, the one the same command has just inserted), while it is one
        /// writable collection with no property. Null, with the reason, otherwise: the draft is never written over properties the editor
        /// did not see, nor over a collection the authority keeps read only. The reasons name no system (M-2): a cama or a line too.
        /// </summary>
        public static string NewRackFingerprint(RackCustomPropertiesAuthorityResult inserted, out string refusal)
        {
            refusal = null;
            if (inserted == null)
            {
                refusal = "no se pudieron leer del dibujo.";
                return null;
            }

            if (inserted.Outcome != RackCustomPropertiesAuthorityOutcome.Single || inserted.Collection == null || !inserted.Collection.CanWrite
                || inserted.Collection.Document == null)
            {
                refusal = string.IsNullOrWhiteSpace(inserted.Error) ? "el dibujo no admite que se escriban." : inserted.Error.Trim();
                return null;
            }

            if (inserted.Collection.Document.Entries.Count > 0)
            {
                refusal = "en el dibujo ya hay propiedades que el editor no leyó, y no se escriben encima.";
                return null;
            }

            return Fingerprint(inserted);
        }

        /// <summary>
        /// The answer of «Insertar en el dibujo…» with the tab of the new rack (T6 pr). Only when AutoCAD placed the rack; it always carries
        /// what the drawing holds then (<paramref name="after"/>), so the tab goes on from the rack of the drawing: with nothing to write the
        /// message does not change; written, it says so; not written, it says why and, when the tab keeps them (the rack can hold them, or
        /// it could not be read again: M-1), that they stay in its tab to write with «Actualizar».
        /// </summary>
        public static RackDesignWritten AnswerForNewRack(RackDesignWritten inserted, RackPropertiesWrite write, string error, RackPropertiesRead after)
        {
            if (inserted == null || !inserted.Written) return inserted;
            if (write == null) return inserted with { Properties = after };
            if (error == null) return inserted with { Message = inserted.Message + NewRackWrittenText, Properties = after };
            return inserted with
            {
                Message = inserted.Message + " Pero sus propiedades no se escribieron: " + error.Trim() + (after == null || after.Writable ? NewRackKeptText : string.Empty),
                PropertiesError = error.Trim(),
                Properties = after,
            };
        }

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
