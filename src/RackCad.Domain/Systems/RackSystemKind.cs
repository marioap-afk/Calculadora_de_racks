namespace RackCad.Domain.Systems
{
    /// <summary>Type of rack system. Allows different systems to coexist in the model.</summary>
    public enum RackSystemKind
    {
        /// <summary>A standalone cabecera (header frame). Historical name; the on-disk library shows it as "Cabecera".</summary>
        Selective,
        PalletFlow,

        /// <summary>A selective PALLET RACK design (the whole rack, not just a header) — distinct from <see cref="Selective"/>.</summary>
        SelectiveRack,

        /// <summary>A flow bed ("cama de rodamiento").</summary>
        Cama,

        /// <summary>A larguero (beam) component (visual + BOM only).</summary>
        Larguero,

        /// <summary>A Push Back pallet-rack system (LIFO; load and unload from the same low end). New in I-18.</summary>
        PushBack
    }
}
