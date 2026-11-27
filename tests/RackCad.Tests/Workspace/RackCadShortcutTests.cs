#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.VentanaRackCad;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// T6 rl (I-64; answer of the Owner, 2026-11-27: «La ventana, ya»; VA-S2 completed): RACKVARIABLES and RVA open the page «Variables», and
/// RACKPROPIEDADES and RPR the tab «Propiedades» of the rack (or «Propiedades del proyecto» with «Proyecto») in the RackCad window. The
/// command is the gesture: the reads the window hands to the door while the command asks it run INSIDE the command, never as a second
/// command. The classic windows stay in the classic menu (RACKMENU). The rules are pure; the commands and the door, by their source.
/// </summary>
public class RackCadShortcutTests
{
    // ------------------------------------------------------------------ the rules

    [Fact]
    public void OnlyTheReadsOfTheWindowRunInsideTheCommand()
    {
        Assert.Equal(new[] { "RACKVARIABLESLEER", "RACKSELECTIVOABRIR", "RACKDINAMICOABRIR", "RACKPUSHBACKABRIR", "RACKCANTILEVERABRIR", "RACKCAMAABRIR" },
            RackCadShortcutRules.InlineReads);
        Assert.Equal("RACKSELECTIVOABRIR", RackCadShortcutRules.InlineReadOf("RACKSELECTIVOABRIR "));
        Assert.Equal("RACKVARIABLESLEER", RackCadShortcutRules.InlineReadOf(" rackvariablesleer "));
        foreach (var write in new[] { "RACKSELECTIVOAPLICAR ", "RACKVARIABLESAPLICAR ", "RACKDINAMICOINSERTAR ", "RACKALMACENCOLOCAR ", "RACKBOMLEER ", "RACKCABECERAABRIR ", "", null })
            Assert.Null(RackCadShortcutRules.InlineReadOf(write));
    }

    [Fact]
    public void TheRackIsTheOneSelectedAndSeveralAskWhich()
    {
        Assert.Equal("SEL-1", RackCadShortcutRules.DesignatedRack(new[] { "SEL-1", " sel-1 ", " " }, out var note));
        Assert.Null(note);
        Assert.Null(RackCadShortcutRules.DesignatedRack(Array.Empty<string>(), out note));
        Assert.Null(note);
        Assert.Null(RackCadShortcutRules.DesignatedRack(new[] { "SEL-1", "PB-1" }, out note));
        Assert.Equal("Hay 2 racks seleccionados en AutoCAD: elige uno.", note);
        Assert.Equal("\nSelecciona un rack o [Proyecto]: ", RackCadShortcutRules.PropertiesPrompt); // the question of the classic window
    }

    [Fact]
    public void TheCommandLineSaysWhatOpenedOrWhyNot()
    {
        var ask = new RackCadShortcutAsk(RackCadShortcutTarget.RackProperties, " SEL-1 ", " ");
        Assert.Equal(("SEL-1", (string?)null, false), (ask.RackId, ask.Note, ask.Taken));
        Assert.Equal("RackCad: no se pudo abrir la ventana RackCad.", RackCadShortcutRules.CommandLineText(ask));
        ask.Taken = true;
        Assert.Equal("RackCad: «Propiedades» del rack en la ventana RackCad.", RackCadShortcutRules.CommandLineText(ask));
        ask.Said = RackCadShortcutRules.BusyText;
        Assert.Equal("RackCad: «Propiedades» del rack en la ventana RackCad: " + RackCadShortcutRules.BusyText, RackCadShortcutRules.CommandLineText(ask));
        Assert.Equal("RackCad: «Variables» en la ventana RackCad.",
            RackCadShortcutRules.CommandLineText(new RackCadShortcutAsk(RackCadShortcutTarget.Variables, null!, null!) { Taken = true }));
        Assert.Equal("RackCad: «Variables» › «Propiedades del proyecto» en la ventana RackCad.",
            RackCadShortcutRules.CommandLineText(new RackCadShortcutAsk(RackCadShortcutTarget.ProjectProperties, null!, null!) { Taken = true }));
    }

    // ------------------------------------------------------------------ the commands, by their source

