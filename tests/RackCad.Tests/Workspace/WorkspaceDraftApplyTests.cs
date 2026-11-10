#nullable enable
using System;
using System.Linq;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T1-DRAFT-CORE: INV-F4T1-09, 10 and 11. Update as a state machine over the D-13 table: the outcomes are values
// the caller reports, nothing is ever assumed applied, and only the re-read of POST leaves a draft Clean.
public class WorkspaceDraftApplyTests
{
    private static DraftSession<DraftForm> Prior(string previous)
    {
        var draft = previous == "dirty" ? DraftKit.Dirty() : DraftKit.Born();
        Assert.Equal(previous == "dirty" ? DraftStatus.Dirty : DraftStatus.Clean, draft.Status);
        return draft;
    }

    private static DraftStatus PriorStatus(string previous) => previous == "dirty" ? DraftStatus.Dirty : DraftStatus.Clean;

    // ------------------------------------------------------------------ beginning an Update

    [Theory]
    [InlineData("clean")]
    [InlineData("dirty")]
    public void AnUpdateBeginsFromCleanOrFromDirtyWithoutPendingTextAndReportsThePreviousStatus(string previous)
    {
        var draft = Prior(previous);

        var start = draft.TryBeginApply();

        Assert.True(start.IsStarted);
        Assert.Equal(PriorStatus(previous), start.PreviousStatus);
        Assert.Equal(DraftApplyRefusal.None, start.Refusal);
        Assert.Equal(DraftStatus.Applying, draft.Status);
    }

    [Fact]
    public void AnUpdateDoesNotBeginFromADirtyDraftWithPendingText()
    {
        var draft = DraftKit.Dirty();
        draft.SetPendingText("width", "4x");

        var start = draft.TryBeginApply();

        Assert.False(start.IsStarted);
        Assert.Equal(DraftApplyRefusal.PendingText, start.Refusal);
        Assert.Equal(DraftStatus.Dirty, start.PreviousStatus);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Equal("4x", draft.PendingText["width"]);
    }

    [Theory]
    [InlineData("conflict")]
    [InlineData("unknown-commit")]
    [InlineData("unknown-post")]
    [InlineData("error")]
    [InlineData("applying")]
    public void AnUpdateDoesNotBeginFromConflictUnknownErrorOrApplying(string status)
    {
        var draft = DraftKit.Dirty();
        switch (status)
        {
            case "conflict": DraftKit.ToConflict(draft); break;
            case "unknown-commit": DraftKit.ToUnknownCommit(draft); break;
            case "unknown-post": DraftKit.ToUnknownPost(draft); break;
            case "error": DraftKit.ToError(draft); break;
            default: DraftKit.ToApplying(draft); break;
        }

        var before = draft.Status;
        var start = draft.TryBeginApply();

        Assert.False(start.IsStarted);
        Assert.Equal(DraftApplyRefusal.InvalidStatus, start.Refusal);
        Assert.Equal(before, start.PreviousStatus);
        Assert.Equal(before, draft.Status);
    }

    // ------------------------------------------------------------------ at most one Applying in the whole registry

    [Fact]
    public void ThereCanBeAtMostOneApplyingDraftInTheWholeRegistryEvenAcrossSessions()
    {
        var registry = new DraftRegistry();
        var first = DraftKit.Born(registry, DraftKit.S1, "R1");
        var sameSession = DraftKit.Born(registry, DraftKit.S1, "R2");
        var otherSession = DraftKit.Born(registry, DraftKit.S2, "R1");

        Assert.True(first.TryBeginApply().IsStarted);
        Assert.True(registry.HasUpdateInProgress);

        var second = sameSession.TryBeginApply();
        var third = otherSession.TryBeginApply();

        Assert.False(second.IsStarted);
        Assert.Equal(DraftApplyRefusal.AnotherUpdateInProgress, second.Refusal);
        Assert.False(third.IsStarted);
        Assert.Equal(DraftApplyRefusal.AnotherUpdateInProgress, third.Refusal);
        Assert.Equal(DraftStatus.Clean, sameSession.Status);
        Assert.Equal(DraftStatus.Clean, otherSession.Status);
    }

    [Fact]
    public void TheApplyingSlotIsFreedWhenTheUpdateEnds()
    {
        var registry = new DraftRegistry();
        var first = DraftKit.Born(registry, DraftKit.S1, "R1");
        var second = DraftKit.Born(registry, DraftKit.S1, "R2");
        first.TryBeginApply();

        first.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedFailure("nothing written"));

