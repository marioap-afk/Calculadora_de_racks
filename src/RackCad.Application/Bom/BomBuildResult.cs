namespace RackCad.Application.Bom
{
    /// <summary>How building ONE rack's bill of materials turned out.</summary>
    public enum BomBuildOutcome
    {
        Success = 1,

        /// <summary>The design could not be read. Historic policy: the rack is skipped with a visible warning.</summary>
        UnreadablePayload = 2,

        /// <summary>
        /// The design is perfectly readable and a project-variable reference is not resolvable. It ABORTS the
        /// whole total, and the difference from <see cref="UnreadablePayload"/> is deliberate.
        /// </summary>
        BrokenProjectVariableReference = 3,

        /// <summary>The rack's views do not agree on one authored state, so there is nothing to quote from.</summary>
        NoAuthoredAuthority = 4,
    }

    /// <summary>
    /// The typed outcome of building one rack's BOM (I-47 G13).
    ///
    /// <para>
    /// It replaces <c>null</c> plus a blanket <c>catch</c>. Those two turned every expected state into the
    /// same one: a rack whose payload was corrupt and a rack whose VARIABLE was missing both arrived as "no
    /// se pudo interpretar", and the second one is not a payload problem at all — the design is perfectly
    /// readable, the register is what does not have the variable. Collapsing them cost the user the only
    /// piece of information that leads to a repair.
    /// </para>
    /// <para>
    /// So the states are named, and each carries what a user needs to act: the rack, the property and the
    /// variable. A caller decides what to do with each — and the policies differ on purpose.
    /// </para>
    /// </summary>
    public sealed class BomBuildResult
    {
        private BomBuildResult(
            BomBuildOutcome outcome,
            BillOfMaterials bom,
            string rackId,
            string rackName,
            string propertyId,
            string variableId,
            string error)
        {
            Outcome = outcome;
            Bom = bom;
            RackId = rackId;
            RackName = rackName;
            PropertyId = propertyId;
            VariableId = variableId;
            Error = error;
        }

        public BomBuildOutcome Outcome { get; }

        /// <summary>The bill of materials. Null on every failure — there is no partial BOM.</summary>
        public BillOfMaterials Bom { get; }

        public string RackId { get; }

        public string RackName { get; }

        /// <summary>The bound property, on a reference failure. Null otherwise.</summary>
        public string PropertyId { get; }

        /// <summary>The variable the reference names, RAW as persisted. Null otherwise.</summary>
        public string VariableId { get; }

        /// <summary>The visible reason. Null on success.</summary>
        public string Error { get; }

        public bool IsSuccess => Outcome == BomBuildOutcome.Success;

        public static BomBuildResult Success(BillOfMaterials bom)
            => new BomBuildResult(BomBuildOutcome.Success, bom, null, null, null, null, null);

        public static BomBuildResult UnreadablePayload(string rackId, string rackName, string error)
            => new BomBuildResult(BomBuildOutcome.UnreadablePayload, null, rackId, rackName, null, null, error);

        public static BomBuildResult BrokenProjectVariableReference(
            string rackId, string rackName, string propertyId, string variableId, string error)
            => new BomBuildResult(
                BomBuildOutcome.BrokenProjectVariableReference, null, rackId, rackName, propertyId, variableId, error);

        public static BomBuildResult NoAuthoredAuthority(string rackId, string rackName, string error)
            => new BomBuildResult(BomBuildOutcome.NoAuthoredAuthority, null, rackId, rackName, null, null, error);
    }
}
