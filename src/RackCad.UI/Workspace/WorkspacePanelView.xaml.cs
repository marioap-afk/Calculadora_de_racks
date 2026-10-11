using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// Root control of the workspace panel (I-64 F1-T3B): the one visual the palette host adds, labelled "Workspace".
    /// Its data context is the view-model. The control never asks the host for anything by itself: it has no timer and does
    /// not react to being shown or resized. Only two explicit user interactions reach the view-model (the retry and the
    /// instrumentation switch), and both are contained here as well: a failure becomes the Error state of the panel and
    /// never reaches the message loop of the host. It installs no global exception handler.
    /// </summary>
    public partial class WorkspacePanelView : UserControl
    {
        /// <summary>The label of the root of the panel.</summary>
        public const string RootLabel = "Workspace";

        private readonly WorkspacePanelViewModel _viewModel;

        public WorkspacePanelView(WorkspacePanelViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;

            // The error banner follows the view-model directly, so it never depends on when the binding engine runs.
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            ShowErrorBanner(_viewModel.HasError);
        }

        public WorkspacePanelViewModel ViewModel => _viewModel;

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkspacePanelViewModel.HasError)) ShowErrorBanner(_viewModel.HasError);
        }

        private void ShowErrorBanner(bool visible)
        {
            var visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            ErrorPanel.Visibility = visibility;
            RetryButton.Visibility = visibility;
        }

        private void OnRetryClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Retry());

        private void OnInstrumentationClick(object sender, RoutedEventArgs e) =>
            Contained(() => _viewModel.SetInstrumentation(((CheckBox)sender).IsChecked == true));

        private void Contained(Action interaction)
        {
            try
            {
                interaction();
            }
            catch (System.Exception ex)
            {
                _viewModel.ShowError(ex);
            }
        }
    }
}
