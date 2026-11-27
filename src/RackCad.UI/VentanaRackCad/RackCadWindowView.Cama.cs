using System.ComponentModel;
using System.Linq;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 cm (I-64, quinta tanda): the page of the rack shows the editor of a cama when it is open (and the user did not ask for the panel),
    /// and «Nuevo» hosts the editor of a new cama being made here, with one view per editor that the window keeps, as Dinámico, Selectivo and
    /// the cabecera. The constructor calls <see cref="WireCamaView"/> (one line) and <c>ShowRackContent</c> asks <see cref="ShowCamaContent"/>
    /// first (one line).
    /// </summary>
    public partial class RackCadWindowView
    {
        private readonly System.Collections.Generic.Dictionary<CamaEditorViewModel, CamaEditorView> _camaViews = new();

        /// <summary>How many cama editor views the window keeps.</summary>
        internal int CamaViewCount => _camaViews.Count;

        private void WireCamaView() => _viewModel.PropertyChanged += OnCamaViewModelPropertyChanged;

        private void OnCamaViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RackCadWindowViewModel.ActiveCamaEditor) || e.PropertyName == nameof(RackCadWindowViewModel.CamaEditors))
                ShowRackContent();
            if (e.PropertyName == nameof(RackCadWindowViewModel.NewCamaEditor) || e.PropertyName == nameof(RackCadWindowViewModel.CamaEditors))
                ShowNewCamaContent();
        }

        // True when the page shows the editor of the active cama; the views of editors the window no longer keeps are forgotten.
        private bool ShowCamaContent()
        {
            ForgetGoneCamaViews();
            var active = _viewModel.ActiveCamaEditor;
            if (active == null || _viewModel.ShowPanel) return false;
            var view = CamaViewOf(active);
            if (ReferenceEquals(NewCamaRackHost.Content, view)) NewCamaRackHost.Content = null; // a view has one parent
            if (!ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = view;
            return true;
        }

        // «Nuevo» hosts the editor of the new cama; the view is the one the page of the rack shows once it is inserted.
        private void ShowNewCamaContent()
        {
            ForgetGoneCamaViews();
            var editor = _viewModel.NewCamaEditor;
            if (editor == null)
            {
                NewCamaRackHost.Content = null;
                return;
            }

            var view = CamaViewOf(editor);
            if (ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = PanelView; // a view has one parent
            if (!ReferenceEquals(NewCamaRackHost.Content, view)) NewCamaRackHost.Content = view;
        }

        private CamaEditorView CamaViewOf(CamaEditorViewModel editor)
        {
            if (_camaViews.TryGetValue(editor, out var view)) return view;
            view = new CamaEditorView(editor);
            _camaViews[editor] = view;
            return view;
        }

        private void ForgetGoneCamaViews()
        {
            var live = _viewModel.CamaEditors;
            foreach (var gone in _camaViews.Keys.Where(editor => !live.Contains(editor)).ToList())
            {
                if (NewCamaRackHost.Content is CamaEditorView hosted && ReferenceEquals(hosted.ViewModel, gone)) NewCamaRackHost.Content = null;
                _camaViews.Remove(gone);
            }
        }
    }
}
