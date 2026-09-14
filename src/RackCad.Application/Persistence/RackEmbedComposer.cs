using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Pure factory for the uniform drawing envelope (<see cref="RackEmbedDocument"/>). Constructing a NEW envelope object
    /// for a redraw or an inserted view would DROP the unknown JSON fields (<see cref="RackEmbedDocument.ExtensionData"/>)
    /// of the block it replaces and could DOWNGRADE its schema version (I-11 D3). <see cref="Compose"/> keeps both: it
    /// inherits the extension data and a non-downgraded write version (via <see cref="SchemaVersionPolicy"/>) from the
    /// SOURCE envelope, while taking the fresh identity / view / section / design from the caller.
    ///
    /// Callers pass:
    /// <list type="bullet">
    /// <item>for an EXISTING view-block being redrawn — THAT block's own embed (preserving its per-view metadata);</item>
    /// <item>for a NEW view inserted during an edit — the initiating (picked) envelope, so it inherits its metadata;</item>
    /// <item>for a brand-new rack (a Quick* insert) — <c>null</c> (fresh current version, no extension data).</item>
    /// </list>
    /// No I/O; not a Plugin type. It preserves the envelope only — it does NOT recurse into the type-specific
    /// <see cref="RackEmbedDocument.Design"/> payload (see the FlowBed/Larguero documents for their own preservation).
    ///
    /// <para>
    /// I-54 (ADR-0039 §4 and §6): the rack's custom properties travel with the envelope the same way. <see cref="Compose"/>
    /// inherits <see cref="RackEmbedDocument.CustomProperties"/> from the source and <see cref="WithCustomProperties"/>
    /// replaces only that member. Neither interprets it, both normalize an <c>Undefined</c> or <c>Null</c> element to an
    /// absent member, and both refuse a source whose extension data carries a key equal, ignoring case, to a declared
    /// envelope member: such an envelope would be written with that key twice.
    /// </para>
    /// </summary>
    public static class RackEmbedComposer
    {
        /// <summary>The JSON members the envelope declares, which its extension data must never repeat.</summary>
        private static readonly string[] DeclaredMembers = typeof(RackEmbedDocument)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => !property.IsDefined(typeof(JsonExtensionDataAttribute), false))
            .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
            .ToArray();

        public static RackEmbedDocument Compose(
            RackEmbedDocument source, string kind, string id, string name, string view, int section, string design)
        {
            RequireNoDeclaredMemberInExtensionData(source);

            return new RackEmbedDocument
            {
                SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source?.SchemaVersion, RackEmbedDocument.CurrentSchemaVersion),
                Kind = kind,
                Id = id,
                Name = name,
                View = view,
                Section = section,
                Design = design,
                CustomProperties = Normalize(source?.CustomProperties),
                ExtensionData = source?.ExtensionData
            };
        }

        /// <summary>
        /// A NEW envelope equal to <paramref name="source"/> in kind, identity, name, view, section, design and extension data,
        /// whose only change is <paramref name="customProperties"/> (null removes the member), with a non-downgraded write
        /// version. The source is not modified, and the result gets its own copy of the extension data.
        /// </summary>
        public static RackEmbedDocument WithCustomProperties(RackEmbedDocument source, JsonElement? customProperties)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            RequireNoDeclaredMemberInExtensionData(source);

            return new RackEmbedDocument
            {
                SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source.SchemaVersion, RackEmbedDocument.CurrentSchemaVersion),
                Kind = source.Kind,
                Id = source.Id,
                Name = source.Name,
                View = source.View,
                Section = source.Section,
                Design = source.Design,
                CustomProperties = Normalize(customProperties),
                ExtensionData = source.ExtensionData == null
                    ? null
                    : new Dictionary<string, JsonElement>(source.ExtensionData, source.ExtensionData.Comparer)
            };
        }

        /// <summary>An element that is <c>Undefined</c> or <c>Null</c> is no member at all: never written, never inherited.</summary>
        private static JsonElement? Normalize(JsonElement? value)
            => value.HasValue && (value.Value.ValueKind == JsonValueKind.Undefined || value.Value.ValueKind == JsonValueKind.Null)
                ? null
                : value;

        /// <summary>
        /// The store never files a declared member under extension data, so a source that has one was built in memory: a
        /// programming error. Refusing it before producing anything is the only honest option, since dropping the key or
        /// letting one of the two values win would both lose data silently.
        /// </summary>
        private static void RequireNoDeclaredMemberInExtensionData(RackEmbedDocument source)
        {
            if (source?.ExtensionData == null)
            {
                return;
            }

            foreach (var key in source.ExtensionData.Keys)
            {
                var member = DeclaredMembers.FirstOrDefault(declared => string.Equals(declared, key, StringComparison.OrdinalIgnoreCase));

                if (member != null)
                {
                    throw new InvalidOperationException(
                        "El sobre de origen trae en ExtensionData la clave '" + key + "', que coincide con el miembro declarado '"
                        + member + "': es un error de programacion y no se compone un sobre con esa clave repetida.");
                }
            }
        }
    }
}
