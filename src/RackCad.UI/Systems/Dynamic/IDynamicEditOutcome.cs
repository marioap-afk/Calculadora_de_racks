using RackCad.Domain.Systems.Dynamic;
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

    /// <summary>The draft of the RackCad window as an outcome: update only, with the identity of the rack in the drawing.</summary>
    public sealed class DynamicDraftOutcome : IDynamicEditOutcome
    {
        public DynamicDraftOutcome(string rackId, string rackName, DynamicRackDesign design, DynamicRackSystem system)
        {
            RackId = rackId;
            RackName = rackName;
            DesignToInsert = design;
            SystemToInsert = system;
        }

        public DynamicRackDesign DesignToInsert { get; }

        public DynamicRackSystem SystemToInsert { get; }

        public string RackId { get; }

        public string RackName { get; }

        public bool UpdateOnly => true;

        public RackInsertionRequest InsertionRequest => null;
    }
}
