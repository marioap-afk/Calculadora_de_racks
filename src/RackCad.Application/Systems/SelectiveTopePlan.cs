using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The physical larguero-TOPE topology of a resolved run (I-22, E6): resolved ONCE, then projected/counted by the
    /// LATERAL, PLANTA and BOM. A tope is a rear pallet stop at a fondo's back post; the pieces live per TopeSpot
    /// (shared central, or the central pair per fondo) × bay (frente) × grid-on level × loaded medio-frente tramo.
    /// <list type="bullet">
    /// <item>the LATERAL groups by spot+corte and draws one per DISTINCT larguero Y (rise-and-snapped) at the back post;</item>
    /// <item>the PLANTA collapses a bay/tramo's levels to one top-view line;</item>
    /// <item>the BOM counts one piece per grid-on level per loaded tramo, grouped by length.</item>
    /// </list>
    /// The FRONTAL is NOT one of these: it is an opt-in per-frente schematic (gated by <c>TopeFrontal</c>) that draws a
    /// tope at every fondo-0 frente/level regardless of the physical spots, so it is not a projection of this plan and
    /// keeps its own traversal. Uses the MASTER troquel grid for the tramos (as the BOM and planta do). Pure — no AutoCAD.
    /// </summary>
    public static class SelectiveTopePlan
    {
        /// <summary>One drawable slice of a bay: a loaded medio-frente tramo (or the whole bay), by its start offset
        /// within the bay and the tope LONGITUD (larguero/tramo length + the allowance).</summary>
        public readonly struct Segment
        {
            public Segment(double startOffset, double longitud)
            {
                StartOffset = startOffset;
                Longitud = longitud;
            }

            /// <summary>Offset of this tramo's left post within the bay (0 for a full bay).</summary>
            public double StartOffset { get; }

            /// <summary>The tope LONGITUD = the larguero/tramo length + the allowance.</summary>
            public double Longitud { get; }
        }

        /// <summary>The topes of one bay of a spot: the grid-on levels' resolved Ys (their count is the BOM tally per
        /// tramo; their distinct values feed the lateral) and the loaded tramos (or one full-bay segment).</summary>
        public sealed class Cell
        {
            public Cell(int frente, IReadOnlyList<double> largueroYs, IReadOnlyList<Segment> segments)
            {
                Frente = frente;
                LargueroYs = largueroYs;
                Segments = segments;
            }

            /// <summary>Bay index within the spot's fondo.</summary>
            public int Frente { get; }

            /// <summary>Resolved Y of every grid-on level of this bay (off-cells removed). Never empty.</summary>
            public IReadOnlyList<double> LargueroYs { get; }

            /// <summary>The loaded tramos (or one full-bay segment) this bay draws a tope on.</summary>
            public IReadOnlyList<Segment> Segments { get; }
        }

        /// <summary>The topes of one TopeSpot: which fondo carries it, whether it sits at that fondo's FRONT post, and
        /// whether the block mirrors — plus its bays.</summary>
        public sealed class SpotTopes
        {
            public SpotTopes(int fondo, bool atFront, bool mirror, IReadOnlyList<Cell> cells)
            {
                Fondo = fondo;
                AtFront = atFront;
                Mirror = mirror;
                Cells = cells;
            }

            public int Fondo { get; }

            public bool AtFront { get; }

            public bool Mirror { get; }

            public IReadOnlyList<Cell> Cells { get; }
        }

        /// <summary>Resolves the physical topes once. Empty when no tope family is selected (the views additionally gate
        /// on the per-view drawable check + block via <c>EnabledOfType</c>).</summary>
        public static IReadOnlyList<SpotTopes> Build(SelectiveRackSystem system, RackCatalog catalog)
        {
            var result = new List<SpotTopes>();
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system?.SafetySelections, catalog?.SafetyElements, SelectiveSafetyDefaults.TopeType);
            if (system == null || catalog == null || selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
            {
                return result;
            }

            var fondoCount = SelectiveDepthLayout.Count(system);
            var troquelXs = SelectiveDepthLayout.MasterGrid(system, catalog).TroquelXs;
            var offCells = SelectiveSafetyGrid.OffCellKeys(selection.TopeOffCells);

            foreach (var spot in SelectiveSafetyPlacement.TopeSpots(selection, fondoCount))
            {
                if (spot.Fondo < 0 || spot.Fondo >= fondoCount)
                {
                    continue;
                }

                var bays = SelectiveDepthLayout.BaysOfFondo(system, spot.Fondo);
                var cells = new List<Cell>();
                for (var b = 0; b < bays.Count; b++)
                {
                    var bay = bays[b];
                    if (bay.BeamLength <= 0.0 || bay.Levels.Count == 0)
                    {
                        continue;
                    }

                    var largueroYs = new List<double>();
                    for (var lvl = 0; lvl < bay.Levels.Count; lvl++)
                    {
                        if (!offCells.Contains((b, lvl)))
                        {
                            largueroYs.Add(bay.Levels[lvl].Y);
                        }
                    }

                    if (largueroYs.Count == 0)
                    {
                        continue; // no grid-on level → no tope on this bay
                    }

                    var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, SelectiveRackDefaults.View);
                    var troquelX = b < troquelXs.Count ? troquelXs[b] : 0.0;
                    var tramos = SelectiveMedioFrente.Resolve(bay, troquelX, inicioX);
                    var segments = new List<Segment>();
                    if (tramos == null)
                    {
                        segments.Add(new Segment(0.0, bay.BeamLength + SelectiveSafetyPlacement.TopeLengthAllowance));
                    }
                    else
                    {
                        foreach (var tramo in tramos)
                        {
                            if (tramo.Loaded)
                            {
                                segments.Add(new Segment(tramo.StartOffset, tramo.Length + SelectiveSafetyPlacement.TopeLengthAllowance));
                            }
                        }
                    }

                    cells.Add(new Cell(b, largueroYs, segments));
                }

                result.Add(new SpotTopes(spot.Fondo, spot.AtFront, spot.Mirror, cells));
            }

            return result;
        }
    }
}
