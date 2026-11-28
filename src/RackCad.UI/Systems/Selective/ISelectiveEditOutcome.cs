using System.Collections.Generic;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;

namespace RackCad.UI.Systems.Selective
{
    /// <summary>
    /// What an editor of a Selectivo hands to the drawing half of «Actualizar» of RACKEDITAR (I-64 A-14, line D, Selectivo E1-A): the design
    /// and the resolved system to draw, the identity and the final state of every linked property, which the drawing half reconciles onto
    /// the authored document. The classic window (<see cref="RackSelectiveWindow"/>) is one; the draft of the RackCad window is another.
    /// </summary>
    public interface ISelectiveEditOutcome
    {
        SelectivePalletDesign DesignToInsert { get; }

        SelectiveRackSystem SystemToInsert { get; }

        string RackId { get; }

        string RackName { get; }

        IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> LinkedPropertyFinalStates { get; }
    }

    /// <summary>
    /// The draft of the RackCad window as an outcome, with the identity of the rack in the drawing: the update, which «Insertar vista…»
    /// follows with its views (<c>RackSelectivoCommands.InsertSelectiveViews</c>). A new Selectivo is not an outcome: it is the request of a
    /// new rack (<see cref="NewRackRequest"/>).
    /// </summary>
    public sealed class SelectiveDraftOutcome : ISelectiveEditOutcome
    {
        public SelectiveDraftOutcome(
            string rackId,
            string rackName,
            SelectivePalletDesign design,
            SelectiveRackSystem system,
            IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> linkedPropertyFinalStates)
        {
            RackId = rackId;
            RackName = rackName;
            DesignToInsert = design;
            SystemToInsert = system;
            LinkedPropertyFinalStates = linkedPropertyFinalStates ?? new Dictionary<PropertyId, LinkedPropertyEditState>();
        }

        /// <summary>
        /// A new Selectivo of the RackCad window (Selectivo E2) as the request of a new rack, the one the classic editor opened from «Nuevo»
        /// hands to the dispatch of RACKNUEVO (<c>RackSelectiveWindow.InsertBatch_Click</c>): its identity (given by the window), its name
        /// (blank for the automatic name of I-60), the design and system to draw and the views to insert, in this order. Null without views.
        /// </summary>
        public static RackInsertionRequest NewRackRequest(string rackId, string rackName, SelectivePalletDesign design, SelectiveRackSystem system,
            IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0) return null;
            var request = new SelectiveInsertionRequest(system, design, rackId, rackName,
                RackViewCodec.Encode(RackSystemKind.SelectiveRack, views[0]).View);
            request.SetViews(views);
            return request;
        }

        public SelectivePalletDesign DesignToInsert { get; }

        public SelectiveRackSystem SystemToInsert { get; }

        public string RackId { get; }

        public string RackName { get; }

        public IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> LinkedPropertyFinalStates { get; }
    }
}
