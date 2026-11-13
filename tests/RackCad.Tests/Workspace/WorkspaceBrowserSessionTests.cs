#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

/// <summary>Shared helpers of the F3-T1-BROWSER-CORE tests. Shapes are plain strings so a failure is always an assertion.</summary>
internal static class WorkspaceBrowserTestSupport
{
    public static BrowserSession NewSession(long id = 1)
    {
        var session = new BrowserSessionRegistry().GetOrCreate(new SessionId(id));
        Assert.NotNull(session);
        return session!;
    }

    /// <summary>Tabs in presentation order, "+" pinned and "~" preview, then the active tab: "A+,B~|B".</summary>
    public static string Shape(BrowserSession session) =>
        string.Join(",", session.Tabs.Select(t => t.RackId + (t.IsPreview ? "~" : "+"))) + "|" + (session.ActiveRackId ?? "-");

    /// <summary>Like <see cref="Shape"/> with the summary of every tab: "A+[Ready/name/kind]".</summary>
    public static string Detail(BrowserSession session) =>
        string.Join(",", session.Tabs.Select(t => t.RackId + (t.IsPreview ? "~" : "+") + "[" + t.Status + "/" + (t.Name ?? "null") + "/" + (t.Kind ?? "null") + "]"))
        + "|" + (session.ActiveRackId ?? "-");

    public static SelectionContext One(string rackId) => SelectionContext.One(rackId);
}

// I-64 F3-T1-BROWSER-CORE: behavior of the pure browser session (INV-F3T1-01 to 11, 17).
public class WorkspaceBrowserSessionTests
{
    private static BrowserSession New() => WorkspaceBrowserTestSupport.NewSession();

    private static string Shape(BrowserSession s) => WorkspaceBrowserTestSupport.Shape(s);

    private static string Detail(BrowserSession s) => WorkspaceBrowserTestSupport.Detail(s);

    private static SelectionContext One(string id) => SelectionContext.One(id);

    [Fact]
    public void ANewSessionIsEmptyAndTheLiveSurfacePolicyIsOne()
    {
        var s = New();

        Assert.Equal("|-", Shape(s));
        Assert.Null(s.ActiveRackId);
        Assert.Null(s.LiveSurfaceRackId);
        Assert.False(s.IsDestroyed);
        Assert.Equal(1, BrowserSession.MaxLiveSurfaces);
        Assert.Equal(new SessionId(1), s.Id);
    }

    [Fact]
    public void ATabRecordIsAValueThatNeedsARackId()
    {
        var a = new BrowserTabRecord("R1", "n", "k", true, BrowserTabStatus.Ready);
        var b = new BrowserTabRecord("R1", "n", "k", true, BrowserTabStatus.Ready);

        Assert.Equal(a, b);
        Assert.NotEqual(a, a with { IsPreview = false });
        Assert.Throws<ArgumentException>(() => new BrowserTabRecord("", null, null, true, BrowserTabStatus.NotLoaded));
        Assert.Throws<ArgumentException>(() => new BrowserTabRecord("  ", null, null, true, BrowserTabStatus.NotLoaded));
        Assert.Throws<ArgumentException>(() => new BrowserTabRecord(null!, null, null, true, BrowserTabStatus.NotLoaded));
    }

