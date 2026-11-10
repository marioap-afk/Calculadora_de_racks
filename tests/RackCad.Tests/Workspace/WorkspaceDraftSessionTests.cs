#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4-T1-DRAFT-CORE: the editable state used by the tests. It is a stand-in for the state that already exists in
/// Application: the core is generic over it. <see cref="Source"/> plays the role of the raw JSON the state came from.
/// </summary>
internal sealed record DraftForm(string Name, int Width, string Source = "");

/// <summary>Equality of the materialized state: the origin text never counts (INV-26).</summary>
internal sealed class DraftFormComparer : IEqualityComparer<DraftForm>
{
    public int Comparisons { get; private set; }

    public bool Equals(DraftForm? x, DraftForm? y)
    {
        Comparisons++;
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        return string.Equals(x.Name, y.Name, StringComparison.Ordinal) && x.Width == y.Width;
    }

    public int GetHashCode(DraftForm obj) => HashCode.Combine(obj.Name, obj.Width);
}

internal sealed class DraftAlwaysEqual : IEqualityComparer<DraftForm>
{
    public bool Equals(DraftForm? x, DraftForm? y) => true;

    public int GetHashCode(DraftForm obj) => 0;
}

internal sealed class DraftNeverEqual : IEqualityComparer<DraftForm>
{
    public bool Equals(DraftForm? x, DraftForm? y) => false;

    public int GetHashCode(DraftForm obj) => 0;
}

/// <summary>The validator and the one-step application the adapter of a system would contribute. It records every call.</summary>
internal sealed class DraftFormAdapter : IDraftPendingTextAdapter<DraftForm>
{
    public List<string> Validated { get; } = new();

    public int ApplyCalls { get; private set; }

    public DraftFieldValidation Validate(string fieldKey, string text)
    {
        Validated.Add(fieldKey);
        if (fieldKey == "name") return string.IsNullOrWhiteSpace(text) ? DraftFieldValidation.Invalid("name is empty") : DraftFieldValidation.Valid;
        if (fieldKey == "width") return int.TryParse(text, out var w) && w > 0 ? DraftFieldValidation.Valid : DraftFieldValidation.Invalid("width is not a positive integer");
        return DraftFieldValidation.Valid;
    }

    public DraftForm Apply(DraftForm current, IReadOnlyDictionary<string, string> pendingText)
    {
        ApplyCalls++;
        var name = pendingText.TryGetValue("name", out var n) ? n : current.Name;
        var width = pendingText.TryGetValue("width", out var w) ? int.Parse(w) : current.Width;
        return current with { Name = name, Width = width };
    }
}

/// <summary>Builders shared by the five draft test classes. Failures here are always assertions.</summary>
internal static class DraftKit
{
    public static readonly SessionId S1 = new(1);
    public static readonly SessionId S2 = new(2);
    public static readonly DraftFormComparer Comparer = new();

    public static DraftStaleBaseEntry Mutable(string key, string? view = "Front", string? section = "S1", string? text = "{\"v\":1}") =>
        new(key, DraftMembership.Mutable, view, section, text);

    public static DraftStaleBaseEntry ReadOnly(string key, string? view = "Front", string? section = "S1", string? text = "{\"v\":1}") =>
        new(key, DraftMembership.ReadOnly, view, section, text);

    public static DraftStaleBaseEntry Blocking(string key) =>
        new(key, DraftMembership.Blocking, null, null, null);

    public static DraftStaleBaseEntry NotMember(string key, string? text = "{\"other\":1}") =>
        new(key, DraftMembership.NotMember, "Other", "Z", text);

    public static DraftStaleBase Base(params DraftStaleBaseEntry[] entries) => DraftStaleBase.Create(entries);

    public static DraftStaleBase DefaultBase() => Base(Mutable("D1"), Mutable("D2", "Back", "S2", "{\"v\":2}"));

    public static DraftForm Form(string name = "Rack", int width = 30, string source = "{a}") => new(name, width, source);

    public static DraftIntentRead<DraftForm> Read(DraftForm? form = null, DraftStaleBase? staleBase = null) =>
        DraftIntentRead<DraftForm>.Admitted(staleBase ?? DefaultBase(), form ?? Form());

    public static DraftSession<DraftForm> Born(DraftRegistry? registry = null, SessionId? session = null, string rack = "R1", DraftForm? initial = null, DraftStaleBase? staleBase = null, IEqualityComparer<DraftForm>? comparer = null)
    {
        registry ??= new DraftRegistry();
        var result = registry.Begin(session ?? S1, rack, Read(initial, staleBase), comparer ?? Comparer);
        Assert.Equal(DraftBeginStatus.Created, result.Status);
        Assert.NotNull(result.Draft);
        return result.Draft!;
    }

