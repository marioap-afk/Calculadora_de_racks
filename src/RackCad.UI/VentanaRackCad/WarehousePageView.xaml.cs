using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 al: the view of the page «Almacén». Its data context is <see cref="WarehousePageViewModel"/>; only the gestures of the user reach it
    /// (a click, a value typed, a rack or a layer chosen), and every failure is contained and said in the message of the page. The two lists
    /// are kept in step with the page by the code (as the table of the BOM page): a list that changes is never a choice of the user.
    /// </summary>
    public partial class WarehousePageView : UserControl
    {
        /// <summary>Below this width the values and the result do not fit side by side: the result goes under the values.</summary>
        internal const double NarrowWidth = 760;

        private readonly WarehousePageViewModel _viewModel;
        private bool _syncing;

        public WarehousePageView(WarehousePageViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
            SizeChanged += OnSizeChanged;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            // Correction 6 of the review: a value typed is remembered when its box loses the focus, not on every key.
            AddHandler(Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnFieldLostFocus), handledEventsToo: true);
            SyncChoices();
        }

        private void OnFieldLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OriginalSource is TextBox) Contained(_viewModel.CommitValues);
        }

        public WarehousePageViewModel ViewModel => _viewModel;

        /// <summary>The result is under the values (a narrow page).</summary>
        internal bool ResultBelow => Grid.GetRow(ResultBorder) == 1;

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName)
                || e.PropertyName == nameof(WarehousePageViewModel.SeedOptions) || e.PropertyName == nameof(WarehousePageViewModel.SelectedSeedId)
                || e.PropertyName == nameof(WarehousePageViewModel.LayerOptions) || e.PropertyName == nameof(WarehousePageViewModel.SelectedLayer))
                SyncChoices();
        }

        // The rack and the layer the page has chosen, shown again after its lists changed.
        private void SyncChoices()
        {
            _syncing = true;
            try
            {
                if (!ReferenceEquals(SeedCombo.ItemsSource, _viewModel.SeedOptions)) SeedCombo.ItemsSource = _viewModel.SeedOptions;
                SeedCombo.SelectedValue = _viewModel.SelectedSeedId;
                if (!ReferenceEquals(LayerCombo.ItemsSource, _viewModel.LayerOptions)) LayerCombo.ItemsSource = _viewModel.LayerOptions;
                LayerCombo.SelectedItem = _viewModel.SelectedLayer;
            }
            finally
            {
                _syncing = false;
            }
        }

        private void OnSeedSelectionChanged(object sender, SelectionChangedEventArgs e) => Contained(() =>
        {
            if (_syncing) return;
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is WarehouseSeedOption option) _viewModel.ChooseSeed(option.RackId);
        });

        private void OnLayerSelectionChanged(object sender, SelectionChangedEventArgs e) => Contained(() =>
        {
            if (_syncing) return;
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is string layer) _viewModel.ChooseLayer(layer);
        });

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            var below = e.NewSize.Width < NarrowWidth;
            if (below == ResultBelow) return;
            Grid.SetRow(ResultBorder, below ? 1 : 0);
            Grid.SetColumn(ResultBorder, below ? 0 : 1);
            Grid.SetColumnSpan(ResultBorder, below ? 2 : 1);
            Grid.SetColumnSpan(FormPanel, below ? 2 : 1);
            FormColumn.Width = below ? new GridLength(1, GridUnitType.Star) : new GridLength(400);
            ResultBorder.Margin = below ? new Thickness(0, 12, 0, 0) : new Thickness(16, 0, 0, 0);
            ResultBorder.Padding = below ? new Thickness(0, 8, 0, 0) : new Thickness(16, 0, 0, 0);
            ResultBorder.BorderThickness = below ? new Thickness(0, 1, 0, 0) : new Thickness(1, 0, 0, 0);
        }

        private void OnPlaceClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Place);

        private void OnPlaceAnywayClick(object sender, RoutedEventArgs e) => Contained(_viewModel.PlaceAnyway);

        private void OnKeepAdjustingClick(object sender, RoutedEventArgs e) => Contained(_viewModel.KeepAdjusting);

        private void OnReadClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Read);

        private void OnResetClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Reset);

        private void OnStopWaitingClick(object sender, RoutedEventArgs e) => Contained(_viewModel.StopWaiting);

        private void OnDismissMessageClick(object sender, RoutedEventArgs e) => Contained(_viewModel.DismissMessage);

        private void OnGridTabClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ShowGrid);

        private void OnFillTabClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ShowFill);

        private void OnLinkedClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ChooseLinked);

        private void OnIndependentClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ChooseIndependent);

        private void OnFitToggleClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ToggleFit);

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _viewModel.ShowError(ex);
            }
        }
    }
}
