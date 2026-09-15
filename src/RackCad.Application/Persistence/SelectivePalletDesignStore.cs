using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.ProjectVariables;

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

        /// <summary>
        /// Writes the design, STAMPING its schema version through the sticky rule
        /// (<see cref="SelectiveDesignSchema.ResolveWriteVersion"/>): a design that carries a binding is
        /// promoted, one that never did keeps the legacy line, and a promoted one never goes back down.
        /// A design stored above the readable major throws rather than being overwritten.
        /// </summary>
        public string Serialize(SelectivePalletDesignDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.SchemaVersion = SelectiveDesignSchema.ResolveWriteVersion(
                document.SchemaVersion,
                document.HasPropertyValues);

            return JsonSerializer.Serialize(document, SerializerOptions);
        }

        public SelectivePalletDesignDocument Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("El diseño del selectivo está vacío.");
            }

            SelectivePalletDesignDocument document;
            try
            {
                document = JsonSerializer.Deserialize<SelectivePalletDesignDocument>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("El diseño del selectivo no es un JSON válido: " + ex.Message, ex);
            }

            // The READ constant is the PROMOTED line, not the legacy one this build usually writes: otherwise
            // the guard would reject the very documents the sticky promotion just produced (I-47 C4-9).
            SchemaGuard.CheckReadable(document?.SchemaVersion, SelectivePalletDesignDocument.PromotedSchemaVersion, "El diseño del selectivo");

            if (!RackDesignValidation.IsUsableSelective(document))
            {
                throw new InvalidOperationException("El diseño del selectivo no tiene frentes (¿archivo vacío o incompleto?).");
            }

            ValidatePropertyValues(document);

            return document;
        }

        private static void ValidatePropertyValues(SelectivePalletDesignDocument document)
        {
            if (document.PropertyValues == null)
            {
                return;
            }

            foreach (var pair in document.PropertyValues)
            {
                var source = pair.Value;
                if (source == null)
                {
                    // Historical documents may carry an explicit null. Presence still freezes the authored
                    // literal, and the resolver classifies it as an unknown source rather than the store
                    // rewriting or discarding it.
                    continue;
                }

                if (string.Equals(source.Kind, SelectivePropertyValueDocument.ProjectVariableKind, StringComparison.Ordinal))
                {
                    if (!VariableId.TryParse(source.VariableId, out _) || source.Expression != null)
                    {
                        throw new InvalidOperationException(
                            "La propiedad '" + pair.Key + "' contiene una referencia de variable mal formada.");
                    }

                    continue;
                }

                if (string.Equals(source.Kind, SelectivePropertyValueDocument.ExpressionKind, StringComparison.Ordinal))
                {
                    if (source.Expression == null || source.VariableId != null ||
                        (source.ExtensionData != null && source.ExtensionData.Count > 0))
                    {
                        throw new InvalidOperationException(
                            "La propiedad '" + pair.Key + "' contiene una expresion mal formada.");
                    }

                    continue;
                }

                throw new InvalidOperationException(
                    "La propiedad '" + pair.Key + "' declara una fuente desconocida ('" +
                    (source.Kind ?? "<null>") + "').");
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
            PersistedBoundExpressionJson.AddConverter(options);
            return options;
        }
    }
}
