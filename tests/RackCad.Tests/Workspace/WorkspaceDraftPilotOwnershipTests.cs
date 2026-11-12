#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4-T2A (N-2, INV-F4T2A-10, 11): the ownership boundary of the draft core. The scenarios run over a MUTABLE test
/// state and over the pilot state, built from the same recipe: <see cref="Make"/> gives a state whose content is the number
/// it is given, <see cref="Mutate"/> changes it in place and <see cref="Fingerprint"/> reads its content.
/// </summary>
internal sealed class OwnershipKit<TState>
    where TState : class
{
    public OwnershipKit(
        Func<int, TState> make, Action<TState> mutate, Func<TState, string> fingerprint, IEqualityComparer<TState> comparer, Func<TState, TState> copy)
    {
        Make = make;
        Mutate = mutate;
        Fingerprint = fingerprint;
        Comparer = comparer;
        Copy = copy;
    }

    public Func<int, TState> Make { get; }

    public Action<TState> Mutate { get; }

    public Func<TState, string> Fingerprint { get; }

    public IEqualityComparer<TState> Comparer { get; }

    public Func<TState, TState> Copy { get; }

    public DraftIntentRead<TState> Read(TState state) => DraftIntentRead<TState>.Admitted(DraftKit.DefaultBase(), state);

    public DraftSession<TState> Born(DraftRegistry registry, TState state, SessionId? session = null, string rack = "R1")
    {
        var result = registry.Begin(session ?? DraftKit.S1, rack, Read(state), Comparer, Copy);
        Assert.Equal(DraftBeginStatus.Created, result.Status);
        return result.Draft!;
    }
}

internal static class OwnershipKits
{
    public static OwnershipKit<DraftBox> Box() => new(
        make: n => new DraftBox { Name = "Rack", Width = n, Source = "{box}" },
        mutate: box => box.Width += 100,
        fingerprint: box => box.Name + "|" + box.Width + "|" + box.Source,
        comparer: DraftKit.BoxComparer,
        copy: DraftKit.CopyBox);

    public static OwnershipKit<PushBackPilotDraftState> Pilot() => new(
        make: n =>
        {
            var state = PilotKit.State("pb-01-base");
            state.Inputs.Annotations.AnnotationScale = 1.0 + n / 4.0;
            return state;
        },
        mutate: state => state.Inputs.Annotations.AnnotationScale += 100.0,
        fingerprint: state => state.Inputs.Annotations.AnnotationScale.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
        comparer: new PushBackPilotStateComparer(),
        copy: PushBackPilotStateComparer.Copy);
}

// I-64 F4-T2A-PILOT-STATE: INV-F4T2A-10 (the core copies what enters and the initial state it hands out) and 11 (adversarial).
public class WorkspaceDraftPilotOwnershipTests
{
    public static IEnumerable<object[]> Kinds() => new[] { new object[] { "box" }, new object[] { "pilot" } };

    private static void Both(string kind, Action<IOwnership> scenario)
    {
        if (kind == "box") scenario(new Ownership<DraftBox>(OwnershipKits.Box()));
        else scenario(new Ownership<PushBackPilotDraftState>(OwnershipKits.Pilot()));
    }

    private interface IOwnership
    {
        void MutatingTheReadObjectAfterBeginChangesNothing();

        void MutatingTheReadObjectAfterDiscardAndReloadChangesNothing();

        void MutatingTheReadObjectAfterKeepMineChangesNothing();

        void MutatingThePostReadObjectAfterAnAppliedRebaseChangesNothing(bool withWarnings);

        void MutatingTheObjectPassedToSetCurrentDraftStateChangesNothing();

        void MutatingTheObjectReturnedByInitialDraftStateChangesNothing();

        void OneReadObjectForTwoDraftsLeavesThemIndependent();

        void InPlaceEditOfTheCurrentStateLeavesTheDraftDirtyAndVetoesTheClose();
    }

