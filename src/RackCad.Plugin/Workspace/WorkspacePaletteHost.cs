using Autodesk.AutoCAD.Windows;
using RackCad.Application.Diagnostics;
using RackCad.Application.Workspace;
using RackCad.Plugin.VentanaRackCad;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The visual host of the workspace panel (I-64 F1-T3B, D-01, D-02): the one palette of the process. It is created
    /// lazily, the first time RACKCAD, RK or RACKPANEL shows the window, and it lives until AutoCAD ends: nothing is created when
    /// the plugin loads and nothing is released on terminate.
    ///
    /// The palette is built with only its title (no tool identifier, so it has no persisted identity) and with no
    /// load or save handlers: the host persists nothing. It is placed once, when the process creates it: a size, a floating
    /// rectangle on screen and an initial dock to the right (F1-T3B-HOST-UI-C1); showing it again never moves it. Closing the palette hides it; the host mirrors that to the
    /// controller. Exactly one root control of the UI project is added: since I-64 A-14 it is the RackCad window, which shows the
    /// workspace panel as the page of the rack; its view-models are fed only by the ports of the controller, by the neutral delegate
    /// of the instrumentation switch and by the door to the classic commands.
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

        // The first placement of the palette (F1-T3B-HOST-UI-C1), in device-independent units: a size and a minimum size, and a
        // floating rectangle that fits any primary screen of at least 1024 x 768. Since I-64 A-14 the window has two columns (the side
        // column and the center), so it starts wider; the side column can be hidden when the palette is narrow.
        private const double InitialWidth = 600;
        private const double InitialHeight = 600;
        private const int MinimumWidth = 300;
        private const int MinimumHeight = 320;
        private const double FloatingLeft = 80;
        private const double FloatingTop = 80;

        private readonly WorkspaceController _controller;
        private readonly PaletteSet _palette;
        private readonly WorkspacePanelViewModel _viewModel;
        private readonly RackCadWindowViewModel _window;
        private readonly RackCadWindowView _root;

        private WorkspacePaletteHost()
        {
            _controller = WorkspaceController.GetOrCreate();
            _viewModel = new WorkspacePanelViewModel(_controller.Port, _controller.SetInstrumentationEnabled, _controller.Navigation, _controller.BrowserSummary, _controller.PilotEdit);
            _window = new RackCadWindowViewModel(_viewModel, new RackCadClassicCommandPort(), null, new RackCadRackDoorPort(), modal: new RackCadModalPort());
            // A drawing whose window has work not written does not close (decision 11 of the Owner on the prototype of Dinámico); since
            // Selectivo E1-B the work counts the drafts of Dinámico and of Selectivo.
            _controller.WindowCloseAdvisor.SetWindowWork(_window.UnwrittenWorkOf);
            _root = new RackCadWindowView(_window);

            _palette = new PaletteSet("RackCad");
            _palette.Style = PaletteSetStyles.ShowPropertiesMenu | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
            _palette.DockEnabled = DockSides.Left | DockSides.Right;
            _palette.KeepFocus = true;
            _palette.AddVisual("RackCad", _root);

            // Once per process, before the palette is ever shown: Show() never repeats it.
            ApplyInitialPlacement();

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

        // The first placement, applied once when the process creates the palette (F1-T3B-HOST-UI-C1, D-01): Smoke-1 showed that a
        // palette created with no size and no position can float off the screen. It gets a size and a minimum size, a floating
        // rectangle on screen for when the user undocks it, and an initial dock to the right. AutoCAD keeps any later move or dock
        // of the user for the rest of the process; nothing is persisted, because the palette still has no identity. A failure is
        // contained and logged: the panel is still shown.
        private void ApplyInitialPlacement()
        {
            try
            {
                _palette.DeviceIndependentMinimumSize = new System.Drawing.Size(MinimumWidth, MinimumHeight);
                _palette.DeviceIndependentSize = new System.Windows.Size(InitialWidth, InitialHeight);
                _palette.InitializeFloatingPosition(new System.Windows.Rect(FloatingLeft, FloatingTop, InitialWidth, InitialHeight));
                _palette.Dock = DockSides.Right;
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspacePaletteHost.ApplyInitialPlacement", ex);
            }
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
                if (_controller.PanelVisible) _window.Refresh();
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspacePaletteHost.OnControllerStateChanged", ex);
            }
        }

        private static void LogFault(string context, System.Exception exception) => RackLog.Exception(context, exception);
    }
}
