#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T4C1-HOST (D-T4C-14, D-T4C-20 rows 19 and 22; INV-F4T4C-03, 04, 06, 13, 14, 15 and 19): "NO SECOND WRITE PATH". The guards of the whole
// product for the integration of the update of the pilot, each with its negative over the REAL text: the purity of the runner, the guard of
// the adapter, G-1 to G-7, G-9 and G-10, the census of Task and async, I-01, M-01, the callers of EnterError and the handler of the command
// that is not the update. The Core test project cannot reference the Plugin (it needs AutoCAD), so the product is read as text.
public class WorkspacePilotUpdateIntegrationGuardTests
{
    private const string AdapterPath = "src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs";
    private const string WriterPath = "src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs";
    private const string HostRunPath = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs";
    private const string RunPath = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs";
    private const string CorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string GatePath = "src/RackCad.Application/Workspace/WorkspacePilotSupport.cs";
    private const string IntentAdapterPath = "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs";
    private const string NavigationHostPath = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs";

    private static IReadOnlyList<(string Path, string Code)> Product() => Src.AllProduct();

    private static IReadOnlyList<(string Path, string Code)> With(IReadOnlyList<(string Path, string Code)> product, string path, Func<string, string> change) =>
        product.Select(f => f.Path == path ? (f.Path, change(f.Code)) : f).ToList();

    private static IReadOnlyList<(string Path, string Code)> Append(IReadOnlyList<(string Path, string Code)> product, string path, string text) =>
        With(product, path, code => code + "\n" + text);

    private static string CodeOf(string path) => Src.Code(path);

    // ================================================================== the purity of the runner (D-T4C-04 (a))

    // Each prohibition with the line that must trip it. The word Transaction is read RAW, comments included: nothing of the pilot folder may
    // even mention it (H-06). The others are read without comments.
    private static readonly (string Name, Regex Pattern, string Injected)[] RunnerImpure =
    {
        ("Task", new Regex(@"\bTask\b"), "Task t = null;"),
        ("async", new Regex(@"\basync\b"), "async void M() { }"),
        ("await", new Regex(@"\bawait\b"), "await x;"),
        ("Dispatcher", new Regex(@"Dispatcher"), "var d = Dispatcher.CurrentDispatcher;"),
        ("Timer", new Regex(@"\bTimer\b"), "var t = new Timer();"),
        ("Autodesk", new Regex(@"Autodesk"), "using Autodesk.AutoCAD.ApplicationServices;"),
        ("Database", new Regex(@"\bDatabase\b"), "Database db = null;"),
        ("LockDocument", new Regex(@"LockDocument"), "document.LockDocument();"),
        ("reflection", new Regex(@"System\.Reflection|\btypeof\s*\(|\.GetType\s*\("), "var t = typeof(int);"),
        ("files", new Regex(@"System\.IO|\bFile\.|StreamWriter|\bDirectory\."), "System.IO.File.ReadAllText(x);"),
        ("the workspace", new Regex(@"Workspace"), "using RackCad.Application.Workspace;"),
        ("the update of T4B", new Regex(@"TryUpdatePilot|PushBackPilotUpdateRun\.Execute|\bPrecheck\b|TryBeginApply|CompleteApply|\bRebase\b|MarkPossiblyStale"), "run.CompleteApply(x);"),
    };

    private static IEnumerable<string> RunnerViolations(string raw)
    {
        if (raw.Contains("Transaction", StringComparison.Ordinal)) yield return "the word Transaction";
        var code = Src.CodeOnly(raw);
        foreach (var (name, pattern, _) in RunnerImpure)
            if (pattern.IsMatch(code)) yield return name;

        // A true, total runner: OnCallback and OnCompleted begin with a try whose catch never throws.
        foreach (var name in new[] { "OnCallback", "OnCompleted" })
        {
            var body = Src.BodyOf(code, @"private\s+void\s+" + name + @"\s*\(\s*\)");
            if (!Regex.IsMatch(body, @"^private\s+void\s+" + name + @"\s*\(\s*\)\s*\{\s*try\b")) yield return name + " does not begin with a try";
            foreach (Match match in Regex.Matches(body, @"catch\s*\([^)]*\)\s*\{[^{}]*\}"))
                if (Regex.IsMatch(match.Value, @"\bthrow\b")) yield return name + " throws from a catch";
        }
    }

    [Fact]
    public void TheRunnerIsPureAndTotalAndNamesNeitherTheWorkspaceNorTheUpdateOfT4B()
    {
        Assert.Empty(RunnerViolations(Src.Text(HostRunPath)));
        Assert.DoesNotContain(Src.Files("src/RackCad.Application/Systems/PushBack/Pilot"), f => Src.Text(f.Path).Contains("Transaction", StringComparison.Ordinal));
    }

    [Fact]
    public void ThePurityGuardOfTheRunnerTripsOnEachProhibitionInjectedIntoTheRealText()
    {
        var real = Src.Text(HostRunPath);

        Assert.Empty(RunnerViolations(real));
        Assert.NotEmpty(RunnerViolations(real + "\nvar t = db.TransactionManager;"));
        Assert.NotEmpty(RunnerViolations(real + "\n// a comment that names a Transaction"));
        foreach (var (name, _, injected) in RunnerImpure)
            Assert.True(RunnerViolations(real + "\n" + injected).Any(v => v == name), name);
    }

    [Fact]
    public void TheTotalityGuardOfTheRunnerTripsOnACallbackWithoutATryAndOnAThrowInACatch()
    {
        var real = Src.Text(HostRunPath);
        var body = Src.BodyOf(Src.CodeOnly(real), @"private\s+void\s+OnCallback\s*\(\s*\)");

        Assert.NotEmpty(RunnerViolations(real.Replace(body, "private void OnCallback() { RunSteps(); }", StringComparison.Ordinal)));
        Assert.NotEmpty(RunnerViolations(real.Replace(
            "Log(\"PushBackPilotHostRunner.OnCompleted\", ex);\n            }\n        }", "Log(\"PushBackPilotHostRunner.OnCompleted\", ex); throw;\n            }\n        }", StringComparison.Ordinal)
            .Replace("Log(\"PushBackPilotHostRunner.OnCompleted\", ex);\r\n            }\r\n        }", "Log(\"PushBackPilotHostRunner.OnCompleted\", ex); throw;\r\n            }\r\n        }", StringComparison.Ordinal)));
    }

    // ================================================================== the adapter of the command context (D-T4C-04 (b))

    private static readonly string[] WriteApiNames =
    {
        "RedrawInTransaction", "RedrawInPlace", "DrawAndPlace", "SystemBlockWriter", "RackBlockData.Write", "ForWrite", "UpgradeOpen", "AppendEntity",
        ".Commit(", "StartTransaction", "StartOpenCloseTransaction", "SyncName", "PurgeUnreferenced", "EnsureForPlan", "WblockCloneObjects", "SetAt(",
        ".Erase(", "SendStringToExecute", "CommandMethod", "RackPushBackCommands", "EditPushBack", "PushBackKindHandler", "RackMenuCommands",
        "RackViewBatchDriver", "SiblingRedrawPlan", "AutoCadSiblingRedrawPort", "RackSelectivoInsertIntegration", "RackCommandSupport",
    };

    private static readonly Regex WriteApi = new(
        @"RedrawInTransaction|RedrawInPlace|DrawAndPlace|SystemBlockWriter|RackBlockData\.Write|\bForWrite\b|\bUpgradeOpen\b|\bAppendEntity\b|\.Commit\s*\(|StartTransaction|StartOpenCloseTransaction|"
        + @"\bSyncName\b|PurgeUnreferenced|EnsureForPlan|WblockCloneObjects|\bSetAt\s*\(|\.Erase\s*\(|SendStringToExecute|CommandMethod|RackPushBackCommands|EditPushBack|PushBackKindHandler|"
        + @"RackMenuCommands|RackViewBatch\w*|SiblingRedraw\w*|AutoCadSiblingRedrawPort|RackSelectivoInsertIntegration|RackCommandSupport");

    private static readonly Regex Blocking = new(
        @"\.Wait\s*\(|\.Result\b|GetAwaiter|ConfigureAwait|Task\.Run|Task\.Factory|StartNew|ContinueWith|async\s+void|\bThread\b|ThreadPool|\bTimer\b|\bDispatcher\b|TaskScheduler|\basync\b|\bawait\b");

