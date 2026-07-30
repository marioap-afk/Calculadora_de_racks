using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// What one arm of one cell measures AT THE CONNECTION PLANE, for a candidate punch index.
    ///
    /// It exists so the level layout can decide where a level goes BEFORE the column exists and before I-37B
    /// has resolved anything. Everything here is predicted; step 11 of the station resolution then CHECKS the
    /// prediction against the arms I-37B actually built, and a disagreement fails closed (ADR-0026, D5).
    ///
    /// The section extent comes from <c>StructuralSectionGeometry.Bounds</c> through the arrangement
    /// authority, never from <c>d</c> or a tabulated dimension.
    /// </summary>
    public sealed class CantileverStationArmMetrics
    {
        private CantileverStationArmMetrics(
            CantileverArmSide side,
            int lowerPunchIndex,
            int verticalPunchCount,
            double firstElevation,
            double lastElevation,
            double plateBottomZ,
            double plateTopZ,
            double bodyBottomZ,
            double bodyTopZ)
        {
            Side = side;
            LowerPunchIndex = lowerPunchIndex;
            VerticalPunchCount = verticalPunchCount;
            FirstElevation = firstElevation;
            LastElevation = lastElevation;
            PlateBottomZ = plateBottomZ;
            PlateTopZ = plateTopZ;
            BodyBottomZ = bodyBottomZ;
            BodyTopZ = bodyTopZ;
        }

        public CantileverArmSide Side { get; }

        public int LowerPunchIndex { get; }

        public int VerticalPunchCount { get; }

        /// <summary>Index of the highest punch this arm uses. The next level must start ABOVE it.</summary>
        public int UpperPunchIndex => LowerPunchIndex + VerticalPunchCount - 1;

        public double FirstElevation { get; }

        public double LastElevation { get; }

        /// <summary>Bottom edge of the mounting plate, and the anchor of the body's lower envelope.</summary>
        public double PlateBottomZ { get; }

        /// <summary>Top edge of the mounting plate. More rows raise THIS (ADR-0025, D6).</summary>
        public double PlateTopZ { get; }

        /// <summary>Lower edge of the BODY at the connection plane. Equal to <see cref="PlateBottomZ"/>.</summary>
        public double BodyBottomZ { get; }

        /// <summary>
        /// Upper edge of the BODY at the connection plane.
        ///
        /// It is <c>BodyBottomZ + cos(slope) × combined section height</c>. The cosine is there because the
        /// section's depth axis is tilted, so its APPARENT vertical extent in this plane is foreshortened —
        /// the same factor I-37B's plate-fit gate applies, and the reason the clear check is not simply
        /// "plate ≥ depth".
        /// </summary>
        public double BodyTopZ { get; }

        /// <summary>
        /// Predicts the metrics of one arm.
        /// </summary>
        /// <param name="grid">The one regular-punch authority.</param>
        /// <param name="lowerPunchIndex">Candidate index, base zero.</param>
        /// <param name="verticalPunchCount">How many rows the plate uses.</param>
        /// <param name="verticalEndOffset">Plate margin above and below the outermost punches.</param>
        /// <param name="combinedSectionHeight">Depth of the body's combined section, from <c>Bounds</c>.</param>
        /// <param name="slopeRisePer12">The body slope. Zero is legal.</param>
        public static CantileverStationArmMetrics Predict(
            CantileverColumnRegularPunchGrid grid,
            CantileverArmSide side,
            int lowerPunchIndex,
            int verticalPunchCount,
            double verticalEndOffset,
            double combinedSectionHeight,
            double slopeRisePer12)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            var first = grid.ElevationAt(lowerPunchIndex);
            var last = grid.ElevationAt(lowerPunchIndex + verticalPunchCount - 1);
            var bottom = first - verticalEndOffset;
            var run = Math.Cos(CantileverArmFrameResolver.AngleRadians(slopeRisePer12));

            return new CantileverStationArmMetrics(
                side, lowerPunchIndex, verticalPunchCount, first, last,
                bottom, last + verticalEndOffset,
                bottom, bottom + (run * combinedSectionHeight));
        }

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "{0}:idx={1}..{2};plate={3:0.######}..{4:0.######};body={5:0.######}..{6:0.######}",
            Side, LowerPunchIndex, UpperPunchIndex, PlateBottomZ, PlateTopZ, BodyBottomZ, BodyTopZ);

        public override string ToString() => "ArmMetrics " + Signature();
    }

    /// <summary>One resolved level of the layout: its index, its punch index and one cell per active side.</summary>
    public sealed class CantileverStationLevelPlan
    {
        internal CantileverStationLevelPlan(
            int levelIndex,
            int lowerPunchIndex,
            IReadOnlyList<CantileverStationArmMetrics> cells,
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

        /// <summary>One entry per ACTIVE side. A single station has one; the inactive side is not a cell.</summary>
        public IReadOnlyList<CantileverStationArmMetrics> Cells { get; }

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

        public CantileverStationArmMetrics Cell(CantileverArmSide side) =>
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
    /// Three rules decide whether a candidate index is acceptable, and all three must hold on EVERY active
    /// side: the body-to-body clear reaches the requested value; the mounting plates do not overlap; and the
    /// punch ranges are disjoint. The search walks UPWARDS and takes the first index that passes — the
    /// adjustment is mandatory upwards, and the answer is always an index, never a rounded elevation
    /// (ADR-0026, D4).
    /// </summary>
    public static class CantileverStationLevelLayoutResolver
    {
        private const double FitTolerance = 1e-9;

        /// <summary>
        /// How many candidate indices above the previous level are tried before giving up.
        ///
        /// It is a SEARCH bound and not a product limit: with a 4 in pitch it covers over eighty feet of extra
        /// rise for one level, so reaching it means the clear cannot be satisfied at all rather than that the
        /// search was too short. Blocking with a diagnostic beats looping forever on a bad number.
        /// </summary>
        public const int MaxCandidatesPerLevel = 250;

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
        public static CantileverStationLevelLayout Resolve(
            CantileverStationDesign design,
            CantileverColumnRegularPunchGrid grid,
            Func<int, CantileverArmSide, double> sectionHeights)
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

            var diagnostics = new List<CantileverDiagnostic>();
            var sides = design.ActiveSides();
            var levels = new List<CantileverStationLevelPlan>();

            if (design.LevelCount == 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationNoLevels,
                    "Una estacion necesita al menos un nivel."));
                return new CantileverStationLevelLayout(levels, 0.0, 0.0, diagnostics);
            }

            var topClear = ValidateTopClearFactor(design, diagnostics);

            for (var levelIndex = 0; levelIndex < design.LevelCount; levelIndex++)
            {
                var previous = levels.Count == 0 ? null : levels[levels.Count - 1];

                var plan = ResolveLevel(design, grid, sectionHeights, sides, levelIndex, previous, diagnostics);

                if (plan == null)
                {
                    return new CantileverStationLevelLayout(levels, 0.0, topClear, diagnostics);
                }

                levels.Add(plan);
            }

            var minimum = MinimumColumnHeight(levels, grid, design, topClear);

            return new CantileverStationLevelLayout(levels, minimum, topClear, diagnostics);
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
            IReadOnlyList<CantileverArmSide> sides,
            int levelIndex,
            CantileverStationLevelPlan previous,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            // Level 0 uses the requested index EXACTLY. It is the one index the user chose, so it is not a
            // candidate to be searched from — searching would let the station move the level the user placed.
            if (previous == null)
            {
                if (design.FirstLevelPunchIndex < 0)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.ArmPunchIndexOutOfRange,
                        "El indice del primer nivel es base cero y no puede ser negativo; se recibio " +
                        design.FirstLevelPunchIndex + "."));
                    return null;
                }

                var cells = sides
                    .Select(s => Predict(design, grid, sectionHeights, levelIndex, s, design.FirstLevelPunchIndex))
                    .ToList();

                return new CantileverStationLevelPlan(
                    levelIndex,
                    design.FirstLevelPunchIndex,
                    cells,
                    new Dictionary<CantileverArmSide, double>(),
                    Array.Empty<CantileverDiagnostic>());
            }

            // Two brand-new arms of two different levels may not share a hole. The floor is therefore the
            // previous level's HIGHEST used index plus one, taken across every active side — the most
            // restrictive side governs, which is what makes a double station safe when its two arms differ
            // in row count (ADR-0026, D4).
            var floor = previous.UpperPunchIndex + 1;

            for (var offset = 0; offset < MaxCandidatesPerLevel; offset++)
            {
                var candidate = floor + offset;
                var cells = sides
                    .Select(s => Predict(design, grid, sectionHeights, levelIndex, s, candidate))
                    .ToList();

                var clears = new Dictionary<CantileverArmSide, double>();
                var acceptable = true;

                foreach (var cell in cells)
                {
                    var below = previous.Cell(cell.Side);
                    var clear = cell.BodyBottomZ - below.BodyTopZ;
                    clears[cell.Side] = clear;

                    // The clear is BODY to BODY, in the connection plane. Not axis to axis, not punch to
                    // punch, and not plate edge to plate edge (ADR-0026, D4).
                    if (clear + FitTolerance < design.RequestedClearHeight)
                    {
                        acceptable = false;
                    }

                    // And the plates must not overlap either. The clear alone does not imply it: a plate with
                    // many rows reaches well above its own body.
                    if (cell.PlateBottomZ + FitTolerance < below.PlateTopZ)
                    {
                        acceptable = false;
                    }
                }

                if (acceptable)
                {
                    return new CantileverStationLevelPlan(
                        levelIndex, candidate, cells, clears, Array.Empty<CantileverDiagnostic>());
                }
            }

            diagnostics.Add(CantileverDiagnostic.Blocking(
                CantileverDiagnostics.StationLevelDoesNotFit,
                "El nivel " + (levelIndex + 1) + " no cabe: ningun troquel entre los indices " + floor +
                " y " + (floor + MaxCandidatesPerLevel - 1) + " deja el claro pedido de " +
                Format(design.RequestedClearHeight) + " in en todos los lados activos. Reduce el claro, " +
                "cambia el brazo o baja la cantidad de troqueles."));

            return null;
        }

        private static CantileverStationArmMetrics Predict(
            CantileverStationDesign design,
            CantileverColumnRegularPunchGrid grid,
            Func<int, CantileverArmSide, double> sectionHeights,
            int levelIndex,
            CantileverArmSide side,
            int candidate)
        {
            // The EFFECTIVE arm of this cell, never the default assumed. A level whose override is a deeper
            // section needs more room, and a layout that used the default would place the next level on top
            // of it (ADR-0026, D4).
            var template = design.EffectiveArm(levelIndex, side) ?? new CantileverArmTemplateDesign();
            var mount = template.MountingPlate ?? new CantileverArmMountingPlateTemplateDesign();
            var body = template.Body ?? new CantileverArmBodyDesign();

            return CantileverStationArmMetrics.Predict(
                grid,
                side,
                candidate,
                Math.Max(2, mount.VerticalPunchCount),
                mount.VerticalEndOffset ?? 0.0,
                sectionHeights(levelIndex, side),
                body.SlopeRisePer12);
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
        private static double MinimumColumnHeight(
            IReadOnlyList<CantileverStationLevelPlan> levels,
            CantileverColumnRegularPunchGrid grid,
            CantileverStationDesign design,
            double requestedTopClear)
        {
            var top = levels[levels.Count - 1];
            var highestPunchIndex = levels.Max(l => l.UpperPunchIndex);

            var byOccupation = top.OccupiedTopZ + requestedTopClear;
            var byPunch = grid.MinimumColumnHeightFor(
                highestPunchIndex,
                design.ColumnBaseTemplate?.Connection?.Punches?.ColumnTopPunchOffset ?? 0.0);

            return Math.Max(byOccupation, byPunch);
        }

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
