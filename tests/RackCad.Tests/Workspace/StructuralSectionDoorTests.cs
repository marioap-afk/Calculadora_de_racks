#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// T7 per (I-64, séptima tanda; the Owner, 2026-11-27: «Igual que la clásica» and «Abren la ventana»): the door of the page «Perfil
/// estructural» of the RackCad window. RACKSECCION opens the page (the pattern of the shortcuts of the sixth batch); «Insertar en el dibujo…»
/// is RACKSECCIONINSERTAR, which draws the approved plan with the materialisation of RACKSECCION itself (StructuralSectionCommandFlow.Place:
/// units advisory, point first, one transaction, one regen, the same report), so one «U» undoes it. The classic inspector stays behind
/// «Generar perfil estructural» of RACKMENU and draws exactly as before. The rules are pure; the commands and the door, by their source.
/// </summary>
public class StructuralSectionDoorTests
{
    private static readonly StructuralSectionCatalog Catalog = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

    private const string Seccion = "src/RackCad.Plugin/RackSeccionCommands.cs";
    private const string SeccionVentana = "src/RackCad.Plugin/RackSeccionCommands.Ventana.cs";
    private const string Flow = "src/RackCad.Plugin/StructuralSectionCommandFlow.cs";
    private const string DoorPartial = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.StructuralSection.cs";

    // The plan the inspector builds for a section (the steps of StructuralSectionInspectorState.BuildPlan, which lives in the UI).
    private static (StructuralSectionDefinition Section, StructuralSectionRepresentationPlan Plan) Pick(string label, double length = 120)
    {
        var section = Catalog.Enabled.Single(s => s.DisplayName == label);
        var geometry = new StructuralSectionGeometryFactory(Catalog).Get(section, SectionDetailLevel.Tabulated);
        var plan = StructuralSectionPlanBuilder.Build(geometry, PrismaticSectionInstance.Create(section.SectionId, length, null, 0, false),
            new SectionRepresentationOptions
            {
                Viewpoint = SectionViewpoint.Standard(SectionViewKind.CrossSection),
                Mode = SectionRepresentationMode.Wireframe,
                Detail = SectionDetailLevel.Tabulated,
            });
        return (section, plan);
    }

    // ------------------------------------------------------------------ the rules

    [Fact]
    public void ARequestIsInsertedOnlyWithASectionAndItsOwnPlan()
    {
        var (w, plan) = Pick("W12X26");
        var (other, otherPlan) = Pick("W14X22");
        var session = new SessionId(7);

        Assert.Null(StructuralSectionDoorRules.RefusalOf(new StructuralSectionInsertRequest(1, session, w, plan)));
        Assert.Equal(StructuralSectionDoorRules.NothingToInsertText, StructuralSectionDoorRules.RefusalOf(null!));
        Assert.Equal(StructuralSectionDoorRules.NothingToInsertText, StructuralSectionDoorRules.RefusalOf(new StructuralSectionInsertRequest(1, session, null!, plan)));
        Assert.Equal(StructuralSectionDoorRules.NothingToInsertText, StructuralSectionDoorRules.RefusalOf(new StructuralSectionInsertRequest(1, session, w, null!)));
        Assert.Equal(StructuralSectionDoorRules.MismatchText, StructuralSectionDoorRules.RefusalOf(new StructuralSectionInsertRequest(1, session, w, otherPlan)));
        Assert.Null(StructuralSectionDoorRules.RefusalOf(new StructuralSectionInsertRequest(2, session, other, otherPlan)));
        Assert.Equal("RACKSECCIONINSERTAR", StructuralSectionDoorRules.InsertCommand);
    }

    [Fact]
    public void TheAnswersSayWhatWasDrawnOrWhyNotAndCarryTheReportAndTheUnits()
    {
        var (w, _) = Pick("W12X26");

        var inserted = StructuralSectionDoorRules.Inserted(4, w, "RACKCAD_SECCION_W12X26", "RackCad: W12X26 insertada", unitsWarned: true);
        Assert.Equal((4L, true, false, "RACKCAD_SECCION_W12X26", "RackCad: W12X26 insertada", true),
            (inserted.RequestId, inserted.Inserted, inserted.Failed, inserted.BlockName, inserted.Report, inserted.UnitsWarned));
        Assert.Equal("AutoCAD insertó «W12X26» como el bloque «RACKCAD_SECCION_W12X26». Es una sección suelta, sin datos de rack: no sale en la lista "
                     + "de la ventana ni se reabre. Un «U» la deshace.", inserted.Message);

        var cancelled = StructuralSectionDoorRules.Cancelled(5, "RackCad: insercion cancelada; no se dejo nada en el dibujo.", unitsWarned: true);
        Assert.Equal((false, false, true), (cancelled.Inserted, cancelled.Failed, cancelled.UnitsWarned));
        Assert.Equal("No se insertó nada: se canceló el punto de inserción. El dibujo no cambió.", cancelled.Message);

        var failed = StructuralSectionDoorRules.Failed(6, "  eInvalidInput ", "RackCad: no se pudo dibujar la seccion. eInvalidInput", false);
        Assert.Equal((false, true), (failed.Inserted, failed.Failed));
        Assert.Equal("No se pudo dibujar la sección: eInvalidInput El dibujo no cambió.", failed.Message);
        Assert.Equal("No se pudo dibujar la sección: sin detalle. El dibujo no cambió.", StructuralSectionDoorRules.Failed(6, " ", null!, false).Message);

        var refused = StructuralSectionDoorRules.Refused(7, StructuralSectionDoorRules.OtherDrawingText);
        Assert.Equal((false, true, (string?)null, false), (refused.Inserted, refused.Failed, refused.Report, refused.UnitsWarned));
        Assert.Equal(StructuralSectionDoorRules.NothingToInsertText, StructuralSectionDoorRules.Refused(7, null!).Message);

        // The page says the units advisory with its words; the command line keeps those of RackUnitsGuard.
        Assert.StartsWith("Aviso de unidades: el dibujo no está en pulgadas (INSUNITS).", StructuralSectionDoorRules.UnitsText);
        Assert.Contains("no convierte ni reescala nada", StructuralSectionDoorRules.UnitsText);
    }

