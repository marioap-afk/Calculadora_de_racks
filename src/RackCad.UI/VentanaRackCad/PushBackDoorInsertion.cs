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
    /// </summary>
    public static class PushBackDoorInsertion
    {
        public static RackInsertionRequest NewRackRequest(string rackId, string rackName, PushBackDesign design, PushBackSystem system,
            IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0) return null;
            var first = RackViewCodec.Encode(RackSystemKind.PushBack, views[0]);
            var request = new PushBackInsertionRequest(system, design, rackId, rackName, first.View, first.Section, null);
            request.SetViews(views);
            return request;
        }
    }
}
