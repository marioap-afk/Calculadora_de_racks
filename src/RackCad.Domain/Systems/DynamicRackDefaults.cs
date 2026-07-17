namespace RackCad.Domain.Systems
{
    /// <summary>Domain-level constants for dynamic (pallet flow) systems.</summary>
    public static class DynamicRackDefaults
    {
        public const int DefaultLoadLevels = 3;
        public const int DefaultPalletsDeep = 8;
        public const double DefaultFirstLevelHeight = 6.0;
        public const double DefaultBeamDepth = 6.0;
        public const double DefaultIntermediateBeamDepth = 3.5;
        public const double DefaultClearHeight = 6.0;
        public const double DefaultPostPeralte = 3.0;
        /// <summary>Legacy persisted input; dynamic beam cuts now use the fixed BFR contract.</summary>
        public const double DefaultPalletTolerance = 4.0;
        public const int DefaultPalletsWide = 1;

        /// <summary>The bed-frame width (BFR) exceeds one pallet front by two inches.</summary>
        public const double BfrAllowance = 2.0;

        /// <summary>A complete IN/OUT beam exceeds the sum of its lane BFR widths by six inches.</summary>
        public const double InOutBeamLengthAllowance = 6.0;

        /// <summary>The complete bed is four inches shorter than its front's total longitudinal span.</summary>
        public const double FlowBedLengthClearance = 4.0;

        /// <summary>Historical WPF value used only when opening a document written before beam inputs were persisted.</summary>
        public const double LegacyDefaultBeamDepth = 4.0;

        /// <summary>The only complete entrance/exit beam enabled for the first pallet-flow implementation.</summary>
        public const string InOutBeamCatalogId = "LARGUERO_IN_OUT_C6";
        public const string InOutBeamView = "LATERAL";

        /// <summary>Connection point on the complete IN/OUT beam where a roller bed bolts on.</summary>
        public const string InOutBeamBedMatePoint = "TROQUEL_CAMA";

        /// <summary>The intermediate support beam used at a derived reinforced post.</summary>
        public const string IntermediateBeamCatalogId = "LARGUERO_ESCALON_INFINITO";
        public const string IntermediateBeamView = "LATERAL";

        /// <summary>Bed contact used on the first and mirrored second post of a reinforced pair.</summary>
        public const string IntermediateBeamLeftBedMatePoint = "INICIO_IZQUIERDO";
        public const string IntermediateBeamRightBedMatePoint = "INICIO_DERECHO";

        /// <summary>Extra length each header adds beyond the pallet depth (the +12 split across both ends).</summary>
        public const double HeaderEndAllowance = 6.0;

        /// <summary>Catalog id of the separator beam that links adjacent headers (its block lives per view).</summary>
        public const string SeparatorCatalogId = "SEPARADOR_DE_CABECERA_FORMADA_DE_CINTA_CALIBRE_12";

        /// <summary>View the separator block/connection point use in the system's lateral drawing.</summary>
        public const string SeparatorView = "FRONTAL";

        /// <summary>Connection point on the post where separators stack (lateral view).</summary>
        public const string SeparatorPostPoint = "TROQUEL_SEPARADOR";

        /// <summary>Connection point on the separator that mates to the post.</summary>
        public const string SeparatorMatePoint = "TROQUEL_CABECERA";
    }
}
