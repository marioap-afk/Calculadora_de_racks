namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Type of a longitudinal module of a dynamic system. There are exactly N modules for N pallets
    /// deep; intermediate posts are NOT modules (they are zero-length position markers).
    /// </summary>
    public enum RackModuleKind
    {
        /// <summary>Cabecera inicial (end frame at the start). Length = pallet depth + 6".</summary>
        HeaderStart,

        /// <summary>Modulo interior (separador / tarima). Length = pallet depth.</summary>
        Separator,

        /// <summary>Cabecera final (end frame at the end). Length = pallet depth + 6".</summary>
        HeaderEnd
    }
}
