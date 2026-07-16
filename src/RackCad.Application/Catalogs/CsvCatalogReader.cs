using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Reads a catalog from CSV (the format Excel edits natively). The header row names the columns;
    /// columns that match a typed property are converted to that type, and any EXTRA column lands in
    /// the entry's <see cref="CatalogEntryBase.Properties"/> bag. So engineers can add columns (Ix,
    /// Iy, etc.) in Excel and they round-trip without any code change.
    /// </summary>
    public static class CsvCatalogReader
    {
        public static List<T> Read<T>(string text) where T : CatalogEntryBase, new()
        {
            var result = new List<T>();
            var rows = ParseCsv(text);

            // Excel sometimes leaves a blank (or comma-only) first row after an edit. Taking it as the header
            // would map NO columns and load N entries with every field default — the whole catalog silently
            // empties. The header is the FIRST row with any content.
            var headerIndex = 0;
            while (headerIndex < rows.Count && rows[headerIndex].All(string.IsNullOrWhiteSpace))
            {
                headerIndex++;
            }

            if (headerIndex >= rows.Count)
            {
                return result;
            }

            var headers = rows[headerIndex].Select(h => h.Trim()).ToArray();
            var writableProperties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanWrite && property.Name != nameof(CatalogEntryBase.Properties))
                .ToDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

            for (var r = headerIndex + 1; r < rows.Count; r++)
            {
                var cells = rows[r];

                if (cells.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                var entry = new T();

                for (var c = 0; c < headers.Length && c < cells.Length; c++)
                {
                    var header = headers[c];
                    var value = cells[c];

                    if (string.IsNullOrEmpty(header))
                    {
                        continue;
                    }

                    if (writableProperties.TryGetValue(header, out var property))
                    {
                        TrySetTyped(entry, property, value);
                    }
                    else if (!string.IsNullOrWhiteSpace(value))
                    {
                        entry.Properties[header] = value.Trim();
                    }
                }

                result.Add(entry);
            }

            return result;
        }

        private static void TrySetTyped(object entry, PropertyInfo property, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            value = value.Trim();

            try
            {
                object converted;

                if (targetType == typeof(string))
                {
                    converted = value;
                }
                else if (targetType == typeof(double))
                {
                    converted = double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                }
                else if (targetType == typeof(int))
                {
                    converted = int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                }
                else if (targetType == typeof(bool))
                {
                    converted = bool.Parse(value);
                }
                else if (targetType.IsEnum)
                {
                    converted = Enum.Parse(targetType, value, ignoreCase: true);
                }
                else
                {
                    converted = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }

                property.SetValue(entry, converted);
            }
            catch (Exception)
            {
                // A single malformed cell must not break the whole catalog: leave the default.
            }
        }

        /// <summary>Minimal RFC-4180 parser: quoted fields, doubled quotes, commas and CR/LF.</summary>
        private static List<string[]> ParseCsv(string text)
        {
            var rows = new List<string[]>();

            if (string.IsNullOrEmpty(text))
            {
                return rows;
            }

            var record = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;
            var sawAny = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(ch);
                    }

                    continue;
                }

                switch (ch)
                {
                    case '"':
                        inQuotes = true;
                        sawAny = true;
                        break;
                    case ',':
                        record.Add(field.ToString());
                        field.Clear();
                        sawAny = true;
                        break;
                    case '\r':
                        break;
                    case '\n':
                        record.Add(field.ToString());
                        field.Clear();
                        rows.Add(record.ToArray());
                        record = new List<string>();
                        sawAny = false;
                        break;
                    default:
                        field.Append(ch);
                        sawAny = true;
                        break;
                }
            }

            if (sawAny || field.Length > 0 || record.Count > 0)
            {
                record.Add(field.ToString());
                rows.Add(record.ToArray());
            }

            return rows;
        }
    }
}
