using System;
using System.Text.Json;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Uniform envelope embedded in a drawing's block definition for EVERY rack type, so one round-trip
    /// mechanism (embed on insert → select → reopen the right editor → redefine in place) serves them all.
    /// <see cref="Kind"/> says which editor/store to use; <see cref="Design"/> is that type's own serialized
    /// design (JSON). Reusable across selective, dynamic, cabecera and cama.
    /// </summary>
    public sealed class RackEmbedDocument
    {
        public const string KindSelective = "selective";
        public const string KindDynamic = "dynamic";
        public const string KindCabecera = "cabecera";
        public const string KindCama = "cama";

        public const string ViewFrontal = "frontal";
        public const string ViewLateral = "lateral";
        public const string ViewPlanta = "planta";

        public string SchemaVersion { get; set; } = "1.0";

        /// <summary>Which rack type this is — picks the editor/store on reopen.</summary>
        public string Kind { get; set; }

        /// <summary>Which VIEW this block draws (a rack can have several view-blocks sharing <see cref="Id"/>). Null/empty = frontal.</summary>
        public string View { get; set; }

        /// <summary>
        /// For a multi-block view (the lateral is one block per post/section), which section this block is.
        /// -1 = not a sectioned view (e.g. the single frontal block). Lets an edit redraw the right section in place.
        /// </summary>
        public int Section { get; set; } = -1;

        /// <summary>Stable identity (GUID) of the rack; kept across edits.</summary>
        public string Id { get; set; }

        /// <summary>Client-facing name ("Rack A").</summary>
        public string Name { get; set; }

        /// <summary>The type-specific serialized design (JSON produced by that type's store).</summary>
        public string Design { get; set; }
    }

    /// <summary>Serializes/deserializes the <see cref="RackEmbedDocument"/> envelope (compact JSON).</summary>
    public sealed class RackEmbedStore
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        public string Serialize(RackEmbedDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(document, Options);
        }

        public RackEmbedDocument Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<RackEmbedDocument>(json, Options);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
