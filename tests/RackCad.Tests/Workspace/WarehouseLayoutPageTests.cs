#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Layout;
using RackCad.Application.VentanaRackCad;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// T5 al (I-64 A-14, line D; prototype «almacen», questions 1 to 11 accepted): the rules of the page «Almacén» of the RackCad window and the
/// wiring of its door, by their source. The computation is TODAY's (question 9): the grid is the planner of RACKLAYOUT over the measured
/// seed, its fit check the one of <c>ConfirmFits</c>, and the fill the auto-fill of RACKRELLENAR; the same rules decide what
/// RACKALMACENCOLOCAR places over the reading it makes again, which places nothing when the planta or the site changed (question 5).
/// </summary>
public class WarehouseLayoutPageTests
{
    // A planta of 48 × 96 in (the footprint of the tests of the classic windows), inserted at (100, 50) with its corner 2 in off its origin.
    private static WarehouseSeedFacts Seed(double originX = 100, double originY = 50, double rotation = 0, string hash = "H1", bool kindKnown = true,
        bool embedded = true, string handle = "2A1") =>
        new("RACK-1", "Selectivo 2", "selective", handle, "1F0", originX, originY, 48, 96, -2, -2, rotation, 1, 1, 1, hash, embedded, kindKnown, false);

    // The L of the tests of the auto-fill (500 × 400 with a 200 × 200 notch), with one column.
    private static WarehouseSiteFacts LSite(string layer = "RACKCAD_SITIO", params WarehouseObstacleFact[] obstacles) => new(
        layer, null,
        new[] { new Point2D(0, 0), new Point2D(500, 0), new Point2D(500, 200), new Point2D(300, 200), new Point2D(300, 400), new Point2D(0, 400) },
        obstacles, 0);

    private static WarehouseReading Reading(WarehouseSeedFacts? seed = null, WarehouseSiteFacts? site = null, int plantas = 1) =>
        new(1, null, "RACK-1", "Selectivo 2", plantas, seed ?? Seed(), null, site ?? LSite(), new[] { "0", "MUROS", "RACKCAD_SITIO" });

    private static WarehouseLayoutValues Values(Func<WarehouseLayoutValues, WarehouseLayoutValues>? change = null) =>
        change == null ? WarehouseLayoutValues.Defaults : change(WarehouseLayoutValues.Defaults);

    // ------------------------------------------------------------------ the fields

    [Fact]
    public void TheDefaultsAreTheValuesOfTheTwoClassicWindowsAndHaveNoErrors()
    {
        var d = WarehouseLayoutValues.Defaults;
        Assert.Equal(("4", "1", "96", "96", false, "6", false), (d.Rows, d.Cols, d.AisleRows, d.AisleCols, d.BackToBack, d.BackGap, d.Independent));
        Assert.Equal(("", "", "RACKCAD_SITIO", "0", "4", true), (d.BuildingX, d.BuildingY, d.Layer, d.WallClearance, d.ColumnClearance, d.TryRotated));
        Assert.Empty(WarehouseLayoutRules.FieldErrors(d));
        Assert.Null(WarehouseLayoutRules.GridSizeError(d));
    }

    [Fact]
    public void EachFieldSaysItsOwnErrorAndTheGapCountsOnlyWithItsSwitch()
    {
        var errors = WarehouseLayoutRules.FieldErrors(Values(v => v with
        {
            Rows = "0", Cols = "x", AisleRows = "-1", AisleCols = "", BackGap = "-3", WallClearance = "-1", ColumnClearance = "a",
        }));
        Assert.Equal(WarehouseLayoutRules.IntegerText, errors[WarehouseLayoutRules.FieldRows]);
        Assert.Equal(WarehouseLayoutRules.IntegerText, errors[WarehouseLayoutRules.FieldCols]);
        Assert.Equal(WarehouseLayoutRules.NonNegativeText, errors[WarehouseLayoutRules.FieldAisleRows]);
        Assert.Equal(WarehouseLayoutRules.NonNegativeText, errors[WarehouseLayoutRules.FieldAisleCols]);
        Assert.Equal(WarehouseLayoutRules.NonNegativeText, errors[WarehouseLayoutRules.FieldWallClearance]);
        Assert.Equal(WarehouseLayoutRules.NonNegativeText, errors[WarehouseLayoutRules.FieldColumnClearance]);
        Assert.False(errors.ContainsKey(WarehouseLayoutRules.FieldBackGap)); // dolor 7: without its switch the gap does not count

        var paired = WarehouseLayoutRules.FieldErrors(Values(v => v with { BackToBack = true, BackGap = "-3" }));
        Assert.Equal(WarehouseLayoutRules.NonNegativeText, paired[WarehouseLayoutRules.FieldBackGap]);

        // A comma is a decimal separator, as in the classic windows (LocalizedNumberParser).
        Assert.Empty(WarehouseLayoutRules.FieldErrors(Values(v => v with { AisleRows = "96,5" })));
    }

