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
    /// The FRONTAL is NOT a projection of <see cref="SpotTopes"/>: it is an opt-in per-frente schematic (gated by
    /// <c>TopeFrontal</c>) that draws a tope at every grid-on frente/level of ONE fondo regardless of the physical
    /// spots — so projecting the spots would double it on a per-fondo pair. It therefore has its OWN result in this
    /// same family service, <see cref="BuildFrontal"/> (<see cref="FrontalCell"/>/<see cref="FrontalTope"/>), which
    /// resolves the frontal's pure intent (off-cells, levels, loaded tramos, offsets, longitudes and each tope's
    /// source larguero Y); the builder keeps only the catalogued troquel point, the rise-and-snap and the placement.
    /// <see cref="Build"/> uses the MASTER troquel grid for the tramos (as the BOM and planta do); <see cref="BuildFrontal"/>
    /// uses the drawn fondo's own grid (as the frontal builder does). Pure — no AutoCAD.
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

        /// <summary>One schematic FRONTAL tope of a bay: the grid level it stops, its tramo start offset within the bay
        /// (0 for a full bay), the tope LONGITUD (larguero/tramo length + the allowance) and the SOURCE larguero Y the
        /// snap rises from. The builder turns (StartOffset, SourceY) into the placed (X, Y) with the catalogued troquel
        /// point and <see cref="SelectiveTopePlacement.SnapY"/>.</summary>
        public readonly struct FrontalTope
        {
            public FrontalTope(int level, double startOffset, double longitud, double sourceY)
            {
                Level = level;
                StartOffset = startOffset;
                Longitud = longitud;
                SourceY = sourceY;
            }

            /// <summary>Grid level index this tope stops (off-cell levels are already removed).</summary>
            public int Level { get; }

            /// <summary>Offset of this tope's left post within the bay (0 for a full bay; a tramo's start otherwise).</summary>
            public double StartOffset { get; }

            /// <summary>The tope LONGITUD = the larguero/tramo length + <see cref="SelectiveSafetyPlacement.TopeLengthAllowance"/>.</summary>
            public double Longitud { get; }

            /// <summary>The larguero Y this tope rises ~<see cref="SelectiveTopePlacement.YOffset"/>" above before snapping.</summary>
            public double SourceY { get; }
        }

        /// <summary>The frontal topes of one bay (frente): its bay index and the topes to draw, in draw order
        /// (level-major, then loaded tramo within the level). Only bays that draw at least one tope appear.</summary>
        public sealed class FrontalCell
        {
            public FrontalCell(int frente, IReadOnlyList<FrontalTope> topes)
            {
                Frente = frente;
                Topes = topes;
            }

            /// <summary>Bay index within the drawn fondo; its post peralte drives the troquel the builder resolves.</summary>
            public int Frente { get; }

            public IReadOnlyList<FrontalTope> Topes { get; }
        }

        /// <summary>
        /// The FRONTAL topes of a resolved run's ONE fondo (<paramref name="system"/> is that fondo's view, as the
        /// frontal builder passes): per bay with a larguero, per grid-on level (off-cells removed), per LOADED medio-
        /// frente tramo (or the whole bay). Resolves the pure per-frente intent — active cells, levels, loaded tramos,
        /// start offsets, longitudes (+ allowance) and each tope's source larguero Y. It does NOT consult
        /// <see cref="SpotTopes"/>/<c>TopeShared</c>: the frontal is per-frente, so it never multiplies by the physical
        /// spots (that would double a per-fondo pair). Empty when no tope family is selected; the builder additionally
        /// gates on the <c>TopeFrontal</c> toggle and the drawable block, resolves the catalogued troquel point and
        /// applies the rise-and-snap. Uses the drawn fondo's own troquel grid — the SAME
        /// <see cref="SelectivePostGeometry.Compute"/> the builder computes for its post Xs — so the tramos are resolved
        /// identically. Pure — no AutoCAD.
        /// </summary>
        public static IReadOnlyList<FrontalCell> BuildFrontal(SelectiveRackSystem system, RackCatalog catalog)
        {
            var cells = new List<FrontalCell>();
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system?.SafetySelections, catalog?.SafetyElements, SelectiveSafetyDefaults.TopeType);
            if (system == null || catalog == null || selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
            {
                return cells;
            }

            var view = SelectiveRackDefaults.View;
            var troquelXs = SelectivePostGeometry.Compute(system, catalog).TroquelXs;
            var offCells = SelectiveSafetyGrid.OffCellKeys(selection.TopeOffCells);

            for (var i = 0; i < system.Bays.Count; i++)
            {
                var bay = system.Bays[i];
                if (bay.BeamLength <= 0.0)
                {
                    continue;
                }

                var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, view);
                var troquelX = i < troquelXs.Count ? troquelXs[i] : 0.0;
                var tramos = SelectiveMedioFrente.Resolve(bay, troquelX, inicioX);

                var topes = new List<FrontalTope>();
                for (var lvl = 0; lvl < bay.Levels.Count; lvl++)
                {
                    if (offCells.Contains((i, lvl)))
                    {
                        continue; // this (frente, level) cell is off
                    }

                    var sourceY = bay.Levels[lvl].Y;
                    if (tramos == null)
                    {
                        topes.Add(new FrontalTope(lvl, 0.0, bay.BeamLength + SelectiveSafetyPlacement.TopeLengthAllowance, sourceY));
                        continue;
                    }

                    foreach (var tramo in tramos)
                    {
                        if (tramo.Loaded)
                        {
                            topes.Add(new FrontalTope(lvl, tramo.StartOffset, tramo.Length + SelectiveSafetyPlacement.TopeLengthAllowance, sourceY));
                        }
                    }
                }

                if (topes.Count > 0)
                {
                    cells.Add(new FrontalCell(i, topes));
                }
            }

            return cells;
        }
    }
}
