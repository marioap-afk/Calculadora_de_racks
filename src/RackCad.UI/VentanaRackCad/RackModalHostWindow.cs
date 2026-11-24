using System;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// An invisible owner for the editors that the RackCad window opens through a command (I-64 A-14, phase 1). The editors of the classic
    /// menu open as modal children of the menu, which AutoCAD shows as its modal window; without the menu they need an owner that AutoCAD
    /// shows the same way, so AutoCAD stays disabled while the editor is open, exactly as with the menu.
    ///
    /// It is a plain WPF <see cref="System.Windows.Window"/> configured here, not a new window type (the census of windows of ADR-0029 has no
    /// archetype for it). When the window is loaded and its modal loop runs, the work runs with the window as its owner, at background
    /// priority, keeps its result or its failure, and closes the window. The window has an empty content, transparent, 1 × 1, centered.
    /// </summary>
    public sealed class RackModalHostWindow
    {
        private readonly Func<Window, object> _work;
        private bool _ran;

        public RackModalHostWindow(Func<Window, object> work)
        {
            _work = work ?? throw new ArgumentNullException(nameof(work));
            Window = new Window
            {
                Title = "RackCad",
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = 1,
                Height = 1,
                Content = new Border(),
            };
            Window.Loaded += OnLoaded;
        }

        /// <summary>The window the host command shows as AutoCAD's modal window.</summary>
        public Window Window { get; }

        /// <summary>What the work returned; null when it returned nothing or failed.</summary>
        public object Result { get; private set; }

        /// <summary>The failure of the work, for the command to report it; null when it ended normally.</summary>
        public Exception Failure { get; private set; }

        /// <summary>Rethrows the failure of the work with its original stack, if there was one.</summary>
        public void ThrowIfFailed()
        {
            if (Failure != null) ExceptionDispatchInfo.Capture(Failure).Throw();
        }

        // Loaded comes inside the modal loop of ShowDialog; the work is posted so it runs once the window is shown and owns its children.
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Window.Loaded -= OnLoaded;
            Window.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(Run));
        }

        private void Run()
        {
            if (_ran) return;
            _ran = true;

            try
            {
                Result = _work(Window);
            }
            catch (Exception ex)
            {
                Failure = ex;
            }
            finally
            {
                Window.Close();
            }
        }
    }
}
