using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Versioned, self-contained persistence DTO for a Cantilever LINE (I-37D).
    ///
    /// It follows the FLAT, self-versioned pattern of <see cref="PushBackDesignDocument"/> and
    /// <see cref="FlowBedDocument"/>: its own <see cref="SchemaVersion"/> plus <c>[JsonExtensionData]</c>, so a
    /// legacy JSON with no version loads through the fallback, unknown fields survive a load/save, and a
    /// re-write from a loaded source never silently DOWNGRADES a newer same-major minor (I-11, D3).
    ///
    /// <para><b>Why it carries the design tree itself and does not mirror it.</b> The two shapes a payload can
    /// take here are a field-by-field mirror of the domain — what Push Back does — or the intention itself. A
    /// mirror decouples the on-disk key names from the domain property names, which is worth having. It also has
    /// one failure mode: a field added to the design and FORGOTTEN in the mirror is dropped on save, silently,
    /// and a round-trip test only catches it if somebody remembered to assert that particular field. The
    /// Cantilever design is a tree of eleven types; mirroring it would be a second declaration of the same tree
    /// and would carry that risk on every future edit.</para>
    ///
    /// <para>Serializing the intention directly cannot omit a field. What it costs is that a domain property
    /// RENAMED is a JSON key renamed — the same constraint the catalogue CSVs already live under — and that cost
    /// is paid by a guard test that pins the persisted names, so a rename fails the build instead of orphaning
    /// every saved line. That trade is deliberate: an omission is silent, and a rename is not.</para>
    ///
    /// <para>The design's own defaults do the work of a legacy fallback. A key absent from an older JSON leaves
    /// the property at the value the design declares, which is the value that design always had.</para>
    /// </summary>
    public sealed class CantileverLineDocument
    {
        /// <summary>Schema version this build writes; a file with a higher MAJOR is rejected (see <see cref="SchemaGuard"/>).</summary>
        public const string CurrentSchemaVersion = "1.0";

        public string SchemaVersion { get; set; } = CurrentSchemaVersion;

        /// <summary>
        /// The persisted INTENTION: the line design, including its identity, its topology, its default arm, its
        /// per-cell overrides and its bracing. Never the resolved result (ADR-0028, D1).
        /// </summary>
        public CantileverLineDesign Line { get; set; }

        /// <summary>JSON fields this build does not know about, preserved verbatim across a load/save (I-11, D3).</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }

        public static CantileverLineDocument FromDomain(CantileverLineDesign design) =>
            FromDomain(design, null);

        /// <summary>
        /// Maps a design to a document, INHERITING the persistence metadata of a previously loaded
        /// <paramref name="source"/> — its unknown fields and a non-downgraded schema version — so a
        /// Document→Domain→Document re-save preserves both.
        /// </summary>
        public static CantileverLineDocument FromDomain(
            CantileverLineDesign design, CantileverLineDocument source)
        {
            return new CantileverLineDocument
            {
                SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(
                    source?.SchemaVersion, CurrentSchemaVersion),
                ExtensionData = source?.ExtensionData,

                // A DEEP COPY. Holding the caller's instance would let an edit made after the save change what
                // the document says was saved, and the source document is kept alive precisely to be compared
                // against later.
                Line = design?.DeepCopy()
            };
        }

        /// <summary>
        /// The design this document describes.
        ///
        /// A deep copy again, and for the mirror of the reason above: the loaded document is retained as the
        /// source for the next save, so handing out its own instance would let the editor mutate the record of
        /// what was on disk.
        /// </summary>
        public CantileverLineDesign ToDomain() => Line?.DeepCopy() ?? new CantileverLineDesign();
    }
}
