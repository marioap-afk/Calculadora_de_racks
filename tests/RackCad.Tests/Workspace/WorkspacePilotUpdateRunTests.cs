#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T4B-MUTATE-POST (INV-F4T4B-01, 02, 03, 05, 06, 10, 16): the pure and TOTAL run of the update of the pilot, with the real draft
// session, the real planner and the real reader, and a double of the port that simulates the drawing. The classification is the table of
// D-13 by frontier, with the three rows of the abort, the row of the outer release, the precedence and its two declared exceptions.
public class WorkspacePilotUpdateRunTests
{
    private static readonly string[] HappyCalls =
    {
        "Begin", "ReadForPrepare", "PrepareUnits", "BeginMutation", "Reread", "WriteUnit:A1", "WriteUnit:B2", "WriteUnit:C3", "Seal",
        "Dispose:mutation", "Post", "Dispose:scope",
    };

    private static InvalidOperationException Boom(string message) => new(message);

    private static string Cause(DraftApplyOutcome<PushBackPilotDraftState> outcome) => outcome.Cause ?? string.Empty;

    // ------------------------------------------------------------------ the check before the slot (step 1)

    [Fact]
    public void ThePrecheckOnlyStopsForTheThreeKindsThatNeedNeitherAReadNorACatalog()
    {
        var clean = PrepareKit.DraftOf(IntentKit.Read(PrepareKit.RealScan()));
        var nothing = PushBackPilotUpdateRun.Precheck(clean);
        Assert.False(nothing.Proceeds);
        Assert.Equal(PushBackPilotPrepareKind.NothingToApply, nothing.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NothingToApplyReason, nothing.Reason);

        var pending = PrepareKit.DraftOf(IntentKit.Read(PrepareKit.RealScan()));
        PrepareKit.SetScale(pending, 2.25);
        pending.SetPendingText("AnnotationScale", "3");
        var typed = PushBackPilotUpdateRun.Precheck(pending);
        Assert.False(typed.Proceeds);
        Assert.Equal(PushBackPilotPrepareKind.PendingText, typed.Kind);
        Assert.Equal(PushBackPilotUpdateRun.PendingTextReason, typed.Reason);

        var conflict = PrepareKit.DraftOf(IntentKit.Read(PrepareKit.RealScan()));
        PrepareKit.SetScale(conflict, 2.25);
        conflict.ConfirmBase(PrepareKit.BaseOf(("A1", PrepareKit.Frontal("pb-02-modulos"))));
        Assert.Equal(DraftStatus.Conflict, conflict.Status);
        var notApplicable = PushBackPilotUpdateRun.Precheck(conflict);
        Assert.False(notApplicable.Proceeds);
        Assert.Equal(PushBackPilotPrepareKind.NotApplicable, notApplicable.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NotApplicableReason, notApplicable.Reason);

        // A dirty draft without pending text goes on: the missing read is not a reason to stop.
        var dirty = new PilotUpdateSetup().Draft;
        var go = PushBackPilotUpdateRun.Precheck(dirty);
        Assert.True(go.Proceeds);
        Assert.Null(go.Reason);
    }

    // ------------------------------------------------------------------ the happy path: the order and what each step receives

