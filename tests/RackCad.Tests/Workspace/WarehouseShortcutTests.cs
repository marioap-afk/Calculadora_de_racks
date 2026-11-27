#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Geometry;
using RackCad.Application.VentanaRackCad;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// T6 rl (I-64, AL-S14; answer of the Owner, 2026-11-27: «La página «Almacén»»): the shortcuts RLY and RR open the page «Almacén» of the
/// RackCad window, RLY on «Rejilla» and RR on «Rellenar un área», with the rack selected in AutoCAD as the seed, and read the drawing inside
/// their own command; the full commands RACKLAYOUT and RACKRELLENAR keep their classic windows. The rules are pure; the commands, by their
/// source (the Plugin needs AutoCAD).
/// </summary>
public class WarehouseShortcutTests
{
    private static WarehouseReading Reading(int requestId = 1) => new(
        requestId, null, "RACK-1", "Selectivo 2", 1,
        new WarehouseSeedFacts("RACK-1", "Selectivo 2", "selective", "2A1", "1F0", 100, 50, 48, 96, -2, -2, 0, 1, 1, 1, "H1", true, true, false),
        null,
        new WarehouseSiteFacts("RACKCAD_SITIO", null,
            new[] { new Point2D(0, 0), new Point2D(500, 0), new Point2D(500, 200), new Point2D(300, 200), new Point2D(300, 400), new Point2D(0, 400) },
            new[] { new WarehouseObstacleFact(140, 90, 12, 12, "columna 1") }, 0),
        new[] { "0", "RACKCAD_SITIO" });

    // ------------------------------------------------------------------ the seed the selection designates

    [Fact]
    public void TheSeedIsTheOneRackSelectedAndSeveralRacksDesignateNoneAndSaySo()
    {
        Assert.Null(WarehouseLayoutRules.DesignatedSeed(null!, WarehouseLayoutMode.Grid, out var none));
        Assert.Null(none);
        Assert.Null(WarehouseLayoutRules.DesignatedSeed(Array.Empty<WarehouseSelectedRack>(), WarehouseLayoutMode.Grid, out none));
        Assert.Null(none);

        // One rack, by any of its views (its frontal and its planta, the id in any case): that rack.
        var one = new[] { new WarehouseSelectedRack("RACK-1", "Selectivo 2", false), new WarehouseSelectedRack(" rack-1 ", "Selectivo 2", true), new WarehouseSelectedRack(" ", "x", true) };
        Assert.Equal("RACK-1", WarehouseLayoutRules.DesignatedSeed(one, WarehouseLayoutMode.Grid, out var note));
        Assert.Null(note);

        // Two racks: none, and the page says it (with the shortcut that was typed).
        var two = new[] { new WarehouseSelectedRack("RACK-1", "Selectivo 2", true), new WarehouseSelectedRack("PB-1", "Push Back 1", false) };
        Assert.Null(WarehouseLayoutRules.DesignatedSeed(two, WarehouseLayoutMode.Fill, out note));
        Assert.Equal("Hay 2 racks seleccionados en AutoCAD: RR no tomó ninguno como semilla y la página sigue con la suya. Para usar otro, déjalo seleccionado solo a él y vuelve a escribir RR, o elígelo en «Rack semilla».", note);
        Assert.Null(WarehouseLayoutRules.DesignatedSeed(two, WarehouseLayoutMode.Grid, out note));
        Assert.StartsWith("Hay 2 racks seleccionados en AutoCAD: RLY no tomó ninguno", note, StringComparison.Ordinal);

        Assert.Equal(("RLY", "RR"), (WarehouseLayoutRules.GridShortcut, WarehouseLayoutRules.FillShortcut));
        Assert.Equal(("Rejilla", "Rellenar un área"), (WarehouseLayoutRules.TabName(WarehouseLayoutMode.Grid), WarehouseLayoutRules.TabName(WarehouseLayoutMode.Fill)));
    }

    [Fact]
    public void TheAskTrimsWhatTheCommandGivesAndStartsWithNothingForTheWindowToAnswer()
    {
        var ask = new WarehousePageAsk(WarehouseLayoutMode.Fill, "  PB-1 ", "   ");
        Assert.Equal((WarehouseLayoutMode.Fill, "PB-1", (string?)null), (ask.Mode, ask.DesignatedRackId, ask.DesignationNote));
        Assert.False(ask.Taken);
        Assert.Null(ask.Read);
        Assert.Null(ask.Note);
        Assert.Null(new WarehousePageAsk(WarehouseLayoutMode.Grid, " ", null!).DesignatedRackId);
    }