    [Fact]
    public void TheBuildingTakesItsTwoSidesOrNoneAndTheGridMoreThanOneRackAndAtMostAThousand()
    {
        var one = WarehouseLayoutRules.FieldErrors(Values(v => v with { BuildingX = "400" }));
        Assert.False(one.ContainsKey(WarehouseLayoutRules.FieldBuildingX));
        Assert.Equal(WarehouseLayoutRules.BothSidesText, one[WarehouseLayoutRules.FieldBuildingY]);
        var zero = WarehouseLayoutRules.FieldErrors(Values(v => v with { BuildingX = "0", BuildingY = "200" }));
        Assert.Equal(WarehouseLayoutRules.PositiveText, zero[WarehouseLayoutRules.FieldBuildingX]);

        Assert.Equal(WarehouseLayoutRules.OneRackText, WarehouseLayoutRules.GridSizeError(Values(v => v with { Rows = "1", Cols = "1" })));
        Assert.Equal(WarehouseLayoutRules.TooManyGridText, WarehouseLayoutRules.GridSizeError(Values(v => v with { Rows = "40", Cols = "26" })));
        Assert.Null(WarehouseLayoutRules.GridSizeError(Values(v => v with { Rows = "40", Cols = "25" })));
    }

    // ------------------------------------------------------------------ «Rejilla»: the planner of RACKLAYOUT

    [Fact]
    public void TheGridIsThePlannerOfRackLayoutFromTheSeedWithItsLabelsAndItsCount()
    {
        var values = Values(v => v with { Rows = "3", Cols = "2", BackToBack = true, BackGap = "6" });
        var a = WarehouseLayoutRules.Assess(Reading(), values, WarehouseLayoutMode.Grid);

        // RACKLAYOUT: Plan(footprint, rows, cols, aisles, seed origin, pairing, gap).
        var expected = WarehouseGridPlanner.Plan(48, 96, 3, 2, 96, 96, 100, 50, RowPairing.BackToBack, 6);
        Assert.True(a.CanPlace);
        Assert.Equal(5, a.Count);
        Assert.Equal(expected.Cells.Select(c => (c.X, c.Y, c.Label)), a.Grid!.Plan.Cells.Select(c => (c.X, c.Y, c.Label)));
        Assert.Equal((expected.TotalX, expected.TotalY), (a.Grid.Plan.TotalX, a.Grid.Plan.TotalY));
        Assert.Equal(new[] { "A1", "A2", "B1", "B2", "C1", "C2" }, a.Grid.Cells.Select(c => c.Label));
        Assert.Equal(WarehouseCellKind.Seed, a.Grid.Cells[0].Kind);
        Assert.All(a.Grid.Cells.Skip(1), c => Assert.Equal(WarehouseCellKind.New, c.Kind));
        Assert.False(a.Grid.FitChecked);

        // An odd last row of a back-to-back grid stays alone, and the page says it.
        Assert.Contains(a.Problems, p => p.Kind == WarehouseProblemKind.Info && p.Text == "Con 3 filas espalda con espalda, la fila C queda sola.");
    }

