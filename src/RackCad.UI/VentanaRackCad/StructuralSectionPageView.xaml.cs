using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using RackCad.UI.StructuralSections;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T7 per (I-64, séptima tanda): «Nuevo › Perfil estructural» of the RackCad window. It places the controls of the classic inspector
    /// (<see cref="StructuralSectionInspectorControls"/>, the same ones «Secciones estructurales» shows) over the state of its view-model, the
    /// preview beside them when there is room and below them when there is not, and passes the clicks of the user to the view-model. A new
    /// state (the first showing, or a catalogue that changed) gets new controls; without a state (the catalogue failed) there are none. A
    /// failure is contained and said in the message of the window.
    /// </summary>
    public partial class StructuralSectionPageView : UserControl
    {
        /// <summary>Below this width of the page, the preview goes below the parameters.</summary>
        internal const double PreviewBesideWidth = 620;

        private const string NameSuffix = " (perfil estructural)";

        private readonly StructuralSectionPageViewModel _viewModel;
        private StructuralSectionInspectorControls _controls;

        public StructuralSectionPageView(StructuralSectionPageViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
            _viewModel.PropertyChanged += OnViewModelChanged;
            ShowControls();
        }

        /// <summary>The controls of the inspector the page shows now; null without a state.</summary>
        internal StructuralSectionInspectorControls Controls => _controls;

        /// <summary>Whether the preview is below the parameters (a narrow window).</summary>
        internal bool PreviewBelow { get; private set; }

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StructuralSectionPageViewModel.State)) _viewModel.Contained(ShowControls);
        }

        // The controls of the classic inspector over the state of the page, filled at once (the page is being shown).
        private void ShowControls()
        {
            var state = _viewModel.State;
            if (_controls != null && ReferenceEquals(_controls.State, state)) return;

            if (_controls != null) _controls.Refreshing -= OnControlsRefreshing;
            _controls = null;
            SectionParametersHost.Content = null;
            SectionPreviewHost.Content = null;
            SectionInformationHost.Content = null;
            if (state == null) return;

            _controls = new StructuralSectionInspectorControls(state);
            _controls.Refreshing += OnControlsRefreshing;
            NameTheControls(_controls.Parameters);
            SectionParametersHost.Content = _controls.Parameters;
            SectionPreviewHost.Content = _controls.Preview;
            SectionInformationHost.Content = _controls.Information;
            _controls.Activate();
        }

        // The classic window names nothing; in the page every control is named by its label, so its names are unique in the window.
        private static void NameTheControls(Panel parameters)
        {
            string label = null;
            foreach (var child in parameters.Children)
            {
                if (child is TextBlock text)
                {
                    label = text.Text;
                    continue;
                }

                if (child is CheckBox check)
                {
                    AutomationProperties.SetName(check, (check.Content as string) + NameSuffix);
                }
                else if (child is Control control && label != null)
                {
                    AutomationProperties.SetName(control, label + NameSuffix);
                }
            }
        }

        private void OnControlsRefreshing() => _viewModel.SelectionMayHaveChanged();

        private void OnLayoutSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            PlacePreview(e.NewSize.Width < PreviewBesideWidth);
        }

        private void PlacePreview(bool below)
        {
            if (PreviewBelow == below) return;
            PreviewBelow = below;
            if (below)
            {
                Grid.SetColumnSpan(SectionParametersPanel, 2);
                Grid.SetRow(SectionPreviewPanel, 1);
                Grid.SetColumn(SectionPreviewPanel, 0);
                Grid.SetColumnSpan(SectionPreviewPanel, 2);
                SectionPreviewPanel.Margin = new Thickness(0, 10, 0, 0);
                SectionPreviewRow.Height = GridLength.Auto;
                SectionParametersColumn.Width = new GridLength(1, GridUnitType.Star);
                SectionPreviewColumn.Width = new GridLength(0);
            }
            else
            {
                Grid.SetColumnSpan(SectionParametersPanel, 1);
                Grid.SetRow(SectionPreviewPanel, 0);
                Grid.SetColumn(SectionPreviewPanel, 1);
                Grid.SetColumnSpan(SectionPreviewPanel, 1);
                SectionPreviewPanel.Margin = new Thickness(10, 0, 0, 0);
                SectionPreviewRow.Height = new GridLength(0);
                SectionParametersColumn.Width = new GridLength(300);
                SectionPreviewColumn.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        private void OnHomeClick(object sender, RoutedEventArgs e) => _viewModel.Contained(_viewModel.GoHome);

        private void OnInsertClick(object sender, RoutedEventArgs e) => _viewModel.Contained(_viewModel.Insert);
    }
}
