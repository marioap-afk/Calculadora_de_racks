using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin.Headers
{
    /// <summary>Outcome of turning a header plan into a single AutoCAD block definition.</summary>
    public sealed class LateralHeaderBlockResult
    {
        public LateralHeaderBlockResult(ObjectId definitionId, string blockName, LateralHeaderDrawOutcome outcome)
        {
            DefinitionId = definitionId;
            BlockName = blockName;
            Outcome = outcome;
        }

        /// <summary>Id of the created block definition (BlockTableRecord).</summary>
        public ObjectId DefinitionId { get; }

        /// <summary>Final, unique block name actually used.</summary>
        public string BlockName { get; }

        /// <summary>What was inserted into the definition and which blocks were missing.</summary>
        public LateralHeaderDrawOutcome Outcome { get; }
    }
}