    [Fact]
    public void TheFitCheckIsTheOneOfRackLayoutOverABuildingAnchoredAtTheCornerOfTheSeed()
    {
        var values = Values(v => v with { Rows = "4", Cols = "2", BuildingX = "400", BuildingY = "200" });
        var a = WarehouseLayoutRules.Assess(Reading(), values, WarehouseLayoutMode.Grid);

        // ConfirmFits: a site of the building with its corner at the corner of the seed, every rack with the offset origin → corner.
        var seed = Seed();
        var plan = WarehouseGridPlanner.Plan(48, 96, 4, 2, 96, 96, 100, 50);
        var site = new WarehouseSite(400, 200, originX: seed.OriginX + seed.OffsetX, originY: seed.OriginY + seed.OffsetY);
        var classic = WarehouseFitChecker.Check(plan, 48, 96, site, seed.OffsetX, seed.OffsetY);
        var classicOut = classic.Violations.Select(v => v.Message.Split(' ')[2]).ToList(); // «El rack B1 se sale…»

        Assert.True(a.Grid!.FitChecked);
        Assert.Equal(classicOut, a.Grid.OutLabels);
        Assert.Equal(new[] { "A2", "B2", "C2", "D1", "D2" }, a.Grid.OutLabels);
        Assert.Equal(WarehouseCellKind.OutOfBuilding, a.Grid.Cells.Single(c => c.Label == "D1").Kind);
        Assert.Equal(WarehouseCellKind.New, a.Grid.Cells.Single(c => c.Label == "C1").Kind);
        Assert.Contains(a.Problems, p => p.Kind == WarehouseProblemKind.Error && p.Text == "Se salen del edificio: A2, B2, C2, D1 y D2.");
        Assert.True(a.CanPlace); // the question «Colocar de todos modos» is the page's; the plan says what leaves
    }

    [Fact]
    public void AnUnknownKindOrAnIndependentCopyWithoutDataBlocksTheGridAsRackLayoutDoes()
    {
        var unknown = WarehouseLayoutRules.Assess(Reading(Seed(kindKnown: false)), Values(), WarehouseLayoutMode.Grid);
        Assert.False(unknown.CanPlace);
        Assert.Contains("no reconoce el tipo", unknown.BlockedReason);

        var notEmbedded = Reading(Seed(embedded: false));
        Assert.True(WarehouseLayoutRules.Assess(notEmbedded, Values(), WarehouseLayoutMode.Grid).CanPlace);
        var independent = WarehouseLayoutRules.Assess(notEmbedded, Values(v => v with { Independent = true }), WarehouseLayoutMode.Grid);
        Assert.Equal(WarehouseLayoutRules.NoEmbeddedDesignText, independent.BlockedReason);

        // RACKRELLENAR does not gate on the kind (inventory, dolor 12): the computation of today.
        Assert.True(WarehouseLayoutRules.Assess(Reading(Seed(kindKnown: false)), Values(), WarehouseLayoutMode.Fill).CanPlace);
    }

    // ------------------------------------------------------------------ «Rellenar un área»: the auto-fill of RACKRELLENAR

    [Fact]
    public void TheFillIsTheAutoFillOfRackRellenarAndTheSchemeHasEveryCandidate()
    {
        var column = new WarehouseObstacleFact(150, 20, 12, 12, "columna 1");
        var reading = Reading(site: LSite(obstacles: column));
        var values = Values(v => v with { AisleRows = "24", AisleCols = "0", TryRotated = false });
        var a = WarehouseLayoutRules.Assess(reading, values, WarehouseLayoutMode.Fill);

        var site = WarehouseSite.FromBoundary(reading.Site!.Boundary, 0, 0, new[] { new SiteObstacle(150, 20, 12, 12, 4, "columna 1") });
        var classic = WarehouseAutoFill.Fill(site, 48, 96, 24, 0, RowPairing.Single, 0, tryRotated: false);
        Assert.Equal(classic.Cells.Select(c => (c.X, c.Y, c.Label)), a.Fill!.Result.Cells.Select(c => (c.X, c.Y, c.Label)));
        Assert.Equal(classic.Cells.Count, a.Count);
        Assert.True(a.CanPlace);

        // Every candidate is in the scheme: kept + outside + on a column = rows tried × columns tried, with the labels of the grid.
        var r = a.Fill.Result;
        Assert.Equal(r.RowsTried * r.ColsTried, a.Fill.Cells.Count);
        Assert.Equal(r.Cells.Count, a.Fill.Cells.Count(c => c.Kind == WarehouseCellKind.New));
        Assert.Equal(r.OmittedOutside, a.Fill.Cells.Count(c => c.Kind == WarehouseCellKind.Outside));
        Assert.Equal(r.OmittedByObstacle, a.Fill.Cells.Count(c => c.Kind == WarehouseCellKind.OnColumn));
        Assert.True(r.OmittedOutside > 0);
        Assert.Equal(new[] { "C1" }, a.Fill.OnColumnLabels);
        Assert.Contains(a.Problems, p => p.Kind == WarehouseProblemKind.Warning && p.Text == "Queda fuera por una columna: C1.");
        Assert.Contains(a.Problems, p => p.Kind == WarehouseProblemKind.Info && p.Text.StartsWith("Las etiquetas conservan su fila y su columna", StringComparison.Ordinal));
    }

