using System.Globalization;
using System.Text;

namespace RackCad.Application.Bom
{
    /// <summary>Renders a <see cref="BillOfMaterials"/> as RFC-4180 CSV (InvariantCulture numbers).</summary>
    public static class BomCsvExporter
    {
        public static string ToCsv(BillOfMaterials bom)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Categoria,Perfil,Descripcion,Longitud_in,Cantidad");

            if (bom != null)
            {
                foreach (var line in bom.Lines)
                {
                    builder.Append(Escape(line.Category)).Append(',')
                        .Append(Escape(line.ProfileId)).Append(',')
                        .Append(Escape(line.Description)).Append(',')
                        .Append(line.Length.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                        .Append(line.Quantity.ToString(CultureInfo.InvariantCulture))
                        .Append('\n');
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
