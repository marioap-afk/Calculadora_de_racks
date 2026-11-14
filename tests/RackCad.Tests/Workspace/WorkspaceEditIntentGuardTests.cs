#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T2B2A-INTENT-WIRING (D-T2B2-01, 02, 03, 05, 06, 08): source guards of the wiring of the edit intention. The Core test project
// cannot reference the Plugin (it needs AutoCAD), so the Plugin files are read as text, like the rest of the guards of the workspace.
// These guards are ADDITIVE and live in a new file: no existing guard was changed or weakened for this task. Every guard fails by
// ASSERTION when the text it expects is missing (a missing file or member is an assertion too). Only the guards that are followed by a
// test named "...Detects..." (or by a theory that injects a line into the real text) have such a detection test; the others read the
// real text and nothing more. They are limited to the names of this task and to the bodies of its two operations: what the package
// assigns to F4-T2B2B (the lifecycle) is not prohibited here, and the exposure to the UI exists since F4-T3A and F4-T3B1 (the edit port
// of the pilot and the panel that calls it).
//
// Evidence named by the package: R6 (no write to the drawing: the adapter, the scan, the host and the core), obligation 3 (the
// preview never reaches the intention), obligation 10 (the base of a read only reaches Begin, CheckNow or DiscardAndReload) and
// obligation 13 (the session of A never operates on B: the order of the nested reader, with the authorization before the revalidation).
public class WorkspaceEditIntentGuardTests
{
    private const string AdapterPath = "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs";
    private const string ScanPath = "src/RackCad.Plugin/Views/RackSiblingScan.cs";
    private const string HostPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string CorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string InterfacePath = "src/RackCad.Application/Workspace/IWorkspaceEditIntentReader.cs";
    private const string IntentPath = "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs";
    private const string BridgeCorePath = "src/RackCad.Application/Workspace/EventBridgeCore.cs";

    private static string Adapter() => Src.Code(AdapterPath);

    private static string Host() => Src.Code(HostPath);

    private static string Core() => Src.Code(CorePath);

    private static string ReaderBody() => Src.BodyOf(Host(), @"private\s+EditIntentReadResult\s+ReadOnActiveDocument\s*\(");

    private static string AdapterRead() => Src.BodyOf(Adapter(), @"internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(");

    private static string AdapterPreview() => Src.BodyOf(Adapter(), @"internal\s+static\s+RackPilotWitnessResult\s+ReadPreviewWitness\s*\(");

    private static string IntentBody() => Src.BodyOf(Core(), @"public\s+EditIntentOutcome\s+TryBeginPilotEdit\s*\(");

    private static string CheckBody() => Src.BodyOf(Core(), @"public\s+EditCheckOutcome\s+TryCheckPilotEdit\s*\(");

    // I-64 F4-T4A1 (D-T4A1-03): the three bodies of the explicit reconciliation of the host.
    private static string ConfirmBody() => Src.BodyOf(Core(), @"public\s+EditConfirmOutcome\s+TryConfirmPilotBase\s*\(");

    private static string KeepBody() => Src.BodyOf(Core(), @"public\s+EditIntentOutcome\s+TryKeepPilotEdit\s*\(");

    private static string DiscardAndReloadBody() => Src.BodyOf(Core(), @"public\s+EditIntentOutcome\s+TryDiscardAndReloadPilotEdit\s*\(");

    private static int IndexOf(string text, string needle) => text.IndexOf(needle, StringComparison.Ordinal);

    private static void AssertInOrder(string text, params string[] needles)
    {
        var previous = -1;
        foreach (var needle in needles)
        {
            var at = IndexOf(text, needle);
            Assert.True(at >= 0, "missing: " + needle);
            Assert.True(at > previous, "out of order: " + needle);
            previous = at;
        }
    }

    // ------------------------------------------------------------------ INV-F4T2B2A-01: the additive projection of the scan

    [Fact]
    public void TheScanGainsTheRawEnvelopesAsAnInternalReadOnlyOrdinalViewFilledFromTheSameJsonThatTraverseReads()
    {
        var scan = Src.Code(ScanPath);

        Assert.Matches(@"internal\s+IReadOnlyDictionary<string,\s*string>\s+RawEnvelopes\s*\{\s*get;\s*init;\s*\}", scan);
        Assert.Matches(@"var\s+rawEnvelopes\s*=\s*new\s+Dictionary<string,\s*string>\s*\(\s*StringComparer\.Ordinal\s*\)\s*;", scan);
        Assert.Equal(1, Src.Count(scan, @"\brawEnvelopes\.Add\s*\(\s*key\s*,\s*json\s*\)"));
        Assert.Equal(1, Src.Count(scan, @"new\s+ReadOnlyDictionary<string,\s*string>\s*\(\s*rawEnvelopes\s*\)"));
        Assert.Equal(1, Src.Count(scan, @"RackBlockData\.Read\s*\("));
        // The text is added right after it was read and only for a definition that has text.
        AssertInOrder(scan, "RackBlockData.Read(", "string.IsNullOrEmpty(json)", "rawEnvelopes.Add(key, json)");
    }

    [Fact]
    public void TheScanKeepsTheSignaturesOfItsMethodsAndTheFiveParametersOfTheConstructorAndSetsRawEnvelopesAtItsOnePointOfConstruction()
    {
        var scan = Src.Code(ScanPath);

        Assert.Matches(
            @"internal\s+static\s+RackSiblingScanSnapshot\s+Capture\s*\(\s*Document\s+document\s*,\s*ObjectId\s+selectedDefinition\s*,\s*string\s+rackId\s*,\s*string\s+originalSourceId\s*,\s*bool\s+originalSourceIdIsAttributable\s*,\s*Func<RackEmbedDocument,\s*bool>\s+eraseByKind\s*\)",
            scan);
        Assert.Matches(@"internal\s+static\s+RackSiblingDrawingScan\s+CaptureDrawing\s*\(\s*Document\s+document\s*\)", scan);
        Assert.Matches(
            @"private\s+static\s+RackSiblingDrawingScan\s+Traverse\s*\(\s*Document\s+document\s*,\s*ObjectId\s+selectedDefinition\s*,\s*Func<RackEmbedDocument,\s*bool>\s+eraseByKind\s*\)",
            scan);
        // M-5: the constructor keeps its ORIGINAL signature of five parameters (no sixth one, optional or not)...
        Assert.Matches(
            @"internal\s+RackSiblingDrawingScan\s*\(\s*IReadOnlyList<RackSiblingScanFact>\s+facts\s*,\s*IReadOnlyDictionary<string,\s*ObjectId>\s+definitions\s*,\s*IReadOnlyDictionary<string,\s*RackEmbedDocument>\s+envelopes\s*,\s*IReadOnlyDictionary<string,\s*CustomPropertiesReadResult>\s+properties\s*,\s*IReadOnlyDictionary<string,\s*IReadOnlyList<RackSiblingLayerRequirement>>\s+layers\s*\)",
            scan);
        var constructor = Src.BodyOf(scan, @"internal\s+RackSiblingDrawingScan\s*\(");
        // ...and it never invents a value for the raw text: no assignment of it and no default (an empty dictionary) in the constructor.
        Assert.DoesNotMatch(new Regex(@"RawEnvelopes|rawEnvelopes|\?\?|new\s+ReadOnlyDictionary|new\s+Dictionary"), constructor);
        Assert.DoesNotMatch(new Regex(@"rawEnvelopes\s*=\s*null"), scan);
        // The property is assigned at the one point of construction of the scan, with the dictionary that Traverse filled.
        Assert.Equal(1, Src.Count(scan, @"\bnew\s+RackSiblingDrawingScan\s*\("));
        Assert.Matches(
            @"new\s+RackSiblingDrawingScan\s*\([^;{]*\)\s*\{\s*RawEnvelopes\s*=\s*new\s+ReadOnlyDictionary<string,\s*string>\s*\(\s*rawEnvelopes\s*\)\s*\}\s*;",
            scan);
        Assert.Equal(1, Src.Count(scan, @"\bRawEnvelopes\s*="));
        // A scan that arrives without the raw text is a typed failure of the read, never an empty dictionary received in silence.
        var read = AdapterRead();
        Assert.Matches(@"if\s*\(\s*scan\.RawEnvelopes\s*==\s*null\s*\)\s*return\s+new\s+RackPilotIntentReadResult\s*\(\s*null\s*,", read);
        AssertInOrder(read, "CaptureDrawing(", "scan.RawEnvelopes == null", "new PushBackPilotIntentScan(");
    }

