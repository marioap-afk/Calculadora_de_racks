using System.Windows;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 pp: the row «Todos los postes» of «Avanzado» › «Postes y cabeceras». «Aplicar a todos» (or Enter in its box) passes the gesture to
    /// the view-model (<see cref="SelectiveEditorViewModel.ApplyPeralteToAllPosts"/>); leaving the box writes nothing.
    /// </summary>
    public partial class SelectiveEditorView
    {
        private void OnApplyAllPostsClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitFocusedText();
            _viewModel.ApplyPeralteToAllPosts();
        });

        private void OnAllPostsKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            Contained(_viewModel.ApplyPeralteToAllPosts);
        }
    }
}
