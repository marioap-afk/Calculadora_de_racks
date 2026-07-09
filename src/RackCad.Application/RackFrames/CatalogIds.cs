namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Single source of truth for the catalog ids the standard frame and the factory rely on.
    /// Avoids the same string literals being redeclared across builders. These point at the real
    /// shipped catalog: horizontals and diagonals share one celosía/truss profile.
    /// </summary>
    public static class CatalogIds
    {
        public const string StandardPost = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        public const string BasePlate = "PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16";

        // Horizontals and diagonals are all the same celosía/truss profile.
        public const string TrussProfile = "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14";
        public const string LowerHorizontal = TrussProfile;
        public const string IntermediateHorizontal = TrussProfile;
        public const string UpperHorizontal = TrussProfile;
        public const string Diagonal = TrussProfile;

        public const string BraceStartConnectionPoint = "TROQUEL_CELOSIA";
        public const string BraceEndConnectionPoint = "CELOSIA";
        public const string BasePlateConnectionPoint = "MONTAJE_POSTE";
    }
}