    [Fact]
    public void SelectingARackWithNoTabCreatesAPreviewBornWithOnlyItsRackId()
    {
        var s = New();

        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("A")));

        Assert.Equal("A~|A", Shape(s));
        Assert.Equal("A~[NotLoaded/null/null]|A", Detail(s));
        Assert.Equal("A", s.LiveSurfaceRackId);
    }

    [Fact]
    public void ThereIsAtMostOnePreviewAndANewRackReplacesTheOldPreview()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));

        Assert.Equal(BrowserChange.Replaced, s.ApplySelectionContext(One("B")));
        Assert.Equal("B~|B", Shape(s));

        Assert.Equal(BrowserChange.Replaced, s.ApplySelectionContext(One("C")));
        Assert.Equal("C~|C", Shape(s));
    }

    [Fact]
    public void ReplacingThePreviewKeepsItsPositionAndNeverTouchesPinnedTabs()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        s.Pin("B");
        s.ApplySelectionContext(One("C"));
        Assert.Equal("A+,B+,C~|C", Shape(s));

        Assert.Equal(BrowserChange.Replaced, s.ApplySelectionContext(One("D")));

        Assert.Equal("A+,B+,D~|D", Shape(s));
    }

    [Fact]
    public void ThePreviewReplacementStartsFromAnEmptySummaryEvenIfTheOldOneHadOne()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.ApplySummary("A", BrowserTabStatus.Ready, "Rack A", "Selective");

        s.ApplySelectionContext(One("B"));

        Assert.Equal("B~[NotLoaded/null/null]|B", Detail(s));
    }

    [Fact]
    public void SelectingARackThatAlreadyHasATabActivatesTheExistingOneWithoutChangingModeOrOrder()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        Assert.Equal("A+,B~|B", Shape(s));

        Assert.Equal(BrowserChange.Activated, s.ApplySelectionContext(One("A")));

        Assert.Equal("A+,B~|A", Shape(s));
    }

    [Fact]
    public void SelectingThePreviewRackAgainAfterAnotherTabWasActivatedKeepsItAPreview()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        s.Activate("A");
        s.ApplySelectionContext(SelectionContext.None);

        Assert.Equal(BrowserChange.Activated, s.ApplySelectionContext(One("B")));

        Assert.Equal("A+,B~|B", Shape(s));
    }

    [Fact]
    public void APinnedTabIsNeverReplacedAndTheNextRackCreatesANewPreviewAtTheEnd()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        Assert.Equal(BrowserChange.Pinned, s.Pin("A"));
        Assert.Equal("A+|A", Shape(s));

        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("B")));
        Assert.Equal("A+,B~|B", Shape(s));

        Assert.Equal(BrowserChange.Replaced, s.ApplySelectionContext(One("C")));
        Assert.Equal("A+,C~|C", Shape(s));
    }

    [Fact]
    public void PinIsIdempotentKeepsTheActiveTabAndIgnoresUnknownRacks()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        Assert.Equal("A+,B~|B", Shape(s));

        Assert.Equal(BrowserChange.None, s.Pin("A"));
        Assert.Equal(BrowserChange.None, s.Pin("Z"));
        Assert.Equal(BrowserChange.None, s.Pin(null));
        Assert.Equal(BrowserChange.None, s.Pin("  "));
        Assert.Equal("A+,B~|B", Shape(s));

        Assert.Equal(BrowserChange.Pinned, s.Pin("B"));
        Assert.Equal("A+,B+|B", Shape(s));
    }

    [Fact]
    public void ClosingTheActiveTabActivatesTheNextPositionElseThePreviousElseNone()
    {
        var s = New();
        foreach (var id in new[] { "A", "B", "C" })
        {
            s.ApplySelectionContext(One(id));
            s.Pin(id);
        }

        s.Activate("B");
        Assert.Equal("A+,B+,C+|B", Shape(s));

        Assert.Equal(BrowserChange.Closed, s.Close("B"));
        Assert.Equal("A+,C+|C", Shape(s));

        Assert.Equal(BrowserChange.Closed, s.Close("C"));
        Assert.Equal("A+|A", Shape(s));

        Assert.Equal(BrowserChange.Closed, s.Close("A"));
        Assert.Equal("|-", Shape(s));
        Assert.Null(s.ActiveRackId);
        Assert.Null(s.LiveSurfaceRackId);
    }

    [Fact]
    public void ClosingTheFirstActiveTabActivatesTheOneThatTakesItsPosition()
    {
        var s = New();
        foreach (var id in new[] { "A", "B", "C" })
        {
            s.ApplySelectionContext(One(id));
            s.Pin(id);
        }

        s.Activate("A");

        s.Close("A");

        Assert.Equal("B+,C+|B", Shape(s));
    }

    [Fact]
    public void ClosingANonActiveTabKeepsTheActiveOneAndClosingAPreviewWorks()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        s.Activate("A");

        Assert.Equal(BrowserChange.Closed, s.Close("B"));
        Assert.Equal("A+|A", Shape(s));

        s.ApplySelectionContext(One("C"));
        Assert.Equal("A+,C~|C", Shape(s));
        Assert.Equal(BrowserChange.Closed, s.Close("C"));
        Assert.Equal("A+|A", Shape(s));
    }

    [Fact]
    public void ClosingUnknownNullOrBlankChangesNothing()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));

        Assert.Equal(BrowserChange.None, s.Close("Z"));
        Assert.Equal(BrowserChange.None, s.Close(null));
        Assert.Equal(BrowserChange.None, s.Close(""));
        Assert.Equal(BrowserChange.None, s.Close("   "));

        Assert.Equal("A~|A", Shape(s));
    }

    [Fact]
    public void ClosingACleanTabAsksNothingAndOnlyChangesMemory()
    {
        // The type has no way to reach a host: no port, no delegate, no dependency. Closing returns and that is all.
        var type = typeof(BrowserSession);
        var shapes = type.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            .SelectMany(c => c.GetParameters().Select(p => p.ParameterType))
            .Concat(type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly)
                .SelectMany(m => m.GetParameters().Select(p => p.ParameterType)))
            .ToList();
        Assert.All(shapes, t => Assert.False(typeof(Delegate).IsAssignableFrom(t) || t.IsInterface, t.FullName));

        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySummary("A", BrowserTabStatus.Ready, "Rack A", "Selective");

        Assert.Equal(BrowserChange.Closed, s.Close("A"));

        Assert.Equal("|-", Shape(s));
    }

    [Fact]
    public void SelectionNoneNoIdentityAndDiagnosticNeverCreateReplaceActivateOrClose()
    {
        var contexts = new[]
        {
            SelectionContext.None,
            SelectionContext.Selection(2, 0),
            SelectionContext.Selection(1, 3),
            SelectionContext.NoIdentity,
            SelectionContext.Diagnostic,
        };

        foreach (var context in contexts)
        {
            var s = New();
            s.ApplySelectionContext(One("A"));
            s.Pin("A");
            s.ApplySelectionContext(One("B"));
            s.Activate("A");
            var before = Detail(s);
            Assert.Equal("A+,B~|A", Shape(s));

            Assert.Equal(BrowserChange.None, s.ApplySelectionContext(context));

            Assert.Equal(before, Detail(s));
        }
    }

    [Fact]
    public void NonSingleContextsOnAnEmptySessionCreateNothing()
    {
        var s = New();

        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(SelectionContext.None));
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(SelectionContext.Selection(3, 1)));
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(SelectionContext.NoIdentity));
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(SelectionContext.Diagnostic));

        Assert.Equal("|-", Shape(s));
    }

    [Fact]
    public void AnEqualContextToTheLastAppliedOneChangesNothingEvenAfterManualActions()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        s.Pin("B");
        s.Activate("A");
        Assert.Equal("A+,B+|A", Shape(s));

        // A separately built context equals the last one by value.
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(SelectionContext.One("B")));

        Assert.Equal("A+,B+|A", Shape(s));
    }

    [Fact]
    public void ARepeatedContextDoesNotRecreateATabTheUserClosed()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Close("A");

        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("A")));

        Assert.Equal("|-", Shape(s));
    }

    [Fact]
    public void ARealTransitionAppliesAgainAfterNoneAndAfterAnotherRack()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Close("A");
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(SelectionContext.None));

        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("A")));
        Assert.Equal("A~|A", Shape(s));

        s.ApplySelectionContext(One("B"));
        Assert.Equal("B~|B", Shape(s));
        Assert.Equal(BrowserChange.Replaced, s.ApplySelectionContext(One("A")));
        Assert.Equal("A~|A", Shape(s));
    }

    [Fact]
    public void TheLastContextRememberedIsOfAnyKind()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.ApplySelectionContext(SelectionContext.Selection(2, 0));
        s.Close("A");

        // Selection is the last context: repeating it is idempotent, and One(A) is a real transition.
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(SelectionContext.Selection(2, 0)));
        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("A")));
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("A")));
    }

    [Fact]
    public void ActivateChangesNeitherModesNorOrderNorTheRememberedContext()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        Assert.Equal("A+,B~|B", Shape(s));

        Assert.Equal(BrowserChange.Activated, s.Activate("A"));
        Assert.Equal("A+,B~|A", Shape(s));

        // The remembered context is still One(B): repeating it is not a transition.
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("B")));
        Assert.Equal("A+,B~|A", Shape(s));
    }

    [Fact]
    public void ActivateIgnoresUnknownNullBlankAndTheAlreadyActiveTabWithoutThrowing()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));

        Assert.Equal(BrowserChange.None, s.Activate("Z"));
        Assert.Equal(BrowserChange.None, s.Activate(null));
        Assert.Equal(BrowserChange.None, s.Activate(""));
        Assert.Equal(BrowserChange.None, s.Activate(" "));
        Assert.Equal(BrowserChange.None, s.Activate("B"));

        Assert.Equal("A+,B~|B", Shape(s));
    }

    [Fact]
    public void ActiveRackIdIsNullExactlyWithoutTabsAndAlwaysNamesOneExistingTab()
    {
        var s = New();
        Assert.Null(s.ActiveRackId);

        s.ApplySelectionContext(One("A"));
        Assert.Contains(s.Tabs, t => t.RackId == s.ActiveRackId);

        s.Close("A");
        Assert.Empty(s.Tabs);
        Assert.Null(s.ActiveRackId);
    }

    [Fact]
    public void TabsIsAReadOnlyStableSnapshotInPresentationOrder()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        var snapshot = s.Tabs;
        Assert.Equal(new[] { "A", "B" }, snapshot.Select(t => t.RackId).ToArray());

        s.ApplySelectionContext(One("C"));
        s.Close("A");
        s.ApplySummary("C", BrowserTabStatus.Ready, "n", "k");

        Assert.Equal(new[] { "A", "B" }, snapshot.Select(t => t.RackId).ToArray());
        Assert.All(snapshot, t => Assert.Equal(BrowserTabStatus.NotLoaded, t.Status));
        if (snapshot is IList<BrowserTabRecord> list)
        {
            Assert.Throws<NotSupportedException>(() => list.Add(new BrowserTabRecord("X", null, null, true, BrowserTabStatus.NotLoaded)));
            Assert.Throws<NotSupportedException>(() => list[0] = new BrowserTabRecord("X", null, null, true, BrowserTabStatus.NotLoaded));
            Assert.Throws<NotSupportedException>(() => list.Clear());
        }

        Assert.Equal("C~|C", Shape(s));
    }

    [Fact]
    public void TheLiveSurfaceIsExactlyTheActiveTabAndNothingIsMaterialized()
    {
        var s = New();
        Assert.Null(s.LiveSurfaceRackId);

        foreach (var id in new[] { "A", "B", "C" })
        {
            s.ApplySelectionContext(One(id));
            Assert.Equal(s.ActiveRackId, s.LiveSurfaceRackId);
        }

        s.Pin("C");
        s.ApplySelectionContext(One("D"));
        s.Activate("C");
        Assert.Equal("C", s.LiveSurfaceRackId);
        Assert.Equal(1, BrowserSession.MaxLiveSurfaces);
    }

    [Fact]
    public void ASummaryUpdatesOnlyStatusNameAndKindOfThatRecord()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        s.Activate("A");

        Assert.Equal(BrowserChange.SummaryApplied, s.ApplySummary("B", BrowserTabStatus.Ready, "Rack B", "Selective"));

        Assert.Equal("A+[NotLoaded/null/null],B~[Ready/Rack B/Selective]|A", Detail(s));
        Assert.Equal(BrowserChange.SummaryApplied, s.ApplySummary("B", BrowserTabStatus.Error, null, null));
        Assert.Equal("A+[NotLoaded/null/null],B~[Error/null/null]|A", Detail(s));
    }

    [Fact]
    public void ASummaryNeverChangesTheRememberedContextAndKeepsTheModeWhenPinned()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.ApplySummary("A", BrowserTabStatus.Unavailable, "Rack A", "Push Back");
        s.Pin("A");

        Assert.Equal("A+[Unavailable/Rack A/Push Back]|A", Detail(s));
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("A")));
        Assert.Equal("A+[Unavailable/Rack A/Push Back]|A", Detail(s));
    }

    [Fact]
    public void ALateSummaryOfAReplacedPreviewIsIgnoredWithoutThrowing()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.ApplySelectionContext(One("B"));
        var before = Detail(s);

        Assert.Equal(BrowserChange.None, s.ApplySummary("A", BrowserTabStatus.Ready, "Rack A", "Selective"));

        Assert.Equal(before, Detail(s));
        Assert.Equal("B~[NotLoaded/null/null]|B", Detail(s));
    }

    [Fact]
    public void ASummaryForUnknownNullBlankOrIdenticalValuesChangesNothing()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.ApplySummary("A", BrowserTabStatus.Ready, "Rack A", "Selective");
        var before = Detail(s);

        Assert.Equal(BrowserChange.None, s.ApplySummary("Z", BrowserTabStatus.Ready, "n", "k"));
        Assert.Equal(BrowserChange.None, s.ApplySummary(null, BrowserTabStatus.Ready, "n", "k"));
        Assert.Equal(BrowserChange.None, s.ApplySummary("  ", BrowserTabStatus.Ready, "n", "k"));
        Assert.Equal(BrowserChange.None, s.ApplySummary("A", BrowserTabStatus.Ready, "Rack A", "Selective"));

        Assert.Equal(before, Detail(s));
    }

    [Fact]
    public void ASummaryDoesNotChangeOrderOrTheActiveTab()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        s.Pin("A");
        s.ApplySelectionContext(One("B"));
        s.Pin("B");
        s.Activate("A");

        s.ApplySummary("B", BrowserTabStatus.Ready, "n", "k");

        Assert.Equal("A+,B+|A", Shape(s));
    }

    [Fact]
    public void ANullContextIsAProgrammingErrorAndNothingElseThrows()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));

        Assert.Throws<ArgumentNullException>(() => s.ApplySelectionContext(null!));

        Assert.Equal("A~|A", Shape(s));
        var ex = Record.Exception(() =>
        {
            s.Activate(null);
            s.Pin(null);
            s.Close(null);
            s.ApplySummary(null, BrowserTabStatus.Error, null, null);
        });
        Assert.Null(ex);
    }

    [Fact]
    public void EveryChangeValueIsReturnedForItsOperationAndNoneMeansNothingChanged()
    {
        var s = New();
        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("A")));
        Assert.Equal(BrowserChange.Replaced, s.ApplySelectionContext(One("B")));
        Assert.Equal(BrowserChange.Pinned, s.Pin("B"));
        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("C")));
        Assert.Equal(BrowserChange.Activated, s.Activate("B"));
        Assert.Equal(BrowserChange.SummaryApplied, s.ApplySummary("C", BrowserTabStatus.Ready, "n", "k"));
        Assert.Equal(BrowserChange.Closed, s.Close("C"));
        Assert.Equal(BrowserChange.None, s.Close("C"));
    }

    // ------------------------------------------------------------------ long deterministic sequence (INV-F3T1-02 and 08 at every step)

    private static IReadOnlyList<string> RunLongSequence()
    {
        var s = New();
        var racks = new[] { "R1", "R2", "R3", "R4", "R5" };
        var trace = new List<string>();
        uint seed = 20261110;

        uint Next()
        {
            seed = unchecked(seed * 1664525u + 1013904223u);
            return seed >> 8;
        }

        for (var step = 0; step < 3000; step++)
        {
            var rack = racks[Next() % (uint)racks.Length];
            var before = Detail(s);
            BrowserChange change;
            switch (Next() % 10)
            {
                case 0:
                case 1:
                case 2:
                    change = s.ApplySelectionContext(SelectionContext.One(rack));
                    break;
                case 3:
                    change = s.ApplySelectionContext(SelectionContext.None);
                    break;
                case 4:
                    change = s.ApplySelectionContext(Next() % 2 == 0 ? SelectionContext.Selection(2, 1) : SelectionContext.Diagnostic);
                    break;
                case 5:
                    change = s.Activate(rack);
                    break;
                case 6:
                    change = s.Pin(rack);
                    break;
                case 7:
                    change = s.Close(rack);
                    break;
                case 8:
                    change = s.ApplySummary(rack, (BrowserTabStatus)(Next() % 4), "n" + (Next() % 3), "k" + (Next() % 2));
                    break;
                default:
                    change = s.ApplySelectionContext(SelectionContext.NoIdentity);
                    break;
            }

            var after = Detail(s);

            // INV-F3T1-02: at most one preview.
            Assert.True(s.Tabs.Count(t => t.IsPreview) <= 1, "more than one preview at step " + step);

            // INV-F3T1-08: ActiveRackId is null iff there are no tabs; otherwise it names exactly one existing tab.
            if (s.Tabs.Count == 0) Assert.Null(s.ActiveRackId);
            else
            {
                Assert.NotNull(s.ActiveRackId);
                Assert.Equal(1, s.Tabs.Count(t => t.RackId == s.ActiveRackId));
            }

            Assert.Equal(s.Tabs.Count, s.Tabs.Select(t => t.RackId).Distinct().Count());
            Assert.Equal(s.ActiveRackId, s.LiveSurfaceRackId);

            // INV-F3T1-11: None exactly when nothing changed.
            Assert.Equal(change == BrowserChange.None, before == after);

            trace.Add(change + ":" + after);
        }

        return trace;
    }

    [Fact]
    public void ALongDeterministicSequenceKeepsTheInvariantsAtEveryStep()
    {
        var trace = RunLongSequence();

        Assert.Equal(3000, trace.Count);
        // The sequence really exercises every kind of change.
        foreach (var kind in Enum.GetValues<BrowserChange>())
            Assert.Contains(trace, t => t.StartsWith(kind + ":", StringComparison.Ordinal));
    }

    [Fact]
    public void ALongDeterministicSequenceIsReproducible()
    {
        Assert.Equal(RunLongSequence(), RunLongSequence());
    }

    // ------------------------------------------------------------------ I-64 F4-T3B2 (INV-F4T3B2-09): retaining the active tab

    private static BrowserSession WithPinnedActive(string rack)
    {
        var s = New();
        s.ApplySelectionContext(One(rack));
        s.Pin(rack);
        return s;
    }

    [Fact]
    public void KeepActiveWithAnOpenRackChangesNothingAndDoesNotActivateIt()
    {
        var s = WithPinnedActive("A");
        s.ApplySelectionContext(One("B"));
        s.Pin("B");
        s.Activate("A");
        Assert.Equal("A+,B+|A", Shape(s));

        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("B"), keepActive: true));

        Assert.Equal("A+,B+|A", Shape(s));
    }

    [Fact]
    public void KeepActiveWithARackWithoutATabReplacesThePreviewThatIsNotActiveInPlaceAndKeepsTheActiveTab()
    {
        var s = WithPinnedActive("A");
        s.ApplySelectionContext(One("B"));
        s.Activate("A");
        Assert.Equal("A+,B~|A", Shape(s));

        Assert.Equal(BrowserChange.Replaced, s.ApplySelectionContext(One("C"), keepActive: true));

        Assert.Equal("A+,C~|A", Shape(s));
    }

    [Fact]
    public void KeepActiveNeverReplacesTheActivePreview()
    {
        var s = New();
        s.ApplySelectionContext(One("A"));
        Assert.Equal("A~|A", Shape(s));

        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("B"), keepActive: true));

        Assert.Equal("A~|A", Shape(s));
    }

    [Fact]
    public void KeepActiveWithARackWithoutATabAndNoPreviewAddsAPreviewAtTheEndAndKeepsTheActiveTab()
    {
        var s = WithPinnedActive("A");
        Assert.Equal("A+|A", Shape(s));

        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("B"), keepActive: true));

        Assert.Equal("A+,B~|A", Shape(s));
    }

    [Fact]
    public void KeepActiveWithoutAnActiveTabCreatesAndActivatesAsBefore()
    {
        var s = New();

        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("A"), keepActive: true));

        Assert.Equal("A~|A", Shape(s));
    }

    [Fact]
    public void WithoutKeepActiveTheSelectionStillActivatesAndCreatesAsBefore()
    {
        var s = WithPinnedActive("A");
        s.ApplySelectionContext(One("B"));
        s.Pin("B");
        s.Activate("A");
        // The context of B was consumed when its tab was created: another context in between makes the next one a real transition.
        s.ApplySelectionContext(SelectionContext.None);

        Assert.Equal(BrowserChange.Activated, s.ApplySelectionContext(One("B")));
        Assert.Equal("A+,B+|B", Shape(s));
        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("C"), keepActive: false));
        Assert.Equal("A+,B+,C~|C", Shape(s));
    }

    [Fact]
    public void TheContextThatWasRetainedIsConsumedAndPresentingItAgainDoesNotOpenIt()
    {
        var s = WithPinnedActive("A");
        Assert.Equal(BrowserChange.Created, s.ApplySelectionContext(One("B"), keepActive: true));

        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("B")));
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("B"), keepActive: true));
        Assert.Equal("A+,B~|A", Shape(s));

        // A real transition (another context in between) applies again.
        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(SelectionContext.None));
        Assert.Equal(BrowserChange.Activated, s.ApplySelectionContext(One("B")));
        Assert.Equal("A+,B~|B", Shape(s));
    }

    [Fact]
    public void KeepActiveWithAContextThatIsNotOneRackChangesNothing()
    {
        var s = WithPinnedActive("A");

        foreach (var context in new[] { SelectionContext.None, SelectionContext.Selection(2, 0), SelectionContext.NoIdentity, SelectionContext.Diagnostic })
            Assert.Equal(BrowserChange.None, s.ApplySelectionContext(context, keepActive: true));

        Assert.Equal("A+|A", Shape(s));
    }

    [Fact]
    public void KeepActiveOnARetiredSessionDoesNothing()
    {
        var registry = new BrowserSessionRegistry();
        var s = registry.GetOrCreate(new SessionId(5))!;
        s.ApplySelectionContext(One("A"));
        registry.Destroy(new SessionId(5));

        Assert.Equal(BrowserChange.None, s.ApplySelectionContext(One("B"), keepActive: true));
        Assert.Equal("|-", Shape(s));
    }
}