    [Fact]
    public void TheOmittedCellsOfTheAutoFillChangeNothingOfWhatItKeeps()
    {
        // T5 al adds the omitted cells to the result; the cells kept, the counts and the orientation are what they were.
        var site = WarehouseSite.FromBoundary(new[]
        {
            new Point2D(0, 0), new Point2D(500, 0), new Point2D(500, 200), new Point2D(300, 200), new Point2D(300, 400), new Point2D(0, 400),
        });
        var fill = WarehouseAutoFill.Fill(site, 40, 100, 96, 12, tryRotated: false);
        Assert.Equal(12, fill.RowsTried * fill.ColsTried);
        Assert.Equal(8, fill.Cells.Count); // Fill_LShapedSite_DropsTheCellsInTheNotch: the four cells of the notch drop
        Assert.Equal(4, fill.OmittedOutside);
        Assert.Equal(4, fill.Omitted.Count);
        Assert.All(fill.Omitted, o => Assert.Equal(FitViolationKind.OutOfBounds, o.Reason));
        Assert.Empty(fill.Cells.Select(c => c.Label).Intersect(fill.Omitted.Select(o => o.Cell.Label)));
    }

    [Fact]
    public void TheFillSaysWhatTheClassicSaidLaterAndWhatThrewAsAnErrorOfAutoCad()
    {
        // More than 5000 racks: the auto-fill throws; the page says it in its place (dolor 12).
        var huge = Reading(site: new WarehouseSiteFacts("RACKCAD_SITIO", null,
            new[] { new Point2D(0, 0), new Point2D(100000, 0), new Point2D(100000, 100000), new Point2D(0, 100000) }, Array.Empty<WarehouseObstacleFact>(), 0));
        var tooMany = WarehouseLayoutRules.Assess(huge, Values(v => v with { AisleRows = "0", AisleCols = "0" }), WarehouseLayoutMode.Fill);
        Assert.Equal(WarehouseLayoutRules.TooManyFillText, tooMany.BlockedReason);
        Assert.Equal(WarehouseLayoutRules.TooManyFillText, tooMany.FillError);

        // A site too small: nothing fits, with the counts of the classic message.
        var tiny = Reading(site: new WarehouseSiteFacts("RACKCAD_SITIO", null,
            new[] { new Point2D(0, 0), new Point2D(40, 0), new Point2D(40, 40), new Point2D(0, 40) }, Array.Empty<WarehouseObstacleFact>(), 0));
        var none = WarehouseLayoutRules.Assess(tiny, Values(), WarehouseLayoutMode.Fill);
        Assert.Equal(WarehouseLayoutRules.NothingFitsText, none.BlockedReason);
        Assert.Contains(none.Problems, p => p.Kind == WarehouseProblemKind.Error && p.Text.StartsWith("No cabe ningún rack en el área: rejilla probada 0 × 0", StringComparison.Ordinal));

        // A layer without outline, and a layer chosen after the reading (not read yet).
        var noOutline = Reading(site: new WarehouseSiteFacts("MUROS", "No hay entidades en la capa 'MUROS'.", null, Array.Empty<WarehouseObstacleFact>(), 0));
        Assert.Equal(WarehouseLayoutRules.NoOutlineText, WarehouseLayoutRules.Assess(noOutline, Values(v => v with { Layer = "MUROS" }), WarehouseLayoutMode.Fill).BlockedReason);
        var other = WarehouseLayoutRules.Assess(Reading(), Values(v => v with { Layer = "MUROS" }), WarehouseLayoutMode.Fill);
        Assert.Equal(WarehouseLayoutRules.SiteNotReadText("MUROS"), other.BlockedReason);
        Assert.True(other.FillHasErrors);
        Assert.False(other.GridHasErrors);
    }