    public static DraftSession<DraftForm> Dirty(DraftRegistry? registry = null, SessionId? session = null, string rack = "R1")
    {
        var draft = Born(registry, session, rack);
        Assert.Equal(DraftEditStatus.Updated, draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "Edited" }).Status);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        return draft;
    }

    public static IReadOnlyList<DraftDifference> OneDifference() =>
        new[] { new DraftDifference("D1", DraftDifferenceKind.EnvelopeText) };

    public static DraftSession<DraftForm> ToApplying(DraftSession<DraftForm> draft)
    {
        var start = draft.TryBeginApply();
        Assert.True(start.IsStarted);
        Assert.Equal(DraftStatus.Applying, draft.Status);
        return draft;
    }

    public static DraftSession<DraftForm> ToConflict(DraftSession<DraftForm> draft)
    {
        ToApplying(draft);
        draft.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedConflict(OneDifference()));
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        return draft;
    }

    public static DraftSession<DraftForm> ToUnknownCommit(DraftSession<DraftForm> draft)
    {
        ToApplying(draft);
        draft.CompleteApply(DraftApplyOutcome<DraftForm>.CommitThrew("commit threw"));
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
        return draft;
    }

    public static DraftSession<DraftForm> ToUnknownPost(DraftSession<DraftForm> draft)
    {
        ToApplying(draft);
        draft.CompleteApply(DraftApplyOutcome<DraftForm>.AppliedPostRereadFailed("post reread failed"));
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);
        return draft;
    }

    public static DraftSession<DraftForm> ToError(DraftSession<DraftForm> draft)
    {
        draft.EnterError("surface failed");
        Assert.Equal(DraftStatus.Error, draft.Status);
        return draft;
    }
}

// I-64 F4-T1-DRAFT-CORE: INV-F4T1-01, 02, 04, 05, 08, 11, 14, 15 over the generic draft session.
public class WorkspaceDraftSessionTests
{
    // ------------------------------------------------------------------ birth and dirty (INV-F4T1-01, 02, 03)

