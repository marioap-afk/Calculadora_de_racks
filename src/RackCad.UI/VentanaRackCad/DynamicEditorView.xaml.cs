using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of the Dinámico editor of the RackCad window (I-64 A-14, line D). It only passes the gestures of the user to the view-model:
    /// a field is committed with Enter or when it loses the focus, a click on the matrix selects with the modifiers of the keyboard, and
    /// «Restaurar layout» and «Cerrar el editor» with changes ask inline first. The lists of the view-model keep their items, so a commit
    /// never replaces the box that has the focus or the button under the mouse. Every failure is contained and said in the message of the
    /// editor.
    /// </summary>
    public partial class DynamicEditorView : UserControl
    {
        private readonly DynamicEditorViewModel _viewModel;

        public DynamicEditorView(DynamicEditorViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
        }

        public DynamicEditorViewModel ViewModel => _viewModel;

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
            if ((sender as FrameworkElement)?.DataContext is not DynamicCellItem cell) return;
            var modifiers = Keyboard.Modifiers;
            _viewModel.ClickCell(cell.FrontIndex, cell.LevelIndex, modifiers.HasFlag(ModifierKeys.Control), modifiers.HasFlag(ModifierKeys.Shift));
        });

        private void OnColumnClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is DynamicColumnHead column) _viewModel.SelectFront(column.FrontIndex);
        });

        private void OnRowClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is DynamicMatrixRow row) _viewModel.SelectLevel(row.LevelIndex);
        });

        private void OnSelectAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.SelectAll());

        private void OnBlankFrontClick(object sender, RoutedEventArgs e) =>
            Contained(() => _viewModel.SetFrontBlank(_viewModel.Draft.Matrix.SelectedFrontIndex, true));

        private void OnActivateFrontClick(object sender, RoutedEventArgs e) =>
            Contained(() => _viewModel.SetFrontBlank(_viewModel.Draft.Matrix.SelectedFrontIndex, false));

        private void OnUpdateClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            // The name is bound on leaving its box: a click on «Actualizar» right after typing it must not lose it.
            NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            _viewModel.RequestWrite();
        });

        private void OnDiscardClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.Discard());

        private void OnRestoreClick(object sender, RoutedEventArgs e) => RestoreConfirmPanel.Visibility = Visibility.Visible;

        private void OnRestoreYesClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            RestoreConfirmPanel.Visibility = Visibility.Collapsed;
            _viewModel.RestoreStandard();
        });

        private void OnRestoreNoClick(object sender, RoutedEventArgs e) => RestoreConfirmPanel.Visibility = Visibility.Collapsed;

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
