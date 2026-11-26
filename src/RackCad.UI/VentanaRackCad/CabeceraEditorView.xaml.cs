using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of the editor of cabeceras of the RackCad window (I-64, fourth batch, part cb). Like the views of Selectivo and Dinámico it
    /// only passes the gestures to the view-model: a field is committed with Enter or when it loses the focus, a click on a row of «Alzado»
    /// selects with the modifiers of the keyboard, and every question is inline. Every failure is contained and said in the message.
    /// </summary>
    public partial class CabeceraEditorView : UserControl
    {
        private readonly CabeceraEditorViewModel _viewModel;

        public CabeceraEditorView(CabeceraEditorViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
        }

        public CabeceraEditorViewModel ViewModel => _viewModel;

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

        // The text of the field with the focus (and the name) goes to the draft before a button acts, as «Actualizar» of Selectivo.
        private void CommitFocusedText()
        {
            if (Keyboard.FocusedElement is TextBox { DataContext: DynamicFieldItem field }) field.Commit();
            if (ReferenceEquals(Keyboard.FocusedElement, NameBox)) NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private void OnRowClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is not CabeceraElevationRow row) return;
            var modifiers = Keyboard.Modifiers;
            _viewModel.ClickRow(row.IsPanel, row.Index, modifiers.HasFlag(ModifierKeys.Control), modifiers.HasFlag(ModifierKeys.Shift));
        });

        private void OnUpdateClick(object sender, RoutedEventArgs e) => Contained(() => { CommitFocusedText(); _viewModel.RequestWrite(); });

        private void OnDiscardClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Discard);

        private void OnRestoreClick(object sender, RoutedEventArgs e) => Contained(() => { CommitFocusedText(); _viewModel.OpenRestore(); });

        private void OnRestoreConfirmClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ConfirmRestore);

        private void OnRestoreCancelClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelRestore);

        private void OnCloseEditorClick(object sender, RoutedEventArgs e) => Contained(() => { CommitFocusedText(); _viewModel.RequestClose(); });

        private void OnCloseYesClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmClose(true));

        private void OnCloseNoClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmClose(false));

        private void OnBackClick(object sender, RoutedEventArgs e) => Contained(() => { CommitFocusedText(); _viewModel.Back(); });

        private void OnBackApplyClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ApplyAndBack);

        private void OnBackDropClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmBack(true));

        private void OnBackNoClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelBack);

        private void OnApplyClick(object sender, RoutedEventArgs e) => Contained(() => { CommitFocusedText(); _viewModel.Apply(); });

        private void OnHeightYesClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmHeight(true));

        private void OnHeightNoClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmHeight(false));

        private void OnRegenYesClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmRegen(true));

        private void OnRegenNoClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ConfirmRegen(false));

        private void OnCopyClick(object sender, RoutedEventArgs e) => Contained(() => { CommitFocusedText(); _viewModel.OpenCopy(); });

        private void OnCopyAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.CopyAll(true));

        private void OnCopyNoneClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.CopyAll(false));

        private void OnCopyConfirmClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ConfirmCopy);

        private void OnCopyCancelClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelCopy);

        private void OnSelectPanelsClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.SelectKind(true, false));

        private void OnSelectHorizontalsClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.SelectKind(false, true));

        private void OnSelectAllClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.SelectKind(true, true));

        private void OnAddPanelClick(object sender, RoutedEventArgs e) => Contained(_viewModel.AddPanel);

        private void OnSplitClick(object sender, RoutedEventArgs e) => Contained(_viewModel.SplitPanel);

        private void OnCombineClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CombinePanel);

        private void OnPanelsStandardClick(object sender, RoutedEventArgs e) => Contained(_viewModel.PanelsToStandard);

        private void OnDuplicateClick(object sender, RoutedEventArgs e) => Contained(_viewModel.DuplicateHorizontal);

        private void OnDeleteClick(object sender, RoutedEventArgs e) => Contained(_viewModel.DeleteHorizontals);

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _viewModel.Fail(ex);
            }
        }
    }
}