    [Fact]
    public void TheRunFollowsTheOrderOfThePackageAndEndsAppliedWithTheRereadOfPost()
    {
        var setup = new PilotUpdateSetup();

        var outcome = setup.Run();

        Assert.Equal(HappyCalls, setup.Port.Calls.ToArray());
        Assert.True(outcome.Kind == DraftApplyOutcomeKind.Applied, outcome.Cause);
        Assert.NotNull(outcome.PostRead);
        Assert.True(outcome.PostRead!.IsAdmitted);
        Assert.Empty(outcome.Warnings);
        Assert.Equal(setup.Request, setup.Port.Request);
        Assert.Equal(PilotUpdateSetup.NewScale, outcome.PostRead.InitialState!.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void EveryWriteUnitReceivesTheTextOfThePlanForItsKeyInOrdinalOrder()
    {
        var setup = new PilotUpdateSetup();
        var plan = setup.ExpectedPlan();

        setup.Run();

        Assert.Equal(new[] { "A1", "B2", "C3" }, setup.Port.Written.Select(w => w.Key).ToArray());
        foreach (var (key, text) in setup.Port.Written) Assert.Equal(plan.Envelope(key), text);
        Assert.Equal(plan, setup.Port.Plan);
    }

    [Fact]
    public void TheDraftIsRebasedAndCleanFromTheRereadOfPostWhenTheRunIsClosed()
    {
        var setup = new PilotUpdateSetup();
        var plan = setup.ExpectedPlan();
        // The text of the origin changes with the write: POST finds the new text, and the origin of POST is the KEY of the plan.
        Assert.NotEqual(setup.Scan.RawEnvelopes["A1"], plan.Envelope("A1"));
        var outcome = setup.Run();

        var closed = setup.Draft.CompleteApply(outcome);

        Assert.True(closed.Accepted);
        Assert.True(closed.Rebased);
        Assert.Equal(DraftStatus.Clean, setup.Draft.Status);
        Assert.False(setup.Draft.IsDirty);
        Assert.Equal(PilotUpdateSetup.NewScale, setup.Draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.Equal(plan.Envelope("A1"), setup.Draft.StaleBase.Entries["A1"].EnvelopeText);
        Assert.Equal(plan.Envelope("A1"), setup.Draft.InitialDraftState.Origin);
    }

    // ------------------------------------------------------------------ PREPARE: the origin, the plan, the survivors and the layers

    [Fact]
    public void ADocumentThatIsNotTheAuthorizedOneStopsTheRunBeforeAnythingIsImported()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.DocumentMatches = false;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.DocumentMismatchReason, outcome.Cause);
        Assert.Equal(new[] { "Begin", "ReadForPrepare", "Dispose:scope" }, setup.Port.Calls.ToArray());
    }

    [Fact]
    public void ThereIsNoOriginWhenNoMutableMemberHasTheTextOfTheOriginOfTheDraft()
    {
        var setup = new PilotUpdateSetup();
        // The origin of the draft is the text of A1; the drawing no longer has it in any member.
        var others = setup.Members.Select(m => m.Fact.DefinitionKey == "A1" ? IntentKit.Member("A1", PrepareKit.Frontal("pb-02-modulos")) : m).ToArray();
        setup.Port.PrepareDrawing = others;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.NoOriginReason, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
        Assert.DoesNotContain("BeginMutation", setup.Port.Calls);
    }

    [Fact]
    public void SeveralMembersWithTheTextOfTheOriginAreAPreflightWithTheirOwnReasonAndNotTheMisleadingOneOfTheReader()
    {
        var setup = new PilotUpdateSetup();
        var twin = IntentKit.Member("D4", setup.Scan.RawEnvelopes["A1"]);
        setup.Port.PrepareDrawing = setup.Members.Append(twin).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.SeveralOriginsReason, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
    }

    [Fact]
    public void ADifferenceOfTheBaseInPrepareIsAConflictWithItsDifferencesAndNothingIsImported()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.PrepareDrawing = setup.Members.Select(m => m.Fact.DefinitionKey == "B2" ? IntentKit.Member("B2", PrepareKit.Lateral(PrepareKit.ExistingPost(), "pb-02-modulos")) : m).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, outcome.Kind);
        Assert.Contains(new DraftDifference("B2", DraftDifferenceKind.EnvelopeText), outcome.Differences);
        Assert.Null(outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
        Assert.DoesNotContain("BeginMutation", setup.Port.Calls);
    }

