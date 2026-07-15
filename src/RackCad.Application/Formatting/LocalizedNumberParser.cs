using System;
using System.Globalization;

namespace RackCad.Application.Formatting
{
    /// <summary>
    /// Parses human-entered measurements without treating the other decimal separator as a thousands separator.
    /// Rack dimensions do not accept grouped numbers: this makes both "96.5" and "96,5" deterministic across Windows
    /// cultures and prevents the dangerous invariant parse where "96,5" could otherwise become 965.
    /// </summary>
    public static class LocalizedNumberParser
    {
        private const NumberStyles DecimalStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
                                                   NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

        public static bool TryDouble(string text, out double value)
            => TryDouble(text, CultureInfo.CurrentCulture, out value);

        public static bool TryDouble(string text, CultureInfo culture, out double value)
        {
            value = 0.0;
            var candidate = (text ?? string.Empty).Trim();
            if (candidate.Length == 0)
            {
                return false;
            }

            culture ??= CultureInfo.CurrentCulture;
            if (double.TryParse(candidate, DecimalStyles, culture, out value) && IsFinite(value))
            {
                return true;
            }

            if (!Equals(culture, CultureInfo.InvariantCulture)
                && double.TryParse(candidate, DecimalStyles, CultureInfo.InvariantCulture, out value)
                && IsFinite(value))
            {
                return true;
            }

            // A single comma with no dot is unambiguously a decimal separator in RackCad input (grouping is forbidden).
            var comma = candidate.IndexOf(',');
            if (candidate.IndexOf('.') < 0 && comma >= 0 && comma == candidate.LastIndexOf(','))
            {
                var normalized = candidate.Replace(',', '.');
                if (double.TryParse(normalized, DecimalStyles, CultureInfo.InvariantCulture, out value) && IsFinite(value))
                {
                    return true;
                }
            }

            value = 0.0;
            return false;
        }

        public static bool TryInteger(string text, out int value)
        {
            var candidate = (text ?? string.Empty).Trim();
            return int.TryParse(candidate, NumberStyles.Integer, CultureInfo.CurrentCulture, out value)
                   || int.TryParse(candidate, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
