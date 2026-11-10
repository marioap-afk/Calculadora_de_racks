using System.Collections.Generic;

namespace RackCad.Application.Bom
{
    /// <summary>Renders a <see cref="ConsolidatedBom"/> as a real <c>.xlsx</c>: "Por rack" (each rack's componentes + piezas,
    /// a native Excel table) and "Total del dibujo" (the drawing-wide grand total, by component).
    /// <para>"Por rack" quantities are for ONE copy of the rack, so "Cantidad total" = Cantidad × Copias, computed here and
    /// nowhere else. "Total del dibujo" gets no such column: <see cref="ConsolidatedBomBuilder"/> already multiplied it.</para></summary>
    public static class ConsolidatedBomXlsxExporter
    {
        /// <summary>Stable name of the detail table in every exported workbook.</summary>
        public const string DetailTableName = "RackCadBOM";

        public static byte[] ToXlsx(ConsolidatedBom consolidated)
            => XlsxWriter.Build(new[]
            {
                new XlsxSheet("Por rack", PerRackRows(consolidated), new XlsxTableDefinition(DetailTableName)),
                new XlsxSheet("Total del dibujo", GrandTotalRows(consolidated))
            });

        private static List<IReadOnlyList<XlsxCell>> PerRackRows(ConsolidatedBom consolidated)
        {
            var rows = new List<IReadOnlyList<XlsxCell>>
            {
                BomXlsxExporter.Header("Rack", "Tipo", "Copias", "Nivel", "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad", "Cantidad total")
            };

            if (consolidated == null)
            {
                return rows;
            }

            foreach (var rack in consolidated.Racks)
            {
                if (rack?.Bom == null)
                {
                    continue;
                }

                var copies = rack.Copies < 1 ? 1 : rack.Copies;
                if (rack.Bom.IsComponentBased)
                {
                    foreach (var component in rack.Bom.Components)
                    {
                        rows.Add(Row(rack.Name, rack.Kind, copies, "Componente", component.Category, component.ProfileId, component.Description, component.Length, component.Quantity));
                        foreach (var piece in component.Pieces)
                        {
                            rows.Add(Row(rack.Name, rack.Kind, copies, "Pieza", piece.Category, piece.ProfileId, piece.Description, piece.Length, piece.Quantity * component.Quantity));
                        }
                    }
                }
                else
                {
                    foreach (var line in rack.Bom.Lines)
                    {
                        rows.Add(Row(rack.Name, rack.Kind, copies, "Pieza", line.Category, line.ProfileId, line.Description, line.Length, line.Quantity));
                    }
                }
            }

            return rows;
        }

        private static List<IReadOnlyList<XlsxCell>> GrandTotalRows(ConsolidatedBom consolidated)
        {
            var rows = new List<IReadOnlyList<XlsxCell>>
            {
                BomXlsxExporter.Header("Nivel", "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad")
            };

            if (consolidated?.GrandTotal == null)
            {
                return rows;
            }

            foreach (var component in consolidated.GrandTotal.Components)
            {
                rows.Add(new List<XlsxCell>
                {
                    XlsxCell.Str("Componente"), XlsxCell.Str(component.Category), XlsxCell.Str(component.ProfileId),
                    XlsxCell.Str(component.Description), XlsxCell.Num(component.Length), XlsxCell.Num(component.Quantity)
                });
                foreach (var piece in component.Pieces)
                {
                    rows.Add(new List<XlsxCell>
                    {
                        XlsxCell.Str("Pieza"), XlsxCell.Str(piece.Category), XlsxCell.Str(piece.ProfileId),
                        XlsxCell.Str(piece.Description), XlsxCell.Num(piece.Length), XlsxCell.Num(piece.Quantity * component.Quantity)
                    });
                }
            }

            return rows;
        }

        private static IReadOnlyList<XlsxCell> Row(string rack, string kind, int copies, string level, string category, string profileId, string description, double length, int quantity)
            => new List<XlsxCell>
            {
                XlsxCell.Str(rack),
                XlsxCell.Str(kind),
                XlsxCell.Num(copies),
                XlsxCell.Str(level),
                XlsxCell.Str(category),
                XlsxCell.Str(profileId),
                XlsxCell.Str(description),
                XlsxCell.Num(length),
                XlsxCell.Num(quantity),
                XlsxCell.Num((long)quantity * copies)
            };
    }
}
