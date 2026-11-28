using System.ComponentModel;
using System.Windows;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T4 cb: while the editor of the cabecera of a post is open (<see cref="SelectiveEditorViewModel.EmbeddedCabecera"/>) it takes the place of
    /// the editor of the rack (the prototype: «‹ Selectivo 2 › Cabecera de P2 · Fondo 1»), and «‹ Selectivo 2» gives it back; the editor of
    /// the rack keeps its state underneath. The buttons of the table «Postes y cabeceras» open it and put a cabecera back to the calculated one.
    /// </summary>
    public partial class SelectiveEditorView
    {
        private object _rackContent;
        private CabeceraEditorView _cabeceraView;

        /// <summary>The view of the editor of cabeceras now shown in place of the rack; null when the rack is shown.</summary>
        internal CabeceraEditorView CabeceraView => _cabeceraView;

        private void WireCabecera()
        {
            _rackContent = Content;
            _viewModel.PropertyChanged += OnCabeceraPropertyChanged;
            ShowCabecera();
        }

        private void OnCabeceraPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectiveEditorViewModel.EmbeddedCabecera)) Contained(ShowCabecera);
        }

        private void ShowCabecera()
        {
            var cabecera = _viewModel.EmbeddedCabecera;
            if (cabecera == null)
            {
                _cabeceraView = null;
                if (!ReferenceEquals(Content, _rackContent)) Content = _rackContent;
                return;
            }

            if (_cabeceraView?.ViewModel == cabecera) return;
            _cabeceraView = new CabeceraEditorView(cabecera);
            Content = _cabeceraView;
        }

        private void OnEditCabeceraClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitFocusedText();
            if ((sender as FrameworkElement)?.DataContext is SelectiveCabeceraCell cell) _viewModel.EditCabecera(cell.Fondo, cell.Post);
        });

        private void OnCabeceraToCalculatedClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitFocusedText();
            if ((sender as FrameworkElement)?.DataContext is SelectiveCabeceraCell cell) _viewModel.ResetCabecera(cell.Fondo, cell.Post);
        });
    }
}
