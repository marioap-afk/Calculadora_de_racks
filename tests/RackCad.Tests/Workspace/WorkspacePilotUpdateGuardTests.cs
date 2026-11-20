#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.Systems.PushBack.Pilot;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T4B-MUTATE-POST (D-T4B-04 and D-T4B-09; INV-F4T4B-04, 07, 12, 14, 15, 16 and 17): the source guards of the update of the pilot. The Core
// test project cannot reference the Plugin (it needs AutoCAD), so the writer and the traversal are read as text. Every guard is followed by the
// proof that it trips on the real text with ONE injected change: a guard that never fails watches nothing. They read the code without its
// comments unless a test says that it reads the raw text.
public class WorkspacePilotUpdateGuardTests
{
    private const string PilotFolder = "src/RackCad.Application/Systems/PushBack/Pilot/";
    private const string PortPath = PilotFolder + "IPushBackPilotUpdatePort.cs";
    private const string RunPath = PilotFolder + "PushBackPilotUpdateRun.cs";
    private const string RebuildPath = PilotFolder + "PushBackPilotSystemRebuild.cs";
    private const string WriterPath = "src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs";
    private const string ScanPath = "src/RackCad.Plugin/Views/RackSiblingScan.cs";
    private const string CorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string MaterializerPath = PilotFolder + "PushBackPilotMaterializer.cs";

    private static readonly string[] PureFiles = { PortPath, RunPath, RebuildPath };

    private static string Writer() => Src.Code(WriterPath);

    private static string Class(string code, string name) => Src.BodyOf(code, @"internal\s+sealed\s+class\s+" + name + @"\b");

    private static string PortClass(string code) => Class(code, "RackPilotUpdateWriter");

    private static string UpdateScope(string code) => Class(code, "RackPilotUpdateScope");

    private static string MutationScope(string code) => Class(code, "RackPilotMutationScope");

    private static string Method(string classBody, string signature) => Src.BodyOf(classBody, signature);

    private static void AssertInOrder(string text, params string[] needles)
    {
        var previous = -1;
        foreach (var needle in needles)
        {
            var at = text.IndexOf(needle, StringComparison.Ordinal);
            Assert.True(at >= 0, "missing: " + needle);
            Assert.True(at > previous, "out of order: " + needle);
            previous = at;
        }
    }

    // ------------------------------------------------------------------ the purity of the run, the port and the help (INV-F4T4B-12)

    // Each prohibition with the line that must trip it. The text is read RAW, comments included: nothing of the three files may even mention these.
    private static readonly (string Name, Regex Pattern, string Injected)[] Impure =
    {
        ("Transaction", new Regex(@"Transaction"), "var t = db.TransactionManager;"),
        ("Database", new Regex(@"\bDatabase\b"), "Database db = null;"),
        ("Commit", new Regex(@"\.\s*Commit\s*\("), "tr.Commit();"),
        ("LockDocument", new Regex(@"LockDocument"), "using (doc.LockDocument()) { }"),
        ("Autodesk", new Regex(@"Autodesk"), "using Autodesk.AutoCAD.DatabaseServices;"),
        ("RackCad.Plugin", new Regex(@"RackCad\.Plugin"), "using RackCad.Plugin.Views;"),
        ("RackCad.UI", new Regex(@"RackCad\.UI"), "using RackCad.UI.Workspace;"),
        ("System.Windows", new Regex(@"System\.Windows"), "using System.Windows.Threading;"),
        ("async", new Regex(@"\basync\b"), "public async void Go() { }"),
        ("await", new Regex(@"\bawait\b"), "var x = await Work();"),
        ("Task", new Regex(@"\bTask\b|Task\.|ValueTask"), "var done = Task.CompletedTask;"),
        ("Dispatcher", new Regex(@"Dispatcher"), "Dispatcher.CurrentDispatcher.Invoke(() => { });"),
        ("files", new Regex(@"System\.IO|\bFile\.|\bDirectory\.|\bStream\b"), "var text = File.ReadAllText(path);"),
        ("reflection", new Regex(@"System\.Reflection|\btypeof\s*\(|\.GetMethod\s*\(|\bActivator\."), "var m = typeof(X).GetMethod(\"Y\");"),
    };

    [Fact]
    public void TheRunThePortAndTheHelpAreNullableEnabledAndInThePilotNamespace()
    {
        foreach (var path in PureFiles)
        {
            var text = Src.Text(path);

            Assert.StartsWith("#nullable enable", text.TrimStart((char)0xFEFF));
            var namespaces = Regex.Matches(Src.CodeOnly(text), @"^\s*namespace\s+([\w\.]+)\s*(\{|;)", RegexOptions.Multiline);
            var declared = Assert.Single(namespaces);
            Assert.Equal("RackCad.Application.Systems.PushBack.Pilot", declared.Groups[1].Value);
        }
    }

    [Fact]
    public void TheRunThePortAndTheHelpNameNothingOfAutoCadOfThePluginOfTheUiOfThreadingOfFilesOrOfReflectionEvenInComments()
    {
        foreach (var path in PureFiles)
        {
            var text = Src.Text(path);
            foreach (var (name, pattern, _) in Impure) Assert.True(!pattern.IsMatch(text), path + " names " + name);
        }

        // The cause of a foreign scope is a constant whose identifier does not carry the word that the guard above forbids.
        Assert.Equal("TOP_TRANSACTION_OPEN", PushBackPilotUpdateRun.ForeignScopeOpenCause);
        Assert.DoesNotMatch(new Regex(@"Transaction"), nameof(PushBackPilotUpdateRun.ForeignScopeOpenCause));
    }

    [Fact]
    public void ThePurityGuardTripsOnEachProhibitionInTheRealTextOfEachFile()
    {
        foreach (var path in PureFiles)
        {
            var real = Src.Text(path);
            foreach (var (name, pattern, injected) in Impure)
            {
                Assert.DoesNotMatch(pattern, real);
                Assert.True(pattern.IsMatch(real + "\n" + injected), path + " / " + name);
            }
        }
    }

    [Fact]
    public void NoFileOfThePilotFolderNamesTheWordTransactionNotEvenInAComment()
    {
        var files = Src.Files(PilotFolder.TrimEnd('/')).Select(f => f.Path).ToList();

        Assert.True(files.Count >= 11);
        foreach (var path in files) Assert.DoesNotMatch(new Regex(@"Transaction"), Src.Text(path));
    }

    [Fact(Skip = "I-64 F4B PB-T3b: sustituida por E-4 de PushBackPilotUpdateEquivalenceGuardTests (registro declarado superseding-characterization.md; decisión del Owner F4B-E29, D-1).")]
    public void TheRunReleasesBothScopesInFinallyBlocksOfItsOwnAndNeverWithAUsing()
    {
        var code = Src.Code(RunPath);

        Assert.DoesNotMatch(new Regex(@"\busing\s*\(|\busing\s+var\b"), code);
        Assert.Matches(@"finally\s*\{\s*try\s*\{\s*mutation\.Dispose\s*\(\s*\)\s*;\s*\}\s*catch\s*\(\s*Exception\s+ex\s*\)", code);
        Assert.Matches(@"finally\s*\{\s*if\s*\(\s*scope\s*!=\s*null\s*\)\s*\{\s*try\s*\{\s*scope\.Dispose\s*\(\s*\)\s*;\s*\}\s*catch\s*\(\s*Exception\s+ex\s*\)", code);
        Assert.Equal(1, Src.Count(code, @"\bmutation\.Dispose\s*\("));
        Assert.Equal(1, Src.Count(code, @"\bscope\.Dispose\s*\("));
        // The scope of MUTATE is released inside the run of the scope of the run, and POST is reached only after that release.
        var release = code.IndexOf("mutation.Dispose()", StringComparison.Ordinal);
        var goesToPost = code.IndexOf("return null;", release, StringComparison.Ordinal);
        Assert.True(release > 0 && goesToPost > release, "the scope of MUTATE is released before the run says that POST follows");
        Assert.True(code.IndexOf("private DraftApplyOutcome<PushBackPilotDraftState> Post(IPushBackPilotUpdateScope scope)", goesToPost, StringComparison.Ordinal) > goesToPost);
    }

    [Fact]
    public void ThePortHasTheOperationsAndTheValuesOfThePackageAndNoWordOfAutoCad()
    {
        var code = Src.Code(PortPath);

        Assert.Matches(@"IPushBackPilotUpdateScope\s+Begin\s*\(\s*PushBackPilotUpdateRequest\s+request\s*\)", code);
        Assert.Matches(@"PushBackPilotScanValues\s+ReadForPrepare\s*\(\s*\)", code);
        Assert.Matches(@"PushBackPilotImportResult\s+PrepareUnits\s*\(\s*PushBackPilotUpdatePlan\s+plan\s*\)", code);
        Assert.Matches(@"PushBackPilotMutationStart\s+BeginMutation\s*\(\s*\)", code);
        Assert.Matches(@"PushBackPilotPostValues\s+Post\s*\(\s*PushBackPilotPostRequest\s+request\s*\)", code);
        Assert.Matches(@"PushBackPilotScanValues\s+Reread\s*\(\s*\)", code);
        Assert.Matches(@"PushBackPilotUnitResult\s+WriteUnit\s*\(\s*string\s+memberKey\s*,\s*string\s+envelopeText\s*\)", code);
        Assert.Matches(@"void\s+Seal\s*\(\s*\)", code);
        Assert.Matches(@"interface\s+IPushBackPilotUpdateScope\s*:\s*IDisposable", code);
        Assert.Matches(@"interface\s+IPushBackPilotMutationScope\s*:\s*IDisposable", code);
        Assert.Matches(@"record\s+PushBackPilotUpdateRequest\s*\(\s*long\s+AuthorizedDocumentNativeId\s*,\s*string\s+RackId\s*\)", code);
    }

