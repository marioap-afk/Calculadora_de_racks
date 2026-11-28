using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The views «Insertar vista…» of the Selectivo editor of the window offers (I-64 A-14, line D, Selectivo E2, part P3), as «Insertar
    /// varias…» of the classic Selectivo editor lists them (<c>RackSelectiveWindow.InsertBatch_Click</c>): a frontal per fondo, a lateral per
    /// post with a cut, the plan.
    /// <para>
    /// T4 ins (DG-01, accepted by the Owner on 2026-11-26): they are chosen in the panel of the editor
    /// (<see cref="RackViewInsertPanelViewModel"/>), not in the classic dialog «Insertar varias vistas» as a modal of AutoCAD (the provisional
    /// P3-01). The window no longer shows that dialog; the classic editors keep it until they migrate.
    /// </para>
    /// </summary>
    public static class SelectiveViewChoice
    {
        /// <summary>The views of the classic «Insertar varias…» for this Selectivo: a frontal per fondo, a lateral per post with a cut, the plan.</summary>
        public static IReadOnlyList<RackViewInsertOption> Options(SelectiveRackSystem system, RackCatalog catalog)
            => SelectiveDoorInsertion.ViewOptions(system, catalog)
                .Select(option => new RackViewInsertOption(option.Label, option.Address))
                .ToList();
    }
}
