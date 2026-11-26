using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of the panel «Insertar vista…» (<see cref="RackViewInsertPanelViewModel"/>; I-64 A-14, line D, fourth batch, part «ins»),
    /// the same for every editor of the window (DG-07). It only passes the gestures to its view-model; the editor that hosts it learns of
    /// «Insertar N vistas» through <see cref="Inserting"/> first (for example, to take to the draft the name just typed, as «Actualizar»).
    /// Below <see cref="StackWidth"/> the order goes under the views. Every failure is contained and said in the panel.
    /// </summary>
    public partial class RackViewInsertPanelView : UserControl
    {
        /// <summary>Below this width the two columns do not fit side by side: «Orden de colocación» goes under «Vistas de este rack».</summary>
        internal const double StackWidth = 560.0;

        public RackViewInsertPanelView()
        {
            InitializeComponent();
            SizeChanged += (_, e) =>
            {
                if (e.WidthChanged) Place(e.NewSize.Width < StackWidth);
            };
        }

        /// <summary>«Insertar N vistas» is about to hand the views to the editor.</summary>
        public event EventHandler Inserting;

        /// <summary>Whether «Orden de colocación» is under the views (a narrow page).</summary>
        internal bool OrderBelow { get; private set; }

        private RackViewInsertPanelViewModel ViewModel => DataContext as RackViewInsertPanelViewModel;

        internal void Place(bool below)
        {
            OrderBelow = below;
            Grid.SetColumn(OrderColumn, below ? 0 : 1);
            Grid.SetRow(OrderColumn, below ? 1 : 0);
            Grid.SetColumnSpan(ViewsColumn, below ? 2 : 1);
            Grid.SetColumnSpan(OrderColumn, below ? 2 : 1);
            OrderColumnDefinition.Width = below ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            ViewsColumn.Margin = below ? new Thickness(0) : new Thickness(0, 0, 16, 0);
        }

        private void OnMarkAllCutsClick(object sender, RoutedEventArgs e) => Contained(vm => vm.MarkAllCuts());

        private void OnUnmarkAllCutsClick(object sender, RoutedEventArgs e) => Contained(vm => vm.UnmarkAllCuts());

        private void OnMoveUpClick(object sender, RoutedEventArgs e) => OnRow(sender, (vm, index) => vm.MoveUp(index));

        private void OnMoveDownClick(object sender, RoutedEventArgs e) => OnRow(sender, (vm, index) => vm.MoveDown(index));

        private void OnRemoveClick(object sender, RoutedEventArgs e) => OnRow(sender, (vm, index) => vm.Remove(index));

        private void OnInsertClick(object sender, RoutedEventArgs e) => Contained(vm =>
        {
            Inserting?.Invoke(this, EventArgs.Empty);
            vm.Insert();
        });

        private void OnCancelClick(object sender, RoutedEventArgs e) => Contained(vm => vm.Close());

        // A button of a row of the order acts on the row of its view (its index now, not when the row was made).
        private void OnRow(object sender, Action<RackViewInsertPanelViewModel, int> act) => Contained(vm =>
        {
            if (!((sender as FrameworkElement)?.DataContext is RackViewInsertOrderItem row)) return;
            var index = vm.IndexOf(row.Address);
            if (index >= 0) act(vm, index);
        });

        private void Contained(Action<RackViewInsertPanelViewModel> action)
        {
            var vm = ViewModel;
            if (vm == null) return;
            try
            {
                vm.SayFailure(string.Empty);
                action(vm);
            }
            catch (Exception ex)
            {
                vm.SayFailure("El panel tuvo un fallo contenido: " + ex.Message);
            }
        }
    }
}
