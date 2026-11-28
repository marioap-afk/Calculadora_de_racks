using System.Windows;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>«Más» and «Guardar en biblioteca…» of the Selectivo editor (part «ut», UT-04): what was being written goes to the draft first.</summary>
    public partial class SelectiveEditorView
    {
        private void OnMoreClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ToggleMore);

        private void OnSaveToLibraryClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitFocusedText();
            _viewModel.SaveToLibrary();
        });
    }
}
