namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 st (O-5, M-16; the Owner: «Avisar y ofrecer releer»): «El dibujo cambió» of the editor of a cabecera of its own
    /// (<see cref="RackEditorDrawingChange"/>). The window marks it after a command that is not of the window; «Actualizar» of a draft with
    /// changes asks first; a write that AutoCAD did clears it. A cabecera inside the editor of its rack (embedded) is never marked: it is a
    /// copy of the draft of that rack, and its editor has the notice.
    /// </summary>
    public sealed partial class CabeceraEditorViewModel
    {
        private RackEditorDrawingChange _drawingChange;

        /// <summary>The notice «El dibujo cambió desde que se leyó», «Volver a leer del dibujo» and the question of «Actualizar».</summary>
        public RackEditorDrawingChange DrawingChange => _drawingChange ??= new RackEditorDrawingChange(() => IsDirty, () => _writing, WriteOverTheDrawing, RaiseState);

        // «Escribir encima» of the question: the write of «Actualizar», with its checks again.
        private void WriteOverTheDrawing()
        {
            if (!CanUpdate)
            {
                Say(UpdateBlockedText, true); // review of 5114345e, M-6: as the other five
                return;
            }

            _update(this);
        }
    }
}
