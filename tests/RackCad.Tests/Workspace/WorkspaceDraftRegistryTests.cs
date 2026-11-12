#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T1-DRAFT-CORE: INV-F4T1-03, 12, 13. The registry by (SessionId, RackId), its retirement with the loss notice, and the
// veto policy of the close of a document, all from memory.
public class WorkspaceDraftRegistryTests
{
    // ------------------------------------------------------------------ birth only from an admitted read (INV-F4T1-03)

    [Fact]
    public void AnAdmittedReadCreatesTheDraftWithTheBaseAndTheInitialStateOfThatSameRead()
    {
        var registry = new DraftRegistry();
        var staleBase = DraftKit.DefaultBase();
        var read = DraftKit.Read(DraftKit.Form("Rack", 30), staleBase);

        var result = registry.Begin(DraftKit.S1, "R1", read, DraftKit.Comparer, DraftKit.CopyForm);

        Assert.Equal(DraftBeginStatus.Created, result.Status);
        Assert.NotNull(result.Draft);
        Assert.True(result.Admission.IsAdmitted);
        Assert.Same(staleBase, result.Draft!.StaleBase);
        Assert.NotSame(read.InitialState, result.Draft.InitialDraftState);
        Assert.Equal(read.InitialState, result.Draft.InitialDraftState);
        Assert.NotSame(result.Draft.InitialDraftState, result.Draft.CurrentDraftState);
        Assert.True(DraftKit.Comparer.Equals(read.InitialState, result.Draft.CurrentDraftState));
        Assert.True(registry.Contains(DraftKit.S1, "R1"));
        Assert.Same(result.Draft, registry.Find<DraftForm>(DraftKit.S1, "R1"));
        Assert.Equal(1, registry.CountOf(DraftKit.S1));
    }

    [Fact]
    public void ANotAdmittedReadCreatesNoDraftAndReturnsTheAdmissionDiagnosticAsAValue()
    {
        var registry = new DraftRegistry();

        var rejected = registry.Begin(DraftKit.S1, "R1", DraftIntentRead<DraftForm>.NotAdmitted("divergent members"), DraftKit.Comparer, DraftKit.CopyForm);

        Assert.Equal(DraftBeginStatus.NotAdmitted, rejected.Status);
        Assert.Null(rejected.Draft);
        Assert.False(rejected.Admission.IsAdmitted);
        Assert.Equal("divergent members", rejected.Admission.Diagnostic);
        Assert.False(registry.Contains(DraftKit.S1, "R1"));
        Assert.Equal(0, registry.CountOf(DraftKit.S1));

        // The same registry still admits a later read that is admitted.
        var created = registry.Begin(DraftKit.S1, "R1", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm);
        Assert.Equal(DraftBeginStatus.Created, created.Status);
        Assert.Equal(1, registry.CountOf(DraftKit.S1));
    }

    [Fact]
    public void BeginningAgainForTheSameRackReturnsTheExistingDraftUntouched()
    {
        var registry = new DraftRegistry();
        var draft = DraftKit.Dirty(registry);
        var current = draft.CurrentDraftState;

        var again = registry.Begin(DraftKit.S1, "R1", DraftKit.Read(DraftKit.Form("Replacement", 1)), DraftKit.Comparer, DraftKit.CopyForm);

        Assert.Equal(DraftBeginStatus.AlreadyExists, again.Status);
        Assert.Same(draft, again.Draft);
        Assert.Same(current, draft.CurrentDraftState);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Equal(1, registry.CountOf(DraftKit.S1));
    }

