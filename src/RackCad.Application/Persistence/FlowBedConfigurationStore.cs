using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>Serializes/deserializes a <see cref="FlowBedConfiguration"/> (a plain roller-bed config) to JSON.</summary>
    public sealed class FlowBedConfigurationStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        public string Serialize(FlowBedConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return JsonSerializer.Serialize(config, SerializerOptions);
        }

        public FlowBedConfiguration Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var config = JsonSerializer.Deserialize<FlowBedConfiguration>(json, SerializerOptions);

                // This store is tolerant by contract (its callers treat null as "no cama"); a degenerate/empty bed
                // (e.g. {} → length 0) is treated as absent rather than a drawable 0-length bed.
                return RackDesignValidation.IsUsableFlowBed(config) ? config : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
