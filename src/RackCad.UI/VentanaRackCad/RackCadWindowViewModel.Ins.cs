namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The fourth batch of the window, part «ins» (I-64 A-14, line D; the prototype «dialogos», DG-01 to DG-08): «Insertar vista…» of the
    /// editors of the window is the panel of the editor (<see cref="RackViewInsertPanelViewModel"/>), not the classic dialog «Insertar
    /// varias vistas» as a modal of AutoCAD. The window gives every editor what the panel shows that only the window knows: the kinds of view
    /// the drawing already has for the rack (the last reading of the list, DG-04) and whether the door can take a write now (another request
    /// waits for AutoCAD, DG-05), and it tells the open panels when that changes. The writes are the ones of «Insertar vista…» as before.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        // The panel of a Dinámico editor.
        private void WireInsertPanel(DynamicEditorViewModel editor)
        {
            editor.ExistingViewKinds = () => RackList.ViewKindsOf(editor.RackId);
            editor.OtherRequestText = OtherRequestText;
        }

        // The panel of a Selectivo editor.
        private void WireInsertPanel(SelectiveEditorViewModel editor)
        {
            editor.ExistingViewKinds = () => RackList.ViewKindsOf(editor.RackId);
            editor.OtherRequestText = OtherRequestText;
        }

        // The one request of the door waits for AutoCAD: a write would be refused now (DoorBusyText). While AutoCAD writes the editor's own
        // rack, the editor says it first (WritingText) and its panel is closed.
        private string OtherRequestText() => _inFlight != null ? DoorBusyText : string.Empty;

        // The request in flight changed: every editor of the window says it on its button and in its open panel.
        private void RefreshInsertPanels()
        {
            foreach (var entry in _dynamicDrafts.Values) entry.Editor.RefreshInsertState();
            foreach (var entry in _selectiveDrafts.Values) entry.Editor.RefreshInsertState();
        }
    }
}
