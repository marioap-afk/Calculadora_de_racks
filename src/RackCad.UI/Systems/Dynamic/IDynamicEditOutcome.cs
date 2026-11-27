using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;

namespace RackCad.UI.Systems.Dynamic
{
    /// <summary>
    /// What an editor of a dynamic system hands to the drawing half of RACKEDITAR (I-64 A-14, line D): the design and the resolved system
    /// to write, the identity, whether it only updates the drawn views and, when it inserts, the request with its views. The classic window
    /// (<see cref="RackDynamicSystemWindow"/>) is one; the draft of the RackCad window is another, update only.
    /// </summary>
    public interface IDynamicEditOutcome
    {
        DynamicRackDesign DesignToInsert { get; }

        DynamicRackSystem SystemToInsert { get; }

        string RackId { get; }

        string RackName { get; }

        bool UpdateOnly { get; }

        /// <summary>The insertion request with its views; read only when <see cref="UpdateOnly"/> is false.</summary>
        RackInsertionRequest InsertionRequest { get; }
    }

    /// <summary>
    /// The draft of the RackCad window as an outcome, with the identity of the rack in the drawing: update only, or («Insertar vista…»,
    /// step 4 of Dinámico) an update that then inserts views of the rack, as «Insertar» of the classic editor on a rack of the drawing.
    /// </summary>
    public sealed class DynamicDraftOutcome : IDynamicEditOutcome
    {
        public DynamicDraftOutcome(string rackId, string rackName, DynamicRackDesign design, DynamicRackSystem system)
        {
            RackId = rackId;
            RackName = rackName;
            DesignToInsert = design;
            SystemToInsert = system;
        }

        /// <summary>The draft written in the views that exist and then these views inserted, in this order, as linked copies.</summary>
        public static DynamicDraftOutcome Inserting(string rackId, string rackName, DynamicRackDesign design, DynamicRackSystem system,
            IReadOnlyList<RackViewAddress> views)
        {
            var outcome = new DynamicDraftOutcome(rackId, rackName, design, system);
            if (views == null || views.Count == 0) return outcome;

            var first = RackViewCodec.Encode(RackSystemKind.PalletFlow, views[0]);
            var request = new DynamicInsertionRequest(system, design, rackId, rackName, first.View, first.Section, null);
            request.SetViews(views);
            outcome.insertion = request;
            return outcome;
        }

        private RackInsertionRequest insertion;

        /// <summary>
        /// A new Dinámico of the RackCad window as the request of a new rack, the one the classic editor hands to the dispatch of «Nuevo»:
        /// its identity (given by the window), its name (empty for the automatic name of I-60) and the views to insert, in this order.
        /// T6 lib (UT-S10): with <paramref name="sourceDesignJson"/>, the design of the library it was opened from, the request carries that
        /// project as the classic editor opened from the library carries it (<c>SourceProjectToInsert</c>), so its unknown fields reach the
        /// drawing (I-11).
        /// </summary>
        public static RackInsertionRequest NewRackRequest(string rackId, string rackName, DynamicRackDesign design, DynamicRackSystem system,
            IReadOnlyList<RackViewAddress> views, string sourceDesignJson = null)
        {
            if (views == null || views.Count == 0) return null;
            var first = RackViewCodec.Encode(RackSystemKind.PalletFlow, views[0]);
            var request = new DynamicInsertionRequest(system, design, rackId, rackName, first.View, first.Section,
                RackLibrarySourceDesign.Read(sourceDesignJson, RackSystemKind.PalletFlow));
            request.SetViews(views);
            return request;
        }

        public DynamicRackDesign DesignToInsert { get; }

        public DynamicRackSystem SystemToInsert { get; }

        public string RackId { get; }

        public string RackName { get; }

        public bool UpdateOnly => insertion == null;

        public RackInsertionRequest InsertionRequest => insertion;
    }
}