    private sealed class Ownership<TState> : IOwnership
        where TState : class
    {
        private readonly OwnershipKit<TState> _kit;

        public Ownership(OwnershipKit<TState> kit)
        {
            _kit = kit;
        }

        public void MutatingTheReadObjectAfterBeginChangesNothing()
        {
            var registry = new DraftRegistry();
            var source = _kit.Make(1);
            var draft = _kit.Born(registry, source);
            var before = _kit.Fingerprint(draft.InitialDraftState);

            _kit.Mutate(source);

            Assert.Equal(before, _kit.Fingerprint(draft.InitialDraftState));
            Assert.Equal(_kit.Fingerprint(_kit.Make(1)), _kit.Fingerprint(draft.InitialDraftState));
            Assert.Equal(_kit.Fingerprint(_kit.Make(1)), _kit.Fingerprint(draft.CurrentDraftState));
            Assert.False(draft.IsDirty);
            Assert.Equal(DraftStatus.Clean, draft.Status);
            Assert.Empty(registry.RacksAtRiskOf(DraftKit.S1));
        }

        public void MutatingTheReadObjectAfterDiscardAndReloadChangesNothing()
        {
            var draft = _kit.Born(new DraftRegistry(), _kit.Make(1));
            _kit.Mutate(draft.CurrentDraftState);
            Assert.True(draft.IsDirty);
            var fresh = _kit.Make(2);

            var result = draft.DiscardAndReload(_kit.Read(fresh));
            _kit.Mutate(fresh);

            Assert.Equal(DraftReloadStatus.Reloaded, result.Status);
            Assert.Equal(_kit.Fingerprint(_kit.Make(2)), _kit.Fingerprint(draft.InitialDraftState));
            Assert.Equal(_kit.Fingerprint(_kit.Make(2)), _kit.Fingerprint(draft.CurrentDraftState));
            Assert.False(draft.IsDirty);
        }

        public void MutatingTheReadObjectAfterKeepMineChangesNothing()
        {
            var draft = _kit.Born(new DraftRegistry(), _kit.Make(1));
            draft.SetCurrentDraftState(_kit.Make(5));
            var theirs = _kit.Make(3);

            var result = draft.KeepMineOnCurrentVersion(_kit.Read(theirs), confirmed: true);
            _kit.Mutate(theirs);

            Assert.Equal(DraftReloadStatus.Reloaded, result.Status);
            Assert.Equal(_kit.Fingerprint(_kit.Make(3)), _kit.Fingerprint(draft.InitialDraftState));
            Assert.Equal(_kit.Fingerprint(_kit.Make(5)), _kit.Fingerprint(draft.CurrentDraftState));
            Assert.True(draft.IsDirty);
        }

        public void MutatingThePostReadObjectAfterAnAppliedRebaseChangesNothing(bool withWarnings)
        {
            var draft = _kit.Born(new DraftRegistry(), _kit.Make(1));
            draft.SetCurrentDraftState(_kit.Make(5));
            Assert.True(draft.TryBeginApply().IsStarted);
            var post = _kit.Make(4);

            var result = draft.CompleteApply(withWarnings
                ? DraftApplyOutcome<TState>.AppliedWithWarnings(_kit.Read(post), new[] { "w" })
                : DraftApplyOutcome<TState>.Applied(_kit.Read(post)));
            _kit.Mutate(post);

            Assert.True(result.Rebased);
            Assert.Equal(_kit.Fingerprint(_kit.Make(4)), _kit.Fingerprint(draft.InitialDraftState));
            Assert.Equal(_kit.Fingerprint(_kit.Make(4)), _kit.Fingerprint(draft.CurrentDraftState));
            Assert.False(draft.IsDirty);
        }

        public void MutatingTheObjectPassedToSetCurrentDraftStateChangesNothing()
        {
            var draft = _kit.Born(new DraftRegistry(), _kit.Make(1));
            var edit = _kit.Make(6);

            Assert.Equal(DraftEditStatus.Updated, draft.SetCurrentDraftState(edit).Status);
            _kit.Mutate(edit);

            Assert.Equal(_kit.Fingerprint(_kit.Make(6)), _kit.Fingerprint(draft.CurrentDraftState));
            Assert.NotSame(edit, draft.CurrentDraftState);
            Assert.True(draft.IsDirty);
        }

        public void MutatingTheObjectReturnedByInitialDraftStateChangesNothing()
        {
            var draft = _kit.Born(new DraftRegistry(), _kit.Make(1));
            var returned = draft.InitialDraftState;

            _kit.Mutate(returned);

            Assert.Equal(_kit.Fingerprint(_kit.Make(1)), _kit.Fingerprint(draft.InitialDraftState));
            Assert.NotSame(returned, draft.InitialDraftState);
            Assert.NotSame(draft.InitialDraftState, draft.InitialDraftState);
            Assert.False(draft.IsDirty);
        }

        public void OneReadObjectForTwoDraftsLeavesThemIndependent()
        {
            var registry = new DraftRegistry();
            var shared = _kit.Make(1);
            var one = _kit.Born(registry, shared, DraftKit.S1, "R1");
            var two = _kit.Born(registry, shared, DraftKit.S1, "R2");
            var other = _kit.Born(registry, shared, DraftKit.S2, "R1");

            _kit.Mutate(one.CurrentDraftState);
            _kit.Mutate(shared);

            Assert.True(one.IsDirty);
            Assert.False(two.IsDirty);
            Assert.False(other.IsDirty);
            Assert.Equal(_kit.Fingerprint(_kit.Make(1)), _kit.Fingerprint(two.CurrentDraftState));
            Assert.Equal(_kit.Fingerprint(_kit.Make(1)), _kit.Fingerprint(two.InitialDraftState));
            Assert.Equal(_kit.Fingerprint(_kit.Make(1)), _kit.Fingerprint(other.InitialDraftState));
            Assert.NotSame(two.CurrentDraftState, other.CurrentDraftState);
            Assert.Equal(new[] { "R1" }, registry.RacksAtRiskOf(DraftKit.S1));
            Assert.Empty(registry.RacksAtRiskOf(DraftKit.S2));
        }

        public void InPlaceEditOfTheCurrentStateLeavesTheDraftDirtyAndVetoesTheClose()
        {
            var registry = new DraftRegistry();
            var draft = _kit.Born(registry, _kit.Make(1));
            Assert.False(draft.IsDirty);

            _kit.Mutate(draft.CurrentDraftState);

            Assert.True(draft.IsDirty);
            Assert.Equal(DraftStatus.Dirty, draft.Status);
            Assert.Equal(_kit.Fingerprint(_kit.Make(1)), _kit.Fingerprint(draft.InitialDraftState));
            Assert.Equal(new[] { "R1" }, registry.RacksAtRiskOf(DraftKit.S1));
            Assert.True(new DraftClosePolicy(registry).EvaluateClose(DraftKit.S1).IsVeto);
        }
    }