    // ------------------------------------------------------------------ the writer, anchored by body (D-T4B-09)

    // I-64 F4-T4C1-HOST (D-T4C-07, D-T4C-20 row 13): exactly these parts are taken out of the text of the port before the prohibitions are
    // applied to the rest: the constructor with the exact document, the two declarations (the document, read-only, and the mark of one use),
    // the line of the retirement and the lines of Begin that check and set the mark and hand the document to the scope. The prohibitions
    // are the SAME expression as before, applied to what is left.
    private static readonly string[] AllowedPortParts =
    {
        @"internal\s+RackPilotUpdateWriter\s*\(\s*Document\s+document\s*\)\s*\{[^{}]*\}",
        @"private\s+readonly\s+Document\s+_document\s*;",
        @"private\s+bool\s+_spent\s*;",
        @"internal\s+void\s+Retire\s*\(\s*\)\s*=>\s*_spent\s*=\s*true\s*;",
        @"if\s*\(\s*_spent\s*\)\s*throw\s+new\s+InvalidOperationException\s*\([^;]*\)\s*;",
        @"_spent\s*=\s*true\s*;",
        @"return\s+new\s+RackPilotUpdateScope\s*\(\s*_document\s*,\s*request\s*\)\s*;",
    };

    private static string PortWithoutItsAllowedParts(string port) =>
        AllowedPortParts.Aggregate(port, (text, pattern) => Regex.Replace(text, pattern, string.Empty));

    // What the writer NEVER does. These are prohibitions: they hold for any text of the three classes, so they hold for a writer that does nothing.
    private static IEnumerable<string> WriterProhibitionViolations(string code)
    {
        // It only reads through the traversal of the nucleus.
        foreach (var banned in new[] { @"RackBlockData\.Read", @"\bBlockTableId\b", @"GetBlockReferenceIds" })
            if (Regex.IsMatch(code, banned)) yield return "the writer names " + banned;

        // Never: the lock, an erasure, the guard of units, a context of execution, the navigation, the names and the texts of the warnings.
        foreach (var banned in new[] { @"LockDocument", @"\bErase\s*\(", @"EraseViewBlocks", @"RackUnitsGuard", @"\basync\b", @"\bawait\b", @"ExecuteInCommandContextAsync", @"NavigationTargets", @"BlockNaming", @"RackViewBaseName" })
            if (Regex.IsMatch(code, banned)) yield return "the writer names " + banned;
        foreach (var text in new[] { PushBackPilotUpdateRun.MissingPiecePrefix, PushBackPilotUpdateRun.NameDiffersPrefix, PushBackPilotUpdateRun.RegenFailedPrefix })
            if (code.Contains(text, StringComparison.Ordinal)) yield return "the writer carries the text of a warning: " + text;

        // The writer never opens anything for writing by itself, and the purge only goes through the one entry of the redraw paths.
        if (Regex.IsMatch(code, @"\bForWrite\b|\bUpgradeOpen\b|\bAppendEntity\b")) yield return "the writer opens something for writing by itself";
        if (Regex.IsMatch(code, @"LateralHeaderDrawer\s*\.\s*PurgeUnreferenced|\.Purge\s*\(")) yield return "the purge does not go through PurgeAfterCommit";

        // PREPARE only in PrepareUnits: nothing prepares a redraw outside its body.
        var update = UpdateScope(code);
        var prepare = Method(update, @"public\s+PushBackPilotImportResult\s+PrepareUnits\s*\(");
        if (Src.Count(code, @"\bPrepareRedraw\s*\(") != Src.Count(prepare, @"\bPrepareRedraw\s*\(")) yield return "PrepareRedraw is outside the body of PrepareUnits";

        // INV-14: no object of AutoCAD outside the two classes of scope, and the port has no field at all.
        var port = PortWithoutItsAllowedParts(PortClass(code));
        if (Regex.IsMatch(port, @"\b(?:Transaction|DBObject|Document|ObjectId|Database|PreparedViewRedraw)\b|staleDefinitions")) yield return "the port names an object of AutoCAD";
        if (Regex.IsMatch(port, @"\bprivate\b|\breadonly\b|\bstatic\s+[^(){};=]*[;=]")) yield return "the port has a field";
        if (Regex.IsMatch(code, @"MdiActiveDocument")) yield return "the writer resolves the active document by itself";

        // The scope of MUTATE never purges, renames, regenerates or reads the whole drawing.
        var mutation = MutationScope(code);
        if (Regex.IsMatch(mutation, @"PurgeAfterCommit|PurgeUnreferenced|\.Purge\s*\(|SyncName|ApplyRegen|CaptureDrawing")) yield return "the scope of MUTATE purges, renames, regenerates or reads the whole drawing";
    }

    // What the writer DOES, in the order of the package. These fail for a writer that does nothing.
    private static IEnumerable<string> WriterStructureViolations(string code)
    {
        var update = UpdateScope(code);
        var mutation = MutationScope(code);
        var prepare = Method(update, @"public\s+PushBackPilotImportResult\s+PrepareUnits\s*\(");
        var begin = Method(update, @"public\s+PushBackPilotMutationStart\s+BeginMutation\s*\(");
        var post = Method(update, @"public\s+PushBackPilotPostValues\s+Post\s*\(");
        var seal = Method(mutation, @"public\s+void\s+Seal\s*\(");

        // PREPARE: the three facades are prepared in PrepareUnits.
        if (Src.Count(prepare, @"\bPrepareRedraw\s*\(") != 3) yield return "PrepareUnits does not prepare the three facades";

        // MUTATE: ONE scope, started after the check of the top one, and nothing else starts one.
        if (Src.Count(code, @"\bStartTransaction\s*\(") != 1) yield return "StartTransaction is not exactly one";
        if (Src.Count(begin, @"\bStartTransaction\s*\(") != 1) yield return "StartTransaction is not in BeginMutation";
        if (!Regex.IsMatch(begin, @"TopTransaction\s*!=\s*null[\s\S]*StartTransaction\s*\(")) yield return "the top scope is not checked before the start";
        if (Src.Count(code, @"\bStartOpenCloseTransaction\s*\(") != 1 || Src.Count(post, @"\bStartOpenCloseTransaction\s*\(") != 1) yield return "the read of the names is not one scope in Post";
        if (Src.Count(code, @"\.Commit\s*\(") != 1 || Src.Count(seal, @"\.Commit\s*\(") != 1) yield return "Commit is not exactly one, in Seal";

        // The port is the port of ONE run: it has exactly its two fields, it begins once and the target retires it. Begin checks the mark of one
        // use before it sets it, and hands the exact document to the scope (D-T4C-07).
        var portClass = PortClass(code);
        if (Src.Count(portClass, @"private\s+readonly\s+Document\s+_document\s*;") != 1 || Src.Count(portClass, @"private\s+bool\s+_spent\s*;") != 1) yield return "the port does not have exactly its two fields";
        if (Src.Count(portClass, @"\bprivate\b") != 2) yield return "the port has more than its two declarations";
        if (Src.Count(portClass, @"internal\s+void\s+Retire\s*\(\s*\)\s*=>\s*_spent\s*=\s*true\s*;") != 1) yield return "the port does not retire itself";
        var portBegin = Method(portClass, @"public\s+IPushBackPilotUpdateScope\s+Begin\s*\(");
        if (!Regex.IsMatch(portBegin, @"if\s*\(\s*_spent\s*\)\s*throw\s+new\s+InvalidOperationException[\s\S]*_spent\s*=\s*true\s*;[\s\S]*return\s+new\s+RackPilotUpdateScope\s*\(\s*_document\s*,\s*request\s*\)\s*;")) yield return "Begin does not check and set the mark of one use before it hands the document to the scope";

        // The port hands back a new scope each time, and the scope of MUTATE releases its scope of the drawing.
        if (!Regex.IsMatch(Method(PortClass(code), @"public\s+IPushBackPilotUpdateScope\s+Begin\s*\("), @"return\s+new\s+RackPilotUpdateScope\s*\(")) yield return "Begin does not hand back a new scope each time";
        if (!Regex.IsMatch(Method(mutation, @"public\s+void\s+Dispose\s*\("), @"_transaction\.Dispose\s*\(\s*\)")) yield return "Dispose does not release the scope of the drawing";

        // INV-16: the purge goes once, in Post, after the check of the top scope, and the order of Post is the one of the classic editor.
        if (Src.Count(code, @"\bPurgeAfterCommit\s*\(") != 1 || Src.Count(post, @"\bPurgeAfterCommit\s*\(") != 1) yield return "PurgeAfterCommit is not exactly one, in Post";
        foreach (var once in new[] { @"TopTransaction", @"PurgeAfterCommit\s*\(", @"RackBlockRenamer\.SyncName\s*\(", @"StartOpenCloseTransaction\s*\(", @"SystemBlockWriter\.ApplyRegen\s*\(", @"RackSiblingScan\.CaptureDrawing\s*\(" })
            if (Src.Count(post, once) != 1) yield return "Post does not have exactly one " + once;
        var positions = new[] { @"TopTransaction", @"PurgeAfterCommit\s*\(", @"SyncName\s*\(", @"StartOpenCloseTransaction\s*\(", @"ApplyRegen\s*\(", @"CaptureDrawing\s*\(" }
            .Select(pattern => Regex.Match(post, pattern).Index).ToArray();
        for (var i = 1; i < positions.Length; i++)
            if (positions[i] <= positions[i - 1]) yield return "the order of Post is not check, purge, rename, read of the names, regeneration, re-read";
    }

