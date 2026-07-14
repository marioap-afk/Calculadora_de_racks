namespace RackCad.Domain.Systems
{
    /// <summary>Domain-level constants for the selective rack (frontal view).</summary>
    public static class SelectiveRackDefaults
    {
        /// <summary>The only view built so far.</summary>
        public const string View = "FRONTAL";

        /// <summary>Connection point ON THE POST where a larguero hooks (its X slides with the post peralte).</summary>
        public const string PostBeamPoint = "TROQUEL_LARGUERO";

        /// <summary>
        /// Connection point ON THE LARGUERO where the steel profile starts. The larguero's LONGITUD is the
        /// "A corte" (profile cut length), NOT the clear span: the ménsula juts out from the profile end to the
        /// hook (the origin), so this point's X is that ménsula overhang. Post-to-post therefore adds it twice.
        /// </summary>
        public const string BeamProfileStartPoint = "INICIO_PERFIL";

        /// <summary>Troquel pitch on the post (in): levels snap to this grid.</summary>
        public const double TroquelPaso = 2.0;

        /// <summary>Default pallet depth / fondo (in) when a design does not specify one (legacy documents too).</summary>
        public const double DefaultPalletDepth = 48.0;

        /// <summary>Default separation (in) between consecutive fondos in a doble-profundidad rack when a gap has no value.</summary>
        public const double DefaultSeparator = 12.0;

        /// <summary>How much shallower the cabecera (frame) is than the pallet it holds (in): the frame fondo = pallet
        /// fondo − this allowance. Real-world rule: a 48" pallet sits on a 42" cabecera (6" less).</summary>
        public const double CabeceraFondoAllowance = 6.0;

        /// <summary>Maximum number of fondos (cabecera-lines in depth) the editor allows (1 = sencillo).</summary>
        public const int MaxDepthCount = 4;

        /// <summary>Default larguero peralte (in) for a fresh design cell.</summary>
        public const double DefaultBeamPeralte = 4.0;

        /// <summary>Block parameter that stretches a piece to a length/height.</summary>
        public const string LengthParam = "LONGITUD";

        /// <summary>Block parameter for the section peralte.</summary>
        public const string PeralteParam = "PERALTE";

        /// <summary>Connection point on the base plate that mates to the post (lands on the post origin).</summary>
        public const string PlateMatePoint = "MONTAJE_POSTE";

        // ---- Pallet (tarima) visual reference ----

        /// <summary>Catalog pieceId (blocks.csv) for the visual-reference pallet block, resolved per view. If the row
        /// is missing for a view, the pallet is simply not drawn there (never in the BOM). Must match the pieceId column
        /// of the TARIMA row in blocks.csv (its blockName points at the actual library block, e.g. TARIMA_GENERICA).</summary>
        public const string PalletPieceId = "TARIMA_GENERICA";

        /// <summary>Block parameter that stretches the pallet HORIZONTALLY: in the FRONTAL view this is the pallet's
        /// frente; in the (future) LATERAL view the same param carries the fondo. Ignored if the block lacks it. Matched
        /// case-insensitively against the block's parameter names, so casing (e.g. "longitud") does not have to be exact.</summary>
        public const string PalletFrenteParam = "longitud";

        /// <summary>Block parameter that stretches the pallet VERTICALLY (its alto). Ignored if the block lacks it.
        /// Matched case-insensitively against the block's parameter names.</summary>
        public const string PalletAltoParam = "Alto";
    }
}
