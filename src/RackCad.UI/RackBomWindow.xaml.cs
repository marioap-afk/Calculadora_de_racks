using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using RackCad.Application.Bom;

namespace RackCad.UI
{
    public partial class RackBomWindow : Window
    {
        private readonly BillOfMaterials bom;

        public RackBomWindow(BillOfMaterials bom)
        {
            InitializeComponent();
            this.bom = bom ?? new BillOfMaterials(new List<BomLine>());

            var ic = System.Globalization.CultureInfo.InvariantCulture;
            if (this.bom.IsComponentBased)
            {
                // System BOM: show componentes (cabeceras, largueros…); click one to expand its pieces.
                ComponentGrid.ItemsSource = this.bom.Components;
                ComponentGrid.Visibility = Visibility.Visible;
                BomGrid.Visibility = Visibility.Collapsed;
                SummaryText.Text = this.bom.TotalComponents.ToString(ic) + " componentes · "
                    + this.bom.TotalPieces.ToString(ic) + " piezas. Clic en un componente para ver sus piezas.";
            }
            else
            {
                // Piece BOM: a single cabecera or cama on its own.
                BomGrid.ItemsSource = this.bom.Lines;
                SummaryText.Text = this.bom.TotalPieces.ToString(ic)
                    + " piezas en " + this.bom.Lines.Count.ToString(ic) + " líneas.";
            }
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Exportar lista de materiales",
                Filter = "CSV (*.csv)|*.csv|Todos (*.*)|*.*",
                FileName = "lista-materiales.csv",
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                File.WriteAllText(dialog.FileName, BomCsvExporter.ToCsv(bom));
                MessageBox.Show(this, "Lista exportada a:\n" + dialog.FileName, "RackCad", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo exportar: " + ex.Message, "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
