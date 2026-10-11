#nullable enable
using System;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-01, 05, 09, 10, 11 with a simulated host and a simulated reader.
public class WorkspaceHostLifecycleTests
{
    // ------------------------------------------------------------------ before the first panel open: zero work

    [Fact]
    public void BeforeTheFirstOpenNothingIsSubscribedNothingIsStartedAndNothingIsRead()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        Assert.False(rig.Host.IsStarted);
        Assert.Equal(0, rig.Link.StartCalls);
        Assert.Equal(0, rig.Link.ReconciliationCalls);
        Assert.Equal(0, rig.Bridge.Source.AttachCalls);
        Assert.Equal(0, rig.Bridge.Source.Total);
        Assert.Equal(0, rig.Reader.ReadCalls);
    }

    [Fact]
    public void BeforeTheFirstOpenTheSnapshotIsNotStartedWithEveryActionDisabled()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        var snapshot = rig.Host.GetSnapshot();

        Assert.Equal(WorkspaceHostStatus.NotStarted, snapshot.Status);
        Assert.Null(snapshot.ActiveSession);
        Assert.False(snapshot.Actions.CanReadDrawing);
        Assert.False(snapshot.Actions.CanWriteDrawing);
        Assert.Equal(WorkspaceActionBlock.NotStarted, snapshot.Actions.Block);
        Assert.Equal(0, rig.Bridge.Source.Total);
    }

    // ------------------------------------------------------------------ the first RACKPANEL

    [Fact]
    public void TheFirstOpenStartsTheBridgeOnceAndSubscribesOnlyTheExpectedSources()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        rig.Host.OpenPanel();

        Assert.True(rig.Host.IsStarted);
        Assert.Equal(1, rig.Link.StartCalls);
        Assert.Equal(WorkspaceBridgeRig.CollectionSources + WorkspaceBridgeRig.SourcesPerSession, rig.Bridge.Source.Total - (rig.Bridge.Core.IsIdleAttached ? 1 : 0));
        Assert.Empty(rig.Bridge.Source.Violations);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(25)]
    public void ARepeatedOpenNeverStartsTheBridgeAgainNorDuplicatesASubscription(int opens)
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        rig.Host.OpenPanel();
        var attachCalls = rig.Bridge.Source.AttachCalls - (rig.Bridge.Core.IsIdleAttached ? 1 : 0);
        var hooks = rig.Bridge.Source.Total;
        for (var i = 1; i < opens; i++) rig.Host.OpenPanel();

        Assert.Equal(1, rig.Link.StartCalls);
        Assert.Empty(rig.Bridge.Source.Violations);
        Assert.True(rig.Bridge.Source.AllHooksAreSingle);
        Assert.Equal(hooks, rig.Bridge.Source.Total);
        Assert.Equal(attachCalls + (rig.Bridge.Core.IsIdleAttached ? 1 : 0), rig.Bridge.Source.AttachCalls);
        Assert.Equal(1, rig.Bridge.Core.LiveSessionCount);
    }

    [Fact]
    public void TheFirstOpenMakesThePanelVisibleInTheBridge()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        rig.Host.OpenPanel();

        Assert.True(rig.Host.PanelVisible);
        Assert.True(rig.Link.VisibleHistory.LastOrDefault());
    }

    [Fact]
    public void TheFirstOpenRequestsTheInitialReconciliationAndDoesNotReadYet()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        rig.Host.OpenPanel();

        Assert.Equal(1, rig.Link.ReconciliationCalls);
        Assert.Equal(1, rig.Bridge.SessionOf(rig.ActiveSession).PendingHintCount);
        Assert.True(rig.Bridge.Core.IsIdleAttached);
        Assert.Equal(0, rig.Reader.ReadCalls);
    }

    [Fact]
    public void TheInitialReconciliationHappensAtTheNextIdleThroughTheNormalDrain()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1"));
        rig.Host.OpenPanel();
        var session = rig.ActiveSession;

        var outcomes = rig.Idle();

        Assert.Single(outcomes);
        Assert.Equal(1, rig.Reader.ReadCalls);
        Assert.Equal((session, 100L), rig.Reader.Reads[0]);
        Assert.Equal(SelectionContext.One("RACK-1"), rig.Bridge.SessionOf(session).Selection);
        Assert.False(rig.Bridge.Core.IsIdleAttached);
    }

    [Fact]
    public void NoReadHappensInsideTheOpenCallItself()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1"));

        rig.Host.OpenPanel();
        rig.Host.ShowPanel();
        rig.Host.RefreshPanel();

        Assert.Equal(0, rig.Reader.ReadCalls);
        Assert.Equal(SelectionContext.None, rig.Bridge.SessionOf(rig.ActiveSession).Selection);
    }

    // ------------------------------------------------------------------ visible and idle: zero reads without hints

    [Fact]
    public void AVisibleIdlePanelReadsNothingWithoutPendingHints()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        Assert.Equal(1, rig.Reader.ReadCalls);

        for (var i = 0; i < 20; i++) rig.Idle();

        Assert.Equal(1, rig.Reader.ReadCalls);
        Assert.Empty(rig.Reader.Invalidations);
    }

    [Fact]
    public void AskingForTheSnapshotRepeatedlyReadsNothingAndRequestsNothing()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        var reads = rig.Reader.ReadCalls;
        var reconciliations = rig.Link.ReconciliationCalls;

        for (var i = 0; i < 30; i++) rig.Host.GetSnapshot();

        Assert.Equal(reads, rig.Reader.ReadCalls);
        Assert.Equal(reconciliations, rig.Link.ReconciliationCalls);
        Assert.False(rig.Bridge.Core.IsIdleAttached);
    }

    // ------------------------------------------------------------------ hidden panel: zero reads, hints keep queueing

    [Fact]
    public void AHiddenPanelReadsNothingWhileTheHintsKeepQueueing()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        var session = rig.ActiveSession;
        var reads = rig.Reader.ReadCalls;

        rig.Host.HidePanel();
        for (var i = 0; i < 500; i++) rig.SelectionChanged(session);
        rig.Idle();
        rig.Idle();

        Assert.False(rig.Host.PanelVisible);
        Assert.Equal(reads, rig.Reader.ReadCalls);
        Assert.Equal(1, rig.Bridge.SessionOf(session).PendingHintCount);
        Assert.True(rig.Bridge.Core.IsIdleAttached);
    }

    [Fact]
    public void HidingThePanelIsMirroredInTheBridgeAndRequestsNoReconciliation()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        var reconciliations = rig.Link.ReconciliationCalls;

        rig.Host.HidePanel();

        Assert.False(rig.Link.VisibleHistory.LastOrDefault());
        Assert.Equal(reconciliations, rig.Link.ReconciliationCalls);
    }

    [Fact]
    public void ShowingTheHiddenPanelReconcilesAtTheNextIdleAndNotBefore()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-1"));
        rig.Host.OpenPanel();
        rig.Idle();
        var session = rig.ActiveSession;
        rig.Host.HidePanel();
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-2"));
        rig.SelectionChanged(session);
        rig.Idle();
        var readsWhileHidden = rig.Reader.ReadCalls;
        var reconciliations = rig.Link.ReconciliationCalls;

        rig.Host.ShowPanel();

        Assert.Equal(reconciliations + 1, rig.Link.ReconciliationCalls);
        Assert.Equal(readsWhileHidden, rig.Reader.ReadCalls);
        Assert.True(rig.Link.VisibleHistory.LastOrDefault());

        rig.Idle();

        Assert.Equal(readsWhileHidden + 1, rig.Reader.ReadCalls);
        Assert.Equal(SelectionContext.One("RACK-2"), rig.Bridge.SessionOf(session).Selection);
    }

    [Fact]
    public void RefreshingAVisiblePanelRequestsAReconciliationThatDrainsLater()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        var reconciliations = rig.Link.ReconciliationCalls;
        var reads = rig.Reader.ReadCalls;

        rig.Host.RefreshPanel();

        Assert.Equal(reconciliations + 1, rig.Link.ReconciliationCalls);
        Assert.Equal(reads, rig.Reader.ReadCalls);
        rig.Idle();
        Assert.Equal(reads + 1, rig.Reader.ReadCalls);
    }

    [Fact]
    public void RefreshingAHiddenPanelRequestsNothing()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        rig.Host.HidePanel();
        var reconciliations = rig.Link.ReconciliationCalls;

        rig.Host.RefreshPanel();

        Assert.Equal(reconciliations, rig.Link.ReconciliationCalls);
    }

    [Fact]
    public void ShowingAPanelThatWasNeverOpenedStartsNothing()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        rig.Host.ShowPanel();
        rig.Host.HidePanel();
        rig.Host.RefreshPanel();

        Assert.False(rig.Host.IsStarted);
        Assert.Equal(0, rig.Link.StartCalls);
        Assert.Equal(0, rig.Bridge.Source.Total);
        Assert.Equal(0, rig.Link.ReconciliationCalls);
    }

    // ------------------------------------------------------------------ no document: empty state

    [Fact]
    public void WithoutAnOpenDocumentThePanelIsEmptyWithActionsDisabledAndOnlyTheCollectionSubscriptions()
    {
        var rig = new WorkspaceHostRig();

        rig.Host.OpenPanel();
        var snapshot = rig.Host.GetSnapshot();

        Assert.Equal(WorkspaceHostStatus.NoDocument, snapshot.Status);
        Assert.Null(snapshot.ActiveSession);
        Assert.False(snapshot.Actions.CanReadDrawing);
        Assert.False(snapshot.Actions.CanWriteDrawing);
        Assert.Equal(WorkspaceActionBlock.NoDocument, snapshot.Actions.Block);
        Assert.Equal(SelectionContext.None, snapshot.Selection);
        Assert.Equal(WorkspaceBridgeRig.CollectionSources, rig.Bridge.Source.Total);
        Assert.Equal(0, rig.Bridge.Core.LiveSessionCount);
    }

    [Fact]
    public void WithoutADocumentTheReconciliationRequestIsHarmlessAndNothingIsRead()
    {
        var rig = new WorkspaceHostRig();

        rig.Host.OpenPanel();
        rig.Idle();

        Assert.False(rig.Bridge.Core.IsIdleAttached);
        Assert.Equal(0, rig.Reader.ReadCalls);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void ADocumentOpenedAfterThePanelBecomesTheActiveSession()
    {
        var rig = new WorkspaceHostRig();
        rig.Host.OpenPanel();

        var session = rig.Activate(100);
        var snapshot = rig.Host.GetSnapshot();

        Assert.Equal(WorkspaceHostStatus.Ready, snapshot.Status);
        Assert.Equal(session, snapshot.ActiveSession);
        Assert.True(snapshot.Actions.CanReadDrawing);
        Assert.True(snapshot.Actions.CanWriteDrawing);
        Assert.Equal(WorkspaceActionBlock.None, snapshot.Actions.Block);
    }

    [Fact]
    public void ClosingTheOnlyDocumentReturnsToTheEmptyState()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();

        rig.CloseActive(100);
        var snapshot = rig.Host.GetSnapshot();

        Assert.Equal(WorkspaceHostStatus.NoDocument, snapshot.Status);
        Assert.Null(snapshot.ActiveSession);
        Assert.Equal(SelectionContext.None, snapshot.Selection);
        Assert.False(snapshot.Actions.CanReadDrawing);
    }

    [Fact]
    public void SwitchingDocumentsBindsTheOtherSessionAndKeepsEachContextApart()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("IN-A"));
        rig.Host.OpenPanel();
        rig.Idle();
        var a = rig.ActiveSession;

        var b = rig.Activate(200);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("IN-B"));
        rig.Host.RefreshPanel();
        rig.Idle();

        Assert.NotEqual(a, b);
        Assert.Equal(b, rig.Host.GetSnapshot().ActiveSession);
        Assert.Equal(SelectionContext.One("IN-B"), rig.Host.GetSnapshot().Selection);
        Assert.Equal(SelectionContext.One("IN-A"), rig.Bridge.SessionOf(a).Selection);
        Assert.Equal(200L, rig.Reader.Reads.Last().NativeId);
    }

    [Fact]
    public void ComingBackToTheFirstDocumentShowsItsOwnContextAgain()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("IN-A"));
        rig.Host.OpenPanel();
        rig.Idle();
        rig.Activate(200);

        rig.Activate(100);

        Assert.Equal(SelectionContext.One("IN-A"), rig.Host.GetSnapshot().Selection);
        Assert.Equal(2, rig.Bridge.Core.LiveSessionCount);
    }

    // ------------------------------------------------------------------ command and modal: actions and reads

    [Fact]
    public void AnActiveCommandDisablesTheActionsThatReadOrWriteTheDrawing()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();

        rig.CommandStarts(rig.ActiveSession);
        var snapshot = rig.Host.GetSnapshot();

        Assert.False(snapshot.Actions.CanReadDrawing);
        Assert.False(snapshot.Actions.CanWriteDrawing);
        Assert.Equal(WorkspaceActionBlock.CommandActive, snapshot.Actions.Block);
        Assert.Equal(WorkspaceHostStatus.Ready, snapshot.Status);
    }

    [Fact]
    public void TheActionsComeBackWhenTheCommandEnds()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        rig.CommandStarts(rig.ActiveSession);

        rig.CommandEnds(rig.ActiveSession);

        Assert.True(rig.Host.GetSnapshot().Actions.CanReadDrawing);
    }

    [Fact]
    public void NothingIsReadWhileACommandRunsAndTheSelectionIsReadAtTheNextRest()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        var session = rig.ActiveSession;
        var reads = rig.Reader.ReadCalls;
        rig.Reader.Result = WorkspaceHostRig.Read(0, WorkspaceHostRig.Rack("RACK-9"));

        rig.CommandStarts(session);
        rig.SelectionChanged(session);
        rig.Idle();
        Assert.Equal(reads, rig.Reader.ReadCalls);

        rig.CommandEnds(session);
        rig.Idle();

        Assert.True(rig.Reader.ReadCalls > reads);
        Assert.Equal(SelectionContext.One("RACK-9"), rig.Bridge.SessionOf(session).Selection);
    }

    [Fact]
    public void ARackCadModalDisablesTheActionsAndBlocksTheReads()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        var session = rig.ActiveSession;
        var reads = rig.Reader.ReadCalls;

        rig.Host.SetModalRackCadActive(true);
        rig.SelectionChanged(session);
        rig.Idle();

        var snapshot = rig.Host.GetSnapshot();
        Assert.Equal(WorkspaceActionBlock.ModalActive, snapshot.Actions.Block);
        Assert.False(snapshot.Actions.CanReadDrawing);
        Assert.Equal(reads, rig.Reader.ReadCalls);
        Assert.True(rig.Link.ModalHistory.LastOrDefault());
        Assert.Equal(1, rig.Bridge.SessionOf(session).PendingHintCount);
    }

    [Fact]
    public void WhenTheModalClosesThePendingHintsDrainAtTheNextIdle()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        var session = rig.ActiveSession;
        rig.Host.SetModalRackCadActive(true);
        rig.SelectionChanged(session);
        rig.Idle();
        var reads = rig.Reader.ReadCalls;

        rig.Host.SetModalRackCadActive(false);
        rig.Idle();

        Assert.Equal(reads + 1, rig.Reader.ReadCalls);
        Assert.Equal(WorkspaceActionBlock.None, rig.Host.GetSnapshot().Actions.Block);
    }

    // ------------------------------------------------------------------ instrumentation switch

    [Fact]
    public void TheInstrumentationIsOffByDefaultAndCollectsNothing()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        rig.Host.OpenPanel();
        rig.Idle();

        Assert.False(rig.Host.InstrumentationEnabled);
        Assert.False(rig.Host.GetSnapshot().InstrumentationEnabled);
        Assert.Empty(rig.Host.GetMeasurementReport().Points);
    }

    [Fact]
    public void TheInstrumentationSwitchIsInMemoryAndShownInTheSnapshot()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);

        rig.Host.SetInstrumentationEnabled(true);

        Assert.True(rig.Host.InstrumentationEnabled);
        Assert.True(rig.Host.GetSnapshot().InstrumentationEnabled);
        Assert.True(rig.Instrumentation.Enabled);
        Assert.True(rig.Host.GetMeasurementReport().Enabled);

        rig.Host.SetInstrumentationEnabled(false);

        Assert.False(rig.Instrumentation.Enabled);
    }

    [Fact]
    public void WithTheInstrumentationOnTheLifecycleEmitsStructuredMeasurementPoints()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.SetInstrumentationEnabled(true);

        rig.Host.OpenPanel();
        rig.Idle();
        rig.Host.HidePanel();
        rig.Host.ShowPanel();

        var points = rig.Host.GetMeasurementReport().Points.Select(p => p.Point).ToArray();
        Assert.Contains(WorkspaceMeasurementPoints.PanelOpen, points);
        Assert.Contains(WorkspaceMeasurementPoints.Drain, points);
        Assert.Contains(WorkspaceMeasurementPoints.SelectionRead, points);
        Assert.Contains(WorkspaceMeasurementPoints.PanelHide, points);
        Assert.Contains(WorkspaceMeasurementPoints.PanelShow, points);
        Assert.All(rig.Host.GetMeasurementReport().Points, p => Assert.True(p.Milliseconds >= 0));
        Assert.All(rig.Host.GetMeasurementReport().Points, p => Assert.True(p.ProcessMemoryBytes > 0));
    }

    [Fact]
    public void ANormalMeasurementIsNeverReportedAsAFault()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.SetInstrumentationEnabled(true);

        rig.Host.OpenPanel();
        rig.Idle();
        rig.Host.HidePanel();
        rig.Host.ShowPanel();
        rig.Idle();

        Assert.NotEmpty(rig.Host.GetMeasurementReport().Points);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void ASelectionReadPointCarriesTheNumberOfSelectedReferences()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.SetInstrumentationEnabled(true);
        rig.Reader.Result = WorkspaceHostRig.Read(2, WorkspaceHostRig.Rack("A"), WorkspaceHostRig.Rack("B"), WorkspaceHostRig.Rack("C"));

        rig.Host.OpenPanel();
        rig.Idle();

        var read = Assert.Single(rig.Host.GetMeasurementReport().Points, p => p.Point == WorkspaceMeasurementPoints.SelectionRead);
        Assert.Equal(3, read.Count);
    }

    [Fact]
    public void TheHostCoreRequiresItsCollaborators()
    {
        var rig = new WorkspaceHostRig();

        Assert.Throws<ArgumentNullException>(() => new WorkspaceHostCore(null!, rig.Probe, rig.Reader, rig.Instrumentation));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceHostCore(rig.Link, null!, rig.Reader, rig.Instrumentation));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceHostCore(rig.Link, rig.Probe, null!, rig.Instrumentation));
        Assert.Throws<ArgumentNullException>(() => new WorkspaceHostCore(rig.Link, rig.Probe, rig.Reader, null!));
    }
}
