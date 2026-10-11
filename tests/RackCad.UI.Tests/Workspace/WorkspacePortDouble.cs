#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Workspace;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F1-T3B-HOST-UI: the simulated host port of the panel tests. It returns what the test sets and counts every
    /// call, so a test can prove that the panel asks only when it is told to and never asks for what it must not.
    /// </summary>
    internal sealed class WorkspacePortDouble : IWorkspaceHostPort
    {
        public WorkspaceHostSnapshot Snapshot { get; set; } = WorkspacePanelFixtures.NoDocument();

        public WorkspaceMeasurementReport Report { get; set; } = WorkspacePanelFixtures.EmptyReport();

        public Exception? SnapshotFault { get; set; }

        public Exception? ReportFault { get; set; }

        public int SnapshotCalls { get; private set; }

        public int ReportCalls { get; private set; }

        public int AuthorizeCalls { get; private set; }

        public int SelectionCalls { get; private set; }

        public int TotalCalls => SnapshotCalls + ReportCalls + AuthorizeCalls + SelectionCalls;

        public WorkspaceHostSnapshot GetSnapshot()
        {
            SnapshotCalls++;
            if (SnapshotFault != null) throw SnapshotFault;
            return Snapshot;
        }

        public WorkspaceMeasurementReport GetMeasurementReport()
        {
            ReportCalls++;
            if (ReportFault != null) throw ReportFault;
            return Report;
        }

        public RequestDecision Authorize(WorkspaceRequest request)
        {
            AuthorizeCalls++;
            return RequestDecision.Accepted;
        }

        public WorkspaceSelectionResponse GetSelectionContext(WorkspaceRequest request)
        {
            SelectionCalls++;
            return new WorkspaceSelectionResponse(RequestDecision.Accepted, SelectionContext.None);
        }
    }

    /// <summary>Plain-value snapshots and reports for the panel tests.</summary>
    internal static class WorkspacePanelFixtures
    {
        private static readonly WorkspaceActionAvailability Open = new(true, false, WorkspaceActionBlock.None);

        public static WorkspaceHostSnapshot NotStarted() =>
            new(WorkspaceHostStatus.NotStarted, null, false, new WorkspaceActionAvailability(false, false, WorkspaceActionBlock.NotStarted), SelectionContext.None, false, false);

        public static WorkspaceHostSnapshot NoDocument() =>
            new(WorkspaceHostStatus.NoDocument, null, true, new WorkspaceActionAvailability(false, false, WorkspaceActionBlock.NoDocument), SelectionContext.None, false, false);

        public static WorkspaceHostSnapshot Ready(long session = 7, SelectionContext? selection = null, bool pickFirstDisabled = false, bool instrumentation = false) =>
            new(WorkspaceHostStatus.Ready, new SessionId(session), true, Open, selection ?? SelectionContext.None, pickFirstDisabled, instrumentation);

        public static WorkspaceHostSnapshot Degraded(long session = 7) =>
            new(WorkspaceHostStatus.Degraded, new SessionId(session), true, new WorkspaceActionAvailability(false, false, WorkspaceActionBlock.Degraded), SelectionContext.None, false, false);

        public static WorkspaceHostSnapshot WithBlock(WorkspaceActionBlock block) =>
            new(WorkspaceHostStatus.Ready, new SessionId(3), true, new WorkspaceActionAvailability(block == WorkspaceActionBlock.None, false, block), SelectionContext.None, false, false);

        public static WorkspaceMeasurementReport EmptyReport() =>
            new(false, Array.Empty<WorkspaceMeasurement>(), 0);

        public static WorkspaceMeasurementReport Report(long memoryBytes, params WorkspaceMeasurement[] points) =>
            new(true, new List<WorkspaceMeasurement>(points), memoryBytes);
    }
}
