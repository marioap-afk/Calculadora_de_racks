using System.Globalization;
using System.Text;

namespace RackCad.Application.Bom
{
    /// <summary>Renders a <see cref="ConsolidatedBom"/> as RFC-4180 CSV: a per-rack breakdown (componentes + piezas)
    /// followed by the drawing-wide grand total (piece level). InvariantCulture numbers, CRLF line endings.</summary>
    public static class ConsolidatedBomCsvExporter
    {
        private const string NewLine = "\r\n";

        public static string ToCsv(ConsolidatedBom consolidated)
        {
            var builder = new StringBuilder();
            builder.Append("Rack,Tipo,Copias,Nivel,Categoria,Perfil,Descripcion,Longitud_in,Cantidad").Append(NewLine);

            if (consolidated == null)
            {
                return builder.ToString();
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
                        Row(builder, rack.Name, rack.Kind, copies, "Componente", component.Category, component.ProfileId, component.Description, component.Length, component.Quantity);
                        foreach (var piece in component.Pieces)
                        {
                            Row(builder, rack.Name, rack.Kind, copies, "Pieza", piece.Category, piece.ProfileId, piece.Description, piece.Length, piece.Quantity * component.Quantity);
                        }
                    }
                }
                else
                {
                    foreach (var line in rack.Bom.Lines)
                    {
                        Row(builder, rack.Name, rack.Kind, copies, "Pieza", line.Category, line.ProfileId, line.Description, line.Length, line.Quantity);
                    }
                }
            }

            // Grand total, by component (each component + its pieces ×component qty).
            foreach (var component in consolidated.GrandTotal.Components)
            {
                Row(builder, "TOTAL DEL DIBUJO", string.Empty, 0, "Componente", component.Category, component.ProfileId, component.Description, component.Length, component.Quantity);
                foreach (var piece in component.Pieces)
                {
                    Row(builder, "TOTAL DEL DIBUJO", string.Empty, 0, "Pieza", piece.Category, piece.ProfileId, piece.Description, piece.Length, piece.Quantity * component.Quantity);
                }
            }

            return builder.ToString();
        }

        private static void Row(StringBuilder builder, string rack, string kind, int copies, string level, string category, string profileId, string description, double length, int quantity)
        {
            builder.Append(Escape(rack)).Append(',')
                .Append(Escape(kind)).Append(',')
                .Append(copies > 0 ? copies.ToString(CultureInfo.InvariantCulture) : string.Empty).Append(',')
                .Append(Escape(level)).Append(',')
                .Append(Escape(category)).Append(',')
                .Append(Escape(profileId)).Append(',')
                .Append(Escape(description)).Append(',')
                .Append(length.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                .Append(quantity.ToString(CultureInfo.InvariantCulture))
                .Append(NewLine);
        }

        private static string Escape(string value)
        {
            value ??= string.Empty;
            if (value.IndexOf(',') < 0 && value.IndexOf('"') < 0 && value.IndexOf('\n') < 0 && value.IndexOf('\r') < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
