using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The own peralte one post had before «Aplicar a todos» took it away (T5 pp).</summary>
    public sealed record SelectiveOwnPostPeralte(int Post, double Peralte);

    /// <summary>
    /// What «Aplicar a todos» of «Avanzado» › «Postes y cabeceras» did in the draft of a Selectivo (I-64, fifth batch, part pp; the request of
    /// the Owner of 2026-11-26: «no hay forma de colocar un peralte de poste en todos los postes, tienes que ir uno por uno»). The peralte is
    /// the one of the rack now and no post keeps an own one; <see cref="Removed"/> are the own peraltes it took away, in the order of the posts,
    /// and <see cref="PlatesKept"/> the custom cabeceras whose base plates keep a peralte of their own (they do not follow the post). The draft
    /// is recomposed already (the check of the «medios frentes» needs it); the caller may recompose again, as after every write.
    /// </summary>
    public sealed record SelectivePeralteForAllPosts(
        SelectiveDraftEdit Edit,
        double Peralte,
        IReadOnlyList<SelectiveOwnPostPeralte> Removed,
        bool RackPeralteChanged,
        IReadOnlyList<string> PlatesKept)
    {
        public bool Refused => Edit.Refused;

        /// <summary>Whether the draft changed: the peralte of the rack, or at least one own peralte taken away.</summary>
        public bool Changed => !Refused && (RackPeralteChanged || Removed.Count > 0);

        /// <summary>
        /// What the editor says (the message of the Owner's request says how many own peraltes it took away), the base plates that keep their
        /// own peralte, and what «Descartar» does for that rack: a rack of the drawing goes back to what was last read or written, a new
        /// Selectivo to how it opened (T6 ui, UT-S11). T6 ui (point 8): with <paramref name="canDiscard"/> false (the draft is again what the
        /// drawing has, so «Descartar» is off) it says nothing of «Descartar»; <paramref name="copy"/>, a copy of «Duplicar» (correction 1).
        /// </summary>
        public string NoticeFor(bool newRack, bool canDiscard = true, bool copy = false)
        {
            if (Refused) return Edit.Error;
            var peralte = Num(Peralte) + " in";
            var plates = PlatesKept.Count == 0 ? string.Empty : " " + SelectiveDraftEditor.OwnPlatePeralteText(PlatesKept);
            if (!Changed) return "Todos los postes ya tienen " + peralte + " y ninguno tiene peralte propio: el borrador no cambió." + plates;

            var removed = Removed.Count == 0
                ? "ningún poste tenía peralte propio"
                : "se " + (Removed.Count == 1 ? "quitó 1 peralte propio" : "quitaron " + Removed.Count.ToString(CultureInfo.InvariantCulture) + " peraltes propios")
                  + " (" + string.Join(", ", Removed.Select(r => "P" + (r.Post + 1).ToString(CultureInfo.InvariantCulture) + ": " + Num(r.Peralte) + " in")) + ")";
            return "Peralte de " + peralte + " en todos los postes del borrador: " + removed + "." + plates
                   + (canDiscard ? " " + SelectiveLayoutRestoreResult.DiscardTextOf(newRack, copy) : string.Empty);
        }

        internal static SelectivePeralteForAllPosts Refuse(string error)
            => new SelectivePeralteForAllPosts(SelectiveDraftEdit.Refuse(error), 0.0, Array.Empty<SelectiveOwnPostPeralte>(), false, Array.Empty<string>());

        private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// T5 pp: one peralte for every post of a Selectivo in one gesture. Until now the peralte of the rack («Peralte del poste») only reached the
    /// posts with no own peralte, and taking the own ones away was one box at a time in «Avanzado» (or «Restaurar layout…» › «Peraltes
    /// propios», which also takes the rest of the layout). The own peralte is of the post in every fondo (the authority of
    /// <c>SelectiveEditorState.PostPeraltes</c>), so this write is of the whole rack, whatever the fondos of the view are.
    /// <para>
    /// Correction 1 of the review: a larger peralte makes every intermediate post of a «medio frente» take more of its front (the troquel of the
    /// larguero slides with the peralte of the post), so a split can stop fitting and the resolver would draw the whole front with no notice
    /// (DG-10). Both writes of the peralte of the rack here («Aplicar a todos» and <see cref="SetRackPostPeralte"/>, the box of «Lo esencial»)
    /// recompose and check every split front with <see cref="SelectiveSegmentFit.Of"/>: a split that fitted and no longer does undoes the
    /// write, all of it, and refuses it with the front and the reason. A split that did not fit before is not a reason (it was drawn whole
    /// already).
    /// </para>
    /// </summary>
    public sealed partial class SelectiveDraftEditor
    {
        /// <summary>The text of a peralte that is not a number greater than zero.</summary>
        public const string InvalidAllPostsPeralteText = "El peralte de poste debe ser un número mayor que cero.";

        /// <summary>The posts with an own peralte now, in their order, with that peralte.</summary>
        public IReadOnlyList<SelectiveOwnPostPeralte> OwnPostPeraltes()
            => Enumerable.Range(0, PostCount)
                .Where(post => PostPeralteOverride(post).HasValue)
                .Select(post => new SelectiveOwnPostPeralte(post, PostPeralteOverride(post).Value))
                .ToList();

        /// <summary>
        /// Correction 3 of the review: the custom cabeceras («P2», «P2 · Fondo 1») whose base plates have a peralte of their own
        /// (<c>PeralteOverride</c> of the left or the right plate). Those plates keep it when the peralte of the post changes.
        /// </summary>
        public IReadOnlyList<string> CabecerasWithOwnPlatePeralte()
        {
            var list = new List<string>();
            for (var fondo = 0; fondo < FondoCount; fondo++)
            {
                for (var post = 0; post <= FrontCount(fondo); post++)
                {
                    var cabecera = _state.CabeceraAt(fondo, post);
                    if (cabecera?.LeftBasePlate?.PeralteOverride != null || cabecera?.RightBasePlate?.PeralteOverride != null)
                        list.Add(CabeceraWhere(fondo, post));
                }
            }

            return list;
        }

        /// <summary>«La cabecera personalizada de P2 tiene placas base con peralte propio: no cambian con el poste.» (or the plural).</summary>
        public static string OwnPlatePeralteText(IReadOnlyList<string> cabeceras)
        {
            if (cabeceras == null || cabeceras.Count == 0) return string.Empty;
            var names = cabeceras.Count == 1 ? cabeceras[0] : string.Join(", ", cabeceras.Take(cabeceras.Count - 1)) + " y " + cabeceras[cabeceras.Count - 1];
            return cabeceras.Count == 1
                ? "La cabecera personalizada de " + names + " tiene placas base con peralte propio: no cambian con el poste."
                : "Las cabeceras personalizadas de " + names + " tienen placas base con peralte propio: no cambian con el poste.";
        }

        /// <summary>
        /// «Aplicar a todos»: <paramref name="peralte"/> becomes the peralte of the rack and every own peralte goes, so every post of every
        /// fondo takes it. A value that is not a number greater than zero writes nothing; a «medio frente» that would stop fitting writes
        /// nothing either (correction 1 of the review), and the refusal names it.
        /// </summary>
        public SelectivePeralteForAllPosts SetPeralteForAllPosts(double peralte)
        {
            if (!(peralte > 0.0) || double.IsInfinity(peralte)) return SelectivePeralteForAllPosts.Refuse(InvalidAllPostsPeralteText);

            var removed = OwnPostPeraltes();
            var plates = CabecerasWithOwnPlatePeralte();
            var rackChanged = Math.Abs(PostPeralte - peralte) > 1e-9;
            var previousRack = PostPeralte;
            var previousOwn = _state.PostPeraltes.ToList();
            var refusal = KeepingTheSplits(peralte, () =>
            {
                PostPeralte = peralte;
                for (var post = 0; post < _state.PostPeraltes.Count; post++) _state.PostPeraltes[post] = 0.0;
            }, () =>
            {
                PostPeralte = previousRack;
                for (var post = 0; post < _state.PostPeraltes.Count && post < previousOwn.Count; post++) _state.PostPeraltes[post] = previousOwn[post];
            });
            if (refusal != null) return SelectivePeralteForAllPosts.Refuse(refusal);
            return new SelectivePeralteForAllPosts(new SelectiveDraftEdit(PostCount, 0, null), peralte, removed, rackChanged, plates);
        }

        /// <summary>
        /// The peralte of the rack alone («Lo esencial» › «Peralte de poste»): the own peraltes stay. A «medio frente» that would stop fitting
        /// writes nothing (correction 1 of the review), as «Aplicar a todos».
        /// </summary>
        public SelectiveDraftEdit SetRackPostPeralte(double peralte)
        {
            if (!(peralte > 0.0) || double.IsInfinity(peralte)) return SelectiveDraftEdit.Refuse(InvalidAllPostsPeralteText);
            var previous = PostPeralte;
            var refusal = KeepingTheSplits(peralte, () => PostPeralte = peralte, () => PostPeralte = previous);
            return refusal != null ? SelectiveDraftEdit.Refuse(refusal) : new SelectiveDraftEdit(1, 0, null);
        }

        // Writes, recomposes and checks the split fronts: one that fitted and no longer fits undoes the write, recomposes back and gives the
        // refusal. A recomposition that fails is left to the caller, which recomposes again, says why and goes back to the last valid one.
        private string KeepingTheSplits(double peralte, Action write, Action undo)
        {
            var before = SplitsThatDoNotFit();
            write();
            if (!Recompose()) return null;
            var broken = SplitsThatDoNotFit().Where(pair => !before.ContainsKey(pair.Key)).ToList();
            if (broken.Count == 0) return null;

            undo();
            Recompose();
            var names = broken.Select(pair => FrontName(pair.Key.Fondo, pair.Key.Front)).ToList();
            var first = broken[0];
            return "No se aplicó el peralte de " + Num(peralte) + " in: "
                   + (names.Count == 1 ? "el medio frente de " + names[0] + " dejaría de caber" : "los medios frentes de " + string.Join(", ", names.Take(names.Count - 1)) + " y " + names[names.Count - 1] + " dejarían de caber")
                   + ", porque cada poste intermedio ocupa más con más peralte. "
                   + (names.Count == 1 ? string.Empty : "En " + names[0] + ": ")
                   + SelectiveSegmentFit.RefusalOf(first.Value, severalFondos: false)
                   + " Ajusta antes sus tramos en «Matriz» (inspector del frente).";
        }

        // The split fronts the last valid system does not fit, by fondo and front, with how they do not fit.
        private Dictionary<(int Fondo, int Front), SelectiveSegmentFitResult> SplitsThatDoNotFit()
        {
            var list = new Dictionary<(int Fondo, int Front), SelectiveSegmentFitResult>();
            if (System == null) return list;
            for (var fondo = 0; fondo < FondoCount; fondo++)
            {
                for (var front = 0; front < FrontCount(fondo); front++)
                {
                    var data = Front(fondo, front);
                    if (data == null || !data.IsSplit) continue;
                    var segments = data.Segments;
                    var fit = SelectiveSegmentFit.Of(System, Catalog, new[] { fondo }, front, segments.Take(segments.Count - 1).Select(s => s.Length).ToList(), segments.Count);
                    if (!fit.Fits) list[(fondo, front)] = fit;
                }
            }

            return list;
        }

        private string FrontName(int fondo, int front)
            => "F" + (front + 1).ToString(CultureInfo.InvariantCulture) + (FondoCount > 1 ? " del fondo " + (fondo + 1).ToString(CultureInfo.InvariantCulture) : string.Empty);
    }
}
