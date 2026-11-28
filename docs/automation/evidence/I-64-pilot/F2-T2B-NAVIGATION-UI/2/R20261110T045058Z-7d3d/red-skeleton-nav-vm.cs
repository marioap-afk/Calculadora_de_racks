using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>One entry of the explicit view choice of "Localizar vista".</summary>
    public sealed class WorkspaceViewChoice
    {
        public WorkspaceViewChoice(NavigationViewOrder order, string label)
        {
            Order = order;
            Label = label ?? string.Empty;
        }

        public NavigationViewOrder Order { get; }

        public string Label { get; }
    }

    /// <summary>
    /// RED skeleton of the navigation view-model (I-64 F2-T2B): the public surface compiles and does nothing. Nothing is
    /// enabled, no copy is kept, no port is asked and no text is produced; the behavior arrives with the GREEN commit.
    /// </summary>
    public sealed class WorkspaceNavigationViewModel : ObservableObject
    {
        public WorkspaceNavigationViewModel(IWorkspaceNavigationPort port, Action refreshRequested = null)
        {
        }

        public bool IsAvailable => false;

        public bool IsBusy => false;

        public bool CanNavigate => false;

        public string BlockedReason => string.Empty;

        public bool HasBlockedReason => false;

        public IReadOnlyList<WorkspaceViewChoice> ViewChoices => Array.Empty<WorkspaceViewChoice>();

        public NavigationViewOrder SelectedView { get; set; }

        public bool LocateCentered { get; set; }

        public NavigationActionKind LocateWith => NavigationActionKind.View;

        public int CopyIndex => 0;

        public int? TargetCount => null;

        public string CopyText => string.Empty;

        public bool CanNextCopy => false;

        public string ResultText => string.Empty;

        public bool HasResult => false;

        public void Present(
            SessionId? session,
            SelectionContextKind kind,
            string rackId,
            bool canReadDrawing,
            WorkspaceActionBlock block,
            string blockText)
        {
        }

        public void NextCopy()
        {
        }

        public Task SelectAsync() => Task.CompletedTask;

        public Task ViewAsync() => Task.CompletedTask;

        public Task CenterAsync() => Task.CompletedTask;

        public Task LocateAsync() => Task.CompletedTask;

        public Task NavigateAsync(NavigationActionKind action) => Task.CompletedTask;
    }
}