    private static IEnumerable<string> AdapterViolations(string code)
    {
        // Exactly one of each, in their places.
        foreach (var (name, pattern) in new[]
                 {
                     ("SynchronizationContext.Current", @"SynchronizationContext\.Current"),
                     ("ExecuteInCommandContextAsync", @"ExecuteInCommandContextAsync"),
                     (".OnCompleted(", @"\.OnCompleted\s*\("),
                     (".Post(", @"\.Post\s*\("),
                     (".LockDocument()", @"\.LockDocument\s*\(\s*\)"),
                     ("new RackPilotUpdateWriter(", @"\bnew\s+RackPilotUpdateWriter\s*\("),
                 })
            if (Src.Count(code, pattern) != 1) yield return name + " is not exactly once";

        var context = code.IndexOf("SynchronizationContext.Current", StringComparison.Ordinal);
        var call = code.IndexOf("ExecuteInCommandContextAsync", StringComparison.Ordinal);
        if (context < 0 || call < 0 || context > call) yield return "the scheduler is not captured before the call";

        // The completion: at most one read of IsCompleted, at most one GetResult and only inside a try; OnCompleted only on the branch where
        // IsCompleted is false (the branch that returns when it is true comes first).
        if (Src.Count(code, @"\.IsCompleted\b") > 1) yield return "IsCompleted is read more than once";

        // M-07: the raw continuation hangs only from the branch where IsCompleted is false, which comes after the branch that returns when it is
        // true; and the hop posts to the scheduler that was captured before the call, in the one place that posts.
        var early = Regex.Match(code, @"if\s*\(\s*result\.IsCompleted\s*\)\s*\{[^{}]*\breturn\s*;\s*\}");
        var continuation = code.IndexOf(".OnCompleted(", StringComparison.Ordinal);
        if (!early.Success || continuation < early.Index + early.Length) yield return "OnCompleted is not on the branch of an IsCompleted that is false";
        if (!Regex.IsMatch(code, @"\bHop\s*\(\s*SynchronizationContext\s+context\s*,\s*Action\s+completed\s*\)\s*=>\s*context\s*\.\s*Post\s*\(")) yield return "the Post is not the one of the context that was captured";
        if (Src.Count(code, @"\bHop\s*\(\s*context\s*,\s*completed\s*\)") != 2) yield return "Hop is not called, twice, with the captured context";

        // M-04: the target answers the reason of the run and no text of its own.
        var open = Src.BodyOf(code, @"public\s+bool\s+TryOpenTarget\s*\(");
        if (open.Contains('"')) yield return "TryOpenTarget carries a literal text";
        if (!open.Contains("PushBackPilotUpdateRun.DocumentMismatchReason", StringComparison.Ordinal)) yield return "TryOpenTarget does not answer the reason of the run";
        var getResults = Regex.Matches(code, @"\.GetResult\s*\(").Count;
        if (getResults > 1) yield return "GetResult is called more than once";
        if (getResults == 1 && !Regex.IsMatch(code, @"try\s*\{[^{}]*\.GetResult\s*\(")) yield return "GetResult is outside a try";

        // No fields but the exact document of the target.
        if (Src.Count(code, @"private\s+readonly\s+Document\s+_document\s*;") != 1) yield return "the target does not have its one exact field";
        var fields = Regex.Matches(code, @"\b(?:private|internal|public|protected)\s+(?:static\s+|readonly\s+)*[\w.<>?\[\],]+\s+\w+\s*(?:;|=(?!>))").Count;
        if (fields != 1) yield return "the adapter has " + fields + " fields (expected the one of the target)";
        if (Regex.IsMatch(code, @"\bstatic\s+[^(){};=]*[;=]")) yield return "the adapter has a static field";

        // What the precedent forbids, and what the adapter must never do or name.
        if (Blocking.IsMatch(code)) yield return "the adapter blocks, schedules or names " + Blocking.Match(code).Value;
        if (Regex.IsMatch(code, @"StartTransaction|StartOpenCloseTransaction|\.Commit\s*\(|TopTransaction")) yield return "the adapter opens or names a transaction";
        if (WriteApi.IsMatch(Regex.Replace(code, @"\bnew\s+RackPilotUpdateWriter\b", string.Empty))) yield return "the adapter names the write API " + WriteApi.Match(code).Value;
        if (Regex.IsMatch(code, @"RackCad\.Application\.Workspace|RackCad\.Plugin\.Workspace")) yield return "the adapter names the workspace";
        if (Regex.IsMatch(code, @"TryUpdatePilot|PushBackPilotUpdateRun\.Execute|\bPrecheck\b|TryBeginApply|CompleteApply|\bRebase\b|MarkPossiblyStale")) yield return "the adapter names the update of T4B";

        // Total by text: the callback, the raw continuation and the published action begin with a try, and no catch contains a throw.
        if (!Regex.IsMatch(code, @"data\s*=>\s*\{\s*try\b")) yield return "the callback does not begin with a try";
        if (!Regex.IsMatch(code, @"OnCompleted\s*\(\s*\(\s*\)\s*=>\s*\{\s*try\b")) yield return "the raw continuation does not begin with a try";
        if (!Regex.IsMatch(code, @"_\s*=>\s*\{\s*try\b")) yield return "the published action does not begin with a try";
        foreach (Match match in Regex.Matches(code, @"catch\s*\([^)]*\)\s*\{[^{}]*\}"))
            if (Regex.IsMatch(match.Value, @"\bthrow\b")) yield return "a catch throws";
    }

    [Fact]
    public void TheAdapterHasItsExactCountsAndNoFieldButTheDocumentOfTheTargetAndIsTotalByText()
    {
        Assert.Empty(AdapterViolations(CodeOf(AdapterPath)));
        Assert.Matches(@"internal\s+sealed\s+class\s+RackPilotUpdateCommandContext\s*:\s*IPushBackPilotUpdateHost\b", CodeOf(AdapterPath));
        Assert.Matches(@"private\s+sealed\s+class\s+Primitives\s*:\s*IPushBackPilotHostPrimitives\b", CodeOf(AdapterPath));
        Assert.Matches(@"private\s+sealed\s+class\s+Target\s*:\s*IPushBackPilotHostTarget\b", CodeOf(AdapterPath));
    }

    [Theory]
    [InlineData("a second OnCompleted", "var x = result.OnCompleted(() => { });")]
    [InlineData("a second Post", "context.Post(null, null);")]
    [InlineData("a second call of the context", "AcApplication.DocumentManager.ExecuteInCommandContextAsync(null, null);")]
    [InlineData("a second lock", "var l = document.LockDocument();")]
    [InlineData("a second writer", "var w = new RackPilotUpdateWriter(document);")]
    [InlineData("a second capture of the scheduler", "var c = SynchronizationContext.Current;")]
    [InlineData("a second read of IsCompleted", "var c = result.IsCompleted;")]
    [InlineData("a GetResult outside a try", "result.GetResult();")]
    [InlineData("a Task.Factory.StartNew", "Task.Factory.StartNew(() => { });")]
    [InlineData("a ContinueWith", "task.ContinueWith(t => { });")]
    [InlineData("a field", "private int _count;")]
    [InlineData("a static field", "private static int _shared;")]
    [InlineData("a Wait", "task.Wait();")]
    [InlineData("a Result", "var r = task.Result;")]
    [InlineData("a transaction", "var t = database.TransactionManager.StartTransaction();")]
    [InlineData("a Commit", "transaction.Commit();")]
    [InlineData("a write API", "SystemBlockWriter.ApplyRegen(database);")]
    [InlineData("a thread", "var t = new Thread(() => { });")]
    [InlineData("the workspace", "using RackCad.Application.Workspace;")]
    [InlineData("the update of T4B", "draft.CompleteApply(outcome);")]
    public void TheAdapterGuardTripsOnEachViolationInjectedIntoItsRealText(string name, string injected)
    {
        var real = CodeOf(AdapterPath);

        Assert.Empty(AdapterViolations(real));
        var head = Regex.Match(real, @"internal\s+sealed\s+class\s+RackPilotUpdateCommandContext\s*:\s*IPushBackPilotUpdateHost\s*\{");
        Assert.True(head.Success);
        var mutated = real.Insert(head.Index + head.Length, " " + injected + " ");
        Assert.True(AdapterViolations(mutated).Any(), name);
    }

    [Fact]
    public void TheAdapterGuardTripsWhenTheContinuationIsNotOnTheBranchOfAnIsCompletedThatIsFalseOrThePostIsNotOfTheCapturedContext()
    {
        var real = CodeOf(AdapterPath);

        Assert.Empty(AdapterViolations(real));
        // M-07: the condition turned around, the branch that no longer returns, a Post of another scheduler and a hop with another context.
        Assert.NotEmpty(AdapterViolations(real.Replace("if (result.IsCompleted)", "if (!result.IsCompleted)", StringComparison.Ordinal)));
        Assert.NotEmpty(AdapterViolations(Regex.Replace(real, @"(if\s*\(\s*result\.IsCompleted\s*\)\s*\{[^{}]*?)\breturn\s*;", "$1")));
        Assert.NotEmpty(AdapterViolations(real.Replace("context.Post(", "other.Post(", StringComparison.Ordinal)));
        Assert.NotEmpty(AdapterViolations(real.Replace("Hop(context, completed);", "Hop(other, completed);", StringComparison.Ordinal)));
        // The continuation before the branch: the first OnCompleted moved over it.
        var branch = Regex.Match(real, @"if\s*\(\s*result\.IsCompleted\s*\)\s*\{[^{}]*\breturn\s*;\s*\}");
        Assert.True(branch.Success);
        Assert.NotEmpty(AdapterViolations(real.Insert(branch.Index, "result.OnCompleted(() => { }); ").Replace("result.OnCompleted(() =>\r\n", "result.Other(() =>\r\n", StringComparison.Ordinal).Replace("result.OnCompleted(() =>\n", "result.Other(() =>\n", StringComparison.Ordinal)));
    }

    [Fact]
    public void TheAdapterGuardTripsWhenTheTargetOfTheRunCarriesATextOfItsOwnOrDoesNotAnswerTheReasonOfTheRun()
    {
        var real = CodeOf(AdapterPath);
        var open = Src.BodyOf(real, @"public\s+bool\s+TryOpenTarget\s*\(");

        Assert.Empty(AdapterViolations(real));
        // M-04: the two literals that the adapter had.
        Assert.NotEmpty(AdapterViolations(real.Replace(open, open.TrimEnd('}') + " reason = \"El documento ya no está abierto.\"; }", StringComparison.Ordinal)));
        Assert.NotEmpty(AdapterViolations(real.Replace(open, open.Replace("PushBackPilotUpdateRun.DocumentMismatchReason", "string.Empty", StringComparison.Ordinal), StringComparison.Ordinal)));
    }

    [Fact]
    public void TheAdapterGuardTripsOnAContinuationWithoutATryAndOnAThrowInACatch()
    {
        var real = CodeOf(AdapterPath);

        Assert.NotEmpty(AdapterViolations(Regex.Replace(real, @"data\s*=>\s*\{\s*try\b", "data => { /*x*/ if (true) { } try", RegexOptions.None)));
        Assert.NotEmpty(AdapterViolations(Regex.Replace(real, @"OnCompleted\s*\(\s*\(\s*\)\s*=>\s*\{\s*try\b", "OnCompleted(() => { Hop(context, completed); try")));
        Assert.NotEmpty(AdapterViolations(real.Replace("RackLog.Exception(\"RackPilotUpdateCommandContext.Completed\", ex);", "throw;", StringComparison.Ordinal)));
    }

