using System.Collections.Generic;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;

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

    /// <summary>The draft of the RackCad window as an outcome, with the identity of the rack in the drawing: update only.</summary>
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

        public SelectivePalletDesign DesignToInsert { get; }

        public SelectiveRackSystem SystemToInsert { get; }

        public string RackId { get; }

        public string RackName { get; }

        public IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> LinkedPropertyFinalStates { get; }
    }
}
