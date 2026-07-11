using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RackCad.UI
{
    /// <summary>
    /// RACKLISTA: tabulates every rack found in the drawing (name, type, views present, placed copies).
    /// The plugin scans the drawing and hands the finished rows in; on "Ir al rack" it sets
    /// <see cref="SelectedEntry"/> and closes with a positive result so the caller can zoom to that rack.
    /// </summary>
    public partial class RackListWindow : Window
    {
        public RackListWindow(IReadOnlyList<RackListRow> rows)
        {
            InitializeComponent();

            rows = rows ?? new List<RackListRow>();
            RacksGrid.ItemsSource = rows;
            SetStatus(rows.Count == 0
                ? "No hay racks en el dibujo."
                : rows.Count + " rack(s) en el dibujo.", false);

            // Preselect the first rack so Enter ("Ir al rack") works without a prior click.
            if (rows.Count > 0)
            {
                RacksGrid.SelectedIndex = 0;
                RacksGrid.Focus();
            }
        }

        /// <summary>The rack the user chose to zoom to (null if none / cancelled).</summary>
        public RackListRow SelectedEntry { get; private set; }

        private void GoTo_Click(object sender, RoutedEventArgs e) => TryGoToSelected();

        private void RacksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => TryGoToSelected();

        private void TryGoToSelected()
        {
            if (!(RacksGrid.SelectedItem is RackListRow row))
            {
                SetStatus("Selecciona un rack de la lista.", true);
                return;
            }

            SelectedEntry = row;
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void SetStatus(string message, bool isError) => UiSupport.SetStatus(StatusText, message, isError);
    }
}
