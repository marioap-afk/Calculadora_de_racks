using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// What the drafts of «Nuevo» (cuarta tanda, part «new») need from the editor of a Selectivo, in a file of their own. «Duplicar»
    /// (point 1 of the review of the part): the copy keeps the base of «Modificado» of its original, so a copy of a draft with changes is
    /// born «Modificado» and its work is protected like any other (the veto of the close, the prune, the question of «Descartar el
    /// borrador» and of «Cerrar el editor»). The window opens the copy over the base of the original and then hands it the copied design
    /// as an edit.
    /// </summary>
    public sealed partial class SelectiveEditorViewModel
    {
        /// <summary>The base of «Modificado» and of «Descartar»: the design (its document JSON) this editor compares with; null before a valid one.</summary>
        internal string BaselineDesignJson => _baselineJson;

        /// <summary>
        /// The draft takes <paramref name="design"/> as an edit over its base: the same load as «Descartar», without taking a new base,
        /// so the editor is «Modificado» exactly when the design differs from its base. False when the design does not recompose: the
        /// draft is then left without a valid design, and the window drops this editor (it is not registered yet).
        /// </summary>
        internal bool TakeDesignAsEdit(SelectivePalletDesign design)
        {
            if (design == null || !CanEdit) return false;
            if (!_editor.LoadFrom(design, _openedStates, _editor.Name, _variableNames)) return false;

            _isCopy = true; // T6 ui (correction 1 of the review): only a copy of «Duplicar» takes a design as an edit
            UpdateDirty();
            Rebuild();
            return true;
        }

        // T6 ui: a copy of «Duplicar»: its «Descartar» goes back to how its original opened, and its texts say so.
        private bool _isCopy;
    }
}
