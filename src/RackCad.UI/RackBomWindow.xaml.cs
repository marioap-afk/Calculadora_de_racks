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
            BomGrid.ItemsSource = this.bom.Lines;
            SummaryText.Text = this.bom.TotalPieces.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + " piezas en " + this.bom.Lines.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " lineas.";
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv|Todos (*.*)|*.*",
                FileName = "bom-cabecera.csv"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                File.WriteAllText(dialog.FileName, BomCsvExporter.ToCsv(bom));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo exportar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
