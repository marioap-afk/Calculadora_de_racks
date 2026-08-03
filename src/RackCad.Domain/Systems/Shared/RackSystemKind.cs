namespace RackCad.Domain.Systems.Shared
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
        PushBack,

        /// <summary>
        /// A Cantilever LINE: a sequence of stations sharing one column height, with the separators and braced
        /// panels between them. New in I-37D, and added at the END so the six values above keep their numbers.
        ///
        /// The persisted unit is the LINE and not a station. A station is not independently editable, insertable
        /// or duplicable, and per-station persistence would let a line's stations drift apart in the drawing
        /// while the design said they were one rack.
        /// </summary>
        Cantilever
    }
}
