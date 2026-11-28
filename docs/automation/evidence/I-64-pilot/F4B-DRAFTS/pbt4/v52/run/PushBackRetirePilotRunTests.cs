#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md; owner-pbt4d-deadcode-27-decisions.md, pregunta 2): the substitutes, over ExecuteFull,
/// of the 27 tests of WorkspacePilotUpdateRunTests that exercise lines or branches of the generic run (Execution&lt;TState&gt;) and of the plan
/// that nothing else exercises, measured line by line and branch by branch. The run of the pilot that they used as a vehicle is retired; the
/// run, the planner and the reader are the real ones of the full model, with the double of the port that simulates the drawing. Each test keeps
/// the name and the assertions of its original, with the map of PB-T4b (the kit of the copies, the full draft and ExecuteFull); the ones that
/// do not fit the map say so. The other 22 tests of the original class are deleted with their record.
/// </summary>
public class PushBackRetirePilotRunTests
{
    private static InvalidOperationException Boom(string message) => new(message);

    private static string Cause(DraftApplyOutcome<PushBackFullDraftState> outcome) => outcome.Cause ?? string.Empty;

    [Fact]
    public void ThereIsNoOriginWhenNoMutableMemberHasTheTextOfTheOriginOfTheDraft()
    {
        var setup = new FullUpdateSetup();
        // The origin of the draft is the text of A1; the drawing no longer has it in any member.
        var others = setup.Members.Select(m => m.Fact.DefinitionKey == "A1" ? IntentKit.Member("A1", FullPrepareKit.Frontal("pb-02-modulos")) : m).ToArray();
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
        var setup = new FullUpdateSetup();
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
        var setup = new FullUpdateSetup();
        setup.Port.PrepareDrawing = setup.Members.Select(m => m.Fact.DefinitionKey == "B2" ? IntentKit.Member("B2", FullPrepareKit.Lateral(FullPrepareKit.ExistingPost(), "pb-02-modulos")) : m).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, outcome.Kind);
        Assert.Contains(new DraftDifference("B2", DraftDifferenceKind.EnvelopeText), outcome.Differences);
        Assert.Null(outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
        Assert.DoesNotContain("BeginMutation", setup.Port.Calls);
    }

    // I-64 F4B PB-T4d (owner-pbt4d-deadcode-27-decisions.md, pregunta 1): by hand. The full model erases a lateral that is not available, and
    // BuildCore no longer has a view that is not available (the mode of the pilot that never erases is retired with it); the Invalid of PREPARE
    // inside the run is the frontal of side B of a rack without side B, prepared as in CT-PBT3B-04 (ii), and nothing is imported.
    [Fact]
    public void AViewThatIsNotAvailableInTheOutgoingSystemIsAPreflightAndNothingIsImported()
    {
        var scan = IntentKit.Scan(FullPrepareKit.Rack, "A1", IntentKit.Member("A1", FullPrepareKit.Frontal("pb-20-ab")), IntentKit.Member("FB", FullPrepareKit.Frontal("pb-20-ab", 2)));
        var draft = FullWriteKit.DraftOf(FullWriteKit.Read(scan));
        FullWriteKit.Edit(draft, state => state.Editable.Editor.SetSideBPresent(false));
        var port = new PilotUpdateScopeDouble(FullWriteKit.ItemsOf(scan));
        Assert.True(draft.TryBeginApply().IsStarted);

        var outcome = PushBackPilotUpdateRun.ExecuteFull(draft, new PushBackPilotUpdateRequest(100, FullPrepareKit.Rack), port);

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.FrontalBOfOneWayPrefix + "FB" + PushBackPilotUpdatePlanner.FrontalBOfOneWaySuffix, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", port.Calls);
    }

