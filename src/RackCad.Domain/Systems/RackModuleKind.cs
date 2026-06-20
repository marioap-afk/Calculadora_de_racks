namespace RackCad.Domain.Systems
{
    /// <summary>Type of a side-view module along the run of a dynamic system.</summary>
    public enum RackModuleKind
    {
        /// <summary>Cabecera inicial (end frame at the start). Length = pallet depth + 6".</summary>
        HeaderStart,

        /// <summary>Separador intermedio (pallet bay). Length = pallet depth.</summary>
        Separator,

        /// <summary>Poste intermedio: a vertical line with zero length.</summary>
        IntermediatePost,

        /// <summary>Cabecera final (end frame at the end). Length = pallet depth + 6".</summary>
        HeaderEnd
    }
}
