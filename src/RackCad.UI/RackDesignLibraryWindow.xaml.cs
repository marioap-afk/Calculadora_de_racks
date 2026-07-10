using System.Windows;
using System.Windows.Input;
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
            Reload();
        }

        /// <summary>The design the user chose to open (null if none / cancelled).</summary>
        public RackDesignLibraryEntry SelectedDesign { get; private set; }

        private void Reload()
        {
            var entries = RackDesignLibrary.List(folder);
            DesignsGrid.ItemsSource = entries;
            StatusText.Text = entries.Count == 0
                ? "No hay diseños guardados en esta carpeta."
                : entries.Count + " diseño(s).";
        }

        private void Open_Click(object sender, RoutedEventArgs e) => TryOpenSelected();

        private void DesignsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => TryOpenSelected();

        private void TryOpenSelected()
        {
            if (!(DesignsGrid.SelectedItem is RackDesignLibraryEntry entry))
            {
                StatusText.Text = "Selecciona un diseño de la lista.";
                return;
            }

            SelectedDesign = entry;
            DialogResult = true;
            Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => Reload();

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
