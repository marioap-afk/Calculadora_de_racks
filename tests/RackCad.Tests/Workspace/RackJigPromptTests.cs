#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

/// <summary>
/// I-64, sexta tanda, ui (O-3 of the smoke of the fifth batch, and the answer of the Owner on 2026-11-27: «Todas», in the window and in the
/// classic): the command line said «Punto de insercion de la cabecera» while a command placed a cama, a Push Back, a Dinámico or a view of a
/// Selectivo. Those commands reach the jig shared by every system through paths that name nothing (the dispatch of «Nuevo», the «Insertar»
/// half of RACKEDITAR, the draw services), so each of them names what it inserts in a scope around its work; RACKCABECERA, which inserts a
/// cabecera, keeps the text. The Core project cannot reference the Plugin (it needs AutoCAD): the commands and the jig are read as text, as
/// the other guards of the Plugin.
/// </summary>
public class RackJigPromptTests
{
    private const string BlockPlacementPath = "src/RackCad.Plugin/Drawing/BlockPlacement.cs";
    private const string MenuPath = "src/RackCad.Plugin/RackMenuCommands.cs";
    private const string SelectivoPath = "src/RackCad.Plugin/RackSelectivoCommands.cs";
    private const string PushBackPath = "src/RackCad.Plugin/RackPushBackCommands.cs";
    private const string DinamicoPath = "src/RackCad.Plugin/RackDinamicoCommands.cs";
    private const string CamaPath = "src/RackCad.Plugin/RackCamaCommands.cs";
    private const string PushBackDoorPath = "src/RackCad.Plugin/VentanaRackCad/RackCadPushBackDoorCommands.cs";
    private const string RackDoorPath = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs";

    // ------------------------------------------------------------------ the prompt of the jig

    [Fact]
    public void OutsideAScopeTheJigAsksWhatItAlwaysAsked()
    {
        Assert.Equal("\nPunto de insercion de la cabecera: ", RackJigPrompt.Cabecera);
        Assert.Equal(RackJigPrompt.Cabecera, RackJigPrompt.Resolve(null));
        Assert.Equal(RackJigPrompt.Cabecera, RackJigPrompt.Resolve("   "));
        Assert.Equal("\nPunto de insercion de la vista Cantilever: ", RackJigPrompt.Resolve("\nPunto de insercion de la vista Cantilever: "));
    }

    [Fact]
    public void InsideAScopeTheJigThatNamesNothingAsksThePromptOfTheCommandAndANamedPromptStillWins()
    {
        using (RackJigPrompt.Scope(RackJigPrompt.Cama))
        {
            Assert.Equal("\nPunto de insercion de la cama: ", RackJigPrompt.Resolve(null));
            Assert.Equal("\nOtro: ", RackJigPrompt.Resolve("\nOtro: "));
        }

        Assert.Equal(RackJigPrompt.Cabecera, RackJigPrompt.Resolve(null));
    }

    [Fact]
    public void ClosingAScopeGivesBackTheOneThatWasOpenEvenWhenItIsClosedTwice()
    {
        using (RackJigPrompt.Scope(RackJigPrompt.PushBackView))
        {
            var inner = RackJigPrompt.Scope(RackJigPrompt.SelectiveView);
            Assert.Equal(RackJigPrompt.SelectiveView, RackJigPrompt.Resolve(null));
            inner.Dispose();
            Assert.Equal(RackJigPrompt.PushBackView, RackJigPrompt.Resolve(null));
            inner.Dispose();
            Assert.Equal(RackJigPrompt.PushBackView, RackJigPrompt.Resolve(null));
        }

        Assert.Equal(RackJigPrompt.Cabecera, RackJigPrompt.Resolve(null));
    }

    [Fact]
    public void AScopeThatNamesNothingKeepsThePromptThatWasOpen()
    {
        using (RackJigPrompt.Scope(RackJigPrompt.DynamicView))
        {
            using (RackJigPrompt.Scope(null)) Assert.Equal(RackJigPrompt.DynamicView, RackJigPrompt.Resolve(null));
            Assert.Equal(RackJigPrompt.DynamicView, RackJigPrompt.Resolve(null));
        }

        using (RackJigPrompt.Scope(null)) Assert.Equal(RackJigPrompt.Cabecera, RackJigPrompt.Resolve(null));
    }

    [Fact]
    public void TheScopeBelongsToTheThreadOfTheCommand()
    {
        using (RackJigPrompt.Scope(RackJigPrompt.Cama))
        {
            string? other = null;
            var thread = new System.Threading.Thread(() => other = RackJigPrompt.Resolve(null));
            thread.Start();
            thread.Join();
            Assert.Equal(RackJigPrompt.Cabecera, other);
        }
    }

    [Fact]
    public void EachPromptNamesWhatItsCommandInserts()
    {
        Assert.Equal("\nPunto de insercion de la cama: ", RackJigPrompt.Cama);
        Assert.Equal("\nPunto de insercion de la vista Push Back: ", RackJigPrompt.PushBackView);
        Assert.Equal("\nPunto de insercion de la vista Selectivo: ", RackJigPrompt.SelectiveView);
        Assert.Equal("\nPunto de insercion de la vista Dinamico: ", RackJigPrompt.DynamicView);
    }

