namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Type of a longitudinal module. Only headers and separators are real modules; in the UI the
    /// three header kinds are all shown as "cabecera". Intermediate posts are NOT a module type:
    /// they are derived markers drawn where two separators meet.
    /// </summary>
    public enum DynamicRackModuleKind
    {
        /// <summary>Cabecera inicial (end frame at the start). Length = pallet depth + 6".</summary>
        HeaderStart,

        /// <summary>Cabecera intermedia. Length = pallet depth.</summary>
        HeaderIntermediate,

        /// <summary>Cabecera final (end frame at the end). Length = pallet depth + 6".</summary>
        HeaderEnd,

        /// <summary>Separador / bahia de tarima. Length = pallet depth.</summary>
        Separator
    }
}