    // ------------------------------------------------------------------ the command line

    [Fact]
    public void TheCommandLineSaysThePageAndTabAndWhatTheShortcutReadOrWhyNothing()
    {
        Assert.Equal("RackCad: «Almacén» › «Rejilla» en la ventana RackCad: planta de «Selectivo 2» leída, 48 × 96 in, 1 planta en el dibujo; sitio de la capa RACKCAD_SITIO: contorno de 6 vértices y 1 obstáculo.",
            WarehouseLayoutRules.ShortcutCommandLineText(WarehouseLayoutMode.Grid, Reading(0), null!));
        Assert.Equal("RackCad: «Almacén» › «Rejilla» en la ventana RackCad: planta de «Selectivo 2» leída, 48 × 96 in, 1 planta en el dibujo; sitio de la capa RACKCAD_SITIO: contorno de 6 vértices y 1 obstáculo. Hay 2 racks seleccionados.",
            WarehouseLayoutRules.ShortcutCommandLineText(WarehouseLayoutMode.Grid, Reading(0), "Hay 2 racks seleccionados."));
        Assert.Equal("RackCad: «Almacén» › «Rellenar un área» en la ventana RackCad: no se pudo leer el dibujo: fallo.",
            WarehouseLayoutRules.ShortcutCommandLineText(WarehouseLayoutMode.Fill, WarehouseReading.Failed(0, "RACK-1", "No se pudo leer el dibujo: fallo."), null!));
        Assert.Equal("RackCad: «Almacén» › «Rellenar un área» en la ventana RackCad: sin planta semilla (este rack no tiene vista en planta.).",
            WarehouseLayoutRules.ShortcutCommandLineText(WarehouseLayoutMode.Fill, Reading(0) with { Seed = null, SeedProblem = "Este rack no tiene vista en planta.", Site = null }, null!));
        Assert.Equal("RackCad: «Almacén» › «Rellenar un área» en la ventana RackCad: «Almacén» espera a AutoCAD (lee la planta o coloca): cuando termine, vuelve a escribir RR o pulsa «Volver a leer».",
            WarehouseLayoutRules.ShortcutCommandLineText(WarehouseLayoutMode.Fill, null!, WarehouseLayoutRules.ShortcutWaitText(WarehouseLayoutMode.Fill)));
        Assert.Equal("RackCad: «Almacén» › «Rejilla» en la ventana RackCad: elige el rack semilla: los racks con planta del dibujo.",
            WarehouseLayoutRules.ShortcutCommandLineText(WarehouseLayoutMode.Grid, null!, "Elige el rack semilla: los racks con planta del dibujo."));
        Assert.Equal("RackCad: «Almacén» › «Rejilla» en la ventana RackCad.", WarehouseLayoutRules.ShortcutCommandLineText(WarehouseLayoutMode.Grid, null!, null!));
    }

    [Fact]
    public void TheLineOfRackAlmacenLeerIsTheSameAfterItsSummaryIsShared()
    {
        Assert.Equal("RackCad: planta de «Selectivo 2» leída, 48 × 96 in, 1 planta en el dibujo; sitio de la capa RACKCAD_SITIO: contorno de 6 vértices y 1 obstáculo (RACKALMACENLEER, petición n.º 1).",
            WarehouseLayoutRules.ReadCommandLineText(Reading()));
        Assert.Equal("RackCad: sin planta semilla (sin detalle); capa MUROS: sin contorno (RACKALMACENLEER, petición n.º 3).",
            WarehouseLayoutRules.ReadCommandLineText(Reading(3) with { Seed = null, Site = new WarehouseSiteFacts("MUROS", "Sin contorno.", null, Array.Empty<WarehouseObstacleFact>(), 0) }));
        Assert.Equal("RackCad: no se pudo leer el dibujo: fallo. (RACKALMACENLEER, petición n.º 2)",
            WarehouseLayoutRules.ReadCommandLineText(WarehouseReading.Failed(2, "RACK-1", "No se pudo leer el dibujo: fallo.")));
        Assert.Equal("RackCad: no se leyó nada para la ventana RackCad.", WarehouseLayoutRules.ReadCommandLineText(null!));
    }

    // ------------------------------------------------------------------ the commands, by their source

    private const string Window = "src/RackCad.Plugin/RackLayoutCommands.Ventana.cs";

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

