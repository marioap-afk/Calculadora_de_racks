namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Kind of roller bed ("cama de rodamiento"). The geometry is shared, but a pushback bed has no
    /// brakes (frenos), so the type drives whether brakes are placed.
    /// </summary>
    public enum FlowBedType
    {
        /// <summary>Gravity flow bed: rollers + brakes (frenos) every N rollers + end stop.</summary>
        Dynamic,

        /// <summary>Pushback bed: rollers + end stop, NO brakes.</summary>
        Pushback
    }
}
