using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.UI.Editor;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T7 cab (I-64, séptima tanda; the Owner on 2026-11-27: «Las dos»): a new cabecera of the RackCad window as the request of a new rack,
    /// the one the classic configurator opened from «Nuevo» hands to the dispatch of the menu (RACKNUEVO, <c>HeaderEditorModule</c>): its
    /// identity (given by the window), the cabecera (its name empty for the automatic name of I-60) and the views to insert, in this order.
    /// A new cabecera carries no project of the library (the window opens a design of the library of a cabecera in the classic configurator,
    /// T7cab-S9), so its source is null, as the classic one opened from «Nuevo». The dispatch names the cabecera and places every view with
    /// the same batch as the classic configurator.
    /// </summary>
    public static class CabeceraDoorInsertion
    {
        public static RackInsertionRequest NewRackRequest(string rackId, RackFrameConfiguration cabecera, IReadOnlyList<RackViewAddress> views)
        {
            if (cabecera == null || views == null || views.Count == 0) return null;
            var request = new HeaderInsertionRequest(cabecera, sourceProject: null, rackId, views[0]);
            request.SetViews(views);
            return request;
        }
    }
}