    [Fact]
    public void AReadThatIsNotAdmittedWithTheBaseOfTheDraftIsNotAppliedAdmission()
    {
        var originText = PrepareKit.Frontal();
        var divergent = PrepareKit.Lateral(PrepareKit.ExistingPost(), "pb-02-modulos");
        var members = new[] { IntentKit.Member("A1", originText), IntentKit.Member("B2", divergent) };
        var draft = PrepareKit.DraftOf(PrepareKit.BaseOf(("A1", originText), ("B2", divergent)), originText);
        PrepareKit.SetScale(draft, 2.25);
        var port = new PilotUpdateScopeDouble(members);
        Assert.True(draft.TryBeginApply().IsStarted);

        var outcome = PushBackPilotUpdateRun.Execute(draft, new PushBackPilotUpdateRequest(100, PrepareKit.Rack), port);

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedAdmission, outcome.Kind);
        Assert.False(string.IsNullOrEmpty(outcome.Cause));
        Assert.DoesNotContain("PrepareUnits", port.Calls);
    }

    [Fact]
    public void AViewThatIsNotAvailableInTheOutgoingSystemIsAPreflightAndNothingIsImported()
    {
        var originText = PrepareKit.Frontal();
        var phantom = PrepareKit.Lateral(PrepareKit.MissingPost);
        var draft = PrepareKit.DraftOf(PrepareKit.BaseOf(("A1", originText), ("B2", phantom)), originText);
        PrepareKit.SetScale(draft, 2.25);
        var port = new PilotUpdateScopeDouble(IntentKit.Member("A1", originText), IntentKit.Member("B2", phantom));
        Assert.True(draft.TryBeginApply().IsStarted);

        var outcome = PushBackPilotUpdateRun.Execute(draft, new PushBackPilotUpdateRequest(100, PrepareKit.Rack), port);

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdatePlanner.ViewUnavailablePrefix + "B2", outcome.Cause, StringComparison.Ordinal);
        Assert.DoesNotContain("PrepareUnits", port.Calls);
    }

    [Fact]
    public void ACleanDraftIsNothingToApplyAndAConflictDraftIsNotApplicableAndAPendingTextIsPendingTextEvenWithoutTheSlot()
    {
        var clean = new PilotUpdateSetup();
        clean.Draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = clean.Draft.InitialDraftState.Inputs.Annotations.AnnotationScale;
        Assert.Equal(DraftStatus.Clean, clean.Draft.Status);
        var nothing = PushBackPilotUpdateRun.Execute(clean.Draft, clean.Request, clean.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, nothing.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NothingToApplyReason, nothing.Cause);

        var pending = new PilotUpdateSetup();
        pending.Draft.SetPendingText("AnnotationScale", "3");
        var typed = PushBackPilotUpdateRun.Execute(pending.Draft, pending.Request, pending.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, typed.Kind);
        Assert.Equal(PushBackPilotUpdateRun.PendingTextReason, typed.Cause);

        var conflict = new PilotUpdateSetup();
        conflict.Draft.ConfirmBase(PrepareKit.BaseOf(("A1", PrepareKit.Frontal("pb-02-modulos"))));
        var notApplicable = PushBackPilotUpdateRun.Execute(conflict.Draft, conflict.Request, conflict.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, notApplicable.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NotApplicableReason, notApplicable.Cause);
    }

    [Fact]
    public void WithoutSurvivorsNothingIsImportedAndTheRunStops()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.PrepareDrawing = setup.Members.Select(m => new IntentItem(PilotUpdateScopeDouble.FactOf(m.Fact.DefinitionKey, m.Text!, layoutReferences: 0), m.Text)).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoSurvivorsReason, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
    }

    [Fact]
    public void ALockedLayerOfAMemberOfThePlanStopsTheRunBeforeTheImportAndLeavesNoResidue()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.LockedAtPrepare = new[] { "B2", "Z9" };

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.LockedLayerPrefix + "B2", outcome.Cause);
        Assert.Equal(new[] { "Begin", "ReadForPrepare", "Dispose:scope" }, setup.Port.Calls.ToArray());
    }

    [Fact]
    public void ALockedLayerOfADefinitionThatIsNotInThePlanIsIgnored()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.LockedAtPrepare = new[] { "Z9" };

        Assert.Equal(DraftApplyOutcomeKind.Applied, setup.Run().Kind);
    }

    [Fact]
    public void AnImportThatFailsIsAPreflightWithItsCauseAndNoScopeOfMutationIsStarted()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.Import = PushBackPilotImportResult.Failed("no block library");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal("no block library", outcome.Cause);
        Assert.DoesNotContain("BeginMutation", setup.Port.Calls);
    }

    [Fact]
    public void AForeignScopeOpenIsRejectedAsAValueAndNothingIsRereadWrittenOrReleased()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.Rejection = PushBackPilotUpdateRun.ForeignScopeOpenCause;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal("TOP_TRANSACTION_OPEN", outcome.Cause);
        Assert.Equal(new[] { "Begin", "ReadForPrepare", "PrepareUnits", "BeginMutation", "Dispose:scope" }, setup.Port.Calls.ToArray());
    }

    // ------------------------------------------------------------------ MUTATE: the re-read is the authority (INV-F4T4B-01 and 02)

    [Fact]
    public void ADifferenceFoundByTheRereadOfMutateIsAConflictAndNoUnitIsWritten()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.RereadDrawing = setup.Members.Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", PrepareKit.Planta("pb-02-modulos")) : m).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, outcome.Kind);
        Assert.Contains(new DraftDifference("C3", DraftDifferenceKind.EnvelopeText), outcome.Differences);
        Assert.Empty(setup.Port.Written);
        Assert.DoesNotContain("Seal", setup.Port.Calls);
        Assert.Contains("Dispose:mutation", setup.Port.Calls);
        Assert.DoesNotContain("Post", setup.Port.Calls);
        Assert.Equal(setup.Members.Select(m => m.Text).ToArray(), setup.Port.Drawing.Select(m => m.Text).ToArray());
    }

    [Fact]
    public void AnExtraMemberAppearingBetweenPrepareAndMutateIsAConflictToo()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.RereadDrawing = setup.Members.Append(IntentKit.Member("D4", PrepareKit.Planta())).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, outcome.Kind);
        Assert.Contains(new DraftDifference("D4", DraftDifferenceKind.Membership), outcome.Differences);
        Assert.Empty(setup.Port.Written);
    }

    [Fact]
    public void ADocumentThatChangedBetweenPrepareAndMutateIsAPreflightWithoutWrites()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.DocumentMatchesAtReread = false;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.DocumentMismatchReason, outcome.Cause);
        Assert.Empty(setup.Port.Written);
    }

    [Fact]
    public void ABlockThatIsNotTheOneThatWasPreparedIsAPreflightAndNothingIsWritten()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.PreparedBlockMatches = key => key != "B2";

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.DestinationChangedReason, outcome.Cause);
        Assert.Empty(setup.Port.Written);
        Assert.DoesNotContain("Seal", setup.Port.Calls);
    }

    [Fact]
    public void TheSurvivorsAndTheLockedLayersAreCheckedAgainInMutateAndNothingIsWritten()
    {
        var noSurvivors = new PilotUpdateSetup();
        noSurvivors.Port.RereadDrawing = noSurvivors.Members.Select(m => new IntentItem(PilotUpdateScopeDouble.FactOf(m.Fact.DefinitionKey, m.Text!, layoutReferences: 0), m.Text)).ToArray();
        var first = noSurvivors.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, first.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoSurvivorsReason, first.Cause);
        Assert.Empty(noSurvivors.Port.Written);
        Assert.Contains("PrepareUnits", noSurvivors.Port.Calls);

        var locked = new PilotUpdateSetup();
        locked.Port.LockedAtReread = new[] { "C3" };
        var second = locked.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, second.Kind);
        Assert.Equal(PushBackPilotUpdateRun.LockedLayerPrefix + "C3", second.Cause);
        Assert.Empty(locked.Port.Written);
        Assert.DoesNotContain("Seal", locked.Port.Calls);
    }

    // ------------------------------------------------------------------ the table of D-13 by frontier (INV-F4T4B-03)

    [Theory]
    [InlineData("Begin")]
    [InlineData("ReadForPrepare")]
    [InlineData("PrepareUnits")]
    [InlineData("BeginMutation")]
    [InlineData("Reread")]
    [InlineData("WriteUnit:A1")]
    [InlineData("WriteUnit:B2")]
    public void AnExceptionBeforeTheSealIsNotAppliedFailureWithItsCauseAndTheSealIsNeverCalled(string call)
    {
        var setup = new PilotUpdateSetup();
        setup.Port.ThrowAt[call] = Boom("boom at " + call);

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, outcome.Kind);
        Assert.Contains("boom at " + call, Cause(outcome), StringComparison.Ordinal);
        Assert.DoesNotContain("Seal", setup.Port.Calls);
        Assert.DoesNotContain("Post", setup.Port.Calls);
        Assert.Equal(setup.Members.Select(m => m.Text).ToArray(), setup.Port.Drawing.Select(m => m.Text).ToArray());
        // Whatever scope was opened was released.
        Assert.Equal(call == "Begin" ? 0 : 1, setup.Port.Calls.Count(c => c == "Dispose:scope"));
    }

    [Fact]
    public void AbortThatFailsAfterAUnitStartedIsCommitThrewWithAbortFailedAndNoRollbackIsClaimed()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.ThrowAt["WriteUnit:B2"] = Boom("unit boom");
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.AbortFailedPrefix, Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("abort boom", Cause(outcome), StringComparison.Ordinal);
        Assert.DoesNotContain("Seal", setup.Port.Calls);
        Assert.DoesNotContain("Post", setup.Port.Calls);

        var closed = setup.Draft.CompleteApply(outcome);
        Assert.Equal(DraftStatus.Unknown, setup.Draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, setup.Draft.UnknownOrigin);
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, closed.Kind);
    }

    [Fact]
    public void AReleaseThatFailsWithoutSealAndWithoutAUnitStartedIsNotAppliedFailureAndNotAnUncertainOutcome()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.ThrowAt["Reread"] = Boom("reread boom");
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, outcome.Kind);
        Assert.Contains("reread boom", Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("abort boom", Cause(outcome), StringComparison.Ordinal);
        Assert.Empty(setup.Port.Written);
    }

    [Fact]
    public void ASealThatThrowsIsCommitThrewAndPostIsNeverReached()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.ThrowAt["Seal"] = Boom("seal boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.SealFailedPrefix, Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("seal boom", Cause(outcome), StringComparison.Ordinal);
        Assert.DoesNotContain("Post", setup.Port.Calls);
        Assert.Contains("Dispose:mutation", setup.Port.Calls);
        Assert.Contains("Dispose:scope", setup.Port.Calls);
    }

    [Fact]
    public void ASealThatReturnsAndAReleaseThatThrowsGoesToPostAndTheReleaseIsAWarning()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("release boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Contains(outcome.Warnings, w => w.Contains("release boom", StringComparison.Ordinal));
        Assert.Contains("Post", setup.Port.Calls);
        Assert.True(outcome.PostRead!.IsAdmitted);
    }

    [Fact]
    public void APostThatThrowsIsAppliedPostRereadFailedWithTheWarningsThatWereKnown()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.MissingPieces["B2"] = new[] { "POSTE_X" };
        setup.Port.ThrowAt["Post"] = Boom("post boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);
        Assert.Contains("post boom", Cause(outcome), StringComparison.Ordinal);
        Assert.Single(outcome.Warnings);
        Assert.Contains("POSTE_X", outcome.Warnings[0], StringComparison.Ordinal);
    }

    [Fact]
    public void APostWithoutValuesOrWithoutItsRereadIsAppliedPostRereadFailed()
    {
        var withoutValues = new PilotUpdateSetup();
        withoutValues.Port.NullPostValues = true;
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, withoutValues.Run().Kind);

        var withoutScan = new PilotUpdateSetup();
        withoutScan.Port.NullPostScan = true;
        var outcome = withoutScan.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.PostRereadMissingReason, outcome.Cause);
    }

    [Fact]
    public void AForeignScopeOpenAtPostIsAppliedPostRereadFailedWithItsCauseAndTheWarnings()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.MissingPieces["A1"] = new[] { "PLACA_Y" };
        setup.Port.ForeignAtPost = true;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.ForeignScopeOpenCause, outcome.Cause);
        Assert.Single(outcome.Warnings);
        var closed = setup.Draft.CompleteApply(outcome);
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, closed.Kind);
        Assert.Equal(DraftStatus.Unknown, setup.Draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, setup.Draft.UnknownOrigin);
        Assert.Single(setup.Draft.Warnings);
    }

    [Fact]
    public void AComparisonThatThrowsAfterTheSealIsAppliedPostRereadFailed()
    {
        var setup = new PilotUpdateSetup();
        // The scan of POST carries a null fact: building the read of POST throws after the seal.
        setup.Port.PostOverride = request => new PushBackPilotPostValues(
            false,
            new PushBackPilotScanValues(true, new RackSiblingScanFact[] { null! }, new Dictionary<string, string>()),
            new Dictionary<string, string>(),
            null);

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.PostFailurePrefix, Cause(outcome), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the release of the outer scope: it never changes what was decided

    [Fact]
    public void AnOuterReleaseThatThrowsAfterPostTurnsAppliedIntoAppliedWithWarningsAndKeepsTheOtherWarnings()
    {
        var plain = new PilotUpdateSetup();
        plain.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var outcome = plain.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Contains(outcome.Warnings, w => w.Contains("outer boom", StringComparison.Ordinal));

        var warned = new PilotUpdateSetup();
        warned.Port.MissingPieces["C3"] = new[] { "PIEZA_Z" };
        warned.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var second = warned.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, second.Kind);
        Assert.Equal(2, second.Warnings.Count);
        Assert.True(second.PostRead!.IsAdmitted);

        // Both end Clean: the warnings do not change the status of the draft.
        Assert.Equal(DraftStatus.Clean, StatusAfter(plain, outcome));
    }

    private static DraftStatus StatusAfter(PilotUpdateSetup setup, DraftApplyOutcome<PushBackPilotDraftState> outcome)
    {
        setup.Draft.CompleteApply(outcome);
        return setup.Draft.Status;
    }

    [Fact]
    public void AnOuterReleaseThatThrowsOutsidePostAddsItsCauseToTheClassificationThatWasAlreadyDecided()
    {
        var preflight = new PilotUpdateSetup();
        preflight.Port.DocumentMatches = false;
        preflight.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var first = preflight.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, first.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.DocumentMismatchReason, Cause(first), StringComparison.Ordinal);
        Assert.Contains("outer boom", Cause(first), StringComparison.Ordinal);

        var failure = new PilotUpdateSetup();
        failure.Port.ThrowAt["PrepareUnits"] = Boom("import boom");
        failure.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var second = failure.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, second.Kind);
        Assert.Contains("import boom", Cause(second), StringComparison.Ordinal);
        Assert.Contains("outer boom", Cause(second), StringComparison.Ordinal);

        var committed = new PilotUpdateSetup();
        committed.Port.ThrowAt["Seal"] = Boom("seal boom");
        committed.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var third = committed.Run();
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, third.Kind);
        Assert.Contains("outer boom", Cause(third), StringComparison.Ordinal);

        var abort = new PilotUpdateSetup();
        abort.Port.ThrowAt["WriteUnit:A1"] = Boom("unit boom");
        abort.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");
        abort.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var fourth = abort.Run();
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, fourth.Kind);
        Assert.Contains("abort boom", Cause(fourth), StringComparison.Ordinal);
        Assert.Contains("outer boom", Cause(fourth), StringComparison.Ordinal);
    }

    [Fact]
    public void AnOuterReleaseThatThrowsOnAConflictIsDroppedBecauseAConflictCarriesNoCause()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.RereadDrawing = setup.Members.Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", PrepareKit.Planta("pb-02-modulos")) : m).ToArray();
        setup.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");

        var outcome = setup.Run();

        // The declared limitation of the package: the exception of the release is lost, the classification is not.
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, outcome.Kind);
        Assert.Null(outcome.Cause);
    }

    // ------------------------------------------------------------------ the precedence between the rows

    [Fact]
    public void AConflictOrAPreflightOfMutateStaysWhatItIsEvenWhenTheReleaseOfTheScopeOfMutateThrows()
    {
        var conflict = new PilotUpdateSetup();
        conflict.Port.RereadDrawing = conflict.Members.Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", PrepareKit.Planta("pb-02-modulos")) : m).ToArray();
        conflict.Port.ThrowAt["Dispose:mutation"] = Boom("release boom");
        var first = conflict.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, first.Kind);
        Assert.NotEmpty(first.Differences);

        var preflight = new PilotUpdateSetup();
        preflight.Port.LockedAtReread = new[] { "B2" };
        preflight.Port.ThrowAt["Dispose:mutation"] = Boom("release boom");
        var second = preflight.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, second.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.LockedLayerPrefix + "B2", Cause(second), StringComparison.Ordinal);
        Assert.Contains("release boom", Cause(second), StringComparison.Ordinal);
        Assert.Empty(preflight.Port.Written);
    }

    [Fact]
    public void ASealThatThrewKeepsBeingCommitThrewBecauseOfTheSealEvenIfTheAbortThrowsToo()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.ThrowAt["Seal"] = Boom("seal boom");
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.SealFailedPrefix, Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("abort boom", Cause(outcome), StringComparison.Ordinal);
    }

    [Fact]
    public void AbortFailedOnlyAppliesToANotAppliedFailureWithAUnitStarted()
    {
        // A preflight of MUTATE has no unit started: it stays a preflight (the test above); a unit that threw and a clean abort stays NotAppliedFailure.
        var setup = new PilotUpdateSetup();
        setup.Port.ThrowAt["WriteUnit:C3"] = Boom("unit boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, outcome.Kind);
        Assert.Contains("unit boom", Cause(outcome), StringComparison.Ordinal);
        Assert.Equal(new[] { "A1", "B2" }, setup.Port.Written.Select(w => w.Key).ToArray());
        Assert.Contains("Dispose:mutation", setup.Port.Calls);
        Assert.DoesNotContain(PushBackPilotUpdateRun.AbortFailedPrefix, Cause(outcome), StringComparison.Ordinal);
        // The two units that were staged before the failure are not in the drawing: the abort threw them away.
        Assert.Equal(setup.Members.Select(m => m.Text).ToArray(), setup.Port.Drawing.Select(m => m.Text).ToArray());
    }

    // ------------------------------------------------------------------ the run is total (INV-F4T4B-03)

    [Fact]
    public void TheRunNeverThrowsForMissingArgumentsOrForAPortThatAnswersNothing()
    {
        var setup = new PilotUpdateSetup();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.Execute(null!, setup.Request, setup.Port).Kind);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.Execute(setup.Draft, null!, setup.Port).Kind);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.Execute(setup.Draft, setup.Request, null!).Kind);

        var noScope = new PilotUpdateSetup();
        noScope.Port.ReturnNullScope = true;
        var outcome = noScope.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoScopeReason, outcome.Cause);

        var noStart = new PilotUpdateSetup();
        noStart.Port.NullMutationStart = true;
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, noStart.Run().Kind);
        Assert.Contains("Dispose:scope", noStart.Port.Calls);
    }

    // ------------------------------------------------------------------ POST: the names, the warnings and the comparison

    private static PushBackPilotIntentScan NamedScan(string name) => PrepareKit.Scan(
        IntentKit.Member("A1", PrepareKit.Frontal(name: name)),
        IntentKit.Member("B2", PrepareKit.Lateral(PrepareKit.ExistingPost(), name: name)),
        IntentKit.Member("C3", PrepareKit.Planta(name: name)));

    [Fact]
    public void TheDesiredNamesFollowTheRuleOfTheClassicEditorPerView()
    {
        var setup = new PilotUpdateSetup(() => NamedScan("  Mi rack  "));
        var post = PrepareKit.ExistingPost();

        var outcome = setup.Run();

        Assert.True(outcome.Kind == DraftApplyOutcomeKind.Applied, outcome.Cause);
        var names = setup.Port.PostRequest!.DesiredNames;
        Assert.Equal(3, names.Count);
        Assert.Equal("Mi rack - frontal entrada-salida", names["A1"]);
        Assert.Equal("Mi rack - lateral " + (post + 1), names["B2"]);
        Assert.Equal("Mi rack - planta", names["C3"]);
    }

    [Fact]
    public void AViewWithABlankNameHasNoDesiredNameAndGivesNoWarningEvenIfItsDefinitionHasAnotherName()
    {
        var setup = new PilotUpdateSetup(() => NamedScan("   "));

        var outcome = setup.Run();

        Assert.True(outcome.Kind == DraftApplyOutcomeKind.Applied, outcome.Cause);
        Assert.Empty(setup.Port.PostRequest!.DesiredNames);
        Assert.Empty(outcome.Warnings);
    }

    [Fact]
    public void ANameThatDiffersAfterTheRenameIsAWarningAndACaseOnlyDifferenceIsNot()
    {
        var differs = new PilotUpdateSetup(() => NamedScan("Mi rack"));
        differs.Port.NameAfterPost = (key, desired) => key == "C3" ? "Ocupado" : desired;
        var outcome = differs.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        var warning = Assert.Single(outcome.Warnings);
        Assert.Contains("C3", warning, StringComparison.Ordinal);

        var sameCase = new PilotUpdateSetup(() => NamedScan("Mi rack"));
        sameCase.Port.NameAfterPost = (key, desired) => desired.ToUpperInvariant();
        Assert.Equal(DraftApplyOutcomeKind.Applied, sameCase.Run().Kind);
    }

    [Fact]
    public void TheMissingPiecesAndAFailedRegenerationAreWarningsAndTheStateStaysClean()
    {
        var setup = new PilotUpdateSetup();
        setup.Port.MissingPieces["A1"] = new[] { "POSTE_Q", "PLACA_R" };
        setup.Port.RegenFailure = "InvalidOperationException: regen";

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Equal(3, outcome.Warnings.Count);
        Assert.Contains(outcome.Warnings, w => w.Contains("POSTE_Q", StringComparison.Ordinal) && w.Contains("A1", StringComparison.Ordinal));
        Assert.Contains(outcome.Warnings, w => w.Contains("regen", StringComparison.Ordinal));
        var closed = setup.Draft.CompleteApply(outcome);
        Assert.True(closed.Rebased);
        Assert.Equal(DraftStatus.Clean, setup.Draft.Status);
        Assert.Equal(outcome.Warnings.ToArray(), setup.Draft.Warnings.ToArray());
    }

    // The comparison of POST is byte for byte: nothing is normalized, so each of these differences fails it.
    [Theory]
    [InlineData("trailing")]
    [InlineData("crlf")]
    [InlineData("case")]
    public void ThePostComparisonDoesNotNormalizeAnything(string change)
    {
        var setup = new PilotUpdateSetup();
        setup.Port.TextAfterSeal = (key, text) =>
        {
            if (key != "B2") return text;
            return change switch
            {
                "trailing" => text + " ",
                "crlf" => text.Replace("\n", "\r\n", StringComparison.Ordinal) + "\r\n",
                _ => text.Replace("\"lateral\"", "\"Lateral\"", StringComparison.Ordinal),
            };
        };

        var outcome = setup.Run();

        Assert.NotEqual(DraftApplyOutcomeKind.Applied, outcome.Kind);
        Assert.NotEqual(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);

        // M-06 (b): the cause names the envelope that differs (B2), not just any cause.
        Assert.StartsWith(PushBackPilotUpdateRun.PostEnvelopeDiffersPrefix + "B2", outcome.Cause, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePostComparisonFailsForAnotherSetOfKeysAnotherEnvelopeAndAnotherScale()
    {
        var plan = new PilotUpdateSetup().ExpectedPlan();
        var good = IntentKit.Read(IntentKit.Scan(PrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).ToArray()));
        Assert.Null(PushBackPilotUpdateRun.ComparePost(plan, good));

        var fewer = IntentKit.Read(IntentKit.Scan(PrepareKit.Rack, "A1", plan.Members.Take(2).Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).ToArray()));
        Assert.Equal(PushBackPilotUpdateRun.PostKeysDifferReason, PushBackPilotUpdateRun.ComparePost(plan, fewer));

        var more = IntentKit.Read(IntentKit.Scan(PrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).Append(IntentKit.Member("D4", plan.Envelope("C3")!)).ToArray()));
        Assert.Equal(PushBackPilotUpdateRun.PostKeysDifferReason, PushBackPilotUpdateRun.ComparePost(plan, more));

        var edited = IntentKit.Read(IntentKit.Scan(PrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.Key == "C3" ? m.EnvelopeText + " " : m.EnvelopeText)).ToArray()));
        Assert.StartsWith(PushBackPilotUpdateRun.PostEnvelopeDiffersPrefix + "C3", PushBackPilotUpdateRun.ComparePost(plan, edited), StringComparison.Ordinal);

        Assert.Equal(PushBackPilotUpdateRun.PostRereadMissingReason, PushBackPilotUpdateRun.ComparePost(plan, null));
        var notAdmitted = IntentKit.Read(IntentKit.Scan(PrepareKit.Rack, "ZZ", IntentKit.Member("A1", plan.Envelope("A1")!)));
        Assert.StartsWith(PushBackPilotUpdateRun.PostNotAdmittedPrefix, PushBackPilotUpdateRun.ComparePost(plan, notAdmitted), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePostComparisonSeesAnotherScaleInTheStateThatTheRereadMaterializes()
    {
        // The plan of another scale, over the very same envelopes of the drawing: the envelopes of the plan carry the scale of the design, so a
        // re-read of the envelopes of a plan of another scale does not match it.
        var plan = new PilotUpdateSetup().ExpectedPlan();
        var other = new PilotUpdateSetup(scale: 3.5).ExpectedPlan();
        var read = IntentKit.Read(IntentKit.Scan(PrepareKit.Rack, "A1", other.Members.Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).ToArray()));

        var cause = PushBackPilotUpdateRun.ComparePost(plan, read);
        Assert.NotNull(cause);

        // M-06 (c): the branch of the scale (PostScaleDiffersReason) is UNREACHABLE with equal envelopes: the scale that the re-read
        // materializes comes from the design inside the envelope, so envelopes that are equal byte for byte give the scale of the plan. A
        // difference of scale always shows first as a difference of envelope, and that is what is fixed here.
        Assert.StartsWith(PushBackPilotUpdateRun.PostEnvelopeDiffersPrefix, cause, StringComparison.Ordinal);
        Assert.NotEqual(PushBackPilotUpdateRun.PostScaleDiffersReason, cause);
        var same = IntentKit.Read(IntentKit.Scan(PrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).ToArray()));
        Assert.Null(PushBackPilotUpdateRun.ComparePost(plan, same));
    }

    [Fact]
    public void ThePostRereadUsesTheKeyOfTheOriginOfThePlanAndNotTheOneOfTheTextThatChanged()
    {
        // The origin of the draft was the text of A1; after the write A1 has the new text, which no member of the drawing had before.
        var setup = new PilotUpdateSetup();
        var plan = setup.ExpectedPlan();
        Assert.Equal("A1", plan.OriginKey);

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.Applied, outcome.Kind);
        Assert.Equal(plan.Envelope("A1"), setup.Port.Drawing.Single(m => m.Key == "A1").Text);
    }

    [Fact]
    public void TheRunKeepsNoTextOfTheDrawingBetweenTwoRuns()
    {
        // M-06 (a): two runs over the SAME double, the second one from a draft read from what the first one left in the drawing and with
        // another scale. Nothing of the texts of the first run may survive in the second.
        var first = new PilotUpdateSetup();
        Assert.Equal(DraftApplyOutcomeKind.Applied, first.Run().Kind);
        var firstWritten = first.Port.Written.ToList();
        Assert.NotEmpty(firstWritten);

        var afterFirst = IntentKit.Scan(PrepareKit.Rack, "A1", first.Port.Drawing.Select(m => IntentKit.Member(m.Key, m.Text)).ToArray());
        var draft = PrepareKit.DraftOf(IntentKit.Read(afterFirst));
        PrepareKit.SetScale(draft, 3.5);
        Assert.True(draft.TryBeginApply().IsStarted);

        var outcome = PushBackPilotUpdateRun.Execute(draft, first.Request, first.Port);

        Assert.True(outcome.Kind == DraftApplyOutcomeKind.Applied, outcome.Cause);
        var secondWritten = first.Port.Written.Skip(firstWritten.Count).ToList();
        Assert.Equal(firstWritten.Select(w => w.Key).ToArray(), secondWritten.Select(w => w.Key).ToArray());
        Assert.All(secondWritten, w => Assert.DoesNotContain(firstWritten, f => f.Text == w.Text));
        Assert.All(secondWritten, w => Assert.Contains("3.5", w.Text, StringComparison.Ordinal));
    }

    // M-06 (d): the names of the views of a composite rack carry the side of each cut. The pilot refuses a composite rack (its reader says it is
    // out of the pilot), so the rule is exercised on the plan itself, built by hand over the composite fixture, and the names are compared with
    // the rule of the classic editor for the same cuts.
    [Fact]
    public void TheDesiredNamesOfACompositeRackCarryTheSideOfEachCutAndAPosteriorCutKeepsItsOwnName()
    {
        const string fixture = "pb-20-ab";
        var members = new List<PushBackPilotMemberPlan>();
        foreach (var section in new[] { 0, 1, 2, 3 })
        {
            var text = PrepareKit.Frontal(fixture, section, "Mi rack");
            var embed = new RackEmbedStore().Deserialize(text)!;
            members.Add(new PushBackPilotMemberPlan("K" + section, embed.View, embed.Section, embed.Design, text));
        }

        var plan = new PushBackPilotUpdatePlan(
            PrepareKit.Rack, "K0", Array.Empty<DraftStaleBaseEntry>(), null, 2.25, members,
            new PushBackPilotPostExpectation(2.25, Array.Empty<PushBackPilotKeyedText>()));

        var names = PushBackPilotUpdateRun.DesiredNamesOf(plan, PilotKit.Catalog);

        Assert.Equal(members.Count, names.Count);
        Assert.All(names.Values, name => Assert.Matches(@"^Mi rack - frontal (entrada-salida|posterior) [AB]$", name));
        Assert.Contains(names.Values, name => name.EndsWith(" A", StringComparison.Ordinal));
        Assert.Contains(names.Values, name => name.EndsWith(" B", StringComparison.Ordinal));
        Assert.Equal(names.Count, names.Values.Distinct(StringComparer.Ordinal).Count());
    }

    // The system of the outgoing design, rebuilt apart from the text of a member of the plan with the same resolver as PREPARE.
    [Fact]
    public void TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan()
    {
        var plan = new PilotUpdateSetup().ExpectedPlan();

        var system = PushBackPilotSystemRebuild.Build(plan.Members[0].DesignText, PilotKit.Catalog);

        Assert.NotNull(system);
        Assert.False(PushBackPilotSystemRebuild.IsComposite(system));
        Assert.Throws<InvalidOperationException>(() => PushBackPilotSystemRebuild.Build("not a design", PilotKit.Catalog));
        Assert.Throws<ArgumentNullException>(() => PushBackPilotSystemRebuild.Build(null!, PilotKit.Catalog));
        Assert.Throws<ArgumentNullException>(() => PushBackPilotSystemRebuild.IsComposite(null!));
    }
}
