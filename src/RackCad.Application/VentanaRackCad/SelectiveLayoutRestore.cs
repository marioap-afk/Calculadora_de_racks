using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
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

        /// <summary>The notice for the user: what was restored, and the own peraltes kept because the scope is not the whole rack.</summary>
        public string Notice
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
                var text = parts.Count == 0 ? "No había nada que restaurar." : "Restaurado: " + string.Join(", ", parts) + ".";
                if (PostPeraltesSkipped)
                    text += " Los peraltes propios son de todo el rack: se restauran solo con «Todos los fondos».";
                return text;
            }
        }

        private static void Add(List<string> parts, int count, string one, string many)
        {
            if (count > 0) parts.Add(count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many));
        }
    }

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

            var result = new SelectiveLayoutRestoreResult(
                options.BeamLength ? lengths : 0,
                options.Clear ? clears : 0,
                options.HalfFront ? splits : 0,
                options.ManualHeight ? heights : 0,
                options.PostPeraltes && allFondos ? peraltePosts.Count : 0,
                options.CustomCabeceras ? cabeceraPosts.Sum(entry => entry.count) : 0,
                options.PostPeraltes && !allFondos && peraltePosts.Count > 0);
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
