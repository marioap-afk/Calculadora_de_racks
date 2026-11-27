namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 st (O-5, M-16; the Owner: «Avisar y ofrecer releer»): «El dibujo cambió» of the Dinámico editor (<see cref="RackEditorDrawingChange"/>).
    /// The window marks it after a command that is not of the window; «Actualizar» of a draft with changes asks first; a write that AutoCAD
    /// did clears it.
    /// </summary>
    public sealed partial class DynamicEditorViewModel
    {
        private RackEditorDrawingChange _drawingChange;

        /// <summary>The notice «El dibujo cambió desde que se leyó», «Volver a leer del dibujo» and the question of «Actualizar».</summary>
        public RackEditorDrawingChange DrawingChange => _drawingChange ??= new RackEditorDrawingChange(() => _dirty, () => _writing, WriteOverTheDrawing, RaiseState);

        // «Escribir encima» of the question: the write of «Actualizar», with its checks again.
        private void WriteOverTheDrawing()
        {
            if (!CanUpdate)
            {
                Say(UpdateBlockedText, error: true);
                return;
            }

            _update(this);
        }
    }
}