    [Fact]
    public void RlyAndRrOpenThePageOnTheirTabAndTheFullCommandsKeepTheirClassicWindows()
    {
        var window = WorkspaceHostSource.Code(Window);
        Assert.Contains("private const CommandFlags WarehouseShortcutFlags = CommandFlags.NoUndoMarker | CommandFlags.UsePickSet | CommandFlags.Redraw;", window);
        Assert.Contains("[CommandMethod(\"RLY\", WarehouseShortcutFlags)] public void AliasRackLayout() => OpenWarehousePage(WarehouseLayoutMode.Grid);", window);
        Assert.Contains("[CommandMethod(\"RR\", WarehouseShortcutFlags)] public void AliasRackRellenar() => OpenWarehousePage(WarehouseLayoutMode.Fill);", window);

        // The full commands are the only registrations of their files, and they still open their classic windows.
        var layout = WorkspaceHostSource.Code("src/RackCad.Plugin/RackLayoutCommands.cs");
        var fill = WorkspaceHostSource.Code("src/RackCad.Plugin/RackLayoutCommands.Fill.cs");
        Assert.Equal(new[] { "RACKLAYOUT" }, Regex.Matches(layout, @"\[CommandMethod\(""(?<name>[^""]*)""").Select(m => m.Groups["name"].Value));
        Assert.Equal(new[] { "RACKRELLENAR" }, Regex.Matches(fill, @"\[CommandMethod\(""(?<name>[^""]*)""").Select(m => m.Groups["name"].Value));
        Assert.Contains("new RackWarehouseLayoutWindow(embed.Name, seed.FootprintX, seed.FootprintY);", layout);
        Assert.Contains("new RackWarehouseFillWindow(embed.Name, seed.FootprintX, seed.FootprintY);", fill);
        Assert.DoesNotMatch(@"\bRackLayout\(\)", window); // the shortcuts no longer call the classic bodies
        Assert.DoesNotMatch(@"\bRackRellenar\(\)", window);
    }

    [Fact]
    public void TheShortcutReadsInsideItsOwnCommandWhatThePageAskedAndSendsNoSecondCommand()
    {
        var window = WorkspaceHostSource.Code(Window);
        var open = WorkspaceHostSource.BodyOf(window, @"private static void OpenWarehousePage\(WarehouseLayoutMode mode\)");
        InOrder(open,
            "WarehouseLayoutRules.DesignatedSeed(SelectedRacks(document), mode, out var note);",
            "Workspace.WorkspacePanelCommand.ShowRackCadWindow();",
            "var ask = new WarehousePageAsk(mode, designated, note);",
            "door?.AskWarehousePage(ask);",
            "if (door == null || !ask.Taken)",
            "if (ask.Read != null)",
            "reading = ReadWarehouse(document, 0, ask.Read.SeedRackId, ask.Read.Layer, ask.Read.PreferredSeedHandle,",
            "useSelection: true, onlyPreferred: false, out _, out _);",
            "door.AnswerWarehouseShortcut(ask, reading);",
            "WarehouseLayoutRules.ShortcutCommandLineText(mode, reading, ask.Note ?? note)");

        // No second command: no request of the door, nothing handed to AutoCAD; no write and no warning of the units (that is «Colocar»).
        foreach (var forbidden in new[] { "RequestWarehouseRead", "RequestWarehousePlace", "Send(", "SendStringToExecute", "RackUnitsGuard", "PlaceGrid", "PlaceFill", "Started(" })
            Assert.DoesNotContain(forbidden, open);
        Assert.DoesNotContain("SendStringToExecute", window);

        // The selection is read as RACKLAYOUT reads the rack it picks (the data of the definition), for reading only.
        var selected = WorkspaceHostSource.BodyOf(window, @"internal static List<WarehouseSelectedRack> SelectedRacks\(Document document\)");
        InOrder(selected, "SelectedIds(document.Editor)", "OpenMode.ForRead", "reference.BlockTableRecord", "RackBlockData.Read(transaction, definitionId)",
            "RackCommandSupport.IsPlantaView(embed)", "transaction.Commit();");
        Assert.Contains("catch (System.Exception ex)", selected);

        // The door raises the two events of the window and holds nothing of AutoCAD.
        var port = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.Warehouse.cs");
        Assert.Contains("internal void AskWarehousePage(WarehousePageAsk ask) => Raise(() => WarehousePageAsked?.Invoke(ask));", port);
        Assert.Contains("internal void AnswerWarehouseShortcut(WarehousePageAsk ask, WarehouseReading reading) => Raise(() => WarehouseShortcutRead?.Invoke(ask, reading));", port);
    }
}