    [Fact]
    public void TheScanStillWritesNothingAndKeepsItsOneCommitOfTheReadOnlyTransaction()
    {
        var scan = Src.Code(ScanPath);

        Assert.DoesNotMatch(new Regex(@"OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|AppendEntity|\.Erase\s*\(|RackBlockData\.Write|SetImpliedSelection|SetCurrentView|ProjectVariables"), scan);
        // The one commit of the read-only scan was already there; the projection adds none.
        Assert.Equal(1, Src.Count(scan, @"\.Commit\s*\("));
    }

    // ------------------------------------------------------------------ INV-F4T2B2A-02 and 11: the adapter

    [Fact]
    public void TheAdapterIsAnInternalStaticClassOfTheViewsFolderWithTheThreeOperationsOfThePackage()
    {
        var code = Adapter();

        Assert.Matches(@"\bnamespace\s+RackCad\.Plugin\.Views\b", code);
        Assert.Matches(@"internal\s+static\s+class\s+RackPilotIntentAdapter\b", code);
        Assert.Matches(@"internal\s+static\s+RackPilotCatalogResult\s+LoadCatalog\s*\(\s*\)", code);
        Assert.Matches(
            @"internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(\s*Document\s+document\s*,\s*string\s+rackId\s*,\s*long\s+originDefinitionHandle\s*,\s*string\??\s+witnessKey\s*,\s*string\??\s+witnessText\s*,\s*RackCatalog\s+catalog\s*\)",
            code);
        Assert.Matches(@"internal\s+static\s+RackPilotWitnessResult\s+ReadPreviewWitness\s*\(\s*Document\s+document\s*,\s*long\s+definitionHandle\s*\)", code);
        // The result types are internal and of this file; nothing of the adapter is public.
        Assert.DoesNotMatch(new Regex(@"\bpublic\s+(?:sealed\s+|static\s+)*(?:class|struct|record|enum|interface)\b"), code);
        Assert.Equal(3, Src.Count(code, @"internal\s+sealed\s+class\s+RackPilot\w+Result\b"));
        Assert.StartsWith("#nullable enable", Src.Text(AdapterPath));
    }

    private static readonly Regex HostObjectField = new(
        @"^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:[\w.]+\.)?(?:Transaction|DBObject|Document|Database|Entity|BlockReference|ObjectId|Editor)\b[^(]*[;=]",
        RegexOptions.Multiline);

    [Fact]
    public void TheAdapterKeepsNoDocumentNoTransactionAndNoDatabaseObjectInAField()
    {
        Assert.DoesNotMatch(HostObjectField, Adapter());
        // No static field either (a static auto-property would also keep an object alive); the adapter has constants and methods only.
        Assert.DoesNotMatch(new Regex(@"\bstatic\s+(?:readonly\s+)?[\w<>,.?]+\s+\w+\s*(?:;|=(?!>)|\{\s*get)"), Adapter());
    }

    [Theory]
    [InlineData("private Document _document;")]
    [InlineData("internal static Transaction _transaction = null;")]
    [InlineData("private readonly Database _database;")]
    [InlineData("public DBObject Current { get; set; }")]
    [InlineData("private ObjectId _definition;")]
    public void TheFieldGuardOfTheAdapterDetectsEachObjectOfTheHostInItsRealText(string injected)
    {
        var real = Adapter();

        Assert.DoesNotMatch(HostObjectField, real);
        Assert.Matches(HostObjectField, real + "\n" + injected);
    }

    [Theory]
    [InlineData("var l = document.LockDocument();")]
    [InlineData("var t = document.Database.TransactionManager.StartTransaction();")]
    public void TheUsingGuardOfTheAdapterDetectsALockOrATransactionOutsideUsingInItsRealText(string injected)
    {
        var offending = new Regex(@"\b(?:StartTransaction|StartOpenCloseTransaction|LockDocument)\s*\(");

        Assert.All(Adapter().Split('\n').Where(l => offending.IsMatch(l)), l => Assert.Matches(@"\busing\b", l));
        Assert.Contains(
            (Adapter() + "\n" + injected).Split('\n').Where(l => offending.IsMatch(l)),
            l => !Regex.IsMatch(l, @"\busing\b"));
    }

    [Fact]
    public void TheLockAndTheTransactionsAreOnlyInsideUsingAndTheReadTakesNoLockOfItsOwn()
    {
        var code = Adapter();

        foreach (var line in code.Split('\n').Where(l => Regex.IsMatch(l, @"\b(?:StartTransaction|StartOpenCloseTransaction|LockDocument)\s*\(")))
            Assert.True(Regex.IsMatch(line, @"\busing\b"), "outside a using: " + line.Trim());
        Assert.DoesNotMatch(new Regex(@"StartOpenCloseTransaction|OpenCloseTransaction"), code);

        // Read runs inside the lock that the operation of the host already holds; the light read of the witness has its own.
        Assert.DoesNotMatch(new Regex(@"LockDocument|StartTransaction"), AdapterRead());
        var preview = AdapterPreview();
        Assert.Equal(1, Src.Count(preview, @"using\s*\(\s*document\.LockDocument\s*\(\s*\)\s*\)"));
        Assert.Equal(1, Src.Count(preview, @"using\s*\(\s*var\s+\w+\s*=\s*document\.Database\.TransactionManager\.StartTransaction\s*\(\s*\)\s*\)"));
        AssertInOrder(preview, "LockDocument()", "StartTransaction()", "RackBlockData.Read(");
    }

    [Fact]
    public void TheAdapterChecksInDebugThatNoTransactionIsOpenBeforeAndAfterTheOneRead()
    {
        var code = Adapter();
        var read = AdapterRead();

        Assert.Matches(@"\[Conditional\(""DEBUG""\)\]\s*private\s+static\s+void\s+AssertNoOpenTransaction\s*\(", code);
        var helper = Src.BodyOf(code, @"private\s+static\s+void\s+AssertNoOpenTransaction\s*\(");
        Assert.Contains("Debug.Assert(", helper);
        Assert.Contains("TransactionManager.TopTransaction == null", helper);
        AssertInOrder(read, "AssertNoOpenTransaction(document, \"before\")", "CaptureDrawing(", "AssertNoOpenTransaction(document, \"after\")");
    }

