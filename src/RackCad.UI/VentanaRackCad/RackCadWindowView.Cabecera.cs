using System.Linq;
using System.Windows;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T4 cb: the page of the rack shows the editor of a cabecera of its own when it is open (and the user did not ask for the panel), with
    /// one view per editor that the window keeps, as Dinámico and Selectivo.
    /// </summary>
    public partial class RackCadWindowView
    {
        private readonly System.Collections.Generic.Dictionary<CabeceraEditorViewModel, CabeceraEditorView> _cabeceraViews = new();

        /// <summary>How many cabecera editor views the page keeps.</summary>
        internal int CabeceraViewCount => _cabeceraViews.Count;

        // True when the page shows the editor of the active cabecera; the views of editors the window no longer keeps are forgotten.
        private bool ShowCabeceraContent()
        {
            var live = _viewModel.CabeceraEditors;
            foreach (var gone in _cabeceraViews.Keys.Where(editor => !live.Contains(editor)).ToList()) _cabeceraViews.Remove(gone);
            var active = _viewModel.ActiveCabeceraEditor;
            if (active == null || _viewModel.ShowPanel) return false;
            if (!_cabeceraViews.TryGetValue(active, out var view))
            {
                view = new CabeceraEditorView(active);
                _cabeceraViews[active] = view;
            }

            if (!ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = view;
            return true;
        }

        private void OnEditCabeceraHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.EditCabeceraHere());
    }
}
