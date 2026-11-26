using System.Windows;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>The gestures of the utilities of the window (part «ut») that live in the root view: «Ayuda» of the header (UT-09).</summary>
    public partial class RackCadWindowView
    {
        private void OnHelpClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ShowHelp());
    }
}
