using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The view of the editor of the cama of the RackCad window (I-64, quinta tanda, part cm). It only passes the gestures of the user to the
    /// view-model: a field is committed with Enter or when it loses the focus; «Restaurar layout…» and «Cerrar el editor» with changes ask
    /// inline. The fields of the view-model keep their items, so a commit never replaces the box that has the focus. Every failure is
    /// contained and said in the message of the editor.
    /// </summary>
    public partial class CamaEditorView : UserControl
    {
        private readonly CamaEditorViewModel _viewModel;

        public CamaEditorView(CamaEditorViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
        }

        public CamaEditorViewModel ViewModel => _viewModel;

        private void OnFieldKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            Commit(sender);
        }

        private void OnFieldLostFocus(object sender, RoutedEventArgs e) => Commit(sender);

        private void Commit(object sender) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is CamaFieldItem field) field.Commit();
        });

        // What is being written goes to the draft first: the name (bound on leaving its box) and the field that has the focus.
        private void CommitPending()
        {
            NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            if (Keyboard.FocusedElement is TextBox { DataContext: CamaFieldItem field }) field.Commit();
        }

        private void OnPrimaryClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitPending();
            _viewModel.RequestWrite();
        });

        private void OnDiscardClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Discard);

        private void OnRestoreClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitPending();
            _viewModel.OpenRestore();
        });

        private void OnRestoreYesClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ConfirmRestore);

        private void OnRestoreNoClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelRestore);

        private void OnCloseEditorClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitPending();
            _viewModel.RequestClose();
        });

        private void OnCloseYesClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ConfirmClose);

        private void OnCloseNoClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelClose);

        private void OnMoreClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ToggleMore);

        private void OnSaveToLibraryClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitPending();
            _viewModel.SaveToLibrary();
        });

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