    [Fact]
    public void TheCommandContextAndTheDocumentLockAreNamedInOneFileOfTheProductEachWithItsOwnCount()
    {
        var product = Product();

        Assert.Equal(new[] { AdapterPath }, product.Where(f => Regex.IsMatch(f.Code, @"ExecuteInCommandContextAsync")).Select(f => f.Path).ToArray());
        Assert.Equal(new[] { AdapterPath }, product.Where(f => Regex.IsMatch(f.Code, @"PushBackPilotHostRunner\.Request\s*\(")).Select(f => f.Path).ToArray());
        Assert.Equal(new[] { WriterPath, AdapterPath }.OrderBy(p => p, StringComparer.Ordinal).ToArray(),
            product.Where(f => Regex.IsMatch(f.Code, @"\bnew\s+RackPilotUpdateWriter\s*\(|class\s+RackPilotUpdateWriter\b")).Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray());
        Assert.Equal(1, Src.Count(CodeOf(AdapterPath), @"\bnew\s+RackPilotUpdateWriter\s*\("));
        Assert.Equal(0, Src.Count(CodeOf(WriterPath), @"\bnew\s+RackPilotUpdateWriter\s*\("));
    }

    // ================================================================== G-1: the census of the primitives of Push Back


    // The three facades of Push Back: the only files of src that may call their own primitives, besides the writer (I-01).
    private static readonly string[] PushBackDrawServicePaths =
    {
        "src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs",
        "src/RackCad.Plugin/Systems/PushBack/PushBackPlantaDrawService.cs",
        "src/RackCad.Plugin/Systems/PushBack/PushBackSystemDrawService.cs",
    };

    private static IEnumerable<string> G1Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        // I-01: a census of the WHOLE of src. A file that names one of the three facades and calls .PrepareRedraw( or .RedrawInTransaction( is one
        // of the three facades themselves or the writer: no other file of any folder, a new file of the Plugin included.
        var allowed = PushBackDrawServicePaths.Append(WriterPath).ToHashSet(StringComparer.Ordinal);
        foreach (var (path, code) in product)
        {
            var named = Regex.IsMatch(code, @"PushBack(?:System|Frontal|Planta)DrawService");
            var calls = Regex.IsMatch(code, @"\.(?:PrepareRedraw|RedrawInTransaction)\s*\(");
            if (named && calls && !allowed.Contains(path)) yield return path + " calls a primitive of Push Back";
        }

        foreach (var path in PushBackDrawServicePaths)
            if (product.All(f => f.Path != path)) yield return path + " is missing from the census";