    [Fact]
    public void TheSeedInsideTheAreaIsSaidBeforePlacing()
    {
        Assert.True(WarehouseLayoutRules.Assess(Reading(Seed(100, 50)), Values(), WarehouseLayoutMode.Fill).Fill!.SeedInside);
        Assert.False(WarehouseLayoutRules.Assess(Reading(Seed(900, 900)), Values(), WarehouseLayoutMode.Fill).Fill!.SeedInside);
    }

    // ------------------------------------------------------------------ «Colocar» (question 5)

    [Fact]
    public void TheMarkFollowsThePlantaAndToFillTheSiteToo()
    {
        var reading = Reading();
        var grid = WarehouseLayoutRules.Fingerprint(reading, WarehouseLayoutMode.Grid);
        Assert.Equal(grid, WarehouseLayoutRules.Fingerprint(Reading(), WarehouseLayoutMode.Grid));
        Assert.NotEqual(grid, WarehouseLayoutRules.Fingerprint(Reading(Seed(originX: 101)), WarehouseLayoutMode.Grid));
        Assert.NotEqual(grid, WarehouseLayoutRules.Fingerprint(Reading(Seed(rotation: Math.PI / 2)), WarehouseLayoutMode.Grid));
        Assert.NotEqual(grid, WarehouseLayoutRules.Fingerprint(Reading(Seed(hash: "H2")), WarehouseLayoutMode.Grid)); // its design changed
        Assert.NotEqual(grid, WarehouseLayoutRules.Fingerprint(Reading(Seed(handle: "2A2")), WarehouseLayoutMode.Grid)); // another planta

        // The grid does not read the site; the fill does.
        var moved = Reading(site: LSite(obstacles: new WarehouseObstacleFact(10, 10, 12, 12, "columna 1")));
        Assert.Equal(grid, WarehouseLayoutRules.Fingerprint(moved, WarehouseLayoutMode.Grid));
        Assert.NotEqual(WarehouseLayoutRules.Fingerprint(reading, WarehouseLayoutMode.Fill), WarehouseLayoutRules.Fingerprint(moved, WarehouseLayoutMode.Fill));
        Assert.Null(WarehouseLayoutRules.Fingerprint(reading with { Seed = null }, WarehouseLayoutMode.Grid));
    }

    [Fact]
    public void ColocarPlacesWhatTheSchemeShowedAndNothingWhenTheDrawingChanged()
    {
        var reading = Reading();
        var values = Values(v => v with { Rows = "2", Cols = "3" });
        var request = new WarehousePlaceRequest(9, default, WarehouseLayoutMode.Grid, "RACK-1", "2A1", "RACKCAD_SITIO",
            WarehouseLayoutRules.Fingerprint(reading, WarehouseLayoutMode.Grid)!, values, false);

        var same = WarehouseLayoutRules.PlanPlace(request, Reading());
        Assert.True(same.CanPlace);
        Assert.Equal(WarehouseLayoutRules.Assess(reading, values, WarehouseLayoutMode.Grid).Grid!.Plan.Cells.Select(c => (c.X, c.Y)),
            same.Assessment!.Grid!.Plan.Cells.Select(c => (c.X, c.Y)));

        var changed = WarehouseLayoutRules.PlanPlace(request, Reading(Seed(originX: 130)));
        Assert.False(changed.CanPlace);
        Assert.True(changed.DrawingChanged);
        Assert.Equal("No se colocó nada: la planta de Selectivo 2 cambió en el dibujo desde la lectura. La página la volvió a leer: revisa el esquema y vuelve a pulsar «Colocar».", changed.Reason);

        var fillRequest = request with { Mode = WarehouseLayoutMode.Fill, Fingerprint = WarehouseLayoutRules.Fingerprint(reading, WarehouseLayoutMode.Fill)! };
        var siteMoved = WarehouseLayoutRules.PlanPlace(fillRequest, Reading(site: LSite(obstacles: new WarehouseObstacleFact(10, 10, 12, 12, "columna 1"))));
        Assert.True(siteMoved.DrawingChanged);
        Assert.Contains("o el sitio cambiaron", siteMoved.Reason);

        var noSeed = WarehouseLayoutRules.PlanPlace(request, reading with { Seed = null, SeedProblem = "La planta semilla ya no está." });
        Assert.True(noSeed.DrawingChanged);
    }

