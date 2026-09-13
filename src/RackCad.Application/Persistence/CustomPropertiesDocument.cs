using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// One collection of custom properties, as persisted (I-54 D-01 / ADR-0039 §1-§2). The same document serves both
    /// scopes: the Project collection in the drawing dictionary and the Rack collection in each member's envelope.
    ///
    /// <code>
    /// { "SchemaVersion": "1.0", "Entries": [ { "Id": "…", "Name": "…", "Value": "…" } ] }
    /// </code>
    ///
    /// <para>
    /// <b><see cref="SchemaVersion"/> has no initializer, and neither has <see cref="Entries"/>.</b> Both are required
    /// members of the format, so "the JSON did not carry it" must stay distinguishable from a real value: a default
    /// would turn an UNKNOWN document into a readable one. Only <see cref="CreateNew"/> stamps them.
    /// </para>
    /// <para>
    /// Only <see cref="CustomPropertiesStore"/> reads and writes this type. It maps the JSON by hand, after validating the
    /// whole tree, precisely so that nothing collapses on the way in: repeated member names, undecodable strings and
    /// declared members spelled in another case are decided BEFORE a dictionary could pick first or last.
    /// </para>
    /// </summary>
    public sealed class CustomPropertiesDocument
    {
        /// <summary>The version this build writes. The format is born here, so there is no legacy line below it.</summary>
        public const string CurrentSchemaVersion = "1.0";

        /// <summary>The highest MAJOR this build reads. A document above it is read-only, never rewritten.</summary>
        public const int SupportedMajor = 1;

        /// <summary>
        /// The depth limit of the 1.x format: the root object counts 1 and every nested object or array adds 1 (D-05.7).
        /// A FORMAT constant, not tuning — raising it requires a new MAJOR.
        /// </summary>
        public const int MaxDepth = 16;

        /// <summary>The stored version, exactly <c>major.minor</c>. No initializer on purpose.</summary>
        public string SchemaVersion { get; set; }

        /// <summary>The entries, in presentation order. Required; an empty list is a valid collection. No initializer on purpose.</summary>
        public List<CustomPropertyEntryDocument> Entries { get; set; }

        /// <summary>Root members a later MINOR wrote and this build does not understand, preserved verbatim. Null when there are none.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement> ExtensionData { get; set; }

        /// <summary>A brand-new, empty collection with its version and its entry list STAMPED explicitly.</summary>
        public static CustomPropertiesDocument CreateNew()
            => new CustomPropertiesDocument
            {
                SchemaVersion = CurrentSchemaVersion,
                Entries = new List<CustomPropertyEntryDocument>(),
            };
    }

    /// <summary>
    /// One persisted custom property. The identity is <see cref="Id"/>; <see cref="Name"/> only presents; <see cref="Value"/>
    /// is literal text (D-02, D-04).
    /// </summary>
    public sealed class CustomPropertyEntryDocument
    {
        /// <summary>The GUID text as stored. Its identity is the value; this build writes it back in lower-case <c>D</c> form.</summary>
        public string Id { get; set; }

        /// <summary>The label the user sees. Never an identity, a lookup key or an aggregation key.</summary>
        public string Name { get; set; }

        /// <summary>Always a string, possibly empty. No type field in V1.</summary>
        public string Value { get; set; }

        /// <summary>Entry members a later MINOR wrote and this build does not understand, preserved verbatim. Null when there are none.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement> ExtensionData { get; set; }
    }
}
