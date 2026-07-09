using System.Collections.Generic;
using System.Globalization;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Parses the catalog's "Peraltes" column — the user-maintained ";" (or ",") separated list of allowed
    /// PERALTE values a piece declares (e.g. "3;3.5;4;4.5;5"). Pure and testable here instead of living as
    /// private UI code: invalid tokens are skipped, non-positives dropped, duplicates removed, order kept.
    /// </summary>
    public static class PeralteList
    {
        public static IReadOnlyList<double> Parse(string raw)
        {
            var values = new List<double>();

            if (string.IsNullOrWhiteSpace(raw))
            {
                return values;
            }

            foreach (var part in raw.Split(';', ','))
            {
                if (TryNum(part, out var value) && value > 0.0 && !values.Contains(value))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        /// <summary>Invariant-first tolerant parse (same semantics as the UI's numeric fields).</summary>
        private static bool TryNum(string text, out double value)
        {
            text = (text ?? string.Empty).Trim();
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value)
                || double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
        }
    }
}
