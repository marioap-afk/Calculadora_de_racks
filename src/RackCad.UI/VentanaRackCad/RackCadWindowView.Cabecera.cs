using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T4 cb: the page of the rack shows the editor of a cabecera of its own when it is open (and the user did not ask for the panel), with
    /// one view per editor that the window keeps, as Dinámico and Selectivo.
    /// <para>T7 cab: «Cabecera en esta ventana» in «Nuevo» (after «Cantilever en esta ventana») and the host of the editor of a new cabecera,
    /// built here so the shared XAML of the window does not change (the pattern of T5 pb). The constructor calls
    /// <see cref="BuildCabeceraChrome"/> (one line). A view has one parent: when a new cabecera becomes a cabecera of the drawing, its view
    /// moves from «Nuevo» to the page of the rack (the rule of the cama).</para>
    /// </summary>
    public partial class RackCadWindowView
    {
        private readonly System.Collections.Generic.Dictionary<CabeceraEditorViewModel, CabeceraEditorView> _cabeceraViews = new();

        /// <summary>How many cabecera editor views the page keeps.</summary>
        internal int CabeceraViewCount => _cabeceraViews.Count;

        /// <summary>T7 cab: «Cabecera en esta ventana» of «Nuevo».</summary>
        internal Button NewCabeceraHereButton { get; private set; }

        /// <summary>T7 cab: the host of the editor of a new cabecera in «Nuevo».</summary>
        internal ContentControl NewCabeceraRackHost { get; private set; }

        // T7 cab: one line of the constructor.
        private void BuildCabeceraChrome()
        {
            var flag = (IValueConverter)FindResource("FlagToVisibility");
            var title = new TextBlock { Text = "Cabecera en esta ventana", FontWeight = FontWeights.SemiBold };
            var caption = new TextBlock
            {
                Text = "El editor de cabeceras de la ventana, con los valores por defecto del configurador clásico, sin bloquear AutoCAD. Se coloca con "
                       + "«Insertar en el dibujo…» (lateral, planta o las dos) y, si no le pones nombre, toma el automático (I-60).",
                TextWrapping = TextWrapping.Wrap,
            };
            var content = new StackPanel();
            content.Children.Add(title);
            content.Children.Add(caption);
            NewCabeceraHereButton = new Button
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(12, 6, 12, 6),
                MaxWidth = 560,
            };
            NewCabeceraHereButton.Style = (Style)FindResource("PrimaryButtonStyle");
            AutomationProperties.SetName(NewCabeceraHereButton, "Nueva cabecera en esta ventana");
            NewCabeceraHereButton.SetBinding(IsEnabledProperty, new Binding(nameof(RackCadWindowViewModel.CanStartNewCabecera)));
            NewCabeceraHereButton.Click += (_, __) => Contained(() => _viewModel.StartNewCabecera());
            if (NuevoPage.Content is Panel nuevo) nuevo.Children.Insert(nuevo.Children.IndexOf(NewCantileverHereButton) + 1, NewCabeceraHereButton);

            NewCabeceraRackHost = new ContentControl { Focusable = false };
            NewCabeceraRackHost.SetBinding(VisibilityProperty, new Binding(nameof(RackCadWindowViewModel.ShowNewCabeceraEditor)) { Converter = flag });
            if (NewSelectiveRackHost.Parent is Panel center) center.Children.Insert(center.Children.IndexOf(NewSelectiveRackHost) + 1, NewCabeceraRackHost);

            _viewModel.PropertyChanged += OnCabeceraViewModelPropertyChanged;
        }

        private void OnCabeceraViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RackCadWindowViewModel.NewCabeceraEditor) || e.PropertyName == nameof(RackCadWindowViewModel.CabeceraEditors))
                ShowNewCabeceraContent();
        }

        // True when the page shows the editor of the active cabecera; the views of editors the window no longer keeps are forgotten.
        private bool ShowCabeceraContent()
        {
            ForgetGoneCabeceraViews();
            var active = _viewModel.ActiveCabeceraEditor;
            if (active == null || _viewModel.ShowPanel) return false;
            var view = CabeceraViewOf(active);
            if (NewCabeceraRackHost != null && ReferenceEquals(NewCabeceraRackHost.Content, view)) NewCabeceraRackHost.Content = null; // T7 cab: one parent
            if (!ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = view;
            return true;
        }

        // T7 cab: «Nuevo» hosts the editor of the new cabecera; the view is the one the page of the rack shows once it is inserted.
        private void ShowNewCabeceraContent()
        {
            ForgetGoneCabeceraViews();
            var editor = _viewModel.NewCabeceraEditor;
            if (editor == null)
            {
                NewCabeceraRackHost.Content = null;
                return;
            }

            var view = CabeceraViewOf(editor);
            if (ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = PanelView; // a view has one parent
            if (!ReferenceEquals(NewCabeceraRackHost.Content, view)) NewCabeceraRackHost.Content = view;
        }

        private CabeceraEditorView CabeceraViewOf(CabeceraEditorViewModel editor)
        {
            if (_cabeceraViews.TryGetValue(editor, out var view)) return view;
            view = new CabeceraEditorView(editor);
            _cabeceraViews[editor] = view;
            return view;
        }

        private void ForgetGoneCabeceraViews()
        {
            var live = _viewModel.CabeceraEditors;
            foreach (var gone in _cabeceraViews.Keys.Where(editor => !live.Contains(editor)).ToList())
            {
                if (NewCabeceraRackHost?.Content is CabeceraEditorView hosted && ReferenceEquals(hosted.ViewModel, gone)) NewCabeceraRackHost.Content = null;
                _cabeceraViews.Remove(gone);
            }
        }

        private void OnEditCabeceraHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.EditCabeceraHere());
    }
}