    [Theory]
    [InlineData(RackSystemKind.SelectiveRack, RackJigPrompt.SelectiveView)]
    [InlineData(RackSystemKind.PalletFlow, RackJigPrompt.DynamicView)]
    [InlineData(RackSystemKind.PushBack, RackJigPrompt.PushBackView)]
    [InlineData(RackSystemKind.Cama, RackJigPrompt.Cama)]
    [InlineData(RackSystemKind.Selective, null)] // the cabecera: «de la cabecera», as RACKCABECERA
    [InlineData(RackSystemKind.Cantilever, null)] // its views and its components name their own
    [InlineData(RackSystemKind.Larguero, null)]
    public void EachSystemNamesItsViewAndTheCabeceraKeepsItsText(RackSystemKind kind, string? prompt)
    {
        Assert.Equal(prompt, RackJigPrompt.ForSystem(kind));
    }

    [Theory]
    [InlineData(RackEmbedDocument.KindSelective, RackJigPrompt.SelectiveView)]
    [InlineData(RackEmbedDocument.KindDynamic, RackJigPrompt.DynamicView)]
    [InlineData(RackEmbedDocument.KindPushBack, RackJigPrompt.PushBackView)]
    [InlineData(RackEmbedDocument.KindCama, RackJigPrompt.Cama)]
    [InlineData(RackEmbedDocument.KindCabecera, null)]
    [InlineData(RackEmbedDocument.KindCantilever, null)]
    [InlineData("otro", null)]
    [InlineData(null, null)]
    public void EachKindOfTheEnvelopeNamesItsViewForRackeditar(string? kind, string? prompt)
    {
        Assert.Equal(prompt, RackJigPrompt.ForEnvelopeKind(kind));
    }

    // ------------------------------------------------------------------ the commands and the jig, as text

    // Every command that inserts something else than a cabecera, with the first call of its work: the scope opens before it.
    [Theory]
    [InlineData(RackDoorPath, @"public\s+void\s+InsertSelectiveViews\s*\(", "RackJigPrompt.SelectiveView", "RackSelectivoCommands.InsertSelectiveViews(")] // RACKSELECTIVOINSERTAR
    [InlineData(RackDoorPath, @"public\s+void\s+InsertViews\s*\(", "RackJigPrompt.DynamicView", "RackDinamicoCommands.ApplyDynamicEdit(")] // RACKDINAMICOINSERTAR
    [InlineData(PushBackDoorPath, @"public\s+void\s+InsertPushBackViews\s*\(", "RackJigPrompt.PushBackView", "Write(")] // RACKPUSHBACKINSERTAR
    [InlineData(MenuPath, @"internal\s+static\s+void\s+DispatchInsertion\s*\(", "RackJigPrompt.ForSystem(request.Kind)", "switch (request)")] // RACKNUEVO, the menu, RACKBIBLIOTECA and every …NUEVO of the window
    [InlineData(MenuPath, @"public\s+void\s+RackEditar\s*\(", "RackJigPrompt.ForEnvelopeKind(embed.Kind)", "handler.Edit(")] // RACKEDITAR «Insertar»
    [InlineData(SelectivoPath, @"public\s+void\s+RackSelectivo\s*\(", "RackJigPrompt.SelectiveView", "DrawSelectiveView(")] // RACKSELECTIVO
    [InlineData(PushBackPath, @"public\s+void\s+RackPushBack\s*\(", "RackJigPrompt.PushBackView", "DrawPushBackView(")] // RACKPUSHBACK
    [InlineData(DinamicoPath, @"public\s+void\s+RackSistemaDinamico\s*\(", "RackJigPrompt.DynamicView", "DrawDynamicView(")] // RACKSISTEMADINAMICO
    [InlineData(CamaPath, @"public\s+void\s+QuickCama\s*\(", "RackJigPrompt.Cama", "new FlowBedDrawService().DrawAndPlace(")] // QUICKCAMA
    public void EachCommandNamesWhatItInsertsBeforeItsWork(string path, string command, string prompt, string work)
    {
        var body = Src.BodyOf(Src.Code(path), command);
        var scope = "using var jigPrompt = RackJigPrompt.Scope(" + prompt + ");";

        Assert.Equal(1, Src.Count(body, @"RackJigPrompt\.Scope\("));
        var at = body.IndexOf(scope, StringComparison.Ordinal);
        Assert.True(at >= 0, "the scope is not opened in " + command + ": " + scope);
        var first = body.IndexOf(work, StringComparison.Ordinal);
        Assert.True(first > at, "the scope opens after the work of " + command);
    }

    [Fact]
    public void TheJigAsksTheNamedPromptOrTheOneOfTheCommandAndKeepsTheTextOfTheCabeceraOtherwise()
    {
        var placement = Src.Code(BlockPlacementPath);
        var jig = Src.BodyOf(placement, @"public\s+HeaderInsertionJig\s*\(");

        Assert.Contains("this.prompt = RackJigPrompt.Resolve(prompt);", jig, StringComparison.Ordinal);
        Assert.DoesNotContain("Punto de insercion", placement, StringComparison.Ordinal); // the text lives in one place
    }

    [Fact]
    public void OnlyTheseCommandsOpenAScopeAndRackcabeceraKeepsItsText()
    {
        var opening = Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"RackJigPrompt\.Scope\(")).Select(f => f.Path).ToArray();

        Assert.Equal(
            new[] { CamaPath, DinamicoPath, MenuPath, PushBackPath, SelectivoPath, PushBackDoorPath, RackDoorPath }.OrderBy(p => p, StringComparer.Ordinal),
            opening);
        Assert.DoesNotContain("RackJigPrompt", Src.Code("src/RackCad.Plugin/RackCabeceraCommands.cs"), StringComparison.Ordinal);
    }
}
