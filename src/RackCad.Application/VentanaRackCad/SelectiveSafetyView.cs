using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>One post of the read-only table of «Extremos y seguridad»: the levels of the desviador and the side of the bota.</summary>
    public sealed record SelectivePostSafetyRow(int PostIndex, IReadOnlyList<int> DeflectorLevels, string BootSide)
    {
        /// <summary>How many desviadores the drawing places at this post (the BOM tally of the post).</summary>
        public int Deflectors => DeflectorLevels.Count;
    }

    /// <summary>
    /// «Extremos y seguridad» of a Selectivo in the RackCad window (entrega 2, decisions 5, 6 and 13 of the Owner), read from and written
    /// to the safety of the draft (<see cref="SelectiveDraftEditor.CopySafety"/> / <see cref="SelectiveDraftEditor.ReplaceSafety"/>).
    /// T (tope) and P (parrilla) are per front × level for the WHOLE rack (not per fondo), on the classic grid of resolved BEAM levels
    /// (<see cref="SelectiveSafetyGrid.LevelCounts"/>): a floor level without «Larguero a piso» is not a cell, so it never has a tope or a
    /// parrilla (decision 4). The table by post counts with the same plan the BOM uses for the desviador
    /// (<see cref="SelectiveDesviadorPlan"/>) and reads the side of the bota from its one resolution of faces
    /// (<see cref="SelectiveSafetySelection.BootFacesAt"/>). Variant and measures stay in «Editar seguridad…» (the classic dialog).
    /// </summary>
    public static class SelectiveSafetyView
    {
        public const string Left = "Izquierda";
        public const string Right = "Derecha";
        public const string BothSides = "Izquierda y Derecha";
        public const string NoSide = "";

        /// <summary>Resolved beam levels per front of the whole rack (the maximum over the fondos); empty before a valid recomposition.</summary>
        public static IReadOnlyList<int> LevelCounts(SelectiveDraftEditor editor)
            => editor?.System == null ? Array.Empty<int>() : SelectiveSafetyGrid.LevelCounts(editor.System);

        /// <summary>
        /// The level of the classic grid of a level of the MATRIX of the window: the same when the front has «Larguero a piso»; one less
        /// when its first pallet rests on the floor, whose N1 is then no cell (null). The front is read on the master fondo (fondo 1).
        /// </summary>
        public static int? GridLevel(SelectiveDraftEditor editor, int front, int matrixLevel)
        {
            var floorBeam = editor?.Front(0, front)?.FloorBeam ?? true;
            var level = floorBeam ? matrixLevel : matrixLevel - 1;
            return level >= 0 ? level : (int?)null;
        }

        /// <summary>True when the cell of the matrix is on the grid: the front exists and the level carries a larguero.</summary>
        public static bool IsCell(SelectiveDraftEditor editor, int front, int matrixLevel)
        {
            var counts = LevelCounts(editor);
            var level = GridLevel(editor, front, matrixLevel);
            return front >= 0 && front < counts.Count && level is int grid && grid < counts[front];
        }

        /// <summary>The selection of a grid family (<see cref="SelectiveSafetyDefaults.TopeType"/> or <see cref="SelectiveSafetyDefaults.ParrillaType"/>) in the draft; null when none is chosen.</summary>
        public static SelectiveSafetySelection Selected(SelectiveDraftEditor editor, string type)
            => editor == null ? null : SelectiveSafetyFamilies.SelectedOfType(editor.CopySafety(), editor.Catalog?.SafetyElements, type);

        /// <summary>T at a cell: null when no tope is chosen or the cell is not on the grid.</summary>
        public static bool? Tope(SelectiveDraftEditor editor, int front, int level)
            => At(editor, SelectiveSafetyDefaults.TopeType, front, level);

        /// <summary>P at a cell: null when no parrilla is chosen or the cell is not on the grid.</summary>
        public static bool? Parrilla(SelectiveDraftEditor editor, int front, int level)
            => At(editor, SelectiveSafetyDefaults.ParrillaType, front, level);

        /// <summary>T at the selected cells: varies when they differ; cells off the grid do not count.</summary>
        public static SelectiveScopedValue<bool> ReadTope(SelectiveDraftEditor editor, IEnumerable<SelectiveMatrixPosition> positions)
            => Read(editor, SelectiveSafetyDefaults.TopeType, positions);

        /// <summary>P at the selected cells: varies when they differ; cells off the grid do not count.</summary>
        public static SelectiveScopedValue<bool> ReadParrilla(SelectiveDraftEditor editor, IEnumerable<SelectiveMatrixPosition> positions)
            => Read(editor, SelectiveSafetyDefaults.ParrillaType, positions);

        /// <summary>Turns T on or off at the selected cells, for the whole rack. Refuses when no tope is chosen.</summary>
        public static SelectiveDraftEdit WriteTope(SelectiveDraftEditor editor, IEnumerable<SelectiveMatrixPosition> positions, bool on)
            => Write(editor, SelectiveSafetyDefaults.TopeType, positions, on);

        /// <summary>Turns P on or off at the selected cells, for the whole rack. Refuses when no parrilla is chosen.</summary>
        public static SelectiveDraftEdit WriteParrilla(SelectiveDraftEditor editor, IEnumerable<SelectiveMatrixPosition> positions, bool on)
            => Write(editor, SelectiveSafetyDefaults.ParrillaType, positions, on);

        /// <summary>
        /// The table by post: the desviador levels the drawing places at every post (one per enabled spot of <see cref="SelectiveDesviadorPlan"/>,
        /// the same spots the BOM counts) and the side of the bota (Izquierda = the near face, the aisle of entrance/exit; Derecha = the far one).
        /// </summary>
        public static IReadOnlyList<SelectivePostSafetyRow> PostRows(SelectiveDraftEditor editor)
        {
            var system = editor?.System;
            if (system == null || system.Bays.Count == 0)
            {
                return Array.Empty<SelectivePostSafetyRow>();
            }

            var safety = editor.CopySafety();
            var desviador = SelectiveSafetyFamilies.SelectedOfType(safety, editor.Catalog?.SafetyElements, SelectiveSafetyDefaults.DesviadorType);
            var bota = SelectiveSafetyFamilies.SelectedOfType(safety, editor.Catalog?.SafetyElements, SelectiveSafetyDefaults.BotaType);
            var spots = desviador == null
                ? Array.Empty<SelectiveDesviadorPlan.Spot>()
                : SelectiveDesviadorPlan.Build(system, editor.Catalog, desviador).Spots.Where(s => s.Enabled).ToArray();

            var postCount = Math.Max(system.Bays.Count + 1, spots.Select(s => s.PostIndex + 1).DefaultIfEmpty(0).Max());
            var rows = new List<SelectivePostSafetyRow>(postCount);
            for (var post = 0; post < postCount; post++)
            {
                var levels = spots.Where(s => s.PostIndex == post).Select(s => s.Level).OrderBy(l => l).ToList();
                rows.Add(new SelectivePostSafetyRow(post, levels, BootSide(bota, post)));
            }

            return rows;
        }

        /// <summary>The side of the bota at a post: «Izquierda», «Derecha», both, or empty when that post has none.</summary>
        public static string BootSide(SelectiveSafetySelection bota, int post)
        {
            if (bota == null)
            {
                return NoSide;
            }

            var faces = bota.BootFacesAt(post);
            return faces.Near && faces.Far ? BothSides : faces.Near ? Left : faces.Far ? Right : NoSide;
        }

        // ------------------------------------------------------------------ helpers

        private static IList<SelectiveGridCell> OffCells(SelectiveSafetySelection selection, string type)
            => string.Equals(type, SelectiveSafetyDefaults.TopeType, StringComparison.Ordinal) ? selection.TopeOffCells : selection.ParrillaOffCells;

        private static bool? At(SelectiveDraftEditor editor, string type, int front, int level)
        {
            if (!IsCell(editor, front, level))
            {
                return null;
            }

            var selection = Selected(editor, type);
            if (selection == null)
            {
                return null;
            }

            return !SelectiveSafetyGrid.OffCellKeys(OffCells(selection, type)).Contains((front, GridLevel(editor, front, level).Value));
        }

        private static SelectiveScopedValue<bool> Read(SelectiveDraftEditor editor, string type, IEnumerable<SelectiveMatrixPosition> positions)
        {
            var values = (positions ?? Enumerable.Empty<SelectiveMatrixPosition>())
                .Select(p => At(editor, type, p.FrontIndex, p.LevelIndex))
                .ToList();
            var present = values.Where(v => v.HasValue).Select(v => v.Value).ToList();
            var varies = present.Distinct().Count() > 1;
            return new SelectiveScopedValue<bool>(present.Count, values.Count - present.Count, varies, present.Count > 0 && !varies && present[0]);
        }

        private static SelectiveDraftEdit Write(SelectiveDraftEditor editor, string type, IEnumerable<SelectiveMatrixPosition> positions, bool on)
        {
            if (editor?.System == null)
            {
                return SelectiveDraftEdit.Refuse("El rack todavía no tiene una composición válida.");
            }

            var family = string.Equals(type, SelectiveSafetyDefaults.TopeType, StringComparison.Ordinal) ? "tope" : "parrilla";
            var safety = editor.CopySafety().ToList();
            var selection = SelectiveSafetyFamilies.SelectedOfType(safety, editor.Catalog?.SafetyElements, type);
            if (selection == null)
            {
                return SelectiveDraftEdit.Refuse($"Elige primero la variante de {family} en «Editar seguridad…».");
            }

            var written = 0;
            var omitted = 0;
            var off = OffCells(selection, type);
            foreach (var position in (positions ?? Enumerable.Empty<SelectiveMatrixPosition>()).Distinct())
            {
                if (!IsCell(editor, position.FrontIndex, position.LevelIndex))
                {
                    omitted++; // decision 4: no tope nor parrilla where there is no larguero (N1 without «Larguero a piso»)
                    continue;
                }

                var grid = GridLevel(editor, position.FrontIndex, position.LevelIndex).Value;
                var existing = off.Where(c => c != null && c.Frente == position.FrontIndex && c.Level == grid).ToList();
                if (on)
                {
                    foreach (var cell in existing) off.Remove(cell);
                }
                else if (existing.Count == 0)
                {
                    off.Add(new SelectiveGridCell { Frente = position.FrontIndex, Level = grid });
                }

                written++;
            }

            if (written == 0)
            {
                return omitted > 0
                    ? SelectiveDraftEdit.Refuse($"Ninguna celda elegida lleva larguero: ahí no hay {family}.")
                    : SelectiveDraftEdit.Refuse("No hay celdas elegidas.");
            }

            editor.ReplaceSafety(safety);
            return new SelectiveDraftEdit(written, omitted, null);
        }
    }
}
