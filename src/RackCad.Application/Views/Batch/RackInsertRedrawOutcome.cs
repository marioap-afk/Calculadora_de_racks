using System;
using System.Globalization;

namespace RackCad.Application.Views.Batch
{
    /// <summary>
    /// T6 b1 (I-64, sixth batch; I-02 of the review of B-1 and the review of dc0ada4c): what the drawing half of «Insertar» tells the queue of
    /// ID18 after redrawing with the draft the views of the rack that exist (RACKEDITAR «Insertar» of Dinámico, Push Back, Cantilever and
    /// Cabecera, and «Insertar vista…» of the RackCad window), and when the views a change left obsolete are erased.
    ///
    /// <para>
    /// A view whose redraw fails keeps the design of before (<c>ViewBlockDraw.RedrawInPlace</c> turns any exception into a failure and the
    /// loops only stop counting it). If the queue placed the new views anyway, they would carry the draft and that view the design of before:
    /// sibling views of different designs. So the queue places only when every view that remains was redrawn; otherwise it stops with
    /// <see cref="IncompleteCode"/>. What was already redrawn is NOT undone (the redraws are not one transaction): the text says so, and that
    /// «U» undoes the whole command.
    /// </para>
    ///
    /// <para>
    /// When EVERY view that existed is obsolete (a lateral of a post the change removes, a frontal of side B of a rack that is no longer
    /// two-way), erasing them first would leave the rack without views if the placement were then cancelled (the last link of a rack is never
    /// erased). They are erased after the first new view is placed (<see cref="ErasesStaleAfterPlacement"/>): the new views carry the draft,
    /// and if nothing is placed nothing changed.
    /// </para>
    /// </summary>
    public static class RackInsertRedrawOutcome
    {
        /// <summary>The code that opens the diagnostic when the queue does not place because a view that exists was not redrawn.</summary>
        public const string IncompleteCode = "REDRAW_INCOMPLETE";

        /// <summary>
        /// Whether the obsolete views wait for the first new view: there are obsolete views and none of the <paramref name="views"/> of the
        /// rack survives the change. With a survivor they are erased at once, as before.
        /// </summary>
        public static bool ErasesStaleAfterPlacement(int views, int stale) => stale > 0 && stale >= views;

        /// <summary>
        /// The answer of the redraw to the queue: <see cref="RackViewBatchRedrawResult.Applied"/> when every view that remains was redrawn
        /// with the draft; otherwise a result that stops every placement, with the text of what happened. <paramref name="views"/> are the
        /// views of the rack the drawing half took, <paramref name="stale"/> those it found obsolete, <paramref name="redrawn"/> those it
        /// redrew and <paramref name="erased"/> the obsolete ones it erased before the queue (none when they wait for the first new view).
        /// </summary>
        public static RackViewBatchRedrawResult Of(int views, int stale, int redrawn, int erased)
        {
            views = Math.Max(0, views);
            stale = Math.Max(0, stale);
            var remaining = Math.Max(0, views - (ErasesStaleAfterPlacement(views, stale) ? stale : Math.Max(0, erased)));
            if (Math.Max(0, redrawn) >= remaining) return RackViewBatchRedrawResult.Applied();
            return RackViewBatchRedrawResult.RolledBack(IncompleteText(remaining - Math.Max(0, redrawn), remaining));
        }

        /// <summary>The diagnostic of <see cref="Of"/> when it stops the queue: how many views were not redrawn, and that nothing was undone.</summary>
        public static string IncompleteText(int missing, int remaining)
        {
            var what = remaining <= 1
                ? "no se pudo redibujar la vista que ya había del rack"
                : "no se pudo redibujar " + missing.ToString(CultureInfo.InvariantCulture) + " de las "
                  + remaining.ToString(CultureInfo.InvariantCulture) + " vistas que ya había del rack";
            return IncompleteCode + ": " + what + ", así que no se colocó ninguna vista nueva (sus hermanas quedarían con diseños distintos). "
                + "Lo que sí se redibujó con el borrador no se deshizo: «U» deshace la orden entera, y «Actualizar» vuelve a intentarlo.";
        }

        /// <summary>
        /// The line after the queue when the obsolete views waited for the first new view (<see cref="ErasesStaleAfterPlacement"/>): what was
        /// erased once <paramref name="placed"/> new views were placed, or that nothing changed when none was.
        /// </summary>
        public static string StaleAfterPlacementText(int stale, int erased, int placed)
        {
            if (placed <= 0)
                return "no se colocó ninguna vista nueva, así que las vistas que el cambio deja obsoletas se conservan y el rack no cambió.";
            var text = "todas las vistas que había quedaban obsoletas con el cambio; tras colocar la vista nueva se "
                + (erased == 1 ? "borró 1" : "borraron " + erased.ToString(CultureInfo.InvariantCulture));
            return erased >= stale
                ? text + "."
                : text + " de " + stale.ToString(CultureInfo.InvariantCulture) + ": revisa el dibujo.";
        }
    }
}
