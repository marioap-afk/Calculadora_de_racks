using System;
using System.Windows;
using System.Windows.Media;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// An invisible owner for the editors that the RackCad window opens through a command (I-64 A-14, phase 1). The editors of the classic
    /// menu open as modal children of the menu, which AutoCAD shows as its modal window; without the menu they need an owner that AutoCAD
    /// shows the same way, so AutoCAD stays disabled while the editor is open, exactly as with the menu. This window has no content and no
    /// size: when it is rendered it runs the work with itself as the owner, keeps its result or its failure, and closes.
    /// </summary>
    public sealed class RackModalHostWindow : Window
    {
        private readonly Func<Window, object> _work;
        private bool _ran;

        public RackModalHostWindow(Func<Window, object> work)
        {
            _work = work ?? throw new ArgumentNullException(nameof(work));
            Title = "RackCad";
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Width = 1;
            Height = 1;
            ContentRendered += OnContentRendered;
        }

        /// <summary>What the work returned; null when it returned nothing or failed.</summary>
        public object Result { get; private set; }

        /// <summary>The failure of the work, for the command to report it; null when it ended normally.</summary>
        public Exception Failure { get; private set; }

        private void OnContentRendered(object sender, EventArgs e)
        {
            if (_ran) return;
            _ran = true;
            ContentRendered -= OnContentRendered;

            try
            {
                Result = _work(this);
            }
            catch (Exception ex)
            {
                Failure = ex;
            }
            finally
            {
                Close();
            }
        }
    }
}
