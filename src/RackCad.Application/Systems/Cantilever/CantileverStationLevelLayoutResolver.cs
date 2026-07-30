using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>One resolved level of the layout: its index, its punch index and one cell per active side.</summary>
    public sealed class CantileverStationLevelPlan
    {
        internal CantileverStationLevelPlan(
            int levelIndex,
            int lowerPunchIndex,
            IReadOnlyList<CantileverArmConnectionMetrics> cells,
            IReadOnlyDictionary<CantileverArmSide, double> clearBySide,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            LevelIndex = levelIndex;
            LowerPunchIndex = lowerPunchIndex;
            Cells = cells;
            ClearBySide = clearBySide;
            Diagnostics = diagnostics;
        }

        /// <summary>
        /// Position of this level in the design's list, base zero.
        ///
        /// The identity of a level in I-37C is POSITIONAL. There is deliberately no persisted level id: ids are
        /// for things that survive reordering, and nothing here reorders levels.
        /// </summary>
        public int LevelIndex { get; }

        /// <summary>
        /// The lowest column punch index this level bolts to. SHARED by every active side.
        ///
        /// One index and not one per side: in a double station both faces sit at the same elevation, and two
        /// fields for one number is how the two faces end up at different heights (ADR-0026, D1).
        /// </summary>
        public int LowerPunchIndex { get; }

        /// <summary>
        /// One entry per ACTIVE side, measured by the shared connection authority.
        ///
        /// They are <see cref="CantileverArmConnectionMetrics"/> — the very type I-37B resolves its own arms
        /// with — so the prediction and the resolve cannot come from two different formulas.
        /// </summary>
        public IReadOnlyList<CantileverArmConnectionMetrics> Cells { get; }

        /// <summary>
        /// The REAL clear below this level, per side: this level's body bottom minus the previous level's body
        /// top. Empty on level 0, which has nothing below it.
        /// </summary>
        public IReadOnlyDictionary<CantileverArmSide, double> ClearBySide { get; }

        /// <summary>
        /// The clear that GOVERNS: the smallest of the sides.
        ///
        /// The minimum and not the average, because a load has to fit on the tightest face. On level 0 it is
        /// <c>NaN</c> — there is no clear below the first level, and reporting zero would read as "they touch".
        /// </summary>
        public double GoverningClear =>
            ClearBySide.Count == 0 ? double.NaN : ClearBySide.Values.Min();

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        /// <summary>The highest punch index any side of this level uses.</summary>
        public int UpperPunchIndex => Cells.Max(c => c.UpperPunchIndex);

        /// <summary>The highest point any side of this level occupies at the connection plane.</summary>
        public double OccupiedTopZ => Math.Max(Cells.Max(c => c.BodyTopZ), Cells.Max(c => c.PlateTopZ));

        public CantileverArmConnectionMetrics Cell(CantileverArmSide side) =>
            Cells.FirstOrDefault(c => c.Side == side);

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "L{0}@{1}[{2}]clear={3}",
            LevelIndex,
            LowerPunchIndex,
            string.Join(",", Cells.Select(c => c.Signature())),
            ClearBySide.Count == 0
                ? "-"
                : string.Join(",", ClearBySide
                    .OrderBy(kv => kv.Key)
                    .Select(kv => kv.Key + "=" + kv.Value.ToString("0.######", CultureInfo.InvariantCulture))));

        public override string ToString() => "Level " + Signature();
    }

    /// <summary>The whole level layout, plus the column height it implies.</summary>
    public sealed class CantileverStationLevelLayout
    {
        internal CantileverStationLevelLayout(
            IReadOnlyList<CantileverStationLevelPlan> levels,
            double minimumColumnHeight,
            double requestedTopClear,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            Levels = levels;
            MinimumColumnHeight = minimumColumnHeight;
            RequestedTopClear = requestedTopClear;
            Diagnostics = diagnostics;
        }

        public IReadOnlyList<CantileverStationLevelPlan> Levels { get; }

        /// <summary>The shortest column that holds this layout with its top margin.</summary>
        public double MinimumColumnHeight { get; }

        /// <summary>The margin the factor asked for, in inches.</summary>
        public double RequestedTopClear { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        /// <summary>The highest punch index the whole layout uses.</summary>
        public int HighestUsedPunchIndex => Levels.Max(l => l.UpperPunchIndex);

        public string Signature() =>
            string.Join(";", Levels.Select(l => l.Signature())) +
            "|min=" + MinimumColumnHeight.ToString("0.######", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// THE authority that turns a station design into level indices, level metrics and the column height they
    /// imply — without knowing the column height.
    ///
    /// That is the point. The circularity is real —height decides how many punches exist, punches decide where
    /// levels go, levels decide the minimum height— and this type breaks it by working off
    /// <see cref="CantileverColumnRegularPunchGrid"/>, which needs only the connection pattern. No provisional
    /// height, no oversized column, no convergence loop (ADR-0026, D5).
    ///
    /// It measures NOTHING itself. Every elevation, plate edge and body edge comes from
    /// <see cref="CantileverArmConnectionMetricsResolver"/>, the same authority I-37B resolves its arms with.
    /// The earlier version computed them here with its own <c>Math.Max(2, count)</c> and <c>offset ?? 0</c>,
    /// which is how a design I-37B would reject produced a confident layout.
    ///
    /// The search has NO candidate cap. Because the grid is strictly increasing, the first index that satisfies
    /// a lower bound is found DIRECTLY: each rule becomes "the elevation must reach Z", the grid answers with
    /// the first index that does, and the answer is the largest of those per-side minima. Monotonicity is what
    /// makes that the first feasible index rather than merely a feasible one — so a level whose home is ten
    /// thousand indices up is found in constant time instead of being rejected by an arbitrary limit.
    /// </summary>
    public static class CantileverStationLevelLayoutResolver
    {
        private const double FitTolerance = 1e-9;

        /// <summary>
        /// Resolves the layout.
        /// </summary>
        /// <param name="design">The station intent.</param>
        /// <param name="grid">The one regular-punch authority, from the connection pattern.</param>
        /// <param name="sectionHeights">
        /// Combined section depth per cell, already resolved from I-36 through the arrangement authority. It is
        /// passed IN rather than resolved here so this type stays free of the catalogue and of the section
        /// policy — the caller has already done that lookup for its own reasons.
        /// </param>
        /// <param name="orientations">The registered body orientation per cell, for the same reason.</param>
        public static CantileverStationLevelLayout Resolve(
            CantileverStationDesign design,
            CantileverColumnRegularPunchGrid grid,
            Func<int, CantileverArmSide, double> sectionHeights,
            Func<int, CantileverArmSide, CantileverArmBodyOrientation> orientations)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            if (sectionHeights == null)
            {
                throw new ArgumentNullException(nameof(sectionHeights));
            }

            if (orientations == null)
            {
                throw new ArgumentNullException(nameof(orientations));
            }

            var diagnostics = new List<CantileverDiagnostic>();
            var levels = new List<CantileverStationLevelPlan>();

            if (design.LevelCount == 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationNoLevels,
                    "Una estacion necesita al menos un nivel."));
                return new CantileverStationLevelLayout(levels, 0.0, 0.0, diagnostics);
            }

            // A grid that does not rise makes every later question meaningless, so it is answered FIRST rather
            // than discovered inside a search that would never terminate.
            if (!grid.IsIncreasing)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationGridNotIncreasing,
                    "La reticula de troqueles no crece: su pitch debe ser finito y positivo, y se recibio " +
                    Format(grid.Pitch) + "."));
                return new CantileverStationLevelLayout(levels, 0.0, 0.0, diagnostics);
            }

            IReadOnlyList<CantileverArmSide> sides;

            if (!design.TryActiveSides(out sides))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationFaceModeNotSupported,
                    "El modo de cara '" + design.FaceMode + "' no tiene regla de lados activos."));
                return new CantileverStationLevelLayout(levels, 0.0, 0.0, diagnostics);
            }

            var topClear = ValidateTopClearFactor(design, diagnostics);

            for (var levelIndex = 0; levelIndex < design.LevelCount; levelIndex++)
            {
                var previous = levels.Count == 0 ? null : levels[levels.Count - 1];

                var plan = ResolveLevel(
                    design, grid, sectionHeights, orientations, sides, levelIndex, previous, diagnostics);

                if (plan == null)
                {
                    return new CantileverStationLevelLayout(levels, 0.0, topClear, diagnostics);
                }

                levels.Add(plan);
            }

            var minimum = MinimumColumnHeight(levels, grid, design, topClear, diagnostics);

            if (minimum == null)
            {
                return new CantileverStationLevelLayout(levels, 0.0, topClear, diagnostics);
            }

            return new CantileverStationLevelLayout(levels, minimum.Value, topClear, diagnostics);
        }

        private static double ValidateTopClearFactor(
            CantileverStationDesign design, ICollection<CantileverDiagnostic> diagnostics)
        {
            var factor = design.TopClearFactor;

            // The floor is one third and exact equality passes: the default IS the floor, so rejecting it
            // would reject every unedited design.
            if (!GeometryTolerance.IsFinite(factor) ||
                factor + FitTolerance < CantileverStationDefaults.TopClearFactor)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationTopClearFactorTooSmall,
                    "El factor de margen superior debe ser finito y al menos " +
                    Format(CantileverStationDefaults.TopClearFactor) + "; se recibio " + Format(factor) + "."));
                return 0.0;
            }

            return design.RequestedClearHeight * factor;
        }

        private static CantileverStationLevelPlan ResolveLevel(
            CantileverStationDesign design,
            CantileverColumnRegularPunchGrid grid,
            Func<int, CantileverArmSide, double> sectionHeights,
            Func<int, CantileverArmSide, CantileverArmBodyOrientation> orientations,
            IReadOnlyList<CantileverArmSide> sides,
            int levelIndex,
            CantileverStationLevelPlan previous,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            // Level 0 uses the requested index EXACTLY. It is the one index the user chose, so it is not a
            // candidate to be searched from — searching would let the station move the level the user placed.
            if (previous == null)
            {
                var cells = Measure(
                    design, grid, sectionHeights, orientations, sides, levelIndex,
                    design.FirstLevelPunchIndex, diagnostics);

                return cells == null
                    ? null
                    : new CantileverStationLevelPlan(
                        levelIndex, design.FirstLevelPunchIndex, cells,
                        new Dictionary<CantileverArmSide, double>(),
                        Array.Empty<CantileverDiagnostic>());
            }

            // ---- the first feasible index, found rather than searched for ---------------------------------
            //
            // Three rules, each a lower bound on this level's index:
            //
            //   punches   the range must be disjoint from the previous level's, in EVERY active side
            //   clear     body bottom - previous body top >= requested, per side
            //   plates    plate bottom >= previous plate top, per side
            //
            // The last two are "the elevation must reach Z", and the grid answers those directly. Each is
            // monotone in the index, so the largest of the per-side minima IS the first index that satisfies
            // all of them — which is why no candidate loop and no cap are needed.
            var floor = (long)previous.UpperPunchIndex + 1;

            if (floor > CantileverColumnRegularPunchGrid.MaxDefinedIndex)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationPunchIndexDomainOverflow,
                    "El nivel " + (levelIndex + 1) + " tendria que empezar en el indice " + floor +
                    ", fuera del dominio de la reticula."));
                return null;
            }

            var candidate = floor;

            foreach (var side in sides)
            {
                var below = previous.Cell(side);
                var offset = OffsetOf(design, levelIndex, side);

                if (offset == null)
                {
                    // A missing margin is the shared authority's business, and Measure below will report it.
                    // Skipping it here keeps ONE place that decides what an absent margin means.
                    continue;
                }

                foreach (var target in new[]
                         {
                             below.BodyTopZ + design.RequestedClearHeight + offset.Value,
                             below.PlateTopZ + offset.Value
                         })
                {
                    if (!grid.TryFirstIndexAtOrAbove(target, out var needed))
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.StationLevelDoesNotFit,
                            "El nivel " + (levelIndex + 1) + " en el lado " + side +
                            " necesitaria un troquel en z = " + Format(target) +
                            " in, fuera del dominio de la reticula. Reduce el claro o cambia el brazo."));
                        return null;
                    }

                    candidate = Math.Max(candidate, needed);
                }
            }

            if (candidate > CantileverColumnRegularPunchGrid.MaxDefinedIndex)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationPunchIndexDomainOverflow,
                    "El nivel " + (levelIndex + 1) + " caeria en el indice " + candidate +
                    ", fuera del dominio de la reticula."));
                return null;
            }

            var measured = Measure(
                design, grid, sectionHeights, orientations, sides, levelIndex, (int)candidate, diagnostics);

            if (measured == null)
            {
                return null;
            }

            // The bounds were derived, so they hold. Verified anyway, and BLOCKING if not: a derivation nobody
            // checks is a derivation that eventually stops matching the rule it came from.
            var clears = new Dictionary<CantileverArmSide, double>();

            foreach (var cell in measured)
            {
                var below = previous.Cell(cell.Side);
                var clear = cell.BodyBottomZ - below.BodyTopZ;
                clears[cell.Side] = clear;

                if (clear + FitTolerance < design.RequestedClearHeight)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.StationLevelDoesNotFit,
                        "El nivel " + (levelIndex + 1) + " en el lado " + cell.Side + " deja un claro de " +
                        Format(clear) + " in y se pidieron " + Format(design.RequestedClearHeight) + " in."));
                    return null;
                }

                if (cell.PlateBottomZ + FitTolerance < below.PlateTopZ)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.StationLevelDoesNotFit,
                        "Las placas de los niveles " + levelIndex + " y " + (levelIndex + 1) + " en el lado " +
                        cell.Side + " se traslapan."));
                    return null;
                }

                if (cell.LowerPunchIndex <= below.UpperPunchIndex)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.StationLevelDoesNotFit,
                        "Los niveles " + levelIndex + " y " + (levelIndex + 1) + " en el lado " + cell.Side +
                        " compartirian troqueles."));
                    return null;
                }
            }

            return new CantileverStationLevelPlan(
                levelIndex, (int)candidate, measured, clears, Array.Empty<CantileverDiagnostic>());
        }

        /// <summary>
        /// Measures every active cell of one level through the SHARED authority.
        ///
        /// Returns null when any cell is blocked, with its diagnostics already recorded. That is the whole
        /// point: an invalid row count or a missing margin stops the layout HERE instead of being normalised
        /// into a plausible number the column would then be sized from.
        /// </summary>
        private static IReadOnlyList<CantileverArmConnectionMetrics> Measure(
            CantileverStationDesign design,
            CantileverColumnRegularPunchGrid grid,
            Func<int, CantileverArmSide, double> sectionHeights,
            Func<int, CantileverArmSide, CantileverArmBodyOrientation> orientations,
            IReadOnlyList<CantileverArmSide> sides,
            int levelIndex,
            int lowerPunchIndex,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            var cells = new List<CantileverArmConnectionMetrics>(sides.Count);
            var blocked = false;

            foreach (var side in sides)
            {
                // The EFFECTIVE arm of this cell, never the default assumed. A level whose override is a deeper
                // section needs more room, and a layout that used the default would place the next level on top
                // of it (ADR-0026, D4).
                var template = design.EffectiveArm(levelIndex, side) ?? new CantileverArmTemplateDesign();
                var mount = template.MountingPlate ?? new CantileverArmMountingPlateTemplateDesign();
                var body = template.Body ?? new CantileverArmBodyDesign();

                var metrics = CantileverArmConnectionMetricsResolver.Resolve(
                    side,
                    body.Arrangement,
                    orientations(levelIndex, side),
                    sectionHeights(levelIndex, side),
                    body.SlopeRisePer12,
                    lowerPunchIndex,
                    mount.VerticalPunchCount,
                    mount.VerticalEndOffset,
                    grid,
                    grid.Diameter);

                if (metrics.IsBlocked)
                {
                    blocked = true;

                    foreach (var diagnostic in metrics.Diagnostics.Where(d => d.IsBlocking))
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            diagnostic.Code,
                            "Nivel " + (levelIndex + 1) + ", lado " + side + ": " + diagnostic.Message));
                    }

                    continue;
                }

                cells.Add(metrics);
            }

            return blocked ? null : cells;
        }

        /// <summary>The plate margin of one cell, or null when the design does not declare it.</summary>
        private static double? OffsetOf(
            CantileverStationDesign design, int levelIndex, CantileverArmSide side)
        {
            var template = design.EffectiveArm(levelIndex, side);
            var offset = template?.MountingPlate?.VerticalEndOffset;

            return offset != null && GeometryTolerance.IsFinite(offset.Value) && offset.Value >= 0.0
                ? offset
                : null;
        }

        /// <summary>
        /// The shortest column that holds the layout.
        ///
        /// Two independent requirements, and the answer is the larger: the top level's own occupation plus the
        /// requested margin, and the highest USED punch plus the column's top punch offset. The second is not
        /// implied by the first — a plate with many rows can reach above the body it carries.
        ///
        /// The free end's cap and stop are deliberately NOT counted: they are out at the tip, not in the
        /// connection plane, so they never decide how tall the column is (ADR-0026, D6).
        /// </summary>
        private static double? MinimumColumnHeight(
            IReadOnlyList<CantileverStationLevelPlan> levels,
            CantileverColumnRegularPunchGrid grid,
            CantileverStationDesign design,
            double requestedTopClear,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            var top = levels[levels.Count - 1];
            var highestPunchIndex = levels.Max(l => l.UpperPunchIndex);

            var byOccupation = top.OccupiedTopZ + requestedTopClear;

            if (!grid.TryElevationAt(highestPunchIndex, out var highestElevation))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationPunchIndexDomainOverflow,
                    "El indice " + highestPunchIndex + " no esta definido en la reticula."));
                return null;
            }

            var byPunch = highestElevation +
                          (design.ColumnBaseTemplate?.Connection?.Punches?.ColumnTopPunchOffset ?? 0.0);

            return Math.Max(byOccupation, byPunch);
        }

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