    [Fact]
    public void RackVariablesAndRackPropiedadesOpenTheWindowAndTheirClassicLoopsStayForTheClassicMenu()
    {
        var variables = WorkspaceHostSource.Code("src/RackCad.Plugin/RackVariablesCommands.cs");
        Assert.Contains("[CommandMethod(\"RVA\", RackCadWindowShortcuts.WindowFlags)] public void AliasRackVariables() => RackVariables();", variables);
        var open = WorkspaceHostSource.BodyOf(variables, @"public void RackVariables\(\)");
        Assert.Contains("RackCadWindowShortcuts.Open(document, new RackCadShortcutAsk(RackCadShortcutTarget.Variables, null, null));", open);
        Assert.DoesNotContain("ShowModalWindow", open);
        Assert.Contains("ShowModalWindow(window)", WorkspaceHostSource.BodyOf(variables, @"internal void RackVariablesClassic\(\)"));

        var properties = WorkspaceHostSource.Code("src/RackCad.Plugin/RackPropiedadesCommands.cs");
        var pick = WorkspaceHostSource.BodyOf(properties, @"public void RackPropiedades\(\)");
        InOrder(pick, "RackCadShortcutRules.DesignatedRack(RackCadWindowShortcuts.SelectedRackIds(document), out var note)",
            "if (rackId == null)", "new PromptEntityOptions(RackCadShortcutRules.PropertiesPrompt, ProjectKeyword)", "PromptStatus.Keyword",
            "RackCadShortcutTarget.ProjectProperties", "RackCadWindowShortcuts.RackIdOf(document, pick.ObjectId)",
            "RackCadWindowShortcuts.Open(document, new RackCadShortcutAsk(target, rackId, null));");
        Assert.DoesNotContain("RackCustomPropertiesWindow", pick);
        Assert.Contains("EditRack(document, definitionId);", WorkspaceHostSource.BodyOf(properties, @"internal void RackPropiedadesClassic\(\)"));

        // The classic menu runs the classic loops, after it closed and before the dispatch of a rack.
        var menu = WorkspaceHostSource.Code("src/RackCad.Plugin/RackMenuCommands.cs");
        InOrder(menu, "MainMenuAction.GenerateStructuralSection", "if (menu.RequestedAction == MainMenuAction.ProjectVariablesClassic)",
            "new RackVariablesCommands().RackVariablesClassic();", "if (menu.RequestedAction == MainMenuAction.CustomPropertiesClassic)",
            "new RackPropiedadesCommands().RackPropiedadesClassic();", "DispatchInsertion(document, menu.InsertionRequest);");
    }

    [Fact]
    public void TheCommandRunsInsideItselfTheReadTheWindowHandedToTheDoorAndSendsNoSecondCommand()
    {
        var shortcuts = WorkspaceHostSource.Code("src/RackCad.Plugin/RackCadWindowShortcuts.cs");
        Assert.Contains("internal const CommandFlags WindowFlags = CommandFlags.NoUndoMarker | CommandFlags.UsePickSet | CommandFlags.Redraw;", shortcuts);
        var open = WorkspaceHostSource.BodyOf(shortcuts, @"internal static void Open\(Document document, RackCadShortcutAsk ask\)");
        InOrder(open, "Workspace.WorkspacePanelCommand.ShowRackCadWindow();", "RackCadRackDoorPort.CollectInlineReads(document, () => door.AskShortcut(ask))",
            "foreach (var read in reads) RunRead(read);", "if (ask.Taken) door.SayShortcutDone(ask);", "RackCadShortcutRules.CommandLineText(ask)");
        Assert.DoesNotContain("SendStringToExecute", shortcuts);

        // Each read of the list is run by the very command method AutoCAD would run, and only those.
        var run = WorkspaceHostSource.BodyOf(shortcuts, @"private static void RunRead\(string command\)");
        var cases = Regex.Matches(run, @"case ""(?<name>[A-Z]+)"":").Select(m => m.Groups["name"].Value).ToArray();
        Assert.Equal(RackCadShortcutRules.InlineReads, cases);
        foreach (var call in new[] { "new RackCadVariablesDoorCommands().ReadVariables()", "new RackCadRackDoorCommands().ReadSelectiveDesign()",
                     "new RackCadRackDoorCommands().ReadDesign()", "new RackCadPushBackDoorCommands().ReadPushBack()",
                     "new RackCadCantileverDoorCommands().ReadCantilever()", "new RackCadCamaDoorCommands().ReadCama()" })
            Assert.Contains(call, run);

        // The door: while a command asks, a read of the list is kept and noted, any other request refused as during any command; the hand
        // over to AutoCAD is never reached then.
        var port = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs");
        var send = WorkspaceHostSource.BodyOf(port, @"private static RackCadClassicResult Send\(string text, Action<AcDocument> keep, Action drop\)");
        InOrder(send, "if (KeptInline(text, document, keep, out var inline)) return inline;", "document.CommandInProgress", "SendStringToExecute(");
        var inlinePort = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.Shortcut.cs");
        var kept = WorkspaceHostSource.BodyOf(inlinePort, @"private static bool KeptInline\(");
        InOrder(kept, "if (inline == null) return false;", "RackCadShortcutRules.InlineReadOf(text)",
            "if (command == null || inline.Document != KeyOf(document) || inline.Commands.Count > 0)", "RackCadClassicStatus.Busy", "keep(document);",
            "inline.Commands.Add(command);", "RackCadClassicResult.Started()");
        Assert.DoesNotMatch(new Regex(@"LockDocument|StartTransaction|SendStringToExecute|\[CommandMethod"), inlinePort);
        var collect = WorkspaceHostSource.BodyOf(inlinePort, @"internal static IReadOnlyList<string> CollectInlineReads\(");
        InOrder(collect, "_inlineReads = collected;", "ask();", "finally", "_inlineReads = null;");
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
