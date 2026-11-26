using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// What a write of T or P on the selected cells did: how many cells it wrote, and why it left the others (no larguero there, the
    /// fondos of the scope disagree on «Larguero a piso», or the cell does not exist in the fondos of the scope), with the words for the
    /// line of messages. <see cref="ToDraftEdit"/> is the shape the editor of the window applies.
    /// </summary>
    public sealed record SelectiveSafetyEdit(
        int Written,
        int OnFloor,
        int Ambiguous,
        int Missing,
        IReadOnlyList<int> AmbiguousFronts,
        IReadOnlyList<int> ShiftedFronts,
        string Error)
    {
        public bool Refused => Error != null;

        public int Omitted => OnFloor + Ambiguous + Missing;

        public static SelectiveSafetyEdit Refuse(string error)
            => new SelectiveSafetyEdit(0, 0, 0, 0, Array.Empty<int>(), Array.Empty<int>(), error);

        /// <summary>The reason of a refusal, or what the user has to know about a write; null when there is nothing to say.</summary>
        public string Notice
        {
            get
            {
                if (Error != null) return Error;
                var parts = new List<string>();
                if (OnFloor > 0) parts.Add(SelectiveSafetyView.OnFloorText(OnFloor));
                if (Ambiguous > 0) parts.Add(SelectiveSafetyView.AmbiguousText(AmbiguousFronts));
                if (Missing > 0) parts.Add("Parte de lo elegido no existe en los fondos de la vista.");
                if (ShiftedFronts.Count > 0) parts.Add(SelectiveSafetyView.ShiftedText(ShiftedFronts));
                return parts.Count == 0 ? null : string.Join(" ", parts);
            }
        }

        /// <summary>The write as the editor applies it: its notice is said by the editor of the window, not by the generic one.</summary>
        public SelectiveDraftEdit ToDraftEdit() => Refused ? SelectiveDraftEdit.Refuse(Error) : new SelectiveDraftEdit(Written, 0, null);
    }

    /// <summary>
    /// «Extremos y seguridad» of a Selectivo in the RackCad window (I-64 A-14, line D, entrega 2; decisions 4, 5, 6 and 13 of the Owner),
    /// read from and written to the safety of the draft (<see cref="SelectiveDraftEditor.CopySafety"/> / <see cref="SelectiveDraftEditor.ReplaceSafety"/>).
    /// <para>
    /// <b>T (tope) and P (parrilla)</b> are per front × LARGUERO for the whole rack, not per fondo: the off cells of the classic grid
    /// (<see cref="SelectiveTopeConfig.OffCells"/>, <see cref="SelectiveParrillaConfig.OffCells"/>) are keyed by the RESOLVED beam levels
    /// of a front (the resolver drops the floor pallet row of a front without «Larguero a piso»; the classic dialog calls its rows
    /// «Larg. 1», «Larg. 2»…), and the BOM applies one key to the same larguero of that front in every fondo (the parrilla to all of
    /// them, the tope to the fondos of its spots). A level of the MATRIX of the window (N1 is the floor row) is translated to its larguero
    /// with the «Larguero a piso» of ITS OWN fondo, read on the resolved system the BOM counts (<see cref="GridLevel"/>): N1 of a front
    /// without it is no cell (decision 4). With several fondos in the scope that disagree on «Larguero a piso» at a front, the same row of
    /// the matrix is a different larguero in each, so that front is not written from there (the fondo selector writes it).
    /// </para>
    /// <para>
    /// <b>The table by post</b> is <see cref="SelectivePostSafetySummary"/> over the last valid recomposition (what «Actualizar» writes).
    /// Variant and measures stay in «Editar seguridad…» (the classic dialog).
    /// </para>
    /// </summary>
    public static class SelectiveSafetyView
    {
        /// <summary>The near face of the rack (the front of fondo 1, the low end of its own depth axis): the classic «Izquierda».</summary>
        public const string Left = "Izquierda";

        /// <summary>The far face of the rack (the back of the last fondo): the classic «Derecha».</summary>
        public const string Right = "Derecha";

        /// <summary>
        /// A bota on BOTH faces of a post. <b>PROVISIONAL</b>: open question to the Owner (question 2 of the sprint of 2026-11-25); the
        /// classic dialog calls that choice «Ambas». The only place that says it: changing the answer is changing this constant.
        /// </summary>
        public const string BothSides = "Izquierda y Derecha";

        public const string NoSide = "";

        // ------------------------------------------------------------------ the grid of T and P

        /// <summary>Resolved beam levels per front of the whole rack (the maximum over the fondos, the classic grid); empty before a valid recomposition.</summary>
        public static IReadOnlyList<int> LevelCounts(SelectiveDraftEditor editor)
            => editor?.System == null ? Array.Empty<int>() : SelectiveSafetyGrid.LevelCounts(editor.System);

        /// <summary>
        /// The larguero (grid level of T and P) of a level of the MATRIX in one fondo: the same level when the front of that fondo has
        /// «Larguero a piso», one less when its first pallet rests on the floor (whose N1 is then no cell: null). Read on the resolved
        /// system, as the BOM reads it; null when that fondo does not have the front or the level.
        /// </summary>
        public static int? GridLevel(SelectiveDraftEditor editor, int fondo, int front, int matrixLevel)
        {
            var bay = Bay(editor, fondo, front);
            if (bay == null || matrixLevel < 0) return null;
            var level = OnFloor(bay) ? matrixLevel - 1 : matrixLevel;
            return level >= 0 && level < bay.Levels.Count ? level : (int?)null;
        }

        /// <summary>True when the cell of the matrix carries a larguero in some fondo of the scope.</summary>
        public static bool IsCell(SelectiveDraftEditor editor, int front, int matrixLevel)
            => Fondos(editor).Any(fondo => GridLevel(editor, fondo, front, matrixLevel).HasValue);

        /// <summary>The selection of a grid family (<see cref="SelectiveSafetyDefaults.TopeType"/> or <see cref="SelectiveSafetyDefaults.ParrillaType"/>) in the draft; null when none is chosen.</summary>
        public static SelectiveSafetySelection Selected(SelectiveDraftEditor editor, string type)
            => editor == null ? null : SelectiveSafetyFamilies.SelectedOfType(editor.CopySafety(), editor.Catalog?.SafetyElements, type);

        /// <summary>T at a cell over the scope: null when no tope is chosen, the cell has no larguero, or the fondos of the scope differ.</summary>
        public static bool? Tope(SelectiveDraftEditor editor, int front, int level)
            => Common(Read(editor, SelectiveSafetyDefaults.TopeType, new[] { new SelectiveMatrixPosition(front, level) }));

        /// <summary>P at a cell over the scope: null when no parrilla is chosen, the cell has no larguero, or the fondos of the scope differ.</summary>
        public static bool? Parrilla(SelectiveDraftEditor editor, int front, int level)
            => Common(Read(editor, SelectiveSafetyDefaults.ParrillaType, new[] { new SelectiveMatrixPosition(front, level) }));

        /// <summary>T at the selected cells over the scope: varies when they differ; cells without larguero do not count.</summary>
        public static SelectiveScopedValue<bool> ReadTope(SelectiveDraftEditor editor, IEnumerable<SelectiveMatrixPosition> positions)
            => Read(editor, SelectiveSafetyDefaults.TopeType, positions);

        /// <summary>P at the selected cells over the scope: varies when they differ; cells without larguero do not count.</summary>
        public static SelectiveScopedValue<bool> ReadParrilla(SelectiveDraftEditor editor, IEnumerable<SelectiveMatrixPosition> positions)
            => Read(editor, SelectiveSafetyDefaults.ParrillaType, positions);

        /// <summary>Turns T on or off at the selected cells, for the whole rack. Refuses when no tope is chosen.</summary>
        public static SelectiveSafetyEdit WriteTope(SelectiveDraftEditor editor, IEnumerable<SelectiveMatrixPosition> positions, bool on)
            => Write(editor, SelectiveSafetyDefaults.TopeType, positions, on);

        /// <summary>Turns P on or off at the selected cells, for the whole rack. Refuses when no parrilla is chosen.</summary>
        public static SelectiveSafetyEdit WriteParrilla(SelectiveDraftEditor editor, IEnumerable<SelectiveMatrixPosition> positions, bool on)
            => Write(editor, SelectiveSafetyDefaults.ParrillaType, positions, on);

        /// <summary>
        /// «En todas las celdas» / «En ninguna» of a family card: every larguero of the classic grid on or off, in every fondo. A family
        /// with every cell off keeps its variant (so «En todas las celdas» brings it back) and places nothing; the classic «Aceptar»
        /// drops it instead (declared difference: the drawing and the BOM are the same).
        /// </summary>
        public static SelectiveSafetyEdit WriteAll(SelectiveDraftEditor editor, string type, bool on)
        {
            if (editor?.System == null) return SelectiveSafetyEdit.Refuse(NoSystem);
            var safety = editor.CopySafety().ToList();
            var selection = SelectiveSafetyFamilies.SelectedOfType(safety, editor.Catalog?.SafetyElements, type);
            if (selection == null) return SelectiveSafetyEdit.Refuse(ChooseFirst(type));

            var counts = LevelCounts(editor);
            var off = OffCells(selection, type);
            off.Clear();
            if (!on)
            {
                for (var front = 0; front < counts.Count; front++)
                {
                    for (var level = 0; level < counts[front]; level++)
                    {
                        off.Add(new SelectiveGridCell { Frente = front, Level = level });
                    }
                }
            }

            editor.ReplaceSafety(safety);
            return new SelectiveSafetyEdit(counts.Sum(), 0, 0, 0, Array.Empty<int>(), Array.Empty<int>(), null);
        }

        /// <summary>Largueros of the classic grid with that family on, and how many the grid has (the «En X de Y celdas» of the card).</summary>
        public static (int On, int Total) CellsOn(SelectiveDraftEditor editor, string type)
        {
            var counts = LevelCounts(editor);
            var total = counts.Sum(c => Math.Max(0, c));
            var selection = Selected(editor, type);
            if (selection == null) return (0, total);

            var off = SelectiveSafetyGrid.OffCellKeys(OffCells(selection, type));
            var on = 0;
            for (var front = 0; front < counts.Count; front++)
            {
                for (var level = 0; level < counts[front]; level++)
                {
                    if (!off.Contains((front, level))) on++;
                }
            }

            return (on, total);
        }

        /// <summary>True when every larguero of the grid has that family off (the classic rule, which ignores duplicated or stale cells).</summary>
        public static bool AllOff(SelectiveDraftEditor editor, string type)
        {
            var selection = Selected(editor, type);
            return selection != null && SelectiveSafetyGrid.AllCellsOff(LevelCounts(editor), OffCells(selection, type));
        }

        // ------------------------------------------------------------------ the table by post

        /// <summary>The table by post of the last valid recomposition, counted as the BOM counts it.</summary>
        public static IReadOnlyList<SelectivePostSafetyRow> PostRows(SelectiveDraftEditor editor)
            => editor?.System == null ? Array.Empty<SelectivePostSafetyRow>() : SelectivePostSafetySummary.Build(editor.System, editor.Catalog);

        /// <summary>The side of the bota at a post: «Izquierda», «Derecha», both (provisional text), or empty when that post has none.</summary>
        public static string BootSide(SelectivePostSafetyRow row)
        {
            if (row == null || row.Boots <= 0) return NoSide;
            return row.BootNear && row.BootFar ? BothSides : row.BootNear ? Left : row.BootFar ? Right : NoSide;
        }

        // ------------------------------------------------------------------ texts of a write

        internal static string OnFloorText(int count)
            => count == 1
                ? "Una celda elegida no lleva larguero (la tarima está en el piso): ahí no hay tope ni parrilla."
                : count.ToString(CultureInfo.InvariantCulture) + " celdas elegidas no llevan larguero (la tarima está en el piso): ahí no hay tope ni parrilla.";

        internal static string AmbiguousText(IReadOnlyList<int> fronts)
            => "En " + Fronts(fronts) + " los fondos de la vista no tienen el mismo «Larguero a piso», así que esa fila es un larguero distinto en cada "
               + "fondo: elige un fondo en el selector para poner o quitar ahí.";

        internal static string ShiftedText(IReadOnlyList<int> fronts)
            => "El tope y la parrilla van con el larguero en todo el rack: en " + Fronts(fronts)
               + " otro fondo no tiene el mismo «Larguero a piso» y la marca se ve allí en otro nivel.";

        // ------------------------------------------------------------------ helpers

        private const string NoSystem = "El rack todavía no tiene una composición válida.";

        private static string ChooseFirst(string type) => "Elige primero la variante de " + Family(type) + " en «Editar seguridad…».";

        private static string Family(string type) => IsTope(type) ? "tope" : "parrilla";

        private static bool IsTope(string type) => string.Equals(type, SelectiveSafetyDefaults.TopeType, StringComparison.Ordinal);

        private static string Fronts(IReadOnlyList<int> fronts)
            => string.Join(", ", fronts.Select(front => "F" + (front + 1).ToString(CultureInfo.InvariantCulture)));

        private static IList<SelectiveGridCell> OffCells(SelectiveSafetySelection selection, string type)
            => IsTope(type) ? selection.TopeOffCells : selection.ParrillaOffCells;

        // The fondos of the scope that the resolved system has.
        private static IReadOnlyList<int> Fondos(SelectiveDraftEditor editor)
            => editor?.System == null
                ? Array.Empty<int>()
                : editor.ScopeFondos.Where(fondo => fondo >= 0 && fondo < SelectiveDepthLayout.Count(editor.System)).ToList();

        private static SelectiveBay Bay(SelectiveDraftEditor editor, int fondo, int front)
        {
            if (editor?.System == null || fondo < 0 || fondo >= SelectiveDepthLayout.Count(editor.System)) return null;
            var bays = SelectiveDepthLayout.BaysOfFondo(editor.System, fondo);
            return bays != null && front >= 0 && front < bays.Count ? bays[front] : null;
        }

        // The first pallet of the front rests on the floor, with no larguero under it (the rule the resolver and the desviador plan use).
        private static bool OnFloor(SelectiveBay bay) => bay.FloorPalletCount > 0;

        // The rows of the matrix that front has in its fondo: its largueros, plus the floor row when its first pallet rests on the floor.
        private static int MatrixLevels(SelectiveBay bay) => bay == null ? 0 : bay.Levels.Count + (OnFloor(bay) ? 1 : 0);

        // The fondos of the scope (or of the whole rack) that have the front disagree on «Larguero a piso».
        private static bool FloorDiffers(SelectiveDraftEditor editor, IEnumerable<int> fondos, int front)
            => fondos.Select(fondo => Bay(editor, fondo, front)).Where(bay => bay != null).Select(OnFloor).Distinct().Count() > 1;

        private static bool? Common(SelectiveScopedValue<bool> value) => value.HasValue ? value.Value : (bool?)null;

        private static SelectiveScopedValue<bool> Read(SelectiveDraftEditor editor, string type, IEnumerable<SelectiveMatrixPosition> positions)
        {
            var asked = (positions ?? Enumerable.Empty<SelectiveMatrixPosition>()).Distinct().ToList();
            var selection = Selected(editor, type);
            if (selection == null) return new SelectiveScopedValue<bool>(0, asked.Count, false, false);

            var off = SelectiveSafetyGrid.OffCellKeys(OffCells(selection, type));
            var values = new List<bool>();
            var missing = 0;
            foreach (var position in asked)
            {
                var read = false;
                foreach (var fondo in Fondos(editor))
                {
                    if (GridLevel(editor, fondo, position.FrontIndex, position.LevelIndex) is int grid)
                    {
                        values.Add(!off.Contains((position.FrontIndex, grid)));
                        read = true;
                    }
                }

                if (!read) missing++;
            }

            var varies = values.Distinct().Count() > 1;
            return new SelectiveScopedValue<bool>(values.Count, missing, varies, values.Count > 0 && !varies && values[0]);
        }

        private static SelectiveSafetyEdit Write(SelectiveDraftEditor editor, string type, IEnumerable<SelectiveMatrixPosition> positions, bool on)
        {
            if (editor?.System == null) return SelectiveSafetyEdit.Refuse(NoSystem);

            var safety = editor.CopySafety().ToList();
            var selection = SelectiveSafetyFamilies.SelectedOfType(safety, editor.Catalog?.SafetyElements, type);
            if (selection == null) return SelectiveSafetyEdit.Refuse(ChooseFirst(type));

            var scope = Fondos(editor);
            var all = Enumerable.Range(0, SelectiveDepthLayout.Count(editor.System)).ToList();
            var written = 0;
            var onFloor = 0;
            var missing = 0;
            var ambiguous = new SortedSet<int>();
            var ambiguousCells = 0;
            var shifted = new SortedSet<int>();
            var off = OffCells(selection, type);
            foreach (var position in (positions ?? Enumerable.Empty<SelectiveMatrixPosition>()).Distinct())
            {
                var front = position.FrontIndex;
                var holders = scope.Where(fondo => position.LevelIndex >= 0 && position.LevelIndex < MatrixLevels(Bay(editor, fondo, front))).ToList();
                if (holders.Count == 0)
                {
                    missing++;
                    continue;
                }

                if (FloorDiffers(editor, holders, front))
                {
                    ambiguous.Add(front);
                    ambiguousCells++;
                    continue;
                }

                var grids = holders.Select(fondo => GridLevel(editor, fondo, front, position.LevelIndex)).Where(g => g.HasValue).Select(g => g.Value).Distinct().ToList();
                if (grids.Count == 0)
                {
                    onFloor++; // decision 4: no tope nor parrilla where there is no larguero (N1 without «Larguero a piso»)
                    continue;
                }

                var grid = grids[0];
                var existing = off.Where(c => c != null && c.Frente == front && c.Level == grid).ToList();
                if (on)
                {
                    foreach (var cell in existing) off.Remove(cell);
                }
                else if (existing.Count == 0)
                {
                    off.Add(new SelectiveGridCell { Frente = front, Level = grid });
                }

                if (FloorDiffers(editor, all, front)) shifted.Add(front);
                written++;
            }

            if (written == 0)
            {
                var reasons = new List<string>();
                if (onFloor > 0) reasons.Add(OnFloorText(onFloor));
                if (ambiguousCells > 0) reasons.Add(AmbiguousText(ambiguous.ToList()));
                if (missing > 0 || reasons.Count == 0) reasons.Add(missing > 0 ? "Lo elegido no existe en los fondos de la vista." : "No hay celdas elegidas.");
                return SelectiveSafetyEdit.Refuse("No se escribió nada. " + string.Join(" ", reasons));
            }

            editor.ReplaceSafety(safety);
            return new SelectiveSafetyEdit(written, onFloor, ambiguousCells, missing, ambiguous.ToList(), shifted.ToList(), null);
        }
    }
}
