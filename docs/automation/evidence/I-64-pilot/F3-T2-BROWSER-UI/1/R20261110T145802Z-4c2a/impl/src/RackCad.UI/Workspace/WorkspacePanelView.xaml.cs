using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// Root control of the workspace panel (I-64 F1-T3B): the one visual the palette host adds, labelled "Workspace".
    /// Its data context is the view-model. The control never asks the host for anything by itself: it has no timer and does
    /// not react to being shown or resized. Only explicit user interactions reach the view-model (the retry, the
    /// instrumentation switch and, since I-64 F2-T2B, the clicks of the navigation actions), and all are contained here as
    /// well: a failure becomes the Error state of the panel and never reaches the message loop of the host. It installs no
    /// global exception handler.
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

        // The navigation clicks (I-64 F2-T2B): each one is one explicit interaction. The view-model's entry contains every
        // failure and presents it, so the task it returns never faults and needs no observer here.
        private void OnNavigationSelectClick(object sender, RoutedEventArgs e) => Contained(() => _ = _viewModel.Navigation.SelectAsync());

        private void OnNavigationViewClick(object sender, RoutedEventArgs e) => Contained(() => _ = _viewModel.Navigation.ViewAsync());

        private void OnNavigationCenterClick(object sender, RoutedEventArgs e) => Contained(() => _ = _viewModel.Navigation.CenterAsync());

        private void OnNavigationLocateClick(object sender, RoutedEventArgs e) => Contained(() => _ = _viewModel.Navigation.LocateAsync());

        private void OnNavigationNextCopyClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Navigation.NextCopy());

        // The tab clicks (I-64 F3-T2): each one is one explicit interaction that only changes memory. The tab is the data context of its
        // own button, so the handler needs no lookup in the visual tree.
        private void OnBrowserTabActivateClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.ActivateTab(RackOf(sender)));

        private void OnBrowserTabPinClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.PinTab(RackOf(sender)));

        private void OnBrowserTabCloseClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.CloseTab(RackOf(sender)));

        private static string RackOf(object sender) => ((sender as FrameworkElement)?.DataContext as WorkspaceBrowserTabItem)?.RackId;

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
