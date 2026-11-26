using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The six boxes of «Restaurar layout…» (decision 8 of the Owner), in the order of the list.</summary>
    public enum SelectiveLayoutRestoreBox
    {
        BeamLength,
        Clear,
        HalfFront,
        ManualHeight,
        PostPeralte,
        CustomCabecera,
    }

    /// <summary>
    /// I-64 A-14, line D (Selectivo E2-B, decision 8 of the Owner): what «Restaurar layout…» restores. Every box is checked by default; an
    /// unchecked box keeps that personalisation as it is.
    /// </summary>
    public sealed record SelectiveLayoutRestoreOptions(
        bool BeamLength = true,
        bool Clear = true,
        bool HalfFront = true,
        bool ManualHeight = true,
        bool PostPeraltes = true,
        bool CustomCabeceras = true)
    {
        /// <summary>Everything checked (the default of the list).</summary>
        public static SelectiveLayoutRestoreOptions All { get; } = new SelectiveLayoutRestoreOptions();

        /// <summary>Nothing checked.</summary>
        public static SelectiveLayoutRestoreOptions None { get; } = new SelectiveLayoutRestoreOptions(false, false, false, false, false, false);

        /// <summary>At least one box is checked.</summary>
        public bool Any => BeamLength || Clear || HalfFront || ManualHeight || PostPeraltes || CustomCabeceras;

        /// <summary>Whether that box is checked.</summary>
        public bool Includes(SelectiveLayoutRestoreBox box) => box switch
        {
            SelectiveLayoutRestoreBox.BeamLength => BeamLength,
            SelectiveLayoutRestoreBox.Clear => Clear,
            SelectiveLayoutRestoreBox.HalfFront => HalfFront,
            SelectiveLayoutRestoreBox.ManualHeight => ManualHeight,
            SelectiveLayoutRestoreBox.PostPeralte => PostPeraltes,
            SelectiveLayoutRestoreBox.CustomCabecera => CustomCabeceras,
            _ => false,
        };
    }

    /// <summary>What a «Restaurar layout…» restored, counted for the notice.</summary>
    public sealed record SelectiveLayoutRestoreResult(
        int BeamLengths,
        int Clears,
        int HalfFronts,
        int ManualHeights,
        int PostPeraltes,
        int CustomCabeceras,
        bool PostPeraltesSkipped)
    {
        /// <summary>Total of personalisations restored.</summary>
        public int Total => BeamLengths + Clears + HalfFronts + ManualHeights + PostPeraltes + CustomCabeceras;

        /// <summary>What the unchecked boxes keep in the fondos of the scope (the own peraltes skipped by the scope are not counted here).</summary>
        public SelectiveLayoutRestoreResult Kept { get; init; }

        /// <summary>How many values that box counts.</summary>
        public int Count(SelectiveLayoutRestoreBox box) => box switch
        {
            SelectiveLayoutRestoreBox.BeamLength => BeamLengths,
            SelectiveLayoutRestoreBox.Clear => Clears,
            SelectiveLayoutRestoreBox.HalfFront => HalfFronts,
            SelectiveLayoutRestoreBox.ManualHeight => ManualHeights,
            SelectiveLayoutRestoreBox.PostPeralte => PostPeraltes,
            SelectiveLayoutRestoreBox.CustomCabecera => CustomCabeceras,
            _ => 0,
        };

        /// <summary>
        /// The categories with something, in the order of the list (spec of the screen, §6): «3 largos de larguero, 1 claro, 1 medio frente,
        /// 2 alturas manuales, 1 peralte propio, 2 cabeceras personalizadas». Empty when there is nothing.
        /// </summary>
        public string Breakdown
        {
            get
            {
                var parts = new List<string>();
                Add(parts, BeamLengths, "largo de larguero", "largos de larguero");
                Add(parts, Clears, "claro", "claros");
                Add(parts, HalfFronts, "medio frente", "medios frentes");
                Add(parts, ManualHeights, "altura manual", "alturas manuales");
                Add(parts, PostPeraltes, "peralte propio", "peraltes propios");
                Add(parts, CustomCabeceras, "cabecera personalizada", "cabeceras personalizadas");
                return string.Join(", ", parts);
            }
        }

        /// <summary>What «Descartar» does for a rack of the drawing: it goes back to what was last read or written.</summary>
        public const string DiscardText = "«Descartar» vuelve a lo último que se leyó o se escribió.";

        /// <summary>What «Descartar» does for a new Selectivo of «Nuevo», never read nor written: it goes back to the defaults (P3-05).</summary>
        public const string NewRackDiscardText = "«Descartar» vuelve a los valores por defecto.";

        /// <summary>
        /// The notice for the user (spec of the screen, §6): how many manual values were removed and which, what the unchecked boxes keep,
        /// and the own peraltes kept because the scope is not the whole rack. For a rack of the drawing (<see cref="NoticeFor"/>).
        /// </summary>
        public string Notice => NoticeFor(newRack: false);

        /// <summary>
        /// The notice, with what «Descartar» does for that rack: a rack of the drawing goes back to what was last read or written, a new
        /// Selectivo to the defaults (M-4 of the review of the entrega 2).
        /// </summary>
        public string NoticeFor(bool newRack)
        {
            string text;
            if (Total == 0)
            {
                text = "No había nada que restaurar.";
            }
            else
            {
                text = "Layout restaurado en el borrador: " + Plural(Total, "valor manual quitado", "valores manuales quitados")
                       + " (" + Breakdown + "). " + (newRack ? NewRackDiscardText : DiscardText);
                if (Kept != null && Kept.Total > 0) text += " Se conservan: " + Kept.Breakdown + ".";
            }

            if (PostPeraltesSkipped)
                text += " Los peraltes propios son de todo el rack: se restauran solo con «Todos los fondos».";
            return text;
        }

        private static void Add(List<string> parts, int count, string one, string many)
        {
            if (count > 0) parts.Add(Plural(count, one, many));
        }

        private static string Plural(int count, string one, string many) => count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
    }

    /// <summary>One value «Restaurar layout…» would remove, with the box that removes it (a line of «Ver lo que se pierde»).</summary>
    public sealed record SelectiveLayoutRestoreLoss(SelectiveLayoutRestoreBox Box, string Text);

    /// <summary>
    /// «Restaurar layout…» over the draft of the RackCad window: the personalisations checked go back to automatic in the fondos of the scope
    /// (manual length and clear of each larguero, the tramos of a half front, the manual height of a front, the custom cabeceras), and the
    /// others stay. Each one is restored with the same write the editor uses for it with a blank value (the classic rule «vacío = auto»), and
    /// the custom cabecera like the classic «Restablecer poste». The own peralte of a post is GLOBAL (I-43): like «Restablecer poste», it is
    /// restored only when the scope is every fondo.
    /// </summary>
    public static class SelectiveLayoutRestore
    {
        /// <summary>Counts what <see cref="Apply"/> would restore, without writing.</summary>
        public static SelectiveLayoutRestoreResult Preview(SelectiveDraftEditor editor, SelectiveLayoutRestoreOptions options)
            => Run(editor, options ?? SelectiveLayoutRestoreOptions.All, write: false);

        /// <summary>Restores the checked personalisations of the fondos of the scope and says what it restored.</summary>
        public static SelectiveLayoutRestoreResult Apply(SelectiveDraftEditor editor, SelectiveLayoutRestoreOptions options)
            => Run(editor, options ?? SelectiveLayoutRestoreOptions.All, write: true);

        /// <summary>
        /// Every value that «Restaurar layout…» with every box checked removes in the fondos of the scope, one line per value with the texts
        /// of the approved draft (<c>lossList</c>) and in its order: fondo by fondo and front by front, its half front, its manual height and
        /// the manual length and clear of each level; then the own peraltes (only when the scope is every fondo) and the custom cabeceras.
        /// «Fondo n · » only when the rack has more than one fondo. The lines of each box are as many as <see cref="Preview"/> counts.
        /// </summary>
        public static IReadOnlyList<SelectiveLayoutRestoreLoss> Losses(SelectiveDraftEditor editor)
        {
            var losses = new List<SelectiveLayoutRestoreLoss>();
            var fondos = editor.ScopeFondos;
            var several = editor.FondoCount > 1;
            string FondoPrefix(int fondo) => several ? "Fondo " + N(fondo + 1) + " · " : string.Empty;

            foreach (var fondo in fondos)
            {
                for (var front = 0; front < editor.FrontCount(fondo); front++)
                {
                    var where = FondoPrefix(fondo) + "F" + N(front + 1);
                    var draftFront = editor.Front(fondo, front);
                    if (draftFront != null && draftFront.IsSplit)
                    {
                        var tramos = draftFront.Segments.Count;
                        losses.Add(new SelectiveLayoutRestoreLoss(SelectiveLayoutRestoreBox.HalfFront,
                            "Medio frente en " + where + " (" + N(tramos) + (tramos == 1 ? " tramo)" : " tramos)")));
                    }

                    if (draftFront?.HeightOverride is double height)
                        losses.Add(new SelectiveLayoutRestoreLoss(SelectiveLayoutRestoreBox.ManualHeight, "Altura manual de cabecera en " + where + " (" + Num(height) + " in)"));

                    for (var level = 0; level < editor.LevelCount(fondo, front); level++)
                    {
                        var cell = editor.Cell(fondo, front, level);
                        if (cell == null) continue;
                        var at = where + " · N" + N(level + 1);
                        if (cell.BeamLength is double length)
                            losses.Add(new SelectiveLayoutRestoreLoss(SelectiveLayoutRestoreBox.BeamLength, "Largo manual de larguero en " + at + " (" + Num(length) + " in)"));
                        if (cell.Clear is double clear)
                            losses.Add(new SelectiveLayoutRestoreLoss(SelectiveLayoutRestoreBox.Clear, "Claro manual en " + at + " (" + Num(clear) + " in)"));
                    }
                }
            }

            if (fondos.Count >= editor.FondoCount)
            {
                for (var post = 0; post < editor.PostCount; post++)
                {
                    if (editor.PostPeralteOverride(post) is double peralte)
                        losses.Add(new SelectiveLayoutRestoreLoss(SelectiveLayoutRestoreBox.PostPeralte, "Peralte propio en P" + N(post + 1) + " (" + Num(peralte) + " in)"));
                }
            }

            foreach (var fondo in fondos)
            {
                for (var post = 0; post < editor.PostCount; post++)
                {
                    if (editor.HasCustomCabecera(fondo, post))
                        losses.Add(new SelectiveLayoutRestoreLoss(SelectiveLayoutRestoreBox.CustomCabecera, "Cabecera personalizada en " + FondoPrefix(fondo) + "P" + N(post + 1)));
                }
            }

            return losses;
        }

        private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static SelectiveLayoutRestoreResult Run(SelectiveDraftEditor editor, SelectiveLayoutRestoreOptions options, bool write)
        {
            var fondos = editor.ScopeFondos;
            var lengthCells = new List<SelectiveMatrixPosition>();
            var clearCells = new List<SelectiveMatrixPosition>();
            int lengths = 0, clears = 0;
            var splitFronts = new SortedSet<int>();
            var heightFronts = new SortedSet<int>();
            int splits = 0, heights = 0;

            foreach (var fondo in fondos)
            {
                for (var front = 0; front < editor.FrontCount(fondo); front++)
                {
                    var draftFront = editor.Front(fondo, front);
                    if (draftFront != null && draftFront.IsSplit) { splits++; splitFronts.Add(front); }
                    if (draftFront != null && draftFront.HeightOverride.HasValue) { heights++; heightFronts.Add(front); }

                    for (var level = 0; level < editor.LevelCount(fondo, front); level++)
                    {
                        var cell = editor.Cell(fondo, front, level);
                        if (cell == null) continue;
                        var position = new SelectiveMatrixPosition(front, level);
                        if (cell.BeamLength.HasValue) { lengths++; if (!lengthCells.Contains(position)) lengthCells.Add(position); }
                        if (cell.Clear.HasValue) { clears++; if (!clearCells.Contains(position)) clearCells.Add(position); }
                    }
                }
            }

            var allFondos = fondos.Count >= editor.FondoCount;
            var peraltePosts = Enumerable.Range(0, editor.PostCount).Where(post => editor.PostPeralteOverride(post).HasValue).ToList();
            var cabeceraPosts = Enumerable.Range(0, editor.PostCount)
                .Select(post => (post, count: fondos.Count(fondo => editor.HasCustomCabecera(fondo, post))))
                .Where(entry => entry.count > 0)
                .ToList();
            var cabeceras = cabeceraPosts.Sum(entry => entry.count);
            var peraltes = allFondos ? peraltePosts.Count : 0;

            var result = new SelectiveLayoutRestoreResult(
                options.BeamLength ? lengths : 0,
                options.Clear ? clears : 0,
                options.HalfFront ? splits : 0,
                options.ManualHeight ? heights : 0,
                options.PostPeraltes ? peraltes : 0,
                options.CustomCabeceras ? cabeceras : 0,
                options.PostPeraltes && !allFondos && peraltePosts.Count > 0)
            {
                Kept = new SelectiveLayoutRestoreResult(
                    options.BeamLength ? 0 : lengths,
                    options.Clear ? 0 : clears,
                    options.HalfFront ? 0 : splits,
                    options.ManualHeight ? 0 : heights,
                    options.PostPeraltes ? 0 : peraltes,
                    options.CustomCabeceras ? 0 : cabeceras,
                    false),
            };
            if (!write) return result;

            if (options.BeamLength && lengthCells.Count > 0) editor.SetBeamLengthOverride(lengthCells, null);
            if (options.Clear && clearCells.Count > 0) editor.SetClearOverride(clearCells, null);
            if (options.HalfFront)
            {
                foreach (var front in splitFronts) editor.SetSegments(front, null);
            }

            if (options.ManualHeight && heightFronts.Count > 0) editor.SetHeightOverride(heightFronts, null);
            if (options.CustomCabeceras)
            {
                foreach (var entry in cabeceraPosts) editor.ResetCustomCabecera(entry.post);
            }

            if (options.PostPeraltes && allFondos)
            {
                foreach (var post in peraltePosts) editor.SetPostPeralte(post, null);
            }

            if (result.Total > 0) editor.Recompose();
            return result;
        }
    }
}