    // ------------------------------------------------------------------ INV-F4T2A-11 (a) to (e)

    [Theory]
    [MemberData(nameof(Kinds))]
    public void MutatingTheObjectOfTheReadAfterBeginChangesNeitherTheInitialStateNorDirty(string kind) =>
        Both(kind, scenario => scenario.MutatingTheReadObjectAfterBeginChangesNothing());

    [Theory]
    [MemberData(nameof(Kinds))]
    public void MutatingTheObjectOfTheReadAfterDiscardAndReloadChangesNothing(string kind) =>
        Both(kind, scenario => scenario.MutatingTheReadObjectAfterDiscardAndReloadChangesNothing());

    [Theory]
    [MemberData(nameof(Kinds))]
    public void MutatingTheObjectOfTheReadAfterKeepMineChangesNothing(string kind) =>
        Both(kind, scenario => scenario.MutatingTheReadObjectAfterKeepMineChangesNothing());

    [Theory]
    [InlineData("box", false)]
    [InlineData("box", true)]
    [InlineData("pilot", false)]
    [InlineData("pilot", true)]
    public void MutatingTheObjectOfThePostReadAfterTheRebaseOfAnAppliedUpdateChangesNothing(string kind, bool withWarnings) =>
        Both(kind, scenario => scenario.MutatingThePostReadObjectAfterAnAppliedRebaseChangesNothing(withWarnings));

    [Theory]
    [MemberData(nameof(Kinds))]
    public void MutatingTheObjectPassedToSetCurrentDraftStateAfterTheCallDoesNotChangeTheCurrentState(string kind) =>
        Both(kind, scenario => scenario.MutatingTheObjectPassedToSetCurrentDraftStateChangesNothing());

    [Theory]
    [MemberData(nameof(Kinds))]
    public void MutatingTheObjectReturnedByInitialDraftStateDoesNotChangeTheInternalInitialState(string kind) =>
        Both(kind, scenario => scenario.MutatingTheObjectReturnedByInitialDraftStateChangesNothing());

    [Theory]
    [MemberData(nameof(Kinds))]
    public void ReusingOneObjectOfOriginForTwoDraftsOrTwoSessionsLeavesThemIndependent(string kind) =>
        Both(kind, scenario => scenario.OneReadObjectForTwoDraftsLeavesThemIndependent());

    // ------------------------------------------------------------------ conservation of INV-F4T1-19 and the rest of INV-F4T2A-10

