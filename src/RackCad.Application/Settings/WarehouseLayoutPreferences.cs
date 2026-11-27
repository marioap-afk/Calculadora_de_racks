namespace RackCad.Application.Settings
{
    /// <summary>
    /// T5 al (I-64, prototype «almacen», question 6): the last values of the page «Almacén» of the RackCad window, per user, as they were
    /// typed. They describe the tool, not a drawing, so they live in the settings of the user (like «Fondos destino») and never travel
    /// inside a drawing. A field that is null was never saved and takes the value of the classic windows (RACKLAYOUT, RACKRELLENAR).
    /// </summary>
    public sealed class WarehouseLayoutPreferences
    {
        public string Rows { get; set; }

        public string Cols { get; set; }

        public string AisleRows { get; set; }

        public string AisleCols { get; set; }

        public bool? BackToBack { get; set; }

        public string BackGap { get; set; }

        public bool? Independent { get; set; }

        public string BuildingX { get; set; }

        public string BuildingY { get; set; }

        public string Layer { get; set; }

        public string WallClearance { get; set; }

        public string ColumnClearance { get; set; }

        public bool? TryRotated { get; set; }
    }
}
