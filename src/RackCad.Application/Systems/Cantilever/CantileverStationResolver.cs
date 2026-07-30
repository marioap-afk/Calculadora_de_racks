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
    /// Turns the editable intent of a STATION into resolved geometry, by composing I-37A and I-37B.
    ///
    /// It recalculates none of their geometry. It builds their designs, calls their resolvers and composes the
    /// results — plus the one thing neither of them can do, which is decide where the levels go and how tall
    /// the column must be.
    ///
    /// The resolution is an explicit ELEVEN-STEP sequence, and the order is the point. The dependency
    /// height→punches→levels→height is real, and the sequence breaks it by getting the regular-punch grid from
    /// a seam that does not read the height at all. There is no provisional height, no oversized column, no
    /// second pitch formula and no convergence loop (ADR-0026, D5).
    ///
    /// Step 11 is not decoration: the arms I-37B resolves against the FINAL column are checked against the
    /// layout that sized that column. If they disagree the station fails closed, because a prediction nobody
    /// verifies is a prediction that eventually goes wrong in silence.
    /// </summary>
    public sealed class CantileverStationResolver
    {
        private const double FitTolerance = 1e-9;

        /// <summary>
        /// How far the final pass may differ from the layout before the station fails closed.
        ///
        /// It is a FLOATING-POINT tolerance and nothing else: the two passes compute the same quantities from
        /// the same inputs, so a difference beyond this is a real disagreement and not accumulated noise.
        /// </summary>
        public const double FinalPassTolerance = 1e-6;

        private readonly StructuralSectionCatalog _catalog;
        private readonly StructuralSectionGeometryFactory _geometry;
        private readonly CantileverColumnBaseResolver _columnBase;
        private readonly CantileverArmResolver _arm;

        public CantileverStationResolver(
            StructuralSectionCatalog catalog,
            StructuralSectionGeometryFactory geometry,
            ICantileverColumnBaseSectionPolicy columnBasePolicy,
            ICantileverArmSectionPolicy armPolicy)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            _columnBase = new CantileverColumnBaseResolver(
                catalog, geometry, columnBasePolicy ?? throw new ArgumentNullException(nameof(columnBasePolicy)));
            _arm = new CantileverArmResolver(
                catalog, geometry, armPolicy ?? throw new ArgumentNullException(nameof(armPolicy)));
        }

        public CantileverStationAssembly Resolve(CantileverStationDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            var diagnostics = new List<CantileverDiagnostic>();
            var faceMode = design.FaceMode;
            var singleSide = faceMode == CantileverStationFaceMode.Single
                ? design.SingleSide
                : (CantileverArmSide?)null;

            // ---- 1. validate the design -------------------------------------------------------------------
            if (!IsSupported(faceMode))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationFaceModeNotSupported,
                    "El modo de cara '" + faceMode + "' no tiene regla de composicion."));
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            if (faceMode == CantileverStationFaceMode.Single &&
                !CantileverStationBaseSideResolver.IsSupported(design.SingleSide))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmSideNotSupported,
                    "El lado '" + design.SingleSide + "' no tiene regla de composicion de base."));
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            if (design.LevelCount == 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationNoLevels,
                    "Una estacion necesita al menos un nivel."));
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            if (!GeometryTolerance.IsFinite(design.RequestedClearHeight) || design.RequestedClearHeight <= 0.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "El claro libre solicitado debe ser un numero positivo; se recibio " +
                    Format(design.RequestedClearHeight) + "."));
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            var template = design.ColumnBaseTemplate ?? new CantileverStationColumnBaseTemplateDesign();
            var sides = design.ActiveSides();

            // ---- 2. sections and variants the levels need --------------------------------------------------
            var sectionHeights = ResolveSectionHeights(design, sides, diagnostics);

            if (sectionHeights == null)
            {
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            // ---- 3. the canonical regular-punch grid, WITHOUT a height -------------------------------------
            // The height passed here is never read: ResolveRegularPunchGrid works off the height-independent
            // seam. Zero is passed precisely so a reader can see it is not used — a plausible-looking number
            // would be the provisional height this design exists to avoid.
            var probe = template.ToColumnBaseDesign(0.0);
            var grid = _columnBase.ResolveRegularPunchGrid(probe, diagnostics);

            if (grid == null)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationColumnBaseBlocked,
                    "La configuracion de columna y base no resuelve, asi que la estacion no tiene reticula " +
                    "de troqueles sobre la que colocar niveles."));
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            // ---- 4. level indices and metrics --------------------------------------------------------------
            var layout = CantileverStationLevelLayoutResolver.Resolve(
                design, grid, (level, side) => sectionHeights[(level, side)]);

            diagnostics.AddRange(layout.Diagnostics);

            if (layout.IsBlocked)
            {
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            // ---- 5 and 6. the minimum height, then the one to build with ----------------------------------
            var height = ResolveColumnHeight(design, layout.MinimumColumnHeight, diagnostics);

            if (height == null)
            {
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            // ---- 7 and 8. the FINAL column–base design, and I-37A on it -----------------------------------
            var columnBaseDesign = template.ToColumnBaseDesign(height.Value);
            var resolvedColumnBase = _columnBase.Resolve(columnBaseDesign);

            if (resolvedColumnBase.IsBlocked)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationColumnBaseBlocked,
                    "La columna y la base no resuelven con la altura " + Format(height.Value) + " in."));
                diagnostics.AddRange(resolvedColumnBase.Diagnostics.Where(d => d.IsBlocking));
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            diagnostics.AddRange(resolvedColumnBase.Diagnostics.Where(d => !d.IsBlocking));

            // The column has to actually contain every punch the layout used. It is checked rather than
            // assumed, because the height came from a formula and this is the thing that formula is FOR.
            var available = resolvedColumnBase.ColumnRegularPunches
                .Select(p => p.Datum.V).Distinct().Count();

            if (available <= layout.HighestUsedPunchIndex)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationNotEnoughColumnPunches,
                    "La columna resolvio " + available + " elevaciones regulares y el layout usa hasta el " +
                    "indice " + layout.HighestUsedPunchIndex + "."));
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            // ---- 9. one or two bases from that single resolve ---------------------------------------------
            var columnBase = CantileverStationColumnBaseAssembly.Compose(resolvedColumnBase, faceMode, sides);

            // ---- 10. every arm, through I-37B, against the FINAL column ----------------------------------
            var levels = new List<CantileverStationResolvedLevel>(layout.Levels.Count);

            foreach (var plan in layout.Levels)
            {
                var arms = new List<CantileverArmAssembly>(plan.Cells.Count);

                foreach (var cell in plan.Cells)
                {
                    var armTemplate = design.EffectiveArm(plan.LevelIndex, cell.Side)
                                      ?? new CantileverArmTemplateDesign();

                    var armDesign = CantileverStationArmAdapter.ToArmDesign(
                        armTemplate, cell.Side, plan.LowerPunchIndex);

                    var owner = CantileverPieceTokens.StationArmOwner(plan.LevelIndex, cell.Side);
                    var arm = _arm.Resolve(armDesign, resolvedColumnBase, owner);

                    if (arm.IsBlocked)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.StationArmBlocked,
                            "El brazo del nivel " + (plan.LevelIndex + 1) + " en el lado " + cell.Side +
                            " no resuelve."));
                        diagnostics.AddRange(arm.Diagnostics.Where(d => d.IsBlocking));
                        return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
                    }

                    diagnostics.AddRange(arm.Diagnostics.Where(d => !d.IsBlocking));
                    arms.Add(arm);
                }

                levels.Add(new CantileverStationResolvedLevel(plan, arms));
            }

            // ---- 11. the final pass must AGREE with the layout --------------------------------------------
            VerifyFinalPassMatchesLayout(levels, diagnostics);

            if (diagnostics.Any(d => d.IsBlocking))
            {
                return CantileverStationAssembly.Blocked(faceMode, singleSide, diagnostics);
            }

            // ---- 12. the station --------------------------------------------------------------------------
            return CantileverStationAssembly.Create(
                faceMode, singleSide, columnBase, levels,
                layout.MinimumColumnHeight, height.Value, design.RequestedClearHeight, diagnostics);
        }

        private static bool IsSupported(CantileverStationFaceMode faceMode) =>
            faceMode == CantileverStationFaceMode.Single || faceMode == CantileverStationFaceMode.Double;

        /// <summary>
        /// The combined section depth of every cell, resolved once through I-36 and the arm policy.
        ///
        /// It is resolved HERE and handed to the layout as a lookup, so the layout stays free of the catalogue
        /// and the policy: what it needs is a number per cell, not the ability to look one up.
        /// </summary>
        private Dictionary<(int Level, CantileverArmSide Side), double> ResolveSectionHeights(
            CantileverStationDesign design,
            IReadOnlyList<CantileverArmSide> sides,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            var heights = new Dictionary<(int, CantileverArmSide), double>();

            for (var level = 0; level < design.LevelCount; level++)
            {
                foreach (var side in sides)
                {
                    var template = design.EffectiveArm(level, side);
                    var body = template?.Body ?? new CantileverArmBodyDesign();

                    var section = CantileverSectionResolver.Resolve(
                        _catalog, body.SectionId, CantileverMemberRole.Arm, diagnostics);

                    if (!section.IsResolved)
                    {
                        return null;
                    }

                    if (!_arm.Policy.TryGetVariant(section.SectionId, body.Arrangement, out var variant))
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.CombinationNotEligible,
                            "La combinacion seccion '" + section.SectionId.Value + "' con arreglo '" +
                            body.Arrangement + "' no esta registrada para el nivel " + (level + 1) +
                            " en el lado " + side + "."));
                        return null;
                    }

                    if (!CantileverArmBodyArrangementResolver.IsSupported(body.Arrangement))
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.ArmArrangementNotSupported,
                            "El arreglo '" + body.Arrangement + "' no tiene regla de colocacion."));
                        return null;
                    }

                    var geometry = _geometry.Get(section.SectionId, variant.Detail);

                    // The DEPTH comes from the arrangement authority over Bounds — never from d, and never
                    // from a single member's bounds when the body is a pair.
                    heights[(level, side)] = CantileverArmBodyArrangementResolver
                        .CombinedBounds(body.Arrangement, geometry).Height;
                }
            }

            return heights;
        }

        /// <summary>
        /// Automatic or manual, with an exhaustive <c>switch</c>.
        ///
        /// A manual height BELOW the minimum blocks. It is not raised to the minimum, the levels are not
        /// trimmed, the arms are not moved and the clear is not reduced: the user asked for a specific column,
        /// and quietly building a different one is how a drawing stops matching its own inputs (ADR-0026, D6).
        /// </summary>
        private static double? ResolveColumnHeight(
            CantileverStationDesign design,
            double minimum,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            var height = design.ColumnHeight ?? new CantileverStationColumnHeightDesign();

            switch (height.Mode)
            {
                case CantileverStationColumnHeightMode.Automatic:
                    // No commercial rounding: none is approved, and rounding up here would be a number nobody
                    // authorised sitting in the middle of a resolved design.
                    return minimum;

                case CantileverStationColumnHeightMode.Manual:
                    if (height.ManualHeight == null)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.StationManualHeightMissing,
                            "El modo de altura manual exige declarar la altura de la columna."));
                        return null;
                    }

                    var manual = height.ManualHeight.Value;

                    if (!GeometryTolerance.IsFinite(manual) || manual <= 0.0)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.ParameterNotPositive,
                            "La altura manual de la columna debe ser un numero positivo; se recibio " +
                            Format(manual) + "."));
                        return null;
                    }

                    if (manual + FitTolerance < minimum)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.StationManualHeightBelowMinimum,
                            "La altura manual de " + Format(manual) + " in queda " +
                            Format(minimum - manual) + " in por debajo de la minima que los niveles exigen (" +
                            Format(minimum) + " in). Aumenta la altura, reduce el claro o quita un nivel: la " +
                            "estacion NO recorta niveles ni reduce el claro por su cuenta."));
                        return null;
                    }

                    return manual;

                default:
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.StationHeightModeNotSupported,
                        "El modo de altura '" + height.Mode + "' no esta declarado."));
                    return null;
            }
        }

        /// <summary>
        /// Checks the arms I-37B built against the layout that sized the column.
        ///
        /// The two passes compute the same quantities — the selected elevations, the plate edges — from the
        /// same inputs, so they must agree. The check exists because "must" is not "does": if the prediction
        /// and the reality ever part company, the station has to say so rather than quietly ship the second
        /// answer (ADR-0026, D5).
        /// </summary>
        private static void VerifyFinalPassMatchesLayout(
            IReadOnlyList<CantileverStationResolvedLevel> levels,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            foreach (var level in levels)
            {
                foreach (var cell in level.Plan.Cells)
                {
                    var arm = level.Arm(cell.Side);

                    if (arm?.ConnectionPattern == null)
                    {
                        diagnostics.Add(Mismatch(level, cell, "no hay patron de conexion resuelto"));
                        continue;
                    }

                    var pattern = arm.ConnectionPattern;

                    if (pattern.LowerColumnPunchIndex != cell.LowerPunchIndex)
                    {
                        diagnostics.Add(Mismatch(
                            level, cell,
                            "el indice inferior resuelto es " + pattern.LowerColumnPunchIndex +
                            " y el layout uso " + cell.LowerPunchIndex));
                    }

                    if (pattern.VerticalPunchCount != cell.VerticalPunchCount)
                    {
                        diagnostics.Add(Mismatch(
                            level, cell,
                            "la cantidad de troqueles resuelta es " + pattern.VerticalPunchCount +
                            " y el layout uso " + cell.VerticalPunchCount));
                    }

                    Compare(level, cell, "la primera elevacion", pattern.FirstElevation, cell.FirstElevation, diagnostics);
                    Compare(level, cell, "la ultima elevacion", pattern.LastElevation, cell.LastElevation, diagnostics);
                    Compare(level, cell, "el borde inferior de la placa", pattern.PlateBottomZ, cell.PlateBottomZ, diagnostics);
                    Compare(level, cell, "el borde superior de la placa", pattern.PlateTopZ, cell.PlateTopZ, diagnostics);
                }
            }
        }

        private static void Compare(
            CantileverStationResolvedLevel level,
            CantileverStationArmMetrics cell,
            string what,
            double resolved,
            double predicted,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (Math.Abs(resolved - predicted) > FinalPassTolerance)
            {
                diagnostics.Add(Mismatch(
                    level, cell,
                    what + " resuelta es " + Format(resolved) + " in y el layout predijo " +
                    Format(predicted) + " in"));
            }
        }

        private static CantileverDiagnostic Mismatch(
            CantileverStationResolvedLevel level, CantileverStationArmMetrics cell, string detail) =>
            CantileverDiagnostic.Blocking(
                CantileverDiagnostics.StationFinalPassDiffersFromLayout,
                "El pase final difiere del layout en el nivel " + (level.LevelIndex + 1) + ", lado " +
                cell.Side + ": " + detail + ". La estacion no se construye con un resultado que no coincide " +
                "con el que dimensiono la columna.");

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