    [Theory]
    [MemberData(nameof(Kinds))]
    public void EditingTheCurrentStateInPlaceLeavesTheDraftDirtyAndTheCloseVetoed(string kind) =>
        Both(kind, scenario => scenario.InPlaceEditOfTheCurrentStateLeavesTheDraftDirtyAndVetoesTheClose());

    [Fact]
    public void TheCurrentStateStillLeavesByReferenceAndTheInitialStateAsAFreshCopyEachTime()
    {
        var draft = OwnershipKits.Box().Born(new DraftRegistry(), new DraftBox { Name = "Rack", Width = 30 });

        Assert.Same(draft.CurrentDraftState, draft.CurrentDraftState);
        Assert.NotSame(draft.InitialDraftState, draft.InitialDraftState);
        Assert.NotSame(draft.InitialDraftState, draft.CurrentDraftState);
    }

    [Fact]
    public void ThePilotStateKeepsTheInPlaceEditModelOfTheCurrentState()
    {
        var kit = OwnershipKits.Pilot();
        var registry = new DraftRegistry();
        var draft = kit.Born(registry, PilotKit.State("pb-01-base"));
        var current = draft.CurrentDraftState;

        current.Inputs.Annotations.AnnotationScale = 3.5;

        Assert.Same(current, draft.CurrentDraftState);
        Assert.True(draft.IsDirty);
        Assert.Equal(1.5, draft.InitialDraftState.Inputs.Annotations.AnnotationScale);
        Assert.True(new DraftClosePolicy(registry).EvaluateClose(DraftKit.S1).IsVeto);
    }

    [Fact]
    public void TheResultOfTheAdapterInACrossedBoundaryIsNotCopied()
    {
        var draft = OwnershipKits.Box().Born(new DraftRegistry(), new DraftBox { Name = "Rack", Width = 30 });
        var returned = new DraftBox { Name = "Rack", Width = 77 };
        draft.SetPendingText("width", "77");

        var result = draft.CrossBoundary(DraftBoundary.TabChange, new BoxReturningAdapter(returned));

        Assert.Equal(DraftBoundaryDecision.Proceeded, result.Decision);
        Assert.Same(returned, draft.CurrentDraftState);
        Assert.True(draft.IsDirty);
    }

    [Fact]
    public void DirtyStatusAndTheClosePolicyReadThePrivateFieldsAndNeverTheGetterThatCopies()
    {
        var copies = 0;
        var kit = new OwnershipKit<DraftBox>(
            make: OwnershipKits.Box().Make,
            mutate: OwnershipKits.Box().Mutate,
            fingerprint: OwnershipKits.Box().Fingerprint,
            comparer: DraftKit.BoxComparer,
            copy: box =>
            {
                copies++;
                return DraftKit.CopyBox(box);
            });
        var registry = new DraftRegistry();
        var draft = kit.Born(registry, kit.Make(1));
        kit.Mutate(draft.CurrentDraftState);
        var afterEdit = copies;

        for (var round = 0; round < 3; round++)
        {
            Assert.True(draft.IsDirty);
            Assert.Equal(DraftStatus.Dirty, draft.Status);
            Assert.Equal(new[] { "R1" }, registry.RacksAtRiskOf(DraftKit.S1));
            Assert.True(new DraftClosePolicy(registry).EvaluateClose(DraftKit.S1).IsVeto);
        }

        Assert.Equal(afterEdit, copies);
        _ = draft.InitialDraftState;
        Assert.Equal(afterEdit + 1, copies);
    }

    [Fact]
    public void EveryStateThatEntersTheCoreIsCopiedWithTheInjectedCopier()
    {
        var copies = new List<DraftBox>();
        var kit = new OwnershipKit<DraftBox>(
            make: OwnershipKits.Box().Make,
            mutate: OwnershipKits.Box().Mutate,
            fingerprint: OwnershipKits.Box().Fingerprint,
            comparer: DraftKit.BoxComparer,
            copy: box =>
            {
                var copy = DraftKit.CopyBox(box);
                copies.Add(copy);
                return copy;
            });
        var read = kit.Make(1);
        var draft = kit.Born(new DraftRegistry(), read);

        // Begin copies the initial state of the read twice: once for the initial state and once for the current one.
        Assert.Equal(2, copies.Count);
        Assert.All(copies, copy => Assert.NotSame(read, copy));

        var edit = kit.Make(2);
        draft.SetCurrentDraftState(edit);
        Assert.Equal(3, copies.Count);
        Assert.Same(copies[2], draft.CurrentDraftState);

        var reloaded = kit.Make(3);
        draft.DiscardAndReload(kit.Read(reloaded));
        Assert.Equal(5, copies.Count);
        Assert.DoesNotContain(copies, copy => ReferenceEquals(copy, reloaded));
    }

