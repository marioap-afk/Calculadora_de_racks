using System.Linq;
using System.Windows;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 ct: the page of the rack shows the editor of a Cantilever line when it is open (and the user did not ask for the panel), and «Nuevo»
    /// shows the editor of a new line (a draft); one view per editor that the window keeps, as Dinámico, Selectivo and Cabecera.
    /// </summary>
    public partial class RackCadWindowView
    {
        private readonly System.Collections.Generic.Dictionary<CantileverEditorViewModel, CantileverEditorView> _cantileverViews = new();

        /// <summary>How many Cantilever editor views the page keeps.</summary>
        internal int CantileverViewCount => _cantileverViews.Count;

        private CantileverEditorView CantileverViewOf(CantileverEditorViewModel editor)
        {
            if (!_cantileverViews.TryGetValue(editor, out var view))
            {
                view = new CantileverEditorView(editor);
                _cantileverViews[editor] = view;
            }

            return view;
        }

        // True when the page shows the editor of the active line; the views of editors the window no longer keeps are forgotten.
        private bool ShowCantileverContent()
        {
            var live = _viewModel.CantileverEditors;
            foreach (var gone in _cantileverViews.Keys.Where(editor => !live.Contains(editor)).ToList()) _cantileverViews.Remove(gone);
            var active = _viewModel.ActiveCantileverEditor;
            if (active == null || _viewModel.ShowPanel) return false;
            var view = CantileverViewOf(active);
            if (!ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = view;
            return true;
        }

        // «Nuevo» hosts the editor of the new line being made here; the view is the one the page of the rack shows once it is inserted.
        private void ShowNewCantileverContent()
        {
            var editor = _viewModel.NewCantileverEditor;
            if (editor == null)
            {
                NewCantileverRackHost.Content = null;
                return;
            }

            var view = CantileverViewOf(editor);
            if (!ReferenceEquals(NewCantileverRackHost.Content, view)) NewCantileverRackHost.Content = view;
        }

        private void OnEditCantileverHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.EditCantileverHere());

        private void OnNewCantileverHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.StartNewCantilever());
    }
}
