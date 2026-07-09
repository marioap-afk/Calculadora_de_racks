using System.Globalization;
using System.Text;

namespace RackCad.Application.Bom
{
    /// <summary>Renders a <see cref="BillOfMaterials"/> as RFC-4180 CSV (InvariantCulture numbers).</summary>
    public static class BomCsvExporter
    {
        public static string ToCsv(BillOfMaterials bom)
        {
            // RFC-4180 mandates CRLF; use it explicitly for header AND rows (AppendLine is OS-dependent).
            const string NewLine = "\r\n";

            var builder = new StringBuilder();
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