    [Fact]
    public void TheWriterHasItsThreeClassesAndNeverNamesWhatItMustNotName()
    {
        Assert.Empty(WriterProhibitionViolations(Writer()));
        Assert.Equal(3, Src.Count(Writer(), @"\bclass\s+RackPilot\w+"));
        Assert.Matches(@"internal\s+sealed\s+class\s+RackPilotUpdateWriter\s*:\s*IPushBackPilotUpdatePort\b", Writer());
        Assert.Matches(@"internal\s+sealed\s+class\s+RackPilotUpdateScope\s*:\s*IPushBackPilotUpdateScope\b", Writer());
        Assert.Matches(@"internal\s+sealed\s+class\s+RackPilotMutationScope\s*:\s*IPushBackPilotMutationScope\b", Writer());
    }

    // M-10: since D-T4C-07 the writer no longer resolves the active document, so the alias of the application that it only used for that is gone.
    [Fact]
    public void TheWriterDoesNotKeepTheAliasOfTheApplicationThatOnlyTheActiveDocumentNeeded()
    {
        var real = Writer();

        Assert.DoesNotContain("AcApplication", real, StringComparison.Ordinal);
        Assert.Empty(WriterProhibitionViolations(real));
    }

    [Fact]
    public void TheWriterDoesThePrimitiveOperationsOfThePackageInsideItsScopesInTheOrderOfThePackage()
    {
        Assert.Empty(WriterStructureViolations(Writer()));
    }

    // I-64 F4-T4-HOST-C1 (D-C1-01, INV-C1-01, test 2 of D-C1-09): the system that the writer redraws is the one of the member WITH the rack name of
    // its envelope. PrepareUnits builds it with ONE call of the help of the member, and the writer does not name the help that has no name.
    private static IEnumerable<string> RackNameViolations(string code)
    {
        var prepare = Method(UpdateScope(code), @"public\s+PushBackPilotImportResult\s+PrepareUnits\s*\(");

        if (Src.Count(prepare, @"\bPushBackPilotSystemRebuild\s*\.\s*BuildForMember\s*\(\s*member\s*,\s*_catalog\s*\)") != 1) yield return "PrepareUnits does not build the system with exactly one BuildForMember(member, _catalog)";
        if (Src.Count(code, @"\bBuildForMember\b") != 1) yield return "BuildForMember is named outside PrepareUnits or more than once";
        if (Src.Count(code, @"\bPushBackPilotSystemRebuild\s*\.\s*Build\s*\(") != 0) yield return "the writer names PushBackPilotSystemRebuild.Build";
        if (Src.Count(code, @"\bPushBackPilotSystemRebuild\b") != 1) yield return "the writer names PushBackPilotSystemRebuild other than in the one call of PrepareUnits";
    }

