#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T2B1-INTENT-READ: INV-F4T2B1-01 (purity), 08 (one read) and 13 (the new guards). These guards are ADDITIVE: no existing
// guard is modified or weakened. They read the sources as text, like the rest of the guards of the workspace.
public class WorkspaceDraftPilotIntentGuardTests
{
    private const string PilotFolder = "src/RackCad.Application/Systems/PushBack/Pilot/";
    private const string ScanPath = PilotFolder + "PushBackPilotIntentScan.cs";
    private const string ReaderPath = PilotFolder + "PushBackPilotIntentReader.cs";
    private const string ResultPath = PilotFolder + "PushBackPilotIntentResult.cs";
    private const string PilotNamespace = "RackCad.Application.Systems.PushBack.Pilot";

    private static readonly string[] ProductFiles = { ScanPath, ReaderPath, ResultPath };

    // ------------------------------------------------------------------ INV-F4T2B1-01: purity

    // Each prohibition with the line that must trip it. The text of the product is read raw, comments included: nothing of the
    // three files may even mention these names.
    private static readonly (string Name, Regex Pattern, string Injected)[] Prohibited =
    {
        ("Autodesk", new Regex(@"Autodesk"), "using Autodesk.AutoCAD.DatabaseServices;"),
        ("RackCad.Plugin", new Regex(@"RackCad\.Plugin"), "using RackCad.Plugin.Views;"),
        ("RackCad.UI", new Regex(@"RackCad\.UI"), "using RackCad.UI.Workspace;"),
        ("System.Windows", new Regex(@"System\.Windows"), "using System.Windows.Threading;"),
        ("System.IO", new Regex(@"System\.IO"), "using System.IO;"),
        ("Transaction", new Regex(@"Transaction"), "var t = db.TransactionManager.StartTransaction();"),
        ("LockDocument", new Regex(@"LockDocument"), "using (doc.LockDocument()) { }"),
        ("Dispatcher", new Regex(@"Dispatcher"), "Dispatcher.CurrentDispatcher.Invoke(() => { });"),
        ("Task", new Regex(@"\bTask\b"), "Task.Run(() => { });"),
        // The threading prohibition is aligned with WorkspaceDraftBoundaryTests (Task., ValueTask and System.Threading.Tasks).
        ("Task.", new Regex(@"Task\."), "var done = Task.CompletedTask;"),
        ("ValueTask", new Regex(@"ValueTask"), "ValueTask<int> pending = default;"),
        ("System.Threading.Tasks", new Regex(@"System\.Threading\.Tasks"), "using System.Threading.Tasks;"),
        ("async", new Regex(@"\basync\b"), "public async void Go() { }"),
        ("await", new Regex(@"\bawait\b"), "var x = await Work();"),
    };

    [Fact]
    public void TheThreeProductFilesExistInThePilotNamespaceAndAreNullableEnabled()
    {
        foreach (var path in ProductFiles)
        {
            var text = Src.Text(path);

            Assert.StartsWith("#nullable enable", text);
            var namespaces = Regex.Matches(Src.CodeOnly(text), @"^\s*namespace\s+([\w\.]+)\s*(\{|;)", RegexOptions.Multiline);
            var declared = Assert.Single(namespaces);
            Assert.Equal(PilotNamespace, declared.Groups[1].Value);
            Assert.Equal("{", declared.Groups[2].Value);
        }
    }

    [Fact]
    public void TheThreeProductFilesNameNothingOfAutoCadOfThePluginOfTheUiOfThreadingOrOfInputOutput()
    {
        foreach (var path in ProductFiles)
        {
            var text = Src.Text(path);
            Assert.NotEmpty(text);

            foreach (var (name, pattern, _) in Prohibited)
                Assert.True(!pattern.IsMatch(text), path + " names " + name);
        }
    }

    [Fact]
    public void ThePurityGuardDetectsEachProhibitionInTheRealTextOfEachProductFile()
    {
        foreach (var path in ProductFiles)
        {
            var real = Src.Text(path);
            foreach (var (name, pattern, injected) in Prohibited)
            {
                Assert.DoesNotMatch(pattern, real);
                Assert.True(pattern.IsMatch(real + "\n" + injected), path + " / " + name);
            }
        }
    }

    [Fact]
    public void ThePurityGuardDoesNotRejectNamesThatOnlyContainTheProhibitedWordsInsideOtherWords()
    {
        var harmless = "var tasks = 1; var asyncLike = 2; var awaiting = 3; var Tasked = 4;";

        foreach (var (name, pattern, _) in Prohibited.Where(p => p.Name is "Task" or "async" or "await"))
            Assert.True(!pattern.IsMatch(harmless), name);
    }

    // ------------------------------------------------------------------ INV-F4T2B1-08: one read

    private static readonly (string Name, string Needle, string Injected)[] NeverRead =
    {
        ("Entries", ".Entries", "var all = staleBase.Entries;"),
        ("EnvelopeText", ".EnvelopeText", "var text = entry.EnvelopeText;"),
        ("MembershipOf(", "MembershipOf(", "var m = staleBase.MembershipOf(key);"),
    };

    [Fact]
    public void NoProductFileReadsTheBaseBackToMaterializeTheState()
    {
        foreach (var path in ProductFiles)
        {
            var text = Src.Text(path);

            foreach (var (name, needle, _) in NeverRead)
                Assert.True(!text.Contains(needle, StringComparison.Ordinal), path + " contains " + name);
        }
    }

