using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The page «Configuración» of the RackCad window (part «ut»). It passes the clicks to the view-model of the window; «Examinar…» and
    /// «Cambiar…» open the selector of Windows, the only windows of the page. Every failure is contained and said in the message of the window.
    /// </summary>
    public partial class RackCadSettingsPage : UserControl
    {
        public RackCadSettingsPage()
        {
            InitializeComponent();
        }

        private RackCadWindowViewModel Window => DataContext as RackCadWindowViewModel;

        // «Examinar…» of the block library: the same dialog the window had (moved here with the page).
        private void OnBrowseBlockLibraryClick(object sender, RoutedEventArgs e) => Contained(window =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Selecciona la biblioteca de bloques",
                Filter = "Dibujo de AutoCAD (*.dwg)|*.dwg|Todos (*.*)|*.*",
                CheckFileExists = true,
            };

            var current = window.BlockLibraryPath;
            try
            {
                var directory = string.IsNullOrWhiteSpace(current) ? null : Path.GetDirectoryName(current);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory)) dialog.InitialDirectory = directory;
                if (!string.IsNullOrWhiteSpace(current) && File.Exists(current)) dialog.FileName = Path.GetFileName(current);
            }
            catch (Exception)
            {
                // An invalid current path only means the dialog opens at its default folder.
            }

            // Inside the palette of AutoCAD the control has no WPF window: the dialog then takes the active window as its owner.
            var owner = System.Windows.Window.GetWindow(this);
            var chosen = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
            if (chosen == true) window.SetBlockLibraryPath(dialog.FileName);
        });

        private void OnResetBlockLibraryClick(object sender, RoutedEventArgs e) => Contained(window => window.ResetBlockLibraryPath());

        // «Cambiar…» of the folder of the designs (UT-02): the folder selector of Windows, opened at the current folder when it exists.
        private void OnChangeDesignLibraryClick(object sender, RoutedEventArgs e) => Contained(window =>
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Selecciona la carpeta de la biblioteca de diseños", Multiselect = false };
            try
            {
                var current = window.DesignLibraryPath;
                if (!string.IsNullOrWhiteSpace(current) && Directory.Exists(current)) dialog.InitialDirectory = current;
            }
            catch (Exception)
            {
                // The dialog opens at its default folder.
            }

            var owner = System.Windows.Window.GetWindow(this);
            var chosen = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
            if (chosen == true) window.SetDesignLibraryPath(dialog.FolderName);
        });

        private void OnResetDesignLibraryClick(object sender, RoutedEventArgs e) => Contained(window => window.ResetDesignLibraryPath());

        private void Contained(Action<RackCadWindowViewModel> interaction)
        {
            var window = Window;
            if (window == null) return;
            try
            {
                interaction(window);
            }
            catch (Exception ex)
            {
                window.ShowError(ex);
            }
        }
    }
}
