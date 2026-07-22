using System;
using System.Globalization;
using System.Text;

namespace RackCad.Application.Diagnostics
{
    /// <summary>
    /// Pure formatting of a single diagnostics entry (no I/O) so its shape is unit-testable. An entry is a
    /// header (UTC timestamp, level, context) followed by the body; for an exception the body is its full
    /// string form — type + message + <b>stack trace</b>, which is exactly what I-03 stops throwing away.
    /// Never throws (a null context/exception is tolerated).
    /// </summary>
    internal static class RackLogFormatter
    {
        public static string Format(DateTime timestampUtc, string level, string context, Exception ex)
        {
            return Compose(timestampUtc, level, context, ex == null ? "(sin excepcion)" : ex.ToString());
        }

        public static string Format(DateTime timestampUtc, string level, string context, string message)
        {
            return Compose(timestampUtc, level, context, message ?? string.Empty);
        }

        private static string Compose(DateTime timestampUtc, string level, string context, string body)
        {
            var stamp = timestampUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            var builder = new StringBuilder();
            builder.Append('[').Append(stamp).Append("] ");
            builder.Append(string.IsNullOrEmpty(level) ? "INFO" : level).Append(' ');
            builder.Append(string.IsNullOrEmpty(context) ? "-" : context);
            if (!string.IsNullOrEmpty(body))
            {
                builder.Append(": ").Append(body);
            }

            builder.Append(Environment.NewLine);
            return builder.ToString();
        }
    }
}
