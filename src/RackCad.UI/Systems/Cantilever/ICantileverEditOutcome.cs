using System.Collections.Generic;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;

namespace RackCad.UI.Systems.Cantilever
{
    /// <summary>
    /// T5 ct (I-64, fifth batch): what an editor of a Cantilever line hands to the drawing half of RACKEDITAR (<c>ApplyCantileverEdit</c>): the
    /// design and the resolved line to write, the identity, whether it only updates the views drawn and, when it inserts, the request with its
    /// views. The classic window (<see cref="RackCantileverWindow"/>) is one; the draft of the RackCad window is another
    /// (<see cref="CantileverDraftOutcome"/>), as <c>IDynamicEditOutcome</c> for Dinámico.
    /// </summary>
    public interface ICantileverEditOutcome
    {
        CantileverLineDesign DesignToInsert { get; }

        CantileverLineAssembly LineToInsert { get; }

        string RackId { get; }

        string RackName { get; }

        bool UpdateOnly { get; }

        /// <summary>The insertion request with its views; read only when <see cref="UpdateOnly"/> is false.</summary>
        RackInsertionRequest InsertionRequest { get; }
    }

    /// <summary>
    /// The draft of the RackCad window as an outcome, with the identity of the line in the drawing: update only (RACKCANTILEVERAPLICAR), or an
    /// update that then inserts views of the line (RACKCANTILEVERINSERTAR), as «Insertar» of the classic editor on a line of the drawing. A new
    /// line of the window is the request of a new rack (<see cref="NewRackRequest"/>), the one the classic editor hands to the dispatch of
    /// «Nuevo».
    /// </summary>
    public sealed class CantileverDraftOutcome : ICantileverEditOutcome
    {
        private RackInsertionRequest insertion;

        public CantileverDraftOutcome(string rackId, string rackName, CantileverLineDesign design, CantileverLineAssembly line)
        {
            RackId = rackId;
            RackName = rackName;
            DesignToInsert = design;
            LineToInsert = line;
        }

        /// <summary>The draft written in the views that exist and then these views inserted, in this order, as linked copies.</summary>
        public static CantileverDraftOutcome Inserting(string rackId, string rackName, CantileverLineDesign design, CantileverLineAssembly line,
            IReadOnlyList<RackViewAddress> views)
        {
            var outcome = new CantileverDraftOutcome(rackId, rackName, design, line);
            if (views == null || views.Count == 0) return outcome;
            outcome.insertion = Request(rackId, rackName, design, line, views);
            return outcome;
        }

        /// <summary>
        /// A new line of the RackCad window as the request of a new rack: its identity (given by the window), its name (empty for the automatic
        /// name of I-60) and the views to insert, in this order. Null without views.
        /// </summary>
        public static RackInsertionRequest NewRackRequest(string rackId, string rackName, CantileverLineDesign design, CantileverLineAssembly line,
            IReadOnlyList<RackViewAddress> views)
            => views == null || views.Count == 0 ? null : Request(rackId, rackName, design, line, views);

        private static CantileverInsertionRequest Request(string rackId, string rackName, CantileverLineDesign design, CantileverLineAssembly line,
            IReadOnlyList<RackViewAddress> views)
        {
            var first = RackViewCodec.Encode(RackSystemKind.Cantilever, views[0]);
            var request = new CantileverInsertionRequest(line, design, rackId, rackName, first.View, first.Section, null);
            request.SetViews(views);
            return request;
        }

        public CantileverLineDesign DesignToInsert { get; }

        public CantileverLineAssembly LineToInsert { get; }

        public string RackId { get; }

        public string RackName { get; }

        public bool UpdateOnly => insertion == null;

        public RackInsertionRequest InsertionRequest => insertion;
    }
}
