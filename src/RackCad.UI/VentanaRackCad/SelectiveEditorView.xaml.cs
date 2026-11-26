using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of the Selectivo editor of the RackCad window (I-64 A-14, line D, Selectivo E1-B). Like the view of the Dinámico editor it
    /// only passes the gestures of the user to the view-model: a field is committed with Enter or when it loses the focus, a click on the
    /// matrix selects with the modifiers of the keyboard, and «Cerrar el editor» with changes asks inline first. The lists of the view-model
    /// keep their items, so a commit never replaces the box that has the focus or the button under the mouse. Every failure is contained
    /// and said in the message of the editor.
    /// </summary>
    public partial class SelectiveEditorView : UserControl
    {
        private readonly SelectiveEditorViewModel _viewModel;

        public SelectiveEditorView(SelectiveEditorViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
        }

        public SelectiveEditorViewModel ViewModel => _viewModel;

        /// <summary>Below this width of the Matriz tab the inspector does not fit beside the matrix and goes below it (as in Dinámico).</summary>
        internal const double InspectorBesideWidth = DynamicEditorView.InspectorBesideWidth;

        /// <summary>Whether the inspector is below the matrix (a narrow window).</summary>
        internal bool InspectorBelow { get; private set; }

        private void OnMatrixTabSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            PlaceInspector(e.NewSize.Width < InspectorBesideWidth);
        }

        private void PlaceInspector(bool below)
        {
            if (InspectorBelow == below) return;
            InspectorBelow = below;
            if (below)
            {
                Grid.SetRow(InspectorPanel, 1);
                Grid.SetColumn(InspectorPanel, 0);
                Grid.SetColumnSpan(InspectorPanel, 2);
                InspectorPanel.Width = double.NaN;
                InspectorPanel.Margin = new Thickness(0, 10, 0, 0);
                InspectorRow.Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                Grid.SetRow(InspectorPanel, 0);
                Grid.SetColumn(InspectorPanel, 1);
                Grid.SetColumnSpan(InspectorPanel, 1);
                InspectorPanel.Width = 420;
                InspectorPanel.Margin = new Thickness(10, 0, 0, 0);
                InspectorRow.Height = new GridLength(0);
            }
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

        private void OnCellClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is not SelectiveCellItem cell) return;
            var modifiers = Keyboard.Modifiers;
            _viewModel.ClickCell(cell.FrontIndex, cell.LevelIndex, modifiers.HasFlag(ModifierKeys.Control), modifiers.HasFlag(ModifierKeys.Shift));
        });

        private void OnColumnClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is SelectiveColumnHead column) _viewModel.SelectFront(column.FrontIndex);
        });

        private void OnRowClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is SelectiveMatrixRow row) _viewModel.SelectLevel(row.LevelIndex);
        });

        private void OnSelectAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.SelectAll());

        // A button of «Todos los fondos · Fondo 1 · Fondo n» over the matrix (the approved draft; no drop-down list, O-D of the smoke).
        private void OnFondoViewClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is SelectiveSegmentOption option) _viewModel.FondoScopeIndex = option.Value;
        });

        // A button of «Celda · Frente · Nivel · Todo» of the inspector (the approved draft).
        private void OnScopeClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is SelectiveSegmentOption option) _viewModel.SetScope((SelectiveEditScope)option.Value);
        });

        private void OnStartSegmentsClick(object sender, RoutedEventArgs e) => Contained(_viewModel.StartSegments);

        private void OnAddSegmentClick(object sender, RoutedEventArgs e) => Contained(_viewModel.AddSegment);

        private void OnApplySegmentsClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ApplySegments);

        private void OnClearSegmentsClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ClearSegments);

        private void OnDiscardSegmentsClick(object sender, RoutedEventArgs e) => Contained(_viewModel.DiscardSegments);

        private void OnRemoveSegmentClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is SelectiveSegmentRow row) _viewModel.RemoveSegment(row.Index);
        });

        // E2 seguridad: «En todas las celdas» / «En ninguna» of a card of «Extremos y seguridad», and «Editar seguridad…» (D-09).
        private void OnFamilyAllClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is SelectiveSafetyFamilyCard card) _viewModel.SetFamilyEverywhere(card.Type, true);
        });

        private void OnFamilyNoneClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is SelectiveSafetyFamilyCard card) _viewModel.SetFamilyEverywhere(card.Type, false);
        });

        private void OnEditSafetyClick(object sender, RoutedEventArgs e) => Contained(_viewModel.EditSafety);

        private void OnUpdateClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            // The name is bound on leaving its box: a click on «Actualizar» right after typing it must not lose it.
            NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            _viewModel.RequestWrite();
        });

        private void OnDiscardClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Discard());

        private void OnCloseEditorClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            _viewModel.RequestClose();
        });

        private void OnCloseYesClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmClose());

        private void OnCloseNoClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.CancelClose());

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _viewModel.Say("El editor tuvo un fallo contenido: " + ex.Message, error: true);
            }
        }
    }
}
