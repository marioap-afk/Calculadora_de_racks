using System;
using System.Collections.Generic;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// View-model of the workspace panel (I-64 F1-T3B). Skeleton of the RED step: the full surface the view and the tests
    /// bind to, with bodies that do not yet present anything.
    /// </summary>
    public sealed class WorkspacePanelViewModel : ObservableObject
    {
        private readonly IWorkspaceHostPort _port;
        private readonly Action<bool> _setInstrumentationEnabled;

        public WorkspacePanelViewModel(IWorkspaceHostPort port, Action<bool> setInstrumentationEnabled)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _setInstrumentationEnabled = setInstrumentationEnabled ?? throw new ArgumentNullException(nameof(setInstrumentationEnabled));
        }

        public string Label => string.Empty;

        public WorkspacePanelState State => WorkspacePanelState.NotStarted;

        public string StateTitle => string.Empty;

        public string StateDetail => string.Empty;

        public bool IsEmptyState => false;

        public string SessionText => string.Empty;

        public bool CanReadDrawing => false;

        public bool CanWriteDrawing => false;

        public WorkspaceActionBlock ActionBlock => WorkspaceActionBlock.None;

        public string ActionBlockText => string.Empty;

        public bool PickFirstNoticeVisible => false;

        public string PickFirstNotice => string.Empty;

        public SelectionContextKind ContextKind => SelectionContextKind.None;

        public string ContextTitle => string.Empty;

        public string ContextText => string.Empty;

        public string RackId => null;

        public bool HasError => false;

        public string ErrorMessage => string.Empty;

        public bool InstrumentationEnabled => false;

        public string InstrumentationSummary => string.Empty;

        public string ProcessMemoryText => string.Empty;

        public IReadOnlyList<WorkspaceMeasurementRow> Measurements { get; } = Array.Empty<WorkspaceMeasurementRow>();

        /// <summary>Host-requested refresh (after a state notice): one query of the snapshot and, when it applies, of the report.</summary>
        public void Refresh()
        {
        }

        /// <summary>Explicit retry of the user after a contained failure.</summary>
        public void Retry()
        {
        }

        /// <summary>Explicit change of the in-memory instrumentation switch.</summary>
        public void SetInstrumentation(bool enabled)
        {
        }
    }
}
