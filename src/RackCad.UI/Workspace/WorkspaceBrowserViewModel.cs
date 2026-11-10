using System;
using System.Collections.Generic;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// One tab of the strip as the panel shows it: a plain value (I-64 F3-T2, P-07, INV-02). It carries no event, no delegate, no
    /// object of AutoCAD and no reference to a visual element, so a tab adds no subscription of any kind.
    /// </summary>
    public sealed record WorkspaceBrowserTabItem(string RackId, string Title, bool IsActive, bool IsPreview);

    /// <summary>
    /// Browser of the workspace panel (I-64 F3-T2). SKELETON of the RED step: the surface compiles and presents nothing; the
    /// behavior (tabs per session, the one synchronization, the lazy summary, the one live surface) arrives with the GREEN step.
    /// </summary>
    public sealed class WorkspaceBrowserViewModel : ObservableObject
    {
        private readonly IWorkspaceBrowserSummaryPort _port;
        private readonly Action _tabChanged;
        private readonly BrowserSessionRegistry _registry = new();

        public WorkspaceBrowserViewModel(IWorkspaceBrowserSummaryPort port, Action tabChanged = null)
        {
            _port = port;
            _tabChanged = tabChanged;
        }

        public bool IsAvailable => _port != null;

        public IReadOnlyList<WorkspaceBrowserTabItem> Tabs => Array.Empty<WorkspaceBrowserTabItem>();

        public bool HasTabs => false;

        public string ActiveRackId => null;

        public int LiveSurfaceCount => 0;

        public bool HasSurface => false;

        public string EmptyText => string.Empty;

        public string SurfaceTitle => string.Empty;

        public string SurfaceKindText => string.Empty;

        public string SurfaceViewsText => string.Empty;

        public string SurfaceStatusText => string.Empty;

        public void Present(SessionId? session, SelectionContext context, bool canReadDrawing, WorkspaceActionBlock block, string blockText)
        {
        }

        public void ActivateTab(string rackId)
        {
        }

        public void PinTab(string rackId)
        {
        }

        public void CloseTab(string rackId)
        {
        }
    }
}
