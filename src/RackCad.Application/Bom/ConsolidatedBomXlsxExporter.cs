using System.Collections.Generic;

namespace RackCad.Application.Bom
{
    /// <summary>Renders a <see cref="ConsolidatedBom"/> as a real <c>.xlsx</c>: "Por rack" (each rack's componentes + piezas,
    /// a native Excel table) and "Total del dibujo" (the drawing-wide grand total, by component).
    /// <para>"Por rack" quantities are for ONE copy of the rack, so "Cantidad total" = Cantidad × Copias, computed here and
    /// nowhere else. "Total del dibujo" gets no such column: <see cref="ConsolidatedBomBuilder"/> already multiplied it.</para>
    /// <para>"Resumen" is a native PivotTable over that table. It opens with no filter selected, so a row can add a component
    /// and its own pieces when they share description and length; the sheet warns about it and nothing is hidden or merged.</para></summary>
    public static class ConsolidatedBomXlsxExporter
    {
        /// <summary>Stable name of the detail table in every exported workbook.</summary>
        public const string DetailTableName = "RackCadBOM";

        /// <summary>Stable name of the summary PivotTable.</summary>
        public const string SummaryPivotName = "ResumenBOM";

        public static byte[] ToXlsx(ConsolidatedBom consolidated)
            => XlsxWriter.Build(new[]
            {
                new XlsxSheet("Por rack", PerRackRows(consolidated), new XlsxTableDefinition(DetailTableName)),
                new XlsxSheet("Total del dibujo", GrandTotalRows(consolidated)),
                new XlsxSheet("Resumen", SummaryPivot())
            });

        private static XlsxPivotDefinition SummaryPivot()
            => new XlsxPivotDefinition(
                SummaryPivotName,
                DetailTableName,
                new[] { "Rack", "Nivel" },
                new[] { "Descripcion", "Longitud (in)" },
                "Cantidad total",
                "Suma de Cantidad total",
                "Descripcion / Longitud",
                "Total general",
                "(Todas)",
                new[]
                {
                    XlsxCell.Str("Atención: con Nivel en (Todas) la suma no representa necesariamente un total físico.", bold: true),
                    XlsxCell.Str("Pueden coexistir componentes y piezas relacionadas; usa el filtro Nivel para verlos por separado."),
                    XlsxCell.Str("Las camas de Dinámico y Push Back solo aparecen como Componente. Si los filtros salen vacíos, usa Datos > Actualizar todo.")
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