    [Fact]
    public void AGridThatLeavesItsBuildingIsPlacedOnlyWhenTheUserSaidSoAndInvalidValuesNever()
    {
        var reading = Reading();
        var outside = Values(v => v with { Rows = "4", Cols = "2", BuildingX = "400", BuildingY = "200" });
        var mark = WarehouseLayoutRules.Fingerprint(reading, WarehouseLayoutMode.Grid)!;
        var request = new WarehousePlaceRequest(3, default, WarehouseLayoutMode.Grid, "RACK-1", "2A1", "RACKCAD_SITIO", mark, outside, false);

        var asked = WarehouseLayoutRules.PlanPlace(request, reading);
        Assert.False(asked.CanPlace);
        Assert.False(asked.DrawingChanged);
        Assert.StartsWith("No se colocó nada: la rejilla no cabe en el edificio (A2, B2", asked.Reason, StringComparison.Ordinal);
        Assert.True(WarehouseLayoutRules.PlanPlace(request with { PlaceAnyway = true }, reading).CanPlace);

        var invalid = WarehouseLayoutRules.PlanPlace(request with { Values = Values(v => v with { Rows = "0" }) }, reading);
        Assert.False(invalid.CanPlace);
        Assert.Equal("No se colocó nada: corrige lo marcado en «Rejilla», «Pasillos» o «Verificar encaje».", invalid.Reason);
    }

    [Fact]
    public void TheTextsOfThePlacementAreTheOnesOfTheClassicCommandsAndOfThePage()
    {
        var grid = WarehouseLayoutRules.Assess(Reading(), Values(v => v with { Rows = "2", Cols = "3", BackToBack = true }), WarehouseLayoutMode.Grid).Grid;
        Assert.Equal("RackCad: layout colocado — 5 racks nuevos (2 filas × 3 columnas, enlazados, espalda-con-espalda).",
            WarehouseLayoutRules.CommandLineText(WarehouseLayoutMode.Grid, 5, grid, independent: false));
        Assert.Equal("RackCad: relleno colocado — 12 racks enlazados + etiquetas.", WarehouseLayoutRules.CommandLineText(WarehouseLayoutMode.Fill, 12, null, false));
        Assert.Equal("Layout colocado: 5 racks nuevos (2 × 3, independientes, espalda con espalda) y 6 etiquetas en la capa RACKCAD_LAYOUT. Un «U» en AutoCAD lo deshace. Las copias independientes salen en la lista al «Volver a leer la lista».",
            WarehouseLayoutRules.PlacedText(WarehouseLayoutMode.Grid, 5, grid, independent: true));
        Assert.Equal("Relleno colocado: 1 rack enlazado y sus etiquetas en la capa RACKCAD_LAYOUT. Un «U» en AutoCAD lo deshace.",
            WarehouseLayoutRules.PlacedText(WarehouseLayoutMode.Fill, 1, null, false));
        Assert.Equal("A1, B1 y C1", WarehouseLayoutRules.ListText(new[] { "A1", "B1", "C1" }, 12));
        Assert.Equal("A1, B1 y 2 más", WarehouseLayoutRules.ListText(new[] { "A1", "B1", "C1", "D1" }, 2));
    }

