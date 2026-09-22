using System;
using System.Text.Json;

namespace RackCad.Application.Views.Insertion
{
    /// <summary>Best-effort top-level Id probe for membership diagnostics. It never treats a nested Id as rack identity.</summary>
    public static class RackEnvelopeIdProbe
    {
        public static string Probe(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                using (var document = JsonDocument.Parse(json))
                {
                    if (document.RootElement.ValueKind != JsonValueKind.Object) return null;
                    string value = null;
                    var found = false;
                    foreach (var property in document.RootElement.EnumerateObject())
                    {
                        if (!string.Equals(property.Name, "Id", StringComparison.OrdinalIgnoreCase)) continue;
                        if (found || property.Value.ValueKind != JsonValueKind.String) return null;
                        found = true;
                        value = property.Value.GetString();
                    }
                    return value;
                }
            }
            catch (JsonException) { return null; }
            catch (ArgumentException) { return null; }
        }
    }
}
