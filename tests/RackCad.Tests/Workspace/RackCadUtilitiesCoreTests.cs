#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.Settings;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.Larguero;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// I-64 A-14, line D, fourth batch, part «ut» (prototype «utilidades», UT-01 to UT-12): the pure parts of the utilities of the RackCad window and
// the wiring of their doors in the Plugin, which needs AutoCAD to run and is fixed here by its source. The reading of the library says which
// files it could not read and whether the folder exists; the file of a saved design follows the classic rule; «Ejecutar» of the help runs only
// the commands of its list; the door of a design in its classic editor takes a numbered request kept for its drawing; and «Abrir RackCad al
// iniciar AutoCAD» is off by default, read once when the plugin loads.
public class RackCadUtilitiesCoreTests
{
    private static readonly RackProjectStore Store = new();

    private static void InTempDir(Action<string> body)
    {
        var dir = Path.Combine(Path.GetTempPath(), "RackCadUtCore_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            body(dir);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    // ------------------------------------------------------------------ the reading of the library (UT-06, UT-08)

    [Fact]
    public void TheReadingSaysWhichFilesItCouldNotReadAndTheListStaysTheSame()
    {
        InTempDir(dir =>
        {
            Store.Save(RackProject.ForCama(new FlowBedConfiguration { LaneDepth = 120.0 }), Path.Combine(dir, "cama" + RackProjectStore.FileExtension));
            Store.Save(RackProject.ForLarguero(new LargueroDesign { Name = "L1", BeamProfileId = "B" }), Path.Combine(dir, "lar" + RackProjectStore.FileExtension));
            File.WriteAllText(Path.Combine(dir, "zeta rota" + RackProjectStore.FileExtension), "{ no");
            File.WriteAllText(Path.Combine(dir, "alfa rota" + RackProjectStore.FileExtension), "");
            File.WriteAllText(Path.Combine(dir, "otra cosa.json"), "{}"); // not a design file: not read at all

            var listing = RackDesignLibrary.Read(dir);

            Assert.True(listing.FolderExists);
            Assert.Equal(dir, listing.Folder);
            Assert.Equal(new[] { "alfa rota.rackcad.json", "zeta rota.rackcad.json" }, listing.Unreadable.ToArray());
            Assert.Equal(new[] { "L1", "cama" }, listing.Entries.Select(e => e.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
            // The list of the classic library is the same reading.
            Assert.Equal(listing.Entries.Select(e => e.Path), RackDesignLibrary.List(dir).Select(e => e.Path));
        });
    }

    [Fact]
    public void AFolderThatDoesNotExistIsToldApartFromAnEmptyOne()
    {
        var missing = Path.Combine(Path.GetTempPath(), "RackCadUtMissing_" + Guid.NewGuid().ToString("N"));
        var none = RackDesignLibrary.Read(missing);
        Assert.False(none.FolderExists);
        Assert.Empty(none.Entries);
        Assert.Empty(none.Unreadable);
        Assert.False(RackDesignLibrary.Read(" ").FolderExists);

        InTempDir(dir =>
        {
            var empty = RackDesignLibrary.Read(dir);
            Assert.True(empty.FolderExists);
            Assert.Empty(empty.Entries);
        });
    }

    [Fact]
    public void AKindWithNoDescriptorIsAnUnreadableFileOfTheReadingAndTheEntryOfOneFileHasTheRulesOfTheList()
    {
        InTempDir(dir =>
        {
            var larguero = Path.Combine(dir, "archivo" + RackProjectStore.FileExtension);
            Store.Save(RackProject.ForLarguero(new LargueroDesign { Name = " Larguero 96 ", BeamProfileId = "B" }), larguero);
            var withoutLarguero = new SystemRegistry(SystemRegistry.Default.Descriptors.Where(d => d.Kind != RackSystemKind.Larguero));

            var listing = RackDesignLibrary.Read(dir, withoutLarguero);
            Assert.Empty(listing.Entries);
            Assert.Equal(new[] { "archivo.rackcad.json" }, listing.Unreadable.ToArray());

            var entry = RackDesignLibrary.EntryOf(larguero, Store.Load(larguero), SystemRegistry.Default)!;
            Assert.Equal(("Larguero 96", RackSystemKind.Larguero, "Larguero"), (entry.Name, entry.Kind, entry.KindLabel));
            Assert.Null(RackDesignLibrary.EntryOf(larguero, Store.Load(larguero), withoutLarguero));
            Assert.Null(RackDesignLibrary.EntryOf(larguero, null!, SystemRegistry.Default));
        });
    }

    // ------------------------------------------------------------------ the file of a saved design (UT-04)

    [Theory]
    [InlineData("Selectivo 2", "Selectivo 2", false)]
    [InlineData("  Selectivo 2  ", "Selectivo 2", false)]
    [InlineData("Nave 3: pasillo/B", "Nave 3_ pasillo_B", true)]
    [InlineData("a*b?c<d>e|f\"g\\h", "a_b_c_d_e_f_g_h", true)]
    public void TheFileOfADesignIsItsNameWithTheCharactersWindowsDoesNotAdmitAsUnderscores(string name, string expected, bool cleaned)
    {
        Assert.Equal(expected, RackCadLibraryFileName.BaseOf(name));
        Assert.Equal(expected + RackProjectStore.FileExtension, RackCadLibraryFileName.FileOf(name));
        Assert.Equal(cleaned, RackCadLibraryFileName.IsCleaned(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ADesignWithoutANameHasNoFile(string? name)
    {
        Assert.Null(RackCadLibraryFileName.BaseOf(name));
        Assert.Null(RackCadLibraryFileName.FileOf(name));
        Assert.False(RackCadLibraryFileName.IsCleaned(name));
    }

    [Fact]
    public void ChangeTheNameIsTheFirstNameWhoseFileIsNotInTheFolder()
    {
        var taken = new[] { @"C:\L\Selectivo 2 (2).rackcad.json", @"C:\L\Selectivo 2 (3).rackcad.json" };
        Assert.Equal("Selectivo 2 (4)", RackCadLibraryFileName.NextFreeName(@"C:\L", "Selectivo 2", path => taken.Contains(path, StringComparer.OrdinalIgnoreCase)));
        Assert.Equal("Selectivo 2 (2)", RackCadLibraryFileName.NextFreeName(@"C:\L", " Selectivo 2 ", _ => false));
    }

    // ------------------------------------------------------------------ «Ejecutar» of the help (UT-10)

    [Theory]
    [InlineData("RACKLAYOUT", "RACKLAYOUT ")]
    [InlineData(" RACKAYUDA ", "RACKAYUDA ")]
    [InlineData("RACKMENU", "RACKMENU ")]
    [InlineData("RACKSECCION", "RACKSECCION ")]
    public void TheHelpRunsTheCommandsOfItsListEndedByASpace(string command, string text)
    {
        Assert.Equal(text, RackCadHelpCommands.TextOf(command));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("RACKCAD")]
    [InlineData("RACKNUEVO")]
    [InlineData("RACKBIBLIOTECA")]
    [InlineData("RACKBIBLIOTECAABRIR")]
    [InlineData("RACKPANEL")]
    [InlineData("RACKSELECTIVOAPLICAR")]
    [InlineData("rackayuda")]
    [InlineData("RACKAYUDA\nLINE")]
    [InlineData("_ERASE")]
    public void TheHelpRunsNothingElse(string? command)
    {
        Assert.Null(RackCadHelpCommands.TextOf(command));
    }

    [Fact]
    public void EveryCommandTheHelpRunsIsRegisteredInThePlugin()
    {
        var registered = Directory.GetFiles(Path.Combine(WorkspaceHostSource.RepoRoot(), "src", "RackCad.Plugin"), "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => Regex.Matches(File.ReadAllText(path), @"\[CommandMethod\(""(?<name>[^""]*)""").Select(m => m.Groups["name"].Value))
            .ToHashSet(StringComparer.Ordinal);

        Assert.All(RackCadHelpCommands.Runnable, command => Assert.Contains(command, registered));
        Assert.Equal(RackCadHelpCommands.Runnable.Count, RackCadHelpCommands.Runnable.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(RackCadLibraryDoorRules.Command, registered);
    }

    // ------------------------------------------------------------------ the door of a design in its classic editor (UT-03)

    [Fact]
    public void TheWindowAsksOnlyForANumberedRequestOfADesignFile()
    {
        var session = new SessionId(7);
        Assert.Null(RackCadLibraryDoorRules.RefusalOf(new RackCadLibraryDesignRequest(1, session, @"C:\L\Cama 48 in.rackcad.json")));
        Assert.Equal("La petición no es válida.", RackCadLibraryDoorRules.RefusalOf(null));
        Assert.Equal("La petición no es válida.", RackCadLibraryDoorRules.RefusalOf(new RackCadLibraryDesignRequest(0, session, @"C:\L\a.rackcad.json")));
        Assert.Equal("El diseño no es un archivo de la biblioteca (.rackcad.json).",
            RackCadLibraryDoorRules.RefusalOf(new RackCadLibraryDesignRequest(2, session, "a.rackcad.json")));
        Assert.Equal("El diseño no es un archivo de la biblioteca (.rackcad.json).",
            RackCadLibraryDoorRules.RefusalOf(new RackCadLibraryDesignRequest(3, session, @"C:\L\a.dwg")));
        Assert.Equal("El diseño no es un archivo de la biblioteca (.rackcad.json).", RackCadLibraryDoorRules.RefusalOf(new RackCadLibraryDesignRequest(4, session, " ")));
    }

    [Fact]
    public void TheCommandOpensOnlyTheRequestOfItsDrawingWhoseFileExists()
    {
        Assert.Null(RackCadLibraryDoorRules.RefusalWhenRun(hasRequest: true, sameDrawing: true, fileExists: true));
        Assert.StartsWith("RACKBIBLIOTECAABRIR la usa la ventana RackCad", RackCadLibraryDoorRules.RefusalWhenRun(false, false, false));
        Assert.Equal("El diseño se pidió para otro dibujo: no se abrió nada. Vuelve a pedirlo desde la ventana.",
            RackCadLibraryDoorRules.RefusalWhenRun(true, false, true));
        Assert.Equal("El archivo del diseño ya no está en la carpeta: no se abrió nada. Vuelve a leer la carpeta en la ventana.",
            RackCadLibraryDoorRules.RefusalWhenRun(true, true, false));
    }

    // The wiring in the Plugin needs AutoCAD to run: its order is fixed by its source.
    [Fact]
    public void ThePortKeepsTheRequestBeforeHandingItsCommandAndRunsOnlyTheCommandsOfTheHelp()
    {
        var port = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadClassicCommandPort.cs");
        Assert.Matches(@"class\s+RackCadClassicCommandPort\s*:\s*IRackCadClassicCommandPort\s*,\s*IRackCadUtilityCommandPort", port);

        var help = WorkspaceHostSource.BodyOf(port, @"public\s+RackCadClassicResult\s+RunHelpCommand\s*\(");
        Assert.Matches(@"RackCadHelpCommands\.TextOf\s*\(\s*command\s*\)", help);
        Assert.Matches(@"if\s*\(\s*text\s*==\s*null\s*\)\s*return", help);

        var open = WorkspaceHostSource.BodyOf(port, @"public\s+RackCadClassicResult\s+OpenLibraryDesign\s*\(");
        var refusal = open.IndexOf("RackCadLibraryDoorRules.RefusalOf(request)", StringComparison.Ordinal);
        var send = open.IndexOf("Send(", StringComparison.Ordinal);
        Assert.True(refusal >= 0 && send > refusal, "the request is checked before the command is handed over");
        Assert.Contains("RackCadLibraryDoorRules.Command", open, StringComparison.Ordinal);
        Assert.Contains("RackCadRackDoorPort.KeyOf(document)", open, StringComparison.Ordinal);

        var sender = WorkspaceHostSource.BodyOf(port, @"private\s+static\s+RackCadClassicResult\s+Send\s*\(");
        var order = new[] { "CommandInProgress", "keep?.Invoke(document)", "SendStringToExecute", "drop?.Invoke()", "ReturnFocusToDrawing(document)" };
        var at = -1;
        foreach (var step in order)
        {
            var next = sender.IndexOf(step, at + 1, StringComparison.Ordinal);
            Assert.True(next > at, "out of order or missing: " + step);
            at = next;
        }
    }

    [Fact]
    public void TheCommandTakesTheRequestOnceChecksItsDrawingAndDrawsWithTheDispatchOfTheMenu()
    {
        var code = WorkspaceHostSource.Code("src/RackCad.Plugin/RackBibliotecaCommands.cs");
        Assert.Equal(1, WorkspaceHostSource.Count(code, @"\[CommandMethod\(""RACKBIBLIOTECAABRIR""\)\]"));
        Assert.Equal(1, WorkspaceHostSource.Count(code, @"\[CommandMethod\("));

        var body = WorkspaceHostSource.BodyOf(code, @"public\s+void\s+RackBibliotecaAbrir\s*\(");
        var order = new[]
        {
            "TakePendingLibraryDesign()",
            "RackCadLibraryDoorRules.RefusalWhenRun(",
            "pending.Document == VentanaRackCad.RackCadRackDoorPort.KeyOf(document)",
            "if (refusal != null)",
            "RackCadLibraryFlows.OpenDesign(owner, pending.Path, styles)",
            "AcApplication.ShowModalWindow(host.Window)",
            "host.ThrowIfFailed()",
            "RackMenuCommands.DispatchInsertion(document, host.Result as RackInsertionRequest)",
        };
        var at = -1;
        foreach (var step in order)
        {
            var next = body.IndexOf(step, at + 1, StringComparison.Ordinal);
            Assert.True(next > at, "out of order or missing: " + step);
            at = next;
        }

        Assert.Equal(1, WorkspaceHostSource.Count(body, @"TakePendingLibraryDesign\s*\("));
    }

    // ------------------------------------------------------------------ «Abrir RackCad al iniciar AutoCAD» (UT-01)

    [Fact]
    public void OpenOnStartupIsOffByDefaultAlsoForEverySettingsFileWrittenBefore()
    {
        Assert.False(RackCadStartupRule.OpensWindow(null));
        Assert.False(RackCadStartupRule.OpensWindow(new UserSettings()));
        Assert.True(RackCadStartupRule.OpensWindow(new UserSettings { OpenRackCadOnStartup = true }));

        var path = Path.Combine(Path.GetTempPath(), "rackcad-ut-settings-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(path, "{ \"BlockLibraryPath\": \"C:\\\\b.dwg\" }");
            Assert.False(UserSettingsStore.Load(path).OpenRackCadOnStartup);

            UserSettingsStore.Save(new UserSettings { OpenRackCadOnStartup = true, DesignLibraryPath = @"\\servidor\Diseños" }, path);
            var loaded = UserSettingsStore.Load(path);
            Assert.True(loaded.OpenRackCadOnStartup);
            Assert.Equal(@"\\servidor\Diseños", loaded.DesignLibraryPath);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ThePluginReadsTheSettingOnceWhenItLoadsAndOpensTheWindowWithTheFirstDrawingOnlyWhenItIsOn()
    {
        var initializer = WorkspaceHostSource.Code("src/RackCad.Plugin/PluginInitializer.cs");
        var initialize = WorkspaceHostSource.BodyOf(initializer, @"public\s+void\s+Initialize\s*\(");
        // Initialize only arms the startup: the words the guards of F1 forbid there (Workspace, Controller, PaletteHost…) stay out.
        Assert.Equal("publicvoidInitialize(){VentanaRackCad.RackCadStartup.Arm();}", Regex.Replace(initialize, @"\s+", string.Empty));

        // No event of AutoCAD (the bridge of the workspace is their one subscriber, F1): a timer of the dispatcher of the main thread.
        var startup = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadStartup.cs");
        var arm = WorkspaceHostSource.BodyOf(startup, @"internal\s+static\s+void\s+Arm\s*\(");
        var rule = arm.IndexOf("RackCadStartupRule.OpensWindow(UserSettingsStore.Load())", StringComparison.Ordinal);
        var timer = arm.IndexOf("new DispatcherTimer(", StringComparison.Ordinal);
        var start = arm.IndexOf("_timer.Start()", StringComparison.Ordinal);
        Assert.True(rule >= 0 && timer > rule && start > timer, "the timer only after the rule says it opens");
        Assert.Equal(1, WorkspaceHostSource.Count(startup, @"Tick\s*\+="));

        var tick = WorkspaceHostSource.BodyOf(startup, @"private\s+static\s+void\s+OnTick\s*\(");
        var waits = tick.IndexOf("MdiActiveDocument == null && ++_ticks < MaxTicks) return", StringComparison.Ordinal);
        var stops = tick.IndexOf("Stop();", StringComparison.Ordinal);
        var opens = tick.IndexOf("new RackMenuCommands().RackCad()", StringComparison.Ordinal);
        Assert.True(waits >= 0 && stops > waits && opens > stops, "waits for a drawing, stops, then runs what RACKCAD runs");
        Assert.DoesNotMatch(new Regex(@"\.Idle\s*[-+]=|DocumentManager\.\w+\s*[-+]="), startup);
        Assert.DoesNotMatch(new Regex(@"LockDocument|StartTransaction|SendStringToExecute"), startup);
    }
}