        var writer = product.Single(f => f.Path == WriterPath).Code;
        var update = Src.BodyOf(writer, @"internal\s+sealed\s+class\s+RackPilotUpdateScope\b");
        var prepare = Src.BodyOf(update, @"public\s+PushBackPilotImportResult\s+PrepareUnits\s*\(");
        if (Src.Count(writer, @"\.PrepareRedraw\s*\(") != Src.Count(prepare, @"\.PrepareRedraw\s*\(")) yield return "PrepareRedraw is outside PrepareUnits";
        var mutation = Src.BodyOf(writer, @"internal\s+sealed\s+class\s+RackPilotMutationScope\b");
        var write = Src.BodyOf(mutation, @"public\s+PushBackPilotUnitResult\s+WriteUnit\s*\(");
        if (Src.Count(writer, @"\.RedrawInTransaction\s*\(") != Src.Count(write, @"\.RedrawInTransaction\s*\(")) yield return "RedrawInTransaction is outside WriteUnit";
    }

    [Fact]
    public void G1TheTwoPrimitivesOfPushBackAreCalledOnlyByTheWriterInPrepareUnitsAndWriteUnit()
    {
        Assert.Empty(G1Violations(Product()));
    }

    [Fact]
    public void G1TripsWhenThePrimitivesAreInjectedIntoTheAdapterOrTheController()
    {
        var injected = "var p = new PushBackPlantaDrawService().PrepareRedraw(db, id, system, text); var r = new PushBackSystemDrawService().RedrawInTransaction(a);";
        Assert.NotEmpty(G1Violations(Append(Product(), AdapterPath, injected)));
        Assert.NotEmpty(G1Violations(Append(Product(), ControllerPath, injected)));
        Assert.NotEmpty(G1Violations(Append(Product(), HostRunPath, injected)));
        Assert.NotEmpty(G1Violations(With(Product(), WriterPath, code => code + "\nvar q = new PushBackFrontalDrawService().PrepareRedraw(db, id, system, text);")));
    }

    [Fact]
    public void G1TripsOnANewFileOfThePluginOutsideTheIntegrationFoldersAndOnAnExistingOneThatCallsAPrimitive()
    {
        var call = "class Other { void M() { new PushBackPlantaDrawService().PrepareRedraw(db, id, system, text); new PushBackSystemDrawService().RedrawInTransaction(a, b, c, out s); } }";
        var product = Product();

        Assert.Empty(G1Violations(product));
        // I-01: a new file of the Plugin, in any folder, trips the census.
        Assert.NotEmpty(G1Violations(product.Append(("src/RackCad.Plugin/Views/RackPilotAnotherWriter.cs", call)).ToList()));
        Assert.NotEmpty(G1Violations(product.Append(("src/RackCad.Plugin/Drawing/NewDrawing.cs", call)).ToList()));
        Assert.NotEmpty(G1Violations(product.Append(("src/RackCad.UI/Editor/NewThing.cs", call)).ToList()));
        // Existing files of the Plugin that name the facades and are outside the integration folders.
        Assert.NotEmpty(G1Violations(Append(product, "src/RackCad.Plugin/RackPushBackCommands.cs", "var p = new PushBackFrontalDrawService().PrepareRedraw(db, id, system, text);")));
        Assert.NotEmpty(G1Violations(Append(product, "src/RackCad.Plugin/Systems/Selective/SelectivePlantaDrawService.cs", "var p = new PushBackSystemDrawService().RedrawInTransaction(a, b, c, out s);")));
        // The facades themselves and the writer are the exceptions, by exact file.
        Assert.Empty(G1Violations(Append(product, PushBackDrawServicePaths[0], "var p = new PushBackFrontalDrawService().PrepareRedraw(db, id, system, text);")));
        // A missing facade is a trip too: the census names three exact files.
        Assert.NotEmpty(G1Violations(product.Where(f => f.Path != PushBackDrawServicePaths[1]).ToList()));
    }

    // ================================================================== G-2: one implementer of the port of the update

    private static IEnumerable<string> G2Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        var implementers = product
            .SelectMany(f => Regex.Matches(f.Code, @"\bclass\s+(\w+)\s*:\s*[^{]*\bIPushBackPilotUpdatePort\b").Cast<Match>().Select(m => f.Path + ":" + m.Groups[1].Value))
            .ToList();
        if (!implementers.SequenceEqual(new[] { WriterPath + ":RackPilotUpdateWriter" })) yield return "the implementers are " + string.Join(", ", implementers);
    }

    [Fact]
    public void G2OnlyTheWriterImplementsThePortOfTheUpdateAndAnotherClassTripsTheGuard()
    {
        Assert.Empty(G2Violations(Product()));
        Assert.NotEmpty(G2Violations(Append(Product(), AdapterPath, "internal sealed class OtherWriter : IPushBackPilotUpdatePort { }")));
        Assert.NotEmpty(G2Violations(Append(Product(), ControllerPath, "private sealed class Fake : Object, IPushBackPilotUpdatePort { }")));
    }

    // ================================================================== G-3: no long-lived type keeps what belongs to a run

    private static readonly string[] RunTypes = { "IPushBackPilotUpdatePort", "RackPilotUpdateWriter", "PushBackPilotHostRunner", "IPushBackPilotHostRun", "IPushBackPilotHostTarget" };

    private static readonly Regex RunFieldInText = new(
        @"\b(?:private|internal|public|protected|static|readonly)\b[^;(){}=]*\b(?:IPushBackPilotUpdatePort|RackPilotUpdateWriter|PushBackPilotHostRunner|IPushBackPilotHostRun|IPushBackPilotHostTarget)\b[^;(){}=]*[;=]");

    private static IEnumerable<string> G3TextViolations(IReadOnlyList<(string Path, string Code)> product)
    {
        // The one construction of the runner: in its static Request. The only caller of Request: TryRequest of the adapter.
        var runner = product.Single(f => f.Path == HostRunPath).Code;
        if (Src.Count(runner, @"\bnew\s+PushBackPilotHostRunner\s*\(") != 1) yield return "the runner is not built exactly once";
        var request = Src.BodyOf(runner, @"public\s+static\s+bool\s+Request\s*\(");
        if (Src.Count(request, @"\bnew\s+PushBackPilotHostRunner\s*\(") != 1) yield return "the runner is not built in Request";
        if (Regex.IsMatch(runner, @"\bstatic\b[^;(){}]*PushBackPilotHostRunner[^;(){}]*[;=]")) yield return "a static field of the runner";

        foreach (var (path, code) in product.Where(f => f.Path != HostRunPath))
            if (Regex.IsMatch(code, @"\bnew\s+PushBackPilotHostRunner\s*\(")) yield return path + " builds the runner";

        foreach (var (path, code) in product.Where(f => f.Path != AdapterPath))
            if (Regex.IsMatch(code, @"PushBackPilotHostRunner\.Request\s*\(")) yield return path + " asks the runner for a request";

        var adapter = product.Single(f => f.Path == AdapterPath).Code;
        var tryRequest = Src.BodyOf(adapter, @"public\s+bool\s+TryRequest\s*\(");
        if (Src.Count(tryRequest, @"PushBackPilotHostRunner\.Request\s*\(") != 1 || Src.Count(adapter, @"PushBackPilotHostRunner\.Request\s*\(") != 1) yield return "Request is not called exactly once, from TryRequest";

        // The writer: built once, by CreateWriter of the target.
        var createWriter = Src.BodyOf(adapter, @"public\s+IPushBackPilotUpdatePort\s+CreateWriter\s*\(");
        if (Src.Count(createWriter, @"\bnew\s+RackPilotUpdateWriter\s*\(") != 1 || Src.Count(adapter, @"\bnew\s+RackPilotUpdateWriter\s*\(") != 1) yield return "the writer is not built exactly once, in CreateWriter";
        foreach (var (path, code) in product.Where(f => f.Path != AdapterPath))
            if (Regex.IsMatch(code, @"\bnew\s+RackPilotUpdateWriter\s*\(")) yield return path + " builds the writer";

        // The long-lived types that the tests cannot reflect: the controller and the executor of the adapter, by text.
        foreach (var path in new[] { ControllerPath, AdapterPath })
        {
            var code = product.Single(f => f.Path == path).Code;
            foreach (Match match in RunFieldInText.Matches(code)) yield return path + " keeps " + match.Value.Trim();
        }
    }

    private static IEnumerable<string> G3FieldTypeViolations(IEnumerable<(string Name, Type Type)> fields)
    {
        static IEnumerable<Type> Flatten(Type type)
        {
            yield return type;
            if (type.IsGenericType)
                foreach (var argument in type.GetGenericArguments().SelectMany(Flatten)) yield return argument;
            if (type.HasElementType)
                foreach (var inner in Flatten(type.GetElementType()!)) yield return inner;
        }

        foreach (var (name, type) in fields)
            foreach (var part in Flatten(type))
                if (RunTypes.Contains(part.Name)) yield return name + " is of the type " + part.Name;
    }

    private static IReadOnlyList<(string Name, Type Type)> CoreFields()
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly;
        var types = new List<Type> { typeof(WorkspaceHostCore) };
        types.AddRange(typeof(WorkspaceHostCore).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Where(t => !t.Name.StartsWith("<", StringComparison.Ordinal)));
        return types.SelectMany(t => t.GetFields(flags).Where(f => !f.Name.StartsWith("<", StringComparison.Ordinal)).Select(f => (t.Name + "." + f.Name, f.FieldType))).ToList();
    }

    [Fact]
    public void G3NoLongLivedTypeKeepsAWriterARunnerARunOrATargetAndTheWriterAndTheRunnerAreBuiltOnce()
    {
        Assert.Empty(G3TextViolations(Product()));
        var fields = CoreFields();
        Assert.NotEmpty(fields);
        Assert.Empty(G3FieldTypeViolations(fields));
        // The nested run of the core exists, with fields of its own, and none of them is of those types.
        Assert.Contains(fields, f => f.Name.StartsWith("PilotUpdateRun.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("the controller", ControllerPath, "private IPushBackPilotUpdatePort _port;")]
    [InlineData("the controller", ControllerPath, "private RackPilotUpdateWriter _writer;")]
    [InlineData("the controller", ControllerPath, "private static PushBackPilotHostRunner _runner;")]
    [InlineData("the controller", ControllerPath, "private readonly IPushBackPilotHostTarget _target = null;")]
    [InlineData("the adapter", AdapterPath, "private IPushBackPilotHostRun _run;")]
    [InlineData("the adapter", AdapterPath, "private RackPilotUpdateWriter _writer;")]
    [InlineData("the runner", HostRunPath, "private static PushBackPilotHostRunner _last;")]
    [InlineData("the runner", HostRunPath, "static void Build() { var r = new PushBackPilotHostRunner(1, null!, null!, null); }")]
    [InlineData("the controller", ControllerPath, "var w = new PushBackPilotHostRunner(1, null, null, null);")]
    [InlineData("the core", CorePath, "var w = new RackPilotUpdateWriter(document);")]
    [InlineData("the controller", ControllerPath, "var w = new RackPilotUpdateWriter(document);")]
    [InlineData("the adapter", AdapterPath, "var w = new RackPilotUpdateWriter(document);")]
    [InlineData("the controller", ControllerPath, "PushBackPilotHostRunner.Request(1, null, null, null, out var f);")]
    public void G3TripsOnAFieldOfEachTypeOnASecondConstructionAndOnARequestFromAnotherFile(string who, string path, string injected)
    {
        Assert.Empty(G3TextViolations(Product()));
        var head = Regex.Match(CodeOf(path), @"\b(?:class)\s+\w+[^{]*\{");
        Assert.True(head.Success, who);
        var mutated = With(Product(), path, code => code.Insert(Regex.Match(code, @"\bclass\s+\w+[^{]*\{").Index + Regex.Match(code, @"\bclass\s+\w+[^{]*\{").Length, " " + injected + " "));

        Assert.NotEmpty(G3TextViolations(mutated));
    }

    [Fact]
    public void G3TheFieldRuleOfTheCoreTripsOnAFieldOfEachTypeAlsoInsideAGenericAndInANestedType()
    {
        var real = CoreFields();

        Assert.Empty(G3FieldTypeViolations(real));
        Assert.NotEmpty(G3FieldTypeViolations(real.Append(("_port", typeof(IPushBackPilotUpdatePort)))));
        Assert.NotEmpty(G3FieldTypeViolations(real.Append(("_run", typeof(IPushBackPilotHostRun)))));
        Assert.NotEmpty(G3FieldTypeViolations(real.Append(("_target", typeof(IPushBackPilotHostTarget)))));
        Assert.NotEmpty(G3FieldTypeViolations(real.Append(("_runner", typeof(PushBackPilotHostRunner)))));
        Assert.NotEmpty(G3FieldTypeViolations(real.Append(("_runs", typeof(Dictionary<int, IPushBackPilotHostRun>)))));
        Assert.NotEmpty(G3FieldTypeViolations(real.Append(("PilotUpdateRun._port", typeof(IPushBackPilotUpdatePort[])))));
    }

    // ================================================================== G-4: one caller of the update of T4B

    // The code without the text of its strings: the context that the core gives to its log is not an identifier.
    private static string WithoutStrings(string code) => Regex.Replace(code, "\"(?:[^\"\\\\\\r\\n]|\\\\.)*\"", "\"\"");

    private static IEnumerable<string> G4Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        // M-07: the IDENTIFIER, as a whole word and outside the text of a string, so that a group of methods (a delegate that is the method itself)
        // is seen too: it appears in the core twice (its declaration and its one call) and nowhere else.
        var callers = product.Where(f => Regex.IsMatch(WithoutStrings(f.Code), @"\bTryUpdatePilot\b")).Select(f => f.Path).ToArray();
        if (!callers.SequenceEqual(new[] { CorePath })) yield return "the callers are in " + string.Join(", ", callers);

        var core = product.Single(f => f.Path == CorePath).Code;
        var run = Src.BodyOf(core, @"private\s+sealed\s+class\s+PilotUpdateRun\b");
        var execute = Src.BodyOf(run, @"public\s+void\s+Execute\s*\(");
        if (Src.Count(core, @"\.TryUpdatePilot\s*\(") != 1 || Src.Count(execute, @"\.TryUpdatePilot\s*\(") != 1) yield return "TryUpdatePilot is not called exactly once, from Execute of the run of the core";
        if (Src.Count(WithoutStrings(core), @"\bTryUpdatePilot\b") != 2) yield return "the identifier TryUpdatePilot appears other than as its declaration and its one call";

        var runCallers = product.Where(f => Regex.IsMatch(f.Code, @"PushBackPilotUpdateRun\.Execute\s*\(")).Select(f => f.Path).ToArray();
        if (!runCallers.SequenceEqual(new[] { CorePath })) yield return "PushBackPilotUpdateRun.Execute is named in " + string.Join(", ", runCallers);
    }

    [Fact]
    public void G4OnlyTheExecuteOfTheRunOfTheCoreCallsTheUpdateAndOnlyTheCoreCallsTheRunOfT4B()
    {
        Assert.Empty(G4Violations(Product()));
    }

    [Fact]
    public void G4TripsOnACallerInTheControllerTheAdapterOrASecondCallInTheCore()
    {
        Assert.NotEmpty(G4Violations(Append(Product(), ControllerPath, "var u = _host.TryUpdatePilot(request, port);")));
        Assert.NotEmpty(G4Violations(Append(Product(), AdapterPath, "var u = core.TryUpdatePilot(request, port);")));
        Assert.NotEmpty(G4Violations(Append(Product(), HostRunPath, "var o = PushBackPilotUpdateRun.Execute(draft, request, port);")));
        Assert.NotEmpty(G4Violations(With(Product(), CorePath, code => code.Replace("public EditUpdateOutcome TryUpdatePilot(", "public EditUpdateOutcome Other() { return TryUpdatePilot(null!, null!); }\n        public EditUpdateOutcome TryUpdatePilot(", StringComparison.Ordinal))));
    }

    [Fact]
    public void G4TripsOnAGroupOfMethodsOfTheUpdateOutsideTheCoreAndInsideItAndNotOnTheNameInsideAString()
    {
        var product = Product();

        Assert.Empty(G4Violations(product));
        // M-07: no parenthesis at all, so the old expression of the guard did not see these.
        Assert.NotEmpty(G4Violations(Append(product, ControllerPath, "Func<EditIntentRequest, IPushBackPilotUpdatePort, EditUpdateOutcome> f = core.TryUpdatePilot;")));
        Assert.NotEmpty(G4Violations(Append(product, AdapterPath, "var f = (Delegate)host.TryUpdatePilot;")));
        Assert.NotEmpty(G4Violations(Append(product, CorePath, "Func<EditIntentRequest, IPushBackPilotUpdatePort, EditUpdateOutcome> f = TryUpdatePilot;")));
        // The name of the method that the log of the core carries in a string is not an identifier: the guard does not count it.
        Assert.Contains("\"WorkspaceHostCore.TryUpdatePilot\"", CodeOf(CorePath));
        Assert.Empty(G4Violations(Append(product, CorePath, "var context = \"WorkspaceHostCore.TryUpdatePilot\";")));
    }

    // ================================================================== G-5: no authored write API in the adapter and the runner

    private static IEnumerable<string> G5Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        foreach (var path in new[] { AdapterPath, HostRunPath })
        {
            var code = product.Single(f => f.Path == path).Code;
            var scrubbed = path == AdapterPath ? Regex.Replace(code, @"\bnew\s+RackPilotUpdateWriter\b", string.Empty) : code;
            foreach (Match match in WriteApi.Matches(scrubbed)) yield return path + " names " + match.Value;
        }
    }

    [Fact]
    public void G5TheAdapterAndTheRunnerNameNoAuthoredWriteApi()
    {
        Assert.Empty(G5Violations(Product()));
        Assert.Equal(28, WriteApiNames.Length);
    }

    [Theory]
    [InlineData("RedrawInTransaction")]
    [InlineData("RedrawInPlace")]
    [InlineData("DrawAndPlace")]
    [InlineData("SystemBlockWriter")]
    [InlineData("RackBlockData.Write")]
    [InlineData("ForWrite")]
    [InlineData("UpgradeOpen")]
    [InlineData("AppendEntity")]
    [InlineData("x.Commit()")]
    [InlineData("StartTransaction")]
    [InlineData("StartOpenCloseTransaction")]
    [InlineData("SyncName")]
    [InlineData("PurgeUnreferenced")]
    [InlineData("EnsureForPlan")]
    [InlineData("WblockCloneObjects")]
    [InlineData("a.SetAt(1)")]
    [InlineData("x.Erase()")]
    [InlineData("SendStringToExecute")]
    [InlineData("CommandMethod")]
    [InlineData("RackPushBackCommands")]
    [InlineData("EditPushBack")]
    [InlineData("PushBackKindHandler")]
    [InlineData("RackMenuCommands")]
    [InlineData("RackViewBatchDriver")]
    [InlineData("SiblingRedrawPlan")]
    [InlineData("AutoCadSiblingRedrawPort")]
    [InlineData("RackSelectivoInsertIntegration")]
    [InlineData("RackCommandSupport")]
    public void G5TripsOnEachNameInjectedIntoTheAdapterAndIntoTheRunner(string injected)
    {
        Assert.NotEmpty(G5Violations(Append(Product(), AdapterPath, "var x = " + injected + ";")));
        Assert.NotEmpty(G5Violations(Append(Product(), HostRunPath, "var x = " + injected + ";")));
    }

    // ================================================================== G-6: the two folders of the workspace

    // The exceptions are the ones of today, by exact file and with their counts (measured at the emission).
    private static readonly Dictionary<string, Dictionary<string, int>> WorkspaceExceptions = new(StringComparer.Ordinal)
    {
        ["src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs"] = new() { ["StartTransaction"] = 2, ["LockDocument"] = 2 },
        ["src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs"] = new() { ["StartTransaction"] = 1, ["LockDocument"] = 1 },
        ["src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs"] = new() { ["LockDocument"] = 2 },
        ["src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs"] = new() { ["StartTransaction"] = 1, ["LockDocument"] = 1 },
        ["src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs"] = new() { ["CommandMethod"] = 2 },
    };

    private static readonly (string Name, Regex Pattern)[] WriteTerms =
    {
        ("RedrawInTransaction", new Regex(@"RedrawInTransaction")), ("RedrawInPlace", new Regex(@"RedrawInPlace")), ("DrawAndPlace", new Regex(@"DrawAndPlace")),
        ("SystemBlockWriter", new Regex(@"SystemBlockWriter")), ("RackBlockData.Write", new Regex(@"RackBlockData\.Write")), ("ForWrite", new Regex(@"\bForWrite\b")),
        ("UpgradeOpen", new Regex(@"\bUpgradeOpen\b")), ("AppendEntity", new Regex(@"\bAppendEntity\b")), ("Commit", new Regex(@"\.Commit\s*\(")),
        ("StartTransaction", new Regex(@"StartTransaction")), ("StartOpenCloseTransaction", new Regex(@"StartOpenCloseTransaction")), ("SyncName", new Regex(@"\bSyncName\b")),
        ("PurgeUnreferenced", new Regex(@"PurgeUnreferenced")), ("EnsureForPlan", new Regex(@"EnsureForPlan")), ("WblockCloneObjects", new Regex(@"WblockCloneObjects")),
        ("SetAt", new Regex(@"\bSetAt\s*\(")), ("Erase", new Regex(@"\.Erase\s*\(")), ("SendStringToExecute", new Regex(@"SendStringToExecute")),
        ("CommandMethod", new Regex(@"CommandMethod")), ("LockDocument", new Regex(@"LockDocument")),
    };

    private static Dictionary<string, int> TermCounts(string code) =>
        WriteTerms.Select(t => (t.Name, Count: t.Pattern.Matches(code).Count)).Where(t => t.Count > 0).ToDictionary(t => t.Name, t => t.Count);

    private static IEnumerable<string> G6Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        foreach (var (path, code) in product.Where(f => f.Path.StartsWith("src/RackCad.Plugin/Workspace/", StringComparison.Ordinal) || f.Path.StartsWith("src/RackCad.Application/Workspace/", StringComparison.Ordinal)))
        {
            var counts = TermCounts(code);
            var expected = WorkspaceExceptions.TryGetValue(path, out var exception) ? exception : new Dictionary<string, int>();
            if (!counts.OrderBy(c => c.Key, StringComparer.Ordinal).SequenceEqual(expected.OrderBy(c => c.Key, StringComparer.Ordinal)))
                yield return path + ": " + string.Join(", ", counts.Select(c => c.Key + "=" + c.Value)) + " (expected " + string.Join(", ", expected.Select(c => c.Key + "=" + c.Value)) + ")";
        }
    }

    [Fact]
    public void G6TheTwoWorkspaceFoldersHaveNoWriteApiOutsideTheFiveExceptionsWithTheirExactCounts()
    {
        Assert.Empty(G6Violations(Product()));
        Assert.Equal(5, WorkspaceExceptions.Count);
    }

    [Theory]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs", "var t = db.StartTransaction();")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs", "using (d.LockDocument()) { }")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs", "var t = db.StartTransaction();")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs", "using (d.LockDocument()) { }")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs", "var t = db.StartTransaction();")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs", "[CommandMethod(\"X\")]")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceController.cs", "[CommandMethod(\"X\")]")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspaceController.cs", "using (d.LockDocument()) { }")]
    [InlineData("src/RackCad.Application/Workspace/WorkspaceHostCore.cs", "var t = db.StartTransaction();")]
    [InlineData("src/RackCad.Application/Workspace/EventBridgeCore.cs", "x.Erase();")]
    public void G6TripsOnOneMoreThanTheCountOfEachExceptionAndOnAnyNameInTheOtherFiles(string path, string injected)
    {
        Assert.Empty(G6Violations(Product()));
        Assert.NotEmpty(G6Violations(Append(Product(), path, injected)));
    }

    // ================================================================== G-7: the calls of the port of the update, only in the run of T4B

    private static IEnumerable<string> G7Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        var callers = product
            .Where(f => f.Path.StartsWith("src/RackCad.Application/", StringComparison.Ordinal))
            .Where(f => Regex.IsMatch(f.Code, @"\.(?:PrepareUnits|BeginMutation|WriteUnit|Seal)\s*\(") || (f.Path.StartsWith("src/RackCad.Application/Systems/PushBack/Pilot/", StringComparison.Ordinal) && Regex.IsMatch(f.Code, @"\.Post\s*\(")))
            .Select(f => f.Path).ToArray();
        if (!callers.SequenceEqual(new[] { RunPath })) yield return "the calls of the port are in " + string.Join(", ", callers);
    }

    [Fact]
    public void G7TheWriteCallsOfThePortAreOnlyInTheRunOfT4B()
    {
        Assert.Empty(G7Violations(Product()));
    }

    [Theory]
    [InlineData(CorePath, "scope.PrepareUnits(plan);")]
    [InlineData(CorePath, "mutation.WriteUnit(key, text);")]
    [InlineData(HostRunPath, "mutation.Seal();")]
    [InlineData(HostRunPath, "scope.BeginMutation();")]
    [InlineData(GatePath, "scope.PrepareUnits(plan);")]
    public void G7TripsOnACallOfThePortOutsideTheRun(string path, string injected)
    {
        Assert.NotEmpty(G7Violations(Append(Product(), path, injected)));
    }

    // ================================================================== G-9: the pilot and the write API, in the whole Plugin and Application

    private static readonly Regex NamesThePilot = new(@"PushBackPilot\w*|RackPilot\w*|RackCad\.Application\.Systems\.PushBack\.Pilot");

    private static readonly Dictionary<string, Dictionary<string, int>> PilotExceptions = new(StringComparer.Ordinal)
    {
        [WriterPath] = new() { ["RedrawInTransaction"] = 3, ["SystemBlockWriter"] = 2, ["Commit"] = 1, ["StartTransaction"] = 1, ["StartOpenCloseTransaction"] = 1, ["SyncName"] = 1 },
        [IntentAdapterPath] = new() { ["StartTransaction"] = 1, ["LockDocument"] = 1 },
        [NavigationHostPath] = new() { ["LockDocument"] = 2 },
        [AdapterPath] = new() { ["LockDocument"] = 1 },
    };

    private static IEnumerable<string> G9Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        foreach (var (path, code) in product.Where(f => f.Path.StartsWith("src/RackCad.Plugin/", StringComparison.Ordinal) || f.Path.StartsWith("src/RackCad.Application/", StringComparison.Ordinal)))
        {
            if (!NamesThePilot.IsMatch(code)) continue;
            var counts = TermCounts(code);
            if (counts.Count == 0 && !PilotExceptions.ContainsKey(path)) continue;
            var expected = PilotExceptions.TryGetValue(path, out var exception) ? exception : new Dictionary<string, int>();
            if (!counts.OrderBy(c => c.Key, StringComparer.Ordinal).SequenceEqual(expected.OrderBy(c => c.Key, StringComparer.Ordinal)))
                yield return path + ": " + string.Join(", ", counts.Select(c => c.Key + "=" + c.Value)) + " (expected " + string.Join(", ", expected.Select(c => c.Key + "=" + c.Value)) + ")";
        }
    }

    [Fact]
    public void G9NoFileOfThePluginOrOfApplicationThatNamesThePilotUsesAWriteApiOrALockOutsideTheFourExceptionsWithTheirExactCounts()
    {
        Assert.Empty(G9Violations(Product()));
        Assert.Equal(4, PilotExceptions.Count);
    }

    [Fact]
    public void G9TripsOnAPilotFileThatRedrawsInPlaceThatWritesBlockDataThatOpensForWriteOrThatLocksASecondTime()
    {
        var product = Product();
        Assert.NotEmpty(G9Violations(product.Append(("src/RackCad.Plugin/Views/RackPilotOtherThing.cs", "class RackPilotOther { void M() { x.RedrawInPlace(a); } }")).ToList()));
        Assert.NotEmpty(G9Violations(product.Append(("src/RackCad.Plugin/Views/RackPilotNewWriter.cs", "class RackPilotNewWriter { void M() { RackBlockData.Write(a, b); } }")).ToList()));
        Assert.NotEmpty(G9Violations(Append(product, IntentAdapterPath, "var o = tr.GetObject(id, OpenMode.ForWrite); object _ = RackPilotX;")));
        Assert.NotEmpty(G9Violations(Append(product, AdapterPath, "var second = document.LockDocument();")));
        Assert.NotEmpty(G9Violations(Append(product, WriterPath, "var again = database.TransactionManager.StartTransaction();")));
        Assert.NotEmpty(G9Violations(Append(product, "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs", "var t = db.StartTransaction();")));
    }

    // ================================================================== G-10: the callers of the flight

    // I-01: four windows of the editors of the systems have a RequestUpdate of their own, of their editor session: another member that is not the port
    // of the panel. They are exceptions by EXACT file with a count of one (measured), and no folder is left out of the census.
    private static readonly string[] OwnRequestUpdateWindows =
    {
        "src/RackCad.UI/Systems/Cantilever/RackCantileverWindow.xaml.cs",
        "src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs",
        "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs",
        "src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs",
    };

    private const string FieldViewModelPath = "src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs";

    private static IEnumerable<string> G10Violations(IReadOnlyList<(string Path, string Code)> product)
    {
        var requestCallers = product
            .Where(f => Regex.IsMatch(f.Code, @"(?<!IWorkspacePilotEditPort)\.RequestUpdate\s*\("))
            .Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray();
        // The gesture of the field (F4-T4C2) may call it from its view model; until then the two forwards and the four windows do.
        var allowed = new[] { ControllerPath, GatePath }.Concat(OwnRequestUpdateWindows).OrderBy(p => p, StringComparer.Ordinal).ToArray();
        var withoutGesture = requestCallers.Where(p => p != FieldViewModelPath).ToArray();
        if (!withoutGesture.SequenceEqual(allowed)) yield return ".RequestUpdate( is called from " + string.Join(", ", requestCallers);
        foreach (var path in allowed)
        {
            var files = product.Where(f => f.Path == path).ToList();
            if (files.Count == 0) { yield return path + " is missing"; continue; }
            if (Src.Count(files[0].Code, @"(?<!IWorkspacePilotEditPort)\.RequestUpdate\s*\(") != 1)
                yield return path + (OwnRequestUpdateWindows.Contains(path) ? " does not have its one RequestUpdate of its own" : " does not forward the request exactly once");
        }

        var tryRequest = product.Where(f => Regex.IsMatch(f.Code, @"\.TryRequest\s*\(")).Select(f => f.Path).ToArray();
        if (!tryRequest.SequenceEqual(new[] { CorePath })) yield return ".TryRequest( is called from " + string.Join(", ", tryRequest);
        var core = product.Single(f => f.Path == CorePath).Code;
        var request = Src.BodyOf(core, @"EditUpdateStart\s+IWorkspacePilotEditPort\.RequestUpdate\s*\(");
        if (Src.Count(request, @"\.TryRequest\s*\(") != 1 || Src.Count(core, @"\.TryRequest\s*\(") != 1) yield return "TryRequest is not called exactly once, from RequestUpdate";

        // The own mutation: opened and closed only by the run of the core and by the forwards of the controller and of the bridge.
        foreach (var name in new[] { "OpenOwnMutation", "CloseOwnMutation" })
        {
            var callers = product.Where(f => Regex.IsMatch(f.Code, @"\." + name + @"\s*\(")).Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToArray();
            var expected = new[] { CorePath, ControllerPath, BridgePath }.OrderBy(p => p, StringComparer.Ordinal).ToArray();
            if (!callers.SequenceEqual(expected)) yield return "." + name + "( is called from " + string.Join(", ", callers);
            var run = Src.BodyOf(core, @"private\s+sealed\s+class\s+PilotUpdateRun\b");
            if (Src.Count(core, @"\." + name + @"\s*\(") != Src.Count(run, @"\." + name + @"\s*\(")) yield return "." + name + "( is named in the core outside the run";
        }
    }

    [Fact]
    public void G10TheRequestIsCalledOnlyByTheTwoForwardsTheExecutorOnlyByTheRequestAndTheOwnMutationOnlyByTheRunAndTheForwards()
    {
        Assert.Empty(G10Violations(Product()));
    }

    [Fact]
    public void G10TripsOnAnotherCallerOfTheRequestOfTheExecutorOrOfTheOwnMutation()
    {
        var product = Product();

        Assert.NotEmpty(G10Violations(Append(product, "src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs", "port.RequestUpdate(r);")));
        // I-01: no folder is outside the census any more. The folders that the guard used to leave out trip it, in a new file and in an old one.
        Assert.NotEmpty(G10Violations(product.Append(("src/RackCad.UI/Systems/PushBack/OtherThing.cs", "class C { void M() { port.RequestUpdate(r); } }")).ToList()));
        Assert.NotEmpty(G10Violations(product.Append(("src/RackCad.UI/Editor/OtherThing.cs", "class C { void M() { port.RequestUpdate(r); } }")).ToList()));
        Assert.NotEmpty(G10Violations(Append(product, "src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs", "port.RequestUpdate(r);")));
        Assert.NotEmpty(G10Violations(Append(product, "src/RackCad.UI/Systems/Cantilever/RackCantileverWindow.xaml.cs", "session.RequestUpdate(ctx => null);")));
        // A window that had its own RequestUpdate and lost it is a trip too: the exception is a count of one, by exact file.
        Assert.NotEmpty(G10Violations(With(product, "src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs", code => code.Replace(".RequestUpdate(", ".RequestUpdateX(", StringComparison.Ordinal))));
        // The gesture of the field stays admitted for F4-T4C2.
        Assert.Empty(G10Violations(Append(product, FieldViewModelPath, "_port.RequestUpdate(request);")));
        Assert.NotEmpty(G10Violations(Append(product, CorePath, "var s = _pilotEdit.RequestUpdate(r);")));
        Assert.NotEmpty(G10Violations(Append(product, AdapterPath, "_host.TryRequest(1, run, out var f);")));
        Assert.NotEmpty(G10Violations(Append(product, CorePath, "var s = _updateHost.TryRequest(1, run, out var f);")));
        Assert.NotEmpty(G10Violations(Append(product, GatePath, "_bridge.OpenOwnMutation(s);")));
        Assert.NotEmpty(G10Violations(With(product, CorePath, code => code.Replace("private UpdateTicket? ExpireStaleUpdate(SessionId session, string? requestRack = null)\n        {", "private UpdateTicket? ExpireStaleUpdate(SessionId session, string? requestRack = null)\n        { _bridge.CloseOwnMutation(session);", StringComparison.Ordinal).Replace("private UpdateTicket? ExpireStaleUpdate(SessionId session, string? requestRack = null)\r\n        {", "private UpdateTicket? ExpireStaleUpdate(SessionId session, string? requestRack = null)\r\n        { _bridge.CloseOwnMutation(session);", StringComparison.Ordinal))));
    }

    // ================================================================== INV-F4T4C-03: the census of Task, async and await

    private static readonly Dictionary<string, (int Task, int AsyncAwait)> TaskCensus = new(StringComparer.Ordinal)
    {
        ["src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs"] = (1, 0),
        ["src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs"] = (2, 0),
        ["src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs"] = (1, 0),
        ["src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs"] = (5, 2),
    };

    private static bool InTheCensus(string path) =>
        path.StartsWith("src/RackCad.Plugin/Workspace/", StringComparison.Ordinal)
        || path.StartsWith("src/RackCad.Application/Workspace/", StringComparison.Ordinal)
        || path.StartsWith("src/RackCad.UI/Workspace/", StringComparison.Ordinal)
        || path.StartsWith("src/RackCad.Application/Systems/PushBack/Pilot/", StringComparison.Ordinal)
        || path == AdapterPath;

    private static IEnumerable<string> TaskCensusViolations(IReadOnlyList<(string Path, string Code)> product)
    {
        foreach (var (path, code) in product.Where(f => InTheCensus(f.Path)))
        {
            var found = (Task: Regex.Matches(code, @"\bTask\b").Count, AsyncAwait: Regex.Matches(code, @"\basync\b|\bawait\b").Count);
            var expected = TaskCensus.TryGetValue(path, out var e) ? e : (0, 0);
            if (found != expected) yield return path + ": Task=" + found.Task + ", async/await=" + found.AsyncAwait + " (expected " + expected.Item1 + ", " + expected.Item2 + ")";
        }

        // No Dispatcher, Timer, .Result, .Wait( or GetAwaiter in anything that is new.
        foreach (var path in new[] { HostRunPath, AdapterPath })
        {
            var code = product.Single(f => f.Path == path).Code;
            if (Regex.IsMatch(code, @"\bDispatcher\b|\bTimer\b|\.Result\b|\.Wait\s*\(|GetAwaiter")) yield return path + " blocks or schedules";
        }
    }

    [Fact]
    public void ThereIsNoNewTaskAsyncOrAwaitOutsideTheAdapterAndTheNavigationKeepsItsExactCountsPerFile()
    {
        Assert.Empty(TaskCensusViolations(Product()));
        Assert.Equal(4, TaskCensus.Count);
    }

    [Theory]
    [InlineData(CorePath, "Task t = null;")]
    [InlineData(ControllerPath, "await Task.Delay(1);")]
    [InlineData(HostRunPath, "Task.Run(() => { });")]
    [InlineData(GatePath, "async void M() { }")]
    [InlineData("src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs", "Task more = null;")]
    [InlineData("src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs", "Task another = null;")]
    [InlineData(AdapterPath, "Task.Run(() => { });")]
    [InlineData(HostRunPath, "var d = Dispatcher.CurrentDispatcher;")]
    [InlineData(AdapterPath, "var r = task.Result;")]
    public void TheCensusTripsOnANewTaskAsyncAwaitDispatcherOrBlockingCall(string path, string injected)
    {
        Assert.NotEmpty(TaskCensusViolations(Append(Product(), path, injected)));
    }

    // ================================================================== D-T4C-13: I-01, M-01 and the callers of EnterError

    private static IEnumerable<string> EnterErrorViolations(IReadOnlyList<(string Path, string Code)> product) =>
        product
            .Where(f => f.Path.StartsWith("src/RackCad.Application/", StringComparison.Ordinal) || f.Path.StartsWith("src/RackCad.Plugin/", StringComparison.Ordinal))
            .Where(f => Regex.IsMatch(f.Code, @"\.EnterError\s*\("))
            .Select(f => f.Path + " calls EnterError");

    [Fact]
    public void NoProductCallerOfEnterErrorExistsInApplicationOrPluginAndTheDeferredBlockOfTheDraftIsUntouched()
    {
        Assert.Empty(EnterErrorViolations(Product()));
        // The private EnterError of the view model of the panel is another thing: it has no dot.
        Assert.Matches(@"\bEnterError\s*\(\s*Exception\s+exception\s*\)", Src.Code("src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs"));
    }

    [Theory]
    [InlineData(CorePath, "draft.EnterError(\"x\");")]
    [InlineData(ControllerPath, "session.EnterError(ex.Message);")]
    [InlineData(AdapterPath, "d.EnterError(\"x\");")]
    [InlineData("src/RackCad.Application/Workspace/WorkspacePilotSupport.cs", "draft.EnterError(\"x\");")]
    public void TheEnterErrorGuardTripsOnAProductCaller(string path, string injected)
    {
        Assert.NotEmpty(EnterErrorViolations(Append(Product(), path, injected)));
    }

    // M-01 (1): the state of the run is outside the try, and what escapes is classified by what the run did (the seal, then a write).
    private static IEnumerable<string> RunEscapeViolations(string run)
    {
        var execute = Src.BodyOf(run, @"public\s+static\s+DraftApplyOutcome<PushBackPilotDraftState>\s+Execute\s*\(");
        var created = execute.IndexOf("var execution = new Execution(", StringComparison.Ordinal);
        var tryAt = Regex.Match(execute, @"\btry\b").Index;
        if (created < 0 || created > tryAt) yield return "the state of the run is not created before the try";
        if (!Regex.IsMatch(execute, @"catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*return\s+execution\.OutcomeOfAnEscape\s*\(\s*ex\s*\)\s*;")) yield return "the catch does not classify by the state of the run";
        var escape = Src.BodyOf(run, @"public\s+DraftApplyOutcome<PushBackPilotDraftState>\s+OutcomeOfAnEscape\s*\(");
        var seal = escape.IndexOf("_sealReturned", StringComparison.Ordinal);
        var write = escape.IndexOf("_writeStarted", StringComparison.Ordinal);
        var plain = escape.IndexOf("NotAppliedFailure(", StringComparison.Ordinal);
        if (seal < 0 || write < seal || plain < write) yield return "the escape does not consult the seal, then the write, before a plain failure";
        if (Regex.IsMatch(escape, @"DraftApplyOutcomeKind\.Applied\b|\.Applied\s*\(")) yield return "the escape can answer an applied";
    }

    [Fact]
    public void M01TheEscapeOfTheRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal()
    {
        Assert.Empty(RunEscapeViolations(CodeOf(RunPath)));
        Assert.Contains("OutcomeOfAnEscape", CodeOf(RunPath));
    }

    [Fact]
    public void M01TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst()
    {
        var run = CodeOf(RunPath);

        Assert.NotEmpty(RunEscapeViolations(run.Replace("return execution.OutcomeOfAnEscape(ex);", "return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(RunFailurePrefix + ex.Message);", StringComparison.Ordinal)));
        Assert.NotEmpty(RunEscapeViolations(run.Replace("var execution = new Execution(draft, request, port);", string.Empty, StringComparison.Ordinal)));
        Assert.NotEmpty(RunEscapeViolations(run.Replace("if (_sealReturned) return PostFailed(PostFailurePrefix + ex.Message);", string.Empty, StringComparison.Ordinal)));
    }

    // M-01 (2) and (4): a close that was not accepted is never a success, and the answer of the accepted one is built in a try with a fallback.
    private static IEnumerable<string> CloseAnswerViolations(string core)
    {
        var notClosed = Src.BodyOf(core, @"private\s+static\s+EditUpdateOutcome\s+AnswerOfAClosedThatWasNotAccepted\s*\(");
        if (!notClosed.Contains("ClosedInError(", StringComparison.Ordinal) || !notClosed.Contains("UpdateNotClosedReason", StringComparison.Ordinal) || !notClosed.Contains("StatusOfOutcomeKind(kind)", StringComparison.Ordinal))
            yield return "a close that was not accepted does not answer a ClosedInError with the classification";
        if (Regex.IsMatch(notClosed, @"Closed\s*\(\s*EditUpdateStatus\.(?:Applied|AppliedWithWarnings|AppliedPostRereadFailed|CommitThrew)\b")) yield return "a close that was not accepted answers a classified outcome";

        var update = Src.BodyOf(core, @"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\(");
        if (!Regex.IsMatch(update, @"if\s*\(\s*!closed\.Accepted\s*\)\s*return\s+AnswerOfAClosedThatWasNotAccepted\s*\(\s*outcome\s*,\s*closed\.Kind\s*\)\s*;")) yield return "TryUpdatePilot does not answer through the close that was not accepted";
        var status = update.IndexOf("var status = StatusOfOutcomeKind(closed.Kind);", StringComparison.Ordinal);
        var tryAt = status < 0 ? -1 : Regex.Match(update.Substring(status), @"\btry\b") is { Success: true } found ? status + found.Index : -1;
        if (status < 0 || tryAt < status) yield return "the state of the answer is not computed before the try that builds it";

        // M-05: everything that can build the answer or note the edit is inside that try, with the fallback of its catch: the answer of a close
        // that was not accepted, and the note of the edit together with the mark of known clean.
        var catchAt = tryAt < 0 ? -1 : update.IndexOf("catch", tryAt, StringComparison.Ordinal);
        foreach (var (name, pattern) in new[]
                 {
                     ("the answer of a close that was not accepted", @"if\s*\(\s*!closed\.Accepted\s*\)\s*return\s+AnswerOfAClosedThatWasNotAccepted\b"),
                     ("the note of the edit", @"\bNotePilotEdit\s*\(\s*request\.Session\s*\)\s*;"),
                     ("the mark of known clean", @"_knownClean\.Remove\s*\(\s*request\.Session\s*\)\s*;"),
                 })
        {
            var at = status < 0 ? Match.Empty : Regex.Match(update.Substring(status), pattern);
            if (!at.Success || tryAt < 0 || catchAt < 0 || status + at.Index < tryAt || status + at.Index > catchAt) yield return name + " is not inside the try of the answer";
        }

        // The fallback of a close that was not accepted is never a success either: it carries the reason that the draft did not accept it.
        if (!Regex.IsMatch(update, @"catch\s*\(\s*Exception\s+ex\s*\)\s*\{[^{}]*!closed\.Accepted[^{}]*UpdateNotClosedReason")) yield return "the fallback of a close that was not accepted does not carry its reason";
        if (!Regex.IsMatch(update, @"catch\s*\(\s*Exception\s+ex\s*\)\s*\{[^{}]*Closed\s*\(\s*status\s*,\s*draft\.LastApplyCause\s*,\s*Array\.Empty<string>\(\)\s*,\s*Array\.Empty<string>\(\)\s*\)")) yield return "the answer has no fallback that carries the state";
    }

    [Fact]
    public void M01TheCloseThatWasNotAcceptedIsNeverASuccessAndTheAnswerIsBuiltInATryWithAFallback()
    {
        Assert.Empty(CloseAnswerViolations(CodeOf(CorePath)));
    }

    [Fact]
    public void M01TheCloseGuardTripsOnAFixedNotAppliedFailureAFixedAppliedAndAnAnswerWithoutAFallback()
    {
        var core = CodeOf(CorePath);
        var notClosed = Src.BodyOf(core, @"private\s+static\s+EditUpdateOutcome\s+AnswerOfAClosedThatWasNotAccepted\s*\(");

        Assert.NotEmpty(CloseAnswerViolations(core.Replace(notClosed, "private static EditUpdateOutcome AnswerOfAClosedThatWasNotAccepted(DraftApplyOutcome<PushBackPilotDraftState> outcome, DraftApplyOutcomeKind kind) { return EditUpdateOutcome.Closed(EditUpdateStatus.NotAppliedFailure, \"x\", Array.Empty<string>(), Array.Empty<string>()); }", StringComparison.Ordinal)));
        Assert.NotEmpty(CloseAnswerViolations(core.Replace(notClosed, "private static EditUpdateOutcome AnswerOfAClosedThatWasNotAccepted(DraftApplyOutcome<PushBackPilotDraftState> outcome, DraftApplyOutcomeKind kind) { var u = ClosedInError(null, StatusOfOutcomeKind(kind), null, null, null); return EditUpdateOutcome.Closed(EditUpdateStatus.Applied, null, Array.Empty<string>(), Array.Empty<string>()); }", StringComparison.Ordinal)));
        Assert.NotEmpty(CloseAnswerViolations(Regex.Replace(core, @"Closed\s*\(\s*status\s*,\s*draft\.LastApplyCause\s*,\s*Array\.Empty<string>\(\)\s*,\s*Array\.Empty<string>\(\)\s*\)", "Closed(status, null, null!, null!)")));
        Assert.NotEmpty(CloseAnswerViolations(core.Replace("return AnswerOfAClosedThatWasNotAccepted(outcome, closed.Kind);", "return EditUpdateOutcome.Closed(EditUpdateStatus.Applied, null, Array.Empty<string>(), Array.Empty<string>());", StringComparison.Ordinal)));
    }

    [Fact]
    public void M05TheCloseGuardTripsWhenTheAnswerOfANotAcceptedCloseOrTheNoteOfTheEditOrTheMarkOfKnownCleanLeaveTheTryOfTheAnswer()
    {
        var core = CodeOf(CorePath);
        var update = Src.BodyOf(core, @"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\(");
        const string statusLine = "var status = StatusOfOutcomeKind(closed.Kind);";
        var notAccepted = Regex.Match(update, @"if\s*\(\s*!closed\.Accepted\s*\)\s*return\s+AnswerOfAClosedThatWasNotAccepted\s*\(\s*outcome\s*,\s*closed\.Kind\s*\)\s*;");
        var note = Regex.Match(update, @"NotePilotEdit\s*\(\s*request\.Session\s*\)\s*;");
        var known = Regex.Match(update, @"_knownClean\.Remove\s*\(\s*request\.Session\s*\)\s*;");
        Assert.Empty(CloseAnswerViolations(core));
        Assert.True(notAccepted.Success && note.Success && known.Success && update.Contains(statusLine, StringComparison.Ordinal));

        // Each of the three moved in front of the state of the answer, outside the try.
        foreach (var piece in new[] { notAccepted, note, known })
        {
            var moved = update.Remove(piece.Index, piece.Length).Replace(statusLine, piece.Value + " " + statusLine, StringComparison.Ordinal);
            Assert.NotEmpty(CloseAnswerViolations(core.Replace(update, moved, StringComparison.Ordinal)));
        }

        // The fallback that does not carry the reason of a close that was not accepted.
        Assert.NotEmpty(CloseAnswerViolations(core.Replace(update, update.Replace("UpdateNotClosedReason, status", "null, status", StringComparison.Ordinal), StringComparison.Ordinal)));
    }

    // ================================================================== M-04: the sentence of the host that did not run is never used as a prefix

    private static IEnumerable<string> NotRunPrefixViolations(IReadOnlyList<(string Path, string Code)> product)
    {
        foreach (var path in new[] { HostRunPath, CorePath })
        {
            var code = product.Single(f => f.Path == path).Code;
            if (Regex.IsMatch(code, @"HostNotRunReason\s*\+|\+\s*(?:PushBackPilotHostTexts\.)?HostNotRunReason\b")) yield return path + " uses HostNotRunReason as a prefix";
        }

        // The run of the core is new in this task: it has no literal space of its own to compose a text with.
        var run = Src.BodyOf(product.Single(f => f.Path == CorePath).Code, @"private\s+sealed\s+class\s+PilotUpdateRun\b");
        if (run.Contains("\" \"", StringComparison.Ordinal)) yield return "the run of the core composes a text with a literal space";
    }

    [Fact]
    public void M04TheSentenceOfTheHostThatDidNotRunIsTheWholeTextInTheRunnerAndInTheCoreAndTheExceptionGoesToTheLog()
    {
        Assert.Empty(NotRunPrefixViolations(Product()));
    }

    [Fact]
    public void M04TheGuardTripsOnTheSentenceUsedAsAPrefixInTheRunnerAndInTheCoreAndOnALiteralSpaceInTheRunOfTheCore()
    {
        var product = Product();
        var composed = "var cause = PushBackPilotHostTexts.HostNotRunReason + \" \" + ex.Message;";

        Assert.Empty(NotRunPrefixViolations(product));
        Assert.NotEmpty(NotRunPrefixViolations(Append(product, HostRunPath, composed)));
        Assert.NotEmpty(NotRunPrefixViolations(Append(product, CorePath, composed)));
        Assert.NotEmpty(NotRunPrefixViolations(Append(product, CorePath, "var again = ex.Message + PushBackPilotHostTexts.HostNotRunReason;")));
        Assert.NotEmpty(NotRunPrefixViolations(With(product, CorePath, code => code.Replace("public void Begin()", "public void Begin() { var s = \" \"; }\n            public void BeginAgain()", StringComparison.Ordinal))));
    }

    // ================================================================== D-T4C-20 row 22: the command that is not the update, in the handler of CommandWillStart

    private static string CommandWillStartBlock(string bridge)
    {
        var hook = Src.BodyOf(bridge, @"private\s+Action\s+Hook\s*\(");
        var start = hook.IndexOf("case BridgeSource.CommandWillStart:", StringComparison.Ordinal);
        var next = hook.IndexOf("case BridgeSource.CommandEnded:", StringComparison.Ordinal);
        Assert.True(start >= 0 && next > start);
        return hook.Substring(start, next - start);
    }

    private static IEnumerable<string> ForeignCommandViolations(string bridge)
    {
        var block = CommandWillStartBlock(bridge);
        if (Src.Count(bridge, @"\bNoteForeignCommandStart\s*\(") != 1) yield return "NoteForeignCommandStart is not named exactly once in the bridge";
        if (Src.Count(block, @"_core\.NoteForeignCommandStart\s*\(\s*a\.Session\s*\)") != 1) yield return "the block of CommandWillStart does not call it once";
        var contain = block.IndexOf("ContainSession(a,", StringComparison.Ordinal);
        var decision = Regex.Match(block, @"if\s*\(\s*_core\.OnSourceEvent\s*\(\s*a\.Session\s*,\s*a\.Source\s*\)\s*\)\s*RaiseCommandStateChanged\s*\(\s*\)\s*;");
        var note = Regex.Match(block, @"if\s*\(\s*!string\.Equals\s*\(\s*e\.GlobalCommandName\s*,\s*""EXECUTEFUNCTION""\s*,\s*StringComparison\.OrdinalIgnoreCase\s*\)\s*\)\s*_core\.NoteForeignCommandStart\s*\(\s*a\.Session\s*\)\s*;");
        if (contain < 0 || !decision.Success || !note.Success) yield return "the call is not conditioned on the name of the command inside the contained handler";
        else if (!(contain < decision.Index && decision.Index < note.Index)) yield return "the call does not come after the decision of the notice, inside the contained handler";

        // It is the only place; no other block of the bridge names it, and the handler reads nothing of the drawing.
        var elsewhere = bridge.Replace(block, string.Empty);
        if (Regex.IsMatch(elsewhere, @"NoteForeignCommandStart")) yield return "another block names NoteForeignCommandStart";
        if (Regex.IsMatch(block, @"GetObject|StartTransaction|LockDocument|OpenMode|\.Database\b|\.Read\w*\s*\(")) yield return "the handler of CommandWillStart reads the drawing";
    }

    [Fact]
    public void TheHandlerOfCommandWillStartNotesAForeignCommandOnceInsideTheContainedHandlerConditionedOnItsNameAfterTheNoticeDecision()
    {
        Assert.Empty(ForeignCommandViolations(CodeOf(BridgePath)));
    }

    [Fact]
    public void TheForeignCommandGuardTripsOnTheCallWithoutTheConditionOnTheCallInAnotherBlockAndOnAReadOfTheDrawing()
    {
        var bridge = CodeOf(BridgePath);

        var condition = Regex.Match(bridge, @"if\s*\(\s*!string\.Equals\s*\(\s*e\.GlobalCommandName\s*,\s*""EXECUTEFUNCTION""\s*,\s*StringComparison\.OrdinalIgnoreCase\s*\)\s*\)\s*");
        Assert.True(condition.Success);
        Assert.NotEmpty(ForeignCommandViolations(bridge.Remove(condition.Index, condition.Length)));
        Assert.NotEmpty(ForeignCommandViolations(bridge.Replace("case BridgeSource.CommandEnded:\n                {", "case BridgeSource.CommandEnded:\n                { _core.NoteForeignCommandStart(a.Session);", StringComparison.Ordinal)
            .Replace("case BridgeSource.CommandEnded:\r\n                {", "case BridgeSource.CommandEnded:\r\n                { _core.NoteForeignCommandStart(a.Session);", StringComparison.Ordinal)));
        Assert.NotEmpty(ForeignCommandViolations(bridge.Replace("document.CommandWillStart += handler;", "var o = tr.GetObject(id, OpenMode.ForRead); document.CommandWillStart += handler;", StringComparison.Ordinal)));
        // Before the decision of the notice, outside the order that the package fixed.
        var note = Regex.Match(bridge, @"if\s*\(\s*!string\.Equals[^;]*NoteForeignCommandStart\s*\(\s*a\.Session\s*\)\s*;");
        var decision = Regex.Match(bridge, @"if\s*\(\s*_core\.OnSourceEvent\s*\(\s*a\.Session\s*,\s*a\.Source\s*\)\s*\)\s*RaiseCommandStateChanged\s*\(\s*\)\s*;");
        Assert.True(note.Success && decision.Success && decision.Index < note.Index);
        var swapped = bridge.Remove(note.Index, note.Length).Insert(decision.Index, note.Value + " ");
        Assert.NotEmpty(ForeignCommandViolations(swapped));
    }

    // ================================================================== INV-F4T4C-15 and 16: Undo and the closure of T1B

    [Fact]
    public void TheNewFilesNameNoUndoApiNoSendStringToExecuteNoCommandMethodAndNoCommandFlags()
    {
        foreach (var path in new[] { HostRunPath, AdapterPath })
        {
            var code = CodeOf(path);
            Assert.DoesNotMatch(new Regex(@"StartUndoMark|EndUndoMark|\bUndo\b|SendStringToExecute|CommandMethod|CommandFlags|InsertRequestedRaised"), code);
        }

    }
}