    [Fact]
    public void ACleanDraftIsNothingToApplyAndAConflictDraftIsNotApplicableAndAPendingTextIsPendingTextEvenWithoutTheSlot()
    {
        var clean = new FullUpdateSetup();
        clean.Draft.CurrentDraftState.Editable.AnnotationScale = clean.Draft.InitialDraftState.Editable.AnnotationScale;
        Assert.Equal(DraftStatus.Clean, clean.Draft.Status);
        var nothing = PushBackPilotUpdateRun.ExecuteFull(clean.Draft, clean.Request, clean.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, nothing.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NothingToApplyReason, nothing.Cause);

        var pending = new FullUpdateSetup();
        pending.Draft.SetPendingText("AnnotationScale", "3");
        var typed = PushBackPilotUpdateRun.ExecuteFull(pending.Draft, pending.Request, pending.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, typed.Kind);
        Assert.Equal(PushBackPilotUpdateRun.PendingTextReason, typed.Cause);

        var conflict = new FullUpdateSetup();
        conflict.Draft.ConfirmBase(FullPrepareKit.BaseOf(("A1", FullPrepareKit.Frontal("pb-02-modulos"))));
        var notApplicable = PushBackPilotUpdateRun.ExecuteFull(conflict.Draft, conflict.Request, conflict.Port);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, notApplicable.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NotApplicableReason, notApplicable.Cause);
    }

    [Fact]
    public void WithoutSurvivorsNothingIsImportedAndTheRunStops()
    {
        var setup = new FullUpdateSetup();
        setup.Port.PrepareDrawing = setup.Members.Select(m => new IntentItem(PilotUpdateScopeDouble.FactOf(m.Fact.DefinitionKey, m.Text!, layoutReferences: 0), m.Text)).ToArray();

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoSurvivorsReason, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", setup.Port.Calls);
    }

    [Fact]
    public void AnImportThatFailsIsAPreflightWithItsCauseAndNoScopeOfMutationIsStarted()
    {
        var setup = new FullUpdateSetup();
        setup.Port.Import = PushBackPilotImportResult.Failed("no block library");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal("no block library", outcome.Cause);
        Assert.DoesNotContain("BeginMutation", setup.Port.Calls);
    }

    [Fact]
    public void AForeignScopeOpenIsRejectedAsAValueAndNothingIsRereadWrittenOrReleased()
    {
        var setup = new FullUpdateSetup();
        setup.Port.Rejection = PushBackPilotUpdateRun.ForeignScopeOpenCause;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal("TOP_TRANSACTION_OPEN", outcome.Cause);
        Assert.Equal(new[] { "Begin", "ReadForPrepare", "PrepareUnits", "BeginMutation", "Dispose:scope" }, setup.Port.Calls.ToArray());
    }

    [Fact]
    public void ADocumentThatChangedBetweenPrepareAndMutateIsAPreflightWithoutWrites()
    {
        var setup = new FullUpdateSetup();
        setup.Port.DocumentMatchesAtReread = false;

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.DocumentMismatchReason, outcome.Cause);
        Assert.Empty(setup.Port.Written);
    }

    [Fact]
    public void ABlockThatIsNotTheOneThatWasPreparedIsAPreflightAndNothingIsWritten()
    {
        var setup = new FullUpdateSetup();
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
        var noSurvivors = new FullUpdateSetup();
        noSurvivors.Port.RereadDrawing = noSurvivors.Members.Select(m => new IntentItem(PilotUpdateScopeDouble.FactOf(m.Fact.DefinitionKey, m.Text!, layoutReferences: 0), m.Text)).ToArray();
        var first = noSurvivors.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, first.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoSurvivorsReason, first.Cause);
        Assert.Empty(noSurvivors.Port.Written);
        Assert.Contains("PrepareUnits", noSurvivors.Port.Calls);

        var locked = new FullUpdateSetup();
        locked.Port.LockedAtReread = new[] { "C3" };
        var second = locked.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, second.Kind);
        Assert.Equal(PushBackPilotUpdateRun.LockedLayerPrefix + "C3", second.Cause);
        Assert.Empty(locked.Port.Written);
        Assert.DoesNotContain("Seal", locked.Port.Calls);
    }

    [Fact]
    public void AbortThatFailsAfterAUnitStartedIsCommitThrewWithAbortFailedAndNoRollbackIsClaimed()
    {
        var setup = new FullUpdateSetup();
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
        var setup = new FullUpdateSetup();
        setup.Port.ThrowAt["Reread"] = Boom("reread boom");
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, outcome.Kind);
        Assert.Contains("reread boom", Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("abort boom", Cause(outcome), StringComparison.Ordinal);
        Assert.Empty(setup.Port.Written);
    }

    [Fact]
    public void ASealThatReturnsAndAReleaseThatThrowsGoesToPostAndTheReleaseIsAWarning()
    {
        var setup = new FullUpdateSetup();
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("release boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Contains(outcome.Warnings, w => w.Contains("release boom", StringComparison.Ordinal));
        Assert.Contains("Post", setup.Port.Calls);
        Assert.True(outcome.PostRead!.IsAdmitted);
    }

    [Fact]
    public void APostWithoutValuesOrWithoutItsRereadIsAppliedPostRereadFailed()
    {
        var withoutValues = new FullUpdateSetup();
        withoutValues.Port.NullPostValues = true;
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, withoutValues.Run().Kind);

        var withoutScan = new FullUpdateSetup();
        withoutScan.Port.NullPostScan = true;
        var outcome = withoutScan.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.PostRereadMissingReason, outcome.Cause);
    }

    [Fact]
    public void AForeignScopeOpenAtPostIsAppliedPostRereadFailedWithItsCauseAndTheWarnings()
    {
        var setup = new FullUpdateSetup();
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
        var setup = new FullUpdateSetup();
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

    [Fact]
    public void AnOuterReleaseThatThrowsAfterPostTurnsAppliedIntoAppliedWithWarningsAndKeepsTheOtherWarnings()
    {
        var plain = new FullUpdateSetup();
        plain.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var outcome = plain.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        Assert.Contains(outcome.Warnings, w => w.Contains("outer boom", StringComparison.Ordinal));

        var warned = new FullUpdateSetup();
        warned.Port.MissingPieces["C3"] = new[] { "PIEZA_Z" };
        warned.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var second = warned.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, second.Kind);
        Assert.Equal(2, second.Warnings.Count);
        Assert.True(second.PostRead!.IsAdmitted);

        // Both end Clean: the warnings do not change the status of the draft.
        Assert.Equal(DraftStatus.Clean, StatusAfter(plain, outcome));
    }

    private static DraftStatus StatusAfter(FullUpdateSetup setup, DraftApplyOutcome<PushBackFullDraftState> outcome)
    {
        setup.Draft.CompleteApply(outcome);
        return setup.Draft.Status;
    }

    [Fact]
    public void AnOuterReleaseThatThrowsOutsidePostAddsItsCauseToTheClassificationThatWasAlreadyDecided()
    {
        var preflight = new FullUpdateSetup();
        preflight.Port.DocumentMatches = false;
        preflight.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var first = preflight.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, first.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.DocumentMismatchReason, Cause(first), StringComparison.Ordinal);
        Assert.Contains("outer boom", Cause(first), StringComparison.Ordinal);

        var failure = new FullUpdateSetup();
        failure.Port.ThrowAt["PrepareUnits"] = Boom("import boom");
        failure.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var second = failure.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, second.Kind);
        Assert.Contains("import boom", Cause(second), StringComparison.Ordinal);
        Assert.Contains("outer boom", Cause(second), StringComparison.Ordinal);

        var committed = new FullUpdateSetup();
        committed.Port.ThrowAt["Seal"] = Boom("seal boom");
        committed.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");
        var third = committed.Run();
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, third.Kind);
        Assert.Contains("outer boom", Cause(third), StringComparison.Ordinal);

        var abort = new FullUpdateSetup();
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
        var setup = new FullUpdateSetup();
        setup.Port.RereadDrawing = setup.Members.Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", FullPrepareKit.Planta("pb-02-modulos")) : m).ToArray();
        setup.Port.ThrowAt["Dispose:scope"] = Boom("outer boom");

        var outcome = setup.Run();

        // The declared limitation of the package: the exception of the release is lost, the classification is not.
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, outcome.Kind);
        Assert.Null(outcome.Cause);
    }

    [Fact]
    public void AConflictOrAPreflightOfMutateStaysWhatItIsEvenWhenTheReleaseOfTheScopeOfMutateThrows()
    {
        var conflict = new FullUpdateSetup();
        conflict.Port.RereadDrawing = conflict.Members.Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", FullPrepareKit.Planta("pb-02-modulos")) : m).ToArray();
        conflict.Port.ThrowAt["Dispose:mutation"] = Boom("release boom");
        var first = conflict.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, first.Kind);
        Assert.NotEmpty(first.Differences);

        var preflight = new FullUpdateSetup();
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
        var setup = new FullUpdateSetup();
        setup.Port.ThrowAt["Seal"] = Boom("seal boom");
        setup.Port.ThrowAt["Dispose:mutation"] = Boom("abort boom");

        var outcome = setup.Run();

        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, outcome.Kind);
        Assert.StartsWith(PushBackPilotUpdateRun.SealFailedPrefix, Cause(outcome), StringComparison.Ordinal);
        Assert.Contains("abort boom", Cause(outcome), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRunNeverThrowsForMissingArgumentsOrForAPortThatAnswersNothing()
    {
        var setup = new FullUpdateSetup();

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.ExecuteFull(null!, setup.Request, setup.Port).Kind);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.ExecuteFull(setup.Draft, null!, setup.Port).Kind);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, PushBackPilotUpdateRun.ExecuteFull(setup.Draft, setup.Request, null!).Kind);

        var noScope = new FullUpdateSetup();
        noScope.Port.ReturnNullScope = true;
        var outcome = noScope.Run();
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, outcome.Kind);
        Assert.Equal(PushBackPilotUpdateRun.NoScopeReason, outcome.Cause);

        var noStart = new FullUpdateSetup();
        noStart.Port.NullMutationStart = true;
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedFailure, noStart.Run().Kind);
        Assert.Contains("Dispose:scope", noStart.Port.Calls);
    }

    private static PushBackPilotIntentScan NamedScan(string name) => FullPrepareKit.Scan(
        IntentKit.Member("A1", FullPrepareKit.Frontal(name: name)),
        IntentKit.Member("B2", FullPrepareKit.Lateral(FullPrepareKit.ExistingPost(), name: name)),
        IntentKit.Member("C3", FullPrepareKit.Planta(name: name)));

    [Fact]
    public void ANameThatDiffersAfterTheRenameIsAWarningAndACaseOnlyDifferenceIsNot()
    {
        var differs = new FullUpdateSetup(() => NamedScan("Mi rack"));
        differs.Port.NameAfterPost = (key, desired) => key == "C3" ? "Ocupado" : desired;
        var outcome = differs.Run();
        Assert.Equal(DraftApplyOutcomeKind.AppliedWithWarnings, outcome.Kind);
        var warning = Assert.Single(outcome.Warnings);
        Assert.Contains("C3", warning, StringComparison.Ordinal);

        var sameCase = new FullUpdateSetup(() => NamedScan("Mi rack"));
        sameCase.Port.NameAfterPost = (key, desired) => desired.ToUpperInvariant();
        Assert.Equal(DraftApplyOutcomeKind.Applied, sameCase.Run().Kind);
    }

    [Fact]
    public void TheMissingPiecesAndAFailedRegenerationAreWarningsAndTheStateStaysClean()
    {
        var setup = new FullUpdateSetup();
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
        var setup = new FullUpdateSetup();
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

    // I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md): by hand over the map. The comparison of POST of the full model, ComparePostFull,
    // over the full read of each re-read (the comparison of the pilot, ComparePost, is retired); the cases and the causes are the same.
    [Fact]
    public void ThePostComparisonFailsForAnotherSetOfKeysAnotherEnvelopeAndAnotherScale()
    {
        var plan = new FullUpdateSetup().ExpectedPlan();
        var good = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).ToArray()));
        Assert.Null(PushBackPilotUpdateRun.ComparePostFull(plan, good));

        var fewer = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "A1", plan.Members.Take(2).Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).ToArray()));
        Assert.Equal(PushBackPilotUpdateRun.PostKeysDifferReason, PushBackPilotUpdateRun.ComparePostFull(plan, fewer));

        var more = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.EnvelopeText)).Append(IntentKit.Member("D4", plan.Envelope("C3")!)).ToArray()));
        Assert.Equal(PushBackPilotUpdateRun.PostKeysDifferReason, PushBackPilotUpdateRun.ComparePostFull(plan, more));

        var edited = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "A1", plan.Members.Select(m => IntentKit.Member(m.Key, m.Key == "C3" ? m.EnvelopeText + " " : m.EnvelopeText)).ToArray()));
        Assert.StartsWith(PushBackPilotUpdateRun.PostEnvelopeDiffersPrefix + "C3", PushBackPilotUpdateRun.ComparePostFull(plan, edited), StringComparison.Ordinal);

        Assert.Equal(PushBackPilotUpdateRun.PostRereadMissingReason, PushBackPilotUpdateRun.ComparePostFull(plan, null));
        var notAdmitted = FullWriteKit.Read(IntentKit.Scan(FullPrepareKit.Rack, "ZZ", IntentKit.Member("A1", plan.Envelope("A1")!)));
        Assert.StartsWith(PushBackPilotUpdateRun.PostNotAdmittedPrefix, PushBackPilotUpdateRun.ComparePostFull(plan, notAdmitted), StringComparison.Ordinal);
    }
}

