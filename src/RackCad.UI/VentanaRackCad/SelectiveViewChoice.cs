using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Views;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Insertar vista…» of the Selectivo editor of the window (I-64 A-14, line D, Selectivo E2, part P3): the views it offers and the
    /// dialog that chooses them, as «Insertar varias…» of the classic Selectivo editor (<c>RackSelectiveWindow.InsertBatch_Click</c>) and
    /// as Dinámico does it (<see cref="RackCadModalDialogs.ChooseViews"/>), shown as a modal of AutoCAD by the port of the host. Its own
    /// file, so the shared dialogs of the window are not touched: the only difference is the system of the dialog, which decides the views
    /// it exposes (a frontal by fondo exists only for <see cref="RackSystemKind.SelectiveRack"/>).
    /// <para>
    /// Provisional (an open question to the Owner): the views are chosen with the classic dialog, as Dinámico, and not with the panel of
    /// the approved draft (one frontal by fondo, ONE lateral by a list of posts, the plan). The editor only knows
    /// <see cref="SelectiveEditorViewModel.ViewChooser"/>, so the panel can replace this class without touching it.
    /// </para>
    /// </summary>
    public static class SelectiveViewChoice
    {
        /// <summary>The views of the classic «Insertar varias…» for this Selectivo: a frontal per fondo, a lateral per post with a cut, the plan.</summary>
        public static IReadOnlyList<RackViewBatchOption> Options(SelectiveRackSystem system, RackCatalog catalog)
            => SelectiveDoorInsertion.ViewOptions(system, catalog)
                .Select(option => new RackViewBatchOption(option.Label, option.Address))
                .ToList();

        /// <summary>The dialog of «Insertar varias…» of a Selectivo over AutoCAD; the views chosen in their order, or null when cancelled.</summary>
        public static IReadOnlyList<RackViewAddress> Choose(IReadOnlyList<RackViewBatchOption> options, IRackCadModalPort modal)
        {
            if (modal == null) return null;
            var presenter = new RackViewBatchDialogPresenter(RackSystemKind.SelectiveRack, options);
            var dialog = new RackViewBatchDialog(presenter);
            return modal.ShowModal(dialog) == true ? presenter.Result : null;
        }
    }
}
