using Autodesk.AutoCAD.Windows;
using RackCad.Application.Diagnostics;
using RackCad.Application.VentanaRackCad;
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
    /// T6 rl (A-14 §3, «la ventana recuerda», approved by the Owner on 2026-11-23; the guards of F1-T3B re-pointed by the Owner on
    /// 2026-11-27): the palette is built with its title and ONE stable identity (<see cref="PaletteId"/>, a constant that never changes), so
    /// AutoCAD saves its size, its position and whether it is docked in the profile of the user, and puts them back in the next session. The
    /// host writes nothing itself: its only handler of persistence is a minimal <c>Load</c> that notes that AutoCAD had a saved state, and
    /// there is no <c>Save</c>. The first placement of F1-T3B-HOST-UI-C1 (a size, a floating rectangle on screen and an initial dock to the
    /// right) is applied once, when the process creates the palette, and only when AutoCAD restored nothing; showing the palette never applies
    /// it again, and only brings a floating palette back onto a screen when it would be out of reach (<see cref="KeepOnScreen"/>). Closing the palette hides it; the host mirrors that to the
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

        // T6 rl (A-14 §3): the identity under which AutoCAD saves the state of the palette in the profile of the user. It never changes: a new
        // value would forget, for every user, where the palette was.
        private static readonly System.Guid PaletteId = new System.Guid("4dc6f19b-296f-424d-8e74-82c7312e2c6a");

        // T6 rl: AutoCAD had a saved state for this identity (its Load came): the first placement is not applied over it.
        private bool _restoredByAutoCad;

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

            _palette = new PaletteSet("RackCad", PaletteId);
            _palette.Load += OnPaletteLoad; // T6 rl: as soon as the palette exists, before anything can make AutoCAD restore it
            _palette.Style = PaletteSetStyles.ShowPropertiesMenu | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
            _palette.DockEnabled = DockSides.Left | DockSides.Right;
            _palette.KeepFocus = true;
            _palette.AddVisual("RackCad", _root);

            // Once per process, before the palette is ever shown: Show() never repeats it. It places nothing over a state AutoCAD restored.
            ApplyInitialPlacement();

            // The two other subscriptions are the last step: once they exist nothing can fail any more.
            _palette.StateChanged += OnPaletteStateChanged;
            _controller.StateChanged += OnControllerStateChanged;

            // The first state: the controller announced its opening before this host existed.
            OnControllerStateChanged();
        }

        /// <summary>The host of the process. The first call creates it and its palette; later calls return the same one.</summary>
        public static WorkspacePaletteHost GetOrCreate() => Holder.GetOrCreate();

        /// <summary>Whether the first RACKPANEL already created the host.</summary>
        public static bool IsCreated => Holder.IsCreated;

        /// <summary>Shows the palette (docked or floating, wherever the user left it), on a screen. Repeating it creates nothing new.</summary>
        public void Show()
        {
            _palette.Visible = true;
            KeepOnScreen();
        }

        // The first placement, applied once when the process creates the palette (F1-T3B-HOST-UI-C1, D-01): Smoke-1 showed that a
        // palette created with no size and no position can float off the screen. It gets a size and a minimum size, a floating
        // rectangle on screen for when the user undocks it, and an initial dock to the right. T6 rl (A-14 §3): AutoCAD saves any later
        // move, size or dock of the user under the identity of the palette and restores it in the next session; so the placement is applied
        // only when AutoCAD restored nothing (no Load came). If AutoCAD restores the state at the first Visible instead, it lays it over
        // these values, which are then only those of the first time. A failure is contained and logged: the panel is still shown.
        private void ApplyInitialPlacement()
        {
            try
            {
                if (_restoredByAutoCad) return; // T6 rl: AutoCAD put back where the user left it
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

        // T6 rl (A-14 §3): AutoCAD restores the state it saved for the identity of the palette; the host only notes that there was one.
        private void OnPaletteLoad(object sender, PalettePersistEventArgs e)
        {
            try
            {
                _restoredByAutoCad = true;
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspacePaletteHost.OnPaletteLoad", ex);
            }
        }

        // T6 rl (A-14 §3; SM-03 of Smoke-1 was a palette off the screen): a floating palette whose title bar is on no work area of the monitors
        // of now (one was removed, or the scaling changed), or larger than the work area that holds it, is brought back onto a screen and cut to
        // it. In device pixels, the units of Size, of the screen point of the root control and of the work areas of the screens, so monitors
        // with different scaling compare alike. A docked palette is AutoCAD's. A failure is contained and logged: the panel is still shown.
        // T6 smoke (2026-11-27): the getter of PaletteSet.Location of AutoCAD 2025 gives the RIGHT edge as its Y (a floating palette at
        // 60,60-1350,920 read {X=60,Y=1350}), so every RACKCAD sent a palette that was on the screen to 80,80 and the position the user left
        // was lost on the next session. The corner of the palette is therefore read where WPF shows the RackCad window, its top-left on the
        // screen; Location is only written. A window not on a screen yet has no corner: nothing is measured and nothing is moved.
        private void KeepOnScreen()
        {
            try
            {
                if (_palette.Dock != DockSides.None) return;
                if (System.Windows.PresentationSource.FromVisual(_root) == null) return;
                var corner = _root.PointToScreen(new System.Windows.Point(0, 0));
                var size = _palette.Size;
                var areas = new System.Collections.Generic.List<ScreenRect>();
                foreach (var screen in System.Windows.Forms.Screen.AllScreens) areas.Add(RectOf(screen.WorkingArea));
                var primary = System.Windows.Forms.Screen.PrimaryScreen;
                var kept = RackCadPalettePlacement.KeepOnScreen(
                    new ScreenRect((int)System.Math.Round(corner.X), (int)System.Math.Round(corner.Y), size.Width, size.Height), areas,
                    primary == null ? default : RectOf(primary.WorkingArea));
                if (kept is not ScreenRect target) return;
                _palette.Size = new System.Drawing.Size(target.Width, target.Height);
                _palette.Location = new System.Drawing.Point(target.Left, target.Top);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspacePaletteHost.KeepOnScreen", ex);
            }
        }

        private static ScreenRect RectOf(System.Drawing.Rectangle area) => new ScreenRect(area.X, area.Y, area.Width, area.Height);

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
