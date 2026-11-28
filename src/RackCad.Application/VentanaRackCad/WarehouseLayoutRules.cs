using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Formatting;
using RackCad.Application.Geometry;
using RackCad.Application.Layout;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>T5 al: what one square of the scheme of the page «Almacén» is.</summary>
    public enum WarehouseCellKind
    {
        /// <summary>The seed of the grid: it is in the drawing already.</summary>
        Seed,

        /// <summary>A new rack.</summary>
        New,

        /// <summary>A rack of the grid that leaves the building of «Verificar encaje».</summary>
        OutOfBuilding,

        /// <summary>A candidate of the fill outside the outline (with the clearance to the walls).</summary>
        Outside,

        /// <summary>A candidate of the fill on a column or obstacle (with its clearance).</summary>
        OnColumn,
    }

    public enum WarehouseProblemKind
    {
        Error,
        Warning,
        Info,
    }

    /// <summary>T5 al: one line of «Resultado»: an error, a warning or a note.</summary>
    public sealed record WarehouseProblem(WarehouseProblemKind Kind, string Text);

    /// <summary>T5 al: one square of the scheme: its row and column in the grid, its label (A1, B2…) and what it is.</summary>
    public sealed record WarehouseSchemeCell(int Row, int Col, string Label, WarehouseCellKind Kind);

    /// <summary>
    /// T5 al: the grid of «Rejilla», computed now by the planner of RACKLAYOUT over the measured seed (the same call, the same origin): every
    /// cell with its label and what it is, the labels that leave the building when it is given, and the extent.
    /// </summary>
    public sealed record WarehouseGridPreview(
        WarehouseGridPlan Plan,
        int Rows,
        int Cols,
        bool Paired,
        double AisleX,
        double AisleY,
        double FootprintX,
        double FootprintY,
        IReadOnlyList<WarehouseSchemeCell> Cells,
        IReadOnlyList<string> OutLabels,
        bool FitChecked,
        double BuildingX,
        double BuildingY)
    {
        /// <summary>The racks «Colocar» adds: every cell but the seed.</summary>
        public int NewRacks => Rows * Cols - 1;
    }

    /// <summary>
    /// T5 al: the fill of «Rellenar un área», computed now by the auto-fill of RACKRELLENAR (the same call): the result, the site it was made
    /// over, the aisles on the world axes of the orientation chosen, whether the seed lies in the area, and every candidate cell.
    /// </summary>
    public sealed record WarehouseFillPreview(
        WarehouseAutoFillResult Result,
        WarehouseSite Site,
        double AisleX,
        double AisleY,
        bool SeedInside,
        IReadOnlyList<WarehouseSchemeCell> Cells,
        IReadOnlyList<string> OnColumnLabels)
    {
        public int Kept => Result.Cells.Count;
    }

    /// <summary>
    /// T5 al: everything the page says about its values over a reading, for one tab: the error of each field, whether each tab has problems,
    /// the grid or the fill, how many racks «Colocar» adds, why it cannot (null when it can) and the lines of «Resultado».
    /// </summary>
    public sealed record WarehouseLayoutAssessment(
        WarehouseLayoutMode Mode,
        IReadOnlyDictionary<string, string> Errors,
        string GridError,
        bool GridHasErrors,
        bool FillHasErrors,
        WarehouseGridPreview Grid,
        WarehouseFillPreview Fill,
        string FillError,
        int Count,
        string BlockedReason,
        IReadOnlyList<WarehouseProblem> Problems)
    {
        public bool CanPlace => BlockedReason == null;
    }

    /// <summary>T5 al: what RACKALMACENCOLOCAR does with a request over the reading it made now.</summary>
    public sealed record WarehousePlacePlan(bool CanPlace, bool DrawingChanged, string Reason, WarehouseLayoutAssessment Assessment);

    /// <summary>
    /// T5 al (I-64, prototype «almacen», questions 1 to 11 accepted): the rules of the page «Almacén» of the RackCad window, pure. The
    /// computation is TODAY's (question 9): the grid is <see cref="WarehouseGridPlanner.Plan"/> and its fit check is
    /// <see cref="WarehouseFitChecker"/> over a building anchored at the corner of the seed, as RACKLAYOUT does; the fill is
    /// <see cref="WarehouseAutoFill.Fill"/> over <see cref="WarehouseSite.FromBoundary"/>, as RACKRELLENAR does. The page uses these rules
    /// to show the result before anything is written, and RACKALMACENCOLOCAR uses the SAME rules over the reading it makes again (question
    /// 5), so what is placed is what the scheme showed. The validation is the one of the classic windows, said field by field.
    /// </summary>
    public static partial class WarehouseLayoutRules // T6 rl: partial (WarehouseLayoutRules.Shortcut.cs, RLY and RR)
    {
        public const string ReadCommand = "RACKALMACENLEER";
        public const string PlaceCommand = "RACKALMACENCOLOCAR";
        public const string LabelLayer = "RACKCAD_LAYOUT";
        public const string DefaultSiteLayer = "RACKCAD_SITIO";

        /// <summary>The limit of RACKLAYOUT (RackWarehouseLayoutWindow), said in the page.</summary>
        public const int MaxGridRacks = 1000;

        /// <summary>The limit of RACKRELLENAR (<see cref="WarehouseAutoFill"/>), said in the page instead of an error of AutoCAD.</summary>
        public const int MaxFillRacks = 5000;

        public const string FieldRows = "Rows";
        public const string FieldCols = "Cols";
        public const string FieldAisleRows = "AisleRows";
        public const string FieldAisleCols = "AisleCols";
        public const string FieldBackGap = "BackGap";
        public const string FieldBuildingX = "BuildingX";
        public const string FieldBuildingY = "BuildingY";
        public const string FieldWallClearance = "WallClearance";
        public const string FieldColumnClearance = "ColumnClearance";

        public const string IntegerText = "Usa un entero de 1 o más.";
        public const string NonNegativeText = "Usa un número de 0 o más.";
        public const string PositiveText = "Usa un número mayor que cero.";
        public const string BothSidesText = "Escribe los dos lados del edificio, o deja los dos vacíos.";
        public const string OneRackText = "La rejilla debe tener más de un rack: aumenta filas o columnas.";
        public const string TooManyGridText = "Máximo 1000 racks por rejilla: reduce filas o columnas, o hazlo por partes.";
        public const string TooManyFillText = "El relleno pasaría de 5000 racks: revisa las unidades del contorno o los pasillos.";
        public const string NotReadText = "Sin leer: «Volver a leer» pide a AutoCAD la planta de este rack.";
        public const string GridFixText = "Corrige lo marcado en «Rejilla», «Pasillos» o «Verificar encaje».";
        public const string FillFixText = "Corrige lo marcado en «Sitio» o «Pasillos».";
        public const string NoOutlineText = "La capa elegida no tiene contorno.";
        public const string NothingFitsText = "No cabe ningún rack.";
        public const string NoEmbeddedDesignText = "La planta no tiene datos de rack embebidos: usa copias enlazadas.";

        // ------------------------------------------------------------------ the fields

        /// <summary>The error of every field that has one (by its key), the rules of the two classic windows with the words of the page.</summary>
        public static IReadOnlyDictionary<string, string> FieldErrors(WarehouseLayoutValues values)
        {
            var v = values ?? WarehouseLayoutValues.Defaults;
            var errors = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!IsCount(v.Rows)) errors[FieldRows] = IntegerText;
            if (!IsCount(v.Cols)) errors[FieldCols] = IntegerText;
            if (!IsLength(v.AisleRows)) errors[FieldAisleRows] = NonNegativeText;
            if (!IsLength(v.AisleCols)) errors[FieldAisleCols] = NonNegativeText;
            if (v.BackToBack && !IsLength(v.BackGap)) errors[FieldBackGap] = NonNegativeText; // read only with its switch (dolor 7)
            if (!IsLength(v.WallClearance)) errors[FieldWallClearance] = NonNegativeText;
            if (!IsLength(v.ColumnClearance)) errors[FieldColumnClearance] = NonNegativeText;

            // «Verificar encaje»: the two sides or none (RackWarehouseLayoutWindow), each said in its own field.
            var hasX = !string.IsNullOrWhiteSpace(v.BuildingX);
            var hasY = !string.IsNullOrWhiteSpace(v.BuildingY);
            if (hasX || hasY)
            {
                if (!IsPositive(v.BuildingX)) errors[FieldBuildingX] = hasX ? PositiveText : BothSidesText;
                if (!IsPositive(v.BuildingY)) errors[FieldBuildingY] = hasY ? PositiveText : BothSidesText;
            }

            return errors;
        }

        /// <summary>The size of the grid, once filas and columnas are integers: more than one rack, and at most 1000 (RACKLAYOUT).</summary>
        public static string GridSizeError(WarehouseLayoutValues values)
        {
            var v = values ?? WarehouseLayoutValues.Defaults;
            if (!TryCount(v.Rows, out var rows) || !TryCount(v.Cols, out var cols)) return null;
            if ((long)rows * cols == 1) return OneRackText;
            if ((long)rows * cols > MaxGridRacks) return TooManyGridText;
            return null;
        }

        // ------------------------------------------------------------------ the result

        /// <summary>
        /// Everything the page says about its values over a reading, for the tab of <paramref name="mode"/> (the problems of the other tab
        /// are counted too, for its mark «con problemas»). The grid and the fill are today's computation over the measured seed.
        /// </summary>
        public static WarehouseLayoutAssessment Assess(WarehouseReading reading, WarehouseLayoutValues values, WarehouseLayoutMode mode)
        {
            var v = values ?? WarehouseLayoutValues.Defaults;
            var errors = FieldErrors(v);
            var gridError = GridSizeError(v);
            var shared = errors.ContainsKey(FieldAisleRows) || errors.ContainsKey(FieldAisleCols) || errors.ContainsKey(FieldBackGap);
            var gridHas = shared || gridError != null || errors.ContainsKey(FieldRows) || errors.ContainsKey(FieldCols)
                          || errors.ContainsKey(FieldBuildingX) || errors.ContainsKey(FieldBuildingY);
            var site = SiteFor(reading, v.Layer);
            var siteOk = site != null && site.HasOutline;
            var fillHas = shared || !siteOk || errors.ContainsKey(FieldWallClearance) || errors.ContainsKey(FieldColumnClearance);
            var problems = new List<WarehouseProblem>();

            WarehouseLayoutAssessment Result(string blocked, int count = 0, WarehouseGridPreview grid = null, WarehouseFillPreview fill = null, string fillError = null) =>
                new(mode, errors, gridError, gridHas, fillHas, grid, fill, fillError, count, blocked, problems);

            if (reading == null) return Result(NotReadText);
            if (!reading.IsRead) return Result(reading.Failure);
            var seed = reading.Seed;
            if (seed == null) return Result(string.IsNullOrWhiteSpace(reading.SeedProblem) ? NotReadText : reading.SeedProblem);
            if (!(seed.FootprintX > 0.0) || !(seed.FootprintY > 0.0))
                return Result("La vista en planta no tiene copias insertadas, o no se pudo medir su tamaño.");

            if (mode == WarehouseLayoutMode.Grid)
            {
                if (gridHas) return Result(GridFixText);
                var grid = PlanGrid(seed, v);
                if (grid.OutLabels.Count > 0)
                    problems.Add(new WarehouseProblem(WarehouseProblemKind.Error,
                        (grid.OutLabels.Count == 1 ? "Se sale" : "Se salen") + " del edificio: " + ListText(grid.OutLabels, 12) + "."));
                if (grid.Paired && grid.Rows > 1 && grid.Rows % 2 == 1)
                    problems.Add(new WarehouseProblem(WarehouseProblemKind.Info,
                        "Con " + grid.Rows.ToString(CultureInfo.InvariantCulture) + " filas espalda con espalda, la fila "
                        + WarehouseGridPlanner.RowLetters(grid.Rows - 1) + " queda sola."));
                if (!seed.KindKnown) return Result(UnknownKindText(seed.Kind), grid.NewRacks, grid);
                if (v.Independent && !seed.HasEmbeddedDesign) return Result(NoEmbeddedDesignText, grid.NewRacks, grid);
                return Result(null, grid.NewRacks, grid);
            }

            if (fillHas)
            {
                if (site == null) return Result(SiteNotReadText(v.Layer));
                if (!site.HasOutline) return Result(NoOutlineText);
                return Result(FillFixText);
            }

            WarehouseFillPreview fill;
            try
            {
                fill = PlanFill(seed, site, v);
            }
            catch (InvalidOperationException)
            {
                // WarehouseAutoFill refuses more than 5000 racks with an exception (dolor 12): the page says it in its place.
                problems.Add(new WarehouseProblem(WarehouseProblemKind.Error, TooManyFillText));
                return Result(TooManyFillText, fillError: TooManyFillText);
            }

            var result = fill.Result;
            if (fill.Kept == 0)
                problems.Add(new WarehouseProblem(WarehouseProblemKind.Error, string.Format(CultureInfo.InvariantCulture,
                    "No cabe ningún rack en el área: rejilla probada {0} × {1}, {2} celdas sobre obstáculos y {3} fuera del contorno. Revisa pasillos, holguras y unidades.",
                    result.RowsTried, result.ColsTried, result.OmittedByObstacle, result.OmittedOutside)));
            if (fill.OnColumnLabels.Count > 0)
                problems.Add(new WarehouseProblem(WarehouseProblemKind.Warning,
                    (fill.OnColumnLabels.Count == 1 ? "Queda fuera por una columna: " : "Quedan fuera por columnas: ") + ListText(fill.OnColumnLabels, 12) + "."));
            if (fill.Kept > 0 && (result.OmittedOutside > 0 || result.OmittedByObstacle > 0))
                problems.Add(new WarehouseProblem(WarehouseProblemKind.Info,
                    "Las etiquetas conservan su fila y su columna, así que lo descartado deja huecos en la numeración."));
            return Result(fill.Kept == 0 ? NothingFitsText : null, fill.Kept, fill: fill);
        }

        /// <summary>
        /// The grid of RACKLAYOUT over the seed: <see cref="WarehouseGridPlanner.Plan"/> from the insertion point of the seed, and, with a
        /// building, every rack against a site anchored at the corner of the seed (its bounding box), as <c>ConfirmFits</c> does. The values
        /// must be valid (see <see cref="Assess"/>).
        /// </summary>
        public static WarehouseGridPreview PlanGrid(WarehouseSeedFacts seed, WarehouseLayoutValues values)
        {
            if (seed == null) throw new ArgumentNullException(nameof(seed));
            var v = values ?? WarehouseLayoutValues.Defaults;
            TryCount(v.Rows, out var rows);
            TryCount(v.Cols, out var cols);
            var aisleX = Length(v.AisleRows);
            var aisleY = Length(v.AisleCols);
            var paired = v.BackToBack;
            var gap = paired ? Length(v.BackGap) : 0.0;
            var plan = WarehouseGridPlanner.Plan(
                seed.FootprintX, seed.FootprintY, rows, cols, aisleX, aisleY, seed.OriginX, seed.OriginY,
                paired ? RowPairing.BackToBack : RowPairing.Single, gap);

            var fitChecked = TryPositive(v.BuildingX, out var buildingX) & TryPositive(v.BuildingY, out var buildingY);
            WarehouseSite building = fitChecked
                ? new WarehouseSite(buildingX, buildingY, originX: seed.OriginX + seed.OffsetX, originY: seed.OriginY + seed.OffsetY)
                : null;
            var cells = new List<WarehouseSchemeCell>(plan.Cells.Count);
            var outLabels = new List<string>();
            foreach (var cell in plan.Cells)
            {
                var kind = cell.IsSeed ? WarehouseCellKind.Seed : WarehouseCellKind.New;
                if (building != null)
                {
                    var x0 = cell.X + seed.OffsetX;
                    var y0 = cell.Y + seed.OffsetY;
                    if (WarehouseFitChecker.ClassifyRect(x0, y0, x0 + seed.FootprintX, y0 + seed.FootprintY, building) != null)
                    {
                        outLabels.Add(cell.Label);
                        if (!cell.IsSeed) kind = WarehouseCellKind.OutOfBuilding;
                    }
                }

                cells.Add(new WarehouseSchemeCell(cell.Row, cell.Col, cell.Label, kind));
            }

            return new WarehouseGridPreview(plan, rows, cols, paired, aisleX, aisleY, seed.FootprintX, seed.FootprintY, cells, outLabels,
                fitChecked, fitChecked ? buildingX : 0.0, fitChecked ? buildingY : 0.0);
        }

        /// <summary>
        /// The fill of RACKRELLENAR over the seed and the site: <see cref="WarehouseAutoFill.Fill"/> with the same arguments. It throws as the
        /// auto-fill does past 5000 racks (<see cref="Assess"/> says it). The values must be valid.
        /// </summary>
        public static WarehouseFillPreview PlanFill(WarehouseSeedFacts seed, WarehouseSiteFacts site, WarehouseLayoutValues values)
        {
            if (seed == null) throw new ArgumentNullException(nameof(seed));
            if (site == null || !site.HasOutline) throw new ArgumentException("El sitio no tiene contorno.", nameof(site));
            var v = values ?? WarehouseLayoutValues.Defaults;
            var aisleRows = Length(v.AisleRows);
            var aisleCols = Length(v.AisleCols);
            var paired = v.BackToBack;
            var gap = paired ? Length(v.BackGap) : 0.0;
            var warehouse = SiteOf(site, Length(v.WallClearance), Length(v.ColumnClearance));
            var result = WarehouseAutoFill.Fill(
                warehouse, seed.FootprintX, seed.FootprintY, aisleRows, aisleCols,
                paired ? RowPairing.BackToBack : RowPairing.Single, gap, v.TryRotated);

            var rotated = result.Orientation == RackOrientation.Rotated;
            var byCell = new Dictionary<(int Row, int Col), WarehouseSchemeCell>();
            foreach (var cell in result.Cells) byCell[(cell.Row, cell.Col)] = new WarehouseSchemeCell(cell.Row, cell.Col, cell.Label, WarehouseCellKind.New);
            var onColumn = new List<string>();
            foreach (var omitted in result.Omitted)
            {
                var kind = omitted.Reason == FitViolationKind.OutOfBounds ? WarehouseCellKind.Outside : WarehouseCellKind.OnColumn;
                byCell[(omitted.Cell.Row, omitted.Cell.Col)] = new WarehouseSchemeCell(omitted.Cell.Row, omitted.Cell.Col, omitted.Cell.Label, kind);
            }

            var cells = byCell.Values.OrderBy(c => c.Row).ThenBy(c => c.Col).ToList();
            onColumn.AddRange(cells.Where(c => c.Kind == WarehouseCellKind.OnColumn).Select(c => c.Label));
            return new WarehouseFillPreview(result, warehouse, rotated ? aisleCols : aisleRows, rotated ? aisleRows : aisleCols,
                SeedInsideSite(seed, warehouse), cells, onColumn);
        }

        /// <summary>The site of RACKRELLENAR: the outline with the clearance to the walls, and every obstacle with the clearance to the columns.</summary>
        public static WarehouseSite SiteOf(WarehouseSiteFacts site, double wallClearance, double columnClearance)
        {
            if (site == null || !site.HasOutline) throw new ArgumentException("El sitio no tiene contorno.", nameof(site));
            var obstacles = (site.Obstacles ?? Array.Empty<WarehouseObstacleFact>())
                .Select(o => new SiteObstacle(o.X, o.Y, o.Width, o.Depth, columnClearance, o.Label))
                .ToList();
            return WarehouseSite.FromBoundary(site.Boundary, wallClearance, 0.0, obstacles);
        }

        /// <summary>The bounding box of the seed overlaps the one of the site (RACKRELLENAR warns: the fill does not consider it, dolor 13).</summary>
        public static bool SeedInsideSite(WarehouseSeedFacts seed, WarehouseSite site)
        {
            if (seed == null || site == null) return false;
            var x0 = seed.OriginX + seed.OffsetX;
            var y0 = seed.OriginY + seed.OffsetY;
            var x1 = x0 + seed.FootprintX;
            var y1 = y0 + seed.FootprintY;
            return x0 < site.OriginX + site.Width && x1 > site.OriginX && y0 < site.OriginY + site.Depth && y1 > site.OriginY;
        }

        // ------------------------------------------------------------------ «Colocar»

        /// <summary>
        /// The mark of what «Colocar» relies on (question 5): the planta of the seed (its reference, its definition, where it is, how it is
        /// turned, how big it is and its design), and to fill, the site of the layer too. Null when the reading has no seed.
        /// </summary>
        public static string Fingerprint(WarehouseReading reading, WarehouseLayoutMode mode)
        {
            var seed = reading?.Seed;
            if (seed == null) return null;
            var text = new StringBuilder();
            text.Append("seed|").Append(seed.RackId).Append('|').Append(seed.ReferenceHandle).Append('|').Append(seed.DefinitionHandle);
            foreach (var value in new[]
                     {
                         seed.OriginX, seed.OriginY, seed.FootprintX, seed.FootprintY, seed.OffsetX, seed.OffsetY, seed.Rotation, seed.ScaleX,
                         seed.ScaleY, seed.ScaleZ,
                     })
                text.Append('|').Append(value.ToString("R", CultureInfo.InvariantCulture));
            text.Append('|').Append(seed.DesignHash).Append('|').Append(seed.KindKnown).Append('|').Append(seed.HasEmbeddedDesign);
            if (mode == WarehouseLayoutMode.Fill)
            {
                var site = reading.Site;
                text.Append("|site|").Append(site?.Layer).Append('|').Append(site?.Problem).Append('|')
                    .Append(site?.IgnoredLarge.ToString(CultureInfo.InvariantCulture));
                foreach (var point in site?.Boundary ?? Array.Empty<Point2D>())
                    text.Append('|').Append(point.X.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(point.Y.ToString("R", CultureInfo.InvariantCulture));
                foreach (var o in site?.Obstacles ?? Array.Empty<WarehouseObstacleFact>())
                    text.Append("|o|").Append(o.X.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(o.Y.ToString("R", CultureInfo.InvariantCulture))
                        .Append(',').Append(o.Width.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(o.Depth.ToString("R", CultureInfo.InvariantCulture))
                        .Append(',').Append(o.Label);
            }

            return HashOf(text.ToString());
        }

        /// <summary>A stable hash of a text (SHA-256, hexadecimal): the design of the planta, and the mark of <see cref="Fingerprint"/>.</summary>
        public static string HashOf(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text ?? string.Empty)));

        /// <summary>
        /// What RACKALMACENCOLOCAR does, over the reading it made NOW: nothing if the planta (or the site) changed since the page read it
        /// (question 5: the page is given the new reading), nothing if the values do not place anything (the same assessment as the page),
        /// nothing if the grid leaves its building and the user did not say «Colocar de todos modos»; otherwise place, with that plan.
        /// </summary>
        public static WarehousePlacePlan PlanPlace(WarehousePlaceRequest request, WarehouseReading fresh)
        {
            if (request?.Values == null) return new WarehousePlacePlan(false, false, "La petición no es válida.", null);
            if (fresh == null || !fresh.IsRead) return new WarehousePlacePlan(false, false, "No se colocó nada: " + Lower(fresh?.Failure ?? "no se pudo leer el dibujo."), null);

            var name = DisplayName(fresh.SeedName ?? fresh.Seed?.Name);
            var now = Fingerprint(fresh, request.Mode);
            if (now == null || !string.Equals(now, request.Fingerprint, StringComparison.Ordinal))
                return new WarehousePlacePlan(false, true, ChangedText(request.Mode, name), null);

            var assessment = Assess(fresh, request.Values, request.Mode);
            if (!assessment.CanPlace) return new WarehousePlacePlan(false, false, "No se colocó nada: " + Lower(assessment.BlockedReason), assessment);
            if (request.Mode == WarehouseLayoutMode.Grid && assessment.Grid.OutLabels.Count > 0 && !request.PlaceAnyway)
                return new WarehousePlacePlan(false, false,
                    "No se colocó nada: la rejilla no cabe en el edificio (" + ListText(assessment.Grid.OutLabels, 12) + ").", assessment);
            return new WarehousePlacePlan(true, false, null, assessment);
        }

        /// <summary>The planta (or the site) changed between the reading of the page and «Colocar».</summary>
        public static string ChangedText(WarehouseLayoutMode mode, string name)
        {
            var who = DisplayName(name);
            return mode == WarehouseLayoutMode.Grid
                ? "No se colocó nada: la planta de " + who + " cambió en el dibujo desde la lectura. La página la volvió a leer: revisa el esquema y vuelve a pulsar «Colocar»."
                : "No se colocó nada: la planta de " + who + " o el sitio cambiaron en el dibujo desde la lectura. La página los volvió a leer: revisa el esquema y vuelve a pulsar «Colocar».";
        }

        /// <summary>What the page says once AutoCAD placed the grid or the fill.</summary>
        public static string PlacedText(WarehouseLayoutMode mode, int placed, WarehouseGridPreview grid, bool independent)
        {
            if (mode == WarehouseLayoutMode.Fill)
                return "Relleno colocado: " + Plural(placed, "rack enlazado", "racks enlazados") + " y sus etiquetas en la capa " + LabelLayer
                       + ". Un «U» en AutoCAD lo deshace.";
            var shape = grid == null
                ? string.Empty
                : " (" + grid.Rows.ToString(CultureInfo.InvariantCulture) + " × " + grid.Cols.ToString(CultureInfo.InvariantCulture) + ", "
                  + (independent ? "independientes" : "enlazados") + (grid.Paired && grid.Rows > 1 ? ", espalda con espalda" : string.Empty) + ")";
            return "Layout colocado: " + Plural(placed, "rack nuevo", "racks nuevos") + shape + " y "
                   + Plural(placed + 1, "etiqueta", "etiquetas") + " en la capa " + LabelLayer + ". Un «U» en AutoCAD lo deshace."
                   + (independent ? " Las copias independientes salen en la lista al «Volver a leer la lista»." : string.Empty);
        }

        /// <summary>The line of the command line, the one RACKLAYOUT and RACKRELLENAR write.</summary>
        public static string CommandLineText(WarehouseLayoutMode mode, int placed, WarehouseGridPreview grid, bool independent)
        {
            if (mode == WarehouseLayoutMode.Fill)
                return string.Format(CultureInfo.InvariantCulture, "RackCad: relleno colocado — {0} racks enlazados + etiquetas.", placed);
            return string.Format(CultureInfo.InvariantCulture, "RackCad: layout colocado — {0} racks nuevos ({1} filas × {2} columnas, {3}{4}).",
                placed, grid?.Rows ?? 0, grid?.Cols ?? 0, independent ? "independientes" : "enlazados",
                grid != null && grid.Paired && grid.Rows > 1 ? ", espalda-con-espalda" : string.Empty);
        }

        /// <summary>The line RACKALMACENLEER leaves in the command line.</summary>
        public static string ReadCommandLineText(WarehouseReading reading)
        {
            if (reading == null) return "RackCad: no se leyó nada para la ventana RackCad.";
            var number = " (" + ReadCommand + ", petición n.º " + reading.RequestId.ToString(CultureInfo.InvariantCulture) + ")";
            if (!reading.IsRead) return "RackCad: " + Lower(reading.Failure) + number;
            return "RackCad: " + ReadingSummary(reading) + number + ".";
        }

        // T6 rl: what a reading read (the seed and the site), shared by RACKALMACENLEER and by RLY and RR (ShortcutCommandLineText).
        private static string ReadingSummary(WarehouseReading reading)
        {
            var seed = reading.Seed != null
                ? "planta de «" + DisplayName(reading.SeedName) + "» leída, " + Number(reading.Seed.FootprintX) + " × " + Number(reading.Seed.FootprintY)
                  + " in, " + Plural(reading.PlantaCount, "planta", "plantas") + " en el dibujo"
                : "sin planta semilla (" + Lower(reading.SeedProblem ?? "sin detalle") + ")";
            var site = reading.Site == null ? string.Empty
                : reading.Site.HasOutline
                    ? "; sitio de la capa " + reading.Site.Layer + ": contorno de " + Plural(reading.Site.Boundary.Count, "vértice", "vértices") + " y "
                      + Plural(reading.Site.Obstacles?.Count ?? 0, "obstáculo", "obstáculos")
                    : "; capa " + reading.Site.Layer + ": sin contorno";
            return seed + site;
        }

        /// <summary>The site of the reading when it is the one of <paramref name="layer"/> (a layer chosen after the reading is not read yet).</summary>
        public static WarehouseSiteFacts SiteFor(WarehouseReading reading, string layer)
        {
            var site = reading?.Site;
            if (site == null) return null;
            return string.Equals((site.Layer ?? string.Empty).Trim(), (layer ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase) ? site : null;
        }

        /// <summary>
        /// Correction 2 of the review: the references go to the current layer of the drawing (as RACKRELLENAR places them, without a layer
        /// of their own; the computation does not change). When it is the layer of the site, the next reading of the site takes them as
        /// obstacles; the page says it before «Colocar». Empty otherwise.
        /// </summary>
        public static string CurrentLayerWarning(WarehouseReading reading, string siteLayer)
        {
            var current = (reading?.CurrentLayer ?? string.Empty).Trim();
            var site = (siteLayer ?? string.Empty).Trim();
            if (current.Length == 0 || !string.Equals(current, site, StringComparison.OrdinalIgnoreCase)) return string.Empty;
            return "La capa actual del dibujo es «" + current + "», la del sitio: los racks se colocan en la capa actual, así que la próxima "
                   + "lectura del sitio los tomaría como obstáculos. Cambia la capa actual en AutoCAD antes de «Colocar».";
        }

        public static string SiteNotReadText(string layer) =>
            "Sin leer el sitio de la capa «" + (layer ?? string.Empty).Trim() + "»: pulsa «Volver a leer».";

        public static string UnknownKindText(string kind) =>
            "RackCad no reconoce el tipo de este rack («" + (kind ?? string.Empty) + "»): no se replica, como en RACKLAYOUT.";

        // ------------------------------------------------------------------ words

        /// <summary>«A1», «A1 y B1», «A1, B1 y C1», «A1, B1 y 3 más».</summary>
        public static string ListText(IReadOnlyList<string> items, int max)
        {
            var list = (items ?? Array.Empty<string>()).ToList();
            var shown = list.Take(Math.Max(1, max)).ToList();
            var rest = list.Count - shown.Count;
            if (rest > 0) return string.Join(", ", shown) + " y " + rest.ToString(CultureInfo.InvariantCulture) + " más";
            if (shown.Count <= 1) return string.Join(string.Empty, shown);
            return string.Join(", ", shown.Take(shown.Count - 1)) + " y " + shown[shown.Count - 1];
        }

        public static string Plural(int count, string one, string many) =>
            count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        /// <summary>A length as the page writes it: up to two decimals, a dot.</summary>
        public static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        /// <summary>A text of the Plugin (in lower case after «RackCad: ») as a sentence of the page.</summary>
        public static string Sentence(string text)
        {
            var t = (text ?? string.Empty).Trim();
            if (t.Length == 0) return t;
            return char.ToUpper(t[0], CultureInfo.CurrentCulture) + t.Substring(1);
        }

        private static string Lower(string text)
        {
            var t = (text ?? string.Empty).Trim();
            if (t.Length == 0) return t;
            return char.ToLower(t[0], CultureInfo.CurrentCulture) + t.Substring(1);
        }

        private static string DisplayName(string name) => string.IsNullOrWhiteSpace(name) ? "el rack" : name.Trim();

        // ------------------------------------------------------------------ numbers, as the classic windows read them (UiSupport)

        public static bool TryCount(string text, out int value) => LocalizedNumberParser.TryInteger(text, out value) && value >= 1;

        private static bool IsCount(string text) => TryCount(text, out _);

        private static bool IsLength(string text) => LocalizedNumberParser.TryDouble(text, out var value) && value >= 0.0;

        private static bool IsPositive(string text) => TryPositive(text, out _);

        private static bool TryPositive(string text, out double value) => LocalizedNumberParser.TryDouble(text, out value) && value > 0.0;

        /// <summary>A valid length (≥ 0); 0 when it is not one (the callers validate first).</summary>
        public static double Length(string text) => LocalizedNumberParser.TryDouble(text, out var value) && value >= 0.0 ? value : 0.0;
    }
}
