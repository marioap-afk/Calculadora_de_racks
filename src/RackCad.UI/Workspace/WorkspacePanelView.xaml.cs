using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        // The reload of the surface (I-64 F3-T2, T2-F4): one explicit interaction that asks again for the summary of the active tab.
        private void OnBrowserSurfaceReloadClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.ReloadSummary());

        // The double click on the button of a tab (I-64 F4-T3B1): a gesture of the field only when, after the clicks, that tab is the active one.
        private void OnBrowserTabActivateDoubleClick(object sender, MouseButtonEventArgs e) =>
            Contained(() => _viewModel.Browser.BeginFieldEditFromTab(RackOf(sender)));

        // The field of the pilot (I-64 F4-T3B1). The text typed by the user reaches the view-model only through the setter of the bound
        // property; here live the buttons, the gestures that open a read-only box and the keys Enter and Esc of an editable one.
        private void OnBrowserFieldEditClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.BeginFieldEdit());

        private void OnBrowserFieldReloadClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.ReloadField());

        // A click on a read-only box is the first gesture only while the field offers the Edit button; the click itself is not consumed.
        private void OnBrowserFieldBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.Browser.Field.IsEditable) return;
            Contained(() => _viewModel.Browser.BeginFieldEditFromBox());
        }

        private void OnBrowserFieldBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var field = _viewModel.Browser.Field;
            if (field.IsEditable)
            {
                // Enter commits and Esc does nothing: both stay in the panel. Esc never reverts the text. Every other key goes to the
                // editor untouched (no handler of the bubble event marks it), so a character key produces its text input.
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    Contained(() => field.CommitPending());
                }
                else if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                }

                return;
            }

            // A read-only box: a key that is not a modifier, Tab or Esc is consumed, so it is not typed into a box that may be editable a
            // moment later and does not leave the panel. It is the first gesture only while the field offers the Edit button and never
            // when it is a repetition of a held key. Esc is consumed with no gesture.
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                return;
            }

            if (IsModifierOrNavigation(e.Key)) return;

            e.Handled = true;
            if (e.IsRepeat) return;
            Contained(() => _viewModel.Browser.BeginFieldEditFromBox());
        }

        // Leaving the box commits the pending text; the view-model does nothing without pending text or with a box that is not editable.
        private void OnBrowserFieldBoxLostFocus(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Browser.Field.CommitPending());

        private static bool IsModifierOrNavigation(Key key)
        {
            switch (key)
            {
                case Key.Tab:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LWin:
                case Key.RWin:
                case Key.System:
                case Key.None:
                    return true;
                default:
                    return false;
            }
        }

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