    [Fact]
    public void ADraftHasNoPublicConstructorAndOnlyTheRegistryBeginCreatesOne()
    {
        Assert.Empty(typeof(DraftSession<DraftForm>).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(typeof(DraftIntentRead<DraftForm>).GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        var producers = typeof(DraftRegistry).Assembly.GetTypes()
            .Where(t => t.Namespace == "RackCad.Application.Workspace.Drafts" && t.IsPublic)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(DraftSession<>))
            .Select(m => m.DeclaringType!.Name + "." + m.Name)
            .Where(n => !n.StartsWith("DraftBeginResult", StringComparison.Ordinal))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(new[] { "DraftRegistry.Find" }, producers);

        var born = typeof(DraftRegistry).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.Name == "Begin").ToArray();
        Assert.Single(born);
        Assert.Equal(typeof(DraftIntentRead<>), born[0].GetParameters()[2].ParameterType.GetGenericTypeDefinition());
    }

    [Fact]
    public void BeginTakesTheCopierNextToTheComparerAndTheCurrentStateIsACopyOfTheInitialOne()
    {
        var begin = typeof(DraftRegistry).GetMethod("Begin")!;
        var parameters = begin.GetParameters();

        Assert.Equal(5, parameters.Length);
        Assert.Equal(typeof(DraftIntentRead<>), parameters[2].ParameterType.GetGenericTypeDefinition());
        Assert.Equal(typeof(IEqualityComparer<>), parameters[3].ParameterType.GetGenericTypeDefinition());
        Assert.Equal(typeof(Func<,>), parameters[4].ParameterType.GetGenericTypeDefinition());

        var copies = 0;
        var registry = new DraftRegistry();
        var read = DraftKit.Read(DraftKit.Form("Rack", 30));

        var born = registry.Begin(DraftKit.S1, "R1", read, DraftKit.Comparer, form => { copies++; return form with { }; });

        Assert.Equal(DraftBeginStatus.Created, born.Status);
        Assert.True(copies >= 1);
        Assert.NotSame(born.Draft!.InitialDraftState, born.Draft.CurrentDraftState);
    }

