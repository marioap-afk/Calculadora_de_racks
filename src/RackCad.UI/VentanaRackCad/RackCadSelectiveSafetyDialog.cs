using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Editar seguridad…» of the Selectivo editor of the window (I-64 A-14, line D, entrega 2; D-09 of Dinámico): the dialog «Elementos de
    /// seguridad» of the classic Selectivo editor, built with the arguments that editor gives it (<c>RackSelectiveWindow.Safety_Click</c>),
    /// over COPIES of the safety of the draft, and shown as a modal of AutoCAD through the port of the host (<see cref="IRackCadModalPort"/>),
    /// as <see cref="RackCadModalDialogs"/> does for Dinámico. «Aceptar» gives its result; «Cancelar» gives null, and nothing changes.
    /// </summary>
    public static class RackCadSelectiveSafetyDialog
    {
        /// <summary>
        /// The dialog of the classic editor over copies of the safety of the draft: every safety element of the catalog, the posts of the
        /// longest fondo, the RESOLVED beam levels of each front (the grid of the topes and the parrillas), the fondos, the live deck count of
        /// the parrillas and the resolved system. The system is resolved from a copy of the last valid design, as the classic editor resolves
        /// its own: the plans the dialog reads never touch the draft.
        /// </summary>
        internal static SelectiveSafetyWindow Build(SelectiveDraftEditor draft)
        {
            var resolved = draft.ResolveCopy();
            var levels = resolved != null ? SelectiveSafetyGrid.LevelCounts(resolved) : new List<int>();
            var parrillaPlan = resolved != null ? SelectiveParrillaPlan.Cells(resolved, draft.Catalog) : null;
            var posts = Enumerable.Range(0, draft.FondoCount).Select(draft.FrontCount).DefaultIfEmpty(0).Max() + 1;
            return new SelectiveSafetyWindow(
                draft.Catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>(),
                draft.CopySafety(),
                posts,
                levels,
                draft.FondoCount,
                parrillaPlan,
                draft.Catalog,
                resolved);
        }

        /// <summary>«Editar seguridad…»: the dialog over AutoCAD; its result, or null when it was cancelled or cannot be shown.</summary>
        public static IReadOnlyList<SelectiveSafetySelection> Edit(SelectiveDraftEditor draft, IRackCadModalPort modal)
        {
            if (draft?.System == null || modal == null) return null;
            var dialog = Build(draft);
            return modal.ShowModal(dialog) == true ? dialog.Result : null;
        }
    }
}
