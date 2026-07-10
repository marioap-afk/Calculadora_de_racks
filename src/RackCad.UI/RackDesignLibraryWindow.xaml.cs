using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RackCad.Application.Persistence;

namespace RackCad.UI
{
    /// <summary>
    /// Browses the named designs saved in the library folder (cabecera / sistema dinámico as <c>.rackcad.json</c>) and
    /// lets the user pick one to open. On "Abrir" it sets <see cref="SelectedDesign"/> and closes with a positive result;
    /// the caller (main menu) opens the matching editor preloaded with the design.
    /// </summary>
    public partial class RackDesignLibraryWindow : Window
    {
        private readonly string folder;

        public RackDesignLibraryWindow(string folder)
        {
            InitializeComponent();
            this.folder = folder;
            FolderText.Text = "Carpeta: " + (folder ?? string.Empty);
            FolderText.ToolTip = folder;
            Reload();
        }

        /// <summary>The design the user chose to open (null if none / cancelled).</summary>
        public RackDesignLibraryEntry SelectedDesign { get; private set; }

        private void Reload()
        {
            var entries = RackDesignLibrary.List(folder);
            DesignsGrid.ItemsSource = entries;
            SetStatus(entries.Count == 0
                ? "No hay diseños guardados en esta carpeta."
                : entries.Count + " diseño(s).", false);

            // Preselect the first design so Enter ("Abrir") works without a prior click.
            if (entries.Count > 0)
            {
                DesignsGrid.SelectedIndex = 0;
                DesignsGrid.Focus();
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e) => TryOpenSelected();

        private void DesignsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => TryOpenSelected();

        private void TryOpenSelected()
        {
            if (!(DesignsGrid.SelectedItem is RackDesignLibraryEntry entry))
            {
                SetStatus("Selecciona un diseño de la lista.", true);
                return;
            }

            SelectedDesign = entry;
            DialogResult = true;
            Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => Reload();

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void SetStatus(string message, bool isError)
        {
            // Shared status palette across the rack windows: red #B00020 error / green #2F855A ok.
            StatusText.Text = message ?? string.Empty;
            StatusText.Foreground = isError
                ? new SolidColorBrush(Color.FromRgb(0xB0, 0x00, 0x20))
                : new SolidColorBrush(Color.FromRgb(0x2F, 0x85, 0x5A));
        }
    }
}
