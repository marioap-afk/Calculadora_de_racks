using System.Globalization;
using System.Text;

namespace RackCad.Application.Bom
{
    /// <summary>Renders a <see cref="BillOfMaterials"/> as RFC-4180 CSV (InvariantCulture numbers). A component BOM
    /// exports two levels (a "Componente" row per component, then its "Pieza" rows with TOTAL quantities = per-unit ×
    /// component qty); a flat/piece BOM keeps the classic single-level columns.</summary>
    public static class BomCsvExporter
    {
        // RFC-4180 mandates CRLF; use it explicitly for header AND rows (AppendLine is OS-dependent).
        private const string NewLine = "\r\n";

        public static string ToCsv(BillOfMaterials bom)
        {
            var builder = new StringBuilder();

            if (bom != null && bom.IsComponentBased)
            {
                builder.Append("Nivel,Categoria,Perfil,Descripcion,Longitud_in,Cantidad").Append(NewLine);
                foreach (var component in bom.Components)
                {
                    AppendRow(builder, "Componente", component.Category, component.ProfileId, component.Description, component.Length, component.Quantity);
                    foreach (var piece in component.Pieces)
                    {
                        AppendRow(builder, "Pieza", piece.Category, piece.ProfileId, piece.Description, piece.Length, piece.Quantity * component.Quantity);
                    }
                }

                return builder.ToString();
            }

            builder.Append("Categoria,Perfil,Descripcion,Longitud_in,Cantidad").Append(NewLine);
            if (bom != null)
            {
                foreach (var line in bom.Lines)
                {
                    builder.Append(Escape(line.Category)).Append(',')
                        .Append(Escape(line.ProfileId)).Append(',')
                        .Append(Escape(line.Description)).Append(',')
                        .Append(line.Length.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                        .Append(line.Quantity.ToString(CultureInfo.InvariantCulture))
                        .Append(NewLine);
                }
            }

            return builder.ToString();
        }

        private static void AppendRow(StringBuilder builder, string level, string category, string profileId, string description, double length, int quantity)
        {
            builder.Append(Escape(level)).Append(',')
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
