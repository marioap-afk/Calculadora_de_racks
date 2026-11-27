using System.Windows;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 at (I-64, sexta tanda): the clicks of «Actualizar todo» in the header and of its panel (the question, the list). Each one is a
    /// gesture of the user: nothing here hands a command to AutoCAD by itself.
    /// </summary>
    public partial class RackCadWindowView
    {
        private void OnUpdateAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.RequestUpdateAll());

        private void OnConfirmUpdateAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmUpdateAll());

        private void OnCancelUpdateAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.CancelUpdateAll());

        private void OnDismissUpdateAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.DismissUpdateAllResults());
    }
}