    [Fact]
    public void RackSeccionSaysInTheCommandLineThatTheWindowShowsThePage()
    {
        var ask = new RackCadShortcutAsk(RackCadShortcutTarget.StructuralSection, null!, null!);
        Assert.Equal("RackCad: no se pudo abrir la ventana RackCad.", RackCadShortcutRules.CommandLineText(ask));
        ask.Taken = true;
        Assert.Equal("RackCad: «Nuevo» › «Perfil estructural» en la ventana RackCad.", RackCadShortcutRules.CommandLineText(ask));
        ask.Said = "El catálogo no es válido.";
        Assert.Equal("RackCad: «Nuevo» › «Perfil estructural» en la ventana RackCad: El catálogo no es válido.", RackCadShortcutRules.CommandLineText(ask));

        // RACKSECCION reads nothing inside itself: no read of the window runs in its command.
        Assert.DoesNotContain(RackCadShortcutRules.InlineReads, r => r.StartsWith("RACKSECCION", StringComparison.Ordinal));
        Assert.Null(RackCadShortcutRules.InlineReadOf("RACKSECCIONINSERTAR "));
    }

    // ------------------------------------------------------------------ the commands and the door, by their source

    [Fact]
    public void RackSeccionOpensTheWindowAndTheClassicInspectorStaysInTheClassicMenu()
    {
        var command = WorkspaceHostSource.Code(Seccion);
        Assert.Contains("[CommandMethod(\"RACKSECCION\", RackCadWindowShortcuts.WindowFlags)]", command);
        var open = WorkspaceHostSource.BodyOf(command, @"public void RackSeccion\(\)");
        Assert.Contains("RackCadWindowShortcuts.Open(document, new RackCadShortcutAsk(RackCadShortcutTarget.StructuralSection, null, null));", open);
        Assert.Contains("RackCommandSupport.Report(ex);", open);
        foreach (var forbidden in new[] { "ShowModalWindow", "StructuralSectionCommandFlow", "SendStringToExecute", "StartTransaction", "LockDocument" })
            Assert.DoesNotContain(forbidden, command);

        // RACKSECCION is registered once, in its file; the classic menu runs the classic flow, before the dispatch of a rack.
        var registrations = WorkspaceHostSource.Files("src/RackCad.Plugin")
            .SelectMany(f => Regex.Matches(f.Code, @"\[CommandMethod\(""(?<name>RACKSECCION[A-Z]*)""").Select(m => f.Path + ":" + m.Groups["name"].Value))
            .OrderBy(e => e, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(new[] { SeccionVentana + ":RACKSECCIONINSERTAR", Seccion + ":RACKSECCION" }, registrations); // ordinal: «.V» before «.c»

        var menu = WorkspaceHostSource.Code("src/RackCad.Plugin/RackMenuCommands.cs");
        InOrder(menu, "if (menu.RequestedAction == MainMenuAction.GenerateStructuralSection)", "StructuralSectionCommandFlow.Run(document);",
            "DispatchInsertion(document, menu.InsertionRequest);");
    }

    [Fact]
    public void TheDoorCommandDrawsTheApprovedPlanWithTheMaterializationOfRackSeccionInOneUndoStep()
    {
        var command = WorkspaceHostSource.Code(SeccionVentana);

        // No flags: the command keeps its undo marker, so ONE «U» undoes the whole insertion.
        Assert.Contains("[CommandMethod(\"RACKSECCIONINSERTAR\")]", command);
        var body = WorkspaceHostSource.BodyOf(command, @"public void RackSeccionInsertar\(\)");
        InOrder(body, "RackCadRackDoorPort.TakePendingStructuralSectionInsert()", "if (pending == null)", "StructuralSectionDoorRules.NoRequestText",
            "door?.Started(pending.RequestId);", "pending.Document != RackCadRackDoorPort.KeyOf(document)", "StructuralSectionDoorRules.OtherDrawingText",
            "StructuralSectionDoorRules.RefusalOf(request)", "StructuralSectionCommandFlow.Place(document, request.Section, request.Plan)",
            "catch (System.Exception ex)", "door?.AnswerStructuralSectionInsert(answer);", "document.Editor.WriteMessage(\"\\n\" + report);");
        Assert.Equal(1, WorkspaceHostSource.Count(command, @"StructuralSectionCommandFlow\.Place\("));

        // The command computes and materialises nothing of its own: the point, the transaction, the block, the regen and the units are
        // the flow's.
        Assert.DoesNotMatch(new Regex(@"StartTransaction|LockDocument|GetPoint|CreateBlockDefinition|InsertReference|WarnIfNotInches|ApplyRegen|"
                                      + @"StructuralSectionInsertService|StructuralSectionInspectorWindow|SendStringToExecute|Math\.|PlanBuilder|Insunits"), command);

        // Its answer: drawn, cancelled at the point, or failed.
        var answer = WorkspaceHostSource.BodyOf(command, @"private static StructuralSectionInserted AnswerOf\(");
        InOrder(answer, "if (!outcome.Success) return StructuralSectionDoorRules.Failed(", "if (!outcome.Placed) return StructuralSectionDoorRules.Cancelled(",
            "return StructuralSectionDoorRules.Inserted(request.RequestId, request.Section, outcome.BlockName, placement.Report, placement.UnitsWarned);");
    }

    [Fact]
    public void TheFlowPlacesTheApprovedPlanOnceForTheClassicAndThePage()
    {
        var flow = WorkspaceHostSource.Code(Flow);

        // The classic: the inspector, then the one materialisation, then the same line in the command line.
        var run = WorkspaceHostSource.BodyOf(flow, @"internal static void Run\(Document document\)");
        InOrder(run, "StructuralSectionCatalogAccess.TryLoad(editor, out var catalog)", "new StructuralSectionInspectorWindow(catalog)",
            "AcApplication.ShowModalWindow(window);", "if (window.Result == null)", "var placement = Place(document, window.AcceptedSection, window.Result);",
            "editor.WriteMessage(\"\\n\" + placement.Report);");
        Assert.DoesNotContain("WarnIfNotInches", run);
        Assert.DoesNotContain("StructuralSectionInsertService", run);

        // Place: the units advisory BEFORE the point, the insert of the plan it received, and the report of that very plan.
        var place = WorkspaceHostSource.BodyOf(flow, @"internal static StructuralSectionPlacement Place\(");
        InOrder(place, "var unitsWarned = RackUnitsGuard.WarnIfNotInches(document);", "var outcome = StructuralSectionInsertService.Insert(document, plan);",
            "return new StructuralSectionPlacement(outcome, Describe(outcome, section, plan), unitsWarned);");
        Assert.Equal(1, WorkspaceHostSource.Count(flow, @"StructuralSectionInsertService\.Insert\("));
        Assert.Equal(1, WorkspaceHostSource.Count(flow, @"RackUnitsGuard\.WarnIfNotInches\("));

        // The units guard says whether it warned; it still writes its advisory itself, once.
        var guard = WorkspaceHostSource.Code("src/RackCad.Plugin/RackUnitsGuard.cs");
        var warn = WorkspaceHostSource.BodyOf(guard, @"internal static bool WarnIfNotInches\(Document document\)");
        InOrder(warn, "if (document?.Database == null)", "return false;", "DrawingUnitsAdvisory.RequiresInsertionAdvisory(", "document.Editor.WriteMessage(\"\\n\" + Advisory);",
            "return true;", "return false;");
    }

    [Fact]
    public void TheDoorOfThePageKeepsTheRequestBeforeHandingItOverAndForgetsItWhenAbandoned()
    {
        var door = WorkspaceHostSource.Code(DoorPartial);
        var request = WorkspaceHostSource.BodyOf(door, @"public RackCadClassicResult RequestStructuralSectionInsert\(StructuralSectionInsertRequest request\)");
        InOrder(request, "StructuralSectionDoorRules.RefusalOf(request)", "RackCadClassicStatus.Failed", "Send(StructuralSectionDoorRules.InsertCommand + \" \",",
            "_pendingStructuralSectionInsert = new PendingStructuralSectionInsert(request, KeyOf(document))", "_pendingStructuralSectionInsert = null");
        var take = WorkspaceHostSource.BodyOf(door, @"internal static PendingStructuralSectionInsert TakePendingStructuralSectionInsert\(\)");
        InOrder(take, "var pending = _pendingStructuralSectionInsert;", "_pendingStructuralSectionInsert = null;", "return pending;");
        Assert.DoesNotMatch(new Regex(@"LockDocument|StartTransaction|SendStringToExecute|\[CommandMethod"), door);

        var abandon = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs"), @"public void Abandon\(long requestId\)");
        Assert.Contains("AbandonStructuralSection(requestId);", abandon);
    }

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
}