    [Fact]
    public void TheCurrentLayerOfTheDrawingOnTheLayerOfTheSiteIsSaidBeforeColocar()
    {
        // Correction 2 of the review: the references go to the current layer, and on the layer of the site the next reading takes them
        // as obstacles. The computation does not change; the page says it.
        var onSite = Reading() with { CurrentLayer = "rackcad_sitio" };
        Assert.StartsWith("La capa actual del dibujo es «rackcad_sitio», la del sitio:", WarehouseLayoutRules.CurrentLayerWarning(onSite, "RACKCAD_SITIO"), StringComparison.Ordinal);
        Assert.Equal(string.Empty, WarehouseLayoutRules.CurrentLayerWarning(Reading() with { CurrentLayer = "0" }, "RACKCAD_SITIO"));
        Assert.Equal(string.Empty, WarehouseLayoutRules.CurrentLayerWarning(Reading(), "RACKCAD_SITIO"));
        Assert.Equal(WarehouseLayoutRules.Fingerprint(Reading(), WarehouseLayoutMode.Fill), WarehouseLayoutRules.Fingerprint(onSite, WarehouseLayoutMode.Fill));

        // And the command reads it with the layers.
        Assert.Contains("document.Database.Clayer", WorkspaceHostSource.Code(Commands));
    }

    // ------------------------------------------------------------------ the door, by its source

    private const string Commands = "src/RackCad.Plugin/RackLayoutCommands.Ventana.cs";

    private static void InOrder(string body, params string[] steps)
    {
        var at = -1;
        foreach (var step in steps)
        {
            var next = body.IndexOf(step, at + 1, StringComparison.Ordinal);
            Assert.True(next > at, "out of order or missing: " + step);
            at = next;
        }
    }