/// <summary>
/// I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md): the copy of PilotUpdateSetup for the FULL model, the helper of the substitutes of
/// the run: the standard rack, a full draft born from the full read with an edited scale, and the double of the port; the plan is the one of
/// the full PREPARE and the run is ExecuteFull.
/// </summary>
internal sealed class FullUpdateSetup
{
    public const double NewScale = 2.25;

    public FullUpdateSetup(Func<PushBackPilotIntentScan>? scan = null, double scale = NewScale)
    {
        Scan = (scan ?? (() => FullPrepareKit.RealScan()))();
        var read = FullWriteKit.Read(Scan);
        Assert.True(read.Read.IsAdmitted);
        Draft = FullWriteKit.DraftOf(read);
        FullWriteKit.Edit(Draft, s => s.Editable.AnnotationScale = scale);
        Members = Scan.Facts.Select(f => new IntentItem(f, Scan.RawEnvelopes[f.DefinitionKey])).ToArray();
        Port = new PilotUpdateScopeDouble(Members);
        Request = new PushBackPilotUpdateRequest(100, FullPrepareKit.Rack);
    }

    public PushBackPilotIntentScan Scan { get; }

    public DraftSession<PushBackFullDraftState> Draft { get; }

    public IntentItem[] Members { get; }

    public PilotUpdateScopeDouble Port { get; }

    public PushBackPilotUpdateRequest Request { get; }

    /// <summary>The plan that the run builds for the standard rack, computed apart with the pure full PREPARE.</summary>
    public PushBackPilotUpdatePlan ExpectedPlan()
    {
        var result = PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(Draft), FullWriteKit.Read(Scan), PilotKit.Catalog);
        Assert.True(result.Kind == PushBackPilotPrepareKind.Ready, result.Reason);
        return result.Plan!;
    }

    /// <summary>Takes the slot like the host does, and runs.</summary>
    public DraftApplyOutcome<PushBackFullDraftState> Run()
    {
        Assert.True(Draft.TryBeginApply().IsStarted);
        return PushBackPilotUpdateRun.ExecuteFull(Draft, Request, Port);
    }
}
