#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// I-64 A-14 (phases 1 and 2): the pure parts of the RackCad window. The list of the racks of the drawing is projected from the same capture
// facts and with the same accumulator as the lazy summary, remembered per session until a drain invalidates it; a rack picked in the list
// opens in the browser session like a one-rack selection; and the classic actions of the window map to exactly one AutoCAD command each.
public class RackCadWindowListTests
{
    private static readonly SessionId S1 = new(1);
    private static readonly SessionId S2 = new(2);

    private sealed class Rig
    {
        public Rig()
        {
            Facts[S1] = new List<BrowserRackFact>
            {
                new(true, "R-1", "selective", "lateral", "Rack uno"),
                new(true, "R-2", null, "frontal", " "),
                new(true, "r-1", "selective", "frontal", null),
                new(false, "R-1", "Wrong", "ignored", "Ignored"),
                new(true, "R-2", "cantilever", "lateral", "Segundo"),
                new(true, null, "selective", "frontal", "No id"),
                new(false, "R-3", "selective", "frontal", "Unreadable"),
                new(true, "R-4", "pushback", "planta", null),
            };
            Alive.Add(S1);
            Alive.Add(S2);
            Summaries = new BrowserRackSummaries(ReadFacts, id => Alive.Contains(id));
        }

        public Dictionary<SessionId, List<BrowserRackFact>> Facts { get; } = new();

        public HashSet<SessionId> Alive { get; } = new();

        public List<SessionId> Reads { get; } = new();

        public Exception? Fault { get; set; }

        public BrowserRackSummaries Summaries { get; }

        private IReadOnlyList<BrowserRackFact> ReadFacts(SessionId id)
        {
            Reads.Add(id);
            if (Fault != null) throw Fault;
            return Facts.TryGetValue(id, out var facts) ? facts : new List<BrowserRackFact>();
        }
    }

    private static DrainOutcome Drain(SessionId session, bool drained, params DrainOperation[] operations) =>
        new(session, new DrainResult(drained, operations));

    // ------------------------------------------------------------------ the projection of the list

    [Fact]
    public void TheListHasOneEntryPerRackInTheOrderOfItsFirstInterpretableFactWithTheProjectionOfTheSummary()
    {
        var rig = new Rig();

        var list = rig.Summaries.ReadList(S1);

        Assert.True(list.IsReady);
        Assert.Null(list.Reason);
        Assert.Equal(new[] { "R-1", "R-2", "R-4" }, list.Racks.Select(r => r.RackId).ToArray());

        var first = list.Racks[0];
        Assert.Equal("Rack uno", first.Name);
        Assert.Equal("selective", first.Kind);
        Assert.Equal(new[] { "frontal", "lateral" }, first.Views);

        // The first non-blank name and Kind, in reading order: the blank name and the null Kind of the first fact of R-2 do not count.
        Assert.Equal("Segundo", list.Racks[1].Name);
        Assert.Equal("cantilever", list.Racks[1].Kind);
        Assert.Null(list.Racks[2].Name);
    }

    [Fact]
    public void EachEntryOfTheListMatchesTheSummaryOfThatRack()
    {
        var rig = new Rig();

        foreach (var entry in rig.Summaries.ReadList(S1).Racks)
        {
            var summary = rig.Summaries.Read(S1, entry.RackId);
            Assert.Equal(summary.Name, entry.Name);
            Assert.Equal(summary.Kind, entry.Kind);
            Assert.Equal(summary.Views, entry.Views);
        }
    }

    [Fact]
    public void AnEmptyDrawingIsAReadyEmptyList()
    {
        var rig = new Rig();

        var list = rig.Summaries.ReadList(S2);

        Assert.True(list.IsReady);
        Assert.Empty(list.Racks);
    }

    // ------------------------------------------------------------------ the memory of the list