    [Fact]
    public void NoOperationOfTheCoreButTheReadFactoryAndCheckNowTakesAStaleBaseAlone()
    {
        var types = typeof(DraftRegistry).Assembly.GetTypes().Where(t => t.Namespace == "RackCad.Application.Workspace.Drafts" && t.IsPublic).ToArray();

        // The only way to a read that can create a draft: the two factories of DraftIntentRead.
        var readFactories = typeof(DraftIntentRead<DraftForm>).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "Admitted", "NotAdmitted" }, readFactories);

        // The base alone is never accepted by an operation that creates, replaces or reloads a draft.
        foreach (var type in types)
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (type == typeof(DraftStaleBase) || method.Name == "CheckNow") continue;
                Assert.DoesNotContain(method.GetParameters(), p => p.ParameterType == typeof(DraftStaleBase) && method.Name != "Admitted");
            }

        Assert.Contains(types, t => t.Name == "DraftRegistry");
    }

    [Fact]
    public void NullOrEmptyArgumentsToTheRegistryAreProgrammingErrors()
    {
        var registry = new DraftRegistry();

        Assert.Throws<ArgumentException>(() => registry.Begin(DraftKit.S1, "", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm));
        Assert.Throws<ArgumentException>(() => registry.Begin(DraftKit.S1, null!, DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm));
        Assert.Throws<ArgumentNullException>(() => registry.Begin(DraftKit.S1, "R1", null!, DraftKit.Comparer, DraftKit.CopyForm));
        Assert.Throws<ArgumentNullException>(() => registry.Begin(DraftKit.S1, "R1", DraftKit.Read(), null!, DraftKit.CopyForm));
        Assert.Throws<ArgumentNullException>(() => registry.Begin(DraftKit.S1, "R1", DraftKit.Read(), DraftKit.Comparer, null!));
        Assert.Throws<ArgumentException>(() => registry.Find<DraftForm>(DraftKit.S1, ""));
        Assert.Throws<ArgumentException>(() => registry.Contains(DraftKit.S1, ""));
        Assert.Throws<ArgumentException>(() => registry.Discard(DraftKit.S1, ""));
        Assert.Throws<ArgumentNullException>(() => new DraftClosePolicy(null!));
        Assert.Equal(0, registry.CountOf(DraftKit.S1));
    }

    // ------------------------------------------------------------------ keys and isolation (INV-F4T1-12)

    [Fact]
    public void TheSameRackIdInTwoSessionsGivesTwoIndependentDrafts()
    {
        var registry = new DraftRegistry();
        var one = DraftKit.Born(registry, DraftKit.S1, "R1");
        var two = DraftKit.Born(registry, DraftKit.S2, "R1");

        one.SetCurrentDraftState(DraftKit.Form("Edited in S1", 1));
        one.SetPendingText("width", "4x");
        one.MarkPossiblyStale();

        Assert.NotSame(one, two);
        Assert.Equal(DraftStatus.Clean, two.Status);
        Assert.Empty(two.PendingText);
        Assert.False(two.IsPossiblyStale);
        Assert.Equal("Rack", two.CurrentDraftState.Name);
        Assert.Same(one, registry.Find<DraftForm>(DraftKit.S1, "R1"));
        Assert.Same(two, registry.Find<DraftForm>(DraftKit.S2, "R1"));

        registry.Discard(DraftKit.S1, "R1");
        Assert.True(registry.Contains(DraftKit.S2, "R1"));
        Assert.Equal(DraftStatus.Clean, two.Status);
    }

    [Fact]
    public void AnUpdateInOneSessionDoesNotTouchTheDraftOfTheSameRackInAnother()
    {
        var registry = new DraftRegistry();
        var one = DraftKit.Dirty(registry, DraftKit.S1, "R1");
        var two = DraftKit.Dirty(registry, DraftKit.S2, "R1");
        var twoCurrent = two.CurrentDraftState;

        DraftKit.ToApplying(one);
        one.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(DraftKit.Read(DraftKit.Form("AsWritten", 5))));

        Assert.Equal(DraftStatus.Clean, one.Status);
        Assert.Equal(DraftStatus.Dirty, two.Status);
        Assert.Same(twoCurrent, two.CurrentDraftState);
    }

    [Fact]
    public void TheRackIdComparesOrdinally()
    {
        var registry = new DraftRegistry();
        var upper = DraftKit.Born(registry, DraftKit.S1, "Rack-A");
        var lower = DraftKit.Born(registry, DraftKit.S1, "rack-a");
        var composed = DraftKit.Born(registry, DraftKit.S1, "café");
        var decomposed = DraftKit.Born(registry, DraftKit.S1, "café");

        Assert.Equal(4, registry.CountOf(DraftKit.S1));
        Assert.NotSame(upper, lower);
        Assert.NotSame(composed, decomposed);
        Assert.Same(lower, registry.Find<DraftForm>(DraftKit.S1, "rack-a"));
        Assert.Null(registry.Find<DraftForm>(DraftKit.S1, "RACK-A"));
        Assert.Equal(new[] { "Rack-A", "café", "café", "rack-a" }, registry.RackIdsOf(DraftKit.S1).ToArray());
    }

    [Fact]
    public void FindReturnsNullForAnUnknownRackAnUnknownSessionOrAnotherStateType()
    {
        var registry = new DraftRegistry();
        DraftKit.Born(registry);

        Assert.Null(registry.Find<DraftForm>(DraftKit.S1, "other"));
        Assert.Null(registry.Find<DraftForm>(DraftKit.S2, "R1"));
        Assert.Null(registry.Find<string>(DraftKit.S1, "R1"));
        Assert.False(registry.Contains(DraftKit.S2, "R1"));
    }

    // ------------------------------------------------------------------ discard and surfaces

    [Fact]
    public void DiscardRemovesTheDraftWithoutWritingAnythingAndWithoutTouchingTheOthers()
    {
        var registry = new DraftRegistry();
        var dirty = DraftKit.Dirty(registry, DraftKit.S1, "R1");
        dirty.SetPendingText("width", "4x");
        var other = DraftKit.Born(registry, DraftKit.S1, "R2");
        var adapter = new DraftFormAdapter();

        var result = registry.Discard(DraftKit.S1, "R1");

        Assert.True(result.Removed);
        Assert.True(result.WasDirty);
        Assert.False(registry.Contains(DraftKit.S1, "R1"));
        Assert.True(registry.Contains(DraftKit.S1, "R2"));
        Assert.Same(other, registry.Find<DraftForm>(DraftKit.S1, "R2"));
        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Empty(adapter.Validated);
        Assert.False(registry.IsRetired(DraftKit.S1));

        var again = registry.Discard(DraftKit.S1, "R1");
        Assert.False(again.Removed);
        Assert.False(again.WasDirty);
    }

    [Fact]
    public void ADiscardedDraftCanBeBornAgainFreshAndTheOldOneIsDetached()
    {
        var registry = new DraftRegistry();
        var old = DraftKit.Dirty(registry);
        registry.Discard(DraftKit.S1, "R1");

        var fresh = DraftKit.Born(registry);

        Assert.NotSame(old, fresh);
        Assert.False(old.IsRegistered);
        Assert.True(fresh.IsRegistered);
        Assert.Equal(DraftStatus.Clean, fresh.Status);
        Assert.Equal(DraftEditStatus.NotEditable, old.SetCurrentDraftState(DraftKit.Form("late", 1)).Status);
        Assert.Equal(DraftMarkResult.NotRegistered, old.MarkPossiblyStale());
        Assert.Equal(DraftApplyRefusal.NotRegistered, old.TryBeginApply().Refusal);
    }

    [Fact]
    public void DiscardingACleanDraftReportsThatItWasNotDirty()
    {
        var registry = new DraftRegistry();
        DraftKit.Born(registry);

        var result = registry.Discard(DraftKit.S1, "R1");

        Assert.True(result.Removed);
        Assert.False(result.WasDirty);
    }

    [Fact]
    public void DiscardAllRemovesEveryDraftOfTheSessionWithoutRetiringItAndKeepsTheOtherSession()
    {
        var registry = new DraftRegistry();
        DraftKit.Dirty(registry, DraftKit.S1, "B");
        DraftKit.Born(registry, DraftKit.S1, "A");
        DraftKit.Born(registry, DraftKit.S2, "A");

        var result = registry.DiscardAll(DraftKit.S1);

        Assert.Equal(new[] { "A", "B" }, result.RemovedRackIds.ToArray());
        Assert.Equal(new[] { "B" }, result.DirtyRackIds.ToArray());
        Assert.Equal(0, registry.CountOf(DraftKit.S1));
        Assert.Equal(1, registry.CountOf(DraftKit.S2));
        Assert.False(registry.IsRetired(DraftKit.S1));
        Assert.Equal(DraftBeginStatus.Created, registry.Begin(DraftKit.S1, "A", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm).Status);
        Assert.Empty(registry.DiscardAll(new SessionId(99)).RemovedRackIds);
    }

    [Fact]
    public void DraftsDoNotDependOnAnySurfaceSoTheySurviveEveryNonBlockingBoundary()
    {
        var registry = new DraftRegistry();
        var draft = DraftKit.Dirty(registry);
        draft.SetPendingText("width", "4x");
        var adapter = new DraftFormAdapter();

        foreach (var boundary in new[] { DraftBoundary.DocumentChange, DraftBoundary.HidePanel, DraftBoundary.Dematerialize, DraftBoundary.DocumentClose })
            draft.CrossBoundary(boundary, adapter);

        Assert.Same(draft, registry.Find<DraftForm>(DraftKit.S1, "R1"));
        Assert.True(draft.IsRegistered);
        Assert.Equal("4x", draft.PendingText["width"]);
        Assert.Equal("Edited", draft.CurrentDraftState.Name);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Empty(adapter.Validated);
    }

    // ------------------------------------------------------------------ retirement with the loss notice (INV-F4T1-12)

    [Fact]
    public void RetiringASessionRemovesAllItsDraftsAndNamesTheRacksThatWereDirtyOrHadPendingText()
    {
        var registry = new DraftRegistry();
        DraftKit.Dirty(registry, DraftKit.S1, "R-dirty");
        DraftKit.Born(registry, DraftKit.S1, "R-clean");
        var pendingOnly = DraftKit.Born(registry, DraftKit.S1, "R-pending");
        pendingOnly.SetPendingText("width", "4x");
        var untouched = DraftKit.Dirty(registry, DraftKit.S2, "R-other-session");

        var notice = registry.RetireSession(DraftKit.S1);

        Assert.True(notice.HasLoss);
        Assert.Equal(DraftKit.S1, notice.Session);
        Assert.Equal(new[] { "R-dirty", "R-pending" }, notice.LostRackIds.ToArray());
        Assert.Equal(0, registry.CountOf(DraftKit.S1));
        Assert.Empty(registry.RackIdsOf(DraftKit.S1));
        Assert.Same(untouched, registry.Find<DraftForm>(DraftKit.S2, "R-other-session"));
        Assert.Equal(DraftStatus.Dirty, untouched.Status);
    }

    [Fact]
    public void RetiringASessionWithOnlyCleanDraftsGivesNoLossNotice()
    {
        var registry = new DraftRegistry();
        DraftKit.Born(registry, DraftKit.S1, "A");
        DraftKit.Born(registry, DraftKit.S1, "B");

        var notice = registry.RetireSession(DraftKit.S1);

        Assert.False(notice.HasLoss);
        Assert.Empty(notice.LostRackIds);
        Assert.Equal(0, registry.CountOf(DraftKit.S1));
        Assert.True(registry.IsRetired(DraftKit.S1));
    }

    [Fact]
    public void IfTheComparerThrowsWhileRetiringTheSessionIsNotLeftMarkedAsRetiredAndItsDraftsStayRegistered()
    {
        var registry = new DraftRegistry();
        var comparer = new DraftThrowingComparer();
        var risky = DraftKit.Born(registry, DraftKit.S1, "R1", comparer: comparer);
        var other = DraftKit.Born(registry, DraftKit.S1, "R2");
        risky.SetCurrentDraftState(DraftKit.Form("Edited", 1));
        comparer.Throw = true;

        Assert.Throws<InvalidOperationException>(() => registry.RetireSession(DraftKit.S1));
        comparer.Throw = false;

        Assert.False(registry.IsRetired(DraftKit.S1));
        Assert.Equal(2, registry.CountOf(DraftKit.S1));
        Assert.True(risky.IsRegistered);
        Assert.True(other.IsRegistered);
        Assert.Same(risky, registry.Find<DraftForm>(DraftKit.S1, "R1"));
        Assert.Equal(DraftBeginStatus.AlreadyExists, registry.Begin(DraftKit.S1, "R1", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm).Status);

        var notice = registry.RetireSession(DraftKit.S1);

        Assert.True(registry.IsRetired(DraftKit.S1));
        Assert.Equal(new[] { "R1" }, notice.LostRackIds.ToArray());
        Assert.Equal(0, registry.CountOf(DraftKit.S1));
        Assert.False(risky.IsRegistered);
    }

    [Fact]
    public void ARetiredSessionIsNeverResurrected()
    {
        var registry = new DraftRegistry();
        var draft = DraftKit.Dirty(registry);
        registry.RetireSession(DraftKit.S1);

        var again = registry.Begin(DraftKit.S1, "R1", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm);

        Assert.Equal(DraftBeginStatus.SessionRetired, again.Status);
        Assert.Null(again.Draft);
        Assert.False(registry.Contains(DraftKit.S1, "R1"));
        Assert.False(draft.IsRegistered);
        Assert.True(registry.IsRetired(DraftKit.S1));
        var second = registry.RetireSession(DraftKit.S1);
        Assert.False(second.HasLoss);
        Assert.Equal(DraftBeginStatus.Created, registry.Begin(DraftKit.S2, "R1", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm).Status);
    }

    [Fact]
    public void RetiringASessionNeverWritesAnythingNorRunsTheValidatorOrTheApplication()
    {
        var registry = new DraftRegistry();
        var draft = DraftKit.Dirty(registry);
        draft.SetPendingText("width", "4x");
        var adapter = new DraftFormAdapter();

        registry.RetireSession(DraftKit.S1);
        registry.Discard(DraftKit.S1, "R1");

        Assert.Equal(0, adapter.ApplyCalls);
        Assert.Empty(adapter.Validated);
        Assert.Equal("Edited", draft.CurrentDraftState.Name);
    }

    // ------------------------------------------------------------------ close veto (INV-F4T1-13)

    [Fact]
    public void ASessionWithNoDraftsOrOnlyCleanOnesIsAllowedToClose()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        Assert.Equal(DraftCloseVerdict.Allow, policy.EvaluateClose(DraftKit.S1).Verdict);

        DraftKit.Born(registry, DraftKit.S1, "A");
        var decision = policy.EvaluateClose(DraftKit.S1);

        Assert.Equal(DraftCloseVerdict.Allow, decision.Verdict);
        Assert.False(decision.IsVeto);
        Assert.Empty(decision.RackIds);
    }

    [Fact]
    public void ADirtyDraftVetoesTheCloseWithTheListOfRacks()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        DraftKit.Dirty(registry, DraftKit.S1, "B");
        DraftKit.Dirty(registry, DraftKit.S1, "A");
        DraftKit.Born(registry, DraftKit.S1, "C");

        var decision = policy.EvaluateClose(DraftKit.S1);

        Assert.Equal(DraftCloseVerdict.Veto, decision.Verdict);
        Assert.True(decision.IsVeto);
        Assert.Equal(new[] { "A", "B" }, decision.RackIds.ToArray());
    }

    [Fact]
    public void PendingTextAloneVetoesTheClose()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        var draft = DraftKit.Born(registry);
        Assert.Equal(DraftCloseVerdict.Allow, policy.EvaluateClose(DraftKit.S1).Verdict);

        draft.SetPendingText("width", "4x");

        var decision = policy.EvaluateClose(DraftKit.S1);
        Assert.Equal(DraftCloseVerdict.Veto, decision.Verdict);
        Assert.Equal(new[] { "R1" }, decision.RackIds.ToArray());
    }

    [Fact]
    public void RepeatingTheAttemptIsNeverConsentAndEveryAttemptIsEvaluatedAnew()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        DraftKit.Dirty(registry);

        for (var attempt = 0; attempt < 5; attempt++)
            Assert.Equal(DraftCloseVerdict.Veto, policy.EvaluateClose(DraftKit.S1).Verdict);

        Assert.Equal(1, registry.CountOf(DraftKit.S1));
        Assert.Equal(DraftStatus.Dirty, registry.Find<DraftForm>(DraftKit.S1, "R1")!.Status);
    }

    [Fact]
    public void AfterDiscardAllTheNextCloseIsAllowed()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        DraftKit.Dirty(registry, DraftKit.S1, "A");
        DraftKit.Born(registry, DraftKit.S1, "B").SetPendingText("width", "4x");
        Assert.Equal(DraftCloseVerdict.Veto, policy.EvaluateClose(DraftKit.S1).Verdict);
        Assert.Equal(DraftCloseVerdict.Veto, policy.EvaluateClose(DraftKit.S1).Verdict);

        registry.DiscardAll(DraftKit.S1);

        var decision = policy.EvaluateClose(DraftKit.S1);
        Assert.Equal(DraftCloseVerdict.Allow, decision.Verdict);
        Assert.Empty(decision.RackIds);
    }

    [Fact]
    public void ADraftThatBecomesDirtyAfterAnAllowedCloseVetoesTheNextOne()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        var draft = DraftKit.Born(registry);
        Assert.Equal(DraftCloseVerdict.Allow, policy.EvaluateClose(DraftKit.S1).Verdict);

        draft.SetCurrentDraftState(DraftKit.Form("late edit", 1));

        Assert.Equal(DraftCloseVerdict.Veto, policy.EvaluateClose(DraftKit.S1).Verdict);
    }

    [Fact]
    public void ACleanDraftAfterAnAppliedUpdateAllowsTheClose()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        var draft = DraftKit.Dirty(registry);
        Assert.Equal(DraftCloseVerdict.Veto, policy.EvaluateClose(DraftKit.S1).Verdict);

        DraftKit.ToApplying(draft);
        draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(DraftKit.Read(DraftKit.Form("AsWritten", 5))));

        Assert.Equal(DraftCloseVerdict.Allow, policy.EvaluateClose(DraftKit.S1).Verdict);
    }

    [Theory]
    [InlineData("conflict")]
    [InlineData("unknown-commit")]
    [InlineData("unknown-post")]
    [InlineData("error")]
    [InlineData("applying")]
    public void ADirtyDraftVetoesTheCloseWhateverItsStatus(string status)
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        var draft = DraftKit.Dirty(registry);
        switch (status)
        {
            case "conflict": DraftKit.ToConflict(draft); break;
            case "unknown-commit": DraftKit.ToUnknownCommit(draft); break;
            case "unknown-post": DraftKit.ToUnknownPost(draft); break;
            case "error": DraftKit.ToError(draft); break;
            default: DraftKit.ToApplying(draft); break;
        }

        Assert.Equal(DraftCloseVerdict.Veto, policy.EvaluateClose(DraftKit.S1).Verdict);
    }

    [Fact]
    public void TheDraftsOfAnotherSessionNeverVetoThisOnesClose()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        DraftKit.Dirty(registry, DraftKit.S2, "R1");
        DraftKit.Born(registry, DraftKit.S1, "R1");

        Assert.Equal(DraftCloseVerdict.Allow, policy.EvaluateClose(DraftKit.S1).Verdict);
        Assert.Equal(DraftCloseVerdict.Veto, policy.EvaluateClose(DraftKit.S2).Verdict);
    }

    [Fact]
    public void TheDecisionUsesOnlyTheInMemoryDraftsAndNothingAboutTheDegradedModeOfTheSession()
    {
        var evaluate = typeof(DraftClosePolicy).GetMethod("EvaluateClose");
        Assert.NotNull(evaluate);
        Assert.Equal(new[] { typeof(SessionId) }, evaluate!.GetParameters().Select(p => p.ParameterType).ToArray());

        var fields = typeof(DraftClosePolicy).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.Equal(new[] { typeof(DraftRegistry) }, fields.Select(f => f.FieldType).ToArray());
        Assert.Single(typeof(DraftClosePolicy).GetConstructors());
        Assert.Equal(new[] { typeof(DraftRegistry) }, typeof(DraftClosePolicy).GetConstructors()[0].GetParameters().Select(p => p.ParameterType).ToArray());

        var registry = new DraftRegistry();
        DraftKit.Dirty(registry);
        var first = new DraftClosePolicy(registry).EvaluateClose(DraftKit.S1);
        var second = new DraftClosePolicy(registry).EvaluateClose(DraftKit.S1);
        Assert.Equal(first.Verdict, second.Verdict);
        Assert.Equal(first.RackIds.ToArray(), second.RackIds.ToArray());
    }

    [Fact]
    public void RetiringTheSessionAfterAVetoedCloseLeavesNothingToVeto()
    {
        var registry = new DraftRegistry();
        var policy = new DraftClosePolicy(registry);
        DraftKit.Dirty(registry);
        Assert.Equal(DraftCloseVerdict.Veto, policy.EvaluateClose(DraftKit.S1).Verdict);

        var notice = registry.RetireSession(DraftKit.S1);

        Assert.True(notice.HasLoss);
        Assert.Equal(DraftCloseVerdict.Allow, policy.EvaluateClose(DraftKit.S1).Verdict);
    }
}
