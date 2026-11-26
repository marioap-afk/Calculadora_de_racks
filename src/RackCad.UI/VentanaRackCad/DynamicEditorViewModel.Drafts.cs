using RackCad.Domain.Systems.Dynamic;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// What the drafts of «Nuevo» (cuarta tanda, part «new») need from the editor of a Dinámico, in a file of their own. «Duplicar»
    /// (point 1 of the review of the part): the copy keeps the base of «Modificado» of its original, so a copy of a draft with changes is
    /// born «Modificado» and its work is protected like any other. The window opens the copy over the base of the original and then hands
    /// it the copied design as an edit.
    /// </summary>
    public sealed partial class DynamicEditorViewModel
    {
        /// <summary>The base of «Modificado» and of «Descartar»: the design (its project JSON) this editor compares with; null before a valid one.</summary>
        internal string BaselineDesignJson => _baselineJson;

        /// <summary>
        /// The draft takes <paramref name="design"/> as an edit over its base: the same load as «Descartar», without taking a new base,
        /// so the editor is «Modificado» exactly when the design differs from its base. False when it cannot be edited now.
        /// </summary>
        internal bool TakeDesignAsEdit(DynamicRackDesign design)
        {
            if (design == null || !CanEdit) return false;
            _editor.LoadFrom(design, _editor.Name);
            UpdateDirty();
            Rebuild();
            return true;
        }
    }
}
