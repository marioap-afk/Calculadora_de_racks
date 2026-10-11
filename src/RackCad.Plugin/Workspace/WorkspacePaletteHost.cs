using Autodesk.AutoCAD.Windows;
using RackCad.Application.Diagnostics;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The visual host of the workspace panel (I-64 F1-T3B, D-01, D-02): the one palette of the process. It is created
    /// lazily, the first time RACKPANEL shows the panel, and it lives until AutoCAD ends: nothing is created when the plugin
    /// loads and nothing is released on terminate.
    ///
    /// The palette is built with only its title (no tool identifier, so it has no persisted identity) and with no
    /// load or save handlers: the host persists nothing. Closing the palette hides it; the host mirrors that to the
    /// controller. Exactly one root control of the UI project is added, labelled "Workspace"; its view-model is fed only by
    /// the read-only port of the controller and by the neutral delegate of the instrumentation switch.
    ///
    /// The refresh is driven by events and never by time: the host pulls the state (through the view-model) only when the
    /// controller announces a change. It runs no timer, no dispatcher work, no thread, reads nothing of the drawing and
    /// holds no AutoCAD object. Focus uses only the public <c>KeepFocus</c> of the palette; real keyboard behaviour is for
    /// the manual smoke. Every handler is contained: a failure is logged and never reaches the host message loop.
    /// </summary>
    public sealed class WorkspacePaletteHost
    {
        private static readonly WorkspaceLazyInstance<WorkspacePaletteHost> Holder =
            new WorkspaceLazyInstance<WorkspacePaletteHost>(() => new WorkspacePaletteHost());

        private readonly WorkspaceController _controller;
        private readonly PaletteSet _palette;
        private readonly WorkspacePanelViewModel _viewModel;
        private readonly WorkspacePanelView _root;

        private WorkspacePaletteHost()
        {
            _controller = WorkspaceController.GetOrCreate();
            _viewModel = new WorkspacePanelViewModel(_controller.Port, _controller.SetInstrumentationEnabled);
            _root = new WorkspacePanelView(_viewModel);

            _palette = new PaletteSet("RackCad");
            _palette.Style = PaletteSetStyles.ShowPropertiesMenu | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
            _palette.DockEnabled = DockSides.Left | DockSides.Right;
            _palette.KeepFocus = true;
            _palette.AddVisual("Workspace", _root);

            // The subscriptions are the last step: once they exist nothing can fail any more.
            _palette.StateChanged += OnPaletteStateChanged;
            _controller.StateChanged += OnControllerStateChanged;

            // The first state: the controller announced its opening before this host existed.
            OnControllerStateChanged();
        }

        /// <summary>The host of the process. The first call creates it and its palette; later calls return the same one.</summary>
        public static WorkspacePaletteHost GetOrCreate() => Holder.GetOrCreate();

        /// <summary>Whether the first RACKPANEL already created the host.</summary>
        public static bool IsCreated => Holder.IsCreated;

        /// <summary>Shows the palette (docked or floating, wherever the user left it). Repeating it creates nothing new.</summary>
        public void Show()
        {
            _palette.Visible = true;
        }

        // The palette was shown or hidden (closing it hides it): the controller mirrors the panel visibility.
        private void OnPaletteStateChanged(object sender, PaletteSetStateEventArgs e)
        {
            try
            {
                switch (e.NewState)
                {
                    case StateEventIndex.Show:
                        _controller.ShowPanel();
                        break;
                    case StateEventIndex.Hide:
                        _controller.HidePanel();
                        break;
                }
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspacePaletteHost.OnPaletteStateChanged", ex);
            }
        }

        // The controller announced a change of state: the view-model pulls one snapshot through the port, but only while
        // the panel is visible (a hidden panel presents nothing and asks for nothing).
        private void OnControllerStateChanged()
        {
            try
            {
                if (_controller.PanelVisible) _viewModel.Refresh();
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspacePaletteHost.OnControllerStateChanged", ex);
            }
        }

        private static void LogFault(string context, System.Exception exception) => RackLog.Exception(context, exception);
    }
}
