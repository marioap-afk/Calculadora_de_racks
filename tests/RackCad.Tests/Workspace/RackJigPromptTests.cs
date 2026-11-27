#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Views.Placement;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

/// <summary>
/// I-64, sexta tanda, ui (O-3 of the smoke of the fifth batch): the command line said «Punto de insercion de la cabecera» while a command of
/// the RackCad window placed a new cama (RACKCAMANUEVO), a new Push Back (RACKPUSHBACKNUEVO) or a view of a Selectivo (RACKSELECTIVOINSERTAR).
/// The three reach the jig shared by every system through paths that name nothing (the dispatch of «Nuevo», the «Insertar» half of
/// RACKEDITAR), so each of them names what it inserts around its insertion, and every other command keeps the text it had. The Core project
/// cannot reference the Plugin (it needs AutoCAD): the commands and the jig are read as text, as the other guards of the Plugin.
/// </summary>
public class RackJigPromptTests
{
    private const string BlockPlacementPath = "src/RackCad.Plugin/Drawing/BlockPlacement.cs";
    private const string CamaDoorPath = "src/RackCad.Plugin/VentanaRackCad/RackCadCamaDoorCommands.cs";
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
        using (RackJigPrompt.Scope(RackJigPrompt.NewCama))
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
    public void TheScopeBelongsToTheThreadOfTheCommand()
    {
        using (RackJigPrompt.Scope(RackJigPrompt.NewCama))
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
        Assert.Equal("\nPunto de insercion de la cama: ", RackJigPrompt.NewCama);
        Assert.Equal("\nPunto de insercion de la vista Push Back: ", RackJigPrompt.PushBackView);
        Assert.Equal("\nPunto de insercion de la vista Selectivo: ", RackJigPrompt.SelectiveView);
    }

    // ------------------------------------------------------------------ the commands and the jig, as text

    [Theory]
    [InlineData(CamaDoorPath, @"public\s+void\s+InsertNewCama\s*\(", "NewCama", "RackMenuCommands.DispatchInsertion(")]
    [InlineData(PushBackDoorPath, @"public\s+void\s+InsertNewPushBack\s*\(", "PushBackView", "RackMenuCommands.DispatchInsertion(")]
    [InlineData(RackDoorPath, @"public\s+void\s+InsertSelectiveViews\s*\(", "SelectiveView", "RackSelectivoCommands.InsertSelectiveViews(")]
    public void EachCommandOfTheWindowNamesWhatItInsertsAroundItsInsertion(string path, string command, string prompt, string insertion)
    {
        var body = Src.BodyOf(Src.Code(path), command);
        var scope = @"using\s*\(\s*RackJigPrompt\.Scope\(\s*RackJigPrompt\." + prompt + @"\s*\)\s*\)";

        Assert.Equal(1, Src.Count(body, @"RackJigPrompt\.Scope\("));
        var block = Src.BodyOf(body, scope);
        Assert.Contains(insertion, block, StringComparison.Ordinal);
        Assert.Equal(1, Src.Count(body, Regex.Escape(insertion))); // the one insertion of the command is the one inside the scope
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
    public void OnlyTheThreeCommandsOpenAScopeSoEveryOtherCommandKeepsItsText()
    {
        var opening = Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"RackJigPrompt\.Scope\(")).Select(f => f.Path).ToArray();

        Assert.Equal(new[] { CamaDoorPath, PushBackDoorPath, RackDoorPath }, opening);
    }
}
