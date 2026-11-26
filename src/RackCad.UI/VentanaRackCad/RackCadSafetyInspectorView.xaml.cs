using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of the inspector of a family of «Extremos y seguridad» (T4 seg). It passes the gestures to the inspector (a measure is
    /// committed with Enter or when it is left, a button runs its action, the inline question of «Sin variante» is answered) and places itself
    /// in the Grid that holds it: beside the tab when it is wide, below it when it is narrow, as the inspector of «Matriz» (O-3 of the smoke).
    /// The Grid has two columns (the tab, the inspector) and two rows (the second one, for the inspector below).
    /// </summary>
    public partial class RackCadSafetyInspectorView : UserControl
    {
        /// <summary>Below this width of the Grid the inspector goes below the table: the table alone is wider than the matrix.</summary>
        internal const double BesideWidth = 900;

        /// <summary>The width of the inspector beside the tab.</summary>
        internal const double InspectorWidth = 380;

        private Grid _host;

        public RackCadSafetyInspectorView()
        {
            InitializeComponent();
        }

        // The Grid that holds it is known as soon as the inspector is one of its children (no window is needed, as in the tests).
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            Attach();
        }

        /// <summary>Whether the inspector is below the tab (a narrow window).</summary>
        internal bool IsBelow { get; private set; }

        private RackCadSafetyInspector Inspector => DataContext as RackCadSafetyInspector;

        // The Grid that holds it: its width says where the inspector goes.
        private void Attach()
        {
            if (_host != null || Parent is not Grid grid) return;
            _host = grid;
            grid.SizeChanged += (s, e) => { if (e.WidthChanged) Place(e.NewSize.Width < BesideWidth); };
            Place(grid.ActualWidth > 0 && grid.ActualWidth < BesideWidth, force: true);
        }

        /// <summary>Beside (false) or below (true) the tab, in the Grid that holds the inspector.</summary>
        internal void Place(bool below, bool force = false)
        {
            if (_host == null || (!force && IsBelow == below)) return;
            IsBelow = below;
            Grid.SetRow(this, below ? 1 : 0);
            Grid.SetColumn(this, below ? 0 : 1);
            Grid.SetColumnSpan(this, below ? 2 : 1);
            Width = below ? double.NaN : InspectorWidth;
            Margin = below ? new Thickness(10, 0, 10, 10) : new Thickness(0, 10, 10, 10);
            if (_host.RowDefinitions.Count > 1) _host.RowDefinitions[1].Height = below ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        }

        private void OnFieldKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            Commit(sender);
        }

        private void OnFieldLostFocus(object sender, RoutedEventArgs e) => Commit(sender);

        private void Commit(object sender) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is DynamicFieldItem field) field.Commit();
        });

        private void OnActionClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackCadSafetyAction action) action.Run();
        });

        private void OnAskYesClick(object sender, RoutedEventArgs e) => Contained(() => Inspector?.ConfirmRemove());

        private void OnAskNoClick(object sender, RoutedEventArgs e) => Contained(() => Inspector?.CancelRemove());

        // A failure is contained and said in the line of messages of the editor.
        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Inspector?.SayAction?.Invoke("El editor tuvo un fallo contenido: " + ex.Message, true);
            }
        }
    }
}
