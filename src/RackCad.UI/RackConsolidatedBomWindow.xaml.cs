using System;
using System.Globalization;
using System.IO;
using System.Windows;
using RackCad.Application.Bom;

namespace RackCad.UI
{
    /// <summary>Shows the whole-drawing bill of materials: a per-rack breakdown (double-click a rack to open its own
    /// component BOM) plus the grand total (every piece summed ×copies). Exports the lot to CSV.</summary>
    public partial class RackConsolidatedBomWindow : Window
    {
        private readonly ConsolidatedBom consolidated;

        public RackConsolidatedBomWindow(ConsolidatedBom consolidated)
        {
            InitializeComponent();
            this.consolidated = consolidated ?? new ConsolidatedBom(new System.Collections.Generic.List<ConsolidatedRackBom>(), new BillOfMaterials(new System.Collections.Generic.List<BomLine>()));

            RacksGrid.ItemsSource = this.consolidated.Racks;
            TotalGrid.ItemsSource = this.consolidated.GrandTotal.Components;

            var ic = CultureInfo.InvariantCulture;
            SummaryText.Text = this.consolidated.RackCount.ToString(ic) + " racks · "
                + this.consolidated.TotalCopies.ToString(ic) + " copias · "
                + this.consolidated.GrandTotal.TotalComponents.ToString(ic) + " componentes · "
                + this.consolidated.GrandTotal.TotalPieces.ToString(ic) + " piezas en total.";
        }

        private void RacksGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (RacksGrid.SelectedItem is ConsolidatedRackBom rack && rack.Bom != null)
            {
                new RackBomWindow(rack.Bom) { Owner = this, Title = "BOM · " + (rack.Name ?? "rack") }.ShowDialog();
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Exportar lista de materiales del dibujo a Excel",
                Filter = "Excel (*.xlsx)|*.xlsx|Todos (*.*)|*.*",
                FileName = "lista-materiales-dibujo.xlsx",
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                File.WriteAllBytes(dialog.FileName, ConsolidatedBomXlsxExporter.ToXlsx(consolidated));
                MessageBox.Show(this, "Lista exportada a:\n" + dialog.FileName, "RackCad", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo exportar: " + ex.Message, "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Exportar lista de materiales del dibujo",
                Filter = "CSV (*.csv)|*.csv|Todos (*.*)|*.*",
                FileName = "lista-materiales-dibujo.csv",
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                File.WriteAllText(dialog.FileName, ConsolidatedBomCsvExporter.ToCsv(consolidated));
                MessageBox.Show(this, "Lista exportada a:\n" + dialog.FileName, "RackCad", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo exportar: " + ex.Message, "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