    [Fact]
    public void ABornDraftIsCleanAndItsCurrentStateStartsEqualToTheInitialOne()
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30));

        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.False(draft.IsDirty);
        Assert.Same(draft.InitialDraftState, draft.CurrentDraftState);
        Assert.Empty(draft.PendingText);
        Assert.False(draft.HasPendingText);
        Assert.False(draft.IsPossiblyStale);
        Assert.True(draft.IsRegistered);
        Assert.Equal(DraftKit.S1, draft.Session);
        Assert.Equal("R1", draft.RackId);
    }

    [Theory]
    [InlineData("Other", 30)]
    [InlineData("Rack", 31)]
    [InlineData("Other", 31)]
    public void TheDraftIsDirtyWhenTheComparerSeesADifferenceBetweenCurrentAndInitial(string name, int width)
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30));

        var edit = draft.SetCurrentDraftState(DraftKit.Form(name, width));

        Assert.Equal(DraftEditStatus.Updated, edit.Status);
        Assert.True(edit.IsDirty);
        Assert.True(draft.IsDirty);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void TwoStatesTheComparerCallsEqualAreCleanEvenWhenTheirSourceJsonDiffers()
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30, "{\"a\":1,\"b\":2}"));

        var edit = draft.SetCurrentDraftState(DraftKit.Form("Rack", 30, "{ \"b\": 2, \"a\": 1 }"));

        Assert.Equal(DraftEditStatus.Updated, edit.Status);
        Assert.NotEqual(draft.InitialDraftState.Source, draft.CurrentDraftState.Source);
        Assert.False(draft.IsDirty);
        Assert.Equal(DraftStatus.Clean, draft.Status);
    }

    [Fact]
    public void ReturningToTheInitialStateMakesTheDraftCleanAgain()
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30));
        draft.SetCurrentDraftState(DraftKit.Form("Edited", 30));
        Assert.Equal(DraftStatus.Dirty, draft.Status);

        draft.SetCurrentDraftState(DraftKit.Form("Rack", 30, "{other source}"));

        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.False(draft.IsDirty);
    }

    [Fact]
    public void PendingTextMakesTheDraftDirtyEvenWhenBothStatesAreEqual()
    {
        var draft = DraftKit.Born();
        Assert.False(draft.IsDirty);

        var set = draft.SetPendingText("width", "4x");

        Assert.Equal(DraftEditStatus.Updated, set.Status);
        Assert.True(set.IsDirty);
        Assert.True(draft.HasPendingText);
        Assert.True(draft.IsDirty);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Equal(draft.InitialDraftState, draft.CurrentDraftState);

        var removed = draft.RemovePendingText("width");

        Assert.False(removed.IsDirty);
        Assert.Equal(DraftStatus.Clean, draft.Status);
    }

    [Theory]
    [InlineData("  12 ")]
    [InlineData("")]
    [InlineData("abc\r\n")]
    [InlineData("１２")]
    public void PendingTextIsKeptExactlyAsGivenPerFieldKey(string text)
    {
        var draft = DraftKit.Born();

        draft.SetPendingText("width", text);
        draft.SetPendingText("name", "  keep me  ");

        Assert.Equal(text, draft.PendingText["width"]);
        Assert.Equal("  keep me  ", draft.PendingText["name"]);
        Assert.Equal(new[] { "name", "width" }, draft.PendingText.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());
        Assert.True(draft.IsDirty);
    }

    [Fact]
    public void SettingTheSameFieldAgainReplacesItsTextWithoutAddingAKey()
    {
        var draft = DraftKit.Born();

        draft.SetPendingText("width", "1");
        draft.SetPendingText("width", "2");

        Assert.Single(draft.PendingText);
        Assert.Equal("2", draft.PendingText["width"]);
    }

    [Fact]
    public void DirtyIsDecidedByTheInjectedComparerAndNotByTheStateItself()
    {
        var always = DraftKit.Born(initial: DraftKit.Form("Rack", 30), comparer: new DraftAlwaysEqual());
        always.SetCurrentDraftState(DraftKit.Form("Completely different", 99));
        Assert.False(always.IsDirty);
        always.SetPendingText("name", "x");
        Assert.True(always.IsDirty);

        var never = DraftKit.Born(initial: DraftKit.Form("Rack", 30), comparer: new DraftNeverEqual());
        Assert.True(never.IsDirty);

        var spy = new DraftFormComparer();
        var observed = DraftKit.Born(comparer: spy);
        var before = spy.Comparisons;
        _ = observed.IsDirty;
        Assert.True(spy.Comparisons > before);
    }

    [Fact]
    public void ADraftWithPendingTextNeverNeedsTheComparerToBeDirty()
    {
        var spy = new DraftFormComparer();
        var draft = DraftKit.Born(comparer: spy);
        draft.SetPendingText("name", "x");
        var before = spy.Comparisons;

        Assert.True(draft.IsDirty);
        Assert.Equal(before, spy.Comparisons);
    }

    [Theory]
    [InlineData("applying")]
    [InlineData("unknown")]
    [InlineData("error")]
    public void EditsAreRefusedWhileTheDraftIsApplyingUnknownOrInError(string status)
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("name", "keep");
        if (status == "error") DraftKit.ToError(draft);
        else
        {
            draft.RemovePendingText("name");
            if (status == "applying") DraftKit.ToApplying(draft);
            else DraftKit.ToUnknownCommit(draft);
        }

        var before = draft.CurrentDraftState;
        var edit = draft.SetCurrentDraftState(DraftKit.Form("Nope", 1));
        var text = draft.SetPendingText("width", "7");
        var remove = draft.RemovePendingText("name");

        Assert.Equal(DraftEditStatus.NotEditable, edit.Status);
        Assert.Equal(DraftEditStatus.NotEditable, text.Status);
        Assert.Equal(DraftEditStatus.NotEditable, remove.Status);
        Assert.Same(before, draft.CurrentDraftState);
        Assert.False(draft.PendingText.ContainsKey("width"));
    }

    // ------------------------------------------------------------------ boundaries (INV-F4T1-05)

    [Theory]
    [InlineData(DraftBoundary.TabChange, true)]
    [InlineData(DraftBoundary.RackChange, true)]
    [InlineData(DraftBoundary.Update, true)]
    [InlineData(DraftBoundary.DocumentChange, false)]
    [InlineData(DraftBoundary.HidePanel, false)]
    [InlineData(DraftBoundary.Dematerialize, false)]
    [InlineData(DraftBoundary.DocumentClose, false)]
    public void OnlyTabChangeRackChangeAndUpdateAreBlockingBoundaries(DraftBoundary boundary, bool blocking)
    {
        Assert.Equal(blocking, DraftBoundaries.IsBlocking(boundary));
        Assert.Equal(7, Enum.GetValues<DraftBoundary>().Length);
    }

    [Theory]
    [InlineData(DraftBoundary.TabChange)]
    [InlineData(DraftBoundary.RackChange)]
    [InlineData(DraftBoundary.Update)]
    public void ABlockingBoundaryWithAllTheTextValidAppliesEverythingInOneStepAndEmptiesThePending(DraftBoundary boundary)
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30));
        draft.SetPendingText("name", "Renamed");
        draft.SetPendingText("width", "45");
        var adapter = new DraftFormAdapter();

        var result = draft.CrossBoundary(boundary, adapter);

        Assert.Equal(DraftBoundaryDecision.Proceeded, result.Decision);
        Assert.True(result.CurrentStateChanged);
        Assert.Empty(result.InvalidFields);
        Assert.Equal(1, adapter.ApplyCalls);
        Assert.Equal("Renamed", draft.CurrentDraftState.Name);
        Assert.Equal(45, draft.CurrentDraftState.Width);
        Assert.Empty(draft.PendingText);
        Assert.False(draft.HasPendingText);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Theory]
    [InlineData(DraftBoundary.TabChange)]
    [InlineData(DraftBoundary.RackChange)]
    [InlineData(DraftBoundary.Update)]
    public void ABlockingBoundaryWithAnInvalidFieldAbortsAndLeavesStateAndTextExactlyAsTheyWere(DraftBoundary boundary)
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30));
        draft.SetPendingText("name", "Renamed");
        draft.SetPendingText("width", "abc");
        var currentBefore = draft.CurrentDraftState;
        var adapter = new DraftFormAdapter();

        var result = draft.CrossBoundary(boundary, adapter);

        Assert.Equal(DraftBoundaryDecision.Aborted, result.Decision);
        Assert.False(result.CurrentStateChanged);
        Assert.Equal(new[] { "width" }, result.InvalidFields.Select(f => f.FieldKey).ToArray());
        Assert.False(string.IsNullOrEmpty(result.InvalidFields[0].Message));
        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Same(currentBefore, draft.CurrentDraftState);
        Assert.Equal("Renamed", draft.PendingText["name"]);
        Assert.Equal("abc", draft.PendingText["width"]);
        Assert.Equal(2, draft.PendingText.Count);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void TheFirstPhaseValidatesEveryPendingFieldBeforeAnythingIsApplied()
    {
        var draft = DraftKit.Born();
        draft.SetPendingText("name", " ");
        draft.SetPendingText("width", "0");
        draft.SetPendingText("note", "free");
        var adapter = new DraftFormAdapter();

        var result = draft.CrossBoundary(DraftBoundary.TabChange, adapter);

        Assert.Equal(DraftBoundaryDecision.Aborted, result.Decision);
        Assert.Equal(new[] { "name", "note", "width" }, adapter.Validated.OrderBy(k => k, StringComparer.Ordinal).ToArray());
        Assert.Equal(new[] { "name", "width" }, result.InvalidFields.Select(f => f.FieldKey).OrderBy(k => k, StringComparer.Ordinal).ToArray());
        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Equal(3, draft.PendingText.Count);
    }

    [Theory]
    [InlineData(DraftBoundary.DocumentChange)]
    [InlineData(DraftBoundary.HidePanel)]
    [InlineData(DraftBoundary.Dematerialize)]
    [InlineData(DraftBoundary.DocumentClose)]
    public void ANonBlockingBoundaryKeepsInvalidTextAsItIsWithoutCommittingOrRepairing(DraftBoundary boundary)
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30));
        draft.SetPendingText("width", "abc");
        var currentBefore = draft.CurrentDraftState;
        var adapter = new DraftFormAdapter();

        var result = draft.CrossBoundary(boundary, adapter);

        Assert.Equal(DraftBoundaryDecision.Kept, result.Decision);
        Assert.False(result.CurrentStateChanged);
        Assert.Empty(adapter.Validated);
        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Equal("abc", draft.PendingText["width"]);
        Assert.Same(currentBefore, draft.CurrentDraftState);
        Assert.True(draft.IsDirty);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Theory]
    [InlineData(DraftBoundary.DocumentChange)]
    [InlineData(DraftBoundary.HidePanel)]
    [InlineData(DraftBoundary.Dematerialize)]
    [InlineData(DraftBoundary.DocumentClose)]
    public void ANonBlockingBoundaryDoesNotCommitTextThatWouldBeValidEither(DraftBoundary boundary)
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30));
        draft.SetPendingText("width", "55");
        var adapter = new DraftFormAdapter();

        var result = draft.CrossBoundary(boundary, adapter);

        Assert.Equal(DraftBoundaryDecision.Kept, result.Decision);
        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Equal(30, draft.CurrentDraftState.Width);
        Assert.Equal("55", draft.PendingText["width"]);
    }

    [Fact]
    public void ABlockingBoundaryWithNothingPendingProceedsWithoutChangingAnything()
    {
        var draft = DraftKit.Born();
        var adapter = new DraftFormAdapter();

        var result = draft.CrossBoundary(DraftBoundary.TabChange, adapter);

        Assert.Equal(DraftBoundaryDecision.Proceeded, result.Decision);
        Assert.False(result.CurrentStateChanged);
        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Empty(adapter.Validated);
        Assert.Equal(DraftStatus.Clean, draft.Status);
    }

    [Fact]
    public void ABlockingBoundaryCannotCommitInAStatusThatDoesNotAdmitEditsSoTheTextStays()
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("width", "55");
        DraftKit.ToError(draft);
        var adapter = new DraftFormAdapter();

        var result = draft.CrossBoundary(DraftBoundary.TabChange, adapter);

        Assert.Equal(DraftBoundaryDecision.Kept, result.Decision);
        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Equal("55", draft.PendingText["width"]);
        Assert.Equal(DraftStatus.Error, draft.Status);
    }

    [Fact]
    public void TheUpdateBoundaryCommitsThePendingTextAndOnlyThenAnUpdateMayBegin()
    {
        var draft = DraftKit.Born();
        draft.SetPendingText("width", "60");

        var refused = draft.TryBeginApply();
        Assert.False(refused.IsStarted);
        Assert.Equal(DraftApplyRefusal.PendingText, refused.Refusal);

        draft.CrossBoundary(DraftBoundary.Update, new DraftFormAdapter());
        var started = draft.TryBeginApply();

        Assert.True(started.IsStarted);
        Assert.Equal(DraftStatus.Dirty, started.PreviousStatus);
        Assert.Equal(60, draft.CurrentDraftState.Width);
    }

    [Fact]
    public void AnInvalidFieldAtTheUpdateBoundaryKeepsTheDraftFromBeginningAnUpdate()
    {
        var draft = DraftKit.Born();
        draft.SetPendingText("width", "sixty");

        var boundary = draft.CrossBoundary(DraftBoundary.Update, new DraftFormAdapter());
        var start = draft.TryBeginApply();

        Assert.Equal(DraftBoundaryDecision.Aborted, boundary.Decision);
        Assert.False(start.IsStarted);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Equal("sixty", draft.PendingText["width"]);
    }

    // ------------------------------------------------------------------ possibly stale (INV-F4T1-08)

    [Fact]
    public void MarkingPossiblyStaleChangesNeitherTheStateNorDirtyNorThePendingText()
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("width", "9x");
        var initial = draft.InitialDraftState;
        var current = draft.CurrentDraftState;
        var staleBase = draft.StaleBase;

        var mark = draft.MarkPossiblyStale();

        Assert.Equal(DraftMarkResult.Marked, mark);
        Assert.True(draft.IsPossiblyStale);
        Assert.Same(initial, draft.InitialDraftState);
        Assert.Same(current, draft.CurrentDraftState);
        Assert.Same(staleBase, draft.StaleBase);
        Assert.Equal("9x", draft.PendingText["width"]);
        Assert.True(draft.IsDirty);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void MarkingIsIdempotentAndSaysSo()
    {
        var draft = DraftKit.Born();

        Assert.Equal(DraftMarkResult.Marked, draft.MarkPossiblyStale());
        Assert.Equal(DraftMarkResult.AlreadyMarked, draft.MarkPossiblyStale());
        Assert.True(draft.IsPossiblyStale);
    }

    [Fact]
    public void ACleanMarkedDraftNeedsALazyReloadAndAnUnmarkedOneDoesNot()
    {
        var unmarked = DraftKit.Born();
        var marked = DraftKit.Born(rack: "R2");
        marked.MarkPossiblyStale();

        Assert.False(unmarked.NeedsLazyReload);
        Assert.True(marked.NeedsLazyReload);
        Assert.Equal(DraftStatus.Clean, marked.Status);
    }

    [Fact]
    public void ADirtyMarkedDraftStaysDirtyAndMarkedAndNeverNeedsALazyReload()
    {
        var draft = DraftKit.Dirty();
        draft.MarkPossiblyStale();

        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.True(draft.IsPossiblyStale);
        Assert.False(draft.NeedsLazyReload);
    }

    [Theory]
    [InlineData("applying")]
    [InlineData("conflict")]
    [InlineData("unknown-commit")]
    [InlineData("unknown-post")]
    [InlineData("error")]
    public void AMarkedDraftNeverNeedsALazyReloadWhileItIsApplyingInConflictUnknownOrInError(string status)
    {
        var draft = DraftKit.Born();
        draft.MarkPossiblyStale();
        switch (status)
        {
            case "applying": DraftKit.ToApplying(draft); break;
            case "conflict": DraftKit.ToConflict(draft); break;
            case "unknown-commit": DraftKit.ToUnknownCommit(draft); break;
            case "unknown-post": DraftKit.ToUnknownPost(draft); break;
            default: DraftKit.ToError(draft); break;
        }

        Assert.True(draft.IsPossiblyStale);
        Assert.False(draft.NeedsLazyReload);
    }

    [Fact]
    public void EveryReloadClearsThePossiblyStaleMark()
    {
        var discard = DraftKit.Dirty(rack: "A");
        discard.MarkPossiblyStale();
        discard.DiscardAndReload(DraftKit.Read());
        Assert.False(discard.IsPossiblyStale);

        var keep = DraftKit.Dirty(rack: "B");
        keep.MarkPossiblyStale();
        keep.KeepMineOnCurrentVersion(DraftKit.Read(), confirmed: true);
        Assert.False(keep.IsPossiblyStale);

        var applied = DraftKit.Dirty(rack: "C");
        applied.MarkPossiblyStale();
        DraftKit.ToApplying(applied);
        applied.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(DraftKit.Read()));
        Assert.False(applied.IsPossiblyStale);
    }

    // ------------------------------------------------------------------ reloads and reconciliation (INV-F4T1-04, 11)

    [Fact]
    public void DiscardAndReloadReplacesTheBaseTheInitialAndTheCurrentStateFromOneReadAndEmptiesThePending()
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("width", "9x");
        var newBase = DraftKit.Base(DraftKit.Mutable("D9", "Top", "S9", "{\"new\":1}"));
        var read = DraftKit.Read(DraftKit.Form("Fresh", 77, "{fresh}"), newBase);

        var result = draft.DiscardAndReload(read);

        Assert.Equal(DraftReloadStatus.Reloaded, result.Status);
        Assert.Same(newBase, draft.StaleBase);
        Assert.Same(read.InitialState, draft.InitialDraftState);
        Assert.Same(read.InitialState, draft.CurrentDraftState);
        Assert.Empty(draft.PendingText);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.False(draft.IsDirty);
    }

    [Theory]
    [InlineData("dirty")]
    [InlineData("conflict")]
    [InlineData("unknown-commit")]
    [InlineData("unknown-post")]
    [InlineData("error")]
    public void DiscardAndReloadLeavesEveryStatusAndGivesACleanDraft(string status)
    {
        var draft = DraftKit.Dirty();
        switch (status)
        {
            case "conflict": DraftKit.ToConflict(draft); break;
            case "unknown-commit": DraftKit.ToUnknownCommit(draft); break;
            case "unknown-post": DraftKit.ToUnknownPost(draft); break;
            case "error": DraftKit.ToError(draft); break;
        }

        var result = draft.DiscardAndReload(DraftKit.Read(DraftKit.Form("Fresh", 10)));

        Assert.Equal(DraftReloadStatus.Reloaded, result.Status);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Null(draft.UnknownOrigin);
        Assert.Empty(draft.ConflictDifferences);
        Assert.Null(draft.ErrorReason);
        Assert.Equal("Fresh", draft.CurrentDraftState.Name);
    }

    [Fact]
    public void DiscardAndReloadIsNotAllowedWhileTheDraftIsApplying()
    {
        var draft = DraftKit.ToApplying(DraftKit.Dirty());
        var current = draft.CurrentDraftState;

        var result = draft.DiscardAndReload(DraftKit.Read(DraftKit.Form("Fresh", 10)));

        Assert.Equal(DraftReloadStatus.NotAllowed, result.Status);
        Assert.Equal(DraftStatus.Applying, draft.Status);
        Assert.Same(current, draft.CurrentDraftState);
    }

    [Fact]
    public void ANotAdmittedReadChangesNothingAndReturnsItsDiagnosticAsAValue()
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("width", "9x");
        var staleBase = draft.StaleBase;
        var initial = draft.InitialDraftState;
        var current = draft.CurrentDraftState;
        var rejected = DraftIntentRead<DraftForm>.NotAdmitted("divergent members");

        var discard = draft.DiscardAndReload(rejected);
        var keep = draft.KeepMineOnCurrentVersion(rejected, confirmed: true);

        Assert.Equal(DraftReloadStatus.NotAdmitted, discard.Status);
        Assert.Equal("divergent members", discard.Admission.Diagnostic);
        Assert.False(discard.Admission.IsAdmitted);
        Assert.Equal(DraftReloadStatus.NotAdmitted, keep.Status);
        Assert.Equal("divergent members", keep.Admission.Diagnostic);
        Assert.Same(staleBase, draft.StaleBase);
        Assert.Same(initial, draft.InitialDraftState);
        Assert.Same(current, draft.CurrentDraftState);
        Assert.Equal("9x", draft.PendingText["width"]);
    }

    [Fact]
    public void KeepMineWithoutConfirmationChangesNothing()
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("width", "9x");
        var staleBase = draft.StaleBase;
        var initial = draft.InitialDraftState;
        var current = draft.CurrentDraftState;

        var result = draft.KeepMineOnCurrentVersion(DraftKit.Read(DraftKit.Form("Theirs", 80)), confirmed: false);

        Assert.Equal(DraftReloadStatus.NotConfirmed, result.Status);
        Assert.Same(staleBase, draft.StaleBase);
        Assert.Same(initial, draft.InitialDraftState);
        Assert.Same(current, draft.CurrentDraftState);
        Assert.Equal("9x", draft.PendingText["width"]);
    }

    [Fact]
    public void KeepMineConfirmedReplacesTheBaseAndTheInitialStateAndKeepsTheCurrentStateAndThePendingText()
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("width", "9x");
        var current = draft.CurrentDraftState;
        var newBase = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{\"changed\":1}"));
        var read = DraftKit.Read(DraftKit.Form("Theirs", 80), newBase);

        var result = draft.KeepMineOnCurrentVersion(read, confirmed: true);

        Assert.Equal(DraftReloadStatus.Reloaded, result.Status);
        Assert.Same(newBase, draft.StaleBase);
        Assert.Same(read.InitialState, draft.InitialDraftState);
        Assert.Same(current, draft.CurrentDraftState);
        Assert.Equal("9x", draft.PendingText["width"]);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void KeepMineNeverMergesTheTwoVersions()
    {
        var draft = DraftKit.Born(initial: DraftKit.Form("Rack", 30));
        draft.SetCurrentDraftState(DraftKit.Form("Mine", 30));
        var theirs = DraftKit.Form("Rack", 99);

        draft.KeepMineOnCurrentVersion(DraftKit.Read(theirs), confirmed: true);

        Assert.Equal("Mine", draft.CurrentDraftState.Name);
        Assert.Equal(30, draft.CurrentDraftState.Width);
        Assert.Equal(99, draft.InitialDraftState.Width);
    }

    [Theory]
    [InlineData("clean")]
    [InlineData("unknown-commit")]
    [InlineData("unknown-post")]
    [InlineData("error")]
    [InlineData("applying")]
    public void KeepMineIsOnlyServedForADirtyOrConflictingDraft(string status)
    {
        var draft = DraftKit.Born();
        switch (status)
        {
            case "unknown-commit": DraftKit.ToUnknownCommit(draft); break;
            case "unknown-post": DraftKit.ToUnknownPost(draft); break;
            case "error": DraftKit.ToError(draft); break;
            case "applying": DraftKit.ToApplying(draft); break;
        }

        var before = draft.Status;
        var result = draft.KeepMineOnCurrentVersion(DraftKit.Read(DraftKit.Form("Theirs", 80)), confirmed: true);

        Assert.Equal(DraftReloadStatus.NotAllowed, result.Status);
        Assert.Equal(before, draft.Status);
    }

    [Fact]
    public void AConflictKeepsTheDraftAndOnlyLeavesThroughDiscardAndReloadOrAConfirmedKeepMine()
    {
        var draft = DraftKit.ToConflict(DraftKit.Dirty());
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        var current = draft.CurrentDraftState;

        draft.MarkPossiblyStale();
        draft.SetCurrentDraftState(draft.CurrentDraftState with { Width = 41 });
        var check = draft.CheckNow(DraftKit.DefaultBase());
        var begin = draft.TryBeginApply();
        var unconfirmed = draft.KeepMineOnCurrentVersion(DraftKit.Read(), confirmed: false);
        var boundary = draft.CrossBoundary(DraftBoundary.TabChange, new DraftFormAdapter());

        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Equal(DraftCheckStatus.NotApplicable, check.Status);
        Assert.False(begin.IsStarted);
        Assert.Equal(DraftReloadStatus.NotConfirmed, unconfirmed.Status);
        Assert.Equal(DraftBoundaryDecision.Proceeded, boundary.Decision);
        Assert.NotSame(current, draft.CurrentDraftState);
        Assert.Equal(41, draft.CurrentDraftState.Width);
        Assert.Single(draft.ConflictDifferences);

        var keep = draft.KeepMineOnCurrentVersion(DraftKit.Read(DraftKit.Form("Theirs", 80)), confirmed: true);

        Assert.Equal(DraftReloadStatus.Reloaded, keep.Status);
        Assert.NotEqual(DraftStatus.Conflict, draft.Status);
        Assert.Empty(draft.ConflictDifferences);
        Assert.Equal(41, draft.CurrentDraftState.Width);

        var other = DraftKit.ToConflict(DraftKit.Dirty(rack: "R2"));
        var discard = other.DiscardAndReload(DraftKit.Read(DraftKit.Form("Theirs", 80)));
        Assert.Equal(DraftReloadStatus.Reloaded, discard.Status);
        Assert.Equal(DraftStatus.Clean, other.Status);
    }

    // ------------------------------------------------------------------ error (INV-F4T1-14)

    [Fact]
    public void EnteringErrorKeepsTheCurrentStateThePendingTextAndTheStaleBase()
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("width", "9x");
        var staleBase = draft.StaleBase;
        var initial = draft.InitialDraftState;
        var current = draft.CurrentDraftState;

        var transition = draft.EnterError("surface threw");

        Assert.Equal(DraftStatus.Dirty, transition.From);
        Assert.Equal(DraftStatus.Error, transition.To);
        Assert.True(transition.Changed);
        Assert.Equal(DraftStatus.Error, draft.Status);
        Assert.Equal("surface threw", draft.ErrorReason);
        Assert.Same(staleBase, draft.StaleBase);
        Assert.Same(initial, draft.InitialDraftState);
        Assert.Same(current, draft.CurrentDraftState);
        Assert.Equal("9x", draft.PendingText["width"]);
        Assert.True(draft.IsDirty);
    }

    [Fact]
    public void ADraftInErrorLeavesOnlyThroughDiscardAndReload()
    {
        var draft = DraftKit.Dirty();
        DraftKit.ToError(draft);

        Assert.False(draft.TryBeginApply().IsStarted);
        Assert.Equal(DraftReloadStatus.NotAllowed, draft.KeepMineOnCurrentVersion(DraftKit.Read(), confirmed: true).Status);
        Assert.Equal(DraftCheckStatus.NotApplicable, draft.CheckNow(DraftKit.DefaultBase()).Status);
        Assert.Equal(DraftStatus.Error, draft.Status);

        var reload = draft.DiscardAndReload(DraftKit.Read());

        Assert.Equal(DraftReloadStatus.Reloaded, reload.Status);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Null(draft.ErrorReason);
    }

    [Fact]
    public void ADraftInErrorLeavesThroughDiscardToo()
    {
        var registry = new DraftRegistry();
        var draft = DraftKit.Dirty(registry);
        DraftKit.ToError(draft);

        var discard = registry.Discard(DraftKit.S1, "R1");

        Assert.True(discard.Removed);
        Assert.False(registry.Contains(DraftKit.S1, "R1"));
    }

    [Fact]
    public void EnteringErrorFromACleanDraftReportsTheTransition()
    {
        var draft = DraftKit.Born();

        var transition = draft.EnterError("boom");

        Assert.Equal(DraftStatus.Clean, transition.From);
        Assert.Equal(DraftStatus.Error, transition.To);
        Assert.Equal(DraftStatus.Error, draft.Status);
    }

    // ------------------------------------------------------------------ programming errors and values (INV-F4T1-15)

    [Fact]
    public void OnlyANullOrEmptyArgumentIsAProgrammingError()
    {
        var draft = DraftKit.Born();
        var adapter = new DraftFormAdapter();

        Assert.Throws<ArgumentException>(() => draft.SetPendingText("", "x"));
        Assert.Throws<ArgumentException>(() => draft.SetPendingText(null!, "x"));
        Assert.Throws<ArgumentNullException>(() => draft.SetPendingText("name", null!));
        Assert.Throws<ArgumentException>(() => draft.RemovePendingText(""));
        Assert.Throws<ArgumentException>(() => draft.EnterError(""));
        Assert.Throws<ArgumentException>(() => draft.EnterError(null!));
        Assert.Throws<ArgumentNullException>(() => draft.CrossBoundary(DraftBoundary.TabChange, null!));
        Assert.Throws<ArgumentNullException>(() => draft.DiscardAndReload(null!));
        Assert.Throws<ArgumentNullException>(() => draft.KeepMineOnCurrentVersion(null!, true));
        Assert.Throws<ArgumentNullException>(() => draft.CheckNow(null!));
        Assert.Throws<ArgumentNullException>(() => draft.CompleteApply(null!));
        Assert.Throws<ArgumentException>(() => DraftIntentRead<DraftForm>.NotAdmitted(""));
        Assert.Throws<ArgumentNullException>(() => DraftIntentRead<DraftForm>.Admitted(null!, DraftKit.Form()));
        Assert.Equal(0, adapter.ApplyCalls);
    }

    [Fact]
    public void TheNormalFlowReturnsValuesAndNeverThrows()
    {
        var registry = new DraftRegistry();
        var draft = DraftKit.Dirty(registry);
        draft.SetPendingText("width", "abc");
        var adapter = new DraftFormAdapter();

        var exception = Record.Exception(() =>
        {
            draft.CrossBoundary(DraftBoundary.TabChange, adapter);
            draft.CrossBoundary(DraftBoundary.HidePanel, adapter);
            draft.TryBeginApply();
            draft.CheckNow(DraftKit.DefaultBase());
            draft.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedFailure("x"));
            draft.KeepMineOnCurrentVersion(DraftKit.Read(), confirmed: false);
            draft.DiscardAndReload(DraftIntentRead<DraftForm>.NotAdmitted("no"));
            draft.MarkPossiblyStale();
            registry.Discard(DraftKit.S1, "R1");
            draft.SetCurrentDraftState(DraftKit.Form("late", 1));
            draft.TryBeginApply();
            draft.MarkPossiblyStale();
            draft.EnterError("late");
            draft.DiscardAndReload(DraftKit.Read());
        });

        Assert.Null(exception);
        Assert.False(draft.IsRegistered);
    }
}
