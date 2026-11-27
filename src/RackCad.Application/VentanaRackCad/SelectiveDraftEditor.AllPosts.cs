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
    /// the one of the rack now and no post keeps an own one; <see cref="Removed"/> are the own peraltes it took away, in the order of the posts.
    /// The caller recomposes, as every write of this draft.
    /// </summary>
    public sealed record SelectivePeralteForAllPosts(SelectiveDraftEdit Edit, double Peralte, IReadOnlyList<SelectiveOwnPostPeralte> Removed, bool RackPeralteChanged)
    {
        public bool Refused => Edit.Refused;

        /// <summary>Whether the draft changed: the peralte of the rack, or at least one own peralte taken away.</summary>
        public bool Changed => !Refused && (RackPeralteChanged || Removed.Count > 0);

        /// <summary>
        /// What the editor says (the message of the Owner's request says how many own peraltes it took away), with what «Descartar» does for
        /// that rack: a rack of the drawing goes back to what was last read or written, a new Selectivo to the defaults.
        /// </summary>
        public string NoticeFor(bool newRack)
        {
            if (Refused) return Edit.Error;
            var peralte = Num(Peralte) + " in";
            if (!Changed) return "Todos los postes ya tienen " + peralte + " y ninguno tiene peralte propio: el borrador no cambió.";

            var removed = Removed.Count == 0
                ? "ningún poste tenía peralte propio"
                : "se " + (Removed.Count == 1 ? "quitó 1 peralte propio" : "quitaron " + Removed.Count.ToString(CultureInfo.InvariantCulture) + " peraltes propios")
                  + " (" + string.Join(", ", Removed.Select(r => "P" + (r.Post + 1).ToString(CultureInfo.InvariantCulture) + ": " + Num(r.Peralte) + " in")) + ")";
            return "Peralte de " + peralte + " en todos los postes del borrador: " + removed + ". "
                   + (newRack ? SelectiveLayoutRestoreResult.NewRackDiscardText : SelectiveLayoutRestoreResult.DiscardText);
        }

        internal static SelectivePeralteForAllPosts Refuse(string error)
            => new SelectivePeralteForAllPosts(SelectiveDraftEdit.Refuse(error), 0.0, Array.Empty<SelectiveOwnPostPeralte>(), false);

        private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// T5 pp: one peralte for every post of a Selectivo in one gesture. Until now the peralte of the rack («Peralte del poste») only reached the
    /// posts with no own peralte, and taking the own ones away was one box at a time in «Avanzado» (or «Restaurar layout…» › «Peraltes
    /// propios», which also takes the rest of the layout). The own peralte is of the post in every fondo (the authority of
    /// <c>SelectiveEditorState.PostPeraltes</c>), so this write is of the whole rack, whatever the fondos of the view are.
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
        /// «Aplicar a todos»: <paramref name="peralte"/> becomes the peralte of the rack and every own peralte goes, so every post of every
        /// fondo takes it. A value that is not a number greater than zero writes nothing. The caller recomposes.
        /// </summary>
        public SelectivePeralteForAllPosts SetPeralteForAllPosts(double peralte)
        {
            if (!(peralte > 0.0) || double.IsInfinity(peralte)) return SelectivePeralteForAllPosts.Refuse(InvalidAllPostsPeralteText);

            var removed = OwnPostPeraltes();
            var rackChanged = Math.Abs(PostPeralte - peralte) > 1e-9;
            PostPeralte = peralte;
            for (var post = 0; post < _state.PostPeraltes.Count; post++) _state.PostPeraltes[post] = 0.0;
            return new SelectivePeralteForAllPosts(new SelectiveDraftEdit(PostCount, 0, null), peralte, removed, rackChanged);
        }
    }
}
