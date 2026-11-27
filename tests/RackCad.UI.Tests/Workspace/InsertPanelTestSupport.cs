#nullable enable
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.UI.VentanaRackCad;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, fourth batch, part «ins» (DG-01): «Insertar vista…» opens the panel of the editor instead of the classic dialog
    /// «Insertar varias vistas». What the tests did with the chooser of the dialog («the user chose these views») they do with the panel:
    /// open it with the button of the editor, leave marked exactly these views in this order, and press «Insertar N vistas».
    /// </summary>
    internal static class InsertPanelTestSupport
    {
        /// <summary>«Insertar vista…» with these views, in this order; the panel, or null when the button did not open it.</summary>
        public static RackViewInsertPanelViewModel? InsertWith(this SelectiveEditorViewModel editor, params RackViewAddress[] views)
        {
            editor.InsertView();
            return Choose(editor.InsertPanel, views);
        }

        /// <summary>«Insertar vista…» of a Dinámico with these views, in this order.</summary>
        public static RackViewInsertPanelViewModel? InsertWith(this DynamicEditorViewModel editor, params RackViewAddress[] views)
        {
            editor.InsertView();
            return Choose(editor.InsertPanel, views);
        }

        /// <summary>T7 cab: «Insertar vista…» (or «Insertar en el dibujo…») of a cabecera of its own with these views, in this order.</summary>
        public static RackViewInsertPanelViewModel? InsertWith(this CabeceraEditorViewModel editor, params RackViewAddress[] views)
        {
            editor.InsertView();
            return Choose(editor.InsertPanel, views);
        }

        /// <summary>Leaves marked exactly these views, in this order (the seed of a new rack is taken out first).</summary>
        public static void Mark(this RackViewInsertPanelViewModel panel, params RackViewAddress[] views)
        {
            foreach (var marked in panel.Choice.Order.Select(e => e.Address).ToList()) panel.Toggle(marked);
            foreach (var view in views) panel.Toggle(view);
        }

        private static RackViewInsertPanelViewModel? Choose(RackViewInsertPanelViewModel? panel, RackViewAddress[] views)
        {
            if (panel == null) return null;
            panel.Mark(views);
            panel.Insert();
            return panel;
        }
    }
}