    // ------------------------------------------------------------------ INV-F4T2A-10: a reload is atomic

    /// <summary>A copier that throws on the Nth call after it is armed, so a reload can fail halfway.</summary>
    private sealed class ArmedCopier
    {
        private int _calls;
        private int _throwOnCall = -1;

        public void Arm(int callNumber)
        {
            _calls = 0;
            _throwOnCall = callNumber;
        }

        public void Disarm() => _throwOnCall = -1;

        public DraftBox Copy(DraftBox box)
        {
            _calls++;
            if (_calls == _throwOnCall) throw new InvalidOperationException("copier-boom");
            return DraftKit.CopyBox(box);
        }
    }

    [Theory]
    [InlineData("discard", 1)]
    [InlineData("discard", 2)]
    [InlineData("keep", 1)]
    [InlineData("applied", 1)]
    [InlineData("applied", 2)]
    [InlineData("warnings", 1)]
    [InlineData("warnings", 2)]
    public void AReloadWhoseCopierThrowsLeavesTheStaleBaseTheInitialStateAndTheCurrentStateExactlyAsTheyWere(string reload, int failingCopy)
    {
        var copier = new ArmedCopier();
        var kit = new OwnershipKit<DraftBox>(
            make: OwnershipKits.Box().Make,
            mutate: OwnershipKits.Box().Mutate,
            fingerprint: OwnershipKits.Box().Fingerprint,
            comparer: DraftKit.BoxComparer,
            copy: copier.Copy);
        var draft = kit.Born(new DraftRegistry(), kit.Make(1));
        draft.SetCurrentDraftState(kit.Make(5));
        var staleBefore = draft.StaleBase;
        var initialBefore = kit.Fingerprint(draft.InitialDraftState);
        var currentBefore = kit.Fingerprint(draft.CurrentDraftState);
        var statusBefore = draft.Status;
        var read = DraftIntentRead<DraftBox>.Admitted(DraftKit.Base(DraftKit.Mutable("D9")), kit.Make(3));
        Assert.NotSame(staleBefore, read.StaleBase);
        if (reload is "applied" or "warnings") Assert.True(draft.TryBeginApply().IsStarted);

        copier.Arm(failingCopy);
        var thrown = Assert.Throws<InvalidOperationException>(() =>
        {
            if (reload == "discard") draft.DiscardAndReload(read);
            else if (reload == "keep") draft.KeepMineOnCurrentVersion(read, confirmed: true);
            else if (reload == "applied") draft.CompleteApply(DraftApplyOutcome<DraftBox>.Applied(read));
            else draft.CompleteApply(DraftApplyOutcome<DraftBox>.AppliedWithWarnings(read, new[] { "w" }));
        });
        copier.Disarm();

        Assert.Equal("copier-boom", thrown.Message);
        Assert.Same(staleBefore, draft.StaleBase);
        Assert.Equal(initialBefore, kit.Fingerprint(draft.InitialDraftState));
        Assert.Equal(currentBefore, kit.Fingerprint(draft.CurrentDraftState));
        if (reload is "discard" or "keep") Assert.Equal(statusBefore, draft.Status);
    }

    [Fact]
    public void SetCurrentDraftStateDocumentsThatItKeepsACopyOfItsArgument()
    {
        var xml = System.IO.File.ReadAllText(System.IO.Path.Combine(
            PilotKit.RepoRoot().FullName, "src", "RackCad.Application", "Workspace", "Drafts", "DraftSession.cs"));
        var at = xml.IndexOf("public DraftEditResult SetCurrentDraftState", StringComparison.Ordinal);
        Assert.True(at > 0);
        var documentation = xml.Substring(Math.Max(0, at - 700), Math.Min(700, at));

        Assert.Contains("COPY of the argument", documentation, StringComparison.Ordinal);
    }

    private sealed class BoxReturningAdapter : IDraftPendingTextAdapter<DraftBox>
    {
        private readonly DraftBox _result;

        public BoxReturningAdapter(DraftBox result)
        {
            _result = result;
        }

        public DraftFieldValidation Validate(string fieldKey, string text) => DraftFieldValidation.Valid;

        public DraftBox Apply(DraftBox current, IReadOnlyDictionary<string, string> pendingText) => _result;
    }
}