    [Fact]
    public void PrepareUnitsBuildsTheSystemOfEachMemberWithItsRackName()
    {
        var real = Writer();
        const string call = "PushBackPilotSystemRebuild.BuildForMember(member, _catalog)";

        Assert.Empty(RackNameViolations(real));
        Assert.Contains(call, real, StringComparison.Ordinal);
        // Without the name: the old help of the design alone.
        Assert.NotEmpty(RackNameViolations(real.Replace(call, "PushBackPilotSystemRebuild.Build(member.DesignText, _catalog)", StringComparison.Ordinal)));
        // Two builds of the system, or the build apart from the member of the loop, or the call outside PrepareUnits.
        Assert.NotEmpty(RackNameViolations(real.Replace(call, call + ", " + call, StringComparison.Ordinal)));
        Assert.NotEmpty(RackNameViolations(real.Replace(call, "PushBackPilotSystemRebuild.BuildForMember(plan.Members[0], _catalog)", StringComparison.Ordinal)));
        Assert.NotEmpty(RackNameViolations(real.Replace("internal sealed class RackPilotMutationScope", "var other = " + call + ";\ninternal sealed class RackPilotMutationScope", StringComparison.Ordinal)));
        Assert.NotEmpty(RackNameViolations(real.Replace(call, "_ = 0", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("var r = RackBlockData.Read(transaction, id);")]
    [InlineData("var t = document.Database.BlockTableId;")]
    [InlineData("var ids = definition.GetBlockReferenceIds(true, true);")]
    [InlineData("using (document.LockDocument()) { }")]
    [InlineData("entity.Erase();")]
    [InlineData("RackCommandSupport.EraseViewBlocks(document, ids);")]
    [InlineData("RackUnitsGuard.WarnIfNotInches(document);")]
    [InlineData("var n = BlockNaming.SanitizeBlockName(x);")]
    [InlineData("var n = RackViewBaseName.LinkedPlanta(x);")]
    [InlineData("definition.UpgradeOpen();")]
    [InlineData("LateralHeaderDrawer.PurgeUnreferenced(database, ids);")]
    [InlineData("var targets = NavigationTargets.Resolve(x);")]
    [InlineData("var p = new PushBackPlantaDrawService().PrepareRedraw(database, id, system, text);")]
    public void TheProhibitionGuardOfTheWriterTripsOnEachProhibitedNameInjectedIntoTheRealTextOfTheWriter(string injected)
    {
        var real = Writer();

        Assert.Empty(WriterProhibitionViolations(real));
        Assert.NotEmpty(WriterProhibitionViolations(real.Replace("internal sealed class RackPilotMutationScope", injected + "\ninternal sealed class RackPilotMutationScope", StringComparison.Ordinal)));
    }

    // D-T4C-20 row 13: the new negatives, apart from the four that stay. The port admits exactly its two declarations, its constructor, its
    // retirement and the lines of Begin, and nothing else: a second document, a transaction, a static field, a Begin that does not check the
    // mark, a port that cannot retire itself and the active document resolved by the writer all trip the guard.
    [Theory]
    [InlineData("private readonly Document _other;")]
    [InlineData("private Document _second;")]
    [InlineData("private Transaction _transaction;")]
    [InlineData("private static int _count;")]
    [InlineData("private readonly DBObject _object;")]
    public void TheWriterGuardTripsOnASecondDocumentATransactionOrAStaticFieldInThePort(string injected)
    {
        var real = Writer();
        var head = Regex.Match(real, @"internal\s+sealed\s+class\s+RackPilotUpdateWriter\s*:\s*IPushBackPilotUpdatePort\s*\{");
        Assert.True(head.Success);

        Assert.Empty(WriterProhibitionViolations(real));
        Assert.Empty(WriterStructureViolations(real));
        var mutated = real.Insert(head.Index + head.Length, " " + injected);
        Assert.True(WriterProhibitionViolations(mutated).Any() || WriterStructureViolations(mutated).Any(), injected);
    }

    [Fact]
    public void TheWriterGuardTripsOnABeginThatDoesNotCheckTheMarkOfOneUseAndOnAPortThatCannotRetireItself()
    {
        var real = Writer();
        var check = Regex.Match(real, @"if\s*\(\s*_spent\s*\)\s*throw\s+new\s+InvalidOperationException\s*\([^;]*\)\s*;");
        var retire = Regex.Match(real, @"internal\s+void\s+Retire\s*\(\s*\)\s*=>\s*_spent\s*=\s*true\s*;");
        Assert.True(check.Success && retire.Success);

        Assert.NotEmpty(WriterStructureViolations(real.Remove(check.Index, check.Length)));
        Assert.NotEmpty(WriterStructureViolations(real.Remove(retire.Index, retire.Length)));
        Assert.NotEmpty(WriterStructureViolations(real.Replace("return new RackPilotUpdateScope(_document, request);", "return new RackPilotUpdateScope(null, request);", StringComparison.Ordinal)));
        Assert.NotEmpty(WriterProhibitionViolations(real.Replace("return new RackPilotUpdateScope(_document, request);", "return new RackPilotUpdateScope(AcApplication.DocumentManager.MdiActiveDocument, request);", StringComparison.Ordinal)));
    }

    [Fact]
    public void TheProhibitionGuardOfTheWriterSeesAFieldInThePortAnObjectOfAutoCadInThePortAndAPurgeInTheScopeOfMutate()
    {
        var real = Writer();

        Assert.NotEmpty(WriterProhibitionViolations(real.Replace("internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort\n    {", "internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort\n    {\n        private Document _document;", StringComparison.Ordinal).Replace("internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort\r\n    {", "internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort\r\n    {\r\n        private Document _document;", StringComparison.Ordinal)));
        Assert.NotEmpty(WriterProhibitionViolations(real.Replace("internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort", "internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort /* ObjectId */", StringComparison.Ordinal)
            .Replace("public IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request)", "public IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request, ObjectId id = default)", StringComparison.Ordinal)));
        Assert.NotEmpty(WriterProhibitionViolations(real.Replace("public void Seal()", "public void Seal() { SystemBlockWriter.PurgeAfterCommit(_database, _staleDefinitions.ToList()); }\n        public void Other()", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("var t = database.TransactionManager.StartTransaction();")]
    [InlineData("transaction.Commit();")]
    [InlineData("SystemBlockWriter.PurgeAfterCommit(database, ids);")]
    public void TheStructureGuardOfTheWriterTripsOnASecondScopeASecondConfirmationAndASecondPurge(string injected)
    {
        var real = Writer();

        Assert.Empty(WriterStructureViolations(real));
        Assert.NotEmpty(WriterStructureViolations(real.Replace("internal sealed class RackPilotMutationScope", injected + "\ninternal sealed class RackPilotMutationScope", StringComparison.Ordinal)));
    }

    [Fact]
    public void TheStructureGuardOfTheWriterSeesTheOrderOfPostAPurgeAfterTheRenameAMovedCheckAndAScopeThatIsNotReleased()
    {
        var real = Writer();
        var purge = "SystemBlockWriter.PurgeAfterCommit(database, _staleDefinitions.ToList());";
        var rename = "RackBlockRenamer.SyncName(document, blockId, pair.Value);";

        Assert.Contains(purge, real, StringComparison.Ordinal);
        Assert.Contains(rename, real, StringComparison.Ordinal);
        // The purge after the rename.
        Assert.NotEmpty(WriterStructureViolations(real.Replace(purge, string.Empty, StringComparison.Ordinal).Replace(rename, rename + "\n            " + purge, StringComparison.Ordinal)));
        // The check of the top scope removed from Post.
        Assert.NotEmpty(WriterStructureViolations(real.Replace("database.TransactionManager.TopTransaction != null)\n                return new PushBackPilotPostValues", "database.TransactionManager.Count > 99)\n                return new PushBackPilotPostValues", StringComparison.Ordinal)
            .Replace("database.TransactionManager.TopTransaction != null)\r\n                return new PushBackPilotPostValues", "database.TransactionManager.Count > 99)\r\n                return new PushBackPilotPostValues", StringComparison.Ordinal)));
        // The scope of MUTATE that does not release its scope of the drawing.
        Assert.NotEmpty(WriterStructureViolations(real.Replace("_transaction.Dispose();", "_ = 0;", StringComparison.Ordinal)));
    }

    [Fact]
    public void TheDebugInjectionHasConditionalMethodsAndOnlyTheReadOfTheVariableIsInsideIfDebug()
    {
        var text = Src.Text(WriterPath).Replace("\r\n", "\n");
        var mutation = MutationScope(Writer());

        Assert.Contains("RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT", text, StringComparison.Ordinal);
        Assert.Contains("RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL", text, StringComparison.Ordinal);
        Assert.Equal(2, Src.Count(text, @"\[Conditional\(""DEBUG""\)\]"));
        Assert.Matches(@"\[Conditional\(""DEBUG""\)\]\s*private\s+static\s+void\s+FailUnitIfRequested\s*\(\s*int\s+oneBasedUnit\s*\)", text);
        Assert.Matches(@"\[Conditional\(""DEBUG""\)\]\s*private\s+static\s+void\s+FailSealIfRequested\s*\(\s*string\s+moment\s*\)", text);
        // The only conditional compilation is around the read of the environment, with its Release branch.
        Assert.Equal(1, Src.Count(text, @"#if DEBUG"));
        Assert.Equal(1, Src.Count(text, @"#else"));
        Assert.Equal(1, Src.Count(text, @"#endif"));
        Assert.Equal(1, Src.Count(text, @"Environment\.GetEnvironmentVariable"));
        var inside = text.Substring(text.IndexOf("#if DEBUG", StringComparison.Ordinal), text.IndexOf("#endif", StringComparison.Ordinal) - text.IndexOf("#if DEBUG", StringComparison.Ordinal));
        Assert.Contains("Environment.GetEnvironmentVariable", inside, StringComparison.Ordinal);
        Assert.Contains("return null;", inside.Substring(inside.IndexOf("#else", StringComparison.Ordinal)), StringComparison.Ordinal);
        // The points: a unit that throws, and the seal that throws before and after the confirmation.
        AssertInOrder(Method(mutation, @"public\s+PushBackPilotUnitResult\s+WriteUnit\s*\("), "FailUnitIfRequested(_units);", "RedrawInTransaction(");
        AssertInOrder(Method(mutation, @"public\s+void\s+Seal\s*\("), "FailSealIfRequested(\"before\");", "_transaction.Commit();", "FailSealIfRequested(\"after\");");
    }

    [Fact]
    public void TheDebugGuardTripsOnAnEnvironmentReadOutsideIfDebugAndOnAConditionalThatIsMissing()
    {
        var text = Src.Text(WriterPath).Replace("\r\n", "\n");

        Assert.Equal(1, Src.Count(text + "\n// var a = 1;", @"Environment\.GetEnvironmentVariable"));
        Assert.Equal(2, Src.Count(text + "\nvar x = Environment.GetEnvironmentVariable(\"A\");", @"Environment\.GetEnvironmentVariable"));
        Assert.Equal(1, Src.Count(text.Replace("[Conditional(\"DEBUG\")]\n        private static void FailSealIfRequested", "private static void FailSealIfRequested", StringComparison.Ordinal), @"\[Conditional\(""DEBUG""\)\]"));
    }

    // ------------------------------------------------------------------ the re-read caller-owned (INV-39)

    private static readonly Regex CallerOwnedForbidden = new(@"StartTransaction|StartOpenCloseTransaction|LockDocument|\.Commit\s*\(|CaptureDrawing");

    [Fact]
    public void TheCallerOwnedReadOfMutateOpensNoScopeTakesNoLockAndConfirmsNothing()
    {
        var reread = Method(MutationScope(Writer()), @"public\s+PushBackPilotScanValues\s+Reread\s*\(");

        Assert.DoesNotMatch(CallerOwnedForbidden, reread);
    }

    [Fact]
    public void TheCallerOwnedReadOfMutateGoesThroughTheNucleusAndComparesItsScopeByNativeIdentity()
    {
        var reread = Method(MutationScope(Writer()), @"public\s+PushBackPilotScanValues\s+Reread\s*\(");

        Assert.Contains("RackSiblingScan.TraverseCore(_transaction, _database, ObjectId.Null, null)", reread, StringComparison.Ordinal);
        // The scope that it reads in is compared by its native identity, never by reference.
        var owned = Method(MutationScope(Writer()), @"private\s+void\s+EnsureOwned\s*\(");
        Assert.Contains("UnmanagedObject", owned, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"ReferenceEquals|\bis\s+null\b|==\s*_transaction\b"), owned);
    }

    [Theory]
    [InlineData("var t = database.TransactionManager.StartTransaction();")]
    [InlineData("using (document.LockDocument()) { }")]
    [InlineData("transaction.Commit();")]
    [InlineData("var s = RackSiblingScan.CaptureDrawing(document);")]
    public void TheGuardOfTheCallerOwnedReadTripsOnEachOperationThatItMustNotHaveInTheRealBody(string injected)
    {
        var real = Method(MutationScope(Writer()), @"public\s+PushBackPilotScanValues\s+Reread\s*\(");

        Assert.DoesNotMatch(CallerOwnedForbidden, real);
        Assert.Matches(CallerOwnedForbidden, real + "\n" + injected);
    }

    // ------------------------------------------------------------------ the movement of Traverse (D-T4B-04 (1) to (4b); INV-F4T4B-17)

    /// <summary>
    /// The normalization of the package, fixed: line endings to LF, the comments ("//" up to the end of the line and "/* */") removed outside
    /// the string and character literals, each run of white space collapsed to one space, and the ends trimmed.
    /// </summary>
    internal static string Normalize(string text)
    {
        text = text.Replace("\r\n", "\n");
        var result = new StringBuilder(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '"')
            {
                var verbatim = i > 0 && text[i - 1] == '@';
                result.Append(c);
                i++;
                while (i < text.Length)
                {
                    if (verbatim && text[i] == '"' && i + 1 < text.Length && text[i + 1] == '"') { result.Append("\"\""); i += 2; continue; }
                    if (!verbatim && text[i] == '\\' && i + 1 < text.Length) { result.Append(text[i]).Append(text[i + 1]); i += 2; continue; }
                    result.Append(text[i]);
                    i++;
                    if (text[i - 1] == '"') break;
                }

                continue;
            }

            if (c == '\'')
            {
                result.Append(c);
                i++;
                while (i < text.Length)
                {
                    if (text[i] == '\\' && i + 1 < text.Length) { result.Append(text[i]).Append(text[i + 1]); i += 2; continue; }
                    result.Append(text[i]);
                    i++;
                    if (text[i - 1] == '\'') break;
                }

                continue;
            }

            if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
            {
                while (i < text.Length && text[i] != '\n') i++;
                continue;
            }

            if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/')) i++;
                i += 2;
                continue;
            }

            result.Append(c);
            i++;
        }

        return Regex.Replace(result.ToString(), @"\s+", " ").Trim();
    }

    // The member of the file that starts with this signature, with its signature, to the closing brace (or to the semicolon of an expression body).
    private static (int Start, int End) MemberSpan(string text, string signaturePattern)
    {
        var match = Regex.Match(text, signaturePattern);
        Assert.True(match.Success, "member not found: " + signaturePattern);
        var open = text.IndexOf('{', match.Index + match.Length - 1);
        var arrow = text.IndexOf("=>", match.Index + match.Length - 1, StringComparison.Ordinal);
        if (arrow >= 0 && (open < 0 || arrow < open)) return (match.Index, text.IndexOf(';', arrow) + 1);

        var depth = 0;
        for (var i = open; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}' && --depth == 0) return (match.Index, i + 1);
        }

        throw new InvalidOperationException("unbalanced braces: " + signaturePattern);
    }

    private const string TraverseSignature = @"private\s+static\s+RackSiblingDrawingScan\s+Traverse\s*\(";
    private const string CoreSignature = @"internal\s+static\s+RackSiblingDrawingScan\s+TraverseCore\s*\(";
    private const string CaptureSignature = @"internal\s+static\s+RackSiblingScanSnapshot\s+Capture\s*\(";
    private const string CaptureDrawingSignature = @"internal\s+static\s+RackSiblingDrawingScan\s+CaptureDrawing\s*\(";

    private static string Slice(string text, (int Start, int End) span) => text.Substring(span.Start, span.End - span.Start);

    // The body of a member WITHOUT the braces of the method.
    private static string BodyWithoutBraces(string member)
    {
        var open = member.IndexOf('{');
        var close = member.LastIndexOf('}');
        return member.Substring(open + 1, close - open - 1);
    }

    private static string Without(string text, params (int Start, int End)[] spans)
    {
        foreach (var span in spans.OrderByDescending(s => s.Start)) text = text.Remove(span.Start, span.End - span.Start);
        return text;
    }

    private static IEnumerable<string> CoreViolations(string scan)
    {
        var core = Slice(scan, MemberSpan(scan, CoreSignature));
        if (Normalize(BodyWithoutBraces(core)) != Normalize(TraverseBaseline.CoreLiteral)) yield return "the body of the nucleus is not the literal of the base";
        if (!Regex.IsMatch(core, @"^internal\s+static\s+RackSiblingDrawingScan\s+TraverseCore\s*\(\s*Transaction\s+transaction\s*,\s*Database\s+database\s*,\s*ObjectId\s+selectedDefinition\s*,\s*Func<RackEmbedDocument,\s*bool>\s+eraseByKind\s*\)\s*\{"))
            yield return "the signature of the nucleus is not the declared one";
    }

    private static IEnumerable<string> WrapperViolations(string scan)
    {
        var traverse = MemberSpan(scan, TraverseSignature);
        var member = Slice(scan, traverse);
        const string expected = "private static RackSiblingDrawingScan Traverse( Document document, ObjectId selectedDefinition, Func<RackEmbedDocument, bool> eraseByKind) { using (document.LockDocument()) using (var transaction = document.Database.TransactionManager.StartTransaction()) { var scan = TraverseCore(transaction, document.Database, selectedDefinition, eraseByKind); transaction.Commit(); return scan; } }";
        if (Normalize(member) != Normalize(expected)) yield return "Traverse is not the exact wrapper";
        if (Regex.IsMatch(Src.CodeOnly(scan), @"\btry\b|\bcatch\b")) yield return "there is a try or a catch in the file";
        // The nucleus is the very next member after Traverse.
        if (!Regex.IsMatch(scan.Substring(traverse.End), @"^\s*internal\s+static\s+RackSiblingDrawingScan\s+TraverseCore\s*\(")) yield return "the nucleus does not come right after Traverse";
    }

    private static IEnumerable<string> FrozenViolations(string scan)
    {
        var spans = new List<(int Start, int End)> { MemberSpan(scan, TraverseSignature) };
        if (Regex.IsMatch(scan, CoreSignature)) spans.Add(MemberSpan(scan, CoreSignature));
        var rest = Without(scan, spans.ToArray());
        if (Normalize(rest) != Normalize(TraverseBaseline.RestLiteral)) yield return "the rest of the file is not the frozen one";
        // Capture and CaptureDrawing, as exact text (they are inside the rest, and this says it on its own).
        if (Normalize(Slice(scan, MemberSpan(scan, CaptureDrawingSignature))) != "internal static RackSiblingDrawingScan CaptureDrawing(Document document) => Traverse(document, ObjectId.Null, null);")
            yield return "CaptureDrawing is not the exact one";
        var capture = Normalize(Slice(scan, MemberSpan(scan, CaptureSignature)));
        if (!capture.Contains("var scan = Traverse(document, selectedDefinition, eraseByKind); var membership = RackSiblingMembership.Classify( scan.Facts, rackId, originalSourceId, originalSourceIdIsAttributable);", StringComparison.Ordinal))
            yield return "Capture is not the exact one";
    }

    [Fact]
    public void TheNucleusOfTheTraversalIsTheLiteralOfTheBaseWithTheOneDeclaredSubstitution()
    {
        var scan = Src.Text(ScanPath);

        Assert.Empty(CoreViolations(scan));
        Assert.Contains("database.BlockTableId", TraverseBaseline.CoreLiteral, StringComparison.Ordinal);
        Assert.DoesNotContain("document.Database.BlockTableId", TraverseBaseline.CoreLiteral, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("directOnly: true", "directOnly: false")]
    [InlineData("if (definition.IsLayout || definition.IsAnonymous) continue;", "if (definition.IsLayout) continue;")]
    [InlineData("envelope != null && eraseByKind != null && eraseByKind(envelope)", "eraseByKind != null && eraseByKind(envelope)")]
    [InlineData("forceValidity: true", "forceValidity: false")]
    [InlineData("rawEnvelopes.Add(key, json);", "rawEnvelopes.Add(key, key);")]
    [InlineData("OpenMode.ForRead", "OpenMode.ForWrite")]
    public void TheGuardOfTheNucleusTripsOnEachChangeOfItsBehavior(string from, string to)
    {
        var scan = Src.Text(ScanPath);
        var core = Slice(scan, MemberSpan(scan, CoreSignature));
        Assert.Contains(from, core, StringComparison.Ordinal);

        var mutated = scan.Replace(core, core.Replace(from, to, StringComparison.Ordinal), StringComparison.Ordinal);

        Assert.NotEqual(scan, mutated);
        Assert.NotEmpty(CoreViolations(mutated));
    }

    [Fact]
    public void TheGuardOfTheNucleusDoesNotSeeCommentsWhiteSpaceOrLineEndingsButSeesTheSubstitutionUndone()
    {
        var scan = Src.Text(ScanPath);
        var core = Slice(scan, MemberSpan(scan, CoreSignature));

        Assert.Empty(CoreViolations(scan.Replace(core, core.Replace("var facts", "// a comment\r\n            /* another */ var   facts", StringComparison.Ordinal), StringComparison.Ordinal)));
        Assert.Empty(CoreViolations(scan.Replace("\r\n", "\n", StringComparison.Ordinal)));
        Assert.Empty(CoreViolations(scan.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\r\n", StringComparison.Ordinal)));
        // The substitution undone is another text: it needs a document and the nucleus has none.
        Assert.NotEmpty(CoreViolations(scan.Replace(core, core.Replace("database.BlockTableId", "document.Database.BlockTableId", StringComparison.Ordinal), StringComparison.Ordinal)));
    }

    [Fact]
    public void TraverseIsExactlyTheLockTheScopeTheCallOfTheNucleusTheConfirmationAndTheReturn()
    {
        var violations = WrapperViolations(Src.Text(ScanPath)).ToList();
        Assert.True(violations.Count == 0, string.Join("; ", violations));
    }

    [Theory]
    [InlineData("transaction.Commit();", "")]
    [InlineData("return scan;", "return null;")]
    [InlineData("using (document.LockDocument())", "")]
    [InlineData("var scan = TraverseCore(transaction, document.Database, selectedDefinition, eraseByKind);", "var scan = TraverseCore(transaction, document.Database, ObjectId.Null, null);")]
    [InlineData("transaction.Commit();", "try { transaction.Commit(); } catch (System.Exception) { }")]
    public void TheGuardOfTheWrapperTripsOnEachChangeOfTheWrapper(string from, string to)
    {
        var scan = Src.Text(ScanPath);
        var traverse = Slice(scan, MemberSpan(scan, TraverseSignature));
        Assert.Contains(from, traverse, StringComparison.Ordinal);

        Assert.NotEmpty(WrapperViolations(scan.Replace(traverse, traverse.Replace(from, to, StringComparison.Ordinal), StringComparison.Ordinal)));
    }

    [Fact]
    public void TheGuardOfTheWrapperSeesTheNucleusMovedAwayFromTraverseAndNewDeclaredMembersBetweenThem()
    {
        var scan = Src.Text(ScanPath);
        var core = Slice(scan, MemberSpan(scan, CoreSignature));
        var traverse = Slice(scan, MemberSpan(scan, TraverseSignature));

        Assert.NotEmpty(WrapperViolations(scan.Replace(traverse, traverse + "\n\n        private static void Between() { }", StringComparison.Ordinal)));
        Assert.NotEmpty(WrapperViolations(scan.Replace(core, string.Empty, StringComparison.Ordinal).Replace("    }\n}", core + "\n    }\n}", StringComparison.Ordinal).Replace("    }\r\n}", core + "\r\n    }\r\n}", StringComparison.Ordinal)));
    }

    [Fact]
    public void CaptureCaptureDrawingAndTheRestOfTheFileAreTheFrozenOnesOfTheBaseAddLayerIncluded()
    {
        var scan = Src.Text(ScanPath);

        Assert.Empty(FrozenViolations(scan));
        Assert.Contains("AddLayer(", scan, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("layer.IsLocked", "false")]
    [InlineData("RackSiblingLayerUse use", "RackSiblingLayerUse use, int extra = 0")]
    [InlineData("var scan = Traverse(document, selectedDefinition, eraseByKind);", "var scan = Traverse(document, ObjectId.Null, eraseByKind);")]
    [InlineData("=> Traverse(document, ObjectId.Null, null);", "=> Traverse(document, ObjectId.Null, x => true);")]
    [InlineData("internal RackSiblingDrawingScan(", "internal RackSiblingDrawingScan(int none, ")]
    [InlineData("public IReadOnlyList<RackSiblingScanFact>", "public IReadOnlyList<RackSiblingScanFact> Extra { get; }\n        public IReadOnlyList<RackSiblingScanFact>")]
    public void TheGuardOfTheFrozenRestTripsOnAChangeOutsideTheTwoMembers(string from, string to)
    {
        var scan = Src.Text(ScanPath);
        if (!scan.Contains(from, StringComparison.Ordinal))
        {
            // The constructor and the properties may be spelled with other layout: the change goes at the class of the scan instead.
            from = "internal static class RackSiblingScan";
            to = "internal static class RackSiblingScan /* x */ : object";
        }

        var mutated = scan.Replace(from, to, StringComparison.Ordinal);

        Assert.NotEqual(scan, mutated);
        Assert.NotEmpty(FrozenViolations(mutated));
    }

    [Fact]
    public void TheNucleusCanMoveAnywhereInTheFileWithoutTheFrozenRestChanging()
    {
        var scan = Src.Text(ScanPath).Replace("\r\n", "\n");
        var core = Slice(scan, MemberSpan(scan, CoreSignature));
        var moved = scan.Replace(core, string.Empty, StringComparison.Ordinal);
        moved = moved.Replace("        internal static RackSiblingScanSnapshot Capture(", core + "\n\n        internal static RackSiblingScanSnapshot Capture(", StringComparison.Ordinal);

        // The rest does not depend on where the two members are (N-6 of the check of the package): only the order check of the wrapper sees it.
        Assert.Empty(FrozenViolations(moved));
        Assert.Empty(CoreViolations(moved));
        Assert.NotEmpty(WrapperViolations(moved));
    }

    // ------------------------------------------------------------------ the census of the nucleus (D-T4B-04 (4))

    // Exactly two calls: the one of Traverse and the one of the writer. The declaration is not a call.
    private static IEnumerable<string> CensusViolations(IReadOnlyList<(string Path, string Code)> files)
    {
        var call = new Regex(@"\bTraverseCore\s*\(");
        var declaration = new Regex(@"static\s+RackSiblingDrawingScan\s+TraverseCore\s*\(");
        var calls = new List<(string Path, int Count)>();
        foreach (var (path, code) in files)
        {
            var count = call.Matches(code).Count - declaration.Matches(code).Count;
            if (count > 0) calls.Add((path, count));
        }

        if (calls.Sum(c => c.Count) != 2) yield return "the nucleus is called " + calls.Sum(c => c.Count) + " times";
        if (!calls.Contains((ScanPath, 1))) yield return "Traverse does not call the nucleus exactly once";
        if (!calls.Contains((WriterPath, 1))) yield return "the writer does not call the nucleus exactly once";
        foreach (var other in calls.Where(c => c.Path != ScanPath && c.Path != WriterPath)) yield return other.Path + " calls the nucleus";
    }

    [Fact]
    public void TheNucleusIsCalledExactlyTwiceByTraverseAndByTheWriterAndByNoOtherFileOfTheProduct()
    {
        Assert.Empty(CensusViolations(Src.AllProduct()));
        Assert.Contains("TraverseCore(transaction, document.Database, selectedDefinition, eraseByKind)", Method(Src.Code(ScanPath), @"private\s+static\s+RackSiblingDrawingScan\s+Traverse\s*\("), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCensusGuardSeesAThirdCallASecondCallInTraverseAndAMissingCall()
    {
        var files = Src.AllProduct().ToList();

        Assert.NotEmpty(CensusViolations(files.Append(("src/RackCad.Plugin/Other.cs", "var s = RackSiblingScan.TraverseCore(t, d, id, null);")).ToList()));
        Assert.NotEmpty(CensusViolations(files.Select(f => f.Path == ScanPath ? (f.Path, f.Code + "\nvar again = TraverseCore(t, d, id, null);") : f).ToList()));
        Assert.NotEmpty(CensusViolations(files.Select(f => f.Path == WriterPath ? (f.Path, f.Code.Replace("TraverseCore(", "Other(", StringComparison.Ordinal)) : f).ToList()));
    }

    // ------------------------------------------------------------------ the gate of its own and the order of the body of the update (D-T4B-07)

    private const string UpdateGateAuthorizeForm =
        @"if\s*\(\s*Authorize\s*\(\s*new\s+WorkspaceRequest\s*\(\s*session\s*\)\s*\)\s*!=\s*RequestDecision\.Accepted\s*\)\s*\{\s*reason\s*=\s*NotThisDocumentReason\s*;\s*return\s+EditGate\.Rejected\s*;\s*\}";

    private static IEnumerable<string> UpdateGateViolations(string core)
    {
        var body = Src.BodyOf(core, @"private\s+EditGate\s+UpdateGate\s*\(");
        if (Src.Count(body, @"\bAuthorize\s*\(") != 1) yield return "UpdateGate does not call Authorize exactly once";
        if (Src.Count(body, UpdateGateAuthorizeForm) != 1) yield return "the authorization of UpdateGate is not of the exact form";
        if (!body.Contains("_probe.ActiveDocumentNativeId", StringComparison.Ordinal)) yield return "UpdateGate does not ask for the active document";
        if (!body.Contains("_modalRackCadActive", StringComparison.Ordinal)) yield return "UpdateGate does not ask for a modal of RackCad";
        if (Regex.IsMatch(body, @"_editIntentReader|ReadIntent|IsActiveDocumentQuiescent|_reader\b|_drafts|_bridge")) yield return "UpdateGate has a rest or a reader";
        var order = new[] { "_probe.ActiveDocumentNativeId", "_modalRackCadActive", "Authorize(" }.Select(n => body.IndexOf(n, StringComparison.Ordinal)).ToArray();
        if (order[0] >= order[1] || order[1] >= order[2]) yield return "UpdateGate is not document, modal, authorization";
    }

    [Fact]
    public void TheGateOfTheUpdateAuthorizesTheSessionByTheExactFormOnlyOnceAndHasNeitherRestNorReader()
    {
        var core = Src.Code(CorePath);

        Assert.Empty(UpdateGateViolations(core));
        Assert.Equal(1, Src.Count(Src.BodyOf(core, @"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\("), @"\bUpdateGate\s*\("));
    }

    [Theory]
    [InlineData("Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted", "Authorize(new WorkspaceRequest(session)) == RequestDecision.Accepted")]
    [InlineData("Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted", "Authorize(new WorkspaceRequest(default)) != RequestDecision.Accepted")]
    [InlineData("Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted", "false")]
    [InlineData("Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted", "Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted || true")]
    [InlineData("return EditGate.Rejected;", "return EditGate.Open;")]
    [InlineData("if (_modalRackCadActive)", "if (!_probe.IsActiveDocumentQuiescent)")]
    [InlineData("var active = _probe.ActiveDocumentNativeId;", "var active = _probe.ActiveDocumentNativeId; var read = _editIntentReader;")]
    public void TheGuardOfTheGateOfTheUpdateSeesTheComparisonTheDecisionTheSessionTheAlwaysTrueTheRestAndTheReader(string from, string to)
    {
        var core = Src.Code(CorePath);
        var body = Src.BodyOf(core, @"private\s+EditGate\s+UpdateGate\s*\(");
        Assert.Contains(from, body, StringComparison.Ordinal);

        Assert.NotEmpty(UpdateGateViolations(core.Replace(body, body.Replace(from, to, StringComparison.Ordinal), StringComparison.Ordinal)));
    }

    [Fact]
    public void TheBodyOfTheUpdateFollowsTheOrderOfThePackageAndEndsTheMarkOfKnownCleanAfterTheClose()
    {
        var body = Src.BodyOf(Src.Code(CorePath), @"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\(");

        AssertInOrder(
            body,
            "UpdateGate(",
            "string.IsNullOrWhiteSpace(request.RackId)",
            "_drafts.Find<PushBackPilotDraftState>(",
            "PushBackPilotUpdateRun.Precheck(",
            ".TryBeginApply(",
            "PushBackPilotUpdateRun.Execute(",
            ".CompleteApply(",
            "_knownClean.Remove(request.Session);");
        Assert.Equal(1, Src.Count(body, @"PushBackPilotUpdateRun\.Execute\s*\("));
        Assert.Equal(1, Src.Count(body, @"PushBackPilotUpdateRun\.Precheck\s*\("));
        Assert.DoesNotMatch(new Regex(@"\.SetCurrentDraftState\s*\(|\.DiscardAndReload\s*\(|\.KeepMineOnCurrentVersion\s*\(|\.ConfirmBase\s*\(|\.CheckNow\s*\(|\.Begin\s*\("), body);
    }

    // ------------------------------------------------------------------ the traps of the census of all of src (m-11), which reach the new files

    [Fact]
    public void NoNewFileOfTheUpdateBuildsOrComposesAnEnvelopeAddsACommandOrNamesTheUnits()
    {
        foreach (var path in PureFiles.Append(WriterPath))
        {
            var masked = PluginSourceCode.Mask(Src.Text(path));

            Assert.Empty(EnvelopeSourceGuards.Constructions(masked));
            Assert.DoesNotMatch(new Regex(@"RackEmbedComposer\s*\.\s*Compose\b"), masked);
            Assert.DoesNotMatch(new Regex(@"\[\s*CommandMethod\s*\("), masked);
            Assert.DoesNotMatch(new Regex(@"Insunits|RackUnitsGuard"), masked);
        }
    }

    [Theory]
    [InlineData("object M() { return new RackEmbedDocument(); }")]
    [InlineData("RackEmbedDocument sobre = new();")]
    [InlineData("var e = RackEmbedComposer.Compose(source, k, id, n, v, s, d);")]
    [InlineData("[CommandMethod(\"RACKX\")] public void X() { }")]
    [InlineData("var u = Insunits.Inches;")]
    public void TheTrapsOfTheCensusSeeEachInjectedConstructionCompositionCommandAndUnitInTheRealTextOfEachNewFile(string injected)
    {
        foreach (var path in PureFiles.Append(WriterPath))
        {
            var real = PluginSourceCode.Mask(Src.Text(path));
            var violation = new Regex(@"RackEmbedComposer\s*\.\s*Compose\b|\[\s*CommandMethod\s*\(|Insunits|RackUnitsGuard");
            var masked = PluginSourceCode.Mask("class X { void M() { " + injected + " } }");

            Assert.DoesNotMatch(violation, real);
            Assert.True(violation.IsMatch(masked) || EnvelopeSourceGuards.Constructions(masked).Count > 0, injected);
        }
    }

    [Fact]
    public void NoNewTestFileOfTheUpdateBuildsOrComposesAnEnvelopeTheyUseTheTextsOfTheirNeighbours()
    {
        // The conservative reading of the criterion of the package (F4-E60, INV-F4T4B-15): no new file, the tests included, builds or composes one.
        foreach (var path in new[]
                 {
                     "tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs",
                     "tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs",
                     "tests/RackCad.Tests/Workspace/WorkspacePilotUpdateScopeDouble.cs",
                 })
        {
            var text = Src.CodeOnly(Src.Text(path));

            Assert.DoesNotMatch(new Regex(@"\bnew\s+(?:[\w\.]+\.)?RackEmbedDocument\b"), text);
            Assert.DoesNotMatch(new Regex(@"RackEmbedComposer"), text);
            Assert.DoesNotMatch(new Regex(@"\[\s*CommandMethod\s*\(|Insunits"), text);
        }
    }

    // ------------------------------------------------------------------ the deferred block of DraftSession, byte for byte (D-T4B-06)

    // The block of the error that arrived while an Update was in flight, with its indentation: a guard of the older tasks looks for it literally.
    private const string DeferredBlock =
        "_deferredErrorReason = null;\n            _explicitStatus = DraftStatus.Error;\n            ErrorReason = deferred;\n            UnknownOrigin = null;\n            return DraftApplyResult.Closed(result.Kind, Status, result.Rebased);";

    private const string DraftSessionPath = "src/RackCad.Application/Workspace/Drafts/DraftSession.cs";

    [Fact]
    public void TheDeferredErrorBlockOfTheDraftSessionIsIdenticalByteForByteAndTheFallbackToErrorUsesTheSameForm()
    {
        var text = Src.Text(DraftSessionPath).Replace("\r\n", "\n");

        Assert.Contains(DeferredBlock, text, StringComparison.Ordinal);
        Assert.Equal(1, Src.Count(text, Regex.Escape(DeferredBlock)));
        // The fallback of the branches that did not apply sets the status Error and its reason in the same block, as the guard of R-3 asks.
        var recover = Src.BodyOf(Src.CodeOnly(text), @"private\s+DraftApplyResult\s+Recover\s*\(");
        Assert.Matches(@"_explicitStatus\s*=\s*DraftStatus\.Error\s*;\s*ErrorReason\s*=\s*cause\s*;", recover);
        // The close never leaves the update half done: no state outside the draft and nothing of the drawing.
        Assert.DoesNotMatch(new Regex(@"Transaction|\.\s*Commit\s*\(|\bDebug\s*\."), Src.CodeOnly(text));
        Assert.Equal(1, Src.Count(Src.BodyOf(Src.CodeOnly(text), @"public\s+DraftApplyResult\s+CompleteApply\s*\("), @"_registry\.ReleaseApplySlot\s*\("));
        // The deferred block guard sees the block with ONE change of indentation.
        Assert.DoesNotContain(DeferredBlock, text.Replace("            ErrorReason = deferred;", "          ErrorReason = deferred;", StringComparison.Ordinal), StringComparison.Ordinal);
    }

    [Fact]
    public void TheXmlOfTheAtomicKeepDocumentsThatKeepOnMustNotMutateItsSecondArgument()
    {
        // M-10 (I-64 F4-T4A1, carried to F4-T4B): the atomicity of the overload rests on a function that does not edit the state in place.
        var text = Src.Text(DraftSessionPath).Replace("\r\n", "\n");
        var at = text.IndexOf("public DraftReloadResult KeepMineOnCurrentVersion(DraftIntentRead<TState> read, Func<TState, TState, TState> keepOn, bool confirmed)", StringComparison.Ordinal);

        Assert.True(at > 0);
        var documentation = text.Substring(Math.Max(0, at - 1700), Math.Min(1700, at));
        Assert.Contains("does not mutate its second argument", documentation.Replace("\n        /// ", " "), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ M-1: the composition for writing never copies the current state

    private static IEnumerable<string> ComposeForWriteViolations(string materializer)
    {
        var body = Src.BodyOf(materializer, @"public\s+static\s+PushBackPilotComposition\s+ComposeForWrite\s*\(");
        if (Regex.IsMatch(body, @"Copy\s*\(\s*current\s*\)")) yield return "ComposeForWrite copies the current state: it hides what the edit changed";
        if (Src.Count(body, @"PushBackPilotStateComparer\.Copy\s*\(\s*initial\s*\)") != 1) yield return "ComposeForWrite does not copy the initial state exactly once";
        if (!Regex.IsMatch(body, @"PushBackPilotStateComparer\.Instance\.Equals\s*\(\s*neutralInitial\s*,\s*current\s*\)")) yield return "ComposeForWrite does not compare the current state as it is";
    }

    [Fact]
    public void TheCompositionForWritingComparesTheCurrentStateAsItIsAndNeverCopiesIt()
    {
        Assert.Empty(ComposeForWriteViolations(Src.Code(MaterializerPath)));
    }

    [Fact]
    public void TheGuardOfTheCompositionTripsWhenTheCurrentStateIsCopiedBeforeTheComparison()
    {
        var code = Src.Code(MaterializerPath);

        Assert.NotEmpty(ComposeForWriteViolations(code.Replace(
            "PushBackPilotStateComparer.Instance.Equals(neutralInitial, current)",
            "PushBackPilotStateComparer.Instance.Equals(neutralInitial, PushBackPilotStateComparer.Copy(current))",
            StringComparison.Ordinal)));
        Assert.NotEmpty(ComposeForWriteViolations(code.Replace("var neutralInitial = PushBackPilotStateComparer.Copy(initial);", "var neutralInitial = initial;", StringComparison.Ordinal)));
        Assert.NotEmpty(ComposeForWriteViolations(code.Replace("PushBackPilotStateComparer.Instance.Equals(neutralInitial, current)", "PushBackPilotStateComparer.Instance.Equals(current, neutralInitial)", StringComparison.Ordinal)));
    }
}

/// <summary>
/// The text of the traversal of the base (c3ae1c97, RackSiblingScan.cs) that D-T4B-04 freezes, raw: the nucleus (lines 103 to 110, 115 to 161 and 166 to
/// 169, with the ONE substitution of document.Database.BlockTableId by database.BlockTableId) and the rest of the file (every line but 98 to 170).
/// The guards compare it normalized, so layout and comments do not count.
/// </summary>
internal static class TraverseBaseline
{
    public const string CoreLiteral = """
            var facts = new List<RackSiblingScanFact>();
            var definitions = new Dictionary<string, ObjectId>(StringComparer.Ordinal);
            var envelopes = new Dictionary<string, RackEmbedDocument>(StringComparer.Ordinal);
            var properties = new Dictionary<string, CustomPropertiesReadResult>(StringComparer.Ordinal);
            var layers = new Dictionary<string, IReadOnlyList<RackSiblingLayerRequirement>>(StringComparer.Ordinal);
            var rawEnvelopes = new Dictionary<string, string>(StringComparer.Ordinal);
            var embedStore = new RackEmbedStore();
            var propertyStore = new CustomPropertiesStore();

                var table = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId definitionId in table)
                {
                    var definition = (BlockTableRecord)transaction.GetObject(definitionId, OpenMode.ForRead);
                    if (definition.IsLayout || definition.IsAnonymous) continue;

                    var json = RackBlockData.Read(transaction, definitionId);
                    if (string.IsNullOrEmpty(json)) continue;

                    var envelope = embedStore.Deserialize(json);
                    var key = definitionId.Handle.ToString();
                    var layoutReferences = 0;
                    var nestedReferences = 0;
                    var layerFacts = new List<RackSiblingLayerRequirement>();
                    foreach (ObjectId referenceId in definition.GetBlockReferenceIds(directOnly: true, forceValidity: true))
                    {
                        var reference = transaction.GetObject(referenceId, OpenMode.ForRead) as BlockReference;
                        var owner = reference == null ? null : transaction.GetObject(reference.OwnerId, OpenMode.ForRead) as BlockTableRecord;
                        if (owner?.IsLayout == true) layoutReferences++; else nestedReferences++;
                        AddLayer(transaction, reference?.LayerId ?? ObjectId.Null, RackSiblingLayerUse.DirectReference, layerFacts);
                    }

                    foreach (ObjectId entityId in definition)
                        AddLayer(transaction, (transaction.GetObject(entityId, OpenMode.ForRead) as Entity)?.LayerId ?? ObjectId.Null,
                            RackSiblingLayerUse.ExistingDefinitionEntity, layerFacts);

                    var probe = envelope?.Id ?? RackEnvelopeIdProbe.Probe(json);
                    facts.Add(new RackSiblingScanFact(
                        key,
                        envelope?.View,
                        definitionId == selectedDefinition,
                        definition.IsFromExternalReference,
                        envelope != null,
                        envelope?.Id,
                        probe,
                        layoutReferences,
                        nestedReferences,
                        envelope != null && eraseByKind != null && eraseByKind(envelope)));
                    definitions.Add(key, definitionId);
                    rawEnvelopes.Add(key, json);
                    layers.Add(key, layerFacts);
                    if (envelope != null)
                    {
                        envelopes.Add(key, envelope);
                        properties.Add(key, propertyStore.ReadElement(envelope.CustomProperties));
                    }
                }

            return new RackSiblingDrawingScan(facts, definitions, envelopes, properties, layers)
            {
                RawEnvelopes = new ReadOnlyDictionary<string, string>(rawEnvelopes)
            };
""";

    // The raw text has the keyword that opens a namespace written as a placeholder, so no line of this file starts with it (a guard of the
    // namespaces of the test projects counts those lines).
    private const string RestRaw = """
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.Views.Insertion;
using RackCad.Application.Views.Redraw;
using RackCad.Plugin.Systems.Shared;

@NS@ RackCad.Plugin.Views
{
    internal sealed class RackSiblingScanSnapshot
    {
        internal RackSiblingScanSnapshot(
            RackSiblingMembershipSnapshot membership,
            IReadOnlyDictionary<string, ObjectId> definitions,
            IReadOnlyDictionary<string, RackEmbedDocument> envelopes,
            IReadOnlyDictionary<string, CustomPropertiesReadResult> properties,
            IReadOnlyDictionary<string, IReadOnlyList<RackSiblingLayerRequirement>> layers)
        {
            Membership = membership;
            Definitions = definitions;
            Envelopes = envelopes;
            Properties = properties;
            Layers = layers;
        }

        internal RackSiblingMembershipSnapshot Membership { get; }
        internal IReadOnlyDictionary<string, ObjectId> Definitions { get; }
        internal IReadOnlyDictionary<string, RackEmbedDocument> Envelopes { get; }
        internal IReadOnlyDictionary<string, CustomPropertiesReadResult> Properties { get; }
        internal IReadOnlyDictionary<string, IReadOnlyList<RackSiblingLayerRequirement>> Layers { get; }
    }

    /// <summary>
    /// One whole-drawing scan, unclassified: every definition of the BlockTable read once. ID19 (RACKPROYECTAR) classifies it
    /// per RackId without touching the drawing again.
    /// </summary>
    internal sealed class RackSiblingDrawingScan
    {
        internal RackSiblingDrawingScan(
            IReadOnlyList<RackSiblingScanFact> facts,
            IReadOnlyDictionary<string, ObjectId> definitions,
            IReadOnlyDictionary<string, RackEmbedDocument> envelopes,
            IReadOnlyDictionary<string, CustomPropertiesReadResult> properties,
            IReadOnlyDictionary<string, IReadOnlyList<RackSiblingLayerRequirement>> layers)
        {
            Facts = facts;
            Definitions = definitions;
            Envelopes = envelopes;
            Properties = properties;
            Layers = layers;
        }

        internal IReadOnlyList<RackSiblingScanFact> Facts { get; }
        internal IReadOnlyDictionary<string, ObjectId> Definitions { get; }
        internal IReadOnlyDictionary<string, RackEmbedDocument> Envelopes { get; }
        internal IReadOnlyDictionary<string, CustomPropertiesReadResult> Properties { get; }
        internal IReadOnlyDictionary<string, IReadOnlyList<RackSiblingLayerRequirement>> Layers { get; }

        /// <summary>
        /// The raw text of the envelope of each scanned definition that has text, by key (ordinal): the same text that
        /// <c>Traverse</c> deserializes. It is a read-only view; the scan still writes nothing. It is assigned at the one point
        /// where a scan is built (<c>Traverse</c>); a scan without it carries null, and the read of the intention then fails
        /// with a typed failure instead of receiving an empty dictionary in silence.
        /// </summary>
        internal IReadOnlyDictionary<string, string> RawEnvelopes { get; init; }

        /// <summary>The members of one rack among the scanned definitions; no source is selected, so none is erased by kind.</summary>
        internal RackSiblingMembershipSnapshot MembershipFor(string rackId)
            => RackSiblingMembership.Classify(Facts, rackId, null, false);
    }

    /// <summary>The single physical BlockTable traversal used by one Insert gesture.</summary>
    internal static class RackSiblingScan
    {
        internal static RackSiblingScanSnapshot Capture(
            Document document,
            ObjectId selectedDefinition,
            string rackId,
            string originalSourceId,
            bool originalSourceIdIsAttributable,
            Func<RackEmbedDocument, bool> eraseByKind)
        {
            var scan = Traverse(document, selectedDefinition, eraseByKind);
            var membership = RackSiblingMembership.Classify(
                scan.Facts, rackId, originalSourceId, originalSourceIdIsAttributable);
            return new RackSiblingScanSnapshot(
                membership, scan.Definitions, scan.Envelopes, scan.Properties, scan.Layers);
        }

        /// <summary>The same traversal with nothing selected, for a caller that classifies several racks from ONE pass.</summary>
        internal static RackSiblingDrawingScan CaptureDrawing(Document document)
            => Traverse(document, ObjectId.Null, null);


        private static void AddLayer(Transaction transaction, ObjectId layerId, RackSiblingLayerUse use,
            List<RackSiblingLayerRequirement> facts)
        {
            if (layerId.IsNull) return;
            var layer = transaction.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
            if (layer == null || facts.Exists(item => item.Use == use && string.Equals(item.Layer, layer.Name, StringComparison.OrdinalIgnoreCase))) return;
            facts.Add(new RackSiblingLayerRequirement(layer.Name, layer.IsLocked, use));
        }
    }
}

""";

    public static readonly string RestLiteral = RestRaw.Replace("@NS@ ", "namespace ", StringComparison.Ordinal);
}
