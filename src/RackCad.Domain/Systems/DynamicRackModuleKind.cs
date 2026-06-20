namespace RackCad.Domain.Systems
{
    /// <summary>Type of a longitudinal module of a dynamic system (each position can be edited).</summary>
    public enum DynamicRackModuleKind
    {
        /// <summary>Cabecera inicial (end frame at the start). Length = pallet depth + 6".</summary>
        HeaderStart,

        /// <summary>Cabecera intermedia (an interior position turned into a frame).</summary>
        HeaderIntermediate,

        /// <summary>Cabecera final (end frame at the end). Length = pallet depth + 6".</summary>
        HeaderEnd,

        /// <summary>Separador / bahia de tarima. Length = pallet depth.</summary>
        Separator,

        /// <summary>Poste intermedio: a zero-length position marker (drawn as a line).</summary>
        IntermediatePost,

        /// <summary>Modulo a medida.</summary>
        Custom
    }
}
