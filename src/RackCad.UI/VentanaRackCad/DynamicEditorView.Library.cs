using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>«Más» and «Guardar en biblioteca…» of the Dinámico editor (part «ut», UT-04): the name and the field being written go to the draft first.</summary>
    public partial class DynamicEditorView
    {
        private void OnMoreClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ToggleMore);

        private void OnSaveToLibraryClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            if (Keyboard.FocusedElement is TextBox { DataContext: DynamicFieldItem field }) field.Commit();
            _viewModel.SaveToLibrary();
        });
    }
}