    [Fact]
    public void TheListIsRememberedPerSessionAndReadOnlyOnceUntilAnInvalidatingDrain()
    {
        var rig = new Rig();

        var first = rig.Summaries.ReadList(S1);
        var second = rig.Summaries.ReadList(S1);

        Assert.Same(first, second);
        Assert.Single(rig.Reads);
        Assert.True(rig.Summaries.IsListCached(S1));
        Assert.False(rig.Summaries.IsListCached(S2));

        // A drain that does not invalidate the navigation caches keeps it.
        rig.Summaries.OnDrained(Drain(S1, true));
        Assert.True(rig.Summaries.IsListCached(S1));
        rig.Summaries.OnDrained(Drain(S1, false, DrainOperation.InvalidateNavigationCaches));
        Assert.True(rig.Summaries.IsListCached(S1));

        // The drain that invalidates them forgets it, and the next read reads the facts again.
        rig.Summaries.OnDrained(Drain(S1, true, DrainOperation.InvalidateNavigationCaches));
        Assert.False(rig.Summaries.IsListCached(S1));
        rig.Facts[S1].Add(new BrowserRackFact(true, "R-5", "dynamic", "lateral", "Nuevo"));
        var third = rig.Summaries.ReadList(S1);

        Assert.Equal(2, rig.Reads.Count);
        Assert.Contains(third.Racks, r => r.RackId == "R-5");
    }

    [Fact]
    public void AnExplicitReadAgainForgetsTheListAndReadsTheFactsOnceMore()
    {
        var rig = new Rig();

        rig.Summaries.ReadList(S1);
        rig.Facts[S1].Add(new BrowserRackFact(true, "R-6", "cama", null, "Cama"));
        var refreshed = rig.Summaries.ReadList(S1, refresh: true);

        Assert.Equal(2, rig.Reads.Count);
        Assert.Contains(refreshed.Racks, r => r.RackId == "R-6");
    }

    [Fact]
    public void AnInvalidatingDrainOfAnotherSessionKeepsTheList()
    {
        var rig = new Rig();

        rig.Summaries.ReadList(S1);
        rig.Summaries.OnDrained(Drain(S2, true, DrainOperation.InvalidateNavigationCaches));

        Assert.True(rig.Summaries.IsListCached(S1));
    }

    [Fact]
    public void ASessionThatEndedOrIsDeadAnswersRejectedWithoutReadingAndIsNeverResurrected()
    {
        var rig = new Rig();
        rig.Summaries.ReadList(S1);

        rig.Summaries.EndSession(S1);
        var ended = rig.Summaries.ReadList(S1);
        Assert.Equal(BrowserSummaryStatus.Rejected, ended.Status);
        Assert.Empty(ended.Racks);
        Assert.False(rig.Summaries.IsListCached(S1));

        rig.Alive.Remove(S2);
        var dead = rig.Summaries.ReadList(S2);
        Assert.Equal(BrowserSummaryStatus.Rejected, dead.Status);
        rig.Alive.Add(S2);
        Assert.Equal(BrowserSummaryStatus.Rejected, rig.Summaries.ReadList(S2).Status);

        Assert.Single(rig.Reads);
    }

    [Fact]
    public void AFailingReadRemembersNothingAndTheNextReadReadsAgain()
    {
        var rig = new Rig { Fault = new InvalidOperationException("boom") };

        Assert.Throws<InvalidOperationException>(() => rig.Summaries.ReadList(S1));
        Assert.False(rig.Summaries.IsListCached(S1));

        rig.Fault = null;
        Assert.True(rig.Summaries.ReadList(S1).IsReady);
        Assert.Equal(2, rig.Reads.Count);
    }

    [Fact]
    public void APortThatDoesNotOfferTheListAnswersNotOfferedWithoutReading()
    {
        IWorkspaceBrowserSummaryPort port = new SummaryOnlyPort();

        var list = port.ReadRackList(new WorkspaceRequest(S1), userRequested: true);

        Assert.Same(BrowserRackList.NotOffered, list);
        Assert.False(list.IsReady);
        Assert.Empty(list.Racks);
    }

    private sealed class SummaryOnlyPort : IWorkspaceBrowserSummaryPort
    {
        public BrowserRackSummary ReadSummary(WorkspaceRequest origin, string rackId, bool userRequested) =>
            BrowserRackSummary.Without(BrowserSummaryStatus.NotFound, "none");

