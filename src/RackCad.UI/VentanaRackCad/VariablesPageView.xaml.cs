using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of the page «Variables» of the RackCad window (I-64 A-14, cuarta tanda, va). Like the editors of the window it only passes the
    /// gestures of the user to the view-model: a box of the inspector is committed with Enter or when it loses the focus, a row is chosen
    /// with a click, and what is irreversible asks inline. Below <see cref="InspectorBesideWidth"/> the inspector goes below the list. Every
    /// failure is contained and said in the message of the page.
    /// </summary>
    public partial class VariablesPageView : UserControl
    {
        /// <summary>Below this width of the tab the inspector does not fit beside the list and goes below it.</summary>
        internal const double InspectorBesideWidth = 760;

        private readonly VariablesPageViewModel _viewModel;

        public VariablesPageView(VariablesPageViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
        }

        public VariablesPageViewModel ViewModel => _viewModel;

        /// <summary>Whether the inspector is below the list (a narrow window).</summary>
        internal bool InspectorBelow { get; private set; }

        private void OnVariablesTabSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            PlaceInspector(e.NewSize.Width < InspectorBesideWidth);
        }

        internal void PlaceInspector(bool below)
        {
            if (InspectorBelow == below) return;
            InspectorBelow = below;
            Grid.SetRow(InspectorPanel, below ? 1 : 0);
            Grid.SetColumn(InspectorPanel, below ? 0 : 1);
            Grid.SetColumnSpan(InspectorPanel, below ? 2 : 1);
            InspectorPanel.Width = below ? double.NaN : 340;
            InspectorPanel.Margin = below ? new Thickness(0, 10, 0, 0) : new Thickness(10, 0, 0, 0);
            InspectorRow.Height = below ? GridLength.Auto : new GridLength(0);
        }

        private void OnUpdateClick(object sender, RoutedEventArgs e) => Contained(_viewModel.RequestWrite);

        private void OnDiscardClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Discard);

        private void OnMoreClick(object sender, RoutedEventArgs e)
            => MorePanel.Visibility = MorePanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        private void OnCheckClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Check);

        private void OnReadClick(object sender, RoutedEventArgs e) => Contained(_viewModel.RequestRead);

        private void OnClassicClick(object sender, RoutedEventArgs e) => Contained(_viewModel.OpenClassic);

        private void OnNewVariableClick(object sender, RoutedEventArgs e) => Contained(_viewModel.NewVariable);

        private void OnRowClick(object sender, RoutedEventArgs e)
            => Contained(() => _viewModel.Select((sender as FrameworkElement)?.DataContext as VariableRowItem));

        private void OnRepairAskClick(object sender, RoutedEventArgs e)
            => Contained(() => _viewModel.AskRepair((sender as FrameworkElement)?.DataContext as BrokenRackItem));

        private void OnRepairUndoClick(object sender, RoutedEventArgs e)
            => Contained(() => _viewModel.UndoRepair((sender as FrameworkElement)?.DataContext as BrokenRackItem));

        private void OnRepairYesClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ConfirmRepair);

        private void OnAskNoClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelAsk);

        private void OnConflictGoClick(object sender, RoutedEventArgs e)
            => Contained(() => _viewModel.OpenRack(((sender as FrameworkElement)?.DataContext as VariablesConflictItem)?.RackId));

        private void OnConsumerOpenClick(object sender, RoutedEventArgs e)
            => Contained(() => _viewModel.OpenRack(((sender as FrameworkElement)?.DataContext as VariableConsumerItem)?.RackId));

        private void OnUndoDeleteClick(object sender, RoutedEventArgs e) => Contained(_viewModel.UndoDelete);

        private void OnDeleteClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Delete);

        private void OnUnlinkClick(object sender, RoutedEventArgs e) => Contained(_viewModel.AskUnlink);

        private void OnUnlinkYesClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ConfirmUnlink);

        private void OnNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Contained(_viewModel.CommitInspectorName);
        }

        private void OnNameLostFocus(object sender, RoutedEventArgs e) => Contained(_viewModel.CommitInspectorName);

        private void OnDefinitionKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Contained(_viewModel.CommitInspectorDefinition);
        }

        private void OnDefinitionLostFocus(object sender, RoutedEventArgs e) => Contained(_viewModel.CommitInspectorDefinition);

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _viewModel.Say("La página tuvo un fallo contenido: " + ex.Message, true);
            }
        }
    }
}
