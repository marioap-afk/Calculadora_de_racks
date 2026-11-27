using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 pb: a new Push Back of the RackCad window as the request of a new rack, the one the classic editor opened from «Nuevo» hands to
    /// the dispatch of the menu (RACKNUEVO): its identity (given by the window), its name (empty for the automatic name of I-60) and the
    /// views to insert, in this order. The dispatch names the rack and places every view with the same batch as the classic editor.
    /// T6 lib (UT-S10): with <paramref name="sourceDesignJson"/>, the design of the library it was opened from, the request carries that
    /// project as the classic editor opened from the library carries it, so its unknown fields reach the drawing (I-11).
    /// </summary>
    public static class PushBackDoorInsertion
    {
        public static RackInsertionRequest NewRackRequest(string rackId, string rackName, PushBackDesign design, PushBackSystem system,
            IReadOnlyList<RackViewAddress> views, string sourceDesignJson = null)
        {
            if (views == null || views.Count == 0) return null;
            var first = RackViewCodec.Encode(RackSystemKind.PushBack, views[0]);
            var request = new PushBackInsertionRequest(system, design, rackId, rackName, first.View, first.Section,
                RackCad.Application.VentanaRackCad.RackLibrarySourceDesign.Read(sourceDesignJson, RackSystemKind.PushBack));
            request.SetViews(views);
            return request;
        }
    }
}
