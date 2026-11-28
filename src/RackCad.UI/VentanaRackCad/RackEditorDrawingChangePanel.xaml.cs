using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 st (O-5, M-16): «El dibujo cambió desde que se leyó» under the head of an editor of the window, with «Volver a leer del dibujo» and
    /// the inline question of «Actualizar». Its data context is the <see cref="RackEditorDrawingChange"/> of the editor.
    /// </summary>
    public partial class RackEditorDrawingChangePanel : UserControl
    {
        public RackEditorDrawingChangePanel()
        {
            InitializeComponent();
        }

        private void OnRereadClick(object sender, RoutedEventArgs e) => Contained(change => change.Reread());

        private void OnOverwriteClick(object sender, RoutedEventArgs e) => Contained(change => change.ConfirmUpdate());

        private void OnCancelOverwriteClick(object sender, RoutedEventArgs e) => Contained(change => change.CancelUpdate());

        // A failure is contained here: the editor and the window keep working (as every handler of the views).
        private void Contained(Action<RackEditorDrawingChange> action)
        {
            if (DataContext is not RackEditorDrawingChange change) return;
            try
            {
                action(change);
            }
            catch (Exception ex)
            {
                // The notice stays as it is; the failure goes to the log of RackCad.
                RackCad.Application.Diagnostics.RackLog.Exception("RackEditorDrawingChangePanel", ex);
            }
        }
    }
}
