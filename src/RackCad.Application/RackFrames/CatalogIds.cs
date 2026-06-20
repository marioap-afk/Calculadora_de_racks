namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Single source of truth for the catalog ids the standard frame and the factory rely on.
    /// Avoids the same string literals being redeclared across builders.
    /// </summary>
    public static class CatalogIds
    {
        public const string StandardPost = "POSTE_OMEGA_3X3";
        public const string BasePlate = "PLACA_BASE_ATORNILLABLE";
        public const string LowerHorizontal = "HORIZONTAL_INFERIOR";
        public const string IntermediateHorizontal = "HORIZONTAL_INTERMEDIA";
        public const string UpperHorizontal = "HORIZONTAL_SUPERIOR";
        public const string Diagonal = "TRAVESANO_DINAMICO_OMEGA_3X3";
        public const string BraceStartConnectionPoint = "TroquelCelosia_01";
        public const string BraceEndConnectionPoint = "TroquelCelosia_02";
        public const string BasePlateConnectionPoint = "PlacaBase_01";
    }
}