        public bool IsSessionAlive(SessionId session) => true;
    }

    [Fact]
    public void TheListRemembersThatItWasReadAcrossAnInvalidationButNotAcrossTheEndOfTheSession()
    {
        var rig = new Rig();
        Assert.False(rig.Summaries.HasListBeenRead(S1));

        rig.Summaries.ReadList(S1);
        rig.Summaries.OnDrained(Drain(S1, true, DrainOperation.InvalidateNavigationCaches));

        Assert.False(rig.Summaries.IsListCached(S1));
        Assert.True(rig.Summaries.HasListBeenRead(S1));

        rig.Summaries.EndSession(S1);
        Assert.False(rig.Summaries.HasListBeenRead(S1));
    }

    [Fact]
    public void TheHostReadsTheListInTheOrderOfTheSummaryAndAfterTheFirstReadOnlyWhenTheUserAsks()
    {
        var host = WorkspaceHostSource.Code("src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs");
        var body = WorkspaceHostSource.BodyOf(host, @"public\s+BrowserRackList\s+ReadRackList\s*\(");

        var order = new[]
        {
            "MdiActiveDocument",
            "CommandInProgress",
            "HasListBeenRead(origin.Origin)) return BrowserRackList.Outdated()",
            "_drainNoticeOpen",
            "_authorize(origin",
            "_summaries.ReadList(origin.Origin, refresh: userRequested)",
        };
        var at = -1;
        foreach (var step in order)
        {
            var next = body.IndexOf(step, at + 1, StringComparison.Ordinal);
            Assert.True(next > at, "out of order or missing: " + step);
            at = next;
        }

        // The palette never locks the document for the list outside the facts reader of the host.
        Assert.DoesNotContain("LockDocument", body, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFoldersOfTheWindowKeepTheirLayers()
    {
        foreach (var path in Directory.GetFiles(Path.Combine(Root(), "src", "RackCad.UI", "VentanaRackCad"), "*.cs"))
        {
            var code = WorkspaceHostSource.CodeOnly(File.ReadAllText(path));
            Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"\bAutodesk\.|RackCad\.Plugin\b|LockDocument|SendStringToExecute"), code);
        }

        foreach (var path in Directory.GetFiles(Path.Combine(Root(), "src", "RackCad.Application", "VentanaRackCad"), "*.cs"))
        {
            var code = WorkspaceHostSource.CodeOnly(File.ReadAllText(path));
            Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"\bAutodesk\.|System\.Windows|RackCad\.UI\b|RackCad\.Plugin\b"), code);
        }

        // The doors of the Plugin hand commands to AutoCAD and never lock the document or open a transaction from the palette. The commands
        // of the door to a rack (RackCadRackDoorCommands) read the drawing in their own command context, through RackCommandSupport; the
        // files the palette calls (the ports) do not reach those helpers either, which lock and open transactions (M-05 of the second review).
        foreach (var path in Directory.GetFiles(Path.Combine(Root(), "src", "RackCad.Plugin", "VentanaRackCad"), "*.cs"))
        {
            var code = WorkspaceHostSource.CodeOnly(File.ReadAllText(path));
            Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"LockDocument|StartTransaction|StartOpenCloseTransaction"), code);
            if (!path.EndsWith("RackCadRackDoorCommands.cs", StringComparison.Ordinal))
                Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"\bRackCommandSupport\.|\bApplyDynamicEdit\b|\[CommandMethod"), code);
        }
    }

    // I-02 and M-05 of the second review. The modal host of the window is a plain WPF Window configured in code, not a new window type, so
    // the census of ADR-0029 does not see it (decision V-13): no other code may create a plain Window, or a real window could leave the
    // census the same way. And the palette hands commands to AutoCAD only through the doors of the window, which give the focus back.
    [Fact]
    public void OnlyTheModalHostCreatesAPlainWindowAndOnlyTheDoorOfTheWindowHandsCommandsToAutoCad()
    {
        var plainWindow = PlainWindowPattern();
        var sendsCommands = new System.Text.RegularExpressions.Regex(@"\bSendStringToExecute\b");

        Assert.Equal(new[] { "src/RackCad.UI/VentanaRackCad/RackModalHostWindow.cs" }, SourcesMatching(plainWindow));
        Assert.Equal(
            new[] { "src/RackCad.Plugin/VentanaRackCad/RackCadClassicCommandPort.cs", "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs" },
            SourcesMatching(sendsCommands));
    }

    [Theory]
    [InlineData("var w = new Window { Title = \"x\" };")]
    [InlineData("var w = new System.Windows.Window();")]
    [InlineData("Window w = new() { Title = \"x\" };")]
    [InlineData("private readonly Window _owner = new();")]
    [InlineData("public Window Owner { get; } = new();")]
    [InlineData("var w = (Window)Activator.CreateInstance(typeof(Window));")]
    public void TheGuardOfThePlainWindowSeesEveryWayOfMakingOne(string code)
    {
        Assert.Matches(PlainWindowPattern(), code);
    }

    [Theory]
    [InlineData("public sealed class RackDialog : Window { }")]
    [InlineData("var owner = Window.GetWindow(this);")]
    [InlineData("var list = new List<Window>();")]
    public void TheGuardOfThePlainWindowDoesNotSeeWhatIsNotOne(string code)
    {
        Assert.DoesNotMatch(PlainWindowPattern(), code);
    }

    // A plain WPF Window made in code: `new Window`, a target-typed new() into a Window (a field, a local or a property) and a Window made
    // by reflection (M-20 of the third review).
    // ------------------------------------------------------------------ the rules of the door to a rack (I-64 A-14, line D)

    [Fact]
    public void AWriteIsWrittenOnlyWhenAViewWasRedrawn()
    {
        // I-08 of the third review: the drawing half of RACKEDITAR can stop before any geometry, or redraw nothing.
        var written = RackDoorRules.AnswerForApply(5, "DIN-1", redrawn: 2, erased: 1);
        Assert.Equal((5L, "DIN-1", true), (written.RequestId, written.RackId, written.Written));
        Assert.Equal("AutoCAD redibujó 2 vistas del rack con el borrador y borró 1 vista que ya no existe; la línea de órdenes dice cuáles.", written.Message);

        var nothing = RackDoorRules.AnswerForApply(6, "DIN-1", redrawn: 0, erased: 0);
        Assert.False(nothing.Written);
        Assert.Equal("AutoCAD no redibujó ninguna vista del rack: el dibujo no cambió (la línea de órdenes dice por qué). El borrador sigue sin escribir.", nothing.Message);

        var erasedOnly = RackDoorRules.AnswerForApply(7, "DIN-1", redrawn: 0, erased: 2);
        Assert.False(erasedOnly.Written);
        Assert.Contains("borró 2 vistas que ya no existen", erasedOnly.Message);
    }

    [Fact]
    public void TheCommandOfTheWriteAnswersWithTheRule()
    {
        var commands = WorkspaceHostSource.CodeOnly(File.ReadAllText(Path.Combine(Root(), "src", "RackCad.Plugin", "VentanaRackCad", "RackCadRackDoorCommands.cs")));
        Assert.Contains("RackDoorRules.AnswerForApply(", commands, StringComparison.Ordinal);
        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), commands);
    }

    private static System.Text.RegularExpressions.Regex PlainWindowPattern() => new(
        @"\bnew\s+(System\.Windows\.)?Window\s*[({]"
        + @"|\b(System\.Windows\.)?Window\??\s+\w+\s*(\{\s*get;[^}]*\}\s*)?=\s*new\s*[({]"
        + @"|typeof\s*\(\s*(System\.Windows\.)?Window\s*\)");

    private static string[] SourcesMatching(System.Text.RegularExpressions.Regex pattern)
    {
        var root = Root();
        return Directory.GetFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(s => s == "bin" || s == "obj"))
            .Where(path => pattern.IsMatch(WorkspaceHostSource.CodeOnly(File.ReadAllText(path))))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }

    private static string Root()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("RackCad.sln not found");
    }
    // ------------------------------------------------------------------ opening a rack of the list

    [Fact]
    public void OpeningARackWithoutATabCreatesAPreviewAndMakesItActive()
    {
        var session = new BrowserSessionRegistry().GetOrCreate(S1)!;

        Assert.Equal(BrowserChange.Created, session.Open("R-1"));

        var tab = Assert.Single(session.Tabs);
        Assert.Equal("R-1", tab.RackId);
        Assert.True(tab.IsPreview);
        Assert.Equal(BrowserTabStatus.NotLoaded, tab.Status);
        Assert.Equal("R-1", session.ActiveRackId);
    }

    [Fact]
    public void OpeningAnotherRackReplacesThePreviewButNeverAPinnedTab()
    {
        var session = new BrowserSessionRegistry().GetOrCreate(S1)!;
        session.Open("R-1");
        session.Pin("R-1");
        session.Open("R-2");

        Assert.Equal(BrowserChange.Replaced, session.Open("R-3"));

        Assert.Equal(new[] { "R-1", "R-3" }, session.Tabs.Select(t => t.RackId).ToArray());
        Assert.False(session.Tabs[0].IsPreview);
        Assert.True(session.Tabs[1].IsPreview);
        Assert.Equal("R-3", session.ActiveRackId);
    }

    [Fact]
    public void OpeningARackThatHasATabActivatesItAndOpeningTheActiveOneChangesNothing()
    {
        var session = new BrowserSessionRegistry().GetOrCreate(S1)!;
        session.Open("R-1");
        session.Pin("R-1");
        session.Open("R-2");

        Assert.Equal(BrowserChange.Activated, session.Open("R-1"));
        Assert.Equal("R-1", session.ActiveRackId);
        Assert.Equal(BrowserChange.None, session.Open("R-1"));
        Assert.Equal(2, session.Tabs.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OpeningNoRackChangesNothing(string? rackId)
    {
        var session = new BrowserSessionRegistry().GetOrCreate(S1)!;

        Assert.Equal(BrowserChange.None, session.Open(rackId));
        Assert.Empty(session.Tabs);
        Assert.Null(session.ActiveRackId);
    }

    [Fact]
    public void OpeningARackDoesNotConsumeTheSelectionSoTheNextRealSelectionStillApplies()
    {
        var session = new BrowserSessionRegistry().GetOrCreate(S1)!;
        var selection = SelectionContext.One("R-1");
        session.ApplySelectionContext(selection);

        session.Open("R-2");
        Assert.Equal("R-2", session.ActiveRackId);

        // The same selection as before is not a transition: it changes nothing.
        Assert.Equal(BrowserChange.None, session.ApplySelectionContext(selection));
        // A new selection is.
        Assert.NotEqual(BrowserChange.None, session.ApplySelectionContext(SelectionContext.One("R-1-B")));
        Assert.Equal("R-1-B", session.ActiveRackId);
    }

    [Fact]
    public void ARetiredSessionOpensNothing()
    {
        var registry = new BrowserSessionRegistry();
        var session = registry.GetOrCreate(S1)!;
        registry.Destroy(S1);

        Assert.Equal(BrowserChange.None, session.Open("R-1"));
        Assert.Empty(session.Tabs);
    }

    // ------------------------------------------------------------------ the classic actions of the window

    [Fact]
    public void EveryClassicActionHasExactlyOneCommandTextEndedByASpace()
    {
        var texts = Enum.GetValues<RackCadClassicAction>().Select(RackCadClassicCommands.TextOf).ToArray();

        Assert.All(texts, text => Assert.EndsWith(" ", text, StringComparison.Ordinal));
        Assert.All(texts, text => Assert.DoesNotContain("\n", text, StringComparison.Ordinal));
        Assert.Equal(texts.Length, texts.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData(RackCadClassicAction.NewSelective, "RACKNUEVO Selectivo ")]
    [InlineData(RackCadClassicAction.NewPushBack, "RACKNUEVO PushBack ")]
    [InlineData(RackCadClassicAction.NewDynamic, "RACKNUEVO Dinamico ")]
    [InlineData(RackCadClassicAction.NewCantilever, "RACKNUEVO Cantilever ")]
    [InlineData(RackCadClassicAction.NewHeader, "RACKNUEVO Cabecera ")]
    [InlineData(RackCadClassicAction.NewFlowBed, "RACKNUEVO Cama ")]
    [InlineData(RackCadClassicAction.NewLarguero, "RACKNUEVO Larguero ")]
    [InlineData(RackCadClassicAction.StructuralSection, "RACKSECCION ")]
    [InlineData(RackCadClassicAction.Variables, "RACKVARIABLES ")]
    [InlineData(RackCadClassicAction.RackList, "RACKLISTA ")]
    [InlineData(RackCadClassicAction.TotalBom, "RACKBOMTOTAL ")]
    [InlineData(RackCadClassicAction.DesignLibrary, "RACKBIBLIOTECA ")]
    [InlineData(RackCadClassicAction.ClassicMenu, "RACKMENU ")]
    [InlineData(RackCadClassicAction.Help, "RACKAYUDA ")]
    public void EachClassicActionRunsItsCommand(RackCadClassicAction action, string text)
    {
        Assert.Equal(text, RackCadClassicCommands.TextOf(action));
    }

    [Fact]
    public void TheKeywordsOfRackNuevoNameTheSevenSystemsOfTheMenuAndMapBackToTheirKind()
    {
        Assert.Equal(new[] { "Selectivo", "PushBack", "Dinamico", "Cantilever", "Cabecera", "Cama", "Larguero" }, RackCadClassicCommands.NewKeywords);

        var expected = new Dictionary<string, RackSystemKind>
        {
            ["Selectivo"] = RackSystemKind.SelectiveRack,
            ["PushBack"] = RackSystemKind.PushBack,
            ["Dinamico"] = RackSystemKind.PalletFlow,
            ["Cantilever"] = RackSystemKind.Cantilever,
            ["Cabecera"] = RackSystemKind.Selective,
            ["Cama"] = RackSystemKind.Cama,
            ["Larguero"] = RackSystemKind.Larguero,
        };
        foreach (var (keyword, kind) in expected)
        {
            Assert.True(RackCadClassicCommands.TryKindOfKeyword(keyword, out var found));
            Assert.Equal(kind, found);
            Assert.True(RackCadClassicCommands.TryKindOfKeyword(" " + keyword.ToUpperInvariant() + " ", out found));
            Assert.Equal(kind, found);
        }

        Assert.False(RackCadClassicCommands.TryKindOfKeyword("DriveIn", out _));
        Assert.False(RackCadClassicCommands.TryKindOfKeyword(null, out _));
    }

    [Fact]
    public void TheCommandsOfTheDoorsAreRegisteredInTheMenuFileOnceEach()
    {
        var menu = WorkspaceHostSource.Code("src/RackCad.Plugin/RackMenuCommands.cs");

        foreach (var command in new[] { RackCadClassicCommands.NewCommand, RackCadClassicCommands.LibraryCommand, RackCadClassicCommands.ClassicMenuCommand })
            Assert.Equal(1, WorkspaceHostSource.Count(menu, "\\[CommandMethod\\(\"" + command + "\"\\)\\]"));

        // RACKCAD and RK open the window, which is what RACKPANEL shows; the classic menu is RACKMENU.
        Assert.Matches(@"public\s+void\s+RackCad\s*\(\s*\)\s*\{\s*try\s*\{\s*Workspace\.WorkspacePanelCommand\.ShowRackCadWindow\s*\(\s*\)\s*;", menu);
        Assert.Matches(@"\[CommandMethod\(""RACKMENU""\)\]\s*public\s+void\s+RackMenu\s*\(\s*\)", menu);
        // The three doors draw the request with the one dispatch of the menu.
        Assert.Equal(3, WorkspaceHostSource.Count(menu, @"\bDispatchInsertion\s*\(\s*document\s*,"));
    }
}
