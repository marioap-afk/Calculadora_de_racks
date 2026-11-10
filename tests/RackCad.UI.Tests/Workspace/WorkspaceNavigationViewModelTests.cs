#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F2-T2B-NAVIGATION-UI (INV-F2T2B-01..06, 13): the navigation view-model enables Seleccionar, Ver, Centrar and Localizar
    /// vista only with exactly one rack in the context, a readable drawing, no action block and no action in progress; it sends
    /// the session and the rack of the same snapshot, keeps the copy k and takes M only from the port, uses the explicit view of
    /// Localizar, presents every answer as a Spanish value, asks for exactly one new snapshot when the answer requires a
    /// refresh and contains every failure. It never asks the port by itself.
    /// </summary>
    public class WorkspaceNavigationViewModelTests
    {
        private static readonly NavigationActionKind[] AllActions =
        {
            NavigationActionKind.Select, NavigationActionKind.View, NavigationActionKind.Center, NavigationActionKind.LocateView,
        };

        private static WorkspaceNavigationViewModel Ready(
            WorkspaceNavigationPortDouble port, long session = 7, string rack = "R-1", Action? refresh = null)
        {
            var model = new WorkspaceNavigationViewModel(port, refresh);
            model.Present(new SessionId(session), SelectionContextKind.One, rack, true, WorkspaceActionBlock.None, string.Empty);
            return model;
        }

        private static Task Act(WorkspaceNavigationViewModel model, NavigationActionKind action) => model.NavigateAsync(action);

        // ------------------------------------------------------------------ INV-F2T2B-02: enabled only with One(R), readable, no block

        [Theory]
        [InlineData(SelectionContextKind.One, true, WorkspaceActionBlock.None, true)]
        [InlineData(SelectionContextKind.None, true, WorkspaceActionBlock.None, false)]
        [InlineData(SelectionContextKind.Selection, true, WorkspaceActionBlock.None, false)]
        [InlineData(SelectionContextKind.NoIdentity, true, WorkspaceActionBlock.None, false)]
        [InlineData(SelectionContextKind.Diagnostic, true, WorkspaceActionBlock.None, false)]
        [InlineData(SelectionContextKind.One, false, WorkspaceActionBlock.None, false)]
        [InlineData(SelectionContextKind.One, true, WorkspaceActionBlock.NotStarted, false)]
        [InlineData(SelectionContextKind.One, true, WorkspaceActionBlock.NoDocument, false)]
        [InlineData(SelectionContextKind.One, true, WorkspaceActionBlock.Degraded, false)]
        [InlineData(SelectionContextKind.One, true, WorkspaceActionBlock.ModalActive, false)]
        [InlineData(SelectionContextKind.One, true, WorkspaceActionBlock.CommandActive, false)]
        public void TheActionsAreEnabledOnlyWithExactlyOneRackAReadableDrawingAndNoActionBlock(
            SelectionContextKind kind, bool canRead, WorkspaceActionBlock block, bool expected)
        {
            var model = new WorkspaceNavigationViewModel(new WorkspaceNavigationPortDouble());

            model.Present(new SessionId(4), kind, kind == SelectionContextKind.One ? "R-1" : null!, canRead, block, "bloqueado");

            Assert.Equal(expected, model.CanNavigate);
            Assert.Equal(!expected, model.HasBlockedReason);
            Assert.Equal(expected, model.BlockedReason.Length == 0);
        }

        [Theory]
        [InlineData(SelectionContextKind.None, true, WorkspaceActionBlock.None)]
        [InlineData(SelectionContextKind.Selection, true, WorkspaceActionBlock.None)]
        [InlineData(SelectionContextKind.NoIdentity, true, WorkspaceActionBlock.None)]
        [InlineData(SelectionContextKind.Diagnostic, true, WorkspaceActionBlock.None)]
        [InlineData(SelectionContextKind.One, false, WorkspaceActionBlock.None)]
        [InlineData(SelectionContextKind.One, true, WorkspaceActionBlock.CommandActive)]
        [InlineData(SelectionContextKind.One, true, WorkspaceActionBlock.Degraded)]
        public async Task ADisabledActionExplainsWhyAndNeverCallsThePort(SelectionContextKind kind, bool canRead, WorkspaceActionBlock block)
        {
            var port = new WorkspaceNavigationPortDouble();
            var model = new WorkspaceNavigationViewModel(port);
            model.Present(new SessionId(4), kind, kind == SelectionContextKind.One ? "R-1" : null!, canRead, block, "Hay un comando en curso.");

            foreach (var action in AllActions) await Act(model, action);

            Assert.Empty(port.Calls);
            Assert.False(string.IsNullOrWhiteSpace(model.BlockedReason));
        }

        [Fact]
        public async Task WithoutASessionNothingIsEnabledEvenWithARackInTheContext()
        {
            var port = new WorkspaceNavigationPortDouble();
            var model = new WorkspaceNavigationViewModel(port);
            model.Present(null, SelectionContextKind.One, "R-1", true, WorkspaceActionBlock.None, string.Empty);

            await Act(model, NavigationActionKind.View);

            Assert.False(model.CanNavigate);
            Assert.Empty(port.Calls);
            Assert.False(string.IsNullOrWhiteSpace(model.BlockedReason));
        }

        [Fact]
        public void TheReasonOfABlockedPanelIsTheTextTheSnapshotGave()
        {
            var model = new WorkspaceNavigationViewModel(new WorkspaceNavigationPortDouble());
            model.Present(new SessionId(1), SelectionContextKind.One, "R-1", true, WorkspaceActionBlock.ModalActive, "Hay un cuadro de diálogo de RackCad abierto.");

            Assert.Equal("Hay un cuadro de diálogo de RackCad abierto.", model.BlockedReason);
        }

        // T2B-F3: one object that is not a rack is not "several objects".
        [Theory]
        [InlineData(0, 1)]
        [InlineData(2, 0)]
        [InlineData(1, 1)]
        public void ASelectionThatIsNotOneRackGetsANeutralReasonThatIsTrueForOneOrSeveralObjects(int racks, int others)
        {
            var model = new WorkspaceNavigationViewModel(new WorkspaceNavigationPortDouble());
            Assert.Equal(SelectionContextKind.Selection, SelectionContext.Selection(racks, others).Kind);

            model.Present(new SessionId(1), SelectionContextKind.Selection, null!, true, WorkspaceActionBlock.None, string.Empty);

            Assert.Equal("Lo seleccionado no es un rack individual; selecciona un solo rack para navegar.", model.BlockedReason);
            Assert.DoesNotContain("varios", model.BlockedReason, StringComparison.Ordinal);
        }

        [Fact]
        public async Task WithoutANavigationPortTheActionsAreUnavailableAndSayIt()
        {
            var model = new WorkspaceNavigationViewModel(null!);
            model.Present(new SessionId(1), SelectionContextKind.One, "R-1", true, WorkspaceActionBlock.None, string.Empty);

            await Act(model, NavigationActionKind.View);

            Assert.False(model.IsAvailable);
            Assert.False(model.CanNavigate);
            Assert.Contains("no está disponible", model.BlockedReason, StringComparison.Ordinal);
        }

        [Fact]
        public async Task AnActionInProgressDisablesEverythingAndASecondClickDoesNotCallThePort()
        {
            var port = new WorkspaceNavigationPortDouble { Gate = new TaskCompletionSource<WorkspaceNavigationResult>() };
            var model = Ready(port);

            var first = Act(model, NavigationActionKind.View);

            Assert.False(first.IsCompleted);
            Assert.True(model.IsBusy);
            Assert.False(model.CanNavigate);
            await Act(model, NavigationActionKind.Center);
            await Act(model, NavigationActionKind.Select);
            Assert.Single(port.Calls);

            port.Gate.SetResult(WorkspaceNavigationPortDouble.Executed(2));
            await first;

            Assert.False(model.IsBusy);
            Assert.True(model.CanNavigate);
        }

        // ------------------------------------------------------------------ INV-F2T2B-03: the session and the rack of the same snapshot

        [Fact]
        public async Task EveryRequestCarriesTheSessionAndTheRackOfTheSnapshotPresentedAtTheClick()
        {
            var port = new WorkspaceNavigationPortDouble();
            var model = Ready(port, session: 7, rack: "R-1");

            await Act(model, NavigationActionKind.View);
            model.Present(new SessionId(8), SelectionContextKind.One, "R-2", true, WorkspaceActionBlock.None, string.Empty);
            await Act(model, NavigationActionKind.Select);

            Assert.Equal(
                new[] { (7L, "R-1"), (8L, "R-2") },
                port.Calls.Select(c => (c.Origin.Origin.Value, c.RackId)).ToArray());
        }

        [Fact]
        public async Task ANewSnapshotDoesNotRedirectAnActionAlreadyInProgressNorFeedTheNewRack()
        {
            var port = new WorkspaceNavigationPortDouble { Gate = new TaskCompletionSource<WorkspaceNavigationResult>() };
            var model = Ready(port, session: 7, rack: "R-1");

            var pending = Act(model, NavigationActionKind.View);
            model.Present(new SessionId(9), SelectionContextKind.One, "R-3", true, WorkspaceActionBlock.None, string.Empty);
            port.Gate.SetResult(WorkspaceNavigationPortDouble.Executed(5));
            await pending;

            var call = Assert.Single(port.Calls);
            Assert.Equal(new SessionId(7), call.Origin.Origin);
            Assert.Equal("R-1", call.RackId);
            Assert.Null(model.TargetCount);
            Assert.Equal(0, model.CopyIndex);
            Assert.Equal(string.Empty, model.ResultText);
            Assert.True(model.CanNavigate);
        }

        [Fact]
        public async Task ThePanelFeedsTheNavigationWithTheSessionAndTheRackOfOneSnapshotAndNeverMixesThem()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 5, selection: SelectionContext.One("R-A")) };
            var port = new WorkspaceNavigationPortDouble();
            var panel = new WorkspacePanelViewModel(host, _ => { }, port);

            panel.Refresh();
            await panel.Navigation.ViewAsync();
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 6, selection: SelectionContext.One("R-B"));
            panel.Refresh();
            await panel.Navigation.ViewAsync();

            Assert.Equal(
                new[] { (5L, "R-A"), (6L, "R-B") },
                port.Calls.Select(c => (c.Origin.Origin.Value, c.RackId)).ToArray());
        }

        // ------------------------------------------------------------------ INV-F2T2B-04: the copy k of M

        [Fact]
        public async Task TheCopyStartsUnknownAndMComesOnlyFromTheAnswerOfThePort()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);

            Assert.Null(model.TargetCount);
            Assert.Equal(0, model.CopyIndex);
            Assert.Equal("Copia 1 de ?", model.CopyText);
            Assert.False(model.CanNextCopy);
            model.NextCopy();
            Assert.Equal(0, model.CopyIndex);

            await Act(model, NavigationActionKind.View);

            Assert.Equal(3, model.TargetCount);
            Assert.Equal("Copia 1 de 3", model.CopyText);
            Assert.True(model.CanNextCopy);
        }

        [Fact]
        public async Task TheNextCopyTravelsAsTheCopyIndexAndAfterTheLastOneReturnsToTheFirst()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);

            await Act(model, NavigationActionKind.View);
            model.NextCopy();
            await Act(model, NavigationActionKind.View);
            model.NextCopy();
            await Act(model, NavigationActionKind.Center);
            Assert.Equal("Copia 3 de 3", model.CopyText);
            model.NextCopy();
            await Act(model, NavigationActionKind.View);

            Assert.Equal(new[] { 0, 1, 2, 0 }, port.Calls.Select(c => c.CopyIndex).ToArray());
            Assert.Equal(
                new[] { NavigationActionKind.View, NavigationActionKind.View, NavigationActionKind.Center, NavigationActionKind.View },
                port.Calls.Select(c => c.Action).ToArray());
            Assert.Equal("Copia 1 de 3", model.CopyText);
        }

        [Fact]
        public async Task OneTargetOffersNoNextCopyAndZeroTargetsSayTheyHaveNone()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(1) };
            var model = Ready(port);

            await Act(model, NavigationActionKind.View);
            Assert.False(model.CanNextCopy);
            Assert.Equal("Copia 1 de 1", model.CopyText);

            port.Result = WorkspaceNavigationResult.Unavailable("x", NavigationActionVerdict.RejectedNoTargets) with { TargetCount = 0 };
            await Act(model, NavigationActionKind.View);

            Assert.Equal(0, model.TargetCount);
            Assert.False(model.CanNextCopy);
            Assert.Equal("Sin objetivos", model.CopyText);
        }

        [Fact]
        public async Task ACopyBeyondTheTargetsThatShrankGoesBackToTheFirstOne()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);
            await Act(model, NavigationActionKind.View);
            model.NextCopy();
            model.NextCopy();
            Assert.Equal(2, model.CopyIndex);

            port.Result = WorkspaceNavigationPortDouble.Executed(2);
            await Act(model, NavigationActionKind.View);

            Assert.Equal(2, model.TargetCount);
            Assert.Equal(0, model.CopyIndex);
        }

        [Fact]
        public async Task SelectNeverUsesTheCopyAndLeavesItUntouched()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);
            await Act(model, NavigationActionKind.View);
            model.NextCopy();
            model.NextCopy();

            port.Result = WorkspaceNavigationPortDouble.Executed(9);
            await Act(model, NavigationActionKind.Select);

            var select = Assert.Single(port.Calls, c => c.Action == NavigationActionKind.Select);
            Assert.Equal(NavigationActionKind.Select, select.Action);
            Assert.Equal(0, select.CopyIndex);
            Assert.Null(select.View);
            Assert.Equal(2, model.CopyIndex);
            Assert.Equal(3, model.TargetCount);
        }

        [Fact]
        public async Task TheCopyRestartsWhenTheRackChangesAndMBecomesUnknownAgain()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port, rack: "R-1");
            await Act(model, NavigationActionKind.View);
            model.NextCopy();
            Assert.Equal(1, model.CopyIndex);

            model.Present(new SessionId(7), SelectionContextKind.One, "R-2", true, WorkspaceActionBlock.None, string.Empty);

            Assert.Equal(0, model.CopyIndex);
            Assert.Null(model.TargetCount);
            Assert.Equal("Copia 1 de ?", model.CopyText);
        }

        [Fact]
        public async Task TheCopyRestartsWhenTheSessionChanges()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port, session: 7);
            await Act(model, NavigationActionKind.View);
            model.NextCopy();

            model.Present(new SessionId(8), SelectionContextKind.One, "R-1", true, WorkspaceActionBlock.None, string.Empty);

            Assert.Equal(0, model.CopyIndex);
            Assert.Null(model.TargetCount);
        }

        [Fact]
        public async Task TheCopyRestartsWhenTheRequestedViewChanges()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(2) };
            var model = Ready(port);
            model.SelectedView = NavigationViewOrder.Frontal;
            await Act(model, NavigationActionKind.LocateView);
            model.NextCopy();
            Assert.Equal(1, model.CopyIndex);

            model.SelectedView = NavigationViewOrder.Lateral;

            Assert.Equal(0, model.CopyIndex);
            Assert.Null(model.TargetCount);
        }

        [Fact]
        public async Task ARepeatedSnapshotOfTheSameRackKeepsTheCopy()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);
            await Act(model, NavigationActionKind.View);
            model.NextCopy();

            model.Present(new SessionId(7), SelectionContextKind.One, "R-1", true, WorkspaceActionBlock.None, string.Empty);

            Assert.Equal(1, model.CopyIndex);
            Assert.Equal(3, model.TargetCount);
        }

        // ------------------------------------------------------------------ INV-F2T2B-05: Localizar vista

        [Fact]
        public async Task LocateUsesTheExplicitViewAndLocatesWithViewByDefault()
        {
            var port = new WorkspaceNavigationPortDouble();
            var model = Ready(port);
            Assert.Equal(NavigationViewOrder.Frontal, model.SelectedView);
            Assert.Equal(NavigationActionKind.View, model.LocateWith);

            await Act(model, NavigationActionKind.LocateView);
            model.SelectedView = NavigationViewOrder.Lateral;
            await Act(model, NavigationActionKind.LocateView);
            model.SelectedView = NavigationViewOrder.Planta;
            model.LocateCentered = true;
            await Act(model, NavigationActionKind.LocateView);

            Assert.All(port.Calls, c => Assert.Equal(NavigationActionKind.LocateView, c.Action));
            Assert.Equal(
                new NavigationViewOrder?[] { NavigationViewOrder.Frontal, NavigationViewOrder.Lateral, NavigationViewOrder.Planta },
                port.Calls.Select(c => c.View).ToArray());
            Assert.Equal(
                new[] { NavigationActionKind.View, NavigationActionKind.View, NavigationActionKind.Center },
                port.Calls.Select(c => c.LocateWith).ToArray());
        }

        [Fact]
        public async Task ViewAndCenterAskForNoSpecificViewAndTheChoiceOfViewOffersTheFourOrders()
        {
            var port = new WorkspaceNavigationPortDouble();
            var model = Ready(port);
            model.SelectedView = NavigationViewOrder.Planta;

            await Act(model, NavigationActionKind.View);
            await Act(model, NavigationActionKind.Center);

            Assert.All(port.Calls, c => Assert.Null(c.View));
            Assert.Equal(
                new[] { NavigationViewOrder.Frontal, NavigationViewOrder.Lateral, NavigationViewOrder.Planta, NavigationViewOrder.Other },
                model.ViewChoices.Select(v => v.Order).ToArray());
            Assert.All(model.ViewChoices, v => Assert.False(string.IsNullOrWhiteSpace(v.Label)));
        }

        [Fact]
        public async Task LocateAfterViewStartsItsOwnCopyBecauseItsCountBelongsToTheRequestedView()
        {
            var port = new WorkspaceNavigationPortDouble
            {
                Responder = call => WorkspaceNavigationPortDouble.Executed(call.Action == NavigationActionKind.LocateView ? 2 : 3),
            };
            var model = Ready(port);
            await Act(model, NavigationActionKind.View);
            model.NextCopy();
            model.NextCopy();
            Assert.Equal(2, model.CopyIndex);

            await Act(model, NavigationActionKind.LocateView);

            Assert.Equal(0, Assert.Single(port.Calls, c => c.Action == NavigationActionKind.LocateView).CopyIndex);
            Assert.Equal(2, model.TargetCount);
            Assert.Equal("Copia 1 de 2", model.CopyText);
        }

        // T2B-F1: the copy of Localizar vista advances, and View or Center after Localizar restart it.

        [Fact]
        public async Task LocateOnTheSameViewAdvancesTheCopyAndAfterTheLastOneReturnsToTheFirst()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);
            model.SelectedView = NavigationViewOrder.Frontal;

            await Act(model, NavigationActionKind.LocateView);
            model.NextCopy();
            await Act(model, NavigationActionKind.LocateView);
            model.NextCopy();
            await Act(model, NavigationActionKind.LocateView);
            model.NextCopy();
            Assert.Equal("Copia 1 de 3", model.CopyText);
            await Act(model, NavigationActionKind.LocateView);

            Assert.Equal(new[] { 0, 1, 2, 0 }, port.Calls.Select(c => c.CopyIndex).ToArray());
            Assert.All(port.Calls, c => Assert.Equal(NavigationViewOrder.Frontal, c.View));
        }

        [Theory]
        [InlineData(NavigationActionKind.View)]
        [InlineData(NavigationActionKind.Center)]
        public async Task ViewOrCenterAfterLocateRestartTheCopyAndMIsUnknownUntilThePortAnswers(NavigationActionKind action)
        {
            WorkspaceNavigationViewModel? model = null;
            var observed = new List<(string Text, int? Count)>();
            var port = new WorkspaceNavigationPortDouble();
            port.Responder = call =>
            {
                if (call.Action == action) observed.Add((model!.CopyText, model.TargetCount));
                return WorkspaceNavigationPortDouble.Executed(3);
            };
            model = Ready(port);

            await Act(model, NavigationActionKind.LocateView);
            model.NextCopy();
            model.NextCopy();
            Assert.Equal(2, model.CopyIndex);
            await Act(model, action);

            Assert.Equal(0, Assert.Single(port.Calls, c => c.Action == action).CopyIndex);
            var during = Assert.Single(observed);
            Assert.Equal("Copia 1 de ?", during.Text);
            Assert.Null(during.Count);
        }

        [Fact]
        public async Task ViewAndCenterShareTheCopyAndALaterLocateStartsItsOwn()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);

            await Act(model, NavigationActionKind.View);
            model.NextCopy();
            await Act(model, NavigationActionKind.View);
            await Act(model, NavigationActionKind.Center);
            await Act(model, NavigationActionKind.LocateView);

            Assert.Equal(new[] { 0, 1, 1, 0 }, port.Calls.Select(c => c.CopyIndex).ToArray());
        }

        [Fact]
        public async Task AChangeOfTheRequestedViewRestartsTheCopyOfLocate()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);
            model.SelectedView = NavigationViewOrder.Frontal;

            await Act(model, NavigationActionKind.LocateView);
            model.NextCopy();
            model.SelectedView = NavigationViewOrder.Lateral;
            await Act(model, NavigationActionKind.LocateView);

            Assert.Equal(new[] { 0, 0 }, port.Calls.Select(c => c.CopyIndex).ToArray());
            Assert.Equal(
                new NavigationViewOrder?[] { NavigationViewOrder.Frontal, NavigationViewOrder.Lateral },
                port.Calls.Select(c => c.View).ToArray());
        }

        // ------------------------------------------------------------------ INV-F2T2B-06: results as values, in Spanish

        [Fact]
        public async Task EveryStatusHasItsOwnSpanishText()
        {
            var texts = new Dictionary<string, string>();
            var results = new Dictionary<string, WorkspaceNavigationResult>
            {
                ["executed"] = WorkspaceNavigationPortDouble.Executed(3),
                ["no-targets"] = WorkspaceNavigationResult.Unavailable("The rack has no target for that action.", NavigationActionVerdict.RejectedNoTargets) with { TargetCount = 0 },
                ["model-space"] = WorkspaceNavigationResult.Unavailable("Navigation needs the Model Space."),
                ["rejected"] = WorkspaceNavigationResult.Rejected("The request belongs to another document.", NavigationActionVerdict.RejectedSessionMismatch),
                ["stale"] = WorkspaceNavigationResult.Stale("A target of the rack is gone.", true, NavigationActionVerdict.RejectedTargetGone),
                ["stale-gone"] = WorkspaceNavigationResult.Stale("The document of the request is gone.", false, NavigationActionVerdict.RejectedSessionDestroyed),
                ["failed"] = WorkspaceNavigationResult.Failed("boom"),
            };

            foreach (var (name, result) in results)
            {
                var port = new WorkspaceNavigationPortDouble { Result = result };
                var model = Ready(port);
                await Act(model, NavigationActionKind.View);
                texts[name] = model.ResultText;
                Assert.True(model.HasResult);
            }

            Assert.Equal(results.Count, texts.Values.Distinct(StringComparer.Ordinal).Count());
            Assert.All(texts.Values, t => Assert.False(string.IsNullOrWhiteSpace(t)));
            Assert.Contains("Espacio Modelo", texts["model-space"], StringComparison.Ordinal);
            Assert.DoesNotContain("Espacio Modelo", texts["no-targets"], StringComparison.Ordinal);
            Assert.Contains("otro documento", texts["rejected"], StringComparison.Ordinal);
            Assert.Contains("boom", texts["failed"], StringComparison.Ordinal);
            Assert.Contains("copia 1 de 3", texts["executed"], StringComparison.Ordinal);
            foreach (var name in new[] { "executed", "no-targets", "model-space", "rejected", "stale", "stale-gone" })
            {
                Assert.DoesNotContain("The ", texts[name], StringComparison.Ordinal);
                Assert.DoesNotContain("Navigation needs", texts[name], StringComparison.Ordinal);
            }
        }

        [Fact]
        public async Task TheExecutedTextNamesWhatEachActionDid()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(2) };
            var model = Ready(port);
            var texts = new List<string>();

            foreach (var action in AllActions)
            {
                await Act(model, action);
                texts.Add(model.ResultText);
            }

            Assert.Equal(4, texts.Distinct(StringComparer.Ordinal).Count());
            Assert.Contains("seleccionado", texts[0], StringComparison.Ordinal);
            Assert.Contains("ajustada", texts[1], StringComparison.Ordinal);
            Assert.Contains("centrado", texts[2], StringComparison.Ordinal);
            Assert.Contains("Frontal", texts[3], StringComparison.Ordinal);
        }

        [Fact]
        public async Task ModelSpaceUnavailabilityIsExplainedAsSuchWithoutChangingSpace()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationResult.Unavailable("Navigation needs the Model Space.") };
            var model = Ready(port);

            await Act(model, NavigationActionKind.Center);

            Assert.Contains("Espacio Modelo", model.ResultText, StringComparison.Ordinal);
            Assert.Contains("no cambia de espacio", model.ResultText, StringComparison.Ordinal);
            Assert.Single(port.Calls);
        }

        // T2B-F2: a copy beyond the targets is not presented as a rack without targets.

        [Fact]
        public async Task ACopyThatNoLongerExistsIsExplainedAndGoesBackToTheFirstOne()
        {
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationPortDouble.Executed(3) };
            var model = Ready(port);
            await Act(model, NavigationActionKind.View);
            model.NextCopy();
            model.NextCopy();
            Assert.Equal(2, model.CopyIndex);

            port.Result = WorkspaceNavigationResult.Unavailable("The rack has no target for that action.", NavigationActionVerdict.RejectedNoTargets) with { TargetCount = 2 };
            await Act(model, NavigationActionKind.View);

            Assert.Equal("La copia pedida ya no existe; no se hizo nada. Se vuelve a la primera copia.", model.ResultText);
            Assert.DoesNotContain("no tiene objetivos", model.ResultText, StringComparison.Ordinal);
            Assert.Equal(0, model.CopyIndex);
            Assert.Equal("Copia 1 de 2", model.CopyText);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(null)]
        public async Task ARackWithoutTargetsKeepsItsTextWhenTheCountIsZeroOrUnknown(int? count)
        {
            var port = new WorkspaceNavigationPortDouble
            {
                Result = WorkspaceNavigationResult.Unavailable("The rack has no target for that action.", NavigationActionVerdict.RejectedNoTargets) with { TargetCount = count },
            };
            var model = Ready(port);

            await Act(model, NavigationActionKind.View);

            Assert.Contains("no tiene objetivos", model.ResultText, StringComparison.Ordinal);
            Assert.DoesNotContain("ya no existe", model.ResultText, StringComparison.Ordinal);
        }

        public static IEnumerable<object[]> PortFailures()
        {
            yield return new object[] { new InvalidOperationException("fallo-síncrono"), null!, "fallo-síncrono" };
            yield return new object[] { null!, new InvalidOperationException("fallo-en-la-tarea"), "fallo-en-la-tarea" };
        }

        [Theory]
        [MemberData(nameof(PortFailures))]
        public async Task AFailureOfThePortIsContainedAndPresentedAndTheActionsAreReleased(Exception? synchronous, Exception? inTheTask, string expected)
        {
            var port = new WorkspaceNavigationPortDouble { ThrowSynchronously = synchronous, FaultTheTask = inTheTask };
            var model = Ready(port);

            var thrown = await Record.ExceptionAsync(() => model.ViewAsync());

            Assert.Null(thrown);
            Assert.Contains(expected, model.ResultText, StringComparison.Ordinal);
            Assert.Contains("No se pudo", model.ResultText, StringComparison.Ordinal);
            Assert.False(model.IsBusy);
            Assert.True(model.CanNavigate);
        }

        [Fact]
        public async Task AFailureWhilePresentingIsContainedToo()
        {
            var port = new WorkspaceNavigationPortDouble();
            var model = Ready(port);
            model.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(WorkspaceNavigationViewModel.ResultText)) throw new InvalidOperationException("handler-roto");
            };

            var thrown = await Record.ExceptionAsync(() => model.ViewAsync());

            Assert.Null(thrown);
            Assert.False(model.IsBusy);
        }

        // ------------------------------------------------------------------ INV-F2T2B-06: exactly one snapshot query with RefreshRequired

        [Theory]
        [InlineData(WorkspaceNavigationStatus.Executed, false, 0)]
        [InlineData(WorkspaceNavigationStatus.Unavailable, false, 0)]
        [InlineData(WorkspaceNavigationStatus.Rejected, false, 0)]
        [InlineData(WorkspaceNavigationStatus.Stale, false, 0)]
        [InlineData(WorkspaceNavigationStatus.Stale, true, 1)]
        [InlineData(WorkspaceNavigationStatus.Failed, false, 0)]
        public async Task OnlyAnAnswerThatRequiresARefreshAsksThePanelForExactlyOneNewSnapshot(
            WorkspaceNavigationStatus status, bool refreshRequired, int expectedRefreshes)
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var port = new WorkspaceNavigationPortDouble
            {
                Result = new WorkspaceNavigationResult(status, null, refreshRequired, "x", 2),
            };
            var panel = new WorkspacePanelViewModel(host, _ => { }, port);
            panel.Refresh();
            var before = host.SnapshotCalls;

            await panel.Navigation.ViewAsync();

            Assert.Single(port.Calls);
            Assert.Equal(expectedRefreshes, host.SnapshotCalls - before);
            Assert.Equal(0, host.ReportCalls);
            Assert.Equal(0, host.AuthorizeCalls);
            Assert.Equal(0, host.SelectionCalls);
        }

        [Fact]
        public async Task TheRefreshCallbackIsCalledOncePerAnswerNeverMore()
        {
            var refreshes = 0;
            var port = new WorkspaceNavigationPortDouble { Result = WorkspaceNavigationResult.Stale("x", true, NavigationActionVerdict.RejectedTargetGone) };
            var model = Ready(port, refresh: () => refreshes++);

            await Act(model, NavigationActionKind.Select);
            Assert.Equal(1, refreshes);
            await Act(model, NavigationActionKind.View);
            Assert.Equal(2, refreshes);
        }

        // ------------------------------------------------------------------ INV-F2T2B-13: nothing runs by itself

        [Fact]
        public void BuildingPresentingAndRefreshingNeverAskThePortForNavigation()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1"), instrumentation: true) };
            host.Report = WorkspacePanelFixtures.Report(1024);
            var port = new WorkspaceNavigationPortDouble();
            var panel = new WorkspacePanelViewModel(host, _ => { }, port);

            panel.Refresh();
            panel.Retry();
            panel.SetInstrumentation(false);
            host.Snapshot = WorkspacePanelFixtures.Ready(session: 8, selection: SelectionContext.One("R-2"));
            panel.Refresh();
            host.Snapshot = WorkspacePanelFixtures.NoDocument();
            panel.Refresh();
            host.SnapshotFault = new InvalidOperationException("boom");
            panel.Refresh();
            panel.ShowError(new InvalidOperationException("view"));

            Assert.Empty(port.Calls);
            Assert.False(panel.Navigation.IsBusy);
        }

        [Fact]
        public void TheNavigationViewModelItselfIsBuiltAndPresentedWithoutAnyCall()
        {
            var port = new WorkspaceNavigationPortDouble();

            var model = Ready(port);
            model.NextCopy();
            model.SelectedView = NavigationViewOrder.Planta;
            model.LocateCentered = true;

            Assert.Empty(port.Calls);
        }

        // ------------------------------------------------------------------ INV-F2T2B-12: the panel view-model and its optional third parameter

        [Fact]
        public void TheTwoArgumentConstructorStillWorksAndTheNavigationIsPresentButUnavailable()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };

            var panel = new WorkspacePanelViewModel(host, _ => { });
            panel.Refresh();

            Assert.NotNull(panel.Navigation);
            Assert.False(panel.Navigation.IsAvailable);
            Assert.False(panel.Navigation.CanNavigate);
            Assert.False(string.IsNullOrWhiteSpace(panel.Navigation.BlockedReason));
        }

        [Theory]
        [InlineData("one", true)]
        [InlineData("selection", false)]
        [InlineData("none", false)]
        [InlineData("degraded", false)]
        [InlineData("command", false)]
        [InlineData("nodocument", false)]
        public void ThePanelEnablesTheNavigationOnlyForTheSnapshotsThatAllowIt(string snapshot, bool expected)
        {
            var host = new WorkspacePortDouble
            {
                Snapshot = snapshot switch
                {
                    "one" => WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")),
                    "selection" => WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.Selection(2, 0)),
                    "none" => WorkspacePanelFixtures.Ready(session: 7),
                    "degraded" => WorkspacePanelFixtures.Degraded(),
                    "command" => WorkspacePanelFixtures.WithBlock(WorkspaceActionBlock.CommandActive),
                    _ => WorkspacePanelFixtures.NoDocument(),
                },
            };
            var panel = new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble());

            panel.Refresh();

            Assert.Equal(expected, panel.Navigation.CanNavigate);
            if (snapshot == "command") Assert.Equal(panel.ActionBlockText, panel.Navigation.BlockedReason);
        }

        [Fact]
        public void TheErrorStateOfThePanelTurnsTheNavigationOffAndARecoveryTurnsItOnAgain()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var panel = new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble());
            panel.Refresh();
            Assert.True(panel.Navigation.CanNavigate);

            host.SnapshotFault = new InvalidOperationException("boom");
            panel.Refresh();
            Assert.False(panel.Navigation.CanNavigate);
            Assert.False(string.IsNullOrWhiteSpace(panel.Navigation.BlockedReason));

            host.SnapshotFault = null;
            panel.Retry();
            Assert.True(panel.Navigation.CanNavigate);
        }

        [Fact]
        public void ThePropertiesTheViewBindsToAreAnnounced()
        {
            var model = new WorkspaceNavigationViewModel(new WorkspaceNavigationPortDouble());
            var names = new List<string>();
            model.PropertyChanged += (_, e) => names.Add(e.PropertyName ?? string.Empty);

            model.Present(new SessionId(1), SelectionContextKind.One, "R-1", true, WorkspaceActionBlock.None, string.Empty);
            model.SelectedView = NavigationViewOrder.Lateral;
            model.LocateCentered = true;

            foreach (var expected in new[] { "CanNavigate", "BlockedReason", "CopyText", "CanNextCopy", "SelectedView", "LocateCentered" })
                Assert.Contains(expected, names);
        }
    }
}
