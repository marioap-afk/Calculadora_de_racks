using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// What a write of a cabecera of a post in the draft of a Selectivo did (T4 cb): written (<see cref="Edit"/>, the caller recomposes), or
    /// not written because the height asks first (<see cref="Confirmation"/>, CB-09: below the top load level, «Aplicar igual»).
    /// </summary>
    public sealed record SelectiveCabeceraWrite(SelectiveDraftEdit Edit, string Confirmation, string Notice)
    {
        public bool NeedsConfirmation => Confirmation != null;

        public static SelectiveCabeceraWrite Refuse(string error) => new SelectiveCabeceraWrite(SelectiveDraftEdit.Refuse(error), null, null);
    }

    /// <summary>
    /// The cabeceras of the posts of a Selectivo in the window (I-64, fourth batch, part cb; prototype «cabecera», CB-01, CB-08, CB-09, CB-10):
    /// the three capacities of the classic editor that only lived there — «Personalizar» (now «Editar cabecera…», the editor of cabeceras
    /// inside the one of Selectivo), «Reutilizar cabecera» (now «Copiar a otros postes…») and «Restablecer poste» (now «Volver a la
    /// calculada»). Every write goes through the ONE protocol of the classic editor (I-53, <see cref="SelectiveHeaderBatchPlanner"/> and
    /// <see cref="SelectiveEditorState.ApplyHeaderBatch"/>): each copy takes the depth of ITS fondo and the peralte of ITS post, and the
    /// height is reviewed at every destination. The caller recomposes, as every write of this draft.
    /// </summary>
    public sealed partial class SelectiveDraftEditor
    {
        // The identity of the resolution a write reads: prepared and applied inside one call, with no recomposition between them.
        private const long CabeceraWriteGeneration = 1;

        /// <summary>
        /// The cabecera «Editar cabecera…» opens on the post <paramref name="post"/> of the fondo <paramref name="fondo"/> (the seed of the
        /// classic «Personalizar»): a copy of its custom cabecera, or the standard one resolved with the height of that post in that fondo
        /// and the cabecera depth of that fondo; its peralte, the own one of the post or the one of the rack. Null when the post is not there.
        /// </summary>
        public RackFrameConfiguration CabeceraSeed(int fondo, int post)
        {
            if (fondo < 0 || fondo >= FondoCount || post < 0 || post > FrontCount(fondo)) return null;
            Commit();
            var depth = CabeceraDepthAt(fondo);
            var custom = _state.CabeceraAt(fondo, post);
            RackFrameConfiguration seed;
            if (custom != null)
            {
                seed = new RackFrameProjectStore().DeepCopy(custom);
            }
            else
            {
                var template = RackFrameTemplateCatalog.FindStandardOrDefault();
                var height = CabeceraHeightAt(fondo, post);
                seed = new RackFrameConfigurationFactory(Catalog).Build(template, PostId, height > 0.0 ? height : template.DefaultHeight, depth);
            }

            if (depth > 0.0) seed.Depth = depth;
            seed.PostPeralte = PostPeralteOverride(post) ?? PostPeralte;
            return seed;
        }

        /// <summary>The cabecera depth of a fondo (its override, else the pallet less 6 in), the authority of I-43 that the rack imposes.</summary>
        public double CabeceraDepthAt(int fondo)
        {
            var depth = _state.CabeceraDepthOfFondo(fondo);
            if (depth > 0.0) return depth;
            var derived = SelectiveRackDefaults.DefaultPalletDepth - SelectiveRackDefaults.CabeceraFondoAllowance;
            return derived > 0.0 ? derived : SelectiveRackDefaults.DefaultPalletDepth;
        }

        /// <summary>The height of a post in a fondo as the last valid system resolves it (the tallest front it touches); 0 with no system.</summary>
        public double CabeceraHeightAt(int fondo, int post)
        {
            if (System == null) return 0.0;
            var bays = SelectiveDepthLayout.BaysOfFondo(System, fondo);
            if (bays == null || post < 0 || post > bays.Count) return 0.0;
            var height = SelectivePostGeometry.PostHeight(bays, post, SelectivePostGeometry.FallbackHeight(bays, System.Height));
            return height > 0.0 ? height : System.Height;
        }

        /// <summary>
        /// Where the editor of cabeceras works inside this Selectivo (CB-08): the depth is the one of the fondo, read only with its origin;
        /// the peralte is the own one of the post (editable; empty = the one of the rack); and the height is reviewed against the post in
        /// that fondo (CB-09).
        /// </summary>
        public CabeceraDraftContext CabeceraContext(int fondo, int post, string rackName)
        {
            var depth = CabeceraDepthAt(fondo);
            var where = CabeceraWhere(fondo, post);
            var slot = Fondo(fondo);
            var origin = slot?.CabeceraDepthOverride.HasValue == true
                ? "La del fondo " + (fondo + 1) + ": su profundidad de cabecera propia. Se cambia en «Rack» de " + Named(rackName) + "."
                : "La del fondo " + (fondo + 1) + ": la tarima (" + Num(slot?.PalletDepth ?? 0.0) + " in) − 6 in. Se cambia en «Rack» de " + Named(rackName) + ".";
            return new CabeceraDraftContext(
                Embedded: true,
                LockedDepth: depth,
                DepthOrigin: origin,
                RackPostPeralte: PostPeralte,
                PostPeralteOrigin: "Peralte propio de P" + (post + 1) + " (el de «Avanzado»); vacío = el del rack, " + Num(PostPeralte) + " in.",
                HeightReview: height => CabeceraHeightNoteAt(fondo, post, height),
                Where: where);
        }

        /// <summary>«P2 · Fondo 1» (or «P2» with one fondo).</summary>
        public string CabeceraWhere(int fondo, int post)
            => "P" + (post + 1).ToString(CultureInfo.InvariantCulture) + (FondoCount > 1 ? " · Fondo " + (fondo + 1).ToString(CultureInfo.InvariantCulture) : string.Empty);

        /// <summary>The height of a cabecera against its post in that fondo (CB-09), with the review of the classic editor.</summary>
        public CabeceraHeightNote CabeceraHeightNoteAt(int fondo, int post, double height)
        {
            if (System == null || !(height > 0.0)) return CabeceraHeightNote.None;
            var review = SelectiveCabeceraHeightReview.Of(System, new[] { fondo }, post, height);
            var finding = review.Findings.FirstOrDefault();
            if (finding == null) return CabeceraHeightNote.None;
            var where = FondoCount > 1 ? " en el fondo " + (fondo + 1) : string.Empty;
            return finding.Issue == SelectiveCabeceraHeightIssue.Severe
                ? new CabeceraHeightNote(CabeceraHeightSeverity.Severe, "La cabecera mide " + Num(height) + " in: queda por debajo del nivel de carga superior" + where
                    + " (" + Num(finding.Reference) + " in). El larguero o la tarima superiores sobresaldrían por encima del poste.")
                : new CabeceraHeightNote(CabeceraHeightSeverity.Informative, "La cabecera mide " + Num(height) + " in y el alto resuelto de este poste" + where
                    + " es " + Num(finding.Reference) + " in: el frontal y el corte lateral pueden dejar de coincidir.");
        }

        /// <summary>The destinations «Copiar a otros postes…» offers (CB-10): every post of every fondo, but the one being edited.</summary>
        public IReadOnlyList<SelectiveHeaderAddress> CabeceraCopyTargets(int fondo, int post)
        {
            var list = new List<SelectiveHeaderAddress>();
            for (var k = 0; k < FondoCount; k++)
            {
                for (var p = 0; p <= FrontCount(k); p++)
                {
                    if (k == fondo && p == post) continue;
                    list.Add(new SelectiveHeaderAddress(k, p));
                }
            }

            return list;
        }

        /// <summary>
        /// «Aplicar al borrador» of the cabecera of a post (and, with <paramref name="copies"/>, «Aplicar aquí y copiar a n destinos»): the
        /// EDIT of the protocol of I-53 on that post of that fondo, and the same EDIT, normalized to the peralte of each destination post, on
        /// every destination. Everything is prepared before anything is written: a destination that does not hold writes nothing (CB-10). A
        /// severe height at any of them asks first (CB-09) unless <paramref name="force"/>; an informative one is said in the notice. The
        /// caller recomposes.
        /// </summary>
        public SelectiveCabeceraWrite WriteCabecera(int fondo, int post, RackFrameConfiguration cabecera, IReadOnlyList<SelectiveHeaderAddress> copies, bool force)
        {
            if (cabecera == null) return SelectiveCabeceraWrite.Refuse("No hay cabecera que aplicar.");
            if (System == null) return SelectiveCabeceraWrite.Refuse("No se aplicó nada: el rack todavía no tiene geometría resuelta. Corrige primero los datos del rack.");
            Commit();
            var resolution = SelectiveHeaderResolution.Of(System, CabeceraWriteGeneration, false);
            var destinations = new List<(SelectiveHeaderAddress Address, RackFrameConfiguration Copy)> { (new SelectiveHeaderAddress(fondo, post), cabecera) };
            foreach (var target in (copies ?? Array.Empty<SelectiveHeaderAddress>()).Distinct())
            {
                if (target.FondoIndex == fondo && target.PostIndex == post) continue;
                var copy = new RackFrameProjectStore().DeepCopy(cabecera);

                // Each copy takes the peralte of ITS post: with it, the EDIT writes back the peralte that post already has.
                copy.PostPeralte = SelectivePostGeometry.PostPeralteAt(System, target.PostIndex);
                destinations.Add((target, copy));
            }

            var view = FondoScope;
            var preparations = new List<SelectiveHeaderBatchPreparation>();
            var warnings = new List<HeaderBatchWarning<SelectiveHeaderAddress>>();
            try
            {
                foreach (var (address, copy) in destinations)
                {
                    _state.SetTargetFondos(new[] { address.FondoIndex });
                    var preparation = SelectiveHeaderBatchPlanner.Prepare(_state, resolution, SelectiveHeaderBatchRequest.Edit(copy, address.PostIndex));
                    if (preparation.PreconditionFailure.HasValue)
                        return SelectiveCabeceraWrite.Refuse("No se aplicó nada: el rack todavía no tiene geometría resuelta. Corrige primero los datos del rack.");
                    if (preparation.Plan is HeaderBatchPlan<SelectiveHeaderAddress>.Rejected rejected)
                        return SelectiveCabeceraWrite.Refuse("No se aplicó nada: " + CabeceraWhere(address.FondoIndex, address.PostIndex) + " " + Rejection(rejected.Code));
                    if (preparation.Plan is HeaderBatchPlan<SelectiveHeaderAddress>.Prepared prepared) warnings.AddRange(prepared.Warnings);
                    preparations.Add(preparation);
                }

                var severe = warnings.Where(w => w.Severity == HeaderWarningSeverity.Severe).ToList();
                if (severe.Count > 0 && !force)
                    return new SelectiveCabeceraWrite(null, string.Join(" ", severe.Select(w => Flat(w.Message))), null);

                foreach (var preparation in preparations)
                {
                    var outcome = _state.ApplyHeaderBatch(preparation, resolution);
                    if (outcome is HeaderBatchOutcome<SelectiveHeaderAddress>.Rejected stale)
                        return SelectiveCabeceraWrite.Refuse("No se aplicó todo: " + Rejection(stale.Code) + " «Descartar» del rack vuelve a lo último que se leyó o se escribió.");
                }
            }
            finally
            {
                SetFondoScope(view);
            }

            var informative = warnings.Where(w => w.Severity == HeaderWarningSeverity.Informative).Select(w => Flat(w.Message)).Distinct().ToList();
            return new SelectiveCabeceraWrite(new SelectiveDraftEdit(destinations.Count, 0, null), null, informative.Count == 0 ? null : "Aviso: " + string.Join(" ", informative));
        }

        /// <summary>
        /// «Volver a la calculada» (today «Restablecer poste») of ONE post of ONE fondo: its custom cabecera goes; the own peralte of the
        /// post stays (it is of the whole rack and is written in its own column). The caller recomposes.
        /// </summary>
        public SelectiveDraftEdit ResetCabeceraAt(int fondo, int post)
        {
            if (fondo < 0 || fondo >= FondoCount) return SelectiveDraftEdit.Refuse("Ese fondo no existe.");
            if (!HasCustomCabecera(fondo, post)) return SelectiveDraftEdit.Refuse("La cabecera de " + CabeceraWhere(fondo, post) + " ya es la calculada.");
            return WithinScope(fondo, () => ResetCustomCabecera(post));
        }

        private static string Rejection(HeaderRejectionCode code)
        {
            switch (code)
            {
                case HeaderRejectionCode.StaleTargets: return "el rack cambió mientras se preparaban las copias.";
                case HeaderRejectionCode.DestinationInvalid: return "no admite la cabecera (su profundidad o la cabecera normalizada no valen).";
                case HeaderRejectionCode.NoApplicableTargets:
                case HeaderRejectionCode.NoTargets: return "no existe en el rack.";
                default: return "no es una cabecera utilizable.";
            }
        }

        private static string Flat(string text) => (text ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").Trim();

        private static string Named(string rackName) => string.IsNullOrWhiteSpace(rackName) ? "este rack" : rackName.Trim();

        private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