    [Fact]
    public void TheReadGuardDetectsEachReadInTheRealTextOfEachProductFile()
    {
        foreach (var path in ProductFiles)
        {
            var real = Src.Text(path);
            foreach (var (name, needle, injected) in NeverRead)
            {
                Assert.DoesNotContain(needle, real, StringComparison.Ordinal);
                Assert.Contains(needle, real + "\n" + injected, StringComparison.Ordinal);
            }
        }
    }

    private static int Count(string text, string pattern) => Regex.Matches(text, pattern).Count;

    private const string ClassifyCall = @"RackSiblingMembership\.Classify\(";
    private const string ComparatorCall = @"RackAuthoredComparatorPorts\.PushBack\(\)";
    private const string MaterializeCall = @"PushBackPilotMaterializer\.Materialize\(";

    [Fact]
    public void TheReaderCallsTheClassificationTheAuthoredComparatorAndTheMaterializerExactlyOnceEach()
    {
        var reader = Src.Text(ReaderPath);

        Assert.Equal(1, Count(reader, ClassifyCall));
        Assert.Equal(1, Count(reader, ComparatorCall));
        Assert.Equal(1, Count(reader, MaterializeCall));
    }

    [Fact]
    public void NoOtherProductFileOfTheTaskCallsThem()
    {
        foreach (var path in new[] { ScanPath, ResultPath })
        {
            var text = Src.Text(path);

            Assert.Equal(0, Count(text, ClassifyCall));
            Assert.Equal(0, Count(text, ComparatorCall));
            Assert.Equal(0, Count(text, MaterializeCall));
        }
    }

    [Fact]
    public void TheCountGuardDetectsASecondCallInTheRealTextOfTheReader()
    {
        var reader = Src.Text(ReaderPath);

        Assert.Equal(2, Count(reader + "\nvar a = RackSiblingMembership.Classify(f, r, null, false);", ClassifyCall));
        Assert.Equal(2, Count(reader + "\nvar b = RackAuthoredComparatorPorts.PushBack();", ComparatorCall));
        Assert.Equal(2, Count(reader + "\nvar c = PushBackPilotMaterializer.Materialize(t, catalog);", MaterializeCall));
        Assert.Equal(0, Count(reader.Replace("RackSiblingMembership.Classify(", "X(", StringComparison.Ordinal), ClassifyCall));
    }

    // ------------------------------------------------------------------ obligation 3: a preview has no way to the intention

    private static IReadOnlyList<string> PreviewFilesOfTheBoundaryGuard()
    {
        var field = typeof(WorkspaceDraftBoundaryTests).GetField("PreviewFiles", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(field);
        return Assert.IsType<string[]>(field!.GetValue(null));
    }

    private static readonly Regex IntentNamedByAPreview = new(@"PushBackPilotIntent|RackCad\.Application\.Systems\.PushBack\.Pilot");

    [Fact]
    public void TheFilesThatServeAPreviewAreTheFourOfTheBoundaryGuard()
    {
        var files = PreviewFilesOfTheBoundaryGuard();

        Assert.Equal(4, files.Count);
        Assert.Equal(
            new[]
            {
                "src/RackCad.Application/Workspace/BrowserRackSummaries.cs",
                "src/RackCad.Application/Workspace/BrowserSession.cs",
                "src/RackCad.Application/Workspace/BrowserSessionRegistry.cs",
                "src/RackCad.Application/Workspace/IWorkspaceBrowserSummaryPort.cs",
            },
            files.OrderBy(f => f, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void NoFileThatServesAPreviewNamesTheIntentOfThePilotNorItsNamespace()
    {
        foreach (var path in PreviewFilesOfTheBoundaryGuard())
        {
            var text = Src.Text(path);
            Assert.NotEmpty(text);

            Assert.True(!IntentNamedByAPreview.IsMatch(text), path + " names " + IntentNamedByAPreview.Match(text).Value);
        }
    }

    [Theory]
    [InlineData("PushBackPilotIntentScan scan = null;")]
    [InlineData("var r = PushBackPilotIntentReader.Read(scan, catalog);")]
    [InlineData("PushBackPilotIntentResult result = null;")]
    [InlineData("using RackCad.Application.Systems.PushBack.Pilot;")]
    [InlineData("var s = new RackCad.Application.Systems.PushBack.Pilot.PushBackPilotDraftState(e, i, o);")]
    public void ThePreviewGuardDetectsEachNameInTheRealTextOfEachPreviewFile(string injected)
    {
        foreach (var path in PreviewFilesOfTheBoundaryGuard())
        {
            var real = Src.Text(path);

            Assert.DoesNotMatch(IntentNamedByAPreview, real);
            Assert.Matches(IntentNamedByAPreview, real + "\n" + injected);
        }
    }

    [Fact]
    public void TheDraftCoreStillNamesNoSystem()
    {
        var files = Src.Files("src/RackCad.Application/Workspace/Drafts/");

        Assert.NotEmpty(files);
        foreach (var (path, code) in files)
        {
            Assert.DoesNotContain("PushBack", code, StringComparison.Ordinal);
            Assert.DoesNotContain("Pilot", code, StringComparison.Ordinal);
            Assert.DoesNotContain("RackCad.Application.Systems", code, StringComparison.Ordinal);
        }
    }
}
