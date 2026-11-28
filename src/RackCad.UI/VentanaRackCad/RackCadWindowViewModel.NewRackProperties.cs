using System.Linq;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T6 pr (I-64, sexta tanda; VA-S9, answered by the Owner on 2026-11-27: «Sí, y se escriben al insertarlo»): the tab «Propiedades» of
    /// a draft of «Nuevo», the one of VA-07 (note of the Owner: «Hay que crear una pestaña en cada rack en donde se puedan escribir las
    /// propiedades»). Until this part a rack not inserted had no editable tab and its properties were written afterwards with
    /// RACKPROPIEDADES. Now every draft of «Nuevo» (Selectivo, Dinámico, Push Back, Cantilever and cama; a cabecera has no draft of
    /// «Nuevo» and no tab) opens with the tab of a new rack (<see cref="RackPropertiesRules.ForNewRack"/>), writable and empty: its rows are
    /// part of the draft («Modificado», «Descartar», D-08), a wrong one stops «Insertar en el dibujo…», and the NUEVO command of the door
    /// that inserts the rack writes them after the insertion, in the same command (one «U»), as the APLICAR commands do after the redraw.
    /// «Duplicar» copies them, as the rest of the draft. Nothing here reads or writes the drawing.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        // The tab of the editor of a draft, whatever its system; null for an editor with no tab.
        private static RackPropertiesTabViewModel PropertiesTabOf(object editor)
        {
            switch (editor)
            {
                case SelectiveEditorViewModel selective: return selective.PropertiesTab;
                case DynamicEditorViewModel dynamic: return dynamic.PropertiesTab;
                case CamaEditorViewModel cama: return cama.PropertiesTab;
                case CantileverEditorViewModel cantilever: return cantilever.PropertiesTab;
                case PushBackEditorViewModel pushBack: return pushBack.PropertiesTab;
                case CabeceraEditorViewModel cabecera: return cabecera.IsStandalone ? cabecera.PropertiesTab : null; // T7 cab: a cabecera of its own (a draft of «Nuevo» too)
                default: return null;
            }
        }

        // A new draft (the defaults, a design of the library or a copy) opens with the tab of a new rack: nothing was read, nothing is lost.
        private static void LoadNewRackProperties(object editor) => PropertiesTabOf(editor)?.Load(RackPropertiesRules.ForNewRack());

        // «Duplicar»: the copy that opened takes the rows of its original as rows of its own draft (they are not part of the design).
        private RackCadDraftOpening WithPropertiesOf(object original, RackCadDraftOpening opening)
        {
            if (opening == null || !opening.Opened || opening.Draft == null) return opening;
            var copy = _draftEntries.FirstOrDefault(e => ReferenceEquals(e.Item, opening.Draft));
            if (copy != null) PropertiesTabOf(copy.Editor)?.CopyRowsOf(PropertiesTabOf(original));
            return opening;
        }
    }
}
