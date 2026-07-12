using System.Collections.Generic;

namespace RackCad.Application.Bom
{
    /// <summary>Renders a <see cref="BillOfMaterials"/> as a real <c>.xlsx</c> (one sheet). A component BOM exports two
    /// levels (a "Componente" row per component, then its "Pieza" rows with TOTAL quantities = per-unit × component qty);
    /// a flat/piece BOM keeps the classic single-level columns. Same data as <see cref="BomCsvExporter"/>, in Excel.</summary>
    public static class BomXlsxExporter
    {
        public static byte[] ToXlsx(BillOfMaterials bom)
            => XlsxWriter.Build(new[] { new XlsxSheet("Lista de materiales", Rows(bom)) });

        /// <summary>The BOM sheet rows (header + data), reused by the consolidated exporter's per-rack blocks.</summary>
        internal static List<IReadOnlyList<XlsxCell>> Rows(BillOfMaterials bom)
        {
            var rows = new List<IReadOnlyList<XlsxCell>>();

            if (bom != null && bom.IsComponentBased)
            {
                rows.Add(Header("Nivel", "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad"));
                foreach (var component in bom.Components)
                {
                    rows.Add(Row("Componente", component.Category, component.ProfileId, component.Description, component.Length, component.Quantity));
                    foreach (var piece in component.Pieces)
                    {
                        rows.Add(Row("Pieza", piece.Category, piece.ProfileId, piece.Description, piece.Length, piece.Quantity * component.Quantity));
                    }
                }

                return rows;
            }

            rows.Add(Header("Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad"));
            if (bom != null)
            {
                foreach (var line in bom.Lines)
                {
                    rows.Add(new List<XlsxCell>
                    {
                        XlsxCell.Str(line.Category),
                        XlsxCell.Str(line.ProfileId),
                        XlsxCell.Str(line.Description),
                        XlsxCell.Num(line.Length),
                        XlsxCell.Num(line.Quantity)
                    });
                }
            }

            return rows;
        }

        internal static IReadOnlyList<XlsxCell> Header(params string[] titles)
        {
            var cells = new List<XlsxCell>(titles.Length);
            foreach (var title in titles) cells.Add(XlsxCell.Str(title, bold: true));
            return cells;
        }

        private static IReadOnlyList<XlsxCell> Row(string level, string category, string profileId, string description, double length, int quantity)
            => new List<XlsxCell>
            {
                XlsxCell.Str(level),
                XlsxCell.Str(category),
                XlsxCell.Str(profileId),
                XlsxCell.Str(description),
                XlsxCell.Num(length),
                XlsxCell.Num(quantity)
            };
    }
}
