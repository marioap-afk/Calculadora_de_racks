using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Serializes/deserializes a roller-bed (cama) design to JSON for the drawing embed. As of I-11 the on-disk shape is a
    /// versioned <see cref="FlowBedDocument"/> — a FLAT object with the same field names as before plus an additive
    /// <c>SchemaVersion</c> — so older builds keep reading it. The public config API stays tolerant (its callers treat null
    /// as "no cama"); the document-level API preserves the unknown JSON fields (<see cref="FlowBedDocument.ExtensionData"/>)
    /// so a re-save does not drop metadata a newer build wrote.
    /// </summary>
    public sealed class FlowBedConfigurationStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        /// <summary>Serialize a domain config as a versioned, flat <see cref="FlowBedDocument"/> (writes <c>SchemaVersion</c>).</summary>
        public string Serialize(FlowBedConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return SerializeDocument(FlowBedDocument.FromDomain(config));
        }

        /// <summary>
        /// Tolerant read to a domain config. Returns null on blank/invalid JSON, a degenerate/empty bed (e.g. {} → length 0),
        /// or a bed written by a newer MAJOR — the callers treat null as "no cama / datos invalidos". Legacy flat JSON with
        /// no <c>SchemaVersion</c> still loads (the fallback default is the current version).
        /// </summary>
        public FlowBedConfiguration Deserialize(string json)
        {
            var document = DeserializeDocumentOrNull(json);
            if (document == null)
            {
                return null;
            }

            var config = document.ToDomain();
            return RackDesignValidation.IsUsableFlowBed(config) ? config : null;
        }

        /// <summary>Serialize the whole versioned document, preserving its <see cref="FlowBedDocument.ExtensionData"/> (re-save that keeps unknown fields).</summary>
        public string SerializeDocument(FlowBedDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(document, SerializerOptions);
        }

        /// <summary>
        /// Read the whole versioned document (with its <see cref="FlowBedDocument.ExtensionData"/>) so a re-save can carry
        /// unknown fields forward. Tolerant like <see cref="Deserialize"/> (null on blank/invalid/newer-major); does NOT apply
        /// the usable-bed check, so the caller can inspect or re-serialize even a mid-edit document.
        /// </summary>
        public FlowBedDocument DeserializeDocument(string json) => DeserializeDocumentOrNull(json);

        private static FlowBedDocument DeserializeDocumentOrNull(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            FlowBedDocument document;
            try
            {
                document = JsonSerializer.Deserialize<FlowBedDocument>(json, SerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }

            if (document == null)
            {
                return null;
            }

            // A bed written by a newer major is not readable here. Stay tolerant (treat it as absent) instead of throwing:
            // the embed is scanned/edited without a surrounding catch, and the library wrapper (RackProjectStore) already
            // reports the clear "versión más nueva" message for the wrapper path.
            try
            {
                SchemaGuard.CheckReadable(document.SchemaVersion, FlowBedDocument.CurrentSchemaVersion, "La cama");
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return document;
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