    private static string Body(string code, string signature)
    {
        var start = code.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, signature);
        var next = code.IndexOf("[CommandMethod(", start + signature.Length, StringComparison.Ordinal);
        var helpers = new[] { "private static WarehouseReading ReadWarehouse(", "private static WarehouseReading AfterPlacing(" }
            .Select(h => code.IndexOf(h, start, StringComparison.Ordinal)).Where(i => i > start).DefaultIfEmpty(-1).Min();
        var end = new[] { next, helpers }.Where(i => i > start).DefaultIfEmpty(code.Length).Min();
        return code.Substring(start, end - start);
    }

    [Fact]
    public void RackAlmacenLeerReadsWithNoUndoMarkerKeepsTheSelectionAndAnswersItsRequest()
    {
        var code = WorkspaceHostSource.Code(Commands);
        Assert.Contains("private const CommandFlags WarehouseReadFlags = CommandFlags.NoUndoMarker | CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.NoHistory;", code);
        Assert.Contains("[CommandMethod(\"RACKALMACENLEER\", WarehouseReadFlags)]", code);
        InOrder(Body(code, "public void RackAlmacenLeer()"),
            "RackCadRackDoorPort.TakePendingWarehouseRead()", "door?.Started(pending.RequestId);", "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "ReadWarehouse(document, request.RequestId", "useSelection: true", "door?.AnswerWarehouseRead(reading)");

        // The seed is measured with the measure of RACKLAYOUT, and the site is read as RACKRELLENAR reads it.
        Assert.Contains("seed = SeedOf(transaction, chosen);", code);
        Assert.Contains("ReadSiteFromLayer(document, layerName, 0.0,", code);
        Assert.Contains("editor.SelectImplied()", code);
        Assert.DoesNotContain("OpenMode.ForWrite", code); // the door never writes by itself: the halves of the classic commands do
        Assert.DoesNotContain("AppendEntity", code);
        Assert.DoesNotContain("new BlockReference", code);

        // ReadPlantaSeed of RACKLAYOUT uses the same measure (extracted, its body unchanged).
        var layout = WorkspaceHostSource.Code("src/RackCad.Plugin/RackLayoutCommands.cs");
        Assert.Contains("seed = SeedOf(transaction, reference);", layout);
        Assert.Contains("private static PlantaSeed SeedOf(Transaction transaction, BlockReference reference)", layout);
    }

    [Fact]
    public void RackAlmacenColocarReadsAgainComparesAndPlacesWithTheHalvesOfTheClassicCommands()
    {
        var code = WorkspaceHostSource.Code(Commands);
        // Correction 4 of the review: «Colocar» keeps the selection (the reading it gives when the drawing changed sees it, AL-S2), and it
        // keeps its undo marker (no NoUndoMarker): one «U» undoes it.
        Assert.Contains("private const CommandFlags WarehousePlaceFlags = CommandFlags.UsePickSet | CommandFlags.Redraw;", code);
        Assert.Contains("[CommandMethod(\"RACKALMACENCOLOCAR\", WarehousePlaceFlags)]", code);
        var body = Body(code, "public void RackAlmacenColocar()");
        InOrder(body,
            "RackCadRackDoorPort.TakePendingWarehousePlace()", "door?.Started(pending.RequestId);", "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "onlyPreferred: true", "WarehouseLayoutRules.PlanPlace(request, fresh)", "if (!plan.CanPlace)", "useSelection: true", "door?.AnswerWarehousePlace(",
            "RackUnitsGuard.WarnIfNotInches(document);", "PlaceGrid(document, plan.Assessment.Grid.Plan, seed, embed, request.Values.Independent, regen: false)",
            "PlaceFill(document, plan.Assessment.Fill.Result, seed, regen: false)", "document.Editor.Regen();", "after = AfterPlacing(",
            "door?.AnswerWarehousePlace(new WarehousePlaced(request.RequestId, true,");

        // Correction 1: once placed, nothing answers «no se colocó»: the regeneration and the reading after it have their own try, and the
        // only answer that says false after the placement is gone.
        var placedAt = body.IndexOf("regen: false)", StringComparison.Ordinal);
        Assert.DoesNotContain("new WarehousePlaced(request.RequestId, false", body.Substring(body.IndexOf("document.Editor.Regen();", placedAt, StringComparison.Ordinal)));
        Assert.Matches(new System.Text.RegularExpressions.Regex(@"try\s*\{\s*document\.Editor\.Regen\(\);\s*\}\s*catch"), body);
        Assert.Matches(new System.Text.RegularExpressions.Regex(@"try\s*\{\s*after = AfterPlacing\([^;]*;\s*\}\s*catch"), body);
        Assert.Contains("after = WarehouseReading.Failed(", body);
        foreach (var half in new[] { "src/RackCad.Plugin/RackLayoutCommands.cs", "src/RackCad.Plugin/RackLayoutCommands.Fill.cs" })
            Assert.Contains("if (regen) document.Editor.Regen();", WorkspaceHostSource.Code(half)); // RACKLAYOUT and RACKRELLENAR regenerate as before

        // Correction 3: the fresh read of the grid reads no site and no read of «Colocar» reads the layers; after placing, the block table
        // is not scanned again (one FindRackBlocks in the file, the one of the seed).
        Assert.Contains("readSite: !grid, readLayers: false", body);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(code, @"RackCommandSupport\.FindRackBlocks\("));
        var after = code.Substring(code.IndexOf("private static WarehouseReading AfterPlacing(", StringComparison.Ordinal));
        after = after.Substring(0, after.IndexOf("private static WarehouseReading ReadWarehouse(", StringComparison.Ordinal));
        Assert.DoesNotContain("ReadSeedFacts(", after);
        Assert.Contains("PlantaCount = fresh.PlantaCount + (independent ? 0 : placed)", after);
    }

    [Fact]
    public void TheDoorOfThePageKeepsItsRequestsAndDropsThemWithDejarDeEsperar()
    {
        var port = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.Warehouse.cs");
        Assert.Contains("return Send(WarehouseLayoutRules.ReadCommand + \" \",", port);
        Assert.Contains("return Send(WarehouseLayoutRules.PlaceCommand + \" \",", port);
        Assert.Contains("private static void AbandonWarehouse(long requestId)", port);
        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"LockDocument|StartTransaction|\[CommandMethod"), port);
        Assert.Contains("AbandonWarehouse(requestId);", WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs"));
        Assert.Equal("RACKALMACENLEER", WarehouseLayoutRules.ReadCommand);
        Assert.Equal("RACKALMACENCOLOCAR", WarehouseLayoutRules.PlaceCommand);
    }
}
