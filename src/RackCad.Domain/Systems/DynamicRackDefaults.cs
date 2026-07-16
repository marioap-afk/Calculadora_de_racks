namespace RackCad.Domain.Systems
{
    /// <summary>Domain-level constants for dynamic (pallet flow) systems.</summary>
    public static class DynamicRackDefaults
    {
        public const int DefaultLoadLevels = 3;
        public const double DefaultFirstLevelHeight = 6.0;
        public const double DefaultBeamDepth = 4.0;
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
