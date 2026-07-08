using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Serializes/deserializes a <see cref="SelectivePalletDesignDocument"/> to compact JSON. Used to embed
    /// the design (with its Id + Name) in the drawing so a rack can be reopened and edited. Mirrors the JSON
    /// conventions of <see cref="RackProjectStore"/> (enum-as-string, case-insensitive), but compact for embedding.
    /// </summary>
    public sealed class SelectivePalletDesignStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        public string Serialize(SelectivePalletDesignDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(document, SerializerOptions);
        }

        public SelectivePalletDesignDocument Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("El diseño del selectivo está vacío.");
            }

            try
            {
                return JsonSerializer.Deserialize<SelectivePalletDesignDocument>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("El diseño del selectivo no es un JSON válido: " + ex.Message, ex);
            }
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
