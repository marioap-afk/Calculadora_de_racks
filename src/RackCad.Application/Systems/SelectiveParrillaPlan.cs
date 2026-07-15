using System;
using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The parrilla PLAN of a resolved run: for each (frente, nivel) grid cell, the LOAD ROWS that carry decks — a full
    /// bay is one row, a medio frente one row per loaded tramo, and every fondo contributes its own. The geometry here
    /// does not depend on the manual frente/cantidad, so the editor resolves it ONCE and then recounts on each keystroke
    /// through <see cref="SelectiveFrontalBuilder.ParrillaRow"/> — the same rule the frontal draw and the BOM use, so the
    /// number shown in the dialog is the number drawn and the number quoted. Pure: no AutoCAD.
    /// </summary>
    public static class SelectiveParrillaPlan
    {
        /// <summary>One load row that can carry decks: its clear span and the tarima it holds.</summary>
        public readonly struct LoadRow
        {
            public LoadRow(double span, double palletFrente, int palletCount, bool fitToSpan)
            {
                Span = span;
                PalletFrente = palletFrente;
                PalletCount = palletCount;
                FitToSpan = fitToSpan;
            }

            /// <summary>Clear span (in) the decks are distributed over: the bay's larguero length, or a tramo's.</summary>
            public double Span { get; }

            public double PalletFrente { get; }

            public int PalletCount { get; }

            /// <summary>True for a medio-frente tramo (the count is derived from the span, as the tarimas are).</summary>
            public bool FitToSpan { get; }
        }

        /// <summary>Every load row at one (frente, nivel) grid cell, across all fondos.</summary>
        public sealed class Cell
        {
            public Cell(int frente, int level)
            {
                Frente = frente;
                Level = level;
            }

            public int Frente { get; }

            public int Level { get; }

            public IList<LoadRow> Rows { get; } = new List<LoadRow>();
        }

        /// <summary>
        /// The grid cells of <paramref name="system"/> with their load rows, keyed by (frente, nivel). Resolves the
        /// medio-frente tramos on the MASTER troquel grid, as the BOM does (every fondo's posts are a prefix of it).
        /// </summary>
        public static IReadOnlyList<Cell> Cells(SelectiveRackSystem system, RackCatalog catalog)
        {
            var cells = new List<Cell>();
            if (system == null)
            {
                return cells;
            }

            var byKey = new Dictionary<(int, int), Cell>();
            var troquelXs = SelectiveDepthLayout.MasterGrid(system, catalog).TroquelXs;

            for (var k = 0; k < SelectiveDepthLayout.Count(system); k++)
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(system, k);
                if (bays == null)
                {
                    continue;
                }

                for (var i = 0; i < bays.Count; i++)
                {
                    var bay = bays[i];
                    if (bay == null || bay.BeamLength <= 0.0)
                    {
                        continue;
                    }

                    var troquelX = i < troquelXs.Count ? troquelXs[i] : 0.0;
                    var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, SelectiveRackDefaults.View);
                    var tramos = SelectiveMedioFrente.Resolve(bay, troquelX, inicioX);

                    for (var lvl = 0; lvl < bay.Levels.Count; lvl++)
                    {
                        var level = bay.Levels[lvl];
                        if (!byKey.TryGetValue((i, lvl), out var cell))
                        {
                            cell = new Cell(i, lvl);
                            byKey[(i, lvl)] = cell;
                            cells.Add(cell);
                        }

                        if (tramos == null)
                        {
                            cell.Rows.Add(new LoadRow(bay.BeamLength, level.PalletFrente, level.PalletCount, false));
                            continue;
                        }

                        foreach (var tramo in tramos)
                        {
                            if (tramo.Loaded)
                            {
                                cell.Rows.Add(new LoadRow(tramo.Length, level.PalletFrente, level.PalletCount, true));
                            }
                        }
                    }
                }
            }

            return cells;
        }

        /// <summary>Decks at <paramref name="cell"/> for a manual width/count (0 = automatic): the sum over its load rows.</summary>
        public static int CountIn(Cell cell, double overrideFrente, int overrideCount)
        {
            if (cell == null)
            {
                return 0;
            }

            var total = 0;
            foreach (var row in cell.Rows)
            {
                total += SelectiveFrontalBuilder.ParrillaRow(row.Span, row.PalletFrente, row.PalletCount, overrideFrente, overrideCount, row.FitToSpan).count;
            }

            return total;
        }

        /// <summary>
        /// The largest cantidad forceable at <paramref name="cell"/>: the TIGHTEST fit among the rows that can hold at
        /// least one deck, at the width the manual <paramref name="overrideFrente"/> (or each row's tarima) gives.
        /// A row that holds NO deck is skipped rather than dragging the answer to 0 — a tramo narrower than its tarima
        /// holds no tarima either, so it is inherently empty and lowering the cantidad cannot change that; it must not
        /// veto a cantidad the other rows can take. 0 only when NO row holds a deck, i.e. the cell draws nothing
        /// whatever is typed — ask <see cref="CountIn"/>, not this, for "does this cell draw anything".
        /// </summary>
        public static int MaxCountIn(Cell cell, double overrideFrente)
        {
            if (cell == null)
            {
                return 0;
            }

            var max = 0; // 0 = still unset; every counted row contributes at least 1
            foreach (var row in cell.Rows)
            {
                var frente = overrideFrente > 0.0 ? overrideFrente : row.PalletFrente;
                var fit = SelectiveFrontalBuilder.PalletFit(row.Span, frente);
                if (fit <= 0)
                {
                    continue; // inherently empty row: no tarima fits it either, so no cantidad can fill it
                }

                max = max == 0 ? fit : Math.Min(max, fit);
            }

            return max;
        }
    }
}
