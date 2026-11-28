#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T2B2B-LIFECYCLE (INV-F4T2B2B-01 to 04, 06 to 09, 11 and 12): source guards of the lifecycle of the drafts. The Core test project
// cannot reference the Plugin (it needs AutoCAD), so the Plugin files are read as text, like the rest of the guards of the workspace.
// These guards are ADDITIVE and live in a new file: no existing guard was changed or weakened except the two declared changes (the list of
// public members of the bridge core and the names of EditIntentStatus). Every guard fails by ASSERTION when the text it expects is
// missing. By the rule of I-1 (F4-E32, F4-E33) none of them prohibits for ever what belongs to F4-T3 or F4-T4: the hook of the live text
// before the advice of the close (a wrapper of the advisor), the exposure of the reload, the discard and the note by the port or the UI,
// the consumption of the queue of notices, the discard of ONE draft by the host, the calls to NotePilotEdit in Update, new states. They fix
// only what this task needs, for example "Veto() is called if and only if the core decides Veto", never "the handler does nothing else".
// Evidence named by the package: obligation 5 (no write: the lifecycle files name no write API), 14 (the retirement and its notice) and 18
// (no object of the host lives past the call: the lifecycle files hold no Document, Transaction or database object).
public class FullPanelCopyWorkspaceLifecycleGuardRedTests
{
    private const string HostCorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";

    private static string Host() => Src.Code(HostCorePath);

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

    // ------------------------------------------------------------------ INV-F4T2B2B-06: the reload

    private static string ReloadBody() => Src.BodyOf(Host(), @"public\s+EditIntentOutcome\s+TryReloadPilotEdit\s*\(\s*EditIntentRequest\s+request\s*\)");

    [Fact]
    public void TheMarkingOnlyTouchesTheDraftsOfThePilotOfTheSessionAndIsOnlyAHint()
    {
        var body = Src.BodyOf(Host(), @"private\s+void\s+MarkPilotDraftsPossiblyStale\s*\(\s*SessionId\s+\w+\s*\)");

        Assert.Contains("_drafts.RackIdsOf(session)", body);
        Assert.Contains("_drafts.Find<PushBackFullDraftState>(session, rackId)", body);
        Assert.Contains(".MarkPossiblyStale()", body);
        Assert.DoesNotMatch(new Regex(@"ReadIntent|_editIntentReader|CheckNow|DiscardAndReload|EnterError|SetCurrentDraftState|\bThrow\b"), body);
    }

    [Fact]
    public void TheReloadIsAMethodOfItsOwnWithTheGateAndTheEightStepsOfTheTableAndOneReadThatOnlyReachesDiscardAndReload()
    {
        var body = ReloadBody();

        AssertInOrder(
            body,
            "string.IsNullOrWhiteSpace(request.RackId)",
            "HasWitnessWithoutKey(request)",
            "PassGate(",
            "_drafts.Find<PushBackFullDraftState>(",
            ".NeedsLazyReload",
            "_editIntentReader!.ReadIntent(",
            ".DiscardAndReload(full.Read);");
        Assert.Equal(1, Src.Count(body, @"\.ReadIntent\s*\("));
        Assert.Equal(1, Src.Count(body, @"_editIntentReader\s*[.!]"));
        Assert.Equal(1, Src.Count(body, @"\bfull\.Read\b"));
        Assert.Matches(@"\.DiscardAndReload\s*\(\s*full\.Read\s*\)\s*;", body);
        // F4-T3A (M-2): the call is fixed in the body of the reload, not in the whole file, so a later operation of the host that reloads
        // does not trip a guard that is about this one.
        Assert.Equal(1, Src.Count(body, @"\.DiscardAndReload\s*\("));
        Assert.Equal(0, Src.Count(body, @"\.StaleBase\b|\.CheckNow\s*\(|\.Begin\s*\("));
        Assert.DoesNotMatch(new Regex(@"\.(?:RetireSession|DiscardAll|EvaluateClose|MarkPossiblyStale|KeepMineOnCurrentVersion)\s*\("), body);
    }
}
