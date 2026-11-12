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
// ASSERTION when the text it expects is missing (a missing file or member is an assertion too), and every family of prohibition has a
// detection test that injects an offending line into the real text.
//
// Evidence named by the package: R6 (no write to the drawing: the adapter, the scan, the host and the core), obligation 3 (the
// preview never reaches the intention), obligation 10 (the base of a read only reaches Begin or CheckNow) and obligation 13 (the
// session of A never operates on B: the order of the nested reader, with the authorization before the revalidation).
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

        Assert.Matches(@"internal\s+IReadOnlyDictionary<string,\s*string>\s+RawEnvelopes\s*\{\s*get;\s*\}", scan);
        Assert.Matches(@"var\s+rawEnvelopes\s*=\s*new\s+Dictionary<string,\s*string>\s*\(\s*StringComparer\.Ordinal\s*\)\s*;", scan);
        Assert.Equal(1, Src.Count(scan, @"\brawEnvelopes\.Add\s*\(\s*key\s*,\s*json\s*\)"));
        Assert.Equal(1, Src.Count(scan, @"new\s+ReadOnlyDictionary<string,\s*string>\s*\(\s*rawEnvelopes\s*\)"));
        Assert.Equal(1, Src.Count(scan, @"RackBlockData\.Read\s*\("));
        // The text is added right after it was read and only for a definition that has text.
        AssertInOrder(scan, "RackBlockData.Read(", "string.IsNullOrEmpty(json)", "rawEnvelopes.Add(key, json)");
    }

    [Fact]
    public void NoSignatureOfTheScanChangedAndItStillWritesNothing()
    {
        var scan = Src.Code(ScanPath);

        Assert.Matches(
            @"internal\s+static\s+RackSiblingScanSnapshot\s+Capture\s*\(\s*Document\s+document\s*,\s*ObjectId\s+selectedDefinition\s*,\s*string\s+rackId\s*,\s*string\s+originalSourceId\s*,\s*bool\s+originalSourceIdIsAttributable\s*,\s*Func<RackEmbedDocument,\s*bool>\s+eraseByKind\s*\)",
            scan);
        Assert.Matches(@"internal\s+static\s+RackSiblingDrawingScan\s+CaptureDrawing\s*\(\s*Document\s+document\s*\)", scan);
        Assert.Matches(
            @"private\s+static\s+RackSiblingDrawingScan\s+Traverse\s*\(\s*Document\s+document\s*,\s*ObjectId\s+selectedDefinition\s*,\s*Func<RackEmbedDocument,\s*bool>\s+eraseByKind\s*\)",
            scan);
        // The five original arguments of the constructor stay, and the new one is optional, so no existing caller changes.
        Assert.Matches(
            @"internal\s+RackSiblingDrawingScan\s*\(\s*IReadOnlyList<RackSiblingScanFact>\s+facts\s*,\s*IReadOnlyDictionary<string,\s*ObjectId>\s+definitions\s*,\s*IReadOnlyDictionary<string,\s*RackEmbedDocument>\s+envelopes\s*,\s*IReadOnlyDictionary<string,\s*CustomPropertiesReadResult>\s+properties\s*,\s*IReadOnlyDictionary<string,\s*IReadOnlyList<RackSiblingLayerRequirement>>\s+layers\s*,\s*IReadOnlyDictionary<string,\s*string>\s+rawEnvelopes\s*=\s*null\s*\)",
            scan);
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
        Assert.Equal(2, Src.Count(Core(), @"\.ReadIntent\s*\("));
        Assert.Equal(2, Src.Count(Core(), @"_editIntentReader\s*[.!]"));

        // Steps 2 to 5 read nothing: the gate asks the probe and the authorization only.
        var gate = Src.BodyOf(Core(), @"private\s+EditGate\s+PassGate\s*\(");
        AssertInOrder(gate, "_editIntentReader == null", "_probe.ActiveDocumentNativeId", "_modalRackCadActive", "_probe.IsActiveDocumentQuiescent", "Authorize(");
        Assert.DoesNotMatch(new Regex(@"ReadIntent|_reader\b|_drafts|_bridge\.TryGetSession"), gate);
    }

    [Fact]
    public void EveryExceptionOfAnOperationIsContainedAsFailedWithoutDegradingTheSession()
    {
        foreach (var body in new[] { IntentBody(), CheckBody() })
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
        var drain = Src.BodyOf(Core(), @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        Assert.DoesNotMatch(new Regex(@"TryBeginPilotEdit|TryCheckPilotEdit|_editIntentReader|ReadIntent|_drafts|PassGate"), drain);
        Assert.Contains("_reader.Read(", drain);

        var hostDrain = Src.BodyOf(Host(), @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        Assert.DoesNotMatch(new Regex(@"EditIntent|ReadIntent|RackPilotIntentAdapter|_targets"), hostDrain);

        var controller = Src.Code(ControllerPath);
        var controllerDrain = Src.BodyOf(controller, @"private\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");
        Assert.DoesNotMatch(new Regex(@"EditIntent|ReadIntent|SetEditIntentReader|TryBeginPilotEdit|TryCheckPilotEdit"), controllerDrain);

        // Nothing but the host core calls the two operations, and the callers of the nested reader are the two operations only.
        Assert.Equal(
            new[] { CorePath },
            Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bTry(?:Begin|Check)PilotEdit\s*\(")).Select(f => f.Path).ToArray());
        Assert.Equal(
            new[] { CorePath },
            Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\.ReadIntent\s*\(")).Select(f => f.Path).ToArray());
    }

    [Theory]
    [InlineData("var again = TryBeginPilotEdit(request);")]
    [InlineData("_editIntentReader.ReadIntent(session, rackId, null, null);")]
    [InlineData("var gate = PassGate(session, out var reason);")]
    public void TheDrainGuardDetectsACallOfTheIntentionInTheRealTextOfTheDrainOfTheHostCore(string injected)
    {
        var pattern = new Regex(@"TryBeginPilotEdit|TryCheckPilotEdit|_editIntentReader|ReadIntent|_drafts|PassGate");
        var drain = Src.BodyOf(Core(), @"public\s+void\s+OnDrained\s*\(\s*DrainOutcome\s+\w+\s*\)");

        Assert.DoesNotMatch(pattern, drain);
        Assert.Matches(pattern, drain + "\n" + injected);
    }

    // ------------------------------------------------------------------ obligation 10: the base of a read goes only to Begin or CheckNow

    private static readonly Regex StaleBaseUse = new(@"\.StaleBase\b");

    [Fact]
    public void TheBaseOfAReadOnlyReachesBeginOrCheckNowAndTheReadOfTheIntentionOnlyReachesBegin()
    {
        var files = new[] { Core(), Src.Code(IntentPath), Src.Code(InterfacePath) };

        var uses = files.Sum(code => StaleBaseUse.Matches(code).Count);
        var checks = files.Sum(code => Regex.Matches(code, @"\.CheckNow\s*\(\s*\w+\.StaleBase\s*\)").Count);
        Assert.Equal(1, uses);
        Assert.Equal(uses, checks);

        // Every use of the read of the intention (the admission, the initial state and the base of ONE read) is the argument of Begin.
        var core = Core();
        Assert.Equal(1, Src.Count(core, @"\bpilot\.Read\b(?!Intent)"));
        Assert.Matches(@"\.Begin\s*\(\s*[^;]*?\bpilot\.Read\b[^;]*?\)\s*;", core);
        // No other path of a base: no rebase and no reload in this task (they are of F4-T2B2B).
        Assert.DoesNotMatch(new Regex(@"DiscardAndReload|KeepMineOnCurrentVersion|\.StaleBase\s*="), core);
    }

    [Theory]
    [InlineData("var copy = pilot.StaleBase;")]
    [InlineData("draft.DiscardAndReload(pilot.Read);")]
    [InlineData("var text = baseOfRead.Entries;")]
    public void TheBaseGuardDetectsAnotherUseOfTheBaseInTheRealTextOfTheCore(string injected)
    {
        var real = Core();
        var pattern = new Regex(@"\.StaleBase\b(?!\s*\))|DiscardAndReload|\.Entries\b");

        Assert.DoesNotMatch(pattern, real);
        Assert.Matches(pattern, real + "\n" + injected);
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
    public void NothingOfTheLifecycleOfF4T2B2BWasImplemented()
    {
        Assert.False(Src.Exists("src/RackCad.Application/Workspace/IWorkspaceLifecycleAdvisor.cs"));
        Assert.Empty(Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"IWorkspaceLifecycleAdvisor|SetLifecycleAdvisor|\bAdviseClose\b|\bSessionDestroyed\s*\(")).Select(f => f.Path));
        foreach (var path in new[] { BridgeCorePath, BridgePath })
            Assert.DoesNotMatch(new Regex(@"EditIntent|Advisor|Pilot|Draft|ReadIntent"), Src.Code(path));

        // No retirement, discard, stale marking, reload or close veto from the host core, the navigation host or the controller.
        var lifecycle = new Regex(@"\.(?:RetireSession|Discard|DiscardAll|DiscardAndReload|MarkPossiblyStale|KeepMineOnCurrentVersion|EvaluateClose)\s*\(|MarkDraftsPossiblyStale\s*[=(]|\bEnterError\b");
        foreach (var code in new[] { Core(), Host(), Src.Code(ControllerPath), Src.Code(IntentPath), Src.Code(InterfacePath) })
            Assert.True(!lifecycle.IsMatch(code), "lifecycle call: " + lifecycle.Match(code).Value);

        // The drafts core is untouched: its five files, and none names the pilot or the new entry.
        var drafts = Src.Files("src/RackCad.Application/Workspace/Drafts/");
        Assert.Equal(5, drafts.Count);
        foreach (var (path, code) in drafts)
            Assert.DoesNotMatch(new Regex(@"EditIntent|PushBackPilot|IWorkspaceEditIntentReader"), code);
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
