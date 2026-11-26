using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of the page «BOM y lista de racks» (T4 bom). Its data context is <see cref="RackBomPageViewModel"/>; only the clicks of the user
    /// reach it, and every failure is contained and said in the message of the page.
    /// </summary>
    public partial class RackBomPageView : UserControl
    {
        private readonly RackBomPageViewModel _viewModel;

        public RackBomPageView(RackBomPageViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
            SizeChanged += OnSizeChanged;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            // When the list of rows is replaced the ListBox clears its selection after the change: the rack chosen is put back then.
            ((System.Collections.Specialized.INotifyCollectionChanged)RackTable.Items).CollectionChanged += (_, _) => SyncSelection();
            SyncSelection();
        }

        private bool _syncing;

        // The rows are a new list after every filter, order or reading: the table shows again the rack the page has chosen. A selection
        // the list clears while it changes is not a choice of the user.
        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RackBomPageViewModel.Rows) || e.PropertyName == nameof(RackBomPageViewModel.SelectedRackId)) SyncSelection();
        }

        private void SyncSelection()
        {
            _syncing = true;
            try
            {
                RackTable.SelectedValue = _viewModel.SelectedRackId;
            }
            finally
            {
                _syncing = false;
            }
        }

        private void OnRackSelectionChanged(object sender, SelectionChangedEventArgs e) => Contained(() =>
        {
            if (_syncing) return;
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is RackBomRowItem row) _viewModel.Select(row.RackId);
        });

        public RackBomPageViewModel ViewModel => _viewModel;

        /// <summary>Below this width the table and the BOM of the rack do not fit side by side: the BOM goes under the table.</summary>
        internal const double NarrowWidth = 760;

        /// <summary>The BOM of the rack is under the table (a narrow page).</summary>
        internal bool InspectorBelow => Grid.GetRow(InspectorBorder) == 1;

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            var below = e.NewSize.Width < NarrowWidth;
            if (below == InspectorBelow) return;
            Grid.SetRow(InspectorBorder, below ? 1 : 0);
            Grid.SetColumn(InspectorBorder, below ? 0 : 1);
            Grid.SetColumnSpan(InspectorBorder, below ? 2 : 1);
            InspectorBorder.Width = below ? double.NaN : 380;
            InspectorBorder.Padding = below ? new Thickness(0, 10, 0, 0) : new Thickness(12, 0, 0, 0);
            InspectorBorder.Margin = below ? new Thickness(0, 10, 0, 0) : new Thickness(0);
            InspectorBorder.BorderThickness = below ? new Thickness(0, 1, 0, 0) : new Thickness(1, 0, 0, 0);
            TablePanel.Margin = below ? new Thickness(0) : new Thickness(0, 0, 12, 0);
            Grid.SetColumnSpan(TablePanel, below ? 2 : 1);
        }

        private void OnRecalculateClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Recalculate);

        private void OnExportDrawingExcelClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ExportDrawing(excel: true));

        private void OnExportDrawingCsvClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ExportDrawing(excel: false));

        private void OnStopWaitingClick(object sender, RoutedEventArgs e) => Contained(_viewModel.StopWaiting);

        private void OnOpenNoticeClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackBomNoticeItem notice) _viewModel.OpenRack(notice.RackId);
        });

        private void OnAskRecalculateClick(object sender, RoutedEventArgs e) => Contained(_viewModel.AskRecalculateAndExport);

        private void OnAskExportClick(object sender, RoutedEventArgs e) => Contained(_viewModel.AskExportAnyway);

        private void OnAskCancelClick(object sender, RoutedEventArgs e) => Contained(_viewModel.AskCancel);

        private void OnOpenExportedClick(object sender, RoutedEventArgs e) => Contained(_viewModel.OpenExported);

        private void OnShowExportedClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ShowExportedInFolder);

        private void OnSaveExportedAsClick(object sender, RoutedEventArgs e) => Contained(_viewModel.SaveExportedAs);

        private void OnCloseExportedClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CloseExported);

        private void OnShowFirstDraftClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ShowFirstDraft);

        private void OnDismissMessageClick(object sender, RoutedEventArgs e) => Contained(_viewModel.DismissMessage);

        private void OnRacksTabClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ShowRacksTab);

        private void OnTotalTabClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ShowTotalTab);

        // The headers of the table name their column in their Tag.
        private void OnSortClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.Tag is string key) _viewModel.SortBy(key);
        });

        private void OnOpenSelectedClick(object sender, RoutedEventArgs e) => Contained(_viewModel.OpenSelected);

        private void OnViewSelectedClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ViewSelected);

        private void OnNextCopyClick(object sender, RoutedEventArgs e) => Contained(_viewModel.NextCopySelected);

        private void OnSourceDrawingClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ShowDrawingBom);

        private void OnSourceDraftClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ShowDraftBom);

        private void OnExpandAllClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ExpandAll);

        private void OnCollapseAllClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CollapseAll);

        private void OnExportRackExcelClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ExportRack(excel: true));

        private void OnExportRackCsvClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ExportRack(excel: false));

        private void OnExpandAllTotalClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ExpandAllTotal);

        private void OnCollapseAllTotalClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CollapseAllTotal);

        private void Contained(Action interaction)
        {
            try
            {
                interaction();
            }
            catch (Exception ex)
            {
                _viewModel.Say("La página tuvo un fallo contenido: " + (string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message), true);
            }
        }
    }
}