        Assert.False(registry.HasUpdateInProgress);
        Assert.True(second.TryBeginApply().IsStarted);
    }

    [Fact]
    public void TheApplyingSlotIsFreedWhenTheApplyingDraftIsDiscardedRetiredOrFailsIntoError()
    {
        var registry = new DraftRegistry();

        var discarded = DraftKit.ToApplying(DraftKit.Born(registry, DraftKit.S1, "R1"));
        registry.Discard(DraftKit.S1, "R1");
        Assert.False(registry.HasUpdateInProgress);

        var retired = DraftKit.ToApplying(DraftKit.Born(registry, DraftKit.S2, "R1"));
        registry.RetireSession(DraftKit.S2);
        Assert.False(registry.HasUpdateInProgress);

        var failing = DraftKit.ToApplying(DraftKit.Born(registry, new SessionId(3), "R1"));
        failing.EnterError("surface threw");
        Assert.False(registry.HasUpdateInProgress);

        Assert.True(DraftKit.Born(registry, new SessionId(4), "R1").TryBeginApply().IsStarted);
        Assert.False(discarded.IsRegistered);
        Assert.False(retired.IsRegistered);
    }

    [Fact]
    public void ADetachedDraftCannotBeginAnUpdateOrCompleteOne()
    {
        var registry = new DraftRegistry();
        var draft = DraftKit.Born(registry);
        registry.Discard(DraftKit.S1, "R1");

        var start = draft.TryBeginApply();
        var complete = draft.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedFailure("x"));

        Assert.False(start.IsStarted);
        Assert.Equal(DraftApplyRefusal.NotRegistered, start.Refusal);
        Assert.False(complete.Accepted);
    }

    [Fact]
    public void CompletingWithoutAnUpdateInProgressIsRefusedAndChangesNothing()
    {
        var draft = DraftKit.Dirty();
        var current = draft.CurrentDraftState;

        var result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(DraftKit.Read(DraftKit.Form("Fresh", 1))));

        Assert.False(result.Accepted);
        Assert.False(result.Rebased);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Same(current, draft.CurrentDraftState);
    }

    // ------------------------------------------------------------------ the D-13 classification by frontier

    [Theory]
    [InlineData("clean")]
    [InlineData("dirty")]
    public void NotAppliedConflictKeepsTheDraftInConflictWithWhatChanged(string previous)
    {
        var draft = Prior(previous);
        var content = (draft.CurrentDraftState, draft.InitialDraftState, draft.StaleBase);
        DraftKit.ToApplying(draft);
        var differences = new[] { new DraftDifference("D2", DraftDifferenceKind.Membership), new DraftDifference(null, DraftDifferenceKind.VariableRegistry) };

        var result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedConflict(differences));

        Assert.True(result.Accepted);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, result.Kind);
        Assert.Equal(DraftStatus.Conflict, result.Status);
        Assert.False(result.Rebased);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Equal(differences, draft.ConflictDifferences.ToArray());
        Assert.Same(content.CurrentDraftState, draft.CurrentDraftState);
        Assert.Same(content.InitialDraftState, draft.InitialDraftState);
        Assert.Same(content.StaleBase, draft.StaleBase);
    }

    [Theory]
    [InlineData("clean", DraftApplyOutcomeKind.NotAppliedAdmission)]
    [InlineData("dirty", DraftApplyOutcomeKind.NotAppliedAdmission)]
    [InlineData("clean", DraftApplyOutcomeKind.NotAppliedPreflight)]
    [InlineData("dirty", DraftApplyOutcomeKind.NotAppliedPreflight)]
    [InlineData("clean", DraftApplyOutcomeKind.NotAppliedFailure)]
    [InlineData("dirty", DraftApplyOutcomeKind.NotAppliedFailure)]
    public void TheOtherNotAppliedOutcomesKeepTheDraftWithItsPreviousStatusAndTheirCause(string previous, DraftApplyOutcomeKind kind)
    {
        var draft = Prior(previous);
        var content = (draft.CurrentDraftState, draft.InitialDraftState, draft.StaleBase);
        DraftKit.ToApplying(draft);
        var outcome = kind switch
        {
            DraftApplyOutcomeKind.NotAppliedAdmission => DraftApplyOutcome<DraftForm>.NotAppliedAdmission("members diverged"),
            DraftApplyOutcomeKind.NotAppliedPreflight => DraftApplyOutcome<DraftForm>.NotAppliedPreflight("locked layer"),
            _ => DraftApplyOutcome<DraftForm>.NotAppliedFailure("builder failed"),
        };

        var result = draft.CompleteApply(outcome);

        Assert.True(result.Accepted);
        Assert.Equal(kind, result.Kind);
        Assert.Equal(PriorStatus(previous), result.Status);
        Assert.Equal(PriorStatus(previous), draft.Status);
        Assert.False(result.Rebased);
        Assert.False(string.IsNullOrEmpty(draft.LastApplyCause));
        Assert.Equal(outcome.Cause, draft.LastApplyCause);
        Assert.Same(content.CurrentDraftState, draft.CurrentDraftState);
        Assert.Same(content.InitialDraftState, draft.InitialDraftState);
        Assert.Same(content.StaleBase, draft.StaleBase);
        Assert.Empty(draft.ConflictDifferences);
    }

    [Theory]
    [InlineData("clean")]
    [InlineData("dirty")]
    public void WhenTheCommitThrewTheDraftIsUnknownWithOriginCommitThrewAndKeepsItsContent(string previous)
    {
        var draft = Prior(previous);
        var content = (draft.CurrentDraftState, draft.InitialDraftState, draft.StaleBase);
        DraftKit.ToApplying(draft);

        var result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.CommitThrew("commit threw"));

        Assert.True(result.Accepted);
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, result.Kind);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
        Assert.False(result.Rebased);
        Assert.Equal("commit threw", draft.LastApplyCause);
        Assert.Same(content.CurrentDraftState, draft.CurrentDraftState);
        Assert.Same(content.InitialDraftState, draft.InitialDraftState);
        Assert.Same(content.StaleBase, draft.StaleBase);
    }

    [Theory]
    [InlineData("clean")]
    [InlineData("dirty")]
    public void AppliedRebasesTheBaseAndTheInitialStateFromThePostReadAndLeavesTheDraftClean(string previous)
    {
        var draft = Prior(previous);
        draft.MarkPossiblyStale();
        DraftKit.ToApplying(draft);
        var postBase = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{rewritten}"));
        var post = DraftKit.Read(DraftKit.Form("AsWritten", 55, "{rewritten}"), postBase);

        var result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(post));

        Assert.True(result.Accepted);
        Assert.Equal(DraftApplyOutcomeKind.Applied, result.Kind);
        Assert.True(result.Rebased);
        Assert.Equal(DraftStatus.Clean, result.Status);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Same(postBase, draft.StaleBase);
        Assert.Same(post.InitialState, draft.InitialDraftState);
        Assert.Same(draft.InitialDraftState, draft.CurrentDraftState);
        Assert.Empty(draft.PendingText);
        Assert.False(draft.IsDirty);
        Assert.False(draft.IsPossiblyStale);
        Assert.Empty(draft.Warnings);
        Assert.False(draft.HasPendingText);
    }

    [Fact]
    public void AppliedWithWarningsRebasesLikeAppliedAndKeepsTheWarnings()
    {
        var draft = DraftKit.ToApplying(DraftKit.Dirty());
        var post = DraftKit.Read(DraftKit.Form("AsWritten", 55));

        var result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.AppliedWithWarnings(post, new[] { "purge failed", "2 pieces missing" }));

        Assert.True(result.Accepted);
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, result.Kind);
        Assert.True(result.Rebased);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Same(post.StaleBase, draft.StaleBase);
        Assert.Same(post.InitialState, draft.InitialDraftState);
        Assert.Same(draft.InitialDraftState, draft.CurrentDraftState);
        Assert.Equal(new[] { "purge failed", "2 pieces missing" }, draft.Warnings.ToArray());
    }

    [Fact]
    public void WarningsDoNotSurviveTheNextReload()
    {
        var draft = DraftKit.ToApplying(DraftKit.Dirty());
        draft.CompleteApply(DraftApplyOutcome<DraftForm>.AppliedWithWarnings(DraftKit.Read(), new[] { "w" }));
        Assert.Single(draft.Warnings);

        draft.DiscardAndReload(DraftKit.Read());

        Assert.Empty(draft.Warnings);
    }

    [Fact]
    public void WhenThePostRereadFailedTheDraftIsUnknownWithOriginPostRereadAndNothingIsAssumedApplied()
    {
        var draft = DraftKit.Dirty();
        var content = (draft.CurrentDraftState, draft.InitialDraftState, draft.StaleBase);
        DraftKit.ToApplying(draft);

        var result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.AppliedPostRereadFailed("post reread failed"));

        Assert.True(result.Accepted);
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, result.Kind);
        Assert.False(result.Rebased);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);
        Assert.True(draft.IsDirty);
        Assert.Same(content.CurrentDraftState, draft.CurrentDraftState);
        Assert.Same(content.InitialDraftState, draft.InitialDraftState);
        Assert.Same(content.StaleBase, draft.StaleBase);
    }

    [Fact]
    public void AppliedNeverLeavesTheDraftCleanWithoutAnAdmittedPostRead()
    {
        var draft = DraftKit.ToApplying(DraftKit.Dirty());

        var result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(DraftIntentRead<DraftForm>.NotAdmitted("no longer admitted")));

        Assert.True(result.Accepted);
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, result.Kind);
        Assert.False(result.Rebased);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);
        Assert.Equal("no longer admitted", draft.LastApplyCause);
        Assert.True(draft.IsDirty);
    }

    [Fact]
    public void TheAppliedOutcomesRequireTheResultOfThePostRead()
    {
        Assert.Throws<ArgumentNullException>(() => DraftApplyOutcome<DraftForm>.Applied(null!));
        Assert.Throws<ArgumentNullException>(() => DraftApplyOutcome<DraftForm>.AppliedWithWarnings(null!, new[] { "w" }));
        Assert.Throws<ArgumentNullException>(() => DraftApplyOutcome<DraftForm>.AppliedWithWarnings(DraftKit.Read(), null!));
        Assert.Equal(DraftApplyOutcomeKind.Applied, DraftApplyOutcome<DraftForm>.Applied(DraftKit.Read()).Kind);
        Assert.NotNull(DraftApplyOutcome<DraftForm>.Applied(DraftKit.Read()).PostRead);
    }

    [Fact]
    public void TheOutcomeKindsAreExactlyTheEightOfTheTable()
    {
        Assert.Equal(
            new[] { "Applied", "AppliedPostRereadFailed", "AppliedWithWarnings", "CommitThrew", "NotAppliedAdmission", "NotAppliedConflict", "NotAppliedFailure", "NotAppliedPreflight" },
            Enum.GetNames<DraftApplyOutcomeKind>().OrderBy(n => n, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void ANotAppliedOutcomeNeedsItsCause()
    {
        Assert.Throws<ArgumentException>(() => DraftApplyOutcome<DraftForm>.NotAppliedAdmission(""));
        Assert.Throws<ArgumentException>(() => DraftApplyOutcome<DraftForm>.NotAppliedPreflight(null!));
        Assert.Throws<ArgumentException>(() => DraftApplyOutcome<DraftForm>.NotAppliedFailure(""));
        Assert.Throws<ArgumentException>(() => DraftApplyOutcome<DraftForm>.CommitThrew(""));
        Assert.Throws<ArgumentException>(() => DraftApplyOutcome<DraftForm>.AppliedPostRereadFailed(""));
        Assert.Throws<ArgumentNullException>(() => DraftApplyOutcome<DraftForm>.NotAppliedConflict(null!));
    }

    // ------------------------------------------------------------------ Check now (INV-F4T1-10)

    [Theory]
    [InlineData("clean")]
    [InlineData("dirty")]
    public void CheckNowRecoversTheExactPreviousStatusAndContentWhenTheRereadMatchesTheBaseBeforeTheAttempt(string previous)
    {
        var draft = Prior(previous);
        var content = (draft.CurrentDraftState, draft.InitialDraftState, draft.StaleBase);
        DraftKit.ToUnknownCommit(draft);
        var reread = DraftKit.DefaultBase();

        var check = draft.CheckNow(reread);

        Assert.Equal(DraftCheckStatus.Recovered, check.Status);
        Assert.Equal(PriorStatus(previous), check.DraftStatus);
        Assert.Equal(PriorStatus(previous), draft.Status);
        Assert.Null(draft.UnknownOrigin);
        Assert.Empty(check.Differences);
        Assert.Same(content.CurrentDraftState, draft.CurrentDraftState);
        Assert.Same(content.InitialDraftState, draft.InitialDraftState);
        Assert.Same(content.StaleBase, draft.StaleBase);
    }

    [Fact]
    public void CheckNowGoesToConflictWhenTheRereadDiffersFromTheBaseBeforeTheAttempt()
    {
        var draft = DraftKit.ToUnknownCommit(DraftKit.Dirty());
        var content = (draft.CurrentDraftState, draft.InitialDraftState, draft.StaleBase);
        var reread = DraftKit.Base(DraftKit.Mutable("D1"), DraftKit.Mutable("D2", "Back", "S2", "{written by the commit}"));

        var check = draft.CheckNow(reread);

        Assert.Equal(DraftCheckStatus.Conflict, check.Status);
        Assert.Equal(DraftStatus.Conflict, check.DraftStatus);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Contains(new DraftDifference("D2", DraftDifferenceKind.EnvelopeText), check.Differences);
        Assert.Equal(check.Differences.ToArray(), draft.ConflictDifferences.ToArray());
        Assert.Null(draft.UnknownOrigin);
        Assert.Same(content.CurrentDraftState, draft.CurrentDraftState);
        Assert.Same(content.StaleBase, draft.StaleBase);
    }

    [Theory]
    [InlineData("clean")]
    [InlineData("dirty")]
    [InlineData("conflict")]
    [InlineData("error")]
    [InlineData("applying")]
    [InlineData("unknown-post")]
    public void CheckNowOnlyAppliesToUnknownWithOriginCommitThrew(string status)
    {
        var draft = DraftKit.Dirty();
        switch (status)
        {
            case "clean": draft.DiscardAndReload(DraftKit.Read()); break;
            case "conflict": DraftKit.ToConflict(draft); break;
            case "error": DraftKit.ToError(draft); break;
            case "applying": DraftKit.ToApplying(draft); break;
            case "unknown-post": DraftKit.ToUnknownPost(draft); break;
        }

        var before = draft.Status;
        var differentBase = DraftKit.Base(DraftKit.Mutable("ZZ"));

        var check = draft.CheckNow(differentBase);

        Assert.Equal(DraftCheckStatus.NotApplicable, check.Status);
        Assert.Equal(before, draft.Status);
        Assert.Equal(before, check.DraftStatus);
        Assert.Empty(check.Differences);
    }

    [Fact]
    public void UnknownFromThePostRereadHasNoCheckNowAndIsResolvedWithDiscardAndReload()
    {
        var draft = DraftKit.ToUnknownPost(DraftKit.Dirty());

        var check = draft.CheckNow(DraftKit.DefaultBase());
        Assert.Equal(DraftCheckStatus.NotApplicable, check.Status);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);

        var reload = draft.DiscardAndReload(DraftKit.Read(DraftKit.Form("AsWritten", 55)));

        Assert.Equal(DraftReloadStatus.Reloaded, reload.Status);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Null(draft.UnknownOrigin);
        Assert.Equal("AsWritten", draft.CurrentDraftState.Name);
    }

    [Fact]
    public void AfterCheckNowFoundAConflictTheDraftOnlyLeavesThroughTheReconciliationActions()
    {
        var draft = DraftKit.ToUnknownCommit(DraftKit.Dirty());
        draft.CheckNow(DraftKit.Base(DraftKit.Mutable("D1", "Changed")));
        Assert.Equal(DraftStatus.Conflict, draft.Status);

        Assert.False(draft.TryBeginApply().IsStarted);
        Assert.Equal(DraftCheckStatus.NotApplicable, draft.CheckNow(DraftKit.DefaultBase()).Status);
        Assert.Equal(DraftStatus.Conflict, draft.Status);

        var keep = draft.KeepMineOnCurrentVersion(DraftKit.Read(DraftKit.Form("Theirs", 80)), confirmed: true);

        Assert.Equal(DraftReloadStatus.Reloaded, keep.Status);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void AnUnknownDraftThatRecoveredCanBeginANewUpdate()
    {
        var draft = DraftKit.ToUnknownCommit(DraftKit.Dirty());

        draft.CheckNow(DraftKit.DefaultBase());
        var start = draft.TryBeginApply();

        Assert.True(start.IsStarted);
        Assert.Equal(DraftStatus.Dirty, start.PreviousStatus);
    }

    [Fact]
    public void AnUpdateThatEndedAsAppliedLeavesTheNextUpdateComparingAgainstTheNewBase()
    {
        var draft = DraftKit.ToApplying(DraftKit.Dirty());
        var postBase = DraftKit.Base(DraftKit.Mutable("D1", "Front", "S1", "{after the update}"));
        draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(DraftKit.Read(DraftKit.Form("AsWritten", 55), postBase)));

        Assert.True(draft.StaleBase.CompareWith(postBase).IsMatch);
        Assert.False(draft.StaleBase.CompareWith(DraftKit.DefaultBase()).IsMatch);
        Assert.True(draft.TryBeginApply().IsStarted);
    }
}
