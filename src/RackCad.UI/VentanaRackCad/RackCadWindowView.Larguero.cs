using System.Windows;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The gesture of part «lg» that lives in the root view: «Larguero en esta ventana» of «Nuevo» (a draft in construction).</summary>
    public partial class RackCadWindowView
    {
        private void OnLargueroHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.OpenLargueroWorkbench());
    }
}
