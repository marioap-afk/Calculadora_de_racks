using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.Diagnostics;

namespace RackCad.Application.Settings
{
    /// <summary>
    /// T5 al (I-64, prototype «almacen», question 6): the last values of the page «Almacén» of the RackCad window, per user, as they were
    /// typed. They describe the tool, not a drawing, so they live in the settings of the user (like «Fondos destino») and never travel
    /// inside a drawing. A field that is null was never saved and takes the value of the classic windows (RACKLAYOUT, RACKRELLENAR).
    /// </summary>
    public sealed class WarehouseLayoutPreferences
    {
        public string Rows { get; set; }

        public string Cols { get; set; }

        public string AisleRows { get; set; }

        public string AisleCols { get; set; }

        public bool? BackToBack { get; set; }

        public string BackGap { get; set; }

        public bool? Independent { get; set; }

        public string BuildingX { get; set; }

        public string BuildingY { get; set; }

        public string Layer { get; set; }

        public string WallClearance { get; set; }

        public string ColumnClearance { get; set; }

        public bool? TryRotated { get; set; }
    }

    /// <summary>
    /// T5 al (correction 6 of the review): reads <see cref="UserSettings.WarehouseLayout"/> without ever failing the whole file. The value is
    /// consumed whatever it is; an object that this build can read becomes the section, anything else (a field of another type, a value
    /// that is no object) is null, the values of the classic windows, and is logged. The rest of the settings load as before and the file
    /// is never quarantined for it. It writes the section as it is.
    /// </summary>
    public sealed class LenientWarehouseLayoutConverter : JsonConverter<WarehouseLayoutPreferences>
    {
        public override WarehouseLayoutPreferences Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var element = document.RootElement;
            if (element.ValueKind == JsonValueKind.Null) return null;
            try
            {
                if (element.ValueKind != JsonValueKind.Object) throw new JsonException("WarehouseLayout no es un objeto.");
                return element.Deserialize<WarehouseLayoutPreferences>(options);
            }
            catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException || ex is NotSupportedException)
            {
                RackLog.Exception("UserSettings: la sección WarehouseLayout no se pudo leer; se usan los valores de las ventanas clásicas", ex);
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, WarehouseLayoutPreferences value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