    private static void AssertInOrderOfPatterns(string text, params string[] patterns)
    {
        var previous = -1;
        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern);
            Assert.True(match.Success, "missing: " + pattern);
            Assert.True(match.Index > previous, "out of order: " + pattern);
            previous = match.Index;
        }
    }

    private const string CheckBeforePattern = @"AssertNoOpenTransaction\s*\(\s*document\s*,\s*""before""\s*\)";
    private const string CheckAfterPattern = @"AssertNoOpenTransaction\s*\(\s*document\s*,\s*""after""\s*\)";

    [Fact]
    public void TheDebugCheckOfTheOpenTransactionRunsInEveryPathOfReadAndAroundTheLockAndTheTransactionOfThePreviewRead()
    {
        var read = AdapterRead();
        var preview = AdapterPreview();

        // M-4, Read: "before" is the first thing inside the try and "after" is in the finally that follows every catch, so it also
        // runs for the early return of the witness and for each of the three catches (INV-14: at the end of every operation).
        Assert.Equal(1, Src.Count(read, CheckBeforePattern));
        Assert.Equal(1, Src.Count(read, CheckAfterPattern));
        AssertInOrderOfPatterns(
            read, @"\btry\s*\{", CheckBeforePattern, @"IsNullOrWhiteSpace\s*\(\s*witnessKey\s*\)", @"\bCaptureDrawing\s*\(", @"\bcatch\b", @"\bfinally\s*\{", CheckAfterPattern);
        Assert.True(
            Regex.Match(read, @"\bfinally\s*\{").Index > Regex.Matches(read, @"\bcatch\b").Cast<Match>().Last().Index,
            "the finally must come after the last catch of Read");

        // M-4, ReadPreviewWitness: "before" is made before its lock and its transaction are opened, and "after" after both closed
        // (the finally is outside the two using), in every path.
        Assert.Equal(1, Src.Count(preview, CheckBeforePattern));
        Assert.Equal(1, Src.Count(preview, CheckAfterPattern));
        AssertInOrderOfPatterns(
            preview,
            @"\btry\s*\{",
            CheckBeforePattern,
            @"\bLockDocument\s*\(",
            @"\bStartTransaction\s*\(",
            @"RackBlockData\.Read\s*\(",
            @"\bcatch\b",
            @"\bfinally\s*\{",
            CheckAfterPattern);
        Assert.True(
            Regex.Match(preview, @"\bfinally\s*\{").Index > Regex.Matches(preview, @"\bcatch\b").Cast<Match>().Last().Index,
            "the finally must come after the last catch of ReadPreviewWitness");
    }

    [Fact]
    public void TheAdapterCapturesTheLockViolationAndEveryOtherExceptionAndAnswersATypedFailure()
    {
        var read = AdapterRead();
        var preview = AdapterPreview();

        foreach (var body in new[] { read, preview })
        {
            Assert.Matches(@"catch\s*\(\s*Autodesk\.AutoCAD\.Runtime\.Exception\s+\w+\s*\)\s*when\s*\(\s*\w+\.ErrorStatus\s*==\s*ErrorStatus\.LockViolation\s*\)", body);
            Assert.Matches(@"catch\s*\(\s*Autodesk\.AutoCAD\.Runtime\.Exception\s+\w+\s*\)\s*\{", body);
            Assert.Matches(@"catch\s*\(\s*System\.Exception\s+\w+\s*\)", body);
            var filtered = Regex.Match(body, @"catch\s*\(\s*Autodesk\.AutoCAD\.Runtime\.Exception\s+\w+\s*\)\s*when").Index;
            var general = Regex.Match(body, @"catch\s*\(\s*Autodesk\.AutoCAD\.Runtime\.Exception\s+\w+\s*\)\s*\{").Index;
            var other = Regex.Match(body, @"catch\s*\(\s*System\.Exception").Index;
            Assert.True(filtered < general && general < other, "the catches must go from the most specific to the most general");
        }

        Assert.DoesNotMatch(new Regex(@"\bthrow\b"), Adapter());
    }

    private static readonly Regex AdapterWrites = new(
        @"OpenMode\.(?:ForWrite|ForNotify)|\bForWrite\b|UpgradeOpen|DowngradeOpen|AppendEntity|\.Erase\s*\(|\.Commit\s*\(|RackBlockData\.Write|SetImpliedSelection|SetCurrentView|GetCurrentView|ViewTableRecord|\bViewTable\b|SetSystemVariable|\bZoom\w*|\bRegen\b|UpdateScreen|CreateExtensionDictionary|\.Save\s*\(|SaveAs");

    [Fact]
    public void TheAdapterWritesNothingToTheDrawingNorTheSelectionNorTheViewNorTheVariableRegistry()
    {
        var code = Adapter();

        Assert.True(!AdapterWrites.IsMatch(code), "the adapter uses " + AdapterWrites.Match(code).Value);
        Assert.DoesNotMatch(new Regex(@"ProjectVariables"), code);
        Assert.Equal(1, Src.Count(code, @"RackBlockData\.Read\s*\("));
        Assert.Equal(0, Src.Count(code, @"GetObject\s*\("));
    }

    [Theory]
    [InlineData("doc.Editor.SetImpliedSelection(ids);")]
    [InlineData("var e = transaction.GetObject(id, OpenMode.ForWrite);")]
    [InlineData("entity.UpgradeOpen();")]
    [InlineData("space.AppendEntity(line);")]
    [InlineData("entity.Erase();")]
    [InlineData("transaction.Commit();")]
    [InlineData("RackBlockData.Write(transaction, id, json);")]
    [InlineData("doc.Editor.SetCurrentView(view);")]
    [InlineData("var variables = ProjectVariablesData.Read(db);")]
    public void TheWriteGuardOfTheAdapterDetectsEachFamilyInItsRealText(string injected)
    {
        var real = Adapter();
        var pattern = new Regex(AdapterWrites.ToString() + "|ProjectVariables");

        Assert.DoesNotMatch(pattern, real);
        Assert.Matches(pattern, real + "\n" + injected);
    }

    [Fact]
    public void TheAdapterHasOneCaptureOfTheDrawingAndOneNewHandleInsideTheOneHelper()
    {
        var code = Adapter();

        Assert.Equal(1, Src.Count(code, @"\bCaptureDrawing\s*\("));
        Assert.Equal(1, Src.Count(AdapterRead(), @"\bCaptureDrawing\s*\("));
        Assert.Equal(1, Src.Count(code, @"\bnew\s+Handle\s*\("));
        Assert.Matches(@"private\s+static\s+Handle\s+ToHandle\s*\(\s*long\s+value\s*\)\s*=>\s*new\s+Handle\s*\(\s*value\s*\)\s*;", code);
        // The origin key goes through the helper, and so does the light read of the witness; the handle 0 gives the empty key.
        Assert.Matches(@"originDefinitionHandle\s*<=\s*0\s*\?\s*string\.Empty\s*:\s*ToHandle\s*\(\s*originDefinitionHandle\s*\)\.ToString\s*\(\s*\)", AdapterRead());
        Assert.Contains("ToHandle(definitionHandle)", AdapterPreview());
        // The definition of the light read is resolved from the handle of the helper with TryGetObjectId; no other conversion exists.
        Assert.Equal(1, Src.Count(AdapterPreview(), @"\.TryGetObjectId\s*\(\s*handle\s*,"));
        Assert.DoesNotMatch(new Regex(@"Handle\.Parse|Convert\.ToInt64|\bnew\s+ObjectId\b|(?<!Try)GetObjectId\s*\("), code);
    }

    [Theory]
    [InlineData("var h = new Handle(other);", @"\bnew\s+Handle\s*\(")]
    [InlineData("var again = RackSiblingScan.CaptureDrawing(document);", @"\bCaptureDrawing\s*\(")]
    public void TheCountGuardsOfTheAdapterDetectASecondUseInItsRealText(string injected, string pattern)
    {
        var real = Adapter();

        Assert.Equal(1, Src.Count(real, pattern));
        Assert.Equal(2, Src.Count(real + "\n" + injected, pattern));
    }

    [Fact]
    public void TheReadBuildsTheInputFromTheScanAndTheEvidenceOnceAndAWitnessWithoutKeyFailsBeforeAnyInputIsBuilt()
    {
        var read = AdapterRead();
        var code = Adapter();

        Assert.Equal(1, Src.Count(code, @"\bnew\s+PushBackPilotIntentScan\s*\("));
        Assert.Equal(1, Src.Count(code, @"PushBackPilotIntentReader\.Read\s*\("));
        Assert.Matches(@"new\s+PushBackPilotIntentScan\s*\(\s*rackId\s*,\s*originKey\s*,\s*scan\.Facts\s*,\s*scan\.RawEnvelopes\s*,\s*CompletenessEvidence\s*,\s*witness\s*\)", read);
        Assert.Matches(@"CompletenessEvidence\s*=\s*""RackSiblingScan/CaptureDrawing""", code);
        // The constructor of the input throws for a blank key of the witness: the adapter answers a typed failure first.
        Assert.Matches(@"if\s*\(\s*string\.IsNullOrWhiteSpace\s*\(\s*witnessKey\s*\)\s*\)\s*return\s+new\s+RackPilotIntentReadResult\s*\(\s*null\s*,", read);
        AssertInOrder(read, "string.IsNullOrWhiteSpace(witnessKey)", "CaptureDrawing(", "new PushBackPilotIntentScan(", "PushBackPilotIntentReader.Read(");
        Assert.DoesNotMatch(new Regex(@"new\s+RackEmbedDocument|RackEmbedDocument|RackEmbedStore|RackEmbedComposer"), code);
    }

    [Fact]
    public void TheCatalogIsLoadedThroughTheOneWrapperWithATypedFailureAndWithoutAnyLock()
    {
        var code = Adapter();
        var catalog = Src.BodyOf(code, @"internal\s+static\s+RackPilotCatalogResult\s+LoadCatalog\s*\(");

        Assert.Equal(1, Src.Count(code, @"LateralHeaderDrawService\.LoadCatalog\s*\("));
        Assert.Contains("LateralHeaderDrawService.LoadCatalog()", catalog);
        Assert.Contains("catch (System.Exception", catalog);
        Assert.DoesNotMatch(new Regex(@"LockDocument|StartTransaction|MdiActiveDocument|Document\b"), catalog);
    }

    [Fact]
    public void TheAdapterKnowsNothingOfTheWorkspaceNorOfNavigation()
    {
        var code = Adapter();

        Assert.DoesNotMatch(new Regex(@"RackCad\.Application\.Workspace"), code);
        Assert.DoesNotMatch(
            new Regex(@"\b(?:NavigationTargets|NavigationTargetSet|NavigationTarget|NavigationDefinitionFact|INavigationFactsPort|NavigationDiagnostic|NavigationViewOrder|NavigationRevalidation|NavigationActionPolicy|NavigationActionKind|INavigationEffects|NavigationEcho)\b|WorkspaceNavigation|IWorkspaceEditIntentReader|EditIntentReadResult|EditIntentWitness|WorkspaceHostCore|WorkspaceController"),
            code);
    }

    [Theory]
    [InlineData("using RackCad.Application.Workspace;")]
    [InlineData("using RackCad.Application.Workspace.Drafts;")]
    [InlineData("var set = (NavigationTargetSet)null;")]
    [InlineData("var rack = new RackEmbedDocument();")]
    public void TheIndependenceGuardOfTheAdapterDetectsEachFamilyInItsRealText(string injected)
    {
        var real = Adapter();
        var pattern = new Regex(@"RackCad\.Application\.Workspace|\bNavigationTargetSet\b|RackEmbedDocument");

        Assert.DoesNotMatch(pattern, real);
        Assert.Matches(pattern, real + "\n" + injected);
    }

    // ------------------------------------------------------------------ INV-F4T2B2A-03: the nested reader of the navigation host

    [Fact]
    public void TheNestedReaderDoesTheSevenStepsInTheApprovedOrderAndTheAuthorizationComesBeforeTheRevalidation()
    {
        var body = ReaderBody();

        AssertInOrder(
            body,
            "MdiActiveDocument",
            "CommandInProgress",
            "RackPilotIntentAdapter.LoadCatalog()",
            "LockDocument()",
            "_authorize(",
            "_targets.Revalidate(",
            "WorkspaceEditIntent.ChooseOriginDefinition(",
            "RackPilotIntentAdapter.Read(");
        Assert.Matches(@"using\s*\(\s*document\.LockDocument\s*\(\s*\)\s*\)", body);
        Assert.Matches(@"string\.IsNullOrEmpty\(\s*document\.CommandInProgress\s*\)", body);
        Assert.Matches(@"\.Editor\.IsQuiescent", body);
        Assert.Contains("EditIntentReadResult.NoActiveDocument(", body);
        Assert.Contains("EditIntentReadResult.Busy(", body);
        Assert.Contains("EditIntentReadResult.Rejected(", body);
        Assert.Contains("EditIntentReadResult.Failed(", body);
        Assert.Contains("RequestDecision.Accepted", body);
        // The catalog is the only call made outside the lock, and it is made once in the whole host.
        Assert.Equal(1, Src.Count(Host(), @"RackPilotIntentAdapter\.LoadCatalog\s*\("));
        Assert.Equal(1, Src.Count(Host(), @"RackPilotIntentAdapter\.Read\s*\("));
        Assert.Equal(1, Src.Count(Host(), @"_targets\.Revalidate\s*\("));
        // It writes no selection, no view, no focus and no data.
        Assert.DoesNotMatch(new Regex(@"_effects|_runner|ReturnFocusToDrawing|SetImpliedSelection|SetCurrentView|\.Focus\s*\(|GetObject|\.Commit\s*\(|OpenMode|ForWrite"), body);
    }

    [Fact]
    public void TheNestedReaderIsTheOnlyNestedTypeOfTheHostImplementsTheInterfaceAndReusesTheTargetsOfTheHost()
    {
        var host = Host();

        Assert.Matches(@"private\s+sealed\s+class\s+PilotIntentReader\s*:\s*IWorkspaceEditIntentReader\b", host);
        // The host and its nested reader are the only two classes of the file, and no name carries Draft, Editor, Subscription, Tab or Stable.
        Assert.Equal(2, Src.Count(host, @"\bclass\s+\w+"));
        Assert.DoesNotMatch(new Regex(@"\bclass\s+\w*(?:Draft|Editor|Subscription|Tab|Stable)\w*", RegexOptions.IgnoreCase), host);
        Assert.Matches(@"internal\s+IWorkspaceEditIntentReader\s+EditIntentReader\s*=>\s*_editIntentReader\s*;", host);
        Assert.Matches(@"private\s+readonly\s+NavigationTargets\s+_targets\s*;", host);
        Assert.Matches(@"private\s+readonly\s+IWorkspaceEditIntentReader\s+_editIntentReader\s*;", host);
        Assert.Equal(1, Src.Count(host, @"\bnew\s+NavigationTargets\s*\("));
        Assert.Equal(1, Src.Count(host, @"\bnew\s+WorkspaceNavigationFacts\s*\("));
        Assert.Matches(@"_targets\s*=\s*new\s+NavigationTargets\s*\(\s*\w+\s*\)", host);
        Assert.Matches(@"new\s+WorkspaceNavigationRunner\s*\(\s*_targets\s*,", host);
        Assert.Matches(@"new\s+PilotIntentReader\s*\(\s*_targets\s*,\s*_authorize\s*\)", host);
        Assert.Equal(1, Src.Count(host, @"\bnew\s+PilotIntentReader\s*\("));
        // The nested reader receives the targets of the host; it never builds its own.
        var nested = host.Substring(IndexOf(host, "class PilotIntentReader"));
        Assert.DoesNotMatch(new Regex(@"new\s+(?:NavigationTargets|WorkspaceNavigationFacts|NavigationActionPolicy)\s*\("), nested);
    }

    [Fact]
    public void TheHostHandlesTheResultOfThePilotThatTheAdapterReturnsAsAValueAndKeepsNoFieldOfThePilotNorOfTheRead()
    {
        var host = Host();

        // The coupling is declared: the host passes the result of the pilot through, once, and never names a type of the pilot.
        Assert.Equal(1, Src.Count(host, @"\bread\.Pilot\b"));
        Assert.Contains("result of the pilot", Src.Text(HostPath));
        Assert.DoesNotMatch(new Regex(@"PushBackPilot|RackCad\.Application\.Systems"), host);
        Assert.DoesNotMatch(
            new Regex(@"^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:PushBackPilot\w*|EditIntentReadResult|EditIntentOutcome|EditCheckOutcome|DraftSession\S*|DraftIntentRead\S*|DraftRegistry|DraftStaleBase)\b[^(]*[;=]", RegexOptions.Multiline),
            host);
        Assert.DoesNotMatch(new Regex(@"TryBeginPilotEdit|TryCheckPilotEdit|WorkspaceHostCore"), host);
    }

    [Fact]
    public void TheHostStaysWithinTheTrapsOfTheNeighbouringGuards()
    {
        var host = Host();

        Assert.DoesNotMatch(new Regex(@"\.Result\b|GetResult|StartNew|ContinueWith|\+=|-="), host);
        Assert.Equal(1, Src.Count(host, @"RackLog\.Exception\s*\("));
        Assert.DoesNotMatch(new Regex(@"\b(?:async|await)\b|\bTask\.(?:Run|Delay|Factory)|Dispatcher"), host);
        Assert.Matches(@"public\s+Task<WorkspaceNavigationResult>\s+ExecuteAsync\s*\(", host);
        Assert.Matches(@"\bclass\s+WorkspaceNavigationHost\s*:\s*IWorkspaceNavigationPort\s*,\s*IWorkspaceBrowserSummaryPort\b", host);
        foreach (var line in host.Split('\n').Where(l => Regex.IsMatch(l, @"\bLockDocument\s*\(")))
            Assert.Matches(@"\busing\b", line);
    }

    // ------------------------------------------------------------------ the wiring in the controller

    [Fact]
    public void TheControllerAssignsTheReaderInTheNavigationGetterRightAfterCreatingTheHostAndNowhereElse()
    {
        var controller = Src.Code(ControllerPath);
        var getter = Src.BodyOf(controller, @"public\s+IWorkspaceNavigationPort\s+Navigation\b");
        var constructor = Src.BodyOf(controller, @"private\s+WorkspaceController\s*\(\s*\)");

        AssertInOrder(getter, "new WorkspaceNavigationHost(", "_host.SetEditIntentReader(_navigation.EditIntentReader);", "return _navigation;");
        Assert.Equal(1, Src.Count(controller, @"SetEditIntentReader\s*\("));
        Assert.DoesNotMatch(new Regex(@"SetEditIntentReader|EditIntentReader"), constructor);
        Assert.Equal(
            new[] { CorePath, ControllerPath }.OrderBy(p => p, StringComparer.Ordinal).ToArray(),
            Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bSetEditIntentReader\s*\(")).Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray());
        // The constructor of the controller and the arguments of the navigation host are the same as before.
        Assert.Matches(@"new\s+WorkspaceNavigationHost\s*\(\s*_echo\s*,\s*id\s*=>[^;]*\(request,\s*nativeId\)\s*=>\s*_bridge\.Authorize\s*\(\s*request\s*,\s*nativeId\s*\)\s*\)\s*;", getter);
    }

    // ------------------------------------------------------------------ INV-F4T2B2A-05 and 06: the host core

    private static readonly Regex NamesThePilot = new(@"RackCad\.Application\.Systems\.PushBack\.Pilot|\bPushBackPilot\w*");

    [Fact]
    public void AtMostTheHostCoreTheInterfaceAndTheIntentFileNameThePilotInTheApplicationWorkspaceFolder()
    {
        var allowed = new HashSet<string>(StringComparer.Ordinal) { CorePath, InterfacePath, IntentPath };

        var offenders = Src.ApplicationFiles().Where(f => NamesThePilot.IsMatch(f.Code) && !allowed.Contains(f.Path)).Select(f => f.Path).ToArray();

        Assert.Empty(offenders);
        // It is a maximum, not an exact set, and the three files exist; the host core and the intent file do name the pilot.
        Assert.True(Src.Exists(CorePath) && Src.Exists(InterfacePath) && Src.Exists(IntentPath));
        Assert.Matches(NamesThePilot, Core());
        Assert.Matches(NamesThePilot, Src.Code(IntentPath));
    }

    [Theory]
    [InlineData("WorkspaceSession.cs", "var state = new PushBackPilotDraftState(editor, inputs, origin);")]
    [InlineData("NavigationTargets.cs", "using RackCad.Application.Systems.PushBack.Pilot;")]
    [InlineData("BrowserSession.cs", "PushBackPilotIntentResult result = null;")]
    public void TheCouplingGuardDetectsTheNameOfThePilotInAnyOtherFileOfTheFolder(string file, string injected)
    {
        var real = Src.Code(Src.ApplicationWorkspace + file);

        Assert.DoesNotMatch(NamesThePilot, real);
        Assert.Matches(NamesThePilot, real + "\n" + injected);
    }

    [Fact]
    public void TheHostCoreOffersTheTwoOperationsAndTheAssignmentWithTheSignaturesOfThePackage()
    {
        var core = Core();

        Assert.Matches(@"public\s+EditIntentOutcome\s+TryBeginPilotEdit\s*\(\s*EditIntentRequest\s+request\s*\)", core);
        Assert.Matches(@"public\s+EditCheckOutcome\s+TryCheckPilotEdit\s*\(\s*EditIntentRequest\s+request\s*\)", core);
        Assert.Matches(@"public\s+void\s+SetEditIntentReader\s*\(\s*IWorkspaceEditIntentReader\s+reader\s*\)", core);
        Assert.Matches(@"private\s+readonly\s+DraftRegistry\s+_drafts\s*=\s*new\s*\(\s*\)\s*;", core);
        Assert.Matches(@"private\s+IWorkspaceEditIntentReader\?\s+_editIntentReader\s*;", core);
        // ONE registry and ONE close policy in the whole product, and they are the ones of the host core.
        Assert.Equal(
            new[] { CorePath },
            Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bnew\s+DraftRegistry\s*\(|\bDraftRegistry\s+\w+\s*=\s*new\s*\(\s*\)")).Select(f => f.Path).ToArray());
        Assert.Equal(
            new[] { CorePath },
            Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bnew\s+DraftClosePolicy\s*\(")).Select(f => f.Path).ToArray());
        Assert.Matches(@"_closePolicy\s*=\s*new\s+DraftClosePolicy\s*\(\s*_drafts\s*\)", core);
        // The constructor, the arguments of the host core and the port of the host do not change.
        Assert.Matches(
            @"public\s+WorkspaceHostCore\s*\(\s*IWorkspaceBridgeLink\s+bridge\s*,\s*IWorkspaceHostProbe\s+probe\s*,\s*IWorkspaceSelectionReader\s+reader\s*,\s*WorkspaceInstrumentation\s+instrumentation\s*,\s*Action<string,\s*Exception>\?\s+logFault\s*=\s*null\s*,\s*NavigationEcho\?\s+echo\s*=\s*null\s*\)",
            core);
        Assert.DoesNotMatch(new Regex(@"TryBeginPilotEdit|TryCheckPilotEdit|EditIntent"), Src.Code(Src.ApplicationWorkspace + "IWorkspaceHostPort.cs"));
    }

    [Fact]
    public void BothOperationsFollowTheOrderOfTheTableOfThePackageWithOneReadAndNoReadBeforeStepSix()
    {
        var intent = IntentBody();
        var check = CheckBody();

        AssertInOrder(intent, "string.IsNullOrWhiteSpace(request.RackId)", "PassGate(", "_drafts.Contains(", "_editIntentReader!.ReadIntent(", "_drafts.Begin(");
        AssertInOrder(check, "string.IsNullOrWhiteSpace(request.RackId)", "PassGate(", "_drafts.Find<", "_editIntentReader!.ReadIntent(", ".CheckNow(");
        Assert.Equal(1, Src.Count(intent, @"\.ReadIntent\s*\("));
        Assert.Equal(1, Src.Count(check, @"\.ReadIntent\s*\("));
        // The count is per body: the whole file is not counted, because a further operation of the host (F4-T2B2B) reads too.
        Assert.Equal(1, Src.Count(intent, @"_editIntentReader\s*[.!]"));
        Assert.Equal(1, Src.Count(check, @"_editIntentReader\s*[.!]"));

        // Steps 2 to 5 read nothing: the gate asks the probe and the authorization only.
        var gate = Src.BodyOf(Core(), @"private\s+EditGate\s+PassGate\s*\(");
        AssertInOrder(gate, "_editIntentReader == null", "_probe.ActiveDocumentNativeId", "_modalRackCadActive", "_probe.IsActiveDocumentQuiescent", "Authorize(");
        Assert.DoesNotMatch(new Regex(@"ReadIntent|_reader\b|_drafts|_bridge\.TryGetSession"), gate);
    }

    [Fact]
    public void EveryExceptionOfAnOperationIsContainedAsFailedWithoutDegradingTheSession()
    {
        foreach (var body in new[] { IntentBody(), CheckBody(), ConfirmBody(), KeepBody(), DiscardAndReloadBody() })
        {
            Assert.Matches(@"catch\s*\(\s*Exception\s+ex\s*\)", body);
            Assert.Contains("Report(", body);
            Assert.Matches(@"\.Failed\s*\(\s*ex\.Message\s*\)", body);
            Assert.DoesNotMatch(new Regex(@"_faulted|\bContain\s*\(|\bthrow\s*;"), body);
        }

        var report = Src.BodyOf(Core(), @"private\s+void\s+Report\s*\(");
        Assert.DoesNotMatch(new Regex(@"_faulted"), report);
        Assert.Contains("_logFault", report);
    }

    [Fact]
    public void NoOperationOfTheIntentionIsCalledFromTheDrainOrTheIdleOfAnyFile()
    {
        // Only the names of this task are prohibited in the drain and the idle (the operations, their reader, the gate and the
        // read); what F4-T2B2B does with the drafts in the drain is not decided here.
        var drain = Src.BodyOf(Core(), @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        Assert.DoesNotMatch(new Regex(@"TryBeginPilotEdit|TryCheckPilotEdit|TryConfirmPilotBase|TryKeepPilotEdit|TryDiscardAndReloadPilotEdit|_editIntentReader|ReadIntent|PassGate"), drain);
        Assert.Contains("_reader.Read(", drain);

        var idleOfTheCore = Src.BodyOf(Src.Code(BridgeCorePath), @"public\s+IReadOnlyList<DrainOutcome>\s+OnIdle\s*\(\s*\)");
        Assert.DoesNotMatch(new Regex(@"TryBeginPilotEdit|TryCheckPilotEdit|TryConfirmPilotBase|TryKeepPilotEdit|TryDiscardAndReloadPilotEdit|_editIntentReader|ReadIntent|PassGate|RackPilotIntentAdapter"), idleOfTheCore);
        var idleOfTheBridge = Src.BodyOf(Src.Code(BridgePath), @"private\s+void\s+OnIdle\s*\(\s*\)");
        Assert.DoesNotMatch(new Regex(@"TryBeginPilotEdit|TryCheckPilotEdit|TryConfirmPilotBase|TryKeepPilotEdit|TryDiscardAndReloadPilotEdit|_editIntentReader|ReadIntent|PassGate|RackPilotIntentAdapter"), idleOfTheBridge);

        var hostDrain = Src.BodyOf(Host(), @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        Assert.DoesNotMatch(new Regex(@"EditIntent|ReadIntent|RackPilotIntentAdapter|_targets"), hostDrain);

        var controller = Src.Code(ControllerPath);
        var controllerDrain = Src.BodyOf(controller, @"private\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        Assert.DoesNotMatch(new Regex(@"EditIntent|ReadIntent|SetEditIntentReader|TryBeginPilotEdit|TryCheckPilotEdit|TryConfirmPilotBase|TryKeepPilotEdit|TryDiscardAndReloadPilotEdit"), controllerDrain);

        // The callers of the nested reader are in the host core. Who calls the two operations is not fixed here: the drain and the idle
        // do not (above); since F4-T3A the edit port of the pilot exposes them to the UI, and since F4-T3B1 the panel calls that port.
        Assert.Equal(
            new[] { CorePath },
            Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\.ReadIntent\s*\(")).Select(f => f.Path).ToArray());
    }

    [Theory]
    [InlineData("var again = TryBeginPilotEdit(request);")]
    [InlineData("_editIntentReader.ReadIntent(session, rackId, null, null);")]
    [InlineData("var gate = PassGate(session, out var reason);")]
    [InlineData("var confirmed = TryConfirmPilotBase(request);")]
    [InlineData("var kept = TryKeepPilotEdit(request);")]
    [InlineData("var reloaded = TryDiscardAndReloadPilotEdit(request);")]
    public void TheDrainGuardDetectsACallOfTheIntentionInTheRealTextOfTheDrainOfTheHostCore(string injected)
    {
        var pattern = new Regex(@"TryBeginPilotEdit|TryCheckPilotEdit|TryConfirmPilotBase|TryKeepPilotEdit|TryDiscardAndReloadPilotEdit|_editIntentReader|ReadIntent|PassGate");
        var drain = Src.BodyOf(Core(), @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");

        Assert.DoesNotMatch(pattern, drain);
        Assert.Matches(pattern, drain + "\n" + injected);
    }

    // ------------------------------------------------------------------ obligation 10: the base of a result only reaches Begin, CheckNow or DiscardAndReload

    // The ways to use the base or the read of a result of the pilot: the base, the initial state, the read itself and its entries.
    private static readonly Regex BaseOrReadUse = new(@"\.StaleBase\b|\.InitialState\b|\b(?:pilot|result)\.Read\b|\.Entries\b");

    // The consumers that the obligation allows: the registry (Begin), the draft (CheckNow), the reload of F4-T2B2B and, since F4-T4A1
    // (D-T4A1-07), the general check (ConfirmBase) and the atomic keep (KeepMineOnCurrentVersion).
    private static readonly Regex AllowedConsumers = new(@"\.(?:Begin|DiscardAndReload|ConfirmBase|KeepMineOnCurrentVersion)\s*\([^;]*\)\s*;|\.CheckNow\s*\([^;]*\)\s*;");

    // The ONE other use of a read that the obligation allows, since F4-T4A1 (M-5): the diagnostic of the admission of a confirmed check, read
    // as pilot.Read.IsAdmitted and pilot.Read.Admission.Diagnostic and nothing else of the read. It is explicit and it is counted per
    // operation (only the body of the general check has it); no alias and no renamed variable stands for it.
    private const string AdmissionDiagnosticPattern = @"\bpilot\.Read\.IsAdmitted\s*\?\s*null\s*:\s*pilot\.Read\.Admission\.Diagnostic\b";

    private static readonly Regex AllowedAdmissionDiagnostic = new(AdmissionDiagnosticPattern);

    private static string WithoutTheAllowedConsumers(string code) =>
        AllowedAdmissionDiagnostic.Replace(AllowedConsumers.Replace(code, string.Empty), string.Empty);

    [Fact]
    public void TheBaseOfAResultOfThePilotOnlyReachesBeginCheckNowOrDiscardAndReloadInTheHostCoreAndTheTwoOperationsCountTheirOwnUses()
    {
        var core = Core();

        // Whatever the number of operations of the host, every use of the base or of the read of a result is an argument of a consumer.
        Assert.DoesNotMatch(BaseOrReadUse, WithoutTheAllowedConsumers(core));
        foreach (var code in new[] { Src.Code(IntentPath), Src.Code(InterfacePath) })
            Assert.DoesNotMatch(BaseOrReadUse, WithoutTheAllowedConsumers(code));

        // Per operation (these counts are about the two operations of this task, not about the whole file): the intention hands the
        // read to Begin and never the base to CheckNow, and "check now" hands the base to CheckNow and never the read to Begin.
        var intent = IntentBody();
        Assert.Equal(1, Src.Count(intent, @"\bpilot\.Read\b"));
        Assert.Matches(@"\.Begin\s*\(\s*[^;]*?\bpilot\.Read\b[^;]*?\)\s*;", intent);
        Assert.Equal(0, Src.Count(intent, @"\.StaleBase\b|\.CheckNow\s*\("));

        var check = CheckBody();
        Assert.Equal(1, Src.Count(check, @"\.StaleBase\b"));
        Assert.Matches(@"\.CheckNow\s*\(\s*\w+\.StaleBase\s*\)", check);
        Assert.Equal(0, Src.Count(check, @"\bpilot\.Read\b|\.Begin\s*\("));
    }

    [Fact]
    public void ThePerOperationCountsOfTheBaseAndOfTheReadInTheThreeBodiesOfTheReconciliationAreTheirOwnConsumer()
    {
        var confirm = ConfirmBody();
        Assert.Equal(1, Src.Count(confirm, @"\.StaleBase\b"));
        Assert.Matches(@"\.ConfirmBase\s*\(\s*\w+\.StaleBase\s*\)", confirm);
        Assert.Equal(0, Src.Count(confirm, @"\.Begin\s*\(|\.CheckNow\s*\("));
        // The one use of the read in the general check is the diagnostic of the admission of a confirmed check (M-5), and it is counted:
        // the read is named exactly twice in the body (IsAdmitted and Admission.Diagnostic), both inside that one explicit expression.
        Assert.Equal(1, Src.Count(confirm, AdmissionDiagnosticPattern));
        Assert.Equal(2, Src.Count(confirm, @"\bpilot\.Read\b"));
        Assert.Equal(0, Src.Count(WithoutTheAllowedConsumers(confirm), @"\bpilot\.Read\b"));
        foreach (var other in new[] { IntentBody(), CheckBody(), KeepBody(), DiscardAndReloadBody() })
            Assert.Equal(0, Src.Count(other, AdmissionDiagnosticPattern));

        var keep = KeepBody();
        Assert.Equal(1, Src.Count(keep, @"\bpilot\.Read\b"));
        Assert.Matches(@"\.KeepMineOnCurrentVersion\s*\(\s*pilot\.Read\s*,\s*PushBackPilotMaterializer\.KeepPilotEditsOn\s*,\s*confirmed\s*:\s*true\s*\)", keep);
        Assert.Equal(0, Src.Count(keep, @"\.StaleBase\b|\.InitialState\b|\.Begin\s*\("));

        var discard = DiscardAndReloadBody();
        Assert.Equal(1, Src.Count(discard, @"\bpilot\.Read\b"));
        Assert.Matches(@"\.DiscardAndReload\s*\(\s*pilot\.Read\s*\)", discard);
        Assert.Equal(0, Src.Count(discard, @"\.StaleBase\b|\.InitialState\b|\.Begin\s*\("));

        // The host never names the initial state of a draft: the keep gets it through the draft and the function of the pilot.
        Assert.DoesNotMatch(new Regex(@"\.InitialState\b|\.InitialDraftState\b"), Core());
    }

    // The read of a result of the pilot has ONE name in the host core, "pilot", and it is only ever assigned by one statement. So no alias
    // or renamed variable can stand for it and escape the guard of the use of the base (F4-T4A1, INV-F4T4A1-04).
    private static readonly Regex PilotReadAssignment = new(@"var\s+pilot\s*=\s*read\.PilotRead\s*;");

    [Fact]
    public void TheResultOfThePilotIsOnlyEverNamedPilotInTheHostCoreSoNoAliasEscapesTheGuardOfTheUseOfTheBase()
    {
        var core = Core();
        var withoutTheAssignments = PilotReadAssignment.Replace(core, string.Empty);

        Assert.DoesNotMatch(new Regex(@"\.PilotRead\b"), withoutTheAssignments);
        // One assignment per operation that reads: the intention, the check, the reload, the general check, the keep and the discard.
        Assert.Equal(6, PilotReadAssignment.Matches(core).Count);
        Assert.Equal(6, Src.Count(core, @"\.ReadIntent\s*\("));
    }

    [Theory]
    [InlineData("var reread = read.PilotRead;")]
    [InlineData("return read.PilotRead is { } reread && !reread.Read.IsAdmitted ? reread.Read.Admission.Diagnostic : null;")]
    [InlineData("var other = pilot; var s = other.StaleBase;")]
    public void TheAliasGuardDetectsARenamedWayToReachTheReadOrTheBaseInTheRealTextOfTheCore(string injected)
    {
        var core = Core();
        var real = PilotReadAssignment.Replace(core, string.Empty);

        Assert.DoesNotMatch(new Regex(@"\.PilotRead\b"), real);
        var injectedText = PilotReadAssignment.Replace(core + "\n" + injected, string.Empty);
        var detected = Regex.IsMatch(injectedText, @"\.PilotRead\b") || Regex.IsMatch(WithoutTheAllowedConsumers(injectedText), BaseOrReadUse.ToString());
        Assert.True(detected, injected);
    }

    [Fact]
    public void TheThreeBodiesOfTheReconciliationFollowTheStepsOfTheCheckAndTheReloadWithTheApplicabilityBeforeTheOneRead()
    {
        var confirm = ConfirmBody();
        var keep = KeepBody();
        var discard = DiscardAndReloadBody();

        AssertInOrder(confirm, "string.IsNullOrWhiteSpace(request.RackId)", "HasWitnessWithoutKey(request)", "PassGate(", "_drafts.Find<", "NotCheckableReason", "_editIntentReader!.ReadIntent(", ".ConfirmBase(");
        AssertInOrder(keep, "string.IsNullOrWhiteSpace(request.RackId)", "HasWitnessWithoutKey(request)", "PassGate(", "_drafts.Find<", "DraftStatus.Applying", "NotKeepableReason", "_editIntentReader!.ReadIntent(", ".KeepMineOnCurrentVersion(");
        AssertInOrder(discard, "string.IsNullOrWhiteSpace(request.RackId)", "HasWitnessWithoutKey(request)", "PassGate(", "_drafts.Find<", "DraftStatus.Applying", "_editIntentReader!.ReadIntent(", ".DiscardAndReload(");
        foreach (var body in new[] { confirm, keep, discard })
        {
            Assert.Equal(1, Src.Count(body, @"\.ReadIntent\s*\("));
            Assert.Equal(1, Src.Count(body, @"_editIntentReader\s*[.!]"));
            // The mark of known clean is removed in the result that changed the draft, never before the read.
            Assert.True(IndexOf(body, "_knownClean.Remove(") > IndexOf(body, "_editIntentReader!.ReadIntent("));
        }

        Assert.Equal(1, Src.Count(confirm, @"_knownClean\.Remove\s*\("));
        Assert.Equal(1, Src.Count(keep, @"_knownClean\.Remove\s*\("));
        Assert.Equal(1, Src.Count(discard, @"_knownClean\.Remove\s*\("));
        // The keep is called with the function of the pilot and the confirmation that the gesture already is.
        Assert.Contains("PushBackPilotMaterializer.KeepPilotEditsOn", keep);
        Assert.Contains("confirmed: true", keep);
    }

    [Theory]
    [InlineData("var copy = pilot.StaleBase;")]
    [InlineData("var initial = pilot.Read.InitialState;")]
    [InlineData("var state = result.Read;")]
    [InlineData("var text = baseOfRead.Entries;")]
    [InlineData("var kept = _kept = pilot.StaleBase;")]
    [InlineData("var s = pilot.StaleBase; draft.ConfirmBase(s);")]
    [InlineData("var r = pilot.Read; draft.KeepMineOnCurrentVersion(r, keepOn, true);")]
    [InlineData("var d = pilot.Read.Admission.Diagnostic;")]
    [InlineData("var d = pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Message;")]
    [InlineData("var a = !pilot.Read.IsAdmitted;")]
    public void TheBaseGuardDetectsAnotherUseOfTheBaseOrOfTheReadInTheRealTextOfTheCore(string injected)
    {
        var real = Core();

        Assert.DoesNotMatch(BaseOrReadUse, WithoutTheAllowedConsumers(real));
        Assert.Matches(BaseOrReadUse, WithoutTheAllowedConsumers(real + "\n" + injected));
    }

    [Theory]
    [InlineData("draft.DiscardAndReload(pilot.Read);")]
    [InlineData("draft.DiscardAndReload(reload.Read, pilot.StaleBase);")]
    [InlineData("var check = draft.CheckNow(pilot.StaleBase);")]
    [InlineData("var begun = _drafts.Begin(session, rack, pilot.Read, comparer, copy);")]
    [InlineData("var confirmed = draft.ConfirmBase(pilot.StaleBase);")]
    [InlineData("var kept = draft.KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true);")]
    [InlineData("return EditConfirmOutcome.Confirmed(pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic);")]
    public void TheBaseGuardAllowsTheConsumersOfTheObligation(string injected)
    {
        Assert.DoesNotMatch(BaseOrReadUse, WithoutTheAllowedConsumers(Core() + "\n" + injected));
    }

    [Fact]
    public void ThePluginSideHandlesTheResultOfThePilotOnlyAsAValueWithoutItsBaseItsReadOrItsInitialState()
    {
        // M-2: the adapter and the navigation host are the coupling that D-T2B2-05 declares; they build the result and pass it
        // through, and never open it.
        var pluginSide = new Regex(@"\.StaleBase\b|\.Read\.|\bInitialState\b");

        Assert.DoesNotMatch(pluginSide, Adapter());
        Assert.DoesNotMatch(pluginSide, Host());
    }

    [Theory]
    [InlineData("var b = read.Pilot.StaleBase;")]
    [InlineData("var d = result.Read.Admission;")]
    [InlineData("var s = pilot.Read.InitialState;")]
    [InlineData("var i = InitialState;")]
    public void ThePluginSideGuardDetectsEachWayToOpenTheResultInTheRealTextOfTheAdapterAndTheHost(string injected)
    {
        var pluginSide = new Regex(@"\.StaleBase\b|\.Read\.|\bInitialState\b");

        foreach (var real in new[] { Adapter(), Host() })
        {
            Assert.DoesNotMatch(pluginSide, real);
            Assert.Matches(pluginSide, real + "\n" + injected);
        }
    }

    // ------------------------------------------------------------------ obligation 3 (extended): a preview has no way to the intention

    private static readonly Regex IntentNamedByAPreview = new(@"EditIntent|PilotIntent|RackPilotIntentAdapter|PushBackPilot|TryBeginPilotEdit|TryCheckPilotEdit|ReadIntent|WorkspaceEditIntent");

    private static readonly string[] PreviewFiles =
    {
        "src/RackCad.Application/Workspace/BrowserRackSummaries.cs",
        "src/RackCad.Application/Workspace/BrowserSession.cs",
        "src/RackCad.Application/Workspace/BrowserSessionRegistry.cs",
        "src/RackCad.Application/Workspace/IWorkspaceBrowserSummaryPort.cs",
    };

    [Fact]
    public void TheSummaryMethodsOfTheNavigationHostAndTheFourPreviewFilesNameNothingOfTheNewEntry()
    {
        var host = Host();
        foreach (var signature in new[]
                 {
                     @"public\s+BrowserRackSummary\s+ReadSummary\s*\(",
                     @"private\s+BrowserRackSummary\s+ReadSummaryOnActiveDocument\s*\(",
                     @"public\s+bool\s+IsSessionAlive\s*\(",
                     @"public\s+void\s+OpenDrainNotice\s*\(",
                     @"public\s+void\s+CloseDrainNotice\s*\(",
                 })
        {
            var body = Src.BodyOf(host, signature);
            Assert.True(!IntentNamedByAPreview.IsMatch(body), signature + " names " + IntentNamedByAPreview.Match(body).Value);
        }

        Assert.Equal(4, PreviewFiles.Length);
        foreach (var path in PreviewFiles)
        {
            var code = Src.Code(path);
            Assert.NotEmpty(code);
            Assert.True(!IntentNamedByAPreview.IsMatch(code), path + " names " + IntentNamedByAPreview.Match(code).Value);
        }
    }

    [Theory]
    [InlineData("var read = _editIntentReader.ReadIntent(session, rack, null, null);")]
    [InlineData("var r = RackPilotIntentAdapter.Read(document, rack, 0, null, null, catalog);")]
    [InlineData("var outcome = core.TryBeginPilotEdit(request);")]
    [InlineData("var request = new EditIntentRequest(session, rack, null, null);")]
    public void TheSummaryGuardDetectsEachNameOfTheNewEntryInTheRealTextOfEachPreviewFile(string injected)
    {
        foreach (var path in PreviewFiles)
        {
            var real = Src.Code(path);

            Assert.DoesNotMatch(IntentNamedByAPreview, real);
            Assert.Matches(IntentNamedByAPreview, real + "\n" + injected);
        }

        var summary = Src.BodyOf(Host(), @"private\s+BrowserRackSummary\s+ReadSummaryOnActiveDocument\s*\(");
        Assert.DoesNotMatch(IntentNamedByAPreview, summary);
        Assert.Matches(IntentNamedByAPreview, summary + "\n" + injected);
    }

    // ------------------------------------------------------------------ INV-F4T2B2A-09 and 10: no lifecycle, no forbidden surface

    [Fact]
    public void TheBodiesOfTheTwoOperationsCallNoOperationOfTheLifecycleOfTheDrafts()
    {
        // The lifecycle (advisor, close veto, retirement, discard, stale marking, reload) is of F4-T2B2B: the absence of all of it in
        // this task is shown by the diff, the scope and the review, and no guard prohibits it for ever. What is fixed here is only
        // that the two operations of THIS task do not call such an operation.
        var lifecycle = new Regex(@"\.(?:RetireSession|Discard|DiscardAll|DiscardAndReload|MarkPossiblyStale|KeepMineOnCurrentVersion|EvaluateClose)\s*\(|\bEnterError\b");

        foreach (var body in new[] { IntentBody(), CheckBody() })
            Assert.True(!lifecycle.IsMatch(body), "lifecycle call: " + lifecycle.Match(body).Value);
    }

    [Theory]
    [InlineData("_drafts.RetireSession(request.Session);")]
    [InlineData("draft.DiscardAndReload(pilot.Read);")]
    [InlineData("draft.MarkPossiblyStale();")]
    [InlineData("var verdict = _closePolicy.EvaluateClose(request.Session);")]
    public void TheLifecycleGuardOfTheTwoOperationsDetectsEachCallInTheRealTextOfTheirBodies(string injected)
    {
        var lifecycle = new Regex(@"\.(?:RetireSession|Discard|DiscardAll|DiscardAndReload|MarkPossiblyStale|KeepMineOnCurrentVersion|EvaluateClose)\s*\(|\bEnterError\b");

        foreach (var body in new[] { IntentBody(), CheckBody() })
        {
            Assert.DoesNotMatch(lifecycle, body);
            Assert.Matches(lifecycle, body + "\n" + injected);
        }
    }

    [Fact]
    public void TheFoldersKeepTheirFilesAndTheTwoNewFilesOfTheApplicationAreExactFiles()
    {
        var plugin = Src.PluginFiles().Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray();
        Assert.Equal(9, plugin.Length);
        Assert.Empty(plugin.Where(p => p.Substring(Src.PluginWorkspace.Length).Contains('/')));
        Assert.DoesNotContain(plugin, p => p.Contains("Intent", StringComparison.Ordinal));

        var application = Src.ApplicationFiles().Select(f => f.Path).ToHashSet(StringComparer.Ordinal);
        Assert.Contains(InterfacePath, application);
        Assert.Contains(IntentPath, application);
        Assert.True(Src.Exists(AdapterPath));
        Assert.Equal(new[] { AdapterPath }, Src.Files("src/RackCad.Plugin/Views/").Where(f => f.Path.EndsWith("RackPilotIntentAdapter.cs", StringComparison.Ordinal)).Select(f => f.Path).ToArray());
    }

    // ------------------------------------------------------------------ the new files of Application: purity and the traps of the neighbouring guards

    [Fact]
    public void TheThreeFilesOfTheApplicationAreNullableEnabledNameNoHostTypeAndKeepTheWordsThatTheNeighbouringGuardsForbidEvenInComments()
    {
        foreach (var path in new[] { CorePath, InterfacePath, IntentPath })
        {
            var text = Src.Text(path);

            Assert.StartsWith("#nullable enable", text.TrimStart((char)0xFEFF));
            // The words that the guards of the folder forbid, also in comments (the package asks for it).
            Assert.DoesNotMatch(new Regex(@"Transaction|\bDatabase\b|\bCommit\s*\("), text);
            Assert.DoesNotMatch(new Regex(@"ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom", RegexOptions.IgnoreCase), text);
            Assert.DoesNotMatch(new Regex(@"\bAutodesk\.|System\.Windows|RackCad\.Plugin|RackCad\.UI|System\.IO|\bFile\.|\bRegistry\b|RackLog"), Src.CodeOnly(text));
            Assert.DoesNotMatch(new Regex(@"\basync\b|\bawait\b|\bTask\b|\bDispatcher\b|\bThread\b|\bTimer\b|\bGuid\.NewGuid"), Src.CodeOnly(text));
        }

        Assert.DoesNotMatch(new Regex(@"\bRegistry\b"), Core());
        Assert.Equal(1, Src.Count(Src.Code(IntentPath), @"\bnamespace\s+RackCad\.Application\.Workspace\s*\{"));
        Assert.Equal(1, Src.Count(Src.Code(InterfacePath), @"\bnamespace\s+RackCad\.Application\.Workspace\s*\{"));
    }

    [Fact]
    public void TheInterfaceHasTheOneMethodOfThePackageAndNoTypeOfDraftsOrOfNavigation()
    {
        var code = Src.Code(InterfacePath);

        Assert.Matches(@"\bpublic\s+interface\s+IWorkspaceEditIntentReader\b", code);
        Assert.Equal(1, Src.Count(code, @"\bEditIntentReadResult\s+ReadIntent\s*\(\s*SessionId\s+session\s*,\s*string\s+rackId\s*,\s*long\?\s+chosenReference\s*,\s*EditIntentWitness\?\s+witness\s*\)\s*;"));
        Assert.DoesNotMatch(new Regex(@"Draft|Navigation|PushBackPilot|Document|Autodesk"), code);
    }

    [Fact]
    public void TheChoiceOfTheOriginLivesInTheIntentFileAsAStaticFunctionOfTheSetOfTargetsAndTheChosenReference()
    {
        var code = Src.Code(IntentPath);

        Assert.Matches(@"public\s+static\s+long\s+ChooseOriginDefinition\s*\(\s*NavigationTargetSet\s+set\s*,\s*long\?\s+chosenReference\s*\)", code);
        var body = Src.BodyOf(code, @"public\s+static\s+long\s+ChooseOriginDefinition\s*\(");
        Assert.Contains("set.Primary", body);
        Assert.Contains("DefinitionHandle", body);
        Assert.DoesNotMatch(new Regex(@"OwnerId|ObjectId|\bHandle\b|Revalidate|Resolve\s*\("), body);
        Assert.Equal(1, Src.Count(code, @"public\s+static\s+class\s+WorkspaceEditIntent\b"));
    }
}
